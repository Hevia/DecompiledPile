using UnityEngine;

namespace RoR2.Achievements.Bandit2;

[RegisterAchievement("Bandit2StackSuperBleed", "Skills.Bandit2.SerratedShivs", "CompleteThreeStages", typeof(Bandit2StackSuperBleedServerAchievement))]
public class Bandit2StackSuperBleedAchievement : BaseAchievement
{
	private class Bandit2StackSuperBleedServerAchievement : BaseServerAchievement
	{
		public override void OnInstall()
		{
			base.OnInstall();
			GlobalEventManager.onCharacterDeathGlobal += OnCharacterDeathGlobal;
		}

		public override void OnUninstall()
		{
			GlobalEventManager.onCharacterDeathGlobal -= OnCharacterDeathGlobal;
			base.OnUninstall();
		}

		private void OnCharacterDeathGlobal(DamageReport damageReport)
		{
			if (Object.op_Implicit((Object)(object)damageReport.attackerBody) && damageReport.attackerBody == GetCurrentBody() && Object.op_Implicit((Object)(object)damageReport.victimBody) && damageReport.victimBody.GetBuffCount(RoR2Content.Buffs.SuperBleed) >= requirement)
			{
				Grant();
			}
		}
	}

	private static readonly int requirement = 20;

	protected override BodyIndex LookUpRequiredBodyIndex()
	{
		return BodyCatalog.FindBodyIndex("Bandit2Body");
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
