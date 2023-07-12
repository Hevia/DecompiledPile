using System;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace RoR2.UI;

[RequireComponent(typeof(MPEventSystemLocator))]
public class MPToggle : Toggle
{
	private MPControlHelper mpControlHelper;

	public bool allowAllEventSystems;

	public bool disablePointerClick;

	public Graphic hoverGraphic;

	private bool inPointerUp;

	protected override void Awake()
	{
		((Selectable)this).Awake();
		mpControlHelper = new MPControlHelper((Selectable)(object)this, ((Component)this).GetComponent<MPEventSystemLocator>(), allowAllEventSystems, disablePointerClick);
	}

	private void LateUpdate()
	{
		//IL_000e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0014: Invalid comparison between Unknown and I4
		if (Object.op_Implicit((Object)(object)hoverGraphic))
		{
			bool flag = (int)((Selectable)this).currentSelectionState == 1;
			if (((Behaviour)hoverGraphic).enabled != flag)
			{
				((Behaviour)hoverGraphic).enabled = flag;
			}
		}
	}

	public override void OnPointerEnter(PointerEventData eventData)
	{
		mpControlHelper.OnPointerEnter(eventData, (Action<PointerEventData>)((Selectable)this).OnPointerEnter);
	}

	public override void OnPointerExit(PointerEventData eventData)
	{
		mpControlHelper.OnPointerExit(eventData, (Action<PointerEventData>)((Selectable)this).OnPointerExit);
	}

	public override void OnPointerClick(PointerEventData eventData)
	{
		mpControlHelper.OnPointerClick(eventData, (Action<PointerEventData>)base.OnPointerClick);
	}

	public override void OnPointerUp(PointerEventData eventData)
	{
		mpControlHelper.OnPointerUp(eventData, (Action<PointerEventData>)((Selectable)this).OnPointerUp, ref inPointerUp);
	}

	public override void OnPointerDown(PointerEventData eventData)
	{
		mpControlHelper.OnPointerDown(eventData, (Action<PointerEventData>)((Selectable)this).OnPointerDown, ref inPointerUp);
	}

	public override Selectable FindSelectableOnDown()
	{
		return mpControlHelper.FindSelectableOnDown((Func<Selectable>)((Selectable)this).FindSelectableOnDown);
	}

	public override Selectable FindSelectableOnLeft()
	{
		return mpControlHelper.FindSelectableOnLeft((Func<Selectable>)((Selectable)this).FindSelectableOnLeft);
	}

	public override Selectable FindSelectableOnRight()
	{
		return mpControlHelper.FindSelectableOnRight((Func<Selectable>)((Selectable)this).FindSelectableOnRight);
	}

	public override Selectable FindSelectableOnUp()
	{
		return mpControlHelper.FindSelectableOnUp((Func<Selectable>)((Selectable)this).FindSelectableOnUp);
	}
}
