using System;
using System.Runtime.CompilerServices;
using EntityStates;
using EntityStates.VoidSurvivor.Vent;
using On.EntityStates.VoidSurvivor.Vent;
using R2API;
using RoR2;
using RoR2.Skills;
using UnityEngine;
using UnityEngine.AddressableAssets;

namespace IndiesSkills.Skills;

internal class VoidFiendCorruption
{
	[Serializable]
	[CompilerGenerated]
	private sealed class _003C_003Ec
	{
		public static readonly _003C_003Ec _003C_003E9 = new _003C_003Ec();

		public static hook_OnEnter _003C_003E9__0_0;

		public static hook_FixedUpdate _003C_003E9__0_1;

		internal void _003CaddVoidFiendVent_003Eb__0_0(orig_OnEnter orig, VentCorruption self)
		{
			self.corruptionReductionPerTick = 1f;
			orig.Invoke(self);
		}

		internal void _003CaddVoidFiendVent_003Eb__0_1(orig_FixedUpdate orig, VentCorruption self)
		{
			orig.Invoke(self);
		}
	}

	public static void addVoidFiendVent()
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
		//IL_0169: Unknown result type (might be due to invalid IL or missing references)
		//IL_016e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0174: Expected O, but got Unknown
		//IL_018e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0193: Unknown result type (might be due to invalid IL or missing references)
		//IL_0199: Expected O, but got Unknown
		LanguageAPI.Add("VOIDSURVIVOR_SPECIAL_VENT_NAME", "Vent");
		LanguageAPI.Add("VOIDSURVIVOR_SPECIAL_VENT_DESCRIPTION", "Vent corruption out of your system, healing yourself over time at the cost of your corruption.  Also causes you to levitate in the air.");
		SkillDef val = ScriptableObject.CreateInstance<SkillDef>();
		val.activationState = new SerializableEntityStateType(typeof(VentCorruption));
		val.activationStateMachineName = "CorruptMode";
		val.baseMaxStock = 1;
		val.baseRechargeInterval = 10f;
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
		val.skillDescriptionToken = "VOIDSURVIVOR_SPECIAL_VENT_DESCRIPTION";
		val.skillName = "VOIDSURVIVOR_SPECIAL_VENT_NAME";
		val.skillNameToken = "VOIDSURVIVOR_SPECIAL_VENT_NAME";
		ContentAddition.AddSkillDef(val);
		SkillLocator component = Addressables.LoadAssetAsync<GameObject>((object)"RoR2/DLC1/VoidSurvivor/VoidSurvivorBody.prefab").WaitForCompletion().GetComponent<SkillLocator>();
		SkillFamily skillFamily = component.special.skillFamily;
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
		object obj = _003C_003Ec._003C_003E9__0_0;
		if (obj == null)
		{
			hook_OnEnter val3 = delegate(orig_OnEnter orig, VentCorruption self)
			{
				self.corruptionReductionPerTick = 1f;
				orig.Invoke(self);
			};
			_003C_003Ec._003C_003E9__0_0 = val3;
			obj = (object)val3;
		}
		VentCorruption.OnEnter += (hook_OnEnter)obj;
		object obj2 = _003C_003Ec._003C_003E9__0_1;
		if (obj2 == null)
		{
			hook_FixedUpdate val4 = delegate(orig_FixedUpdate orig, VentCorruption self)
			{
				orig.Invoke(self);
			};
			_003C_003Ec._003C_003E9__0_1 = val4;
			obj2 = (object)val4;
		}
		VentCorruption.FixedUpdate += (hook_FixedUpdate)obj2;
	}
}
