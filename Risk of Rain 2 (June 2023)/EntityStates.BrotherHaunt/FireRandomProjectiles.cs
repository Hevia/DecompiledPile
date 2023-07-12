using System.Collections.Generic;
using RoR2;
using RoR2.Navigation;
using RoR2.Projectile;
using UnityEngine;

namespace EntityStates.BrotherHaunt;

public class FireRandomProjectiles : BaseState
{
	public static GameObject projectilePrefab;

	public static float damageCoefficient;

	public static int initialCharges;

	public static int maximumCharges;

	public static float chargeRechargeDuration;

	public static float chanceToFirePerSecond;

	public static float projectileVerticalOffset;

	private int charges;

	private float chargeTimer;

	public override void OnEnter()
	{
		base.OnEnter();
		charges = initialCharges;
	}

	public override void FixedUpdate()
	{
		base.FixedUpdate();
		if (base.isAuthority)
		{
			chargeTimer -= Time.fixedDeltaTime;
			if (chargeTimer <= 0f)
			{
				chargeTimer = chargeRechargeDuration;
				charges = Mathf.Min(charges + 1, maximumCharges);
			}
			if (Random.value < chanceToFirePerSecond && charges > 0)
			{
				FireProjectile();
			}
		}
	}

	private void FireProjectile()
	{
		//IL_0086: Unknown result type (might be due to invalid IL or missing references)
		//IL_0087: Unknown result type (might be due to invalid IL or missing references)
		//IL_0091: Unknown result type (might be due to invalid IL or missing references)
		//IL_0096: Unknown result type (might be due to invalid IL or missing references)
		//IL_009b: Unknown result type (might be due to invalid IL or missing references)
		//IL_00bb: Unknown result type (might be due to invalid IL or missing references)
		//IL_00c0: Unknown result type (might be due to invalid IL or missing references)
		NodeGraph groundNodes = SceneInfo.instance.groundNodes;
		if (Object.op_Implicit((Object)(object)groundNodes))
		{
			List<NodeGraph.NodeIndex> activeNodesForHullMaskWithFlagConditions = groundNodes.GetActiveNodesForHullMaskWithFlagConditions(HullMask.Golem, NodeFlags.None, NodeFlags.NoCharacterSpawn);
			NodeGraph.NodeIndex nodeIndex = activeNodesForHullMaskWithFlagConditions[Random.Range(0, activeNodesForHullMaskWithFlagConditions.Count)];
			charges--;
			groundNodes.GetNodePosition(nodeIndex, out var position);
			ProjectileManager.instance.FireProjectile(new FireProjectileInfo
			{
				projectilePrefab = projectilePrefab,
				owner = base.gameObject,
				damage = damageStat * damageCoefficient,
				position = position + Vector3.up * projectileVerticalOffset,
				rotation = Quaternion.Euler(0f, Random.Range(0f, 360f), 0f)
			});
		}
	}

	public override InterruptPriority GetMinimumInterruptPriority()
	{
		return InterruptPriority.Death;
	}
}
