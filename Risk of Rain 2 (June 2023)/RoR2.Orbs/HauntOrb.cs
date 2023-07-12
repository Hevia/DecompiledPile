using UnityEngine;

namespace RoR2.Orbs;

public class HauntOrb : Orb
{
	public float timeToArrive;

	public float scale;

	public override void Begin()
	{
		//IL_002f: Unknown result type (might be due to invalid IL or missing references)
		base.duration = timeToArrive + Random.Range(0f, 0.4f);
		EffectData effectData = new EffectData
		{
			scale = scale,
			origin = origin,
			genericFloat = base.duration
		};
		effectData.SetHurtBoxReference(target);
		EffectManager.SpawnEffect(LegacyResourcesAPI.Load<GameObject>("Prefabs/Effects/OrbEffects/HauntOrbEffect"), effectData, transmit: true);
	}

	public override void OnArrival()
	{
		if (Object.op_Implicit((Object)(object)target))
		{
			HealthComponent healthComponent = target.healthComponent;
			if (Object.op_Implicit((Object)(object)healthComponent) && Object.op_Implicit((Object)(object)healthComponent.body))
			{
				healthComponent.body.AddTimedBuff(RoR2Content.Buffs.AffixHaunted, 15f);
			}
		}
	}
}
