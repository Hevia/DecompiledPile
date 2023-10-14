using System.Collections.Generic;
using RoR2;

namespace VileMod.Modules.Characters;

public abstract class ItemDisplaysBase
{
	public void SetItemDisplays(ItemDisplayRuleSet itemDisplayRuleSet)
	{
		List<KeyAssetRuleGroup> list = new List<KeyAssetRuleGroup>();
		SetItemDisplayRules(list);
		itemDisplayRuleSet.keyAssetRuleGroups = list.ToArray();
	}

	protected abstract void SetItemDisplayRules(List<KeyAssetRuleGroup> itemDisplayRules);
}
