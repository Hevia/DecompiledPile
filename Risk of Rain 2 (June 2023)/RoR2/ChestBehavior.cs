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

public class ChestBehavior : NetworkBehaviour, IChestBehavior
{
	public PickupDropTable dropTable;

	public Transform dropTransform;

	public float dropUpVelocityStrength = 20f;

	public float dropForwardVelocityStrength = 2f;

	public int minDropCount = 1;

	public int maxDropCount = 1;

	public SerializableEntityStateType openState = new SerializableEntityStateType(typeof(Opening));

	[Header("Deprecated")]
	[Tooltip("Deprecated.  Use DropTable instead.")]
	public float tier1Chance = 0.8f;

	[Tooltip("Deprecated.  Use DropTable instead.")]
	public float tier2Chance = 0.2f;

	[Tooltip("Deprecated.  Use DropTable instead.")]
	public float tier3Chance = 0.01f;

	[Tooltip("Deprecated.  Use DropTable instead.")]
	public float lunarChance;

	[Tooltip("Deprecated.  Use DropTable instead.")]
	public ItemTag requiredItemTag;

	[Tooltip("Deprecated.  Use DropTable instead.")]
	public float lunarCoinChance;

	[Tooltip("Deprecated.  Use DropTable instead.")]
	public UnityEvent dropRoller;

	private Xoroshiro128Plus rng;

	private int dropCount;

	public PickupIndex dropPickup { get; private set; } = PickupIndex.none;


	public override int GetNetworkChannel()
	{
		return QosChannelIndex.defaultReliable.intVal;
	}

	[Server]
	public void Roll()
	{
		if (!NetworkServer.active)
		{
			Debug.LogWarning((object)"[Server] function 'System.Void RoR2.ChestBehavior::Roll()' called on client");
			return;
		}
		if (Object.op_Implicit((Object)(object)dropTable))
		{
			dropPickup = dropTable.GenerateDrop(rng);
			return;
		}
		UnityEvent obj = dropRoller;
		if (obj != null)
		{
			obj.Invoke();
		}
	}

	[Server]
	private void PickFromList(List<PickupIndex> dropList)
	{
		if (!NetworkServer.active)
		{
			Debug.LogWarning((object)"[Server] function 'System.Void RoR2.ChestBehavior::PickFromList(System.Collections.Generic.List`1<RoR2.PickupIndex>)' called on client");
			return;
		}
		dropPickup = PickupIndex.none;
		if (dropList != null && dropList.Count > 0)
		{
			dropPickup = rng.NextElementUniform<PickupIndex>(dropList);
		}
	}

	[Server]
	[Obsolete("Just use a drop table")]
	public void RollItem()
	{
		if (!NetworkServer.active)
		{
			Debug.LogWarning((object)"[Server] function 'System.Void RoR2.ChestBehavior::RollItem()' called on client");
			return;
		}
		WeightedSelection<List<PickupIndex>> selector = new WeightedSelection<List<PickupIndex>>();
		List<PickupIndex> sourceDropList2 = new List<PickupIndex> { PickupCatalog.FindPickupIndex(RoR2Content.MiscPickups.LunarCoin.miscPickupIndex) };
		Add(Run.instance.availableTier1DropList, tier1Chance);
		Add(Run.instance.availableTier2DropList, tier2Chance);
		Add(Run.instance.availableTier3DropList, tier3Chance);
		Add(Run.instance.availableLunarCombinedDropList, lunarChance);
		Add(sourceDropList2, lunarCoinChance);
		List<PickupIndex> dropList = selector.Evaluate(rng.nextNormalizedFloat);
		PickFromList(dropList);
		void Add(List<PickupIndex> sourceDropList, float chance)
		{
			if (!(chance <= 0f))
			{
				selector.AddChoice(sourceDropList.Where(PassesFilter).ToList(), chance);
			}
		}
		bool PassesFilter(PickupIndex pickupIndex)
		{
			if (requiredItemTag == ItemTag.Any)
			{
				return true;
			}
			PickupDef pickupDef = PickupCatalog.GetPickupDef(pickupIndex);
			if (pickupDef.itemIndex != ItemIndex.None)
			{
				return ItemCatalog.GetItemDef(pickupDef.itemIndex).ContainsTag(requiredItemTag);
			}
			return true;
		}
	}

	[Server]
	[Obsolete("Just use a drop table")]
	public void RollEquipment()
	{
		if (!NetworkServer.active)
		{
			Debug.LogWarning((object)"[Server] function 'System.Void RoR2.ChestBehavior::RollEquipment()' called on client");
		}
		else
		{
			PickFromList(Run.instance.availableEquipmentDropList);
		}
	}

	private void Awake()
	{
		if ((Object)(object)dropTransform == (Object)null)
		{
			dropTransform = ((Component)this).transform;
		}
	}

	private void Start()
	{
		//IL_0017: Unknown result type (might be due to invalid IL or missing references)
		//IL_0021: Expected O, but got Unknown
		if (NetworkServer.active)
		{
			rng = new Xoroshiro128Plus(Run.instance.treasureRng.nextUlong);
			dropCount = rng.RangeInt(minDropCount, Math.Max(minDropCount + 1, maxDropCount + 1));
			Roll();
		}
	}

	[Server]
	public void Open()
	{
		if (!NetworkServer.active)
		{
			Debug.LogWarning((object)"[Server] function 'System.Void RoR2.ChestBehavior::Open()' called on client");
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
		//IL_003e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0049: Unknown result type (might be due to invalid IL or missing references)
		//IL_0054: Unknown result type (might be due to invalid IL or missing references)
		//IL_005f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0064: Unknown result type (might be due to invalid IL or missing references)
		//IL_0069: Unknown result type (might be due to invalid IL or missing references)
		//IL_006a: Unknown result type (might be due to invalid IL or missing references)
		//IL_006f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0074: Unknown result type (might be due to invalid IL or missing references)
		//IL_0085: Unknown result type (might be due to invalid IL or missing references)
		//IL_008a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0094: Unknown result type (might be due to invalid IL or missing references)
		//IL_0099: Unknown result type (might be due to invalid IL or missing references)
		//IL_009e: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a4: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a5: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a6: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ab: Unknown result type (might be due to invalid IL or missing references)
		if (!NetworkServer.active)
		{
			Debug.LogWarning((object)"[Server] function 'System.Void RoR2.ChestBehavior::ItemDrop()' called on client");
		}
		else if (!(dropPickup == PickupIndex.none) && dropCount >= 1)
		{
			float num = 360f / (float)dropCount;
			Vector3 val = Vector3.up * dropUpVelocityStrength + dropTransform.forward * dropForwardVelocityStrength;
			Quaternion val2 = Quaternion.AngleAxis(num, Vector3.up);
			for (int i = 0; i < dropCount; i++)
			{
				PickupDropletController.CreatePickupDroplet(dropPickup, dropTransform.position + Vector3.up * 1.5f, val);
				val = val2 * val;
				Roll();
			}
			dropPickup = PickupIndex.none;
		}
	}

	public bool HasRolledPickup(PickupIndex pickupIndex)
	{
		return dropPickup == pickupIndex;
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
