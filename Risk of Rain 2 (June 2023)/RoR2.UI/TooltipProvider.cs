using System;
using UnityEngine;
using UnityEngine.EventSystems;

namespace RoR2.UI;

public class TooltipProvider : MonoBehaviour, IPointerEnterHandler, IEventSystemHandler, IPointerExitHandler
{
	public string titleToken = "";

	public Color titleColor = Color.clear;

	public string bodyToken = "";

	public Color bodyColor;

	public string overrideTitleText = "";

	public string overrideBodyText = "";

	public bool disableTitleRichText;

	public bool disableBodyRichText;

	[NonSerialized]
	public int userCount;

	private static readonly Color playerColor = Color32.op_Implicit(new Color32((byte)242, (byte)65, (byte)65, byte.MaxValue));

	private bool tooltipIsAvailable => titleColor != Color.clear;

	public string titleText
	{
		get
		{
			if (!string.IsNullOrEmpty(overrideTitleText))
			{
				return overrideTitleText;
			}
			if (titleToken == null)
			{
				return null;
			}
			return Language.GetString(titleToken);
		}
	}

	public string bodyText
	{
		get
		{
			if (!string.IsNullOrEmpty(overrideBodyText))
			{
				return overrideBodyText;
			}
			if (bodyToken == null)
			{
				return null;
			}
			return Language.GetString(bodyToken);
		}
	}

	public void SetContent(TooltipContent tooltipContent)
	{
		//IL_001a: Unknown result type (might be due to invalid IL or missing references)
		//IL_001f: Unknown result type (might be due to invalid IL or missing references)
		//IL_003e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0043: Unknown result type (might be due to invalid IL or missing references)
		titleToken = tooltipContent.titleToken;
		overrideTitleText = tooltipContent.overrideTitleText;
		titleColor = tooltipContent.titleColor;
		bodyToken = tooltipContent.bodyToken;
		overrideBodyText = tooltipContent.overrideBodyText;
		bodyColor = tooltipContent.bodyColor;
		disableTitleRichText = tooltipContent.disableTitleRichText;
		disableBodyRichText = tooltipContent.disableBodyRichText;
	}

	private void OnDisable()
	{
		TooltipController.RemoveTooltip(this);
	}

	void IPointerEnterHandler.OnPointerEnter(PointerEventData eventData)
	{
		//IL_001f: Unknown result type (might be due to invalid IL or missing references)
		MPEventSystem mPEventSystem = EventSystem.current as MPEventSystem;
		if ((Object)(object)mPEventSystem != (Object)null && tooltipIsAvailable)
		{
			TooltipController.SetTooltip(mPEventSystem, this, eventData.position);
		}
	}

	void IPointerExitHandler.OnPointerExit(PointerEventData eventData)
	{
		MPEventSystem mPEventSystem = EventSystem.current as MPEventSystem;
		if ((Object)(object)mPEventSystem != (Object)null && tooltipIsAvailable)
		{
			TooltipController.RemoveTooltip(mPEventSystem, this);
		}
	}

	public static TooltipContent GetPlayerNameTooltipContent(string userName)
	{
		//IL_002f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0034: Unknown result type (might be due to invalid IL or missing references)
		string stringFormatted = Language.GetStringFormatted("PLAYER_NAME_TOOLTIP_FORMAT", userName);
		TooltipContent result = default(TooltipContent);
		result.overrideTitleText = stringFormatted;
		result.disableTitleRichText = true;
		result.titleColor = playerColor;
		return result;
	}
}
