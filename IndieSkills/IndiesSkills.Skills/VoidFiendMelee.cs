using System;
using System.Runtime.CompilerServices;
using EntityStates;
using EntityStates.VoidSurvivor.CorruptMode;
using EntityStates.VoidSurvivor.Weapon;
using IndiesSkills.MyEntityStates;
using On.EntityStates.VoidSurvivor.CorruptMode;
using On.EntityStates.VoidSurvivor.Weapon;
using R2API;
using RoR2;
using RoR2.Skills;
using UnityEngine;
using UnityEngine.AddressableAssets;

namespace IndiesSkills.Skills;

internal class VoidFiendMelee
{
	[Serializable]
	[CompilerGenerated]
	private sealed class _003C_003Ec
	{
		public static readonly _003C_003Ec _003C_003E9 = new _003C_003Ec();

		public static hook_GetMinimumInterruptPriority _003C_003E9__0_0;

		public static hook_OnEnter _003C_003E9__0_1;

		internal InterruptPriority _003CaddVoidFiendMelee_003Eb__0_0(orig_GetMinimumInterruptPriority orig, SwingMeleeBase self)
		{
			//IL_0002: Unknown result type (might be due to invalid IL or missing references)
			//IL_0005: Unknown result type (might be due to invalid IL or missing references)
			return (InterruptPriority)2;
		}

		internal void _003CaddVoidFiendMelee_003Eb__0_1(orig_OnEnter orig, SwingMeleeBase self)
		{
			//IL_002a: Unknown result type (might be due to invalid IL or missing references)
			orig.Invoke(self);
			((BasicMeleeAttack)self).damageCoefficient = 4f;
			if (((EntityState)self).isAuthority)
			{
				((BasicMeleeAttack)self).overlapAttack.damageType = (DamageType)32768;
			}
		}
	}

	public static void addVoidFiendMelee()
	{
		//IL_0042: Unknown result type (might be due to invalid IL or missing references)
		//IL_0047: Unknown result type (might be due to invalid IL or missing references)
		//IL_00af: Unknown result type (might be due to invalid IL or missing references)
		//IL_0146: Unknown result type (might be due to invalid IL or missing references)
		//IL_014b: Unknown result type (might be due to invalid IL or missing references)
		//IL_018d: Unknown result type (might be due to invalid IL or missing references)
		//IL_01b7: Unknown result type (might be due to invalid IL or missing references)
		//IL_01c1: Expected O, but got Unknown
		//IL_01c2: Unknown result type (might be due to invalid IL or missing references)
		//IL_01c4: Unknown result type (might be due to invalid IL or missing references)
		//IL_01dd: Unknown result type (might be due to invalid IL or missing references)
		//IL_01e2: Unknown result type (might be due to invalid IL or missing references)
		//IL_01e8: Expected O, but got Unknown
		//IL_021a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0224: Expected O, but got Unknown
		//IL_0202: Unknown result type (might be due to invalid IL or missing references)
		//IL_0207: Unknown result type (might be due to invalid IL or missing references)
		//IL_020d: Expected O, but got Unknown
		LanguageAPI.Add("VOIDSURVIVOR_PRIMARY_MELEE_NAME", "Puncture");
		LanguageAPI.Add("VOIDSURVIVOR_PRIMARY_MELEE_DESCRIPTION", "Use your void power to slash in front of you for 300% damage.  Enemies hit by this attack will receive a stack of Nullify.");
		SteppedSkillDef voidmandoMelee = ScriptableObject.CreateInstance<SteppedSkillDef>();
		((SkillDef)voidmandoMelee).activationState = new SerializableEntityStateType(typeof(VoidmandoMeleeAttack));
		((SkillDef)voidmandoMelee).activationStateMachineName = "Weapon";
		((SkillDef)voidmandoMelee).baseMaxStock = 1;
		((SkillDef)voidmandoMelee).baseRechargeInterval = 0f;
		((SkillDef)voidmandoMelee).beginSkillCooldownOnSkillEnd = true;
		((SkillDef)voidmandoMelee).canceledFromSprinting = false;
		((SkillDef)voidmandoMelee).cancelSprintingOnActivation = false;
		((SkillDef)voidmandoMelee).fullRestockOnAssign = true;
		((SkillDef)voidmandoMelee).interruptPriority = (InterruptPriority)1;
		((SkillDef)voidmandoMelee).isCombatSkill = true;
		((SkillDef)voidmandoMelee).mustKeyPress = false;
		((SkillDef)voidmandoMelee).rechargeStock = 1;
		((SkillDef)voidmandoMelee).requiredStock = 1;
		((SkillDef)voidmandoMelee).stockToConsume = 1;
		((SkillDef)voidmandoMelee).icon = Resources.Load<Sprite>("NotAnActualPath");
		((SkillDef)voidmandoMelee).skillDescriptionToken = "VOIDSURVIVOR_PRIMARY_MELEE_DESCRIPTION";
		((SkillDef)voidmandoMelee).skillName = "VOIDSURVIVOR_PRIMARY_MELEE_NAME";
		((SkillDef)voidmandoMelee).skillNameToken = "VOIDSURVIVOR_PRIMARY_MELEE_NAME";
		ContentAddition.AddSkillDef((SkillDef)(object)voidmandoMelee);
		SkillLocator component = Addressables.LoadAssetAsync<GameObject>((object)"RoR2/DLC1/VoidSurvivor/VoidSurvivorBody.prefab").WaitForCompletion().GetComponent<SkillLocator>();
		SkillFamily skillFamily = component.primary.skillFamily;
		Array.Resize(ref skillFamily.variants, skillFamily.variants.Length + 1);
		Variant[] variants = skillFamily.variants;
		int num = skillFamily.variants.Length - 1;
		Variant val = new Variant
		{
			skillDef = (SkillDef)(object)voidmandoMelee,
			unlockableDef = null
		};
		((Variant)(ref val)).viewableNode = new Node(((SkillDef)voidmandoMelee).skillNameToken, false, (Node)null);
		variants[num] = val;
		object obj = _003C_003Ec._003C_003E9__0_0;
		if (obj == null)
		{
			hook_GetMinimumInterruptPriority val2 = (orig_GetMinimumInterruptPriority orig, SwingMeleeBase self) => (InterruptPriority)2;
			_003C_003Ec._003C_003E9__0_0 = val2;
			obj = (object)val2;
		}
		SwingMeleeBase.GetMinimumInterruptPriority += (hook_GetMinimumInterruptPriority)obj;
		object obj2 = _003C_003Ec._003C_003E9__0_1;
		if (obj2 == null)
		{
			hook_OnEnter val3 = delegate(orig_OnEnter orig, SwingMeleeBase self)
			{
				//IL_002a: Unknown result type (might be due to invalid IL or missing references)
				orig.Invoke(self);
				((BasicMeleeAttack)self).damageCoefficient = 4f;
				if (((EntityState)self).isAuthority)
				{
					((BasicMeleeAttack)self).overlapAttack.damageType = (DamageType)32768;
				}
			};
			_003C_003Ec._003C_003E9__0_1 = val3;
			obj2 = (object)val3;
		}
		SwingMeleeBase.OnEnter += (hook_OnEnter)obj2;
		CorruptMode.OnEnter += (hook_OnEnter)delegate(orig_OnEnter orig, CorruptMode self)
		{
			bool flag = false;
			if (((EntityState)self).isAuthority && Object.op_Implicit((Object)(object)((EntityState)self).skillLocator) && ((EntityState)self).characterBody.skillLocator.primary.skillDef.skillName == ((SkillDef)voidmandoMelee).skillName)
			{
				flag = true;
			}
			orig.Invoke(self);
			if (((EntityState)self).isAuthority && Object.op_Implicit((Object)(object)((EntityState)self).skillLocator))
			{
				((EntityState)self).skillLocator.primary.SetSkillOverride((object)self, (SkillDef)(object)voidmandoMelee, (SkillOverridePriority)4);
			}
		};
	}
}
