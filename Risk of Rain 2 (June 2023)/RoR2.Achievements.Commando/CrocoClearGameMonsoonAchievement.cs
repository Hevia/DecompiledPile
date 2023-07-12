namespace RoR2.Achievements.Commando;

[RegisterAchievement("CrocoClearGameMonsoon", "Skins.Croco.Alt1", "BeatArena", null)]
public class CrocoClearGameMonsoonAchievement : BasePerSurvivorClearGameMonsoonAchievement
{
	protected override BodyIndex LookUpRequiredBodyIndex()
	{
		return BodyCatalog.FindBodyIndex("CrocoBody");
	}
}
