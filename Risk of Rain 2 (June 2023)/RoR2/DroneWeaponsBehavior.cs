using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.Networking;

namespace RoR2;

public class DroneWeaponsBehavior : CharacterBody.ItemBehavior
{
	private const float display2Chance = 0.1f;

	private int previousStack;

	private CharacterSpawnCard droneSpawnCard;

	private Xoroshiro128Plus rng;

	private DirectorPlacementRule placementRule;

	private const float minSpawnDist = 3f;

	private const float maxSpawnDist = 40f;

	private const float spawnRetryDelay = 1f;

	private bool hasSpawnedDrone;

	private float spawnDelay;

	private void OnEnable()
	{
		//IL_0019: Unknown result type (might be due to invalid IL or missing references)
		//IL_0023: Expected O, but got Unknown
		//IL_0029: Unknown result type (might be due to invalid IL or missing references)
		//IL_002e: Unknown result type (might be due to invalid IL or missing references)
		ulong num = Run.instance.seed ^ (ulong)Run.instance.stageClearCount;
		rng = new Xoroshiro128Plus(num);
		droneSpawnCard = Addressables.LoadAssetAsync<CharacterSpawnCard>((object)"RoR2/DLC1/DroneCommander/cscDroneCommander.asset").WaitForCompletion();
		placementRule = new DirectorPlacementRule
		{
			placementMode = DirectorPlacementRule.PlacementMode.Approximate,
			minDistance = 3f,
			maxDistance = 40f,
			spawnOnTarget = ((Component)this).transform
		};
		UpdateAllMinions(stack);
		MasterSummon.onServerMasterSummonGlobal += OnServerMasterSummonGlobal;
	}

	private void OnDisable()
	{
		MasterSummon.onServerMasterSummonGlobal -= OnServerMasterSummonGlobal;
		UpdateAllMinions(0);
	}

	private void FixedUpdate()
	{
		if (previousStack != stack)
		{
			UpdateAllMinions(stack);
		}
		spawnDelay -= Time.fixedDeltaTime;
		if (!hasSpawnedDrone && Object.op_Implicit((Object)(object)body) && spawnDelay <= 0f)
		{
			TrySpawnDrone();
		}
	}

	private void OnServerMasterSummonGlobal(MasterSummon.MasterSummonReport summonReport)
	{
		if (!Object.op_Implicit((Object)(object)body) || !Object.op_Implicit((Object)(object)body.master) || !((Object)(object)body.master == (Object)(object)summonReport.leaderMasterInstance))
		{
			return;
		}
		CharacterMaster summonMasterInstance = summonReport.summonMasterInstance;
		if (Object.op_Implicit((Object)(object)summonMasterInstance))
		{
			CharacterBody characterBody = summonMasterInstance.GetBody();
			if (Object.op_Implicit((Object)(object)characterBody))
			{
				UpdateMinionInventory(summonMasterInstance.inventory, characterBody.bodyFlags, stack);
			}
		}
	}

	private void UpdateAllMinions(int newStack)
	{
		//IL_0037: Unknown result type (might be due to invalid IL or missing references)
		if (!Object.op_Implicit((Object)(object)body) || !Object.op_Implicit((Object)(object)body?.master))
		{
			return;
		}
		MinionOwnership.MinionGroup minionGroup = MinionOwnership.MinionGroup.FindGroup(((NetworkBehaviour)body.master).netId);
		if (minionGroup == null)
		{
			return;
		}
		MinionOwnership[] members = minionGroup.members;
		foreach (MinionOwnership minionOwnership in members)
		{
			if (!Object.op_Implicit((Object)(object)minionOwnership))
			{
				continue;
			}
			CharacterMaster component = ((Component)minionOwnership).GetComponent<CharacterMaster>();
			if (Object.op_Implicit((Object)(object)component) && Object.op_Implicit((Object)(object)component.inventory))
			{
				CharacterBody characterBody = component.GetBody();
				if (Object.op_Implicit((Object)(object)characterBody))
				{
					UpdateMinionInventory(component.inventory, characterBody.bodyFlags, newStack);
				}
			}
		}
		previousStack = newStack;
	}

	private void UpdateMinionInventory(Inventory inventory, CharacterBody.BodyFlags bodyFlags, int newStack)
	{
		if (Object.op_Implicit((Object)(object)inventory) && newStack > 0 && (bodyFlags & CharacterBody.BodyFlags.Mechanical) != 0)
		{
			int itemCount = inventory.GetItemCount(DLC1Content.Items.DroneWeaponsBoost);
			int itemCount2 = inventory.GetItemCount(DLC1Content.Items.DroneWeaponsDisplay1);
			int itemCount3 = inventory.GetItemCount(DLC1Content.Items.DroneWeaponsDisplay2);
			if (itemCount < stack)
			{
				inventory.GiveItem(DLC1Content.Items.DroneWeaponsBoost, stack - itemCount);
			}
			else if (itemCount > stack)
			{
				inventory.RemoveItem(DLC1Content.Items.DroneWeaponsBoost, itemCount - stack);
			}
			if (itemCount2 + itemCount3 <= 0)
			{
				ItemDef itemDef = DLC1Content.Items.DroneWeaponsDisplay1;
				if (Random.value < 0.1f)
				{
					itemDef = DLC1Content.Items.DroneWeaponsDisplay2;
				}
				inventory.GiveItem(itemDef);
			}
		}
		else
		{
			inventory.ResetItem(DLC1Content.Items.DroneWeaponsBoost);
			inventory.ResetItem(DLC1Content.Items.DroneWeaponsDisplay1);
			inventory.ResetItem(DLC1Content.Items.DroneWeaponsDisplay2);
		}
	}

	private void TrySpawnDrone()
	{
		if (!body.master.IsDeployableLimited(DeployableSlot.DroneWeaponsDrone))
		{
			spawnDelay = 1f;
			DirectorSpawnRequest directorSpawnRequest = new DirectorSpawnRequest(droneSpawnCard, placementRule, rng);
			directorSpawnRequest.summonerBodyObject = ((Component)this).gameObject;
			directorSpawnRequest.onSpawnedServer = OnMasterSpawned;
			DirectorCore.instance.TrySpawnObject(directorSpawnRequest);
		}
	}

	private void OnMasterSpawned(SpawnCard.SpawnResult spawnResult)
	{
		hasSpawnedDrone = true;
		GameObject spawnedInstance = spawnResult.spawnedInstance;
		if (!Object.op_Implicit((Object)(object)spawnedInstance))
		{
			return;
		}
		CharacterMaster component = spawnedInstance.GetComponent<CharacterMaster>();
		if (Object.op_Implicit((Object)(object)component))
		{
			Deployable component2 = ((Component)component).GetComponent<Deployable>();
			if (Object.op_Implicit((Object)(object)component2))
			{
				body.master.AddDeployable(component2, DeployableSlot.DroneWeaponsDrone);
			}
		}
	}
}
