using RoR2;
using RoR2.Projectile;
using UnityEngine;

namespace EntityStates.Treebot;

public class TreebotFireFruitSeed : BaseState
{
	[SerializeField]
	public GameObject projectilePrefab;

	[SerializeField]
	public float baseDuration;

	[SerializeField]
	public float damageCoefficient;

	[SerializeField]
	public string enterSoundString;

	[SerializeField]
	public string muzzleName;

	[SerializeField]
	public GameObject muzzleFlashPrefab;

	[SerializeField]
	public string animationLayerName = "Gesture, Additive";

	[SerializeField]
	public string animationStateName = "FireFlower";

	[SerializeField]
	public string playbackRateParam = "FireFlower.playbackRate";

	private float duration;

	public override void OnEnter()
	{
		//IL_006d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0072: Unknown result type (might be due to invalid IL or missing references)
		//IL_00c1: Unknown result type (might be due to invalid IL or missing references)
		//IL_00c6: Unknown result type (might be due to invalid IL or missing references)
		//IL_00e9: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ee: Unknown result type (might be due to invalid IL or missing references)
		//IL_00f3: Unknown result type (might be due to invalid IL or missing references)
		base.OnEnter();
		EffectManager.SimpleMuzzleFlash(muzzleFlashPrefab, base.gameObject, muzzleName, transmit: false);
		duration = baseDuration / attackSpeedStat;
		Util.PlaySound(enterSoundString, base.gameObject);
		PlayAnimation(animationLayerName, animationStateName, playbackRateParam, duration);
		if (base.isAuthority)
		{
			Ray aimRay = GetAimRay();
			FireProjectileInfo fireProjectileInfo = default(FireProjectileInfo);
			fireProjectileInfo.crit = RollCrit();
			fireProjectileInfo.damage = damageCoefficient * damageStat;
			fireProjectileInfo.damageColorIndex = DamageColorIndex.Default;
			fireProjectileInfo.force = 0f;
			fireProjectileInfo.owner = base.gameObject;
			fireProjectileInfo.position = aimRay.origin;
			fireProjectileInfo.procChainMask = default(ProcChainMask);
			fireProjectileInfo.projectilePrefab = projectilePrefab;
			fireProjectileInfo.rotation = Quaternion.LookRotation(aimRay.direction);
			fireProjectileInfo.useSpeedOverride = false;
			FireProjectileInfo fireProjectileInfo2 = fireProjectileInfo;
			ProjectileManager.instance.FireProjectile(fireProjectileInfo2);
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
