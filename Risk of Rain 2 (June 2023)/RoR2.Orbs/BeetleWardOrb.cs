using UnityEngine;
using UnityEngine.Networking;

namespace RoR2.Orbs;

public class BeetleWardOrb : Orb
{
	public float speed;

	public override void Begin()
	{
		//IL_001a: Unknown result type (might be due to invalid IL or missing references)
		base.duration = base.distanceToTarget / speed;
		EffectData effectData = new EffectData
		{
			origin = origin,
			genericFloat = base.duration
		};
		effectData.SetHurtBoxReference(target);
		EffectManager.SpawnEffect(LegacyResourcesAPI.Load<GameObject>("Prefabs/Effects/OrbEffects/BeetleWardOrbEffect"), effectData, transmit: true);
	}

	public override void OnArrival()
	{
		//IL_0022: Unknown result type (might be due to invalid IL or missing references)
		//IL_0027: Unknown result type (might be due to invalid IL or missing references)
		if (Object.op_Implicit((Object)(object)target))
		{
			GameObject obj = Object.Instantiate<GameObject>(LegacyResourcesAPI.Load<GameObject>("Prefabs/NetworkedObjects/BeetleWard"), ((Component)target).transform.position, Quaternion.identity);
			obj.GetComponent<TeamFilter>().teamIndex = target.teamIndex;
			NetworkServer.Spawn(obj);
		}
	}
}
