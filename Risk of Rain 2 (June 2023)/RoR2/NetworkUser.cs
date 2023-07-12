using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Runtime.InteropServices;
using JetBrains.Annotations;
using Rewired;
using RoR2.Networking;
using RoR2.Stats;
using Unity;
using UnityEngine;
using UnityEngine.Networking;

namespace RoR2;

[RequireComponent(typeof(NetworkLoadout))]
public class NetworkUser : NetworkBehaviour
{
	public delegate void NetworkUserGenericDelegate(NetworkUser networkUser);

	private static readonly List<NetworkUser> instancesList;

	public static readonly ReadOnlyCollection<NetworkUser> readOnlyInstancesList;

	private static readonly List<NetworkUser> localPlayers;

	public static readonly ReadOnlyCollection<NetworkUser> readOnlyLocalPlayersList;

	[SyncVar(hook = "OnSyncId")]
	private NetworkUserId _id;

	[SyncVar]
	public byte rewiredPlayerId;

	[SyncVar(hook = "OnSyncMasterObjectId")]
	private NetworkInstanceId _masterObjectId;

	[CanBeNull]
	public LocalUser localUser;

	public CameraRigController cameraRigController;

	public string userName = "";

	[SyncVar]
	public Color32 userColor = Color32.op_Implicit(Color.red);

	[SyncVar]
	private uint netLunarCoins;

	private MemoizedGetComponent<CharacterMaster> cachedMaster;

	private MemoizedGetComponent<PlayerCharacterMasterController> cachedPlayerCharacterMasterController;

	private MemoizedGetComponent<PlayerStatsComponent> cachedPlayerStatsComponent;

	private GameObject _masterObject;

	[NonSerialized]
	[SyncVar(hook = "SetBodyPreference")]
	public BodyIndex bodyIndexPreference = BodyIndex.None;

	private float secondAccumulator;

	[NonSerialized]
	public List<UnlockableDef> unlockables = new List<UnlockableDef>();

	public List<string> debugUnlockablesList = new List<string>();

	private static NetworkInstanceId serverCurrentStage;

	private NetworkInstanceId _serverLastStageAcknowledgedByClient;

	private static int kRpcRpcDeductLunarCoins;

	private static int kRpcRpcAwardLunarCoins;

	private static int kCmdCmdSetNetLunarCoins;

	private static int kCmdCmdSetBodyPreference;

	private static int kCmdCmdSendConsoleCommand;

	private static int kCmdCmdSendNewUnlockables;

	private static int kRpcRpcRequestUnlockables;

	private static int kCmdCmdReportAchievement;

	private static int kCmdCmdReportUnlock;

	private static int kCmdCmdAcknowledgeStage;

	private static int kCmdCmdSubmitVote;

	public NetworkLoadout networkLoadout { get; private set; }

	public NetworkUserId id
	{
		get
		{
			return _id;
		}
		set
		{
			if (!_id.Equals(value))
			{
				Network_id = value;
				UpdateUserName();
			}
		}
	}

	public bool authed => id.HasValidValue();

	public Player inputPlayer => localUser?.inputPlayer;

	public uint lunarCoins
	{
		get
		{
			if (localUser != null)
			{
				return localUser.userProfile.coins;
			}
			return netLunarCoins;
		}
	}

	public CharacterMaster master => cachedMaster.Get(masterObject);

	public PlayerCharacterMasterController masterController => cachedPlayerCharacterMasterController.Get(masterObject);

	public PlayerStatsComponent masterPlayerStatsComponent => cachedPlayerStatsComponent.Get(masterObject);

	public GameObject masterObject
	{
		get
		{
			//IL_000f: Unknown result type (might be due to invalid IL or missing references)
			if (!Object.op_Implicit((Object)(object)_masterObject))
			{
				_masterObject = Util.FindNetworkObject(_masterObjectId);
			}
			return _masterObject;
		}
		set
		{
			//IL_000f: Unknown result type (might be due to invalid IL or missing references)
			//IL_0022: Unknown result type (might be due to invalid IL or missing references)
			if (Object.op_Implicit((Object)(object)value))
			{
				Network_masterObjectId = value.GetComponent<NetworkIdentity>().netId;
				_masterObject = value;
			}
			else
			{
				Network_masterObjectId = NetworkInstanceId.Invalid;
				_masterObject = null;
			}
		}
	}

	public bool isParticipating => Object.op_Implicit((Object)(object)masterObject);

	public bool isSplitScreenExtraPlayer => id.subId != 0;

	public bool serverIsClientLoaded { get; private set; }

	private NetworkInstanceId serverLastStageAcknowledgedByClient
	{
		get
		{
			//IL_0001: Unknown result type (might be due to invalid IL or missing references)
			return _serverLastStageAcknowledgedByClient;
		}
		set
		{
			//IL_0001: Unknown result type (might be due to invalid IL or missing references)
			//IL_0006: Unknown result type (might be due to invalid IL or missing references)
			//IL_0010: Unknown result type (might be due to invalid IL or missing references)
			//IL_0011: Unknown result type (might be due to invalid IL or missing references)
			//IL_0016: Unknown result type (might be due to invalid IL or missing references)
			//IL_001c: Unknown result type (might be due to invalid IL or missing references)
			if (!(_serverLastStageAcknowledgedByClient == value))
			{
				_serverLastStageAcknowledgedByClient = value;
				if (serverCurrentStage == _serverLastStageAcknowledgedByClient)
				{
					serverIsClientLoaded = true;
					NetworkUser.onNetworkUserLoadedSceneServer?.Invoke(this);
				}
			}
		}
	}

	public NetworkUserId Network_id
	{
		get
		{
			return _id;
		}
		[param: In]
		set
		{
			if (NetworkServer.localClientActive && !((NetworkBehaviour)this).syncVarHookGuard)
			{
				((NetworkBehaviour)this).syncVarHookGuard = true;
				OnSyncId(value);
				((NetworkBehaviour)this).syncVarHookGuard = false;
			}
			((NetworkBehaviour)this).SetSyncVar<NetworkUserId>(value, ref _id, 1u);
		}
	}

	public byte NetworkrewiredPlayerId
	{
		get
		{
			return rewiredPlayerId;
		}
		[param: In]
		set
		{
			((NetworkBehaviour)this).SetSyncVar<byte>(value, ref rewiredPlayerId, 2u);
		}
	}

	public NetworkInstanceId Network_masterObjectId
	{
		get
		{
			//IL_0001: Unknown result type (might be due to invalid IL or missing references)
			return _masterObjectId;
		}
		[param: In]
		set
		{
			//IL_001d: Unknown result type (might be due to invalid IL or missing references)
			//IL_002c: Unknown result type (might be due to invalid IL or missing references)
			if (NetworkServer.localClientActive && !((NetworkBehaviour)this).syncVarHookGuard)
			{
				((NetworkBehaviour)this).syncVarHookGuard = true;
				OnSyncMasterObjectId(value);
				((NetworkBehaviour)this).syncVarHookGuard = false;
			}
			((NetworkBehaviour)this).SetSyncVar<NetworkInstanceId>(value, ref _masterObjectId, 4u);
		}
	}

	public Color32 NetworkuserColor
	{
		get
		{
			//IL_0001: Unknown result type (might be due to invalid IL or missing references)
			return userColor;
		}
		[param: In]
		set
		{
			//IL_0001: Unknown result type (might be due to invalid IL or missing references)
			((NetworkBehaviour)this).SetSyncVar<Color32>(value, ref userColor, 8u);
		}
	}

	public uint NetworknetLunarCoins
	{
		get
		{
			return netLunarCoins;
		}
		[param: In]
		set
		{
			((NetworkBehaviour)this).SetSyncVar<uint>(value, ref netLunarCoins, 16u);
		}
	}

	public BodyIndex NetworkbodyIndexPreference
	{
		get
		{
			return bodyIndexPreference;
		}
		[param: In]
		set
		{
			if (NetworkServer.localClientActive && !((NetworkBehaviour)this).syncVarHookGuard)
			{
				((NetworkBehaviour)this).syncVarHookGuard = true;
				SetBodyPreference(value);
				((NetworkBehaviour)this).syncVarHookGuard = false;
			}
			ulong num = (ulong)value;
			ulong num2 = (ulong)bodyIndexPreference;
			((NetworkBehaviour)this).SetSyncVarEnum<BodyIndex>(value, num, ref bodyIndexPreference, num2, 32u);
		}
	}

	public static event Action<NetworkUser> onNetworkUserLoadedSceneServer;

	public static event Action<NetworkUser> onLoadoutChangedGlobal;

	[Obsolete("Use onPostNetworkUserStart instead", false)]
	public static event NetworkUserGenericDelegate OnPostNetworkUserStart
	{
		add
		{
			onPostNetworkUserStart += value;
		}
		remove
		{
			onPostNetworkUserStart -= value;
		}
	}

	public static event NetworkUserGenericDelegate onPostNetworkUserStart;

	[Obsolete("Use onNetworkUserUnlockablesUpdated instead", false)]
	public static event NetworkUserGenericDelegate OnNetworkUserUnlockablesUpdated
	{
		add
		{
			onNetworkUserUnlockablesUpdated += value;
		}
		remove
		{
			onNetworkUserUnlockablesUpdated -= value;
		}
	}

	public static event NetworkUserGenericDelegate onNetworkUserUnlockablesUpdated;

	public static event NetworkUserGenericDelegate onNetworkUserDiscovered;

	public static event NetworkUserGenericDelegate onNetworkUserLost;

	public static event NetworkUserGenericDelegate onNetworkUserBodyPreferenceChanged;

	[RuntimeInitializeOnLoadMethod(/*Could not decode attribute arguments.*/)]
	private static void Init()
	{
		UserProfile.onUnlockableGranted += delegate(UserProfile userProfile, UnlockableDef unlockableDef)
		{
			if (NetworkClient.active)
			{
				NetworkUser networkUser3 = FindNetworkUserByUserProfile(userProfile);
				if (Object.op_Implicit((Object)(object)networkUser3))
				{
					networkUser3.SendServerUnlockables();
				}
			}
		};
		UserProfile.onLoadoutChangedGlobal += delegate(UserProfile userProfile)
		{
			if (NetworkClient.active)
			{
				NetworkUser networkUser2 = FindNetworkUserByUserProfile(userProfile);
				if (Object.op_Implicit((Object)(object)networkUser2))
				{
					networkUser2.PullLoadoutFromUserProfile();
				}
			}
		};
		UserProfile.onSurvivorPreferenceChangedGlobal += delegate(UserProfile userProfile)
		{
			if (NetworkClient.active)
			{
				NetworkUser networkUser = FindNetworkUserByUserProfile(userProfile);
				if (Object.op_Implicit((Object)(object)networkUser))
				{
					networkUser.SetSurvivorPreferenceClient(userProfile.GetSurvivorPreference());
				}
			}
		};
		Stage.onStageStartGlobal += delegate(Stage stage)
		{
			//IL_0008: Unknown result type (might be due to invalid IL or missing references)
			//IL_000d: Unknown result type (might be due to invalid IL or missing references)
			//IL_0027: Unknown result type (might be due to invalid IL or missing references)
			if (NetworkServer.active)
			{
				serverCurrentStage = ((NetworkBehaviour)stage).netId;
			}
			foreach (NetworkUser localPlayer in localPlayers)
			{
				localPlayer.CallCmdAcknowledgeStage(((NetworkBehaviour)stage).netId);
			}
		};
	}

	private void OnEnable()
	{
		instancesList.Add(this);
		NetworkUser.onNetworkUserDiscovered?.Invoke(this);
	}

	private void OnDisable()
	{
		NetworkUser.onNetworkUserLost?.Invoke(this);
		instancesList.Remove(this);
	}

	private void Awake()
	{
		Object.DontDestroyOnLoad((Object)(object)((Component)this).gameObject);
		networkLoadout = ((Component)this).GetComponent<NetworkLoadout>();
		networkLoadout.onLoadoutUpdated += OnLoadoutUpdated;
	}

	private void OnLoadoutUpdated()
	{
		NetworkUser.onLoadoutChangedGlobal?.Invoke(this);
	}

	private void PullLoadoutFromUserProfile()
	{
		UserProfile userProfile = localUser?.userProfile;
		if (userProfile != null)
		{
			Loadout loadout = Loadout.RequestInstance();
			userProfile.CopyLoadout(loadout);
			networkLoadout.SetLoadout(loadout);
			Loadout.ReturnInstance(loadout);
		}
	}

	private void Start()
	{
		//IL_00a2: Unknown result type (might be due to invalid IL or missing references)
		if (((NetworkBehaviour)this).isLocalPlayer)
		{
			LocalUserManager.FindLocalUser((int)((NetworkBehaviour)this).playerControllerId)?.LinkNetworkUser(this);
			PullLoadoutFromUserProfile();
			if (!Object.op_Implicit((Object)(object)Run.instance))
			{
				SetSurvivorPreferenceClient(localUser.userProfile.GetSurvivorPreference());
			}
		}
		if (Object.op_Implicit((Object)(object)Run.instance))
		{
			Run.instance.OnUserAdded(this);
		}
		if (NetworkClient.active)
		{
			SyncLunarCoinsToServer();
			SendServerUnlockables();
		}
		OnLoadoutUpdated();
		NetworkUser.onPostNetworkUserStart?.Invoke(this);
		if (((NetworkBehaviour)this).isLocalPlayer && Object.op_Implicit((Object)(object)Stage.instance))
		{
			CallCmdAcknowledgeStage(((NetworkBehaviour)Stage.instance).netId);
		}
	}

	private void OnDestroy()
	{
		localPlayers.Remove(this);
		Run.instance?.OnUserRemoved(this);
		localUser?.UnlinkNetworkUser();
	}

	public override void OnStartLocalPlayer()
	{
		((NetworkBehaviour)this).OnStartLocalPlayer();
		localPlayers.Add(this);
	}

	public override void OnStartClient()
	{
		UpdateUserName();
	}

	private void OnSyncId(NetworkUserId newId)
	{
		id = newId;
	}

	private void OnSyncMasterObjectId(NetworkInstanceId newValue)
	{
		//IL_0008: Unknown result type (might be due to invalid IL or missing references)
		_masterObject = null;
		Network_masterObjectId = newValue;
	}

	public NetworkPlayerName GetNetworkPlayerName()
	{
		NetworkPlayerName result = default(NetworkPlayerName);
		result.nameOverride = ((id.strValue != null) ? id.strValue : null);
		result.steamId = ((!string.IsNullOrEmpty(id.strValue)) ? default(CSteamID) : new CSteamID(id.value));
		return result;
	}

	[Server]
	public void DeductLunarCoins(uint count)
	{
		if (!NetworkServer.active)
		{
			Debug.LogWarning((object)"[Server] function 'System.Void RoR2.NetworkUser::DeductLunarCoins(System.UInt32)' called on client");
			return;
		}
		NetworknetLunarCoins = HGMath.UintSafeSubtract(netLunarCoins, count);
		CallRpcDeductLunarCoins(count);
	}

	[ClientRpc]
	private void RpcDeductLunarCoins(uint count)
	{
		if (localUser != null)
		{
			localUser.userProfile.coins = HGMath.UintSafeSubtract(localUser.userProfile.coins, count);
			SyncLunarCoinsToServer();
		}
	}

	[Server]
	public void AwardLunarCoins(uint count)
	{
		if (!NetworkServer.active)
		{
			Debug.LogWarning((object)"[Server] function 'System.Void RoR2.NetworkUser::AwardLunarCoins(System.UInt32)' called on client");
			return;
		}
		NetworknetLunarCoins = HGMath.UintSafeAdd(netLunarCoins, count);
		CallRpcAwardLunarCoins(count);
	}

	[ClientRpc]
	private void RpcAwardLunarCoins(uint count)
	{
		if (localUser != null)
		{
			localUser.userProfile.coins = HGMath.UintSafeAdd(localUser.userProfile.coins, count);
			localUser.userProfile.totalCollectedCoins = HGMath.UintSafeAdd(localUser.userProfile.totalCollectedCoins, count);
			SyncLunarCoinsToServer();
		}
	}

	[Client]
	private void SyncLunarCoinsToServer()
	{
		if (!NetworkClient.active)
		{
			Debug.LogWarning((object)"[Client] function 'System.Void RoR2.NetworkUser::SyncLunarCoinsToServer()' called on server");
		}
		else if (localUser != null)
		{
			CallCmdSetNetLunarCoins(localUser.userProfile.coins);
		}
	}

	[Command]
	private void CmdSetNetLunarCoins(uint newNetLunarCoins)
	{
		NetworknetLunarCoins = newNetLunarCoins;
	}

	public CharacterBody GetCurrentBody()
	{
		CharacterMaster characterMaster = master;
		if (Object.op_Implicit((Object)(object)characterMaster))
		{
			return characterMaster.GetBody();
		}
		return null;
	}

	[Server]
	public void CopyLoadoutFromMaster()
	{
		if (!NetworkServer.active)
		{
			Debug.LogWarning((object)"[Server] function 'System.Void RoR2.NetworkUser::CopyLoadoutFromMaster()' called on client");
		}
		else
		{
			networkLoadout.SetLoadout(master.loadout);
		}
	}

	[Server]
	public void CopyLoadoutToMaster()
	{
		if (!NetworkServer.active)
		{
			Debug.LogWarning((object)"[Server] function 'System.Void RoR2.NetworkUser::CopyLoadoutToMaster()' called on client");
			return;
		}
		Loadout loadout = Loadout.RequestInstance();
		networkLoadout.CopyLoadout(loadout);
		master.SetLoadoutServer(loadout);
		Loadout.ReturnInstance(loadout);
	}

	private void SetBodyPreference(BodyIndex newBodyIndexPreference)
	{
		Debug.Log((object)$"Changinging body preference for {GetNetworkPlayerName().GetResolvedName()} ({id}) from {bodyIndexPreference} to {newBodyIndexPreference}");
		NetworkbodyIndexPreference = newBodyIndexPreference;
		NetworkUser.onNetworkUserBodyPreferenceChanged?.Invoke(this);
	}

	[Command]
	public void CmdSetBodyPreference(BodyIndex newBodyIndexPreference)
	{
		SetBodyPreference(newBodyIndexPreference);
	}

	[Client]
	public void SetSurvivorPreferenceClient(SurvivorDef survivorDef)
	{
		if (!NetworkClient.active)
		{
			Debug.LogWarning((object)"[Client] function 'System.Void RoR2.NetworkUser::SetSurvivorPreferenceClient(RoR2.SurvivorDef)' called on server");
			return;
		}
		if (!Object.op_Implicit((Object)(object)survivorDef))
		{
			throw new ArgumentException("Provided object is null or invalid", "survivorDef");
		}
		BodyIndex bodyIndexFromSurvivorIndex = SurvivorCatalog.GetBodyIndexFromSurvivorIndex(Object.op_Implicit((Object)(object)survivorDef) ? survivorDef.survivorIndex : SurvivorIndex.None);
		Debug.Log((object)$"SetSurvivorPreferenceClient survivorIndex={survivorDef.survivorIndex}, bodyIndex={bodyIndexFromSurvivorIndex}");
		CallCmdSetBodyPreference(bodyIndexFromSurvivorIndex);
	}

	public SurvivorDef GetSurvivorPreference()
	{
		return SurvivorCatalog.GetSurvivorDef(SurvivorCatalog.GetSurvivorIndexFromBodyIndex(bodyIndexPreference));
	}

	private void Update()
	{
		if (localUser == null)
		{
			return;
		}
		if (Time.timeScale != 0f)
		{
			secondAccumulator += Time.unscaledDeltaTime;
		}
		if (!(secondAccumulator >= 1f))
		{
			return;
		}
		secondAccumulator -= 1f;
		if (!Object.op_Implicit((Object)(object)Run.instance))
		{
			return;
		}
		localUser.userProfile.totalRunSeconds++;
		if (Object.op_Implicit((Object)(object)masterObject))
		{
			CharacterMaster component = masterObject.GetComponent<CharacterMaster>();
			if (Object.op_Implicit((Object)(object)component) && component.hasBody)
			{
				localUser.userProfile.totalAliveSeconds++;
			}
		}
	}

	public void UpdateUserName()
	{
		userName = GetNetworkPlayerName().GetResolvedName();
	}

	[Command]
	public void CmdSendConsoleCommand(string commandName, string[] args)
	{
		Console.instance.RunClientCmd(this, commandName, args);
	}

	[Client]
	public void SendServerUnlockables()
	{
		if (!NetworkClient.active)
		{
			Debug.LogWarning((object)"[Client] function 'System.Void RoR2.NetworkUser::SendServerUnlockables()' called on server");
		}
		else if (localUser != null)
		{
			int unlockableCount = localUser.userProfile.statSheet.GetUnlockableCount();
			UnlockableIndex[] array = new UnlockableIndex[unlockableCount];
			for (int i = 0; i < unlockableCount; i++)
			{
				array[i] = localUser.userProfile.statSheet.GetUnlockableIndex(i);
			}
			CallCmdSendNewUnlockables(array);
		}
	}

	[Command]
	private void CmdSendNewUnlockables(UnlockableIndex[] newUnlockableIndices)
	{
		unlockables.Clear();
		debugUnlockablesList.Clear();
		int i = 0;
		for (int num = newUnlockableIndices.Length; i < num; i++)
		{
			UnlockableDef unlockableDef = UnlockableCatalog.GetUnlockableDef(newUnlockableIndices[i]);
			if ((Object)(object)unlockableDef != (Object)null)
			{
				unlockables.Add(unlockableDef);
				debugUnlockablesList.Add(unlockableDef.cachedName);
			}
		}
		NetworkUser.onNetworkUserUnlockablesUpdated?.Invoke(this);
	}

	[Server]
	public void ServerRequestUnlockables()
	{
		if (!NetworkServer.active)
		{
			Debug.LogWarning((object)"[Server] function 'System.Void RoR2.NetworkUser::ServerRequestUnlockables()' called on client");
		}
		else
		{
			CallRpcRequestUnlockables();
		}
	}

	[ClientRpc]
	private void RpcRequestUnlockables()
	{
		if (Util.HasEffectiveAuthority(((Component)this).gameObject))
		{
			SendServerUnlockables();
		}
	}

	[Command]
	public void CmdReportAchievement(string achievementNameToken)
	{
		Chat.SubjectFormatChatMessage subjectFormatChatMessage = new Chat.SubjectFormatChatMessage();
		subjectFormatChatMessage.baseToken = "ACHIEVEMENT_UNLOCKED_MESSAGE";
		subjectFormatChatMessage.subjectAsNetworkUser = this;
		subjectFormatChatMessage.paramTokens = new string[1] { achievementNameToken };
		Chat.SendBroadcastChat(subjectFormatChatMessage);
	}

	[Command]
	public void CmdReportUnlock(UnlockableIndex unlockIndex)
	{
		Debug.LogFormat("NetworkUser.CmdReportUnlock({0})", new object[1] { unlockIndex });
		UnlockableDef unlockableDef = UnlockableCatalog.GetUnlockableDef(unlockIndex);
		if ((Object)(object)unlockableDef != (Object)null)
		{
			ServerHandleUnlock(unlockableDef);
		}
	}

	[Server]
	public void ServerHandleUnlock([NotNull] UnlockableDef unlockableDef)
	{
		if (!NetworkServer.active)
		{
			Debug.LogWarning((object)"[Server] function 'System.Void RoR2.NetworkUser::ServerHandleUnlock(RoR2.UnlockableDef)' called on client");
			return;
		}
		Debug.LogFormat("NetworkUser.ServerHandleUnlock({0})", new object[1] { unlockableDef.cachedName });
		if (Object.op_Implicit((Object)(object)masterObject))
		{
			PlayerStatsComponent component = masterObject.GetComponent<PlayerStatsComponent>();
			if (Object.op_Implicit((Object)(object)component))
			{
				component.currentStats.AddUnlockable(unlockableDef);
				component.ForceNextTransmit();
			}
		}
	}

	[Command]
	public void CmdAcknowledgeStage(NetworkInstanceId stageNetworkId)
	{
		//IL_0001: Unknown result type (might be due to invalid IL or missing references)
		serverLastStageAcknowledgedByClient = stageNetworkId;
	}

	[Command]
	public void CmdSubmitVote(GameObject voteControllerGameObject, int choiceIndex)
	{
		if (Object.op_Implicit((Object)(object)voteControllerGameObject))
		{
			VoteController component = voteControllerGameObject.GetComponent<VoteController>();
			if (Object.op_Implicit((Object)(object)component))
			{
				component.ReceiveUserVote(this, choiceIndex);
			}
		}
	}

	public static bool AllParticipatingNetworkUsersReady()
	{
		ReadOnlyCollection<NetworkUser> readOnlyCollection = readOnlyInstancesList;
		for (int i = 0; i < readOnlyCollection.Count; i++)
		{
			NetworkUser networkUser = readOnlyCollection[i];
			if (networkUser.isParticipating && !((NetworkBehaviour)networkUser).connectionToClient.isReady)
			{
				return false;
			}
		}
		return true;
	}

	[CanBeNull]
	private static NetworkUser FindNetworkUserByUserProfile([NotNull] UserProfile userProfile)
	{
		if (userProfile == null)
		{
			throw new ArgumentNullException("userProfile");
		}
		ReadOnlyCollection<LocalUser> readOnlyLocalUsersList = LocalUserManager.readOnlyLocalUsersList;
		for (int i = 0; i < readOnlyLocalUsersList.Count; i++)
		{
			LocalUser localUser = readOnlyLocalUsersList[i];
			if (localUser.userProfile == userProfile)
			{
				return localUser.currentNetworkUser;
			}
		}
		return null;
	}

	static NetworkUser()
	{
		//IL_0052: Unknown result type (might be due to invalid IL or missing references)
		//IL_005c: Expected O, but got Unknown
		//IL_007c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0086: Expected O, but got Unknown
		//IL_00a6: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b0: Expected O, but got Unknown
		//IL_00d0: Unknown result type (might be due to invalid IL or missing references)
		//IL_00da: Expected O, but got Unknown
		//IL_00fa: Unknown result type (might be due to invalid IL or missing references)
		//IL_0104: Expected O, but got Unknown
		//IL_0124: Unknown result type (might be due to invalid IL or missing references)
		//IL_012e: Expected O, but got Unknown
		//IL_014e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0158: Expected O, but got Unknown
		//IL_0178: Unknown result type (might be due to invalid IL or missing references)
		//IL_0182: Expected O, but got Unknown
		//IL_01a2: Unknown result type (might be due to invalid IL or missing references)
		//IL_01ac: Expected O, but got Unknown
		//IL_01cc: Unknown result type (might be due to invalid IL or missing references)
		//IL_01d6: Expected O, but got Unknown
		//IL_01f6: Unknown result type (might be due to invalid IL or missing references)
		//IL_0200: Expected O, but got Unknown
		instancesList = new List<NetworkUser>();
		readOnlyInstancesList = new ReadOnlyCollection<NetworkUser>(instancesList);
		localPlayers = new List<NetworkUser>();
		readOnlyLocalPlayersList = new ReadOnlyCollection<NetworkUser>(localPlayers);
		kCmdCmdSetNetLunarCoins = -934763456;
		NetworkBehaviour.RegisterCommandDelegate(typeof(NetworkUser), kCmdCmdSetNetLunarCoins, new CmdDelegate(InvokeCmdCmdSetNetLunarCoins));
		kCmdCmdSetBodyPreference = 234442470;
		NetworkBehaviour.RegisterCommandDelegate(typeof(NetworkUser), kCmdCmdSetBodyPreference, new CmdDelegate(InvokeCmdCmdSetBodyPreference));
		kCmdCmdSendConsoleCommand = -1997680971;
		NetworkBehaviour.RegisterCommandDelegate(typeof(NetworkUser), kCmdCmdSendConsoleCommand, new CmdDelegate(InvokeCmdCmdSendConsoleCommand));
		kCmdCmdSendNewUnlockables = 1855027350;
		NetworkBehaviour.RegisterCommandDelegate(typeof(NetworkUser), kCmdCmdSendNewUnlockables, new CmdDelegate(InvokeCmdCmdSendNewUnlockables));
		kCmdCmdReportAchievement = -1674656990;
		NetworkBehaviour.RegisterCommandDelegate(typeof(NetworkUser), kCmdCmdReportAchievement, new CmdDelegate(InvokeCmdCmdReportAchievement));
		kCmdCmdReportUnlock = -1831223439;
		NetworkBehaviour.RegisterCommandDelegate(typeof(NetworkUser), kCmdCmdReportUnlock, new CmdDelegate(InvokeCmdCmdReportUnlock));
		kCmdCmdAcknowledgeStage = -2118585573;
		NetworkBehaviour.RegisterCommandDelegate(typeof(NetworkUser), kCmdCmdAcknowledgeStage, new CmdDelegate(InvokeCmdCmdAcknowledgeStage));
		kCmdCmdSubmitVote = 329593659;
		NetworkBehaviour.RegisterCommandDelegate(typeof(NetworkUser), kCmdCmdSubmitVote, new CmdDelegate(InvokeCmdCmdSubmitVote));
		kRpcRpcDeductLunarCoins = -1554352898;
		NetworkBehaviour.RegisterRpcDelegate(typeof(NetworkUser), kRpcRpcDeductLunarCoins, new CmdDelegate(InvokeRpcRpcDeductLunarCoins));
		kRpcRpcAwardLunarCoins = -604060198;
		NetworkBehaviour.RegisterRpcDelegate(typeof(NetworkUser), kRpcRpcAwardLunarCoins, new CmdDelegate(InvokeRpcRpcAwardLunarCoins));
		kRpcRpcRequestUnlockables = -1809653515;
		NetworkBehaviour.RegisterRpcDelegate(typeof(NetworkUser), kRpcRpcRequestUnlockables, new CmdDelegate(InvokeRpcRpcRequestUnlockables));
		NetworkCRC.RegisterBehaviour("NetworkUser", 0);
	}

	private void UNetVersion()
	{
	}

	protected static void InvokeCmdCmdSetNetLunarCoins(NetworkBehaviour obj, NetworkReader reader)
	{
		if (!NetworkServer.active)
		{
			Debug.LogError((object)"Command CmdSetNetLunarCoins called on client.");
		}
		else
		{
			((NetworkUser)(object)obj).CmdSetNetLunarCoins(reader.ReadPackedUInt32());
		}
	}

	protected static void InvokeCmdCmdSetBodyPreference(NetworkBehaviour obj, NetworkReader reader)
	{
		if (!NetworkServer.active)
		{
			Debug.LogError((object)"Command CmdSetBodyPreference called on client.");
		}
		else
		{
			((NetworkUser)(object)obj).CmdSetBodyPreference((BodyIndex)reader.ReadInt32());
		}
	}

	protected static void InvokeCmdCmdSendConsoleCommand(NetworkBehaviour obj, NetworkReader reader)
	{
		if (!NetworkServer.active)
		{
			Debug.LogError((object)"Command CmdSendConsoleCommand called on client.");
		}
		else
		{
			((NetworkUser)(object)obj).CmdSendConsoleCommand(reader.ReadString(), GeneratedNetworkCode._ReadArrayString_None(reader));
		}
	}

	protected static void InvokeCmdCmdSendNewUnlockables(NetworkBehaviour obj, NetworkReader reader)
	{
		if (!NetworkServer.active)
		{
			Debug.LogError((object)"Command CmdSendNewUnlockables called on client.");
		}
		else
		{
			((NetworkUser)(object)obj).CmdSendNewUnlockables(GeneratedNetworkCode._ReadArrayUnlockableIndex_None(reader));
		}
	}

	protected static void InvokeCmdCmdReportAchievement(NetworkBehaviour obj, NetworkReader reader)
	{
		if (!NetworkServer.active)
		{
			Debug.LogError((object)"Command CmdReportAchievement called on client.");
		}
		else
		{
			((NetworkUser)(object)obj).CmdReportAchievement(reader.ReadString());
		}
	}

	protected static void InvokeCmdCmdReportUnlock(NetworkBehaviour obj, NetworkReader reader)
	{
		if (!NetworkServer.active)
		{
			Debug.LogError((object)"Command CmdReportUnlock called on client.");
		}
		else
		{
			((NetworkUser)(object)obj).CmdReportUnlock((UnlockableIndex)reader.ReadInt32());
		}
	}

	protected static void InvokeCmdCmdAcknowledgeStage(NetworkBehaviour obj, NetworkReader reader)
	{
		//IL_001d: Unknown result type (might be due to invalid IL or missing references)
		if (!NetworkServer.active)
		{
			Debug.LogError((object)"Command CmdAcknowledgeStage called on client.");
		}
		else
		{
			((NetworkUser)(object)obj).CmdAcknowledgeStage(reader.ReadNetworkId());
		}
	}

	protected static void InvokeCmdCmdSubmitVote(NetworkBehaviour obj, NetworkReader reader)
	{
		if (!NetworkServer.active)
		{
			Debug.LogError((object)"Command CmdSubmitVote called on client.");
		}
		else
		{
			((NetworkUser)(object)obj).CmdSubmitVote(reader.ReadGameObject(), (int)reader.ReadPackedUInt32());
		}
	}

	public void CallCmdSetNetLunarCoins(uint newNetLunarCoins)
	{
		//IL_002d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0033: Expected O, but got Unknown
		//IL_0058: Unknown result type (might be due to invalid IL or missing references)
		if (!NetworkClient.active)
		{
			Debug.LogError((object)"Command function CmdSetNetLunarCoins called on server.");
			return;
		}
		if (((NetworkBehaviour)this).isServer)
		{
			CmdSetNetLunarCoins(newNetLunarCoins);
			return;
		}
		NetworkWriter val = new NetworkWriter();
		val.Write((short)0);
		val.Write((short)5);
		val.WritePackedUInt32((uint)kCmdCmdSetNetLunarCoins);
		val.Write(((Component)this).GetComponent<NetworkIdentity>().netId);
		val.WritePackedUInt32(newNetLunarCoins);
		((NetworkBehaviour)this).SendCommandInternal(val, 0, "CmdSetNetLunarCoins");
	}

	public void CallCmdSetBodyPreference(BodyIndex newBodyIndexPreference)
	{
		//IL_002d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0033: Expected O, but got Unknown
		//IL_0058: Unknown result type (might be due to invalid IL or missing references)
		if (!NetworkClient.active)
		{
			Debug.LogError((object)"Command function CmdSetBodyPreference called on server.");
			return;
		}
		if (((NetworkBehaviour)this).isServer)
		{
			CmdSetBodyPreference(newBodyIndexPreference);
			return;
		}
		NetworkWriter val = new NetworkWriter();
		val.Write((short)0);
		val.Write((short)5);
		val.WritePackedUInt32((uint)kCmdCmdSetBodyPreference);
		val.Write(((Component)this).GetComponent<NetworkIdentity>().netId);
		val.Write((int)newBodyIndexPreference);
		((NetworkBehaviour)this).SendCommandInternal(val, 0, "CmdSetBodyPreference");
	}

	public void CallCmdSendConsoleCommand(string commandName, string[] args)
	{
		//IL_0031: Unknown result type (might be due to invalid IL or missing references)
		//IL_0037: Expected O, but got Unknown
		//IL_005c: Unknown result type (might be due to invalid IL or missing references)
		if (!NetworkClient.active)
		{
			Debug.LogError((object)"Command function CmdSendConsoleCommand called on server.");
			return;
		}
		if (((NetworkBehaviour)this).isServer)
		{
			CmdSendConsoleCommand(commandName, args);
			return;
		}
		NetworkWriter val = new NetworkWriter();
		val.Write((short)0);
		val.Write((short)5);
		val.WritePackedUInt32((uint)kCmdCmdSendConsoleCommand);
		val.Write(((Component)this).GetComponent<NetworkIdentity>().netId);
		val.Write(commandName);
		GeneratedNetworkCode._WriteArrayString_None(val, args);
		((NetworkBehaviour)this).SendCommandInternal(val, 0, "CmdSendConsoleCommand");
	}

	public void CallCmdSendNewUnlockables(UnlockableIndex[] newUnlockableIndices)
	{
		//IL_002d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0033: Expected O, but got Unknown
		//IL_0058: Unknown result type (might be due to invalid IL or missing references)
		if (!NetworkClient.active)
		{
			Debug.LogError((object)"Command function CmdSendNewUnlockables called on server.");
			return;
		}
		if (((NetworkBehaviour)this).isServer)
		{
			CmdSendNewUnlockables(newUnlockableIndices);
			return;
		}
		NetworkWriter val = new NetworkWriter();
		val.Write((short)0);
		val.Write((short)5);
		val.WritePackedUInt32((uint)kCmdCmdSendNewUnlockables);
		val.Write(((Component)this).GetComponent<NetworkIdentity>().netId);
		GeneratedNetworkCode._WriteArrayUnlockableIndex_None(val, newUnlockableIndices);
		((NetworkBehaviour)this).SendCommandInternal(val, 0, "CmdSendNewUnlockables");
	}

	public void CallCmdReportAchievement(string achievementNameToken)
	{
		//IL_002d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0033: Expected O, but got Unknown
		//IL_0058: Unknown result type (might be due to invalid IL or missing references)
		if (!NetworkClient.active)
		{
			Debug.LogError((object)"Command function CmdReportAchievement called on server.");
			return;
		}
		if (((NetworkBehaviour)this).isServer)
		{
			CmdReportAchievement(achievementNameToken);
			return;
		}
		NetworkWriter val = new NetworkWriter();
		val.Write((short)0);
		val.Write((short)5);
		val.WritePackedUInt32((uint)kCmdCmdReportAchievement);
		val.Write(((Component)this).GetComponent<NetworkIdentity>().netId);
		val.Write(achievementNameToken);
		((NetworkBehaviour)this).SendCommandInternal(val, 0, "CmdReportAchievement");
	}

	public void CallCmdReportUnlock(UnlockableIndex unlockIndex)
	{
		//IL_002d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0033: Expected O, but got Unknown
		//IL_0058: Unknown result type (might be due to invalid IL or missing references)
		if (!NetworkClient.active)
		{
			Debug.LogError((object)"Command function CmdReportUnlock called on server.");
			return;
		}
		if (((NetworkBehaviour)this).isServer)
		{
			CmdReportUnlock(unlockIndex);
			return;
		}
		NetworkWriter val = new NetworkWriter();
		val.Write((short)0);
		val.Write((short)5);
		val.WritePackedUInt32((uint)kCmdCmdReportUnlock);
		val.Write(((Component)this).GetComponent<NetworkIdentity>().netId);
		val.Write((int)unlockIndex);
		((NetworkBehaviour)this).SendCommandInternal(val, 0, "CmdReportUnlock");
	}

	public void CallCmdAcknowledgeStage(NetworkInstanceId stageNetworkId)
	{
		//IL_0022: Unknown result type (might be due to invalid IL or missing references)
		//IL_002d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0033: Expected O, but got Unknown
		//IL_0058: Unknown result type (might be due to invalid IL or missing references)
		//IL_0063: Unknown result type (might be due to invalid IL or missing references)
		if (!NetworkClient.active)
		{
			Debug.LogError((object)"Command function CmdAcknowledgeStage called on server.");
			return;
		}
		if (((NetworkBehaviour)this).isServer)
		{
			CmdAcknowledgeStage(stageNetworkId);
			return;
		}
		NetworkWriter val = new NetworkWriter();
		val.Write((short)0);
		val.Write((short)5);
		val.WritePackedUInt32((uint)kCmdCmdAcknowledgeStage);
		val.Write(((Component)this).GetComponent<NetworkIdentity>().netId);
		val.Write(stageNetworkId);
		((NetworkBehaviour)this).SendCommandInternal(val, 0, "CmdAcknowledgeStage");
	}

	public void CallCmdSubmitVote(GameObject voteControllerGameObject, int choiceIndex)
	{
		//IL_0031: Unknown result type (might be due to invalid IL or missing references)
		//IL_0037: Expected O, but got Unknown
		//IL_005c: Unknown result type (might be due to invalid IL or missing references)
		if (!NetworkClient.active)
		{
			Debug.LogError((object)"Command function CmdSubmitVote called on server.");
			return;
		}
		if (((NetworkBehaviour)this).isServer)
		{
			CmdSubmitVote(voteControllerGameObject, choiceIndex);
			return;
		}
		NetworkWriter val = new NetworkWriter();
		val.Write((short)0);
		val.Write((short)5);
		val.WritePackedUInt32((uint)kCmdCmdSubmitVote);
		val.Write(((Component)this).GetComponent<NetworkIdentity>().netId);
		val.Write(voteControllerGameObject);
		val.WritePackedUInt32((uint)choiceIndex);
		((NetworkBehaviour)this).SendCommandInternal(val, 0, "CmdSubmitVote");
	}

	protected static void InvokeRpcRpcDeductLunarCoins(NetworkBehaviour obj, NetworkReader reader)
	{
		if (!NetworkClient.active)
		{
			Debug.LogError((object)"RPC RpcDeductLunarCoins called on server.");
		}
		else
		{
			((NetworkUser)(object)obj).RpcDeductLunarCoins(reader.ReadPackedUInt32());
		}
	}

	protected static void InvokeRpcRpcAwardLunarCoins(NetworkBehaviour obj, NetworkReader reader)
	{
		if (!NetworkClient.active)
		{
			Debug.LogError((object)"RPC RpcAwardLunarCoins called on server.");
		}
		else
		{
			((NetworkUser)(object)obj).RpcAwardLunarCoins(reader.ReadPackedUInt32());
		}
	}

	protected static void InvokeRpcRpcRequestUnlockables(NetworkBehaviour obj, NetworkReader reader)
	{
		if (!NetworkClient.active)
		{
			Debug.LogError((object)"RPC RpcRequestUnlockables called on server.");
		}
		else
		{
			((NetworkUser)(object)obj).RpcRequestUnlockables();
		}
	}

	public void CallRpcDeductLunarCoins(uint count)
	{
		//IL_0016: Unknown result type (might be due to invalid IL or missing references)
		//IL_001c: Expected O, but got Unknown
		//IL_0041: Unknown result type (might be due to invalid IL or missing references)
		if (!NetworkServer.active)
		{
			Debug.LogError((object)"RPC Function RpcDeductLunarCoins called on client.");
			return;
		}
		NetworkWriter val = new NetworkWriter();
		val.Write((short)0);
		val.Write((short)2);
		val.WritePackedUInt32((uint)kRpcRpcDeductLunarCoins);
		val.Write(((Component)this).GetComponent<NetworkIdentity>().netId);
		val.WritePackedUInt32(count);
		((NetworkBehaviour)this).SendRPCInternal(val, 0, "RpcDeductLunarCoins");
	}

	public void CallRpcAwardLunarCoins(uint count)
	{
		//IL_0016: Unknown result type (might be due to invalid IL or missing references)
		//IL_001c: Expected O, but got Unknown
		//IL_0041: Unknown result type (might be due to invalid IL or missing references)
		if (!NetworkServer.active)
		{
			Debug.LogError((object)"RPC Function RpcAwardLunarCoins called on client.");
			return;
		}
		NetworkWriter val = new NetworkWriter();
		val.Write((short)0);
		val.Write((short)2);
		val.WritePackedUInt32((uint)kRpcRpcAwardLunarCoins);
		val.Write(((Component)this).GetComponent<NetworkIdentity>().netId);
		val.WritePackedUInt32(count);
		((NetworkBehaviour)this).SendRPCInternal(val, 0, "RpcAwardLunarCoins");
	}

	public void CallRpcRequestUnlockables()
	{
		//IL_0016: Unknown result type (might be due to invalid IL or missing references)
		//IL_001c: Expected O, but got Unknown
		//IL_0041: Unknown result type (might be due to invalid IL or missing references)
		if (!NetworkServer.active)
		{
			Debug.LogError((object)"RPC Function RpcRequestUnlockables called on client.");
			return;
		}
		NetworkWriter val = new NetworkWriter();
		val.Write((short)0);
		val.Write((short)2);
		val.WritePackedUInt32((uint)kRpcRpcRequestUnlockables);
		val.Write(((Component)this).GetComponent<NetworkIdentity>().netId);
		((NetworkBehaviour)this).SendRPCInternal(val, 0, "RpcRequestUnlockables");
	}

	public override bool OnSerialize(NetworkWriter writer, bool forceAll)
	{
		//IL_0020: Unknown result type (might be due to invalid IL or missing references)
		//IL_002c: Unknown result type (might be due to invalid IL or missing references)
		//IL_00e1: Unknown result type (might be due to invalid IL or missing references)
		//IL_0114: Unknown result type (might be due to invalid IL or missing references)
		if (forceAll)
		{
			GeneratedNetworkCode._WriteNetworkUserId_None(writer, _id);
			writer.WritePackedUInt32((uint)rewiredPlayerId);
			writer.Write(_masterObjectId);
			writer.Write(userColor);
			writer.WritePackedUInt32(netLunarCoins);
			writer.Write((int)bodyIndexPreference);
			return true;
		}
		bool flag = false;
		if ((((NetworkBehaviour)this).syncVarDirtyBits & (true ? 1u : 0u)) != 0)
		{
			if (!flag)
			{
				writer.WritePackedUInt32(((NetworkBehaviour)this).syncVarDirtyBits);
				flag = true;
			}
			GeneratedNetworkCode._WriteNetworkUserId_None(writer, _id);
		}
		if ((((NetworkBehaviour)this).syncVarDirtyBits & 2u) != 0)
		{
			if (!flag)
			{
				writer.WritePackedUInt32(((NetworkBehaviour)this).syncVarDirtyBits);
				flag = true;
			}
			writer.WritePackedUInt32((uint)rewiredPlayerId);
		}
		if ((((NetworkBehaviour)this).syncVarDirtyBits & 4u) != 0)
		{
			if (!flag)
			{
				writer.WritePackedUInt32(((NetworkBehaviour)this).syncVarDirtyBits);
				flag = true;
			}
			writer.Write(_masterObjectId);
		}
		if ((((NetworkBehaviour)this).syncVarDirtyBits & 8u) != 0)
		{
			if (!flag)
			{
				writer.WritePackedUInt32(((NetworkBehaviour)this).syncVarDirtyBits);
				flag = true;
			}
			writer.Write(userColor);
		}
		if ((((NetworkBehaviour)this).syncVarDirtyBits & 0x10u) != 0)
		{
			if (!flag)
			{
				writer.WritePackedUInt32(((NetworkBehaviour)this).syncVarDirtyBits);
				flag = true;
			}
			writer.WritePackedUInt32(netLunarCoins);
		}
		if ((((NetworkBehaviour)this).syncVarDirtyBits & 0x20u) != 0)
		{
			if (!flag)
			{
				writer.WritePackedUInt32(((NetworkBehaviour)this).syncVarDirtyBits);
				flag = true;
			}
			writer.Write((int)bodyIndexPreference);
		}
		if (!flag)
		{
			writer.WritePackedUInt32(((NetworkBehaviour)this).syncVarDirtyBits);
		}
		return flag;
	}

	public override void OnDeserialize(NetworkReader reader, bool initialState)
	{
		//IL_0020: Unknown result type (might be due to invalid IL or missing references)
		//IL_0025: Unknown result type (might be due to invalid IL or missing references)
		//IL_002c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0031: Unknown result type (might be due to invalid IL or missing references)
		//IL_0097: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b0: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b5: Unknown result type (might be due to invalid IL or missing references)
		if (initialState)
		{
			_id = GeneratedNetworkCode._ReadNetworkUserId_None(reader);
			rewiredPlayerId = (byte)reader.ReadPackedUInt32();
			_masterObjectId = reader.ReadNetworkId();
			userColor = reader.ReadColor32();
			netLunarCoins = reader.ReadPackedUInt32();
			bodyIndexPreference = (BodyIndex)reader.ReadInt32();
			return;
		}
		int num = (int)reader.ReadPackedUInt32();
		if (((uint)num & (true ? 1u : 0u)) != 0)
		{
			OnSyncId(GeneratedNetworkCode._ReadNetworkUserId_None(reader));
		}
		if (((uint)num & 2u) != 0)
		{
			rewiredPlayerId = (byte)reader.ReadPackedUInt32();
		}
		if (((uint)num & 4u) != 0)
		{
			OnSyncMasterObjectId(reader.ReadNetworkId());
		}
		if (((uint)num & 8u) != 0)
		{
			userColor = reader.ReadColor32();
		}
		if (((uint)num & 0x10u) != 0)
		{
			netLunarCoins = reader.ReadPackedUInt32();
		}
		if (((uint)num & 0x20u) != 0)
		{
			SetBodyPreference((BodyIndex)reader.ReadInt32());
		}
	}

	public override void PreStartClient()
	{
	}
}
