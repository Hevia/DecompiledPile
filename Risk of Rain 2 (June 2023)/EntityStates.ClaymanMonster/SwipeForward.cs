using System;
using RoR2;
using UnityEngine;
using UnityEngine.Networking;

namespace EntityStates.ClaymanMonster;

public class SwipeForward : BaseState
{
	public static float baseDuration = 3.5f;

	public static float damageCoefficient = 4f;

	public static float forceMagnitude = 16f;

	public static float selfForceMagnitude;

	public static float radius = 3f;

	public static GameObject hitEffectPrefab;

	public static GameObject swingEffectPrefab;

	public static string attackString;

	private OverlapAttack attack;

	private Animator modelAnimator;

	private float duration;

	private bool hasSlashed;

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
		Util.PlaySound(attackString, base.gameObject);
		if (Object.op_Implicit((Object)(object)modelTransform))
		{
			attack.hitBoxGroup = Array.Find(((Component)modelTransform).GetComponents<HitBoxGroup>(), (HitBoxGroup element) => element.groupName == "Sword");
		}
		if (Object.op_Implicit((Object)(object)modelAnimator))
		{
			PlayAnimation("Gesture, Override", "SwipeForward", "SwipeForward.playbackRate", duration);
			PlayAnimation("Gesture, Additive", "SwipeForward", "SwipeForward.playbackRate", duration);
		}
		if (Object.op_Implicit((Object)(object)base.characterBody))
		{
			base.characterBody.SetAimTimer(2f);
		}
	}

	public override void FixedUpdate()
	{
		//IL_007f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0084: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a3: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ad: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b2: Unknown result type (might be due to invalid IL or missing references)
		base.FixedUpdate();
		if (NetworkServer.active && Object.op_Implicit((Object)(object)modelAnimator) && modelAnimator.GetFloat("SwipeForward.hitBoxActive") > 0.1f)
		{
			if (!hasSlashed)
			{
				EffectManager.SimpleMuzzleFlash(swingEffectPrefab, base.gameObject, "SwingCenter", transmit: true);
				HealthComponent healthComponent = base.characterBody.healthComponent;
				CharacterDirection component = ((Component)base.characterBody).GetComponent<CharacterDirection>();
				if (Object.op_Implicit((Object)(object)healthComponent))
				{
					healthComponent.TakeDamageForce(selfForceMagnitude * component.forward, alwaysApply: true);
				}
				hasSlashed = true;
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
