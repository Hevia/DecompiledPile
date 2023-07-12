using UnityEngine;

namespace RoR2;

public class ColorCatalog
{
	public enum ColorIndex
	{
		None,
		Tier1Item,
		Tier2Item,
		Tier3Item,
		LunarItem,
		Equipment,
		Interactable,
		Teleporter,
		Money,
		Blood,
		Unaffordable,
		Unlockable,
		LunarCoin,
		BossItem,
		Error,
		EasyDifficulty,
		NormalDifficulty,
		HardDifficulty,
		Tier1ItemDark,
		Tier2ItemDark,
		Tier3ItemDark,
		LunarItemDark,
		BossItemDark,
		WIP,
		Artifact,
		VoidItem,
		VoidItemDark,
		VoidCoin,
		Count
	}

	private static readonly Color32[] indexToColor32;

	private static readonly string[] indexToHexString;

	private static readonly Color[] multiplayerColors;

	static ColorCatalog()
	{
		//IL_002e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0033: Unknown result type (might be due to invalid IL or missing references)
		//IL_0038: Unknown result type (might be due to invalid IL or missing references)
		//IL_004d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0052: Unknown result type (might be due to invalid IL or missing references)
		//IL_0057: Unknown result type (might be due to invalid IL or missing references)
		//IL_006f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0074: Unknown result type (might be due to invalid IL or missing references)
		//IL_0079: Unknown result type (might be due to invalid IL or missing references)
		//IL_0091: Unknown result type (might be due to invalid IL or missing references)
		//IL_0096: Unknown result type (might be due to invalid IL or missing references)
		//IL_009b: Unknown result type (might be due to invalid IL or missing references)
		//IL_00bf: Unknown result type (might be due to invalid IL or missing references)
		//IL_00c4: Unknown result type (might be due to invalid IL or missing references)
		//IL_00dd: Unknown result type (might be due to invalid IL or missing references)
		//IL_00e2: Unknown result type (might be due to invalid IL or missing references)
		//IL_00fb: Unknown result type (might be due to invalid IL or missing references)
		//IL_0100: Unknown result type (might be due to invalid IL or missing references)
		//IL_0119: Unknown result type (might be due to invalid IL or missing references)
		//IL_011e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0139: Unknown result type (might be due to invalid IL or missing references)
		//IL_013e: Unknown result type (might be due to invalid IL or missing references)
		//IL_015a: Unknown result type (might be due to invalid IL or missing references)
		//IL_015f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0178: Unknown result type (might be due to invalid IL or missing references)
		//IL_017d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0199: Unknown result type (might be due to invalid IL or missing references)
		//IL_019e: Unknown result type (might be due to invalid IL or missing references)
		//IL_01b8: Unknown result type (might be due to invalid IL or missing references)
		//IL_01bd: Unknown result type (might be due to invalid IL or missing references)
		//IL_01d4: Unknown result type (might be due to invalid IL or missing references)
		//IL_01d9: Unknown result type (might be due to invalid IL or missing references)
		//IL_01f6: Unknown result type (might be due to invalid IL or missing references)
		//IL_020f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0219: Unknown result type (might be due to invalid IL or missing references)
		//IL_021e: Unknown result type (might be due to invalid IL or missing references)
		//IL_023e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0243: Unknown result type (might be due to invalid IL or missing references)
		//IL_024f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0254: Unknown result type (might be due to invalid IL or missing references)
		//IL_0259: Unknown result type (might be due to invalid IL or missing references)
		//IL_0275: Unknown result type (might be due to invalid IL or missing references)
		//IL_027a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0294: Unknown result type (might be due to invalid IL or missing references)
		//IL_0299: Unknown result type (might be due to invalid IL or missing references)
		//IL_02b3: Unknown result type (might be due to invalid IL or missing references)
		//IL_02b8: Unknown result type (might be due to invalid IL or missing references)
		//IL_02d2: Unknown result type (might be due to invalid IL or missing references)
		//IL_02d7: Unknown result type (might be due to invalid IL or missing references)
		//IL_02f7: Unknown result type (might be due to invalid IL or missing references)
		//IL_02fc: Unknown result type (might be due to invalid IL or missing references)
		//IL_0316: Unknown result type (might be due to invalid IL or missing references)
		//IL_031b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0335: Unknown result type (might be due to invalid IL or missing references)
		//IL_033a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0354: Unknown result type (might be due to invalid IL or missing references)
		//IL_0359: Unknown result type (might be due to invalid IL or missing references)
		//IL_0376: Unknown result type (might be due to invalid IL or missing references)
		//IL_037b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0394: Unknown result type (might be due to invalid IL or missing references)
		//IL_0399: Unknown result type (might be due to invalid IL or missing references)
		//IL_03b6: Unknown result type (might be due to invalid IL or missing references)
		//IL_03bb: Unknown result type (might be due to invalid IL or missing references)
		//IL_03d8: Unknown result type (might be due to invalid IL or missing references)
		//IL_03dd: Unknown result type (might be due to invalid IL or missing references)
		//IL_03fa: Unknown result type (might be due to invalid IL or missing references)
		//IL_03ff: Unknown result type (might be due to invalid IL or missing references)
		//IL_041f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0424: Unknown result type (might be due to invalid IL or missing references)
		//IL_0439: Unknown result type (might be due to invalid IL or missing references)
		indexToColor32 = (Color32[])(object)new Color32[28];
		indexToHexString = new string[28];
		multiplayerColors = (Color[])(object)new Color[4]
		{
			Color32.op_Implicit(new Color32((byte)252, (byte)62, (byte)62, byte.MaxValue)),
			Color32.op_Implicit(new Color32((byte)62, (byte)109, (byte)252, byte.MaxValue)),
			Color32.op_Implicit(new Color32((byte)129, (byte)252, (byte)62, byte.MaxValue)),
			Color32.op_Implicit(new Color32((byte)252, (byte)241, (byte)62, byte.MaxValue))
		};
		indexToColor32[1] = new Color32(byte.MaxValue, byte.MaxValue, byte.MaxValue, byte.MaxValue);
		indexToColor32[2] = new Color32((byte)119, byte.MaxValue, (byte)23, byte.MaxValue);
		indexToColor32[3] = new Color32((byte)231, (byte)84, (byte)58, byte.MaxValue);
		indexToColor32[4] = new Color32((byte)48, (byte)127, byte.MaxValue, byte.MaxValue);
		indexToColor32[5] = new Color32(byte.MaxValue, (byte)128, (byte)0, byte.MaxValue);
		indexToColor32[6] = new Color32((byte)235, (byte)232, (byte)122, byte.MaxValue);
		indexToColor32[7] = new Color32((byte)231, (byte)84, (byte)58, byte.MaxValue);
		indexToColor32[8] = new Color32((byte)239, (byte)235, (byte)26, byte.MaxValue);
		indexToColor32[9] = new Color32((byte)206, (byte)41, (byte)41, byte.MaxValue);
		indexToColor32[10] = new Color32((byte)100, (byte)100, (byte)100, byte.MaxValue);
		indexToColor32[11] = Color32.Lerp(new Color32((byte)142, (byte)56, (byte)206, byte.MaxValue), new Color32(byte.MaxValue, byte.MaxValue, byte.MaxValue, byte.MaxValue), 0.575f);
		indexToColor32[12] = new Color32((byte)173, (byte)189, (byte)250, byte.MaxValue);
		indexToColor32[13] = Color32.op_Implicit(Color.yellow);
		indexToColor32[14] = new Color32(byte.MaxValue, (byte)0, byte.MaxValue, byte.MaxValue);
		indexToColor32[15] = new Color32((byte)106, (byte)170, (byte)95, byte.MaxValue);
		indexToColor32[16] = new Color32((byte)173, (byte)117, (byte)80, byte.MaxValue);
		indexToColor32[17] = new Color32((byte)142, (byte)49, (byte)49, byte.MaxValue);
		indexToColor32[18] = new Color32((byte)193, (byte)193, (byte)193, byte.MaxValue);
		indexToColor32[19] = new Color32((byte)88, (byte)149, (byte)88, byte.MaxValue);
		indexToColor32[20] = new Color32((byte)142, (byte)49, (byte)49, byte.MaxValue);
		indexToColor32[21] = new Color32((byte)76, (byte)84, (byte)144, byte.MaxValue);
		indexToColor32[22] = new Color32((byte)189, (byte)180, (byte)60, byte.MaxValue);
		indexToColor32[23] = new Color32((byte)200, (byte)80, (byte)0, byte.MaxValue);
		indexToColor32[24] = new Color32((byte)140, (byte)114, (byte)219, byte.MaxValue);
		indexToColor32[25] = new Color32((byte)237, (byte)127, (byte)205, byte.MaxValue);
		indexToColor32[26] = new Color32((byte)163, (byte)77, (byte)132, byte.MaxValue);
		indexToColor32[27] = new Color32((byte)244, (byte)173, (byte)250, byte.MaxValue);
		for (ColorIndex colorIndex = ColorIndex.None; colorIndex < ColorIndex.Count; colorIndex++)
		{
			indexToHexString[(int)colorIndex] = Util.RGBToHex(indexToColor32[(int)colorIndex]);
		}
	}

	public static Color32 GetColor(ColorIndex colorIndex)
	{
		//IL_0013: Unknown result type (might be due to invalid IL or missing references)
		if (colorIndex < ColorIndex.None || colorIndex >= ColorIndex.Count)
		{
			colorIndex = ColorIndex.Error;
		}
		return indexToColor32[(int)colorIndex];
	}

	public static string GetColorHexString(ColorIndex colorIndex)
	{
		if (colorIndex < ColorIndex.None || colorIndex >= ColorIndex.Count)
		{
			colorIndex = ColorIndex.Error;
		}
		return indexToHexString[(int)colorIndex];
	}

	public static Color GetMultiplayerColor(int playerSlot)
	{
		//IL_0014: Unknown result type (might be due to invalid IL or missing references)
		//IL_001a: Unknown result type (might be due to invalid IL or missing references)
		if (playerSlot >= 0 && playerSlot < multiplayerColors.Length)
		{
			return multiplayerColors[playerSlot];
		}
		return Color.black;
	}
}
