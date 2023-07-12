using System;
using UnityEngine;
using UnityEngine.UI;

namespace RoR2.UI;

public class FlashPanel : MonoBehaviour
{
	public RectTransform flashRectTransform;

	public float strength = 1f;

	public float freq = 1f;

	public float flashAlpha = 0.7f;

	public bool alwaysFlash = true;

	private bool isFlashing;

	private float theta = 1f;

	private Image image;

	private void Start()
	{
		image = ((Component)flashRectTransform).GetComponent<Image>();
	}

	private void Update()
	{
		//IL_0010: Unknown result type (might be due to invalid IL or missing references)
		//IL_002a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0105: Unknown result type (might be due to invalid IL or missing references)
		//IL_0122: Unknown result type (might be due to invalid IL or missing references)
		//IL_0127: Unknown result type (might be due to invalid IL or missing references)
		//IL_014a: Unknown result type (might be due to invalid IL or missing references)
		flashRectTransform.anchorMin = new Vector2(0f, 0f);
		flashRectTransform.anchorMax = new Vector2(1f, 1f);
		if (alwaysFlash)
		{
			isFlashing = true;
		}
		if (isFlashing)
		{
			theta += Time.deltaTime * freq;
		}
		if (theta > 1f)
		{
			if (alwaysFlash)
			{
				theta -= theta - theta % 1f;
			}
			else
			{
				theta = 1f;
			}
			isFlashing = false;
		}
		float num = 1f - (1f + Mathf.Cos(theta * MathF.PI * 0.5f + MathF.PI / 2f));
		flashRectTransform.sizeDelta = new Vector2(1f + num * 20f * strength, 1f + num * 20f * strength);
		if (Object.op_Implicit((Object)(object)image))
		{
			Color color = ((Graphic)image).color;
			color.a = (1f - num) * strength * flashAlpha;
			((Graphic)image).color = color;
		}
	}

	public void Flash()
	{
		theta = 0f;
		isFlashing = true;
	}
}
