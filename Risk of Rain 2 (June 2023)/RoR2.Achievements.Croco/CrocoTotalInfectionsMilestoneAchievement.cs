using RoR2.Stats;

namespace RoR2.Achievements.Croco;

[RegisterAchievement("CrocoTotalInfectionsMilestone", "Skills.Croco.ChainableLeap", "BeatArena", null)]
public class CrocoTotalInfectionsMilestoneAchievement : BaseStatMilestoneAchievement
{
	protected override StatDef statDef => StatDef.totalCrocoInfectionsInflicted;

	protected override ulong statRequirement => 1000uL;
}
