using RoR2;
using UnityEngine;

namespace EntityStates;

public abstract class GenericBulletBaseState : BaseState
{
	[SerializeField]
	public float baseDuration = 0.1f;

	[SerializeField]
	public int bulletCount = 1;

	[SerializeField]
	public float maxDistance = 50f;

	[SerializeField]
	public float bulletRadius;

	[SerializeField]
	public bool useSmartCollision;

	[SerializeField]
	public float damageCoefficient = 0.1f;

	[SerializeField]
	public float procCoefficient = 1f;

	[SerializeField]
	public float force = 100f;

	[SerializeField]
	public float minSpread;

	[SerializeField]
	public float maxSpread;

	[SerializeField]
	public float spreadPitchScale = 0.5f;

	[SerializeField]
	public float spreadYawScale = 1f;

	[SerializeField]
	public float spreadBloomValue = 0.2f;

	[SerializeField]
	public float recoilAmplitudeY;

	[SerializeField]
	public float recoilAmplitudeX;

	[SerializeField]
	public string muzzleName;

	[SerializeField]
	public string fireSoundString;

	[SerializeField]
	public GameObject muzzleFlashPrefab;

	[SerializeField]
	public GameObject tracerEffectPrefab;

	[SerializeField]
	public GameObject hitEffectPrefab;

	protected float duration;

	protected Transform muzzleTransform;

	protected BulletAttack GenerateBulletAttack(Ray aimRay)
	{
		//IL_0027: Unknown result type (might be due to invalid IL or missing references)
		//IL_0034: Unknown result type (might be due to invalid IL or missing references)
		//IL_0039: Unknown result type (might be due to invalid IL or missing references)
		float num = 0f;
		if (Object.op_Implicit((Object)(object)base.characterBody))
		{
			num = base.characterBody.spreadBloomAngle;
		}
		return new BulletAttack
		{
			aimVector = ((Ray)(ref aimRay)).direction,
			origin = ((Ray)(ref aimRay)).origin,
			owner = base.gameObject,
			weapon = null,
			bulletCount = (uint)bulletCount,
			damage = damageStat * damageCoefficient,
			damageColorIndex = DamageColorIndex.Default,
			damageType = DamageType.Generic,
			falloffModel = BulletAttack.FalloffModel.Buckshot,
			force = force,
			HitEffectNormal = false,
			procChainMask = default(ProcChainMask),
			procCoefficient = procCoefficient,
			maxDistance = maxDistance,
			radius = bulletRadius,
			isCrit = RollCrit(),
			muzzleName = muzzleName,
			minSpread = minSpread,
			maxSpread = maxSpread + num,
			hitEffectPrefab = hitEffectPrefab,
			smartCollision = useSmartCollision,
			sniper = false,
			spreadPitchScale = spreadPitchScale,
			spreadYawScale = spreadYawScale,
			tracerEffectPrefab = tracerEffectPrefab
		};
	}

	protected virtual void PlayFireAnimation()
	{
	}

	protected virtual void DoFireEffects()
	{
		Util.PlaySound(fireSoundString, base.gameObject);
		if (Object.op_Implicit((Object)(object)muzzleTransform))
		{
			EffectManager.SimpleMuzzleFlash(muzzleFlashPrefab, base.gameObject, muzzleName, transmit: false);
		}
	}

	protected virtual void ModifyBullet(BulletAttack bulletAttack)
	{
	}

	protected virtual void FireBullet(Ray aimRay)
	{
		//IL_0001: Unknown result type (might be due to invalid IL or missing references)
		//IL_0058: Unknown result type (might be due to invalid IL or missing references)
		//IL_006d: Unknown result type (might be due to invalid IL or missing references)
		StartAimMode(aimRay, 3f);
		DoFireEffects();
		PlayFireAnimation();
		AddRecoil(-1f * recoilAmplitudeY, -1.5f * recoilAmplitudeY, -1f * recoilAmplitudeX, 1f * recoilAmplitudeX);
		if (base.isAuthority)
		{
			BulletAttack bulletAttack = GenerateBulletAttack(aimRay);
			ModifyBullet(bulletAttack);
			bulletAttack.Fire();
			OnFireBulletAuthority(aimRay);
		}
	}

	protected virtual void OnFireBulletAuthority(Ray aimRay)
	{
	}

	public override void OnEnter()
	{
		//IL_002d: Unknown result type (might be due to invalid IL or missing references)
		base.OnEnter();
		duration = baseDuration / attackSpeedStat;
		muzzleTransform = FindModelChild(muzzleName);
		FireBullet(GetAimRay());
		base.characterBody.AddSpreadBloom(spreadBloomValue);
	}

	public override void FixedUpdate()
	{
		base.FixedUpdate();
		if (base.fixedAge >= duration)
		{
			outer.SetNextState(InstantiateNextState());
		}
	}

	protected virtual EntityState InstantiateNextState()
	{
		return EntityStateCatalog.InstantiateState(outer.mainStateType);
	}

	public override InterruptPriority GetMinimumInterruptPriority()
	{
		return InterruptPriority.Skill;
	}
}
