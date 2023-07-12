using System.Collections.ObjectModel;
using RoR2.Items;
using UnityEngine;

namespace RoR2.UI;

public class ScoreboardController : MonoBehaviour
{
	public GameObject stripPrefab;

	public RectTransform container;

	[SerializeField]
	private ItemInventoryDisplay suppressedItemDisplay;

	private UIElementAllocator<ScoreboardStrip> stripAllocator;

	private void Awake()
	{
		stripAllocator = new UIElementAllocator<ScoreboardStrip>(container, stripPrefab);
	}

	private void SetStripCount(int newCount)
	{
		stripAllocator.AllocateElements(newCount);
	}

	private void Rebuild()
	{
		ReadOnlyCollection<PlayerCharacterMasterController> instances = PlayerCharacterMasterController.instances;
		int count = instances.Count;
		SetStripCount(count);
		for (int i = 0; i < count; i++)
		{
			stripAllocator.elements[i].SetMaster(instances[i].master);
		}
	}

	private void PlayerEventToRebuild(PlayerCharacterMasterController playerCharacterMasterController)
	{
		Rebuild();
	}

	private void OnEnable()
	{
		if (Object.op_Implicit((Object)(object)SuppressedItemManager.suppressedInventory))
		{
			suppressedItemDisplay?.SetSubscribedInventory(SuppressedItemManager.suppressedInventory);
			SuppressedItemManager.suppressedInventory.onInventoryChanged += OnInventoryChanged;
		}
		OnInventoryChanged();
		PlayerCharacterMasterController.onPlayerAdded += PlayerEventToRebuild;
		PlayerCharacterMasterController.onPlayerRemoved += PlayerEventToRebuild;
		Rebuild();
	}

	private void OnDisable()
	{
		if (Object.op_Implicit((Object)(object)SuppressedItemManager.suppressedInventory))
		{
			suppressedItemDisplay?.SetSubscribedInventory(null);
			SuppressedItemManager.suppressedInventory.onInventoryChanged -= OnInventoryChanged;
		}
		PlayerCharacterMasterController.onPlayerRemoved -= PlayerEventToRebuild;
		PlayerCharacterMasterController.onPlayerAdded -= PlayerEventToRebuild;
	}

	private void OnInventoryChanged()
	{
		ItemInventoryDisplay itemInventoryDisplay = suppressedItemDisplay;
		if (itemInventoryDisplay != null)
		{
			GameObject gameObject = ((Component)itemInventoryDisplay).gameObject;
			if (gameObject != null)
			{
				gameObject.SetActive(SuppressedItemManager.HasAnyItemBeenSuppressed());
			}
		}
	}
}
