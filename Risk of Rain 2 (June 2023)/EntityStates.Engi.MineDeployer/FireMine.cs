using System.Collections.Generic;
using RoR2;
using RoR2.Projectile;
using UnityEngine;

namespace EntityStates.Engi.MineDeployer;

public class FireMine : BaseMineDeployerState
{
	public static float duration;

	public static float launchApex;

	public static float patternRadius;

	public static GameObject projectilePrefab;

	private int fireIndex;

	private static Vector3[] velocities;

	private static bool velocitiesResolved;

	public override void OnEnter()
	{
		//IL_003a: Unknown result type (might be due to invalid IL or missing references)
		//IL_003f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0044: Unknown result type (might be due to invalid IL or missing references)
		//IL_0091: Unknown result type (might be due to invalid IL or missing references)
		//IL_0096: Unknown result type (might be due to invalid IL or missing references)
		//IL_00bb: Unknown result type (might be due to invalid IL or missing references)
		//IL_00bc: Unknown result type (might be due to invalid IL or missing references)
		//IL_00c1: Unknown result type (might be due to invalid IL or missing references)
		base.OnEnter();
		if (base.isAuthority)
		{
			ResolveVelocities();
			Transform val = base.transform.Find("FirePoint");
			ProjectileDamage component = GetComponent<ProjectileDamage>();
			Vector3 val2 = val.TransformVector(velocities[fireIndex]);
			FireProjectileInfo fireProjectileInfo = default(FireProjectileInfo);
			fireProjectileInfo.crit = component.crit;
			fireProjectileInfo.damage = component.damage;
			fireProjectileInfo.damageColorIndex = component.damageColorIndex;
			fireProjectileInfo.force = component.force;
			fireProjectileInfo.owner = base.owner;
			fireProjectileInfo.position = val.position;
			fireProjectileInfo.procChainMask = base.projectileController.procChainMask;
			fireProjectileInfo.projectilePrefab = projectilePrefab;
			fireProjectileInfo.rotation = Quaternion.LookRotation(val2);
			fireProjectileInfo.fuseOverride = -1f;
			fireProjectileInfo.useFuseOverride = false;
			fireProjectileInfo.speedOverride = ((Vector3)(ref val2)).magnitude;
			fireProjectileInfo.useSpeedOverride = true;
			FireProjectileInfo fireProjectileInfo2 = fireProjectileInfo;
			ProjectileManager.instance.FireProjectile(fireProjectileInfo2);
		}
	}

	public override void FixedUpdate()
	{
		base.FixedUpdate();
		if (base.isAuthority && duration <= base.fixedAge)
		{
			int num = fireIndex + 1;
			if (num < velocities.Length)
			{
				outer.SetNextState(new FireMine
				{
					fireIndex = num
				});
			}
			else
			{
				outer.SetNextState(new WaitForDeath());
			}
		}
	}

	private static Vector3[] GeneratePoints(float radius)
	{
		//IL_000d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0012: Unknown result type (might be due to invalid IL or missing references)
		//IL_0017: Unknown result type (might be due to invalid IL or missing references)
		//IL_001d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0022: Unknown result type (might be due to invalid IL or missing references)
		//IL_0027: Unknown result type (might be due to invalid IL or missing references)
		//IL_0028: Unknown result type (might be due to invalid IL or missing references)
		//IL_002d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0030: Unknown result type (might be due to invalid IL or missing references)
		//IL_0031: Unknown result type (might be due to invalid IL or missing references)
		//IL_0038: Unknown result type (might be due to invalid IL or missing references)
		//IL_003b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0040: Unknown result type (might be due to invalid IL or missing references)
		//IL_0045: Unknown result type (might be due to invalid IL or missing references)
		//IL_004c: Unknown result type (might be due to invalid IL or missing references)
		//IL_004f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0054: Unknown result type (might be due to invalid IL or missing references)
		//IL_0059: Unknown result type (might be due to invalid IL or missing references)
		//IL_0067: Unknown result type (might be due to invalid IL or missing references)
		//IL_006e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0091: Unknown result type (might be due to invalid IL or missing references)
		//IL_0094: Unknown result type (might be due to invalid IL or missing references)
		//IL_009b: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a0: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a5: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ac: Unknown result type (might be due to invalid IL or missing references)
		//IL_00af: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b4: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b9: Unknown result type (might be due to invalid IL or missing references)
		//IL_00c0: Unknown result type (might be due to invalid IL or missing references)
		//IL_00c3: Unknown result type (might be due to invalid IL or missing references)
		//IL_00c8: Unknown result type (might be due to invalid IL or missing references)
		//IL_00cd: Unknown result type (might be due to invalid IL or missing references)
		//IL_00db: Unknown result type (might be due to invalid IL or missing references)
		//IL_00de: Unknown result type (might be due to invalid IL or missing references)
		//IL_00e5: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ea: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ef: Unknown result type (might be due to invalid IL or missing references)
		//IL_00f6: Unknown result type (might be due to invalid IL or missing references)
		//IL_00f9: Unknown result type (might be due to invalid IL or missing references)
		//IL_00fe: Unknown result type (might be due to invalid IL or missing references)
		//IL_0103: Unknown result type (might be due to invalid IL or missing references)
		//IL_010a: Unknown result type (might be due to invalid IL or missing references)
		//IL_010d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0112: Unknown result type (might be due to invalid IL or missing references)
		//IL_0117: Unknown result type (might be due to invalid IL or missing references)
		//IL_013a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0141: Unknown result type (might be due to invalid IL or missing references)
		//IL_0146: Unknown result type (might be due to invalid IL or missing references)
		Vector3[] array = (Vector3[])(object)new Vector3[9];
		Quaternion val = Quaternion.AngleAxis(60f, Vector3.up);
		Quaternion val2 = Quaternion.AngleAxis(120f, Vector3.up);
		Vector3 forward = Vector3.forward;
		array[0] = forward;
		array[1] = val2 * array[0];
		array[2] = val2 * array[1];
		float num = 1f;
		float num2 = Vector3.Distance(array[0], array[1]);
		float num3 = Mathf.Sqrt(num * num + num2 * num2) / num;
		array[3] = val * (array[2] * num3);
		array[4] = val2 * array[3];
		array[5] = val2 * array[4];
		num3 = 1f;
		array[6] = val * (array[5] * num3);
		array[7] = val2 * array[6];
		array[8] = val2 * array[7];
		float num4 = radius / ((Vector3)(ref array[8])).magnitude;
		for (int i = 0; i < array.Length; i++)
		{
			ref Vector3 reference = ref array[i];
			reference *= num4;
		}
		return array;
	}

	private static Vector3[] GenerateHexPoints(float radius)
	{
		//IL_000c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0011: Unknown result type (might be due to invalid IL or missing references)
		//IL_0016: Unknown result type (might be due to invalid IL or missing references)
		//IL_0020: Unknown result type (might be due to invalid IL or missing references)
		//IL_0026: Unknown result type (might be due to invalid IL or missing references)
		//IL_002b: Unknown result type (might be due to invalid IL or missing references)
		//IL_003c: Unknown result type (might be due to invalid IL or missing references)
		//IL_003e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0043: Unknown result type (might be due to invalid IL or missing references)
		//IL_0048: Unknown result type (might be due to invalid IL or missing references)
		Vector3[] array = (Vector3[])(object)new Vector3[6];
		Quaternion val = Quaternion.AngleAxis(60f, Vector3.up);
		ref Vector3 reference = ref array[0];
		reference = Vector3.forward * radius;
		for (int i = 1; i < array.Length; i++)
		{
			ref Vector3 reference2 = ref array[i];
			reference2 = val * reference;
			reference = ref reference2;
		}
		return array;
	}

	private static Vector3[] GeneratePointsFromPattern(GameObject patternObject)
	{
		//IL_0008: Unknown result type (might be due to invalid IL or missing references)
		//IL_000d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0032: Unknown result type (might be due to invalid IL or missing references)
		//IL_0037: Unknown result type (might be due to invalid IL or missing references)
		//IL_0038: Unknown result type (might be due to invalid IL or missing references)
		Transform val = patternObject.transform;
		Vector3 position = val.position;
		List<Vector3> list = new List<Vector3>();
		for (int i = 0; i < val.childCount; i++)
		{
			Transform child = val.GetChild(i);
			if (((Component)child).gameObject.activeInHierarchy)
			{
				list.Add(child.position - position);
			}
		}
		return list.ToArray();
	}

	private static Vector3[] GenerateVelocitiesFromPoints(Vector3[] points, float apex)
	{
		//IL_001b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0034: Unknown result type (might be due to invalid IL or missing references)
		//IL_0039: Unknown result type (might be due to invalid IL or missing references)
		//IL_0045: Unknown result type (might be due to invalid IL or missing references)
		//IL_0047: Unknown result type (might be due to invalid IL or missing references)
		Vector3[] array = (Vector3[])(object)new Vector3[points.Length];
		float num = Trajectory.CalculateInitialYSpeedForHeight(apex);
		for (int i = 0; i < points.Length; i++)
		{
			Vector3 normalized = ((Vector3)(ref points[i])).normalized;
			float num2 = Trajectory.CalculateGroundSpeedToClearDistance(num, ((Vector3)(ref points[i])).magnitude);
			Vector3 val = normalized * num2;
			val.y = num;
			array[i] = val;
		}
		return array;
	}

	private static void ResolveVelocities()
	{
		if (!velocitiesResolved)
		{
			velocities = GenerateVelocitiesFromPoints(GeneratePoints(patternRadius), launchApex);
			if (!Application.isEditor)
			{
				velocitiesResolved = true;
			}
		}
	}
}
