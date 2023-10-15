using RoR2;
using RoR2.Projectile;
using UnityEngine;

namespace EntityStates.VagrantMonster;

public class FireTrackingBomb : BaseState
{
	public static float baseDuration = 3f;

	public static GameObject projectilePrefab;

	public static GameObject muzzleEffectPrefab;

	public static string fireBombSoundString;

	public static float bombDamageCoefficient;

	public static float bombForce;

	public float novaRadius;

	private float duration;

	private float stopwatch;

	public override void OnEnter()
	{
		base.OnEnter();
		stopwatch = 0f;
		duration = baseDuration / attackSpeedStat;
		PlayAnimation("Gesture, Override", "FireTrackingBomb", "FireTrackingBomb.playbackRate", duration);
		FireBomb();
	}

	public override void OnExit()
	{
		base.OnExit();
	}

	public override void FixedUpdate()
	{
		base.FixedUpdate();
		stopwatch += Time.fixedDeltaTime;
		if (stopwatch >= duration && base.isAuthority)
		{
			outer.SetNextStateToMain();
		}
	}

	private void FireBomb()
	{
		//IL_0001: Unknown result type (might be due to invalid IL or missing references)
		//IL_0006: Unknown result type (might be due to invalid IL or missing references)
		//IL_0037: Unknown result type (might be due to invalid IL or missing references)
		//IL_006b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0072: Unknown result type (might be due to invalid IL or missing references)
		//IL_0077: Unknown result type (might be due to invalid IL or missing references)
		Ray aimRay = GetAimRay();
		Transform modelTransform = GetModelTransform();
		if (Object.op_Implicit((Object)(object)modelTransform))
		{
			ChildLocator component = ((Component)modelTransform).GetComponent<ChildLocator>();
			if (Object.op_Implicit((Object)(object)component))
			{
				aimRay.origin = ((Component)component.FindChild("TrackingBombMuzzle")).transform.position;
			}
		}
		EffectManager.SimpleMuzzleFlash(muzzleEffectPrefab, base.gameObject, "TrackingBombMuzzle", transmit: false);
		if (base.isAuthority)
		{
			ProjectileManager.instance.FireProjectile(projectilePrefab, aimRay.origin, Util.QuaternionSafeLookRotation(aimRay.direction), base.gameObject, damageStat * bombDamageCoefficient, bombForce, Util.CheckRoll(critStat, base.characterBody.master));
		}
	}
}
