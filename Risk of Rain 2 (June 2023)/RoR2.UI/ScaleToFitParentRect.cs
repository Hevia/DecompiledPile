using UnityEngine;
using UnityEngine.UI;

namespace RoR2.UI;

[RequireComponent(typeof(RectTransform))]
[ExecuteAlways]
public class ScaleToFitParentRect : MonoBehaviour, ILayoutSelfController, ILayoutController
{
	private RectTransform rectTransform;

	private RectTransform parentRectTransform;

	public bool fitToWidth = true;

	public bool fitToHeight = true;

	public bool stretchUnfittedAxis;

	public Vector2 referenceSize = Vector2.zero;

	private Vector2 parentSize = Vector2.zero;

	private Vector2 postScaleStretchSize = Vector2.zero;

	private float desiredScale = 1f;

	public bool autoReferenceSize = true;

	private bool inApply;

	private void CacheComponents()
	{
		//IL_0007: Unknown result type (might be due to invalid IL or missing references)
		//IL_0011: Expected O, but got Unknown
		rectTransform = (RectTransform)((Component)this).transform;
	}

	private void Awake()
	{
		CacheComponents();
		ref RectTransform reference = ref parentRectTransform;
		Transform parent = ((Transform)rectTransform).parent;
		reference = (RectTransform)(object)((parent is RectTransform) ? parent : null);
	}

	private void Start()
	{
	}

	private void OnTransformParentChanged()
	{
		ref RectTransform reference = ref parentRectTransform;
		Transform parent = ((Transform)rectTransform).parent;
		reference = (RectTransform)(object)((parent is RectTransform) ? parent : null);
	}

	private void RecalculateScale()
	{
		//IL_0015: Unknown result type (might be due to invalid IL or missing references)
		//IL_001a: Unknown result type (might be due to invalid IL or missing references)
		//IL_001d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0022: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a5: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b0: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b5: Unknown result type (might be due to invalid IL or missing references)
		if (Object.op_Implicit((Object)(object)parentRectTransform))
		{
			Rect rect = parentRectTransform.rect;
			parentSize = ((Rect)(ref rect)).size;
			float num = parentSize.x / referenceSize.x;
			float num2 = parentSize.y / referenceSize.y;
			if (fitToWidth && fitToHeight)
			{
				desiredScale = Mathf.Min(num, num2);
			}
			else if (fitToWidth)
			{
				desiredScale = num;
			}
			else if (fitToHeight)
			{
				desiredScale = num2;
			}
			else
			{
				desiredScale = 1f;
			}
			postScaleStretchSize = parentSize / desiredScale;
			if (float.IsNaN(postScaleStretchSize.x))
			{
				postScaleStretchSize.x = 1f;
			}
			if (float.IsNaN(postScaleStretchSize.y))
			{
				postScaleStretchSize.y = 1f;
			}
		}
	}

	private void OnRectTransformDimensionsChange()
	{
		if (!inApply)
		{
			Apply();
		}
	}

	public void SetLayoutHorizontal()
	{
		//IL_000c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0011: Unknown result type (might be due to invalid IL or missing references)
		//IL_0012: Unknown result type (might be due to invalid IL or missing references)
		//IL_0033: Unknown result type (might be due to invalid IL or missing references)
		//IL_004f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0054: Unknown result type (might be due to invalid IL or missing references)
		RecalculateScale();
		Vector3 localScale = ((Transform)rectTransform).localScale;
		if (localScale.x != desiredScale)
		{
			localScale.x = desiredScale;
			((Transform)rectTransform).localScale = localScale;
		}
		if (stretchUnfittedAxis && !fitToWidth)
		{
			Rect rect = rectTransform.rect;
			if (((Rect)(ref rect)).width != postScaleStretchSize.x)
			{
				rectTransform.SetSizeWithCurrentAnchors((Axis)0, postScaleStretchSize.x);
			}
		}
	}

	public void SetLayoutVertical()
	{
		//IL_000c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0011: Unknown result type (might be due to invalid IL or missing references)
		//IL_0012: Unknown result type (might be due to invalid IL or missing references)
		//IL_0033: Unknown result type (might be due to invalid IL or missing references)
		//IL_004f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0054: Unknown result type (might be due to invalid IL or missing references)
		RecalculateScale();
		Vector3 localScale = ((Transform)rectTransform).localScale;
		if (localScale.y != desiredScale)
		{
			localScale.y = desiredScale;
			((Transform)rectTransform).localScale = localScale;
		}
		if (stretchUnfittedAxis && !fitToHeight)
		{
			Rect rect = rectTransform.rect;
			if (((Rect)(ref rect)).height != postScaleStretchSize.y)
			{
				rectTransform.SetSizeWithCurrentAnchors((Axis)0, postScaleStretchSize.y);
			}
		}
	}

	public void Apply()
	{
		//IL_0024: Unknown result type (might be due to invalid IL or missing references)
		//IL_0029: Unknown result type (might be due to invalid IL or missing references)
		//IL_002a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0065: Unknown result type (might be due to invalid IL or missing references)
		//IL_0079: Unknown result type (might be due to invalid IL or missing references)
		//IL_007e: Unknown result type (might be due to invalid IL or missing references)
		if (!((Behaviour)this).enabled || rectTransform == null)
		{
			return;
		}
		RecalculateScale();
		inApply = true;
		Vector3 localScale = ((Transform)rectTransform).localScale;
		if (localScale.x != desiredScale && desiredScale != 0f)
		{
			localScale.x = desiredScale;
			localScale.y = desiredScale;
			((Transform)rectTransform).localScale = localScale;
		}
		if (stretchUnfittedAxis)
		{
			Rect rect = rectTransform.rect;
			if (!fitToWidth && ((Rect)(ref rect)).width != postScaleStretchSize.x)
			{
				rectTransform.SetSizeWithCurrentAnchors((Axis)0, postScaleStretchSize.x);
			}
			if (!fitToWidth && ((Rect)(ref rect)).height != postScaleStretchSize.y)
			{
				rectTransform.SetSizeWithCurrentAnchors((Axis)1, postScaleStretchSize.y);
			}
		}
		inApply = false;
	}

	private void OnValidate()
	{
		//IL_002b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0030: Unknown result type (might be due to invalid IL or missing references)
		//IL_0033: Unknown result type (might be due to invalid IL or missing references)
		//IL_0038: Unknown result type (might be due to invalid IL or missing references)
		CacheComponents();
		ref RectTransform reference = ref parentRectTransform;
		Transform parent = ((Transform)rectTransform).parent;
		reference = (RectTransform)(object)((parent is RectTransform) ? parent : null);
		if (autoReferenceSize)
		{
			Rect rect = rectTransform.rect;
			referenceSize = ((Rect)(ref rect)).size;
		}
		Apply();
	}
}
