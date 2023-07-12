using UnityEngine;

namespace RoR2;

public class AddCurvedTorque : MonoBehaviour
{
	public AnimationCurve torqueCurve;

	public Vector3 localTorqueVector;

	public float lifetime;

	public Rigidbody[] rigidbodies;

	private float stopwatch;

	private void FixedUpdate()
	{
		//IL_003a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0040: Unknown result type (might be due to invalid IL or missing references)
		stopwatch += Time.fixedDeltaTime;
		float num = torqueCurve.Evaluate(stopwatch / lifetime);
		Rigidbody[] array = rigidbodies;
		for (int i = 0; i < array.Length; i++)
		{
			array[i].AddRelativeTorque(localTorqueVector * num);
		}
	}
}
