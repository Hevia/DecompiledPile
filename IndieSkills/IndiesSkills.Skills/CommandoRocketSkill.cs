using System;
using EntityStates;
using EntityStates.Commando.CommandoWeapon;
using R2API;
using RoR2;
using RoR2.Skills;
using UnityEngine;

namespace IndiesSkills.Skills;

internal class CommandoRocketSkill
{
	public static void addCommandoRocketSkill()
	{
		//IL_0032: Unknown result type (might be due to invalid IL or missing references)
		//IL_0037: Unknown result type (might be due to invalid IL or missing references)
		//IL_0077: Unknown result type (might be due to invalid IL or missing references)
		//IL_011b: Unknown result type (might be due to invalid IL or missing references)
		//IL_013b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0145: Expected O, but got Unknown
		//IL_0146: Unknown result type (might be due to invalid IL or missing references)
		//IL_0147: Unknown result type (might be due to invalid IL or missing references)
		LanguageAPI.Add("COMMANDO_SECONDARY_ROCKET_NAME", "Fire Rocket");
		LanguageAPI.Add("COMMANDO_SECONDARY_ROCKET_DESCRIPTION", "Fire a rocket.");
		SkillDef val = ScriptableObject.CreateInstance<SkillDef>();
		val.activationState = new SerializableEntityStateType(typeof(FireRocket));
		val.activationStateMachineName = "Weapon";
		val.baseMaxStock = 1;
		val.baseRechargeInterval = 4f;
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
		val.skillDescriptionToken = "COMMANDO_SECONDARY_ROCKET_DESCRIPTION";
		val.skillName = "COMMANDO_SECONDARY_ROCKET_NAME";
		val.skillNameToken = "COMMANDO_SECONDARY_ROCKET_NAME";
		ContentAddition.AddSkillDef(val);
		SkillLocator component = Resources.Load<GameObject>("prefabs/characterbodies/CommandoBody").GetComponent<SkillLocator>();
		SkillFamily skillFamily = component.secondary.skillFamily;
		Array.Resize(ref skillFamily.variants, skillFamily.variants.Length + 1);
		Variant[] variants = skillFamily.variants;
		int num = skillFamily.variants.Length - 1;
		Variant val2 = new Variant
		{
			skillDef = val,
			unlockableDef = null
		};
		((Variant)(ref val2)).viewableNode = new Node(val.skillNameToken, false, (Node)null);
		variants[num] = val2;
	}
}
