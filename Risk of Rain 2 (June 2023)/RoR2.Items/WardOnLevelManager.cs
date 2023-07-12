using System;
using UnityEngine;
using UnityEngine.Networking;

namespace RoR2.Items;

public static class WardOnLevelManager
{
	private static GameObject wardPrefab;

	[SystemInitializer(new Type[] { typeof(ItemCatalog) })]
	private static void Init()
	{
		GlobalEventManager.onCharacterLevelUp += OnCharacterLevelUp;
		wardPrefab = LegacyResourcesAPI.Load<GameObject>("Prefabs/NetworkedObjects/WarbannerWard");
	}

	private static void OnCharacterLevelUp(CharacterBody characterBody)
	{
		//IL_0032: Unknown result type (might be due to invalid IL or missing references)
		//IL_0037: Unknown result type (might be due to invalid IL or missing references)
		if (!NetworkServer.active)
		{
			return;
		}
		Inventory inventory = characterBody.inventory;
		if (Object.op_Implicit((Object)(object)inventory))
		{
			int itemCount = inventory.GetItemCount(RoR2Content.Items.WardOnLevel);
			if (itemCount > 0)
			{
				GameObject obj = Object.Instantiate<GameObject>(wardPrefab, ((Component)characterBody).transform.position, Quaternion.identity);
				obj.GetComponent<TeamFilter>().teamIndex = characterBody.teamComponent.teamIndex;
				obj.GetComponent<BuffWard>().Networkradius = 8f + 8f * (float)itemCount;
				NetworkServer.Spawn(obj);
			}
		}
	}
}
