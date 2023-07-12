using System;
using System.Collections.Generic;
using EntityStates;
using Facepunch.Steamworks;
using RoR2.UI.MainMenu;
using UnityEngine;

namespace RoR2.Networking;

public class SteamLobbyFinder : MonoBehaviour
{
	private class LobbyStateBase : BaseState
	{
		protected SteamLobbyFinder lobbyFinder;

		public override void OnEnter()
		{
			base.OnEnter();
			lobbyFinder = GetComponent<SteamLobbyFinder>();
			LobbyManager lobbyManager = PlatformSystems.lobbyManager;
			lobbyManager.onLobbyOwnershipGained = (Action)Delegate.Combine(lobbyManager.onLobbyOwnershipGained, new Action(OnLobbyOwnershipGained));
			LobbyManager lobbyManager2 = PlatformSystems.lobbyManager;
			lobbyManager2.onLobbyOwnershipLost = (Action)Delegate.Combine(lobbyManager2.onLobbyOwnershipLost, new Action(OnLobbyOwnershipLost));
		}

		public override void OnExit()
		{
			LobbyManager lobbyManager = PlatformSystems.lobbyManager;
			lobbyManager.onLobbyOwnershipGained = (Action)Delegate.Remove(lobbyManager.onLobbyOwnershipGained, new Action(OnLobbyOwnershipGained));
			LobbyManager lobbyManager2 = PlatformSystems.lobbyManager;
			lobbyManager2.onLobbyOwnershipLost = (Action)Delegate.Remove(lobbyManager2.onLobbyOwnershipLost, new Action(OnLobbyOwnershipLost));
			base.OnExit();
		}

		public virtual void OnLobbiesUpdated()
		{
		}

		private void OnLobbyOwnershipGained()
		{
			outer.SetNextState(new LobbyStateStart());
		}

		private void OnLobbyOwnershipLost()
		{
			outer.SetNextState(new LobbyStateNonLeader());
		}
	}

	private class LobbyStateNonLeader : LobbyStateBase
	{
		public override void Update()
		{
			base.Update();
			if (PlatformSystems.lobbyManager.ownsLobby)
			{
				if (PlatformSystems.lobbyManager.hasMinimumPlayerCount)
				{
					outer.SetNextState(new LobbyStateMultiSearch());
				}
				else
				{
					outer.SetNextState(new LobbyStateSingleSearch());
				}
			}
		}
	}

	private class LobbyStateStart : LobbyStateBase
	{
		public override void Update()
		{
			base.Update();
			if (lobbyFinder.startDelayDuration <= base.age)
			{
				outer.SetNextState(PlatformSystems.lobbyManager.hasMinimumPlayerCount ? ((LobbyStateBase)new LobbyStateMultiSearch()) : ((LobbyStateBase)new LobbyStateSingleSearch()));
			}
		}
	}

	private class LobbyStateSingleSearch : LobbyStateBase
	{
		public override void OnEnter()
		{
			base.OnEnter();
			(PlatformSystems.lobbyManager as SteamworksLobbyManager).SetLobbyQuickPlayCutoffTimeIfOwner(null);
		}

		public override void Update()
		{
			//IL_0052: Unknown result type (might be due to invalid IL or missing references)
			//IL_0058: Invalid comparison between Unknown and I4
			base.Update();
			if (PlatformSystems.lobbyManager.hasMinimumPlayerCount)
			{
				outer.SetNextState(new LobbyStateMultiSearch());
			}
			else if (lobbyFinder.refreshTimer <= 0f)
			{
				if (base.age >= lobbyFinder.joinOnlyDuration && (int)steamClient.Lobby.LobbyType != 2)
				{
					Debug.LogFormat("Unable to find joinable lobby after {0} seconds. Setting lobby to public.", new object[1] { lobbyFinder.age });
					steamClient.Lobby.LobbyType = (Type)2;
				}
				lobbyFinder.refreshTimer = lobbyFinder.refreshInterval;
				lobbyFinder.RequestLobbyListRefresh();
			}
		}

		public override void OnLobbiesUpdated()
		{
			base.OnLobbiesUpdated();
			if (!((BaseSteamworks)steamClient).IsValid)
			{
				return;
			}
			List<Lobby> lobbies = steamClient.LobbyList.Lobbies;
			List<Lobby> list = new List<Lobby>();
			ulong currentLobby = steamClient.Lobby.CurrentLobby;
			bool isValid = steamClient.Lobby.IsValid;
			int currentLobbySize = (isValid ? GetCurrentLobbyRealPlayerCount() : LocalUserManager.readOnlyLocalUsersList.Count);
			if (PlatformSystems.lobbyManager.ownsLobby || !isValid)
			{
				for (int i = 0; i < lobbies.Count; i++)
				{
					if ((!isValid || lobbies[i].LobbyID < currentLobby) && CanJoinLobby(currentLobbySize, lobbies[i]))
					{
						list.Add(lobbies[i]);
					}
				}
				if (list.Count > 0)
				{
					UserID join = new UserID(list[0].LobbyID);
					PlatformSystems.lobbyManager.JoinLobby(join);
				}
			}
			Debug.LogFormat("Found {0} lobbies, {1} joinable.", new object[2] { lobbies.Count, list.Count });
		}
	}

	private class LobbyStateMultiSearch : LobbyStateBase
	{
		public override void OnEnter()
		{
			base.OnEnter();
			steamClient.Lobby.LobbyType = (Type)2;
			TimeSpan timeSpan = Util.UnixTimeStampToDateTimeUtc(steamClient.Utils.GetServerRealTime()) + TimeSpan.FromSeconds(lobbyFinder.waitForFullDuration) - Util.dateZero;
			(PlatformSystems.lobbyManager as SteamworksLobbyManager).SetLobbyQuickPlayCutoffTimeIfOwner((uint)timeSpan.TotalSeconds);
		}

		public override void OnExit()
		{
			(PlatformSystems.lobbyManager as SteamworksLobbyManager).SetLobbyQuickPlayCutoffTimeIfOwner(null);
			base.OnExit();
		}

		public override void Update()
		{
			base.Update();
			if (!PlatformSystems.lobbyManager.hasMinimumPlayerCount)
			{
				outer.SetNextState(new LobbyStateSingleSearch());
			}
			else if (lobbyFinder.waitForFullDuration <= base.age)
			{
				outer.SetNextState(new LobbyStateBeginGame());
			}
		}
	}

	private class LobbyStateBeginGame : LobbyStateBase
	{
		public override void OnEnter()
		{
			base.OnEnter();
			steamClient.Lobby.LobbyType = (Type)PlatformSystems.lobbyManager.preferredLobbyType;
			(PlatformSystems.lobbyManager as SteamworksLobbyManager).SetStartingIfOwner(startingState: true);
			string arg = "ClassicRun";
			Console.instance.SubmitCmd(null, $"transition_command \"gamemode {arg}; host 1;\"");
		}
	}

	private float age;

	public float joinOnlyDuration = 5f;

	public float waitForFullDuration = 30f;

	public float startDelayDuration = 1f;

	public float refreshInterval = 2f;

	private float refreshTimer;

	private EntityStateMachine stateMachine;

	private static SteamLobbyFinder instance;

	private static bool _userRequestedQuickplayQueue;

	private static bool _lobbyIsInQuickplayQueue;

	private static Client steamClient => Client.Instance;

	public static bool userRequestedQuickplayQueue
	{
		get
		{
			return _userRequestedQuickplayQueue;
		}
		set
		{
			if (_userRequestedQuickplayQueue != value)
			{
				_userRequestedQuickplayQueue = value;
				running = _lobbyIsInQuickplayQueue || _userRequestedQuickplayQueue;
			}
		}
	}

	private static bool lobbyIsInQuickplayQueue
	{
		get
		{
			return _lobbyIsInQuickplayQueue;
		}
		set
		{
			if (_lobbyIsInQuickplayQueue != value)
			{
				_lobbyIsInQuickplayQueue = value;
				running = _lobbyIsInQuickplayQueue || _userRequestedQuickplayQueue;
			}
		}
	}

	public static bool running
	{
		get
		{
			return Object.op_Implicit((Object)(object)instance);
		}
		private set
		{
			//IL_0035: Unknown result type (might be due to invalid IL or missing references)
			if (Object.op_Implicit((Object)(object)instance) != value)
			{
				if (value)
				{
					instance = new GameObject("SteamLobbyFinder", new Type[2]
					{
						typeof(SteamLobbyFinder),
						typeof(SetDontDestroyOnLoad)
					}).GetComponent<SteamLobbyFinder>();
				}
				else
				{
					Object.Destroy((Object)(object)((Component)instance).gameObject);
					instance = null;
				}
			}
		}
	}

	private void Awake()
	{
		stateMachine = ((Component)this).gameObject.AddComponent<EntityStateMachine>();
		stateMachine.initialStateType = new SerializableEntityStateType(typeof(LobbyStateStart));
	}

	private void OnDestroy()
	{
		Object.Destroy((Object)(object)stateMachine);
	}

	private void Update()
	{
		if (steamClient.Lobby.IsValid && !PlatformSystems.lobbyManager.ownsLobby)
		{
			if (stateMachine.state.GetType() != typeof(LobbyStateNonLeader))
			{
				stateMachine.SetNextState(new LobbyStateNonLeader());
			}
		}
		else
		{
			refreshTimer -= Time.unscaledDeltaTime;
			age += Time.unscaledDeltaTime;
		}
	}

	private void RequestLobbyListRefresh()
	{
		//IL_0000: Unknown result type (might be due to invalid IL or missing references)
		//IL_0005: Unknown result type (might be due to invalid IL or missing references)
		//IL_0024: Unknown result type (might be due to invalid IL or missing references)
		//IL_0039: Unknown result type (might be due to invalid IL or missing references)
		//IL_004e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0069: Expected O, but got Unknown
		Filter filter = new Filter
		{
			StringFilters = { ["appid"] = TextSerialization.ToStringInvariant(((BaseSteamworks)steamClient).AppId) },
			StringFilters = { ["build_id"] = RoR2Application.GetBuildId() },
			StringFilters = { ["qp"] = "1" },
			StringFilters = { ["total_max_players"] = TextSerialization.ToStringInvariant(RoR2Application.maxPlayers) }
		};
		(PlatformSystems.lobbyManager as SteamworksLobbyManager).RequestLobbyList(this, filter, OnLobbyListReceived);
	}

	private void OnLobbyListReceived(List<Lobby> newLobbies)
	{
		if (Object.op_Implicit((Object)(object)this))
		{
			(stateMachine.state as LobbyStateBase)?.OnLobbiesUpdated();
		}
	}

	private static bool CanJoinLobby(int currentLobbySize, Lobby lobby)
	{
		return currentLobbySize + GetRealLobbyPlayerCount(lobby) <= lobby.MemberLimit;
	}

	private static int GetRealLobbyPlayerCount(Lobby lobby)
	{
		string data = lobby.GetData("player_count");
		if (data != null && TextSerialization.TryParseInvariant(data, out int result))
		{
			return result;
		}
		return steamClient.Lobby.MaxMembers;
	}

	private static int GetCurrentLobbyRealPlayerCount()
	{
		return PlatformSystems.lobbyManager.newestLobbyData.totalPlayerCount;
	}

	[ConCommand(commandName = "steam_quickplay_start")]
	private static void CCSteamQuickplayStart(ConCommandArgs args)
	{
		NetworkManagerSystemSteam.CheckSteamworks();
		userRequestedQuickplayQueue = true;
		(PlatformSystems.lobbyManager as SteamworksLobbyManager).SetLobbyQuickPlayQueuedIfOwner(quickplayQueuedState: true);
	}

	[ConCommand(commandName = "steam_quickplay_stop")]
	private static void CCSteamQuickplayStop(ConCommandArgs args)
	{
		NetworkManagerSystemSteam.CheckSteamworks();
		userRequestedQuickplayQueue = false;
		PlatformSystems.lobbyManager.CreateLobby();
	}

	public static void Init()
	{
		LobbyManager lobbyManager = PlatformSystems.lobbyManager;
		lobbyManager.onLobbyDataUpdated = (Action)Delegate.Combine(lobbyManager.onLobbyDataUpdated, new Action(OnLobbyDataUpdated));
		NetworkManagerSystem.onStartClientGlobal += delegate
		{
			(PlatformSystems.lobbyManager as SteamworksLobbyManager).SetLobbyQuickPlayQueuedIfOwner(quickplayQueuedState: false);
			userRequestedQuickplayQueue = false;
		};
		LobbyManager lobbyManager2 = PlatformSystems.lobbyManager;
		lobbyManager2.onLobbyOwnershipGained = (Action)Delegate.Combine(lobbyManager2.onLobbyOwnershipGained, (Action)delegate
		{
			if (PlatformSystems.lobbyManager.newestLobbyData.quickplayQueued)
			{
				userRequestedQuickplayQueue = true;
			}
		});
		LobbyManager lobbyManager3 = PlatformSystems.lobbyManager;
		lobbyManager3.onLobbyOwnershipLost = (Action)Delegate.Combine(lobbyManager3.onLobbyOwnershipLost, (Action)delegate
		{
			userRequestedQuickplayQueue = false;
		});
	}

	private static void OnLobbyDataUpdated()
	{
		lobbyIsInQuickplayQueue = PlatformSystems.lobbyManager.newestLobbyData.quickplayQueued;
	}

	public static string GetResolvedStateString()
	{
		//IL_0026: Unknown result type (might be due to invalid IL or missing references)
		//IL_002c: Invalid comparison between Unknown and I4
		if (!steamClient.Lobby.IsValid)
		{
			return Language.GetString("STEAM_LOBBY_STATUS_NOT_IN_LOBBY");
		}
		bool flag = (int)steamClient.Lobby.LobbyType == 2;
		if (Object.op_Implicit((Object)(object)instance))
		{
			_ = instance.stateMachine.state is LobbyStateSingleSearch;
		}
		else
			_ = 0;
		_ = PlatformSystems.lobbyManager.newestLobbyData.totalPlayerCount;
		_ = PlatformSystems.lobbyManager.newestLobbyData.totalMaxPlayers;
		_ = PlatformSystems.lobbyManager.isFull;
		string token = string.Empty;
		object[] args = Array.Empty<object>();
		if (NetworkManagerSystem.singleton.isHost || (Object.op_Implicit((Object)(object)MultiplayerMenuController.instance) && MultiplayerMenuController.instance.isInHostingState))
		{
			token = "STEAM_LOBBY_STATUS_STARTING_SERVER";
		}
		else if (PlatformSystems.lobbyManager.newestLobbyData.starting)
		{
			token = "STEAM_LOBBY_STATUS_GAME_STARTING";
		}
		else if (PlatformSystems.lobbyManager.newestLobbyData.shouldConnect)
		{
			token = "STEAM_LOBBY_STATUS_CONNECTING_TO_HOST";
		}
		else if (Object.op_Implicit((Object)(object)instance) && instance.stateMachine.state is LobbyStateStart)
		{
			token = "STEAM_LOBBY_STATUS_LAUNCHING_QUICKPLAY";
		}
		else if (PlatformSystems.lobbyManager.isInLobby)
		{
			if (PlatformSystems.lobbyManager.newestLobbyData.quickplayQueued)
			{
				if (!flag)
				{
					token = "STEAM_LOBBY_STATUS_QUICKPLAY_SEARCHING_FOR_EXISTING_LOBBY";
				}
				else
				{
					DateTime dateTime = Util.UnixTimeStampToDateTimeUtc(steamClient.Utils.GetServerRealTime());
					DateTime? quickplayCutoffTime = PlatformSystems.lobbyManager.newestLobbyData.quickplayCutoffTime;
					if (!quickplayCutoffTime.HasValue)
					{
						token = "STEAM_LOBBY_STATUS_QUICKPLAY_WAITING_BELOW_MINIMUM_PLAYERS";
						args = new object[2]
						{
							PlatformSystems.lobbyManager.newestLobbyData.totalPlayerCount,
							PlatformSystems.lobbyManager.newestLobbyData.totalMaxPlayers
						};
					}
					else
					{
						TimeSpan timeSpan = quickplayCutoffTime.Value - dateTime;
						token = "STEAM_LOBBY_STATUS_QUICKPLAY_WAITING_ABOVE_MINIMUM_PLAYERS";
						args = new object[3]
						{
							PlatformSystems.lobbyManager.newestLobbyData.totalPlayerCount,
							PlatformSystems.lobbyManager.newestLobbyData.totalMaxPlayers,
							Math.Max(0.0, timeSpan.TotalSeconds)
						};
					}
				}
			}
			else
			{
				token = "STEAM_LOBBY_STATUS_OUT_OF_QUICKPLAY";
			}
		}
		return Language.GetStringFormatted(token, args);
	}
}
