using UnityEngine;

namespace RoR2;

public class ObjectTransformCurve : MonoBehaviour
{
	public bool useRotationCurves;

	public bool useTranslationCurves;

	public bool loop;

	public bool randomizeInitialTime;

	public AnimationCurve rotationCurveX;

	public AnimationCurve rotationCurveY;

	public AnimationCurve rotationCurveZ;

	public AnimationCurve translationCurveX;

	public AnimationCurve translationCurveY;

	public AnimationCurve translationCurveZ;

	public float timeMax = 5f;

	public float time { get; set; }

	public Vector3 basePosition { get; private set; }

	public Quaternion baseRotation { get; private set; }

	private void Awake()
	{
		//IL_0007: Unknown result type (might be due to invalid IL or missing references)
		//IL_0018: Unknown result type (might be due to invalid IL or missing references)
		basePosition = ((Component)this).transform.localPosition;
		baseRotation = ((Component)this).transform.localRotation;
		Reset();
	}

	private void OnEnable()
	{
		Reset();
	}

	public void Reset()
	{
		time = 0f;
		if (randomizeInitialTime)
		{
			time = Random.Range(0f, timeMax);
		}
		UpdateTransform(time);
	}

	private void Update()
	{
		time += Time.deltaTime;
		if (loop && time > timeMax)
		{
			time %= timeMax;
		}
		UpdateTransform(time);
	}

	private void UpdateTransform(float time)
	{
		//IL_0001: Unknown result type (might be due to invalid IL or missing references)
		//IL_0006: Unknown result type (might be due to invalid IL or missing references)
		//IL_0008: Unknown result type (might be due to invalid IL or missing references)
		//IL_000d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0075: Unknown result type (might be due to invalid IL or missing references)
		//IL_007a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0081: Unknown result type (might be due to invalid IL or missing references)
		//IL_00d9: Unknown result type (might be due to invalid IL or missing references)
		Vector3 localPosition = basePosition;
		Quaternion val = baseRotation;
		float num = ((timeMax > 0f) ? Mathf.Clamp01(time / timeMax) : 0f);
		if (useRotationCurves)
		{
			if (rotationCurveX == null || rotationCurveY == null || rotationCurveZ == null)
			{
				return;
			}
			val = Quaternion.Euler(rotationCurveX.Evaluate(num), rotationCurveY.Evaluate(num), rotationCurveZ.Evaluate(num));
			((Component)this).transform.localRotation = val;
		}
		if (useTranslationCurves && translationCurveX != null && translationCurveY != null && translationCurveZ != null)
		{
			((Vector3)(ref localPosition))._002Ector(translationCurveX.Evaluate(num), translationCurveY.Evaluate(num), translationCurveZ.Evaluate(num));
			((Component)this).transform.localPosition = localPosition;
		}
	}
}
