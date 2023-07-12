using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using Facepunch.Steamworks;
using Facepunch.Steamworks.Callbacks;

namespace RoR2.UI;

public class SteamLeaderboardManager : LeaderboardManager
{
	[Serializable]
	[CompilerGenerated]
	private sealed class _003C_003Ec
	{
		public static readonly _003C_003Ec _003C_003E9 = new _003C_003Ec();

		public static FailureCallback _003C_003E9__6_1;

		internal void _003CUpdateLeaderboard_003Eb__6_1(Result reason)
		{
		}
	}

	private Leaderboard _currentLeaderboard;

	private List<LeaderboardInfo> _leaderboardInfoList = new List<LeaderboardInfo>();

	public SteamLeaderboardManager(LeaderboardController leaderboardController)
		: base(leaderboardController)
	{
		base.IsValid = true;
	}

	internal override List<LeaderboardInfo> GetLeaderboardInfoList()
	{
		return _leaderboardInfoList;
	}

	internal override UserID GetUserID(LeaderboardInfo leaderboardInfo)
	{
		return new UserID(Convert.ToUInt64(leaderboardInfo.userID));
	}

	internal override string GetLocalUserIdString()
	{
		return Client.Instance.SteamId.ToString();
	}

	internal override void UpdateLeaderboard()
	{
		//IL_008c: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a5: Unknown result type (might be due to invalid IL or missing references)
		//IL_00aa: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b0: Expected O, but got Unknown
		//IL_00b5: Expected O, but got Unknown
		_currentLeaderboard = Client.Instance.GetLeaderboard(LeaderboardController.currentLeaderboardName, (LeaderboardSortMethod)0, (LeaderboardDisplayType)0);
		int num = LeaderboardController.currentPage * LeaderboardController.entriesPerPage - LeaderboardController.entriesPerPage / 2;
		base.IsValid = _currentLeaderboard != null && _currentLeaderboard.IsValid;
		base.IsQuerying = true;
		Leaderboard currentLeaderboard = _currentLeaderboard;
		RequestType currentRequestType = LeaderboardController.currentRequestType;
		int num2 = num + LeaderboardController.entriesPerPage;
		FetchScoresCallback val = delegate(Entry[] entries)
		{
			_leaderboardInfoList.SetLeaderboardInfo(entries.Select(LeaderboardInfoFromSteamLeaderboardEntry).ToArray());
			base.IsQuerying = false;
			base.IsValid = _currentLeaderboard.IsValid;
		};
		object obj = _003C_003Ec._003C_003E9__6_1;
		if (obj == null)
		{
			FailureCallback val2 = delegate
			{
			};
			obj = (object)val2;
			_003C_003Ec._003C_003E9__6_1 = val2;
		}
		currentLeaderboard.FetchScores((RequestType)currentRequestType, num, num2, val, (FailureCallback)obj);
	}

	private static LeaderboardInfo LeaderboardInfoFromSteamLeaderboardEntry(Entry entry)
	{
		//IL_0002: Unknown result type (might be due to invalid IL or missing references)
		//IL_000a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0018: Unknown result type (might be due to invalid IL or missing references)
		//IL_0039: Unknown result type (might be due to invalid IL or missing references)
		//IL_006d: Unknown result type (might be due to invalid IL or missing references)
		SurvivorIndex value = SurvivorIndex.None;
		int num = ((entry.SubScores != null && entry.SubScores.Length >= 1) ? entry.SubScores[1] : 0);
		if (num >= 0 && num < SurvivorCatalog.survivorCount)
		{
			value = (SurvivorIndex)num;
		}
		LeaderboardInfo result = default(LeaderboardInfo);
		result.timeInSeconds = (float)entry.Score * 0.001f;
		result.survivorIndex = value;
		result.userID = entry.SteamId.ToString();
		result.rank = entry.GlobalRank;
		return result;
	}
}
