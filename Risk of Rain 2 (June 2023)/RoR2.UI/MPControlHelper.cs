using System;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace RoR2.UI;

public struct MPControlHelper
{
	private readonly MPEventSystemLocator eventSystemLocator;

	private readonly Selectable owner;

	private readonly GameObject gameObject;

	private readonly bool allowAllEventSystems;

	private readonly bool disablePointerClick;

	public MPEventSystem eventSystem => eventSystemLocator.eventSystem;

	public MPControlHelper(Selectable owner, MPEventSystemLocator eventSystemLocator, bool allowAllEventSystems, bool disablePointerClick)
	{
		this.owner = owner;
		this.eventSystemLocator = eventSystemLocator;
		gameObject = ((Component)this.owner).gameObject;
		this.allowAllEventSystems = allowAllEventSystems;
		this.disablePointerClick = disablePointerClick;
	}

	public Selectable FilterSelectable(Selectable selectable)
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
			((EventSystem)eventSystem).SetSelectedGameObject(gameObject, (BaseEventData)(object)eventData);
		}
	}

	public Selectable FindSelectableOnDown(Func<Selectable> baseMethod)
	{
		return FilterSelectable(baseMethod());
	}

	public Selectable FindSelectableOnLeft(Func<Selectable> baseMethod)
	{
		return FilterSelectable(baseMethod());
	}

	public Selectable FindSelectableOnRight(Func<Selectable> baseMethod)
	{
		return FilterSelectable(baseMethod());
	}

	public Selectable FindSelectableOnUp(Func<Selectable> baseMethod)
	{
		return FilterSelectable(baseMethod());
	}

	public void OnPointerEnter(PointerEventData eventData, Action<PointerEventData> baseMethod)
	{
		if (InputModuleIsAllowed(((BaseEventData)eventData).currentInputModule))
		{
			baseMethod(eventData);
			AttemptSelection(eventData);
		}
	}

	public void OnPointerExit(PointerEventData eventData, Action<PointerEventData> baseMethod)
	{
		if (InputModuleIsAllowed(((BaseEventData)eventData).currentInputModule))
		{
			if (Object.op_Implicit((Object)(object)eventSystem) && (Object)(object)((Component)owner).gameObject == (Object)(object)((EventSystem)eventSystem).currentSelectedGameObject)
			{
				((Behaviour)owner).enabled = false;
				((Behaviour)owner).enabled = true;
			}
			baseMethod(eventData);
		}
	}

	public void OnPointerClick(PointerEventData eventData, Action<PointerEventData> baseMethod)
	{
		if (InputModuleIsAllowed(((BaseEventData)eventData).currentInputModule) && !disablePointerClick)
		{
			baseMethod(eventData);
		}
	}

	public void OnPointerUp(PointerEventData eventData, Action<PointerEventData> baseMethod, ref bool inPointerUp)
	{
		if (InputModuleIsAllowed(((BaseEventData)eventData).currentInputModule) && !disablePointerClick)
		{
			inPointerUp = true;
			baseMethod(eventData);
			inPointerUp = false;
		}
	}

	public void OnPointerDown(PointerEventData eventData, Action<PointerEventData> baseMethod, ref bool inPointerUp)
	{
		//IL_002a: Unknown result type (might be due to invalid IL or missing references)
		//IL_002f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0032: Unknown result type (might be due to invalid IL or missing references)
		if (!InputModuleIsAllowed(((BaseEventData)eventData).currentInputModule) || disablePointerClick)
		{
			return;
		}
		if (owner.IsInteractable())
		{
			Navigation navigation = owner.navigation;
			if ((int)((Navigation)(ref navigation)).mode != 0)
			{
				AttemptSelection(eventData);
			}
		}
		baseMethod(eventData);
	}

	public void OnSubmit(BaseEventData eventData, Action<BaseEventData> baseMethod, bool submitOnPointerUp, ref bool inPointerUp)
	{
		if (!submitOnPointerUp || inPointerUp)
		{
			baseMethod(eventData);
		}
	}
}
