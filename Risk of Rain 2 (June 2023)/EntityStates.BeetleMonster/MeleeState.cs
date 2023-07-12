using System;
using RoR2;
using UnityEngine;

namespace EntityStates.BeetleMonster;

public class MeleeState : EntityState
{
	public static float duration = 3.5f;

	public static float damage = 10f;

	public static float forceMagnitude = 10f;

	private OverlapAttack attack;

	private Animator modelAnimator;

	public override void OnEnter()
	{
		base.OnEnter();
		modelAnimator = GetModelAnimator();
		attack = new OverlapAttack();
		attack.attacker = base.gameObject;
		attack.inflictor = base.gameObject;
		attack.teamIndex = TeamComponent.GetObjectTeam(attack.attacker);
		attack.damage = 10f;
		Transform modelTransform = GetModelTransform();
		if (Object.op_Implicit((Object)(object)modelTransform))
		{
			attack.hitBoxGroup = Array.Find(((Component)modelTransform).GetComponents<HitBoxGroup>(), (HitBoxGroup element) => element.groupName == "Melee1");
		}
		PlayCrossfade("Body", "Melee1", "Melee1.playbackRate", duration, 0.1f);
	}

	public override void FixedUpdate()
	{
		//IL_0021: Unknown result type (might be due to invalid IL or missing references)
		//IL_002e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0038: Unknown result type (might be due to invalid IL or missing references)
		//IL_003d: Unknown result type (might be due to invalid IL or missing references)
		base.FixedUpdate();
		if (base.isAuthority)
		{
			attack.forceVector = (Object.op_Implicit((Object)(object)base.characterDirection) ? (base.characterDirection.forward * forceMagnitude) : Vector3.zero);
			if (Object.op_Implicit((Object)(object)modelAnimator) && modelAnimator.GetFloat("Melee1.hitBoxActive") > 0.5f)
			{
				attack.Fire();
			}
		}
		if (base.fixedAge >= duration && base.isAuthority)
		{
			outer.SetNextStateToMain();
		}
	}

	public override InterruptPriority GetMinimumInterruptPriority()
	{
		return InterruptPriority.PrioritySkill;
	}
}
