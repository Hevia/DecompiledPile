using UnityEngine;

namespace RoR2.Achievements;

[RegisterAchievement("CompletePrismaticTrial", "Items.HealOnCrit", null, null)]
public class CompletePrismaticTrialAchievement : BaseEndingAchievement
{
	protected override bool ShouldGrant(RunReport runReport)
	{
		return (Object)(object)runReport.gameEnding == (Object)(object)GameEndingCatalog.FindGameEndingDef("PrismaticTrialEnding");
	}
}
