using RoR2.Projectile;
using UnityEngine;
using UnityEngine.Networking;

namespace EntityStates.Engi.Mine;

public class WaitForTarget : BaseMineState
{
	private ProjectileSphereTargetFinder targetFinder;

	private ProjectileTargetComponent projectileTargetComponent;

	private ProjectileImpactExplosion projectileImpactExplosion;

	protected override bool shouldStick => true;

	public override void OnEnter()
	{
		base.OnEnter();
		projectileTargetComponent = GetComponent<ProjectileTargetComponent>();
		targetFinder = GetComponent<ProjectileSphereTargetFinder>();
		if (NetworkServer.active)
		{
			((Behaviour)targetFinder).enabled = true;
			base.armingStateMachine.SetNextState(new MineArmingWeak());
		}
	}

	public override void OnExit()
	{
		if (Object.op_Implicit((Object)(object)targetFinder))
		{
			((Behaviour)targetFinder).enabled = false;
		}
		base.OnExit();
	}

	public override void FixedUpdate()
	{
		base.FixedUpdate();
		if (NetworkServer.active && Object.op_Implicit((Object)(object)targetFinder))
		{
			if (Object.op_Implicit((Object)(object)projectileTargetComponent.target))
			{
				outer.SetNextState(new PreDetonate());
			}
			if (base.armingStateMachine?.state is BaseMineArmingState baseMineArmingState)
			{
				((Behaviour)targetFinder).enabled = baseMineArmingState.triggerRadius != 0f;
				targetFinder.lookRange = baseMineArmingState.triggerRadius;
			}
		}
	}
}
