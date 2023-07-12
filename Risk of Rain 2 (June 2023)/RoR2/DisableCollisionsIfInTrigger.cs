using UnityEngine;

namespace RoR2;

[RequireComponent(typeof(SphereCollider))]
public class DisableCollisionsIfInTrigger : MonoBehaviour
{
	public Collider colliderToIgnore;

	private SphereCollider trigger;

	public void Awake()
	{
		trigger = ((Component)this).GetComponent<SphereCollider>();
	}

	private void OnTriggerEnter(Collider other)
	{
		//IL_0016: Unknown result type (might be due to invalid IL or missing references)
		//IL_001b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0022: Unknown result type (might be due to invalid IL or missing references)
		//IL_0027: Unknown result type (might be due to invalid IL or missing references)
		//IL_0041: Unknown result type (might be due to invalid IL or missing references)
		//IL_0054: Unknown result type (might be due to invalid IL or missing references)
		//IL_0067: Unknown result type (might be due to invalid IL or missing references)
		//IL_007b: Unknown result type (might be due to invalid IL or missing references)
		//IL_007c: Unknown result type (might be due to invalid IL or missing references)
		//IL_007d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0082: Unknown result type (might be due to invalid IL or missing references)
		if (Object.op_Implicit((Object)(object)trigger))
		{
			Vector3 position = ((Component)this).transform.position;
			Vector3 position2 = ((Component)other).transform.position;
			float num = trigger.radius * Mathf.Max(new float[3]
			{
				((Component)this).transform.lossyScale.x,
				((Component)this).transform.lossyScale.y,
				((Component)this).transform.lossyScale.z
			});
			float num2 = num * num;
			Vector3 val = position - position2;
			if (((Vector3)(ref val)).sqrMagnitude < num2)
			{
				Physics.IgnoreCollision(colliderToIgnore, other);
			}
		}
	}
}
