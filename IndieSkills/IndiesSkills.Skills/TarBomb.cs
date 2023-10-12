using System;
using EntityStates;
using IndiesSkills.MyEntityStates;
using PaladinMod;
using R2API;
using RoR2;
using RoR2.Skills;
using UnityEngine;

namespace IndiesSkills.Skills;

internal class TarBomb
{
	public static void addTarBomb()
	{
		//IL_0032: Unknown result type (might be due to invalid IL or missing references)
		//IL_0037: Unknown result type (might be due to invalid IL or missing references)
		//IL_0077: Unknown result type (might be due to invalid IL or missing references)
		//IL_0116: Unknown result type (might be due to invalid IL or missing references)
		//IL_013a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0144: Expected O, but got Unknown
		//IL_0145: Unknown result type (might be due to invalid IL or missing references)
		//IL_0146: Unknown result type (might be due to invalid IL or missing references)
		LanguageAPI.Add("PALADIN_SECONDARY_TARBOMB_NAME", "Tar Bombs");
		LanguageAPI.Add("PALADIN_SECONDARY_TARBOMB_DESCRIPTION", "Throw 3 tar bombs that deals <style=cIsDamage>120% damage each</style> and applies Tar on hit. If Paladin is blessed, throw an additional 6 tar bombs that <style=cIsDamage>deal 60% damage with spread</style>. Store up to 3 charges by default.");
		SkillDef val = ScriptableObject.CreateInstance<SkillDef>();
		val.activationState = new SerializableEntityStateType(typeof(ThrowTarBomb));
		val.activationStateMachineName = "Weapon";
		val.baseMaxStock = 3;
		val.baseRechargeInterval = 3f;
		val.beginSkillCooldownOnSkillEnd = false;
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
		val.skillDescriptionToken = "PALADIN_SECONDARY_TARBOMB_DESCRIPTION";
		val.skillName = "PALADIN_SECONDARY_TARBOMB_NAME";
		val.skillNameToken = "PALADIN_SECONDARY_TARBOMB_NAME";
		ContentAddition.AddSkillDef(val);
		SkillLocator component = PaladinPlugin.characterPrefab.GetComponent<SkillLocator>();
		SkillFamily skillFamily = component.secondary.skillFamily;
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
