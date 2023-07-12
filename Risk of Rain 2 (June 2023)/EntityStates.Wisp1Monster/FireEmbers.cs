using RoR2;
using UnityEngine;

namespace EntityStates.Wisp1Monster;

public class FireEmbers : BaseState
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

	public static string attackString;

	private float duration;

	public override void OnEnter()
	{
		//IL_0030: Unknown result type (might be due to invalid IL or missing references)
		//IL_0035: Unknown result type (might be due to invalid IL or missing references)
		//IL_0037: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ad: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b2: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ba: Unknown result type (might be due to invalid IL or missing references)
		base.OnEnter();
		duration = baseDuration / attackSpeedStat;
		Util.PlayAttackSpeedSound(attackString, base.gameObject, attackSpeedStat);
		Ray aimRay = GetAimRay();
		StartAimMode(aimRay);
		PlayAnimation("Body", "FireAttack1", "FireAttack1.playbackRate", duration);
		string muzzleName = "";
		if (Object.op_Implicit((Object)(object)effectPrefab))
		{
			EffectManager.SimpleMuzzleFlash(effectPrefab, base.gameObject, muzzleName, transmit: false);
		}
		if (base.isAuthority)
		{
			BulletAttack bulletAttack = new BulletAttack();
			bulletAttack.owner = base.gameObject;
			bulletAttack.weapon = base.gameObject;
			bulletAttack.origin = ((Ray)(ref aimRay)).origin;
			bulletAttack.aimVector = ((Ray)(ref aimRay)).direction;
			bulletAttack.minSpread = minSpread;
			bulletAttack.maxSpread = maxSpread;
			bulletAttack.bulletCount = ((bulletCount > 0) ? ((uint)bulletCount) : 0u);
			bulletAttack.damage = damageCoefficient * damageStat;
			bulletAttack.force = force;
			bulletAttack.tracerEffectPrefab = tracerEffectPrefab;
			bulletAttack.muzzleName = muzzleName;
			bulletAttack.hitEffectPrefab = hitEffectPrefab;
			bulletAttack.isCrit = Util.CheckRoll(critStat, base.characterBody.master);
			bulletAttack.falloffModel = BulletAttack.FalloffModel.DefaultBullet;
			bulletAttack.HitEffectNormal = false;
			bulletAttack.radius = 0.5f;
			bulletAttack.procCoefficient = 1f / (float)bulletCount;
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
