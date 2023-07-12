using System;
using System.Collections.Generic;
using System.Reflection;
using System.Text;
using Facepunch.Steamworks;
using HG;
using RoR2.Networking;
using RoR2.UI;
using UnityEngine;
using UnityEngine.Networking;

namespace RoR2;

public class SteamworksLobbyManager : PCLobbyManager
{
	private struct LobbyRefreshRequest
	{
		public object requester;

		public Filter filter;

		public Action<List<Lobby>> callback;
	}

	public bool isInLobbyDelayed;

	private const MPFeatures PlatformFeatureFlags = MPFeatures.HostGame | MPFeatures.FindGame;

	private const MPLobbyFeatures PlatformLobbyUIFlags = MPLobbyFeatures.CreateLobby | MPLobbyFeatures.SocialIcon | MPLobbyFeatures.HostPromotion | MPLobbyFeatures.Clipboard | MPLobbyFeatures.Invite | MPLobbyFeatures.UserIcon | MPLobbyFeatures.LeaveLobby | MPLobbyFeatures.LobbyDropdownOptions;

	private UserID _pendingSteamworksLobbyId;

	private bool _ownsLobby;

	private int minimumPlayerCount = 2;

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

	private readonly List<int> playerCountsList = new List<int>();

	private MemoizedToString<int, ToStringImplementationInvariant> localPlayerCountToString = MemoizedToString<int, ToStringImplementationInvariant>.GetNew();

	private bool startingFadeSet;

	private readonly MethodInfo updateLobbyDataMethodInfo = typeof(Lobby).GetMethod("UpdateLobbyData", BindingFlags.Instance | BindingFlags.NonPublic);

	private CSteamID lastHostingLobbyId;

	private Queue<LobbyRefreshRequest> lobbyRefreshRequests = new Queue<LobbyRefreshRequest>();

	private LobbyRefreshRequest? currentRefreshRequest;

	private bool hostingServer;

	private UserID currentServerId;

	private MemoizedToString<CSteamID, ToStringDefault<CSteamID>> currentServerIdString = MemoizedToString<CSteamID, ToStringDefault<CSteamID>>.GetNew();

	public UserID pendingSteamworksLobbyId
	{
		get
		{
			return _pendingSteamworksLobbyId;
		}
		set
		{
			if (!_pendingSteamworksLobbyId.Equals(value))
			{
				if (_pendingSteamworksLobbyId.CID.value != null)
				{
					RoR2Application.onUpdate -= AttemptToJoinPendingSteamworksLobby;
				}
				_pendingSteamworksLobbyId = value;
				if (_pendingSteamworksLobbyId.CID.value != null)
				{
					RoR2Application.onUpdate += AttemptToJoinPendingSteamworksLobby;
				}
			}
		}
	}

	private Client client => Client.Instance;

	public override bool isInLobby
	{
		get
		{
			if (client == null || client.Lobby == null)
			{
				return false;
			}
			return client.Lobby.IsValid;
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

	public UserID serverId => newestLobbyData.serverId;

	public override LobbyData newestLobbyData { get; protected set; }

	public override int calculatedTotalPlayerCount { get; protected set; }

	public override int calculatedExtraPlayersCount { get; protected set; }

	public override LobbyType currentLobbyType
	{
		get
		{
			//IL_000a: Unknown result type (might be due to invalid IL or missing references)
			//IL_0010: Expected I4, but got Unknown
			return (LobbyType)Client.Instance.Lobby.LobbyType;
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

	public static SteamworksLobbyManager GetFromPlatformSystems()
	{
		return PlatformSystems.lobbyManager as SteamworksLobbyManager;
	}

	private void UpdateOwnsLobby()
	{
		ownsLobby = client.Lobby.IsOwner;
	}

	public void Init()
	{
		Client instance = Client.Instance;
		instance.Lobby.OnChatMessageRecieved = OnChatMessageReceived;
		instance.Lobby.OnLobbyCreated = OnLobbyCreated;
		instance.Lobby.OnLobbyDataUpdated = OnLobbyDataUpdated;
		instance.Lobby.OnLobbyJoined = OnLobbyJoined;
		instance.Lobby.OnLobbyMemberDataUpdated = OnLobbyMemberDataUpdated;
		instance.Lobby.OnLobbyStateChanged = OnLobbyStateChanged;
		instance.Lobby.OnLobbyKicked = OnLobbyKicked;
		instance.Lobby.OnLobbyLeave = OnLobbyLeave;
		instance.Lobby.OnUserInvitedToLobby = OnUserInvitedToLobby;
		instance.Lobby.OnLobbyJoinRequested = OnLobbyJoinRequested;
		instance.LobbyList.OnLobbiesUpdated = OnLobbiesUpdated;
		RoR2Application.onUpdate += StaticUpdate;
		newestLobbyData = new LobbyData();
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
		SetStartingIfOwner(startingState: false);
		pendingSteamworksLobbyId = new UserID(GetLaunchParamsLobbyId());
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
		if (Client.Instance == null)
		{
			throw new ConCommandException("Steamworks client not available.");
		}
	}

	private void UpdatePlayerCount()
	{
		if (client != null && client.Lobby.IsValid)
		{
			int count = LocalUserManager.readOnlyLocalUsersList.Count;
			string memberData = client.Lobby.GetMemberData(client.SteamId, "player_count");
			ref MemoizedToString<int, ToStringImplementationInvariant> reference = ref localPlayerCountToString;
			int input = Math.Max(1, count);
			string @string = reference.GetString(in input);
			if (memberData != @string)
			{
				client.Lobby.SetMemberData("player_count", @string);
			}
			playerCountsList.Clear();
			calculatedTotalPlayerCount = 0;
			remoteMachineCount = 0;
			calculatedExtraPlayersCount = 0;
			ulong steamId = client.SteamId;
			ulong[] memberIDs = client.Lobby.GetMemberIDs();
			foreach (ulong num in memberIDs)
			{
				int result = ((!TextSerialization.TryParseInvariant(client.Lobby.GetMemberData(num, "player_count"), out result)) ? 1 : Math.Max(1, result));
				if (num == steamId)
				{
					result = Math.Max(1, count);
				}
				else
				{
					int num2 = remoteMachineCount + 1;
					remoteMachineCount = num2;
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
		isInLobby = client.Lobby.IsValid;
		isInLobbyDelayed = isInLobby;
		UpdateOwnsLobby();
		UpdatePlayerCount();
		onLobbyChanged?.Invoke();
		OnLobbyDataUpdated();
	}

	public override void CreateLobby()
	{
		if (client != null)
		{
			pendingSteamworksLobbyId = default(UserID);
			client.Lobby.Leave();
			base.awaitingCreate = true;
			client.Lobby.Create((Type)preferredLobbyType, RoR2Application.maxPlayers);
		}
	}

	public override void JoinLobby(UserID uid)
	{
		CSteamID cID = uid.CID;
		if (client != null && LocalUserManager.isAnyUserSignedIn)
		{
			base.awaitingJoin = true;
			LeaveLobby();
			client.Lobby.Join(cID.steamValue);
		}
	}

	public override void LeaveLobby()
	{
		Client obj = client;
		if (obj != null)
		{
			obj.Lobby.Leave();
		}
	}

	public override UserID[] GetLobbyMembers()
	{
		Client instance = Client.Instance;
		object obj;
		if (instance == null)
		{
			obj = null;
		}
		else
		{
			Lobby lobby = instance.Lobby;
			obj = ((lobby != null) ? lobby.GetMemberIDs() : null);
		}
		ulong[] array = (ulong[])obj;
		UserID[] array2 = null;
		if (array != null)
		{
			array2 = new UserID[array.Length];
			for (int i = 0; i < array2.Length; i++)
			{
				array2[i] = new UserID(new CSteamID(array[i]));
			}
		}
		return array2;
	}

	public override bool ShouldShowPromoteButton()
	{
		return SteamLobbyFinder.running;
	}

	private void Update()
	{
		if (client != null && startingFadeSet != (newestLobbyData.starting && !ClientScene.ready))
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
		if (client != null)
		{
			UpdateOwnsLobby();
		}
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
		Client obj = client;
		Lobby val = ((obj != null) ? obj.Lobby : null);
		if (val != null)
		{
			updateLobbyDataMethodInfo.Invoke(val, Array.Empty<object>());
		}
	}

	public void SendLobbyMessage(LobbyMessageType messageType, NetworkWriter writer)
	{
		byte[] array = new byte[1 + writer.Position];
		array[0] = (byte)messageType;
		Array.Copy(writer.AsArray(), 0, array, 1, writer.Position);
		Client.Instance.Lobby.SendChatMessage(Encoding.UTF8.GetString(array));
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
		//IL_0032: Unknown result type (might be due to invalid IL or missing references)
		//IL_0038: Expected O, but got Unknown
		try
		{
			_ = Client.Instance.Lobby.IsOwner;
			bool flag = senderId == Client.Instance.Lobby.Owner;
			bool flag2 = senderId == Client.Instance.SteamId;
			NetworkReader val = new NetworkReader(buffer);
			if (byteCount >= 1)
			{
				LobbyMessageType lobbyMessageType = (LobbyMessageType)val.ReadByte();
				Debug.LogFormat("Received Steamworks Lobby Message from {0} ({1}B). messageType={2}", new object[3] { senderId, byteCount, lobbyMessageType });
				switch (lobbyMessageType)
				{
				case LobbyMessageType.Chat:
				{
					string arg = val.ReadString();
					Chat.AddMessage(string.Format("{0}: {1}", Client.Instance.Friends.Get(senderId)?.Name ?? "???", arg));
					break;
				}
				case LobbyMessageType.Password:
				{
					string @string = val.ReadString();
					if (flag2)
					{
						Debug.Log((object)"Ignoring password message from self.");
					}
					else if (flag)
					{
						NetworkManagerSystem.cvClPassword.SetString(@string);
						Debug.Log((object)"Received password to endpoint from lobby leader.");
					}
					else
					{
						Debug.Log((object)"Ignoring password message from non-leader.");
					}
					break;
				}
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

	private void OnLobbyCreated(bool success)
	{
		base.awaitingCreate = false;
		if (success)
		{
			Debug.LogFormat("Steamworks lobby creation succeeded. Lobby id = {0}", new object[1] { client.Lobby.CurrentLobby });
			OnLobbyChanged();
		}
		else
		{
			Debug.Log((object)"Steamworks lobby creation failed.");
		}
	}

	protected void OnLobbyDataUpdated()
	{
		Lobby lobby = client.Lobby;
		newestLobbyData = (lobby.IsValid ? new LobbyData(lobby) : new LobbyData());
		UpdateOwnsLobby();
		UpdatePlayerCount();
		CSteamID cSteamID = new CSteamID(lobby.CurrentLobby);
		if (lobby.IsValid && !ownsLobby)
		{
			if (newestLobbyData.serverId.isValid)
			{
				bool num = newestLobbyData.serverId == new UserID(NetworkManagerSystem.singleton.serverP2PId) || NetworkManagerSystem.singleton.IsConnectedToServer(newestLobbyData.serverId);
				bool flag = string.CompareOrdinal(RoR2Application.GetBuildId(), newestLobbyData.buildId) == 0;
				if (!num && flag)
				{
					NetworkManagerSystem.singleton.desiredHost = new HostDescription(newestLobbyData.serverId, HostDescription.HostType.Steam);
					lastHostingLobbyId = cSteamID;
				}
			}
			else if (lastHostingLobbyId == cSteamID)
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

	private void OnLobbyJoined(bool success)
	{
		base.awaitingJoin = false;
		if (success)
		{
			if (client.Lobby.CurrentLobbyData != null)
			{
				string buildId = RoR2Application.GetBuildId();
				string data = client.Lobby.CurrentLobbyData.GetData("build_id");
				if (buildId != data)
				{
					Debug.LogFormat("Lobby build_id mismatch, leaving lobby. Ours=\"{0}\" Theirs=\"{1}\"", new object[2] { buildId, data });
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
					object[] formatParams = new string[2] { buildId, data };
					descriptionToken.formatParams = formatParams;
					simpleDialogBox.descriptionToken = descriptionToken;
					client.Lobby.Leave();
					return;
				}
			}
			Debug.LogFormat("Steamworks lobby join succeeded. Lobby id = {0}", new object[1] { client.Lobby.CurrentLobby });
			OnLobbyChanged();
		}
		else
		{
			Debug.Log((object)"Steamworks lobby join failed.");
			Console.instance.SubmitCmd(null, "steam_lobby_create_if_none", recordSubmit: true);
		}
		onLobbyJoined?.Invoke(success);
	}

	private void OnLobbyMemberDataUpdated(ulong memberId)
	{
		UpdateOwnsLobby();
		onLobbyMemberDataUpdated?.Invoke(new UserID(new CSteamID(memberId)));
	}

	protected void OnLobbyStateChanged(MemberStateChange memberStateChange, ulong initiatorUserId, ulong affectedUserId)
	{
		//IL_000d: Unknown result type (might be due to invalid IL or missing references)
		Debug.LogFormat("OnLobbyStateChanged memberStateChange={0} initiatorUserId={1} affectedUserId={2}", new object[3] { memberStateChange, initiatorUserId, affectedUserId });
		OnLobbyChanged();
		onLobbyStateChanged?.Invoke();
	}

	private void OnLobbyKicked(bool kickedDueToDisconnect, ulong lobbyId, ulong adminId)
	{
		Debug.LogFormat("Kicked from lobby. kickedDueToDisconnect={0} lobbyId={1} adminId={2}", new object[3] { kickedDueToDisconnect, lobbyId, adminId });
		OnLobbyChanged();
	}

	private void OnLobbyLeave(ulong lobbyId)
	{
		Debug.LogFormat("Left lobby {0}.", new object[1] { lobbyId });
		onLobbyLeave?.Invoke(new UserID(new CSteamID(lobbyId)));
		OnLobbyChanged();
	}

	private void OnLobbyJoinRequested(ulong lobbyId)
	{
		Debug.LogFormat("Request to join lobby {0} received. Attempting to join lobby.", new object[1] { lobbyId });
		pendingSteamworksLobbyId = new UserID(new CSteamID(lobbyId));
	}

	private void OnUserInvitedToLobby(ulong lobbyId, ulong senderId)
	{
		Debug.LogFormat("Received invitation to lobby {0} from sender {1}.", new object[2] { lobbyId, senderId });
	}

	public bool RequestLobbyList(object requester, Filter filter, Action<List<Lobby>> callback)
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
			currentRefreshRequest = lobbyRefreshRequests.Dequeue();
			client.LobbyList.Refresh(currentRefreshRequest.Value.filter);
		}
	}

	private void OnLobbiesUpdated()
	{
		if (currentRefreshRequest.HasValue)
		{
			LobbyRefreshRequest value = currentRefreshRequest.Value;
			currentRefreshRequest = null;
			List<Lobby> lobbies = client.LobbyList.Lobbies;
			value.callback(lobbies);
		}
		onLobbiesUpdated?.Invoke();
		UpdateRefreshRequestQueue();
	}

	private void OnStopClient()
	{
		if (Object.op_Implicit((Object)(object)NetworkManagerSystem.singleton) && ((NetworkManager)NetworkManagerSystem.singleton).client != null && ((NetworkManager)NetworkManagerSystem.singleton).client.connection != null)
		{
			NetworkConnection connection = ((NetworkManager)NetworkManagerSystem.singleton).client.connection;
			bool flag = false;
			bool flag2 = Util.ConnectionIsLocal(connection);
			flag = ((!(connection is SteamNetworkConnection)) ? (connection.address == newestLobbyData.serverAddressPortPair.address) : (((SteamNetworkConnection)(object)connection).steamId == newestLobbyData.serverId));
			if (flag2 && ownsLobby)
			{
				client.Lobby.CurrentLobbyData.RemoveData("server_id");
			}
			if (!flag2 && flag)
			{
				client.Lobby.Leave();
			}
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
			StartMigrateLobby(newLobbyId.CID);
			return;
		}
		client.Lobby.Leave();
		JoinLobby(newLobbyId);
	}

	public void StartMigrateLobby(CSteamID newLobbyId)
	{
		client.Lobby.Joinable = false;
		client.Lobby.CurrentLobbyData.SetData("migration_id", TextSerialization.ToStringInvariant(newLobbyId.steamValue));
	}

	private void AttemptToJoinPendingSteamworksLobby()
	{
		if (LocalUserManager.isAnyUserSignedIn && pendingSteamworksLobbyId != default(UserID))
		{
			JoinLobby(pendingSteamworksLobbyId);
			pendingSteamworksLobbyId = default(UserID);
		}
	}

	public void SetLobbyQuickPlayQueuedIfOwner(bool quickplayQueuedState)
	{
		Lobby lobby = client.Lobby;
		if (((lobby != null) ? lobby.CurrentLobbyData : null) != null)
		{
			lobby.CurrentLobbyData.SetData("qp", quickplayQueuedState ? "1" : "0");
			if (!quickplayQueuedState)
			{
				lobby.LobbyType = (Type)preferredLobbyType;
			}
		}
	}

	public void SetLobbyQuickPlayCutoffTimeIfOwner(uint? timestamp)
	{
		Lobby lobby = client.Lobby;
		if (((lobby != null) ? lobby.CurrentLobbyData : null) != null)
		{
			if (!timestamp.HasValue)
			{
				lobby.CurrentLobbyData.RemoveData("qp_cutoff_time");
				return;
			}
			string text = TextSerialization.ToStringInvariant(timestamp.Value);
			lobby.CurrentLobbyData.SetData("qp_cutoff_time", text);
		}
	}

	public void SetStartingIfOwner(bool startingState)
	{
		Lobby lobby = client.Lobby;
		if (((lobby != null) ? lobby.CurrentLobbyData : null) != null)
		{
			LobbyData currentLobbyData = lobby.CurrentLobbyData;
			if (currentLobbyData != null)
			{
				currentLobbyData.SetData("starting", startingState ? "1" : "0");
			}
		}
	}

	protected void OnLobbyOwnershipGained()
	{
		onLobbyOwnershipGained?.Invoke();
	}

	private void OnLobbyOwnershipLost()
	{
		onLobbyOwnershipLost?.Invoke();
	}

	public static void CreateCannotJoinSteamLobbyPopup()
	{
		SimpleDialogBox simpleDialogBox = SimpleDialogBox.Create();
		simpleDialogBox.headerToken = new SimpleDialogBox.TokenParamsPair
		{
			token = "EOS_CANNOT_JOIN_STEAM_LOBBY_HEADER",
			formatParams = Array.Empty<object>()
		};
		simpleDialogBox.descriptionToken = new SimpleDialogBox.TokenParamsPair
		{
			token = "EOS_CANNOT_JOIN_STEAM_LOBBY_MESSAGE",
			formatParams = Array.Empty<object>()
		};
		simpleDialogBox.AddCancelButton(CommonLanguageTokens.ok);
	}

	public override string GetLobbyID()
	{
		CheckIfInitializedAndValid();
		return TextSerialization.ToStringInvariant(client.Lobby.CurrentLobby);
	}

	public override bool CheckLobbyIdValidity(string lobbyID)
	{
		if (!IsDigitsOnly(lobbyID))
		{
			return false;
		}
		return true;
		static bool IsDigitsOnly(string str)
		{
			foreach (char c in str)
			{
				if (c < '0' || c > '9')
				{
					return false;
				}
			}
			return true;
		}
	}

	public override void JoinLobby(ConCommandArgs args)
	{
		string text = args[0];
		if (CSteamID.TryParse(text, out var result))
		{
			CheckIfInitializedAndValid();
			Debug.LogFormat("Enqueuing join for lobby {0}...", new object[1] { text });
			pendingSteamworksLobbyId = new UserID(result.steamValue);
		}
		else
		{
			Debug.LogFormat("Failed parsing lobby ID from {0}...", new object[1] { text });
		}
	}

	public override void LobbyCreate(ConCommandArgs args)
	{
		SteamworksLobbyManager obj = PlatformSystems.lobbyManager as SteamworksLobbyManager;
		obj.CheckIfInitializedAndValid();
		if (!LocalUserManager.isAnyUserSignedIn)
		{
			throw new ConCommandException("Cannot create a Steamworks lobby without any local users signed in.");
		}
		obj.CreateLobby();
	}

	public override void LobbyCreateIfNone(ConCommandArgs args)
	{
		SteamworksLobbyManager steamworksLobbyManager = PlatformSystems.lobbyManager as SteamworksLobbyManager;
		Client val = steamworksLobbyManager.client;
		steamworksLobbyManager.CheckIfInitializedAndValid();
		if (!LocalUserManager.isAnyUserSignedIn)
		{
			throw new ConCommandException("Cannot create a Steamworks lobby without any local users signed in.");
		}
		if (!val.Lobby.IsValid)
		{
			steamworksLobbyManager.CreateLobby();
		}
	}

	public override void LobbyLeave(ConCommandArgs args)
	{
		SteamworksLobbyManager obj = PlatformSystems.lobbyManager as SteamworksLobbyManager;
		obj.CheckIfInitializedAndValid();
		obj.pendingSteamworksLobbyId = default(UserID);
		obj.LeaveLobby();
	}

	public override void LobbyAssignOwner(ConCommandArgs args)
	{
		SteamworksLobbyManager obj = PlatformSystems.lobbyManager as SteamworksLobbyManager;
		Client val = obj.client;
		obj.CheckIfInitializedAndValid();
		Debug.LogFormat("Promoting {0} to lobby leader...", new object[1] { args[0] });
		val.Lobby.Owner = args.GetArgSteamID(0).steamValue;
	}

	public override void LobbyInvite(ConCommandArgs args)
	{
		SteamworksLobbyManager obj = PlatformSystems.lobbyManager as SteamworksLobbyManager;
		Client val = obj.client;
		obj.CheckIfInitializedAndValid();
		val.Lobby.InviteUserToLobby(args.GetArgSteamID(0).steamValue);
	}

	public static void DoSteamLobbyOpenOverlay()
	{
		Client instance = Client.Instance;
		NetworkManagerSystemSteam.CheckSteamworks();
		instance.Overlay.OpenInviteDialog(instance.Lobby.CurrentLobby);
	}

	public override void LobbyOpenInviteOverlay(ConCommandArgs args)
	{
		(PlatformSystems.lobbyManager as SteamworksLobbyManager).CheckIfInitializedAndValid();
		DoSteamLobbyOpenOverlay();
	}

	public override void LobbyCopyToClipboard(ConCommandArgs args)
	{
		string lobbyID = GetLobbyID();
		if (!string.IsNullOrWhiteSpace(lobbyID))
		{
			GUIUtility.systemCopyBuffer = lobbyID;
		}
		Chat.AddMessage(Language.GetString("STEAM_COPY_LOBBY_TO_CLIPBOARD_MESSAGE"));
	}

	public override void LobbyPrintData(ConCommandArgs args)
	{
		SteamworksLobbyManager obj = PlatformSystems.lobbyManager as SteamworksLobbyManager;
		Client val = obj.client;
		obj.CheckIfInitializedAndValid();
		if (!val.Lobby.IsValid)
		{
			return;
		}
		List<string> list = new List<string>();
		foreach (KeyValuePair<string, string> allDatum in val.Lobby.CurrentLobbyData.GetAllData())
		{
			list.Add($"\"{allDatum.Key}\" = \"{allDatum.Value}\"");
		}
		Debug.Log((object)string.Join("\n", list.ToArray()));
	}

	public override void DisplayId(ConCommandArgs args)
	{
		SteamworksLobbyManager obj = PlatformSystems.lobbyManager as SteamworksLobbyManager;
		Client val = obj.client;
		obj.CheckIfInitializedAndValid();
		Debug.LogFormat("Steam id = {0}", new object[1] { val.SteamId });
	}

	public override void DisplayLobbyId(ConCommandArgs args)
	{
		SteamworksLobbyManager obj = PlatformSystems.lobbyManager as SteamworksLobbyManager;
		Client val = obj.client;
		obj.CheckIfInitializedAndValid();
		Debug.LogFormat("Lobby id = {0}", new object[1] { val.Lobby.CurrentLobby });
	}

	public override void LobbyPrintMembers(ConCommandArgs args)
	{
		SteamworksLobbyManager obj = PlatformSystems.lobbyManager as SteamworksLobbyManager;
		Client val = obj.client;
		obj.CheckIfInitializedAndValid();
		ulong[] memberIDs = val.Lobby.GetMemberIDs();
		string[] array = new string[memberIDs.Length];
		for (int i = 0; i < memberIDs.Length; i++)
		{
			array[i] = string.Format("[{0}]{1} id={2} name={3}", i, (val.Lobby.Owner == memberIDs[i]) ? "*" : " ", memberIDs[i], val.Friends.GetName(memberIDs[i]));
		}
		Debug.Log((object)string.Join("\n", array));
	}

	public override void ClearLobbies(ConCommandArgs args)
	{
		//IL_0011: Unknown result type (might be due to invalid IL or missing references)
		//IL_0017: Expected O, but got Unknown
		//IL_0026: Unknown result type (might be due to invalid IL or missing references)
		SteamworksLobbyManager obj = PlatformSystems.lobbyManager as SteamworksLobbyManager;
		_ = obj.client;
		Filter val = new Filter();
		val.MaxResults = 50;
		val.DistanceFilter = (Distance)3;
		obj.RequestLobbyList(null, val, delegate(List<Lobby> lobbies)
		{
			foreach (Lobby lobby in lobbies)
			{
				Debug.Log((object)lobby);
			}
		});
	}

	public override void LobbyUpdatePlayerCount(ConCommandArgs args)
	{
		(PlatformSystems.lobbyManager as SteamworksLobbyManager).UpdatePlayerCount();
	}

	public override void LobbyForceUpdateData(ConCommandArgs args)
	{
	}

	public override void LobbyPrintList(ConCommandArgs args)
	{
		SteamworksLobbyManager obj = PlatformSystems.lobbyManager as SteamworksLobbyManager;
		Client val = obj.client;
		obj.CheckIfInitializedAndValid();
		List<Lobby> lobbies = val.LobbyList.Lobbies;
		string[] array = new string[lobbies.Count];
		for (int i = 0; i < lobbies.Count; i++)
		{
			array[i] = $"[{i}] id={lobbies[i].LobbyID}\n      players={lobbies[i].NumMembers}/{lobbies[i].MemberLimit},\n      owner=\"{val.Friends.GetName(lobbies[i].Owner)}\"";
		}
		Debug.Log((object)string.Join("\n", array));
	}

	public override bool IsLobbyOwner(UserID user)
	{
		if (Client.Instance.Lobby.Owner == user.CID.steamValue)
		{
			return user != default(UserID);
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

	public override void OnStartPrivateGame()
	{
		CreateLobby();
	}

	public override void ToggleQuickplay()
	{
		Console.instance.SubmitCmd(null, SteamLobbyFinder.running ? "steam_quickplay_stop" : "steam_quickplay_start");
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
		if (!newestLobbyData.quickplayQueued)
		{
			return ownsLobby;
		}
		return false;
	}

	public override string GetUserDisplayName(UserID user)
	{
		Client instance = Client.Instance;
		string result = "none";
		if (instance != null)
		{
			result = instance.Friends.GetName(user.CID.steamValue);
		}
		return result;
	}

	public override void OpenInviteOverlay()
	{
		SteamworksLobbyManager obj = PlatformSystems.lobbyManager as SteamworksLobbyManager;
		Client val = obj.client;
		obj.CheckIfInitializedAndValid();
		NetworkManagerSystemSteam.CheckSteamworks();
		val.Overlay.OpenInviteDialog(val.Lobby.CurrentLobby);
	}

	public override void SetLobbyTypeConVarString(string newValue)
	{
		//IL_0019: Unknown result type (might be due to invalid IL or missing references)
		//IL_001f: Invalid comparison between Unknown and I4
		//IL_003e: Unknown result type (might be due to invalid IL or missing references)
		//IL_004c: Unknown result type (might be due to invalid IL or missing references)
		//IL_004e: Invalid comparison between Unknown and I4
		//IL_0065: Unknown result type (might be due to invalid IL or missing references)
		(PlatformSystems.lobbyManager as SteamworksLobbyManager).CheckIfInitializedAndValid();
		if ((int)Client.Instance.Lobby.LobbyType == 4 || !Client.Instance.Lobby.IsOwner)
		{
			throw new ConCommandException("Lobby type cannot be set while not the owner of a valid lobby.");
		}
		Type dest = (Type)4;
		SteamLobbyTypeConVar.instance.GetEnumValueAbstract<Type>(newValue, ref dest);
		if ((int)dest == 4)
		{
			throw new ConCommandException("Lobby type \"Error\" is not allowed.");
		}
		Client.Instance.Lobby.LobbyType = dest;
	}

	public override string GetLobbyTypeConVarString()
	{
		//IL_0011: Unknown result type (might be due to invalid IL or missing references)
		//IL_0016: Unknown result type (might be due to invalid IL or missing references)
		Client instance = Client.Instance;
		object obj;
		if (instance == null)
		{
			obj = null;
		}
		else
		{
			Type lobbyType = instance.Lobby.LobbyType;
			obj = ((object)(Type)(ref lobbyType)).ToString();
		}
		if (obj == null)
		{
			obj = string.Empty;
		}
		return (string)obj;
	}
}
