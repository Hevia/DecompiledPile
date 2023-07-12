using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;

namespace RoR2;

public class InfiniteTowerBossWaveController : InfiniteTowerWaveController
{
	[SerializeField]
	[Tooltip("The interactable spawner to activate when the player defeats all enemies")]
	private InteractableSpawner rewardInteractableSpawner;

	[SerializeField]
	[Tooltip("If true, it ensures that the combat director gets enough credits to spawn the initially selected champion spawn card.")]
	private bool guaranteeInitialChampion;

	[SerializeField]
	[Tooltip("If true, clear the pickups when this wave finishes")]
	private bool clearPickupsOnFinish;

	[Server]
	public override void Initialize(int waveIndex, Inventory enemyInventory, GameObject spawnTarget)
	{
		if (!NetworkServer.active)
		{
			Debug.LogWarning((object)"[Server] function 'System.Void RoR2.InfiniteTowerBossWaveController::Initialize(System.Int32,RoR2.Inventory,UnityEngine.GameObject)' called on client");
			return;
		}
		base.Initialize(waveIndex, enemyInventory, spawnTarget);
		combatDirector.SetNextSpawnAsBoss();
		if (guaranteeInitialChampion && combatDirector.monsterCredit < (float)combatDirector.lastAttemptedMonsterCard.cost)
		{
			combatDirector.monsterCredit = combatDirector.lastAttemptedMonsterCard.cost;
		}
	}

	protected override void OnAllEnemiesDefeatedServer()
	{
		//IL_002b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0035: Expected O, but got Unknown
		base.OnAllEnemiesDefeatedServer();
		if (Object.op_Implicit((Object)(object)rewardInteractableSpawner))
		{
			rewardInteractableSpawner.Spawn(new Xoroshiro128Plus(Run.instance.seed ^ (ulong)waveIndex));
		}
		InfiniteTowerRun infiniteTowerRun = Run.instance as InfiniteTowerRun;
		if (Object.op_Implicit((Object)(object)infiniteTowerRun) && !infiniteTowerRun.IsStageTransitionWave())
		{
			infiniteTowerRun.MoveSafeWard();
		}
	}

	protected override void OnFinishedServer()
	{
		base.OnFinishedServer();
		if (Object.op_Implicit((Object)(object)rewardInteractableSpawner))
		{
			rewardInteractableSpawner.DestroySpawnedInteractables();
		}
		if (!clearPickupsOnFinish)
		{
			return;
		}
		foreach (GenericPickupController item in new List<GenericPickupController>(InstanceTracker.GetInstancesList<GenericPickupController>()))
		{
			PickupDef pickupDef = PickupCatalog.GetPickupDef(item.pickupIndex);
			if (pickupDef.itemIndex != ItemIndex.None || pickupDef.equipmentIndex != EquipmentIndex.None)
			{
				Object.Destroy((Object)(object)((Component)item).gameObject);
			}
		}
	}

	protected override void OnTimerExpire()
	{
	}

	private void UNetVersion()
	{
	}

	public override bool OnSerialize(NetworkWriter writer, bool forceAll)
	{
		bool flag = base.OnSerialize(writer, forceAll);
		bool flag2 = default(bool);
		return flag2 || flag;
	}

	public override void OnDeserialize(NetworkReader reader, bool initialState)
	{
		base.OnDeserialize(reader, initialState);
	}

	public override void PreStartClient()
	{
		base.PreStartClient();
	}
}
