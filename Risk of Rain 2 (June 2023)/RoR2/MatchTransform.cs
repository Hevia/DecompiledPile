using UnityEngine;

namespace RoR2;

[ExecuteAlways]
public class MatchTransform : MonoBehaviour
{
	public Transform targetTransform;

	public void LateUpdate()
	{
		UpdateNow();
	}

	[ContextMenu("Update Now")]
	public void UpdateNow()
	{
		//IL_0019: Unknown result type (might be due to invalid IL or missing references)
		//IL_0024: Unknown result type (might be due to invalid IL or missing references)
		if (Object.op_Implicit((Object)(object)targetTransform))
		{
			((Component)this).transform.SetPositionAndRotation(targetTransform.position, targetTransform.rotation);
		}
	}
}
