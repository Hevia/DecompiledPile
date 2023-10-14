using System;
using RoR2;
using RoR2.Projectile;
using UnityEngine;

namespace Rorschach;

internal class RorschachHookBehaviour : MonoBehaviour
{
	private ProjectileStickOnImpact stickComponent;

	private ProjectileController controllerComponent;

	private EntityStateMachine stateMachine;

	[SerializeField]
	public GameObject owner;

	[SerializeField]
	public GameObject ghost;

	[SerializeField]
	public ProjectileController controller;

	[SerializeField]
	public Transform muzzle;

	[SerializeField]
	public LineRenderer line;

	[SerializeField]
	public Transform start;

	private void Start()
	{
		stickComponent = ((Component)this).GetComponent<ProjectileStickOnImpact>();
		controllerComponent = ((Component)this).GetComponent<ProjectileController>();
		if (Object.op_Implicit((Object)(object)controllerComponent) && Object.op_Implicit((Object)(object)controllerComponent.owner))
		{
			stateMachine = Array.Find(controllerComponent.owner.GetComponents<EntityStateMachine>(), (EntityStateMachine element) => element.customName == "Hook");
		}
	}

	private void FixedUpdate()
	{
		//IL_0146: Unknown result type (might be due to invalid IL or missing references)
		//IL_015e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0269: Unknown result type (might be due to invalid IL or missing references)
		//IL_01f7: Unknown result type (might be due to invalid IL or missing references)
		if (!Object.op_Implicit((Object)(object)owner))
		{
			controller = ((Component)this).GetComponent<ProjectileController>();
			if (Object.op_Implicit((Object)(object)controller) && Object.op_Implicit((Object)(object)controller.Networkowner))
			{
				if (Object.op_Implicit((Object)(object)controller.ghost))
				{
					ghost = ((Component)controller.ghost).gameObject;
					line = ghost.GetComponent<LineRenderer>();
					start = ghost.transform.GetChild(1);
				}
				owner = controller.owner;
				ModelLocator component = owner.GetComponent<ModelLocator>();
				if (Object.op_Implicit((Object)(object)component) && Object.op_Implicit((Object)(object)component.modelTransform))
				{
					ChildLocator component2 = ((Component)component.modelTransform).GetComponent<ChildLocator>();
					if (Object.op_Implicit((Object)(object)component2))
					{
						muzzle = component2.FindChild("gunMuzzle");
					}
				}
			}
		}
		if (Object.op_Implicit((Object)(object)muzzle) && Object.op_Implicit((Object)(object)line) && Object.op_Implicit((Object)(object)start))
		{
			line.SetPosition(0, start.position);
			line.SetPosition(1, muzzle.position);
		}
		if (!Object.op_Implicit((Object)(object)stickComponent) || !stickComponent.stuck || !Object.op_Implicit((Object)(object)stickComponent.victim))
		{
			return;
		}
		CharacterBody component3 = stickComponent.victim.GetComponent<CharacterBody>();
		if (Object.op_Implicit((Object)(object)component3))
		{
			if (Object.op_Implicit((Object)(object)stateMachine) && stateMachine.state is Utility utility)
			{
				utility.HookCollision(entityCollision: true, ((Component)this).transform.position, component3.healthComponent);
			}
			SetStateOnHurt component4 = ((Component)component3).GetComponent<SetStateOnHurt>();
			if (Object.op_Implicit((Object)(object)component4))
			{
				component4.SetStun(1.5f);
			}
		}
		else if (Object.op_Implicit((Object)(object)stateMachine) && stateMachine.state is Utility utility2)
		{
			utility2.HookCollision(entityCollision: false, ((Component)this).transform.position);
		}
	}

	private void OnDisable()
	{
		if (Object.op_Implicit((Object)(object)stateMachine) && stateMachine.state is Utility utility)
		{
			utility.NoCollision();
		}
	}
}
