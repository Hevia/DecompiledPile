using UnityEngine;

namespace RoR2.Achievements.Huntress;

[RegisterAchievement("HuntressCollectCrowbars", "Skills.Huntress.MiniBlink", null, null)]
public class HuntressCollectCrowbarsAchievement : BaseAchievement
{
	private Inventory currentInventory;

	private static readonly int requirement = 12;

	protected override BodyIndex LookUpRequiredBodyIndex()
	{
		return BodyCatalog.FindBodyIndex("HuntressBody");
	}

	public override void OnInstall()
	{
		base.OnInstall();
	}

	public override void OnUninstall()
	{
		SetCurrentInventory(null);
		base.OnUninstall();
	}

	private void UpdateInventory()
	{
		Inventory inventory = null;
		if (Object.op_Implicit((Object)(object)base.localUser.cachedMasterController))
		{
			inventory = base.localUser.cachedMasterController.master.inventory;
		}
		SetCurrentInventory(inventory);
	}

	private void SetCurrentInventory(Inventory newInventory)
	{
		if (currentInventory != newInventory)
		{
			if (currentInventory != null)
			{
				currentInventory.onInventoryChanged -= OnInventoryChanged;
			}
			currentInventory = newInventory;
			if (currentInventory != null)
			{
				currentInventory.onInventoryChanged += OnInventoryChanged;
				OnInventoryChanged();
			}
		}
	}

	protected override void OnBodyRequirementMet()
	{
		base.OnBodyRequirementMet();
		base.localUser.onMasterChanged += UpdateInventory;
		UpdateInventory();
	}

	protected override void OnBodyRequirementBroken()
	{
		base.localUser.onMasterChanged -= UpdateInventory;
		SetCurrentInventory(null);
		base.OnBodyRequirementBroken();
	}

	private void OnInventoryChanged()
	{
		if (requirement <= currentInventory.GetItemCount(RoR2Content.Items.Crowbar))
		{
			Grant();
		}
	}
}
