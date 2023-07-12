using UnityEngine;

namespace RoR2.Orbs;

public class HealOrb : Orb
{
	public float healValue;

	public bool scaleOrb = true;

	public float overrideDuration = 0.3f;

	public override void Begin()
	{
		//IL_0058: Unknown result type (might be due to invalid IL or missing references)
		if (Object.op_Implicit((Object)(object)target))
		{
			base.duration = overrideDuration;
			float scale = (scaleOrb ? Mathf.Min(healValue / target.healthComponent.fullHealth, 1f) : 1f);
			EffectData effectData = new EffectData
			{
				scale = scale,
				origin = origin,
				genericFloat = base.duration
			};
			effectData.SetHurtBoxReference(target);
			EffectManager.SpawnEffect(LegacyResourcesAPI.Load<GameObject>("Prefabs/Effects/OrbEffects/HealthOrbEffect"), effectData, transmit: true);
		}
	}

	public override void OnArrival()
	{
		if (Object.op_Implicit((Object)(object)target))
		{
			HealthComponent healthComponent = target.healthComponent;
			if (Object.op_Implicit((Object)(object)healthComponent))
			{
				healthComponent.Heal(healValue, default(ProcChainMask));
			}
		}
	}
}
