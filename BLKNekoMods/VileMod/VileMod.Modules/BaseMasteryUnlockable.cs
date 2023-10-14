using R2API;
using RoR2;
using RoR2.Achievements;
using UnityEngine;

namespace VileMod.Modules;

public abstract class BaseMasteryUnlockable : GenericModdedUnlockable
{
	public abstract string RequiredCharacterBody { get; }

	public abstract float RequiredDifficultyCoefficient { get; }

	public override void OnBodyRequirementMet()
	{
		((ModdedUnlockable)this).OnBodyRequirementMet();
		Run.onClientGameOverGlobal += OnClientGameOverGlobal;
	}

	public override void OnBodyRequirementBroken()
	{
		Run.onClientGameOverGlobal -= OnClientGameOverGlobal;
		((ModdedUnlockable)this).OnBodyRequirementBroken();
	}

	private void OnClientGameOverGlobal(Run run, RunReport runReport)
	{
		//IL_0027: Unknown result type (might be due to invalid IL or missing references)
		//IL_002c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0033: Unknown result type (might be due to invalid IL or missing references)
		//IL_0054: Unknown result type (might be due to invalid IL or missing references)
		//IL_0056: Invalid comparison between Unknown and I4
		//IL_0058: Unknown result type (might be due to invalid IL or missing references)
		//IL_005b: Invalid comparison between Unknown and I4
		if (Object.op_Implicit((Object)(object)runReport.gameEnding) && runReport.gameEnding.isWin)
		{
			DifficultyIndex val = runReport.ruleBook.FindDifficulty();
			DifficultyDef difficultyDef = DifficultyCatalog.GetDifficultyDef(runReport.ruleBook.FindDifficulty());
			if ((difficultyDef.countsAsHardMode && difficultyDef.scalingValue >= RequiredDifficultyCoefficient) || ((int)val >= 3 && (int)val <= 10) || difficultyDef.nameToken == "INFERNO_NAME")
			{
				((BaseAchievement)this).Grant();
			}
		}
	}

	public override BodyIndex LookUpRequiredBodyIndex()
	{
		//IL_0007: Unknown result type (might be due to invalid IL or missing references)
		//IL_000c: Unknown result type (might be due to invalid IL or missing references)
		//IL_000f: Unknown result type (might be due to invalid IL or missing references)
		return BodyCatalog.FindBodyIndex(RequiredCharacterBody);
	}
}
