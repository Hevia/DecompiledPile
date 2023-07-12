using RoR2;
using UnityEngine;

namespace EntityStates.Gup;

internal class SpawnState : GenericCharacterSpawnState
{
	public static GameObject spawnEffectPrefab;

	public static string spawnEffectMuzzle;

	public override void OnEnter()
	{
		//IL_0028: Unknown result type (might be due to invalid IL or missing references)
		base.OnEnter();
		if (Object.op_Implicit((Object)(object)spawnEffectPrefab))
		{
			EffectManager.SpawnEffect(spawnEffectPrefab, new EffectData
			{
				origin = FindModelChild(spawnEffectMuzzle).position,
				scale = base.characterBody.radius
			}, transmit: true);
		}
	}
}
