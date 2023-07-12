using UnityEngine;

namespace RoR2;

public class ApplyTorqueOnStart : MonoBehaviour
{
	public Vector3 localTorque;

	public bool randomize;

	private void Start()
	{
		//IL_0013: Unknown result type (might be due to invalid IL or missing references)
		//IL_0018: Unknown result type (might be due to invalid IL or missing references)
		//IL_0023: Unknown result type (might be due to invalid IL or missing references)
		//IL_0030: Unknown result type (might be due to invalid IL or missing references)
		//IL_0048: Unknown result type (might be due to invalid IL or missing references)
		//IL_0055: Unknown result type (might be due to invalid IL or missing references)
		//IL_006d: Unknown result type (might be due to invalid IL or missing references)
		//IL_007a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0091: Unknown result type (might be due to invalid IL or missing references)
		Rigidbody component = ((Component)this).GetComponent<Rigidbody>();
		if (Object.op_Implicit((Object)(object)component))
		{
			Vector3 val = localTorque;
			if (randomize)
			{
				val.x = Random.Range((0f - val.x) / 2f, val.x / 2f);
				val.y = Random.Range((0f - val.y) / 2f, val.y / 2f);
				val.z = Random.Range((0f - val.z) / 2f, val.z / 2f);
			}
			component.AddRelativeTorque(val);
		}
	}
}
