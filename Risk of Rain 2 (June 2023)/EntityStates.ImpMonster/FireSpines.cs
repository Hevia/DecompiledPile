using RoR2;
using RoR2.Projectile;
using UnityEngine;

namespace EntityStates.ImpMonster;

public class FireSpines : BaseState
{
	public static GameObject projectilePrefab;

	public static GameObject effectPrefab;

	public static float baseDuration = 2f;

	public static float durationBetweenThrows = 0.1f;

	public static int spineCountMax = 3;

	public static float damageCoefficient = 1.2f;

	public static float force = 20f;

	private int spineCount;

	private float spineTimer;

	private float duration;

	public override void OnEnter()
	{
		base.OnEnter();
		duration = baseDuration / attackSpeedStat;
		PlayAnimation("Gesture", "FireSpines", "FireSpines.playbackRate", duration);
	}

	public override void OnExit()
	{
		base.OnExit();
	}

	public override void FixedUpdate()
	{
		//IL_004e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0053: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a5: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ac: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b1: Unknown result type (might be due to invalid IL or missing references)
		base.FixedUpdate();
		spineTimer += Time.fixedDeltaTime;
		if (spineTimer >= durationBetweenThrows / attackSpeedStat && spineCount < spineCountMax)
		{
			spineCount++;
			Ray aimRay = GetAimRay();
			string muzzleName = "MuzzleMouth";
			spineTimer -= durationBetweenThrows / attackSpeedStat;
			if (Object.op_Implicit((Object)(object)effectPrefab))
			{
				EffectManager.SimpleMuzzleFlash(effectPrefab, base.gameObject, muzzleName, transmit: false);
			}
			if (base.isAuthority)
			{
				ProjectileManager.instance.FireProjectile(projectilePrefab, aimRay.origin, Util.QuaternionSafeLookRotation(aimRay.direction), base.gameObject, damageStat * damageCoefficient, force, Util.CheckRoll(critStat, base.characterBody.master));
			}
		}
		if (base.fixedAge >= duration && base.isAuthority)
		{
			outer.SetNextStateToMain();
		}
	}

	public override InterruptPriority GetMinimumInterruptPriority()
	{
		return InterruptPriority.Skill;
	}
}
