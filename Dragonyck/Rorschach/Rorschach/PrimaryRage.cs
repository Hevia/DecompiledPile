using System;
using System.Collections.Generic;
using EntityStates;
using EntityStates.Merc;
using RoR2;
using UnityEngine;

namespace Rorschach;

internal class PrimaryRage : MeleeSkillState
{
	private float duration;

	private float baseDuration = 2f;

	private float damageCoefficient = 2.5f;

	public override void OnEnter()
	{
		base.OnEnter();
		((BaseState)this).StartAimMode(2f, false);
		duration = baseDuration / ((BaseState)this).attackSpeedStat;
		stepDistance = 0.4f * ((BaseState)this).attackSpeedStat;
		damage = ((BaseState)this).damageStat * damageCoefficient;
		swingDelay = 0.15f / ((BaseState)this).attackSpeedStat;
		attackDuration = 0.25f / ((BaseState)this).attackSpeedStat;
		animParameter = "M1";
		hitEffectPrefab = Prefabs.punchHitFX;
		Transform modelTransform = ((EntityState)this).GetModelTransform();
		if (Object.op_Implicit((Object)(object)((EntityState)this).GetModelTransform()))
		{
			hitBoxGroup = Array.Find(((Component)modelTransform).GetComponents<HitBoxGroup>(), (HitBoxGroup element) => element.groupName == "Punch");
		}
		attack = NewOverlapAttack();
		earlyExitDuration = GroundLight.baseEarlyExitDuration / ((BaseState)this).attackSpeedStat;
		((EntityState)this).GetModelAnimator().SetBool("skillOver", false);
		((EntityState)this).GetModelAnimator().SetFloat("M1", 1f / ((BaseState)this).attackSpeedStat);
		((EntityState)this).PlayAnimation("FullBody, Override", "rage_m1");
		AkSoundEngine.PostEvent(Sounds.Play_Rorschach_Punch_Swing, ((EntityState)this).gameObject);
	}

	public override void FixedUpdate()
	{
		//IL_0015: Unknown result type (might be due to invalid IL or missing references)
		//IL_001a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0031: Unknown result type (might be due to invalid IL or missing references)
		//IL_0036: Unknown result type (might be due to invalid IL or missing references)
		//IL_003b: Unknown result type (might be due to invalid IL or missing references)
		//IL_003c: Unknown result type (might be due to invalid IL or missing references)
		//IL_003d: Unknown result type (might be due to invalid IL or missing references)
		//IL_008e: Unknown result type (might be due to invalid IL or missing references)
		//IL_008f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0054: Unknown result type (might be due to invalid IL or missing references)
		//IL_005f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0065: Unknown result type (might be due to invalid IL or missing references)
		//IL_006a: Unknown result type (might be due to invalid IL or missing references)
		//IL_006e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0073: Unknown result type (might be due to invalid IL or missing references)
		base.FixedUpdate();
		((EntityState)this).characterBody.isSprinting = false;
		Vector3 moveVector = Vector3.one;
		if (Object.op_Implicit((Object)(object)((EntityState)this).inputBank))
		{
			Vector2 val = Util.Vector3XZToVector2XY(((EntityState)this).inputBank.aimDirection);
			if (val != Vector2.zero)
			{
				((Vector2)(ref val)).Normalize();
				Vector3 val2 = new Vector3(val.x, 0f, val.y);
				moveVector = ((Vector3)(ref val2)).normalized;
			}
		}
		if (Object.op_Implicit((Object)(object)((EntityState)this).characterDirection))
		{
			((EntityState)this).characterDirection.moveVector = moveVector;
		}
		if (!((EntityState)this).isAuthority)
		{
			return;
		}
		if (attackStopwatch >= attackDuration)
		{
			AkSoundEngine.PostEvent(Sounds.Play_Rorschach_Punch_Swing, ((EntityState)this).gameObject);
			attack = NewOverlapAttack();
			attackStopwatch = 0f;
		}
		if (attackStopwatch <= attackDuration && ((EntityState)this).isAuthority)
		{
			hitCallback = attack.Fire((List<HurtBox>)null);
			if (!hitCallback)
			{
			}
		}
		if (stopwatch >= duration)
		{
			((EntityState)this).outer.SetNextStateToMain();
		}
	}

	public override void OnExit()
	{
		((EntityState)this).OnExit();
		((EntityState)this).GetModelAnimator().SetBool("skillOver", true);
	}

	public override InterruptPriority GetMinimumInterruptPriority()
	{
		//IL_0002: Unknown result type (might be due to invalid IL or missing references)
		//IL_0005: Unknown result type (might be due to invalid IL or missing references)
		return (InterruptPriority)2;
	}
}
