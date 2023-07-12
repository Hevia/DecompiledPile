using System;
using RoR2;
using UnityEngine;
using UnityEngine.Networking;

namespace EntityStates.ImpMonster;

public class Backstab : BaseState
{
	public static float baseDuration = 3.5f;

	public static float damageCoefficient = 4f;

	public static float forceMagnitude = 16f;

	public static float radius = 3f;

	public static GameObject hitEffectPrefab;

	public static GameObject biteEffectPrefab;

	private OverlapAttack attack;

	private Animator modelAnimator;

	private float duration;

	private bool hasBit;

	public override void OnEnter()
	{
		base.OnEnter();
		duration = baseDuration / attackSpeedStat;
		modelAnimator = GetModelAnimator();
		Transform modelTransform = GetModelTransform();
		attack = new OverlapAttack();
		attack.attacker = base.gameObject;
		attack.inflictor = base.gameObject;
		attack.teamIndex = TeamComponent.GetObjectTeam(attack.attacker);
		attack.damage = damageCoefficient * damageStat;
		attack.hitEffectPrefab = hitEffectPrefab;
		attack.isCrit = Util.CheckRoll(critStat, base.characterBody.master);
		if (Object.op_Implicit((Object)(object)modelTransform))
		{
			attack.hitBoxGroup = Array.Find(((Component)modelTransform).GetComponents<HitBoxGroup>(), (HitBoxGroup element) => element.groupName == "Backstab");
		}
		if (Object.op_Implicit((Object)(object)modelAnimator))
		{
			PlayAnimation("Gesture", "Backstab", "Backstab.playbackRate", duration);
		}
		if (Object.op_Implicit((Object)(object)base.characterBody))
		{
			base.characterBody.SetAimTimer(2f);
		}
	}

	public override void FixedUpdate()
	{
		//IL_0062: Unknown result type (might be due to invalid IL or missing references)
		//IL_006c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0071: Unknown result type (might be due to invalid IL or missing references)
		base.FixedUpdate();
		if (NetworkServer.active && Object.op_Implicit((Object)(object)modelAnimator) && modelAnimator.GetFloat("Bite.hitBoxActive") > 0.1f)
		{
			if (!hasBit)
			{
				EffectManager.SimpleMuzzleFlash(biteEffectPrefab, base.gameObject, "MuzzleMouth", transmit: true);
				hasBit = true;
			}
			attack.forceVector = base.transform.forward * forceMagnitude;
			attack.Fire();
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
