using RoR2.Orbs;
using UnityEngine;

namespace RoR2.Achievements.Huntress;

[RegisterAchievement("HuntressAllGlaiveBouncesKill", "Skills.Huntress.FlurryArrow", null, typeof(HuntressAllGlaiveBouncesKillServerAchievement))]
public class HuntressAllGlaiveBouncesKillAchievement : BaseAchievement
{
	private class HuntressAllGlaiveBouncesKillServerAchievement : BaseServerAchievement
	{
		public override void OnInstall()
		{
			base.OnInstall();
			LightningOrb.onLightningOrbKilledOnAllBounces += OnLightningOrbKilledOnAllBounces;
		}

		private void OnLightningOrbKilledOnAllBounces(LightningOrb lightningOrb)
		{
			CharacterBody currentBody = base.networkUser.GetCurrentBody();
			if (Object.op_Implicit((Object)(object)currentBody) && (Object)(object)lightningOrb.attacker == (Object)(object)((Component)currentBody).gameObject && lightningOrb.lightningType == LightningOrb.LightningType.HuntressGlaive)
			{
				Grant();
			}
		}

		public override void OnUninstall()
		{
			LightningOrb.onLightningOrbKilledOnAllBounces -= OnLightningOrbKilledOnAllBounces;
			base.OnUninstall();
		}
	}

	protected override BodyIndex LookUpRequiredBodyIndex()
	{
		return BodyCatalog.FindBodyIndex("HuntressBody");
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
