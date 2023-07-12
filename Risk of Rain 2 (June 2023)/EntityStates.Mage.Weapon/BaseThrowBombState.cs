using RoR2;
using RoR2.Projectile;
using UnityEngine;

namespace EntityStates.Mage.Weapon;

public abstract class BaseThrowBombState : BaseState
{
	[SerializeField]
	public GameObject projectilePrefab;

	[SerializeField]
	public GameObject muzzleflashEffectPrefab;

	[SerializeField]
	public float baseDuration;

	[SerializeField]
	public float minDamageCoefficient;

	[SerializeField]
	public float maxDamageCoefficient;

	[SerializeField]
	public float force;

	[SerializeField]
	public float selfForce;

	protected float duration;

	public float charge;

	public override void OnEnter()
	{
		base.OnEnter();
		duration = baseDuration / attackSpeedStat;
		PlayThrowAnimation();
		if (Object.op_Implicit((Object)(object)muzzleflashEffectPrefab))
		{
			EffectManager.SimpleMuzzleFlash(muzzleflashEffectPrefab, base.gameObject, "MuzzleLeft", transmit: false);
			EffectManager.SimpleMuzzleFlash(muzzleflashEffectPrefab, base.gameObject, "MuzzleRight", transmit: false);
		}
		Fire();
	}

	public override void FixedUpdate()
	{
		base.FixedUpdate();
		if (base.isAuthority && base.fixedAge >= duration)
		{
			outer.SetNextStateToMain();
		}
	}

	public override void OnExit()
	{
		base.OnExit();
	}

	private void Fire()
	{
		//IL_000c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0011: Unknown result type (might be due to invalid IL or missing references)
		//IL_006c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0071: Unknown result type (might be due to invalid IL or missing references)
		//IL_007a: Unknown result type (might be due to invalid IL or missing references)
		//IL_007f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0084: Unknown result type (might be due to invalid IL or missing references)
		//IL_00e5: Unknown result type (might be due to invalid IL or missing references)
		//IL_00f8: Unknown result type (might be due to invalid IL or missing references)
		if (base.isAuthority)
		{
			Ray aimRay = GetAimRay();
			if ((Object)(object)projectilePrefab != (Object)null)
			{
				float num = Util.Remap(charge, 0f, 1f, minDamageCoefficient, maxDamageCoefficient);
				float num2 = charge * force;
				FireProjectileInfo fireProjectileInfo = default(FireProjectileInfo);
				fireProjectileInfo.projectilePrefab = projectilePrefab;
				fireProjectileInfo.position = ((Ray)(ref aimRay)).origin;
				fireProjectileInfo.rotation = Util.QuaternionSafeLookRotation(((Ray)(ref aimRay)).direction);
				fireProjectileInfo.owner = base.gameObject;
				fireProjectileInfo.damage = damageStat * num;
				fireProjectileInfo.force = num2;
				fireProjectileInfo.crit = RollCrit();
				FireProjectileInfo projectileInfo = fireProjectileInfo;
				ModifyProjectile(ref projectileInfo);
				ProjectileManager.instance.FireProjectile(projectileInfo);
			}
			if (Object.op_Implicit((Object)(object)base.characterMotor))
			{
				base.characterMotor.ApplyForce(((Ray)(ref aimRay)).direction * ((0f - selfForce) * charge));
			}
		}
	}

	public override InterruptPriority GetMinimumInterruptPriority()
	{
		return InterruptPriority.PrioritySkill;
	}

	protected virtual void PlayThrowAnimation()
	{
		PlayAnimation("Gesture, Additive", "FireNovaBomb", "FireNovaBomb.playbackRate", duration);
	}

	protected virtual void ModifyProjectile(ref FireProjectileInfo projectileInfo)
	{
	}
}
