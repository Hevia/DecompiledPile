using UnityEngine;

namespace RoR2.Achievements.Mage;

[RegisterAchievement("MageAirborneMultiKill", "Skills.Mage.FlyUp", "FreeMage", typeof(MageAirborneMultiKillServerAchievement))]
public class MageAirborneMultiKillAchievement : BaseAchievement
{
	private class MageAirborneMultiKillServerAchievement : BaseServerAchievement
	{
		private int killCount;

		public override void OnInstall()
		{
			base.OnInstall();
			RoR2Application.onFixedUpdate += OnFixedUpdate;
			GlobalEventManager.onCharacterDeathGlobal += OnCharacterDeath;
		}

		public override void OnUninstall()
		{
			GlobalEventManager.onCharacterDeathGlobal -= OnCharacterDeath;
			RoR2Application.onFixedUpdate -= OnFixedUpdate;
			base.OnUninstall();
		}

		private bool CharacterIsInAir()
		{
			CharacterBody currentBody = base.networkUser.GetCurrentBody();
			if (Object.op_Implicit((Object)(object)currentBody) && Object.op_Implicit((Object)(object)currentBody.characterMotor))
			{
				return !currentBody.characterMotor.isGrounded;
			}
			return false;
		}

		private void OnFixedUpdate()
		{
			if (!CharacterIsInAir())
			{
				killCount = 0;
			}
		}

		private void OnCharacterDeath(DamageReport damageReport)
		{
			if (damageReport.attackerMaster == base.networkUser.master && damageReport.attackerMaster != null && CharacterIsInAir())
			{
				killCount++;
				if (requirement <= killCount)
				{
					Grant();
				}
			}
		}
	}

	private static readonly int requirement = 15;

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
