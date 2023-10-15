using RoR2;
using RoR2.Projectile;
using UnityEngine;

namespace EntityStates.VoidSurvivor.Weapon;

public class FireCorruptDisks : BaseSkillState
{
	[SerializeField]
	public GameObject projectilePrefab;

	[SerializeField]
	public GameObject muzzleflashEffectPrefab;

	[SerializeField]
	public float baseDuration = 2f;

	[SerializeField]
	public float damageCoefficient = 1.2f;

	[SerializeField]
	public float force = 20f;

	[SerializeField]
	public string attackSoundString;

	[SerializeField]
	public float recoilAmplitude;

	[SerializeField]
	public float bloom;

	[SerializeField]
	public string muzzle;

	[SerializeField]
	public float spread;

	[SerializeField]
	public int projectileCount;

	[SerializeField]
	public float yawPerProjectile;

	[SerializeField]
	public float offsetPerProjectile;

	[SerializeField]
	public float selfKnockbackForce;

	[SerializeField]
	public float bonusPitch;

	[SerializeField]
	public string animationLayerName;

	[SerializeField]
	public string animationStateName;

	[SerializeField]
	public string animationPlaybackRateParam;

	private float duration;

	public override void OnEnter()
	{
		//IL_0007: Unknown result type (might be due to invalid IL or missing references)
		//IL_000c: Unknown result type (might be due to invalid IL or missing references)
		//IL_00be: Unknown result type (might be due to invalid IL or missing references)
		//IL_00c3: Unknown result type (might be due to invalid IL or missing references)
		base.OnEnter();
		Ray aimRay = GetAimRay();
		duration = baseDuration / attackSpeedStat;
		StartAimMode(duration + 2f);
		PlayAnimation(animationLayerName, animationStateName, animationPlaybackRateParam, duration);
		Util.PlaySound(attackSoundString, base.gameObject);
		AddRecoil(-1f * recoilAmplitude, -1.5f * recoilAmplitude, -0.25f * recoilAmplitude, 0.25f * recoilAmplitude);
		base.characterBody.AddSpreadBloom(bloom);
		base.characterBody.characterMotor.ApplyForce((0f - selfKnockbackForce) * aimRay.direction);
		if (Object.op_Implicit((Object)(object)muzzleflashEffectPrefab))
		{
			EffectManager.SimpleMuzzleFlash(muzzleflashEffectPrefab, base.gameObject, muzzle, transmit: false);
		}
		if (base.isAuthority)
		{
			FireProjectiles();
		}
	}

	private void FireProjectiles()
	{
		//IL_002c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0031: Unknown result type (might be due to invalid IL or missing references)
		//IL_0036: Unknown result type (might be due to invalid IL or missing references)
		//IL_0063: Unknown result type (might be due to invalid IL or missing references)
		//IL_006d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0072: Unknown result type (might be due to invalid IL or missing references)
		//IL_0074: Unknown result type (might be due to invalid IL or missing references)
		//IL_0078: Unknown result type (might be due to invalid IL or missing references)
		//IL_007d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0085: Unknown result type (might be due to invalid IL or missing references)
		//IL_008a: Unknown result type (might be due to invalid IL or missing references)
		//IL_008c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0091: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b0: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b7: Unknown result type (might be due to invalid IL or missing references)
		//IL_00bc: Unknown result type (might be due to invalid IL or missing references)
		//IL_00c1: Unknown result type (might be due to invalid IL or missing references)
		//IL_00c7: Unknown result type (might be due to invalid IL or missing references)
		//IL_00cc: Unknown result type (might be due to invalid IL or missing references)
		//IL_00d1: Unknown result type (might be due to invalid IL or missing references)
		//IL_00d8: Unknown result type (might be due to invalid IL or missing references)
		//IL_00da: Unknown result type (might be due to invalid IL or missing references)
		for (int i = 0; i < projectileCount; i++)
		{
			float num = (float)i - (float)(projectileCount - 1) / 2f;
			float bonusYaw = num * yawPerProjectile;
			float num2 = num * offsetPerProjectile;
			Ray aimRay = GetAimRay();
			aimRay.direction = Util.ApplySpread(aimRay.direction, 0f, base.characterBody.spreadBloomAngle + spread, 1f, 1f, bonusYaw, bonusPitch);
			Vector3 onUnitSphere = Random.onUnitSphere;
			Vector3.ProjectOnPlane(onUnitSphere, aimRay.direction);
			Quaternion rotation = Util.QuaternionSafeLookRotation(aimRay.direction, onUnitSphere);
			FireProjectileInfo fireProjectileInfo = default(FireProjectileInfo);
			fireProjectileInfo.projectilePrefab = projectilePrefab;
			fireProjectileInfo.position = aimRay.origin + Vector3.Cross(aimRay.direction, Vector3.up) * num2;
			fireProjectileInfo.rotation = rotation;
			fireProjectileInfo.owner = base.gameObject;
			fireProjectileInfo.damage = damageStat * damageCoefficient;
			fireProjectileInfo.force = force;
			fireProjectileInfo.crit = Util.CheckRoll(critStat, base.characterBody.master);
			ProjectileManager.instance.FireProjectile(fireProjectileInfo);
		}
	}

	public override void FixedUpdate()
	{
		base.FixedUpdate();
		if (base.isAuthority && base.fixedAge >= duration)
		{
			outer.SetNextState(new Idle());
		}
	}

	public override InterruptPriority GetMinimumInterruptPriority()
	{
		return InterruptPriority.PrioritySkill;
	}
}
