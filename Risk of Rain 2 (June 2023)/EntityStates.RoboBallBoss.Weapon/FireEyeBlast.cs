using RoR2;
using RoR2.Projectile;
using UnityEngine;

namespace EntityStates.RoboBallBoss.Weapon;

public class FireEyeBlast : BaseState
{
	[SerializeField]
	public GameObject projectilePrefab;

	public static GameObject muzzleflashEffectPrefab;

	[SerializeField]
	public int projectileCount = 3;

	[SerializeField]
	public float totalYawSpread = 5f;

	[SerializeField]
	public float baseDuration = 2f;

	[SerializeField]
	public float baseFireDuration = 0.2f;

	[SerializeField]
	public float damageCoefficient = 1.2f;

	[SerializeField]
	public float projectileSpeed;

	public static float force = 20f;

	public static float selfForce;

	public static string attackString;

	public static string muzzleString;

	private float duration;

	private float fireDuration;

	private int projectilesFired;

	private bool projectileSpreadIsYaw;

	public override void OnEnter()
	{
		//IL_0067: Unknown result type (might be due to invalid IL or missing references)
		//IL_006c: Unknown result type (might be due to invalid IL or missing references)
		//IL_006f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0079: Unknown result type (might be due to invalid IL or missing references)
		base.OnEnter();
		duration = baseDuration / attackSpeedStat;
		fireDuration = baseFireDuration / attackSpeedStat;
		PlayAnimation("Gesture, Additive", "FireEyeBlast", "FireEyeBlast.playbackRate", duration);
		Util.PlaySound(attackString, base.gameObject);
		if (base.isAuthority)
		{
			HealthComponent obj = base.healthComponent;
			Ray aimRay = GetAimRay();
			obj.TakeDamageForce(((Ray)(ref aimRay)).direction * selfForce);
		}
		if (Random.value <= 0.5f)
		{
			projectileSpreadIsYaw = true;
		}
	}

	public override void OnExit()
	{
		base.OnExit();
	}

	public override void FixedUpdate()
	{
		//IL_006c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0071: Unknown result type (might be due to invalid IL or missing references)
		//IL_00da: Unknown result type (might be due to invalid IL or missing references)
		//IL_00f7: Unknown result type (might be due to invalid IL or missing references)
		//IL_00fc: Unknown result type (might be due to invalid IL or missing references)
		//IL_010b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0110: Unknown result type (might be due to invalid IL or missing references)
		//IL_0112: Unknown result type (might be due to invalid IL or missing references)
		base.FixedUpdate();
		if (base.isAuthority)
		{
			int num = Mathf.FloorToInt(base.fixedAge / fireDuration * (float)projectileCount);
			if (projectilesFired <= num && projectilesFired < projectileCount)
			{
				if (Object.op_Implicit((Object)(object)muzzleflashEffectPrefab))
				{
					EffectManager.SimpleMuzzleFlash(muzzleflashEffectPrefab, base.gameObject, muzzleString, transmit: false);
				}
				Ray aimRay = GetAimRay();
				float speedOverride = projectileSpeed;
				int num2 = Mathf.FloorToInt((float)projectilesFired - (float)(projectileCount - 1) / 2f);
				float bonusYaw = 0f;
				float bonusPitch = 0f;
				if (projectileSpreadIsYaw)
				{
					bonusYaw = (float)num2 / (float)(projectileCount - 1) * totalYawSpread;
				}
				else
				{
					bonusPitch = (float)num2 / (float)(projectileCount - 1) * totalYawSpread;
				}
				Vector3 forward = Util.ApplySpread(((Ray)(ref aimRay)).direction, 0f, 0f, 1f, 1f, bonusYaw, bonusPitch);
				ProjectileManager.instance.FireProjectile(projectilePrefab, ((Ray)(ref aimRay)).origin, Util.QuaternionSafeLookRotation(forward), base.gameObject, damageStat * damageCoefficient, force, Util.CheckRoll(critStat, base.characterBody.master), DamageColorIndex.Default, null, speedOverride);
				projectilesFired++;
			}
		}
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
