using RoR2;
using RoR2.Projectile;
using UnityEngine;

namespace EntityStates.LunarWisp;

public class SeekingBomb : BaseState
{
	public static float baseDuration = 3f;

	public static GameObject chargingEffectPrefab;

	public static GameObject projectilePrefab;

	public static string spinUpSoundString;

	public static string fireBombSoundString;

	public static string spinDownSoundString;

	public static float bombDamageCoefficient;

	public static float bombForce;

	public static string muzzleName;

	public float novaRadius;

	private float duration;

	public static float spinUpDuration;

	private bool upToSpeed;

	private GameObject chargeEffectInstance;

	private uint chargeLoopSoundID;

	public override void OnEnter()
	{
		base.OnEnter();
		duration = (baseDuration + spinUpDuration) / attackSpeedStat;
		chargeLoopSoundID = Util.PlaySound(spinUpSoundString, base.gameObject);
		PlayCrossfade("Gesture", "BombStart", 0.2f);
		if (Object.op_Implicit((Object)(object)base.characterBody))
		{
			base.characterBody.SetAimTimer(duration);
		}
	}

	public override void OnExit()
	{
		base.OnExit();
		AkSoundEngine.StopPlayingID(chargeLoopSoundID);
		if (Object.op_Implicit((Object)(object)chargeEffectInstance))
		{
			EntityState.Destroy((Object)(object)chargeEffectInstance);
		}
	}

	public override void FixedUpdate()
	{
		//IL_006d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0073: Unknown result type (might be due to invalid IL or missing references)
		base.FixedUpdate();
		if (base.fixedAge >= spinUpDuration && !upToSpeed)
		{
			upToSpeed = true;
			Transform modelTransform = GetModelTransform();
			if (Object.op_Implicit((Object)(object)modelTransform))
			{
				ChildLocator component = ((Component)modelTransform).GetComponent<ChildLocator>();
				if (Object.op_Implicit((Object)(object)component))
				{
					Transform val = component.FindChild(muzzleName);
					if (Object.op_Implicit((Object)(object)val) && Object.op_Implicit((Object)(object)chargingEffectPrefab))
					{
						chargeEffectInstance = Object.Instantiate<GameObject>(chargingEffectPrefab, val.position, val.rotation);
						chargeEffectInstance.transform.parent = val;
						chargeEffectInstance.GetComponent<ScaleParticleSystemDuration>().newDuration = duration;
					}
				}
			}
		}
		if (base.fixedAge >= duration && base.isAuthority)
		{
			FireBomb();
			outer.SetNextStateToMain();
		}
	}

	public override InterruptPriority GetMinimumInterruptPriority()
	{
		return InterruptPriority.PrioritySkill;
	}

	private void FireBomb()
	{
		//IL_0012: Unknown result type (might be due to invalid IL or missing references)
		//IL_0017: Unknown result type (might be due to invalid IL or missing references)
		//IL_0048: Unknown result type (might be due to invalid IL or missing references)
		//IL_0066: Unknown result type (might be due to invalid IL or missing references)
		//IL_006d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0072: Unknown result type (might be due to invalid IL or missing references)
		Util.PlaySound(fireBombSoundString, base.gameObject);
		Ray aimRay = GetAimRay();
		Transform modelTransform = GetModelTransform();
		if (Object.op_Implicit((Object)(object)modelTransform))
		{
			ChildLocator component = ((Component)modelTransform).GetComponent<ChildLocator>();
			if (Object.op_Implicit((Object)(object)component))
			{
				aimRay.origin = ((Component)component.FindChild(muzzleName)).transform.position;
			}
		}
		if (base.isAuthority)
		{
			ProjectileManager.instance.FireProjectile(projectilePrefab, aimRay.origin, Util.QuaternionSafeLookRotation(aimRay.direction), base.gameObject, damageStat * bombDamageCoefficient, bombForce, Util.CheckRoll(critStat, base.characterBody.master));
		}
		Util.PlaySound(spinDownSoundString, base.gameObject);
		PlayCrossfade("Gesture", "BombStop", 0.2f);
	}
}
