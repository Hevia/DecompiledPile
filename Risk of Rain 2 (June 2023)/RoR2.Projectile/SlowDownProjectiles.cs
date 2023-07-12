using System.Collections.Generic;
using UnityEngine;

namespace RoR2.Projectile;

[RequireComponent(typeof(Collider))]
public class SlowDownProjectiles : MonoBehaviour
{
	private struct SlowDownProjectileInfo
	{
		public Rigidbody rb;

		public Vector3 previousVelocity;
	}

	public TeamFilter teamFilter;

	public float slowDownCoefficient;

	private List<SlowDownProjectileInfo> slowDownProjectileInfos;

	private void Start()
	{
		slowDownProjectileInfos = new List<SlowDownProjectileInfo>();
	}

	private void Update()
	{
	}

	private void OnTriggerEnter(Collider other)
	{
		//IL_0042: Unknown result type (might be due to invalid IL or missing references)
		//IL_0047: Unknown result type (might be due to invalid IL or missing references)
		TeamFilter component = ((Component)other).GetComponent<TeamFilter>();
		Rigidbody component2 = ((Component)other).GetComponent<Rigidbody>();
		if (Object.op_Implicit((Object)(object)component2) && component.teamIndex != teamFilter.teamIndex)
		{
			slowDownProjectileInfos.Add(new SlowDownProjectileInfo
			{
				rb = component2,
				previousVelocity = component2.velocity
			});
		}
	}

	private void OnTriggerExit(Collider other)
	{
		TeamFilter component = ((Component)other).GetComponent<TeamFilter>();
		Rigidbody component2 = ((Component)other).GetComponent<Rigidbody>();
		if (Object.op_Implicit((Object)(object)component2) && component.teamIndex != teamFilter.teamIndex)
		{
			RemoveFromSlowDownProjectileInfos(component2);
		}
	}

	private void RemoveFromSlowDownProjectileInfos(Rigidbody rb)
	{
		for (int i = 0; i < slowDownProjectileInfos.Count; i++)
		{
			if ((Object)(object)slowDownProjectileInfos[i].rb == (Object)(object)rb)
			{
				slowDownProjectileInfos.RemoveAt(i);
				break;
			}
		}
	}

	private void FixedUpdate()
	{
		//IL_0019: Unknown result type (might be due to invalid IL or missing references)
		//IL_001e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0029: Unknown result type (might be due to invalid IL or missing references)
		//IL_002e: Unknown result type (might be due to invalid IL or missing references)
		//IL_002f: Unknown result type (might be due to invalid IL or missing references)
		//IL_003a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0044: Unknown result type (might be due to invalid IL or missing references)
		//IL_0049: Unknown result type (might be due to invalid IL or missing references)
		//IL_0056: Unknown result type (might be due to invalid IL or missing references)
		//IL_005b: Unknown result type (might be due to invalid IL or missing references)
		for (int i = 0; i < slowDownProjectileInfos.Count; i++)
		{
			SlowDownProjectileInfo value = slowDownProjectileInfos[i];
			Rigidbody rb = value.rb;
			Vector3 previousVelocity = value.previousVelocity;
			if (Object.op_Implicit((Object)(object)rb))
			{
				rb.MovePosition(rb.position - Vector3.Lerp(previousVelocity, Vector3.zero, slowDownCoefficient) * Time.fixedDeltaTime);
				value.previousVelocity = rb.velocity;
				slowDownProjectileInfos[i] = value;
			}
			else
			{
				RemoveFromSlowDownProjectileInfos(rb);
			}
		}
	}
}
