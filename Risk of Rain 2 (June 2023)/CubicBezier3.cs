using System;
using UnityEngine;

[Serializable]
public struct CubicBezier3
{
	public Vector3 a;

	public Vector3 b;

	public Vector3 c;

	public Vector3 d;

	public Vector3 p0 => a;

	public Vector3 p1 => d;

	public Vector3 v0 => b - a;

	public Vector3 v1 => c - d;

	public static CubicBezier3 FromVelocities(Vector3 p0, Vector3 v0, Vector3 p1, Vector3 v1)
	{
		//IL_000a: Unknown result type (might be due to invalid IL or missing references)
		//IL_000b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0012: Unknown result type (might be due to invalid IL or missing references)
		//IL_0013: Unknown result type (might be due to invalid IL or missing references)
		//IL_0014: Unknown result type (might be due to invalid IL or missing references)
		//IL_0019: Unknown result type (might be due to invalid IL or missing references)
		//IL_0020: Unknown result type (might be due to invalid IL or missing references)
		//IL_0021: Unknown result type (might be due to invalid IL or missing references)
		//IL_0022: Unknown result type (might be due to invalid IL or missing references)
		//IL_0027: Unknown result type (might be due to invalid IL or missing references)
		//IL_002e: Unknown result type (might be due to invalid IL or missing references)
		//IL_002f: Unknown result type (might be due to invalid IL or missing references)
		CubicBezier3 result = default(CubicBezier3);
		result.a = p0;
		result.b = p0 + v0;
		result.c = p1 + v1;
		result.d = p1;
		return result;
	}

	public Vector3 Evaluate(float t)
	{
		//IL_0009: Unknown result type (might be due to invalid IL or missing references)
		//IL_0016: Unknown result type (might be due to invalid IL or missing references)
		//IL_001c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0021: Unknown result type (might be due to invalid IL or missing references)
		//IL_0026: Unknown result type (might be due to invalid IL or missing references)
		//IL_002b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0038: Unknown result type (might be due to invalid IL or missing references)
		//IL_0043: Unknown result type (might be due to invalid IL or missing references)
		//IL_0048: Unknown result type (might be due to invalid IL or missing references)
		//IL_004d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0053: Unknown result type (might be due to invalid IL or missing references)
		//IL_0058: Unknown result type (might be due to invalid IL or missing references)
		//IL_005d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0062: Unknown result type (might be due to invalid IL or missing references)
		//IL_0069: Unknown result type (might be due to invalid IL or missing references)
		//IL_0074: Unknown result type (might be due to invalid IL or missing references)
		//IL_0079: Unknown result type (might be due to invalid IL or missing references)
		//IL_007e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0089: Unknown result type (might be due to invalid IL or missing references)
		//IL_008e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0093: Unknown result type (might be due to invalid IL or missing references)
		//IL_0099: Unknown result type (might be due to invalid IL or missing references)
		//IL_009e: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a3: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a8: Unknown result type (might be due to invalid IL or missing references)
		float num = t * t;
		float num2 = num * t;
		return a + 3f * t * (b - a) + 3f * num * (c - 2f * b + a) + num2 * (d - 3f * c + 3f * b - a);
	}

	public void ToVertices(Vector3[] results)
	{
		ToVertices(results, 0, results.Length);
	}

	public void ToVertices(Vector3[] results, int spanStart, int spanLength)
	{
		//IL_001b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0020: Unknown result type (might be due to invalid IL or missing references)
		float num = 1f / (float)(spanLength - 1);
		for (int i = 0; i < spanLength; i++)
		{
			results[spanStart++] = Evaluate((float)i * num);
		}
	}

	public float ApproximateLength(int samples)
	{
		//IL_0012: Unknown result type (might be due to invalid IL or missing references)
		//IL_0017: Unknown result type (might be due to invalid IL or missing references)
		//IL_0021: Unknown result type (might be due to invalid IL or missing references)
		//IL_0026: Unknown result type (might be due to invalid IL or missing references)
		//IL_0029: Unknown result type (might be due to invalid IL or missing references)
		//IL_002a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0033: Unknown result type (might be due to invalid IL or missing references)
		//IL_0035: Unknown result type (might be due to invalid IL or missing references)
		float num = 1f / (float)(samples - 1);
		float num2 = 0f;
		Vector3 val = p0;
		for (int i = 1; i < samples; i++)
		{
			Vector3 val2 = Evaluate((float)i * num);
			num2 += Vector3.Distance(val, val2);
			val = val2;
		}
		return num2;
	}
}
