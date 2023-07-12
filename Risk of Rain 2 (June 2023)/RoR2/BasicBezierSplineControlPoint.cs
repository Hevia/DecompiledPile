using UnityEngine;

namespace RoR2;

public class BasicBezierSplineControlPoint : MonoBehaviour
{
	public Vector3 forwardVelocity = Vector3.forward;

	public Vector3 backwardVelocity = Vector3.back;

	private void OnDrawGizmos()
	{
		//IL_0000: Unknown result type (might be due to invalid IL or missing references)
		//IL_000a: Unknown result type (might be due to invalid IL or missing references)
		//IL_000f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0018: Unknown result type (might be due to invalid IL or missing references)
		//IL_0023: Unknown result type (might be due to invalid IL or missing references)
		//IL_0028: Unknown result type (might be due to invalid IL or missing references)
		//IL_0032: Unknown result type (might be due to invalid IL or missing references)
		//IL_0038: Unknown result type (might be due to invalid IL or missing references)
		//IL_003e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0048: Unknown result type (might be due to invalid IL or missing references)
		//IL_004e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0058: Unknown result type (might be due to invalid IL or missing references)
		Gizmos.color = Color.yellow;
		Matrix4x4 identity = Matrix4x4.identity;
		((Matrix4x4)(ref identity)).SetTRS(((Component)this).transform.position, ((Component)this).transform.rotation, Vector3.one);
		Gizmos.matrix = identity;
		Gizmos.DrawRay(Vector3.zero, forwardVelocity);
		Gizmos.DrawRay(Vector3.zero, backwardVelocity);
		Gizmos.DrawFrustum(Vector3.zero, 60f, -0.2f, 0f, 1f);
	}
}
