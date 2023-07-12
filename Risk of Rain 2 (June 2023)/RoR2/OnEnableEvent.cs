using UnityEngine;
using UnityEngine.Events;

namespace RoR2;

public class OnEnableEvent : MonoBehaviour
{
	public UnityEvent action;

	private void OnEnable()
	{
		UnityEvent obj = action;
		if (obj != null)
		{
			obj.Invoke();
		}
	}
}
