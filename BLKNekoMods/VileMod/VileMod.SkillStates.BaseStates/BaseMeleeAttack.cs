using System;
using System.Collections.Generic;
using EntityStates;
using RoR2;
using RoR2.Audio;
using UnityEngine;
using UnityEngine.Networking;

namespace VileMod.SkillStates.BaseStates;

public class BaseMeleeAttack : BaseSkillState
{
	public int swingIndex;

	protected string hitboxName = "Sword";

	protected DamageType damageType = (DamageType)0;

	protected float damageCoefficient = 3.5f;

	protected float procCoefficient = 1f;

	protected float pushForce = 300f;

	protected Vector3 bonusForce = Vector3.zero;

	protected float baseDuration = 1f;

	protected float attackStartTime = 0.2f;

	protected float attackEndTime = 0.4f;

	protected float baseEarlyExitTime = 0.4f;

	protected float hitStopDuration = 0.012f;

	protected float attackRecoil = 0.75f;

	protected float hitHopVelocity = 4f;

	protected bool cancelled = false;

	protected string swingSoundString = "";

	protected string hitSoundString = "";

	protected string muzzleString = "SwingCenter";

	protected GameObject swingEffectPrefab;

	protected GameObject hitEffectPrefab;

	protected NetworkSoundEventIndex impactSound;

	private float earlyExitTime;

	public float duration;

	private bool hasFired;

	private float hitPauseTimer;

	private OverlapAttack attack;

	protected bool inHitPause;

	private bool hasHopped;

	protected float stopwatch;

	protected Animator animator;

	private HitStopCachedState hitStopCachedState;

	private Vector3 storedVelocity;

	public override void OnEnter()
	{
		//IL_00ac: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b6: Expected O, but got Unknown
		//IL_00bd: Unknown result type (might be due to invalid IL or missing references)
		//IL_00c2: Unknown result type (might be due to invalid IL or missing references)
		//IL_00f0: Unknown result type (might be due to invalid IL or missing references)
		//IL_00f5: Unknown result type (might be due to invalid IL or missing references)
		//IL_013b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0140: Unknown result type (might be due to invalid IL or missing references)
		//IL_017a: Unknown result type (might be due to invalid IL or missing references)
		//IL_017f: Unknown result type (might be due to invalid IL or missing references)
		((BaseState)this).OnEnter();
		duration = baseDuration / ((BaseState)this).attackSpeedStat;
		earlyExitTime = baseEarlyExitTime / ((BaseState)this).attackSpeedStat;
		hasFired = false;
		animator = ((EntityState)this).GetModelAnimator();
		((BaseState)this).StartAimMode(0.5f + duration, false);
		((EntityState)this).characterBody.outOfCombatStopwatch = 0f;
		animator.SetBool("attacking", true);
		HitBoxGroup hitBoxGroup = null;
		Transform modelTransform = ((EntityState)this).GetModelTransform();
		if (Object.op_Implicit((Object)(object)modelTransform))
		{
			hitBoxGroup = Array.Find(((Component)modelTransform).GetComponents<HitBoxGroup>(), (HitBoxGroup element) => element.groupName == hitboxName);
		}
		PlayAttackAnimation();
		attack = new OverlapAttack();
		attack.damageType = damageType;
		attack.attacker = ((EntityState)this).gameObject;
		attack.inflictor = ((EntityState)this).gameObject;
		attack.teamIndex = ((BaseState)this).GetTeam();
		attack.damage = damageCoefficient * ((BaseState)this).damageStat;
		attack.procCoefficient = procCoefficient;
		attack.hitEffectPrefab = hitEffectPrefab;
		attack.forceVector = bonusForce;
		attack.pushAwayForce = pushForce;
		attack.hitBoxGroup = hitBoxGroup;
		attack.isCrit = ((BaseState)this).RollCrit();
		attack.impactSound = impactSound;
	}

	protected virtual void PlayAttackAnimation()
	{
		((EntityState)this).PlayCrossfade("Gesture, Override", "Slash" + (1 + swingIndex), "Slash.playbackRate", duration, 0.05f);
	}

	public override void OnExit()
	{
		if (!hasFired && !cancelled)
		{
			FireAttack();
		}
		((EntityState)this).OnExit();
		animator.SetBool("attacking", false);
	}

	protected virtual void PlaySwingEffect()
	{
		EffectManager.SimpleMuzzleFlash(swingEffectPrefab, ((EntityState)this).gameObject, muzzleString, true);
	}

	protected virtual void OnHitEnemyAuthority()
	{
		//IL_0090: Unknown result type (might be due to invalid IL or missing references)
		//IL_0095: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ad: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b2: Unknown result type (might be due to invalid IL or missing references)
		Util.PlaySound(hitSoundString, ((EntityState)this).gameObject);
		if (!hasHopped)
		{
			if (Object.op_Implicit((Object)(object)((EntityState)this).characterMotor) && !((EntityState)this).characterMotor.isGrounded && hitHopVelocity > 0f)
			{
				((BaseState)this).SmallHop(((EntityState)this).characterMotor, hitHopVelocity);
			}
			hasHopped = true;
		}
		if (!inHitPause && hitStopDuration > 0f)
		{
			storedVelocity = ((EntityState)this).characterMotor.velocity;
			hitStopCachedState = ((BaseState)this).CreateHitStopCachedState(((EntityState)this).characterMotor, animator, "Slash.playbackRate");
			hitPauseTimer = hitStopDuration / ((BaseState)this).attackSpeedStat;
			inHitPause = true;
		}
	}

	private void FireAttack()
	{
		if (!hasFired)
		{
			hasFired = true;
			Util.PlayAttackSpeedSound(swingSoundString, ((EntityState)this).gameObject, ((BaseState)this).attackSpeedStat);
			if (((EntityState)this).isAuthority)
			{
				PlaySwingEffect();
				((BaseState)this).AddRecoil(-1f * attackRecoil, -2f * attackRecoil, -0.5f * attackRecoil, 0.5f * attackRecoil);
			}
		}
		if (((EntityState)this).isAuthority && attack.Fire((List<HurtBox>)null))
		{
			OnHitEnemyAuthority();
		}
	}

	protected virtual void SetNextState()
	{
		int num = ((swingIndex == 0) ? 1 : 0);
		((EntityState)this).outer.SetNextState((EntityState)(object)new BaseMeleeAttack
		{
			swingIndex = num
		});
	}

	public override void FixedUpdate()
	{
		//IL_0037: Unknown result type (might be due to invalid IL or missing references)
		//IL_005c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0061: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a0: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a5: Unknown result type (might be due to invalid IL or missing references)
		((EntityState)this).FixedUpdate();
		hitPauseTimer -= Time.fixedDeltaTime;
		if (hitPauseTimer <= 0f && inHitPause)
		{
			((BaseState)this).ConsumeHitStopCachedState(hitStopCachedState, ((EntityState)this).characterMotor, animator);
			inHitPause = false;
			((EntityState)this).characterMotor.velocity = storedVelocity;
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
				animator.SetFloat("Swing.playbackRate", 0f);
			}
		}
		if (stopwatch >= duration * attackStartTime && stopwatch <= duration * attackEndTime)
		{
			FireAttack();
		}
		if (stopwatch >= duration - earlyExitTime && ((EntityState)this).isAuthority && ((EntityState)this).inputBank.skill1.down)
		{
			if (!hasFired)
			{
				FireAttack();
			}
			SetNextState();
		}
		else if (stopwatch >= duration && ((EntityState)this).isAuthority)
		{
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
