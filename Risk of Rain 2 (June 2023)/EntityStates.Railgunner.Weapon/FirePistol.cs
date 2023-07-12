using RoR2;
using RoR2.Projectile;
using UnityEngine;

namespace EntityStates.Railgunner.Weapon;

public class FirePistol : BaseState, IBaseWeaponState
{
	[SerializeField]
	public float baseDuration;

	[SerializeField]
	[Header("Projectile")]
	public float damageCoefficient;

	[SerializeField]
	public float force;

	[SerializeField]
	public float spreadBloomValue;

	[SerializeField]
	public float recoilYMin;

	[SerializeField]
	public float recoilXMin;

	[SerializeField]
	public float recoilYMax;

	[SerializeField]
	public float recoilXMax;

	[SerializeField]
	public GameObject projectilePrefab;

	[SerializeField]
	public float selfKnockbackForce;

	[SerializeField]
	public float baseInaccuracyDegrees;

	[SerializeField]
	[Header("Effects")]
	public string muzzleName;

	[SerializeField]
	public string fireSoundString;

	[SerializeField]
	public GameObject muzzleFlashPrefab;

	[Header("Animation")]
	[SerializeField]
	public string animationLayerName;

	[SerializeField]
	public string animationStateName;

	[SerializeField]
	public string animationPlaybackRateParam;

	protected float duration;

	protected virtual void FireBullet(Ray aimRay)
	{
		//IL_0001: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a9: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ae: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b3: Unknown result type (might be due to invalid IL or missing references)
		//IL_00c6: Unknown result type (might be due to invalid IL or missing references)
		//IL_00cb: Unknown result type (might be due to invalid IL or missing references)
		//IL_00d0: Unknown result type (might be due to invalid IL or missing references)
		//IL_00d3: Unknown result type (might be due to invalid IL or missing references)
		//IL_00d8: Unknown result type (might be due to invalid IL or missing references)
		//IL_00dd: Unknown result type (might be due to invalid IL or missing references)
		//IL_00e2: Unknown result type (might be due to invalid IL or missing references)
		//IL_00e3: Unknown result type (might be due to invalid IL or missing references)
		//IL_00e8: Unknown result type (might be due to invalid IL or missing references)
		//IL_00e9: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ee: Unknown result type (might be due to invalid IL or missing references)
		//IL_0108: Unknown result type (might be due to invalid IL or missing references)
		//IL_010d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0114: Unknown result type (might be due to invalid IL or missing references)
		//IL_0115: Unknown result type (might be due to invalid IL or missing references)
		//IL_018e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0193: Unknown result type (might be due to invalid IL or missing references)
		StartAimMode(aimRay);
		Util.PlaySound(fireSoundString, base.gameObject);
		EffectManager.SimpleMuzzleFlash(muzzleFlashPrefab, base.gameObject, muzzleName, transmit: false);
		PlayAnimation(animationLayerName, animationStateName, animationPlaybackRateParam, duration);
		AddRecoil(recoilYMin, recoilYMax, recoilXMin, recoilXMax);
		if (base.isAuthority)
		{
			float num = 0f;
			if (Object.op_Implicit((Object)(object)base.characterBody))
			{
				num = base.characterBody.spreadBloomAngle;
			}
			Quaternion val = Quaternion.AngleAxis((float)Random.Range(0, 360), Vector3.forward);
			Quaternion val2 = Quaternion.AngleAxis(Random.Range(0f, baseInaccuracyDegrees + num), Vector3.left);
			Quaternion rotation = Util.QuaternionSafeLookRotation(((Ray)(ref aimRay)).direction, Vector3.up) * val * val2;
			FireProjectileInfo fireProjectileInfo = default(FireProjectileInfo);
			fireProjectileInfo.projectilePrefab = projectilePrefab;
			fireProjectileInfo.position = ((Ray)(ref aimRay)).origin;
			fireProjectileInfo.rotation = rotation;
			fireProjectileInfo.owner = base.gameObject;
			fireProjectileInfo.damage = damageStat * damageCoefficient;
			fireProjectileInfo.crit = RollCrit();
			fireProjectileInfo.force = force;
			fireProjectileInfo.procChainMask = default(ProcChainMask);
			fireProjectileInfo.damageColorIndex = DamageColorIndex.Default;
			FireProjectileInfo fireProjectileInfo2 = fireProjectileInfo;
			ProjectileManager.instance.FireProjectile(fireProjectileInfo2);
			base.characterBody.characterMotor.ApplyForce((0f - selfKnockbackForce) * ((Ray)(ref aimRay)).direction);
		}
		base.characterBody.AddSpreadBloom(spreadBloomValue);
	}

	public override void OnEnter()
	{
		//IL_001b: Unknown result type (might be due to invalid IL or missing references)
		base.OnEnter();
		duration = baseDuration / attackSpeedStat;
		FireBullet(GetAimRay());
	}

	public override void FixedUpdate()
	{
		base.FixedUpdate();
		if (base.fixedAge >= duration)
		{
			outer.SetNextStateToMain();
		}
	}

	public override InterruptPriority GetMinimumInterruptPriority()
	{
		return InterruptPriority.Skill;
	}

	public bool CanScope()
	{
		return true;
	}
}
