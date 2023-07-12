using System;
using System.Collections.Generic;
using JetBrains.Annotations;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.Serialization;
using UnityEngine.UI;

namespace RoR2.UI;

[RequireComponent(typeof(RectTransform))]
public class ItemInventoryDisplay : UIBehaviour, ILayoutElement
{
	private struct LayoutValues
	{
		public float width;

		public float height;

		public float iconSize;

		public int iconsPerRow;

		public float rowWidth;

		public int rowCount;

		public Vector3 iconLocalScale;

		public Vector3 topLeftCorner;
	}

	private RectTransform rectTransform;

	public GameObject itemIconPrefab;

	[FormerlySerializedAs("iconWidth")]
	public float itemIconPrefabWidth = 64f;

	public float maxIconWidth = 64f;

	public float maxHeight = 128f;

	public float verticalMargin = 8f;

	[HideInInspector]
	[SerializeField]
	private List<ItemIcon> itemIcons;

	private ItemIndex[] itemOrder;

	private int itemOrderCount;

	private int[] itemStacks;

	private float currentIconScale = 1f;

	private float previousWidth;

	private bool updateRequestPending;

	private Inventory inventory;

	private bool inventoryWasValid;

	private bool isUninitialized => rectTransform == null;

	public float minWidth => preferredWidth;

	public float preferredWidth
	{
		get
		{
			//IL_0006: Unknown result type (might be due to invalid IL or missing references)
			//IL_000b: Unknown result type (might be due to invalid IL or missing references)
			Rect rect = rectTransform.rect;
			return ((Rect)(ref rect)).width;
		}
	}

	public float flexibleWidth => 0f;

	public float minHeight => preferredHeight;

	public float preferredHeight { get; set; }

	public float flexibleHeight => 0f;

	public int layoutPriority => 0;

	public void SetSubscribedInventory([CanBeNull] Inventory newInventory)
	{
		if (inventory != newInventory || Object.op_Implicit((Object)(object)inventory) != inventoryWasValid)
		{
			if (inventory != null)
			{
				inventory.onInventoryChanged -= OnInventoryChanged;
				inventory = null;
			}
			inventory = newInventory;
			inventoryWasValid = Object.op_Implicit((Object)(object)inventory);
			if (Object.op_Implicit((Object)(object)inventory))
			{
				inventory.onInventoryChanged += OnInventoryChanged;
			}
			OnInventoryChanged();
		}
	}

	private void OnInventoryChanged()
	{
		if (((Behaviour)this).isActiveAndEnabled)
		{
			if (Object.op_Implicit((Object)(object)inventory))
			{
				inventory.WriteItemStacks(itemStacks);
				inventory.itemAcquisitionOrder.CopyTo(itemOrder);
				itemOrderCount = inventory.itemAcquisitionOrder.Count;
			}
			else
			{
				Array.Clear(itemStacks, 0, itemStacks.Length);
				itemOrderCount = 0;
			}
			RequestUpdateDisplay();
		}
	}

	private static bool ItemIsVisible(ItemIndex itemIndex)
	{
		ItemDef itemDef = ItemCatalog.GetItemDef(itemIndex);
		if ((Object)(object)itemDef != (Object)null)
		{
			return !itemDef.hidden;
		}
		return false;
	}

	private void AllocateIcons(int desiredItemCount)
	{
		if (desiredItemCount != itemIcons.Count)
		{
			while (itemIcons.Count > desiredItemCount)
			{
				Object.Destroy((Object)(object)((Component)itemIcons[itemIcons.Count - 1]).gameObject);
				itemIcons.RemoveAt(itemIcons.Count - 1);
			}
			CalculateLayoutValues(out var v, desiredItemCount);
			while (itemIcons.Count < desiredItemCount)
			{
				ItemIcon component = Object.Instantiate<GameObject>(itemIconPrefab, (Transform)(object)rectTransform).GetComponent<ItemIcon>();
				itemIcons.Add(component);
				LayoutIndividualIcon(in v, itemIcons.Count - 1);
			}
		}
		OnIconCountChanged();
	}

	private float CalculateIconScale(int iconCount)
	{
		//IL_0006: Unknown result type (might be due to invalid IL or missing references)
		//IL_000b: Unknown result type (might be due to invalid IL or missing references)
		Rect rect = rectTransform.rect;
		int num = (int)((Rect)(ref rect)).width;
		int num2 = (int)maxHeight;
		int num3 = (int)itemIconPrefabWidth;
		int num4 = num3;
		int num5 = num3 / 8;
		int num6 = Math.Max(num / num4, 1);
		int num7 = HGMath.IntDivCeil(iconCount, num6);
		while (num4 * num7 > num2)
		{
			num6++;
			num4 = Math.Min(num / num6, num3);
			num7 = HGMath.IntDivCeil(iconCount, num6);
			if (num4 <= num5)
			{
				num4 = num5;
				break;
			}
		}
		num4 = Math.Min(num4, (int)maxIconWidth);
		return (float)num4 / (float)num3;
	}

	private void OnIconCountChanged()
	{
		float num = CalculateIconScale(itemIcons.Count);
		if (num != currentIconScale)
		{
			currentIconScale = num;
			OnIconScaleChanged();
		}
	}

	private void OnIconScaleChanged()
	{
		LayoutAllIcons();
	}

	private void CalculateLayoutValues(out LayoutValues v, int iconCount)
	{
		//IL_0018: Unknown result type (might be due to invalid IL or missing references)
		//IL_001d: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a0: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a5: Unknown result type (might be due to invalid IL or missing references)
		//IL_00c2: Unknown result type (might be due to invalid IL or missing references)
		//IL_00c7: Unknown result type (might be due to invalid IL or missing references)
		float num = CalculateIconScale(itemIcons.Count);
		Rect rect = rectTransform.rect;
		v.width = ((Rect)(ref rect)).width;
		v.iconSize = itemIconPrefabWidth * num;
		v.iconsPerRow = Math.Max((int)v.width / (int)v.iconSize, 1);
		v.rowWidth = (float)v.iconsPerRow * v.iconSize;
		float num2 = (v.width - v.rowWidth) * 0.5f;
		v.rowCount = HGMath.IntDivCeil(itemIcons.Count, v.iconsPerRow);
		v.iconLocalScale = new Vector3(num, num, 1f);
		v.topLeftCorner = new Vector3(((Rect)(ref rect)).xMin + num2, ((Rect)(ref rect)).yMax - verticalMargin);
		v.height = v.iconSize * (float)v.rowCount + verticalMargin * 2f;
	}

	private void LayoutAllIcons()
	{
		//IL_0039: Unknown result type (might be due to invalid IL or missing references)
		//IL_003e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0083: Unknown result type (might be due to invalid IL or missing references)
		//IL_008d: Unknown result type (might be due to invalid IL or missing references)
		CalculateLayoutValues(out var v, itemIcons.Count);
		int num = itemIcons.Count - (v.rowCount - 1) * v.iconsPerRow;
		int i = 0;
		int num2 = 0;
		for (; i < v.rowCount; i++)
		{
			Vector3 topLeftCorner = v.topLeftCorner;
			topLeftCorner.y += (float)i * (0f - v.iconSize);
			int num3 = v.iconsPerRow;
			if (i == v.rowCount - 1)
			{
				num3 = num;
			}
			int num4 = 0;
			while (num4 < num3)
			{
				RectTransform obj = itemIcons[num2].rectTransform;
				((Transform)obj).localScale = v.iconLocalScale;
				((Transform)obj).localPosition = topLeftCorner;
				topLeftCorner.x += v.iconSize;
				num4++;
				num2++;
			}
		}
	}

	private void LayoutIndividualIcon(in LayoutValues layoutValues, int i)
	{
		//IL_0015: Unknown result type (might be due to invalid IL or missing references)
		//IL_001a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0056: Unknown result type (might be due to invalid IL or missing references)
		//IL_005d: Unknown result type (might be due to invalid IL or missing references)
		int num = i / layoutValues.iconsPerRow;
		int num2 = i - num * layoutValues.iconsPerRow;
		Vector3 topLeftCorner = layoutValues.topLeftCorner;
		topLeftCorner.x += (float)num2 * layoutValues.iconSize;
		topLeftCorner.y += (float)num * (0f - layoutValues.iconSize);
		RectTransform obj = itemIcons[i].rectTransform;
		((Transform)obj).localPosition = topLeftCorner;
		((Transform)obj).localScale = layoutValues.iconLocalScale;
	}

	private void CacheComponents()
	{
		//IL_0007: Unknown result type (might be due to invalid IL or missing references)
		//IL_0011: Expected O, but got Unknown
		rectTransform = (RectTransform)((Component)this).transform;
	}

	protected override void Awake()
	{
		((UIBehaviour)this).Awake();
		CacheComponents();
		itemStacks = ItemCatalog.RequestItemStackArray();
		itemOrder = ItemCatalog.RequestItemOrderBuffer();
	}

	protected override void OnDestroy()
	{
		SetSubscribedInventory(null);
		ItemCatalog.ReturnItemStackArray(itemStacks);
		itemStacks = null;
		ItemCatalog.ReturnItemOrderBuffer(itemOrder);
		itemOrder = null;
		((UIBehaviour)this).OnDestroy();
	}

	protected override void OnEnable()
	{
		((UIBehaviour)this).OnEnable();
		if (Object.op_Implicit((Object)(object)inventory))
		{
			OnInventoryChanged();
		}
		RequestUpdateDisplay();
		LayoutAllIcons();
	}

	private void RequestUpdateDisplay()
	{
		if (!updateRequestPending)
		{
			updateRequestPending = true;
			RoR2Application.onNextUpdate += UpdateDisplay;
		}
	}

	public void UpdateDisplay()
	{
		updateRequestPending = false;
		if (!Object.op_Implicit((Object)(object)this) || !((Behaviour)this).isActiveAndEnabled)
		{
			return;
		}
		ItemIndex[] array = ItemCatalog.RequestItemOrderBuffer();
		int num = 0;
		for (int i = 0; i < itemOrderCount; i++)
		{
			if (ItemIsVisible(itemOrder[i]))
			{
				array[num++] = itemOrder[i];
			}
		}
		AllocateIcons(num);
		for (int j = 0; j < num; j++)
		{
			ItemIndex itemIndex = array[j];
			itemIcons[j].SetItemIndex(itemIndex, itemStacks[(int)itemIndex]);
		}
		ItemCatalog.ReturnItemOrderBuffer(array);
	}

	public void SetItems(List<ItemIndex> newItemOrder, int[] newItemStacks)
	{
		itemOrderCount = newItemOrder.Count;
		for (int i = 0; i < itemOrderCount; i++)
		{
			itemOrder[i] = newItemOrder[i];
		}
		Array.Copy(newItemStacks, itemStacks, itemStacks.Length);
		RequestUpdateDisplay();
	}

	public void SetItems(ItemIndex[] newItemOrder, int newItemOrderCount, int[] newItemStacks)
	{
		itemOrderCount = newItemOrderCount;
		Array.Copy(newItemOrder, itemOrder, itemOrderCount);
		Array.Copy(newItemStacks, itemStacks, itemStacks.Length);
		RequestUpdateDisplay();
	}

	public void ResetItems()
	{
		itemOrderCount = 0;
		Array.Clear(itemStacks, 0, itemStacks.Length);
		RequestUpdateDisplay();
	}

	protected override void OnRectTransformDimensionsChange()
	{
		//IL_0019: Unknown result type (might be due to invalid IL or missing references)
		//IL_001e: Unknown result type (might be due to invalid IL or missing references)
		((UIBehaviour)this).OnRectTransformDimensionsChange();
		if (Object.op_Implicit((Object)(object)rectTransform))
		{
			Rect rect = rectTransform.rect;
			float width = ((Rect)(ref rect)).width;
			if (width != previousWidth)
			{
				previousWidth = width;
				LayoutAllIcons();
			}
		}
	}

	public void CalculateLayoutInputHorizontal()
	{
	}

	public void CalculateLayoutInputVertical()
	{
		if (!isUninitialized)
		{
			CalculateLayoutValues(out var v, itemIcons.Count);
			preferredHeight = v.height;
		}
	}

	protected void OnValidate()
	{
		CacheComponents();
	}
}
