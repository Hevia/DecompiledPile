using RoR2;
using UnityEngine;

namespace VileMod.Modules;

public static class Buffs
{
	internal static BuffDef armorBuff;

	internal static BuffDef VileFuryBuff;

	internal static void RegisterBuffs()
	{
		//IL_0015: Unknown result type (might be due to invalid IL or missing references)
		//IL_0030: Unknown result type (might be due to invalid IL or missing references)
		armorBuff = AddNewBuff("HenryArmorBuff", LegacyResourcesAPI.Load<BuffDef>("BuffDefs/HiddenInvincibility").iconSprite, Color.white, canStack: false, isDebuff: false);
		VileFuryBuff = AddNewBuff("VileFuryBuff", Assets.VilePassiveIcon, Color.white, canStack: false, isDebuff: false);
	}

	internal static BuffDef AddNewBuff(string buffName, Sprite buffIcon, Color buffColor, bool canStack, bool isDebuff)
	{
		//IL_0010: Unknown result type (might be due to invalid IL or missing references)
		//IL_0011: Unknown result type (might be due to invalid IL or missing references)
		BuffDef val = ScriptableObject.CreateInstance<BuffDef>();
		((Object)val).name = buffName;
		val.buffColor = buffColor;
		val.canStack = canStack;
		val.isDebuff = isDebuff;
		val.eliteDef = null;
		val.iconSprite = buffIcon;
		Content.AddBuffDef(val);
		return val;
	}
}
