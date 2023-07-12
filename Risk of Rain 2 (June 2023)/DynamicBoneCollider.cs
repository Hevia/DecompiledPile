using UnityEngine;

[AddComponentMenu("Dynamic Bone/Dynamic Bone Collider")]
public class DynamicBoneCollider : MonoBehaviour
{
	public enum Direction
	{
		X,
		Y,
		Z
	}

	public enum Bound
	{
		Outside,
		Inside
	}

	public Vector3 m_Center = Vector3.zero;

	public float m_Radius = 0.5f;

	public float m_Height;

	public Direction m_Direction;

	public Bound m_Bound;

	private void OnValidate()
	{
		m_Radius = Mathf.Max(m_Radius, 0f);
		m_Height = Mathf.Max(m_Height, 0f);
	}

	public void Collide(ref Vector3 particlePosition, float particleRadius)
	{
		//IL_000c: Unknown result type (might be due to invalid IL or missing references)
		//IL_004a: Unknown result type (might be due to invalid IL or missing references)
		//IL_004f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0064: Unknown result type (might be due to invalid IL or missing references)
		//IL_0069: Unknown result type (might be due to invalid IL or missing references)
		//IL_0076: Unknown result type (might be due to invalid IL or missing references)
		//IL_007b: Unknown result type (might be due to invalid IL or missing references)
		//IL_007d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0082: Unknown result type (might be due to invalid IL or missing references)
		//IL_00fc: Unknown result type (might be due to invalid IL or missing references)
		//IL_00fd: Unknown result type (might be due to invalid IL or missing references)
		//IL_0108: Unknown result type (might be due to invalid IL or missing references)
		//IL_0109: Unknown result type (might be due to invalid IL or missing references)
		//IL_011d: Unknown result type (might be due to invalid IL or missing references)
		//IL_011e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0129: Unknown result type (might be due to invalid IL or missing references)
		//IL_012a: Unknown result type (might be due to invalid IL or missing references)
		float num = m_Radius * Mathf.Abs(((Component)this).transform.lossyScale.x);
		float num2 = m_Height * 0.5f - m_Radius;
		if (num2 <= 0f)
		{
			if (m_Bound == Bound.Outside)
			{
				OutsideSphere(ref particlePosition, particleRadius, ((Component)this).transform.TransformPoint(m_Center), num);
			}
			else
			{
				InsideSphere(ref particlePosition, particleRadius, ((Component)this).transform.TransformPoint(m_Center), num);
			}
			return;
		}
		Vector3 center = m_Center;
		Vector3 center2 = m_Center;
		switch (m_Direction)
		{
		case Direction.X:
			center.x -= num2;
			center2.x += num2;
			break;
		case Direction.Y:
			center.y -= num2;
			center2.y += num2;
			break;
		case Direction.Z:
			center.z -= num2;
			center2.z += num2;
			break;
		}
		if (m_Bound == Bound.Outside)
		{
			OutsideCapsule(ref particlePosition, particleRadius, ((Component)this).transform.TransformPoint(center), ((Component)this).transform.TransformPoint(center2), num);
		}
		else
		{
			InsideCapsule(ref particlePosition, particleRadius, ((Component)this).transform.TransformPoint(center), ((Component)this).transform.TransformPoint(center2), num);
		}
	}

	private static void OutsideSphere(ref Vector3 particlePosition, float particleRadius, Vector3 sphereCenter, float sphereRadius)
	{
		//IL_0009: Unknown result type (might be due to invalid IL or missing references)
		//IL_000e: Unknown result type (might be due to invalid IL or missing references)
		//IL_000f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0014: Unknown result type (might be due to invalid IL or missing references)
		//IL_0032: Unknown result type (might be due to invalid IL or missing references)
		//IL_0033: Unknown result type (might be due to invalid IL or missing references)
		//IL_0038: Unknown result type (might be due to invalid IL or missing references)
		//IL_003d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0042: Unknown result type (might be due to invalid IL or missing references)
		float num = sphereRadius + particleRadius;
		float num2 = num * num;
		Vector3 val = particlePosition - sphereCenter;
		float sqrMagnitude = ((Vector3)(ref val)).sqrMagnitude;
		if (sqrMagnitude > 0f && sqrMagnitude < num2)
		{
			float num3 = Mathf.Sqrt(sqrMagnitude);
			particlePosition = sphereCenter + val * (num / num3);
		}
	}

	private static void InsideSphere(ref Vector3 particlePosition, float particleRadius, Vector3 sphereCenter, float sphereRadius)
	{
		//IL_0009: Unknown result type (might be due to invalid IL or missing references)
		//IL_000e: Unknown result type (might be due to invalid IL or missing references)
		//IL_000f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0014: Unknown result type (might be due to invalid IL or missing references)
		//IL_002a: Unknown result type (might be due to invalid IL or missing references)
		//IL_002b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0030: Unknown result type (might be due to invalid IL or missing references)
		//IL_0035: Unknown result type (might be due to invalid IL or missing references)
		//IL_003a: Unknown result type (might be due to invalid IL or missing references)
		float num = sphereRadius - particleRadius;
		float num2 = num * num;
		Vector3 val = particlePosition - sphereCenter;
		float sqrMagnitude = ((Vector3)(ref val)).sqrMagnitude;
		if (sqrMagnitude > num2)
		{
			float num3 = Mathf.Sqrt(sqrMagnitude);
			particlePosition = sphereCenter + val * (num / num3);
		}
	}

	private static void OutsideCapsule(ref Vector3 particlePosition, float particleRadius, Vector3 capsuleP0, Vector3 capsuleP1, float capsuleRadius)
	{
		//IL_0009: Unknown result type (might be due to invalid IL or missing references)
		//IL_000a: Unknown result type (might be due to invalid IL or missing references)
		//IL_000b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0010: Unknown result type (might be due to invalid IL or missing references)
		//IL_0012: Unknown result type (might be due to invalid IL or missing references)
		//IL_0017: Unknown result type (might be due to invalid IL or missing references)
		//IL_0018: Unknown result type (might be due to invalid IL or missing references)
		//IL_001d: Unknown result type (might be due to invalid IL or missing references)
		//IL_001e: Unknown result type (might be due to invalid IL or missing references)
		//IL_001f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0057: Unknown result type (might be due to invalid IL or missing references)
		//IL_0058: Unknown result type (might be due to invalid IL or missing references)
		//IL_005d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0062: Unknown result type (might be due to invalid IL or missing references)
		//IL_0067: Unknown result type (might be due to invalid IL or missing references)
		//IL_007d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0082: Unknown result type (might be due to invalid IL or missing references)
		//IL_0083: Unknown result type (might be due to invalid IL or missing references)
		//IL_0088: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ad: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ae: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b3: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b8: Unknown result type (might be due to invalid IL or missing references)
		//IL_00bd: Unknown result type (might be due to invalid IL or missing references)
		//IL_00d3: Unknown result type (might be due to invalid IL or missing references)
		//IL_00d4: Unknown result type (might be due to invalid IL or missing references)
		//IL_00d7: Unknown result type (might be due to invalid IL or missing references)
		//IL_00dc: Unknown result type (might be due to invalid IL or missing references)
		//IL_00e1: Unknown result type (might be due to invalid IL or missing references)
		//IL_0104: Unknown result type (might be due to invalid IL or missing references)
		//IL_0109: Unknown result type (might be due to invalid IL or missing references)
		//IL_0111: Unknown result type (might be due to invalid IL or missing references)
		//IL_0116: Unknown result type (might be due to invalid IL or missing references)
		//IL_011b: Unknown result type (might be due to invalid IL or missing references)
		float num = capsuleRadius + particleRadius;
		float num2 = num * num;
		Vector3 val = capsuleP1 - capsuleP0;
		Vector3 val2 = particlePosition - capsuleP0;
		float num3 = Vector3.Dot(val2, val);
		if (num3 <= 0f)
		{
			float sqrMagnitude = ((Vector3)(ref val2)).sqrMagnitude;
			if (sqrMagnitude > 0f && sqrMagnitude < num2)
			{
				float num4 = Mathf.Sqrt(sqrMagnitude);
				particlePosition = capsuleP0 + val2 * (num / num4);
			}
			return;
		}
		float sqrMagnitude2 = ((Vector3)(ref val)).sqrMagnitude;
		if (num3 >= sqrMagnitude2)
		{
			val2 = particlePosition - capsuleP1;
			float sqrMagnitude3 = ((Vector3)(ref val2)).sqrMagnitude;
			if (sqrMagnitude3 > 0f && sqrMagnitude3 < num2)
			{
				float num5 = Mathf.Sqrt(sqrMagnitude3);
				particlePosition = capsuleP1 + val2 * (num / num5);
			}
		}
		else if (sqrMagnitude2 > 0f)
		{
			num3 /= sqrMagnitude2;
			val2 -= val * num3;
			float sqrMagnitude4 = ((Vector3)(ref val2)).sqrMagnitude;
			if (sqrMagnitude4 > 0f && sqrMagnitude4 < num2)
			{
				float num6 = Mathf.Sqrt(sqrMagnitude4);
				particlePosition += val2 * ((num - num6) / num6);
			}
		}
	}

	private static void InsideCapsule(ref Vector3 particlePosition, float particleRadius, Vector3 capsuleP0, Vector3 capsuleP1, float capsuleRadius)
	{
		//IL_0009: Unknown result type (might be due to invalid IL or missing references)
		//IL_000a: Unknown result type (might be due to invalid IL or missing references)
		//IL_000b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0010: Unknown result type (might be due to invalid IL or missing references)
		//IL_0012: Unknown result type (might be due to invalid IL or missing references)
		//IL_0017: Unknown result type (might be due to invalid IL or missing references)
		//IL_0018: Unknown result type (might be due to invalid IL or missing references)
		//IL_001d: Unknown result type (might be due to invalid IL or missing references)
		//IL_001e: Unknown result type (might be due to invalid IL or missing references)
		//IL_001f: Unknown result type (might be due to invalid IL or missing references)
		//IL_004b: Unknown result type (might be due to invalid IL or missing references)
		//IL_004c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0051: Unknown result type (might be due to invalid IL or missing references)
		//IL_0056: Unknown result type (might be due to invalid IL or missing references)
		//IL_005b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0071: Unknown result type (might be due to invalid IL or missing references)
		//IL_0076: Unknown result type (might be due to invalid IL or missing references)
		//IL_0077: Unknown result type (might be due to invalid IL or missing references)
		//IL_007c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0095: Unknown result type (might be due to invalid IL or missing references)
		//IL_0096: Unknown result type (might be due to invalid IL or missing references)
		//IL_009b: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a0: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a5: Unknown result type (might be due to invalid IL or missing references)
		//IL_00bb: Unknown result type (might be due to invalid IL or missing references)
		//IL_00bc: Unknown result type (might be due to invalid IL or missing references)
		//IL_00bf: Unknown result type (might be due to invalid IL or missing references)
		//IL_00c4: Unknown result type (might be due to invalid IL or missing references)
		//IL_00c9: Unknown result type (might be due to invalid IL or missing references)
		//IL_00e3: Unknown result type (might be due to invalid IL or missing references)
		//IL_00e8: Unknown result type (might be due to invalid IL or missing references)
		//IL_00f0: Unknown result type (might be due to invalid IL or missing references)
		//IL_00f5: Unknown result type (might be due to invalid IL or missing references)
		//IL_00fa: Unknown result type (might be due to invalid IL or missing references)
		float num = capsuleRadius - particleRadius;
		float num2 = num * num;
		Vector3 val = capsuleP1 - capsuleP0;
		Vector3 val2 = particlePosition - capsuleP0;
		float num3 = Vector3.Dot(val2, val);
		if (num3 <= 0f)
		{
			float sqrMagnitude = ((Vector3)(ref val2)).sqrMagnitude;
			if (sqrMagnitude > num2)
			{
				float num4 = Mathf.Sqrt(sqrMagnitude);
				particlePosition = capsuleP0 + val2 * (num / num4);
			}
			return;
		}
		float sqrMagnitude2 = ((Vector3)(ref val)).sqrMagnitude;
		if (num3 >= sqrMagnitude2)
		{
			val2 = particlePosition - capsuleP1;
			float sqrMagnitude3 = ((Vector3)(ref val2)).sqrMagnitude;
			if (sqrMagnitude3 > num2)
			{
				float num5 = Mathf.Sqrt(sqrMagnitude3);
				particlePosition = capsuleP1 + val2 * (num / num5);
			}
		}
		else if (sqrMagnitude2 > 0f)
		{
			num3 /= sqrMagnitude2;
			val2 -= val * num3;
			float sqrMagnitude4 = ((Vector3)(ref val2)).sqrMagnitude;
			if (sqrMagnitude4 > num2)
			{
				float num6 = Mathf.Sqrt(sqrMagnitude4);
				particlePosition += val2 * ((num - num6) / num6);
			}
		}
	}

	private void OnDrawGizmosSelected()
	{
		//IL_0011: Unknown result type (might be due to invalid IL or missing references)
		//IL_001d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0033: Unknown result type (might be due to invalid IL or missing references)
		//IL_0067: Unknown result type (might be due to invalid IL or missing references)
		//IL_006c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0079: Unknown result type (might be due to invalid IL or missing references)
		//IL_007e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0080: Unknown result type (might be due to invalid IL or missing references)
		//IL_0085: Unknown result type (might be due to invalid IL or missing references)
		//IL_00f5: Unknown result type (might be due to invalid IL or missing references)
		//IL_00f6: Unknown result type (might be due to invalid IL or missing references)
		//IL_0107: Unknown result type (might be due to invalid IL or missing references)
		//IL_0108: Unknown result type (might be due to invalid IL or missing references)
		if (!((Behaviour)this).enabled)
		{
			return;
		}
		if (m_Bound == Bound.Outside)
		{
			Gizmos.color = Color.yellow;
		}
		else
		{
			Gizmos.color = Color.magenta;
		}
		float num = m_Radius * Mathf.Abs(((Component)this).transform.lossyScale.x);
		float num2 = m_Height * 0.5f - m_Radius;
		if (num2 <= 0f)
		{
			Gizmos.DrawWireSphere(((Component)this).transform.TransformPoint(m_Center), num);
			return;
		}
		Vector3 center = m_Center;
		Vector3 center2 = m_Center;
		switch (m_Direction)
		{
		case Direction.X:
			center.x -= num2;
			center2.x += num2;
			break;
		case Direction.Y:
			center.y -= num2;
			center2.y += num2;
			break;
		case Direction.Z:
			center.z -= num2;
			center2.z += num2;
			break;
		}
		Gizmos.DrawWireSphere(((Component)this).transform.TransformPoint(center), num);
		Gizmos.DrawWireSphere(((Component)this).transform.TransformPoint(center2), num);
	}
}
