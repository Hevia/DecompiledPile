using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Epic.OnlineServices.Lobby;
using HG;
using RoR2.Networking;

namespace RoR2.RemoteGameBrowser;

public class EOSLobbyRemoteGameProvider : BaseAsyncRemoteGameProvider
{
	private readonly List<LobbyDetails> lobbyList = new List<LobbyDetails>();

	private int waitingForLobbyCount;

	private static readonly char[] tagsSeparator = new char[1] { ',' };

	private EOSLobbyManager.Filter BuildFilter()
	{
		//IL_0013: Unknown result type (might be due to invalid IL or missing references)
		//IL_0018: Unknown result type (might be due to invalid IL or missing references)
		//IL_0023: Unknown result type (might be due to invalid IL or missing references)
		//IL_0034: Expected O, but got Unknown
		EOSLobbyManager.Filter filter = new EOSLobbyManager.Filter();
		if (!searchFilters.allowMismatchedMods)
		{
			AttributeData item = new AttributeData
			{
				Key = "_mh",
				Value = AttributeDataValue.op_Implicit(NetworkModCompatibilityHelper.networkModHash)
			};
			filter.SearchData.Add(item);
		}
		return filter;
	}

	public override bool RequestRefresh()
	{
		EOSLobbyManager.Filter filter = BuildFilter();
		bool num = (PlatformSystems.lobbyManager as EOSLobbyManager).RequestLobbyList(this, filter, OnLobbyListReceived);
		if (num)
		{
			waitingForLobbyCount++;
		}
		return num;
	}

	private void OnLobbyListReceived(List<LobbyDetails> lobbies)
	{
		if (base.disposed)
		{
			return;
		}
		waitingForLobbyCount--;
		lock (lobbyList)
		{
			lobbyList.Clear();
			lobbyList.AddRange(lobbies);
			SetDirty();
		}
	}

	protected override Task<RemoteGameInfo[]> CreateTask(CancellationToken cancellationToken)
	{
		LobbyDetails[] lobbies;
		lock (lobbyList)
		{
			lobbies = lobbyList.ToArray();
		}
		return new Task<RemoteGameInfo[]>(delegate
		{
			RemoteGameInfo[] array = new RemoteGameInfo[lobbies.Length];
			for (int i = 0; i < lobbies.Length; i++)
			{
				CreateRemoteGameInfo(lobbies[i], out array[i]);
			}
			return array;
		}, cancellationToken);
	}

	private static void CreateRemoteGameInfo(LobbyDetails lobby, out RemoteGameInfo result)
	{
		//IL_0015: Unknown result type (might be due to invalid IL or missing references)
		//IL_0021: Expected O, but got Unknown
		//IL_001c: Unknown result type (might be due to invalid IL or missing references)
		result = default(RemoteGameInfo);
		LobbyDetailsInfo val = default(LobbyDetailsInfo);
		if ((int)lobby.CopyInfo(new LobbyDetailsCopyInfoOptions(), ref val) == 0)
		{
			result.name = ((object)val.LobbyOwnerUserId).ToString();
			result.lobbyName = val.LobbyId;
			result.lobbyIdStr = val.LobbyId;
			string lobbyStringValue = EOSLobbyManager.GetLobbyStringValue(lobby, "_map");
			result.serverIdStr = EOSLobbyManager.GetLobbyStringValue(lobby, "server_id");
			result.serverAddress = GetAddressPortPair("server_address");
			result.serverName = EOSLobbyManager.GetLobbyStringValue(lobby, "_svnm");
			result.lobbyPlayerCount = GetInt("player_count", 1, int.MaxValue) ?? 1;
			result.lobbyMaxPlayers = GetInt("total_max_players", 1, int.MaxValue) ?? 1;
			result.serverPlayerCount = GetInt("_svplc", 0, int.MaxValue);
			result.serverMaxPlayers = GetInt("_svmpl", 0, int.MaxValue);
			result.inGame = result.IsServerIdValid() || result.serverAddress.HasValue;
			result.currentSceneName = lobbyStringValue;
			if (lobbyStringValue != null)
			{
				result.currentSceneIndex = SceneCatalog.GetSceneDefFromSceneName(lobbyStringValue)?.sceneDefIndex;
			}
			result.requestRefreshImplementation = RemoteGameInfoRequestRefresh;
			result.getPlayersImplementation = RemoteGameInfoGetPlayers;
			result.getRuleBookImplementation = RemoteGameInfoGetRuleBook;
			result.userData = lobby;
			result.hasPassword = GetBool("_pw");
			result.gameModeName = EOSLobbyManager.GetLobbyStringValue(lobby, "_svgm");
			result.buildId = EOSLobbyManager.GetLobbyStringValue(lobby, "build_id") ?? "UNKNOWN";
			result.modHash = EOSLobbyManager.GetLobbyStringValue(lobby, "_mh") ?? "UNKNOWN";
			result.SetTags(GetTags("_svtags"));
			result.CalcExtraFields();
		}
		AddressPortPair? GetAddressPortPair(string key)
		{
			if (!AddressPortPair.TryParse(EOSLobbyManager.GetLobbyStringValue(lobby, key), out var addressPortPair))
			{
				return null;
			}
			return addressPortPair;
		}
		bool? GetBool(string key)
		{
			return GetInt(key, int.MinValue, int.MaxValue) > 0;
		}
		int? GetInt(string key, int min, int max)
		{
			if (int.TryParse(EOSLobbyManager.GetLobbyStringValue(lobby, key), out var result2) && min <= result2 && result2 <= max)
			{
				return result2;
			}
			return null;
		}
		string[] GetTags(string key)
		{
			return EOSLobbyManager.GetLobbyStringValue(lobby, key)?.Split(tagsSeparator, StringSplitOptions.None);
		}
	}

	private static void RemoteGameInfoRequestRefresh(in RemoteGameInfo remoteGameInfo, RemoteGameInfo.RequestRefreshSuccessCallback successCallback, Action failureCallback, bool fetchDetails)
	{
		if (remoteGameInfo.userData is LobbyDetails)
		{
			object userData = remoteGameInfo.userData;
			CreateRemoteGameInfo((LobbyDetails)((userData is LobbyDetails) ? userData : null), out var result);
			successCallback?.Invoke(in result);
		}
	}

	private static bool RemoteGameInfoGetRuleBook(in RemoteGameInfo remoteGameInfo, RuleBook dest)
	{
		//IL_0028: Unknown result type (might be due to invalid IL or missing references)
		//IL_0032: Expected O, but got Unknown
		//IL_0036: Unknown result type (might be due to invalid IL or missing references)
		//IL_003d: Expected O, but got Unknown
		//IL_003e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0043: Unknown result type (might be due to invalid IL or missing references)
		//IL_0052: Expected O, but got Unknown
		//IL_004d: Unknown result type (might be due to invalid IL or missing references)
		object userData = remoteGameInfo.userData;
		LobbyDetails val;
		if ((val = (LobbyDetails)((userData is LobbyDetails) ? userData : null)) != null)
		{
			KeyValueUnsplitter keyValueUnsplitter = new KeyValueUnsplitter("rulebook");
			List<KeyValuePair<string, string>> list = CollectionPool<KeyValuePair<string, string>, List<KeyValuePair<string, string>>>.RentCollection();
			try
			{
				uint num = 0u;
				for (uint attributeCount = val.GetAttributeCount(new LobbyDetailsGetAttributeCountOptions()); num < attributeCount; num++)
				{
					Attribute val2 = new Attribute();
					if ((int)val.CopyAttributeByIndex(new LobbyDetailsCopyAttributeByIndexOptions
					{
						AttrIndex = num
					}, ref val2) == 0)
					{
						list.Add(new KeyValuePair<string, string>(val2.Data.Key.ToLower(), val2.Data.Value.AsUtf8));
					}
				}
				string value = keyValueUnsplitter.GetValue(list);
				if (!string.IsNullOrEmpty(value))
				{
					RuleBook.ReadBase64(value, dest);
					return true;
				}
			}
			finally
			{
				list = CollectionPool<KeyValuePair<string, string>, List<KeyValuePair<string, string>>>.ReturnCollection(list);
			}
		}
		return false;
	}

	private static void RemoteGameInfoGetPlayers(in RemoteGameInfo remoteGameInfo, List<RemotePlayerInfo> output)
	{
		//IL_003d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0047: Expected O, but got Unknown
		object userData = remoteGameInfo.userData;
		LobbyDetails val;
		if ((val = (LobbyDetails)((userData is LobbyDetails) ? userData : null)) != null)
		{
			for (uint num = 0u; num < val.GetMemberCount(new LobbyDetailsGetMemberCountOptions()); num++)
			{
				output.Add(new RemotePlayerInfo
				{
					id = 0uL,
					name = "???"
				});
			}
		}
	}

	public override bool IsBusy()
	{
		if (!base.IsBusy())
		{
			return waitingForLobbyCount > 0;
		}
		return true;
	}
}
