using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using HG;
using RoR2.ConVar;
using RoR2.EntitlementManagement;
using RoR2.ExpansionManagement;
using UnityEngine;

namespace RoR2;

public static class RuleCatalog
{
	public enum RuleCategoryType
	{
		StripVote,
		VoteResultGrid
	}

	private static readonly List<RuleDef> allRuleDefs = new List<RuleDef>();

	private static readonly List<RuleChoiceDef> allChoicesDefs = new List<RuleChoiceDef>();

	public static readonly List<RuleCategoryDef> allCategoryDefs = new List<RuleCategoryDef>();

	public static RuleCategoryDef artifactRuleCategory;

	private static RuleChoiceDef[] _allChoiceDefsWithUnlocks = Array.Empty<RuleChoiceDef>();

	private static readonly Dictionary<string, RuleDef> ruleDefsByGlobalName = new Dictionary<string, RuleDef>();

	private static readonly Dictionary<string, RuleChoiceDef> ruleChoiceDefsByGlobalName = new Dictionary<string, RuleChoiceDef>();

	public static ResourceAvailability availability;

	private static readonly BoolConVar ruleShowItems = new BoolConVar("rule_show_items", ConVarFlags.Cheat, "0", "Whether or not to allow voting on items in the pregame rules.");

	public static IEnumerable<RuleChoiceDef> allChoiceDefsWithUnlocks => _allChoiceDefsWithUnlocks;

	public static int ruleCount => allRuleDefs.Count;

	public static int choiceCount => allChoicesDefs.Count;

	public static int categoryCount => allCategoryDefs.Count;

	public static int highestLocalChoiceCount { get; private set; }

	public static RuleDef GetRuleDef(int ruleDefIndex)
	{
		return allRuleDefs[ruleDefIndex];
	}

	public static RuleDef FindRuleDef(string ruleDefGlobalName)
	{
		ruleDefsByGlobalName.TryGetValue(ruleDefGlobalName, out var value);
		return value;
	}

	public static RuleChoiceDef FindChoiceDef(string ruleChoiceDefGlobalName)
	{
		ruleChoiceDefsByGlobalName.TryGetValue(ruleChoiceDefGlobalName, out var value);
		return value;
	}

	public static RuleChoiceDef GetChoiceDef(int ruleChoiceDefIndex)
	{
		return allChoicesDefs[ruleChoiceDefIndex];
	}

	public static RuleCategoryDef GetCategoryDef(int ruleCategoryDefIndex)
	{
		return allCategoryDefs[ruleCategoryDefIndex];
	}

	private static bool HiddenTestTrue()
	{
		return true;
	}

	private static bool HiddenTestFalse()
	{
		return false;
	}

	private static bool HiddenTestItemsConvar()
	{
		return !ruleShowItems.value;
	}

	private static RuleCategoryDef AddCategory(string displayToken, string subtitleToken, Color color)
	{
		//IL_0002: Unknown result type (might be due to invalid IL or missing references)
		return AddCategory(displayToken, subtitleToken, color, "", null, HiddenTestFalse, RuleCategoryType.StripVote);
	}

	private static RuleCategoryDef AddCategory(string displayToken, string subtitleToken, Color color, string emptyTipToken, string editToken, Func<bool> hiddenTest, RuleCategoryType ruleCategoryType)
	{
		//IL_0024: Unknown result type (might be due to invalid IL or missing references)
		//IL_0025: Unknown result type (might be due to invalid IL or missing references)
		RuleCategoryDef ruleCategoryDef = new RuleCategoryDef
		{
			position = allRuleDefs.Count,
			displayToken = displayToken,
			subtitleToken = subtitleToken,
			color = color,
			emptyTipToken = emptyTipToken,
			editToken = editToken,
			hiddenTest = hiddenTest,
			ruleCategoryType = ruleCategoryType
		};
		allCategoryDefs.Add(ruleCategoryDef);
		return ruleCategoryDef;
	}

	private static void AddRule(RuleDef ruleDef)
	{
		if (allCategoryDefs.Count > 0)
		{
			ruleDef.category = allCategoryDefs[allCategoryDefs.Count - 1];
			allCategoryDefs[allCategoryDefs.Count - 1].children.Add(ruleDef);
		}
		allRuleDefs.Add(ruleDef);
		if (highestLocalChoiceCount < ruleDef.choices.Count)
		{
			highestLocalChoiceCount = ruleDef.choices.Count;
		}
		ruleDefsByGlobalName[ruleDef.globalName] = ruleDef;
		foreach (RuleChoiceDef choice in ruleDef.choices)
		{
			ruleChoiceDefsByGlobalName[choice.globalName] = choice;
		}
	}

	[SystemInitializer(new Type[]
	{
		typeof(ItemCatalog),
		typeof(EquipmentCatalog),
		typeof(ArtifactCatalog),
		typeof(EntitlementCatalog)
	})]
	private static void Init()
	{
		//IL_0018: Unknown result type (might be due to invalid IL or missing references)
		//IL_001d: Unknown result type (might be due to invalid IL or missing references)
		//IL_004a: Unknown result type (might be due to invalid IL or missing references)
		//IL_004f: Unknown result type (might be due to invalid IL or missing references)
		//IL_006d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0072: Unknown result type (might be due to invalid IL or missing references)
		//IL_0075: Unknown result type (might be due to invalid IL or missing references)
		//IL_007a: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b3: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b8: Unknown result type (might be due to invalid IL or missing references)
		//IL_011c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0121: Unknown result type (might be due to invalid IL or missing references)
		//IL_0243: Unknown result type (might be due to invalid IL or missing references)
		//IL_0248: Unknown result type (might be due to invalid IL or missing references)
		//IL_0300: Unknown result type (might be due to invalid IL or missing references)
		//IL_0305: Unknown result type (might be due to invalid IL or missing references)
		//IL_035d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0362: Unknown result type (might be due to invalid IL or missing references)
		//IL_0367: Unknown result type (might be due to invalid IL or missing references)
		//IL_039e: Unknown result type (might be due to invalid IL or missing references)
		//IL_03a3: Unknown result type (might be due to invalid IL or missing references)
		//IL_03a8: Unknown result type (might be due to invalid IL or missing references)
		//IL_03f0: Unknown result type (might be due to invalid IL or missing references)
		//IL_03f5: Unknown result type (might be due to invalid IL or missing references)
		//IL_03fa: Unknown result type (might be due to invalid IL or missing references)
		//IL_0444: Unknown result type (might be due to invalid IL or missing references)
		//IL_0449: Unknown result type (might be due to invalid IL or missing references)
		//IL_044e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0495: Unknown result type (might be due to invalid IL or missing references)
		//IL_049a: Unknown result type (might be due to invalid IL or missing references)
		//IL_049f: Unknown result type (might be due to invalid IL or missing references)
		AddCategory("RULE_HEADER_DIFFICULTY", "", Color32.op_Implicit(new Color32((byte)43, (byte)124, (byte)181, byte.MaxValue)));
		AddRule(RuleDef.FromDifficulty());
		AddCategory("RULE_HEADER_EXPANSIONS", "RULE_HEADER_EXPANSIONS_SUBTITLE", Color32.op_Implicit(new Color32((byte)219, (byte)114, (byte)114, byte.MaxValue)), null, "RULE_HEADER_EXPANSIONS_EDIT", HiddenTestFalse, RuleCategoryType.VoteResultGrid);
		Enumerator<ExpansionDef> enumerator = ExpansionCatalog.expansionDefs.GetEnumerator();
		try
		{
			while (enumerator.MoveNext())
			{
				AddRule(RuleDef.FromExpansion(enumerator.Current));
			}
		}
		finally
		{
			((IDisposable)enumerator).Dispose();
		}
		artifactRuleCategory = AddCategory("RULE_HEADER_ARTIFACTS", "RULE_HEADER_ARTIFACTS_SUBTITLE", Color32.op_Implicit(ColorCatalog.GetColor(ColorCatalog.ColorIndex.Artifact)), "RULE_ARTIFACTS_EMPTY_TIP", "RULE_HEADER_ARTIFACTS_EDIT", HiddenTestFalse, RuleCategoryType.VoteResultGrid);
		for (ArtifactIndex artifactIndex = (ArtifactIndex)0; (int)artifactIndex < ArtifactCatalog.artifactCount; artifactIndex++)
		{
			AddRule(RuleDef.FromArtifact(artifactIndex));
		}
		AddCategory("RULE_HEADER_ITEMS", "RULE_HEADER_ITEMSANDEQUIPMENT_SUBTITLE", Color32.op_Implicit(new Color32((byte)147, (byte)225, (byte)128, byte.MaxValue)), null, "RULE_HEADER_ITEMSANDEQUIPMENT_EDIT", HiddenTestItemsConvar, RuleCategoryType.VoteResultGrid);
		List<ItemIndex> list = new List<ItemIndex>();
		ItemIndex itemIndex = (ItemIndex)0;
		for (ItemIndex itemCount = (ItemIndex)ItemCatalog.itemCount; itemIndex < itemCount; itemIndex++)
		{
			list.Add(itemIndex);
		}
		foreach (ItemIndex item in from i in list
			where ItemCatalog.GetItemDef(i).inDroppableTier
			orderby ItemCatalog.GetItemDef(i).tier
			select i)
		{
			AddRule(RuleDef.FromItem(item));
		}
		_ = from i in list
			where ItemCatalog.GetItemDef(i).inDroppableTier
			orderby ItemCatalog.GetItemDef(i).tier
			select i;
		AddCategory("RULE_HEADER_EQUIPMENT", "RULE_HEADER_ITEMSANDEQUIPMENT_SUBTITLE", Color32.op_Implicit(new Color32(byte.MaxValue, (byte)128, (byte)0, byte.MaxValue)), null, "RULE_HEADER_ITEMSANDEQUIPMENT_EDIT", HiddenTestItemsConvar, RuleCategoryType.VoteResultGrid);
		List<EquipmentIndex> list2 = new List<EquipmentIndex>();
		EquipmentIndex equipmentIndex = (EquipmentIndex)0;
		for (EquipmentIndex equipmentCount = (EquipmentIndex)EquipmentCatalog.equipmentCount; equipmentIndex < equipmentCount; equipmentIndex++)
		{
			list2.Add(equipmentIndex);
		}
		foreach (EquipmentIndex item2 in list2.Where((EquipmentIndex i) => EquipmentCatalog.GetEquipmentDef(i).canDrop))
		{
			AddRule(RuleDef.FromEquipment(item2));
		}
		AddCategory("RULE_HEADER_MISC", "", Color32.op_Implicit(new Color32((byte)192, (byte)192, (byte)192, byte.MaxValue)), null, "", HiddenTestFalse, RuleCategoryType.VoteResultGrid);
		RuleDef ruleDef = new RuleDef("Misc.StartingMoney", "RULE_MISC_STARTING_MONEY");
		RuleChoiceDef ruleChoiceDef = ruleDef.AddChoice("0", 0u, excludeByDefault: true);
		ruleChoiceDef.tooltipNameToken = "RULE_STARTINGMONEY_CHOICE_0_NAME";
		ruleChoiceDef.tooltipBodyToken = "RULE_STARTINGMONEY_CHOICE_0_DESC";
		ruleChoiceDef.tooltipNameColor = Color32.op_Implicit(ColorCatalog.GetColor(ColorCatalog.ColorIndex.LunarCoin));
		ruleChoiceDef.onlyShowInGameBrowserIfNonDefault = true;
		RuleChoiceDef ruleChoiceDef2 = ruleDef.AddChoice("15", 15u, excludeByDefault: true);
		ruleChoiceDef2.tooltipNameToken = "RULE_STARTINGMONEY_CHOICE_15_NAME";
		ruleChoiceDef2.tooltipBodyToken = "RULE_STARTINGMONEY_CHOICE_15_DESC";
		ruleChoiceDef2.tooltipNameColor = Color32.op_Implicit(ColorCatalog.GetColor(ColorCatalog.ColorIndex.LunarCoin));
		ruleChoiceDef2.onlyShowInGameBrowserIfNonDefault = true;
		ruleDef.MakeNewestChoiceDefault();
		RuleChoiceDef ruleChoiceDef3 = ruleDef.AddChoice("50", 50u, excludeByDefault: true);
		ruleChoiceDef3.tooltipNameToken = "RULE_STARTINGMONEY_CHOICE_50_NAME";
		ruleChoiceDef3.tooltipBodyToken = "RULE_STARTINGMONEY_CHOICE_50_DESC";
		ruleChoiceDef3.spritePath = "Textures/MiscIcons/texRuleBonusStartingMoney";
		ruleChoiceDef3.tooltipNameColor = Color32.op_Implicit(ColorCatalog.GetColor(ColorCatalog.ColorIndex.LunarCoin));
		ruleChoiceDef3.onlyShowInGameBrowserIfNonDefault = true;
		AddRule(ruleDef);
		RuleDef ruleDef2 = new RuleDef("Misc.StageOrder", "RULE_MISC_STAGE_ORDER");
		RuleChoiceDef ruleChoiceDef4 = ruleDef2.AddChoice("Normal", StageOrder.Normal, excludeByDefault: true);
		ruleChoiceDef4.tooltipNameToken = "RULE_STAGEORDER_CHOICE_NORMAL_NAME";
		ruleChoiceDef4.tooltipBodyToken = "RULE_STAGEORDER_CHOICE_NORMAL_DESC";
		ruleChoiceDef4.tooltipNameColor = Color32.op_Implicit(ColorCatalog.GetColor(ColorCatalog.ColorIndex.LunarCoin));
		ruleChoiceDef4.onlyShowInGameBrowserIfNonDefault = true;
		ruleDef2.MakeNewestChoiceDefault();
		RuleChoiceDef ruleChoiceDef5 = ruleDef2.AddChoice("Random", StageOrder.Random, excludeByDefault: true);
		ruleChoiceDef5.tooltipNameToken = "RULE_STAGEORDER_CHOICE_RANDOM_NAME";
		ruleChoiceDef5.tooltipBodyToken = "RULE_STAGEORDER_CHOICE_RANDOM_DESC";
		ruleChoiceDef5.spritePath = "Textures/MiscIcons/texRuleMapIsRandom";
		ruleChoiceDef5.tooltipNameColor = Color32.op_Implicit(ColorCatalog.GetColor(ColorCatalog.ColorIndex.LunarCoin));
		ruleChoiceDef5.onlyShowInGameBrowserIfNonDefault = true;
		AddRule(ruleDef2);
		RuleDef ruleDef3 = new RuleDef("Misc.KeepMoneyBetweenStages", "RULE_MISC_KEEP_MONEY_BETWEEN_STAGES");
		RuleChoiceDef ruleChoiceDef6 = ruleDef3.AddChoice("On", true, excludeByDefault: true);
		ruleChoiceDef6.tooltipNameToken = "";
		ruleChoiceDef6.tooltipBodyToken = "RULE_KEEPMONEYBETWEENSTAGES_CHOICE_ON_DESC";
		ruleChoiceDef6.onlyShowInGameBrowserIfNonDefault = true;
		RuleChoiceDef ruleChoiceDef7 = ruleDef3.AddChoice("Off", false, excludeByDefault: true);
		ruleChoiceDef7.tooltipNameToken = "";
		ruleChoiceDef7.tooltipBodyToken = "RULE_KEEPMONEYBETWEENSTAGES_CHOICE_OFF_DESC";
		ruleChoiceDef7.onlyShowInGameBrowserIfNonDefault = true;
		ruleDef3.MakeNewestChoiceDefault();
		AddRule(ruleDef3);
		RuleDef ruleDef4 = new RuleDef("Misc.AllowDropIn", "RULE_MISC_ALLOW_DROP_IN");
		RuleChoiceDef ruleChoiceDef8 = ruleDef4.AddChoice("On", true, excludeByDefault: true);
		ruleChoiceDef8.tooltipNameToken = "";
		ruleChoiceDef8.tooltipBodyToken = "RULE_ALLOWDROPIN_CHOICE_ON_DESC";
		ruleChoiceDef8.onlyShowInGameBrowserIfNonDefault = true;
		RuleChoiceDef ruleChoiceDef9 = ruleDef4.AddChoice("Off", false, excludeByDefault: true);
		ruleChoiceDef9.tooltipNameToken = "";
		ruleChoiceDef9.tooltipBodyToken = "RULE_ALLOWDROPIN_CHOICE_OFF_DESC";
		ruleChoiceDef9.onlyShowInGameBrowserIfNonDefault = true;
		ruleDef4.MakeNewestChoiceDefault();
		AddRule(ruleDef4);
		for (int j = 0; j < allRuleDefs.Count; j++)
		{
			RuleDef ruleDef5 = allRuleDefs[j];
			ruleDef5.globalIndex = j;
			for (int k = 0; k < ruleDef5.choices.Count; k++)
			{
				RuleChoiceDef ruleChoiceDef10 = ruleDef5.choices[k];
				ruleChoiceDef10.localIndex = k;
				ruleChoiceDef10.globalIndex = allChoicesDefs.Count;
				allChoicesDefs.Add(ruleChoiceDef10);
			}
			_allChoiceDefsWithUnlocks = allChoicesDefs.Where((RuleChoiceDef choiceDef) => Object.op_Implicit((Object)(object)choiceDef.unlockable)).ToArray();
		}
		availability.MakeAvailable();
	}

	[ConCommand(commandName = "rules_dump", flags = ConVarFlags.None, helpText = "Dump information about the rules system.")]
	private static void CCRulesDump(ConCommandArgs args)
	{
		List<string> list = new List<string>();
		List<string> list2 = new List<string>();
		for (int i = 0; i < ruleCount; i++)
		{
			RuleDef ruleDef = GetRuleDef(i);
			for (int j = 0; j < ruleDef.choices.Count; j++)
			{
				RuleChoiceDef ruleChoiceDef = ruleDef.choices[j];
				string item = $"  {{localChoiceIndex={ruleChoiceDef.localIndex} globalChoiceIndex={ruleChoiceDef.globalIndex} localName={ruleChoiceDef.localName}}}";
				list2.Add(item);
			}
			string text = string.Join("\n", list2);
			list2.Clear();
			string text2 = $"[{i}] {ruleDef.globalName} defaultChoiceIndex={ruleDef.defaultChoiceIndex}\n";
			list.Add(text2 + text);
		}
		Debug.Log((object)string.Join("\n", list));
	}

	[ConCommand(commandName = "rules_list_choices", flags = ConVarFlags.None, helpText = "Lists all rule choices.")]
	private static void CCRulesListChoices(ConCommandArgs args)
	{
		StringBuilder stringBuilder = new StringBuilder();
		stringBuilder.AppendLine("* = default choice.");
		stringBuilder.AppendLine();
		foreach (RuleChoiceDef allChoicesDef in allChoicesDefs)
		{
			stringBuilder.Append(allChoicesDef.globalName);
			if (allChoicesDef.isDefaultChoice)
			{
				stringBuilder.Append("*");
			}
			stringBuilder.AppendLine();
		}
		Debug.Log((object)stringBuilder.ToString());
	}
}
