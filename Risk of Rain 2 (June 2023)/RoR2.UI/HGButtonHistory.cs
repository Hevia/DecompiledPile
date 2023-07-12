using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace RoR2.UI;

[RequireComponent(typeof(MPEventSystemLocator))]
public class HGButtonHistory : MonoBehaviour
{
	public UILayerKey requiredTopLayer;

	public GameObject lastRememberedGameObject;

	private MPEventSystemLocator eventSystemLocator;

	private bool topLayerWasOn;

	protected MPEventSystem eventSystem => eventSystemLocator?.eventSystem;

	protected void Awake()
	{
		eventSystemLocator = ((Component)this).GetComponent<MPEventSystemLocator>();
	}

	private void OnEnable()
	{
		SelectRememberedButton();
	}

	private void Update()
	{
		//IL_0081: Unknown result type (might be due to invalid IL or missing references)
		//IL_0086: Unknown result type (might be due to invalid IL or missing references)
		//IL_0089: Unknown result type (might be due to invalid IL or missing references)
		bool flag = !Object.op_Implicit((Object)(object)requiredTopLayer) || requiredTopLayer.representsTopLayer;
		if (flag && !topLayerWasOn)
		{
			SelectRememberedButton();
		}
		topLayerWasOn = flag;
		MPEventSystem mPEventSystem = eventSystem;
		if (!Object.op_Implicit((Object)(object)((mPEventSystem != null) ? ((EventSystem)mPEventSystem).currentSelectedGameObject : null)) || !topLayerWasOn)
		{
			return;
		}
		MPEventSystem mPEventSystem2 = eventSystem;
		GameObject val = ((mPEventSystem2 != null) ? ((EventSystem)mPEventSystem2).currentSelectedGameObject : null);
		if (!Object.op_Implicit((Object)(object)val))
		{
			return;
		}
		MPButton component = val.GetComponent<MPButton>();
		if (Object.op_Implicit((Object)(object)component))
		{
			Navigation navigation = ((Selectable)component).navigation;
			if ((int)((Navigation)(ref navigation)).mode != 0)
			{
				MPEventSystem mPEventSystem3 = eventSystem;
				lastRememberedGameObject = ((mPEventSystem3 != null) ? ((EventSystem)mPEventSystem3).currentSelectedGameObject : null);
			}
		}
	}

	private void SelectRememberedButton()
	{
		if (!Object.op_Implicit((Object)(object)lastRememberedGameObject))
		{
			return;
		}
		MPEventSystem mPEventSystem = eventSystem;
		if (mPEventSystem != null && mPEventSystem.currentInputSource == MPEventSystem.InputSource.Gamepad)
		{
			MPButton component = lastRememberedGameObject.GetComponent<MPButton>();
			if (Object.op_Implicit((Object)(object)component))
			{
				Debug.LogFormat("Applying button history ({0})", new object[1] { lastRememberedGameObject });
				((Selectable)component).Select();
				((Selectable)component).OnSelect((BaseEventData)null);
			}
		}
	}
}
