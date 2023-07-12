using System.Collections.Generic;
using RoR2;
using RoR2.Orbs;
using UnityEngine;
using UnityEngine.Networking;

namespace EntityStates.AffixEarthHealer;

public class Heal : BaseState
{
	public static float radius;

	public static float healCoefficient;

	public static float healOrbTravelDuration;

	public static GameObject effectPrefab;

	public override void OnEnter()
	{
		//IL_003f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0044: Unknown result type (might be due to invalid IL or missing references)
		//IL_004b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0059: Unknown result type (might be due to invalid IL or missing references)
		//IL_005e: Unknown result type (might be due to invalid IL or missing references)
		//IL_00c8: Unknown result type (might be due to invalid IL or missing references)
		//IL_00cd: Unknown result type (might be due to invalid IL or missing references)
		//IL_0129: Unknown result type (might be due to invalid IL or missing references)
		//IL_012e: Unknown result type (might be due to invalid IL or missing references)
		base.OnEnter();
		float healValue = base.characterBody.damage * healCoefficient;
		if (!NetworkServer.active)
		{
			return;
		}
		List<HealthComponent> list = new List<HealthComponent>();
		SphereSearch sphereSearch = new SphereSearch();
		sphereSearch.radius = radius;
		sphereSearch.origin = base.transform.position;
		sphereSearch.queryTriggerInteraction = (QueryTriggerInteraction)1;
		sphereSearch.mask = LayerIndex.entityPrecise.mask;
		sphereSearch.RefreshCandidates();
		sphereSearch.FilterCandidatesByDistinctHurtBoxEntities();
		HurtBox[] hurtBoxes = sphereSearch.GetHurtBoxes();
		for (int i = 0; i < hurtBoxes.Length; i++)
		{
			HealthComponent item = hurtBoxes[i].healthComponent;
			if (!list.Contains(item))
			{
				list.Add(item);
			}
		}
		foreach (HealthComponent item2 in list)
		{
			HealOrb healOrb = new HealOrb();
			healOrb.origin = base.transform.position;
			healOrb.target = item2.body.mainHurtBox;
			healOrb.healValue = healValue;
			healOrb.overrideDuration = healOrbTravelDuration;
			OrbManager.instance.AddOrb(healOrb);
		}
		EffectManager.SimpleEffect(effectPrefab, base.transform.position, Quaternion.identity, transmit: true);
		base.characterBody.healthComponent.Suicide();
	}
}
