using UnityEngine;

namespace RoR2.Achievements.Mage;

[RegisterAchievement("MageFastBoss", "Skills.Mage.IceBomb", "FreeMage", typeof(MageFastBossServerAchievement))]
public class MageFastBossAchievement : BaseAchievement
{
	private class MageFastBossServerAchievement : BaseServerAchievement
	{
		private ToggleAction listenForBossDamage;

		private ToggleAction listenForBossDefeated;

		private Run.FixedTimeStamp expirationTimeStamp;

		private void OnBossDamageFirstTaken()
		{
			expirationTimeStamp = Run.FixedTimeStamp.now + window;
			listenForBossDamage.SetActive(newActive: false);
			listenForBossDefeated.SetActive(newActive: true);
		}

		public override void OnInstall()
		{
			base.OnInstall();
			listenForBossDamage = new ToggleAction(delegate
			{
				GlobalEventManager.onServerDamageDealt += OnServerDamageDealt;
			}, delegate
			{
				GlobalEventManager.onServerDamageDealt -= OnServerDamageDealt;
			});
			listenForBossDefeated = new ToggleAction(delegate
			{
				BossGroup.onBossGroupDefeatedServer += OnBossGroupDefeatedServer;
			}, delegate
			{
				BossGroup.onBossGroupDefeatedServer -= OnBossGroupDefeatedServer;
			});
			BossGroup.onBossGroupStartServer += OnBossGroupStartServer;
			Run.onRunStartGlobal += OnRunStart;
			Reset();
		}

		public override void OnUninstall()
		{
			BossGroup.onBossGroupStartServer -= OnBossGroupStartServer;
			listenForBossDefeated.SetActive(newActive: false);
			listenForBossDamage.SetActive(newActive: false);
			base.OnUninstall();
		}

		private void OnRunStart(Run run)
		{
			Reset();
		}

		private void Reset()
		{
			expirationTimeStamp = Run.FixedTimeStamp.negativeInfinity;
			listenForBossDefeated.SetActive(newActive: false);
			listenForBossDamage.SetActive(newActive: false);
		}

		private static bool BossGroupIsTeleporterBoss(BossGroup bossGroup)
		{
			return Object.op_Implicit((Object)(object)((Component)bossGroup).GetComponent<TeleporterInteraction>());
		}

		private void OnBossGroupStartServer(BossGroup bossGroup)
		{
			if (BossGroupIsTeleporterBoss(bossGroup))
			{
				listenForBossDamage.SetActive(newActive: true);
			}
		}

		private void OnServerDamageDealt(DamageReport damageReport)
		{
			if (Object.op_Implicit((Object)(object)damageReport.victimMaster) && damageReport.victimMaster.isBoss)
			{
				OnBossDamageFirstTaken();
			}
		}

		private void OnBossGroupDefeatedServer(BossGroup bossGroup)
		{
			if (BossGroupIsTeleporterBoss(bossGroup) && !expirationTimeStamp.hasPassed)
			{
				Grant();
			}
		}
	}

	private static readonly float window = 1f;

	protected override BodyIndex LookUpRequiredBodyIndex()
	{
		return BodyCatalog.FindBodyIndex("MageBody");
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
