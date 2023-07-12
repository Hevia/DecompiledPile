using RoR2;
using RoR2.Projectile;
using UnityEngine;

namespace EntityStates.Loader;

public class ThrowPylon : BaseState
{
	public static GameObject projectilePrefab;

	public static float baseDuration;

	public static float damageCoefficient;

	public static string muzzleString;

	public static GameObject muzzleflashObject;

	public static string soundString;

	private float duration;

	public override void OnEnter()
	{
		//IL_0024: Unknown result type (might be due to invalid IL or missing references)
		//IL_0029: Unknown result type (might be due to invalid IL or missing references)
		//IL_0077: Unknown result type (might be due to invalid IL or missing references)
		//IL_007c: Unknown result type (might be due to invalid IL or missing references)
		//IL_009e: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a3: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a8: Unknown result type (might be due to invalid IL or missing references)
		base.OnEnter();
		duration = baseDuration / attackSpeedStat;
		if (base.isAuthority)
		{
			Ray aimRay = GetAimRay();
			FireProjectileInfo fireProjectileInfo = default(FireProjectileInfo);
			fireProjectileInfo.crit = RollCrit();
			fireProjectileInfo.damage = damageStat * damageCoefficient;
			fireProjectileInfo.damageColorIndex = DamageColorIndex.Default;
			fireProjectileInfo.force = 0f;
			fireProjectileInfo.owner = base.gameObject;
			fireProjectileInfo.position = ((Ray)(ref aimRay)).origin;
			fireProjectileInfo.procChainMask = default(ProcChainMask);
			fireProjectileInfo.projectilePrefab = projectilePrefab;
			fireProjectileInfo.rotation = Quaternion.LookRotation(((Ray)(ref aimRay)).direction);
			fireProjectileInfo.target = null;
			FireProjectileInfo fireProjectileInfo2 = fireProjectileInfo;
			ProjectileManager.instance.FireProjectile(fireProjectileInfo2);
		}
		EffectManager.SimpleMuzzleFlash(muzzleflashObject, base.gameObject, muzzleString, transmit: false);
		Util.PlaySound(soundString, base.gameObject);
	}

	public override void FixedUpdate()
	{
		base.FixedUpdate();
		if (base.isAuthority && duration <= base.age)
		{
			outer.SetNextStateToMain();
		}
	}

	public override InterruptPriority GetMinimumInterruptPriority()
	{
		return InterruptPriority.Frozen;
	}
}
