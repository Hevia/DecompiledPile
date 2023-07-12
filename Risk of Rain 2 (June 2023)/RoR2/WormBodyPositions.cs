using System.Collections.Generic;
using UnityEngine;

namespace RoR2;

public class WormBodyPositions : MonoBehaviour
{
	private struct Keyframe
	{
		public Vector3 position;

		public Quaternion rotation;

		public Vector3 fromPreviousNormal;

		public float fromPreviousLength;
	}

	public Vector3 headVelocity = Vector3.zero;

	public Transform[] segments;

	public float segmentRadius = 1f;

	private List<Keyframe> positionHistory = new List<Keyframe>();

	private void Start()
	{
		//IL_0018: Unknown result type (might be due to invalid IL or missing references)
		//IL_001d: Unknown result type (might be due to invalid IL or missing references)
		//IL_002c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0031: Unknown result type (might be due to invalid IL or missing references)
		//IL_0038: Unknown result type (might be due to invalid IL or missing references)
		//IL_003d: Unknown result type (might be due to invalid IL or missing references)
		positionHistory.Add(new Keyframe
		{
			rotation = segments[0].rotation,
			position = segments[0].position,
			fromPreviousNormal = Vector3.zero,
			fromPreviousLength = 0f
		});
	}

	private void FixedUpdate()
	{
		//IL_0008: Unknown result type (might be due to invalid IL or missing references)
		//IL_000d: Unknown result type (might be due to invalid IL or missing references)
		//IL_000e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0027: Unknown result type (might be due to invalid IL or missing references)
		//IL_002c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0031: Unknown result type (might be due to invalid IL or missing references)
		//IL_004d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0052: Unknown result type (might be due to invalid IL or missing references)
		//IL_005b: Unknown result type (might be due to invalid IL or missing references)
		//IL_005c: Unknown result type (might be due to invalid IL or missing references)
		//IL_006e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0073: Unknown result type (might be due to invalid IL or missing references)
		//IL_007d: Unknown result type (might be due to invalid IL or missing references)
		//IL_007e: Unknown result type (might be due to invalid IL or missing references)
		//IL_008b: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ad: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b2: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b9: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ba: Unknown result type (might be due to invalid IL or missing references)
		//IL_00c1: Unknown result type (might be due to invalid IL or missing references)
		//IL_00c9: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ce: Unknown result type (might be due to invalid IL or missing references)
		//IL_00f4: Unknown result type (might be due to invalid IL or missing references)
		//IL_00f5: Unknown result type (might be due to invalid IL or missing references)
		//IL_011d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0122: Unknown result type (might be due to invalid IL or missing references)
		//IL_014e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0150: Unknown result type (might be due to invalid IL or missing references)
		//IL_0154: Unknown result type (might be due to invalid IL or missing references)
		//IL_0190: Unknown result type (might be due to invalid IL or missing references)
		//IL_0192: Unknown result type (might be due to invalid IL or missing references)
		//IL_01b8: Unknown result type (might be due to invalid IL or missing references)
		//IL_01bd: Unknown result type (might be due to invalid IL or missing references)
		//IL_01c7: Unknown result type (might be due to invalid IL or missing references)
		//IL_01cc: Unknown result type (might be due to invalid IL or missing references)
		//IL_01d6: Unknown result type (might be due to invalid IL or missing references)
		//IL_01db: Unknown result type (might be due to invalid IL or missing references)
		//IL_01ed: Unknown result type (might be due to invalid IL or missing references)
		//IL_01f2: Unknown result type (might be due to invalid IL or missing references)
		//IL_01f3: Unknown result type (might be due to invalid IL or missing references)
		//IL_01f5: Unknown result type (might be due to invalid IL or missing references)
		//IL_01fa: Unknown result type (might be due to invalid IL or missing references)
		//IL_01fc: Unknown result type (might be due to invalid IL or missing references)
		//IL_01fe: Unknown result type (might be due to invalid IL or missing references)
		//IL_0213: Unknown result type (might be due to invalid IL or missing references)
		//IL_0223: Unknown result type (might be due to invalid IL or missing references)
		//IL_022a: Unknown result type (might be due to invalid IL or missing references)
		//IL_022c: Unknown result type (might be due to invalid IL or missing references)
		//IL_022e: Unknown result type (might be due to invalid IL or missing references)
		Vector3 position = segments[0].position;
		Vector3 val = position - positionHistory[positionHistory.Count - 1].position;
		float magnitude = ((Vector3)(ref val)).magnitude;
		if (magnitude != 0f)
		{
			Quaternion rotation = segments[0].rotation;
			segments[0].up = -val;
			Quaternion rotation2 = segments[0].rotation;
			segments[0].rotation = Quaternion.RotateTowards(rotation, rotation2, 360f * Time.fixedDeltaTime);
			positionHistory.Add(new Keyframe
			{
				rotation = segments[0].rotation,
				position = position,
				fromPreviousNormal = val * (1f / magnitude),
				fromPreviousLength = magnitude
			});
		}
		float num = segmentRadius * 2f;
		float num2 = num;
		Vector3 val2 = position;
		int num3 = 1;
		for (int num4 = positionHistory.Count - 1; num4 >= 1; num4--)
		{
			Vector3 position2 = positionHistory[num4 - 1].position;
			float fromPreviousLength = positionHistory[num4].fromPreviousLength;
			if (num2 < fromPreviousLength)
			{
				float num5 = num2 / fromPreviousLength;
				segments[num3].position = Vector3.Lerp(val2, position2, num5);
				num3++;
				if (num3 >= segments.Length)
				{
					positionHistory.RemoveRange(0, num4 - 1);
					break;
				}
				num2 += num;
			}
			num2 -= fromPreviousLength;
			val2 = position2;
		}
		if (segments.Length <= 1)
		{
			return;
		}
		Quaternion rotation3 = segments[0].rotation;
		Vector3 val3 = segments[0].position;
		Vector3 val4 = segments[1].position;
		for (int i = 1; i < segments.Length - 1; i++)
		{
			Vector3 position3 = segments[i + 1].position;
			Vector3 val5 = position3 - val3;
			if (val5 != Vector3.zero)
			{
				segments[i].rotation = rotation3;
				segments[i].up = val5;
			}
			val3 = val4;
			val4 = position3;
		}
	}
}
