using UnityEngine;

namespace RoR2.Achievements;

[RegisterAchievement("CompleteMainEndingHard", "Items.LunarBadLuck", null, null)]
public class CompleteMainEndingHardAchievement : BaseEndingAchievement
{
	protected override bool ShouldGrant(RunReport runReport)
	{
		if ((Object)(object)runReport.gameEnding == (Object)(object)RoR2Content.GameEndings.MainEnding && runReport.ruleBook.FindDifficulty() >= DifficultyIndex.Hard)
		{
			return true;
		}
		return false;
	}
}
