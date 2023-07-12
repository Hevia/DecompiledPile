using UnityEngine;

namespace RoR2.Achievements;

[RegisterAchievement("KillBossQuick", "Items.TreasureCache", null, typeof(KillBossQuickServerAchievement))]
public class KillBossQuickAchievement : BaseAchievement
{
	private class KillBossQuickServerAchievement : BaseServerAchievement
	{
		private const float requirement = 15f;

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
			if (bossGroup.fixedTimeSinceEnabled <= 15f && Object.op_Implicit((Object)(object)((Component)bossGroup).GetComponent<TeleporterInteraction>()))
			{
				Grant();
			}
		}
	}

	public override void OnInstall()
	{
		base.OnInstall();
		SetServerTracked(shouldTrack: true);
	}

	public override void OnUninstall()
	{
		base.OnUninstall();
	}
}
