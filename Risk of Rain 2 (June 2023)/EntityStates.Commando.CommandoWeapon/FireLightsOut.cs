using RoR2;
using UnityEngine;

namespace EntityStates.Commando.CommandoWeapon;

public class FireLightsOut : BaseState
{
	public static GameObject effectPrefab;

	public static GameObject hitEffectPrefab;

	public static GameObject tracerEffectPrefab;

	public static float damageCoefficient;

	public static float force;

	public static float minSpread;

	public static float maxSpread;

	public static int bulletCount;

	public static float baseDuration = 2f;

	public static string attackSoundString;

	public static float recoilAmplitude;

	private ChildLocator childLocator;

	public int bulletCountCurrent = 1;

	private float duration;

	public override void OnEnter()
	{
		//IL_004b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0050: Unknown result type (might be due to invalid IL or missing references)
		//IL_0052: Unknown result type (might be due to invalid IL or missing references)
		//IL_00de: Unknown result type (might be due to invalid IL or missing references)
		//IL_00e3: Unknown result type (might be due to invalid IL or missing references)
		//IL_00eb: Unknown result type (might be due to invalid IL or missing references)
		base.OnEnter();
		duration = baseDuration / attackSpeedStat;
		AddRecoil(-3f * recoilAmplitude, -4f * recoilAmplitude, -0.5f * recoilAmplitude, 0.5f * recoilAmplitude);
		Ray aimRay = GetAimRay();
		StartAimMode(aimRay);
		string muzzleName = "MuzzlePistol";
		Util.PlaySound(attackSoundString, base.gameObject);
		PlayAnimation("Gesture, Additive", "FireRevolver");
		PlayAnimation("Gesture, Override", "FireRevolver");
		if (Object.op_Implicit((Object)(object)effectPrefab))
		{
			EffectManager.SimpleMuzzleFlash(effectPrefab, base.gameObject, muzzleName, transmit: false);
		}
		if (base.isAuthority)
		{
			BulletAttack obj = new BulletAttack
			{
				owner = base.gameObject,
				weapon = base.gameObject,
				origin = aimRay.origin,
				aimVector = aimRay.direction,
				minSpread = minSpread,
				maxSpread = maxSpread,
				bulletCount = ((bulletCount > 0) ? ((uint)bulletCount) : 0u),
				damage = damageCoefficient * damageStat,
				force = force,
				falloffModel = BulletAttack.FalloffModel.None,
				tracerEffectPrefab = tracerEffectPrefab,
				muzzleName = muzzleName,
				hitEffectPrefab = hitEffectPrefab,
				isCrit = Util.CheckRoll(critStat, base.characterBody.master),
				HitEffectNormal = false,
				radius = 0.5f
			};
			obj.damageType |= DamageType.ResetCooldownsOnKill;
			obj.smartCollision = true;
			obj.Fire();
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
		return InterruptPriority.Any;
	}
}
