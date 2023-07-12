using UnityEngine;

namespace RoR2;

public class ObjectScaleCurve : MonoBehaviour
{
	public bool useOverallCurveOnly;

	public AnimationCurve curveX;

	public AnimationCurve curveY;

	public AnimationCurve curveZ;

	public AnimationCurve overallCurve;

	public float timeMax = 5f;

	public float time { get; set; }

	public Vector3 baseScale { get; set; }

	private void Awake()
	{
		//IL_0007: Unknown result type (might be due to invalid IL or missing references)
		baseScale = ((Component)this).transform.localScale;
		Reset();
	}

	private void OnEnable()
	{
		Reset();
	}

	public void Reset()
	{
		time = 0f;
		UpdateScale(0f);
	}

	private void Update()
	{
		time += Time.deltaTime;
		UpdateScale(time);
	}

	private void UpdateScale(float time)
	{
		//IL_0046: Unknown result type (might be due to invalid IL or missing references)
		//IL_004c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0051: Unknown result type (might be due to invalid IL or missing references)
		//IL_007c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0094: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ac: Unknown result type (might be due to invalid IL or missing references)
		//IL_00c2: Unknown result type (might be due to invalid IL or missing references)
		//IL_00c4: Unknown result type (might be due to invalid IL or missing references)
		float num = ((timeMax > 0f) ? Mathf.Clamp01(time / timeMax) : 0f);
		float num2 = 1f;
		if (overallCurve != null)
		{
			num2 = overallCurve.Evaluate(num);
		}
		Vector3 val = default(Vector3);
		if (useOverallCurveOnly)
		{
			val = baseScale * num2;
		}
		else
		{
			if (curveX == null || curveY == null || curveZ == null)
			{
				return;
			}
			((Vector3)(ref val))._002Ector(curveX.Evaluate(num) * baseScale.x, curveY.Evaluate(num) * baseScale.y, curveZ.Evaluate(num) * baseScale.z);
		}
		((Component)this).transform.localScale = val * num2;
	}
}
