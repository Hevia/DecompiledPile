using RoR2;
using RoR2.Projectile;
using UnityEngine;

namespace EntityStates.ClayBoss.ClayBossWeapon;

public class FireBombardment : BaseState
{
	public static GameObject effectPrefab;

	public static GameObject projectilePrefab;

	public int grenadeCountMax = 3;

	public static float damageCoefficient;

	public static float baseTimeBetweenShots = 1f;

	public static float cooldownDuration = 2f;

	public static float arcAngle = 5f;

	public static float recoilAmplitude = 1f;

	public static string shootSoundString;

	public static float spreadBloomValue = 0.3f;

	private Ray aimRay;

	private Transform modelTransform;

	private float duration;

	private float fireTimer;

	private int grenadeCount;

	private float timeBetweenShots;

	private void FireGrenade(string targetMuzzle)
	{
		//IL_0028: Unknown result type (might be due to invalid IL or missing references)
		//IL_002d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0038: Unknown result type (might be due to invalid IL or missing references)
		//IL_003d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0070: Unknown result type (might be due to invalid IL or missing references)
		//IL_0075: Unknown result type (might be due to invalid IL or missing references)
		//IL_00de: Unknown result type (might be due to invalid IL or missing references)
		//IL_00f3: Unknown result type (might be due to invalid IL or missing references)
		//IL_0106: Unknown result type (might be due to invalid IL or missing references)
		//IL_0111: Unknown result type (might be due to invalid IL or missing references)
		//IL_0123: Unknown result type (might be due to invalid IL or missing references)
		//IL_0139: Unknown result type (might be due to invalid IL or missing references)
		//IL_013a: Unknown result type (might be due to invalid IL or missing references)
		//IL_013f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0143: Unknown result type (might be due to invalid IL or missing references)
		//IL_014a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0164: Unknown result type (might be due to invalid IL or missing references)
		//IL_0174: Unknown result type (might be due to invalid IL or missing references)
		//IL_0183: Unknown result type (might be due to invalid IL or missing references)
		//IL_01a3: Unknown result type (might be due to invalid IL or missing references)
		//IL_01a6: Unknown result type (might be due to invalid IL or missing references)
		//IL_01d8: Unknown result type (might be due to invalid IL or missing references)
		//IL_01dd: Unknown result type (might be due to invalid IL or missing references)
		//IL_01df: Unknown result type (might be due to invalid IL or missing references)
		//IL_01e7: Unknown result type (might be due to invalid IL or missing references)
		//IL_01ec: Unknown result type (might be due to invalid IL or missing references)
		//IL_01f1: Unknown result type (might be due to invalid IL or missing references)
		//IL_01ff: Unknown result type (might be due to invalid IL or missing references)
		//IL_0210: Unknown result type (might be due to invalid IL or missing references)
		//IL_0215: Unknown result type (might be due to invalid IL or missing references)
		//IL_021a: Unknown result type (might be due to invalid IL or missing references)
		//IL_021f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0224: Unknown result type (might be due to invalid IL or missing references)
		//IL_0226: Unknown result type (might be due to invalid IL or missing references)
		//IL_023b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0242: Unknown result type (might be due to invalid IL or missing references)
		//IL_0270: Unknown result type (might be due to invalid IL or missing references)
		//IL_0272: Unknown result type (might be due to invalid IL or missing references)
		//IL_0279: Unknown result type (might be due to invalid IL or missing references)
		//IL_027b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0286: Unknown result type (might be due to invalid IL or missing references)
		//IL_028b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0290: Unknown result type (might be due to invalid IL or missing references)
		//IL_0295: Unknown result type (might be due to invalid IL or missing references)
		//IL_02a1: Unknown result type (might be due to invalid IL or missing references)
		//IL_02a2: Unknown result type (might be due to invalid IL or missing references)
		//IL_02a4: Unknown result type (might be due to invalid IL or missing references)
		PlayCrossfade("Gesture, Bombardment", "FireBombardment", 0.1f);
		Util.PlaySound(shootSoundString, base.gameObject);
		aimRay = GetAimRay();
		Vector3 val = ((Ray)(ref aimRay)).origin;
		if (Object.op_Implicit((Object)(object)modelTransform))
		{
			ChildLocator component = ((Component)modelTransform).GetComponent<ChildLocator>();
			if (Object.op_Implicit((Object)(object)component))
			{
				Transform val2 = component.FindChild(targetMuzzle);
				if (Object.op_Implicit((Object)(object)val2))
				{
					val = val2.position;
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
			float num = -1f;
			if (Util.CharacterRaycast(base.gameObject, aimRay, out var hitInfo, float.PositiveInfinity, LayerMask.op_Implicit(LayerMask.op_Implicit(LayerIndex.world.mask) | LayerMask.op_Implicit(LayerIndex.entityPrecise.mask)), (QueryTriggerInteraction)1))
			{
				Vector3 point = ((RaycastHit)(ref hitInfo)).point;
				float velocity = projectilePrefab.GetComponent<ProjectileSimple>().velocity;
				Vector3 val3 = point - val;
				Vector2 val4 = default(Vector2);
				((Vector2)(ref val4))._002Ector(val3.x, val3.z);
				float magnitude = ((Vector2)(ref val4)).magnitude;
				float num2 = Trajectory.CalculateInitialYSpeed(magnitude / velocity, val3.y);
				Vector3 val5 = default(Vector3);
				((Vector3)(ref val5))._002Ector(val4.x / magnitude * velocity, num2, val4.y / magnitude * velocity);
				num = ((Vector3)(ref val5)).magnitude;
				((Ray)(ref aimRay)).direction = val5 / num;
			}
			float num3 = Random.Range(0f, base.characterBody.spreadBloomAngle);
			float num4 = Random.Range(0f, 360f);
			Vector3 up = Vector3.up;
			Vector3 val6 = Vector3.Cross(up, ((Ray)(ref aimRay)).direction);
			Vector3 val7 = Quaternion.Euler(0f, 0f, num4) * (Quaternion.Euler(num3, 0f, 0f) * Vector3.forward);
			float y = val7.y;
			val7.y = 0f;
			float num5 = Mathf.Atan2(val7.z, val7.x) * 57.29578f - 90f;
			float num6 = Mathf.Atan2(y, ((Vector3)(ref val7)).magnitude) * 57.29578f;
			Vector3 forward = Quaternion.AngleAxis(num5, up) * (Quaternion.AngleAxis(num6, val6) * ((Ray)(ref aimRay)).direction);
			ProjectileManager.instance.FireProjectile(projectilePrefab, val, Util.QuaternionSafeLookRotation(forward), base.gameObject, damageStat * damageCoefficient, 0f, Util.CheckRoll(critStat, base.characterBody.master), DamageColorIndex.Default, null, num);
		}
		base.characterBody.AddSpreadBloom(spreadBloomValue);
	}

	public override void OnEnter()
	{
		base.OnEnter();
		timeBetweenShots = baseTimeBetweenShots / attackSpeedStat;
		duration = (baseTimeBetweenShots * (float)grenadeCount + cooldownDuration) / attackSpeedStat;
		PlayCrossfade("Gesture, Additive", "BeginBombardment", 0.1f);
		modelTransform = GetModelTransform();
		if (Object.op_Implicit((Object)(object)base.characterBody))
		{
			base.characterBody.SetAimTimer(duration);
		}
	}

	public override void OnExit()
	{
		PlayCrossfade("Gesture, Additive", "EndBombardment", 0.1f);
		base.OnExit();
	}

	public override void FixedUpdate()
	{
		base.FixedUpdate();
		if (base.isAuthority)
		{
			fireTimer -= Time.fixedDeltaTime;
			if (fireTimer <= 0f && grenadeCount < grenadeCountMax)
			{
				fireTimer += timeBetweenShots;
				FireGrenade("Muzzle");
				grenadeCount++;
			}
			if (base.fixedAge >= duration && base.isAuthority)
			{
				outer.SetNextStateToMain();
			}
		}
	}

	public override InterruptPriority GetMinimumInterruptPriority()
	{
		return InterruptPriority.Skill;
	}
}
