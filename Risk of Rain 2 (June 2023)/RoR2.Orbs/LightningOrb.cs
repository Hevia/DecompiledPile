using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace RoR2.Orbs;

public class LightningOrb : Orb
{
	public enum LightningType
	{
		Ukulele,
		Tesla,
		BFG,
		TreePoisonDart,
		HuntressGlaive,
		Loader,
		RazorWire,
		CrocoDisease,
		MageLightning,
		Count
	}

	public float speed = 100f;

	public float damageValue;

	public GameObject attacker;

	public GameObject inflictor;

	public int bouncesRemaining;

	public List<HealthComponent> bouncedObjects;

	public TeamIndex teamIndex;

	public bool isCrit;

	public ProcChainMask procChainMask;

	public float procCoefficient = 1f;

	public DamageColorIndex damageColorIndex;

	public float range = 20f;

	public float damageCoefficientPerBounce = 1f;

	public int targetsToFindPerBounce = 1;

	public DamageType damageType;

	private bool canBounceOnSameTarget;

	private bool failedToKill;

	public LightningType lightningType;

	private BullseyeSearch search;

	public static event Action<LightningOrb> onLightningOrbKilledOnAllBounces;

	public override void Begin()
	{
		//IL_0104: Unknown result type (might be due to invalid IL or missing references)
		base.duration = 0.1f;
		string text = null;
		switch (lightningType)
		{
		case LightningType.Ukulele:
			text = "Prefabs/Effects/OrbEffects/LightningOrbEffect";
			break;
		case LightningType.Tesla:
			text = "Prefabs/Effects/OrbEffects/TeslaOrbEffect";
			break;
		case LightningType.BFG:
			text = "Prefabs/Effects/OrbEffects/BeamSphereOrbEffect";
			base.duration = 0.4f;
			break;
		case LightningType.HuntressGlaive:
			text = "Prefabs/Effects/OrbEffects/HuntressGlaiveOrbEffect";
			base.duration = base.distanceToTarget / speed;
			canBounceOnSameTarget = true;
			break;
		case LightningType.TreePoisonDart:
			text = "Prefabs/Effects/OrbEffects/TreePoisonDartOrbEffect";
			speed = 40f;
			base.duration = base.distanceToTarget / speed;
			break;
		case LightningType.Loader:
			text = "Prefabs/Effects/OrbEffects/LoaderLightningOrbEffect";
			break;
		case LightningType.RazorWire:
			text = "Prefabs/Effects/OrbEffects/RazorwireOrbEffect";
			base.duration = 0.2f;
			break;
		case LightningType.CrocoDisease:
			text = "Prefabs/Effects/OrbEffects/CrocoDiseaseOrbEffect";
			base.duration = 0.6f;
			targetsToFindPerBounce = 2;
			break;
		case LightningType.MageLightning:
			text = "Prefabs/Effects/OrbEffects/MageLightningOrbEffect";
			base.duration = 0.1f;
			break;
		}
		EffectData effectData = new EffectData
		{
			origin = origin,
			genericFloat = base.duration
		};
		effectData.SetHurtBoxReference(target);
		EffectManager.SpawnEffect(LegacyResourcesAPI.Load<GameObject>(text), effectData, transmit: true);
	}

	public override void OnArrival()
	{
		//IL_0052: Unknown result type (might be due to invalid IL or missing references)
		//IL_0057: Unknown result type (might be due to invalid IL or missing references)
		//IL_008c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0091: Unknown result type (might be due to invalid IL or missing references)
		//IL_0145: Unknown result type (might be due to invalid IL or missing references)
		//IL_017c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0181: Unknown result type (might be due to invalid IL or missing references)
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
			damageInfo.inflictor = inflictor;
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
		failedToKill |= !Object.op_Implicit((Object)(object)healthComponent) || healthComponent.alive;
		if (bouncesRemaining > 0)
		{
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
					LightningOrb lightningOrb = new LightningOrb();
					lightningOrb.search = search;
					lightningOrb.origin = ((Component)target).transform.position;
					lightningOrb.target = hurtBox;
					lightningOrb.attacker = attacker;
					lightningOrb.inflictor = inflictor;
					lightningOrb.teamIndex = teamIndex;
					lightningOrb.damageValue = damageValue * damageCoefficientPerBounce;
					lightningOrb.bouncesRemaining = bouncesRemaining - 1;
					lightningOrb.isCrit = isCrit;
					lightningOrb.bouncedObjects = bouncedObjects;
					lightningOrb.lightningType = lightningType;
					lightningOrb.procChainMask = procChainMask;
					lightningOrb.procCoefficient = procCoefficient;
					lightningOrb.damageColorIndex = damageColorIndex;
					lightningOrb.damageCoefficientPerBounce = damageCoefficientPerBounce;
					lightningOrb.speed = speed;
					lightningOrb.range = range;
					lightningOrb.damageType = damageType;
					lightningOrb.failedToKill = failedToKill;
					OrbManager.instance.AddOrb(lightningOrb);
				}
			}
		}
		else if (!failedToKill)
		{
			LightningOrb.onLightningOrbKilledOnAllBounces?.Invoke(this);
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
		search.maxDistanceFilter = range;
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
