using UnityEngine;

namespace RoR2;

public class ExplodeRigidbodiesOnStart : MonoBehaviour
{
	public Rigidbody[] bodies;

	public float force;

	public float explosionRadius;

	private void Start()
	{
		//IL_0006: Unknown result type (might be due to invalid IL or missing references)
		//IL_000b: Unknown result type (might be due to invalid IL or missing references)
		//IL_001e: Unknown result type (might be due to invalid IL or missing references)
		Vector3 position = ((Component)this).transform.position;
		for (int i = 0; i < bodies.Length; i++)
		{
			bodies[i].AddExplosionForce(force, position, explosionRadius);
		}
	}
}
