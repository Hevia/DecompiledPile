using System;
using System.Collections.Generic;
using System.Text;
using Epic.OnlineServices;
using Epic.OnlineServices.Lobby;
using HG;
using RoR2.Networking;
using UnityEngine;
using UnityEngine.Networking;

namespace RoR2;

public static class EOSLobbyDataGenerator
{
	private static class ToStringCache
	{
		public static MemoizedToString<CSteamID, ToStringDefault<CSteamID>> serverId;

		public static MemoizedToString<AddressPortPair, ToStringDefault<AddressPortPair>> serverAddress;

		public static MemoizedToString<int, ToStringImplementationInvariant> playerCount;

		public static MemoizedToString<int, ToStringImplementationInvariant> edition;

		public static MemoizedToString<int, ToStringImplementationInvariant> serverMaxPlayers;

		public static MemoizedToString<int, ToStringImplementationInvariant> serverPlayerCount;
	}

	private const int k_cubChatMetadataMax = 8192;

	private const int k_nMaxLobbyKeyLength = 255;

	private static string lobbyId;

	private static int edition;

	private static RuleBook cachedRuleBook;

	private static bool dirty = false;

	private static readonly string[] specialKeys = new string[9] { "joinable", "name", "appid", "lobbytype", "total_max_players", "v", "qp_cutoff_time", "qp", "starting" };

	private static readonly List<KeyValuePair<string, string>> ruleBookKeyValues = new List<KeyValuePair<string, string>>(1);

	private static readonly KeyValueSplitter ruleBookKeyValueSplitter = new KeyValueSplitter("rulebook", 255, 8192, SetRuleBookKeyValue);

	public static event Action<List<KeyValuePair<string, string>>> getAdditionalKeyValues;

	[SystemInitializer(new Type[] { typeof(RuleBook) })]
	private static void Init()
	{
		cachedRuleBook = new RuleBook();
		if (PlatformSystems.EgsToggleConVar.value == 1)
		{
			LobbyManager lobbyManager = PlatformSystems.lobbyManager;
			lobbyManager.onLobbyOwnershipGained = (Action)Delegate.Combine(lobbyManager.onLobbyOwnershipGained, new Action(OnLobbyOwnershipGained));
			LobbyManager lobbyManager2 = PlatformSystems.lobbyManager;
			lobbyManager2.onLobbyOwnershipLost = (Action)Delegate.Combine(lobbyManager2.onLobbyOwnershipLost, new Action(OnLobbyOwnershipLost));
		}
	}

	private static void OnLobbyOwnershipGained()
	{
		//IL_0020: Unknown result type (might be due to invalid IL or missing references)
		//IL_0026: Expected O, but got Unknown
		//IL_0026: Unknown result type (might be due to invalid IL or missing references)
		//IL_002b: Unknown result type (might be due to invalid IL or missing references)
		//IL_003d: Expected O, but got Unknown
		//IL_0038: Unknown result type (might be due to invalid IL or missing references)
		EOSLobbyManager obj = PlatformSystems.lobbyManager as EOSLobbyManager;
		lobbyId = obj.CurrentLobbyId;
		edition = 0;
		LobbyDetails currentLobbyDetails = obj.CurrentLobbyDetails;
		Attribute val = new Attribute();
		if ((int)currentLobbyDetails.CopyAttributeByKey(new LobbyDetailsCopyAttributeByKeyOptions
		{
			AttrKey = "v"
		}, ref val) == 0)
		{
			int.TryParse(val.Data.Value.AsUtf8, out edition);
		}
		LobbyManager lobbyManager = PlatformSystems.lobbyManager;
		lobbyManager.onLobbyMemberDataUpdated = (Action<UserID>)Delegate.Combine(lobbyManager.onLobbyMemberDataUpdated, new Action<UserID>(EOSLobbyManagerOnOnLobbyMemberDataUpdated));
		LobbyManager lobbyManager2 = PlatformSystems.lobbyManager;
		lobbyManager2.onLobbyStateChanged = (Action)Delegate.Combine(lobbyManager2.onLobbyStateChanged, new Action(EOSLobbyManagerOnOnLobbyStateChanged));
		NetworkManagerSystem.onStartClientGlobal += NetworkManagerSystemOnOnStartClientGlobal;
		NetworkManagerSystem.onStopClientGlobal += NetworkManagerSystemOnOnStopClientGlobal;
		SceneCatalog.onMostRecentSceneDefChanged += SceneCatalogOnOnMostRecentSceneDefChanged;
		NetworkUser.onNetworkUserDiscovered += NetworkUserOnOnNetworkUserDiscovered;
		NetworkUser.onNetworkUserLost += NetworkUserOnOnNetworkUserLost;
		PreGameController.onPreGameControllerSetRuleBookGlobal += OnPreGameControllerSetRuleBook;
		Run.onRunSetRuleBookGlobal += OnRunSetRuleBook;
		UpdateRuleBook();
		RebuildLobbyData();
	}

	private static void OnLobbyOwnershipLost()
	{
		lobbyId = string.Empty;
		edition = 0;
		Run.onRunSetRuleBookGlobal -= OnRunSetRuleBook;
		PreGameController.onPreGameControllerSetRuleBookGlobal -= OnPreGameControllerSetRuleBook;
		NetworkUser.onNetworkUserLost -= NetworkUserOnOnNetworkUserLost;
		NetworkUser.onNetworkUserDiscovered -= NetworkUserOnOnNetworkUserDiscovered;
		SceneCatalog.onMostRecentSceneDefChanged -= SceneCatalogOnOnMostRecentSceneDefChanged;
		NetworkManagerSystem.onStopClientGlobal -= NetworkManagerSystemOnOnStopClientGlobal;
		NetworkManagerSystem.onStartClientGlobal -= NetworkManagerSystemOnOnStartClientGlobal;
		LobbyManager lobbyManager = PlatformSystems.lobbyManager;
		lobbyManager.onLobbyMemberDataUpdated = (Action<UserID>)Delegate.Remove(lobbyManager.onLobbyMemberDataUpdated, new Action<UserID>(EOSLobbyManagerOnOnLobbyMemberDataUpdated));
		LobbyManager lobbyManager2 = PlatformSystems.lobbyManager;
		lobbyManager2.onLobbyStateChanged = (Action)Delegate.Remove(lobbyManager2.onLobbyStateChanged, new Action(EOSLobbyManagerOnOnLobbyStateChanged));
	}

	private static void OnPreGameControllerSetRuleBook(PreGameController run, RuleBook ruleBook)
	{
		UpdateRuleBook();
	}

	private static void OnRunSetRuleBook(Run run, RuleBook ruleBook)
	{
		UpdateRuleBook();
	}

	private static void EOSLobbyManagerOnOnLobbyMemberDataUpdated(UserID memberId)
	{
		SetDirty();
	}

	private static void EOSLobbyManagerOnOnLobbyStateChanged()
	{
		SetDirty();
	}

	private static void NetworkManagerSystemOnOnStartClientGlobal(NetworkClient networkClient)
	{
		SetDirty();
	}

	private static void NetworkManagerSystemOnOnStopClientGlobal()
	{
		SetDirty();
	}

	private static void SceneCatalogOnOnMostRecentSceneDefChanged(SceneDef sceneDef)
	{
		SetDirty();
	}

	private static void NetworkUserOnOnNetworkUserDiscovered(NetworkUser networkUser)
	{
		SetDirty();
	}

	private static void NetworkUserOnOnNetworkUserLost(NetworkUser networkUser)
	{
		SetDirty();
	}

	public static void SetDirty()
	{
		if (!dirty)
		{
			dirty = true;
			RoR2Application.onNextUpdate += RebuildLobbyData;
		}
	}

	public static void RebuildLobbyData()
	{
		//IL_00b6: Unknown result type (might be due to invalid IL or missing references)
		//IL_00c0: Expected O, but got Unknown
		//IL_0374: Unknown result type (might be due to invalid IL or missing references)
		//IL_037b: Expected O, but got Unknown
		try
		{
			dirty = false;
			LobbyDetails currentLobbyDetails = (PlatformSystems.lobbyManager as EOSLobbyManager).CurrentLobbyDetails;
			if ((Handle)(object)currentLobbyDetails == (Handle)null)
			{
				return;
			}
			LobbyDetails val = currentLobbyDetails;
			Dictionary<string, string> KVPs = CollectionPool<KeyValuePair<string, string>, Dictionary<string, string>>.RentCollection();
			EOSLobbyManager.GetAllCurrentLobbyKVPs(currentLobbyDetails, ref KVPs);
			if (!EOSLobbyManager.IsLobbyOwner(currentLobbyDetails) || KVPs == null || (Handle)(object)val == (Handle)null)
			{
				CollectionPool<KeyValuePair<string, string>, Dictionary<string, string>>.ReturnCollection(KVPs);
				return;
			}
			LobbyModification currentLobbyModification = (PlatformSystems.lobbyManager as EOSLobbyManager).CurrentLobbyModification;
			string value2 = null;
			KVPs.TryGetValue("total_max_players", out value2);
			if (value2 == null || !int.TryParse(value2, out var result))
			{
				result = LobbyManager.cvSteamLobbyMaxMembers.value;
				EOSLobbyManager.SetLobbyStringValue(currentLobbyModification, "total_max_players", result.ToString());
			}
			int num = result - PlatformSystems.lobbyManager.calculatedExtraPlayersCount;
			currentLobbyDetails.GetMemberCount(new LobbyDetailsGetMemberCountOptions());
			_ = num;
			Dictionary<string, string> newData = CollectionPool<KeyValuePair<string, string>, Dictionary<string, string>>.RentCollection();
			List<KeyValuePair<string, string>> list = CollectionPool<KeyValuePair<string, string>, List<KeyValuePair<string, string>>>.RentCollection();
			EOSLobbyDataGenerator.getAdditionalKeyValues?.Invoke(list);
			for (int i = 0; i < list.Count; i++)
			{
				KeyValuePair<string, string> keyValuePair = list[i];
				AddData(keyValuePair.Key, keyValuePair.Value);
			}
			CollectionPool<KeyValuePair<string, string>, List<KeyValuePair<string, string>>>.ReturnCollection(list);
			AddData("build_id", RoR2Application.GetBuildId());
			AddData("_mh", NetworkModCompatibilityHelper.networkModHash);
			int input = PlatformSystems.lobbyManager.calculatedTotalPlayerCount;
			AddData("player_count", ToStringCache.playerCount.GetString(in input));
			string value3 = SceneCatalog.GetSceneDefForCurrentScene()?.baseSceneName;
			if (!string.IsNullOrEmpty(value3))
			{
				AddData("_map", value3);
			}
			GetServerInfo(out var serverId, out var serverAddress, out var isSelf);
			bool flag = false;
			bool num2 = KVPs?.ContainsKey("server_id") ?? false;
			bool flag2 = KVPs?.ContainsKey("server_address") ?? false;
			bool flag3 = num2 || flag2;
			bool flag4 = false;
			if (Object.op_Implicit((Object)(object)NetworkSession.instance))
			{
				flag = NetworkSession.instance.HasFlag(NetworkSession.Flags.HasPassword);
				AddData("_ds", NetworkSession.instance.HasFlag(NetworkSession.Flags.IsDedicatedServer) ? "1" : "0");
				AddData("_pw", flag ? "1" : "0");
				AddData("_svtags", (NetworkSession.instance.tagsString != null) ? NetworkSession.instance.tagsString : "");
				input = (int)NetworkSession.instance.maxPlayers;
				AddData("_svmpl", ToStringCache.serverMaxPlayers.GetString(in input));
				input = NetworkUser.readOnlyInstancesList.Count;
				AddData("_svplc", ToStringCache.serverPlayerCount.GetString(in input));
				AddData("_svnm", NetworkSession.instance.serverName);
				if ((Handle)(object)serverId != (Handle)null)
				{
					AddData("server_id", ((object)serverId).ToString());
					flag4 = true;
				}
				if (serverAddress.isValid)
				{
					AddData("server_address", ToStringCache.serverAddress.GetString(in serverAddress));
					flag4 = true;
				}
			}
			GameModeIndex gameModeIndex = GameModeIndex.Invalid;
			if (Object.op_Implicit((Object)(object)Run.instance))
			{
				gameModeIndex = Run.instance.gameModeIndex;
			}
			else if (Object.op_Implicit((Object)(object)PreGameController.instance))
			{
				gameModeIndex = PreGameController.instance.gameModeIndex;
			}
			if (gameModeIndex != GameModeIndex.Invalid)
			{
				string gameModeName = GameModeCatalog.GetGameModeName(gameModeIndex);
				AddData("_svgm", gameModeName);
			}
			if (flag4 && !flag3 && flag)
			{
				string text = (isSelf ? NetworkManagerSystem.SvPasswordConVar.instance.value : NetworkManagerSystem.cvClPassword.value);
				NetworkWriter val2 = new NetworkWriter();
				val2.Write(text);
				(PlatformSystems.lobbyManager as EOSLobbyManager).SendLobbyMessage(LobbyManager.LobbyMessageType.Password, val2);
				Debug.Log((object)"Password attempt shared with lobby members to facilitate game join.");
			}
			for (int j = 0; j < ruleBookKeyValues.Count; j++)
			{
				KeyValuePair<string, string> keyValuePair2 = ruleBookKeyValues[j];
				AddData(keyValuePair2.Key, keyValuePair2.Value);
			}
			for (int k = 0; k < specialKeys.Length; k++)
			{
				KVPs.Remove(specialKeys[k]);
			}
			bool flag5 = false;
			List<string> list2 = CollectionPool<string, List<string>>.RentCollection();
			if (KVPs != null)
			{
				foreach (KeyValuePair<string, string> item in KVPs)
				{
					if (!newData.ContainsKey(item.Key))
					{
						list2.Add(item.Key);
					}
				}
			}
			for (int l = 0; l < list2.Count; l++)
			{
				EOSLobbyManager.RemoveLobbyStringValue(currentLobbyModification, list2[l]);
			}
			if (list2.Count > 0)
			{
				flag5 = true;
			}
			CollectionPool<string, List<string>>.ReturnCollection(list2);
			foreach (KeyValuePair<string, string> item2 in newData)
			{
				string value4 = null;
				if (KVPs == null || !KVPs.TryGetValue(item2.Key, out value4) || !item2.Value.Equals(value4, StringComparison.Ordinal))
				{
					EOSLobbyManager.SetLobbyStringValue(currentLobbyModification, item2.Key, item2.Value);
					flag5 = true;
				}
			}
			if (flag5)
			{
				edition++;
			}
			EOSLobbyManager.SetLobbyStringValue(currentLobbyModification, "v", ToStringCache.edition.GetString(in edition));
			EOSLobbyManager.UpdateLobby(currentLobbyModification);
			CollectionPool<KeyValuePair<string, string>, Dictionary<string, string>>.ReturnCollection(newData);
			CollectionPool<KeyValuePair<string, string>, Dictionary<string, string>>.ReturnCollection(KVPs);
			void AddData(string key, string value)
			{
				newData.Add(key, (value != null) ? value : "");
			}
		}
		catch (Exception ex)
		{
			Debug.LogError((object)ex);
		}
	}

	private static void GetServerInfo(out ProductUserId serverId, out AddressPortPair serverAddress, out bool isSelf)
	{
		//IL_0001: Unknown result type (might be due to invalid IL or missing references)
		//IL_0007: Expected O, but got Unknown
		serverId = new ProductUserId();
		serverAddress = default(AddressPortPair);
		isSelf = false;
		HostDescription desiredHost = NetworkManagerSystem.singleton.desiredHost;
		if (desiredHost.hostType == HostDescription.HostType.None)
		{
			return;
		}
		if (desiredHost.hostType == HostDescription.HostType.Self)
		{
			if (NetworkServer.active)
			{
				serverId = NetworkManagerSystem.singleton.serverP2PId.egsValue;
				isSelf = true;
			}
		}
		else if (desiredHost.hostType == HostDescription.HostType.Steam)
		{
			serverId = desiredHost.userID.CID.egsValue;
		}
		else if (desiredHost.hostType == HostDescription.HostType.IPv4)
		{
			serverAddress = desiredHost.addressPortPair;
		}
	}

	private static void UpdateRuleBook()
	{
		RuleBook ruleBook = null;
		if (Object.op_Implicit((Object)(object)Run.instance))
		{
			ruleBook = Run.instance.ruleBook;
		}
		else if (Object.op_Implicit((Object)(object)PreGameController.instance))
		{
			ruleBook = PreGameController.instance.readOnlyRuleBook;
		}
		if (ruleBook != null && !ruleBook.Equals(cachedRuleBook))
		{
			cachedRuleBook.Copy(ruleBook);
			StringBuilder stringBuilder = StringBuilderPool.RentStringBuilder();
			RuleBook.WriteBase64ToStringBuilder(cachedRuleBook, stringBuilder);
			ruleBookKeyValueSplitter.SetValue(stringBuilder);
			StringBuilderPool.ReturnStringBuilder(stringBuilder);
		}
		else
		{
			cachedRuleBook.SetToDefaults();
		}
	}

	private static void SetRuleBookKeyValue(string key, string value)
	{
		int num = -1;
		for (int i = 0; i < ruleBookKeyValues.Count; i++)
		{
			if (ruleBookKeyValues[i].Key.Equals(key, StringComparison.Ordinal))
			{
				if (ruleBookKeyValues[i].Value.Equals(value, StringComparison.Ordinal))
				{
					return;
				}
				num = i;
				break;
			}
		}
		if (value == null)
		{
			if (num != -1)
			{
				ruleBookKeyValues.RemoveAt(num);
			}
		}
		else
		{
			KeyValuePair<string, string> keyValuePair = new KeyValuePair<string, string>(key, value);
			if (num != -1)
			{
				ruleBookKeyValues[num] = keyValuePair;
			}
			else
			{
				ruleBookKeyValues.Add(keyValuePair);
			}
		}
		SetDirty();
	}

	[ConCommand(commandName = "steam_lobby_data_regenerate", flags = ConVarFlags.None, helpText = "Forces the current lobby data to be regenerated.")]
	public static void CCSteamLobbyRegenerateData(ConCommandArgs args)
	{
		SetDirty();
	}
}
