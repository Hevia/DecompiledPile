using RoR2.Stats;

namespace RoR2.Achievements.Croco;

[RegisterAchievement("CrocoKillWeakEnemiesMilestone", "Skills.Croco.PassivePoisonLethal", "BeatArena", null)]
public class CrocoKillWeakEnemiesMilestoneAchievement : BaseStatMilestoneAchievement
{
	protected override StatDef statDef => StatDef.totalCrocoWeakEnemyKills;

	protected override ulong statRequirement => 50uL;
}
