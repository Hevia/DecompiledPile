using UnityEngine;

namespace RoR2;

public static class DamageColor
{
	private static Color[] colors;

	static DamageColor()
	{
		//IL_0012: Unknown result type (might be due to invalid IL or missing references)
		//IL_0017: Unknown result type (might be due to invalid IL or missing references)
		//IL_0031: Unknown result type (might be due to invalid IL or missing references)
		//IL_0036: Unknown result type (might be due to invalid IL or missing references)
		//IL_0050: Unknown result type (might be due to invalid IL or missing references)
		//IL_0055: Unknown result type (might be due to invalid IL or missing references)
		//IL_006f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0074: Unknown result type (might be due to invalid IL or missing references)
		//IL_0091: Unknown result type (might be due to invalid IL or missing references)
		//IL_0096: Unknown result type (might be due to invalid IL or missing references)
		//IL_009b: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b5: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ba: Unknown result type (might be due to invalid IL or missing references)
		//IL_00d4: Unknown result type (might be due to invalid IL or missing references)
		//IL_00d9: Unknown result type (might be due to invalid IL or missing references)
		//IL_00f3: Unknown result type (might be due to invalid IL or missing references)
		//IL_00f8: Unknown result type (might be due to invalid IL or missing references)
		//IL_0112: Unknown result type (might be due to invalid IL or missing references)
		//IL_0117: Unknown result type (might be due to invalid IL or missing references)
		//IL_0132: Unknown result type (might be due to invalid IL or missing references)
		//IL_0137: Unknown result type (might be due to invalid IL or missing references)
		//IL_0152: Unknown result type (might be due to invalid IL or missing references)
		//IL_0157: Unknown result type (might be due to invalid IL or missing references)
		//IL_0172: Unknown result type (might be due to invalid IL or missing references)
		//IL_0177: Unknown result type (might be due to invalid IL or missing references)
		colors = (Color[])(object)new Color[13];
		colors[0] = Color.white;
		colors[1] = new Color(28f / 85f, 84f / 85f, 0.1764706f);
		colors[2] = new Color(0.79607844f, 16f / 85f, 16f / 85f);
		colors[8] = new Color(0.9372549f, 8f / 85f, 8f / 85f);
		colors[9] = Color32.op_Implicit(new Color32((byte)237, (byte)127, (byte)205, byte.MaxValue));
		colors[3] = new Color(0.827451f, 0.7490196f, 16f / 51f);
		colors[4] = new Color(0.76862746f, 0.96862745f, 0.34901962f);
		colors[5] = new Color(0.9372549f, 44f / 85f, 0.20392157f);
		colors[7] = new Color(0.6392157f, 0.2f, 0.20784314f);
		colors[10] = new Color(47f / 51f, 23f / 51f, 0.827451f);
		colors[11] = new Color(1f, 47f / 51f, 32f / 51f);
		colors[12] = new Color(1f, 8f / 15f, 0.54509807f);
	}

	public static Color FindColor(DamageColorIndex colorIndex)
	{
		//IL_0009: Unknown result type (might be due to invalid IL or missing references)
		//IL_0015: Unknown result type (might be due to invalid IL or missing references)
		if ((int)colorIndex < 0 || (int)colorIndex >= 13)
		{
			return Color.white;
		}
		return colors[(uint)colorIndex];
	}
}
