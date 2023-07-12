using UnityEngine;

namespace RoR2.UI;

[RequireComponent(typeof(RectTransform))]
public class LerpUIRect : MonoBehaviour
{
	public enum LerpState
	{
		Entering,
		Holding,
		Leaving
	}

	public Vector3 startLocalPosition;

	public Vector3 finalLocalPosition;

	public LerpState lerpState;

	public AnimationCurve enterCurve;

	public float enterDuration;

	public AnimationCurve leavingCurve;

	public float leaveDuration;

	private float stopwatch;

	private RectTransform rectTransform;

	private void Start()
	{
		rectTransform = ((Component)this).GetComponent<RectTransform>();
	}

	private void OnDisable()
	{
		lerpState = LerpState.Entering;
		stopwatch = 0f;
		UpdateLerp();
	}

	private void Update()
	{
		stopwatch += Time.deltaTime;
		UpdateLerp();
	}

	private void UpdateLerp()
	{
		//IL_0031: Unknown result type (might be due to invalid IL or missing references)
		//IL_0037: Unknown result type (might be due to invalid IL or missing references)
		//IL_003d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0042: Unknown result type (might be due to invalid IL or missing references)
		//IL_0089: Unknown result type (might be due to invalid IL or missing references)
		//IL_008f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0095: Unknown result type (might be due to invalid IL or missing references)
		//IL_009a: Unknown result type (might be due to invalid IL or missing references)
		switch (lerpState)
		{
		case LerpState.Entering:
		{
			float num = stopwatch / enterDuration;
			float num2 = enterCurve.Evaluate(num);
			rectTransform.anchoredPosition = Vector2.op_Implicit(Vector3.LerpUnclamped(startLocalPosition, finalLocalPosition, num2));
			if (num >= 1f)
			{
				lerpState = LerpState.Holding;
				stopwatch = 0f;
			}
			break;
		}
		case LerpState.Leaving:
		{
			float num = stopwatch / enterDuration;
			float num2 = leavingCurve.Evaluate(num);
			rectTransform.anchoredPosition = Vector2.op_Implicit(Vector3.LerpUnclamped(finalLocalPosition, startLocalPosition, num2));
			if (num >= 1f)
			{
				lerpState = LerpState.Holding;
				stopwatch = 0f;
			}
			break;
		}
		}
	}
}
