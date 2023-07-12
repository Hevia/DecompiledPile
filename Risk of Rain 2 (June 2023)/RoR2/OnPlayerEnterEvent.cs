using UnityEngine;
using UnityEngine.Events;
using UnityEngine.Networking;

namespace RoR2;

public class OnPlayerEnterEvent : MonoBehaviour
{
	public bool serverOnly;

	public UnityEvent action;

	private bool calledAction;

	private void OnTriggerEnter(Collider other)
	{
		if ((serverOnly && !NetworkServer.active) || calledAction)
		{
			return;
		}
		CharacterBody component = ((Component)other).GetComponent<CharacterBody>();
		if (Object.op_Implicit((Object)(object)component) && component.isPlayerControlled)
		{
			Debug.LogFormat("OnPlayerEnterEvent called on {0}", new object[1] { ((Object)((Component)this).gameObject).name });
			calledAction = true;
			UnityEvent obj = action;
			if (obj != null)
			{
				obj.Invoke();
			}
		}
	}
}
