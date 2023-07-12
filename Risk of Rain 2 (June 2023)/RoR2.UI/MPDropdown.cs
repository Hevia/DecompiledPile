using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace RoR2.UI;

[RequireComponent(typeof(MPEventSystemLocator))]
public class MPDropdown : TMP_Dropdown
{
	private MPEventSystemLocator eventSystemLocator;

	protected bool isPointerInside;

	public bool allowAllEventSystems;

	private SelectionState previousState = (SelectionState)4;

	protected MPEventSystem eventSystem => eventSystemLocator.eventSystem;

	protected override void Awake()
	{
		((TMP_Dropdown)this).Awake();
		eventSystemLocator = ((Component)this).GetComponent<MPEventSystemLocator>();
	}

	private Selectable FilterSelectable(Selectable selectable)
	{
		if (Object.op_Implicit((Object)(object)selectable))
		{
			MPEventSystemLocator component = ((Component)selectable).GetComponent<MPEventSystemLocator>();
			if (!Object.op_Implicit((Object)(object)component) || (Object)(object)component.eventSystem != (Object)(object)eventSystemLocator.eventSystem)
			{
				selectable = null;
			}
		}
		return selectable;
	}

	public override Selectable FindSelectableOnDown()
	{
		return FilterSelectable(((Selectable)this).FindSelectableOnDown());
	}

	public override Selectable FindSelectableOnLeft()
	{
		return FilterSelectable(((Selectable)this).FindSelectableOnLeft());
	}

	public override Selectable FindSelectableOnRight()
	{
		return FilterSelectable(((Selectable)this).FindSelectableOnRight());
	}

	public override Selectable FindSelectableOnUp()
	{
		return FilterSelectable(((Selectable)this).FindSelectableOnUp());
	}

	public bool InputModuleIsAllowed(BaseInputModule inputModule)
	{
		if (allowAllEventSystems)
		{
			return true;
		}
		EventSystem val = (EventSystem)(object)eventSystem;
		if (Object.op_Implicit((Object)(object)val))
		{
			return (Object)(object)inputModule == (Object)(object)val.currentInputModule;
		}
		return false;
	}

	private void AttemptSelection(PointerEventData eventData)
	{
		if (Object.op_Implicit((Object)(object)eventSystem) && (Object)(object)((EventSystem)eventSystem).currentInputModule == (Object)(object)((BaseEventData)eventData).currentInputModule)
		{
			((EventSystem)eventSystem).SetSelectedGameObject(((Component)this).gameObject, (BaseEventData)(object)eventData);
		}
	}

	public override void OnPointerEnter(PointerEventData eventData)
	{
		if (InputModuleIsAllowed(((BaseEventData)eventData).currentInputModule))
		{
			isPointerInside = true;
			((Selectable)this).OnPointerEnter(eventData);
			AttemptSelection(eventData);
		}
	}

	public override void OnPointerExit(PointerEventData eventData)
	{
		if (InputModuleIsAllowed(((BaseEventData)eventData).currentInputModule))
		{
			if (Object.op_Implicit((Object)(object)eventSystem) && (Object)(object)((Component)this).gameObject == (Object)(object)((EventSystem)eventSystem).currentSelectedGameObject)
			{
				((Behaviour)this).enabled = false;
				((Behaviour)this).enabled = true;
			}
			isPointerInside = false;
			((Selectable)this).OnPointerExit(eventData);
		}
	}

	public override void OnPointerClick(PointerEventData eventData)
	{
		if (InputModuleIsAllowed(((BaseEventData)eventData).currentInputModule))
		{
			((TMP_Dropdown)this).OnPointerClick(eventData);
		}
	}

	public override void OnPointerUp(PointerEventData eventData)
	{
		if (InputModuleIsAllowed(((BaseEventData)eventData).currentInputModule))
		{
			((Selectable)this).OnPointerUp(eventData);
		}
	}

	public override void OnPointerDown(PointerEventData eventData)
	{
		//IL_0018: Unknown result type (might be due to invalid IL or missing references)
		//IL_001d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0020: Unknown result type (might be due to invalid IL or missing references)
		if (!InputModuleIsAllowed(((BaseEventData)eventData).currentInputModule))
		{
			return;
		}
		if (((Selectable)this).IsInteractable())
		{
			Navigation navigation = ((Selectable)this).navigation;
			if ((int)((Navigation)(ref navigation)).mode != 0)
			{
				AttemptSelection(eventData);
			}
		}
		((Selectable)this).OnPointerDown(eventData);
	}

	protected override void OnDisable()
	{
		((TMP_Dropdown)this).OnDisable();
		isPointerInside = false;
	}

	protected override void DoStateTransition(SelectionState state, bool instant)
	{
		//IL_0001: Unknown result type (might be due to invalid IL or missing references)
		//IL_0009: Unknown result type (might be due to invalid IL or missing references)
		//IL_000e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0011: Unknown result type (might be due to invalid IL or missing references)
		//IL_0013: Invalid comparison between Unknown and I4
		//IL_002b: Unknown result type (might be due to invalid IL or missing references)
		//IL_002c: Unknown result type (might be due to invalid IL or missing references)
		((Selectable)this).DoStateTransition(state, instant);
		if (previousState != state)
		{
			if ((int)state == 1)
			{
				Util.PlaySound("Play_UI_menuHover", ((Component)RoR2Application.instance).gameObject);
			}
			previousState = state;
		}
	}
}
