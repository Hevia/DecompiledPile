using UnityEngine;
using UnityEngine.UI;

namespace RoR2.UI;

public class AchievementCardController : MonoBehaviour
{
	public Image iconImage;

	public LanguageTextMeshController nameLabel;

	public LanguageTextMeshController descriptionLabel;

	public LayoutElement tabLayoutElement;

	public float tabWidth;

	public GameObject unlockedImage;

	public GameObject cantBeAchievedImage;

	public TooltipProvider tooltipProvider;

	private static string GetAchievementParentIdentifier(string achievementIdentifier)
	{
		return AchievementManager.GetAchievementDef(achievementIdentifier)?.prerequisiteAchievementIdentifier;
	}

	private static int CalcAchievementTabCount(string achievementIdentifier)
	{
		int num = -1;
		while (!string.IsNullOrEmpty(achievementIdentifier))
		{
			num++;
			achievementIdentifier = GetAchievementParentIdentifier(achievementIdentifier);
		}
		return num;
	}

	public void SetAchievement(string achievementIdentifier, UserProfile userProfile)
	{
		AchievementDef achievementDef = AchievementManager.GetAchievementDef(achievementIdentifier);
		if (achievementDef == null)
		{
			return;
		}
		bool flag = userProfile.HasAchievement(achievementIdentifier);
		bool flag2 = userProfile.CanSeeAchievement(achievementIdentifier);
		if (Object.op_Implicit((Object)(object)iconImage))
		{
			iconImage.sprite = (flag ? achievementDef.GetAchievedIcon() : achievementDef.GetUnachievedIcon());
		}
		if (Object.op_Implicit((Object)(object)nameLabel))
		{
			nameLabel.token = (userProfile.CanSeeAchievement(achievementIdentifier) ? achievementDef.nameToken : "???");
		}
		if (Object.op_Implicit((Object)(object)descriptionLabel))
		{
			descriptionLabel.token = (userProfile.CanSeeAchievement(achievementIdentifier) ? achievementDef.descriptionToken : "???");
		}
		if (Object.op_Implicit((Object)(object)unlockedImage))
		{
			unlockedImage.gameObject.SetActive(flag);
		}
		if (Object.op_Implicit((Object)(object)cantBeAchievedImage))
		{
			cantBeAchievedImage.gameObject.SetActive(!flag2);
		}
		if (Object.op_Implicit((Object)(object)tooltipProvider))
		{
			string overrideBodyText = "???";
			if (flag2)
			{
				if (flag)
				{
					UnlockableDef unlockableDef = UnlockableCatalog.GetUnlockableDef(achievementDef.unlockableRewardIdentifier);
					if ((Object)(object)unlockableDef != (Object)null)
					{
						string @string = Language.GetString("ACHIEVEMENT_CARD_REWARD_FORMAT");
						string string2 = Language.GetString(unlockableDef.nameToken);
						overrideBodyText = string.Format(@string, string2);
					}
				}
				else
				{
					string string3 = Language.GetString("ACHIEVEMENT_CARD_REWARD_FORMAT");
					string arg = "???";
					overrideBodyText = string.Format(string3, arg);
				}
			}
			else
			{
				AchievementDef achievementDef2 = AchievementManager.GetAchievementDef(achievementDef.prerequisiteAchievementIdentifier);
				if (achievementDef2 != null)
				{
					string string4 = Language.GetString("ACHIEVEMENT_CARD_PREREQ_FORMAT");
					string string5 = Language.GetString(achievementDef2.nameToken);
					overrideBodyText = string.Format(string4, string5);
				}
			}
			tooltipProvider.titleToken = (flag2 ? achievementDef.nameToken : "???");
			tooltipProvider.overrideBodyText = overrideBodyText;
		}
		if (Object.op_Implicit((Object)(object)tabLayoutElement))
		{
			tabLayoutElement.preferredWidth = (float)CalcAchievementTabCount(achievementIdentifier) * tabWidth;
		}
	}
}
