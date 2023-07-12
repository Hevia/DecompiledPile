using Epic.OnlineServices.Achievements;
using UnityEngine;

namespace RoR2;

public class AchievementSystemEOS : AchievementSystem
{
	private AchievementsInterface _achievementsInterface;

	public AchievementSystemEOS()
	{
		//IL_0016: Unknown result type (might be due to invalid IL or missing references)
		//IL_001b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0026: Unknown result type (might be due to invalid IL or missing references)
		//IL_0032: Expected O, but got Unknown
		_achievementsInterface = EOSPlatformManager.GetPlatformInterface().GetAchievementsInterface();
		QueryPlayerAchievementsOptions val = new QueryPlayerAchievementsOptions
		{
			LocalUserId = EOSLoginManager.loggedInProductId,
			TargetUserId = EOSLoginManager.loggedInProductId
		};
		_achievementsInterface.QueryPlayerAchievements(val, (object)null, (OnQueryPlayerAchievementsCompleteCallback)null);
	}

	public override void AddAchievement(string achievementName)
	{
		//IL_0025: Unknown result type (might be due to invalid IL or missing references)
		//IL_002b: Expected O, but got Unknown
		if (!string.IsNullOrEmpty(achievementName))
		{
			achievementName = achievementName.ToUpper();
		}
		else
		{
			Debug.LogError((object)"Invalid achievement name. Achievement is null or empty.");
		}
		if (IsValidAchievement(achievementName))
		{
			UnlockAchievementsOptions val = new UnlockAchievementsOptions();
			val.AchievementIds = new string[1] { achievementName };
			val.UserId = EOSLoginManager.loggedInProductId;
			UnlockAchievementsOptions val2 = val;
			_achievementsInterface.UnlockAchievements(val2, (object)null, (OnUnlockAchievementsCompleteCallback)null);
		}
		else
		{
			Debug.LogError((object)("Invalid achievement name: " + achievementName + ". Make sure the achievement's name is defined in the EOS Developer Portal."));
		}
	}

	private bool IsValidAchievement(string achievementName)
	{
		//IL_0000: Unknown result type (might be due to invalid IL or missing references)
		//IL_0005: Unknown result type (might be due to invalid IL or missing references)
		//IL_000d: Expected O, but got Unknown
		//IL_0016: Unknown result type (might be due to invalid IL or missing references)
		CopyAchievementDefinitionV2ByAchievementIdOptions val = new CopyAchievementDefinitionV2ByAchievementIdOptions
		{
			AchievementId = achievementName
		};
		DefinitionV2 val2 = default(DefinitionV2);
		_achievementsInterface.CopyAchievementDefinitionV2ByAchievementId(val, ref val2);
		if (val2 != null)
		{
			return !val2.IsHidden;
		}
		return false;
	}
}
