using RoR2;
using RoR2.Projectile;
using UnityEngine;

namespace EntityStates.VoidSurvivor.Weapon;

public class FireTwinBlaster : BaseSkillState
{
	[SerializeField]
	public GameObject projectile1Prefab;

	[SerializeField]
	public GameObject projectile2Prefab;

	[SerializeField]
	public GameObject muzzleflashEffectPrefab;

	[SerializeField]
	public float baseDuration = 2f;

	[SerializeField]
	public float damageCoefficient = 1.2f;

	[SerializeField]
	public float force = 20f;

	[SerializeField]
	public string attackSoundString;

	[SerializeField]
	public float attackSoundPitchPerStep;

	[SerializeField]
	public float recoilAmplitude;

	[SerializeField]
	public float bloom;

	[SerializeField]
	public string muzzle;

	[SerializeField]
	public float spread;

	[SerializeField]
	public string animationLayerName;

	[SerializeField]
	public string animationStateName;

	[SerializeField]
	public string animationPlaybackRateParam;

	private float duration;

	private float interruptDuration;

	public int step;

	public override void OnEnter()
	{
		//IL_0018: Unknown result type (might be due to invalid IL or missing references)
		base.OnEnter();
		base.activatorSkillSlot = base.skillLocator.primary;
		GetAimRay();
		duration = baseDuration / attackSpeedStat;
		StartAimMode(duration + 2f);
		PlayAnimation(animationLayerName, animationStateName, animationPlaybackRateParam, duration);
		Util.PlayAttackSpeedSound(attackSoundString, base.gameObject, 1f + (float)step * attackSoundPitchPerStep);
		AddRecoil(-1f * recoilAmplitude, -1.5f * recoilAmplitude, -0.25f * recoilAmplitude, 0.25f * recoilAmplitude);
		base.characterBody.AddSpreadBloom(bloom);
		if (Object.op_Implicit((Object)(object)muzzleflashEffectPrefab))
		{
			EffectManager.SimpleMuzzleFlash(muzzleflashEffectPrefab, base.gameObject, muzzle, transmit: false);
		}
		if (base.isAuthority)
		{
			FireProjectiles();
		}
		Debug.Log((object)step);
	}

	private void FireProjectiles()
	{
		//IL_0001: Unknown result type (might be due to invalid IL or missing references)
		//IL_0006: Unknown result type (might be due to invalid IL or missing references)
		//IL_000b: Unknown result type (might be due to invalid IL or missing references)
		//IL_003b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0053: Unknown result type (might be due to invalid IL or missing references)
		//IL_0058: Unknown result type (might be due to invalid IL or missing references)
		//IL_0061: Unknown result type (might be due to invalid IL or missing references)
		//IL_0066: Unknown result type (might be due to invalid IL or missing references)
		//IL_006b: Unknown result type (might be due to invalid IL or missing references)
		Ray aimRay = GetAimRay();
		((Ray)(ref aimRay)).direction = Util.ApplySpread(((Ray)(ref aimRay)).direction, 0f, base.characterBody.spreadBloomAngle * spread, 1f, 1f);
		FireProjectileInfo fireProjectileInfo = default(FireProjectileInfo);
		fireProjectileInfo.position = ((Ray)(ref aimRay)).origin;
		fireProjectileInfo.rotation = Util.QuaternionSafeLookRotation(((Ray)(ref aimRay)).direction);
		fireProjectileInfo.owner = base.gameObject;
		fireProjectileInfo.damage = damageStat * damageCoefficient;
		fireProjectileInfo.force = force;
		fireProjectileInfo.crit = Util.CheckRoll(critStat, base.characterBody.master);
		fireProjectileInfo.projectilePrefab = projectile1Prefab;
		ProjectileManager.instance.FireProjectile(fireProjectileInfo);
		fireProjectileInfo.projectilePrefab = projectile2Prefab;
		ProjectileManager.instance.FireProjectile(fireProjectileInfo);
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
		return InterruptPriority.Skill;
	}
}
