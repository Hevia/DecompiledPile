using System;
using System.Collections.Generic;
using EntityStates;
using EntityStates.Assassin.Weapon;
using EntityStates.Merc;
using RoR2;
using UnityEngine;

namespace Rorschach;

internal class SpecialRage : MeleeSkillState
{
	private float duration;

	private float baseDuration = 0.9f;

	private float damageCoefficient = 2.5f;

	private RorschachRageBarBehaviour behaviour;

	private GameObject hitEffect = Prefabs.cleaveHitFX;

	private int attackCount = 0;

	public override void OnEnter()
	{
		base.OnEnter();
		behaviour = ((EntityState)this).GetComponent<RorschachRageBarBehaviour>();
		if (Object.op_Implicit((Object)(object)behaviour))
		{
			behaviour.canExecute = false;
		}
		((BaseState)this).StartAimMode(2f, false);
		duration = baseDuration / ((BaseState)this).attackSpeedStat;
		stepDistance = 0.65f * ((BaseState)this).attackSpeedStat;
		damage = ((BaseState)this).damageStat * damageCoefficient;
		swingDelay = 0.1f / ((BaseState)this).attackSpeedStat;
		attackDuration = 0.35f / ((BaseState)this).attackSpeedStat;
		animParameter = "Special";
		hitEffectPrefab = hitEffect;
		hitPauseDuration = 0.1f / ((BaseState)this).attackSpeedStat;
		Transform modelTransform = ((EntityState)this).GetModelTransform();
		if (Object.op_Implicit((Object)(object)((EntityState)this).GetModelTransform()))
		{
			hitBoxGroup = Array.Find(((Component)modelTransform).GetComponents<HitBoxGroup>(), (HitBoxGroup element) => element.groupName == "Punch");
		}
		attack = NewOverlapAttack();
		earlyExitDuration = GroundLight.baseEarlyExitDuration / ((BaseState)this).attackSpeedStat;
		((EntityState)this).GetModelAnimator().SetBool("skillOver", false);
		((EntityState)this).PlayAnimation("FullBody, Override", "cleaver", "Special", duration);
		Util.PlaySound(SlashCombo.attackString, ((EntityState)this).gameObject);
		attackCount++;
		PlayEffect("swingMuzzle1");
	}

	private void PlayEffect(string child)
	{
		ParticleSystem[] componentsInChildren = ((Component)((BaseState)this).FindModelChild(child)).GetComponentsInChildren<ParticleSystem>(true);
		foreach (ParticleSystem val in componentsInChildren)
		{
			if (!((Component)((Component)val).transform.parent).gameObject.activeInHierarchy)
			{
				((Component)((Component)val).transform.parent).gameObject.SetActive(true);
			}
			val.Clear();
			val.Play();
		}
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
			attackCount++;
			switch (attackCount)
			{
			case 2:
				PlayEffect("swingMuzzle2");
				break;
			case 3:
				PlayEffect("swingMuzzle3");
				break;
			}
			Util.PlaySound(SlashCombo.attackString, ((EntityState)this).gameObject);
			attack = NewOverlapAttack();
			attackStopwatch = 0f;
		}
		if (!(attackStopwatch <= attackDuration) || !((EntityState)this).isAuthority || attack.Fire((List<HurtBox>)null))
		{
		}
		if (stopwatch >= duration)
		{
			((EntityState)this).outer.SetNextStateToMain();
		}
	}

	public override void OnExit()
	{
		((EntityState)this).OnExit();
		if (Object.op_Implicit((Object)(object)behaviour))
		{
			behaviour.canExecute = true;
		}
		((EntityState)this).GetModelAnimator().SetBool("skillOver", true);
	}

	public override InterruptPriority GetMinimumInterruptPriority()
	{
		//IL_0002: Unknown result type (might be due to invalid IL or missing references)
		//IL_0005: Unknown result type (might be due to invalid IL or missing references)
		return (InterruptPriority)2;
	}
}
