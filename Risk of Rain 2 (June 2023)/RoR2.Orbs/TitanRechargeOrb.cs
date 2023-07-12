using UnityEngine;

namespace RoR2.Orbs;

public class TitanRechargeOrb : Orb
{
	public int targetRockInt;

	public TitanRockController titanRockController;

	public override void Begin()
	{
		//IL_0012: Unknown result type (might be due to invalid IL or missing references)
		base.duration = 1f;
		EffectData effectData = new EffectData
		{
			origin = origin,
			genericFloat = base.duration
		};
		effectData.SetHurtBoxReference(target);
		EffectManager.SpawnEffect(LegacyResourcesAPI.Load<GameObject>("Prefabs/Effects/OrbEffects/HealthOrbEffect"), effectData, transmit: true);
	}

	public override void OnArrival()
	{
	}
}
