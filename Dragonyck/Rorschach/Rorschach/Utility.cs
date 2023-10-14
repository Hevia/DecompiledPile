using System;
using EntityStates;
using EntityStates.Engi.EngiWeapon;
using RoR2;
using RoR2.Projectile;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.Networking;

namespace Rorschach;

internal class Utility : MeleeSkillState
{
	private float maxDuration = 1.1f;

	private bool collision;

	private bool bodyCollision;

	private float dashVelocity = 2.5f;

	private Vector3 dashVector;

	private Vector3 hookPosition;

	private bool noCollision;

	private bool collisionRageAdded;

	private HealthComponent healthComponent;

	private RorschachRageBarBehaviour behaviour;

	public override void OnEnter()
	{
		//IL_00c9: Unknown result type (might be due to invalid IL or missing references)
		//IL_00f1: Unknown result type (might be due to invalid IL or missing references)
		//IL_0120: Unknown result type (might be due to invalid IL or missing references)
		//IL_0125: Unknown result type (might be due to invalid IL or missing references)
		//IL_0129: Unknown result type (might be due to invalid IL or missing references)
		//IL_012e: Unknown result type (might be due to invalid IL or missing references)
		//IL_013a: Unknown result type (might be due to invalid IL or missing references)
		//IL_014f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0154: Unknown result type (might be due to invalid IL or missing references)
		//IL_0158: Unknown result type (might be due to invalid IL or missing references)
		//IL_015d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0162: Unknown result type (might be due to invalid IL or missing references)
		//IL_017f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0181: Unknown result type (might be due to invalid IL or missing references)
		//IL_0187: Unknown result type (might be due to invalid IL or missing references)
		//IL_0195: Unknown result type (might be due to invalid IL or missing references)
		//IL_019a: Unknown result type (might be due to invalid IL or missing references)
		//IL_01a6: Unknown result type (might be due to invalid IL or missing references)
		//IL_0220: Unknown result type (might be due to invalid IL or missing references)
		//IL_0225: Unknown result type (might be due to invalid IL or missing references)
		base.OnEnter();
		behaviour = ((EntityState)this).GetComponent<RorschachRageBarBehaviour>();
		if (Object.op_Implicit((Object)(object)behaviour))
		{
			behaviour.canExecute = false;
		}
		stepDistance = 0.15f * ((BaseState)this).attackSpeedStat;
		damage = ((BaseState)this).damageStat;
		swingDelay = 0.15f / ((BaseState)this).attackSpeedStat;
		attackDuration = 0.37f / ((BaseState)this).attackSpeedStat;
		Transform modelTransform = ((EntityState)this).GetModelTransform();
		if (Object.op_Implicit((Object)(object)((EntityState)this).GetModelTransform()))
		{
			hitBoxGroup = Array.Find(((Component)modelTransform).GetComponents<HitBoxGroup>(), (HitBoxGroup element) => element.groupName == "Kick");
		}
		attack = NewOverlapAttack();
		FireProjectileInfo val = default(FireProjectileInfo);
		val.crit = ((BaseState)this).RollCrit();
		val.damage = ((EntityState)this).characterBody.damage;
		val.damageColorIndex = (DamageColorIndex)0;
		val.damageTypeOverride = (DamageType)32;
		val.force = 0f;
		val.owner = ((EntityState)this).gameObject;
		Ray aimRay = ((BaseState)this).GetAimRay();
		val.position = ((Ray)(ref aimRay)).origin;
		val.procChainMask = default(ProcChainMask);
		val.projectilePrefab = Prefabs.utilityProjectile;
		aimRay = ((BaseState)this).GetAimRay();
		val.rotation = Util.QuaternionSafeLookRotation(((Ray)(ref aimRay)).direction);
		val.useFuseOverride = false;
		val.useSpeedOverride = false;
		val.target = null;
		FireProjectileInfo val2 = val;
		ProjectileManager.instance.FireProjectile(val2);
		dashVector = ((EntityState)this).inputBank.aimDirection;
		((EntityState)this).characterDirection.forward = dashVector;
		((EntityState)this).GetModelAnimator().SetBool("hit", false);
		((EntityState)this).GetModelAnimator().SetBool("miss", false);
		((EntityState)this).GetModelAnimator().SetBool("kick", false);
		((EntityState)this).GetModelAnimator().SetBool("ground", false);
		((EntityState)this).PlayAnimation("FullBody, Override", "hook");
		Util.PlaySound(FireGrenades.attackSoundString, ((EntityState)this).gameObject);
		EffectManager.SimpleMuzzleFlash(Addressables.LoadAssetAsync<GameObject>((object)"RoR2/Base/Common/VFX/MuzzleflashSmokeRing.prefab").WaitForCompletion(), ((EntityState)this).gameObject, "hookFireMuzzle", false);
	}

	internal void NoCollision()
	{
		noCollision = true;
		((EntityState)this).GetModelAnimator().SetBool("miss", true);
	}

	internal void HookCollision(bool entityCollision, Vector3 vector, HealthComponent component = null)
	{
		//IL_001b: Unknown result type (might be due to invalid IL or missing references)
		//IL_001c: Unknown result type (might be due to invalid IL or missing references)
		collision = true;
		((EntityState)this).GetModelAnimator().SetBool("hit", true);
		hookPosition = vector;
		if (Object.op_Implicit((Object)(object)component))
		{
			healthComponent = component;
		}
		if (entityCollision)
		{
			bodyCollision = true;
			((EntityState)this).GetModelAnimator().SetBool("kick", true);
			((EntityState)this).characterBody.outOfCombatStopwatch = 0f;
		}
		else
		{
			((EntityState)this).GetModelAnimator().SetBool("ground", true);
		}
	}

	public override void FixedUpdate()
	{
		//IL_002b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0030: Unknown result type (might be due to invalid IL or missing references)
		//IL_010d: Unknown result type (might be due to invalid IL or missing references)
		//IL_011e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0124: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ad: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b2: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b7: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b9: Unknown result type (might be due to invalid IL or missing references)
		//IL_00bb: Unknown result type (might be due to invalid IL or missing references)
		//IL_0143: Unknown result type (might be due to invalid IL or missing references)
		//IL_0149: Unknown result type (might be due to invalid IL or missing references)
		//IL_0154: Unknown result type (might be due to invalid IL or missing references)
		//IL_0159: Unknown result type (might be due to invalid IL or missing references)
		//IL_015e: Unknown result type (might be due to invalid IL or missing references)
		//IL_00d4: Unknown result type (might be due to invalid IL or missing references)
		//IL_00e0: Unknown result type (might be due to invalid IL or missing references)
		//IL_00e7: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ec: Unknown result type (might be due to invalid IL or missing references)
		//IL_00f0: Unknown result type (might be due to invalid IL or missing references)
		//IL_00f5: Unknown result type (might be due to invalid IL or missing references)
		//IL_00fd: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ff: Unknown result type (might be due to invalid IL or missing references)
		//IL_01c2: Unknown result type (might be due to invalid IL or missing references)
		//IL_01c7: Unknown result type (might be due to invalid IL or missing references)
		//IL_01d3: Unknown result type (might be due to invalid IL or missing references)
		//IL_01df: Unknown result type (might be due to invalid IL or missing references)
		//IL_01f1: Unknown result type (might be due to invalid IL or missing references)
		//IL_01fc: Unknown result type (might be due to invalid IL or missing references)
		//IL_01fe: Unknown result type (might be due to invalid IL or missing references)
		//IL_0203: Unknown result type (might be due to invalid IL or missing references)
		//IL_0205: Unknown result type (might be due to invalid IL or missing references)
		//IL_020a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0216: Unknown result type (might be due to invalid IL or missing references)
		//IL_021b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0220: Unknown result type (might be due to invalid IL or missing references)
		//IL_0229: Expected O, but got Unknown
		//IL_02aa: Unknown result type (might be due to invalid IL or missing references)
		//IL_02af: Unknown result type (might be due to invalid IL or missing references)
		//IL_02bb: Unknown result type (might be due to invalid IL or missing references)
		//IL_02c7: Unknown result type (might be due to invalid IL or missing references)
		//IL_02d9: Unknown result type (might be due to invalid IL or missing references)
		//IL_02e4: Unknown result type (might be due to invalid IL or missing references)
		//IL_02e6: Unknown result type (might be due to invalid IL or missing references)
		//IL_02eb: Unknown result type (might be due to invalid IL or missing references)
		//IL_02ed: Unknown result type (might be due to invalid IL or missing references)
		//IL_02f2: Unknown result type (might be due to invalid IL or missing references)
		//IL_02fe: Unknown result type (might be due to invalid IL or missing references)
		//IL_0303: Unknown result type (might be due to invalid IL or missing references)
		//IL_0308: Unknown result type (might be due to invalid IL or missing references)
		//IL_0311: Expected O, but got Unknown
		base.FixedUpdate();
		((EntityState)this).characterBody.isSprinting = false;
		if (Object.op_Implicit((Object)(object)((EntityState)this).characterMotor))
		{
			((EntityState)this).characterMotor.velocity = Vector3.zero;
		}
		if (collision || !(((EntityState)this).fixedAge < 0.2f) || !Object.op_Implicit((Object)(object)((EntityState)this).characterMotor) || ((EntityState)this).characterMotor.isGrounded)
		{
		}
		if (collision && Object.op_Implicit((Object)(object)((EntityState)this).characterMotor) && Object.op_Implicit((Object)(object)((EntityState)this).characterDirection))
		{
			if (Object.op_Implicit((Object)(object)((EntityState)this).inputBank))
			{
				Vector2 val = Util.Vector3XZToVector2XY(((EntityState)this).inputBank.moveVector);
				if (val != Vector2.zero)
				{
					((Vector2)(ref val)).Normalize();
					Vector3 val2 = new Vector3(val.x, 0f, val.y);
					Vector3 normalized = ((Vector3)(ref val2)).normalized;
					((EntityState)this).characterDirection.moveVector = normalized;
				}
			}
			((EntityState)this).characterDirection.forward = dashVector;
			if (Vector3.Distance(((EntityState)this).transform.position, hookPosition) > 7f)
			{
				CharacterMotor characterMotor = ((EntityState)this).characterMotor;
				characterMotor.rootMotion += dashVector * dashVelocity;
			}
			else
			{
				if (bodyCollision && !collisionRageAdded)
				{
					collisionRageAdded = true;
					if (Object.op_Implicit((Object)(object)healthComponent) && Object.op_Implicit((Object)(object)healthComponent.body) && NetworkServer.active)
					{
						DamageInfo val3 = new DamageInfo
						{
							attacker = ((EntityState)this).gameObject,
							inflictor = ((EntityState)this).gameObject,
							damage = damage * 2f,
							procCoefficient = 0f,
							damageType = (DamageType)0,
							damageColorIndex = (DamageColorIndex)3,
							position = healthComponent.body.corePosition,
							rejected = false
						};
						healthComponent.TakeDamage(val3);
					}
				}
				((EntityState)this).outer.SetNextStateToMain();
			}
			if (bodyCollision && !collisionRageAdded)
			{
				collisionRageAdded = true;
				CharacterMain.AddRage(0.08f);
				if (Object.op_Implicit((Object)(object)healthComponent) && Object.op_Implicit((Object)(object)healthComponent.body) && NetworkServer.active)
				{
					DamageInfo val4 = new DamageInfo
					{
						attacker = ((EntityState)this).gameObject,
						inflictor = ((EntityState)this).gameObject,
						damage = damage * 3.5f,
						procCoefficient = 0f,
						damageType = (DamageType)0,
						damageColorIndex = (DamageColorIndex)3,
						position = healthComponent.body.corePosition,
						rejected = false
					};
					healthComponent.TakeDamage(val4);
				}
			}
		}
		if (!collision && noCollision && ((EntityState)this).isAuthority)
		{
			((EntityState)this).outer.SetNextStateToMain();
		}
		if (((EntityState)this).fixedAge >= maxDuration && ((EntityState)this).isAuthority)
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
	}

	public override InterruptPriority GetMinimumInterruptPriority()
	{
		//IL_0002: Unknown result type (might be due to invalid IL or missing references)
		//IL_0005: Unknown result type (might be due to invalid IL or missing references)
		return (InterruptPriority)1;
	}
}
