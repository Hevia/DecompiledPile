using System.Linq;
using RoR2;
using RoR2.Projectile;
using UnityEngine;

namespace EntityStates.Toolbot;

public class DroneProjectileHoverHeal : DroneProjectileHover
{
	public static float healPointsCoefficient;

	public static float healFraction;

	protected override void Pulse()
	{
		//IL_0041: Unknown result type (might be due to invalid IL or missing references)
		float num = 1f;
		ProjectileDamage component = GetComponent<ProjectileDamage>();
		if (Object.op_Implicit((Object)(object)component))
		{
			num = component.damage;
		}
		HealOccupants(pulseRadius, healPointsCoefficient * num, healFraction);
		EffectData effectData = new EffectData();
		effectData.origin = base.transform.position;
		effectData.scale = pulseRadius;
		EffectManager.SpawnEffect(LegacyResourcesAPI.Load<GameObject>("Prefabs/Effects/ImpactEffects/ExplosionVFX"), effectData, transmit: true);
	}

	private static HealthComponent SelectHealthComponent(Collider collider)
	{
		HurtBox component = ((Component)collider).GetComponent<HurtBox>();
		if (Object.op_Implicit((Object)(object)component) && Object.op_Implicit((Object)(object)component.healthComponent))
		{
			return component.healthComponent;
		}
		return null;
	}

	private void HealOccupants(float radius, float healPoints, float healFraction)
	{
		//IL_000c: Unknown result type (might be due to invalid IL or missing references)
		//IL_001a: Unknown result type (might be due to invalid IL or missing references)
		Collider[] source = Physics.OverlapSphere(base.transform.position, radius, LayerMask.op_Implicit(LayerIndex.entityPrecise.mask));
		TeamIndex teamIndex = (Object.op_Implicit((Object)(object)teamFilter) ? teamFilter.teamIndex : TeamIndex.None);
		foreach (HealthComponent item in (from healthComponent in source.Select(SelectHealthComponent)
			where Object.op_Implicit((Object)(object)healthComponent) && healthComponent.body.teamComponent.teamIndex == teamIndex
			select healthComponent).Distinct())
		{
			float num = healPoints + item.fullHealth * healFraction;
			if (num > 0f)
			{
				item.Heal(num, default(ProcChainMask));
			}
		}
	}
}
