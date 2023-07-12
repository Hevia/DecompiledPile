using UnityEngine;
using UnityEngine.Events;

namespace RoR2.EntityLogic;

public class BoolEventSplitter : MonoBehaviour
{
	public UnityEvent trueEvent;

	public UnityEvent falseEvent;

	public void CallEvent(bool value)
	{
		UnityEvent obj = (value ? trueEvent : falseEvent);
		if (obj != null)
		{
			obj.Invoke();
		}
	}
}
