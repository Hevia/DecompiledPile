using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace RoR2.UI;

public class WeeklyRunScreenController : MonoBehaviour
{
	public LeaderboardController leaderboard;

	public TextMeshProUGUI countdownLabel;

	private uint currentCycle;

	private TimeSpan lastCountdown;

	private float labelFadeValue;

	private bool leaderboardInitiated;

	private void OnEnable()
	{
		currentCycle = WeeklyRun.GetCurrentSeedCycle();
		UpdateLeaderboard();
	}

	private void UpdateLeaderboard()
	{
		if (Object.op_Implicit((Object)(object)leaderboard) && leaderboard.IsValid && !leaderboard.IsQuerying)
		{
			InitializeLeaderboardInfo();
		}
	}

	private void InitializeLeaderboardInfo()
	{
		leaderboard.SetRequestedInfo(WeeklyRun.GetLeaderboardName(1, currentCycle), leaderboard.currentRequestType, leaderboard.currentPage);
		leaderboardInitiated = true;
	}

	public void SetCurrentLeaderboard(GameObject leaderboardGameObject)
	{
		leaderboard = leaderboardGameObject.GetComponent<LeaderboardController>();
		UpdateLeaderboard();
	}

	private void Update()
	{
		//IL_00cf: Unknown result type (might be due to invalid IL or missing references)
		//IL_00d4: Unknown result type (might be due to invalid IL or missing references)
		//IL_0107: Unknown result type (might be due to invalid IL or missing references)
		uint currentSeedCycle = WeeklyRun.GetCurrentSeedCycle();
		if (currentSeedCycle != currentCycle)
		{
			currentCycle = currentSeedCycle;
			UpdateLeaderboard();
		}
		if (!leaderboardInitiated)
		{
			UpdateLeaderboard();
		}
		TimeSpan timeSpan = WeeklyRun.GetSeedCycleStartDateTime(currentCycle + 1) - WeeklyRun.now;
		string @string = Language.GetString("WEEKLY_RUN_NEXT_CYCLE_COUNTDOWN_FORMAT");
		((TMP_Text)countdownLabel).text = string.Format(@string, timeSpan.Hours + timeSpan.Days * 24, timeSpan.Minutes, timeSpan.Seconds);
		if (timeSpan != lastCountdown)
		{
			lastCountdown = timeSpan;
			labelFadeValue = 0f;
		}
		labelFadeValue = Mathf.Max(labelFadeValue + Time.deltaTime * 1f, 0f);
		Color white = Color.white;
		if (timeSpan.Days == 0 && timeSpan.Hours == 0)
		{
			white.g = labelFadeValue;
			white.b = labelFadeValue;
		}
		((Graphic)countdownLabel).color = white;
	}
}
