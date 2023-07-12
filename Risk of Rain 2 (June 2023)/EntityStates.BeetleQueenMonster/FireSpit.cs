using RoR2;
using RoR2.Projectile;
using UnityEngine;

namespace EntityStates.BeetleQueenMonster;

public class FireSpit : BaseState
{
	public static GameObject projectilePrefab;

	public static GameObject effectPrefab;

	public static float baseDuration = 2f;

	public static float damageCoefficient = 1.2f;

	public static float force = 20f;

	public static int projectileCount = 3;

	public static float yawSpread = 5f;

	public static float minSpread = 0f;

	public static float maxSpread = 5f;

	public static float arcAngle = 5f;

	public static float projectileHSpeed = 50f;

	private Ray aimRay;

	private float duration;

	public override void OnEnter()
	{
		//IL_005e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0063: Unknown result type (might be due to invalid IL or missing references)
		//IL_006f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0074: Unknown result type (might be due to invalid IL or missing references)
		//IL_0082: Unknown result type (might be due to invalid IL or missing references)
		//IL_0092: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a3: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b6: Unknown result type (might be due to invalid IL or missing references)
		//IL_00c1: Unknown result type (might be due to invalid IL or missing references)
		//IL_00d6: Unknown result type (might be due to invalid IL or missing references)
		//IL_00e1: Unknown result type (might be due to invalid IL or missing references)
		//IL_00e6: Unknown result type (might be due to invalid IL or missing references)
		//IL_00eb: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ef: Unknown result type (might be due to invalid IL or missing references)
		//IL_00f6: Unknown result type (might be due to invalid IL or missing references)
		//IL_0110: Unknown result type (might be due to invalid IL or missing references)
		//IL_0120: Unknown result type (might be due to invalid IL or missing references)
		//IL_012f: Unknown result type (might be due to invalid IL or missing references)
		//IL_014f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0152: Unknown result type (might be due to invalid IL or missing references)
		//IL_017d: Unknown result type (might be due to invalid IL or missing references)
		base.OnEnter();
		string muzzleName = "Mouth";
		duration = baseDuration / attackSpeedStat;
		if (Object.op_Implicit((Object)(object)effectPrefab))
		{
			EffectManager.SimpleMuzzleFlash(effectPrefab, base.gameObject, muzzleName, transmit: false);
		}
		PlayCrossfade("Gesture", "FireSpit", "FireSpit.playbackRate", duration, 0.1f);
		aimRay = GetAimRay();
		float magnitude = projectileHSpeed;
		Ray ray = aimRay;
		((Ray)(ref ray)).origin = ((Ray)(ref aimRay)).GetPoint(6f);
		if (Util.CharacterRaycast(base.gameObject, ray, out var hitInfo, float.PositiveInfinity, LayerMask.op_Implicit(LayerMask.op_Implicit(LayerIndex.world.mask) | LayerMask.op_Implicit(LayerIndex.entityPrecise.mask)), (QueryTriggerInteraction)1))
		{
			float num = magnitude;
			Vector3 val = ((RaycastHit)(ref hitInfo)).point - ((Ray)(ref aimRay)).origin;
			Vector2 val2 = default(Vector2);
			((Vector2)(ref val2))._002Ector(val.x, val.z);
			float magnitude2 = ((Vector2)(ref val2)).magnitude;
			float num2 = Trajectory.CalculateInitialYSpeed(magnitude2 / num, val.y);
			Vector3 val3 = default(Vector3);
			((Vector3)(ref val3))._002Ector(val2.x / magnitude2 * num, num2, val2.y / magnitude2 * num);
			magnitude = ((Vector3)(ref val3)).magnitude;
			((Ray)(ref aimRay)).direction = val3 / magnitude;
		}
		EffectManager.SimpleMuzzleFlash(effectPrefab, base.gameObject, muzzleName, transmit: false);
		if (base.isAuthority)
		{
			for (int i = 0; i < projectileCount; i++)
			{
				FireBlob(aimRay, 0f, ((float)projectileCount / 2f - (float)i) * yawSpread, magnitude);
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
		Vector3 forward = Util.ApplySpread(((Ray)(ref aimRay)).direction, minSpread, maxSpread, 1f, 1f, bonusYaw, bonusPitch);
		ProjectileManager.instance.FireProjectile(projectilePrefab, ((Ray)(ref aimRay)).origin, Util.QuaternionSafeLookRotation(forward), base.gameObject, damageStat * damageCoefficient, 0f, Util.CheckRoll(critStat, base.characterBody.master), DamageColorIndex.Default, null, speed);
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
