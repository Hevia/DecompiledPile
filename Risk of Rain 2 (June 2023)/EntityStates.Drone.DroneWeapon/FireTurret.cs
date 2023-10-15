using RoR2;
using UnityEngine;

namespace EntityStates.Drone.DroneWeapon;

public class FireTurret : BaseState
{
	public static GameObject effectPrefab;

	public static GameObject hitEffectPrefab;

	public static GameObject tracerEffectPrefab;

	public static string attackSoundString;

	public static float damageCoefficient;

	public static float force;

	public static float minSpread;

	public static float maxSpread;

	public static int bulletCount;

	public static float durationBetweenShots = 1f;

	public static float baseDuration = 2f;

	public int bulletCountCurrent = 1;

	private float duration = 2f;

	public override void OnEnter()
	{
		//IL_0059: Unknown result type (might be due to invalid IL or missing references)
		//IL_005e: Unknown result type (might be due to invalid IL or missing references)
		//IL_007f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0084: Unknown result type (might be due to invalid IL or missing references)
		//IL_008c: Unknown result type (might be due to invalid IL or missing references)
		base.OnEnter();
		duration = baseDuration / attackSpeedStat;
		string muzzleName = "Muzzle";
		Util.PlaySound(attackSoundString, base.gameObject);
		if (Object.op_Implicit((Object)(object)effectPrefab))
		{
			EffectManager.SimpleMuzzleFlash(effectPrefab, base.gameObject, muzzleName, transmit: false);
		}
		if (base.isAuthority)
		{
			Ray aimRay = GetAimRay();
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
			bulletAttack.muzzleName = muzzleName;
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
		if (base.fixedAge >= durationBetweenShots / attackSpeedStat && bulletCountCurrent < bulletCount && base.isAuthority)
		{
			FireTurret fireTurret = new FireTurret();
			fireTurret.bulletCountCurrent = bulletCountCurrent + 1;
			outer.SetNextState(fireTurret);
		}
		else if (base.fixedAge >= duration && bulletCountCurrent >= bulletCount && base.isAuthority)
		{
			outer.SetNextStateToMain();
		}
	}

	public override InterruptPriority GetMinimumInterruptPriority()
	{
		return InterruptPriority.Skill;
	}
}
