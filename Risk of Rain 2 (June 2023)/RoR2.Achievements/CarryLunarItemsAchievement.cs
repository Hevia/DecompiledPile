using UnityEngine;

namespace RoR2.Achievements;

[RegisterAchievement("CarryLunarItems", "Items.Meteor", null, null)]
public class CarryLunarItemsAchievement : BaseAchievement
{
	public const int requirement = 5;

	private PlayerCharacterMasterController currentMasterController;

	private Inventory currentInventory;

	public override void OnInstall()
	{
		base.OnInstall();
		base.localUser.onMasterChanged += OnMasterChanged;
		SetMasterController(base.localUser.cachedMasterController);
	}

	public override void OnUninstall()
	{
		SetMasterController(null);
		base.localUser.onMasterChanged -= OnMasterChanged;
		base.OnUninstall();
	}

	private void SetMasterController(PlayerCharacterMasterController newMasterController)
	{
		if (currentMasterController != newMasterController)
		{
			if (currentInventory != null)
			{
				currentInventory.onInventoryChanged -= OnInventoryChanged;
			}
			currentMasterController = newMasterController;
			currentInventory = currentMasterController?.master?.inventory;
			if (currentInventory != null)
			{
				currentInventory.onInventoryChanged += OnInventoryChanged;
			}
		}
	}

	private void OnInventoryChanged()
	{
		if (Object.op_Implicit((Object)(object)currentInventory))
		{
			int num = 5;
			EquipmentDef equipmentDef = EquipmentCatalog.GetEquipmentDef(currentInventory.currentEquipmentIndex);
			if ((Object)(object)equipmentDef != (Object)null && equipmentDef.isLunar)
			{
				num--;
			}
			EquipmentDef equipmentDef2 = EquipmentCatalog.GetEquipmentDef(currentInventory.alternateEquipmentIndex);
			if ((Object)(object)equipmentDef2 != (Object)null && equipmentDef2.isLunar)
			{
				num--;
			}
			if (currentInventory.HasAtLeastXTotalItemsOfTier(ItemTier.Lunar, num))
			{
				Grant();
			}
		}
	}

	private void OnMasterChanged()
	{
		SetMasterController(base.localUser.cachedMasterController);
	}
}
