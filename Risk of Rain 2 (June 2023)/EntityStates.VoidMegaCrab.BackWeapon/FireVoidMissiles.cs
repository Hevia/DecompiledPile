using RoR2;
using RoR2.Projectile;
using UnityEngine;

namespace EntityStates.VoidMegaCrab.BackWeapon;

public class FireVoidMissiles : BaseSkillState
{
	public static float baseDuration;

	public static string leftMuzzleString;

	public static string rightMuzzleString;

	public static GameObject muzzleEffectPrefab;

	public static GameObject projectilePrefab;

	public static int totalMissileWaveCount;

	public static float baseDurationBetweenMissiles;

	public static float damageCoefficient = 1.2f;

	public static float force = 20f;

	public static string enterSoundString;

	public static string animationLayerName = "Gesture, Additive";

	public static string animationStateName = "FireCrabCannon";

	public static string animationPlaybackRateParam = "FireCrabCannon.playbackRate";

	private float duration;

	private float durationBetweenMissiles;

	private float missileTimer;

	private int missileWaveCount;

	public override void OnEnter()
	{
		base.OnEnter();
		duration = baseDuration / attackSpeedStat;
		durationBetweenMissiles = baseDurationBetweenMissiles / attackSpeedStat;
		PlayAnimation(animationLayerName, animationStateName, animationPlaybackRateParam, duration);
		Util.PlaySound(enterSoundString, base.gameObject);
	}

	private void FireMissile()
	{
		//IL_005c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0061: Unknown result type (might be due to invalid IL or missing references)
		//IL_006e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0074: Unknown result type (might be due to invalid IL or missing references)
		//IL_008a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0091: Unknown result type (might be due to invalid IL or missing references)
		//IL_0096: Unknown result type (might be due to invalid IL or missing references)
		//IL_00e0: Unknown result type (might be due to invalid IL or missing references)
		//IL_00e6: Unknown result type (might be due to invalid IL or missing references)
		//IL_00fc: Unknown result type (might be due to invalid IL or missing references)
		//IL_0103: Unknown result type (might be due to invalid IL or missing references)
		//IL_0108: Unknown result type (might be due to invalid IL or missing references)
		if (Object.op_Implicit((Object)(object)muzzleEffectPrefab))
		{
			EffectManager.SimpleMuzzleFlash(muzzleEffectPrefab, base.gameObject, leftMuzzleString, transmit: false);
			EffectManager.SimpleMuzzleFlash(muzzleEffectPrefab, base.gameObject, rightMuzzleString, transmit: false);
		}
		if (base.isAuthority)
		{
			Transform val = FindModelChild(leftMuzzleString);
			Transform val2 = FindModelChild(rightMuzzleString);
			Ray aimRay = GetAimRay();
			if ((Object)(object)val != (Object)null)
			{
				aimRay._002Ector(val.position, val.forward);
			}
			ProjectileManager.instance.FireProjectile(projectilePrefab, aimRay.origin, Util.QuaternionSafeLookRotation(aimRay.direction), base.gameObject, damageStat * damageCoefficient, force, Util.CheckRoll(critStat, base.characterBody.master));
			if ((Object)(object)val2 != (Object)null)
			{
				aimRay._002Ector(val2.position, val2.forward);
			}
			ProjectileManager.instance.FireProjectile(projectilePrefab, aimRay.origin, Util.QuaternionSafeLookRotation(aimRay.direction), base.gameObject, damageStat * damageCoefficient, force, Util.CheckRoll(critStat, base.characterBody.master));
		}
	}

	public override void FixedUpdate()
	{
		base.FixedUpdate();
		missileTimer -= Time.fixedDeltaTime;
		if (missileWaveCount < totalMissileWaveCount && missileTimer <= 0f)
		{
			missileWaveCount++;
			missileTimer += durationBetweenMissiles;
			FireMissile();
		}
		if (base.fixedAge >= duration)
		{
			outer.SetNextStateToMain();
		}
	}
}
