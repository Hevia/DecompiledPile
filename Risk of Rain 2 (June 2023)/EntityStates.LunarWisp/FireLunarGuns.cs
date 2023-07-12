using RoR2;
using UnityEngine;

namespace EntityStates.LunarWisp;

public class FireLunarGuns : BaseState
{
	public static GameObject muzzleVfxPrefab;

	public static float baseDuration;

	private float duration;

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

	public static string windLoopSound;

	public static string windDownSound;

	public static string shootLoopSound;

	private uint windLoopSoundID;

	private uint shootLoopSoundID;

	private Transform muzzleTransform;

	public Transform muzzleTransformOne;

	public Transform muzzleTransformTwo;

	public string muzzleNameOne;

	public string muzzleNameTwo;

	private GameObject muzzleVFXInstanceOne;

	private GameObject muzzleVFXInstanceTwo;

	private float fireTimer;

	private float baseFireRate;

	private float baseBulletsPerSecond;

	private Run.FixedTimeStamp critEndTime;

	private Run.FixedTimeStamp lastCritCheck;

	protected ref InputBankTest.ButtonState skillButtonState => ref base.inputBank.skill1;

	public override void OnEnter()
	{
		//IL_0074: Unknown result type (might be due to invalid IL or missing references)
		//IL_007f: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b0: Unknown result type (might be due to invalid IL or missing references)
		//IL_00bb: Unknown result type (might be due to invalid IL or missing references)
		base.OnEnter();
		duration = baseDuration / attackSpeedStat;
		muzzleTransformOne = FindModelChild(muzzleNameOne);
		muzzleTransformTwo = FindModelChild(muzzleNameTwo);
		if (Object.op_Implicit((Object)(object)muzzleTransformOne) && Object.op_Implicit((Object)(object)muzzleTransformTwo) && Object.op_Implicit((Object)(object)muzzleVfxPrefab))
		{
			muzzleVFXInstanceOne = Object.Instantiate<GameObject>(muzzleVfxPrefab, muzzleTransformOne.position, muzzleTransformOne.rotation);
			muzzleVFXInstanceOne.transform.parent = muzzleTransformOne;
			muzzleVFXInstanceTwo = Object.Instantiate<GameObject>(muzzleVfxPrefab, muzzleTransformTwo.position, muzzleTransformTwo.rotation);
			muzzleVFXInstanceTwo.transform.parent = muzzleTransformTwo;
		}
		baseFireRate = 1f / baseFireInterval;
		baseBulletsPerSecond = (float)baseBulletCount * baseFireRate;
		critEndTime = Run.FixedTimeStamp.negativeInfinity;
		lastCritCheck = Run.FixedTimeStamp.negativeInfinity;
		windLoopSoundID = Util.PlaySound(windLoopSound, base.gameObject);
		shootLoopSoundID = Util.PlaySound(shootLoopSound, base.gameObject);
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
		Util.PlaySound(windDownSound, base.gameObject);
		if (Object.op_Implicit((Object)(object)muzzleVFXInstanceOne))
		{
			EntityState.Destroy((Object)(object)muzzleVFXInstanceOne.gameObject);
			muzzleVFXInstanceOne = null;
		}
		if (Object.op_Implicit((Object)(object)muzzleVFXInstanceTwo))
		{
			EntityState.Destroy((Object)(object)muzzleVFXInstanceTwo.gameObject);
			muzzleVFXInstanceTwo = null;
		}
		AkSoundEngine.StopPlayingID(windLoopSoundID);
		AkSoundEngine.StopPlayingID(shootLoopSoundID);
		PlayCrossfade("Gesture", "MinigunSpinDown", 0.2f);
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
		//IL_0050: Unknown result type (might be due to invalid IL or missing references)
		//IL_0055: Unknown result type (might be due to invalid IL or missing references)
		//IL_006c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0079: Unknown result type (might be due to invalid IL or missing references)
		//IL_007e: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b2: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b7: Unknown result type (might be due to invalid IL or missing references)
		//IL_011e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0123: Unknown result type (might be due to invalid IL or missing references)
		//IL_0152: Unknown result type (might be due to invalid IL or missing references)
		//IL_0187: Unknown result type (might be due to invalid IL or missing references)
		//IL_0194: Unknown result type (might be due to invalid IL or missing references)
		//IL_0199: Unknown result type (might be due to invalid IL or missing references)
		//IL_01cd: Unknown result type (might be due to invalid IL or missing references)
		//IL_01d2: Unknown result type (might be due to invalid IL or missing references)
		//IL_0239: Unknown result type (might be due to invalid IL or missing references)
		//IL_023e: Unknown result type (might be due to invalid IL or missing references)
		//IL_026d: Unknown result type (might be due to invalid IL or missing references)
		UpdateCrits();
		bool isCrit = !critEndTime.hasPassed;
		float damage = baseDamagePerSecondCoefficient / baseBulletsPerSecond * damageStat;
		float force = baseForcePerSecond / baseBulletsPerSecond;
		float procCoefficient = baseProcCoefficientPerSecond / baseBulletsPerSecond;
		StartAimMode(0.5f);
		Ray aimRay = GetAimRay();
		BulletAttack bulletAttack = new BulletAttack();
		bulletAttack.bulletCount = (uint)baseBulletCount / 2u;
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
		bulletAttack.muzzleName = muzzleNameOne;
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
		BulletAttack bulletAttack2 = new BulletAttack();
		bulletAttack2.bulletCount = (uint)baseBulletCount / 2u;
		bulletAttack2.aimVector = ((Ray)(ref aimRay)).direction;
		bulletAttack2.origin = ((Ray)(ref aimRay)).origin;
		bulletAttack2.damage = damage;
		bulletAttack2.damageColorIndex = DamageColorIndex.Default;
		bulletAttack2.damageType = DamageType.Generic;
		bulletAttack2.falloffModel = BulletAttack.FalloffModel.None;
		bulletAttack2.maxDistance = bulletMaxDistance;
		bulletAttack2.force = force;
		bulletAttack2.hitMask = LayerIndex.CommonMasks.bullet;
		bulletAttack2.minSpread = bulletMinSpread;
		bulletAttack2.maxSpread = bulletMaxSpread;
		bulletAttack2.isCrit = isCrit;
		bulletAttack2.owner = base.gameObject;
		bulletAttack2.muzzleName = muzzleNameTwo;
		bulletAttack2.smartCollision = false;
		bulletAttack2.procChainMask = default(ProcChainMask);
		bulletAttack2.procCoefficient = procCoefficient;
		bulletAttack2.radius = 0f;
		bulletAttack2.sniper = false;
		bulletAttack2.stopperMask = LayerIndex.CommonMasks.bullet;
		bulletAttack2.weapon = null;
		bulletAttack2.tracerEffectPrefab = bulletTracerEffectPrefab;
		bulletAttack2.spreadPitchScale = 1f;
		bulletAttack2.spreadYawScale = 1f;
		bulletAttack2.queryTriggerInteraction = (QueryTriggerInteraction)0;
		bulletAttack2.hitEffectPrefab = bulletHitEffectPrefab;
		bulletAttack2.HitEffectNormal = bulletHitEffectNormal;
		bulletAttack2.Fire();
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
		if (base.fixedAge >= duration && base.isAuthority)
		{
			outer.SetNextStateToMain();
		}
	}

	public override InterruptPriority GetMinimumInterruptPriority()
	{
		return InterruptPriority.PrioritySkill;
	}
}
