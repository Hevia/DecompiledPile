using UnityEngine;
using UnityEngine.Events;
using UnityEngine.Networking;

namespace RoR2;

public class RunEventFlagResponse : MonoBehaviour
{
	public string flagName;

	public UnityEvent onAwakeWithFlagSetServer;

	public UnityEvent onAwakeWithFlagUnsetServer;

	private void Awake()
	{
		if (!NetworkServer.active)
		{
			return;
		}
		if (Object.op_Implicit((Object)(object)Run.instance))
		{
			UnityEvent obj = (Run.instance.GetEventFlag(flagName) ? onAwakeWithFlagSetServer : onAwakeWithFlagUnsetServer);
			if (obj != null)
			{
				obj.Invoke();
			}
		}
		else
		{
			Debug.LogErrorFormat("Cannot handle run event flag response {0}: No run exists.", new object[1] { ((Object)((Component)this).gameObject).name });
		}
	}
}
