using System;
using EntityStates;
using EntityStates.VoidSurvivor.Weapon;
using R2API;
using RoR2;
using RoR2.Skills;
using UnityEngine;
using UnityEngine.AddressableAssets;

namespace IndiesSkills.Skills;

public class CrabCannon
{
	public static void addCrabCannon()
	{
		//IL_0032: Unknown result type (might be due to invalid IL or missing references)
		//IL_0037: Unknown result type (might be due to invalid IL or missing references)
		//IL_0077: Unknown result type (might be due to invalid IL or missing references)
		//IL_00dc: Unknown result type (might be due to invalid IL or missing references)
		//IL_00e1: Unknown result type (might be due to invalid IL or missing references)
		//IL_0123: Unknown result type (might be due to invalid IL or missing references)
		//IL_0143: Unknown result type (might be due to invalid IL or missing references)
		//IL_014d: Expected O, but got Unknown
		//IL_014e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0150: Unknown result type (might be due to invalid IL or missing references)
		LanguageAPI.Add("VOIDSURVIVOR_SECONDARY_CRABCANNON_NAME", "Crab Cannon");
		LanguageAPI.Add("VOIDSURVIVOR_SECONDARY_CRABCANNON_DESCRIPTION", "This doesn't kill the crab, but it certainly kills you.");
		SkillDef val = ScriptableObject.CreateInstance<SkillDef>();
		val.activationState = new SerializableEntityStateType(typeof(FireBlaster1));
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
		val.skillDescriptionToken = "VOIDSURVIVOR_SECONDARY_CRABCANNON_DESCRIPTION";
		val.skillName = "VOIDSURVIVOR_SECONDARY_CRABCANNON_NAME";
		val.skillNameToken = "VOIDSURVIVOR_SECONDARY_CRABCANNON_NAME";
		ContentAddition.AddSkillDef(val);
		SkillLocator component = Addressables.LoadAssetAsync<GameObject>((object)"RoR2/DLC1/VoidSurvivor/VoidSurvivorBody.prefab").WaitForCompletion().GetComponent<SkillLocator>();
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
