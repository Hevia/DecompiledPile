using RoR2.Stats;

namespace RoR2.Achievements;

[RegisterAchievement("KillElitesMilestone", "Items.ExecuteLowHealthElite", null, null)]
public class KillElitesMilestoneAchievement : BaseStatMilestoneAchievement
{
	protected override StatDef statDef => StatDef.totalEliteKills;

	protected override ulong statRequirement => 500uL;
}
