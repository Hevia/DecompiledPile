using System;
using TMPro;
using UnityEngine;
using UnityEngine.Serialization;
using UnityEngine.UI;

namespace RoR2.UI;

[CreateAssetMenu(menuName = "RoR2/UISkinData")]
public class UISkinData : ScriptableObject
{
	[Serializable]
	public struct TextStyle
	{
		public TMP_FontAsset font;

		public float fontSize;

		public TextAlignmentOptions alignment;

		public Color color;

		public void Apply(TextMeshProUGUI label, bool useAlignment = true)
		{
			//IL_004c: Unknown result type (might be due to invalid IL or missing references)
			//IL_0052: Unknown result type (might be due to invalid IL or missing references)
			//IL_0060: Unknown result type (might be due to invalid IL or missing references)
			//IL_006e: Unknown result type (might be due to invalid IL or missing references)
			//IL_0074: Unknown result type (might be due to invalid IL or missing references)
			//IL_007d: Unknown result type (might be due to invalid IL or missing references)
			if ((Object)(object)((TMP_Text)label).font != (Object)(object)font && (!(label is HGTextMeshProUGUI hGTextMeshProUGUI) || !hGTextMeshProUGUI.useLanguageDefaultFont))
			{
				((TMP_Text)label).font = font;
			}
			if (((TMP_Text)label).fontSize != fontSize)
			{
				((TMP_Text)label).fontSize = fontSize;
			}
			if (((Graphic)label).color != color)
			{
				((Graphic)label).color = color;
			}
			if (useAlignment && ((TMP_Text)label).alignment != alignment)
			{
				((TMP_Text)label).alignment = alignment;
			}
		}
	}

	[Serializable]
	public struct PanelStyle
	{
		public Material material;

		public Sprite sprite;

		public Color color;

		public void Apply(Image image)
		{
			//IL_001a: Unknown result type (might be due to invalid IL or missing references)
			((Graphic)image).material = material;
			image.sprite = sprite;
			((Graphic)image).color = color;
		}
	}

	[Serializable]
	public struct ButtonStyle
	{
		public Material material;

		public Sprite sprite;

		public ColorBlock colors;

		public TextStyle interactableTextStyle;

		public TextStyle disabledTextStyle;

		public float recommendedWidth;

		public float recommendedHeight;
	}

	[Serializable]
	public struct ScrollRectStyle
	{
		[FormerlySerializedAs("viewportPanelStyle")]
		public PanelStyle backgroundPanelStyle;

		public PanelStyle scrollbarBackgroundStyle;

		public ColorBlock scrollbarHandleColors;

		public Sprite scrollbarHandleImage;
	}

	[Header("Main Panel Style")]
	public PanelStyle mainPanelStyle;

	[Header("Header Style")]
	public PanelStyle headerPanelStyle;

	public TextStyle headerTextStyle;

	[Header("Detail Style")]
	public PanelStyle detailPanelStyle;

	public TextStyle detailTextStyle;

	[Header("Body Style")]
	public TextStyle bodyTextStyle;

	[Header("Button Style")]
	public ButtonStyle buttonStyle;

	[Header("Scroll Rect Style")]
	public ScrollRectStyle scrollRectStyle;
}
