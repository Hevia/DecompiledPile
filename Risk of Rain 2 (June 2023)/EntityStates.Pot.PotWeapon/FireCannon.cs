using RoR2;
using RoR2.Projectile;
using UnityEngine;

namespace EntityStates.Pot.PotWeapon;

public class FireCannon : BaseState
{
	public static GameObject effectPrefab;

	public static GameObject hitEffectPrefab;

	public static GameObject projectilePrefab;

	public static float selfForce = 1000f;

	public static int grenadeCountMax = 3;

	public static float damageCoefficient;

	public static float fireDuration = 1f;

	public static float baseDuration = 2f;

	public static float minSpread = 0f;

	public static float maxSpread = 5f;

	public static float arcAngle = 5f;

	private Ray aimRay;

	private Transform modelTransform;

	private float duration;

	private float fireTimer;

	private int grenadeCount;

	private void FireBullet(string targetMuzzle)
	{
		//IL_0002: Unknown result type (might be due to invalid IL or missing references)
		//IL_0007: Unknown result type (might be due to invalid IL or missing references)
		//IL_0044: Unknown result type (might be due to invalid IL or missing references)
		//IL_004e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0054: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a8: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ad: Unknown result type (might be due to invalid IL or missing references)
		//IL_00af: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b7: Unknown result type (might be due to invalid IL or missing references)
		//IL_00bc: Unknown result type (might be due to invalid IL or missing references)
		//IL_00c1: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ce: Unknown result type (might be due to invalid IL or missing references)
		//IL_00de: Unknown result type (might be due to invalid IL or missing references)
		//IL_00e3: Unknown result type (might be due to invalid IL or missing references)
		//IL_00e8: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ed: Unknown result type (might be due to invalid IL or missing references)
		//IL_00f2: Unknown result type (might be due to invalid IL or missing references)
		//IL_00f4: Unknown result type (might be due to invalid IL or missing references)
		//IL_0109: Unknown result type (might be due to invalid IL or missing references)
		//IL_0110: Unknown result type (might be due to invalid IL or missing references)
		//IL_0144: Unknown result type (might be due to invalid IL or missing references)
		//IL_0146: Unknown result type (might be due to invalid IL or missing references)
		//IL_014d: Unknown result type (might be due to invalid IL or missing references)
		//IL_014f: Unknown result type (might be due to invalid IL or missing references)
		//IL_015a: Unknown result type (might be due to invalid IL or missing references)
		//IL_015f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0164: Unknown result type (might be due to invalid IL or missing references)
		//IL_0169: Unknown result type (might be due to invalid IL or missing references)
		//IL_017b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0180: Unknown result type (might be due to invalid IL or missing references)
		//IL_0182: Unknown result type (might be due to invalid IL or missing references)
		aimRay = GetAimRay();
		if (Object.op_Implicit((Object)(object)modelTransform))
		{
			ChildLocator component = ((Component)modelTransform).GetComponent<ChildLocator>();
			if (Object.op_Implicit((Object)(object)component))
			{
				Transform val = component.FindChild(targetMuzzle);
				if (Object.op_Implicit((Object)(object)val))
				{
					base.rigidbody.AddForceAtPosition(val.forward * selfForce, val.position, (ForceMode)1);
				}
			}
		}
		if (Object.op_Implicit((Object)(object)effectPrefab))
		{
			EffectManager.SimpleMuzzleFlash(effectPrefab, base.gameObject, targetMuzzle, transmit: false);
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
			float num4 = Mathf.Atan2(y, ((Vector3)(ref val3)).magnitude) * 57.29578f + arcAngle;
			Vector3 forward = Quaternion.AngleAxis(num3, up) * (Quaternion.AngleAxis(num4, val2) * ((Ray)(ref aimRay)).direction);
			ProjectileManager.instance.FireProjectile(projectilePrefab, ((Ray)(ref aimRay)).origin, Util.QuaternionSafeLookRotation(forward), base.gameObject, damageStat * damageCoefficient, 0f, Util.CheckRoll(critStat, base.characterBody.master));
		}
	}

	public override void OnEnter()
	{
		//IL_0026: Unknown result type (might be due to invalid IL or missing references)
		//IL_002b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0032: Unknown result type (might be due to invalid IL or missing references)
		base.OnEnter();
		duration = baseDuration / attackSpeedStat;
		modelTransform = GetModelTransform();
		aimRay = GetAimRay();
		StartAimMode(aimRay);
	}

	public override void OnExit()
	{
		base.OnExit();
	}

	public override void FixedUpdate()
	{
		base.FixedUpdate();
		if (!base.isAuthority)
		{
			return;
		}
		fireTimer -= Time.fixedDeltaTime;
		float num = fireDuration / attackSpeedStat / (float)grenadeCountMax;
		if (fireTimer <= 0f && grenadeCount < grenadeCountMax)
		{
			fireTimer += num;
			if (grenadeCount % 2 == 0)
			{
				FireBullet("MuzzleLeft");
				PlayCrossfade("Gesture, Left Cannon", "FireGrenadeLeft", 0.1f);
			}
			else
			{
				FireBullet("MuzzleRight");
				PlayCrossfade("Gesture, Right Cannon", "FireGrenadeRight", 0.1f);
			}
			grenadeCount++;
		}
		if (base.fixedAge >= duration)
		{
			outer.SetNextStateToMain();
		}
	}

	public override InterruptPriority GetMinimumInterruptPriority()
	{
		return InterruptPriority.Skill;
	}
}
