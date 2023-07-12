using RoR2;
using RoR2.Projectile;
using UnityEngine;

namespace EntityStates.NullifierMonster;

public class FirePortalBomb : BaseState
{
	public static GameObject portalBombProjectileEffect;

	public static GameObject muzzleflashEffectPrefab;

	public static string muzzleString;

	public static int portalBombCount;

	public static float baseDuration;

	public static float maxDistance;

	public static float damageCoefficient;

	public static float procCoefficient;

	public static float randomRadius;

	public static float force;

	public static float minimumDistanceBetweenBombs;

	public Quaternion? startRotation;

	public Quaternion? endRotation;

	private float duration;

	private int bombsFired;

	private float fireTimer;

	private float fireInterval;

	private Vector3 lastBombPosition;

	public override InterruptPriority GetMinimumInterruptPriority()
	{
		return InterruptPriority.PrioritySkill;
	}

	public override void OnEnter()
	{
		base.OnEnter();
		duration = baseDuration / attackSpeedStat;
		StartAimMode(4f);
		if (base.isAuthority)
		{
			fireInterval = duration / (float)portalBombCount;
			fireTimer = 0f;
		}
	}

	private void FireBomb(Ray fireRay)
	{
		//IL_0000: Unknown result type (might be due to invalid IL or missing references)
		//IL_0010: Unknown result type (might be due to invalid IL or missing references)
		//IL_0026: Unknown result type (might be due to invalid IL or missing references)
		//IL_002b: Unknown result type (might be due to invalid IL or missing references)
		//IL_002c: Unknown result type (might be due to invalid IL or missing references)
		//IL_002e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0033: Unknown result type (might be due to invalid IL or missing references)
		//IL_0038: Unknown result type (might be due to invalid IL or missing references)
		//IL_0056: Unknown result type (might be due to invalid IL or missing references)
		//IL_0059: Unknown result type (might be due to invalid IL or missing references)
		//IL_0063: Unknown result type (might be due to invalid IL or missing references)
		//IL_0068: Unknown result type (might be due to invalid IL or missing references)
		//IL_006d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0084: Unknown result type (might be due to invalid IL or missing references)
		//IL_0085: Unknown result type (might be due to invalid IL or missing references)
		//IL_008c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0091: Unknown result type (might be due to invalid IL or missing references)
		//IL_00e1: Unknown result type (might be due to invalid IL or missing references)
		//IL_00e2: Unknown result type (might be due to invalid IL or missing references)
		RaycastHit val = default(RaycastHit);
		if (Physics.Raycast(fireRay, ref val, maxDistance, LayerMask.op_Implicit(LayerIndex.world.mask)))
		{
			Vector3 val2 = ((RaycastHit)(ref val)).point;
			Vector3 val3 = val2 - lastBombPosition;
			if (bombsFired > 0 && ((Vector3)(ref val3)).sqrMagnitude < minimumDistanceBetweenBombs * minimumDistanceBetweenBombs)
			{
				val2 += ((Vector3)(ref val3)).normalized * minimumDistanceBetweenBombs;
			}
			FireProjectileInfo fireProjectileInfo = default(FireProjectileInfo);
			fireProjectileInfo.projectilePrefab = portalBombProjectileEffect;
			fireProjectileInfo.position = val2;
			fireProjectileInfo.rotation = Quaternion.identity;
			fireProjectileInfo.owner = base.gameObject;
			fireProjectileInfo.damage = damageStat * damageCoefficient;
			fireProjectileInfo.force = force;
			fireProjectileInfo.crit = base.characterBody.RollCrit();
			ProjectileManager.instance.FireProjectile(fireProjectileInfo);
			lastBombPosition = val2;
		}
	}

	public override void FixedUpdate()
	{
		//IL_007e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0083: Unknown result type (might be due to invalid IL or missing references)
		//IL_008a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0095: Unknown result type (might be due to invalid IL or missing references)
		//IL_009b: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a0: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a3: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a4: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a9: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b4: Unknown result type (might be due to invalid IL or missing references)
		base.FixedUpdate();
		if (!base.isAuthority)
		{
			return;
		}
		fireTimer -= Time.fixedDeltaTime;
		if (fireTimer <= 0f)
		{
			fireTimer += fireInterval;
			if (startRotation.HasValue && endRotation.HasValue)
			{
				float num = 1f / ((float)portalBombCount - 1f);
				float num2 = (float)bombsFired * num;
				Ray aimRay = GetAimRay();
				Quaternion val = Quaternion.Slerp(startRotation.Value, endRotation.Value, num2);
				((Ray)(ref aimRay)).direction = val * Vector3.forward;
				FireBomb(aimRay);
				EffectManager.SimpleMuzzleFlash(muzzleflashEffectPrefab, base.gameObject, muzzleString, transmit: true);
			}
			bombsFired++;
		}
		if (base.fixedAge >= duration)
		{
			outer.SetNextStateToMain();
		}
	}
}
