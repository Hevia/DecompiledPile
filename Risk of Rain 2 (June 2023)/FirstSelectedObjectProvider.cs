using System.Collections.Generic;
using RoR2.UI;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class FirstSelectedObjectProvider : MonoBehaviour
{
	public GameObject firstSelectedObject;

	public GameObject[] fallBackFirstSelectedObjects;

	private GameObject lastSelected;

	public GameObject[] enforceCurrentSelectionIsInList;

	public bool takeAbsolutePriority;

	public static FirstSelectedObjectProvider priorityHolder;

	private void OnEnable()
	{
		if (takeAbsolutePriority)
		{
			priorityHolder = this;
		}
	}

	private void OnDisable()
	{
		if (takeAbsolutePriority && (Object)(object)priorityHolder == (Object)(object)this)
		{
			priorityHolder = null;
		}
	}

	private GameObject getInteractableFirstSelectedObject()
	{
		if (Object.op_Implicit((Object)(object)firstSelectedObject) && firstSelectedObject.GetComponent<Selectable>().interactable && firstSelectedObject.activeInHierarchy)
		{
			return firstSelectedObject;
		}
		if (fallBackFirstSelectedObjects == null)
		{
			return null;
		}
		for (int i = 0; i < fallBackFirstSelectedObjects.Length; i++)
		{
			if (fallBackFirstSelectedObjects[i].GetComponent<Selectable>().interactable && fallBackFirstSelectedObjects[i].activeInHierarchy)
			{
				return fallBackFirstSelectedObjects[i];
			}
		}
		return null;
	}

	public void EnsureSelectedObject()
	{
		if ((Object)(object)priorityHolder != (Object)null && (Object)(object)priorityHolder != (Object)(object)this)
		{
			return;
		}
		GameObject interactableFirstSelectedObject = getInteractableFirstSelectedObject();
		if (!((Object)(object)interactableFirstSelectedObject != (Object)null))
		{
			return;
		}
		MPEventSystemLocator component = interactableFirstSelectedObject.GetComponent<MPEventSystemLocator>();
		if (!Object.op_Implicit((Object)(object)component))
		{
			return;
		}
		MPEventSystem eventSystem = component.eventSystem;
		if (!Object.op_Implicit((Object)(object)eventSystem))
		{
			return;
		}
		GameObject currentSelectedGameObject = ((EventSystem)eventSystem).currentSelectedGameObject;
		if ((Object)(object)currentSelectedGameObject == (Object)null)
		{
			if ((Object)(object)lastSelected != (Object)null)
			{
				((EventSystem)eventSystem).SetSelectedGameObject(lastSelected);
			}
			else
			{
				if ((Object)(object)((EventSystem)eventSystem).firstSelectedGameObject == (Object)null)
				{
					((EventSystem)eventSystem).firstSelectedGameObject = interactableFirstSelectedObject;
				}
				Debug.Log((object)("FSOP B: SetSelectedGameObject => " + ((Object)interactableFirstSelectedObject).name));
				((EventSystem)eventSystem).SetSelectedGameObject(interactableFirstSelectedObject);
			}
		}
		else
		{
			bool flag = true;
			if (enforceCurrentSelectionIsInList != null && enforceCurrentSelectionIsInList.Length != 0)
			{
				flag = false;
				GameObject[] array = enforceCurrentSelectionIsInList;
				for (int i = 0; i < array.Length; i++)
				{
					if ((Object)(object)array[i] == (Object)(object)currentSelectedGameObject)
					{
						flag = true;
					}
				}
			}
			if (!flag)
			{
				Selectable component2 = currentSelectedGameObject.GetComponent<Selectable>();
				if (Object.op_Implicit((Object)(object)component2) && (!component2.interactable || !((Behaviour)component2).isActiveAndEnabled))
				{
					((EventSystem)eventSystem).SetSelectedGameObject(interactableFirstSelectedObject);
				}
			}
		}
		if ((Object)(object)((EventSystem)component.eventSystem).currentSelectedGameObject != (Object)null)
		{
			lastSelected = ((EventSystem)component.eventSystem).currentSelectedGameObject;
		}
	}

	public void ResetLastSelected()
	{
		Debug.Log((object)"FSOP.ResetLastSelected()");
		lastSelected = null;
	}

	public void AddObject(GameObject obj, bool enforceCurrentSelection = false)
	{
		if ((Object)(object)firstSelectedObject == (Object)null)
		{
			firstSelectedObject = obj;
		}
		else if (fallBackFirstSelectedObjects == null)
		{
			fallBackFirstSelectedObjects = (GameObject[])(object)new GameObject[1];
			fallBackFirstSelectedObjects[0] = obj;
		}
		else if (fallBackFirstSelectedObjects.Length >= 0)
		{
			List<GameObject> list = new List<GameObject>(fallBackFirstSelectedObjects);
			list.Add(obj);
			fallBackFirstSelectedObjects = list.ToArray();
		}
		if (enforceCurrentSelection)
		{
			if (enforceCurrentSelectionIsInList == null)
			{
				enforceCurrentSelectionIsInList = (GameObject[])(object)new GameObject[1];
				enforceCurrentSelectionIsInList[0] = obj;
			}
			else if (enforceCurrentSelectionIsInList.Length >= 0)
			{
				List<GameObject> list2 = new List<GameObject>(enforceCurrentSelectionIsInList);
				list2.Add(obj);
				enforceCurrentSelectionIsInList = list2.ToArray();
			}
		}
	}
}
