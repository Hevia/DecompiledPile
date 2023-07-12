using UnityEngine;
using UnityEngine.Events;

namespace RoR2.EntityLogic;

public class UpdateEvent : MonoBehaviour
{
	[SerializeField]
	[Tooltip("Don't call the action until at least this many updates have fired")]
	private int updateSkipCount;

	[Tooltip("Don't call the action after this many updates have fired.  If negative, ignore.")]
	[SerializeField]
	private int maxInvokeCount = -1;

	[SerializeField]
	private UnityEvent action;

	private int invokeCount;

	private int updateCount;

	private void Update()
	{
		if (updateSkipCount < updateCount && (maxInvokeCount < 0 || invokeCount < maxInvokeCount))
		{
			UnityEvent obj = action;
			if (obj != null)
			{
				obj.Invoke();
			}
			invokeCount++;
		}
		updateCount++;
	}
}
