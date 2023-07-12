using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using HG;
using UnityEngine;

public static class HGMath
{
	[Obsolete("Use HG.Vector3Utils.AverageFast or .AveragePrecise instead.", false)]
	public static Vector3 Average<T>(T entries) where T : ICollection<Vector3>
	{
		//IL_0017: Unknown result type (might be due to invalid IL or missing references)
		//IL_001c: Unknown result type (might be due to invalid IL or missing references)
		//IL_002e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0033: Unknown result type (might be due to invalid IL or missing references)
		//IL_0035: Unknown result type (might be due to invalid IL or missing references)
		//IL_0037: Unknown result type (might be due to invalid IL or missing references)
		//IL_0039: Unknown result type (might be due to invalid IL or missing references)
		//IL_003e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0043: Unknown result type (might be due to invalid IL or missing references)
		//IL_0058: Unknown result type (might be due to invalid IL or missing references)
		int count = entries.Count;
		float num = 1f / (float)count;
		Vector3 val = Vector3.zero;
		foreach (Vector3 item in entries)
		{
			val += num * item;
		}
		return val;
	}

	[Obsolete("Use HG.Vector3Utils.Average instead.", false)]
	public static Vector3 Average(in Vector3 a, in Vector3 b)
	{
		//IL_0002: Unknown result type (might be due to invalid IL or missing references)
		return Vector3Utils.Average(ref a, ref b);
	}

	[Obsolete("Use HG.Vector3Utils.Average instead.", false)]
	public static Vector3 Average(in Vector3 a, in Vector3 b, in Vector3 c)
	{
		//IL_0003: Unknown result type (might be due to invalid IL or missing references)
		return Vector3Utils.Average(ref a, ref b, ref c);
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public static int IntDivCeil(int a, int b)
	{
		return (a - 1) / b + 1;
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public static uint UintSafeSubtract(uint a, uint b)
	{
		if (b <= a)
		{
			return a - b;
		}
		return 0u;
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public static uint UintSafeAdd(uint a, uint b)
	{
		uint num = a + b;
		uint num2 = ((a > b) ? a : b);
		if (num >= num2)
		{
			return num;
		}
		return uint.MaxValue;
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public static byte ByteSafeSubtract(byte a, byte b)
	{
		if (b <= a)
		{
			return (byte)(a - b);
		}
		return 0;
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public static byte ByteSafeAdd(byte a, byte b)
	{
		byte b2 = (byte)(a + b);
		byte b3 = ((a > b) ? a : b);
		if (b2 >= b3)
		{
			return b2;
		}
		return byte.MaxValue;
	}

	public static Vector3 Remap(Vector3 value, Vector3 inMin, Vector3 inMax, Vector3 outMin, Vector3 outMax)
	{
		//IL_0000: Unknown result type (might be due to invalid IL or missing references)
		//IL_0006: Unknown result type (might be due to invalid IL or missing references)
		//IL_000c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0013: Unknown result type (might be due to invalid IL or missing references)
		//IL_0019: Unknown result type (might be due to invalid IL or missing references)
		//IL_0021: Unknown result type (might be due to invalid IL or missing references)
		//IL_0028: Unknown result type (might be due to invalid IL or missing references)
		//IL_0031: Unknown result type (might be due to invalid IL or missing references)
		//IL_0037: Unknown result type (might be due to invalid IL or missing references)
		//IL_003d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0044: Unknown result type (might be due to invalid IL or missing references)
		//IL_004a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0052: Unknown result type (might be due to invalid IL or missing references)
		//IL_0059: Unknown result type (might be due to invalid IL or missing references)
		//IL_0062: Unknown result type (might be due to invalid IL or missing references)
		//IL_0068: Unknown result type (might be due to invalid IL or missing references)
		//IL_006e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0075: Unknown result type (might be due to invalid IL or missing references)
		//IL_007b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0083: Unknown result type (might be due to invalid IL or missing references)
		//IL_008a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0093: Unknown result type (might be due to invalid IL or missing references)
		return new Vector3(outMin.x + (value.x - inMin.x) / (inMax.x - inMin.x) * (outMax.x - outMin.x), outMin.y + (value.y - inMin.y) / (inMax.y - inMin.y) * (outMax.y - outMin.y), outMin.z + (value.z - inMin.z) / (inMax.z - inMin.z) * (outMax.z - outMin.z));
	}

	public static Vector3 Remap(Vector3 value, float inMin, float inMax, float outMin, float outMax)
	{
		//IL_0001: Unknown result type (might be due to invalid IL or missing references)
		//IL_0014: Unknown result type (might be due to invalid IL or missing references)
		//IL_0027: Unknown result type (might be due to invalid IL or missing references)
		//IL_0039: Unknown result type (might be due to invalid IL or missing references)
		return new Vector3(outMin + (value.x - inMin) / (inMax - inMin) * (outMax - outMin), outMin + (value.y - inMin) / (inMax - inMin) * (outMax - outMin), outMin + (value.z - inMin) / (inMax - inMin) * (outMax - outMin));
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	private static float Clamp(float value, float min, float max)
	{
		if (!(value > min))
		{
			return min;
		}
		if (!(value < max))
		{
			return max;
		}
		return value;
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public static int Clamp(int value, int min, int max)
	{
		if (value <= min)
		{
			return min;
		}
		if (value >= max)
		{
			return max;
		}
		return value;
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public static uint Clamp(uint value, uint min, uint max)
	{
		if (value <= min)
		{
			return min;
		}
		if (value >= max)
		{
			return max;
		}
		return value;
	}

	public static Vector3 Clamp(Vector3 value, Vector3 min, Vector3 max)
	{
		//IL_0000: Unknown result type (might be due to invalid IL or missing references)
		//IL_0006: Unknown result type (might be due to invalid IL or missing references)
		//IL_000c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0017: Unknown result type (might be due to invalid IL or missing references)
		//IL_001d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0023: Unknown result type (might be due to invalid IL or missing references)
		//IL_002e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0034: Unknown result type (might be due to invalid IL or missing references)
		//IL_003a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0045: Unknown result type (might be due to invalid IL or missing references)
		return new Vector3(Clamp(value.x, min.x, max.x), Clamp(value.y, min.y, max.y), Clamp(value.z, min.z, max.z));
	}

	public static Vector3 Clamp(Vector3 value, float min, float max)
	{
		//IL_0000: Unknown result type (might be due to invalid IL or missing references)
		//IL_000d: Unknown result type (might be due to invalid IL or missing references)
		//IL_001a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0027: Unknown result type (might be due to invalid IL or missing references)
		return new Vector3(Clamp(value.x, min, max), Clamp(value.y, min, max), Clamp(value.z, min, max));
	}

	public static bool IsVectorNaN(Vector3 value)
	{
		//IL_0000: Unknown result type (might be due to invalid IL or missing references)
		//IL_000d: Unknown result type (might be due to invalid IL or missing references)
		//IL_001a: Unknown result type (might be due to invalid IL or missing references)
		if (!float.IsNaN(value.x) && !float.IsNaN(value.y))
		{
			return float.IsNaN(value.z);
		}
		return true;
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public static bool IsVectorValid(ref Vector3 vector3)
	{
		float f = vector3.x + vector3.y + vector3.z;
		if (!float.IsInfinity(f))
		{
			return !float.IsNaN(f);
		}
		return false;
	}

	public static bool Overshoots(Vector3 startPosition, Vector3 endPosition, Vector3 targetPosition)
	{
		//IL_0000: Unknown result type (might be due to invalid IL or missing references)
		//IL_0001: Unknown result type (might be due to invalid IL or missing references)
		//IL_0002: Unknown result type (might be due to invalid IL or missing references)
		//IL_0007: Unknown result type (might be due to invalid IL or missing references)
		//IL_0008: Unknown result type (might be due to invalid IL or missing references)
		//IL_0009: Unknown result type (might be due to invalid IL or missing references)
		//IL_000e: Unknown result type (might be due to invalid IL or missing references)
		//IL_000f: Unknown result type (might be due to invalid IL or missing references)
		Vector3 val = endPosition - startPosition;
		Vector3 val2 = targetPosition - endPosition;
		return Vector3.Dot(val, val2) <= 0f;
	}

	public static float TriangleArea(in Vector3 a, in Vector3 b, in Vector3 c)
	{
		//IL_0006: Unknown result type (might be due to invalid IL or missing references)
		//IL_000c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0011: Unknown result type (might be due to invalid IL or missing references)
		//IL_0017: Unknown result type (might be due to invalid IL or missing references)
		//IL_001d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0022: Unknown result type (might be due to invalid IL or missing references)
		//IL_0027: Unknown result type (might be due to invalid IL or missing references)
		//IL_002c: Unknown result type (might be due to invalid IL or missing references)
		Vector3 val = Vector3.Cross(b - a, c - a);
		return 0.5f * ((Vector3)(ref val)).magnitude;
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public static float CircleRadiusToArea(float radius)
	{
		return MathF.PI * (radius * radius);
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public static float CircleAreaToRadius(float area)
	{
		return Mathf.Sqrt(area * (1f / MathF.PI));
	}
}
