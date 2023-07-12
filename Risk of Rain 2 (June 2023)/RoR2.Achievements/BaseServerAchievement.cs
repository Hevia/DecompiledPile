using System;
using RoR2.EntitlementManagement;
using RoR2.ExpansionManagement;
using UnityEngine;

namespace RoR2.Achievements;

public class BaseServerAchievement
{
	public ServerAchievementTracker serverAchievementTracker;

	public AchievementDef achievementDef;

	public NetworkUser networkUser => serverAchievementTracker.networkUser;

	protected CharacterBody GetCurrentBody()
	{
		return networkUser.GetCurrentBody();
	}

	protected bool IsCurrentBody(GameObject gameObject)
	{
		CharacterBody currentBody = GetCurrentBody();
		if (Object.op_Implicit((Object)(object)currentBody))
		{
			return ((Component)currentBody).gameObject == gameObject;
		}
		return false;
	}

	protected bool IsCurrentBody(CharacterBody characterBody)
	{
		CharacterBody currentBody = GetCurrentBody();
		if (Object.op_Implicit((Object)(object)currentBody))
		{
			return currentBody == characterBody;
		}
		return false;
	}

	public virtual void OnInstall()
	{
	}

	public virtual void OnUninstall()
	{
	}

	protected void Grant()
	{
		UnlockableDef unlockableDef = UnlockableCatalog.GetUnlockableDef(achievementDef.unlockableRewardIdentifier);
		if (Object.op_Implicit((Object)(object)unlockableDef))
		{
			ExpansionDef expansionDefForUnlockable = UnlockableCatalog.GetExpansionDefForUnlockable(unlockableDef.index);
			if (Object.op_Implicit((Object)(object)expansionDefForUnlockable) && Object.op_Implicit((Object)(object)expansionDefForUnlockable.requiredEntitlement) && !EntitlementManager.networkUserEntitlementTracker.UserHasEntitlement(networkUser, expansionDefForUnlockable.requiredEntitlement))
			{
				return;
			}
		}
		serverAchievementTracker.CallRpcGrantAchievement(achievementDef.serverIndex);
	}

	public static BaseServerAchievement Instantiate(ServerAchievementIndex serverAchievementIndex)
	{
		AchievementDef achievementDef = AchievementManager.GetAchievementDef(serverAchievementIndex);
		if (achievementDef == null || achievementDef.serverTrackerType == null)
		{
			return null;
		}
		BaseServerAchievement obj = (BaseServerAchievement)Activator.CreateInstance(achievementDef.serverTrackerType);
		obj.achievementDef = achievementDef;
		return obj;
	}
}
