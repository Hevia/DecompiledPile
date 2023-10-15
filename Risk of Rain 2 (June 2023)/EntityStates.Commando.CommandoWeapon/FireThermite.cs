using RoR2;
using RoR2.Projectile;
using UnityEngine;

namespace EntityStates.Commando.CommandoWeapon;

public class FireThermite : BaseState
{
	public static GameObject effectPrefab;

	public static GameObject projectilePrefab;

	public static float damageCoefficient;

	public static float force;

	public static float selfForce;

	public static float baseDuration = 2f;

	private float duration;

	public int bulletCountCurrent = 1;

	public override void OnEnter()
	{
		//IL_0019: Unknown result type (might be due to invalid IL or missing references)
		//IL_001e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0020: Unknown result type (might be due to invalid IL or missing references)
		//IL_007f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0086: Unknown result type (might be due to invalid IL or missing references)
		//IL_008b: Unknown result type (might be due to invalid IL or missing references)
		//IL_00e5: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ea: Unknown result type (might be due to invalid IL or missing references)
		//IL_00f4: Unknown result type (might be due to invalid IL or missing references)
		//IL_00f9: Unknown result type (might be due to invalid IL or missing references)
		//IL_0110: Unknown result type (might be due to invalid IL or missing references)
		base.OnEnter();
		duration = baseDuration / attackSpeedStat;
		Ray aimRay = GetAimRay();
		StartAimMode(aimRay);
		PlayAnimation("Gesture", "FireFMJ", "FireFMJ.playbackRate", duration);
		string muzzleName = "MuzzleCenter";
		if (Object.op_Implicit((Object)(object)effectPrefab))
		{
			EffectManager.SimpleMuzzleFlash(effectPrefab, base.gameObject, muzzleName, transmit: false);
		}
		if (base.isAuthority)
		{
			ProjectileManager.instance.FireProjectile(projectilePrefab, aimRay.origin, Util.QuaternionSafeLookRotation(aimRay.direction), base.gameObject, damageStat * damageCoefficient, 0f, Util.CheckRoll(critStat, base.characterBody.master));
		}
		if (Object.op_Implicit((Object)(object)base.characterMotor) && !base.characterMotor.isGrounded)
		{
			Vector3 val = -aimRay.direction * selfForce;
			val.y *= 0.5f;
			base.characterMotor.ApplyForce(val, alwaysApply: true);
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
