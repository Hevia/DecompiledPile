using RoR2;
using UnityEngine;

namespace EntityStates.Drone.DroneWeapon;

public class FireMegaTurret : BaseState
{
	public static GameObject effectPrefab;

	public static GameObject hitEffectPrefab;

	public static GameObject tracerEffectPrefab;

	public static string attackSoundString;

	public static float attackSoundPlaybackCoefficient;

	public static float damageCoefficient;

	public static float force;

	public static float minSpread;

	public static float maxSpread;

	public static int maxBulletCount;

	public static float baseTotalDuration;

	private Transform modelTransform;

	private ChildLocator childLocator;

	private float fireStopwatch;

	private float stopwatch;

	private float durationBetweenShots;

	private float totalDuration;

	private int bulletCount;

	public override void OnEnter()
	{
		//IL_0037: Unknown result type (might be due to invalid IL or missing references)
		base.OnEnter();
		fireStopwatch = 0f;
		totalDuration = baseTotalDuration / attackSpeedStat;
		durationBetweenShots = totalDuration / (float)maxBulletCount;
		GetAimRay();
		Transform val = GetModelTransform();
		if (Object.op_Implicit((Object)(object)val))
		{
			childLocator = ((Component)val).GetComponent<ChildLocator>();
		}
	}

	private void FireBullet(string muzzleString)
	{
		//IL_0001: Unknown result type (might be due to invalid IL or missing references)
		//IL_0006: Unknown result type (might be due to invalid IL or missing references)
		//IL_0009: Unknown result type (might be due to invalid IL or missing references)
		//IL_0058: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a7: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ac: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b4: Unknown result type (might be due to invalid IL or missing references)
		Ray aimRay = GetAimRay();
		_ = aimRay.origin;
		Util.PlayAttackSpeedSound(attackSoundString, base.gameObject, attackSoundPlaybackCoefficient);
		PlayAnimation("Gesture, Additive", "FireGat");
		if (Object.op_Implicit((Object)(object)childLocator))
		{
			Transform val = childLocator.FindChild(muzzleString);
			if (Object.op_Implicit((Object)(object)val))
			{
				_ = val.position;
			}
		}
		if (Object.op_Implicit((Object)(object)effectPrefab))
		{
			EffectManager.SimpleMuzzleFlash(effectPrefab, base.gameObject, muzzleString, transmit: false);
		}
		if (base.isAuthority)
		{
			BulletAttack bulletAttack = new BulletAttack();
			bulletAttack.owner = base.gameObject;
			bulletAttack.weapon = base.gameObject;
			bulletAttack.origin = aimRay.origin;
			bulletAttack.aimVector = aimRay.direction;
			bulletAttack.minSpread = minSpread;
			bulletAttack.maxSpread = maxSpread;
			bulletAttack.damage = damageCoefficient * damageStat;
			bulletAttack.force = force;
			bulletAttack.tracerEffectPrefab = tracerEffectPrefab;
			bulletAttack.muzzleName = muzzleString;
			bulletAttack.hitEffectPrefab = hitEffectPrefab;
			bulletAttack.isCrit = Util.CheckRoll(critStat, base.characterBody.master);
			bulletAttack.Fire();
		}
	}

	public override void OnExit()
	{
		base.OnExit();
	}

	public override void FixedUpdate()
	{
		base.FixedUpdate();
		fireStopwatch += Time.fixedDeltaTime;
		stopwatch += Time.fixedDeltaTime;
		if (fireStopwatch >= durationBetweenShots)
		{
			bulletCount++;
			fireStopwatch -= durationBetweenShots;
			FireBullet((bulletCount % 2 == 0) ? "GatLeft" : "GatRight");
		}
		if (stopwatch >= totalDuration && base.isAuthority)
		{
			outer.SetNextStateToMain();
		}
	}

	public override InterruptPriority GetMinimumInterruptPriority()
	{
		return InterruptPriority.Skill;
	}
}
