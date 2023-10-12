using System;
using EntityStates;
using IndiesSkills.MyEntityStates;
using PaladinMod;
using R2API;
using RoR2;
using RoR2.Skills;
using UnityEngine;

namespace IndiesSkills.Skills;

internal class ViciousSlash
{
	public static void addViciousSlash()
	{
		//IL_0032: Unknown result type (might be due to invalid IL or missing references)
		//IL_0037: Unknown result type (might be due to invalid IL or missing references)
		//IL_0077: Unknown result type (might be due to invalid IL or missing references)
		//IL_0116: Unknown result type (might be due to invalid IL or missing references)
		//IL_013a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0144: Expected O, but got Unknown
		//IL_0145: Unknown result type (might be due to invalid IL or missing references)
		//IL_0146: Unknown result type (might be due to invalid IL or missing references)
		LanguageAPI.Add("PALADIN_PRIMARY_VICIOUSSLASH_NAME", "Believer's Slash");
		LanguageAPI.Add("PALADIN_PRIMARY_VICIOUSSLASH_DESCRIPTION", "Swing for <style=cIsDamage>300% damage</style>.  If Paladin is blessed, fire 3 tar blobs in front of you, <style=cIsDamage>dealing 150% damage each</style>. The tar blobs travel further with faster attack speed.");
		SkillDef val = ScriptableObject.CreateInstance<SkillDef>();
		val.activationState = new SerializableEntityStateType(typeof(OilSlash));
		val.activationStateMachineName = "Weapon";
		val.baseMaxStock = 1;
		val.baseRechargeInterval = 0f;
		val.beginSkillCooldownOnSkillEnd = true;
		val.canceledFromSprinting = false;
		val.cancelSprintingOnActivation = true;
		val.fullRestockOnAssign = true;
		val.interruptPriority = (InterruptPriority)1;
		val.isCombatSkill = true;
		val.mustKeyPress = false;
		val.rechargeStock = 1;
		val.requiredStock = 1;
		val.stockToConsume = 1;
		val.icon = Resources.Load<Sprite>("NotAnActualPath");
		val.skillDescriptionToken = "PALADIN_PRIMARY_VICIOUSSLASH_DESCRIPTION";
		val.skillName = "PALADIN_PRIMARY_VICIOUSSLASH_NAME";
		val.skillNameToken = "PALADIN_PRIMARY_VICIOUSSLASH_NAME";
		ContentAddition.AddSkillDef(val);
		SkillLocator component = PaladinPlugin.characterPrefab.GetComponent<SkillLocator>();
		SkillFamily skillFamily = component.primary.skillFamily;
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
