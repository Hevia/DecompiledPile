using System;
using UnityEngine;

namespace RoR2;

public class AchievementDef
{
	public AchievementIndex index;

	public ServerAchievementIndex serverIndex = new ServerAchievementIndex
	{
		intValue = -1
	};

	public string identifier;

	public string unlockableRewardIdentifier;

	public string prerequisiteAchievementIdentifier;

	public string nameToken;

	public string descriptionToken;

	public string iconPath;

	public Type type;

	public Type serverTrackerType;

	private static readonly string[] emptyStringArray = Array.Empty<string>();

	public string[] childAchievementIdentifiers = emptyStringArray;

	private Sprite achievedIcon;

	private Sprite unachievedIcon;

	public void SetAchievedIcon(Sprite icon)
	{
		achievedIcon = icon;
	}

	public Sprite GetAchievedIcon()
	{
		if (!Object.op_Implicit((Object)(object)achievedIcon))
		{
			achievedIcon = LegacyResourcesAPI.Load<Sprite>(iconPath);
			if (!Object.op_Implicit((Object)(object)achievedIcon))
			{
				achievedIcon = LegacyResourcesAPI.Load<Sprite>("Textures/AchievementIcons/texPlaceholderAchievement");
			}
		}
		return achievedIcon;
	}

	public Sprite GetUnachievedIcon()
	{
		return LegacyResourcesAPI.Load<Sprite>("Textures/MiscIcons/texUnlockIcon");
	}

	public string GetAchievementSoundString()
	{
		if (unlockableRewardIdentifier.Contains("Characters."))
		{
			return "Play_UI_achievementUnlock_enhanced";
		}
		if (unlockableRewardIdentifier.Contains("Skills.") || unlockableRewardIdentifier.Contains("Skins."))
		{
			return "Play_UI_skill_unlock";
		}
		return "Play_UI_achievementUnlock";
	}
}
