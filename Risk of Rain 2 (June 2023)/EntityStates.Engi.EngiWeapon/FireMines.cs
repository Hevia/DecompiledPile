using RoR2;
using RoR2.Projectile;
using UnityEngine;

namespace EntityStates.Engi.EngiWeapon;

public class FireMines : BaseState
{
	public static GameObject effectPrefab;

	public static GameObject hitEffectPrefab;

	[SerializeField]
	public GameObject projectilePrefab;

	[SerializeField]
	public float damageCoefficient;

	[SerializeField]
	public float force;

	public static float baseDuration = 2f;

	public static string throwMineSoundString;

	private float duration;

	public override void OnEnter()
	{
		//IL_002a: Unknown result type (might be due to invalid IL or missing references)
		//IL_002f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0031: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b2: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b9: Unknown result type (might be due to invalid IL or missing references)
		//IL_00be: Unknown result type (might be due to invalid IL or missing references)
		base.OnEnter();
		Util.PlaySound(throwMineSoundString, base.gameObject);
		duration = baseDuration / attackSpeedStat;
		Ray aimRay = GetAimRay();
		StartAimMode(aimRay);
		if (Object.op_Implicit((Object)(object)GetModelAnimator()))
		{
			float num = duration * 0.3f;
			PlayCrossfade("Gesture, Additive", "FireMineRight", "FireMine.playbackRate", duration + num, 0.05f);
		}
		string muzzleName = "MuzzleCenter";
		if (Object.op_Implicit((Object)(object)effectPrefab))
		{
			EffectManager.SimpleMuzzleFlash(effectPrefab, base.gameObject, muzzleName, transmit: false);
		}
		if (base.isAuthority)
		{
			ProjectileManager.instance.FireProjectile(projectilePrefab, ((Ray)(ref aimRay)).origin, Util.QuaternionSafeLookRotation(((Ray)(ref aimRay)).direction), base.gameObject, damageStat * damageCoefficient, force, Util.CheckRoll(critStat, base.characterBody.master));
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
		return InterruptPriority.PrioritySkill;
	}
}
