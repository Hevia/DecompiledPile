using RoR2;
using RoR2.Projectile;
using UnityEngine;

namespace EntityStates.Vulture.Weapon;

public class FireWindblade : BaseSkillState
{
	public static float baseDuration;

	public static string muzzleString;

	public static GameObject muzzleEffectPrefab;

	public static GameObject projectilePrefab;

	public static float damageCoefficient = 1.2f;

	public static float force = 20f;

	public static string soundString;

	private float duration;

	public override void OnEnter()
	{
		//IL_0067: Unknown result type (might be due to invalid IL or missing references)
		//IL_006c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0077: Unknown result type (might be due to invalid IL or missing references)
		//IL_007c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0081: Unknown result type (might be due to invalid IL or missing references)
		//IL_0093: Unknown result type (might be due to invalid IL or missing references)
		//IL_0098: Unknown result type (might be due to invalid IL or missing references)
		//IL_009d: Unknown result type (might be due to invalid IL or missing references)
		//IL_00aa: Unknown result type (might be due to invalid IL or missing references)
		//IL_00af: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b0: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b1: Unknown result type (might be due to invalid IL or missing references)
		base.OnEnter();
		duration = baseDuration / attackSpeedStat;
		PlayAnimation("Gesture, Additive", "FireWindblade", "FireWindblade.playbackRate", duration);
		Util.PlaySound(soundString, base.gameObject);
		if (Object.op_Implicit((Object)(object)muzzleEffectPrefab))
		{
			EffectManager.SimpleMuzzleFlash(muzzleEffectPrefab, base.gameObject, muzzleString, transmit: false);
		}
		Ray aimRay = GetAimRay();
		if (base.isAuthority)
		{
			Quaternion val = Util.QuaternionSafeLookRotation(aimRay.direction);
			Quaternion val2 = Quaternion.AngleAxis(Random.Range(0f, 360f), aimRay.direction);
			ProjectileManager.instance.FireProjectile(projectilePrefab, aimRay.origin, val2 * val, base.gameObject, damageStat * damageCoefficient, force, Util.CheckRoll(critStat, base.characterBody.master));
		}
	}

	public override void FixedUpdate()
	{
		base.FixedUpdate();
		if (base.fixedAge >= duration)
		{
			outer.SetNextStateToMain();
		}
	}
}
