using System;
using EntityStates;
using IndiesSkills.MyEntityStates;
using R2API;
using RoR2;
using RoR2.Skills;
using UnityEngine;

namespace IndiesSkills.Skills;

internal class AcridCharge
{
	public static void addAcridCharge()
	{
		//IL_0032: Unknown result type (might be due to invalid IL or missing references)
		//IL_0037: Unknown result type (might be due to invalid IL or missing references)
		//IL_0077: Unknown result type (might be due to invalid IL or missing references)
		//IL_011b: Unknown result type (might be due to invalid IL or missing references)
		//IL_013f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0149: Expected O, but got Unknown
		//IL_014a: Unknown result type (might be due to invalid IL or missing references)
		//IL_014b: Unknown result type (might be due to invalid IL or missing references)
		LanguageAPI.Add("ACRID_UTILITY_CHARGE_NAME", "Caustic Sludge");
		LanguageAPI.Add("ACRID_UTILITY_CHARGE_DESCRIPTION", "Charge forward, dealing 400% damage to any enemies hit whilst charging.  Leaves a streak of acid behind you as you charge.");
		SkillDef val = ScriptableObject.CreateInstance<SkillDef>();
		val.activationState = new SerializableEntityStateType(typeof(IndiesSkills.MyEntityStates.AcridCharge));
		val.activationStateMachineName = "Weapon";
		val.baseMaxStock = 1;
		val.baseRechargeInterval = 17f;
		val.beginSkillCooldownOnSkillEnd = true;
		val.canceledFromSprinting = false;
		val.cancelSprintingOnActivation = false;
		val.fullRestockOnAssign = true;
		val.interruptPriority = (InterruptPriority)1;
		val.isCombatSkill = true;
		val.mustKeyPress = false;
		val.rechargeStock = 1;
		val.requiredStock = 1;
		val.stockToConsume = 1;
		val.icon = Resources.Load<Sprite>("NotAnActualPath");
		val.skillDescriptionToken = "ACRID_UTILITY_CHARGE_DESCRIPTION";
		val.skillName = "ACRID_UTILITY_CHARGE_NAME";
		val.skillNameToken = "ACRID_UTILITY_CHARGE_NAME";
		ContentAddition.AddSkillDef(val);
		SkillLocator component = Resources.Load<GameObject>("prefabs/characterbodies/CrocoBody").GetComponent<SkillLocator>();
		SkillFamily skillFamily = component.utility.skillFamily;
		Array.Resize(ref skillFamily.variants, skillFamily.variants.Length + 1);
		Variant[] variants = skillFamily.variants;
		int num = skillFamily.variants.Length - 1;
		Variant val2 = new Variant
		{
			skillDef = val,
			unlockableName = ""
		};
		((Variant)(ref val2)).viewableNode = new Node(val.skillNameToken, false, (Node)null);
		variants[num] = val2;
	}
}
