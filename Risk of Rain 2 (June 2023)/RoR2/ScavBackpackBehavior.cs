using System;
using System.Collections.Generic;
using System.Linq;
using EntityStates;
using EntityStates.Barrel;
using RoR2.Networking;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.Networking;

namespace RoR2;

public class ScavBackpackBehavior : NetworkBehaviour
{
	private PickupIndex dropPickup = PickupIndex.none;

	public float tier1Chance = 0.8f;

	public float tier2Chance = 0.2f;

	public float tier3Chance = 0.01f;

	public float lunarChance;

	public int totalItems;

	public float delayBetweenItems;

	public ItemTag requiredItemTag;

	public UnityEvent dropRoller;

	public SerializableEntityStateType openState = new SerializableEntityStateType(typeof(Opening));

	public override int GetNetworkChannel()
	{
		return QosChannelIndex.defaultReliable.intVal;
	}

	[Server]
	private void PickFromList(List<PickupIndex> dropList)
	{
		if (!NetworkServer.active)
		{
			Debug.LogWarning((object)"[Server] function 'System.Void RoR2.ScavBackpackBehavior::PickFromList(System.Collections.Generic.List`1<RoR2.PickupIndex>)' called on client");
			return;
		}
		dropPickup = PickupIndex.none;
		if (dropList != null && dropList.Count > 0)
		{
			dropPickup = Run.instance.treasureRng.NextElementUniform<PickupIndex>(dropList);
		}
	}

	[Server]
	public void RollItem()
	{
		if (!NetworkServer.active)
		{
			Debug.LogWarning((object)"[Server] function 'System.Void RoR2.ScavBackpackBehavior::RollItem()' called on client");
			return;
		}
		WeightedSelection<List<PickupIndex>> weightedSelection = new WeightedSelection<List<PickupIndex>>();
		weightedSelection.AddChoice(Run.instance.availableTier1DropList.Where(RollItemTest).ToList(), tier1Chance);
		weightedSelection.AddChoice(Run.instance.availableTier2DropList.Where(RollItemTest).ToList(), tier2Chance);
		weightedSelection.AddChoice(Run.instance.availableTier3DropList.Where(RollItemTest).ToList(), tier3Chance);
		weightedSelection.AddChoice(Run.instance.availableLunarCombinedDropList.Where(RollItemTest).ToList(), lunarChance);
		List<PickupIndex> dropList = weightedSelection.Evaluate(Run.instance.treasureRng.nextNormalizedFloat);
		PickFromList(dropList);
		bool RollItemTest(PickupIndex pickupIndex)
		{
			return ItemCatalog.GetItemDef(PickupCatalog.GetPickupDef(pickupIndex)?.itemIndex ?? ItemIndex.None)?.ContainsTag(requiredItemTag) ?? false;
		}
	}

	[Server]
	public void RollEquipment()
	{
		if (!NetworkServer.active)
		{
			Debug.LogWarning((object)"[Server] function 'System.Void RoR2.ScavBackpackBehavior::RollEquipment()' called on client");
		}
		else
		{
			PickFromList(Run.instance.availableEquipmentDropList);
		}
	}

	private void Start()
	{
		if (NetworkServer.active)
		{
			if (dropRoller != null)
			{
				dropRoller.Invoke();
			}
			else
			{
				Debug.LogFormat("Chest {0} has no item roller assigned!", Array.Empty<object>());
			}
		}
	}

	[Server]
	public void Open()
	{
		if (!NetworkServer.active)
		{
			Debug.LogWarning((object)"[Server] function 'System.Void RoR2.ScavBackpackBehavior::Open()' called on client");
			return;
		}
		EntityStateMachine component = ((Component)this).GetComponent<EntityStateMachine>();
		if (Object.op_Implicit((Object)(object)component))
		{
			component.SetNextState(EntityStateCatalog.InstantiateState(openState));
		}
	}

	[Server]
	public void ItemDrop()
	{
		//IL_0034: Unknown result type (might be due to invalid IL or missing references)
		//IL_0039: Unknown result type (might be due to invalid IL or missing references)
		//IL_0043: Unknown result type (might be due to invalid IL or missing references)
		//IL_0048: Unknown result type (might be due to invalid IL or missing references)
		//IL_004d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0057: Unknown result type (might be due to invalid IL or missing references)
		//IL_0062: Unknown result type (might be due to invalid IL or missing references)
		//IL_006c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0071: Unknown result type (might be due to invalid IL or missing references)
		if (!NetworkServer.active)
		{
			Debug.LogWarning((object)"[Server] function 'System.Void RoR2.ScavBackpackBehavior::ItemDrop()' called on client");
		}
		else if (!(dropPickup == PickupIndex.none))
		{
			PickupDropletController.CreatePickupDroplet(dropPickup, ((Component)this).transform.position + Vector3.up * 1.5f, Vector3.up * 20f + ((Component)this).transform.forward * 2f);
			dropPickup = PickupIndex.none;
		}
	}

	private void UNetVersion()
	{
	}

	public override bool OnSerialize(NetworkWriter writer, bool forceAll)
	{
		bool result = default(bool);
		return result;
	}

	public override void OnDeserialize(NetworkReader reader, bool initialState)
	{
	}

	public override void PreStartClient()
	{
	}
}
