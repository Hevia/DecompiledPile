using RoR2;
using RoR2.Projectile;
using UnityEngine;

namespace EntityStates.ClayBoss;

public class FireTarball : BaseState
{
	public static GameObject effectPrefab;

	public static GameObject projectilePrefab;

	public static int tarballCountMax = 3;

	public static float damageCoefficient;

	public static float baseTimeBetweenShots = 1f;

	public static float cooldownDuration = 2f;

	public static float recoilAmplitude = 1f;

	public static string attackSoundString;

	public static float spreadBloomValue = 0.3f;

	private int tarballCount;

	private Ray aimRay;

	private Transform modelTransform;

	private float duration;

	private float fireTimer;

	private float timeBetweenShots;

	private void FireSingleTarball(string targetMuzzle)
	{
		//IL_0028: Unknown result type (might be due to invalid IL or missing references)
		//IL_002d: Unknown result type (might be due to invalid IL or missing references)
		//IL_006a: Unknown result type (might be due to invalid IL or missing references)
		//IL_00d2: Unknown result type (might be due to invalid IL or missing references)
		//IL_00d7: Unknown result type (might be due to invalid IL or missing references)
		//IL_00dc: Unknown result type (might be due to invalid IL or missing references)
		//IL_00e1: Unknown result type (might be due to invalid IL or missing references)
		//IL_00f2: Unknown result type (might be due to invalid IL or missing references)
		//IL_00f7: Unknown result type (might be due to invalid IL or missing references)
		//IL_00f8: Unknown result type (might be due to invalid IL or missing references)
		PlayCrossfade("Body", "FireTarBall", 0.1f);
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
			Vector3 forward = Vector3.ProjectOnPlane(aimRay.direction, Vector3.up);
			ProjectileManager.instance.FireProjectile(projectilePrefab, aimRay.origin, Util.QuaternionSafeLookRotation(forward), base.gameObject, damageStat * damageCoefficient, 0f, Util.CheckRoll(critStat, base.characterBody.master));
		}
		base.characterBody.AddSpreadBloom(spreadBloomValue);
	}

	public override void OnEnter()
	{
		base.OnEnter();
		timeBetweenShots = baseTimeBetweenShots / attackSpeedStat;
		duration = (baseTimeBetweenShots * (float)tarballCountMax + cooldownDuration) / attackSpeedStat;
		modelTransform = GetModelTransform();
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
		fireTimer -= Time.fixedDeltaTime;
		if (fireTimer <= 0f)
		{
			if (tarballCount < tarballCountMax)
			{
				fireTimer += timeBetweenShots;
				FireSingleTarball("BottomMuzzle");
				tarballCount++;
			}
			else
			{
				fireTimer += 9999f;
				PlayCrossfade("Body", "ExitTarBall", "ExitTarBall.playbackRate", (cooldownDuration - baseTimeBetweenShots) / attackSpeedStat, 0.1f);
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
