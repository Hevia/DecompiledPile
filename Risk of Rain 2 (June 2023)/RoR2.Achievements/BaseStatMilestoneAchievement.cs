using System;
using RoR2.Stats;

namespace RoR2.Achievements;

public abstract class BaseStatMilestoneAchievement : BaseAchievement
{
	protected abstract StatDef statDef { get; }

	protected abstract ulong statRequirement { get; }

	private ulong statProgress => base.userProfile.statSheet.GetStatValueULong(statDef);

	public override void OnInstall()
	{
		base.OnInstall();
		UserProfile obj = base.userProfile;
		obj.onStatsReceived = (Action)Delegate.Combine(obj.onStatsReceived, new Action(Check));
		Check();
	}

	public override void OnUninstall()
	{
		UserProfile obj = base.userProfile;
		obj.onStatsReceived = (Action)Delegate.Remove(obj.onStatsReceived, new Action(Check));
		base.OnUninstall();
	}

	public override float ProgressForAchievement()
	{
		return (float)statProgress / (float)statRequirement;
	}

	private void Check()
	{
		if (statProgress >= statRequirement)
		{
			Grant();
		}
	}
}
