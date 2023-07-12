using UnityEngine;

namespace RoR2.Achievements;

public class BasePerSurvivorClearGameMonsoonAchievement : BaseAchievement
{
	protected override void OnBodyRequirementMet()
	{
		base.OnBodyRequirementMet();
		Run.onClientGameOverGlobal += OnClientGameOverGlobal;
	}

	protected override void OnBodyRequirementBroken()
	{
		Run.onClientGameOverGlobal -= OnClientGameOverGlobal;
		base.OnBodyRequirementBroken();
	}

	private void OnClientGameOverGlobal(Run run, RunReport runReport)
	{
		if (Object.op_Implicit((Object)(object)runReport.gameEnding) && runReport.gameEnding.isWin)
		{
			DifficultyDef difficultyDef = DifficultyCatalog.GetDifficultyDef(runReport.ruleBook.FindDifficulty());
			if (difficultyDef != null && difficultyDef.countsAsHardMode)
			{
				Grant();
			}
		}
	}
}
