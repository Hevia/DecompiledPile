using System;
using RoR2.ExpansionManagement;
using UnityEngine;

namespace RoR2;

[CreateAssetMenu(menuName = "RoR2/EquipmentDef")]
public class EquipmentDef : ScriptableObject
{
	private EquipmentIndex _equipmentIndex = EquipmentIndex.None;

	public GameObject pickupModelPrefab;

	public float cooldown;

	public string nameToken;

	public string pickupToken;

	public string descriptionToken;

	public string loreToken;

	public Sprite pickupIconSprite;

	public UnlockableDef unlockableDef;

	public ColorCatalog.ColorIndex colorIndex = ColorCatalog.ColorIndex.Equipment;

	public bool canDrop;

	[Range(0f, 1f)]
	public float dropOnDeathChance;

	public bool canBeRandomlyTriggered = true;

	public bool enigmaCompatible;

	public bool isLunar;

	public bool isBoss;

	public BuffDef passiveBuffDef;

	public bool appearsInSinglePlayer = true;

	public bool appearsInMultiPlayer = true;

	public ExpansionDef requiredExpansion;

	[Obsolete]
	[HideInInspector]
	public MageElement mageElement;

	public EquipmentIndex equipmentIndex
	{
		get
		{
			if (_equipmentIndex == EquipmentIndex.None)
			{
				Debug.LogError((object)("EquipmentDef '" + ((Object)this).name + "' has an equipment index of 'None'.  Attempting to fix..."));
				_equipmentIndex = EquipmentCatalog.FindEquipmentIndex(((Object)this).name);
				if (_equipmentIndex != EquipmentIndex.None)
				{
					Debug.LogError((object)$"Able to fix EquipmentDef '{((Object)this).name}' (equipment index = {_equipmentIndex}).  This is probably because the asset is being duplicated across bundles.");
				}
			}
			return _equipmentIndex;
		}
		set
		{
			_equipmentIndex = value;
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

	public Texture bgIconTexture
	{
		get
		{
			if (isLunar)
			{
				return LegacyResourcesAPI.Load<Texture>("Textures/ItemIcons/BG/texLunarBGIcon");
			}
			return LegacyResourcesAPI.Load<Texture>("Textures/ItemIcons/BG/texEquipmentBGIcon");
		}
	}

	public static void AttemptGrant(ref PickupDef.GrantContext context)
	{
		//IL_00c7: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ef: Unknown result type (might be due to invalid IL or missing references)
		context.controller.StartWaitTime();
		Inventory inventory = context.body.inventory;
		EquipmentIndex currentEquipmentIndex = inventory.currentEquipmentIndex;
		EquipmentIndex equipmentIndex = PickupCatalog.GetPickupDef(context.controller.pickupIndex)?.equipmentIndex ?? EquipmentIndex.None;
		inventory.SetEquipmentIndex(equipmentIndex);
		context.controller.NetworkpickupIndex = PickupCatalog.FindPickupIndex(currentEquipmentIndex);
		context.shouldDestroy = false;
		context.shouldNotify = true;
		if (context.controller.pickupIndex == PickupIndex.none)
		{
			context.shouldDestroy = true;
		}
		if (context.controller.selfDestructIfPickupIndexIsNotIdeal && context.controller.pickupIndex != PickupCatalog.FindPickupIndex(context.controller.idealPickupIndex.pickupName))
		{
			PickupDropletController.CreatePickupDroplet(context.controller.pickupIndex, ((Component)context.controller).transform.position, new Vector3(Random.Range(-4f, 4f), 20f, Random.Range(-4f, 4f)));
			context.shouldDestroy = true;
		}
	}

	[ContextMenu("Auto Populate Tokens")]
	public void AutoPopulateTokens()
	{
		string arg = ((Object)this).name.ToUpperInvariant();
		nameToken = $"EQUIPMENT_{arg}_NAME";
		pickupToken = $"EQUIPMENT_{arg}_PICKUP";
		descriptionToken = $"EQUIPMENT_{arg}_DESC";
		loreToken = $"EQUIPMENT_{arg}_LORE";
	}

	public virtual PickupDef CreatePickupDef()
	{
		//IL_0056: Unknown result type (might be due to invalid IL or missing references)
		//IL_005b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0060: Unknown result type (might be due to invalid IL or missing references)
		//IL_0067: Unknown result type (might be due to invalid IL or missing references)
		//IL_006c: Unknown result type (might be due to invalid IL or missing references)
		PickupDef obj = new PickupDef
		{
			internalName = "EquipmentIndex." + ((Object)this).name,
			equipmentIndex = equipmentIndex,
			displayPrefab = pickupModelPrefab,
			dropletDisplayPrefab = LegacyResourcesAPI.Load<GameObject>("Prefabs/ItemPickups/EquipmentOrb"),
			nameToken = nameToken,
			baseColor = Color32.op_Implicit(ColorCatalog.GetColor(colorIndex))
		};
		obj.darkColor = obj.baseColor;
		obj.unlockableDef = unlockableDef;
		obj.interactContextToken = "EQUIPMENT_PICKUP_CONTEXT";
		obj.isLunar = isLunar;
		obj.isBoss = isBoss;
		obj.iconTexture = pickupIconTexture;
		obj.iconSprite = pickupIconSprite;
		obj.attemptGrant = AttemptGrant;
		return obj;
	}

	[ConCommand(commandName = "equipment_migrate", flags = ConVarFlags.None, helpText = "Generates EquipmentDef assets from the existing catalog entries.")]
	private static void CCEquipmentMigrate(ConCommandArgs args)
	{
		for (EquipmentIndex equipmentIndex = (EquipmentIndex)0; (int)equipmentIndex < EquipmentCatalog.equipmentCount; equipmentIndex++)
		{
			EditorUtil.CopyToScriptableObject<EquipmentDef, EquipmentDef>(EquipmentCatalog.GetEquipmentDef(equipmentIndex), "Assets/RoR2/Resources/EquipmentDefs/");
		}
	}
}
