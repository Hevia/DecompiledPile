using RoR2;
using RoR2.Projectile;
using UnityEngine;

namespace EntityStates.VoidSurvivor.Weapon;

public class FireMegaBlasterBase : BaseState
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
	public float yawPerProjectile;

	[SerializeField]
	public float selfKnockbackForce;

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
		//IL_00f1: Unknown result type (might be due to invalid IL or missing references)
		//IL_00f6: Unknown result type (might be due to invalid IL or missing references)
		base.OnEnter();
		Ray aimRay = GetAimRay();
		duration = baseDuration / attackSpeedStat;
		StartAimMode(duration + 2f);
		PlayAnimation(animationLayerName, animationStateName, animationPlaybackRateParam, duration);
		Util.PlaySound(attackSoundString, base.gameObject);
		AddRecoil(-1f * recoilAmplitude, -1.5f * recoilAmplitude, -0.25f * recoilAmplitude, 0.25f * recoilAmplitude);
		base.characterBody.AddSpreadBloom(bloom);
		if (Object.op_Implicit((Object)(object)muzzleflashEffectPrefab))
		{
			EffectManager.SimpleMuzzleFlash(muzzleflashEffectPrefab, base.gameObject, muzzle, transmit: false);
		}
		if (base.isAuthority)
		{
			FireProjectiles();
		}
		base.characterBody.characterMotor.ApplyForce((0f - selfKnockbackForce) * ((Ray)(ref aimRay)).direction);
	}

	private void FireProjectiles()
	{
		//IL_0001: Unknown result type (might be due to invalid IL or missing references)
		//IL_0006: Unknown result type (might be due to invalid IL or missing references)
		//IL_000b: Unknown result type (might be due to invalid IL or missing references)
		//IL_002f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0054: Unknown result type (might be due to invalid IL or missing references)
		//IL_0059: Unknown result type (might be due to invalid IL or missing references)
		//IL_0062: Unknown result type (might be due to invalid IL or missing references)
		//IL_0067: Unknown result type (might be due to invalid IL or missing references)
		//IL_006c: Unknown result type (might be due to invalid IL or missing references)
		Ray aimRay = GetAimRay();
		((Ray)(ref aimRay)).direction = Util.ApplySpread(((Ray)(ref aimRay)).direction, 0f, spread, 1f, 1f);
		FireProjectileInfo fireProjectileInfo = default(FireProjectileInfo);
		fireProjectileInfo.projectilePrefab = projectilePrefab;
		fireProjectileInfo.position = ((Ray)(ref aimRay)).origin;
		fireProjectileInfo.rotation = Util.QuaternionSafeLookRotation(((Ray)(ref aimRay)).direction);
		fireProjectileInfo.owner = base.gameObject;
		fireProjectileInfo.damage = damageStat * damageCoefficient;
		fireProjectileInfo.force = force;
		fireProjectileInfo.crit = Util.CheckRoll(critStat, base.characterBody.master);
		ProjectileManager.instance.FireProjectile(fireProjectileInfo);
	}

	public override void OnExit()
	{
		base.OnExit();
	}

	public override void FixedUpdate()
	{
		base.FixedUpdate();
		if (base.isAuthority && base.fixedAge >= duration)
		{
			outer.SetNextStateToMain();
		}
	}

	public override InterruptPriority GetMinimumInterruptPriority()
	{
		return InterruptPriority.Skill;
	}
}
