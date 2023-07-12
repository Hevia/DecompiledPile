using System;
using UnityEngine;
using UnityEngine.Networking;

namespace RoR2.Items;

public static class CrippleWardOnLevelManager
{
	private static GameObject wardPrefab;

	[SystemInitializer(new Type[] { typeof(ItemCatalog) })]
	private static void Init()
	{
		Run.onRunAmbientLevelUp += onRunAmbientLevelUp;
		wardPrefab = LegacyResourcesAPI.Load<GameObject>("Prefabs/NetworkedObjects/CrippleWard");
	}

	private static void onRunAmbientLevelUp(Run run)
	{
		//IL_004b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0050: Unknown result type (might be due to invalid IL or missing references)
		if (!NetworkServer.active)
		{
			return;
		}
		foreach (CharacterMaster readOnlyInstances in CharacterMaster.readOnlyInstancesList)
		{
			int itemCount = readOnlyInstances.inventory.GetItemCount(RoR2Content.Items.CrippleWardOnLevel);
			if (itemCount > 0)
			{
				CharacterBody body = readOnlyInstances.GetBody();
				if (Object.op_Implicit((Object)(object)body))
				{
					GameObject obj = Object.Instantiate<GameObject>(wardPrefab, ((Component)body).transform.position, Quaternion.identity);
					obj.GetComponent<BuffWard>().Networkradius = 8f + 8f * (float)itemCount;
					NetworkServer.Spawn(obj);
				}
			}
		}
	}
}
