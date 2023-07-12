using System;
using RoR2.ExpansionManagement;
using UnityEngine;
using UnityEngine.Serialization;

namespace RoR2;

[CreateAssetMenu(menuName = "RoR2/ItemDef")]
public class ItemDef : ScriptableObject
{
	[Serializable]
	public struct Pair : IEquatable<Pair>
	{
		public ItemDef itemDef1;

		public ItemDef itemDef2;

		public override int GetHashCode()
		{
			return ((object)itemDef1).GetHashCode() ^ ~((object)itemDef2).GetHashCode();
		}

		public override bool Equals(object obj)
		{
			if (obj is Pair)
			{
				return Equals((Pair)obj);
			}
			return false;
		}

		public bool Equals(Pair other)
		{
			if ((Object)(object)other.itemDef1 == (Object)(object)itemDef1)
			{
				return (Object)(object)other.itemDef2 == (Object)(object)itemDef2;
			}
			return false;
		}
	}

	private ItemIndex _itemIndex = ItemIndex.None;

	[SerializeField]
	[FormerlySerializedAs("tier")]
	[Obsolete("Replaced by itemTierDef field", false)]
	[Tooltip("Deprecated.  Use itemTierDef instead.")]
	private ItemTier deprecatedTier;

	[SerializeField]
	private ItemTierDef _itemTierDef;

	public string nameToken;

	public string pickupToken;

	public string descriptionToken;

	public string loreToken;

	public UnlockableDef unlockableDef;

	public GameObject pickupModelPrefab;

	public Sprite pickupIconSprite;

	public bool hidden;

	public bool canRemove = true;

	public ItemTag[] tags = Array.Empty<ItemTag>();

	public ExpansionDef requiredExpansion;

	public ItemIndex itemIndex
	{
		get
		{
			if (_itemIndex == ItemIndex.None)
			{
				Debug.LogError((object)("ItemDef '" + ((Object)this).name + "' has an item index of 'None'.  Attempting to fix..."));
				_itemIndex = ItemCatalog.FindItemIndex(((Object)this).name);
				if (_itemIndex != ItemIndex.None)
				{
					Debug.LogError((object)$"Able to fix ItemDef '{((Object)this).name}' (item index = {_itemIndex}).  This is probably because the asset is being duplicated across bundles.");
				}
			}
			return _itemIndex;
		}
		set
		{
			_itemIndex = value;
		}
	}

	public ItemTier tier
	{
		get
		{
			if (Object.op_Implicit((Object)(object)_itemTierDef))
			{
				return _itemTierDef.tier;
			}
			return deprecatedTier;
		}
		set
		{
			_itemTierDef = ItemTierCatalog.GetItemTierDef(value);
		}
	}

	[Obsolete("Get isDroppable from the ItemTierDef instead")]
	public bool inDroppableTier
	{
		get
		{
			ItemTierDef itemTierDef = ItemTierCatalog.GetItemTierDef(tier);
			if (Object.op_Implicit((Object)(object)itemTierDef))
			{
				return itemTierDef.isDroppable;
			}
			return false;
		}
	}

	public Texture pickupIconTexture
	{
		get
		{
			if (!Object.op_Implicit((Object)(object)pickupIconSprite))
			{
				return null;
			}
			return (Texture)(object)pickupIconSprite.texture;
		}
	}

	[Obsolete("Get bgIconTexture from the ItemTierDef instead")]
	public Texture bgIconTexture => ItemTierCatalog.GetItemTierDef(tier)?.bgIconTexture;

	[Obsolete("Get colorIndex from the ItemTierDef instead")]
	public ColorCatalog.ColorIndex colorIndex => ItemTierCatalog.GetItemTierDef(tier)?.colorIndex ?? ColorCatalog.ColorIndex.Unaffordable;

	[Obsolete("Get darkColorIndex from the ItemTierDef instead")]
	public ColorCatalog.ColorIndex darkColorIndex => ItemTierCatalog.GetItemTierDef(tier)?.darkColorIndex ?? ColorCatalog.ColorIndex.Unaffordable;

	public static void AttemptGrant(ref PickupDef.GrantContext context)
	{
		context.body.inventory.GiveItem(PickupCatalog.GetPickupDef(context.controller.pickupIndex)?.itemIndex ?? ItemIndex.None);
		context.shouldDestroy = true;
		context.shouldNotify = true;
	}

	public bool ContainsTag(ItemTag tag)
	{
		if (tag == ItemTag.Any)
		{
			return true;
		}
		return Array.IndexOf(tags, tag) != -1;
	}

	public bool DoesNotContainTag(ItemTag tag)
	{
		return Array.IndexOf(tags, tag) == -1;
	}

	public virtual PickupDef CreatePickupDef()
	{
		//IL_0070: Unknown result type (might be due to invalid IL or missing references)
		//IL_0075: Unknown result type (might be due to invalid IL or missing references)
		//IL_007a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0086: Unknown result type (might be due to invalid IL or missing references)
		//IL_008b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0090: Unknown result type (might be due to invalid IL or missing references)
		ItemTierDef itemTierDef = ItemTierCatalog.GetItemTierDef(tier);
		return new PickupDef
		{
			internalName = "ItemIndex." + ((Object)this).name,
			itemIndex = itemIndex,
			itemTier = tier,
			displayPrefab = pickupModelPrefab,
			dropletDisplayPrefab = itemTierDef?.dropletDisplayPrefab,
			nameToken = nameToken,
			baseColor = Color32.op_Implicit(ColorCatalog.GetColor(colorIndex)),
			darkColor = Color32.op_Implicit(ColorCatalog.GetColor(darkColorIndex)),
			unlockableDef = unlockableDef,
			interactContextToken = "ITEM_PICKUP_CONTEXT",
			isLunar = (tier == ItemTier.Lunar),
			isBoss = (tier == ItemTier.Boss),
			iconTexture = pickupIconTexture,
			iconSprite = pickupIconSprite,
			attemptGrant = AttemptGrant
		};
	}

	[ContextMenu("Auto Populate Tokens")]
	public void AutoPopulateTokens()
	{
		string arg = ((Object)this).name.ToUpperInvariant();
		nameToken = $"ITEM_{arg}_NAME";
		pickupToken = $"ITEM_{arg}_PICKUP";
		descriptionToken = $"ITEM_{arg}_DESC";
		loreToken = $"ITEM_{arg}_LORE";
	}

	[ConCommand(commandName = "items_migrate", flags = ConVarFlags.None, helpText = "Generates ItemDef assets from the existing catalog entries.")]
	private static void CCItemsMigrate(ConCommandArgs args)
	{
		for (ItemIndex itemIndex = (ItemIndex)0; (int)itemIndex < ItemCatalog.itemCount; itemIndex++)
		{
			EditorUtil.CopyToScriptableObject<ItemDef, ItemDef>(ItemCatalog.GetItemDef(itemIndex), "Assets/RoR2/Resources/ItemDefs/");
		}
	}
}
