using System.Collections.Generic;
using UnityEngine;

namespace RoR2;

[CreateAssetMenu(menuName = "RoR2/DropTables/FreeChestDropTable")]
public class FreeChestDropTable : PickupDropTable
{
	[SerializeField]
	private float tier1Weight = 0.79f;

	[SerializeField]
	private float tier2Weight = 0.2f;

	[SerializeField]
	private float tier3Weight = 0.01f;

	private readonly WeightedSelection<PickupIndex> selector = new WeightedSelection<PickupIndex>();

	private void Add(List<PickupIndex> sourceDropList, float listWeight)
	{
		if (listWeight <= 0f || sourceDropList.Count == 0)
		{
			return;
		}
		float weight = listWeight / (float)sourceDropList.Count;
		foreach (PickupIndex sourceDrop in sourceDropList)
		{
			selector.AddChoice(sourceDrop, weight);
		}
	}

	protected override PickupIndex GenerateDropPreReplacement(Xoroshiro128Plus rng)
	{
		int num = 0;
		foreach (CharacterMaster readOnlyInstances in CharacterMaster.readOnlyInstancesList)
		{
			int itemCount = readOnlyInstances.inventory.GetItemCount(DLC1Content.Items.FreeChest);
			num += itemCount;
		}
		selector.Clear();
		Add(Run.instance.availableTier1DropList, tier1Weight);
		Add(Run.instance.availableTier2DropList, tier2Weight * (float)num);
		Add(Run.instance.availableTier3DropList, tier3Weight * Mathf.Pow((float)num, 2f));
		return PickupDropTable.GenerateDropFromWeightedSelection(rng, selector);
	}

	public override int GetPickupCount()
	{
		return selector.Count;
	}

	protected override PickupIndex[] GenerateUniqueDropsPreReplacement(int maxDrops, Xoroshiro128Plus rng)
	{
		return PickupDropTable.GenerateUniqueDropsFromWeightedSelection(maxDrops, rng, selector);
	}
}
