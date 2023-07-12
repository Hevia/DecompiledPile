using System;
using RoR2;
using RoR2.Projectile;
using UnityEngine;

namespace EntityStates.VagrantMonster.Weapon;

public class JellyStorm : BaseState
{
	private float stopwatch;

	private float missileStopwatch;

	public static float stormDuration;

	public static float stormToIdleTransitionDuration;

	public static string stormPointChildString;

	public static float missileSpawnFrequency;

	public static float missileSpawnDelay;

	public static int missileTurretCount;

	public static float missileTurretYawFrequency;

	public static float missileTurretPitchFrequency;

	public static float missileTurretPitchMagnitude;

	public static float missileSpeed;

	public static float damageCoefficient;

	public static GameObject projectilePrefab;

	public static GameObject effectPrefab;

	private bool beginExitTransition;

	private ChildLocator childLocator;

	public override void OnEnter()
	{
		base.OnEnter();
		missileStopwatch -= missileSpawnDelay;
		if (Object.op_Implicit((Object)(object)base.sfxLocator) && base.sfxLocator.barkSound != "")
		{
			Util.PlaySound(base.sfxLocator.barkSound, base.gameObject);
		}
		PlayAnimation("Gesture", "StormEnter");
		Transform modelTransform = GetModelTransform();
		if (Object.op_Implicit((Object)(object)modelTransform))
		{
			childLocator = ((Component)modelTransform).GetComponent<ChildLocator>();
			if (Object.op_Implicit((Object)(object)childLocator))
			{
				Object.op_Implicit((Object)(object)childLocator.FindChild(stormPointChildString));
			}
		}
	}

	private void FireBlob(Ray aimRay, float bonusPitch, float bonusYaw, float speed)
	{
		//IL_0002: Unknown result type (might be due to invalid IL or missing references)
		//IL_001d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0022: Unknown result type (might be due to invalid IL or missing references)
		//IL_002f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0034: Unknown result type (might be due to invalid IL or missing references)
		//IL_0035: Unknown result type (might be due to invalid IL or missing references)
		Vector3 forward = Util.ApplySpread(((Ray)(ref aimRay)).direction, 0f, 0f, 1f, 1f, bonusYaw, bonusPitch);
		ProjectileManager.instance.FireProjectile(projectilePrefab, ((Ray)(ref aimRay)).origin, Util.QuaternionSafeLookRotation(forward), base.gameObject, damageStat * damageCoefficient, 0f, Util.CheckRoll(critStat, base.characterBody.master), DamageColorIndex.Default, null, speed);
	}

	public override void FixedUpdate()
	{
		//IL_00c6: Unknown result type (might be due to invalid IL or missing references)
		//IL_00cf: Unknown result type (might be due to invalid IL or missing references)
		//IL_00e1: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ec: Unknown result type (might be due to invalid IL or missing references)
		base.FixedUpdate();
		stopwatch += Time.fixedDeltaTime;
		missileStopwatch += Time.fixedDeltaTime;
		if (missileStopwatch >= 1f / missileSpawnFrequency && !beginExitTransition)
		{
			missileStopwatch -= 1f / missileSpawnFrequency;
			Transform val = childLocator.FindChild(stormPointChildString);
			if (Object.op_Implicit((Object)(object)val))
			{
				for (int i = 0; i < missileTurretCount; i++)
				{
					float bonusYaw = 360f / (float)missileTurretCount * (float)i + 360f * missileTurretYawFrequency * stopwatch;
					float bonusPitch = Mathf.Sin(MathF.PI * 2f * missileTurretPitchFrequency * stopwatch) * missileTurretPitchMagnitude;
					Ray aimRay = default(Ray);
					((Ray)(ref aimRay)).origin = val.position;
					((Ray)(ref aimRay)).direction = ((Component)val).transform.forward;
					FireBlob(aimRay, bonusPitch, bonusYaw, missileSpeed);
				}
			}
		}
		if (stopwatch >= stormDuration - stormToIdleTransitionDuration && !beginExitTransition)
		{
			beginExitTransition = true;
			PlayCrossfade("Gesture", "StormExit", "StormExit.playbackRate", stormToIdleTransitionDuration, 0.5f);
		}
		if (stopwatch >= stormDuration && base.isAuthority)
		{
			outer.SetNextStateToMain();
		}
	}
}
