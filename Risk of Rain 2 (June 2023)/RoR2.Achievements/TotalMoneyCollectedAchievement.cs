using RoR2.Stats;

namespace RoR2.Achievements;

[RegisterAchievement("TotalMoneyCollected", "Items.GoldGat", null, null)]
public class TotalMoneyCollectedAchievement : BaseStatMilestoneAchievement
{
	protected override StatDef statDef => StatDef.goldCollected;

	protected override ulong statRequirement => 30480uL;
}
