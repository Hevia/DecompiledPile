using RoR2;
using UnityEngine;

namespace EntityStates.LunarExploderMonster;

public class SpawnState : GenericCharacterSpawnState
{
	public static GameObject spawnEffectPrefab;

	public static string spawnEffectChildString;

	public override void OnEnter()
	{
		base.OnEnter();
		if (Object.op_Implicit((Object)(object)spawnEffectPrefab))
		{
			EffectManager.SimpleMuzzleFlash(spawnEffectPrefab, base.gameObject, spawnEffectChildString, transmit: false);
		}
	}
}
