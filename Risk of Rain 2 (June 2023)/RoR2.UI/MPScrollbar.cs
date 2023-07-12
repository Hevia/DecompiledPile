using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace RoR2.UI;

[RequireComponent(typeof(MPEventSystemLocator))]
public class MPScrollbar : Scrollbar
{
	public bool allowAllEventSystems;

	private MPEventSystemLocator eventSystemLocator;

	private RectTransform viewPortRectTransform;

	private EventSystem eventSystem => (EventSystem)(object)eventSystemLocator.eventSystem;

	protected override void Awake()
	{
		((Selectable)this).Awake();
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

	public bool InputModuleIsAllowed(BaseInputModule inputModule)
	{
		if (allowAllEventSystems)
		{
			return true;
		}
		EventSystem val = eventSystem;
		if (Object.op_Implicit((Object)(object)val))
		{
			return (Object)(object)inputModule == (Object)(object)val.currentInputModule;
		}
		return false;
	}

	public override Selectable FindSelectableOnDown()
	{
		return FilterSelectable(((Scrollbar)this).FindSelectableOnDown());
	}

	public override Selectable FindSelectableOnLeft()
	{
		return FilterSelectable(((Scrollbar)this).FindSelectableOnLeft());
	}

	public override Selectable FindSelectableOnRight()
	{
		return FilterSelectable(((Scrollbar)this).FindSelectableOnRight());
	}

	public override Selectable FindSelectableOnUp()
	{
		return FilterSelectable(((Scrollbar)this).FindSelectableOnUp());
	}

	public override void OnPointerUp(PointerEventData eventData)
	{
		if (InputModuleIsAllowed(((BaseEventData)eventData).currentInputModule))
		{
			if (Object.op_Implicit((Object)(object)eventSystem) && (Object)(object)((Component)this).gameObject == (Object)(object)eventSystem.currentSelectedGameObject)
			{
				((Behaviour)this).enabled = false;
				((Behaviour)this).enabled = true;
			}
			((Scrollbar)this).OnPointerUp(eventData);
		}
	}

	public override void OnSelect(BaseEventData eventData)
	{
		if (InputModuleIsAllowed(eventData.currentInputModule))
		{
			((Selectable)this).OnSelect(eventData);
		}
	}

	public override void OnDeselect(BaseEventData eventData)
	{
		if (InputModuleIsAllowed(eventData.currentInputModule))
		{
			((Selectable)this).OnDeselect(eventData);
		}
	}

	public override void OnPointerEnter(PointerEventData eventData)
	{
		if (InputModuleIsAllowed(((BaseEventData)eventData).currentInputModule))
		{
			((Selectable)this).OnPointerEnter(eventData);
		}
	}

	public override void OnPointerExit(PointerEventData eventData)
	{
		if (InputModuleIsAllowed(((BaseEventData)eventData).currentInputModule))
		{
			((Selectable)this).OnPointerExit(eventData);
		}
	}

	public override void OnPointerDown(PointerEventData eventData)
	{
		//IL_0001: Unknown result type (might be due to invalid IL or missing references)
		//IL_0012: Unknown result type (might be due to invalid IL or missing references)
		//IL_0017: Unknown result type (might be due to invalid IL or missing references)
		//IL_001a: Unknown result type (might be due to invalid IL or missing references)
		if ((int)eventData.button != 0)
		{
			return;
		}
		if (((Selectable)this).IsInteractable())
		{
			Navigation navigation = ((Selectable)this).navigation;
			if ((int)((Navigation)(ref navigation)).mode != 0)
			{
				eventSystem.SetSelectedGameObject(((Component)this).gameObject, (BaseEventData)(object)eventData);
			}
		}
		((Scrollbar)this).OnPointerDown(eventData);
	}

	public override void Select()
	{
		if (!eventSystem.alreadySelecting)
		{
			eventSystem.SetSelectedGameObject(((Component)this).gameObject);
		}
	}
}
