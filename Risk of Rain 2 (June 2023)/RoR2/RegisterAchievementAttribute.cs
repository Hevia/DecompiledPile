using System;
using JetBrains.Annotations;

namespace RoR2;

[MeansImplicitUse]
public class RegisterAchievementAttribute : Attribute
{
	public readonly string identifier;

	public readonly string unlockableRewardIdentifier;

	public readonly string prerequisiteAchievementIdentifier;

	public readonly Type serverTrackerType;

	public RegisterAchievementAttribute([NotNull] string identifier, string unlockableRewardIdentifier, string prerequisiteAchievementIdentifier, Type serverTrackerType = null)
	{
		this.identifier = identifier;
		this.unlockableRewardIdentifier = unlockableRewardIdentifier;
		this.prerequisiteAchievementIdentifier = prerequisiteAchievementIdentifier;
		this.serverTrackerType = serverTrackerType;
	}
}
