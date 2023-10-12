using EntityStates;
using EntityStates.ClayBoss.ClayBossWeapon;
using EntityStates.Commando.CommandoWeapon;
using PaladinMod.Misc;
using RoR2;
using RoR2.Projectile;
using UnityEngine;

namespace IndiesSkills.MyEntityStates;

public class ThrowTarBomb : BaseSkillState
{
	public GameObject projectilePrefab = FireBombardment.projectilePrefab;

	public GameObject muzzleflashEffectPrefab = FirePistol2.muzzleEffectPrefab;

	public float baseDuration = 0.3f;

	public float force = 1000f;

	public float damageCoeff = 1.2f;

	private float duration = 2.5f;

	private PaladinSwordController swordController;

	public override void OnEnter()
	{
		((BaseState)this).OnEnter();
		duration = baseDuration / ((BaseState)this).attackSpeedStat;
		swordController = ((EntityState)this).GetComponent<PaladinSwordController>();
		if (Object.op_Implicit((Object)(object)swordController))
		{
			swordController.attacking = true;
		}
		((EntityState)this).PlayAnimation("Gesture, Override", "ThrowSpell", "ChargeSpell.playbackRate", duration);
		if (Object.op_Implicit((Object)(object)muzzleflashEffectPrefab))
		{
			EffectManager.SimpleMuzzleFlash(muzzleflashEffectPrefab, ((EntityState)this).gameObject, "HandL", false);
		}
		Util.PlaySound(FireBombardment.shootSoundString, ((EntityState)this).gameObject);
		Fire();
	}

	public override void FixedUpdate()
	{
		((EntityState)this).FixedUpdate();
		if (((EntityState)this).isAuthority && ((EntityState)this).fixedAge >= duration)
		{
			((EntityState)this).outer.SetNextStateToMain();
		}
	}

	public override void OnExit()
	{
		((EntityState)this).OnExit();
		if (Object.op_Implicit((Object)(object)swordController))
		{
			swordController.attacking = false;
		}
	}

	private void Fire()
	{
		if (!((EntityState)this).isAuthority)
		{
			return;
		}
		Ray aimRay = ((BaseState)this).GetAimRay();
		if ((Object)(object)projectilePrefab != (Object)null)
		{
			FireProjectileInfo val = default(FireProjectileInfo);
			val.projectilePrefab = projectilePrefab;
			val.position = ((Ray)(ref aimRay)).origin;
			val.rotation = Util.QuaternionSafeLookRotation(((Ray)(ref aimRay)).direction);
			val.owner = ((EntityState)this).gameObject;
			val.damage = ((BaseState)this).damageStat * damageCoeff;
			val.damageTypeOverride = (DamageType)512;
			val.force = force;
			val.crit = ((BaseState)this).RollCrit();
			((FireProjectileInfo)(ref val)).speedOverride = 120f;
			FireProjectileInfo val2 = val;
			val = default(FireProjectileInfo);
			val.projectilePrefab = projectilePrefab;
			val.position = ((Ray)(ref aimRay)).origin;
			val.rotation = Util.QuaternionSafeLookRotation(((Ray)(ref aimRay)).direction);
			val.owner = ((EntityState)this).gameObject;
			val.damage = ((BaseState)this).damageStat * damageCoeff;
			val.damageTypeOverride = (DamageType)512;
			val.force = force * 0.75f;
			val.crit = ((BaseState)this).RollCrit();
			((FireProjectileInfo)(ref val)).speedOverride = 90f;
			FireProjectileInfo val3 = val;
			val = default(FireProjectileInfo);
			val.projectilePrefab = projectilePrefab;
			val.position = ((Ray)(ref aimRay)).origin;
			val.rotation = Util.QuaternionSafeLookRotation(((Ray)(ref aimRay)).direction);
			val.owner = ((EntityState)this).gameObject;
			val.damage = ((BaseState)this).damageStat * damageCoeff;
			val.damageTypeOverride = (DamageType)512;
			val.force = force * 0.5f;
			val.crit = ((BaseState)this).RollCrit();
			((FireProjectileInfo)(ref val)).speedOverride = 60f;
			FireProjectileInfo val4 = val;
			val = default(FireProjectileInfo);
			val.projectilePrefab = projectilePrefab;
			val.position = ((Ray)(ref aimRay)).origin;
			val.rotation = Util.QuaternionSafeLookRotation(Util.ApplySpread(((Ray)(ref aimRay)).direction, 0f, 15f, 1f, 1f, 0f, 0f));
			val.owner = ((EntityState)this).gameObject;
			val.damage = ((BaseState)this).damageStat * damageCoeff * 0.5f;
			val.damageTypeOverride = (DamageType)512;
			val.force = force;
			val.crit = ((BaseState)this).RollCrit();
			((FireProjectileInfo)(ref val)).speedOverride = 120f;
			FireProjectileInfo val5 = val;
			val = default(FireProjectileInfo);
			val.projectilePrefab = projectilePrefab;
			val.position = ((Ray)(ref aimRay)).origin;
			val.rotation = Util.QuaternionSafeLookRotation(Util.ApplySpread(((Ray)(ref aimRay)).direction, 0f, 15f, 1f, 1f, 0f, 0f));
			val.owner = ((EntityState)this).gameObject;
			val.damage = ((BaseState)this).damageStat * damageCoeff * 0.5f;
			val.damageTypeOverride = (DamageType)512;
			val.force = force * 0.75f;
			val.crit = ((BaseState)this).RollCrit();
			((FireProjectileInfo)(ref val)).speedOverride = 90f;
			FireProjectileInfo val6 = val;
			val = default(FireProjectileInfo);
			val.projectilePrefab = projectilePrefab;
			val.position = ((Ray)(ref aimRay)).origin;
			val.rotation = Util.QuaternionSafeLookRotation(Util.ApplySpread(((Ray)(ref aimRay)).direction, 0f, 10f, 1f, 1f, 0f, 0f));
			val.owner = ((EntityState)this).gameObject;
			val.damage = ((BaseState)this).damageStat * damageCoeff * 0.5f;
			val.damageTypeOverride = (DamageType)512;
			val.force = force * 0.5f;
			val.crit = ((BaseState)this).RollCrit();
			((FireProjectileInfo)(ref val)).speedOverride = 60f;
			FireProjectileInfo val7 = val;
			val = default(FireProjectileInfo);
			val.projectilePrefab = projectilePrefab;
			val.position = ((Ray)(ref aimRay)).origin;
			val.rotation = Util.QuaternionSafeLookRotation(Util.ApplySpread(((Ray)(ref aimRay)).direction, 0f, 15f, 1f, 1f, 0f, 0f));
			val.owner = ((EntityState)this).gameObject;
			val.damage = ((BaseState)this).damageStat * damageCoeff * 0.5f;
			val.damageTypeOverride = (DamageType)512;
			val.force = force;
			val.crit = ((BaseState)this).RollCrit();
			((FireProjectileInfo)(ref val)).speedOverride = 120f;
			FireProjectileInfo val8 = val;
			val = default(FireProjectileInfo);
			val.projectilePrefab = projectilePrefab;
			val.position = ((Ray)(ref aimRay)).origin;
			val.rotation = Util.QuaternionSafeLookRotation(Util.ApplySpread(((Ray)(ref aimRay)).direction, 0f, 15f, 1f, 1f, 0f, 0f));
			val.owner = ((EntityState)this).gameObject;
			val.damage = ((BaseState)this).damageStat * damageCoeff * 0.5f;
			val.damageTypeOverride = (DamageType)512;
			val.force = force * 0.75f;
			val.crit = ((BaseState)this).RollCrit();
			((FireProjectileInfo)(ref val)).speedOverride = 90f;
			FireProjectileInfo val9 = val;
			val = default(FireProjectileInfo);
			val.projectilePrefab = projectilePrefab;
			val.position = ((Ray)(ref aimRay)).origin;
			val.rotation = Util.QuaternionSafeLookRotation(Util.ApplySpread(((Ray)(ref aimRay)).direction, 0f, 15f, 1f, 1f, 0f, 0f));
			val.owner = ((EntityState)this).gameObject;
			val.damage = ((BaseState)this).damageStat * damageCoeff * 0.5f;
			val.damageTypeOverride = (DamageType)512;
			val.force = force * 0.5f;
			val.crit = ((BaseState)this).RollCrit();
			((FireProjectileInfo)(ref val)).speedOverride = 60f;
			FireProjectileInfo val10 = val;
			ProjectileManager.instance.FireProjectile(val2);
			ProjectileManager.instance.FireProjectile(val3);
			ProjectileManager.instance.FireProjectile(val4);
			if ((double)((EntityState)this).characterBody.healthComponent.health >= (double)((EntityState)this).characterBody.maxHealth * 0.9 || ((EntityState)this).characterBody.healthComponent.barrier > 0f)
			{
				ProjectileManager.instance.FireProjectile(val5);
				ProjectileManager.instance.FireProjectile(val6);
				ProjectileManager.instance.FireProjectile(val7);
				ProjectileManager.instance.FireProjectile(val8);
				ProjectileManager.instance.FireProjectile(val9);
				ProjectileManager.instance.FireProjectile(val10);
			}
		}
	}

	public override InterruptPriority GetMinimumInterruptPriority()
	{
		//IL_0002: Unknown result type (might be due to invalid IL or missing references)
		//IL_0005: Unknown result type (might be due to invalid IL or missing references)
		return (InterruptPriority)2;
	}
}
