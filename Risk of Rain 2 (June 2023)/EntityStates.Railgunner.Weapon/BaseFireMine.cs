using RoR2;
using RoR2.Projectile;
using UnityEngine;

namespace EntityStates.Railgunner.Weapon;

public class BaseFireMine : BaseState
{
	[SerializeField]
	public float baseDuration;

	[SerializeField]
	public float baseCrossfadeDuration;

	[SerializeField]
	public GameObject muzzleEffectPrefab;

	[SerializeField]
	public GameObject projectilePrefab;

	[SerializeField]
	public string muzzleName;

	[SerializeField]
	public float damageCoefficient;

	[SerializeField]
	public float force;

	[SerializeField]
	public string enterSoundString;

	[SerializeField]
	public string animationLayerName;

	[SerializeField]
	public string animationStateName;

	[SerializeField]
	public string animationPlaybackRateParam;

	private float duration;

	public override void OnEnter()
	{
		//IL_003a: Unknown result type (might be due to invalid IL or missing references)
		//IL_003f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0041: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b3: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ba: Unknown result type (might be due to invalid IL or missing references)
		//IL_00bf: Unknown result type (might be due to invalid IL or missing references)
		base.OnEnter();
		duration = baseDuration / attackSpeedStat;
		float crossfadeDuration = baseCrossfadeDuration / attackSpeedStat;
		Util.PlaySound(enterSoundString, base.gameObject);
		Ray aimRay = GetAimRay();
		StartAimMode(aimRay);
		if (Object.op_Implicit((Object)(object)GetModelAnimator()))
		{
			PlayCrossfade(animationLayerName, animationStateName, animationPlaybackRateParam, duration, crossfadeDuration);
		}
		if (Object.op_Implicit((Object)(object)muzzleEffectPrefab))
		{
			EffectManager.SimpleMuzzleFlash(muzzleEffectPrefab, base.gameObject, muzzleName, transmit: false);
		}
		if (base.isAuthority)
		{
			ProjectileManager.instance.FireProjectile(projectilePrefab, aimRay.origin, Util.QuaternionSafeLookRotation(aimRay.direction), base.gameObject, damageStat * damageCoefficient, force, Util.CheckRoll(critStat, base.characterBody.master));
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
