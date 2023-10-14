using System.Collections.Generic;
using R2API;
using RoR2;
using UnityEngine;

namespace VileMod.Modules;

internal static class ItemDisplays
{
	private static Dictionary<string, GameObject> itemDisplayPrefabs = new Dictionary<string, GameObject>();

	internal static void PopulateDisplays()
	{
		PopulateFromBody("MageBody");
		PopulateFromBody("LunarExploderBody");
		PopulateCustomLightningArm();
	}

	private static void PopulateFromBody(string bodyName)
	{
		ItemDisplayRuleSet itemDisplayRuleSet = ((Component)LegacyResourcesAPI.Load<GameObject>("Prefabs/CharacterBodies/" + bodyName).GetComponent<ModelLocator>().modelTransform).GetComponent<CharacterModel>().itemDisplayRuleSet;
		KeyAssetRuleGroup[] keyAssetRuleGroups = itemDisplayRuleSet.keyAssetRuleGroups;
		for (int i = 0; i < keyAssetRuleGroups.Length; i++)
		{
			ItemDisplayRule[] rules = keyAssetRuleGroups[i].displayRuleGroup.rules;
			for (int j = 0; j < rules.Length; j++)
			{
				GameObject followerPrefab = rules[j].followerPrefab;
				if (Object.op_Implicit((Object)(object)followerPrefab))
				{
					string key = ((Object)followerPrefab).name?.ToLowerInvariant();
					if (!itemDisplayPrefabs.ContainsKey(key))
					{
						itemDisplayPrefabs[key] = followerPrefab;
					}
				}
			}
		}
	}

	private static void PopulateCustomLightningArm()
	{
		GameObject val = PrefabAPI.InstantiateClone(itemDisplayPrefabs["displaylightningarmright"], "DisplayLightningCustom", false);
		LimbMatcher component = val.GetComponent<LimbMatcher>();
		component.limbPairs[0].targetChildLimb = "LightningArm1";
		component.limbPairs[1].targetChildLimb = "LightningArm2";
		component.limbPairs[2].targetChildLimb = "LightningArmEnd";
		itemDisplayPrefabs["displaylightningarmcustom"] = val;
	}

	public static GameObject LoadDisplay(string name)
	{
		if (itemDisplayPrefabs.ContainsKey(name.ToLowerInvariant()) && Object.op_Implicit((Object)(object)itemDisplayPrefabs[name.ToLowerInvariant()]))
		{
			return itemDisplayPrefabs[name.ToLowerInvariant()];
		}
		Log.Error("item display " + name + " returned null");
		return null;
	}
}
