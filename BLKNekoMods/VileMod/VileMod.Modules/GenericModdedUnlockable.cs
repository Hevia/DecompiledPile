using System;
using R2API;
using RoR2;
using UnityEngine;

namespace VileMod.Modules;

public abstract class GenericModdedUnlockable : ModdedUnlockable
{
	public abstract string AchievementTokenPrefix { get; }

	public abstract string AchievementSpriteName { get; }

	public override string AchievementIdentifier => AchievementTokenPrefix + "UNLOCKABLE_ACHIEVEMENT_ID";

	public override string UnlockableIdentifier => AchievementTokenPrefix + "UNLOCKABLE_REWARD_ID";

	public override string AchievementNameToken => AchievementTokenPrefix + "UNLOCKABLE_ACHIEVEMENT_NAME";

	public override string AchievementDescToken => AchievementTokenPrefix + "UNLOCKABLE_ACHIEVEMENT_DESC";

	public override string UnlockableNameToken => AchievementTokenPrefix + "UNLOCKABLE_UNLOCKABLE_NAME";

	public override Sprite Sprite => Assets.mainAssetBundle.LoadAsset<Sprite>(AchievementSpriteName);

	public override Func<string> GetHowToUnlock => () => Language.GetStringFormatted("UNLOCK_VIA_ACHIEVEMENT_FORMAT", new object[2]
	{
		Language.GetString(((ModdedUnlockable)this).AchievementNameToken),
		Language.GetString(((ModdedUnlockable)this).AchievementDescToken)
	});

	public override Func<string> GetUnlocked => () => Language.GetStringFormatted("UNLOCKED_FORMAT", new object[2]
	{
		Language.GetString(((ModdedUnlockable)this).AchievementNameToken),
		Language.GetString(((ModdedUnlockable)this).AchievementDescToken)
	});
}
