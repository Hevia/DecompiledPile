using RoR2;
using RoR2.Projectile;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.Networking;

namespace EntityStates.Toolbot;

public class DroneProjectileInFlight : BaseState
{
	private ProjectileImpactEventCaller impactEventCaller;

	private ProjectileSimple projectileSimple;

	private ProjectileFuse projectileFuse;

	public override void OnEnter()
	{
		//IL_0079: Unknown result type (might be due to invalid IL or missing references)
		//IL_0083: Expected O, but got Unknown
		base.OnEnter();
		if (NetworkServer.active)
		{
			impactEventCaller = GetComponent<ProjectileImpactEventCaller>();
			if (Object.op_Implicit((Object)(object)impactEventCaller))
			{
				((UnityEvent<ProjectileImpactInfo>)impactEventCaller.impactEvent).AddListener((UnityAction<ProjectileImpactInfo>)OnImpact);
			}
			projectileSimple = GetComponent<ProjectileSimple>();
			projectileFuse = GetComponent<ProjectileFuse>();
			if (Object.op_Implicit((Object)(object)projectileFuse))
			{
				projectileFuse.onFuse.AddListener(new UnityAction(OnFuse));
			}
		}
	}

	public override void OnExit()
	{
		//IL_0048: Unknown result type (might be due to invalid IL or missing references)
		//IL_0052: Expected O, but got Unknown
		if (Object.op_Implicit((Object)(object)impactEventCaller))
		{
			((UnityEvent<ProjectileImpactInfo>)impactEventCaller.impactEvent).RemoveListener((UnityAction<ProjectileImpactInfo>)OnImpact);
		}
		if (Object.op_Implicit((Object)(object)projectileFuse))
		{
			projectileFuse.onFuse.RemoveListener(new UnityAction(OnFuse));
		}
		base.OnEnter();
	}

	private void OnImpact(ProjectileImpactInfo projectileImpactInfo)
	{
		Advance();
	}

	private void OnFuse()
	{
		Advance();
	}

	private void Advance()
	{
		//IL_0057: Unknown result type (might be due to invalid IL or missing references)
		if (NetworkServer.active)
		{
			if (Object.op_Implicit((Object)(object)projectileSimple))
			{
				projectileSimple.velocity = 0f;
				((Behaviour)projectileSimple).enabled = false;
			}
			if (Object.op_Implicit((Object)(object)base.rigidbody))
			{
				base.rigidbody.velocity = new Vector3(0f, Trajectory.CalculateInitialYSpeedForFlightDuration(DroneProjectilePrepHover.duration), 0f);
			}
		}
		if (base.isAuthority)
		{
			outer.SetNextState(new DroneProjectilePrepHover());
		}
	}
}
