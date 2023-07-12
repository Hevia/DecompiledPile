using RoR2;
using UnityEngine;
using UnityEngine.Networking;

namespace EntityStates.Toolbot;

public class DroneProjectileHover : BaseState
{
	[SerializeField]
	public float duration;

	[SerializeField]
	public int pulseCount = 3;

	[SerializeField]
	public float pulseRadius = 7f;

	protected TeamFilter teamFilter;

	protected float interval;

	protected int pulses;

	public override void OnEnter()
	{
		//IL_0019: Unknown result type (might be due to invalid IL or missing references)
		base.OnEnter();
		if (Object.op_Implicit((Object)(object)base.rigidbody))
		{
			base.rigidbody.velocity = Vector3.zero;
			base.rigidbody.useGravity = false;
		}
		if (NetworkServer.active && Object.op_Implicit((Object)(object)base.projectileController))
		{
			teamFilter = base.projectileController.teamFilter;
		}
		interval = duration / (float)(pulseCount + 1);
	}

	public override void FixedUpdate()
	{
		base.FixedUpdate();
		if (NetworkServer.active)
		{
			if (base.age >= duration)
			{
				EntityState.Destroy((Object)(object)base.gameObject);
			}
			else if (base.age >= interval * (float)(pulses + 1))
			{
				pulses++;
				Pulse();
			}
		}
	}

	protected virtual void Pulse()
	{
	}
}
