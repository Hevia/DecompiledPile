using UnityEngine;

namespace RoR2.Achievements.Treebot;

[RegisterAchievement("TreebotBigHeal", "Skills.Treebot.SpecialAlt1", "RescueTreebot", typeof(TreebotBigHealServerAchievement))]
public class TreebotBigHealAchievement : BaseAchievement
{
	private class TreebotBigHealServerAchievement : BaseServerAchievement
	{
		public override void OnInstall()
		{
			base.OnInstall();
			HealthComponent.onCharacterHealServer += OnCharacterHealServer;
		}

		public override void OnUninstall()
		{
			HealthComponent.onCharacterHealServer -= OnCharacterHealServer;
			base.OnInstall();
		}

		private void OnCharacterHealServer(HealthComponent healthComponent, float amount, ProcChainMask procChainMask)
		{
			if (amount >= requirement)
			{
				HealthComponent component = ((Component)GetCurrentBody()).GetComponent<HealthComponent>();
				if (healthComponent == component)
				{
					Grant();
				}
			}
		}
	}

	private static readonly float requirement = 1000f;

	protected override BodyIndex LookUpRequiredBodyIndex()
	{
		return BodyCatalog.FindBodyIndex("TreebotBody");
	}

	public override void OnInstall()
	{
		base.OnInstall();
	}

	public override void OnUninstall()
	{
		base.OnUninstall();
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
