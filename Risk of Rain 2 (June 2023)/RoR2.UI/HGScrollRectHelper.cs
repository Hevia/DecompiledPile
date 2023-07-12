using UnityEngine;
using UnityEngine.UI;

namespace RoR2.UI;

[RequireComponent(typeof(ScrollRect))]
[RequireComponent(typeof(MPEventSystemLocator))]
public class HGScrollRectHelper : MonoBehaviour
{
	public float spacingPerElement;

	public bool allowVerticalScrollingWithGamepadSticks;

	public float stickScale;

	public UILayerKey requiredTopLayer;

	private ScrollRect scrollRect;

	private RectTransform contentRectTransform;

	private MPEventSystemLocator eventSystemLocator;

	private float idealVerticalNormalizedPosition;

	private float scrollbarVelocity;

	private const float scrollbarSmoothdampTime = 0.1f;

	private bool hasInitialized;

	private void Start()
	{
		Initialize();
	}

	private void Initialize()
	{
		if (!hasInitialized)
		{
			hasInitialized = true;
			scrollRect = ((Component)this).GetComponent<ScrollRect>();
			contentRectTransform = ((Component)scrollRect.content).GetComponent<RectTransform>();
			eventSystemLocator = ((Component)this).GetComponent<MPEventSystemLocator>();
			idealVerticalNormalizedPosition = scrollRect.verticalNormalizedPosition;
		}
	}

	private bool GamepadIsCurrentInputSource()
	{
		if (!hasInitialized)
		{
			return false;
		}
		if (Object.op_Implicit((Object)(object)eventSystemLocator.eventSystem))
		{
			return eventSystemLocator.eventSystem.currentInputSource == MPEventSystem.InputSource.Gamepad;
		}
		return false;
	}

	private bool CanAcceptInput()
	{
		if (Object.op_Implicit((Object)(object)requiredTopLayer))
		{
			return requiredTopLayer.representsTopLayer;
		}
		return true;
	}

	private void Update()
	{
		//IL_004a: Unknown result type (might be due to invalid IL or missing references)
		//IL_004f: Unknown result type (might be due to invalid IL or missing references)
		Initialize();
		if (!GamepadIsCurrentInputSource())
		{
			return;
		}
		if (Object.op_Implicit((Object)(object)eventSystemLocator) && Object.op_Implicit((Object)(object)eventSystemLocator.eventSystem) && CanAcceptInput())
		{
			Rect rect = scrollRect.content.rect;
			float height = ((Rect)(ref rect)).height;
			float axis = eventSystemLocator.eventSystem.player.GetAxis(13);
			eventSystemLocator.eventSystem.player.GetAxis(12);
			if (allowVerticalScrollingWithGamepadSticks)
			{
				idealVerticalNormalizedPosition = Mathf.Clamp01(idealVerticalNormalizedPosition + axis * stickScale * Time.unscaledDeltaTime / height);
				scrollRect.verticalNormalizedPosition = idealVerticalNormalizedPosition;
			}
		}
		scrollRect.verticalNormalizedPosition = Mathf.SmoothDamp(scrollRect.verticalNormalizedPosition, idealVerticalNormalizedPosition, ref scrollbarVelocity, 0.1f, float.PositiveInfinity, Time.unscaledDeltaTime);
	}

	public void ScrollToShowMe(MPButton mpButton)
	{
		//IL_003a: Unknown result type (might be due to invalid IL or missing references)
		//IL_003f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0043: Unknown result type (might be due to invalid IL or missing references)
		//IL_0060: Unknown result type (might be due to invalid IL or missing references)
		//IL_0065: Unknown result type (might be due to invalid IL or missing references)
		//IL_0069: Unknown result type (might be due to invalid IL or missing references)
		//IL_007c: Unknown result type (might be due to invalid IL or missing references)
		//IL_009f: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b5: Unknown result type (might be due to invalid IL or missing references)
		//IL_00dc: Unknown result type (might be due to invalid IL or missing references)
		Initialize();
		if (!GamepadIsCurrentInputSource() || !CanAcceptInput())
		{
			return;
		}
		Canvas.ForceUpdateCanvases();
		_ = scrollRect.content;
		RectTransform component = ((Component)mpButton).GetComponent<RectTransform>();
		Rect rect = scrollRect.viewport.rect;
		float num = (0f - ((Rect)(ref rect)).size.y) / 2f;
		rect = scrollRect.content.rect;
		float num2 = 0f - ((Rect)(ref rect)).size.y;
		float num3 = 0f;
		if (component.anchoredPosition.y - spacingPerElement > num3 + num)
		{
			idealVerticalNormalizedPosition = 1f;
		}
		else if (!(component.anchoredPosition.y - spacingPerElement > num))
		{
			if (component.anchoredPosition.y - spacingPerElement < num2 - num)
			{
				idealVerticalNormalizedPosition = 0f;
				return;
			}
			float num4 = Mathf.InverseLerp(num, num2 - num, component.anchoredPosition.y - spacingPerElement);
			float num5 = 1f - num4;
			idealVerticalNormalizedPosition = num5;
		}
	}

	private static Rect GetWorldspaceRect(Vector3 pos, Rect rect, Vector2 offset)
	{
		//IL_0000: Unknown result type (might be due to invalid IL or missing references)
		//IL_000e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0016: Unknown result type (might be due to invalid IL or missing references)
		//IL_0024: Unknown result type (might be due to invalid IL or missing references)
		//IL_002e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0035: Unknown result type (might be due to invalid IL or missing references)
		//IL_003a: Unknown result type (might be due to invalid IL or missing references)
		float num = pos.x + ((Rect)(ref rect)).xMin - offset.x;
		float num2 = pos.y + ((Rect)(ref rect)).yMin - offset.y;
		return new Rect(new Vector2(num, num2), ((Rect)(ref rect)).size);
	}
}
