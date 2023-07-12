using EntityStates.Engi.Mine;
using RoR2.Projectile;
using UnityEngine;

namespace RoR2;

public class EngiMineAnimator : MonoBehaviour
{
	private Transform projectileTransform;

	public Animator animator;

	private EntityStateMachine armingStateMachine;

	private void Start()
	{
		ProjectileGhostController component = ((Component)this).GetComponent<ProjectileGhostController>();
		if (Object.op_Implicit((Object)(object)component))
		{
			projectileTransform = component.authorityTransform;
			if (Object.op_Implicit((Object)(object)projectileTransform))
			{
				armingStateMachine = EntityStateMachine.FindByCustomName(((Component)projectileTransform).gameObject, "Arming");
			}
		}
	}

	private bool IsArmed()
	{
		return ((armingStateMachine?.state as BaseMineArmingState)?.damageScale ?? 0f) > 1f;
	}

	private void Update()
	{
		if (IsArmed())
		{
			animator.SetTrigger("Arming");
		}
	}
}
