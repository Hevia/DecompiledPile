using System;
using System.Collections.Generic;
using HG;
using UnityEngine;
using UnityEngine.Networking;

namespace RoR2.Items;

public static class ContagiousItemManager
{
	public struct TransformationInfo
	{
		public ItemIndex originalItem;

		public ItemIndex transformedItem;
	}

	private struct InventoryReplacementCandidate
	{
		public Inventory inventory;

		public ItemIndex originalItem;

		public Run.FixedTimeStamp time;

		public bool isForced;
	}

	public static readonly float transformDelay = 0.5f;

	private static ItemIndex[] originalToTransformed = Array.Empty<ItemIndex>();

	private static bool[] itemsToCheck = Array.Empty<bool>();

	private static TransformationInfo[] _transformationInfos = Array.Empty<TransformationInfo>();

	public static ReadOnlyArray<TransformationInfo> transformationInfos = new ReadOnlyArray<TransformationInfo>(_transformationInfos);

	private static List<InventoryReplacementCandidate> pendingChanges = new List<InventoryReplacementCandidate>();

	[SystemInitializer(new Type[] { typeof(ItemCatalog) })]
	private static void Init()
	{
		InitTransformationTable();
		Inventory.onInventoryChangedGlobal += OnInventoryChangedGlobal;
		RoR2Application.onFixedUpdate += StaticFixedUpdate;
	}

	private static void StaticFixedUpdate()
	{
		if (pendingChanges.Count > 0)
		{
			ProcessPendingChanges();
		}
	}

	private static void InitTransformationTable()
	{
		//IL_0031: Unknown result type (might be due to invalid IL or missing references)
		//IL_0036: Unknown result type (might be due to invalid IL or missing references)
		//IL_0039: Unknown result type (might be due to invalid IL or missing references)
		//IL_003e: Unknown result type (might be due to invalid IL or missing references)
		//IL_00e5: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ea: Unknown result type (might be due to invalid IL or missing references)
		originalToTransformed = new ItemIndex[ItemCatalog.itemCount];
		ItemIndex[] array = originalToTransformed;
		ItemIndex itemIndex = ItemIndex.None;
		ArrayUtils.SetAll<ItemIndex>(array, ref itemIndex);
		itemsToCheck = new bool[ItemCatalog.itemCount];
		Enumerator<ItemDef.Pair> enumerator = ItemCatalog.GetItemPairsForRelationship(DLC1Content.ItemRelationshipTypes.ContagiousItem).GetEnumerator();
		try
		{
			while (enumerator.MoveNext())
			{
				ItemDef.Pair current = enumerator.Current;
				originalToTransformed[(int)current.itemDef1.itemIndex] = current.itemDef2.itemIndex;
				itemsToCheck[(int)current.itemDef1.itemIndex] = true;
				itemsToCheck[(int)current.itemDef2.itemIndex] = true;
				TransformationInfo transformationInfo = new TransformationInfo
				{
					originalItem = current.itemDef1.itemIndex,
					transformedItem = current.itemDef2.itemIndex
				};
				ArrayUtils.ArrayAppend<TransformationInfo>(ref _transformationInfos, ref transformationInfo);
			}
		}
		finally
		{
			((IDisposable)enumerator).Dispose();
		}
		transformationInfos = new ReadOnlyArray<TransformationInfo>(_transformationInfos);
	}

	public static ItemIndex GetTransformedItemIndex(ItemIndex itemIndex)
	{
		if (itemIndex >= (ItemIndex)0 && (int)itemIndex < originalToTransformed.Length)
		{
			return originalToTransformed[(int)itemIndex];
		}
		return ItemIndex.None;
	}

	public static ItemIndex GetOriginalItemIndex(ItemIndex transformedItemIndex)
	{
		TransformationInfo[] array = _transformationInfos;
		for (int i = 0; i < array.Length; i++)
		{
			TransformationInfo transformationInfo = array[i];
			if (transformedItemIndex == transformationInfo.transformedItem)
			{
				return transformationInfo.originalItem;
			}
		}
		return ItemIndex.None;
	}

	private static int FindInventoryReplacementCandidateIndex(Inventory inventory, ItemIndex originalItem)
	{
		for (int i = 0; i < pendingChanges.Count; i++)
		{
			InventoryReplacementCandidate inventoryReplacementCandidate = pendingChanges[i];
			if (inventoryReplacementCandidate.inventory == inventory && inventoryReplacementCandidate.originalItem == originalItem)
			{
				return i;
			}
		}
		return -1;
	}

	private static void ProcessPendingChanges()
	{
		if (!NetworkServer.active || !Object.op_Implicit((Object)(object)Run.instance))
		{
			pendingChanges.Clear();
			return;
		}
		for (int num = pendingChanges.Count - 1; num >= 0; num--)
		{
			InventoryReplacementCandidate value = pendingChanges[num];
			if (value.time.hasPassed)
			{
				if (!StepInventoryInfection(value.inventory, value.originalItem, int.MaxValue, value.isForced))
				{
					pendingChanges.RemoveAt(num);
				}
				else
				{
					value.time = Run.FixedTimeStamp.now + transformDelay;
					pendingChanges[num] = value;
				}
			}
		}
	}

	private static void OnInventoryChangedGlobal(Inventory inventory)
	{
		if (!NetworkServer.active)
		{
			return;
		}
		for (int i = 0; i < _transformationInfos.Length; i++)
		{
			ref TransformationInfo reference = ref _transformationInfos[i];
			if (inventory.GetItemCount(reference.transformedItem) > 0)
			{
				TryQueueReplacement(inventory, reference.originalItem, reference.transformedItem, isForced: false);
			}
		}
	}

	private static void TryQueueReplacement(Inventory inventory, ItemIndex originalItemIndex, ItemIndex transformedItemIndex, bool isForced)
	{
		if (inventory.GetItemCount(originalItemIndex) > 0 && FindInventoryReplacementCandidateIndex(inventory, transformedItemIndex) == -1)
		{
			pendingChanges.Add(new InventoryReplacementCandidate
			{
				inventory = inventory,
				originalItem = originalItemIndex,
				time = Run.FixedTimeStamp.now + transformDelay,
				isForced = isForced
			});
		}
	}

	public static void TryForceReplacement(Inventory inventory, ItemIndex originalItemIndex)
	{
		ItemIndex transformedItemIndex = GetTransformedItemIndex(originalItemIndex);
		if (transformedItemIndex != ItemIndex.None && Run.instance.IsItemAvailable(transformedItemIndex))
		{
			TryQueueReplacement(inventory, originalItemIndex, transformedItemIndex, isForced: true);
		}
	}

	private static bool StepInventoryInfection(Inventory inventory, ItemIndex originalItem, int limit, bool isForced)
	{
		ItemIndex itemIndex = originalToTransformed[(int)originalItem];
		int itemCount = inventory.GetItemCount(itemIndex);
		if (isForced || itemCount > 0)
		{
			int itemCount2 = inventory.GetItemCount(originalItem);
			int num = Math.Min(limit, itemCount2);
			if (num > 0)
			{
				inventory.RemoveItem(originalItem, num);
				inventory.GiveItem(itemIndex, num);
				CharacterMaster component = ((Component)inventory).GetComponent<CharacterMaster>();
				if (Object.op_Implicit((Object)(object)component))
				{
					CharacterMasterNotificationQueue.SendTransformNotification(component, originalItem, itemIndex, CharacterMasterNotificationQueue.TransformationType.ContagiousVoid);
				}
				return true;
			}
		}
		return false;
	}
}
