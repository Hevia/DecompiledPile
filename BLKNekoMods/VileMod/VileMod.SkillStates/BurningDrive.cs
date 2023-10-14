using System;
using System.Collections.Generic;
using EntityStates;
using EntityStates.Merc;
using RoR2;
using UnityEngine;
using UnityEngine.Networking;
using VileMod.Modules;

namespace VileMod.SkillStates;

internal class BurningDrive : BaseSkillState
{
	public static float damageCoefficient = 10f;

	public static float buffDamageCoefficient = 1f;

	public float baseDuration = 0.8f;

	public static float attackRecoil = 0.5f;

	public static float hitHopVelocity = 5.5f;

	public static float baseEarlyExit = 0.25f;

	public int swingIndex;

	public static GameObject hitEffectPrefab = Resources.Load<GameObject>("prefabs/effects/impacteffects/ImpactMercSwing");

	private float earlyExitDuration;

	private float duration;

	private bool hasFired;

	private float hitPauseTimer;

	private OverlapAttack attack;

	private bool inHitPause;

	private bool hasHopped;

	private float stopwatch;

	private Animator animator;

	private HitStopCachedState hitStopCachedState;

	public override void OnEnter()
	{
		//IL_013b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0145: Expected O, but got Unknown
		//IL_016a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0198: Unknown result type (might be due to invalid IL or missing references)
		//IL_019d: Unknown result type (might be due to invalid IL or missing references)
		//IL_01db: Unknown result type (might be due to invalid IL or missing references)
		//IL_01e0: Unknown result type (might be due to invalid IL or missing references)
		((BaseState)this).OnEnter();
		duration = baseDuration / ((BaseState)this).attackSpeedStat;
		earlyExitDuration = baseEarlyExit / ((BaseState)this).attackSpeedStat;
		hasFired = false;
		animator = ((EntityState)this).GetModelAnimator();
		((BaseState)this).StartAimMode(0.5f + duration, false);
		Util.PlaySound(Sounds.vileHahaha, ((EntityState)this).gameObject);
		HitBoxGroup hitBoxGroup = null;
		Transform modelTransform = ((EntityState)this).GetModelTransform();
		if (Object.op_Implicit((Object)(object)modelTransform))
		{
			hitBoxGroup = Array.Find(((Component)modelTransform).GetComponents<HitBoxGroup>(), (HitBoxGroup element) => element.groupName == "GroundBox");
		}
		((EntityState)this).PlayAnimation("FullBody, Override", "BurningDrive", "attackSpeed", duration);
		EffectManager.SimpleMuzzleFlash(Assets.BurningDriveVFX, ((EntityState)this).gameObject, "GroundBox", true);
		EffectManager.SimpleMuzzleFlash(Assets.BurningDriveVFX, ((EntityState)this).gameObject, "GroundBox", true);
		EffectManager.SimpleMuzzleFlash(Assets.BurningDriveVFX, ((EntityState)this).gameObject, "GroundBox", true);
		EffectManager.SimpleMuzzleFlash(Assets.BurningDriveVFX, ((EntityState)this).gameObject, "GroundBox", true);
		EffectManager.SimpleMuzzleFlash(Assets.BurningDriveVFX, ((EntityState)this).gameObject, "GroundBox", true);
		float num = damageCoefficient;
		attack = new OverlapAttack();
		attack.damageType = (DamageType)(Util.CheckRoll(75f, ((EntityState)this).characterBody.master) ? 128 : 0);
		attack.attacker = ((EntityState)this).gameObject;
		attack.inflictor = ((EntityState)this).gameObject;
		attack.teamIndex = ((BaseState)this).GetTeam();
		attack.damage = num * ((BaseState)this).damageStat;
		attack.procCoefficient = 1f;
		attack.hitEffectPrefab = hitEffectPrefab;
		attack.forceVector = Vector3.zero;
		attack.pushAwayForce = 80f;
		attack.hitBoxGroup = hitBoxGroup;
		attack.isCrit = ((BaseState)this).RollCrit();
	}

	public override void OnExit()
	{
		((EntityState)this).OnExit();
	}

	public void FireAttack()
	{
		//IL_006b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0070: Unknown result type (might be due to invalid IL or missing references)
		//IL_0109: Unknown result type (might be due to invalid IL or missing references)
		//IL_010e: Unknown result type (might be due to invalid IL or missing references)
		if (hasFired)
		{
			return;
		}
		EffectManager.SimpleMuzzleFlash(GroundLight.comboSwingEffectPrefab, ((EntityState)this).gameObject, "SwingLeft", true);
		if (!((EntityState)this).isAuthority)
		{
			return;
		}
		((BaseState)this).AddRecoil(-1f * attackRecoil, -2f * attackRecoil, -0.5f * attackRecoil, 0.5f * attackRecoil);
		Ray aimRay = ((BaseState)this).GetAimRay();
		if (!attack.Fire((List<HurtBox>)null))
		{
			return;
		}
		Util.PlaySound(GroundLight.hitSoundString, ((EntityState)this).gameObject);
		if (!hasHopped)
		{
			if (Object.op_Implicit((Object)(object)((EntityState)this).characterMotor) && !((EntityState)this).characterMotor.isGrounded)
			{
				((BaseState)this).SmallHop(((EntityState)this).characterMotor, hitHopVelocity);
			}
			hasHopped = true;
		}
		if (!inHitPause)
		{
			hitStopCachedState = ((BaseState)this).CreateHitStopCachedState(((EntityState)this).characterMotor, animator, "FireArrow.playbackRate");
			hitPauseTimer = 0.6f * GroundLight.hitPauseDuration / ((BaseState)this).attackSpeedStat;
			inHitPause = true;
		}
	}

	public override void FixedUpdate()
	{
		//IL_0037: Unknown result type (might be due to invalid IL or missing references)
		//IL_008f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0094: Unknown result type (might be due to invalid IL or missing references)
		((EntityState)this).FixedUpdate();
		hitPauseTimer -= Time.fixedDeltaTime;
		if (hitPauseTimer <= 0f && inHitPause)
		{
			((BaseState)this).ConsumeHitStopCachedState(hitStopCachedState, ((EntityState)this).characterMotor, animator);
			inHitPause = false;
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
				animator.SetFloat("FireArrow.playbackRate", 1f);
			}
		}
		if (stopwatch >= duration * 0.2f)
		{
			FireAttack();
		}
		if (((EntityState)this).fixedAge >= duration && ((EntityState)this).isAuthority && ((EntityState)this).inputBank.skill1.down)
		{
			((EntityState)this).outer.SetNextStateToMain();
		}
		if (((EntityState)this).fixedAge >= duration && ((EntityState)this).isAuthority && !((EntityState)this).inputBank.skill1.down)
		{
			((EntityState)this).PlayAnimation("Attack", "Empty", "attackSpeed", duration);
			((EntityState)this).outer.SetNextStateToMain();
		}
	}

	public override InterruptPriority GetMinimumInterruptPriority()
	{
		//IL_0002: Unknown result type (might be due to invalid IL or missing references)
		//IL_0005: Unknown result type (might be due to invalid IL or missing references)
		return (InterruptPriority)1;
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
