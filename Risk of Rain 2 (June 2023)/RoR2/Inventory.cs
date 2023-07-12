using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using HG;
using JetBrains.Annotations;
using RoR2.Networking;
using UnityEngine;
using UnityEngine.Networking;

namespace RoR2;

public class Inventory : NetworkBehaviour
{
	private int[] itemStacks = ItemCatalog.RequestItemStackArray();

	public readonly List<ItemIndex> itemAcquisitionOrder = new List<ItemIndex>();

	private const uint itemListDirtyBit = 1u;

	private const uint infusionBonusDirtyBit = 4u;

	private const uint itemAcquisitionOrderDirtyBit = 8u;

	private const uint equipmentDirtyBit = 16u;

	private const uint allDirtyBits = 29u;

	private EquipmentState[] equipmentStateSlots = Array.Empty<EquipmentState>();

	public static readonly Func<ItemIndex, bool> defaultItemCopyFilterDelegate;

	private static int kRpcRpcItemAdded;

	public EquipmentIndex currentEquipmentIndex => currentEquipmentState.equipmentIndex;

	public EquipmentState currentEquipmentState => GetEquipment(activeEquipmentSlot);

	public EquipmentIndex alternateEquipmentIndex => alternateEquipmentState.equipmentIndex;

	public EquipmentState alternateEquipmentState
	{
		get
		{
			for (uint num = 0u; num < GetEquipmentSlotCount(); num++)
			{
				if (num != activeEquipmentSlot)
				{
					return GetEquipment(num);
				}
			}
			return EquipmentState.empty;
		}
	}

	public byte activeEquipmentSlot { get; private set; }

	public uint infusionBonus { get; private set; }

	private bool spawnedOverNetwork => ((NetworkBehaviour)this).isServer;

	public event Action onInventoryChanged;

	public event Action onEquipmentExternalRestockServer;

	public static event Action<Inventory> onInventoryChangedGlobal;

	public static event Action<Inventory, ItemIndex, int> onServerItemGiven;

	public event Action<ItemIndex> onItemAddedClient;

	private bool SetEquipmentInternal(EquipmentState equipmentState, uint slot)
	{
		if (Run.instance.IsEquipmentExpansionLocked(equipmentState.equipmentIndex))
		{
			return false;
		}
		if (equipmentStateSlots.Length <= slot)
		{
			int num = equipmentStateSlots.Length;
			Array.Resize(ref equipmentStateSlots, (int)(slot + 1));
			for (int i = num; i < equipmentStateSlots.Length; i++)
			{
				equipmentStateSlots[i] = EquipmentState.empty;
			}
		}
		if (equipmentStateSlots[slot].Equals(equipmentState))
		{
			return false;
		}
		equipmentStateSlots[slot] = equipmentState;
		return true;
	}

	[Server]
	public void SetEquipment(EquipmentState equipmentState, uint slot)
	{
		if (!NetworkServer.active)
		{
			Debug.LogWarning((object)"[Server] function 'System.Void RoR2.Inventory::SetEquipment(RoR2.EquipmentState,System.UInt32)' called on client");
		}
		else if (SetEquipmentInternal(equipmentState, slot))
		{
			if (NetworkServer.active)
			{
				((NetworkBehaviour)this).SetDirtyBit(16u);
			}
			HandleInventoryChanged();
		}
	}

	public EquipmentState GetEquipment(uint slot)
	{
		if (slot >= equipmentStateSlots.Length)
		{
			return EquipmentState.empty;
		}
		return equipmentStateSlots[slot];
	}

	[Server]
	public void SetActiveEquipmentSlot(byte slotIndex)
	{
		if (!NetworkServer.active)
		{
			Debug.LogWarning((object)"[Server] function 'System.Void RoR2.Inventory::SetActiveEquipmentSlot(System.Byte)' called on client");
			return;
		}
		activeEquipmentSlot = slotIndex;
		((NetworkBehaviour)this).SetDirtyBit(16u);
		HandleInventoryChanged();
	}

	public int GetEquipmentSlotCount()
	{
		return equipmentStateSlots.Length;
	}

	[Server]
	public void SetEquipmentIndex(EquipmentIndex newEquipmentIndex)
	{
		if (!NetworkServer.active)
		{
			Debug.LogWarning((object)"[Server] function 'System.Void RoR2.Inventory::SetEquipmentIndex(RoR2.EquipmentIndex)' called on client");
		}
		else
		{
			SetEquipmentIndexForSlot(newEquipmentIndex, activeEquipmentSlot);
		}
	}

	[Server]
	public void SetEquipmentIndexForSlot(EquipmentIndex newEquipmentIndex, uint slot)
	{
		if (!NetworkServer.active)
		{
			Debug.LogWarning((object)"[Server] function 'System.Void RoR2.Inventory::SetEquipmentIndexForSlot(RoR2.EquipmentIndex,System.UInt32)' called on client");
		}
		else
		{
			if (Run.instance.IsEquipmentExpansionLocked(newEquipmentIndex))
			{
				return;
			}
			EquipmentState equipment = GetEquipment(slot);
			if (equipment.equipmentIndex != newEquipmentIndex)
			{
				byte charges = equipment.charges;
				Run.FixedTimeStamp chargeFinishTime = equipment.chargeFinishTime;
				if (equipment.equipmentIndex == EquipmentIndex.None && chargeFinishTime.isNegativeInfinity)
				{
					charges = 1;
					chargeFinishTime = Run.FixedTimeStamp.now;
				}
				EquipmentState equipmentState = new EquipmentState(newEquipmentIndex, chargeFinishTime, charges);
				SetEquipment(equipmentState, slot);
			}
		}
	}

	public EquipmentIndex GetEquipmentIndex()
	{
		return currentEquipmentIndex;
	}

	[Server]
	public void DeductEquipmentCharges(byte slot, int deduction)
	{
		if (!NetworkServer.active)
		{
			Debug.LogWarning((object)"[Server] function 'System.Void RoR2.Inventory::DeductEquipmentCharges(System.Byte,System.Int32)' called on client");
			return;
		}
		EquipmentState equipment = GetEquipment(slot);
		byte charges = equipment.charges;
		Run.FixedTimeStamp chargeFinishTime = equipment.chargeFinishTime;
		charges = (byte)((charges >= deduction) ? ((byte)(charges - (byte)deduction)) : 0);
		SetEquipment(new EquipmentState(equipment.equipmentIndex, chargeFinishTime, charges), slot);
		UpdateEquipment();
	}

	public int GetEquipmentRestockableChargeCount(byte slot)
	{
		EquipmentState equipment = GetEquipment(slot);
		if (equipment.equipmentIndex == EquipmentIndex.None)
		{
			return 0;
		}
		return HGMath.ByteSafeSubtract((byte)GetEquipmentSlotMaxCharges(slot), equipment.charges);
	}

	[Server]
	public void RestockEquipmentCharges(byte slot, int amount)
	{
		if (!NetworkServer.active)
		{
			Debug.LogWarning((object)"[Server] function 'System.Void RoR2.Inventory::RestockEquipmentCharges(System.Byte,System.Int32)' called on client");
			return;
		}
		amount = Math.Min(amount, GetEquipmentRestockableChargeCount(slot));
		if (amount > 0)
		{
			EquipmentState equipment = GetEquipment(slot);
			byte charges = HGMath.ByteSafeAdd(equipment.charges, (byte)Math.Min(amount, 255));
			SetEquipment(new EquipmentState(equipment.equipmentIndex, equipment.chargeFinishTime, charges), slot);
			this.onEquipmentExternalRestockServer?.Invoke();
		}
	}

	[Server]
	public void DeductActiveEquipmentCooldown(float seconds)
	{
		if (!NetworkServer.active)
		{
			Debug.LogWarning((object)"[Server] function 'System.Void RoR2.Inventory::DeductActiveEquipmentCooldown(System.Single)' called on client");
			return;
		}
		EquipmentState equipment = GetEquipment(activeEquipmentSlot);
		SetEquipment(new EquipmentState(equipment.equipmentIndex, equipment.chargeFinishTime - seconds, equipment.charges), activeEquipmentSlot);
	}

	public int GetEquipmentSlotMaxCharges(byte slot)
	{
		return Math.Min(1 + GetItemCount(RoR2Content.Items.EquipmentMagazine), 255);
	}

	public int GetActiveEquipmentMaxCharges()
	{
		return GetEquipmentSlotMaxCharges(activeEquipmentSlot);
	}

	[Server]
	private void UpdateEquipment()
	{
		if (!NetworkServer.active)
		{
			Debug.LogWarning((object)"[Server] function 'System.Void RoR2.Inventory::UpdateEquipment()' called on client");
			return;
		}
		Run.FixedTimeStamp now = Run.FixedTimeStamp.now;
		byte b = (byte)Mathf.Min(1 + GetItemCount(RoR2Content.Items.EquipmentMagazine), 255);
		for (uint num = 0u; num < equipmentStateSlots.Length; num++)
		{
			EquipmentState equipmentState = equipmentStateSlots[num];
			if (equipmentState.equipmentIndex == EquipmentIndex.None)
			{
				continue;
			}
			if (equipmentState.charges < b)
			{
				Run.FixedTimeStamp fixedTimeStamp = equipmentState.chargeFinishTime;
				byte b2 = equipmentState.charges;
				if (!fixedTimeStamp.isPositiveInfinity)
				{
					b2 = (byte)(b2 + 1);
				}
				if (fixedTimeStamp.isInfinity)
				{
					fixedTimeStamp = now;
				}
				if (fixedTimeStamp.hasPassed)
				{
					float num2 = equipmentState.equipmentDef.cooldown * CalculateEquipmentCooldownScale();
					SetEquipment(new EquipmentState(equipmentState.equipmentIndex, fixedTimeStamp + num2, b2), num);
				}
			}
			if (equipmentState.charges >= b && !equipmentState.chargeFinishTime.isPositiveInfinity)
			{
				SetEquipment(new EquipmentState(equipmentState.equipmentIndex, Run.FixedTimeStamp.positiveInfinity, b), num);
			}
		}
	}

	private float CalculateEquipmentCooldownScale()
	{
		int itemCount = GetItemCount(RoR2Content.Items.EquipmentMagazine);
		int itemCount2 = GetItemCount(RoR2Content.Items.AutoCastEquipment);
		int itemCount3 = GetItemCount(RoR2Content.Items.BoostEquipmentRecharge);
		float num = Mathf.Pow(0.85f, (float)itemCount);
		if (itemCount2 > 0)
		{
			num *= 0.5f * Mathf.Pow(0.85f, (float)(itemCount2 - 1));
		}
		if (itemCount3 > 0)
		{
			num *= Mathf.Pow(0.9f, (float)itemCount3);
		}
		return num;
	}

	private void HandleInventoryChanged()
	{
		this.onInventoryChanged?.Invoke();
		Inventory.onInventoryChangedGlobal?.Invoke(this);
	}

	private void FixedUpdate()
	{
		if (NetworkServer.active)
		{
			UpdateEquipment();
		}
	}

	[Server]
	public void AddInfusionBonus(uint value)
	{
		if (!NetworkServer.active)
		{
			Debug.LogWarning((object)"[Server] function 'System.Void RoR2.Inventory::AddInfusionBonus(System.UInt32)' called on client");
		}
		else if (value != 0)
		{
			infusionBonus += value;
			((NetworkBehaviour)this).SetDirtyBit(4u);
			HandleInventoryChanged();
		}
	}

	[Server]
	public void GiveItemString(string itemString)
	{
		if (!NetworkServer.active)
		{
			Debug.LogWarning((object)"[Server] function 'System.Void RoR2.Inventory::GiveItemString(System.String)' called on client");
		}
		else
		{
			GiveItem(ItemCatalog.FindItemIndex(itemString));
		}
	}

	[Server]
	public void GiveItemString(string itemString, int count)
	{
		if (!NetworkServer.active)
		{
			Debug.LogWarning((object)"[Server] function 'System.Void RoR2.Inventory::GiveItemString(System.String,System.Int32)' called on client");
		}
		else
		{
			GiveItem(ItemCatalog.FindItemIndex(itemString), count);
		}
	}

	[Server]
	public void GiveEquipmentString(string equipmentString)
	{
		if (!NetworkServer.active)
		{
			Debug.LogWarning((object)"[Server] function 'System.Void RoR2.Inventory::GiveEquipmentString(System.String)' called on client");
		}
		else
		{
			SetEquipmentIndex(EquipmentCatalog.FindEquipmentIndex(equipmentString));
		}
	}

	[Server]
	public void GiveRandomItems(int count, bool lunarEnabled, bool voidEnabled)
	{
		if (!NetworkServer.active)
		{
			Debug.LogWarning((object)"[Server] function 'System.Void RoR2.Inventory::GiveRandomItems(System.Int32,System.Boolean,System.Boolean)' called on client");
			return;
		}
		try
		{
			if (count > 0)
			{
				WeightedSelection<List<PickupIndex>> weightedSelection = new WeightedSelection<List<PickupIndex>>();
				weightedSelection.AddChoice(Run.instance.availableTier1DropList, 100f);
				weightedSelection.AddChoice(Run.instance.availableTier2DropList, 60f);
				weightedSelection.AddChoice(Run.instance.availableTier3DropList, 4f);
				if (lunarEnabled)
				{
					weightedSelection.AddChoice(Run.instance.availableLunarItemDropList, 4f);
				}
				if (voidEnabled)
				{
					weightedSelection.AddChoice(Run.instance.availableVoidTier1DropList, 4f);
					weightedSelection.AddChoice(Run.instance.availableVoidTier1DropList, 2.3999999f);
					weightedSelection.AddChoice(Run.instance.availableVoidTier1DropList, 0.16f);
				}
				for (int i = 0; i < count; i++)
				{
					List<PickupIndex> list = weightedSelection.Evaluate(Random.value);
					GiveItem(PickupCatalog.GetPickupDef(list[Random.Range(0, list.Count)])?.itemIndex ?? ItemIndex.None);
				}
			}
		}
		catch (ArgumentException)
		{
		}
	}

	[Server]
	public void GiveRandomEquipment()
	{
		if (!NetworkServer.active)
		{
			Debug.LogWarning((object)"[Server] function 'System.Void RoR2.Inventory::GiveRandomEquipment()' called on client");
			return;
		}
		PickupIndex pickupIndex = Run.instance.availableEquipmentDropList[Random.Range(0, Run.instance.availableEquipmentDropList.Count)];
		SetEquipmentIndex(PickupCatalog.GetPickupDef(pickupIndex)?.equipmentIndex ?? EquipmentIndex.None);
	}

	[Server]
	public void GiveRandomEquipment(Xoroshiro128Plus rng)
	{
		if (!NetworkServer.active)
		{
			Debug.LogWarning((object)"[Server] function 'System.Void RoR2.Inventory::GiveRandomEquipment(Xoroshiro128Plus)' called on client");
			return;
		}
		PickupIndex pickupIndex = rng.NextElementUniform<PickupIndex>(Run.instance.availableEquipmentDropList);
		SetEquipmentIndex(PickupCatalog.GetPickupDef(pickupIndex)?.equipmentIndex ?? EquipmentIndex.None);
	}

	[Server]
	public void GiveItem(ItemIndex itemIndex, int count = 1)
	{
		if (!NetworkServer.active)
		{
			Debug.LogWarning((object)"[Server] function 'System.Void RoR2.Inventory::GiveItem(RoR2.ItemIndex,System.Int32)' called on client");
		}
		else
		{
			if ((Object.op_Implicit((Object)(object)Run.instance) && Run.instance.IsItemExpansionLocked(itemIndex)) || (uint)itemIndex >= itemStacks.Length)
			{
				return;
			}
			if (count <= 0)
			{
				if (count < 0)
				{
					RemoveItem(itemIndex, -count);
				}
				return;
			}
			((NetworkBehaviour)this).SetDirtyBit(1u);
			if ((itemStacks[(int)itemIndex] += count) == count)
			{
				itemAcquisitionOrder.Add(itemIndex);
				((NetworkBehaviour)this).SetDirtyBit(8u);
			}
			HandleInventoryChanged();
			Inventory.onServerItemGiven?.Invoke(this, itemIndex, count);
			if (spawnedOverNetwork)
			{
				CallRpcItemAdded(itemIndex);
			}
		}
	}

	[Server]
	public void GiveItem(ItemDef itemDef, int count = 1)
	{
		if (!NetworkServer.active)
		{
			Debug.LogWarning((object)"[Server] function 'System.Void RoR2.Inventory::GiveItem(RoR2.ItemDef,System.Int32)' called on client");
		}
		else
		{
			GiveItem(itemDef?.itemIndex ?? ItemIndex.None, count);
		}
	}

	[ClientRpc]
	private void RpcItemAdded(ItemIndex itemIndex)
	{
		this.onItemAddedClient?.Invoke(itemIndex);
	}

	[Server]
	public void RemoveItem(ItemIndex itemIndex, int count = 1)
	{
		if (!NetworkServer.active)
		{
			Debug.LogWarning((object)"[Server] function 'System.Void RoR2.Inventory::RemoveItem(RoR2.ItemIndex,System.Int32)' called on client");
		}
		else
		{
			if ((uint)itemIndex >= itemStacks.Length)
			{
				return;
			}
			if (count <= 0)
			{
				if (count < 0)
				{
					GiveItem(itemIndex, -count);
				}
				return;
			}
			int num = itemStacks[(int)itemIndex];
			count = Math.Min(count, num);
			if (count != 0)
			{
				if ((itemStacks[(int)itemIndex] = num - count) == 0)
				{
					itemAcquisitionOrder.Remove(itemIndex);
					((NetworkBehaviour)this).SetDirtyBit(8u);
				}
				((NetworkBehaviour)this).SetDirtyBit(1u);
				HandleInventoryChanged();
			}
		}
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	[Server]
	public void RemoveItem(ItemDef itemDef, int count = 1)
	{
		if (!NetworkServer.active)
		{
			Debug.LogWarning((object)"[Server] function 'System.Void RoR2.Inventory::RemoveItem(RoR2.ItemDef,System.Int32)' called on client");
		}
		else
		{
			RemoveItem(itemDef?.itemIndex ?? ItemIndex.None, count);
		}
	}

	[Server]
	public void ResetItem(ItemIndex itemIndex)
	{
		if (!NetworkServer.active)
		{
			Debug.LogWarning((object)"[Server] function 'System.Void RoR2.Inventory::ResetItem(RoR2.ItemIndex)' called on client");
		}
		else if ((uint)itemIndex < itemStacks.Length)
		{
			ref int reference = ref itemStacks[(int)itemIndex];
			if (reference > 0)
			{
				reference = 0;
				((NetworkBehaviour)this).SetDirtyBit(1u);
				((NetworkBehaviour)this).SetDirtyBit(8u);
				HandleInventoryChanged();
			}
		}
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	[Server]
	public void ResetItem(ItemDef itemDef)
	{
		if (!NetworkServer.active)
		{
			Debug.LogWarning((object)"[Server] function 'System.Void RoR2.Inventory::ResetItem(RoR2.ItemDef)' called on client");
		}
		else
		{
			ResetItem(itemDef?.itemIndex ?? ItemIndex.None);
		}
	}

	[Server]
	public void CopyEquipmentFrom(Inventory other)
	{
		if (!NetworkServer.active)
		{
			Debug.LogWarning((object)"[Server] function 'System.Void RoR2.Inventory::CopyEquipmentFrom(RoR2.Inventory)' called on client");
			return;
		}
		for (int i = 0; i < other.equipmentStateSlots.Length; i++)
		{
			SetEquipment(new EquipmentState(other.equipmentStateSlots[i].equipmentIndex, Run.FixedTimeStamp.negativeInfinity, 1), (uint)i);
		}
	}

	private static bool DefaultItemCopyFilter(ItemIndex itemIndex)
	{
		return !ItemCatalog.GetItemDef(itemIndex).ContainsTag(ItemTag.CannotCopy);
	}

	[Server]
	public void AddItemsFrom([NotNull] Inventory other)
	{
		if (!NetworkServer.active)
		{
			Debug.LogWarning((object)"[Server] function 'System.Void RoR2.Inventory::AddItemsFrom(RoR2.Inventory)' called on client");
		}
		else
		{
			AddItemsFrom(other, defaultItemCopyFilterDelegate);
		}
	}

	[Server]
	public void AddItemsFrom([NotNull] Inventory other, [NotNull] Func<ItemIndex, bool> filter)
	{
		if (!NetworkServer.active)
		{
			Debug.LogWarning((object)"[Server] function 'System.Void RoR2.Inventory::AddItemsFrom(RoR2.Inventory,System.Func`2<RoR2.ItemIndex,System.Boolean>)' called on client");
		}
		else
		{
			AddItemsFrom(other.itemStacks, filter);
		}
	}

	[Server]
	public void AddItemsFrom([NotNull] int[] otherItemStacks, [NotNull] Func<ItemIndex, bool> filter)
	{
		if (!NetworkServer.active)
		{
			Debug.LogWarning((object)"[Server] function 'System.Void RoR2.Inventory::AddItemsFrom(System.Int32[],System.Func`2<RoR2.ItemIndex,System.Boolean>)' called on client");
			return;
		}
		for (ItemIndex itemIndex = (ItemIndex)0; (int)itemIndex < itemStacks.Length; itemIndex++)
		{
			int num = otherItemStacks[(int)itemIndex];
			if (num > 0 && filter(itemIndex))
			{
				ref int reference = ref itemStacks[(int)itemIndex];
				if (reference == 0)
				{
					itemAcquisitionOrder.Add(itemIndex);
				}
				reference += num;
			}
		}
		((NetworkBehaviour)this).SetDirtyBit(1u);
		((NetworkBehaviour)this).SetDirtyBit(8u);
		HandleInventoryChanged();
	}

	[Server]
	private void AddItemAcquisitionOrderFrom([NotNull] List<ItemIndex> otherItemAcquisitionOrder)
	{
		if (!NetworkServer.active)
		{
			Debug.LogWarning((object)"[Server] function 'System.Void RoR2.Inventory::AddItemAcquisitionOrderFrom(System.Collections.Generic.List`1<RoR2.ItemIndex>)' called on client");
			return;
		}
		int[] array = ItemCatalog.RequestItemStackArray();
		for (int i = 0; i < itemAcquisitionOrder.Count; i++)
		{
			ItemIndex itemIndex = itemAcquisitionOrder[i];
			array[(int)itemIndex] = 1;
		}
		int j = 0;
		for (int count = otherItemAcquisitionOrder.Count; j < count; j++)
		{
			ItemIndex itemIndex2 = otherItemAcquisitionOrder[j];
			ref int reference = ref array[(int)itemIndex2];
			if (reference == 0)
			{
				reference = 1;
				itemAcquisitionOrder.Add(itemIndex2);
			}
		}
		ItemCatalog.ReturnItemStackArray(array);
	}

	[Server]
	public void CopyItemsFrom([NotNull] Inventory other)
	{
		if (!NetworkServer.active)
		{
			Debug.LogWarning((object)"[Server] function 'System.Void RoR2.Inventory::CopyItemsFrom(RoR2.Inventory)' called on client");
		}
		else
		{
			CopyItemsFrom(other, defaultItemCopyFilterDelegate);
		}
	}

	[Server]
	public void CopyItemsFrom([NotNull] Inventory other, [NotNull] Func<ItemIndex, bool> filter)
	{
		if (!NetworkServer.active)
		{
			Debug.LogWarning((object)"[Server] function 'System.Void RoR2.Inventory::CopyItemsFrom(RoR2.Inventory,System.Func`2<RoR2.ItemIndex,System.Boolean>)' called on client");
			return;
		}
		itemAcquisitionOrder.Clear();
		int[] array = itemStacks;
		int num = 0;
		ArrayUtils.SetAll<int>(array, ref num);
		AddItemsFrom(other);
	}

	[Server]
	public void ShrineRestackInventory([NotNull] Xoroshiro128Plus rng)
	{
		//IL_001d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0022: Unknown result type (might be due to invalid IL or missing references)
		//IL_0025: Unknown result type (might be due to invalid IL or missing references)
		//IL_002a: Unknown result type (might be due to invalid IL or missing references)
		if (!NetworkServer.active)
		{
			Debug.LogWarning((object)"[Server] function 'System.Void RoR2.Inventory::ShrineRestackInventory(Xoroshiro128Plus)' called on client");
			return;
		}
		List<ItemIndex> list = new List<ItemIndex>();
		bool flag = false;
		Enumerator<ItemTierDef> enumerator = ItemTierCatalog.allItemTierDefs.GetEnumerator();
		try
		{
			while (enumerator.MoveNext())
			{
				ItemTierDef current = enumerator.Current;
				if (!current.canRestack)
				{
					continue;
				}
				int num = 0;
				list.Clear();
				for (int i = 0; i < itemStacks.Length; i++)
				{
					if (itemStacks[i] > 0)
					{
						ItemIndex itemIndex = (ItemIndex)i;
						ItemDef itemDef = ItemCatalog.GetItemDef(itemIndex);
						if (current.tier == itemDef.tier)
						{
							num += itemStacks[i];
							list.Add(itemIndex);
							itemAcquisitionOrder.Remove(itemIndex);
							ResetItem(itemIndex);
						}
					}
				}
				if (list.Count > 0)
				{
					GiveItem(rng.NextElementUniform<ItemIndex>(list), num);
					flag = true;
				}
			}
		}
		finally
		{
			((IDisposable)enumerator).Dispose();
		}
		if (flag)
		{
			((NetworkBehaviour)this).SetDirtyBit(8u);
		}
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public int GetItemCount(ItemIndex itemIndex)
	{
		return ArrayUtils.GetSafe<int>(itemStacks, (int)itemIndex);
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public int GetItemCount(ItemDef itemDef)
	{
		return GetItemCount(itemDef?.itemIndex ?? ItemIndex.None);
	}

	public bool HasAtLeastXTotalItemsOfTier(ItemTier itemTier, int x)
	{
		int num = 0;
		ItemIndex itemIndex = (ItemIndex)0;
		for (ItemIndex itemCount = (ItemIndex)ItemCatalog.itemCount; itemIndex < itemCount; itemIndex++)
		{
			if (ItemCatalog.GetItemDef(itemIndex).tier == itemTier)
			{
				num += GetItemCount(itemIndex);
				if (num >= x)
				{
					return true;
				}
			}
		}
		return false;
	}

	public int GetTotalItemCountOfTier(ItemTier itemTier)
	{
		int num = 0;
		ItemIndex itemIndex = (ItemIndex)0;
		for (ItemIndex itemCount = (ItemIndex)ItemCatalog.itemCount; itemIndex < itemCount; itemIndex++)
		{
			if (ItemCatalog.GetItemDef(itemIndex).tier == itemTier)
			{
				num += GetItemCount(itemIndex);
			}
		}
		return num;
	}

	public void WriteItemStacks(int[] output)
	{
		Array.Copy(itemStacks, output, output.Length);
	}

	public override int GetNetworkChannel()
	{
		return QosChannelIndex.defaultReliable.intVal;
	}

	public override void OnDeserialize(NetworkReader reader, bool initialState)
	{
		byte num = reader.ReadByte();
		bool flag = (num & 1) != 0;
		bool flag2 = (num & 4) != 0;
		bool flag3 = (num & 8) != 0;
		bool flag4 = (num & 0x10) != 0;
		if (flag)
		{
			reader.ReadItemStacks(itemStacks);
		}
		if (flag2)
		{
			infusionBonus = reader.ReadPackedUInt32();
		}
		if (flag3)
		{
			uint num2 = reader.ReadPackedUInt32();
			itemAcquisitionOrder.Clear();
			itemAcquisitionOrder.Capacity = (int)num2;
			for (uint num3 = 0u; num3 < num2; num3++)
			{
				ItemIndex item = (ItemIndex)reader.ReadPackedUInt32();
				itemAcquisitionOrder.Add(item);
			}
		}
		if (flag4)
		{
			uint num4 = reader.ReadByte();
			for (uint num5 = 0u; num5 < num4; num5++)
			{
				SetEquipmentInternal(EquipmentState.Deserialize(reader), num5);
			}
			activeEquipmentSlot = reader.ReadByte();
		}
		if (flag || flag4 || flag2)
		{
			HandleInventoryChanged();
		}
	}

	public override bool OnSerialize(NetworkWriter writer, bool initialState)
	{
		uint num = ((NetworkBehaviour)this).syncVarDirtyBits;
		if (initialState)
		{
			num = 29u;
		}
		for (int i = 0; i < equipmentStateSlots.Length; i++)
		{
			if (equipmentStateSlots[i].dirty)
			{
				num |= 0x10u;
				break;
			}
		}
		bool num2 = (num & 1) != 0;
		bool flag = (num & 4) != 0;
		bool flag2 = (num & 8) != 0;
		bool flag3 = (num & 0x10) != 0;
		writer.Write((byte)num);
		if (num2)
		{
			writer.WriteItemStacks(itemStacks);
		}
		if (flag)
		{
			writer.WritePackedUInt32(infusionBonus);
		}
		if (flag2)
		{
			int count = itemAcquisitionOrder.Count;
			writer.WritePackedUInt32((uint)count);
			for (int j = 0; j < count; j++)
			{
				writer.WritePackedUInt32((uint)itemAcquisitionOrder[j]);
			}
		}
		if (flag3)
		{
			writer.Write((byte)equipmentStateSlots.Length);
			for (int k = 0; k < equipmentStateSlots.Length; k++)
			{
				EquipmentState.Serialize(writer, equipmentStateSlots[k]);
			}
			writer.Write(activeEquipmentSlot);
		}
		if (!initialState)
		{
			for (int l = 0; l < equipmentStateSlots.Length; l++)
			{
				equipmentStateSlots[l].dirty = false;
			}
		}
		if (!initialState)
		{
			return num != 0;
		}
		return false;
	}

	static Inventory()
	{
		//IL_0031: Unknown result type (might be due to invalid IL or missing references)
		//IL_003b: Expected O, but got Unknown
		defaultItemCopyFilterDelegate = DefaultItemCopyFilter;
		kRpcRpcItemAdded = 1978705787;
		NetworkBehaviour.RegisterRpcDelegate(typeof(Inventory), kRpcRpcItemAdded, new CmdDelegate(InvokeRpcRpcItemAdded));
		NetworkCRC.RegisterBehaviour("Inventory", 0);
	}

	private void UNetVersion()
	{
	}

	protected static void InvokeRpcRpcItemAdded(NetworkBehaviour obj, NetworkReader reader)
	{
		if (!NetworkClient.active)
		{
			Debug.LogError((object)"RPC RpcItemAdded called on server.");
		}
		else
		{
			((Inventory)(object)obj).RpcItemAdded((ItemIndex)reader.ReadInt32());
		}
	}

	public void CallRpcItemAdded(ItemIndex itemIndex)
	{
		//IL_0016: Unknown result type (might be due to invalid IL or missing references)
		//IL_001c: Expected O, but got Unknown
		//IL_0041: Unknown result type (might be due to invalid IL or missing references)
		if (!NetworkServer.active)
		{
			Debug.LogError((object)"RPC Function RpcItemAdded called on client.");
			return;
		}
		NetworkWriter val = new NetworkWriter();
		val.Write((short)0);
		val.Write((short)2);
		val.WritePackedUInt32((uint)kRpcRpcItemAdded);
		val.Write(((Component)this).GetComponent<NetworkIdentity>().netId);
		val.Write((int)itemIndex);
		((NetworkBehaviour)this).SendRPCInternal(val, 0, "RpcItemAdded");
	}

	public override void PreStartClient()
	{
	}
}
