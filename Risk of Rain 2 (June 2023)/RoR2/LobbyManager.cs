using System;
using System.Collections.Generic;
using System.Globalization;
using Facepunch.Steamworks;
using RoR2.ConVar;
using RoR2.Networking;

namespace RoR2;

public abstract class LobbyManager
{
	public enum State
	{
		Idle,
		Hosting,
		Waiting
	}

	public enum LobbyMessageType : byte
	{
		Chat,
		Password
	}

	public class LobbyDataSetupState
	{
		public int totalMaxPlayers;

		public int totalPlayerCount;

		public bool quickplayQueued;

		public UserID lobbyId;

		public UserID serverId;

		public AddressPortPair serverAddressPortPair;

		public bool starting;

		public string buildId = "0";

		public DateTime? quickplayCutoffTime;

		public bool shouldConnect;

		public bool joinable;
	}

	public class LobbyData
	{
		public readonly int totalMaxPlayers;

		public readonly int totalPlayerCount;

		public readonly bool quickplayQueued;

		public readonly UserID lobbyId;

		public readonly UserID serverId;

		public readonly AddressPortPair serverAddressPortPair;

		public readonly bool starting;

		public readonly string buildId = "0";

		public readonly DateTime? quickplayCutoffTime;

		public readonly bool shouldConnect;

		public readonly bool joinable;

		public LobbyData(LobbyDataSetupState setupState)
		{
			totalMaxPlayers = setupState.totalMaxPlayers;
			totalPlayerCount = setupState.totalPlayerCount;
			quickplayQueued = setupState.quickplayQueued;
			lobbyId = setupState.lobbyId;
			serverId = setupState.serverId;
			serverAddressPortPair = setupState.serverAddressPortPair;
			starting = setupState.starting;
			buildId = setupState.buildId;
			quickplayCutoffTime = setupState.quickplayCutoffTime;
			shouldConnect = setupState.shouldConnect;
			joinable = setupState.joinable;
		}

		public LobbyData()
		{
		}

		public static bool TryParseUserID(string str, out UserID result)
		{
			if (CSteamID.TryParse(str, out var result2))
			{
				result = new UserID(result2);
				return true;
			}
			result.CID = CSteamID.nil;
			return false;
		}

		public LobbyData(Lobby lobby)
		{
			//IL_0100: Unknown result type (might be due to invalid IL or missing references)
			//IL_0106: Invalid comparison between Unknown and I4
			UserID userID = new UserID(new CSteamID(lobby.CurrentLobby));
			LobbyData currentLobbyData = lobby.CurrentLobbyData;
			Dictionary<string, string> lobbyDataDictionary = currentLobbyData.GetAllData();
			lobbyId = userID;
			ReadCSteamID("server_id", ref serverId);
			ReadAddressPortPair("server_address", ref serverAddressPortPair);
			ReadInt("total_max_players", ref totalMaxPlayers);
			ReadInt("player_count", ref totalPlayerCount);
			ReadBool("qp", ref quickplayQueued);
			ReadBool("starting", ref starting);
			ReadString("build_id", ref buildId);
			ReadNullableDate("qp_cutoff_time", out quickplayCutoffTime);
			joinable = true;
			joinable &= totalPlayerCount < totalMaxPlayers;
			joinable &= (int)lobby.LobbyType == 2;
			shouldConnect = serverId.CID.isValid || serverAddressPortPair.isValid;
			bool ReadAddressPortPair(string metaDataName, ref AddressPortPair field)
			{
				if (lobbyDataDictionary.TryGetValue(metaDataName, out var value5) && AddressPortPair.TryParse(value5, out var addressPortPair))
				{
					field = addressPortPair;
					return true;
				}
				return false;
			}
			bool ReadBool(string metaDataName, ref bool field)
			{
				if (lobbyDataDictionary.TryGetValue(metaDataName, out var value3) && TextSerialization.TryParseInvariant(value3, out int result2))
				{
					field = result2 != 0;
					return true;
				}
				return false;
			}
			bool ReadCSteamID(string metaDataName, ref UserID field)
			{
				if (lobbyDataDictionary.TryGetValue(metaDataName, out var value6) && TryParseUserID(value6, out var result4))
				{
					field = result4;
					return true;
				}
				return false;
			}
			bool ReadInt(string metaDataName, ref int field)
			{
				if (lobbyDataDictionary.TryGetValue(metaDataName, out var value4) && TextSerialization.TryParseInvariant(value4, out int result3))
				{
					field = result3;
					return true;
				}
				return false;
			}
			bool ReadNullableDate(string metaDataName, out DateTime? field)
			{
				if (lobbyDataDictionary.TryGetValue(metaDataName, out var value) && TextSerialization.TryParseInvariant(value, out uint result))
				{
					field = Util.dateZero + TimeSpan.FromSeconds(result);
					return true;
				}
				field = null;
				return false;
			}
			bool ReadString(string metaDataName, ref string field)
			{
				if (lobbyDataDictionary.TryGetValue(metaDataName, out var value2))
				{
					field = value2;
					return true;
				}
				return false;
			}
		}
	}

	public State state;

	public LobbyType preferredLobbyType;

	public Action onPlayerCountUpdated;

	public Action<bool> onLobbyJoined;

	public Action onLobbiesUpdated;

	public Action onLobbyOwnershipGained;

	public Action onLobbyOwnershipLost;

	public Action onLobbyChanged;

	public Action onLobbyDataUpdated;

	public Action<UserID> onLobbyLeave;

	public Action<UserID> onLobbyMemberDataUpdated;

	public Action onLobbyStateChanged;

	public static readonly IntConVar cvSteamLobbyMaxMembers;

	public abstract int calculatedTotalPlayerCount { get; protected set; }

	public abstract int calculatedExtraPlayersCount { get; protected set; }

	public abstract bool isInLobby { get; protected set; }

	public abstract bool ownsLobby { get; protected set; }

	public abstract bool IsBusy { get; set; }

	public bool awaitingJoin { get; protected set; }

	public bool awaitingCreate { get; protected set; }

	public bool isFull { get; protected set; }

	public abstract bool hasMinimumPlayerCount { get; }

	public abstract LobbyType currentLobbyType { get; set; }

	public abstract LobbyData newestLobbyData { get; protected set; }

	public abstract MPFeatures GetPlatformMPFeatureFlags();

	public abstract MPLobbyFeatures GetPlatformMPLobbyFeatureFlags();

	public bool HasMPLobbyUI()
	{
		return GetPlatformMPLobbyFeatureFlags() != MPLobbyFeatures.None;
	}

	public bool HasMPFeature(MPFeatures flags)
	{
		return GetPlatformMPFeatureFlags().HasFlag(flags);
	}

	public bool HasMPLobbyFeature(MPLobbyFeatures flags)
	{
		return GetPlatformMPLobbyFeatureFlags().HasFlag(flags);
	}

	public abstract int GetLobbyMemberPlayerCountByIndex(int memberIndex);

	public abstract void CreateLobby();

	public abstract string GetLobbyID();

	public virtual void SetNetworkType(bool isInternet)
	{
	}

	public virtual bool IsNetworkTypeInternet()
	{
		return true;
	}

	public abstract void LeaveLobby();

	public abstract void JoinLobby(UserID join);

	public virtual void Shutdown()
	{
	}

	public abstract void OnStartPrivateGame();

	public abstract void AutoMatchmake();

	public abstract void ToggleQuickplay();

	public void OnMultiplayerMenuEnabled(Action<UserID> onLobbyLeave)
	{
		if (!PlatformSystems.lobbyManager.isInLobby)
		{
			PlatformSystems.lobbyManager.CreateLobby();
		}
		LobbyManager lobbyManager = PlatformSystems.lobbyManager;
		lobbyManager.onLobbyLeave = (Action<UserID>)Delegate.Combine(lobbyManager.onLobbyLeave, onLobbyLeave);
	}

	public abstract bool IsLobbyOwner(UserID user);

	public abstract string GetUserDisplayName(UserID user);

	public abstract bool IsLobbyOwner();

	public abstract UserID[] GetLobbyMembers();

	public abstract bool ShouldShowPromoteButton();

	public abstract void CheckIfInitializedAndValid();

	public abstract void CheckIfInvited();

	public abstract bool CanInvite();

	public abstract void CheckBusyTimer();

	public abstract bool ShouldEnableQuickplayButton();

	public abstract bool ShouldEnableStartPrivateGameButton();

	public abstract void OpenInviteOverlay();

	public abstract void SetQuickplayCutoffTime(double cutoffTime);

	public abstract double GetQuickplayCutoffTime();

	public abstract void OnCutoffTimerComplete();

	static LobbyManager()
	{
		int maxPlayers = RoR2Application.maxPlayers;
		cvSteamLobbyMaxMembers = new IntConVar("steam_lobby_max_members", ConVarFlags.None, maxPlayers.ToString(CultureInfo.InvariantCulture), "Sets the maximum number of players allowed in steam lobbies created by this machine.");
	}
}
