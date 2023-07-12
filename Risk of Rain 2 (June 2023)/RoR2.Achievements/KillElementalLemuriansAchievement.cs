namespace RoR2.Achievements;

[RegisterAchievement("KillElementalLemurians", "Items.ElementalRings", null, typeof(KillElementalLemuriansServerAchievement))]
public class KillElementalLemuriansAchievement : BaseAchievement
{
	private class KillElementalLemuriansServerAchievement : BaseServerAchievement
	{
	}

	public override void OnInstall()
	{
		base.OnInstall();
		SetServerTracked(shouldTrack: true);
	}
}
