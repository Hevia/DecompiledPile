using BepInEx;
using EntityStates;
using EntityStates.Merc;
using RoR2;
using UnityEngine;

namespace Rorschach;

internal class MeleeSkillState : BaseSkillState
{
	internal float hitPauseDuration;

	internal float hopVelocity = Assaulter.smallHopVelocity;

	internal string animParameter = "";

	internal float hitPauseTimer;

	internal bool isInHitPause;

	internal HitStopCachedState hitStopCachedState;

	internal Animator animator;

	internal float stopwatch;

	internal OverlapAttack attack;

	internal bool hitCallback;

	internal float damage;

	internal GameObject hitEffectPrefab;

	internal HitBoxGroup hitBoxGroup;

	internal DamageType damageType = (DamageType)0;

	internal DamageColorIndex damageColor = (DamageColorIndex)0;

	internal Vector3 forceVector = Vector3.back * 100f;

	internal float attackStopwatch;

	internal float swingDelay;

	internal float attackDuration;

	internal float stepDistance;

	internal float earlyExitDuration;

	internal float rageCoefficient;

	public override void OnEnter()
	{
		((BaseState)this).OnEnter();
		animator = ((EntityState)this).GetModelAnimator();
		isInHitPause = false;
		hitPauseDuration = GroundLight.hitPauseDuration / ((BaseState)this).attackSpeedStat;
	}

	internal OverlapAttack NewOverlapAttack()
	{
		//IL_0001: Unknown result type (might be due to invalid IL or missing references)
		//IL_0007: Expected O, but got Unknown
		//IL_000d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0042: Unknown result type (might be due to invalid IL or missing references)
		//IL_0047: Unknown result type (might be due to invalid IL or missing references)
		//IL_005a: Unknown result type (might be due to invalid IL or missing references)
		//IL_005f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0083: Unknown result type (might be due to invalid IL or missing references)
		//IL_0088: Unknown result type (might be due to invalid IL or missing references)
		//IL_008f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0094: Unknown result type (might be due to invalid IL or missing references)
		OverlapAttack val = new OverlapAttack();
		val.procChainMask = default(ProcChainMask);
		val.procCoefficient = 1f;
		val.attacker = ((EntityState)this).gameObject;
		val.inflictor = ((EntityState)this).gameObject;
		val.teamIndex = ((EntityState)this).characterBody.teamComponent.teamIndex;
		val.damage = damage;
		val.forceVector = forceVector;
		val.hitEffectPrefab = hitEffectPrefab;
		val.isCrit = ((EntityState)this).characterBody.RollCrit();
		val.damageColorIndex = damageColor;
		val.damageType = damageType;
		val.maximumOverlapTargets = 100;
		val.hitBoxGroup = hitBoxGroup;
		return val;
	}

	public override void FixedUpdate()
	{
		//IL_0096: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a1: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ac: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b1: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b6: Unknown result type (might be due to invalid IL or missing references)
		//IL_0185: Unknown result type (might be due to invalid IL or missing references)
		//IL_0132: Unknown result type (might be due to invalid IL or missing references)
		//IL_0137: Unknown result type (might be due to invalid IL or missing references)
		((EntityState)this).FixedUpdate();
		hitPauseTimer -= Time.fixedDeltaTime;
		if (stepDistance > 0f && ((EntityState)this).isAuthority && stopwatch > 0f && stopwatch <= swingDelay / ((BaseState)this).attackSpeedStat && !isInHitPause && Object.op_Implicit((Object)(object)((EntityState)this).characterMotor) && Object.op_Implicit((Object)(object)((EntityState)this).characterDirection) && ((EntityState)this).characterMotor.isGrounded)
		{
			CharacterMotor characterMotor = ((EntityState)this).characterMotor;
			characterMotor.rootMotion += ((EntityState)this).inputBank.aimDirection * stepDistance;
		}
		if (hitCallback)
		{
			CharacterMain.AddRage(rageCoefficient);
			if (!isInHitPause)
			{
				if (!((BaseState)this).isGrounded)
				{
					((BaseState)this).SmallHop(((EntityState)this).characterMotor, hopVelocity);
				}
				if (!Utility.IsNullOrWhiteSpace(animParameter))
				{
					hitStopCachedState = ((BaseState)this).CreateHitStopCachedState(((EntityState)this).characterMotor, animator, animParameter);
				}
				hitPauseTimer = hitPauseDuration / ((BaseState)this).attackSpeedStat;
				isInHitPause = true;
			}
		}
		if (!Utility.IsNullOrWhiteSpace(animParameter) && hitPauseTimer <= 0f && isInHitPause)
		{
			((BaseState)this).ConsumeHitStopCachedState(hitStopCachedState, ((EntityState)this).characterMotor, animator);
			isInHitPause = false;
		}
		if (!isInHitPause)
		{
			attackStopwatch += Time.fixedDeltaTime;
			stopwatch += Time.fixedDeltaTime;
			if (Object.op_Implicit((Object)(object)animator))
			{
				animator.speed = 1f;
			}
		}
		else if (Object.op_Implicit((Object)(object)animator))
		{
			animator.speed = 0f;
		}
	}
}
