using System;
using UnityEngine;

namespace RoR2;

[Serializable]
public struct PackedUnitVector3
{
	[SerializeField]
	public ushort value;

	private static readonly float[] uvAdjustment;

	private const ushort signMask = 57344;

	private const ushort invSignMask = 8191;

	private const ushort xSignMask = 32768;

	private const ushort ySignMask = 16384;

	private const ushort zSignMask = 8192;

	private const ushort topMask = 8064;

	private const ushort bottomMask = 127;

	static PackedUnitVector3()
	{
		uvAdjustment = new float[8192];
		Vector3 val = default(Vector3);
		for (int i = 0; i < uvAdjustment.Length; i++)
		{
			int num = i >> 7;
			int num2 = i & 0x7F;
			if (num + num2 >= 127)
			{
				num = 127 - num;
				num2 = 127 - num2;
			}
			((Vector3)(ref val))._002Ector((float)num, (float)num2, (float)(126 - num - num2));
			uvAdjustment[i] = 1f / ((Vector3)(ref val)).magnitude;
		}
	}

	public PackedUnitVector3(ushort value)
	{
		this.value = value;
	}

	public PackedUnitVector3(Vector3 src)
	{
		//IL_0007: Unknown result type (might be due to invalid IL or missing references)
		//IL_0029: Unknown result type (might be due to invalid IL or missing references)
		//IL_0035: Unknown result type (might be due to invalid IL or missing references)
		//IL_0057: Unknown result type (might be due to invalid IL or missing references)
		//IL_0063: Unknown result type (might be due to invalid IL or missing references)
		//IL_0085: Unknown result type (might be due to invalid IL or missing references)
		//IL_0096: Unknown result type (might be due to invalid IL or missing references)
		//IL_009c: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a3: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ac: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b6: Unknown result type (might be due to invalid IL or missing references)
		value = 0;
		if (src.x < 0f)
		{
			value |= 32768;
			src.x = 0f - src.x;
		}
		if (src.y < 0f)
		{
			value |= 16384;
			src.y = 0f - src.y;
		}
		if (src.z < 0f)
		{
			value |= 8192;
			src.z = 0f - src.z;
		}
		float num = 126f / (src.x + src.y + src.z);
		int num2 = (int)(src.x * num);
		int num3 = (int)(src.y * num);
		if (num2 >= 64)
		{
			num2 = 127 - num2;
			num3 = 127 - num3;
		}
		value |= (ushort)(num2 << 7);
		value |= (ushort)num3;
	}

	public Vector3 Unpack()
	{
		//IL_0065: Unknown result type (might be due to invalid IL or missing references)
		//IL_0081: Unknown result type (might be due to invalid IL or missing references)
		//IL_009d: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a9: Unknown result type (might be due to invalid IL or missing references)
		int num = (value & 0x1F80) >> 7;
		int num2 = value & 0x7F;
		if (num + num2 >= 127)
		{
			num = 127 - num;
			num2 = 127 - num2;
		}
		float num3 = uvAdjustment[value & 0x1FFF];
		Vector3 val = default(Vector3);
		((Vector3)(ref val))._002Ector(num3 * (float)num, num3 * (float)num2, num3 * (float)(126 - num - num2));
		if ((value & 0x8000u) != 0)
		{
			val.x = 0f - val.x;
		}
		if ((value & 0x4000u) != 0)
		{
			val.y = 0f - val.y;
		}
		if ((value & 0x2000u) != 0)
		{
			val.z = 0f - val.z;
		}
		return val;
	}
}
