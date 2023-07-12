using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;

namespace RoR2.Items;

public static class SuppressedItemManager
{
	private static Dictionary<ItemIndex, ItemIndex> transformationMap = new Dictionary<ItemIndex, ItemIndex>();

	private static GameObject networkedInventoryPrefab;

	private static HashSet<Inventory> pendingTransformationInventories = new HashSet<Inventory>();

	public static Inventory suppressedInventory { get; private set; }

	[SystemInitializer(new Type[] { typeof(ItemCatalog) })]
	private static void Init()
	{
		Run.onRunStartGlobal += OnRunStart;
		Run.onRunDestroyGlobal += OnRunDestroyGlobal;
		Inventory.onInventoryChangedGlobal += OnInventoryChangedGlobal;
		RoR2Application.onFixedUpdate += StaticFixedUpdate;
		networkedInventoryPrefab = LegacyResourcesAPI.Load<GameObject>("Prefabs/NetworkedObjects/SuppressedItemInventory");
	}

	public static bool HasItemBeenSuppressed(ItemIndex itemIndex)
	{
		return transformationMap.ContainsKey(itemIndex);
	}

	public static bool HasAnyItemBeenSuppressed()
	{
		return transformationMap.Count > 0;
	}

	public static bool SuppressItem(ItemIndex suppressedIndex, ItemIndex transformedIndex = ItemIndex.None)
	{
		if (!transformationMap.ContainsKey(suppressedIndex))
		{
			ItemDef itemDef = ItemCatalog.GetItemDef(suppressedIndex);
			if (Object.op_Implicit((Object)(object)itemDef))
			{
				ItemTierDef itemTierDef = ItemTierCatalog.GetItemTierDef(itemDef.tier);
				ItemIndex itemIndex = ItemIndex.None;
				bool flag = false;
				if (Object.op_Implicit((Object)(object)itemTierDef))
				{
					switch (itemTierDef.tier)
					{
					case ItemTier.Tier3:
						itemIndex = DLC1Content.Items.ScrapRedSuppressed.itemIndex;
						flag = Run.instance.availableTier3DropList.Count == 1;
						break;
					case ItemTier.Tier2:
						itemIndex = DLC1Content.Items.ScrapGreenSuppressed.itemIndex;
						flag = Run.instance.availableTier2DropList.Count == 1;
						break;
					case ItemTier.Tier1:
						itemIndex = DLC1Content.Items.ScrapWhiteSuppressed.itemIndex;
						flag = Run.instance.availableTier1DropList.Count == 1;
						break;
					}
				}
				if (itemIndex != suppressedIndex)
				{
					PickupIndex pickupIndex = PickupCatalog.FindPickupIndex(suppressedIndex);
					transformationMap.Add(suppressedIndex, transformedIndex);
					suppressedInventory.GiveItem(suppressedIndex);
					Run.instance.DisableItemDrop(suppressedIndex);
					if (flag)
					{
						Run.instance.EnableItemDrop(itemIndex);
					}
					if (transformedIndex != ItemIndex.None)
					{
						Inventory[] array = Object.FindObjectsOfType<Inventory>();
						foreach (Inventory inventory in array)
						{
							if (inventory != suppressedInventory)
							{
								TransformItem(inventory, suppressedIndex, transformedIndex);
							}
						}
					}
					ChestBehavior[] array2 = Object.FindObjectsOfType<ChestBehavior>();
					foreach (ChestBehavior chestBehavior in array2)
					{
						if (chestBehavior.HasRolledPickup(pickupIndex))
						{
							chestBehavior.Roll();
						}
					}
					OptionChestBehavior[] array3 = Object.FindObjectsOfType<OptionChestBehavior>();
					foreach (OptionChestBehavior optionChestBehavior in array3)
					{
						if (optionChestBehavior.HasRolledPickup(pickupIndex))
						{
							optionChestBehavior.Roll();
						}
					}
					ShopTerminalBehavior[] array4 = Object.FindObjectsOfType<ShopTerminalBehavior>();
					foreach (ShopTerminalBehavior shopTerminalBehavior in array4)
					{
						if (shopTerminalBehavior.CurrentPickupIndex() == pickupIndex)
						{
							shopTerminalBehavior.GenerateNewPickupServer();
						}
					}
					VoidSuppressorBehavior[] array5 = Object.FindObjectsOfType<VoidSuppressorBehavior>();
					for (int i = 0; i < array5.Length; i++)
					{
						array5[i].RefreshItems();
					}
					return true;
				}
			}
		}
		return false;
	}

	private static void OnRunStart(Run run)
	{
		if (NetworkServer.active)
		{
			transformationMap = new Dictionary<ItemIndex, ItemIndex>();
			suppressedInventory = Object.Instantiate<GameObject>(networkedInventoryPrefab).GetComponent<Inventory>();
			NetworkServer.Spawn(((Component)suppressedInventory).gameObject);
		}
	}

	private static void OnRunDestroyGlobal(Run run)
	{
		if (Object.op_Implicit((Object)(object)suppressedInventory))
		{
			NetworkServer.Destroy(((Component)suppressedInventory).gameObject);
		}
		suppressedInventory = null;
	}

	private static void TransformItem(Inventory inventory, ItemIndex suppressedIndex, ItemIndex transformedIndex)
	{
		int itemCount = inventory.GetItemCount(suppressedIndex);
		if (itemCount > 0)
		{
			inventory.RemoveItem(suppressedIndex, itemCount);
			inventory.GiveItem(transformedIndex, itemCount);
			CharacterMaster component = ((Component)inventory).GetComponent<CharacterMaster>();
			if (Object.op_Implicit((Object)(object)component))
			{
				CharacterMasterNotificationQueue.SendTransformNotification(component, suppressedIndex, transformedIndex, CharacterMasterNotificationQueue.TransformationType.Suppressed);
			}
		}
	}

	private static void StaticFixedUpdate()
	{
		foreach (Inventory pendingTransformationInventory in pendingTransformationInventories)
		{
			foreach (KeyValuePair<ItemIndex, ItemIndex> item in transformationMap)
			{
				if (item.Value != ItemIndex.None)
				{
					TransformItem(pendingTransformationInventory, item.Key, item.Value);
				}
			}
		}
		pendingTransformationInventories.Clear();
	}

	private static void OnInventoryChangedGlobal(Inventory inventory)
	{
		if (inventory == suppressedInventory || pendingTransformationInventories.Contains(inventory))
		{
			return;
		}
		foreach (KeyValuePair<ItemIndex, ItemIndex> item in transformationMap)
		{
			if (item.Value != ItemIndex.None && inventory.GetItemCount(item.Key) > 0)
			{
				pendingTransformationInventories.Add(inventory);
				break;
			}
		}
	}
}
