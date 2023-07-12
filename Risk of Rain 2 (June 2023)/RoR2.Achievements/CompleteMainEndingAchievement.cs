using UnityEngine;

namespace RoR2.Achievements;

[RegisterAchievement("CompleteMainEnding", "Characters.Captain", null, null)]
public class CompleteMainEndingAchievement : BaseEndingAchievement
{
	protected override bool ShouldGrant(RunReport runReport)
	{
		if ((Object)(object)runReport.gameEnding == (Object)(object)RoR2Content.GameEndings.MainEnding)
		{
			return true;
		}
		return false;
	}
}
