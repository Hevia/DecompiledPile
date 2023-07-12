using UnityEngine;

public struct NetworkLerpedVector3
{
	private struct InterpPoint
	{
		public float time;

		public Vector3 value;
	}

	public float interpDelay;

	private InterpPoint lowInterpPoint;

	private InterpPoint highInterpPoint;

	private InterpPoint newestInterpPoint;

	private float inverseLowHighTimespan;

	public void SetValueImmediate(Vector3 value)
	{
		//IL_0016: Unknown result type (might be due to invalid IL or missing references)
		//IL_0017: Unknown result type (might be due to invalid IL or missing references)
		newestInterpPoint.time = Time.time;
		newestInterpPoint.value = value;
		highInterpPoint = newestInterpPoint;
		lowInterpPoint = newestInterpPoint;
		inverseLowHighTimespan = 0f;
	}

	public Vector3 GetCurrentValue(bool hasAuthority)
	{
		//IL_0009: Unknown result type (might be due to invalid IL or missing references)
		//IL_008f: Unknown result type (might be due to invalid IL or missing references)
		//IL_009a: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a0: Unknown result type (might be due to invalid IL or missing references)
		if (hasAuthority)
		{
			return newestInterpPoint.value;
		}
		float num = Time.time - interpDelay;
		if (num > highInterpPoint.time)
		{
			lowInterpPoint = highInterpPoint;
			highInterpPoint = newestInterpPoint;
			float num2 = highInterpPoint.time - lowInterpPoint.time;
			inverseLowHighTimespan = ((num2 == 0f) ? 0f : (1f / num2));
		}
		float num3 = (num - lowInterpPoint.time) * inverseLowHighTimespan;
		return Vector3.Lerp(lowInterpPoint.value, highInterpPoint.value, num3);
	}

	public Vector3 GetAuthoritativeValue()
	{
		//IL_0006: Unknown result type (might be due to invalid IL or missing references)
		return newestInterpPoint.value;
	}

	public void PushValue(Vector3 value)
	{
		//IL_0006: Unknown result type (might be due to invalid IL or missing references)
		//IL_000b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0029: Unknown result type (might be due to invalid IL or missing references)
		//IL_002a: Unknown result type (might be due to invalid IL or missing references)
		if (newestInterpPoint.value != value)
		{
			newestInterpPoint.time = Time.time;
			newestInterpPoint.value = value;
		}
	}
}
