using UnityEngine;

namespace RoR2.Orbs;

public class InfusionOrb : Orb
{
	private const float speed = 30f;

	public int maxHpValue;

	private Inventory targetInventory;

	public override void Begin()
	{
		//IL_0019: Unknown result type (might be due to invalid IL or missing references)
		base.duration = base.distanceToTarget / 30f;
		EffectData effectData = new EffectData
		{
			origin = origin,
			genericFloat = base.duration
		};
		effectData.SetHurtBoxReference(target);
		EffectManager.SpawnEffect(LegacyResourcesAPI.Load<GameObject>("Prefabs/Effects/OrbEffects/InfusionOrbEffect"), effectData, transmit: true);
		HurtBox component = ((Component)target).GetComponent<HurtBox>();
		CharacterBody characterBody = ((component != null) ? ((Component)component.healthComponent).GetComponent<CharacterBody>() : null);
		if (Object.op_Implicit((Object)(object)characterBody))
		{
			targetInventory = characterBody.inventory;
		}
	}

	public override void OnArrival()
	{
		if (Object.op_Implicit((Object)(object)targetInventory))
		{
			targetInventory.AddInfusionBonus((uint)maxHpValue);
		}
	}
}
