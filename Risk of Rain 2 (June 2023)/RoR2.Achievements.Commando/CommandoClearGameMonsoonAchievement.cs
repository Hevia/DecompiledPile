namespace RoR2.Achievements.Commando;

[RegisterAchievement("CommandoClearGameMonsoon", "Skins.Commando.Alt1", null, null)]
public class CommandoClearGameMonsoonAchievement : BasePerSurvivorClearGameMonsoonAchievement
{
	protected override BodyIndex LookUpRequiredBodyIndex()
	{
		return BodyCatalog.FindBodyIndex("CommandoBody");
	}
}
