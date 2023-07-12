namespace RoR2.Achievements.Treebot;

[RegisterAchievement("TreebotClearGameMonsoon", "Skins.Treebot.Alt1", "RescueTreebot", null)]
public class TreebotClearGameMonsoonAchievement : BasePerSurvivorClearGameMonsoonAchievement
{
	protected override BodyIndex LookUpRequiredBodyIndex()
	{
		return BodyCatalog.FindBodyIndex("TreebotBody");
	}
}
