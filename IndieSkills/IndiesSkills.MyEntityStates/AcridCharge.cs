using System;
using System.Collections.Generic;
using EntityStates;
using EntityStates.Bison;
using EntityStates.Croco;
using RoR2;
using RoR2.Projectile;
using UnityEngine;

namespace IndiesSkills.MyEntityStates;

internal class AcridCharge : BaseSkillState
{
	public static string endSoundString = Charge.endSoundString;

	public static float turnSpeed = Charge.turnSpeed;

	public static float turnSmoothTime = Charge.turnSmoothTime;

	public static float chargeMovementSpeedCoefficient = 2f;

	public static string headbuttImpactSound = Charge.headbuttImpactSound;

	public static float chargeDuration = 4f;

	public static float damageCoefficient = 4f;

	public static float upwardForceMagnitude = Charge.upwardForceMagnitude;

	public static float awayForceMagnitude = 0f;

	public static GameObject hitEffectPrefab = Charge.hitEffectPrefab;

	private float stopwatch = 0f;

	public string hitboxGroupName = ((BasicMeleeAttack)new Slash()).hitBoxGroupName;

	private Animator animator;

	private ChildLocator childLocator;

	private Vector3 targetMoveVector;

	private Vector3 targetMoveVectorVelocity;

	private OverlapAttack attack;

	private float counter;

	public override void OnEnter()
	{
		//IL_007d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0087: Expected O, but got Unknown
		//IL_00b0: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b5: Unknown result type (might be due to invalid IL or missing references)
		//IL_00e7: Unknown result type (might be due to invalid IL or missing references)
		//IL_00f1: Unknown result type (might be due to invalid IL or missing references)
		//IL_00f6: Unknown result type (might be due to invalid IL or missing references)
		((BaseState)this).OnEnter();
		animator = ((EntityState)this).GetModelAnimator();
		childLocator = ((Component)animator).GetComponent<ChildLocator>();
		SetSprintEffectActive(active: true);
		Util.PlaySound(BaseLeap.leapSoundString, ((EntityState)this).gameObject);
		Util.PlaySound(BaseLeap.soundLoopStartEvent, ((EntityState)this).gameObject);
		HitBoxGroup hitBoxGroup = null;
		Transform modelTransform = ((EntityState)this).GetModelTransform();
		if (Object.op_Implicit((Object)(object)modelTransform))
		{
			hitBoxGroup = Array.Find(((Component)modelTransform).GetComponents<HitBoxGroup>(), (HitBoxGroup element) => element.groupName == hitboxGroupName);
		}
		attack = new OverlapAttack();
		attack.attacker = ((EntityState)this).gameObject;
		attack.inflictor = ((EntityState)this).gameObject;
		attack.teamIndex = ((BaseState)this).GetTeam();
		attack.damage = damageCoefficient * ((BaseState)this).damageStat;
		attack.hitEffectPrefab = hitEffectPrefab;
		attack.forceVector = Vector3.up * upwardForceMagnitude;
		attack.pushAwayForce = awayForceMagnitude;
		attack.hitBoxGroup = hitBoxGroup;
		attack.isCrit = ((BaseState)this).RollCrit();
	}

	private void SetSprintEffectActive(bool active)
	{
		if (Object.op_Implicit((Object)(object)childLocator))
		{
			Transform val = childLocator.FindChild("SprintEffect");
			if (!((Object)(object)val == (Object)null))
			{
				((Component)val).gameObject.SetActive(active);
			}
		}
	}

	public override void OnExit()
	{
		//IL_000e: Unknown result type (might be due to invalid IL or missing references)
		((EntityState)this).OnExit();
		((EntityState)this).characterMotor.moveDirection = Vector3.zero;
		Util.PlaySound(endSoundString, ((EntityState)this).gameObject);
		Util.PlaySound("stop_bison_charge_attack_loop", ((EntityState)this).gameObject);
		SetSprintEffectActive(active: false);
		if (((EntityState)this).isAuthority)
		{
			((EntityState)this).characterBody.isSprinting = false;
		}
		Util.PlaySound(BaseLeap.soundLoopStopEvent, ((EntityState)this).gameObject);
	}

	public override void FixedUpdate()
	{
		//IL_0003: Unknown result type (might be due to invalid IL or missing references)
		//IL_000e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0023: Unknown result type (might be due to invalid IL or missing references)
		//IL_0028: Unknown result type (might be due to invalid IL or missing references)
		//IL_002d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0032: Unknown result type (might be due to invalid IL or missing references)
		//IL_0036: Unknown result type (might be due to invalid IL or missing references)
		//IL_003b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0047: Unknown result type (might be due to invalid IL or missing references)
		//IL_004c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0057: Unknown result type (might be due to invalid IL or missing references)
		//IL_005c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0070: Unknown result type (might be due to invalid IL or missing references)
		//IL_0076: Unknown result type (might be due to invalid IL or missing references)
		//IL_0110: Unknown result type (might be due to invalid IL or missing references)
		//IL_0115: Unknown result type (might be due to invalid IL or missing references)
		Vector3 val = Vector3.ProjectOnPlane(Vector3.SmoothDamp(targetMoveVector, ((EntityState)this).inputBank.aimDirection, ref targetMoveVectorVelocity, turnSmoothTime, turnSpeed), Vector3.up);
		targetMoveVector = ((Vector3)(ref val)).normalized;
		((EntityState)this).characterDirection.moveVector = targetMoveVector;
		Vector3 forward = ((EntityState)this).characterDirection.forward;
		float num = ((BaseState)this).moveSpeedStat * chargeMovementSpeedCoefficient;
		((EntityState)this).characterMotor.moveDirection = forward * chargeMovementSpeedCoefficient;
		animator.SetFloat("forwardSpeed", num);
		if (((EntityState)this).isAuthority)
		{
			((EntityState)this).characterBody.isSprinting = true;
		}
		if (((EntityState)this).isAuthority && attack.Fire((List<HurtBox>)null))
		{
			Util.PlaySound(headbuttImpactSound, ((EntityState)this).gameObject);
		}
		stopwatch += Time.fixedDeltaTime;
		if (stopwatch > chargeDuration)
		{
			((EntityState)this).outer.SetNextStateToMain();
		}
		val = GetIdealVelocity();
		float num2 = ((Vector3)(ref val)).magnitude / ((EntityState)this).characterBody.moveSpeed;
		int num3 = Mathf.FloorToInt(6f * num2);
		if (counter % (float)num3 == 0f && ((EntityState)this).isAuthority)
		{
			DropAcidPoolAuthority();
		}
		counter += 1f;
		((EntityState)this).FixedUpdate();
	}

	protected void DropAcidPoolAuthority()
	{
		//IL_0007: Unknown result type (might be due to invalid IL or missing references)
		//IL_000c: Unknown result type (might be due to invalid IL or missing references)
		//IL_000f: Unknown result type (might be due to invalid IL or missing references)
		//IL_005c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0061: Unknown result type (might be due to invalid IL or missing references)
		//IL_0068: Unknown result type (might be due to invalid IL or missing references)
		//IL_0069: Unknown result type (might be due to invalid IL or missing references)
		//IL_006e: Unknown result type (might be due to invalid IL or missing references)
		//IL_006f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0075: Unknown result type (might be due to invalid IL or missing references)
		Vector3 footPosition = ((EntityState)this).characterBody.footPosition;
		FireProjectileInfo val = default(FireProjectileInfo);
		val.projectilePrefab = BaseLeap.projectilePrefab;
		val.crit = ((BaseState)this).RollCrit();
		val.force = 0f;
		val.damage = ((BaseState)this).damageStat * 0.9f;
		val.owner = ((EntityState)this).gameObject;
		val.rotation = Quaternion.identity;
		val.position = footPosition;
		FireProjectileInfo val2 = val;
		ProjectileManager.instance.FireProjectile(val2);
	}

	private Vector3 GetIdealVelocity()
	{
		//IL_0007: Unknown result type (might be due to invalid IL or missing references)
		//IL_0017: Unknown result type (might be due to invalid IL or missing references)
		//IL_0021: Unknown result type (might be due to invalid IL or missing references)
		//IL_0026: Unknown result type (might be due to invalid IL or missing references)
		//IL_0029: Unknown result type (might be due to invalid IL or missing references)
		return ((EntityState)this).characterDirection.forward * ((EntityState)this).characterBody.moveSpeed * chargeMovementSpeedCoefficient;
	}

	public override InterruptPriority GetMinimumInterruptPriority()
	{
		//IL_0002: Unknown result type (might be due to invalid IL or missing references)
		//IL_0005: Unknown result type (might be due to invalid IL or missing references)
		return (InterruptPriority)2;
	}
}
