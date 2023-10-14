namespace VileMod.Modules.Achievements;

internal class MasteryAchievement : BaseMasteryUnlockable
{
	public override string AchievementTokenPrefix => "BLKNeko_HENRY_BODY_MASTERY";

	public override string AchievementSpriteName => "texMasteryAchievement";

	public override string PrerequisiteUnlockableIdentifier => "BLKNeko_HENRY_BODY_UNLOCKABLE_REWARD_ID";

	public override string RequiredCharacterBody => "HenryBody";

	public override float RequiredDifficultyCoefficient => 3f;
}
