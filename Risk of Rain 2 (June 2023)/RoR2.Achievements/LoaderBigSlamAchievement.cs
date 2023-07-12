using EntityStates.Loader;
using UnityEngine;

namespace RoR2.Achievements;

[RegisterAchievement("LoaderBigSlam", "Skills.Loader.ZapFist", "DefeatSuperRoboBallBoss", null)]
public class LoaderBigSlamAchievement : BaseAchievement
{
	private static readonly float requirement = (float)(300.0 * HGUnitConversions.milesToMeters / (1.0 * HGUnitConversions.hoursToSeconds));

	protected override BodyIndex LookUpRequiredBodyIndex()
	{
		return BodyCatalog.FindBodyIndex("LoaderBody");
	}

	protected override void OnBodyRequirementMet()
	{
		base.OnBodyRequirementMet();
		BaseSwingChargedFist.onHitAuthorityGlobal += SwingChargedFistOnOnHitAuthorityGlobal;
	}

	protected override void OnBodyRequirementBroken()
	{
		BaseSwingChargedFist.onHitAuthorityGlobal -= SwingChargedFistOnOnHitAuthorityGlobal;
		base.OnBodyRequirementBroken();
	}

	private void SwingChargedFistOnOnHitAuthorityGlobal(BaseSwingChargedFist state)
	{
		if (state.outer.commonComponents.characterBody == base.localUser.cachedBody)
		{
			Debug.LogFormat("{0}/{1}", new object[2] { state.punchSpeed, requirement });
			if (state.punchSpeed >= requirement)
			{
				Grant();
			}
		}
	}
}
