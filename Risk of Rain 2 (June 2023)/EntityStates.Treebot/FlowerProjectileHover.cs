using System.Collections.Generic;
using EntityStates.Toolbot;
using RoR2;
using RoR2.Projectile;
using UnityEngine;

namespace EntityStates.Treebot;

public class FlowerProjectileHover : DroneProjectileHover
{
	public static float yankSpeed;

	public static AnimationCurve yankSuitabilityCurve;

	public static float healthFractionYieldPerHit;

	private GameObject owner;

	private ProcChainMask procChainMask;

	private float procCoefficient;

	private float damage;

	private DamageType damageType;

	private bool crit;

	private TeamIndex teamIndex = TeamIndex.None;

	private float healPulseHealthFractionValue;

	public override void OnEnter()
	{
		base.OnEnter();
		ProjectileController component = GetComponent<ProjectileController>();
		if (Object.op_Implicit((Object)(object)component))
		{
			owner = component.owner;
			procChainMask = component.procChainMask;
			procCoefficient = component.procCoefficient;
			teamIndex = component.teamFilter.teamIndex;
		}
		ProjectileDamage component2 = GetComponent<ProjectileDamage>();
		if (Object.op_Implicit((Object)(object)component2))
		{
			damage = component2.damage;
			damageType = component2.damageType;
			crit = component2.crit;
		}
	}

	private void FirstPulse()
	{
		//IL_0006: Unknown result type (might be due to invalid IL or missing references)
		//IL_000b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0012: Unknown result type (might be due to invalid IL or missing references)
		//IL_0013: Unknown result type (might be due to invalid IL or missing references)
		//IL_0073: Unknown result type (might be due to invalid IL or missing references)
		//IL_0078: Unknown result type (might be due to invalid IL or missing references)
		//IL_0079: Unknown result type (might be due to invalid IL or missing references)
		//IL_007e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0089: Unknown result type (might be due to invalid IL or missing references)
		//IL_008d: Unknown result type (might be due to invalid IL or missing references)
		//IL_00d2: Unknown result type (might be due to invalid IL or missing references)
		//IL_00d7: Unknown result type (might be due to invalid IL or missing references)
		//IL_0122: Unknown result type (might be due to invalid IL or missing references)
		//IL_0124: Unknown result type (might be due to invalid IL or missing references)
		//IL_0130: Unknown result type (might be due to invalid IL or missing references)
		//IL_0135: Unknown result type (might be due to invalid IL or missing references)
		Vector3 position = base.transform.position;
		BullseyeSearch bullseyeSearch = new BullseyeSearch();
		bullseyeSearch.searchOrigin = position;
		bullseyeSearch.teamMaskFilter = TeamMask.GetEnemyTeams(teamIndex);
		bullseyeSearch.maxDistanceFilter = pulseRadius;
		bullseyeSearch.sortMode = BullseyeSearch.SortMode.Distance;
		bullseyeSearch.filterByLoS = true;
		bullseyeSearch.filterByDistinctEntity = true;
		bullseyeSearch.RefreshCandidates();
		IEnumerable<HurtBox> results = bullseyeSearch.GetResults();
		int num = 0;
		foreach (HurtBox item in results)
		{
			num++;
			Vector3 val = ((Component)item).transform.position - position;
			float magnitude = ((Vector3)(ref val)).magnitude;
			Vector3 val2 = val / magnitude;
			Rigidbody component = ((Component)item.healthComponent).GetComponent<Rigidbody>();
			float num2 = (Object.op_Implicit((Object)(object)component) ? component.mass : 1f);
			float num3 = yankSuitabilityCurve.Evaluate(num2);
			Vector3 force = val2 * (num2 * num3 * (0f - yankSpeed));
			DamageInfo damageInfo = new DamageInfo
			{
				attacker = owner,
				inflictor = base.gameObject,
				crit = crit,
				damage = damage,
				damageColorIndex = DamageColorIndex.Default,
				damageType = damageType,
				force = force,
				position = ((Component)item).transform.position,
				procChainMask = procChainMask,
				procCoefficient = procCoefficient
			};
			item.healthComponent.TakeDamage(damageInfo);
		}
		healPulseHealthFractionValue = (float)num * healthFractionYieldPerHit / (float)(pulseCount - 1);
	}

	private void HealPulse()
	{
		if (Object.op_Implicit((Object)(object)owner))
		{
			HealthComponent component = owner.GetComponent<HealthComponent>();
			if (Object.op_Implicit((Object)(object)component))
			{
				component.HealFraction(healPulseHealthFractionValue, procChainMask);
			}
		}
	}

	protected override void Pulse()
	{
		if (pulses == 1)
		{
			FirstPulse();
		}
		else
		{
			HealPulse();
		}
	}
}
