using System;
using System.Collections.Generic;
using EntityStates;
using EntityStates.Merc;
using RoR2;
using UnityEngine;
using UnityEngine.Networking;

namespace Rorschach;

internal class Primary : MeleeSkillState
{
	internal enum AttackState
	{
		Attack1,
		Attack2,
		Attack3,
		Attack4,
		Attack5
	}

	private float duration;

	private float baseDuration = 0.55f;

	private float damageCoefficient = 2f;

	internal AttackState comboState;

	public override void OnEnter()
	{
		base.OnEnter();
		((BaseState)this).StartAimMode(2f, false);
		rageCoefficient = 0.02f;
		duration = baseDuration / ((BaseState)this).attackSpeedStat;
		stepDistance = 0.15f * ((BaseState)this).attackSpeedStat;
		damage = ((BaseState)this).damageStat * damageCoefficient;
		swingDelay = 0.15f / ((BaseState)this).attackSpeedStat;
		attackDuration = 0.37f / ((BaseState)this).attackSpeedStat;
		animParameter = "M1";
		hitEffectPrefab = Prefabs.punchHitFX;
		hitPauseDuration = 0.1f / ((BaseState)this).attackSpeedStat;
		Transform modelTransform = ((EntityState)this).GetModelTransform();
		if (Object.op_Implicit((Object)(object)((EntityState)this).GetModelTransform()))
		{
			hitBoxGroup = Array.Find(((Component)modelTransform).GetComponents<HitBoxGroup>(), (HitBoxGroup element) => element.groupName == "Punch");
		}
		attack = NewOverlapAttack();
		earlyExitDuration = GroundLight.baseEarlyExitDuration / ((BaseState)this).attackSpeedStat;
		string text = "";
		switch (comboState)
		{
		case AttackState.Attack1:
			text = "M1-1";
			break;
		case AttackState.Attack2:
			text = "M1-2";
			break;
		case AttackState.Attack3:
			text = "M1-3";
			break;
		case AttackState.Attack4:
			text = "M1-4";
			break;
		case AttackState.Attack5:
			text = "M1-5";
			break;
		}
		((EntityState)this).PlayAnimation("FullBody, Override", text, "M1", attackDuration);
		if (!((BaseState)this).isGrounded || !((EntityState)this).characterBody.GetNotMoving())
		{
		}
		AkSoundEngine.PostEvent(Sounds.Play_Rorschach_Punch_Swing, ((EntityState)this).gameObject);
	}

	public override void FixedUpdate()
	{
		base.FixedUpdate();
		((EntityState)this).characterBody.isSprinting = false;
		if (((EntityState)this).isAuthority)
		{
			if (stopwatch >= swingDelay / ((BaseState)this).attackSpeedStat && stopwatch <= attackDuration + swingDelay)
			{
				hitCallback = attack.Fire((List<HurtBox>)null);
			}
			if (((EntityState)this).inputBank.skill1.down && comboState != AttackState.Attack5 && stopwatch >= attackDuration + swingDelay - earlyExitDuration)
			{
				Primary primary = new Primary();
				primary.comboState = comboState + 1;
				((EntityState)this).outer.SetNextState((EntityState)(object)primary);
			}
			else if (stopwatch >= duration)
			{
				((EntityState)this).outer.SetNextStateToMain();
			}
		}
	}

	public override void OnExit()
	{
		((EntityState)this).OnExit();
	}

	public override InterruptPriority GetMinimumInterruptPriority()
	{
		//IL_0002: Unknown result type (might be due to invalid IL or missing references)
		//IL_0005: Unknown result type (might be due to invalid IL or missing references)
		return (InterruptPriority)2;
	}

	public override void OnSerialize(NetworkWriter writer)
	{
		((BaseSkillState)this).OnSerialize(writer);
		writer.Write((byte)comboState);
	}

	public override void OnDeserialize(NetworkReader reader)
	{
		((BaseSkillState)this).OnDeserialize(reader);
		comboState = (AttackState)reader.ReadByte();
	}
}
