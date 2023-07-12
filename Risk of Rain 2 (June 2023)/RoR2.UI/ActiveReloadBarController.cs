using UnityEngine;

namespace RoR2.UI;

public class ActiveReloadBarController : MonoBehaviour
{
	[SerializeField]
	private RectTransform timeIndicatorTransform;

	[SerializeField]
	private RectTransform windowIndicatorTransform;

	[SerializeField]
	private Animator animator;

	[SerializeField]
	private string isWindowActiveParamName;

	[SerializeField]
	private string wasWindowHitParamName;

	[SerializeField]
	private string wasFailureParamName;

	public void OnEnable()
	{
		SetTValue(0f);
	}

	public void SetWindowRange(float tStart, float tEnd)
	{
		//IL_0027: Unknown result type (might be due to invalid IL or missing references)
		//IL_0031: Unknown result type (might be due to invalid IL or missing references)
		//IL_0048: Unknown result type (might be due to invalid IL or missing references)
		//IL_0052: Unknown result type (might be due to invalid IL or missing references)
		tStart = Mathf.Max(0f, tStart);
		tEnd = Mathf.Min(1f, tEnd);
		windowIndicatorTransform.anchorMin = new Vector2(tStart, windowIndicatorTransform.anchorMin.y);
		windowIndicatorTransform.anchorMax = new Vector2(tEnd, windowIndicatorTransform.anchorMax.y);
	}

	public void SetIsWindowActive(bool isWindowActive)
	{
		if (Object.op_Implicit((Object)(object)animator))
		{
			animator.SetBool(isWindowActiveParamName, isWindowActive);
		}
	}

	public void SetWasWindowHit(bool wasWindowHit)
	{
		if (Object.op_Implicit((Object)(object)animator))
		{
			animator.SetBool(wasWindowHitParamName, wasWindowHit);
		}
	}

	public void SetWasFailure(bool wasFailure)
	{
		if (Object.op_Implicit((Object)(object)animator))
		{
			animator.SetBool(wasFailureParamName, wasFailure);
		}
	}

	public void SetTValue(float t)
	{
		//IL_000d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0017: Unknown result type (might be due to invalid IL or missing references)
		//IL_002e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0038: Unknown result type (might be due to invalid IL or missing references)
		timeIndicatorTransform.anchorMin = new Vector2(t, timeIndicatorTransform.anchorMin.y);
		timeIndicatorTransform.anchorMax = new Vector2(t, timeIndicatorTransform.anchorMax.y);
	}
}
