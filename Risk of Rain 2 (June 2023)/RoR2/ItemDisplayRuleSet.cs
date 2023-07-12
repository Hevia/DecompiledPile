using System;
using System.Collections.Generic;
using HG;
using UnityEngine;

namespace RoR2;

[CreateAssetMenu(menuName = "RoR2/ItemDisplayRuleSet")]
public class ItemDisplayRuleSet : ScriptableObject
{
	[Serializable]
	[Obsolete]
	public struct NamedRuleGroup
	{
		public string name;

		public DisplayRuleGroup displayRuleGroup;
	}

	[Serializable]
	public struct KeyAssetRuleGroup
	{
		public Object keyAsset;

		public DisplayRuleGroup displayRuleGroup;
	}

	[SerializeField]
	[Obsolete("Use .assetRuleGroups instead.")]
	private NamedRuleGroup[] namedItemRuleGroups = Array.Empty<NamedRuleGroup>();

	[SerializeField]
	[Obsolete("Use .assetRuleGroups instead.")]
	private NamedRuleGroup[] namedEquipmentRuleGroups = Array.Empty<NamedRuleGroup>();

	public KeyAssetRuleGroup[] keyAssetRuleGroups = Array.Empty<KeyAssetRuleGroup>();

	private DisplayRuleGroup[] runtimeItemRuleGroups;

	private DisplayRuleGroup[] runtimeEquipmentRuleGroups;

	private static readonly List<ItemDisplayRuleSet> instancesList = new List<ItemDisplayRuleSet>();

	private static bool runtimeDependenciesReady = false;

	private bool hasObsoleteNamedRuleGroups
	{
		get
		{
			if (namedItemRuleGroups.Length == 0)
			{
				return namedEquipmentRuleGroups.Length != 0;
			}
			return true;
		}
	}

	public bool isEmpty => keyAssetRuleGroups.Length == 0;

	[SystemInitializer(new Type[]
	{
		typeof(ItemCatalog),
		typeof(EquipmentCatalog)
	})]
	private static void Init()
	{
		runtimeDependenciesReady = true;
		for (int i = 0; i < instancesList.Count; i++)
		{
			instancesList[i].GenerateRuntimeValues();
		}
	}

	private void OnEnable()
	{
		instancesList.Add(this);
		if (runtimeDependenciesReady)
		{
			GenerateRuntimeValues();
		}
	}

	private void OnDisable()
	{
		instancesList.Remove(this);
	}

	private void OnValidate()
	{
		if (hasObsoleteNamedRuleGroups)
		{
			Debug.LogWarningFormat((Object)(object)this, "ItemDisplayRuleSet \"{0}\" still defines one or more entries in an obsolete format. Run the upgrade from the inspector context menu.", new object[1] { this });
		}
	}

	public void GenerateRuntimeValues()
	{
		runtimeItemRuleGroups = ItemCatalog.GetPerItemBuffer<DisplayRuleGroup>();
		runtimeEquipmentRuleGroups = EquipmentCatalog.GetPerEquipmentBuffer<DisplayRuleGroup>();
		ArrayUtils.SetAll<DisplayRuleGroup>(runtimeItemRuleGroups, ref DisplayRuleGroup.empty);
		ArrayUtils.SetAll<DisplayRuleGroup>(runtimeEquipmentRuleGroups, ref DisplayRuleGroup.empty);
		for (int i = 0; i < keyAssetRuleGroups.Length; i++)
		{
			ref KeyAssetRuleGroup reference = ref keyAssetRuleGroups[i];
			Object keyAsset = reference.keyAsset;
			if (keyAsset == null)
			{
				continue;
			}
			if (!(keyAsset is ItemDef itemDef))
			{
				if (keyAsset is EquipmentDef equipmentDef)
				{
					EquipmentDef equipmentDef2 = equipmentDef;
					if (equipmentDef2.equipmentIndex != EquipmentIndex.None)
					{
						runtimeEquipmentRuleGroups[(int)equipmentDef2.equipmentIndex] = reference.displayRuleGroup;
					}
				}
			}
			else
			{
				ItemDef itemDef2 = itemDef;
				if (itemDef2.itemIndex != ItemIndex.None)
				{
					runtimeItemRuleGroups[(int)itemDef2.itemIndex] = reference.displayRuleGroup;
				}
			}
		}
	}

	public DisplayRuleGroup FindDisplayRuleGroup(Object keyAsset)
	{
		if (Object.op_Implicit(keyAsset))
		{
			for (int i = 0; i < keyAssetRuleGroups.Length; i++)
			{
				ref KeyAssetRuleGroup reference = ref keyAssetRuleGroups[i];
				if (reference.keyAsset == keyAsset)
				{
					return reference.displayRuleGroup;
				}
			}
		}
		return DisplayRuleGroup.empty;
	}

	public void SetDisplayRuleGroup(Object keyAsset, DisplayRuleGroup displayRuleGroup)
	{
		if (!Object.op_Implicit(keyAsset))
		{
			return;
		}
		for (int i = 0; i < keyAssetRuleGroups.Length; i++)
		{
			ref KeyAssetRuleGroup reference = ref keyAssetRuleGroups[i];
			if (reference.keyAsset == keyAsset)
			{
				reference.displayRuleGroup = displayRuleGroup;
				return;
			}
		}
		ref KeyAssetRuleGroup[] reference2 = ref keyAssetRuleGroups;
		KeyAssetRuleGroup keyAssetRuleGroup = new KeyAssetRuleGroup
		{
			keyAsset = keyAsset,
			displayRuleGroup = displayRuleGroup
		};
		ArrayUtils.ArrayAppend<KeyAssetRuleGroup>(ref reference2, ref keyAssetRuleGroup);
	}

	public DisplayRuleGroup GetItemDisplayRuleGroup(ItemIndex itemIndex)
	{
		return ArrayUtils.GetSafe<DisplayRuleGroup>(runtimeItemRuleGroups, (int)itemIndex, ref DisplayRuleGroup.empty);
	}

	public DisplayRuleGroup GetEquipmentDisplayRuleGroup(EquipmentIndex equipmentIndex)
	{
		return ArrayUtils.GetSafe<DisplayRuleGroup>(runtimeEquipmentRuleGroups, (int)equipmentIndex, ref DisplayRuleGroup.empty);
	}

	public void Reset()
	{
		keyAssetRuleGroups = Array.Empty<KeyAssetRuleGroup>();
		runtimeItemRuleGroups = Array.Empty<DisplayRuleGroup>();
		runtimeEquipmentRuleGroups = Array.Empty<DisplayRuleGroup>();
	}

	[ContextMenu("Upgrade to keying by asset")]
	public void UpdgradeToAssetKeying()
	{
		if (!Application.isPlaying)
		{
			Debug.Log((object)"Cannot run upgrade outside play mode, where catalogs are unavailable.");
		}
		if (hasObsoleteNamedRuleGroups)
		{
			List<string> list = new List<string>();
			UpgradeNamedRuleGroups(ref namedItemRuleGroups, "ItemDef", (string assetName) => (Object)(object)ItemCatalog.GetItemDef(ItemCatalog.FindItemIndex(assetName)), list);
			UpgradeNamedRuleGroups(ref namedEquipmentRuleGroups, "EquipmentDef", (string assetName) => (Object)(object)EquipmentCatalog.GetEquipmentDef(EquipmentCatalog.FindEquipmentIndex(assetName)), list);
			if (list.Count > 0)
			{
				Debug.LogWarningFormat("Encountered {0} errors attempting to upgrade ItemDisplayRuleSet \"{1}\":\n{2}", new object[3]
				{
					list.Count,
					((Object)this).name,
					string.Join("\n", list)
				});
			}
			EditorUtil.SetDirty((Object)(object)this);
		}
	}

	private void UpgradeNamedRuleGroups(ref NamedRuleGroup[] namedRuleGroups, string assetTypeName, Func<string, Object> assetLookupMethod, List<string> failureMessagesList)
	{
		int num = namedRuleGroups.Length;
		for (int i = 0; i < num; i++)
		{
			NamedRuleGroup namedRuleGroup = namedRuleGroups[i];
			Object val = assetLookupMethod(namedRuleGroup.name);
			string text = null;
			if (!namedRuleGroup.displayRuleGroup.isEmpty)
			{
				if (Object.op_Implicit(val))
				{
					if (FindDisplayRuleGroup(val).isEmpty)
					{
						SetDisplayRuleGroup(val, namedRuleGroup.displayRuleGroup);
					}
					else
					{
						text = "Conflicts with existing rule group.";
					}
				}
				else
				{
					text = "Named asset not found.";
				}
			}
			if (text != null)
			{
				failureMessagesList.Add(assetTypeName + " \"" + namedRuleGroup.name + "\": " + text);
			}
			else
			{
				ArrayUtils.ArrayRemoveAt<NamedRuleGroup>(namedRuleGroups, ref num, i, 1);
				i--;
			}
		}
		Array.Resize(ref namedRuleGroups, num);
	}
}
