using System;
using System.Collections.Generic;
using EntityStates;
using EntityStates.ClayGrenadier;
using EntityStates.Merc;
using PaladinMod.Misc;
using PaladinMod.Modules;
using PaladinMod.States;
using RoR2;
using RoR2.Projectile;
using UnityEngine;
using UnityEngine.Networking;

namespace IndiesSkills.MyEntityStates;

internal class OilSlash : BaseSkillState
{
	public static GameObject oilPrefab = ((GenericProjectileBaseState)new ThrowBarrel()).projectilePrefab;

	public static float damageCoefficient = 3f;

	public float baseDuration = 1.6f;

	public static float attackRecoil = 1.5f;

	public static float hitHopVelocity = 5.5f;

	public static float earlyExitTime = 0.575f;

	public int swingIndex;

	private bool inCombo;

	private float earlyExitDuration;

	private float duration;

	private bool hasFired;

	private float hitPauseTimer;

	protected OverlapAttack attack;

	private bool inHitPause;

	private bool hasHopped;

	private float stopwatch;

	private bool cancelling;

	private Animator animator;

	private HitStopCachedState hitStopCachedState;

	private PaladinSwordController swordController;

	private Vector3 storedVelocity;

	public override void OnEnter()
	{
		((BaseState)this).OnEnter();
		duration = baseDuration / ((BaseState)this).attackSpeedStat;
		earlyExitDuration = duration * Slash.earlyExitTime;
		hasFired = false;
		cancelling = false;
		animator = ((EntityState)this).GetModelAnimator();
		swordController = ((EntityState)this).GetComponent<PaladinSwordController>();
		((BaseState)this).StartAimMode(0.5f + duration, false);
		((EntityState)this).characterBody.isSprinting = false;
		inCombo = false;
		((EntityState)this).characterBody.outOfCombatStopwatch = 0f;
		if (Object.op_Implicit((Object)(object)swordController))
		{
			swordController.attacking = true;
		}
		HitBoxGroup hitBoxGroup = null;
		Transform modelTransform = ((EntityState)this).GetModelTransform();
		if (Object.op_Implicit((Object)(object)modelTransform))
		{
			hitBoxGroup = Array.Find(((Component)modelTransform).GetComponents<HitBoxGroup>(), (HitBoxGroup element) => element.groupName == "Sword");
		}
		if (swingIndex > 1)
		{
			swingIndex = 0;
			inCombo = true;
		}
		Util.PlaySound("PaladinCloth1", ((EntityState)this).gameObject);
		string text = "Slash" + (1 + swingIndex);
		if (inCombo)
		{
			if (!animator.GetBool("isMoving") && animator.GetBool("isGrounded"))
			{
				((EntityState)this).PlayCrossfade("FullBody, Override", "SlashCombo1", "Slash.playbackRate", duration, 0.05f);
			}
			((EntityState)this).PlayCrossfade("Gesture, Override", "SlashCombo1", "Slash.playbackRate", duration, 0.05f);
		}
		else
		{
			if (!animator.GetBool("isMoving") && animator.GetBool("isGrounded"))
			{
				((EntityState)this).PlayCrossfade("FullBody, Override", text, "Slash.playbackRate", duration, 0.05f);
			}
			((EntityState)this).PlayCrossfade("Gesture, Override", text, "Slash.playbackRate", duration, 0.05f);
		}
		attack = new OverlapAttack();
		attack.damageType = (DamageType)0;
		attack.attacker = ((EntityState)this).gameObject;
		attack.inflictor = ((EntityState)this).gameObject;
		attack.teamIndex = ((BaseState)this).GetTeam();
		attack.damage = damageCoefficient * ((BaseState)this).damageStat;
		attack.procCoefficient = 1f;
		attack.hitEffectPrefab = swordController.hitEffect;
		attack.forceVector = Vector3.zero;
		attack.pushAwayForce = 750f;
		attack.hitBoxGroup = hitBoxGroup;
		attack.isCrit = ((BaseState)this).RollCrit();
		if (((EntityState)this).characterBody.HasBuff(Buffs.overchargeBuff))
		{
			attack.damageType = (DamageType)32;
			attack.damage = 4f * ((BaseState)this).damageStat;
		}
	}

	public override void OnExit()
	{
		//IL_0029: Unknown result type (might be due to invalid IL or missing references)
		((EntityState)this).OnExit();
		if (!hasFired)
		{
			FireAttack();
		}
		if (inHitPause)
		{
			((BaseState)this).ConsumeHitStopCachedState(hitStopCachedState, ((EntityState)this).characterMotor, animator);
			inHitPause = false;
		}
		if (Object.op_Implicit((Object)(object)swordController))
		{
			swordController.attacking = false;
		}
	}

	protected virtual void OnHitAuthority()
	{
		//IL_009a: Unknown result type (might be due to invalid IL or missing references)
		//IL_009f: Unknown result type (might be due to invalid IL or missing references)
		//IL_00d3: Unknown result type (might be due to invalid IL or missing references)
		//IL_00d8: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b6: Unknown result type (might be due to invalid IL or missing references)
		//IL_00bb: Unknown result type (might be due to invalid IL or missing references)
		if (!hasHopped)
		{
			if (Object.op_Implicit((Object)(object)((EntityState)this).characterMotor) && !((EntityState)this).characterMotor.isGrounded)
			{
				((BaseState)this).SmallHop(((EntityState)this).characterMotor, Slash.hitHopVelocity);
			}
			if (((EntityState)this).skillLocator.utility.skillDef.skillNameToken == "PALADIN_UTILITY_DASH_NAME")
			{
				((EntityState)this).skillLocator.utility.RunRecharge(1f);
			}
			hasHopped = true;
		}
		if (!inHitPause)
		{
			if (((EntityState)this).characterMotor.velocity != Vector3.zero)
			{
				storedVelocity = ((EntityState)this).characterMotor.velocity;
			}
			hitStopCachedState = ((BaseState)this).CreateHitStopCachedState(((EntityState)this).characterMotor, animator, "Slash.playbackRate");
			hitPauseTimer = 2f * GroundLight.hitPauseDuration / ((BaseState)this).attackSpeedStat;
			inHitPause = true;
		}
	}

	public void FireAttack()
	{
		if (!hasFired)
		{
			hasFired = true;
			swordController.PlaySwingSound();
			if (((EntityState)this).isAuthority)
			{
				string text = null;
				text = ((swingIndex != 0) ? "SwingLeft" : "SwingRight");
				((BaseState)this).AddRecoil(-1f * Slash.attackRecoil, -2f * Slash.attackRecoil, -0.5f * Slash.attackRecoil, 0.5f * Slash.attackRecoil);
				EffectManager.SimpleMuzzleFlash(swordController.swingEffect, ((EntityState)this).gameObject, text, true);
			}
			if (((EntityState)this).isAuthority)
			{
				launchOil();
			}
		}
		if (((EntityState)this).isAuthority && attack.Fire((List<HurtBox>)null))
		{
			OnHitAuthority();
		}
	}

	public void launchOil()
	{
		if ((double)((EntityState)this).characterBody.healthComponent.health >= (double)((EntityState)this).characterBody.maxHealth * 0.9 || ((EntityState)this).characterBody.healthComponent.barrier > 0f)
		{
			Ray aimRay = ((BaseState)this).GetAimRay();
			FireProjectileInfo val = default(FireProjectileInfo);
			val.projectilePrefab = oilPrefab;
			val.position = aimRay.origin;
			val.rotation = Util.QuaternionSafeLookRotation(aimRay.direction);
			val.owner = ((EntityState)this).gameObject;
			val.damage = ((BaseState)this).damageStat * 1.5f;
			val.damageTypeOverride = (DamageType)512;
			val.force = 5f;
			val.crit = ((BaseState)this).RollCrit();
			val.speedOverride = 40f * ((BaseState)this).attackSpeedStat;
			FireProjectileInfo val2 = val;
			val = default(FireProjectileInfo);
			val.projectilePrefab = oilPrefab;
			val.position = aimRay.origin;
			val.rotation = Util.QuaternionSafeLookRotation(Util.ApplySpread(aimRay.direction, 0f, 0f, 1f, 0f, -5f, 0f));
			val.owner = ((EntityState)this).gameObject;
			val.damage = ((BaseState)this).damageStat * 1.5f;
			val.damageTypeOverride = (DamageType)512;
			val.force = 5f;
			val.crit = ((BaseState)this).RollCrit();
			val.speedOverride = 40f * ((BaseState)this).attackSpeedStat;
			FireProjectileInfo val3 = val;
			val = default(FireProjectileInfo);
			val.projectilePrefab = oilPrefab;
			val.position = aimRay.origin;
			val.rotation = Util.QuaternionSafeLookRotation(Util.ApplySpread(aimRay.direction, 0f, 0f, 1f, 0f, 5f, 0f));
			val.owner = ((EntityState)this).gameObject;
			val.damage = ((BaseState)this).damageStat * 1.5f;
			val.damageTypeOverride = (DamageType)512;
			val.force = 5f;
			val.crit = ((BaseState)this).RollCrit();
			val.speedOverride = 40f * ((BaseState)this).attackSpeedStat;
			FireProjectileInfo val4 = val;
			ProjectileManager.instance.FireProjectile(val2);
			ProjectileManager.instance.FireProjectile(val3);
			ProjectileManager.instance.FireProjectile(val4);
		}
	}

	public override void FixedUpdate()
	{
		//IL_0058: Unknown result type (might be due to invalid IL or missing references)
		//IL_0077: Unknown result type (might be due to invalid IL or missing references)
		//IL_007c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0091: Unknown result type (might be due to invalid IL or missing references)
		//IL_0096: Unknown result type (might be due to invalid IL or missing references)
		//IL_00d7: Unknown result type (might be due to invalid IL or missing references)
		//IL_00dc: Unknown result type (might be due to invalid IL or missing references)
		((EntityState)this).FixedUpdate();
		if (Object.op_Implicit((Object)(object)animator))
		{
			animator.SetBool("inCombat", true);
		}
		hitPauseTimer -= Time.fixedDeltaTime;
		if (hitPauseTimer <= 0f && inHitPause)
		{
			((BaseState)this).ConsumeHitStopCachedState(hitStopCachedState, ((EntityState)this).characterMotor, animator);
			inHitPause = false;
			if (storedVelocity != Vector3.zero)
			{
				((EntityState)this).characterMotor.velocity = storedVelocity;
			}
		}
		if (!inHitPause)
		{
			stopwatch += Time.fixedDeltaTime;
		}
		else
		{
			if (Object.op_Implicit((Object)(object)((EntityState)this).characterMotor))
			{
				((EntityState)this).characterMotor.velocity = Vector3.zero;
			}
			if (Object.op_Implicit((Object)(object)animator))
			{
				animator.SetFloat("Slash.playbackRate", 0f);
			}
		}
		if (stopwatch >= duration * 0.2669f && stopwatch <= duration * 0.4f)
		{
			FireAttack();
		}
		if (((EntityState)this).isAuthority)
		{
			if (((EntityState)this).fixedAge >= earlyExitDuration && ((EntityState)this).inputBank.skill1.down)
			{
				OilSlash oilSlash = new OilSlash();
				oilSlash.swingIndex = swingIndex + 1;
				((EntityState)this).outer.SetNextState((EntityState)(object)oilSlash);
			}
			else if (((EntityState)this).fixedAge >= duration)
			{
				((EntityState)this).outer.SetNextStateToMain();
			}
		}
	}

	public override InterruptPriority GetMinimumInterruptPriority()
	{
		//IL_0010: Unknown result type (might be due to invalid IL or missing references)
		//IL_000c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0013: Unknown result type (might be due to invalid IL or missing references)
		if (cancelling)
		{
			return (InterruptPriority)0;
		}
		return (InterruptPriority)2;
	}

	public override void OnSerialize(NetworkWriter writer)
	{
		((BaseSkillState)this).OnSerialize(writer);
		writer.Write(swingIndex);
	}

	public override void OnDeserialize(NetworkReader reader)
	{
		((BaseSkillState)this).OnDeserialize(reader);
		swingIndex = reader.ReadInt32();
	}
}
