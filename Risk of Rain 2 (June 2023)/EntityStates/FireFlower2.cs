using RoR2;
using RoR2.Projectile;
using UnityEngine;
using UnityEngine.Networking;

namespace EntityStates;

public class FireFlower2 : BaseState
{
	public static GameObject projectilePrefab;

	public static float baseDuration;

	public static float damageCoefficient;

	public static float healthCostFraction;

	public static string enterSoundString;

	public static string muzzleName;

	public static GameObject muzzleFlashPrefab;

	private float duration;

	public override void OnEnter()
	{
		//IL_0066: Unknown result type (might be due to invalid IL or missing references)
		//IL_006b: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b9: Unknown result type (might be due to invalid IL or missing references)
		//IL_00be: Unknown result type (might be due to invalid IL or missing references)
		//IL_00e0: Unknown result type (might be due to invalid IL or missing references)
		//IL_00e5: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ea: Unknown result type (might be due to invalid IL or missing references)
		//IL_013f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0144: Unknown result type (might be due to invalid IL or missing references)
		//IL_014a: Unknown result type (might be due to invalid IL or missing references)
		//IL_014f: Unknown result type (might be due to invalid IL or missing references)
		base.OnEnter();
		EffectManager.SimpleMuzzleFlash(muzzleFlashPrefab, base.gameObject, muzzleName, transmit: false);
		duration = baseDuration / attackSpeedStat;
		Util.PlaySound(enterSoundString, base.gameObject);
		PlayAnimation("Gesture, Additive", "FireFlower", "FireFlower.playbackRate", duration);
		if (base.isAuthority)
		{
			Ray aimRay = GetAimRay();
			FireProjectileInfo fireProjectileInfo = default(FireProjectileInfo);
			fireProjectileInfo.crit = RollCrit();
			fireProjectileInfo.damage = damageCoefficient * damageStat;
			fireProjectileInfo.damageColorIndex = DamageColorIndex.Default;
			fireProjectileInfo.force = 0f;
			fireProjectileInfo.owner = base.gameObject;
			fireProjectileInfo.position = ((Ray)(ref aimRay)).origin;
			fireProjectileInfo.procChainMask = default(ProcChainMask);
			fireProjectileInfo.projectilePrefab = projectilePrefab;
			fireProjectileInfo.rotation = Quaternion.LookRotation(((Ray)(ref aimRay)).direction);
			fireProjectileInfo.useSpeedOverride = false;
			FireProjectileInfo fireProjectileInfo2 = fireProjectileInfo;
			ProjectileManager.instance.FireProjectile(fireProjectileInfo2);
		}
		if (NetworkServer.active && Object.op_Implicit((Object)(object)base.healthComponent))
		{
			DamageInfo damageInfo = new DamageInfo();
			damageInfo.damage = base.healthComponent.combinedHealth * healthCostFraction;
			damageInfo.position = base.characterBody.corePosition;
			damageInfo.force = Vector3.zero;
			damageInfo.damageColorIndex = DamageColorIndex.Default;
			damageInfo.crit = false;
			damageInfo.attacker = null;
			damageInfo.inflictor = null;
			damageInfo.damageType = DamageType.NonLethal | DamageType.BypassArmor;
			damageInfo.procCoefficient = 0f;
			damageInfo.procChainMask = default(ProcChainMask);
			base.healthComponent.TakeDamage(damageInfo);
		}
	}

	public override void FixedUpdate()
	{
		base.FixedUpdate();
		if (base.isAuthority && base.fixedAge >= duration)
		{
			outer.SetNextStateToMain();
		}
	}

	public override InterruptPriority GetMinimumInterruptPriority()
	{
		return InterruptPriority.PrioritySkill;
	}
}
