using UnityEngine;

namespace RoR2.Achievements.Croco;

[RegisterAchievement("CrocoKillScavenger", "Skills.Croco.CrocoBite", "BeatArena", typeof(CrocoKillScavengerServerAchievement))]
public class CrocoKillScavengerAchievement : BaseAchievement
{
	private class CrocoKillScavengerServerAchievement : BaseServerAchievement
	{
		private BodyIndex requiredVictimBodyIndex;

		public override void OnInstall()
		{
			base.OnInstall();
			requiredVictimBodyIndex = BodyCatalog.FindBodyIndex("ScavBody");
			GlobalEventManager.onCharacterDeathGlobal += OnCharacterDeathGlobal;
		}

		public override void OnUninstall()
		{
			GlobalEventManager.onCharacterDeathGlobal -= OnCharacterDeathGlobal;
			base.OnUninstall();
		}

		private void OnCharacterDeathGlobal(DamageReport damageReport)
		{
			if (damageReport.victimBodyIndex == requiredVictimBodyIndex && (Object)(object)serverAchievementTracker.networkUser.master == (Object)(object)damageReport.attackerMaster)
			{
				Grant();
			}
		}
	}

	protected override BodyIndex LookUpRequiredBodyIndex()
	{
		return BodyCatalog.FindBodyIndex("CrocoBody");
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
