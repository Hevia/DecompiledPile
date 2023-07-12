using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace RoR2.UI;

[ExecuteAlways]
[RequireComponent(typeof(RectTransform))]
[RequireComponent(typeof(LayoutElement))]
public class ColumnLayoutGroupElement : MonoBehaviour, IBeginDragHandler, IEventSystemHandler, IDragHandler
{
	private enum ClickLocation
	{
		None,
		Middle,
		RightHandle,
		LeftHandle
	}

	private RectTransform rectTransform;

	private LayoutElement layoutElement;

	public RectTransform rectTransformToLayoutInvalidate;

	private float handleWidth = 4f;

	private ClickLocation lastClickLocation;

	private void Awake()
	{
		//IL_0007: Unknown result type (might be due to invalid IL or missing references)
		//IL_0011: Expected O, but got Unknown
		rectTransform = (RectTransform)((Component)this).transform;
		layoutElement = ((Component)this).GetComponent<LayoutElement>();
	}

	public void OnBeginDrag(PointerEventData eventData)
	{
		//IL_0009: Unknown result type (might be due to invalid IL or missing references)
		//IL_0023: Unknown result type (might be due to invalid IL or missing references)
		//IL_0028: Unknown result type (might be due to invalid IL or missing references)
		//IL_0031: Unknown result type (might be due to invalid IL or missing references)
		//IL_003f: Unknown result type (might be due to invalid IL or missing references)
		//IL_004d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0054: Unknown result type (might be due to invalid IL or missing references)
		ClickLocation clickLocation = ClickLocation.None;
		Vector2 val = default(Vector2);
		if (RectTransformUtility.ScreenPointToLocalPointInRectangle(rectTransform, eventData.position, eventData.pressEventCamera, ref val))
		{
			Rect rect = rectTransform.rect;
			float width = ((Rect)(ref rect)).width;
			Vector2 val2 = new Vector2(val.x * ((Rect)(ref rect)).width, val.y * ((Rect)(ref rect)).height);
			clickLocation = ClickLocation.Middle;
			if (val2.x < handleWidth)
			{
				clickLocation = ClickLocation.LeftHandle;
			}
			if (val2.x > width - handleWidth)
			{
				clickLocation = ClickLocation.RightHandle;
			}
		}
		lastClickLocation = clickLocation;
	}

	public void OnDrag(PointerEventData eventData)
	{
		//IL_0041: Unknown result type (might be due to invalid IL or missing references)
		//IL_0081: Unknown result type (might be due to invalid IL or missing references)
		Transform parent = ((Transform)rectTransform).parent;
		int siblingIndex = ((Transform)rectTransform).GetSiblingIndex();
		if (lastClickLocation == ClickLocation.LeftHandle && siblingIndex != 0)
		{
			Transform child = parent.GetChild(siblingIndex - 1);
			AdjustWidth((child != null) ? ((Component)child).GetComponent<LayoutElement>() : null, layoutElement, eventData.delta.x);
		}
		if (lastClickLocation == ClickLocation.RightHandle && siblingIndex != parent.childCount - 1)
		{
			LayoutElement lhs2 = layoutElement;
			Transform child2 = parent.GetChild(siblingIndex + 1);
			AdjustWidth(lhs2, (child2 != null) ? ((Component)child2).GetComponent<LayoutElement>() : null, eventData.delta.x);
		}
		void AdjustWidth(LayoutElement lhs, LayoutElement rhs, float change)
		{
			if (Object.op_Implicit((Object)(object)lhs) && Object.op_Implicit((Object)(object)rhs))
			{
				if (lhs.preferredWidth + change < lhs.minWidth)
				{
					change = lhs.minWidth - lhs.preferredWidth;
				}
				if (rhs.preferredWidth - change < rhs.minWidth)
				{
					change = rhs.preferredWidth - rhs.minWidth;
				}
				if (change != 0f)
				{
					lhs.preferredWidth += change;
					rhs.preferredWidth -= change;
					if (Object.op_Implicit((Object)(object)rectTransformToLayoutInvalidate))
					{
						LayoutRebuilder.MarkLayoutForRebuild(rectTransformToLayoutInvalidate);
					}
				}
			}
		}
	}
}
