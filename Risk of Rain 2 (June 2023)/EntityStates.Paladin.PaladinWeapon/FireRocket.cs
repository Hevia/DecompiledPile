using RoR2;
using RoR2.Projectile;
using UnityEngine;

namespace EntityStates.Paladin.PaladinWeapon;

public class FireRocket : BaseState
{
	public static GameObject projectilePrefab;

	public static GameObject effectPrefab;

	public static string soundEffectString;

	public static float damageCoefficient;

	public static float force;

	public static float baseDuration = 2f;

	private float duration;

	public int bulletCountCurrent = 1;

	public override void OnEnter()
	{
		//IL_003a: Unknown result type (might be due to invalid IL or missing references)
		//IL_003f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0041: Unknown result type (might be due to invalid IL or missing references)
		//IL_0085: Unknown result type (might be due to invalid IL or missing references)
		//IL_008c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0091: Unknown result type (might be due to invalid IL or missing references)
		base.OnEnter();
		Util.PlaySound(soundEffectString, base.gameObject);
		duration = baseDuration / attackSpeedStat;
		base.characterBody.AddSpreadBloom(0.3f);
		Ray aimRay = GetAimRay();
		StartAimMode(aimRay);
		string muzzleName = "MuzzleCenter";
		if (Object.op_Implicit((Object)(object)effectPrefab))
		{
			EffectManager.SimpleMuzzleFlash(effectPrefab, base.gameObject, muzzleName, transmit: false);
		}
		if (base.isAuthority)
		{
			ProjectileManager.instance.FireProjectile(projectilePrefab, aimRay.origin, Util.QuaternionSafeLookRotation(aimRay.direction), base.gameObject, damageStat * damageCoefficient, 0f, Util.CheckRoll(critStat, base.characterBody.master));
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
			outer.SetNextStateToMain();
		}
	}

	public override InterruptPriority GetMinimumInterruptPriority()
	{
		return InterruptPriority.Skill;
	}
}
