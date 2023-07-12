using UnityEngine;
using UnityEngine.Networking;

namespace RoR2;

public class GravitatePickup : MonoBehaviour
{
	private Transform gravitateTarget;

	[Tooltip("The rigidbody to set the velocity of.")]
	public Rigidbody rigidbody;

	[Tooltip("The TeamFilter which controls which team can activate this trigger.")]
	public TeamFilter teamFilter;

	public float acceleration;

	public float maxSpeed;

	public bool gravitateAtFullHealth = true;

	private void Start()
	{
	}

	private void OnTriggerEnter(Collider other)
	{
		if (NetworkServer.active && !Object.op_Implicit((Object)(object)gravitateTarget) && teamFilter.teamIndex != TeamIndex.None)
		{
			HealthComponent component = ((Component)other).gameObject.GetComponent<HealthComponent>();
			if (TeamComponent.GetObjectTeam(((Component)other).gameObject) == teamFilter.teamIndex && (gravitateAtFullHealth || !Object.op_Implicit((Object)(object)component) || component.health < component.fullHealth))
			{
				gravitateTarget = ((Component)other).gameObject.transform;
			}
		}
	}

	private void FixedUpdate()
	{
		//IL_0019: Unknown result type (might be due to invalid IL or missing references)
		//IL_0029: Unknown result type (might be due to invalid IL or missing references)
		//IL_0034: Unknown result type (might be due to invalid IL or missing references)
		//IL_0039: Unknown result type (might be due to invalid IL or missing references)
		//IL_003e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0041: Unknown result type (might be due to invalid IL or missing references)
		//IL_004c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0057: Unknown result type (might be due to invalid IL or missing references)
		if (Object.op_Implicit((Object)(object)gravitateTarget))
		{
			Rigidbody obj = rigidbody;
			Vector3 velocity = rigidbody.velocity;
			Vector3 val = ((Component)gravitateTarget).transform.position - ((Component)this).transform.position;
			obj.velocity = Vector3.MoveTowards(velocity, ((Vector3)(ref val)).normalized * maxSpeed, acceleration);
		}
	}
}
