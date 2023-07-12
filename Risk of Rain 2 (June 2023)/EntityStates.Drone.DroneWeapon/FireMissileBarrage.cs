using RoR2;
using RoR2.Projectile;
using UnityEngine;

namespace EntityStates.Drone.DroneWeapon;

public class FireMissileBarrage : BaseState
{
	public static GameObject effectPrefab;

	public static GameObject projectilePrefab;

	public static float damageCoefficient = 1f;

	public static float baseFireInterval = 0.1f;

	public static float minSpread = 0f;

	public static float maxSpread = 5f;

	public static int maxMissileCount;

	private float fireTimer;

	private float fireInterval;

	private Transform modelTransform;

	private AimAnimator aimAnimator;

	private int missileCount;

	private void FireMissile(string targetMuzzle)
	{
		//IL_001f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0024: Unknown result type (might be due to invalid IL or missing references)
		//IL_0059: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ca: Unknown result type (might be due to invalid IL or missing references)
		//IL_00cf: Unknown result type (might be due to invalid IL or missing references)
		//IL_00d1: Unknown result type (might be due to invalid IL or missing references)
		//IL_00d5: Unknown result type (might be due to invalid IL or missing references)
		//IL_00da: Unknown result type (might be due to invalid IL or missing references)
		//IL_00df: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ed: Unknown result type (might be due to invalid IL or missing references)
		//IL_00fd: Unknown result type (might be due to invalid IL or missing references)
		//IL_0102: Unknown result type (might be due to invalid IL or missing references)
		//IL_0107: Unknown result type (might be due to invalid IL or missing references)
		//IL_010c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0111: Unknown result type (might be due to invalid IL or missing references)
		//IL_0113: Unknown result type (might be due to invalid IL or missing references)
		//IL_0128: Unknown result type (might be due to invalid IL or missing references)
		//IL_012f: Unknown result type (might be due to invalid IL or missing references)
		//IL_015d: Unknown result type (might be due to invalid IL or missing references)
		//IL_015f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0166: Unknown result type (might be due to invalid IL or missing references)
		//IL_0168: Unknown result type (might be due to invalid IL or missing references)
		//IL_016f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0174: Unknown result type (might be due to invalid IL or missing references)
		//IL_0179: Unknown result type (might be due to invalid IL or missing references)
		//IL_017e: Unknown result type (might be due to invalid IL or missing references)
		//IL_018c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0191: Unknown result type (might be due to invalid IL or missing references)
		//IL_0193: Unknown result type (might be due to invalid IL or missing references)
		missileCount++;
		PlayAnimation("Gesture, Additive", "FireMissile");
		Ray aimRay = GetAimRay();
		if (Object.op_Implicit((Object)(object)modelTransform))
		{
			ChildLocator component = ((Component)modelTransform).GetComponent<ChildLocator>();
			if (Object.op_Implicit((Object)(object)component))
			{
				Transform val = component.FindChild(targetMuzzle);
				if (Object.op_Implicit((Object)(object)val))
				{
					((Ray)(ref aimRay)).origin = val.position;
				}
			}
		}
		if (Object.op_Implicit((Object)(object)effectPrefab))
		{
			EffectManager.SimpleMuzzleFlash(effectPrefab, base.gameObject, targetMuzzle, transmit: false);
		}
		if (Object.op_Implicit((Object)(object)base.characterBody))
		{
			base.characterBody.SetAimTimer(2f);
		}
		if (base.isAuthority)
		{
			float num = Random.Range(minSpread, maxSpread);
			float num2 = Random.Range(0f, 360f);
			Vector3 up = Vector3.up;
			Vector3 val2 = Vector3.Cross(up, ((Ray)(ref aimRay)).direction);
			Vector3 val3 = Quaternion.Euler(0f, 0f, num2) * (Quaternion.Euler(num, 0f, 0f) * Vector3.forward);
			float y = val3.y;
			val3.y = 0f;
			float num3 = Mathf.Atan2(val3.z, val3.x) * 57.29578f - 90f;
			float num4 = Mathf.Atan2(y, ((Vector3)(ref val3)).magnitude) * 57.29578f;
			Vector3 forward = Quaternion.AngleAxis(num3, up) * (Quaternion.AngleAxis(num4, val2) * ((Ray)(ref aimRay)).direction);
			ProjectileManager.instance.FireProjectile(projectilePrefab, ((Ray)(ref aimRay)).origin, Util.QuaternionSafeLookRotation(forward), base.gameObject, damageStat * damageCoefficient, 0f, Util.CheckRoll(critStat, base.characterBody.master));
		}
	}

	public override void OnEnter()
	{
		base.OnEnter();
		modelTransform = GetModelTransform();
		fireInterval = baseFireInterval / attackSpeedStat;
	}

	public override void OnExit()
	{
		base.OnExit();
	}

	public override void FixedUpdate()
	{
		base.FixedUpdate();
		fireTimer -= Time.fixedDeltaTime;
		if (fireTimer <= 0f)
		{
			FireMissile("Muzzle");
			fireTimer += fireInterval;
		}
		if (missileCount >= maxMissileCount && base.isAuthority)
		{
			outer.SetNextStateToMain();
		}
	}

	public override InterruptPriority GetMinimumInterruptPriority()
	{
		return InterruptPriority.Skill;
	}
}
