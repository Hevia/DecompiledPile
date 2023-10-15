using RoR2;
using RoR2.Projectile;
using UnityEngine;

namespace EntityStates.Engi.EngiWeapon;

public class FireSeekerGrenades : BaseState
{
	public static GameObject effectPrefab;

	public static GameObject hitEffectPrefab;

	public static GameObject projectilePrefab;

	public static int grenadeCountMax = 3;

	public static float damageCoefficient;

	public static float fireDuration = 1f;

	public static float baseDuration = 2f;

	public static float minSpread = 0f;

	public static float maxSpread = 5f;

	public static float arcAngle = 5f;

	public static float recoilAmplitude = 1f;

	public static string attackSoundString;

	private Ray aimRay;

	private Transform modelTransform;

	private float duration;

	private float fireTimer;

	private int grenadeCount;

	private void FireGrenade(string targetMuzzle)
	{
		//IL_0013: Unknown result type (might be due to invalid IL or missing references)
		//IL_0018: Unknown result type (might be due to invalid IL or missing references)
		//IL_0055: Unknown result type (might be due to invalid IL or missing references)
		//IL_00da: Unknown result type (might be due to invalid IL or missing references)
		//IL_00df: Unknown result type (might be due to invalid IL or missing references)
		//IL_00e1: Unknown result type (might be due to invalid IL or missing references)
		//IL_00e9: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ee: Unknown result type (might be due to invalid IL or missing references)
		//IL_00f3: Unknown result type (might be due to invalid IL or missing references)
		//IL_0100: Unknown result type (might be due to invalid IL or missing references)
		//IL_0110: Unknown result type (might be due to invalid IL or missing references)
		//IL_0115: Unknown result type (might be due to invalid IL or missing references)
		//IL_011a: Unknown result type (might be due to invalid IL or missing references)
		//IL_011f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0124: Unknown result type (might be due to invalid IL or missing references)
		//IL_0126: Unknown result type (might be due to invalid IL or missing references)
		//IL_013b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0142: Unknown result type (might be due to invalid IL or missing references)
		//IL_0176: Unknown result type (might be due to invalid IL or missing references)
		//IL_0178: Unknown result type (might be due to invalid IL or missing references)
		//IL_017f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0181: Unknown result type (might be due to invalid IL or missing references)
		//IL_018c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0191: Unknown result type (might be due to invalid IL or missing references)
		//IL_0196: Unknown result type (might be due to invalid IL or missing references)
		//IL_019b: Unknown result type (might be due to invalid IL or missing references)
		//IL_01ad: Unknown result type (might be due to invalid IL or missing references)
		//IL_01b2: Unknown result type (might be due to invalid IL or missing references)
		//IL_01b4: Unknown result type (might be due to invalid IL or missing references)
		Util.PlaySound(attackSoundString, base.gameObject);
		aimRay = GetAimRay();
		if (Object.op_Implicit((Object)(object)modelTransform))
		{
			ChildLocator component = ((Component)modelTransform).GetComponent<ChildLocator>();
			if (Object.op_Implicit((Object)(object)component))
			{
				Transform val = component.FindChild(targetMuzzle);
				if (Object.op_Implicit((Object)(object)val))
				{
					aimRay.origin = val.position;
				}
			}
		}
		AddRecoil(-1f * recoilAmplitude, -2f * recoilAmplitude, -1f * recoilAmplitude, 1f * recoilAmplitude);
		if (Object.op_Implicit((Object)(object)effectPrefab))
		{
			EffectManager.SimpleMuzzleFlash(effectPrefab, base.gameObject, targetMuzzle, transmit: false);
		}
		if (base.isAuthority)
		{
			float num = Random.Range(minSpread, maxSpread);
			float num2 = Random.Range(0f, 360f);
			Vector3 up = Vector3.up;
			Vector3 val2 = Vector3.Cross(up, aimRay.direction);
			Vector3 val3 = Quaternion.Euler(0f, 0f, num2) * (Quaternion.Euler(num, 0f, 0f) * Vector3.forward);
			float y = val3.y;
			val3.y = 0f;
			float num3 = Mathf.Atan2(val3.z, val3.x) * 57.29578f - 90f;
			float num4 = Mathf.Atan2(y, ((Vector3)(ref val3)).magnitude) * 57.29578f + arcAngle;
			Vector3 forward = Quaternion.AngleAxis(num3, up) * (Quaternion.AngleAxis(num4, val2) * aimRay.direction);
			ProjectileManager.instance.FireProjectile(projectilePrefab, aimRay.origin, Util.QuaternionSafeLookRotation(forward), base.gameObject, damageStat * damageCoefficient, 0f, Util.CheckRoll(critStat, base.characterBody.master));
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
				FireGrenade("MuzzleLeft");
				PlayCrossfade("Gesture, Left Cannon", "FireGrenadeLeft", 0.1f);
			}
			else
			{
				FireGrenade("MuzzleRight");
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
