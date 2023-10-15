using RoR2;
using RoR2.Projectile;
using UnityEngine;

namespace EntityStates.Bandit2.Weapon;

public class Bandit2FireShiv : BaseSkillState
{
	[SerializeField]
	public GameObject projectilePrefab;

	[SerializeField]
	public float baseDuration;

	[SerializeField]
	public float damageCoefficient;

	[SerializeField]
	public float force;

	[SerializeField]
	public int maxShivCount;

	public static float baseDelayBetweenShivs;

	public static float shortHopVelocity;

	public static GameObject muzzleEffectPrefab;

	public static string muzzleString;

	private float duration;

	private float delayBetweenShivs;

	private float countdownSinceLastShiv;

	private int shivCount;

	public override void OnEnter()
	{
		//IL_009f: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a4: Unknown result type (might be due to invalid IL or missing references)
		base.OnEnter();
		duration = baseDuration / attackSpeedStat;
		delayBetweenShivs = baseDelayBetweenShivs / attackSpeedStat;
		PlayAnimation("Gesture, Additive", "SlashBlade", "SlashBlade.playbackRate", duration);
		StartAimMode();
		if (Object.op_Implicit((Object)(object)base.characterMotor))
		{
			base.characterMotor.velocity = new Vector3(base.characterMotor.velocity.x, Mathf.Max(base.characterMotor.velocity.y, shortHopVelocity), base.characterMotor.velocity.z);
		}
	}

	public override void OnExit()
	{
		base.OnExit();
	}

	public override void FixedUpdate()
	{
		base.FixedUpdate();
		countdownSinceLastShiv -= Time.fixedDeltaTime;
		if (shivCount < maxShivCount && countdownSinceLastShiv <= 0f)
		{
			shivCount++;
			countdownSinceLastShiv += delayBetweenShivs;
			FireShiv();
		}
		if (base.fixedAge >= duration && base.isAuthority)
		{
			outer.SetNextStateToMain();
		}
	}

	private void FireShiv()
	{
		//IL_002e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0033: Unknown result type (might be due to invalid IL or missing references)
		//IL_005e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0063: Unknown result type (might be due to invalid IL or missing references)
		//IL_006c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0071: Unknown result type (might be due to invalid IL or missing references)
		//IL_0076: Unknown result type (might be due to invalid IL or missing references)
		if (Object.op_Implicit((Object)(object)muzzleEffectPrefab))
		{
			EffectManager.SimpleMuzzleFlash(muzzleEffectPrefab, base.gameObject, muzzleString, transmit: false);
		}
		if (base.isAuthority)
		{
			Ray aimRay = GetAimRay();
			if ((Object)(object)projectilePrefab != (Object)null)
			{
				FireProjectileInfo fireProjectileInfo = default(FireProjectileInfo);
				fireProjectileInfo.projectilePrefab = projectilePrefab;
				fireProjectileInfo.position = aimRay.origin;
				fireProjectileInfo.rotation = Util.QuaternionSafeLookRotation(aimRay.direction);
				fireProjectileInfo.owner = base.gameObject;
				fireProjectileInfo.damage = damageStat * damageCoefficient;
				fireProjectileInfo.force = force;
				fireProjectileInfo.crit = RollCrit();
				fireProjectileInfo.damageTypeOverride = DamageType.SuperBleedOnCrit;
				FireProjectileInfo fireProjectileInfo2 = fireProjectileInfo;
				ProjectileManager.instance.FireProjectile(fireProjectileInfo2);
			}
		}
	}

	public override InterruptPriority GetMinimumInterruptPriority()
	{
		return InterruptPriority.PrioritySkill;
	}
}
