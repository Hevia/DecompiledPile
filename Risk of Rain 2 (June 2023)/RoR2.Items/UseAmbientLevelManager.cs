using System;
using System.Collections.ObjectModel;
using UnityEngine;

namespace RoR2.Items;

public class UseAmbientLevelManager
{
	[SystemInitializer(new Type[] { typeof(ItemCatalog) })]
	private static void Init()
	{
		Run.onRunAmbientLevelUp += OnRunAmbientLevelUp;
	}

	private static void OnRunAmbientLevelUp(Run run)
	{
		ReadOnlyCollection<CharacterBody> readOnlyInstancesList = CharacterBody.readOnlyInstancesList;
		int i = 0;
		for (int count = readOnlyInstancesList.Count; i < count; i++)
		{
			CharacterBody characterBody = readOnlyInstancesList[i];
			Inventory inventory = characterBody.inventory;
			if (Object.op_Implicit((Object)(object)inventory) && inventory.GetItemCount(RoR2Content.Items.UseAmbientLevel) > 0)
			{
				characterBody.MarkAllStatsDirty();
			}
		}
	}
}
