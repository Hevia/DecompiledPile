using UnityEngine;

namespace RoR2;

public class ApplyForceOnStart : MonoBehaviour
{
	public Vector3 localForce;

	private void Start()
	{
		//IL_0011: Unknown result type (might be due to invalid IL or missing references)
		Rigidbody component = ((Component)this).GetComponent<Rigidbody>();
		if (Object.op_Implicit((Object)(object)component))
		{
			component.AddRelativeForce(localForce);
		}
	}
}
