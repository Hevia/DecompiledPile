using UnityEngine;

namespace RoR2.UI;

public class UIJuice : MonoBehaviour
{
	[Header("Transition Settings")]
	public CanvasGroup canvasGroup;

	public RectTransform panningRect;

	public float transitionDuration;

	public float panningMagnitude;

	public bool destroyOnEndOfTransition;

	private float transitionStopwatch;

	private float transitionEndAlpha;

	private float transitionStartAlpha;

	private float originalAlpha;

	private Vector2 transitionStartPosition;

	private Vector2 transitionEndPosition;

	private Vector2 originalPosition;

	private Vector2 transitionStartSize;

	private Vector2 transitionEndSize;

	private Vector3 originalSize;

	private bool wasTransition;

	private bool hasInitialized;

	private void Awake()
	{
		InitializeFirstTimeInfo();
	}

	private void Update()
	{
		transitionStopwatch = Mathf.Min(transitionStopwatch + Time.unscaledDeltaTime, transitionDuration);
		ProcessTransition();
	}

	private void ProcessTransition()
	{
		//IL_0071: Unknown result type (might be due to invalid IL or missing references)
		//IL_0077: Expected O, but got Unknown
		//IL_00ae: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b6: Unknown result type (might be due to invalid IL or missing references)
		//IL_00bf: Unknown result type (might be due to invalid IL or missing references)
		//IL_00c5: Unknown result type (might be due to invalid IL or missing references)
		//IL_00dd: Unknown result type (might be due to invalid IL or missing references)
		//IL_00e2: Unknown result type (might be due to invalid IL or missing references)
		//IL_00e5: Unknown result type (might be due to invalid IL or missing references)
		//IL_00eb: Unknown result type (might be due to invalid IL or missing references)
		//IL_0103: Unknown result type (might be due to invalid IL or missing references)
		//IL_0108: Unknown result type (might be due to invalid IL or missing references)
		//IL_011d: Unknown result type (might be due to invalid IL or missing references)
		//IL_012a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0165: Unknown result type (might be due to invalid IL or missing references)
		//IL_0176: Unknown result type (might be due to invalid IL or missing references)
		InitializeFirstTimeInfo();
		bool flag = transitionStopwatch < transitionDuration;
		if (flag || flag != wasTransition)
		{
			if (flag)
			{
				AnimationCurve val = AnimationCurve.EaseInOut(0f, transitionStartAlpha, 1f, transitionEndAlpha);
				if (Object.op_Implicit((Object)(object)canvasGroup))
				{
					canvasGroup.alpha = val.Evaluate(transitionStopwatch / transitionDuration);
				}
				AnimationCurve val2 = new AnimationCurve();
				Keyframe val3 = default(Keyframe);
				((Keyframe)(ref val3))._002Ector(0f, 0f, 3f, 3f);
				Keyframe val4 = default(Keyframe);
				((Keyframe)(ref val4))._002Ector(1f, 1f, 0f, 0f);
				val2.AddKey(val3);
				val2.AddKey(val4);
				Vector2 anchoredPosition = Vector2.Lerp(transitionStartPosition, transitionEndPosition, val2.Evaluate(transitionStopwatch / transitionDuration));
				Vector2 sizeDelta = Vector2.Lerp(transitionStartSize, transitionEndSize, val2.Evaluate(transitionStopwatch / transitionDuration));
				if (Object.op_Implicit((Object)(object)panningRect))
				{
					panningRect.anchoredPosition = anchoredPosition;
					panningRect.sizeDelta = sizeDelta;
				}
			}
			else
			{
				if (Object.op_Implicit((Object)(object)canvasGroup))
				{
					canvasGroup.alpha = transitionEndAlpha;
				}
				if (Object.op_Implicit((Object)(object)panningRect))
				{
					panningRect.anchoredPosition = transitionEndPosition;
					panningRect.sizeDelta = transitionEndSize;
				}
				if (destroyOnEndOfTransition)
				{
					Object.Destroy((Object)(object)((Component)this).gameObject);
				}
			}
		}
		wasTransition = flag;
	}

	public void TransitionScaleUpWidth()
	{
		//IL_002a: Unknown result type (might be due to invalid IL or missing references)
		//IL_002f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0036: Unknown result type (might be due to invalid IL or missing references)
		//IL_003b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0040: Unknown result type (might be due to invalid IL or missing references)
		InitializeFirstTimeInfo();
		if (Object.op_Implicit((Object)(object)panningRect))
		{
			transitionStartSize = new Vector2(0f, transitionEndSize.y * 0.8f);
			transitionEndSize = Vector2.op_Implicit(originalSize);
		}
		BeginTransition();
	}

	public void TransitionPanFromLeft()
	{
		//IL_001e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0029: Unknown result type (might be due to invalid IL or missing references)
		//IL_002e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0035: Unknown result type (might be due to invalid IL or missing references)
		//IL_003a: Unknown result type (might be due to invalid IL or missing references)
		InitializeFirstTimeInfo();
		if (Object.op_Implicit((Object)(object)panningRect))
		{
			transitionStartPosition = new Vector2(-1f, 0f) * panningMagnitude;
			transitionEndPosition = originalPosition;
		}
		BeginTransition();
	}

	public void TransitionPanToLeft()
	{
		//IL_0015: Unknown result type (might be due to invalid IL or missing references)
		//IL_001a: Unknown result type (might be due to invalid IL or missing references)
		//IL_002a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0035: Unknown result type (might be due to invalid IL or missing references)
		//IL_003a: Unknown result type (might be due to invalid IL or missing references)
		InitializeFirstTimeInfo();
		if (Object.op_Implicit((Object)(object)panningRect))
		{
			transitionStartPosition = originalPosition;
			transitionEndPosition = new Vector2(-1f, 0f) * panningMagnitude;
		}
		BeginTransition();
	}

	public void TransitionPanFromRight()
	{
		//IL_001e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0029: Unknown result type (might be due to invalid IL or missing references)
		//IL_002e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0035: Unknown result type (might be due to invalid IL or missing references)
		//IL_003a: Unknown result type (might be due to invalid IL or missing references)
		InitializeFirstTimeInfo();
		if (Object.op_Implicit((Object)(object)panningRect))
		{
			transitionStartPosition = new Vector2(1f, 0f) * panningMagnitude;
			transitionEndPosition = originalPosition;
		}
		BeginTransition();
	}

	public void TransitionPanToRight()
	{
		//IL_0015: Unknown result type (might be due to invalid IL or missing references)
		//IL_001a: Unknown result type (might be due to invalid IL or missing references)
		//IL_002a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0035: Unknown result type (might be due to invalid IL or missing references)
		//IL_003a: Unknown result type (might be due to invalid IL or missing references)
		InitializeFirstTimeInfo();
		if (Object.op_Implicit((Object)(object)panningRect))
		{
			transitionStartPosition = originalPosition;
			transitionEndPosition = new Vector2(1f, 0f) * panningMagnitude;
		}
		BeginTransition();
	}

	public void TransitionPanFromTop()
	{
		//IL_001e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0029: Unknown result type (might be due to invalid IL or missing references)
		//IL_002e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0035: Unknown result type (might be due to invalid IL or missing references)
		//IL_003a: Unknown result type (might be due to invalid IL or missing references)
		InitializeFirstTimeInfo();
		if (Object.op_Implicit((Object)(object)panningRect))
		{
			transitionStartPosition = new Vector2(0f, 1f) * panningMagnitude;
			transitionEndPosition = originalPosition;
		}
		BeginTransition();
	}

	public void TransitionPanToTop()
	{
		//IL_0015: Unknown result type (might be due to invalid IL or missing references)
		//IL_001a: Unknown result type (might be due to invalid IL or missing references)
		//IL_002a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0035: Unknown result type (might be due to invalid IL or missing references)
		//IL_003a: Unknown result type (might be due to invalid IL or missing references)
		InitializeFirstTimeInfo();
		if (Object.op_Implicit((Object)(object)panningRect))
		{
			transitionStartPosition = originalPosition;
			transitionEndPosition = new Vector2(0f, 1f) * panningMagnitude;
		}
		BeginTransition();
	}

	public void TransitionPanFromBottom()
	{
		//IL_001e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0029: Unknown result type (might be due to invalid IL or missing references)
		//IL_002e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0035: Unknown result type (might be due to invalid IL or missing references)
		//IL_003a: Unknown result type (might be due to invalid IL or missing references)
		InitializeFirstTimeInfo();
		if (Object.op_Implicit((Object)(object)panningRect))
		{
			transitionStartPosition = new Vector2(0f, -1f) * panningMagnitude;
			transitionEndPosition = originalPosition;
		}
		BeginTransition();
	}

	public void TransitionPanToBottom()
	{
		//IL_0015: Unknown result type (might be due to invalid IL or missing references)
		//IL_001a: Unknown result type (might be due to invalid IL or missing references)
		//IL_002a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0035: Unknown result type (might be due to invalid IL or missing references)
		//IL_003a: Unknown result type (might be due to invalid IL or missing references)
		InitializeFirstTimeInfo();
		if (Object.op_Implicit((Object)(object)panningRect))
		{
			transitionStartPosition = originalPosition;
			transitionEndPosition = new Vector2(0f, -1f) * panningMagnitude;
		}
		BeginTransition();
	}

	public void TransitionAlphaFadeIn()
	{
		InitializeFirstTimeInfo();
		transitionStartAlpha = 0f;
		transitionEndAlpha = originalAlpha;
		BeginTransition();
	}

	public void TransitionAlphaFadeOut()
	{
		InitializeFirstTimeInfo();
		transitionStartAlpha = originalAlpha;
		transitionEndAlpha = 0f;
		BeginTransition();
	}

	public void DestroyOnEndOfTransition(bool set)
	{
		destroyOnEndOfTransition = set;
	}

	private void BeginTransition()
	{
		transitionStopwatch = 0f;
		ProcessTransition();
	}

	private void InitializeFirstTimeInfo()
	{
		//IL_001d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0022: Unknown result type (might be due to invalid IL or missing references)
		//IL_002e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0033: Unknown result type (might be due to invalid IL or missing references)
		//IL_0038: Unknown result type (might be due to invalid IL or missing references)
		if (!hasInitialized)
		{
			if (Object.op_Implicit((Object)(object)panningRect))
			{
				originalPosition = panningRect.anchoredPosition;
				originalSize = Vector2.op_Implicit(panningRect.sizeDelta);
			}
			if (Object.op_Implicit((Object)(object)canvasGroup))
			{
				originalAlpha = canvasGroup.alpha;
				transitionEndAlpha = originalAlpha;
				transitionStartAlpha = originalAlpha;
			}
			hasInitialized = true;
		}
	}
}
