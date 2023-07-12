using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
public class SetAngularVelocity : MonoBehaviour
{
	public Vector3 angularVelocity;

	private Rigidbody rigidBody;

	private void Start()
	{
		rigidBody = ((Component)this).GetComponent<Rigidbody>();
	}

	private void FixedUpdate()
	{
		//IL_0023: Unknown result type (might be due to invalid IL or missing references)
		//IL_0028: Unknown result type (might be due to invalid IL or missing references)
		rigidBody.maxAngularVelocity = ((Vector3)(ref angularVelocity)).magnitude;
		rigidBody.angularVelocity = ((Component)this).transform.TransformVector(angularVelocity);
	}
}
