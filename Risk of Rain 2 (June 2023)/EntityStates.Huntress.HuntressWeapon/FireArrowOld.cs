using RoR2;
using RoR2.Projectile;
using UnityEngine;

namespace EntityStates.Huntress.HuntressWeapon;

public class FireArrowOld : BaseState
{
	public static GameObject projectilePrefab;

	public static GameObject effectPrefab;

	public static float baseDuration = 2f;

	public static float damageCoefficient = 1.2f;

	public static float force = 20f;

	private float duration;

	public override void OnEnter()
	{
		//IL_0019: Unknown result type (might be due to invalid IL or missing references)
		//IL_001e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0020: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b5: Unknown result type (might be due to invalid IL or missing references)
		//IL_00bc: Unknown result type (might be due to invalid IL or missing references)
		//IL_00c1: Unknown result type (might be due to invalid IL or missing references)
		base.OnEnter();
		duration = baseDuration / attackSpeedStat;
		Ray aimRay = GetAimRay();
		StartAimMode(aimRay);
		string muzzleName = "Muzzle";
		if (Object.op_Implicit((Object)(object)effectPrefab))
		{
			EffectManager.SimpleMuzzleFlash(effectPrefab, base.gameObject, muzzleName, transmit: false);
		}
		Animator modelAnimator = GetModelAnimator();
		if (Object.op_Implicit((Object)(object)modelAnimator))
		{
			int layerIndex = modelAnimator.GetLayerIndex("Gesture");
			modelAnimator.SetFloat("FireArrow.playbackRate", attackSpeedStat);
			modelAnimator.PlayInFixedTime("FireArrow", layerIndex, 0f);
			muzzleName = "MuzzleRight";
			modelAnimator.Update(0f);
			if (base.isAuthority)
			{
				ProjectileManager.instance.FireProjectile(projectilePrefab, aimRay.origin, Util.QuaternionSafeLookRotation(aimRay.direction), base.gameObject, damageStat * damageCoefficient, force, Util.CheckRoll(critStat, base.characterBody.master));
			}
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
