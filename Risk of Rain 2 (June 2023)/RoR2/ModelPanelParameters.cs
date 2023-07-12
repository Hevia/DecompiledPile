using UnityEngine;

namespace RoR2;

public class ModelPanelParameters : MonoBehaviour
{
	public Transform focusPointTransform;

	public Transform cameraPositionTransform;

	public Quaternion modelRotation = Quaternion.identity;

	public float minDistance = 1f;

	public float maxDistance = 10f;

	public Vector3 cameraDirection => focusPointTransform.position - cameraPositionTransform.position;

	public void OnDrawGizmos()
	{
		//IL_000d: Unknown result type (might be due to invalid IL or missing references)
		//IL_001d: Unknown result type (might be due to invalid IL or missing references)
		//IL_002c: Unknown result type (might be due to invalid IL or missing references)
		//IL_003c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0052: Unknown result type (might be due to invalid IL or missing references)
		//IL_006f: Unknown result type (might be due to invalid IL or missing references)
		//IL_007f: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ae: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b9: Unknown result type (might be due to invalid IL or missing references)
		if (Object.op_Implicit((Object)(object)focusPointTransform))
		{
			Gizmos.color = Color.yellow;
			Gizmos.DrawWireSphere(focusPointTransform.position, 0.1f);
			Gizmos.color = Color.gray;
			Gizmos.DrawWireSphere(focusPointTransform.position, minDistance);
			Gizmos.DrawWireSphere(focusPointTransform.position, maxDistance);
		}
		if (Object.op_Implicit((Object)(object)cameraPositionTransform))
		{
			Gizmos.color = Color.yellow;
			Gizmos.DrawWireSphere(cameraPositionTransform.position, 0.2f);
		}
		if (Object.op_Implicit((Object)(object)focusPointTransform) && Object.op_Implicit((Object)(object)cameraPositionTransform))
		{
			Gizmos.DrawLine(focusPointTransform.position, cameraPositionTransform.position);
		}
	}
}
