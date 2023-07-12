using RoR2;
using UnityEngine;

namespace EntityStates.RoboBallBoss;

public class SpawnState : GenericCharacterSpawnState
{
	public static GameObject spawnEffectPrefab;

	public static float spawnEffectRadius;

	public override void OnEnter()
	{
		//IL_0023: Unknown result type (might be due to invalid IL or missing references)
		base.OnEnter();
		if (Object.op_Implicit((Object)(object)spawnEffectPrefab))
		{
			EffectManager.SpawnEffect(spawnEffectPrefab, new EffectData
			{
				origin = base.characterBody.corePosition,
				scale = spawnEffectRadius
			}, transmit: false);
		}
	}
}
