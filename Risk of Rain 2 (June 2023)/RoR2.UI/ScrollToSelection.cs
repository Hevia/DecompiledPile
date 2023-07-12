using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace RoR2.UI;

[RequireComponent(typeof(MPEventSystemLocator))]
[RequireComponent(typeof(ScrollRect))]
public class ScrollToSelection : MonoBehaviour
{
	private ScrollRect scrollRect;

	private MPEventSystemLocator eventSystemLocator;

	private Vector3[] targetWorldCorners = (Vector3[])(object)new Vector3[4];

	private Vector3[] viewPortWorldCorners = (Vector3[])(object)new Vector3[4];

	private GameObject lastSelectedObject;

	private EventSystem eventSystem => (EventSystem)(object)eventSystemLocator.eventSystem;

	private void Awake()
	{
		scrollRect = ((Component)this).GetComponent<ScrollRect>();
		eventSystemLocator = ((Component)this).GetComponent<MPEventSystemLocator>();
	}

	private void Update()
	{
		//IL_004c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0056: Expected O, but got Unknown
		GameObject val = (Object.op_Implicit((Object)(object)eventSystem) ? eventSystem.currentSelectedGameObject : null);
		if ((Object)(object)lastSelectedObject != (Object)(object)val)
		{
			if (Object.op_Implicit((Object)(object)val) && val.transform.IsChildOf(((Component)this).transform))
			{
				ScrollToRect((RectTransform)val.transform);
			}
			lastSelectedObject = val;
		}
	}

	private void ScrollToRect(RectTransform targetRectTransform)
	{
		//IL_0012: Unknown result type (might be due to invalid IL or missing references)
		targetRectTransform.GetWorldCorners(targetWorldCorners);
		((RectTransform)((Component)this).transform).GetWorldCorners(viewPortWorldCorners);
		if (scrollRect.vertical && Object.op_Implicit((Object)(object)scrollRect.verticalScrollbar))
		{
			float y = targetWorldCorners[1].y;
			float y2 = targetWorldCorners[0].y;
			float y3 = viewPortWorldCorners[1].y;
			float y4 = viewPortWorldCorners[0].y;
			float num = y - y3;
			float num2 = y2 - y4;
			float num3 = y3 - y4;
			if (num > 0f)
			{
				Scrollbar verticalScrollbar = scrollRect.verticalScrollbar;
				verticalScrollbar.value += num / num3;
			}
			if (num2 < 0f)
			{
				Scrollbar verticalScrollbar2 = scrollRect.verticalScrollbar;
				verticalScrollbar2.value += num2 / num3;
			}
		}
		if (scrollRect.horizontal && Object.op_Implicit((Object)(object)scrollRect.horizontalScrollbar))
		{
			float y5 = targetWorldCorners[2].y;
			float y6 = targetWorldCorners[0].y;
			float y7 = viewPortWorldCorners[2].y;
			float y8 = viewPortWorldCorners[0].y;
			float num4 = y5 - y7;
			float num5 = y6 - y8;
			float num6 = y7 - y8;
			if (num4 > 0f)
			{
				Scrollbar horizontalScrollbar = scrollRect.horizontalScrollbar;
				horizontalScrollbar.value += num4 / num6;
			}
			if (num5 < 0f)
			{
				Scrollbar horizontalScrollbar2 = scrollRect.horizontalScrollbar;
				horizontalScrollbar2.value += num5 / num6;
			}
		}
	}
}
