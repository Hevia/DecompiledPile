using RoR2;
using UnityEngine;

namespace EntityStates.Bandit2.Weapon;

public abstract class BaseFireSidearmRevolverState : BaseSidearmState
{
	[SerializeField]
	public GameObject effectPrefab;

	[SerializeField]
	public GameObject hitEffectPrefab;

	[SerializeField]
	public GameObject tracerEffectPrefab;

	[SerializeField]
	public float damageCoefficient;

	[SerializeField]
	public float force;

	[SerializeField]
	public float minSpread;

	[SerializeField]
	public float maxSpread;

	[SerializeField]
	public string attackSoundString;

	[SerializeField]
	public float recoilAmplitude;

	[SerializeField]
	public float bulletRadius;

	public override void OnEnter()
	{
		//IL_003d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0042: Unknown result type (might be due to invalid IL or missing references)
		//IL_0044: Unknown result type (might be due to invalid IL or missing references)
		//IL_00cf: Unknown result type (might be due to invalid IL or missing references)
		//IL_00d4: Unknown result type (might be due to invalid IL or missing references)
		//IL_00dc: Unknown result type (might be due to invalid IL or missing references)
		base.OnEnter();
		AddRecoil(-3f * recoilAmplitude, -4f * recoilAmplitude, -0.5f * recoilAmplitude, 0.5f * recoilAmplitude);
		Ray aimRay = GetAimRay();
		StartAimMode(aimRay);
		string muzzleName = "MuzzlePistol";
		Util.PlaySound(attackSoundString, base.gameObject);
		PlayAnimation("Gesture, Additive", "FireSideWeapon", "FireSideWeapon.playbackRate", duration);
		if (Object.op_Implicit((Object)(object)effectPrefab))
		{
			EffectManager.SimpleMuzzleFlash(effectPrefab, base.gameObject, muzzleName, transmit: false);
		}
		if (base.isAuthority)
		{
			BulletAttack bulletAttack = new BulletAttack();
			bulletAttack.owner = base.gameObject;
			bulletAttack.weapon = base.gameObject;
			bulletAttack.origin = aimRay.origin;
			bulletAttack.aimVector = aimRay.direction;
			bulletAttack.minSpread = minSpread;
			bulletAttack.maxSpread = maxSpread;
			bulletAttack.bulletCount = 1u;
			bulletAttack.damage = damageCoefficient * damageStat;
			bulletAttack.force = force;
			bulletAttack.falloffModel = BulletAttack.FalloffModel.None;
			bulletAttack.tracerEffectPrefab = tracerEffectPrefab;
			bulletAttack.muzzleName = muzzleName;
			bulletAttack.hitEffectPrefab = hitEffectPrefab;
			bulletAttack.isCrit = RollCrit();
			bulletAttack.HitEffectNormal = false;
			bulletAttack.radius = bulletRadius;
			bulletAttack.damageType |= DamageType.BonusToLowHealth;
			bulletAttack.smartCollision = true;
			ModifyBullet(bulletAttack);
			bulletAttack.Fire();
		}
	}

	public override void OnExit()
	{
		base.OnExit();
	}

	public override void FixedUpdate()
	{
		base.FixedUpdate();
		if (base.fixedAge >= duration && base.isAuthority)
		{
			outer.SetNextState(new ExitSidearmRevolver());
		}
	}

	public override InterruptPriority GetMinimumInterruptPriority()
	{
		return InterruptPriority.Any;
	}

	protected virtual void ModifyBullet(BulletAttack bulletAttack)
	{
	}
}
