using RoR2;
using RoR2.Projectile;
using UnityEngine;

namespace EntityStates;

public class GenericProjectileBaseState : BaseState
{
	[SerializeField]
	public GameObject effectPrefab;

	[SerializeField]
	public GameObject projectilePrefab;

	[SerializeField]
	public float damageCoefficient;

	[SerializeField]
	public float force;

	[SerializeField]
	public float minSpread;

	[SerializeField]
	public float maxSpread;

	[SerializeField]
	public float baseDuration = 2f;

	[SerializeField]
	public float recoilAmplitude = 1f;

	[SerializeField]
	public string attackSoundString;

	[SerializeField]
	public float projectilePitchBonus;

	[SerializeField]
	public float baseDelayBeforeFiringProjectile;

	[SerializeField]
	public string targetMuzzle;

	[SerializeField]
	public float bloom;

	protected float stopwatch;

	protected float duration;

	protected float delayBeforeFiringProjectile;

	protected bool firedProjectile;

	public override void OnEnter()
	{
		base.OnEnter();
		stopwatch = 0f;
		duration = baseDuration / attackSpeedStat;
		delayBeforeFiringProjectile = baseDelayBeforeFiringProjectile / attackSpeedStat;
		if (Object.op_Implicit((Object)(object)base.characterBody))
		{
			base.characterBody.SetAimTimer(2f);
		}
		PlayAnimation(duration);
	}

	public override void OnExit()
	{
		base.OnExit();
	}

	protected virtual void PlayAnimation(float duration)
	{
	}

	public override void FixedUpdate()
	{
		base.FixedUpdate();
		stopwatch += Time.fixedDeltaTime;
		if (stopwatch >= delayBeforeFiringProjectile && !firedProjectile)
		{
			firedProjectile = true;
			FireProjectile();
			DoFireEffects();
		}
		if (stopwatch >= duration && base.isAuthority)
		{
			outer.SetNextStateToMain();
		}
	}

	protected virtual void FireProjectile()
	{
		//IL_000c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0011: Unknown result type (might be due to invalid IL or missing references)
		//IL_0013: Unknown result type (might be due to invalid IL or missing references)
		//IL_0014: Unknown result type (might be due to invalid IL or missing references)
		//IL_0019: Unknown result type (might be due to invalid IL or missing references)
		//IL_001e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0044: Unknown result type (might be due to invalid IL or missing references)
		//IL_005b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0062: Unknown result type (might be due to invalid IL or missing references)
		//IL_0067: Unknown result type (might be due to invalid IL or missing references)
		if (base.isAuthority)
		{
			Ray aimRay = GetAimRay();
			aimRay = ModifyProjectileAimRay(aimRay);
			aimRay.direction = Util.ApplySpread(aimRay.direction, minSpread, maxSpread, 1f, 1f, 0f, projectilePitchBonus);
			ProjectileManager.instance.FireProjectile(projectilePrefab, aimRay.origin, Util.QuaternionSafeLookRotation(aimRay.direction), base.gameObject, damageStat * damageCoefficient, force, Util.CheckRoll(critStat, base.characterBody.master));
		}
	}

	protected virtual Ray ModifyProjectileAimRay(Ray aimRay)
	{
		//IL_0000: Unknown result type (might be due to invalid IL or missing references)
		return aimRay;
	}

	protected virtual void DoFireEffects()
	{
		Util.PlaySound(attackSoundString, base.gameObject);
		AddRecoil(-2f * recoilAmplitude, -3f * recoilAmplitude, -1f * recoilAmplitude, 1f * recoilAmplitude);
		if (Object.op_Implicit((Object)(object)effectPrefab))
		{
			EffectManager.SimpleMuzzleFlash(effectPrefab, base.gameObject, targetMuzzle, transmit: false);
		}
		base.characterBody.AddSpreadBloom(bloom);
	}

	public override InterruptPriority GetMinimumInterruptPriority()
	{
		return InterruptPriority.PrioritySkill;
	}
}
