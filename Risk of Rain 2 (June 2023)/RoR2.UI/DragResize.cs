using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace RoR2.UI;

[RequireComponent(typeof(RectTransform))]
public class DragResize : MonoBehaviour, IBeginDragHandler, IEventSystemHandler, IDragHandler
{
	public RectTransform targetTransform;

	public Vector2 minSize;

	private Vector2 grabPoint;

	private RectTransform rectTransform;

	private void Awake()
	{
		//IL_0007: Unknown result type (might be due to invalid IL or missing references)
		//IL_0011: Expected O, but got Unknown
		rectTransform = (RectTransform)((Component)this).transform;
	}

	public void OnDrag(PointerEventData eventData)
	{
		UpdateDrag(eventData);
	}

	public void OnBeginDrag(PointerEventData eventData)
	{
		//IL_0014: Unknown result type (might be due to invalid IL or missing references)
		if (Object.op_Implicit((Object)(object)targetTransform))
		{
			RectTransformUtility.ScreenPointToLocalPointInRectangle(targetTransform, eventData.position, eventData.pressEventCamera, ref grabPoint);
		}
	}

	private void UpdateDrag(PointerEventData eventData)
	{
		//IL_0001: Unknown result type (might be due to invalid IL or missing references)
		//IL_001e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0031: Unknown result type (might be due to invalid IL or missing references)
		//IL_0033: Unknown result type (might be due to invalid IL or missing references)
		//IL_0038: Unknown result type (might be due to invalid IL or missing references)
		//IL_003d: Unknown result type (might be due to invalid IL or missing references)
		//IL_003f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0040: Unknown result type (might be due to invalid IL or missing references)
		//IL_0047: Unknown result type (might be due to invalid IL or missing references)
		//IL_0074: Unknown result type (might be due to invalid IL or missing references)
		//IL_0079: Unknown result type (might be due to invalid IL or missing references)
		//IL_007a: Unknown result type (might be due to invalid IL or missing references)
		//IL_007f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0090: Unknown result type (might be due to invalid IL or missing references)
		//IL_0095: Unknown result type (might be due to invalid IL or missing references)
		//IL_0096: Unknown result type (might be due to invalid IL or missing references)
		//IL_009c: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a1: Unknown result type (might be due to invalid IL or missing references)
		if ((int)eventData.button == 0 && Object.op_Implicit((Object)(object)targetTransform))
		{
			Vector2 val = default(Vector2);
			RectTransformUtility.ScreenPointToLocalPointInRectangle(targetTransform, eventData.position, eventData.pressEventCamera, ref val);
			Vector2 val2 = val - grabPoint;
			grabPoint = val;
			val2.y = 0f - val2.y;
			Vector2 val3 = default(Vector2);
			((Vector2)(ref val3))._002Ector(LayoutUtility.GetMinSize(targetTransform, 0), LayoutUtility.GetMinSize(targetTransform, 1));
			minSize = Vector2.Max(minSize, val3);
			targetTransform.sizeDelta = Vector2.Max(targetTransform.sizeDelta + val2, minSize);
		}
	}
}
