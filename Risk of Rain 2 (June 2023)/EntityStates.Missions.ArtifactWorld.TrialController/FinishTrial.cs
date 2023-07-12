using RoR2;
using UnityEngine;

namespace EntityStates.Missions.ArtifactWorld.TrialController;

public class FinishTrial : ArtifactTrialControllerBaseState
{
	[SerializeField]
	public string achievementName;

	public override void OnEnter()
	{
		base.OnEnter();
		((Component)childLocator.FindChild("FinishTrial")).gameObject.SetActive(true);
		AchievementDef achievementDef = AchievementManager.GetAchievementDef(achievementName);
		if (achievementDef == null)
		{
			return;
		}
		foreach (LocalUser readOnlyLocalUsers in LocalUserManager.readOnlyLocalUsersList)
		{
			AchievementManager.GetUserAchievementManager(readOnlyLocalUsers).GrantAchievement(achievementDef);
		}
	}
}
