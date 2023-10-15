using RoR2;
using RoR2.Projectile;
using UnityEngine;

namespace EntityStates.Croco;

public class FireSpit : BaseState
{
	[SerializeField]
	public GameObject projectilePrefab;

	[SerializeField]
	public GameObject effectPrefab;

	[SerializeField]
	public float baseDuration = 2f;

	[SerializeField]
	public float damageCoefficient = 1.2f;

	[SerializeField]
	public float force = 20f;

	[SerializeField]
	public string attackString;

	[SerializeField]
	public float recoilAmplitude;

	[SerializeField]
	public float bloom;

	private float duration;

	private CrocoDamageTypeController crocoDamageTypeController;

	public override void OnEnter()
	{
		//IL_0013: Unknown result type (might be due to invalid IL or missing references)
		//IL_0018: Unknown result type (might be due to invalid IL or missing references)
		//IL_011c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0121: Unknown result type (might be due to invalid IL or missing references)
		//IL_012a: Unknown result type (might be due to invalid IL or missing references)
		//IL_012f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0134: Unknown result type (might be due to invalid IL or missing references)
		base.OnEnter();
		crocoDamageTypeController = GetComponent<CrocoDamageTypeController>();
		Ray aimRay = GetAimRay();
		duration = baseDuration / attackSpeedStat;
		StartAimMode(duration + 2f);
		PlayAnimation("Gesture, Mouth", "FireSpit", "FireSpit.playbackRate", duration);
		Util.PlaySound(attackString, base.gameObject);
		AddRecoil(-1f * recoilAmplitude, -1.5f * recoilAmplitude, -0.25f * recoilAmplitude, 0.25f * recoilAmplitude);
		base.characterBody.AddSpreadBloom(bloom);
		string muzzleName = "MouthMuzzle";
		if (Object.op_Implicit((Object)(object)effectPrefab))
		{
			EffectManager.SimpleMuzzleFlash(effectPrefab, base.gameObject, muzzleName, transmit: false);
		}
		if (base.isAuthority)
		{
			DamageType value = (Object.op_Implicit((Object)(object)crocoDamageTypeController) ? crocoDamageTypeController.GetDamageType() : DamageType.Generic);
			FireProjectileInfo fireProjectileInfo = default(FireProjectileInfo);
			fireProjectileInfo.projectilePrefab = projectilePrefab;
			fireProjectileInfo.position = aimRay.origin;
			fireProjectileInfo.rotation = Util.QuaternionSafeLookRotation(aimRay.direction);
			fireProjectileInfo.owner = base.gameObject;
			fireProjectileInfo.damage = damageStat * damageCoefficient;
			fireProjectileInfo.damageTypeOverride = value;
			fireProjectileInfo.force = force;
			fireProjectileInfo.crit = Util.CheckRoll(critStat, base.characterBody.master);
			ProjectileManager.instance.FireProjectile(fireProjectileInfo);
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
