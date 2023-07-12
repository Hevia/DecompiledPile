using UnityEngine;

namespace RoR2.Achievements.Mage;

[RegisterAchievement("MageMultiKill", "Skills.Mage.LightningBolt", "FreeMage", typeof(MageMultiKillServerAchievement))]
public class MageMultiKillAchievement : BaseAchievement
{
	private class MageMultiKillServerAchievement : BaseServerAchievement
	{
		public override void OnInstall()
		{
			base.OnInstall();
			RoR2Application.onFixedUpdate += OnFixedUpdate;
		}

		public override void OnUninstall()
		{
			RoR2Application.onFixedUpdate -= OnFixedUpdate;
			base.OnUninstall();
		}

		private void OnFixedUpdate()
		{
			CharacterBody currentBody = GetCurrentBody();
			if (Object.op_Implicit((Object)(object)currentBody) && requirement <= currentBody.multiKillCount)
			{
				Grant();
			}
		}
	}

	private static readonly int requirement = 20;

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
