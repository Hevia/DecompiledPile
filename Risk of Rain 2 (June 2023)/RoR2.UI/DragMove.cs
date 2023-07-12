using UnityEngine;
using UnityEngine.EventSystems;

namespace RoR2.UI;

[RequireComponent(typeof(RectTransform))]
public class DragMove : MonoBehaviour, IBeginDragHandler, IEventSystemHandler, IDragHandler
{
	public RectTransform targetTransform;

	private Vector2 grabPoint;

	private RectTransform rectTransform;

	private void OnAwake()
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
		//IL_0045: Unknown result type (might be due to invalid IL or missing references)
		//IL_004a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0050: Unknown result type (might be due to invalid IL or missing references)
		//IL_005b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0060: Unknown result type (might be due to invalid IL or missing references)
		if ((int)eventData.button == 0 && Object.op_Implicit((Object)(object)targetTransform))
		{
			Vector2 val = default(Vector2);
			RectTransformUtility.ScreenPointToLocalPointInRectangle(targetTransform, eventData.position, eventData.pressEventCamera, ref val);
			Vector2 val2 = val - grabPoint;
			RectTransform obj = targetTransform;
			((Transform)obj).localPosition = ((Transform)obj).localPosition + new Vector3(val2.x, val2.y, 0f);
		}
	}
}
