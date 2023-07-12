using RoR2;
using RoR2.Projectile;
using UnityEngine;
using UnityEngine.Networking;

namespace EntityStates.Engi.EngiBubbleShield;

public class Undeployed : EntityState
{
	private ProjectileStickOnImpact projectileStickOnImpact;

	public override void OnEnter()
	{
		base.OnEnter();
		ProjectileController component = GetComponent<ProjectileController>();
		projectileStickOnImpact = GetComponent<ProjectileStickOnImpact>();
		if (!NetworkServer.active || !Object.op_Implicit((Object)(object)component.owner))
		{
			return;
		}
		CharacterBody component2 = component.owner.GetComponent<CharacterBody>();
		if (Object.op_Implicit((Object)(object)component2))
		{
			CharacterMaster master = component2.master;
			if (Object.op_Implicit((Object)(object)master))
			{
				master.AddDeployable(GetComponent<Deployable>(), DeployableSlot.EngiBubbleShield);
			}
		}
	}

	public override void FixedUpdate()
	{
		base.FixedUpdate();
		if (projectileStickOnImpact.stuck && NetworkServer.active)
		{
			SetNextState();
		}
	}

	protected virtual void SetNextState()
	{
		outer.SetNextState(new Deployed());
	}
}
