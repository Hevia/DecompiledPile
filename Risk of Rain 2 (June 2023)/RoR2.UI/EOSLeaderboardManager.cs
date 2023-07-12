using System;
using System.Collections.Generic;
using System.Linq;
using Epic.OnlineServices;
using Epic.OnlineServices.Leaderboards;
using UnityEngine;

namespace RoR2.UI;

public class EOSLeaderboardManager : LeaderboardManager
{
	public const string kPrismaticTrialsLeaderboardId = "PrismaticTrials";

	private readonly LeaderboardsInterface _leaderboardsInterface;

	private readonly List<LeaderboardRecord> _currentGlobalLeaderboardList = new List<LeaderboardRecord>();

	private readonly List<LeaderboardUserScore> _currentFriendsLeaderboardList = new List<LeaderboardUserScore>();

	private static bool _isGlobal;

	private static ProductUserId LocalUserId => EOSLoginManager.loggedInProductId;

	private static DateTime StartTime => WeeklyRun.GetSeedCycleStartDateTime();

	private static DateTime EndTime => WeeklyRun.GetSeedCycleEndDateTime();

	public EOSLeaderboardManager(LeaderboardController leaderboardController)
		: base(leaderboardController)
	{
		//IL_0033: Unknown result type (might be due to invalid IL or missing references)
		//IL_0038: Unknown result type (might be due to invalid IL or missing references)
		//IL_004b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0055: Expected O, but got Unknown
		//IL_0055: Expected O, but got Unknown
		_leaderboardsInterface = EOSPlatformManager.GetPlatformInterface().GetLeaderboardsInterface();
		_leaderboardsInterface.QueryLeaderboardDefinitions(new QueryLeaderboardDefinitionsOptions
		{
			LocalUserId = LocalUserId
		}, (object)null, new OnQueryLeaderboardDefinitionsCompleteCallback(LeaderboardQueryComplete));
		base.IsQuerying = true;
	}

	private void LeaderboardQueryComplete(OnQueryLeaderboardDefinitionsCompleteCallbackInfo data)
	{
		//IL_0002: Unknown result type (might be due to invalid IL or missing references)
		//IL_0008: Invalid comparison between Unknown and I4
		base.IsValid = (int)data.ResultCode == 0;
		base.IsQuerying = false;
	}

	internal override void UpdateLeaderboard()
	{
		switch (LeaderboardController.currentRequestType)
		{
		case RequestType.Global:
		case RequestType.GlobalAroundUser:
			QueryLeaderboardRanks();
			_isGlobal = true;
			break;
		case RequestType.Friends:
			QueryFriendsLeaderboard();
			_isGlobal = false;
			break;
		default:
			throw new ArgumentOutOfRangeException();
		}
		base.IsQuerying = true;
	}

	private void QueryFriendsLeaderboard()
	{
		//IL_0011: Unknown result type (might be due to invalid IL or missing references)
		//IL_0017: Expected O, but got Unknown
		//IL_0055: Unknown result type (might be due to invalid IL or missing references)
		//IL_005a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0061: Unknown result type (might be due to invalid IL or missing references)
		//IL_006d: Expected O, but got Unknown
		//IL_0086: Unknown result type (might be due to invalid IL or missing references)
		//IL_0090: Expected O, but got Unknown
		ProductUserId[] userIds = EOSFriendsManager.FriendsProductUserIds.ToArray();
		LeaderboardsInterface leaderboardsInterface = _leaderboardsInterface;
		QueryLeaderboardUserScoresOptions val = new QueryLeaderboardUserScoresOptions();
		val.StartTime = StartTime;
		val.EndTime = EndTime;
		val.LocalUserId = LocalUserId;
		val.StatInfo = (UserScoresQueryStatInfo[])(object)new UserScoresQueryStatInfo[1]
		{
			new UserScoresQueryStatInfo
			{
				Aggregation = (LeaderboardAggregation)0,
				StatName = "FASTESTWEEKLYRUN"
			}
		};
		val.UserIds = userIds;
		leaderboardsInterface.QueryLeaderboardUserScores(val, (object)"FASTESTWEEKLYRUN", new OnQueryLeaderboardUserScoresCompleteCallback(FriendsQueryComplete));
	}

	private void FriendsQueryComplete(OnQueryLeaderboardUserScoresCompleteCallbackInfo data)
	{
		//IL_0022: Unknown result type (might be due to invalid IL or missing references)
		//IL_0027: Unknown result type (might be due to invalid IL or missing references)
		//IL_0033: Expected O, but got Unknown
		string text = data.ClientData?.ToString() ?? "";
		uint leaderboardUserScoreCount = _leaderboardsInterface.GetLeaderboardUserScoreCount(new GetLeaderboardUserScoreCountOptions
		{
			StatName = text
		});
		UpdateFriendsLeaderboardList(text, leaderboardUserScoreCount);
	}

	private void UpdateFriendsLeaderboardList(string leaderboardName, uint leaderboardMax)
	{
		int num = LeaderboardController.currentPage * LeaderboardController.entriesPerPage;
		long num2 = ((num + LeaderboardController.entriesPerPage > leaderboardMax) ? (leaderboardMax - num) : LeaderboardController.entriesPerPage);
		_currentFriendsLeaderboardList.Clear();
		for (int i = num; i < num2; i++)
		{
			GetFriendsLeaderboardEntry(Convert.ToUInt32(i), leaderboardName);
		}
	}

	private void GetFriendsLeaderboardEntry(uint leaderboardRecordIndex, string statName)
	{
		//IL_0006: Unknown result type (might be due to invalid IL or missing references)
		//IL_000b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0012: Unknown result type (might be due to invalid IL or missing references)
		//IL_0020: Expected O, but got Unknown
		//IL_001b: Unknown result type (might be due to invalid IL or missing references)
		LeaderboardUserScore val = default(LeaderboardUserScore);
		if ((int)_leaderboardsInterface.CopyLeaderboardUserScoreByIndex(new CopyLeaderboardUserScoreByIndexOptions
		{
			LeaderboardUserScoreIndex = leaderboardRecordIndex,
			StatName = statName
		}, ref val) == 0 && val != null)
		{
			Debug.Log((object)($"Leaderboard user score {val}, Index {leaderboardRecordIndex} successfully fetched!" + string.Format("\n {0}: {1}", "Score", val.Score) + string.Format("\n {0}: {1}", "UserId", val.UserId)));
			(PlatformSystems.userManager as UserManagerEOS)?.QueryForDisplayNames(val.UserId, delegate
			{
				base.IsQuerying = false;
			});
			_currentFriendsLeaderboardList.Add(val);
		}
	}

	private void QueryLeaderboardRanks()
	{
		//IL_0006: Unknown result type (might be due to invalid IL or missing references)
		//IL_000b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0016: Unknown result type (might be due to invalid IL or missing references)
		//IL_0033: Unknown result type (might be due to invalid IL or missing references)
		//IL_003d: Expected O, but got Unknown
		//IL_003d: Expected O, but got Unknown
		_leaderboardsInterface.QueryLeaderboardRanks(new QueryLeaderboardRanksOptions
		{
			LeaderboardId = "PrismaticTrials",
			LocalUserId = LocalUserId
		}, (object)LeaderboardController.currentLeaderboardName, new OnQueryLeaderboardRanksCompleteCallback(GlobalQueryComplete));
	}

	private void GlobalQueryComplete(OnQueryLeaderboardRanksCompleteCallbackInfo data)
	{
		//IL_0022: Unknown result type (might be due to invalid IL or missing references)
		//IL_002c: Expected O, but got Unknown
		string leaderboardName = data.ClientData?.ToString() ?? "";
		uint leaderboardRecordCount = _leaderboardsInterface.GetLeaderboardRecordCount(new GetLeaderboardRecordCountOptions());
		UpdateGlobalLeaderboardList(leaderboardName, leaderboardRecordCount);
	}

	private void UpdateGlobalLeaderboardList(string leaderboardName, uint leaderboardMax)
	{
		int num = LeaderboardController.currentPage * LeaderboardController.entriesPerPage;
		long num2 = ((num + LeaderboardController.entriesPerPage > leaderboardMax) ? (leaderboardMax - num) : LeaderboardController.entriesPerPage);
		_currentGlobalLeaderboardList.Clear();
		for (int i = num; i < num2; i++)
		{
			GetGlobalLeaderboardEntry(Convert.ToUInt32(i), leaderboardName);
		}
	}

	private void GetGlobalLeaderboardEntry(uint leaderboardRecordIndex, string leaderboardName)
	{
		//IL_0006: Unknown result type (might be due to invalid IL or missing references)
		//IL_000b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0019: Expected O, but got Unknown
		//IL_0014: Unknown result type (might be due to invalid IL or missing references)
		LeaderboardRecord val = default(LeaderboardRecord);
		if ((int)_leaderboardsInterface.CopyLeaderboardRecordByIndex(new CopyLeaderboardRecordByIndexOptions
		{
			LeaderboardRecordIndex = leaderboardRecordIndex
		}, ref val) == 0 && val != null)
		{
			Debug.Log((object)($"Leaderboard {leaderboardName}, Index {leaderboardRecordIndex} successfully fetched!" + "\n UserDisplayName: " + val.UserDisplayName + string.Format("\n {0}: {1}", "UserId", val.UserId) + string.Format("\n {0}: {1}", "Rank", val.Rank) + string.Format("\n {0}: {1}", "Score", val.Score)));
			(PlatformSystems.userManager as UserManagerEOS)?.QueryForDisplayNames(val.UserId, delegate
			{
				base.IsQuerying = false;
			});
			_currentGlobalLeaderboardList.Add(val);
		}
	}

	internal override List<LeaderboardInfo> GetLeaderboardInfoList()
	{
		if (_isGlobal)
		{
			return _currentGlobalLeaderboardList.Select(delegate(LeaderboardRecord leaderboardRecord)
			{
				LeaderboardInfo result2 = default(LeaderboardInfo);
				result2.rank = Convert.ToInt32(leaderboardRecord.Rank);
				result2.survivorIndex = null;
				result2.timeInSeconds = leaderboardRecord.Score;
				result2.userID = ((object)leaderboardRecord.UserId).ToString();
				return result2;
			}).ToList();
		}
		_currentFriendsLeaderboardList.Sort((LeaderboardUserScore x, LeaderboardUserScore y) => x.Score.CompareTo(y.Score));
		return _currentFriendsLeaderboardList.Select(delegate(LeaderboardUserScore leaderboardUserScore, int index)
		{
			LeaderboardInfo result = default(LeaderboardInfo);
			result.rank = index + 1;
			result.survivorIndex = null;
			result.timeInSeconds = leaderboardUserScore.Score;
			result.userID = ((object)leaderboardUserScore.UserId).ToString();
			return result;
		}).ToList();
	}

	internal override UserID GetUserID(LeaderboardInfo leaderboardInfo)
	{
		return new UserID(ProductUserId.FromString(leaderboardInfo.userID));
	}

	internal override string GetLocalUserIdString()
	{
		return ((object)LocalUserId).ToString();
	}
}
