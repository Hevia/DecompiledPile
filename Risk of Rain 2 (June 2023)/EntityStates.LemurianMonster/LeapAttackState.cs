using System;
using RoR2;
using UnityEngine;

namespace EntityStates.LemurianMonster;

public class LeapAttackState : BaseState
{
	public static float baseDuration = 3.5f;

	public static float damage = 10f;

	public static float forceMagnitude = 16f;

	private OverlapAttack attack;

	private Animator modelAnimator;

	private RootMotionAccumulator rootMotionAccumulator;

	private float duration;

	public override void OnEnter()
	{
		base.OnEnter();
		rootMotionAccumulator = GetModelRootMotionAccumulator();
		modelAnimator = GetModelAnimator();
		duration = baseDuration / attackSpeedStat;
		attack = new OverlapAttack();
		attack.attacker = base.gameObject;
		attack.inflictor = base.gameObject;
		attack.teamIndex = TeamComponent.GetObjectTeam(attack.attacker);
		attack.damage = damage;
		Transform modelTransform = GetModelTransform();
		if (Object.op_Implicit((Object)(object)modelTransform))
		{
			attack.hitBoxGroup = Array.Find(((Component)modelTransform).GetComponents<HitBoxGroup>(), (HitBoxGroup element) => element.groupName == "LeapAttack");
		}
		PlayCrossfade("Body", "LeapAttack", "LeapAttack.playbackRate", duration, 0.1f);
	}

	public override void FixedUpdate()
	{
		//IL_0019: Unknown result type (might be due to invalid IL or missing references)
		//IL_001e: Unknown result type (might be due to invalid IL or missing references)
		//IL_001f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0020: Unknown result type (might be due to invalid IL or missing references)
		//IL_0048: Unknown result type (might be due to invalid IL or missing references)
		//IL_004d: Unknown result type (might be due to invalid IL or missing references)
		//IL_004e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0053: Unknown result type (might be due to invalid IL or missing references)
		//IL_0073: Unknown result type (might be due to invalid IL or missing references)
		//IL_0080: Unknown result type (might be due to invalid IL or missing references)
		//IL_008a: Unknown result type (might be due to invalid IL or missing references)
		//IL_008f: Unknown result type (might be due to invalid IL or missing references)
		base.FixedUpdate();
		if (Object.op_Implicit((Object)(object)rootMotionAccumulator))
		{
			Vector3 val = rootMotionAccumulator.ExtractRootMotion();
			if (val != Vector3.zero && base.isAuthority && Object.op_Implicit((Object)(object)base.characterMotor))
			{
				CharacterMotor obj = base.characterMotor;
				obj.rootMotion += val;
			}
		}
		if (base.isAuthority)
		{
			attack.forceVector = (Object.op_Implicit((Object)(object)base.characterDirection) ? (base.characterDirection.forward * forceMagnitude) : Vector3.zero);
			if (Object.op_Implicit((Object)(object)modelAnimator) && modelAnimator.GetFloat("LeapAttack.hitBoxActive") > 0.5f)
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
