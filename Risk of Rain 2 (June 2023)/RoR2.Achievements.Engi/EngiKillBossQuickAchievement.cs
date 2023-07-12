namespace RoR2.Achievements.Engi;

[RegisterAchievement("EngiKillBossQuick", "Skills.Engi.SpiderMine", "Complete30StagesCareer", typeof(EngiKillBossQuickServerAchievement))]
public class EngiKillBossQuickAchievement : BaseAchievement
{
	private class EngiKillBossQuickServerAchievement : BaseServerAchievement
	{
		private const float requirement = 5f;

		public override void OnInstall()
		{
			base.OnInstall();
			BossGroup.onBossGroupDefeatedServer += OnBossGroupDefeatedServer;
		}

		public override void OnUninstall()
		{
			BossGroup.onBossGroupDefeatedServer -= OnBossGroupDefeatedServer;
			base.OnUninstall();
		}

		private void OnBossGroupDefeatedServer(BossGroup bossGroup)
		{
			if (bossGroup.fixedTimeSinceEnabled <= 5f)
			{
				Grant();
			}
		}
	}

	protected override BodyIndex LookUpRequiredBodyIndex()
	{
		return BodyCatalog.FindBodyIndex("EngiBody");
	}

	protected override void OnBodyRequirementMet()
	{
		base.OnBodyRequirementMet();
		SetServerTracked(shouldTrack: true);
	}

	protected override void OnBodyRequirementBroken()
	{
		SetServerTracked(shouldTrack: false);
		base.OnBodyRequirementBroken();
	}
}
