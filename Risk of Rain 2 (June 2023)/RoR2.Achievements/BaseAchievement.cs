using RoR2.EntitlementManagement;
using RoR2.ExpansionManagement;
using UnityEngine;

namespace RoR2.Achievements;

public abstract class BaseAchievement
{
	public UserAchievementManager owner;

	public bool shouldGrant;

	public AchievementDef achievementDef;

	private bool _meetsBodyRequirement;

	protected LocalUser localUser { get; private set; }

	protected UserProfile userProfile { get; private set; }

	protected bool isUserAlive
	{
		get
		{
			if (localUser != null && Object.op_Implicit((Object)(object)localUser.cachedBody) && Object.op_Implicit((Object)(object)localUser.cachedBody.healthComponent))
			{
				return localUser.cachedBody.healthComponent.alive;
			}
			return false;
		}
	}

	protected BodyIndex requiredBodyIndex { get; private set; } = BodyIndex.None;


	protected bool meetsBodyRequirement
	{
		get
		{
			return _meetsBodyRequirement;
		}
		set
		{
			if (_meetsBodyRequirement != value)
			{
				_meetsBodyRequirement = value;
				if (_meetsBodyRequirement)
				{
					OnBodyRequirementMet();
				}
				else
				{
					OnBodyRequirementBroken();
				}
			}
		}
	}

	protected virtual bool wantsBodyCallbacks { get; }

	public virtual void OnInstall()
	{
		localUser = owner.localUser;
		userProfile = owner.userProfile;
		requiredBodyIndex = LookUpRequiredBodyIndex();
		if (requiredBodyIndex != BodyIndex.None)
		{
			localUser.onBodyChanged += HandleBodyChangedForBodyRequirement;
			Run.onRunDestroyGlobal += SetBodyRequirementBrokenOnRunEnd;
		}
	}

	public virtual float ProgressForAchievement()
	{
		return 0f;
	}

	public virtual void OnUninstall()
	{
		if (achievementDef.serverTrackerType != null)
		{
			SetServerTracked(shouldTrack: false);
		}
		if (requiredBodyIndex != BodyIndex.None)
		{
			Run.onRunDestroyGlobal -= SetBodyRequirementBrokenOnRunEnd;
			localUser.onBodyChanged -= HandleBodyChangedForBodyRequirement;
			meetsBodyRequirement = false;
		}
		owner = null;
		localUser = null;
		userProfile = null;
	}

	public void Grant()
	{
		if (shouldGrant)
		{
			return;
		}
		UnlockableDef unlockableDef = UnlockableCatalog.GetUnlockableDef(achievementDef.unlockableRewardIdentifier);
		if (Object.op_Implicit((Object)(object)unlockableDef))
		{
			ExpansionDef expansionDefForUnlockable = UnlockableCatalog.GetExpansionDefForUnlockable(unlockableDef.index);
			if (Object.op_Implicit((Object)(object)expansionDefForUnlockable) && Object.op_Implicit((Object)(object)expansionDefForUnlockable.requiredEntitlement) && !EntitlementManager.localUserEntitlementTracker.AnyUserHasEntitlement(expansionDefForUnlockable.requiredEntitlement))
			{
				return;
			}
		}
		if (!Object.op_Implicit((Object)(object)localUser.currentNetworkUser) || localUser.currentNetworkUser.isParticipating)
		{
			shouldGrant = true;
			owner.dirtyGrantsCount++;
		}
	}

	public virtual void OnGranted()
	{
		if (!string.IsNullOrEmpty(achievementDef.unlockableRewardIdentifier))
		{
			if (Object.op_Implicit((Object)(object)localUser.currentNetworkUser))
			{
				UnlockableDef unlockableDef = UnlockableCatalog.GetUnlockableDef(achievementDef.unlockableRewardIdentifier);
				localUser.currentNetworkUser.CallCmdReportUnlock(unlockableDef.index);
			}
			userProfile.AddUnlockToken(achievementDef.unlockableRewardIdentifier);
		}
	}

	public void SetServerTracked(bool shouldTrack)
	{
		owner.SetServerAchievementTracked(achievementDef.serverIndex, shouldTrack);
	}

	protected virtual BodyIndex LookUpRequiredBodyIndex()
	{
		return BodyIndex.None;
	}

	private void HandleBodyChangedForBodyRequirement()
	{
		bool num = Object.op_Implicit((Object)(object)localUser.cachedBody);
		bool flag = meetsBodyRequirement;
		if (num)
		{
			flag = localUser.cachedBody.bodyIndex == requiredBodyIndex;
		}
		meetsBodyRequirement = flag;
	}

	protected virtual void OnBodyRequirementMet()
	{
	}

	protected virtual void OnBodyRequirementBroken()
	{
	}

	private void SetBodyRequirementBrokenOnRunEnd(Run run)
	{
		meetsBodyRequirement = false;
	}
}
