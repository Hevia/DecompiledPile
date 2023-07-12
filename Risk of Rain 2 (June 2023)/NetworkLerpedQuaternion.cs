using UnityEngine;

public struct NetworkLerpedQuaternion
{
	private struct InterpPoint
	{
		public float time;

		public Quaternion value;
	}

	private const float interpDelay = 0.1f;

	private InterpPoint lowInterpPoint;

	private InterpPoint highInterpPoint;

	private InterpPoint newestInterpPoint;

	private float inverseLowHighTimespan;

	public void SetValueImmediate(Quaternion value)
	{
		//IL_0016: Unknown result type (might be due to invalid IL or missing references)
		//IL_0017: Unknown result type (might be due to invalid IL or missing references)
		newestInterpPoint.time = Time.time;
		newestInterpPoint.value = value;
		highInterpPoint = newestInterpPoint;
		lowInterpPoint = newestInterpPoint;
		inverseLowHighTimespan = 0f;
	}

	public Quaternion GetCurrentValue(bool hasAuthority)
	{
		//IL_0009: Unknown result type (might be due to invalid IL or missing references)
		//IL_008e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0099: Unknown result type (might be due to invalid IL or missing references)
		//IL_009f: Unknown result type (might be due to invalid IL or missing references)
		if (hasAuthority)
		{
			return newestInterpPoint.value;
		}
		float num = Time.time - 0.1f;
		if (num > highInterpPoint.time)
		{
			lowInterpPoint = highInterpPoint;
			highInterpPoint = newestInterpPoint;
			float num2 = highInterpPoint.time - lowInterpPoint.time;
			inverseLowHighTimespan = ((num2 == 0f) ? 0f : (1f / num2));
		}
		float num3 = (num - lowInterpPoint.time) * inverseLowHighTimespan;
		return Quaternion.Slerp(lowInterpPoint.value, highInterpPoint.value, num3);
	}

	public Quaternion GetAuthoritativeValue()
	{
		//IL_0006: Unknown result type (might be due to invalid IL or missing references)
		return newestInterpPoint.value;
	}

	public void PushValue(Quaternion value)
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
