using System;
using UnityEngine;

namespace RoR2;

[RequireComponent(typeof(Inventory))]
public class ScavengerItemGranter : MonoBehaviour
{
	[Serializable]
	public struct StackRollData
	{
		public PickupDropTable dropTable;

		public int stacks;

		public int numRolls;
	}

	public bool overwriteEquipment;

	public StackRollData[] stackRollDataList;

	private static readonly Xoroshiro128Plus rng;

	private void Start()
	{
		Inventory component = ((Component)this).GetComponent<Inventory>();
		StackRollData[] array = stackRollDataList;
		for (int i = 0; i < array.Length; i++)
		{
			StackRollData stackRollData = array[i];
			if (Object.op_Implicit((Object)(object)stackRollData.dropTable))
			{
				for (int j = 0; j < stackRollData.numRolls; j++)
				{
					PickupDef pickupDef = PickupCatalog.GetPickupDef(stackRollData.dropTable.GenerateDrop(rng));
					component.GiveItem(pickupDef.itemIndex, stackRollData.stacks);
				}
			}
		}
		if (overwriteEquipment || component.currentEquipmentIndex == EquipmentIndex.None)
		{
			component.GiveRandomEquipment(rng);
		}
	}

	static ScavengerItemGranter()
	{
		//IL_0002: Unknown result type (might be due to invalid IL or missing references)
		//IL_000c: Expected O, but got Unknown
		rng = new Xoroshiro128Plus(0uL);
		Run.onRunStartGlobal += OnRunStart;
	}

	private static void OnRunStart(Run run)
	{
		rng.ResetSeed(run.seed);
	}
}
