using UnityEngine;

namespace RoR2.Achievements;

[RegisterAchievement("CompleteMultiBossShrine", "Items.Lightning", null, typeof(CompleteMultiBossShrineServerAchievement))]
public class CompleteMultiBossShrineAchievement : BaseAchievement
{
	private class CompleteMultiBossShrineServerAchievement : BaseServerAchievement
	{
		private const int requirement = 2;

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
			CharacterBody currentBody = GetCurrentBody();
			if (Object.op_Implicit((Object)(object)currentBody) && Object.op_Implicit((Object)(object)currentBody.healthComponent) && currentBody.healthComponent.alive && Object.op_Implicit((Object)(object)TeleporterInteraction.instance) && CheckTeleporter(bossGroup, TeleporterInteraction.instance))
			{
				Grant();
			}
		}

		private bool CheckTeleporter(BossGroup bossGroup, TeleporterInteraction teleporterInteraction)
		{
			if ((Object)(object)teleporterInteraction.bossDirector.combatSquad == (Object)(object)bossGroup.combatSquad)
			{
				return teleporterInteraction.shrineBonusStacks >= 2;
			}
			return false;
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
