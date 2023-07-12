namespace RoR2.Achievements.Huntress;

[RegisterAchievement("HuntressClearGameMonsoon", "Skins.Huntress.Alt1", null, null)]
public class HuntressClearGameMonsoonAchievement : BasePerSurvivorClearGameMonsoonAchievement
{
	protected override BodyIndex LookUpRequiredBodyIndex()
	{
		return BodyCatalog.FindBodyIndex("HuntressBody");
	}
}
