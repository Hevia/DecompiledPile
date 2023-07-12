using RoR2;
using UnityEngine;

namespace EntityStates.ClayBruiser.Weapon;

public class MinigunFire : MinigunState
{
	public static GameObject muzzleVfxPrefab;

	public static float baseFireInterval;

	public static int baseBulletCount;

	public static float baseDamagePerSecondCoefficient;

	public static float baseForcePerSecond;

	public static float baseProcCoefficientPerSecond;

	public static float bulletMinSpread;

	public static float bulletMaxSpread;

	public static GameObject bulletTracerEffectPrefab;

	public static GameObject bulletHitEffectPrefab;

	public static bool bulletHitEffectNormal;

	public static float bulletMaxDistance;

	public static string fireSound;

	public static string startSound;

	public static string endSound;

	private float fireTimer;

	private Transform muzzleVfxTransform;

	private float baseFireRate;

	private float baseBulletsPerSecond;

	private Run.FixedTimeStamp critEndTime;

	private Run.FixedTimeStamp lastCritCheck;

	public override void OnEnter()
	{
		base.OnEnter();
		if (Object.op_Implicit((Object)(object)muzzleTransform) && Object.op_Implicit((Object)(object)muzzleVfxPrefab))
		{
			muzzleVfxTransform = Object.Instantiate<GameObject>(muzzleVfxPrefab, muzzleTransform).transform;
		}
		baseFireRate = 1f / baseFireInterval;
		baseBulletsPerSecond = (float)baseBulletCount * baseFireRate;
		critEndTime = Run.FixedTimeStamp.negativeInfinity;
		lastCritCheck = Run.FixedTimeStamp.negativeInfinity;
		Util.PlaySound(startSound, base.gameObject);
		PlayCrossfade("Gesture, Additive", "FireMinigun", 0.2f);
	}

	private void UpdateCrits()
	{
		if (lastCritCheck.timeSince >= 1f)
		{
			lastCritCheck = Run.FixedTimeStamp.now;
			if (RollCrit())
			{
				critEndTime = Run.FixedTimeStamp.now + 2f;
			}
		}
	}

	public override void OnExit()
	{
		Util.PlaySound(endSound, base.gameObject);
		if (Object.op_Implicit((Object)(object)muzzleVfxTransform))
		{
			EntityState.Destroy((Object)(object)((Component)muzzleVfxTransform).gameObject);
			muzzleVfxTransform = null;
		}
		PlayCrossfade("Gesture, Additive", "BufferEmpty", 0.2f);
		base.OnExit();
	}

	private void OnFireShared()
	{
		Util.PlaySound(fireSound, base.gameObject);
		if (base.isAuthority)
		{
			OnFireAuthority();
		}
	}

	private void OnFireAuthority()
	{
		//IL_0044: Unknown result type (might be due to invalid IL or missing references)
		//IL_0049: Unknown result type (might be due to invalid IL or missing references)
		//IL_005e: Unknown result type (might be due to invalid IL or missing references)
		//IL_006b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0070: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a4: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a9: Unknown result type (might be due to invalid IL or missing references)
		//IL_010f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0114: Unknown result type (might be due to invalid IL or missing references)
		//IL_0143: Unknown result type (might be due to invalid IL or missing references)
		UpdateCrits();
		bool isCrit = !critEndTime.hasPassed;
		float damage = baseDamagePerSecondCoefficient / baseBulletsPerSecond * damageStat;
		float force = baseForcePerSecond / baseBulletsPerSecond;
		float procCoefficient = baseProcCoefficientPerSecond / baseBulletsPerSecond;
		Ray aimRay = GetAimRay();
		BulletAttack bulletAttack = new BulletAttack();
		bulletAttack.bulletCount = (uint)baseBulletCount;
		bulletAttack.aimVector = ((Ray)(ref aimRay)).direction;
		bulletAttack.origin = ((Ray)(ref aimRay)).origin;
		bulletAttack.damage = damage;
		bulletAttack.damageColorIndex = DamageColorIndex.Default;
		bulletAttack.damageType = DamageType.Generic;
		bulletAttack.falloffModel = BulletAttack.FalloffModel.None;
		bulletAttack.maxDistance = bulletMaxDistance;
		bulletAttack.force = force;
		bulletAttack.hitMask = LayerIndex.CommonMasks.bullet;
		bulletAttack.minSpread = bulletMinSpread;
		bulletAttack.maxSpread = bulletMaxSpread;
		bulletAttack.isCrit = isCrit;
		bulletAttack.owner = base.gameObject;
		bulletAttack.muzzleName = MinigunState.muzzleName;
		bulletAttack.smartCollision = false;
		bulletAttack.procChainMask = default(ProcChainMask);
		bulletAttack.procCoefficient = procCoefficient;
		bulletAttack.radius = 0f;
		bulletAttack.sniper = false;
		bulletAttack.stopperMask = LayerIndex.CommonMasks.bullet;
		bulletAttack.weapon = null;
		bulletAttack.tracerEffectPrefab = bulletTracerEffectPrefab;
		bulletAttack.spreadPitchScale = 1f;
		bulletAttack.spreadYawScale = 1f;
		bulletAttack.queryTriggerInteraction = (QueryTriggerInteraction)0;
		bulletAttack.hitEffectPrefab = bulletHitEffectPrefab;
		bulletAttack.HitEffectNormal = bulletHitEffectNormal;
		bulletAttack.Fire();
	}

	public override void FixedUpdate()
	{
		base.FixedUpdate();
		fireTimer -= Time.fixedDeltaTime;
		if (fireTimer <= 0f)
		{
			float num = baseFireInterval / attackSpeedStat;
			fireTimer += num;
			OnFireShared();
		}
		if (base.isAuthority && !base.skillButtonState.down)
		{
			outer.SetNextState(new MinigunSpinDown());
		}
	}
}
