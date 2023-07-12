using UnityEngine;

namespace RoR2;

[RequireComponent(typeof(Rigidbody))]
public class AntiGravityForce : MonoBehaviour
{
	public Rigidbody rb;

	[Tooltip("How much to oppose gravity. A value of 1 means it is unaffected by gravity.")]
	public float antiGravityCoefficient;

	private void FixedUpdate()
	{
		//IL_0006: Unknown result type (might be due to invalid IL or missing references)
		//IL_000b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0016: Unknown result type (might be due to invalid IL or missing references)
		rb.AddForce(-Physics.gravity * antiGravityCoefficient, (ForceMode)5);
	}
}
