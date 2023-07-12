using System;
using System.Collections.Generic;
using Epic.OnlineServices;
using Epic.OnlineServices.Lobby;
using Epic.OnlineServices.UI;
using Facepunch.Steamworks;
using HG;
using RoR2.Networking;
using RoR2.UI;
using UnityEngine;
using UnityEngine.Networking;

namespace RoR2;

public class EOSLobbyManager : PCLobbyManager
{
	public class Filter
	{
		public List<AttributeData> SearchData = new List<AttributeData>();
	}

	private struct LobbyRefreshRequest
	{
		public object requester;

		public Filter filter;

		public Action<List<LobbyDetails>> callback;
	}

	private const MPFeatures PlatformFeatureFlags = MPFeatures.HostGame | MPFeatures.FindGame;

	private const MPLobbyFeatures PlatformLobbyUIFlags = MPLobbyFeatures.CreateLobby | MPLobbyFeatures.SocialIcon | MPLobbyFeatures.HostPromotion | MPLobbyFeatures.Clipboard | MPLobbyFeatures.Invite | MPLobbyFeatures.UserIcon | MPLobbyFeatures.LeaveLobby | MPLobbyFeatures.LobbyDropdownOptions;

	private bool _ownsLobby;

	private int minimumPlayerCount = 2;

	private string currentLobbyId = string.Empty;

	private LobbyDetails currentLobbyDetails;

	private LobbyModification currentLobbyModificationHandle;

	private LobbySearch currentSearchHandle;

	private LobbySearch joinClipboardLobbySearchHandle;

	private const int MaxSearchResults = 50;

	public const string mdEdition = "v";

	public const string mdAppId = "appid";

	public const string mdTotalMaxPlayers = "total_max_players";

	public const string mdPlayerCount = "player_count";

	public const string mdQuickplayQueued = "qp";

	public const string mdQuickplayCutoffTime = "qp_cutoff_time";

	public const string mdStarting = "starting";

	public const string mdBuildId = "build_id";

	public const string mdServerId = "server_id";

	public const string mdServerAddress = "server_address";

	public const string mdMap = "_map";

	public const string mdRuleBook = "rulebook";

	public const string mdMigrationId = "migration_id";

	public const string mdHasPassword = "_pw";

	public const string mdIsDedicatedServer = "_ds";

	public const string mdServerName = "_svnm";

	public const string mdServerTags = "_svtags";

	public const string mdServerMaxPlayers = "_svmpl";

	public const string mdServerPlayerCount = "_svplc";

	public const string mdGameModeName = "_svgm";

	public const string mdModHash = "_mh";

	public const string BucketName = "gbx_internal";

	private LobbyInterface lobbyInterface;

	private readonly List<int> playerCountsList = new List<int>();

	private MemoizedToString<int, ToStringImplementationInvariant> localPlayerCountToString = MemoizedToString<int, ToStringImplementationInvariant>.GetNew();

	private const string LobbyJoinIndicator = "ClipboardJoin";

	private bool startingFadeSet;

	private string lastHostingLobbyId;

	private const float quickplayCutoffTime = 30f;

	private Queue<LobbyRefreshRequest> lobbyRefreshRequests = new Queue<LobbyRefreshRequest>();

	private LobbyRefreshRequest? currentRefreshRequest;

	private bool hostingServer;

	private UserID currentServerId;

	private static readonly char[] charactersToCheck = "abcdef".ToCharArray();

	public override bool isInLobby
	{
		get
		{
			return currentLobbyId != string.Empty;
		}
		protected set
		{
		}
	}

	public override bool ownsLobby
	{
		get
		{
			return _ownsLobby;
		}
		protected set
		{
			if (value != _ownsLobby)
			{
				_ownsLobby = value;
				if (_ownsLobby)
				{
					OnLobbyOwnershipGained();
					UpdatePlayerCount();
				}
				else
				{
					OnLobbyOwnershipLost();
				}
			}
		}
	}

	public override bool hasMinimumPlayerCount => newestLobbyData.totalPlayerCount >= minimumPlayerCount;

	public int remoteMachineCount { get; private set; }

	public string CurrentLobbyId
	{
		get
		{
			return currentLobbyId;
		}
		private set
		{
		}
	}

	public LobbyDetails CurrentLobbyDetails
	{
		get
		{
			return currentLobbyDetails;
		}
		private set
		{
		}
	}

	public LobbyModification CurrentLobbyModification
	{
		get
		{
			return currentLobbyModificationHandle;
		}
		private set
		{
		}
	}

	public UserID serverId => newestLobbyData.serverId;

	public override LobbyData newestLobbyData { get; protected set; }

	public override int calculatedTotalPlayerCount { get; protected set; }

	public override int calculatedExtraPlayersCount { get; protected set; }

	public override LobbyType currentLobbyType
	{
		get
		{
			//IL_000e: Unknown result type (might be due to invalid IL or missing references)
			//IL_0014: Expected O, but got Unknown
			//IL_001a: Unknown result type (might be due to invalid IL or missing references)
			//IL_0026: Expected O, but got Unknown
			//IL_0021: Unknown result type (might be due to invalid IL or missing references)
			//IL_0028: Unknown result type (might be due to invalid IL or missing references)
			if ((Handle)(object)currentLobbyDetails != (Handle)null)
			{
				LobbyDetailsInfo val = new LobbyDetailsInfo();
				currentLobbyDetails.CopyInfo(new LobbyDetailsCopyInfoOptions(), ref val);
				return PermissionLevelToType(val.PermissionLevel);
			}
			return LobbyType.Error;
		}
		set
		{
		}
	}

	public override bool IsBusy { get; set; }

	public override MPFeatures GetPlatformMPFeatureFlags()
	{
		return MPFeatures.HostGame | MPFeatures.FindGame;
	}

	public override MPLobbyFeatures GetPlatformMPLobbyFeatureFlags()
	{
		return MPLobbyFeatures.CreateLobby | MPLobbyFeatures.SocialIcon | MPLobbyFeatures.HostPromotion | MPLobbyFeatures.Clipboard | MPLobbyFeatures.Invite | MPLobbyFeatures.UserIcon | MPLobbyFeatures.LeaveLobby | MPLobbyFeatures.LobbyDropdownOptions;
	}

	public static EOSLobbyManager GetFromPlatformSystems()
	{
		return PlatformSystems.lobbyManager as EOSLobbyManager;
	}

	private void UpdateOwnsLobby()
	{
		//IL_0000: Unknown result type (might be due to invalid IL or missing references)
		//IL_0006: Expected O, but got Unknown
		LobbyDetailsGetLobbyOwnerOptions val = new LobbyDetailsGetLobbyOwnerOptions();
		ownsLobby = (Handle)(object)currentLobbyDetails != (Handle)null && (Handle)(object)currentLobbyDetails.GetLobbyOwner(val) == (Handle)(object)EOSLoginManager.loggedInProductId;
	}

	public void Init()
	{
		lobbyInterface = EOSPlatformManager.GetPlatformInterface().GetLobbyInterface();
		if ((Handle)(object)lobbyInterface == (Handle)null)
		{
			Debug.LogError((object)"Unable to Obtain EOS Lobby Interface!");
		}
		SetupLobbyCallbacks();
		RoR2Application.onUpdate += StaticUpdate;
		LobbyDataSetupState setupState = new LobbyDataSetupState
		{
			totalMaxPlayers = RoR2Application.maxPlayers
		};
		newestLobbyData = new LobbyData(setupState);
		LocalUserManager.onLocalUsersUpdated += UpdatePlayerCount;
		NetworkManagerSystem.onStartServerGlobal += OnStartHostingServer;
		NetworkManagerSystem.onStopServerGlobal += OnStopHostingServer;
		NetworkManagerSystem.onStopClientGlobal += OnStopClient;
		NetworkManagerSystem.onStopClientGlobal += delegate
		{
			SetStartingIfOwner(startingState: false);
		};
		onLobbyOwnershipGained = (Action)Delegate.Combine(onLobbyOwnershipGained, (Action)delegate
		{
			SetStartingIfOwner(startingState: false);
		});
		UserManager.OnDisplayNameMappingComplete += UpdateLobbyNames;
		SetStartingIfOwner(startingState: false);
		SteamworksClientManager.onLoaded += SetSteamLobbyCallbacks;
	}

	public override void Shutdown()
	{
		base.Shutdown();
		lobbyInterface = null;
	}

	private void SetSteamLobbyCallbacks()
	{
		Client steamworksClient = SteamworksClientManager.instance.steamworksClient;
		steamworksClient.Lobby.OnUserInvitedToLobby = OnUserInvitedToSteamLobby;
		steamworksClient.Lobby.OnLobbyJoinRequested = OnSteamLobbyJoinRequested;
	}

	private void OnSteamLobbyJoinRequested(ulong lobbyId)
	{
		Debug.LogFormat("Request to join lobby {0} received but we're in cross-play, rejecting", new object[1] { lobbyId });
		PCLobbyManager.ShowEnableCrossPlayPopup(isLobbyCrossplay: false);
	}

	private void OnUserInvitedToSteamLobby(ulong lobbyId, ulong senderId)
	{
		Debug.LogFormat("Received invitation to lobby {0} from sender {1} but we're in cross play, rejecting", new object[2] { lobbyId, senderId });
	}

	private void UpdateLobbyNames()
	{
		onLobbyDataUpdated?.Invoke();
	}

	private void SetupLobbyCallbacks()
	{
		//IL_0006: Unknown result type (might be due to invalid IL or missing references)
		//IL_0013: Unknown result type (might be due to invalid IL or missing references)
		//IL_001d: Expected O, but got Unknown
		//IL_001d: Expected O, but got Unknown
		//IL_0024: Unknown result type (might be due to invalid IL or missing references)
		//IL_0031: Unknown result type (might be due to invalid IL or missing references)
		//IL_003b: Expected O, but got Unknown
		//IL_003b: Expected O, but got Unknown
		//IL_0042: Unknown result type (might be due to invalid IL or missing references)
		//IL_004f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0059: Expected O, but got Unknown
		//IL_0059: Expected O, but got Unknown
		//IL_0060: Unknown result type (might be due to invalid IL or missing references)
		//IL_006d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0077: Expected O, but got Unknown
		//IL_0077: Expected O, but got Unknown
		lobbyInterface.AddNotifyLobbyUpdateReceived(new AddNotifyLobbyUpdateReceivedOptions(), (object)null, new OnLobbyUpdateReceivedCallback(OnLobbyDataUpdateReceived));
		lobbyInterface.AddNotifyJoinLobbyAccepted(new AddNotifyJoinLobbyAcceptedOptions(), (object)null, new OnJoinLobbyAcceptedCallback(OnJoinLobbyAccepted));
		lobbyInterface.AddNotifyLobbyInviteReceived(new AddNotifyLobbyInviteReceivedOptions(), (object)null, new OnLobbyInviteReceivedCallback(OnUserInvitedToLobby));
		lobbyInterface.AddNotifyLobbyInviteAccepted(new AddNotifyLobbyInviteAcceptedOptions(), (object)null, new OnLobbyInviteAcceptedCallback(OnLobbyInviteAccepted));
	}

	public override int GetLobbyMemberPlayerCountByIndex(int memberIndex)
	{
		if (memberIndex >= playerCountsList.Count)
		{
			return 0;
		}
		return playerCountsList[memberIndex];
	}

	public override void CheckIfInitializedAndValid()
	{
	}

	private void UpdateMemberAttribute(string inKey, string inValue)
	{
		//IL_0034: Unknown result type (might be due to invalid IL or missing references)
		//IL_0039: Unknown result type (might be due to invalid IL or missing references)
		//IL_0045: Expected O, but got Unknown
		//IL_0040: Unknown result type (might be due to invalid IL or missing references)
		//IL_004c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0051: Unknown result type (might be due to invalid IL or missing references)
		//IL_0052: Unknown result type (might be due to invalid IL or missing references)
		//IL_0057: Unknown result type (might be due to invalid IL or missing references)
		//IL_005e: Unknown result type (might be due to invalid IL or missing references)
		//IL_006f: Expected O, but got Unknown
		//IL_006f: Unknown result type (might be due to invalid IL or missing references)
		//IL_007b: Expected O, but got Unknown
		//IL_0076: Unknown result type (might be due to invalid IL or missing references)
		if ((Handle)(object)CurrentLobbyModification != (Handle)null)
		{
			Debug.LogFormat("Setting lobby member attribute {0} to value {1} for user {2}", new object[3]
			{
				inKey,
				inValue,
				EOSLoginManager.loggedInProductId
			});
			CurrentLobbyModification.RemoveMemberAttribute(new LobbyModificationRemoveMemberAttributeOptions
			{
				Key = inKey
			});
			CurrentLobbyModification.AddMemberAttribute(new LobbyModificationAddMemberAttributeOptions
			{
				Attribute = new AttributeData
				{
					Key = inKey,
					Value = AttributeDataValue.op_Implicit(inValue)
				},
				Visibility = (LobbyAttributeVisibility)0
			});
			UpdateLobby(CurrentLobbyModification);
		}
	}

	private void UpdatePlayerCount()
	{
		//IL_0033: Unknown result type (might be due to invalid IL or missing references)
		//IL_003a: Expected O, but got Unknown
		//IL_0040: Unknown result type (might be due to invalid IL or missing references)
		//IL_0045: Unknown result type (might be due to invalid IL or missing references)
		//IL_0050: Unknown result type (might be due to invalid IL or missing references)
		//IL_0062: Expected O, but got Unknown
		//IL_005d: Unknown result type (might be due to invalid IL or missing references)
		//IL_00d1: Unknown result type (might be due to invalid IL or missing references)
		//IL_00db: Expected O, but got Unknown
		//IL_00ea: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ef: Unknown result type (might be due to invalid IL or missing references)
		//IL_00fc: Expected O, but got Unknown
		//IL_00ff: Unknown result type (might be due to invalid IL or missing references)
		//IL_0106: Expected O, but got Unknown
		//IL_010c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0111: Unknown result type (might be due to invalid IL or missing references)
		//IL_011c: Unknown result type (might be due to invalid IL or missing references)
		//IL_012e: Expected O, but got Unknown
		//IL_0129: Unknown result type (might be due to invalid IL or missing references)
		if ((Handle)(object)currentLobbyDetails != (Handle)null)
		{
			int count = LocalUserManager.readOnlyLocalUsersList.Count;
			ref MemoizedToString<int, ToStringImplementationInvariant> reference = ref localPlayerCountToString;
			int input = Math.Max(1, count);
			string @string = reference.GetString(in input);
			Attribute val = new Attribute();
			if ((int)CurrentLobbyDetails.CopyMemberAttributeByKey(new LobbyDetailsCopyMemberAttributeByKeyOptions
			{
				AttrKey = "player_count",
				TargetUserId = EOSLoginManager.loggedInProductId
			}, ref val) == 0)
			{
				if (val.Data.Value.AsUtf8 != @string && (Handle)(object)CurrentLobbyModification != (Handle)null)
				{
					UpdateMemberAttribute("player_count", @string);
				}
			}
			else
			{
				UpdateMemberAttribute("player_count", @string);
			}
			playerCountsList.Clear();
			calculatedTotalPlayerCount = 0;
			remoteMachineCount = 0;
			calculatedExtraPlayersCount = 0;
			ProductUserId loggedInProductId = EOSLoginManager.loggedInProductId;
			uint memberCount = currentLobbyDetails.GetMemberCount(new LobbyDetailsGetMemberCountOptions());
			for (uint num = 0u; num < memberCount; num++)
			{
				ProductUserId memberByIndex = currentLobbyDetails.GetMemberByIndex(new LobbyDetailsGetMemberByIndexOptions
				{
					MemberIndex = num
				});
				int result = 1;
				Attribute val2 = new Attribute();
				if ((int)CurrentLobbyDetails.CopyMemberAttributeByKey(new LobbyDetailsCopyMemberAttributeByKeyOptions
				{
					AttrKey = "player_count",
					TargetUserId = EOSLoginManager.loggedInProductId
				}, ref val2) == 0)
				{
					result = ((!TextSerialization.TryParseInvariant(val2.Data.Value.AsUtf8, out result)) ? 1 : Math.Max(1, result));
				}
				if ((Handle)(object)memberByIndex == (Handle)(object)loggedInProductId)
				{
					result = Math.Max(1, count);
				}
				else
				{
					input = remoteMachineCount + 1;
					remoteMachineCount = input;
				}
				playerCountsList.Add(result);
				calculatedTotalPlayerCount += result;
				if (result > 1)
				{
					calculatedExtraPlayersCount += result - 1;
				}
			}
		}
		onPlayerCountUpdated?.Invoke();
	}

	private void OnLobbyChanged()
	{
		OnLobbyDataUpdated();
		onLobbyChanged?.Invoke();
		(PlatformSystems.userManager as UserManagerEOS).QueryForDisplayNames(GetLobbyMembers());
	}

	public override void CreateLobby()
	{
		//IL_0000: Unknown result type (might be due to invalid IL or missing references)
		//IL_0005: Unknown result type (might be due to invalid IL or missing references)
		//IL_0010: Unknown result type (might be due to invalid IL or missing references)
		//IL_001b: Unknown result type (might be due to invalid IL or missing references)
		//IL_002e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0039: Unknown result type (might be due to invalid IL or missing references)
		//IL_0040: Unknown result type (might be due to invalid IL or missing references)
		//IL_0048: Expected O, but got Unknown
		//IL_0063: Unknown result type (might be due to invalid IL or missing references)
		//IL_006d: Expected O, but got Unknown
		CreateLobbyOptions val = new CreateLobbyOptions
		{
			LocalUserId = EOSLoginManager.loggedInProductId,
			MaxLobbyMembers = (uint)RoR2Application.maxPlayers,
			PermissionLevel = (LobbyPermissionLevel)(preferredLobbyType != LobbyType.Public),
			BucketId = "gbx_internal",
			PresenceEnabled = true,
			AllowInvites = true
		};
		base.awaitingCreate = true;
		LobbyInterface obj = lobbyInterface;
		if (obj != null)
		{
			obj.CreateLobby(val, (object)null, new OnCreateLobbyCallback(OnLobbyCreated));
		}
	}

	public override void JoinLobby(UserID uid)
	{
		LeaveLobby(delegate
		{
			//IL_0000: Unknown result type (might be due to invalid IL or missing references)
			//IL_0005: Unknown result type (might be due to invalid IL or missing references)
			//IL_0011: Expected O, but got Unknown
			//IL_0025: Unknown result type (might be due to invalid IL or missing references)
			//IL_002f: Expected O, but got Unknown
			JoinLobbyOptions val = new JoinLobbyOptions
			{
				LocalUserId = EOSLoginManager.loggedInProductId
			};
			LobbyInterface obj = lobbyInterface;
			if (obj != null)
			{
				obj.JoinLobby(val, (object)null, new OnJoinLobbyCallback(OnLobbyJoined));
			}
		});
		base.awaitingJoin = true;
	}

	public void JoinLobby(LobbyDetails lobbyID)
	{
		LeaveLobby(delegate
		{
			//IL_0000: Unknown result type (might be due to invalid IL or missing references)
			//IL_0005: Unknown result type (might be due to invalid IL or missing references)
			//IL_0010: Unknown result type (might be due to invalid IL or missing references)
			//IL_001d: Expected O, but got Unknown
			//IL_003b: Unknown result type (might be due to invalid IL or missing references)
			//IL_0045: Expected O, but got Unknown
			JoinLobbyOptions val = new JoinLobbyOptions
			{
				LocalUserId = EOSLoginManager.loggedInProductId,
				LobbyDetailsHandle = lobbyID
			};
			LobbyInterface obj = lobbyInterface;
			if (obj != null)
			{
				obj.JoinLobby(val, (object)null, new OnJoinLobbyCallback(OnLobbyJoined));
			}
		});
		base.awaitingJoin = true;
	}

	public void FindClipboardLobby(string lobbyId)
	{
		//IL_0021: Unknown result type (might be due to invalid IL or missing references)
		//IL_002b: Expected O, but got Unknown
		//IL_0040: Unknown result type (might be due to invalid IL or missing references)
		//IL_0045: Unknown result type (might be due to invalid IL or missing references)
		//IL_0057: Expected O, but got Unknown
		//IL_0052: Unknown result type (might be due to invalid IL or missing references)
		//IL_0060: Unknown result type (might be due to invalid IL or missing references)
		//IL_0063: Unknown result type (might be due to invalid IL or missing references)
		//IL_0068: Unknown result type (might be due to invalid IL or missing references)
		//IL_0098: Unknown result type (might be due to invalid IL or missing references)
		//IL_009d: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a9: Expected O, but got Unknown
		//IL_00a4: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b0: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b5: Unknown result type (might be due to invalid IL or missing references)
		//IL_00cc: Unknown result type (might be due to invalid IL or missing references)
		//IL_00d6: Expected O, but got Unknown
		//IL_00d6: Expected O, but got Unknown
		if ((Handle)(object)joinClipboardLobbySearchHandle != (Handle)null)
		{
			joinClipboardLobbySearchHandle.Release();
			joinClipboardLobbySearchHandle = null;
		}
		joinClipboardLobbySearchHandle = new LobbySearch();
		LobbyInterface obj = lobbyInterface;
		Result? val = ((obj != null) ? new Result?(obj.CreateLobbySearch(new CreateLobbySearchOptions
		{
			MaxResults = 1u
		}, ref joinClipboardLobbySearchHandle)) : null);
		if (val != (Result?)0)
		{
			Debug.Log((object)("Unable to create lobby search for joining lobby by ID, result = " + val.ToString()));
			return;
		}
		joinClipboardLobbySearchHandle.SetLobbyId(new LobbySearchSetLobbyIdOptions
		{
			LobbyId = lobbyId
		});
		joinClipboardLobbySearchHandle.Find(new LobbySearchFindOptions
		{
			LocalUserId = EOSLoginManager.loggedInProductId
		}, (object)"ClipboardJoin", new LobbySearchOnFindCallback(OnLobbySearchComplete));
	}

	public override void LeaveLobby()
	{
		//IL_0000: Unknown result type (might be due to invalid IL or missing references)
		//IL_0005: Unknown result type (might be due to invalid IL or missing references)
		//IL_0010: Unknown result type (might be due to invalid IL or missing references)
		//IL_001d: Expected O, but got Unknown
		//IL_0032: Unknown result type (might be due to invalid IL or missing references)
		//IL_003c: Expected O, but got Unknown
		LeaveLobbyOptions val = new LeaveLobbyOptions
		{
			LocalUserId = EOSLoginManager.loggedInProductId,
			LobbyId = currentLobbyId
		};
		LobbyInterface obj = lobbyInterface;
		if (obj != null)
		{
			obj.LeaveLobby(val, (object)null, new OnLeaveLobbyCallback(OnLobbyLeave));
		}
		currentLobbyId = string.Empty;
	}

	private void LeaveLobby(Action callback)
	{
		//IL_0014: Unknown result type (might be due to invalid IL or missing references)
		//IL_0019: Unknown result type (might be due to invalid IL or missing references)
		//IL_0024: Unknown result type (might be due to invalid IL or missing references)
		//IL_0031: Expected O, but got Unknown
		//IL_0046: Unknown result type (might be due to invalid IL or missing references)
		//IL_0050: Expected O, but got Unknown
		LeaveLobbyOptions val = new LeaveLobbyOptions
		{
			LocalUserId = EOSLoginManager.loggedInProductId,
			LobbyId = currentLobbyId
		};
		LobbyInterface obj = lobbyInterface;
		if (obj != null)
		{
			obj.LeaveLobby(val, (object)null, (OnLeaveLobbyCallback)delegate(LeaveLobbyCallbackInfo leaveLobbyCallbackInfo)
			{
				OnLobbyLeave(leaveLobbyCallbackInfo);
				callback?.Invoke();
			});
		}
		currentLobbyId = string.Empty;
	}

	public override UserID[] GetLobbyMembers()
	{
		//IL_0015: Unknown result type (might be due to invalid IL or missing references)
		//IL_001f: Expected O, but got Unknown
		//IL_0049: Unknown result type (might be due to invalid IL or missing references)
		//IL_004e: Unknown result type (might be due to invalid IL or missing references)
		//IL_005a: Expected O, but got Unknown
		LobbyDetails obj = CurrentLobbyDetails;
		uint? num = ((obj != null) ? new uint?(obj.GetMemberCount(new LobbyDetailsGetMemberCountOptions())) : null);
		UserID[] array = null;
		if (num.HasValue)
		{
			array = new UserID[num.Value];
			for (uint num2 = 0u; num2 < array.Length; num2++)
			{
				array[num2] = new UserID(CurrentLobbyDetails.GetMemberByIndex(new LobbyDetailsGetMemberByIndexOptions
				{
					MemberIndex = num2
				}));
			}
		}
		return array;
	}

	public override bool ShouldShowPromoteButton()
	{
		return false;
	}

	private void Update()
	{
		if (startingFadeSet != (newestLobbyData.starting && !ClientScene.ready))
		{
			if (startingFadeSet)
			{
				FadeToBlackManager.fadeCount--;
			}
			else
			{
				FadeToBlackManager.fadeCount++;
			}
			startingFadeSet = !startingFadeSet;
		}
	}

	private void StaticUpdate()
	{
		UpdateOwnsLobby();
	}

	private CSteamID GetLaunchParamsLobbyId()
	{
		string[] commandLineArgs = Environment.GetCommandLineArgs();
		for (int i = 0; i < commandLineArgs.Length - 1; i++)
		{
			if (string.Equals(commandLineArgs[i], "+connect_lobby", StringComparison.OrdinalIgnoreCase) && CSteamID.TryParse(ArrayUtils.GetSafe<string>(commandLineArgs, i + 1, ref string.Empty), out var result))
			{
				return result;
			}
		}
		return CSteamID.nil;
	}

	public void ForceLobbyDataUpdate()
	{
	}

	public void SendLobbyMessage(LobbyMessageType messageType, NetworkWriter writer)
	{
		byte[] array = new byte[1 + writer.Position];
		array[0] = (byte)messageType;
		Array.Copy(writer.AsArray(), 0, array, 1, writer.Position);
	}

	public override void SetQuickplayCutoffTime(double cutoffTime)
	{
	}

	public override double GetQuickplayCutoffTime()
	{
		return 0.0;
	}

	public override void OnCutoffTimerComplete()
	{
	}

	private void OnChatMessageReceived(ulong senderId, byte[] buffer, int byteCount)
	{
		//IL_0001: Unknown result type (might be due to invalid IL or missing references)
		//IL_0007: Expected O, but got Unknown
		try
		{
			NetworkReader val = new NetworkReader(buffer);
			if (byteCount >= 1)
			{
				LobbyMessageType lobbyMessageType = (LobbyMessageType)val.ReadByte();
				Debug.LogFormat("Received Steamworks Lobby Message from {0} ({1}B). messageType={2}", new object[3] { senderId, byteCount, lobbyMessageType });
				switch (lobbyMessageType)
				{
				}
			}
			else
			{
				Debug.LogWarningFormat("Received SteamworksLobbyMessage from {0}, but the message was empty.", Array.Empty<object>());
			}
		}
		catch (Exception ex)
		{
			Debug.LogError((object)ex);
		}
	}

	private void OnLobbyCreated(CreateLobbyCallbackInfo data)
	{
		//IL_0008: Unknown result type (might be due to invalid IL or missing references)
		//IL_002e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0033: Unknown result type (might be due to invalid IL or missing references)
		base.awaitingCreate = false;
		if ((int)data.ResultCode == 0)
		{
			currentLobbyId = data.LobbyId;
			TryGetLobbyDetails();
			OnLobbyChanged();
		}
		else
		{
			Result resultCode = data.ResultCode;
			Debug.Log((object)("EOS lobby creation failed, result = " + ((object)(Result)(ref resultCode)).ToString()));
		}
	}

	protected void OnLobbyDataUpdated()
	{
		UpdateNewestLobbyData();
		UpdateOwnsLobby();
		UpdatePlayerCount();
		if ((Handle)(object)currentLobbyDetails != (Handle)null && !ownsLobby)
		{
			if (newestLobbyData.serverId.isValid)
			{
				bool num = newestLobbyData.serverId == new UserID(NetworkManagerSystem.singleton.serverP2PId) || NetworkManagerSystem.singleton.IsConnectedToServer(newestLobbyData.serverId);
				bool flag = string.CompareOrdinal(RoR2Application.GetBuildId(), newestLobbyData.buildId) == 0;
				if (!num && flag)
				{
					NetworkManagerSystem.singleton.desiredHost = new HostDescription(newestLobbyData.serverId, HostDescription.HostType.EOS);
					lastHostingLobbyId = currentLobbyId;
				}
			}
			else if (lastHostingLobbyId == currentLobbyId)
			{
				Debug.LogFormat("Intercepting bad or out-of-order lobby update to server id.", Array.Empty<object>());
			}
			else
			{
				NetworkManagerSystem.singleton.desiredHost = HostDescription.none;
			}
		}
		onLobbyDataUpdated?.Invoke();
	}

	private void UpdateNewestLobbyData()
	{
		DateTime value = Util.UnixTimeStampToDateTimeUtc((uint)Util.GetCurrentUnixEpochTimeInSeconds()) + TimeSpan.FromSeconds(30.0);
		if ((Handle)(object)currentLobbyDetails != (Handle)null)
		{
			LobbyDataSetupState lobbyDataSetupState = new LobbyDataSetupState
			{
				totalMaxPlayers = RoR2Application.maxPlayers,
				totalPlayerCount = calculatedTotalPlayerCount,
				quickplayQueued = false,
				starting = GetLobbyBoolValue(currentLobbyDetails, "starting"),
				buildId = GetLobbyStringValue(currentLobbyDetails, "build_id"),
				quickplayCutoffTime = value,
				shouldConnect = false,
				joinable = (calculatedTotalPlayerCount < RoR2Application.maxPlayers)
			};
			string lobbyStringValue = GetLobbyStringValue(currentLobbyDetails, "server_id");
			if (lobbyStringValue != string.Empty)
			{
				lobbyDataSetupState.serverId = new UserID(new CSteamID(lobbyStringValue));
			}
			newestLobbyData = new LobbyData(lobbyDataSetupState);
		}
		else
		{
			newestLobbyData = new LobbyData();
		}
	}

	private void OnLobbyJoined(JoinLobbyCallbackInfo data)
	{
		//IL_0008: Unknown result type (might be due to invalid IL or missing references)
		//IL_000e: Invalid comparison between Unknown and I4
		base.awaitingJoin = false;
		bool flag = (int)data.ResultCode == 0;
		if (flag)
		{
			currentLobbyId = data.LobbyId;
			TryGetLobbyDetails();
			if ((Handle)(object)currentLobbyDetails != (Handle)null)
			{
				string buildId = RoR2Application.GetBuildId();
				string lobbyStringValue = GetLobbyStringValue(currentLobbyDetails, "build_id");
				if (buildId != lobbyStringValue)
				{
					Debug.LogFormat("Lobby build_id mismatch, leaving lobby. Ours=\"{0}\" Theirs=\"{1}\"", new object[2] { buildId, lobbyStringValue });
					SimpleDialogBox simpleDialogBox = SimpleDialogBox.Create();
					simpleDialogBox.AddCancelButton(CommonLanguageTokens.ok);
					simpleDialogBox.headerToken = new SimpleDialogBox.TokenParamsPair
					{
						token = "STEAM_LOBBY_VERSION_MISMATCH_DIALOG_TITLE",
						formatParams = Array.Empty<object>()
					};
					SimpleDialogBox.TokenParamsPair descriptionToken = new SimpleDialogBox.TokenParamsPair
					{
						token = "STEAM_LOBBY_VERSION_MISMATCH_DIALOG_DESCRIPTION"
					};
					object[] formatParams = new string[2] { buildId, lobbyStringValue };
					descriptionToken.formatParams = formatParams;
					simpleDialogBox.descriptionToken = descriptionToken;
					return;
				}
			}
			Debug.LogFormat("lobby join succeeded. Lobby id = {0}", new object[1] { currentLobbyId });
			OnLobbyChanged();
		}
		else
		{
			Debug.Log((object)"Steamworks lobby join failed.");
			Console.instance.SubmitCmd(null, "steam_lobby_create_if_none", recordSubmit: true);
		}
		onLobbyJoined?.Invoke(flag);
	}

	private void OnLobbyMemberDataUpdated(UserID memberId)
	{
		UpdateOwnsLobby();
		onLobbyMemberDataUpdated?.Invoke(memberId);
	}

	private void OnLobbyKicked(bool kickedDueToDisconnect, ulong lobbyId, ulong adminId)
	{
		Debug.LogFormat("Kicked from lobby. kickedDueToDisconnect={0} lobbyId={1} adminId={2}", new object[3] { kickedDueToDisconnect, lobbyId, adminId });
		OnLobbyChanged();
	}

	private void OnLobbyLeave(LeaveLobbyCallbackInfo data)
	{
		Debug.LogFormat("Left lobby {0}.", new object[1] { data.LobbyId });
		currentLobbyId = string.Empty;
		if ((Handle)(object)currentLobbyDetails != (Handle)null)
		{
			currentLobbyDetails.Release();
			currentLobbyDetails = null;
		}
		if ((Handle)(object)currentLobbyModificationHandle != (Handle)null)
		{
			currentLobbyModificationHandle.Release();
			currentLobbyModificationHandle = null;
		}
		OnLobbyChanged();
	}

	private void OnLobbyJoinRequested(ulong lobbyId)
	{
		Debug.LogFormat("Request to join lobby {0} received. Attempting to join lobby.", new object[1] { lobbyId });
	}

	private void OnUserInvitedToLobby(LobbyInviteReceivedCallbackInfo data)
	{
		Debug.LogFormat("Received invitation to lobby {0} from sender {1}.", new object[2] { data.InviteId, data.TargetUserId });
	}

	private static void OnSendInviteComplete(SendInviteCallbackInfo data)
	{
		//IL_0008: Unknown result type (might be due to invalid IL or missing references)
		//IL_000b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0010: Unknown result type (might be due to invalid IL or missing references)
		if (data.GetResultCode() != (Result?)0)
		{
			Debug.Log((object)("Unable to send invite!  LobbyID = " + data.LobbyId));
		}
	}

	private void OnLobbySearchComplete(LobbySearchFindCallbackInfo data)
	{
		//IL_0008: Unknown result type (might be due to invalid IL or missing references)
		//IL_000b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0010: Unknown result type (might be due to invalid IL or missing references)
		//IL_005b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0065: Expected O, but got Unknown
		//IL_006c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0073: Expected O, but got Unknown
		//IL_0075: Unknown result type (might be due to invalid IL or missing references)
		//IL_007a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0089: Expected O, but got Unknown
		//IL_0084: Unknown result type (might be due to invalid IL or missing references)
		if (data.GetResultCode() != (Result?)0)
		{
			return;
		}
		bool flag = data.ClientData as string == "ClipboardJoin";
		List<LobbyDetails> list = new List<LobbyDetails>();
		LobbySearch val = (flag ? joinClipboardLobbySearchHandle : currentSearchHandle);
		if ((Handle)(object)val != (Handle)null)
		{
			uint searchResultCount = val.GetSearchResultCount(new LobbySearchGetSearchResultCountOptions());
			for (uint num = 0u; num < searchResultCount; num++)
			{
				LobbyDetails val2 = new LobbyDetails();
				if ((int)val.CopySearchResultByIndex(new LobbySearchCopySearchResultByIndexOptions
				{
					LobbyIndex = num
				}, ref val2) == 0)
				{
					if (flag)
					{
						JoinLobby(val2);
						joinClipboardLobbySearchHandle.Release();
						joinClipboardLobbySearchHandle = null;
						return;
					}
					list.Add(val2);
				}
			}
		}
		(data.ClientData as Action<List<LobbyDetails>>)(list);
	}

	private void OnLobbyDataUpdateReceived(LobbyUpdateReceivedCallbackInfo data)
	{
		if (data.LobbyId == currentLobbyId)
		{
			TryGetLobbyDetails();
			OnLobbyChanged();
		}
	}

	private void OnJoinLobbyAccepted(JoinLobbyAcceptedCallbackInfo data)
	{
		//IL_0027: Unknown result type (might be due to invalid IL or missing references)
		//IL_002d: Expected O, but got Unknown
		//IL_0042: Unknown result type (might be due to invalid IL or missing references)
		//IL_0047: Unknown result type (might be due to invalid IL or missing references)
		//IL_005a: Expected O, but got Unknown
		//IL_0055: Unknown result type (might be due to invalid IL or missing references)
		//IL_0063: Unknown result type (might be due to invalid IL or missing references)
		//IL_0066: Unknown result type (might be due to invalid IL or missing references)
		//IL_006b: Unknown result type (might be due to invalid IL or missing references)
		Debug.LogFormat("Attempting to join from ui event {0} from local user {1}.", new object[2] { data.UiEventId, data.LocalUserId });
		LobbyDetails lobbyID = new LobbyDetails();
		LobbyInterface obj = lobbyInterface;
		Result? val = ((obj != null) ? new Result?(obj.CopyLobbyDetailsHandleByUiEventId(new CopyLobbyDetailsHandleByUiEventIdOptions
		{
			UiEventId = data.UiEventId
		}, ref lobbyID)) : null);
		if (val == (Result?)0)
		{
			JoinLobby(lobbyID);
		}
		else
		{
			Debug.LogFormat("failed getting lobby details from join callback, result = " + val.ToString(), Array.Empty<object>());
		}
	}

	private void OnLobbyInviteAccepted(LobbyInviteAcceptedCallbackInfo data)
	{
		//IL_0022: Unknown result type (might be due to invalid IL or missing references)
		//IL_0028: Expected O, but got Unknown
		//IL_003d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0042: Unknown result type (might be due to invalid IL or missing references)
		//IL_0055: Expected O, but got Unknown
		//IL_0050: Unknown result type (might be due to invalid IL or missing references)
		//IL_005e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0061: Unknown result type (might be due to invalid IL or missing references)
		//IL_0066: Unknown result type (might be due to invalid IL or missing references)
		Debug.LogFormat("Accepted invitation to lobby {0} from sender {1}.", new object[2] { data.InviteId, data.TargetUserId });
		LobbyDetails lobbyID = new LobbyDetails();
		LobbyInterface obj = lobbyInterface;
		Result? val = ((obj != null) ? new Result?(obj.CopyLobbyDetailsHandleByInviteId(new CopyLobbyDetailsHandleByInviteIdOptions
		{
			InviteId = data.InviteId
		}, ref lobbyID)) : null);
		if (val == (Result?)0)
		{
			JoinLobby(lobbyID);
		}
		else
		{
			Debug.LogFormat("failed getting lobby details from invite, result = " + val.ToString(), Array.Empty<object>());
		}
	}

	private void OnShowFriendsComplete(ShowFriendsCallbackInfo data)
	{
		//IL_000a: Unknown result type (might be due to invalid IL or missing references)
		//IL_000d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0012: Unknown result type (might be due to invalid IL or missing references)
		Result? resultCode = data.GetResultCode();
		if (resultCode != (Result?)0)
		{
			Debug.Log((object)("Failed to show friends list, result = " + resultCode.ToString()));
		}
	}

	private void OnLobbyUpdated(UpdateLobbyCallbackInfo data)
	{
		//IL_000a: Unknown result type (might be due to invalid IL or missing references)
		//IL_000d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0012: Unknown result type (might be due to invalid IL or missing references)
		Result? resultCode = data.GetResultCode();
		if (resultCode != (Result?)0)
		{
			Debug.Log((object)("Failed to successfully update lobby.  Result = " + resultCode.ToString()));
		}
		else
		{
			OnLobbyChanged();
		}
	}

	private void TryGetLobbyDetails()
	{
		//IL_0035: Unknown result type (might be due to invalid IL or missing references)
		//IL_003a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0046: Unknown result type (might be due to invalid IL or missing references)
		//IL_005c: Expected O, but got Unknown
		//IL_0057: Unknown result type (might be due to invalid IL or missing references)
		//IL_0065: Unknown result type (might be due to invalid IL or missing references)
		//IL_0068: Unknown result type (might be due to invalid IL or missing references)
		//IL_006d: Unknown result type (might be due to invalid IL or missing references)
		if ((Handle)(object)currentLobbyDetails != (Handle)null)
		{
			currentLobbyDetails.Release();
			currentLobbyDetails = null;
		}
		LobbyInterface obj = lobbyInterface;
		Result? val = ((obj != null) ? new Result?(obj.CopyLobbyDetailsHandle(new CopyLobbyDetailsHandleOptions
		{
			LobbyId = currentLobbyId,
			LocalUserId = EOSLoginManager.loggedInProductId
		}, ref currentLobbyDetails)) : null);
		if (val != (Result?)0)
		{
			Debug.Log((object)("Failed to get details for lobby: " + CurrentLobbyId + " result = " + val.ToString()));
		}
		TryGetLobbyModificationHandle();
	}

	private void TryGetLobbyModificationHandle()
	{
		//IL_0035: Unknown result type (might be due to invalid IL or missing references)
		//IL_003a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0046: Unknown result type (might be due to invalid IL or missing references)
		//IL_005c: Expected O, but got Unknown
		//IL_0057: Unknown result type (might be due to invalid IL or missing references)
		//IL_0065: Unknown result type (might be due to invalid IL or missing references)
		//IL_0068: Unknown result type (might be due to invalid IL or missing references)
		//IL_006d: Unknown result type (might be due to invalid IL or missing references)
		if ((Handle)(object)currentLobbyModificationHandle != (Handle)null)
		{
			currentLobbyModificationHandle.Release();
			currentLobbyModificationHandle = null;
		}
		LobbyInterface obj = lobbyInterface;
		Result? val = ((obj != null) ? new Result?(obj.UpdateLobbyModification(new UpdateLobbyModificationOptions
		{
			LobbyId = currentLobbyId,
			LocalUserId = EOSLoginManager.loggedInProductId
		}, ref currentLobbyModificationHandle)) : null);
		if (val != (Result?)0)
		{
			Debug.Log((object)("Failed to get modification handle for lobby: " + CurrentLobbyId + " result = " + val.ToString()));
		}
	}

	public bool RequestLobbyList(object requester, Filter filter, Action<List<LobbyDetails>> callback)
	{
		if (requester != null)
		{
			foreach (LobbyRefreshRequest lobbyRefreshRequest2 in lobbyRefreshRequests)
			{
				if (requester == lobbyRefreshRequest2.requester)
				{
					return false;
				}
			}
		}
		LobbyRefreshRequest lobbyRefreshRequest = default(LobbyRefreshRequest);
		lobbyRefreshRequest.requester = requester;
		lobbyRefreshRequest.filter = filter;
		lobbyRefreshRequest.callback = callback;
		LobbyRefreshRequest item = lobbyRefreshRequest;
		lobbyRefreshRequests.Enqueue(item);
		UpdateRefreshRequestQueue();
		return true;
	}

	private void UpdateRefreshRequestQueue()
	{
		if (!currentRefreshRequest.HasValue && lobbyRefreshRequests.Count != 0)
		{
			SearchForLobbiesWithRequest(lobbyRefreshRequests.Dequeue());
		}
	}

	private void SearchForLobbiesWithRequest(LobbyRefreshRequest request)
	{
		//IL_0020: Unknown result type (might be due to invalid IL or missing references)
		//IL_0025: Unknown result type (might be due to invalid IL or missing references)
		//IL_002e: Expected O, but got Unknown
		//IL_002f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0039: Expected O, but got Unknown
		//IL_0055: Unknown result type (might be due to invalid IL or missing references)
		//IL_0063: Unknown result type (might be due to invalid IL or missing references)
		//IL_0066: Unknown result type (might be due to invalid IL or missing references)
		//IL_006b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0081: Unknown result type (might be due to invalid IL or missing references)
		//IL_0086: Unknown result type (might be due to invalid IL or missing references)
		//IL_008d: Unknown result type (might be due to invalid IL or missing references)
		//IL_008e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0093: Unknown result type (might be due to invalid IL or missing references)
		//IL_009e: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b3: Expected O, but got Unknown
		//IL_00b8: Expected O, but got Unknown
		//IL_00b3: Unknown result type (might be due to invalid IL or missing references)
		//IL_00dc: Unknown result type (might be due to invalid IL or missing references)
		//IL_00e1: Unknown result type (might be due to invalid IL or missing references)
		//IL_00e8: Unknown result type (might be due to invalid IL or missing references)
		//IL_00f5: Expected O, but got Unknown
		//IL_00f0: Unknown result type (might be due to invalid IL or missing references)
		//IL_0115: Unknown result type (might be due to invalid IL or missing references)
		//IL_011a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0132: Unknown result type (might be due to invalid IL or missing references)
		//IL_013c: Expected O, but got Unknown
		//IL_013c: Expected O, but got Unknown
		if ((Handle)(object)currentSearchHandle != (Handle)null)
		{
			currentSearchHandle.Release();
			currentSearchHandle = null;
		}
		CreateLobbySearchOptions val = new CreateLobbySearchOptions
		{
			MaxResults = 50u
		};
		currentSearchHandle = new LobbySearch();
		LobbyInterface obj = lobbyInterface;
		Result? val2 = ((obj != null) ? new Result?(obj.CreateLobbySearch(val, ref currentSearchHandle)) : null);
		if (val2 == (Result?)0)
		{
			currentSearchHandle.SetParameter(new LobbySearchSetParameterOptions
			{
				ComparisonOp = (ComparisonOp)0,
				Parameter = new AttributeData
				{
					Key = "build_id",
					Value = AttributeDataValue.op_Implicit(RoR2Application.GetBuildId())
				}
			});
			foreach (AttributeData searchDatum in request.filter.SearchData)
			{
				currentSearchHandle.SetParameter(new LobbySearchSetParameterOptions
				{
					ComparisonOp = (ComparisonOp)0,
					Parameter = searchDatum
				});
			}
			currentSearchHandle.Find(new LobbySearchFindOptions
			{
				LocalUserId = EOSLoginManager.loggedInProductId
			}, (object)request.callback, new LobbySearchOnFindCallback(OnLobbySearchComplete));
		}
		else
		{
			Debug.LogError((object)("Error Creating Lobby Search Handle! " + val2.ToString()));
		}
	}

	private void OnStopClient()
	{
		//IL_0087: Unknown result type (might be due to invalid IL or missing references)
		//IL_008c: Unknown result type (might be due to invalid IL or missing references)
		//IL_009c: Expected O, but got Unknown
		//IL_0097: Unknown result type (might be due to invalid IL or missing references)
		NetworkConnection connection = ((NetworkManager)NetworkManagerSystem.singleton).client.connection;
		bool flag = false;
		bool flag2 = Util.ConnectionIsLocal(connection);
		flag = ((!(connection is EOSNetworkConnection)) ? (connection.address == newestLobbyData.serverAddressPortPair.address) : ((Handle)(object)((EOSNetworkConnection)(object)connection).RemoteUserID == (Handle)(object)newestLobbyData.serverId.CID.egsValue));
		if (flag2 && ownsLobby && (Handle)(object)currentLobbyModificationHandle != (Handle)null)
		{
			currentLobbyModificationHandle.RemoveAttribute(new LobbyModificationRemoveAttributeOptions
			{
				Key = "server_id"
			});
		}
		if (!flag2 && flag)
		{
			LeaveLobby();
		}
	}

	private void OnStartHostingServer()
	{
		hostingServer = true;
	}

	private void OnStopHostingServer()
	{
		hostingServer = false;
	}

	public void JoinOrStartMigrate(UserID newLobbyId)
	{
		if (ownsLobby)
		{
			StartMigrateLobby(newLobbyId.CID.egsValue);
		}
		else
		{
			((LobbyManager)this).JoinLobby(newLobbyId);
		}
	}

	public void StartMigrateLobby(ProductUserId newLobbyId)
	{
	}

	private void AttemptToJoinPendingSteamworksLobby()
	{
		_ = LocalUserManager.isAnyUserSignedIn;
	}

	public void SetLobbyQuickPlayQueuedIfOwner(bool quickplayQueuedState)
	{
	}

	public void SetLobbyQuickPlayCutoffTimeIfOwner(uint? timestamp)
	{
	}

	public void SetStartingIfOwner(bool startingState)
	{
	}

	protected void OnLobbyOwnershipGained()
	{
		onLobbyOwnershipGained?.Invoke();
	}

	private void OnLobbyOwnershipLost()
	{
		onLobbyOwnershipLost?.Invoke();
	}

	public override bool IsLobbyOwner(UserID user)
	{
		//IL_0014: Unknown result type (might be due to invalid IL or missing references)
		//IL_001e: Expected O, but got Unknown
		if ((Handle)(object)currentLobbyDetails != (Handle)null)
		{
			return (Handle)(object)currentLobbyDetails.GetLobbyOwner(new LobbyDetailsGetLobbyOwnerOptions()) == (Handle)(object)user.CID.egsValue;
		}
		return false;
	}

	public override void AutoMatchmake()
	{
	}

	public override bool IsLobbyOwner()
	{
		if (isInLobby)
		{
			return _ownsLobby;
		}
		return false;
	}

	public override bool CanInvite()
	{
		return !IsBusy;
	}

	public override void OpenInviteOverlay()
	{
		//IL_000f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0014: Unknown result type (might be due to invalid IL or missing references)
		//IL_0027: Unknown result type (might be due to invalid IL or missing references)
		//IL_0031: Expected O, but got Unknown
		//IL_0031: Expected O, but got Unknown
		UIInterface uIInterface = EOSPlatformManager.GetPlatformInterface().GetUIInterface();
		if (uIInterface != null)
		{
			uIInterface.ShowFriends(new ShowFriendsOptions
			{
				LocalUserId = EOSLoginManager.loggedInAuthId
			}, (object)null, new OnShowFriendsCallback(OnShowFriendsComplete));
		}
	}

	public override void OnStartPrivateGame()
	{
	}

	public override void ToggleQuickplay()
	{
	}

	public override void CheckIfInvited()
	{
	}

	public override void CheckBusyTimer()
	{
	}

	public override bool ShouldEnableQuickplayButton()
	{
		return false;
	}

	public override bool ShouldEnableStartPrivateGameButton()
	{
		return true;
	}

	public override string GetUserDisplayName(UserID user)
	{
		if ((Handle)(object)user.CID.egsValue != (Handle)null)
		{
			string userDisplayName = (PlatformSystems.userManager as UserManagerEOS).GetUserDisplayName(user);
			if (userDisplayName != string.Empty)
			{
				return userDisplayName;
			}
		}
		return "";
	}

	public string GetUserDisplayNameFromProductIdString(string productIdString)
	{
		if (productIdString != string.Empty)
		{
			string userDisplayName = (PlatformSystems.userManager as UserManagerEOS).GetUserDisplayName(new UserID(new CSteamID(productIdString)));
			if (userDisplayName != string.Empty)
			{
				return userDisplayName;
			}
		}
		return "";
	}

	private static LobbyType PermissionLevelToType(LobbyPermissionLevel permissionLevel)
	{
		//IL_0000: Unknown result type (might be due to invalid IL or missing references)
		//IL_0012: Expected I4, but got Unknown
		return (int)permissionLevel switch
		{
			2 => LobbyType.Private, 
			0 => LobbyType.Public, 
			1 => LobbyType.FriendsOnly, 
			_ => LobbyType.Error, 
		};
	}

	private static LobbyPermissionLevel LobbyTypeToPermissionLevel(LobbyType type)
	{
		return (LobbyPermissionLevel)(type switch
		{
			LobbyType.FriendsOnly => 1, 
			LobbyType.Private => 1, 
			LobbyType.Public => 0, 
			_ => 1, 
		});
	}

	public override string GetLobbyID()
	{
		return currentLobbyId;
	}

	public override bool CheckLobbyIdValidity(string lobbyID)
	{
		if (lobbyID.IndexOfAny(charactersToCheck) == -1)
		{
			return false;
		}
		return true;
	}

	public override void JoinLobby(ConCommandArgs args)
	{
		CheckIfInitializedAndValid();
		string text = args[0];
		Debug.LogFormat("Enqueuing join for lobby {0}...", new object[1] { text });
		FindClipboardLobby(text);
	}

	public override void LobbyCreate(ConCommandArgs args)
	{
		EOSLobbyManager obj = PlatformSystems.lobbyManager as EOSLobbyManager;
		obj.CheckIfInitializedAndValid();
		if (!LocalUserManager.isAnyUserSignedIn)
		{
			throw new ConCommandException("Cannot create a Steamworks lobby without any local users signed in.");
		}
		obj.CreateLobby();
	}

	public override void LobbyCreateIfNone(ConCommandArgs args)
	{
		EOSLobbyManager obj = PlatformSystems.lobbyManager as EOSLobbyManager;
		obj.CheckIfInitializedAndValid();
		if (!LocalUserManager.isAnyUserSignedIn)
		{
			throw new ConCommandException("Cannot create a Steamworks lobby without any local users signed in.");
		}
		obj.CreateLobby();
	}

	public override void LobbyLeave(ConCommandArgs args)
	{
		EOSLobbyManager obj = PlatformSystems.lobbyManager as EOSLobbyManager;
		obj.CheckIfInitializedAndValid();
		obj.LeaveLobby();
	}

	public override void LobbyAssignOwner(ConCommandArgs args)
	{
		(PlatformSystems.lobbyManager as EOSLobbyManager).CheckIfInitializedAndValid();
		Debug.LogFormat("Promoting {0} to lobby leader...", new object[1] { args[0] });
	}

	public override void LobbyInvite(ConCommandArgs args)
	{
		//IL_0011: Unknown result type (might be due to invalid IL or missing references)
		//IL_0016: Unknown result type (might be due to invalid IL or missing references)
		//IL_0022: Unknown result type (might be due to invalid IL or missing references)
		//IL_002d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0041: Expected O, but got Unknown
		//IL_0055: Unknown result type (might be due to invalid IL or missing references)
		//IL_005f: Expected O, but got Unknown
		EOSLobbyManager eOSLobbyManager = PlatformSystems.lobbyManager as EOSLobbyManager;
		eOSLobbyManager.CheckIfInitializedAndValid();
		SendInviteOptions val = new SendInviteOptions
		{
			LobbyId = eOSLobbyManager.currentLobbyId,
			LocalUserId = EOSLoginManager.loggedInProductId,
			TargetUserId = ProductUserId.FromString(args.GetArgString(0))
		};
		LobbyInterface obj = eOSLobbyManager.lobbyInterface;
		if (obj != null)
		{
			obj.SendInvite(val, (object)null, new OnSendInviteCallback(OnSendInviteComplete));
		}
	}

	public override void LobbyOpenInviteOverlay(ConCommandArgs args)
	{
		if ((Handle)(object)EOSLoginManager.loggedInAuthId == (Handle)null)
		{
			SteamworksLobbyManager.DoSteamLobbyOpenOverlay();
			return;
		}
		EOSLobbyManager obj = PlatformSystems.lobbyManager as EOSLobbyManager;
		obj.CheckIfInitializedAndValid();
		obj.OpenInviteOverlay();
	}

	public override void LobbyCopyToClipboard(ConCommandArgs args)
	{
		EOSLobbyManager obj = PlatformSystems.lobbyManager as EOSLobbyManager;
		obj.CheckIfInitializedAndValid();
		GUIUtility.systemCopyBuffer = obj.currentLobbyId;
		Chat.AddMessage(Language.GetString("STEAM_COPY_LOBBY_TO_CLIPBOARD_MESSAGE"));
	}

	public override void LobbyPrintData(ConCommandArgs args)
	{
		(PlatformSystems.lobbyManager as EOSLobbyManager).CheckIfInitializedAndValid();
		List<string> list = new List<string>();
		Debug.Log((object)string.Join("\n", list.ToArray()));
	}

	public override void DisplayId(ConCommandArgs args)
	{
		(PlatformSystems.lobbyManager as EOSLobbyManager).CheckIfInitializedAndValid();
	}

	public override void DisplayLobbyId(ConCommandArgs args)
	{
		(PlatformSystems.lobbyManager as EOSLobbyManager).CheckIfInitializedAndValid();
	}

	public override void LobbyPrintMembers(ConCommandArgs args)
	{
		(PlatformSystems.lobbyManager as EOSLobbyManager).CheckIfInitializedAndValid();
	}

	public override void ClearLobbies(ConCommandArgs args)
	{
		EOSLobbyManager obj = PlatformSystems.lobbyManager as EOSLobbyManager;
		Filter filter = new Filter();
		obj.RequestLobbyList(null, filter, delegate(List<LobbyDetails> lobbies)
		{
			foreach (LobbyDetails lobby in lobbies)
			{
				Debug.Log((object)lobby);
			}
		});
	}

	public override void LobbyUpdatePlayerCount(ConCommandArgs args)
	{
		(PlatformSystems.lobbyManager as EOSLobbyManager).UpdatePlayerCount();
	}

	public override void LobbyForceUpdateData(ConCommandArgs args)
	{
		EOSLobbyManager obj = PlatformSystems.lobbyManager as EOSLobbyManager;
		obj.CheckIfInitializedAndValid();
		obj.ForceLobbyDataUpdate();
	}

	public override void LobbyPrintList(ConCommandArgs args)
	{
		(PlatformSystems.lobbyManager as EOSLobbyManager).CheckIfInitializedAndValid();
	}

	public static bool IsLobbyOwner(LobbyDetails lobby)
	{
		//IL_0001: Unknown result type (might be due to invalid IL or missing references)
		//IL_000b: Expected O, but got Unknown
		return (Handle)(object)lobby.GetLobbyOwner(new LobbyDetailsGetLobbyOwnerOptions()) == (Handle)(object)EOSLoginManager.loggedInProductId;
	}

	public static string GetLobbyStringValue(LobbyDetails lobby, string key)
	{
		//IL_0000: Unknown result type (might be due to invalid IL or missing references)
		//IL_0006: Expected O, but got Unknown
		//IL_0007: Unknown result type (might be due to invalid IL or missing references)
		//IL_000c: Unknown result type (might be due to invalid IL or missing references)
		//IL_001a: Expected O, but got Unknown
		//IL_0015: Unknown result type (might be due to invalid IL or missing references)
		Attribute val = new Attribute();
		if ((int)lobby.CopyAttributeByKey(new LobbyDetailsCopyAttributeByKeyOptions
		{
			AttrKey = key
		}, ref val) == 0)
		{
			return val.Data.Value.AsUtf8;
		}
		return string.Empty;
	}

	public static bool GetLobbyBoolValue(LobbyDetails lobbyDetails, string key)
	{
		//IL_0000: Unknown result type (might be due to invalid IL or missing references)
		//IL_0006: Expected O, but got Unknown
		//IL_0007: Unknown result type (might be due to invalid IL or missing references)
		//IL_000c: Unknown result type (might be due to invalid IL or missing references)
		//IL_001a: Expected O, but got Unknown
		//IL_0015: Unknown result type (might be due to invalid IL or missing references)
		Attribute val = new Attribute();
		if ((int)lobbyDetails.CopyAttributeByKey(new LobbyDetailsCopyAttributeByKeyOptions
		{
			AttrKey = key
		}, ref val) == 0)
		{
			if (val.Data.Value.AsBool.HasValue)
			{
				return val.Data.Value.AsBool.Value;
			}
			return false;
		}
		return false;
	}

	public static long GetLobbyIntValue(LobbyDetails lobbyDetails, string key)
	{
		//IL_0000: Unknown result type (might be due to invalid IL or missing references)
		//IL_0006: Expected O, but got Unknown
		//IL_0007: Unknown result type (might be due to invalid IL or missing references)
		//IL_000c: Unknown result type (might be due to invalid IL or missing references)
		//IL_001a: Expected O, but got Unknown
		//IL_0015: Unknown result type (might be due to invalid IL or missing references)
		Attribute val = new Attribute();
		if ((int)lobbyDetails.CopyAttributeByKey(new LobbyDetailsCopyAttributeByKeyOptions
		{
			AttrKey = key
		}, ref val) == 0 && val.Data.Value.AsInt64.HasValue)
		{
			return val.Data.Value.AsInt64.Value;
		}
		return 0L;
	}

	public static double GetLobbyDoubleValue(LobbyDetails lobbyDetails, string key)
	{
		//IL_0000: Unknown result type (might be due to invalid IL or missing references)
		//IL_0006: Expected O, but got Unknown
		//IL_0007: Unknown result type (might be due to invalid IL or missing references)
		//IL_000c: Unknown result type (might be due to invalid IL or missing references)
		//IL_001a: Expected O, but got Unknown
		//IL_0015: Unknown result type (might be due to invalid IL or missing references)
		Attribute val = new Attribute();
		if ((int)lobbyDetails.CopyAttributeByKey(new LobbyDetailsCopyAttributeByKeyOptions
		{
			AttrKey = key
		}, ref val) == 0 && val.Data.Value.AsDouble.HasValue)
		{
			return val.Data.Value.AsDouble.Value;
		}
		return 0.0;
	}

	public static bool SetLobbyStringValue(LobbyModification lobby, string key, string value)
	{
		//IL_002c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0031: Unknown result type (might be due to invalid IL or missing references)
		//IL_0032: Unknown result type (might be due to invalid IL or missing references)
		//IL_0037: Unknown result type (might be due to invalid IL or missing references)
		//IL_003e: Unknown result type (might be due to invalid IL or missing references)
		//IL_004f: Expected O, but got Unknown
		//IL_004f: Unknown result type (might be due to invalid IL or missing references)
		//IL_005b: Expected O, but got Unknown
		//IL_0056: Unknown result type (might be due to invalid IL or missing references)
		//IL_005b: Unknown result type (might be due to invalid IL or missing references)
		//IL_005c: Unknown result type (might be due to invalid IL or missing references)
		//IL_005e: Invalid comparison between Unknown and I4
		if ((Handle)(object)lobby == (Handle)null || key == null || value == null || key == string.Empty || value == string.Empty)
		{
			return false;
		}
		Result val = lobby.AddAttribute(new LobbyModificationAddAttributeOptions
		{
			Attribute = new AttributeData
			{
				Key = key,
				Value = AttributeDataValue.op_Implicit(value)
			},
			Visibility = (LobbyAttributeVisibility)0
		});
		bool flag = (int)val == 0;
		Debug.Log((object)("Setting KVP for Lobby: " + ((object)lobby).ToString() + " key = " + key + " value = " + value));
		if (!flag)
		{
			Debug.Log((object)("Failed to Set KVP for Lobby: " + ((object)lobby).ToString() + " key = " + key + " value = " + value + " result = " + ((object)(Result)(ref val)).ToString()));
		}
		return flag;
	}

	public static void UpdateLobby(LobbyModification lobby)
	{
		//IL_001c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0021: Unknown result type (might be due to invalid IL or missing references)
		//IL_0030: Unknown result type (might be due to invalid IL or missing references)
		//IL_003a: Expected O, but got Unknown
		//IL_003a: Expected O, but got Unknown
		EOSLobbyManager eOSLobbyManager = PlatformSystems.lobbyManager as EOSLobbyManager;
		eOSLobbyManager.CheckIfInitializedAndValid();
		LobbyInterface obj = eOSLobbyManager.lobbyInterface;
		if (obj != null)
		{
			obj.UpdateLobby(new UpdateLobbyOptions
			{
				LobbyModificationHandle = lobby
			}, (object)null, new OnUpdateLobbyCallback(eOSLobbyManager.OnLobbyUpdated));
		}
	}

	public static bool RemoveLobbyStringValue(LobbyModification lobby, string key)
	{
		//IL_000a: Unknown result type (might be due to invalid IL or missing references)
		//IL_000f: Unknown result type (might be due to invalid IL or missing references)
		//IL_001b: Expected O, but got Unknown
		//IL_0016: Unknown result type (might be due to invalid IL or missing references)
		//IL_001b: Unknown result type (might be due to invalid IL or missing references)
		//IL_001c: Unknown result type (might be due to invalid IL or missing references)
		//IL_001e: Invalid comparison between Unknown and I4
		if ((Handle)(object)lobby != (Handle)null)
		{
			Result val = lobby.RemoveAttribute(new LobbyModificationRemoveAttributeOptions
			{
				Key = key
			});
			bool flag = (int)val == 0;
			if (!flag)
			{
				Debug.Log((object)("Failed to Remove KVP from Lobby: " + ((object)lobby).ToString() + " key = " + key + " result = " + ((object)(Result)(ref val)).ToString()));
			}
			return flag;
		}
		return false;
	}

	public static uint GetAllCurrentLobbyKVPs(LobbyDetails lobby, ref Dictionary<string, string> KVPs)
	{
		//IL_0001: Unknown result type (might be due to invalid IL or missing references)
		//IL_000b: Expected O, but got Unknown
		//IL_0012: Unknown result type (might be due to invalid IL or missing references)
		//IL_0018: Expected O, but got Unknown
		//IL_0019: Unknown result type (might be due to invalid IL or missing references)
		//IL_001e: Unknown result type (might be due to invalid IL or missing references)
		//IL_002c: Expected O, but got Unknown
		//IL_0027: Unknown result type (might be due to invalid IL or missing references)
		uint attributeCount = lobby.GetAttributeCount(new LobbyDetailsGetAttributeCountOptions());
		uint num = 0u;
		for (uint num2 = attributeCount; num < num2; num++)
		{
			Attribute val = new Attribute();
			if ((int)lobby.CopyAttributeByIndex(new LobbyDetailsCopyAttributeByIndexOptions
			{
				AttrIndex = num
			}, ref val) == 0)
			{
				KVPs.Add(val.Data.Key, val.Data.Value.AsUtf8);
			}
		}
		return attributeCount;
	}

	public override void SetLobbyTypeConVarString(string newValue)
	{
		//IL_0011: Unknown result type (might be due to invalid IL or missing references)
		//IL_0017: Expected O, but got Unknown
		//IL_001d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0029: Expected O, but got Unknown
		//IL_0024: Unknown result type (might be due to invalid IL or missing references)
		//IL_0062: Unknown result type (might be due to invalid IL or missing references)
		//IL_0067: Unknown result type (might be due to invalid IL or missing references)
		//IL_0069: Unknown result type (might be due to invalid IL or missing references)
		//IL_0078: Expected O, but got Unknown
		//IL_0073: Unknown result type (might be due to invalid IL or missing references)
		EOSLobbyManager eOSLobbyManager = PlatformSystems.lobbyManager as EOSLobbyManager;
		eOSLobbyManager.CheckIfInitializedAndValid();
		LobbyDetailsInfo val = new LobbyDetailsInfo();
		if ((int)eOSLobbyManager.CurrentLobbyDetails.CopyInfo(new LobbyDetailsCopyInfoOptions(), ref val) != 0 || !eOSLobbyManager.IsLobbyOwner())
		{
			throw new ConCommandException("Lobby type cannot be set while not the owner of a valid lobby.");
		}
		LobbyType dest = LobbyType.Error;
		SteamLobbyTypeConVar.instance.GetEnumValueAbstract(newValue, ref dest);
		if (dest == LobbyType.Error)
		{
			throw new ConCommandException("Lobby type \"Error\" is not allowed.");
		}
		if ((int)eOSLobbyManager.CurrentLobbyModification.SetPermissionLevel(new LobbyModificationSetPermissionLevelOptions
		{
			PermissionLevel = LobbyTypeToPermissionLevel(dest)
		}) != 0)
		{
			Debug.Log((object)"Unable to set permission level for lobby!");
		}
		else
		{
			UpdateLobby(eOSLobbyManager.CurrentLobbyModification);
		}
	}

	public override string GetLobbyTypeConVarString()
	{
		//IL_0011: Unknown result type (might be due to invalid IL or missing references)
		//IL_0017: Expected O, but got Unknown
		//IL_002b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0037: Expected O, but got Unknown
		//IL_0032: Unknown result type (might be due to invalid IL or missing references)
		//IL_0040: Unknown result type (might be due to invalid IL or missing references)
		EOSLobbyManager eOSLobbyManager = PlatformSystems.lobbyManager as EOSLobbyManager;
		eOSLobbyManager.CheckIfInitializedAndValid();
		LobbyDetailsInfo val = new LobbyDetailsInfo();
		if ((Handle)(object)eOSLobbyManager.currentLobbyDetails != (Handle)null)
		{
			if ((int)eOSLobbyManager.CurrentLobbyDetails.CopyInfo(new LobbyDetailsCopyInfoOptions(), ref val) != 0)
			{
				return string.Empty;
			}
			return PermissionLevelToType(val.PermissionLevel).ToString();
		}
		return string.Empty;
	}
}
