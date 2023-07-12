using System;
using RoR2;
using UnityEngine;

namespace EntityStates.Bison;

public class Charge : BaseState
{
	public static float chargeDuration;

	public static float chargeMovementSpeedCoefficient;

	public static float turnSpeed;

	public static float turnSmoothTime;

	public static float impactDamageCoefficient;

	public static float impactForce;

	public static float damageCoefficient;

	public static float upwardForceMagnitude;

	public static float awayForceMagnitude;

	public static GameObject hitEffectPrefab;

	public static float overlapResetFrequency;

	public static float overlapSphereRadius;

	public static float selfStunDuration;

	public static float selfStunForce;

	public static string startSoundString;

	public static string endSoundString;

	public static string footstepOverrideSoundString;

	public static string headbuttImpactSound;

	private float stopwatch;

	private float overlapResetStopwatch;

	private Animator animator;

	private Vector3 targetMoveVector;

	private Vector3 targetMoveVectorVelocity;

	private ContactDamage contactDamage;

	private OverlapAttack attack;

	private HitBoxGroup hitboxGroup;

	private ChildLocator childLocator;

	private Transform sphereCheckTransform;

	private string baseFootstepString;

	public override void OnEnter()
	{
		base.OnEnter();
		animator = GetModelAnimator();
		childLocator = ((Component)animator).GetComponent<ChildLocator>();
		FootstepHandler component = ((Component)animator).GetComponent<FootstepHandler>();
		if (Object.op_Implicit((Object)(object)component))
		{
			baseFootstepString = component.baseFootstepString;
			component.baseFootstepString = footstepOverrideSoundString;
		}
		Util.PlaySound(startSoundString, base.gameObject);
		PlayCrossfade("Body", "ChargeForward", 0.2f);
		ResetOverlapAttack();
		SetSprintEffectActive(active: true);
		if (Object.op_Implicit((Object)(object)childLocator))
		{
			sphereCheckTransform = childLocator.FindChild("SphereCheckTransform");
		}
		if (!Object.op_Implicit((Object)(object)sphereCheckTransform) && Object.op_Implicit((Object)(object)base.characterBody))
		{
			sphereCheckTransform = base.characterBody.coreTransform;
		}
		if (!Object.op_Implicit((Object)(object)sphereCheckTransform))
		{
			sphereCheckTransform = base.transform;
		}
	}

	private void SetSprintEffectActive(bool active)
	{
		if (Object.op_Implicit((Object)(object)childLocator))
		{
			Transform obj = childLocator.FindChild("SprintEffect");
			if (obj != null)
			{
				((Component)obj).gameObject.SetActive(active);
			}
		}
	}

	public override void OnExit()
	{
		//IL_000c: Unknown result type (might be due to invalid IL or missing references)
		base.OnExit();
		base.characterMotor.moveDirection = Vector3.zero;
		Util.PlaySound(endSoundString, base.gameObject);
		Util.PlaySound("stop_bison_charge_attack_loop", base.gameObject);
		SetSprintEffectActive(active: false);
		FootstepHandler component = ((Component)animator).GetComponent<FootstepHandler>();
		if (Object.op_Implicit((Object)(object)component))
		{
			component.baseFootstepString = baseFootstepString;
		}
	}

	public override void FixedUpdate()
	{
		//IL_0002: Unknown result type (might be due to invalid IL or missing references)
		//IL_000d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0022: Unknown result type (might be due to invalid IL or missing references)
		//IL_0027: Unknown result type (might be due to invalid IL or missing references)
		//IL_002c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0031: Unknown result type (might be due to invalid IL or missing references)
		//IL_0034: Unknown result type (might be due to invalid IL or missing references)
		//IL_0039: Unknown result type (might be due to invalid IL or missing references)
		//IL_0045: Unknown result type (might be due to invalid IL or missing references)
		//IL_004a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0055: Unknown result type (might be due to invalid IL or missing references)
		//IL_005a: Unknown result type (might be due to invalid IL or missing references)
		//IL_006e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0074: Unknown result type (might be due to invalid IL or missing references)
		//IL_00f2: Unknown result type (might be due to invalid IL or missing references)
		//IL_0104: Unknown result type (might be due to invalid IL or missing references)
		//IL_0143: Unknown result type (might be due to invalid IL or missing references)
		//IL_0149: Unknown result type (might be due to invalid IL or missing references)
		Vector3 val = Vector3.ProjectOnPlane(Vector3.SmoothDamp(targetMoveVector, base.inputBank.aimDirection, ref targetMoveVectorVelocity, turnSmoothTime, turnSpeed), Vector3.up);
		targetMoveVector = ((Vector3)(ref val)).normalized;
		base.characterDirection.moveVector = targetMoveVector;
		Vector3 forward = base.characterDirection.forward;
		float num = moveSpeedStat * chargeMovementSpeedCoefficient;
		base.characterMotor.moveDirection = forward * chargeMovementSpeedCoefficient;
		animator.SetFloat("forwardSpeed", num);
		if (base.isAuthority && attack.Fire())
		{
			Util.PlaySound(headbuttImpactSound, base.gameObject);
		}
		if (overlapResetStopwatch >= 1f / overlapResetFrequency)
		{
			overlapResetStopwatch -= 1f / overlapResetFrequency;
		}
		if (base.isAuthority && Physics.OverlapSphere(sphereCheckTransform.position, overlapSphereRadius, LayerMask.op_Implicit(LayerIndex.world.mask)).Length != 0)
		{
			Util.PlaySound(headbuttImpactSound, base.gameObject);
			EffectManager.SimpleMuzzleFlash(hitEffectPrefab, base.gameObject, "SphereCheckTransform", transmit: true);
			base.healthComponent.TakeDamageForce(forward * selfStunForce, alwaysApply: true);
			StunState stunState = new StunState();
			stunState.stunDuration = selfStunDuration;
			outer.SetNextState(stunState);
		}
		else
		{
			stopwatch += Time.fixedDeltaTime;
			overlapResetStopwatch += Time.fixedDeltaTime;
			if (stopwatch > chargeDuration)
			{
				outer.SetNextStateToMain();
			}
			base.FixedUpdate();
		}
	}

	private void ResetOverlapAttack()
	{
		//IL_00c1: Unknown result type (might be due to invalid IL or missing references)
		//IL_00cb: Unknown result type (might be due to invalid IL or missing references)
		//IL_00d0: Unknown result type (might be due to invalid IL or missing references)
		if (!Object.op_Implicit((Object)(object)hitboxGroup))
		{
			Transform modelTransform = GetModelTransform();
			if (Object.op_Implicit((Object)(object)modelTransform))
			{
				hitboxGroup = Array.Find(((Component)modelTransform).GetComponents<HitBoxGroup>(), (HitBoxGroup element) => element.groupName == "Charge");
			}
		}
		attack = new OverlapAttack();
		attack.attacker = base.gameObject;
		attack.inflictor = base.gameObject;
		attack.teamIndex = TeamComponent.GetObjectTeam(attack.attacker);
		attack.damage = damageCoefficient * damageStat;
		attack.hitEffectPrefab = hitEffectPrefab;
		attack.forceVector = Vector3.up * upwardForceMagnitude;
		attack.pushAwayForce = awayForceMagnitude;
		attack.hitBoxGroup = hitboxGroup;
	}

	public override InterruptPriority GetMinimumInterruptPriority()
	{
		return InterruptPriority.Skill;
	}
}
