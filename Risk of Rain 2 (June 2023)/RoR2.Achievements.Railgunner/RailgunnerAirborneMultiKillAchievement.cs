using EntityStates.Railgunner.Weapon;
using UnityEngine;

namespace RoR2.Achievements.Railgunner;

[RegisterAchievement("RailgunnerAirborneMultiKill", "Skills.Railgunner.SpecialAlt1", null, typeof(RailgunnerAirborneMultiKillServerAchievement))]
public class RailgunnerAirborneMultiKillAchievement : BaseAchievement
{
	private class RailgunnerAirborneMultiKillServerAchievement : BaseServerAchievement
	{
		private int killCount;

		private bool hasFiredSuperSnipe;

		public override void OnInstall()
		{
			base.OnInstall();
			RoR2Application.onFixedUpdate += OnFixedUpdate;
			GlobalEventManager.onCharacterDeathGlobal += OnCharacterDeath;
			BaseFireSnipe.onFireSnipe += OnFireSnipe;
		}

		private void OnFireSnipe(BaseFireSnipe state)
		{
			hasFiredSuperSnipe = state is FireSnipeSuper;
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
			killCount = 0;
			hasFiredSuperSnipe = false;
		}

		private void OnCharacterDeath(DamageReport damageReport)
		{
			if (damageReport.attackerMaster == base.networkUser.master && damageReport.attackerMaster != null && CharacterIsInAir() && hasFiredSuperSnipe)
			{
				killCount++;
				if (requirement <= killCount)
				{
					Grant();
				}
			}
		}
	}

	private static readonly int requirement = 3;

	protected override BodyIndex LookUpRequiredBodyIndex()
	{
		return BodyCatalog.FindBodyIndex("RailgunnerBody");
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
