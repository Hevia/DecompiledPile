using System;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace RoR2.UI;

public class TooltipController : MonoBehaviour
{
	private static readonly List<TooltipController> instancesList = new List<TooltipController>();

	[NonSerialized]
	public MPEventSystem owner;

	public RectTransform tooltipCenterTransform;

	public RectTransform tooltipFlipTransform;

	public Image colorHighlightImage;

	public TextMeshProUGUI titleLabel;

	public TextMeshProUGUI bodyLabel;

	private UICamera uiCamera;

	private void SetTooltipProvider(TooltipProvider provider)
	{
		//IL_0051: Unknown result type (might be due to invalid IL or missing references)
		((TMP_Text)titleLabel).text = provider.titleText;
		((TMP_Text)titleLabel).richText = !provider.disableTitleRichText;
		((TMP_Text)bodyLabel).text = provider.bodyText;
		((TMP_Text)bodyLabel).richText = !provider.disableBodyRichText;
		((Graphic)colorHighlightImage).color = provider.titleColor;
	}

	private static UICamera FindUICamera(MPEventSystem mpEventSystem)
	{
		foreach (UICamera readOnlyInstances in UICamera.readOnlyInstancesList)
		{
			if ((Object)(object)(readOnlyInstances.GetAssociatedEventSystem() as MPEventSystem) == (Object)(object)mpEventSystem)
			{
				return readOnlyInstances;
			}
		}
		return null;
	}

	private void Awake()
	{
		instancesList.Add(this);
	}

	private void OnDestroy()
	{
		instancesList.Remove(this);
	}

	private void LateUpdate()
	{
		//IL_0022: Unknown result type (might be due to invalid IL or missing references)
		//IL_0023: Unknown result type (might be due to invalid IL or missing references)
		if (Object.op_Implicit((Object)(object)owner) && owner.GetCursorPosition(out var position))
		{
			((Transform)tooltipCenterTransform).position = Vector2.op_Implicit(position);
		}
	}

	public static void RemoveTooltip(TooltipProvider tooltipProvider)
	{
		if (tooltipProvider.userCount <= 0)
		{
			return;
		}
		foreach (MPEventSystem readOnlyInstances in MPEventSystem.readOnlyInstancesList)
		{
			RemoveTooltip(readOnlyInstances, tooltipProvider);
		}
	}

	public static void RemoveTooltip(MPEventSystem eventSystem, TooltipProvider tooltipProvider)
	{
		//IL_0010: Unknown result type (might be due to invalid IL or missing references)
		//IL_0015: Unknown result type (might be due to invalid IL or missing references)
		if ((Object)(object)eventSystem.currentTooltipProvider == (Object)(object)tooltipProvider)
		{
			SetTooltip(eventSystem, null, Vector2.op_Implicit(Vector3.zero));
		}
	}

	public static void SetTooltip(MPEventSystem eventSystem, TooltipProvider newTooltipProvider, Vector2 tooltipPosition)
	{
		//IL_00ee: Unknown result type (might be due to invalid IL or missing references)
		//IL_00f3: Unknown result type (might be due to invalid IL or missing references)
		//IL_011e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0124: Unknown result type (might be due to invalid IL or missing references)
		//IL_012f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0134: Unknown result type (might be due to invalid IL or missing references)
		//IL_0139: Unknown result type (might be due to invalid IL or missing references)
		//IL_013d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0144: Unknown result type (might be due to invalid IL or missing references)
		//IL_0163: Unknown result type (might be due to invalid IL or missing references)
		//IL_0183: Unknown result type (might be due to invalid IL or missing references)
		//IL_01ac: Unknown result type (might be due to invalid IL or missing references)
		//IL_01be: Unknown result type (might be due to invalid IL or missing references)
		//IL_01d0: Unknown result type (might be due to invalid IL or missing references)
		if ((Object)(object)eventSystem.currentTooltipProvider != (Object)(object)newTooltipProvider)
		{
			if (Object.op_Implicit((Object)(object)eventSystem.currentTooltip))
			{
				Object.Destroy((Object)(object)((Component)eventSystem.currentTooltip).gameObject);
				eventSystem.currentTooltip = null;
			}
			if (Object.op_Implicit((Object)(object)eventSystem.currentTooltipProvider))
			{
				eventSystem.currentTooltipProvider.userCount--;
			}
			eventSystem.currentTooltipProvider = newTooltipProvider;
			if (Object.op_Implicit((Object)(object)newTooltipProvider))
			{
				newTooltipProvider.userCount++;
				GameObject val = Object.Instantiate<GameObject>(LegacyResourcesAPI.Load<GameObject>("Prefabs/UI/Tooltip"));
				eventSystem.currentTooltip = val.GetComponent<TooltipController>();
				eventSystem.currentTooltip.owner = eventSystem;
				eventSystem.currentTooltip.uiCamera = FindUICamera(eventSystem);
				eventSystem.currentTooltip.SetTooltipProvider(eventSystem.currentTooltipProvider);
				val.GetComponent<Canvas>().worldCamera = eventSystem.currentTooltip.uiCamera?.camera;
			}
		}
		if (Object.op_Implicit((Object)(object)eventSystem.currentTooltip))
		{
			Vector2 zero = Vector2.zero;
			UICamera uICamera = eventSystem.currentTooltip.uiCamera;
			Camera val2 = Camera.main;
			if (Object.op_Implicit((Object)(object)uICamera))
			{
				val2 = uICamera.camera;
			}
			if (Object.op_Implicit((Object)(object)val2))
			{
				Vector3 val3 = val2.ScreenToViewportPoint(new Vector3(tooltipPosition.x, tooltipPosition.y, 0f));
				((Vector2)(ref zero))._002Ector(val3.x, val3.y);
			}
			Vector2 val4 = default(Vector2);
			((Vector2)(ref val4))._002Ector(0f, 0f);
			val4.x = ((zero.x > 0.5f) ? 1f : 0f);
			val4.y = ((zero.y > 0.5f) ? 1f : 0f);
			eventSystem.currentTooltip.tooltipFlipTransform.anchorMin = val4;
			eventSystem.currentTooltip.tooltipFlipTransform.anchorMax = val4;
			eventSystem.currentTooltip.tooltipFlipTransform.pivot = val4;
		}
	}
}
