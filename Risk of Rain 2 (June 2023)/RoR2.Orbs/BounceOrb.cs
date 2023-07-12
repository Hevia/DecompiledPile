using System.Collections.Generic;
using HG;
using JetBrains.Annotations;
using UnityEngine;

namespace RoR2.Orbs;

public class BounceOrb : Orb
{
	private const float speed = 70f;

	public float damageValue;

	public GameObject attacker;

	public TeamIndex teamIndex;

	public List<HealthComponent> bouncedObjects;

	public bool isCrit;

	public float scale;

	public ProcChainMask procChainMask;

	public float procCoefficient = 0.2f;

	public DamageColorIndex damageColorIndex;

	public override void Begin()
	{
		//IL_0025: Unknown result type (might be due to invalid IL or missing references)
		base.duration = base.distanceToTarget / 70f;
		EffectData effectData = new EffectData
		{
			scale = scale,
			origin = origin,
			genericFloat = base.duration
		};
		effectData.SetHurtBoxReference(target);
		EffectManager.SpawnEffect(LegacyResourcesAPI.Load<GameObject>("Prefabs/Effects/OrbEffects/BounceOrbEffect"), effectData, transmit: true);
	}

	public override void OnArrival()
	{
		//IL_0032: Unknown result type (might be due to invalid IL or missing references)
		//IL_0037: Unknown result type (might be due to invalid IL or missing references)
		//IL_0065: Unknown result type (might be due to invalid IL or missing references)
		//IL_0067: Unknown result type (might be due to invalid IL or missing references)
		//IL_006c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0071: Unknown result type (might be due to invalid IL or missing references)
		//IL_0075: Unknown result type (might be due to invalid IL or missing references)
		//IL_007f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0084: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ae: Unknown result type (might be due to invalid IL or missing references)
		//IL_00af: Unknown result type (might be due to invalid IL or missing references)
		if (Object.op_Implicit((Object)(object)target))
		{
			HealthComponent healthComponent = target.healthComponent;
			if (Object.op_Implicit((Object)(object)healthComponent))
			{
				Vector3 position = ((Component)target).transform.position;
				GameObject gameObject = ((Component)healthComponent).gameObject;
				DamageInfo damageInfo = new DamageInfo();
				damageInfo.damage = damageValue;
				damageInfo.attacker = attacker;
				damageInfo.inflictor = null;
				Vector3 val = position - origin;
				damageInfo.force = ((Vector3)(ref val)).normalized * -1000f;
				damageInfo.crit = isCrit;
				damageInfo.procChainMask = procChainMask;
				damageInfo.procCoefficient = procCoefficient;
				damageInfo.position = position;
				damageInfo.damageColorIndex = damageColorIndex;
				healthComponent.TakeDamage(damageInfo);
				GlobalEventManager.instance.OnHitEnemy(damageInfo, gameObject);
				GlobalEventManager.instance.OnHitAll(damageInfo, gameObject);
			}
		}
	}

	public static void SearchForTargets([NotNull] BullseyeSearch search, TeamIndex teamIndex, Vector3 position, float range, int maxTargets, [NotNull] List<HurtBox> dest, [NotNull] List<HealthComponent> exclusions)
	{
		//IL_0001: Unknown result type (might be due to invalid IL or missing references)
		//IL_0002: Unknown result type (might be due to invalid IL or missing references)
		//IL_0008: Unknown result type (might be due to invalid IL or missing references)
		//IL_000d: Unknown result type (might be due to invalid IL or missing references)
		search.searchOrigin = position;
		search.searchDirection = Vector3.zero;
		search.teamMaskFilter = TeamMask.GetEnemyTeams(teamIndex);
		search.filterByLoS = false;
		search.sortMode = BullseyeSearch.SortMode.None;
		search.maxDistanceFilter = range;
		search.RefreshCandidates();
		List<HurtBox> list = CollectionPool<HurtBox, List<HurtBox>>.RentCollection();
		foreach (HurtBox result in search.GetResults())
		{
			list.Add(result);
		}
		Util.ShuffleList(list);
		int i = 0;
		for (int count = list.Count; i < count; i++)
		{
			HurtBox hurtBox = list[i];
			if (Object.op_Implicit((Object)(object)hurtBox.healthComponent) && !exclusions.Contains(hurtBox.healthComponent))
			{
				if (exclusions.Count >= maxTargets)
				{
					break;
				}
				exclusions.Add(hurtBox.healthComponent);
				dest.Add(hurtBox);
			}
		}
		CollectionPool<HurtBox, List<HurtBox>>.ReturnCollection(list);
	}
}
