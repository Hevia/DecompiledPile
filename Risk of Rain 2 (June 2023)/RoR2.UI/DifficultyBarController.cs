using System;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace RoR2.UI;

[ExecuteAlways]
public class DifficultyBarController : MonoBehaviour
{
	[Serializable]
	public struct SegmentDef
	{
		[Tooltip("The default English string to use for the element at design time.")]
		public string debugString;

		[Tooltip("The final language token to use for this element at runtime.")]
		public string token;

		[Tooltip("The color to use for the panel.")]
		public Color color;
	}

	private class SegmentImageAnimation
	{
		public Image segmentImage;

		public float age;

		public float duration;

		public AnimationCurve colorCurve;

		public Color color0;

		public Color color1;

		public void Update(float t)
		{
			//IL_0007: Unknown result type (might be due to invalid IL or missing references)
			//IL_000d: Unknown result type (might be due to invalid IL or missing references)
			//IL_001e: Unknown result type (might be due to invalid IL or missing references)
			((Graphic)segmentImage).color = Color.Lerp(color0, color1, colorCurve.Evaluate(t));
		}
	}

	[Header("Component References")]
	public RectTransform viewPort;

	public RectTransform segmentContainer;

	[Tooltip("How wide each segment should be.")]
	[Header("Layout")]
	public float elementWidth;

	public float levelsPerSegment;

	public float debugTime;

	[Header("Segment Parameters")]
	public SegmentDef[] segmentDefs;

	[Tooltip("The prefab to instantiate for each segment.")]
	public GameObject segmentPrefab;

	[Header("Colors")]
	public float pastSaturationMultiplier;

	public float pastValueMultiplier;

	public Color pastLabelColor;

	public float currentSaturationMultiplier;

	public float currentValueMultiplier;

	public Color currentLabelColor;

	public float upcomingSaturationMultiplier;

	public float upcomingValueMultiplier;

	public Color upcomingLabelColor;

	[Header("Animations")]
	public AnimationCurve fadeAnimationCurve;

	public float fadeAnimationDuration = 1f;

	public AnimationCurve flashAnimationCurve;

	public float flashAnimationDuration = 0.5f;

	private int currentSegmentIndex = -1;

	private static readonly Color labelFadedColor = Color.Lerp(Color.gray, Color.white, 0.5f);

	[Header("Final Segment")]
	public Sprite finalSegmentSprite;

	private float scrollX;

	private float scrollXRaw;

	[Tooltip("Do not set this manually. Regenerate the children instead.")]
	public Image[] images;

	[Tooltip("Do not set this manually. Regenerate the children instead.")]
	public TextMeshProUGUI[] labels;

	public RawImage[] wormGearImages;

	public float UVScaleToScrollX;

	public float gearUVOffset;

	private readonly List<SegmentImageAnimation> playingAnimations = new List<SegmentImageAnimation>();

	private static Color ColorMultiplySaturationAndValue(ref Color col, float saturationMultiplier, float valueMultiplier)
	{
		//IL_0001: Unknown result type (might be due to invalid IL or missing references)
		//IL_0018: Unknown result type (might be due to invalid IL or missing references)
		float num = default(float);
		float num2 = default(float);
		float num3 = default(float);
		Color.RGBToHSV(col, ref num, ref num2, ref num3);
		return Color.HSVToRGB(num, num2 * saturationMultiplier, num3 * valueMultiplier);
	}

	private void OnCurrentSegmentIndexChanged(int newSegmentIndex)
	{
		//IL_0012: Unknown result type (might be due to invalid IL or missing references)
		//IL_0017: Unknown result type (might be due to invalid IL or missing references)
		//IL_0045: Unknown result type (might be due to invalid IL or missing references)
		//IL_005f: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a9: Unknown result type (might be due to invalid IL or missing references)
		//IL_00f9: Unknown result type (might be due to invalid IL or missing references)
		//IL_010d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0150: Unknown result type (might be due to invalid IL or missing references)
		//IL_0164: Unknown result type (might be due to invalid IL or missing references)
		//IL_01fb: Unknown result type (might be due to invalid IL or missing references)
		//IL_0200: Unknown result type (might be due to invalid IL or missing references)
		//IL_0223: Unknown result type (might be due to invalid IL or missing references)
		//IL_0228: Unknown result type (might be due to invalid IL or missing references)
		//IL_028d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0292: Unknown result type (might be due to invalid IL or missing references)
		//IL_0298: Unknown result type (might be due to invalid IL or missing references)
		//IL_029d: Unknown result type (might be due to invalid IL or missing references)
		//IL_02b3: Unknown result type (might be due to invalid IL or missing references)
		if (Application.isPlaying)
		{
			int num = newSegmentIndex - 1;
			Rect rect = viewPort.rect;
			float width = ((Rect)(ref rect)).width;
			int i = 0;
			for (int num2 = images.Length - 1; i < num2; i++)
			{
				Image obj = images[i];
				RectTransform rectTransform = ((Graphic)obj).rectTransform;
				bool enabled = rectTransform.offsetMax.x + scrollX >= 0f && rectTransform.offsetMin.x + scrollX <= width;
				((Behaviour)obj).enabled = enabled;
			}
			int num3 = images.Length - 1;
			Image obj2 = images[num3];
			bool enabled2 = ((Graphic)obj2).rectTransform.offsetMax.x + scrollX >= 0f;
			((Behaviour)obj2).enabled = enabled2;
			for (int j = 0; j <= num; j++)
			{
				((Graphic)images[j]).color = ColorMultiplySaturationAndValue(ref segmentDefs[j].color, pastSaturationMultiplier, pastValueMultiplier);
				((Graphic)labels[j]).color = pastLabelColor;
			}
			for (int k = newSegmentIndex + 1; k < images.Length; k++)
			{
				((Graphic)images[k]).color = ColorMultiplySaturationAndValue(ref segmentDefs[k].color, upcomingSaturationMultiplier, upcomingValueMultiplier);
				((Graphic)labels[k]).color = upcomingLabelColor;
			}
			Image val = ((num != -1) ? images[num] : null);
			Image val2 = ((newSegmentIndex != -1) ? images[newSegmentIndex] : null);
			TextMeshProUGUI val3 = ((newSegmentIndex != -1) ? labels[newSegmentIndex] : null);
			if (Object.op_Implicit((Object)(object)val))
			{
				playingAnimations.Add(new SegmentImageAnimation
				{
					age = 0f,
					duration = fadeAnimationDuration,
					segmentImage = val,
					colorCurve = fadeAnimationCurve,
					color0 = segmentDefs[num].color,
					color1 = ColorMultiplySaturationAndValue(ref segmentDefs[num].color, pastSaturationMultiplier, pastValueMultiplier)
				});
			}
			if (Object.op_Implicit((Object)(object)val2))
			{
				playingAnimations.Add(new SegmentImageAnimation
				{
					age = 0f,
					duration = flashAnimationDuration,
					segmentImage = val2,
					colorCurve = flashAnimationCurve,
					color0 = ColorMultiplySaturationAndValue(ref segmentDefs[newSegmentIndex].color, currentSaturationMultiplier, currentValueMultiplier),
					color1 = Color.white
				});
			}
			if (Object.op_Implicit((Object)(object)val3))
			{
				((Graphic)val3).color = currentLabelColor;
			}
		}
	}

	private void SetSegmentScroll(float segmentScroll)
	{
		//IL_0087: Unknown result type (might be due to invalid IL or missing references)
		//IL_008c: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a0: Unknown result type (might be due to invalid IL or missing references)
		//IL_00e1: Unknown result type (might be due to invalid IL or missing references)
		//IL_00e7: Expected O, but got Unknown
		//IL_00f2: Unknown result type (might be due to invalid IL or missing references)
		//IL_00f9: Expected O, but got Unknown
		//IL_0115: Unknown result type (might be due to invalid IL or missing references)
		//IL_011a: Unknown result type (might be due to invalid IL or missing references)
		//IL_012b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0133: Unknown result type (might be due to invalid IL or missing references)
		//IL_0138: Unknown result type (might be due to invalid IL or missing references)
		//IL_013c: Unknown result type (might be due to invalid IL or missing references)
		//IL_014c: Unknown result type (might be due to invalid IL or missing references)
		//IL_015f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0175: Unknown result type (might be due to invalid IL or missing references)
		//IL_018b: Unknown result type (might be due to invalid IL or missing references)
		//IL_01a2: Unknown result type (might be due to invalid IL or missing references)
		//IL_01af: Unknown result type (might be due to invalid IL or missing references)
		//IL_01bf: Unknown result type (might be due to invalid IL or missing references)
		//IL_01c4: Unknown result type (might be due to invalid IL or missing references)
		float num = segmentDefs.Length + 2;
		if (segmentScroll > num)
		{
			segmentScroll = num - 1f + (segmentScroll - Mathf.Floor(segmentScroll));
		}
		scrollXRaw = (segmentScroll - 1f) * (0f - elementWidth);
		scrollX = Mathf.Floor(scrollXRaw);
		int num2 = currentSegmentIndex;
		currentSegmentIndex = Mathf.Clamp(Mathf.FloorToInt(segmentScroll), 0, ((Transform)segmentContainer).childCount - 1);
		if (num2 != currentSegmentIndex)
		{
			OnCurrentSegmentIndexChanged(currentSegmentIndex);
		}
		Vector2 offsetMin = segmentContainer.offsetMin;
		offsetMin.x = scrollX;
		segmentContainer.offsetMin = offsetMin;
		if (Object.op_Implicit((Object)(object)segmentContainer) && ((Transform)segmentContainer).childCount > 0)
		{
			int num3 = ((Transform)segmentContainer).childCount - 1;
			RectTransform val = (RectTransform)((Transform)segmentContainer).GetChild(num3);
			RectTransform val2 = (RectTransform)((Transform)val).Find("Label");
			_ = labels[num3];
			if (segmentScroll >= (float)(num3 - 1))
			{
				float num4 = elementWidth;
				Vector2 offsetMin2 = val.offsetMin;
				offsetMin2.x = CalcSegmentStartX(num3);
				val.offsetMin = offsetMin2;
				Vector2 offsetMax = val.offsetMax;
				offsetMax.x = offsetMin2.x + num4;
				val.offsetMax = offsetMax;
				val2.anchorMin = new Vector2(0f, 0f);
				val2.anchorMax = new Vector2(0f, 1f);
				val2.offsetMin = new Vector2(0f, 0f);
				val2.offsetMax = new Vector2(elementWidth, 0f);
			}
			else
			{
				val.offsetMax = val.offsetMin + new Vector2(elementWidth, 0f);
				SetLabelDefaultDimensions(val2);
			}
		}
	}

	private float CalcSegmentStartX(int i)
	{
		return (float)i * elementWidth;
	}

	private float CalcSegmentEndX(int i)
	{
		return (float)(i + 1) * elementWidth;
	}

	private void SetLabelDefaultDimensions(RectTransform labelRectTransform)
	{
		//IL_000b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0020: Unknown result type (might be due to invalid IL or missing references)
		//IL_0035: Unknown result type (might be due to invalid IL or missing references)
		//IL_004a: Unknown result type (might be due to invalid IL or missing references)
		//IL_005f: Unknown result type (might be due to invalid IL or missing references)
		labelRectTransform.anchorMin = new Vector2(0f, 0f);
		labelRectTransform.anchorMax = new Vector2(1f, 1f);
		labelRectTransform.pivot = new Vector2(0.5f, 0.5f);
		labelRectTransform.offsetMin = new Vector2(0f, 0f);
		labelRectTransform.offsetMax = new Vector2(0f, 0f);
	}

	private void SetSegmentCount(uint desiredCount)
	{
		if (Object.op_Implicit((Object)(object)segmentContainer) && Object.op_Implicit((Object)(object)segmentPrefab))
		{
			uint num = (uint)((Transform)segmentContainer).childCount;
			if (images == null || images.Length != desiredCount)
			{
				images = (Image[])(object)new Image[desiredCount];
				labels = (TextMeshProUGUI[])(object)new TextMeshProUGUI[desiredCount];
			}
			int i = 0;
			for (int num2 = Mathf.Min(images.Length, ((Transform)segmentContainer).childCount); i < num2; i++)
			{
				Transform child = ((Transform)segmentContainer).GetChild(i);
				images[i] = ((Component)child).GetComponent<Image>();
				labels[i] = ((Component)child.Find("Label")).GetComponent<TextMeshProUGUI>();
			}
			while (num > desiredCount)
			{
				Object.DestroyImmediate((Object)(object)((Component)((Transform)segmentContainer).GetChild((int)(num - 1))).gameObject);
				num--;
			}
			for (; num < desiredCount; num++)
			{
				GameObject val = Object.Instantiate<GameObject>(segmentPrefab, (Transform)(object)segmentContainer);
				val.SetActive(true);
				images[i] = val.GetComponent<Image>();
				labels[i] = ((Component)val.transform.Find("Label")).GetComponent<TextMeshProUGUI>();
				i++;
			}
		}
	}

	private void SetupSegments()
	{
		//IL_003a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0051: Expected O, but got Unknown
		//IL_007c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0086: Expected O, but got Unknown
		if (Object.op_Implicit((Object)(object)segmentContainer) && Object.op_Implicit((Object)(object)segmentPrefab))
		{
			SetSegmentCount((uint)segmentDefs.Length);
			for (int i = 0; i < ((Transform)segmentContainer).childCount; i++)
			{
				SetupSegment((RectTransform)((Transform)segmentContainer).GetChild(i), ref segmentDefs[i], i);
			}
			SetupFinalSegment((RectTransform)((Transform)segmentContainer).GetChild(((Transform)segmentContainer).childCount - 1));
		}
	}

	private static void ScaleLabelToWidth(TextMeshProUGUI label, float width)
	{
		//IL_0006: Unknown result type (might be due to invalid IL or missing references)
		//IL_000c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0011: Unknown result type (might be due to invalid IL or missing references)
		//IL_0014: Unknown result type (might be due to invalid IL or missing references)
		//IL_001f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0020: Unknown result type (might be due to invalid IL or missing references)
		//IL_0025: Unknown result type (might be due to invalid IL or missing references)
		//IL_0030: Unknown result type (might be due to invalid IL or missing references)
		RectTransform val = (RectTransform)((TMP_Text)label).transform;
		Bounds textBounds = ((TMP_Text)label).textBounds;
		float x = ((Bounds)(ref textBounds)).size.x;
		Vector3 localScale = ((Transform)val).localScale;
		localScale.x = width / x;
		((Transform)val).localScale = localScale;
	}

	private void SetupFinalSegment(RectTransform segmentTransform)
	{
		//IL_00ae: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b3: Unknown result type (might be due to invalid IL or missing references)
		//IL_00cb: Unknown result type (might be due to invalid IL or missing references)
		TextMeshProUGUI[] array = ((Component)segmentTransform).GetComponentsInChildren<TextMeshProUGUI>();
		int num = 4;
		if (array.Length < num)
		{
			TextMeshProUGUI[] array2 = (TextMeshProUGUI[])(object)new TextMeshProUGUI[num];
			for (int i = 0; i < array.Length; i++)
			{
				array2[i] = array[i];
			}
			for (int j = array.Length; j < num; j++)
			{
				array2[j] = Object.Instantiate<GameObject>(((Component)array[0]).gameObject, (Transform)(object)segmentTransform).GetComponent<TextMeshProUGUI>();
			}
			array = array2;
		}
		int k = 0;
		for (int num2 = array.Length; k < num2; k++)
		{
			TextMeshProUGUI obj = array[k];
			((TMP_Text)obj).enableWordWrapping = false;
			((TMP_Text)obj).overflowMode = (TextOverflowModes)0;
			((TMP_Text)obj).alignment = (TextAlignmentOptions)4097;
			((TMP_Text)obj).text = Language.GetString(segmentDefs[segmentDefs.Length - 1].token);
			((TMP_Text)obj).enableAutoSizing = true;
			Vector3 localPosition = ((TMP_Text)obj).transform.localPosition;
			localPosition.x = (float)k * elementWidth;
			((TMP_Text)obj).transform.localPosition = localPosition;
		}
		((Component)segmentTransform).GetComponent<Image>().sprite = finalSegmentSprite;
	}

	private void SetupSegment(RectTransform segmentTransform, ref SegmentDef segmentDef, int i)
	{
		//IL_0001: Unknown result type (might be due to invalid IL or missing references)
		//IL_0006: Unknown result type (might be due to invalid IL or missing references)
		//IL_0008: Unknown result type (might be due to invalid IL or missing references)
		//IL_000d: Unknown result type (might be due to invalid IL or missing references)
		//IL_002b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0032: Unknown result type (might be due to invalid IL or missing references)
		//IL_003f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0054: Unknown result type (might be due to invalid IL or missing references)
		Vector2 offsetMin = segmentTransform.offsetMin;
		Vector2 offsetMax = segmentTransform.offsetMax;
		offsetMin.x = CalcSegmentStartX(i);
		offsetMax.x = CalcSegmentEndX(i);
		segmentTransform.offsetMin = offsetMin;
		segmentTransform.offsetMax = offsetMax;
		((Graphic)((Component)segmentTransform).GetComponent<Image>()).color = segmentDef.color;
		((Component)(RectTransform)((Transform)segmentTransform).Find("Label")).GetComponent<LanguageTextMeshController>().token = segmentDef.token;
	}

	private void Awake()
	{
		SetupSegments();
	}

	private void Update()
	{
		if (Object.op_Implicit((Object)(object)Run.instance))
		{
			SetSegmentScroll((Run.instance.ambientLevel - 1f) / levelsPerSegment);
		}
		if (Application.isPlaying)
		{
			RunAnimations(Time.deltaTime);
		}
		UpdateGears();
	}

	private void UpdateGears()
	{
		//IL_000f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0014: Unknown result type (might be due to invalid IL or missing references)
		//IL_004e: Unknown result type (might be due to invalid IL or missing references)
		RawImage[] array = wormGearImages;
		foreach (RawImage obj in array)
		{
			Rect uvRect = obj.uvRect;
			float num = Mathf.Sign(((Rect)(ref uvRect)).width);
			((Rect)(ref uvRect)).x = scrollXRaw * UVScaleToScrollX * num + ((num < 0f) ? gearUVOffset : 0f);
			obj.uvRect = uvRect;
		}
	}

	private void RunAnimations(float deltaTime)
	{
		for (int num = playingAnimations.Count - 1; num >= 0; num--)
		{
			SegmentImageAnimation segmentImageAnimation = playingAnimations[num];
			segmentImageAnimation.age += deltaTime;
			float num2 = Mathf.Clamp01(segmentImageAnimation.age / segmentImageAnimation.duration);
			segmentImageAnimation.Update(num2);
			if (num2 >= 1f)
			{
				playingAnimations.RemoveAt(num);
			}
		}
	}
}
