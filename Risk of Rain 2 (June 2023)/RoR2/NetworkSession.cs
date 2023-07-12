using System;
using System.Collections;
using System.Reflection;
using System.Runtime.InteropServices;
using Facepunch.Steamworks;
using HG;
using RoR2.ConVar;
using RoR2.ExpansionManagement;
using RoR2.Networking;
using UnityEngine;
using UnityEngine.Networking;

namespace RoR2;

public class NetworkSession : NetworkBehaviour
{
	[Flags]
	public enum Flags
	{
		None = 0,
		HasPassword = 1,
		IsDedicatedServer = 2
	}

	[SyncVar(hook = "OnSyncSteamId")]
	private ulong serverSteamId;

	[SyncVar]
	public ulong lobbySteamId;

	[SyncVar]
	private uint _flags;

	[SyncVar]
	public string tagsString;

	[SyncVar]
	public uint maxPlayers;

	[SyncVar]
	public string serverName;

	private TagManager serverManager;

	private static readonly BoolConVar cvSteamLobbyAllowPersistence = new BoolConVar("steam_lobby_allow_persistence", ConVarFlags.None, "1", "Whether or not the application should attempt to reestablish an active game session's Steamworks lobby if it's been lost.");

	public static NetworkSession instance { get; private set; }

	public Flags flags
	{
		get
		{
			return (Flags)_flags;
		}
		set
		{
			Network_flags = (uint)value;
		}
	}

	public ulong NetworkserverSteamId
	{
		get
		{
			return serverSteamId;
		}
		[param: In]
		set
		{
			if (NetworkServer.localClientActive && !((NetworkBehaviour)this).syncVarHookGuard)
			{
				((NetworkBehaviour)this).syncVarHookGuard = true;
				OnSyncSteamId(value);
				((NetworkBehaviour)this).syncVarHookGuard = false;
			}
			((NetworkBehaviour)this).SetSyncVar<ulong>(value, ref serverSteamId, 1u);
		}
	}

	public ulong NetworklobbySteamId
	{
		get
		{
			return lobbySteamId;
		}
		[param: In]
		set
		{
			((NetworkBehaviour)this).SetSyncVar<ulong>(value, ref lobbySteamId, 2u);
		}
	}

	public uint Network_flags
	{
		get
		{
			return _flags;
		}
		[param: In]
		set
		{
			((NetworkBehaviour)this).SetSyncVar<uint>(value, ref _flags, 4u);
		}
	}

	public string NetworktagsString
	{
		get
		{
			return tagsString;
		}
		[param: In]
		set
		{
			((NetworkBehaviour)this).SetSyncVar<string>(value, ref tagsString, 8u);
		}
	}

	public uint NetworkmaxPlayers
	{
		get
		{
			return maxPlayers;
		}
		[param: In]
		set
		{
			((NetworkBehaviour)this).SetSyncVar<uint>(value, ref maxPlayers, 16u);
		}
	}

	public string NetworkserverName
	{
		get
		{
			return serverName;
		}
		[param: In]
		set
		{
			((NetworkBehaviour)this).SetSyncVar<string>(value, ref serverName, 32u);
		}
	}

	private void SetFlag(Flags flag, bool flagEnabled)
	{
		if (flagEnabled)
		{
			flags |= flag;
		}
		else
		{
			flags &= ~flag;
		}
	}

	public bool HasFlag(Flags flag)
	{
		return (flags & flag) == flag;
	}

	public override void OnStartServer()
	{
		((NetworkBehaviour)this).OnStartServer();
		SetFlag(Flags.IsDedicatedServer, flagEnabled: false);
		NetworkManagerSystem.SvPasswordConVar.instance.onValueChanged += UpdatePasswordFlag;
		UpdatePasswordFlag(NetworkManagerSystem.SvPasswordConVar.instance.value);
		RegisterTags();
		NetworkmaxPlayers = (uint)NetworkManagerSystem.SvMaxPlayersConVar.instance.intValue;
		NetworkManagerSystem.SvHostNameConVar.instance.onValueChanged += UpdateServerName;
		UpdateServerName(NetworkManagerSystem.SvHostNameConVar.instance.GetString());
	}

	private void RegisterTags()
	{
		if (PlatformSystems.ShouldUseEpicOnlineSystems)
		{
			serverManager = ServerManagerBase<EOSServerManager>.instance;
		}
		else
		{
			serverManager = ServerManagerBase<SteamworksServerManager>.instance;
			NetworkserverSteamId = NetworkManagerSystem.singleton.serverP2PId.steamValue;
		}
		if (serverManager != null)
		{
			TagManager tagManager = serverManager;
			tagManager.onTagsStringUpdated = (Action<string>)Delegate.Combine(tagManager.onTagsStringUpdated, new Action<string>(UpdateTagsString));
			UpdateTagsString(serverManager.tagsString ?? string.Empty);
		}
	}

	private void OnDestroy()
	{
		NetworkManagerSystem.SvHostNameConVar.instance.onValueChanged -= UpdateServerName;
		NetworkManagerSystem.SvPasswordConVar.instance.onValueChanged -= UpdatePasswordFlag;
		UnregisterTags();
	}

	private void UnregisterTags()
	{
		if (serverManager != null)
		{
			TagManager tagManager = serverManager;
			tagManager.onTagsStringUpdated = (Action<string>)Delegate.Remove(tagManager.onTagsStringUpdated, new Action<string>(UpdateTagsString));
		}
	}

	private void UpdateTagsString(string tagsString)
	{
		NetworktagsString = tagsString;
	}

	public override void OnStartClient()
	{
		((NetworkBehaviour)this).OnStartClient();
		SteamworksAdvertiseGame();
	}

	private void UpdatePasswordFlag(string password)
	{
		SetFlag(Flags.HasPassword, !string.IsNullOrEmpty(password));
	}

	private void OnSyncSteamId(ulong newValue)
	{
		NetworkserverSteamId = newValue;
		SteamworksAdvertiseGame();
	}

	private void SteamworksAdvertiseGame()
	{
		if (RoR2Application.instance.steamworksClient != null)
		{
			ulong num = serverSteamId;
			uint num2 = 0u;
			ushort num3 = 0;
			CallMethod(GetField(GetField(Client.Instance, "native"), "user"), "AdvertiseGame", new object[3] { num, num2, num3 });
		}
		static void CallMethod(object obj, string methodName, object[] args)
		{
			obj.GetType().GetMethod(methodName, BindingFlags.Instance | BindingFlags.NonPublic).Invoke(obj, args);
		}
		static object GetField(object obj, string fieldName)
		{
			return obj.GetType().GetField(fieldName, BindingFlags.Instance | BindingFlags.NonPublic).GetValue(obj);
		}
	}

	private void OnEnable()
	{
		instance = SingletonHelper.Assign<NetworkSession>(instance, this);
	}

	private void OnDisable()
	{
		instance = SingletonHelper.Unassign<NetworkSession>(instance, this);
	}

	private void Start()
	{
		Object.DontDestroyOnLoad((Object)(object)((Component)this).gameObject);
		if (NetworkServer.active)
		{
			NetworkServer.Spawn(((Component)this).gameObject);
		}
		((MonoBehaviour)this).StartCoroutine(SteamworksLobbyPersistenceCoroutine());
	}

	private void UpdateServerName(string newHostName)
	{
		NetworkserverName = newHostName;
	}

	[Server]
	public Run BeginRun(Run runPrefabComponent, RuleBook ruleBook, ulong seed)
	{
		//IL_0057: Unknown result type (might be due to invalid IL or missing references)
		//IL_005c: Unknown result type (might be due to invalid IL or missing references)
		//IL_005f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0064: Unknown result type (might be due to invalid IL or missing references)
		if (!NetworkServer.active)
		{
			Debug.LogWarning((object)"[Server] function 'RoR2.Run RoR2.NetworkSession::BeginRun(RoR2.Run,RoR2.RuleBook,System.UInt64)' called on client");
			return null;
		}
		if (!Object.op_Implicit((Object)(object)Run.instance))
		{
			GameObject val = Object.Instantiate<GameObject>(((Component)runPrefabComponent).gameObject);
			Run component = val.GetComponent<Run>();
			component.SetRuleBook(ruleBook);
			component.seed = seed;
			NetworkServer.Spawn(val);
			Enumerator<ExpansionDef> enumerator = ExpansionCatalog.expansionDefs.GetEnumerator();
			try
			{
				while (enumerator.MoveNext())
				{
					ExpansionDef current = enumerator.Current;
					if (component.IsExpansionEnabled(current) && Object.op_Implicit((Object)(object)current.runBehaviorPrefab))
					{
						NetworkServer.Spawn(Object.Instantiate<GameObject>(current.runBehaviorPrefab, val.transform));
					}
				}
				return component;
			}
			finally
			{
				((IDisposable)enumerator).Dispose();
			}
		}
		return null;
	}

	[Server]
	public void EndRun()
	{
		if (!NetworkServer.active)
		{
			Debug.LogWarning((object)"[Server] function 'System.Void RoR2.NetworkSession::EndRun()' called on client");
		}
		else if (Object.op_Implicit((Object)(object)Run.instance))
		{
			Object.Destroy((Object)(object)((Component)Run.instance).gameObject);
		}
	}

	private IEnumerator SteamworksLobbyPersistenceCoroutine()
	{
		while (true)
		{
			UpdateSteamworksLobbyPersistence();
			yield return (object)new WaitForSecondsRealtime(4f);
		}
	}

	private void UpdateSteamworksLobbyPersistence()
	{
		if (Client.Instance == null || !cvSteamLobbyAllowPersistence.value || NetworkServer.dontListen || PlatformSystems.lobbyManager.awaitingJoin || PlatformSystems.lobbyManager.awaitingCreate)
		{
			return;
		}
		ulong currentLobby = Client.Instance.Lobby.CurrentLobby;
		if (NetworkServer.active)
		{
			if (PlatformSystems.lobbyManager.isInLobby)
			{
				NetworklobbySteamId = currentLobby;
			}
			else
			{
				PlatformSystems.lobbyManager.CreateLobby();
			}
		}
		else if (lobbySteamId != 0L && lobbySteamId != currentLobby)
		{
			PlatformSystems.lobbyManager.JoinLobby(new UserID(lobbySteamId));
		}
	}

	private void UNetVersion()
	{
	}

	public override bool OnSerialize(NetworkWriter writer, bool forceAll)
	{
		if (forceAll)
		{
			writer.WritePackedUInt64(serverSteamId);
			writer.WritePackedUInt64(lobbySteamId);
			writer.WritePackedUInt32(_flags);
			writer.Write(tagsString);
			writer.WritePackedUInt32(maxPlayers);
			writer.Write(serverName);
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
			writer.WritePackedUInt64(serverSteamId);
		}
		if ((((NetworkBehaviour)this).syncVarDirtyBits & 2u) != 0)
		{
			if (!flag)
			{
				writer.WritePackedUInt32(((NetworkBehaviour)this).syncVarDirtyBits);
				flag = true;
			}
			writer.WritePackedUInt64(lobbySteamId);
		}
		if ((((NetworkBehaviour)this).syncVarDirtyBits & 4u) != 0)
		{
			if (!flag)
			{
				writer.WritePackedUInt32(((NetworkBehaviour)this).syncVarDirtyBits);
				flag = true;
			}
			writer.WritePackedUInt32(_flags);
		}
		if ((((NetworkBehaviour)this).syncVarDirtyBits & 8u) != 0)
		{
			if (!flag)
			{
				writer.WritePackedUInt32(((NetworkBehaviour)this).syncVarDirtyBits);
				flag = true;
			}
			writer.Write(tagsString);
		}
		if ((((NetworkBehaviour)this).syncVarDirtyBits & 0x10u) != 0)
		{
			if (!flag)
			{
				writer.WritePackedUInt32(((NetworkBehaviour)this).syncVarDirtyBits);
				flag = true;
			}
			writer.WritePackedUInt32(maxPlayers);
		}
		if ((((NetworkBehaviour)this).syncVarDirtyBits & 0x20u) != 0)
		{
			if (!flag)
			{
				writer.WritePackedUInt32(((NetworkBehaviour)this).syncVarDirtyBits);
				flag = true;
			}
			writer.Write(serverName);
		}
		if (!flag)
		{
			writer.WritePackedUInt32(((NetworkBehaviour)this).syncVarDirtyBits);
		}
		return flag;
	}

	public override void OnDeserialize(NetworkReader reader, bool initialState)
	{
		if (initialState)
		{
			serverSteamId = reader.ReadPackedUInt64();
			lobbySteamId = reader.ReadPackedUInt64();
			_flags = reader.ReadPackedUInt32();
			tagsString = reader.ReadString();
			maxPlayers = reader.ReadPackedUInt32();
			serverName = reader.ReadString();
			return;
		}
		int num = (int)reader.ReadPackedUInt32();
		if (((uint)num & (true ? 1u : 0u)) != 0)
		{
			OnSyncSteamId(reader.ReadPackedUInt64());
		}
		if (((uint)num & 2u) != 0)
		{
			lobbySteamId = reader.ReadPackedUInt64();
		}
		if (((uint)num & 4u) != 0)
		{
			_flags = reader.ReadPackedUInt32();
		}
		if (((uint)num & 8u) != 0)
		{
			tagsString = reader.ReadString();
		}
		if (((uint)num & 0x10u) != 0)
		{
			maxPlayers = reader.ReadPackedUInt32();
		}
		if (((uint)num & 0x20u) != 0)
		{
			serverName = reader.ReadString();
		}
	}

	public override void PreStartClient()
	{
	}
}
