using RoR2;
using RoR2.Projectile;
using UnityEngine;

namespace EntityStates.ScavMonster;

public class ThrowSack : SackBaseState
{
	public static float baseDuration;

	public static string sound;

	public static GameObject effectPrefab;

	public static GameObject projectilePrefab;

	public static float damageCoefficient;

	public static float force;

	public static float minSpread;

	public static float maxSpread;

	public static string attackSoundString;

	public static float projectileVelocity;

	public static float minimumDistance;

	public static float timeToTarget = 3f;

	public static int projectileCount;

	private float duration;

	public override void OnEnter()
	{
		base.OnEnter();
		duration = baseDuration / attackSpeedStat;
		Util.PlayAttackSpeedSound(sound, base.gameObject, attackSpeedStat);
		PlayAnimation("Body", "ThrowSack", "ThrowSack.playbackRate", duration);
		if (Object.op_Implicit((Object)(object)effectPrefab))
		{
			EffectManager.SimpleMuzzleFlash(effectPrefab, base.gameObject, SackBaseState.muzzleName, transmit: false);
		}
		Fire();
	}

	private void Fire()
	{
		//IL_0001: Unknown result type (might be due to invalid IL or missing references)
		//IL_0006: Unknown result type (might be due to invalid IL or missing references)
		//IL_0007: Unknown result type (might be due to invalid IL or missing references)
		//IL_0008: Unknown result type (might be due to invalid IL or missing references)
		//IL_0009: Unknown result type (might be due to invalid IL or missing references)
		//IL_000a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0012: Unknown result type (might be due to invalid IL or missing references)
		//IL_0017: Unknown result type (might be due to invalid IL or missing references)
		//IL_0021: Unknown result type (might be due to invalid IL or missing references)
		//IL_0032: Unknown result type (might be due to invalid IL or missing references)
		//IL_0045: Unknown result type (might be due to invalid IL or missing references)
		//IL_0050: Unknown result type (might be due to invalid IL or missing references)
		//IL_005f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0064: Unknown result type (might be due to invalid IL or missing references)
		//IL_0076: Unknown result type (might be due to invalid IL or missing references)
		//IL_0079: Unknown result type (might be due to invalid IL or missing references)
		//IL_007e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0083: Unknown result type (might be due to invalid IL or missing references)
		//IL_0087: Unknown result type (might be due to invalid IL or missing references)
		//IL_008e: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a3: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a7: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ac: Unknown result type (might be due to invalid IL or missing references)
		//IL_00c3: Unknown result type (might be due to invalid IL or missing references)
		//IL_00dd: Unknown result type (might be due to invalid IL or missing references)
		//IL_00e9: Unknown result type (might be due to invalid IL or missing references)
		//IL_0103: Unknown result type (might be due to invalid IL or missing references)
		//IL_0114: Unknown result type (might be due to invalid IL or missing references)
		//IL_0137: Unknown result type (might be due to invalid IL or missing references)
		//IL_013c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0141: Unknown result type (might be due to invalid IL or missing references)
		//IL_014f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0154: Unknown result type (might be due to invalid IL or missing references)
		Ray aimRay = GetAimRay();
		Ray ray = aimRay;
		Ray val = aimRay;
		Vector3 point = aimRay.GetPoint(minimumDistance);
		bool flag = false;
		if (Util.CharacterRaycast(base.gameObject, ray, out var hitInfo, 500f, LayerMask.op_Implicit(LayerMask.op_Implicit(LayerIndex.world.mask) | LayerMask.op_Implicit(LayerIndex.entityPrecise.mask)), (QueryTriggerInteraction)1))
		{
			point = ((RaycastHit)(ref hitInfo)).point;
			flag = true;
		}
		float magnitude = projectileVelocity;
		if (flag)
		{
			Vector3 val2 = point - ((Ray)(ref val)).origin;
			Vector2 val3 = default(Vector2);
			((Vector2)(ref val3))._002Ector(val2.x, val2.z);
			float magnitude2 = ((Vector2)(ref val3)).magnitude;
			Vector2 val4 = val3 / magnitude2;
			if (magnitude2 < minimumDistance)
			{
				magnitude2 = minimumDistance;
			}
			float num = Trajectory.CalculateInitialYSpeed(timeToTarget, val2.y);
			float num2 = magnitude2 / timeToTarget;
			Vector3 direction = default(Vector3);
			((Vector3)(ref direction))._002Ector(val4.x * num2, num, val4.y * num2);
			magnitude = ((Vector3)(ref direction)).magnitude;
			((Ray)(ref val)).direction = direction;
		}
		for (int i = 0; i < projectileCount; i++)
		{
			Quaternion rotation = Util.QuaternionSafeLookRotation(Util.ApplySpread(((Ray)(ref val)).direction, minSpread, maxSpread, 1f, 1f));
			ProjectileManager.instance.FireProjectile(projectilePrefab, ((Ray)(ref val)).origin, rotation, base.gameObject, damageStat * damageCoefficient, 0f, Util.CheckRoll(critStat, base.characterBody.master), DamageColorIndex.Default, null, magnitude);
		}
	}

	public override void FixedUpdate()
	{
		base.FixedUpdate();
		if (base.fixedAge >= duration && base.isAuthority)
		{
			outer.SetNextStateToMain();
		}
	}
}
