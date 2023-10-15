using RoR2;
using RoR2.Projectile;
using UnityEngine;

namespace EntityStates.Huntress.HuntressWeapon;

public class FireArrow : BaseState
{
	public static GameObject effectPrefab;

	public static GameObject hitEffectPrefab;

	public static GameObject projectilePrefab;

	public static int arrowCountMax = 1;

	public float damageCoefficient;

	public static float fireDuration = 1f;

	public static float baseDuration = 2f;

	public static float arcAngle = 5f;

	public static float recoilAmplitude = 1f;

	public static string attackSoundString;

	public static float spreadBloomValue = 0.3f;

	public static float smallHopStrength;

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
		//IL_00e0: Unknown result type (might be due to invalid IL or missing references)
		//IL_00e5: Unknown result type (might be due to invalid IL or missing references)
		//IL_00e7: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ef: Unknown result type (might be due to invalid IL or missing references)
		//IL_00f4: Unknown result type (might be due to invalid IL or missing references)
		//IL_00f9: Unknown result type (might be due to invalid IL or missing references)
		//IL_0106: Unknown result type (might be due to invalid IL or missing references)
		//IL_0116: Unknown result type (might be due to invalid IL or missing references)
		//IL_011b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0120: Unknown result type (might be due to invalid IL or missing references)
		//IL_0125: Unknown result type (might be due to invalid IL or missing references)
		//IL_012a: Unknown result type (might be due to invalid IL or missing references)
		//IL_012c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0141: Unknown result type (might be due to invalid IL or missing references)
		//IL_0148: Unknown result type (might be due to invalid IL or missing references)
		//IL_017c: Unknown result type (might be due to invalid IL or missing references)
		//IL_017e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0185: Unknown result type (might be due to invalid IL or missing references)
		//IL_0187: Unknown result type (might be due to invalid IL or missing references)
		//IL_0192: Unknown result type (might be due to invalid IL or missing references)
		//IL_0197: Unknown result type (might be due to invalid IL or missing references)
		//IL_019c: Unknown result type (might be due to invalid IL or missing references)
		//IL_01a1: Unknown result type (might be due to invalid IL or missing references)
		//IL_01b3: Unknown result type (might be due to invalid IL or missing references)
		//IL_01b8: Unknown result type (might be due to invalid IL or missing references)
		//IL_01ba: Unknown result type (might be due to invalid IL or missing references)
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
			float num = Random.Range(0f, base.characterBody.spreadBloomAngle);
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
		base.characterBody.AddSpreadBloom(spreadBloomValue);
	}

	public override void OnEnter()
	{
		base.OnEnter();
		duration = baseDuration / attackSpeedStat;
		modelTransform = GetModelTransform();
		if (Object.op_Implicit((Object)(object)base.characterMotor) && smallHopStrength != 0f)
		{
			base.characterMotor.velocity.y = smallHopStrength;
		}
		if (Object.op_Implicit((Object)(object)base.characterBody))
		{
			base.characterBody.SetAimTimer(2f);
		}
	}

	public override void OnExit()
	{
		base.OnExit();
	}

	public override void FixedUpdate()
	{
		base.FixedUpdate();
		if (base.isAuthority)
		{
			fireTimer -= Time.fixedDeltaTime;
			float num = fireDuration / attackSpeedStat / (float)arrowCountMax;
			if (fireTimer <= 0f && grenadeCount < arrowCountMax)
			{
				PlayAnimation("Gesture, Additive", "FireArrow", "FireArrow.playbackRate", duration - num);
				PlayAnimation("Gesture, Override", "FireArrow", "FireArrow.playbackRate", duration - num);
				FireGrenade("Muzzle");
				fireTimer += num;
				grenadeCount++;
			}
			if (base.fixedAge >= duration)
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
