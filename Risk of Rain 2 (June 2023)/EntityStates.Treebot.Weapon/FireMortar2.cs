using RoR2;
using RoR2.Projectile;
using UnityEngine;
using UnityEngine.Networking;

namespace EntityStates.Treebot.Weapon;

public class FireMortar2 : BaseState
{
	public static float baseDuration;

	public static GameObject projectilePrefab;

	public static string fireSound;

	public static float maxDistance;

	public static float damageCoefficient;

	public static float force;

	public static string muzzleName;

	public static GameObject muzzleEffect;

	public static float healthCostFraction;

	private float duration;

	public override void OnEnter()
	{
		//IL_009d: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a2: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a8: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ad: Unknown result type (might be due to invalid IL or missing references)
		base.OnEnter();
		duration = baseDuration / attackSpeedStat;
		EffectManager.SimpleMuzzleFlash(muzzleEffect, base.gameObject, muzzleName, transmit: false);
		Util.PlaySound(fireSound, base.gameObject);
		PlayCrossfade("Gesture, Additive", "FireBomb", 0.1f);
		if (base.isAuthority)
		{
			Fire();
		}
		if (NetworkServer.active && Object.op_Implicit((Object)(object)base.healthComponent))
		{
			DamageInfo damageInfo = new DamageInfo();
			damageInfo.damage = base.healthComponent.combinedHealth * healthCostFraction;
			damageInfo.position = base.characterBody.corePosition;
			damageInfo.force = Vector3.zero;
			damageInfo.damageColorIndex = DamageColorIndex.Default;
			damageInfo.crit = false;
			damageInfo.attacker = null;
			damageInfo.inflictor = null;
			damageInfo.damageType = DamageType.NonLethal | DamageType.BypassArmor;
			damageInfo.procCoefficient = 0f;
			damageInfo.procChainMask = default(ProcChainMask);
			base.healthComponent.TakeDamage(damageInfo);
		}
	}

	public override void FixedUpdate()
	{
		base.FixedUpdate();
		if (base.isAuthority && base.fixedAge >= duration)
		{
			outer.SetNextStateToMain();
		}
	}

	private void Fire()
	{
		//IL_0016: Unknown result type (might be due to invalid IL or missing references)
		//IL_001b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0024: Unknown result type (might be due to invalid IL or missing references)
		//IL_0029: Unknown result type (might be due to invalid IL or missing references)
		//IL_0031: Unknown result type (might be due to invalid IL or missing references)
		//IL_0036: Unknown result type (might be due to invalid IL or missing references)
		//IL_004d: Unknown result type (might be due to invalid IL or missing references)
		//IL_004e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0055: Unknown result type (might be due to invalid IL or missing references)
		//IL_005a: Unknown result type (might be due to invalid IL or missing references)
		Vector3 point;
		if (base.inputBank.GetAimRaycast(maxDistance, out var hitInfo))
		{
			point = ((RaycastHit)(ref hitInfo)).point;
		}
		else
		{
			Ray aimRay = base.inputBank.GetAimRay();
			point = aimRay.GetPoint(maxDistance);
		}
		FireProjectileInfo fireProjectileInfo = default(FireProjectileInfo);
		fireProjectileInfo.projectilePrefab = projectilePrefab;
		fireProjectileInfo.position = point;
		fireProjectileInfo.rotation = Quaternion.identity;
		fireProjectileInfo.owner = base.gameObject;
		fireProjectileInfo.damage = damageCoefficient * damageStat;
		fireProjectileInfo.force = force;
		fireProjectileInfo.crit = RollCrit();
		fireProjectileInfo.damageColorIndex = DamageColorIndex.Default;
		fireProjectileInfo.target = null;
		fireProjectileInfo.speedOverride = 0f;
		fireProjectileInfo.fuseOverride = -1f;
		FireProjectileInfo fireProjectileInfo2 = fireProjectileInfo;
		ProjectileManager.instance.FireProjectile(fireProjectileInfo2);
	}

	public override InterruptPriority GetMinimumInterruptPriority()
	{
		return InterruptPriority.PrioritySkill;
	}
}
