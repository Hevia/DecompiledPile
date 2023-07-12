using RoR2;
using RoR2.Projectile;
using UnityEngine;
using UnityEngine.Networking;

namespace EntityStates.ArtifactShell;

public class FireSolarFlares : BaseState
{
	public static GameObject projectilePrefab;

	public static int minimumProjectileCount;

	public static int maximumProjectileCount;

	public static float arc;

	public static float baseDuration;

	public static float radius;

	public static float projectileDamageCoefficient;

	public static float projectileForce;

	public static float projectileFuse;

	public static float projectileSpeed;

	private float duration;

	private int projectileCount;

	private int projectilesFired;

	private Quaternion currentRotation;

	private Quaternion deltaRotation;

	public override void OnEnter()
	{
		//IL_0050: Unknown result type (might be due to invalid IL or missing references)
		//IL_0055: Unknown result type (might be due to invalid IL or missing references)
		//IL_0057: Unknown result type (might be due to invalid IL or missing references)
		//IL_0058: Unknown result type (might be due to invalid IL or missing references)
		//IL_005d: Unknown result type (might be due to invalid IL or missing references)
		//IL_007a: Unknown result type (might be due to invalid IL or missing references)
		//IL_007f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0084: Unknown result type (might be due to invalid IL or missing references)
		//IL_0089: Unknown result type (might be due to invalid IL or missing references)
		//IL_008b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0090: Unknown result type (might be due to invalid IL or missing references)
		//IL_0095: Unknown result type (might be due to invalid IL or missing references)
		//IL_009a: Unknown result type (might be due to invalid IL or missing references)
		//IL_009c: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a1: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a6: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ba: Unknown result type (might be due to invalid IL or missing references)
		//IL_00bb: Unknown result type (might be due to invalid IL or missing references)
		//IL_00bc: Unknown result type (might be due to invalid IL or missing references)
		//IL_00c1: Unknown result type (might be due to invalid IL or missing references)
		//IL_00c6: Unknown result type (might be due to invalid IL or missing references)
		base.OnEnter();
		if (Object.op_Implicit((Object)(object)base.healthComponent))
		{
			projectileCount = (int)Util.Remap(base.healthComponent.combinedHealthFraction, 0f, 1f, maximumProjectileCount, minimumProjectileCount);
		}
		if (NetworkServer.active)
		{
			Vector3 aimDirection = base.inputBank.aimDirection;
			currentRotation = Quaternion.LookRotation(aimDirection);
			_ = Run.FixedTimeStamp.now.t * 2f % 1f;
			Vector3 val = currentRotation * Vector3.forward;
			Vector3 val2 = currentRotation * Vector3.right;
			_ = currentRotation * Vector3.up;
			deltaRotation = Quaternion.AngleAxis(arc / (float)projectileCount, Vector3.Cross(val2, val));
		}
		duration = baseDuration / attackSpeedStat;
	}

	public override void FixedUpdate()
	{
		//IL_005e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0064: Unknown result type (might be due to invalid IL or missing references)
		//IL_0069: Unknown result type (might be due to invalid IL or missing references)
		//IL_006e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0078: Unknown result type (might be due to invalid IL or missing references)
		//IL_007d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0082: Unknown result type (might be due to invalid IL or missing references)
		//IL_008a: Unknown result type (might be due to invalid IL or missing references)
		//IL_008f: Unknown result type (might be due to invalid IL or missing references)
		//IL_00f4: Unknown result type (might be due to invalid IL or missing references)
		//IL_00fa: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ff: Unknown result type (might be due to invalid IL or missing references)
		//IL_0104: Unknown result type (might be due to invalid IL or missing references)
		base.FixedUpdate();
		if (NetworkServer.active)
		{
			float num = duration / (float)projectileCount;
			if (base.fixedAge >= (float)projectilesFired * num)
			{
				projectilesFired++;
				FireProjectileInfo fireProjectileInfo = default(FireProjectileInfo);
				fireProjectileInfo.owner = base.gameObject;
				fireProjectileInfo.position = base.transform.position + currentRotation * Vector3.forward * radius;
				fireProjectileInfo.rotation = currentRotation;
				fireProjectileInfo.projectilePrefab = projectilePrefab;
				fireProjectileInfo.fuseOverride = projectileFuse;
				fireProjectileInfo.useFuseOverride = true;
				fireProjectileInfo.speedOverride = projectileSpeed;
				fireProjectileInfo.useSpeedOverride = true;
				fireProjectileInfo.damage = damageStat * projectileDamageCoefficient;
				fireProjectileInfo.force = projectileForce;
				ProjectileManager.instance.FireProjectile(fireProjectileInfo);
				currentRotation *= deltaRotation;
			}
			if (base.fixedAge >= duration)
			{
				outer.SetNextStateToMain();
			}
		}
	}
}
