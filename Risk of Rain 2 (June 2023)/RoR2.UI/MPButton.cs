using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;
using UnityEngine.Serialization;
using UnityEngine.UI;

namespace RoR2.UI;

[RequireComponent(typeof(MPEventSystemLocator))]
public class MPButton : Button
{
	private MPEventSystemLocator eventSystemLocator;

	public bool allowAllEventSystems;

	[FormerlySerializedAs("pointerClickOnly")]
	public bool submitOnPointerUp;

	public bool disablePointerClick;

	public bool disableGamepadClick;

	public UILayerKey requiredTopLayer;

	public UnityEvent onFindSelectableLeft;

	public UnityEvent onFindSelectableRight;

	public UnityEvent onSelect;

	public UnityEvent onDeselect;

	public bool defaultFallbackButton;

	private bool inPointerUp;

	public MPEventSystem eventSystem => eventSystemLocator.eventSystem;

	protected override void Awake()
	{
		((Selectable)this).Awake();
		eventSystemLocator = ((Component)this).GetComponent<MPEventSystemLocator>();
	}

	public void Update()
	{
		if (Object.op_Implicit((Object)(object)eventSystem) && eventSystem.player != null)
		{
			if (!disableGamepadClick && eventSystem.player.GetButtonDown(14) && (Object)(object)((EventSystem)eventSystem).currentSelectedGameObject == (Object)(object)((Component)this).gameObject)
			{
				InvokeClick();
			}
			if (defaultFallbackButton && eventSystem.currentInputSource == MPEventSystem.InputSource.Gamepad && (Object)(object)((EventSystem)eventSystem).currentSelectedGameObject == (Object)null && CanBeSelected())
			{
				Debug.LogFormat("Setting {0} as fallback button", new object[1] { ((Component)this).gameObject });
				((Selectable)this).Select();
			}
		}
	}

	public void InvokeClick()
	{
		ButtonClickedEvent onClick = ((Button)this).onClick;
		if (onClick != null)
		{
			((UnityEvent)onClick).Invoke();
		}
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
			MPButton mPButton = ((selectable != null) ? ((Component)selectable).GetComponent<MPButton>() : null);
			if (Object.op_Implicit((Object)(object)mPButton) && !mPButton.CanBeSelected())
			{
				selectable = null;
			}
		}
		return selectable;
	}

	public bool CanBeSelected()
	{
		if (((Component)this).gameObject.activeInHierarchy)
		{
			if (Object.op_Implicit((Object)(object)requiredTopLayer))
			{
				return requiredTopLayer.representsTopLayer;
			}
			return true;
		}
		return false;
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

	public override void OnSelect(BaseEventData eventData)
	{
		UnityEvent obj = onSelect;
		if (obj != null)
		{
			obj.Invoke();
		}
		((Selectable)this).OnSelect(eventData);
	}

	public override void OnDeselect(BaseEventData eventData)
	{
		UnityEvent obj = onDeselect;
		if (obj != null)
		{
			obj.Invoke();
		}
		((Selectable)this).OnDeselect(eventData);
	}

	public override Selectable FindSelectableOnDown()
	{
		return FilterSelectable(((Selectable)this).FindSelectableOnDown());
	}

	public override Selectable FindSelectableOnLeft()
	{
		UnityEvent obj = onFindSelectableLeft;
		if (obj != null)
		{
			obj.Invoke();
		}
		return FilterSelectable(((Selectable)this).FindSelectableOnLeft());
	}

	public override Selectable FindSelectableOnRight()
	{
		UnityEvent obj = onFindSelectableRight;
		if (obj != null)
		{
			obj.Invoke();
		}
		return FilterSelectable(((Selectable)this).FindSelectableOnRight());
	}

	public override Selectable FindSelectableOnUp()
	{
		return FilterSelectable(((Selectable)this).FindSelectableOnUp());
	}

	public override void OnPointerEnter(PointerEventData eventData)
	{
		if (InputModuleIsAllowed(((BaseEventData)eventData).currentInputModule))
		{
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
			((Selectable)this).OnPointerExit(eventData);
		}
	}

	public override void OnPointerClick(PointerEventData eventData)
	{
		if (InputModuleIsAllowed(((BaseEventData)eventData).currentInputModule) && !disablePointerClick)
		{
			((Button)this).OnPointerClick(eventData);
		}
	}

	public override void OnPointerUp(PointerEventData eventData)
	{
		if (InputModuleIsAllowed(((BaseEventData)eventData).currentInputModule) && !disablePointerClick)
		{
			inPointerUp = true;
			((Selectable)this).OnPointerUp(eventData);
			inPointerUp = false;
		}
	}

	public override void OnPointerDown(PointerEventData eventData)
	{
		//IL_0020: Unknown result type (might be due to invalid IL or missing references)
		//IL_0025: Unknown result type (might be due to invalid IL or missing references)
		//IL_0028: Unknown result type (might be due to invalid IL or missing references)
		if (!InputModuleIsAllowed(((BaseEventData)eventData).currentInputModule) || disablePointerClick)
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

	public override void OnSubmit(BaseEventData eventData)
	{
		if (!submitOnPointerUp || inPointerUp)
		{
			((Button)this).OnSubmit(eventData);
		}
	}
}
