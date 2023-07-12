using System;
using RoR2;
using UnityEngine;

namespace EntityStates.BeetleMonster;

public class HeadbuttState : BaseState
{
	public static float baseDuration = 3.5f;

	public static float damageCoefficient;

	public static float forceMagnitude = 16f;

	public static GameObject hitEffectPrefab;

	public static string attackSoundString;

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
		attack.damage = damageCoefficient * damageStat;
		attack.hitEffectPrefab = hitEffectPrefab;
		Transform modelTransform = GetModelTransform();
		if (Object.op_Implicit((Object)(object)modelTransform))
		{
			attack.hitBoxGroup = Array.Find(((Component)modelTransform).GetComponents<HitBoxGroup>(), (HitBoxGroup element) => element.groupName == "Headbutt");
		}
		Util.PlaySound(attackSoundString, base.gameObject);
		PlayCrossfade("Body", "Headbutt", "Headbutt.playbackRate", duration, 0.1f);
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
		//IL_0076: Unknown result type (might be due to invalid IL or missing references)
		//IL_0083: Unknown result type (might be due to invalid IL or missing references)
		//IL_008d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0092: Unknown result type (might be due to invalid IL or missing references)
		//IL_00bd: Unknown result type (might be due to invalid IL or missing references)
		//IL_00c2: Unknown result type (might be due to invalid IL or missing references)
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
			if (Object.op_Implicit((Object)(object)base.characterDirection) && Object.op_Implicit((Object)(object)base.inputBank))
			{
				base.characterDirection.moveVector = base.inputBank.aimDirection;
			}
			if (Object.op_Implicit((Object)(object)modelAnimator) && modelAnimator.GetFloat("Headbutt.hitBoxActive") > 0.5f)
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
