using UnityEngine;

namespace RoR2.Orbs;

public class GoldOrb : Orb
{
	public uint goldAmount;

	public bool scaleOrb = true;

	public float overrideDuration = 0.6f;

	public override void Begin()
	{
		//IL_0054: Unknown result type (might be due to invalid IL or missing references)
		if (Object.op_Implicit((Object)(object)target))
		{
			base.duration = overrideDuration;
			float scale = (scaleOrb ? Mathf.Min((float)goldAmount / Run.instance.difficultyCoefficient, 1f) : 1f);
			EffectData effectData = new EffectData
			{
				scale = scale,
				origin = origin,
				genericFloat = base.duration
			};
			effectData.SetHurtBoxReference(target);
			EffectManager.SpawnEffect(LegacyResourcesAPI.Load<GameObject>("Prefabs/Effects/OrbEffects/GoldOrbEffect"), effectData, transmit: true);
		}
	}

	public override void OnArrival()
	{
		if (Object.op_Implicit((Object)(object)target) && Object.op_Implicit((Object)(object)target.healthComponent) && Object.op_Implicit((Object)(object)target.healthComponent.body) && Object.op_Implicit((Object)(object)target.healthComponent.body.master))
		{
			target.healthComponent.body.master.GiveMoney(goldAmount);
		}
	}
}
