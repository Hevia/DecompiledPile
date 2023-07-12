using RoR2;
using RoR2.Projectile;
using UnityEngine;

namespace EntityStates.UrchinTurret.Weapon;

public class MinigunFire : MinigunState
{
	public static GameObject muzzleVfxPrefab;

	public static GameObject projectilePrefab;

	public static float baseFireInterval;

	public static float baseDamagePerSecondCoefficient;

	public static float bulletMinSpread;

	public static float bulletMaxSpread;

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
		baseBulletsPerSecond = baseFireRate;
		critEndTime = Run.FixedTimeStamp.negativeInfinity;
		lastCritCheck = Run.FixedTimeStamp.negativeInfinity;
		Util.PlaySound(startSound, base.gameObject);
		PlayCrossfade("Gesture, Additive", "ShootLoop", 0.2f);
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
		//IL_002a: Unknown result type (might be due to invalid IL or missing references)
		//IL_002f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0032: Unknown result type (might be due to invalid IL or missing references)
		//IL_0055: Unknown result type (might be due to invalid IL or missing references)
		//IL_005a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0067: Unknown result type (might be due to invalid IL or missing references)
		//IL_006c: Unknown result type (might be due to invalid IL or missing references)
		//IL_006d: Unknown result type (might be due to invalid IL or missing references)
		UpdateCrits();
		bool crit = !critEndTime.hasPassed;
		float damage = baseDamagePerSecondCoefficient / baseBulletsPerSecond * damageStat;
		Ray aimRay = GetAimRay();
		Vector3 forward = Util.ApplySpread(((Ray)(ref aimRay)).direction, bulletMinSpread, bulletMaxSpread, 1f, 1f);
		ProjectileManager.instance.FireProjectile(projectilePrefab, ((Ray)(ref aimRay)).origin, Util.QuaternionSafeLookRotation(forward), base.gameObject, damage, 0f, crit);
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
