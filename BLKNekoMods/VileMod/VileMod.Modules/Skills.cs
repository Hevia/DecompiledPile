using System;
using RoR2;
using RoR2.Skills;
using UnityEngine;

namespace VileMod.Modules;

internal static class Skills
{
	public static void CreateSkillFamilies(GameObject targetPrefab, bool destroyExisting = true)
	{
		if (destroyExisting)
		{
			GenericSkill[] componentsInChildren = targetPrefab.GetComponentsInChildren<GenericSkill>();
			foreach (GenericSkill val in componentsInChildren)
			{
				Object.DestroyImmediate((Object)(object)val);
			}
		}
		SkillLocator component = targetPrefab.GetComponent<SkillLocator>();
		component.primary = CreateGenericSkillWithSkillFamily(targetPrefab, "Primary");
		component.secondary = CreateGenericSkillWithSkillFamily(targetPrefab, "Secondary");
		component.utility = CreateGenericSkillWithSkillFamily(targetPrefab, "Utility");
		component.special = CreateGenericSkillWithSkillFamily(targetPrefab, "Special");
	}

	public static GenericSkill CreateGenericSkillWithSkillFamily(GameObject targetPrefab, string familyName, bool hidden = false)
	{
		GenericSkill val = targetPrefab.AddComponent<GenericSkill>();
		val.skillName = familyName;
		val.hideInCharacterSelect = hidden;
		SkillFamily val2 = ScriptableObject.CreateInstance<SkillFamily>();
		((Object)val2).name = ((Object)targetPrefab).name + familyName + "Family";
		val2.variants = (Variant[])(object)new Variant[0];
		val._skillFamily = val2;
		Content.AddSkillFamily(val2);
		return val;
	}

	public static void AddSkillToFamily(SkillFamily skillFamily, SkillDef skillDef, UnlockableDef unlockableDef = null)
	{
		//IL_0029: Unknown result type (might be due to invalid IL or missing references)
		//IL_0049: Unknown result type (might be due to invalid IL or missing references)
		//IL_0053: Expected O, but got Unknown
		//IL_0054: Unknown result type (might be due to invalid IL or missing references)
		//IL_0055: Unknown result type (might be due to invalid IL or missing references)
		Array.Resize(ref skillFamily.variants, skillFamily.variants.Length + 1);
		Variant[] variants = skillFamily.variants;
		int num = skillFamily.variants.Length - 1;
		Variant val = new Variant
		{
			skillDef = skillDef,
			unlockableDef = unlockableDef
		};
		((Variant)(ref val)).viewableNode = new Node(skillDef.skillNameToken, false, (Node)null);
		variants[num] = val;
	}

	public static void AddSkillsToFamily(SkillFamily skillFamily, params SkillDef[] skillDefs)
	{
		foreach (SkillDef skillDef in skillDefs)
		{
			AddSkillToFamily(skillFamily, skillDef);
		}
	}

	public static void AddPrimarySkills(GameObject targetPrefab, params SkillDef[] skillDefs)
	{
		AddSkillsToFamily(targetPrefab.GetComponent<SkillLocator>().primary.skillFamily, skillDefs);
	}

	public static void AddSecondarySkills(GameObject targetPrefab, params SkillDef[] skillDefs)
	{
		AddSkillsToFamily(targetPrefab.GetComponent<SkillLocator>().secondary.skillFamily, skillDefs);
	}

	public static void AddUtilitySkills(GameObject targetPrefab, params SkillDef[] skillDefs)
	{
		AddSkillsToFamily(targetPrefab.GetComponent<SkillLocator>().utility.skillFamily, skillDefs);
	}

	public static void AddSpecialSkills(GameObject targetPrefab, params SkillDef[] skillDefs)
	{
		AddSkillsToFamily(targetPrefab.GetComponent<SkillLocator>().special.skillFamily, skillDefs);
	}

	public static void AddUnlockablesToFamily(SkillFamily skillFamily, params UnlockableDef[] unlockableDefs)
	{
		//IL_000d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0012: Unknown result type (might be due to invalid IL or missing references)
		//IL_0024: Unknown result type (might be due to invalid IL or missing references)
		//IL_0025: Unknown result type (might be due to invalid IL or missing references)
		for (int i = 0; i < unlockableDefs.Length; i++)
		{
			Variant val = skillFamily.variants[i];
			val.unlockableDef = unlockableDefs[i];
			skillFamily.variants[i] = val;
		}
	}

	public static SkillDef CreateSkillDef(SkillDefInfo skillDefInfo)
	{
		return Skills.CreateSkillDef<SkillDef>(skillDefInfo);
	}

	public static T CreateSkillDef<T>(SkillDefInfo skillDefInfo) where T : SkillDef
	{
		//IL_0064: Unknown result type (might be due to invalid IL or missing references)
		//IL_0069: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ec: Unknown result type (might be due to invalid IL or missing references)
		//IL_00f1: Unknown result type (might be due to invalid IL or missing references)
		T val = ScriptableObject.CreateInstance<T>();
		((SkillDef)val).skillName = skillDefInfo.skillName;
		((Object)(object)val).name = skillDefInfo.skillName;
		((SkillDef)val).skillNameToken = skillDefInfo.skillNameToken;
		((SkillDef)val).skillDescriptionToken = skillDefInfo.skillDescriptionToken;
		((SkillDef)val).icon = skillDefInfo.skillIcon;
		((SkillDef)val).activationState = skillDefInfo.activationState;
		((SkillDef)val).activationStateMachineName = skillDefInfo.activationStateMachineName;
		((SkillDef)val).baseMaxStock = skillDefInfo.baseMaxStock;
		((SkillDef)val).baseRechargeInterval = skillDefInfo.baseRechargeInterval;
		((SkillDef)val).beginSkillCooldownOnSkillEnd = skillDefInfo.beginSkillCooldownOnSkillEnd;
		((SkillDef)val).canceledFromSprinting = skillDefInfo.canceledFromSprinting;
		((SkillDef)val).forceSprintDuringState = skillDefInfo.forceSprintDuringState;
		((SkillDef)val).fullRestockOnAssign = skillDefInfo.fullRestockOnAssign;
		((SkillDef)val).interruptPriority = skillDefInfo.interruptPriority;
		((SkillDef)val).resetCooldownTimerOnUse = skillDefInfo.resetCooldownTimerOnUse;
		((SkillDef)val).isCombatSkill = skillDefInfo.isCombatSkill;
		((SkillDef)val).mustKeyPress = skillDefInfo.mustKeyPress;
		((SkillDef)val).cancelSprintingOnActivation = skillDefInfo.cancelSprintingOnActivation;
		((SkillDef)val).rechargeStock = skillDefInfo.rechargeStock;
		((SkillDef)val).requiredStock = skillDefInfo.requiredStock;
		((SkillDef)val).stockToConsume = skillDefInfo.stockToConsume;
		((SkillDef)val).keywordTokens = skillDefInfo.keywordTokens;
		Content.AddSkillDef((SkillDef)(object)val);
		return val;
	}

	internal static void PassiveSetup(GameObject targetPrefab)
	{
		SkillLocator component = targetPrefab.GetComponent<SkillLocator>();
		string text = "BLKNeko_VILEV3_BODY_";
		component.passiveSkill.enabled = true;
		component.passiveSkill.skillNameToken = text + "PASSIVE_NAME";
		component.passiveSkill.skillDescriptionToken = text + "PASSIVE_DESCRIPTION";
		component.passiveSkill.icon = Assets.VilePassiveIcon;
	}
}
