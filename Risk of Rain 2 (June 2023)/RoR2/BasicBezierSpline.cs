using System;
using UnityEngine;

namespace RoR2;

public class BasicBezierSpline : MonoBehaviour, ISerializationCallbackReceiver
{
	private struct KeyFrame
	{
		public CubicBezier3 curve;

		public float approximateLength;
	}

	public BasicBezierSplineControlPoint[] controlPoints = Array.Empty<BasicBezierSplineControlPoint>();

	public int samplesPerSegment = 20;

	private KeyFrame[] keyFrames = Array.Empty<KeyFrame>();

	private Vector3[] samplesBuffer = Array.Empty<Vector3>();

	private float totalLength;

	private bool keyFramesDirty = true;

	private void BuildKeyFrames()
	{
		totalLength = 0f;
		Array.Resize(ref keyFrames, controlPoints.Length);
		Array.Resize(ref samplesBuffer, samplesPerSegment);
		int i = 0;
		for (int num = controlPoints.Length - 1; i < num; i++)
		{
			BasicBezierSplineControlPoint basicBezierSplineControlPoint = controlPoints[i];
			BasicBezierSplineControlPoint basicBezierSplineControlPoint2 = controlPoints[i + 1];
			if (!Object.op_Implicit((Object)(object)basicBezierSplineControlPoint) || !Object.op_Implicit((Object)(object)basicBezierSplineControlPoint2))
			{
				break;
			}
			CubicBezier3 curveSegment = GetCurveSegment(basicBezierSplineControlPoint, basicBezierSplineControlPoint2);
			float num2 = curveSegment.ApproximateLength(samplesPerSegment);
			keyFrames[i] = new KeyFrame
			{
				curve = curveSegment,
				approximateLength = num2
			};
			totalLength += num2;
		}
	}

	public Vector3 Evaluate(float normalizedPosition)
	{
		//IL_0067: Unknown result type (might be due to invalid IL or missing references)
		//IL_0098: Unknown result type (might be due to invalid IL or missing references)
		//IL_009e: Unknown result type (might be due to invalid IL or missing references)
		if (keyFramesDirty)
		{
			if (Application.isPlaying)
			{
				keyFramesDirty = false;
			}
			BuildKeyFrames();
		}
		float num = normalizedPosition * totalLength;
		float num2 = 0f;
		int i = 0;
		for (int num3 = keyFrames.Length; i < num3; i++)
		{
			ref KeyFrame reference = ref keyFrames[i];
			float num4 = num2;
			num2 += reference.approximateLength;
			if (num2 >= num)
			{
				float t = Mathf.InverseLerp(num4, num2, num);
				return EvaluateKeyFrame(t, in reference);
			}
		}
		if (keyFrames.Length != 0)
		{
			return keyFrames[keyFrames.Length - 1].curve.p1;
		}
		return Vector3.zero;
	}

	private Vector3 EvaluateKeyFrame(float t, in KeyFrame keyFrame)
	{
		//IL_0031: Unknown result type (might be due to invalid IL or missing references)
		//IL_0036: Unknown result type (might be due to invalid IL or missing references)
		//IL_0044: Unknown result type (might be due to invalid IL or missing references)
		//IL_0049: Unknown result type (might be due to invalid IL or missing references)
		//IL_004b: Unknown result type (might be due to invalid IL or missing references)
		//IL_004d: Unknown result type (might be due to invalid IL or missing references)
		//IL_005e: Unknown result type (might be due to invalid IL or missing references)
		//IL_005f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0069: Unknown result type (might be due to invalid IL or missing references)
		//IL_006f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0071: Unknown result type (might be due to invalid IL or missing references)
		//IL_0090: Unknown result type (might be due to invalid IL or missing references)
		float num = t * keyFrame.approximateLength;
		keyFrame.curve.ToVertices(samplesBuffer);
		float num2 = 0f;
		float num3 = 0f;
		Vector3 val = samplesBuffer[0];
		for (int i = 1; i < samplesBuffer.Length; i++)
		{
			Vector3 val2 = samplesBuffer[i];
			float num4 = Vector3.Distance(val2, val);
			num3 += num4;
			if (num3 >= num)
			{
				return Vector3.Lerp(val, val2, Mathf.InverseLerp(num2, num3, num));
			}
			val = val2;
			num2 = num3;
		}
		return keyFrame.curve.p1;
	}

	private void DrawKeyFrame(in KeyFrame keyFrame)
	{
		//IL_0000: Unknown result type (might be due to invalid IL or missing references)
		//IL_0005: Unknown result type (might be due to invalid IL or missing references)
		//IL_000f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0022: Unknown result type (might be due to invalid IL or missing references)
		//IL_0030: Unknown result type (might be due to invalid IL or missing references)
		//IL_003a: Unknown result type (might be due to invalid IL or missing references)
		//IL_003f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0049: Unknown result type (might be due to invalid IL or missing references)
		//IL_005c: Unknown result type (might be due to invalid IL or missing references)
		//IL_006a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0081: Unknown result type (might be due to invalid IL or missing references)
		//IL_0086: Unknown result type (might be due to invalid IL or missing references)
		//IL_008c: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a6: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ab: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b6: Unknown result type (might be due to invalid IL or missing references)
		//IL_00bb: Unknown result type (might be due to invalid IL or missing references)
		//IL_00bd: Unknown result type (might be due to invalid IL or missing references)
		//IL_00be: Unknown result type (might be due to invalid IL or missing references)
		//IL_00c0: Unknown result type (might be due to invalid IL or missing references)
		//IL_00c1: Unknown result type (might be due to invalid IL or missing references)
		Gizmos.color = Color.Lerp(Color.green, Color.black, 0.5f);
		Gizmos.DrawRay(keyFrame.curve.p0, keyFrame.curve.v0);
		Gizmos.color = Color.Lerp(Color.red, Color.black, 0.5f);
		Gizmos.DrawRay(keyFrame.curve.p1, keyFrame.curve.v1);
		for (int i = 1; i <= 20; i++)
		{
			float num = (float)i * 0.05f;
			Gizmos.color = Color.Lerp(Color.red, Color.green, num);
			Vector3 val = keyFrame.curve.Evaluate(num - 0.05f);
			Vector3 val2 = keyFrame.curve.Evaluate(num);
			Gizmos.DrawRay(val, val2 - val);
		}
	}

	public void OnDrawGizmos()
	{
		if (keyFramesDirty)
		{
			if (Application.isPlaying)
			{
				keyFramesDirty = false;
			}
			BuildKeyFrames();
		}
		for (int i = 0; i < keyFrames.Length; i++)
		{
			DrawKeyFrame(in keyFrames[i]);
		}
	}

	public static CubicBezier3 GetCurveSegment(BasicBezierSplineControlPoint startControlPoint, BasicBezierSplineControlPoint endControlPoint)
	{
		//IL_000e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0013: Unknown result type (might be due to invalid IL or missing references)
		//IL_0014: Unknown result type (might be due to invalid IL or missing references)
		//IL_001a: Unknown result type (might be due to invalid IL or missing references)
		//IL_001f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0024: Unknown result type (might be due to invalid IL or missing references)
		//IL_0026: Unknown result type (might be due to invalid IL or missing references)
		//IL_002b: Unknown result type (might be due to invalid IL or missing references)
		//IL_002d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0033: Unknown result type (might be due to invalid IL or missing references)
		//IL_0038: Unknown result type (might be due to invalid IL or missing references)
		//IL_003d: Unknown result type (might be due to invalid IL or missing references)
		//IL_003f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0040: Unknown result type (might be due to invalid IL or missing references)
		//IL_0041: Unknown result type (might be due to invalid IL or missing references)
		//IL_0042: Unknown result type (might be due to invalid IL or missing references)
		Transform transform = ((Component)startControlPoint).transform;
		Transform transform2 = ((Component)endControlPoint).transform;
		Vector3 position = transform.position;
		Vector3 v = transform.rotation * startControlPoint.forwardVelocity;
		Vector3 position2 = transform2.position;
		Vector3 v2 = transform2.rotation * endControlPoint.backwardVelocity;
		return CubicBezier3.FromVelocities(position, v, position2, v2);
	}

	public void OnBeforeSerialize()
	{
	}

	public void OnAfterDeserialize()
	{
		keyFramesDirty = true;
	}
}
