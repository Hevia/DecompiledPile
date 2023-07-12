using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace RoR2.Orbs;

public class ChainGunOrb : GenericDamageOrb
{
	private GameObject orbEffectPrefab;

	public int bouncesRemaining;

	public float bounceRange = 20f;

	public float damageCoefficientPerBounce = 1f;

	public int targetsToFindPerBounce = 1;

	public bool canBounceOnSameTarget;

	private List<HealthComponent> bouncedObjects;

	private BullseyeSearch search;

	public ChainGunOrb(GameObject orbEffectObject)
	{
		orbEffectPrefab = orbEffectObject;
		bouncedObjects = new List<HealthComponent>();
	}

	protected override GameObject GetOrbEffect()
	{
		return orbEffectPrefab;
	}

	public override void OnArrival()
	{
		//IL_0046: Unknown result type (might be due to invalid IL or missing references)
		//IL_004b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0080: Unknown result type (might be due to invalid IL or missing references)
		//IL_0085: Unknown result type (might be due to invalid IL or missing references)
		//IL_011b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0158: Unknown result type (might be due to invalid IL or missing references)
		//IL_015d: Unknown result type (might be due to invalid IL or missing references)
		if (!Object.op_Implicit((Object)(object)target))
		{
			return;
		}
		HealthComponent healthComponent = target.healthComponent;
		if (Object.op_Implicit((Object)(object)healthComponent))
		{
			DamageInfo damageInfo = new DamageInfo();
			damageInfo.damage = damageValue;
			damageInfo.attacker = attacker;
			damageInfo.force = Vector3.zero;
			damageInfo.crit = isCrit;
			damageInfo.procChainMask = procChainMask;
			damageInfo.procCoefficient = procCoefficient;
			damageInfo.position = ((Component)target).transform.position;
			damageInfo.damageColorIndex = damageColorIndex;
			damageInfo.damageType = damageType;
			healthComponent.TakeDamage(damageInfo);
			GlobalEventManager.instance.OnHitEnemy(damageInfo, ((Component)healthComponent).gameObject);
			GlobalEventManager.instance.OnHitAll(damageInfo, ((Component)healthComponent).gameObject);
		}
		if (bouncesRemaining <= 0)
		{
			return;
		}
		for (int i = 0; i < targetsToFindPerBounce; i++)
		{
			if (bouncedObjects != null)
			{
				if (canBounceOnSameTarget)
				{
					bouncedObjects.Clear();
				}
				bouncedObjects.Add(target.healthComponent);
			}
			HurtBox hurtBox = PickNextTarget(((Component)target).transform.position);
			if (Object.op_Implicit((Object)(object)hurtBox))
			{
				ChainGunOrb chainGunOrb = new ChainGunOrb(orbEffectPrefab);
				chainGunOrb.search = search;
				chainGunOrb.origin = ((Component)target).transform.position;
				chainGunOrb.target = hurtBox;
				chainGunOrb.attacker = attacker;
				chainGunOrb.teamIndex = teamIndex;
				chainGunOrb.damageValue = damageValue * damageCoefficientPerBounce;
				chainGunOrb.bouncesRemaining = bouncesRemaining - 1;
				chainGunOrb.isCrit = isCrit;
				chainGunOrb.bouncedObjects = bouncedObjects;
				chainGunOrb.procChainMask = procChainMask;
				chainGunOrb.procCoefficient = procCoefficient;
				chainGunOrb.damageColorIndex = damageColorIndex;
				chainGunOrb.damageCoefficientPerBounce = damageCoefficientPerBounce;
				chainGunOrb.speed = speed;
				chainGunOrb.bounceRange = bounceRange;
				chainGunOrb.damageType = damageType;
				OrbManager.instance.AddOrb(chainGunOrb);
			}
		}
	}

	public HurtBox PickNextTarget(Vector3 position)
	{
		//IL_0019: Unknown result type (might be due to invalid IL or missing references)
		//IL_001a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0025: Unknown result type (might be due to invalid IL or missing references)
		//IL_002a: Unknown result type (might be due to invalid IL or missing references)
		if (search == null)
		{
			search = new BullseyeSearch();
		}
		search.searchOrigin = position;
		search.searchDirection = Vector3.zero;
		search.teamMaskFilter = TeamMask.allButNeutral;
		search.teamMaskFilter.RemoveTeam(teamIndex);
		search.filterByLoS = false;
		search.sortMode = BullseyeSearch.SortMode.Distance;
		search.maxDistanceFilter = bounceRange;
		search.RefreshCandidates();
		HurtBox hurtBox = (from v in search.GetResults()
			where !bouncedObjects.Contains(v.healthComponent)
			select v).FirstOrDefault();
		if (Object.op_Implicit((Object)(object)hurtBox))
		{
			bouncedObjects.Add(hurtBox.healthComponent);
		}
		return hurtBox;
	}
}
