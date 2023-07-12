using RoR2.Projectile;
using UnityEngine;
using UnityEngine.Networking;

namespace EntityStates.Engi.SpiderMine;

public class WaitForTarget : BaseSpiderMineState
{
	protected ProjectileSphereTargetFinder targetFinder { get; private set; }

	protected override bool shouldStick => true;

	public override void OnEnter()
	{
		base.OnEnter();
		if (NetworkServer.active)
		{
			targetFinder = GetComponent<ProjectileSphereTargetFinder>();
			((Behaviour)targetFinder).enabled = true;
		}
		PlayAnimation("Base", "Armed");
	}

	public override void FixedUpdate()
	{
		base.FixedUpdate();
		if (base.isAuthority)
		{
			EntityState entityState = null;
			if (!base.projectileStickOnImpact.stuck)
			{
				entityState = new WaitForStick();
			}
			else if (Object.op_Implicit((Object)(object)base.projectileTargetComponent.target))
			{
				entityState = new Unburrow();
			}
			if (entityState != null)
			{
				outer.SetNextState(entityState);
			}
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
}
