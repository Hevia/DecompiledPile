namespace RoR2.Achievements.Mage;

[RegisterAchievement("MageClearGameMonsoon", "Skins.Mage.Alt1", "FreeMage", null)]
public class MageClearGameMonsoonAchievement : BasePerSurvivorClearGameMonsoonAchievement
{
	protected override BodyIndex LookUpRequiredBodyIndex()
	{
		return BodyCatalog.FindBodyIndex("MageBody");
	}
}
