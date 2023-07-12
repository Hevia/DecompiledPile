using UnityEngine;

namespace RoR2;

[RequireComponent(typeof(WheelCollider))]
public class SetWheelVisuals : MonoBehaviour
{
	public Transform visualTransform;

	private WheelCollider wheelCollider;

	private void Start()
	{
		wheelCollider = ((Component)this).GetComponent<WheelCollider>();
	}

	private void FixedUpdate()
	{
		//IL_0015: Unknown result type (might be due to invalid IL or missing references)
		//IL_0021: Unknown result type (might be due to invalid IL or missing references)
		Vector3 position = default(Vector3);
		Quaternion rotation = default(Quaternion);
		wheelCollider.GetWorldPose(ref position, ref rotation);
		visualTransform.position = position;
		visualTransform.rotation = rotation;
	}
}
