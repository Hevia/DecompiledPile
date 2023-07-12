using RoR2.Stats;

namespace RoR2.Achievements;

[RegisterAchievement("CleanupDuty", "Items.Recycle", null, null)]
public class CleanupDuty : BaseStatMilestoneAchievement
{
	protected override StatDef statDef => StatDef.totalMaulingRockKills;

	protected override ulong statRequirement => 20uL;
}
