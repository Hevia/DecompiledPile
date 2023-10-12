using System;
using System.Runtime.CompilerServices;
using AncientScepter;
using BepInEx.Bootstrap;
using EntityStates;
using IndiesSkills.MyEntityStates;
using IndiesSkills.Properties;
using On.RoR2;
using PaladinMod;
using PaladinMod.Modules;
using R2API;
using RoR2;
using RoR2.Skills;
using UnityEngine;

namespace IndiesSkills.Skills;

public class LifeDrain
{
	[Serializable]
	[CompilerGenerated]
	private sealed class _003C_003Ec
	{
		public static readonly _003C_003Ec _003C_003E9 = new _003C_003Ec();

		public static hook_DoDamageTick _003C_003E9__0_0;

		internal void _003CaddLifeDrain_003Eb__0_0(orig_DoDamageTick orig, TarTetherController self, bool mulch)
		{
			//IL_00b8: Unknown result type (might be due to invalid IL or missing references)
			//IL_00bd: Unknown result type (might be due to invalid IL or missing references)
			//IL_00c9: Unknown result type (might be due to invalid IL or missing references)
			//IL_00ce: Unknown result type (might be due to invalid IL or missing references)
			//IL_00d3: Unknown result type (might be due to invalid IL or missing references)
			//IL_00e4: Unknown result type (might be due to invalid IL or missing references)
			//IL_00f0: Unknown result type (might be due to invalid IL or missing references)
			//IL_011a: Unknown result type (might be due to invalid IL or missing references)
			//IL_011c: Unknown result type (might be due to invalid IL or missing references)
			//IL_0121: Unknown result type (might be due to invalid IL or missing references)
			//IL_013c: Unknown result type (might be due to invalid IL or missing references)
			//IL_0141: Unknown result type (might be due to invalid IL or missing references)
			//IL_0152: Unknown result type (might be due to invalid IL or missing references)
			//IL_0153: Unknown result type (might be due to invalid IL or missing references)
			//IL_0158: Unknown result type (might be due to invalid IL or missing references)
			//IL_015d: Unknown result type (might be due to invalid IL or missing references)
			//IL_0163: Unknown result type (might be due to invalid IL or missing references)
			//IL_0169: Unknown result type (might be due to invalid IL or missing references)
			//IL_0176: Expected O, but got Unknown
			//IL_018e: Unknown result type (might be due to invalid IL or missing references)
			//IL_0193: Unknown result type (might be due to invalid IL or missing references)
			//IL_0195: Unknown result type (might be due to invalid IL or missing references)
			//IL_01c1: Unknown result type (might be due to invalid IL or missing references)
			//IL_01c6: Unknown result type (might be due to invalid IL or missing references)
			//IL_01cb: Unknown result type (might be due to invalid IL or missing references)
			//IL_0234: Unknown result type (might be due to invalid IL or missing references)
			//IL_023a: Unknown result type (might be due to invalid IL or missing references)
			//IL_01ee: Unknown result type (might be due to invalid IL or missing references)
			//IL_01f0: Unknown result type (might be due to invalid IL or missing references)
			//IL_01f5: Unknown result type (might be due to invalid IL or missing references)
			//IL_01fa: Unknown result type (might be due to invalid IL or missing references)
			if ((Object)(object)self.ownerBody != (Object)null && (Object)(object)self.ownerBody.master.bodyPrefab == (Object)(object)Prefabs.paladinPrefab)
			{
				if (!Object.op_Implicit((Object)(object)self.targetHealthComponent))
				{
					self.targetHealthComponent = self.targetRoot.GetComponent<HealthComponent>();
				}
				if (!Object.op_Implicit((Object)(object)self.ownerHealthComponent))
				{
					self.ownerHealthComponent = self.ownerRoot.GetComponent<HealthComponent>();
				}
				if (!Object.op_Implicit((Object)(object)self.ownerBody))
				{
					self.ownerBody = self.ownerRoot.GetComponent<CharacterBody>();
				}
				if (Object.op_Implicit((Object)(object)self.ownerRoot))
				{
					DamageInfo val = new DamageInfo
					{
						position = self.targetRoot.transform.position,
						attacker = ((Component)self.ownerBody).gameObject,
						inflictor = ((Component)self).gameObject,
						damage = (mulch ? (self.damageCoefficientPerTick * self.mulchDamageScale) : self.damageCoefficientPerTick) * self.ownerBody.damage,
						damageColorIndex = (DamageColorIndex)0,
						damageType = (DamageType)(self.ownerBody.HasBuff(Buffs.ArmorBoost) ? 512 : 0),
						crit = self.ownerBody.RollCrit(),
						force = Vector3.zero,
						procChainMask = default(ProcChainMask),
						procCoefficient = 0.65f
					};
					if ((Object)(object)self.ownerBody.rigidbody != (Object)null)
					{
						Vector3 down = Vector3.down;
						down *= Mathf.Min(4f, Mathf.Max(self.ownerBody.rigidbody.mass / 100f, 1f));
						val.force += (self.ownerBody.HasBuff(Buffs.ArmorBoost) ? 1600f : 800f) * down;
					}
					self.targetHealthComponent.TakeDamage(val);
					if (!val.rejected)
					{
						self.ownerHealthComponent.Heal(val.damage * 0.25f, default(ProcChainMask), true);
					}
					if (!self.targetHealthComponent.alive)
					{
						self.NetworktargetRoot = null;
					}
				}
			}
			else
			{
				orig.Invoke(self, mulch);
			}
		}
	}

	public static void addLifeDrain()
	{
		//IL_0032: Unknown result type (might be due to invalid IL or missing references)
		//IL_0037: Unknown result type (might be due to invalid IL or missing references)
		//IL_0077: Unknown result type (might be due to invalid IL or missing references)
		//IL_0108: Unknown result type (might be due to invalid IL or missing references)
		//IL_010d: Unknown result type (might be due to invalid IL or missing references)
		//IL_014d: Unknown result type (might be due to invalid IL or missing references)
		//IL_01ec: Unknown result type (might be due to invalid IL or missing references)
		//IL_0210: Unknown result type (might be due to invalid IL or missing references)
		//IL_021a: Expected O, but got Unknown
		//IL_021b: Unknown result type (might be due to invalid IL or missing references)
		//IL_021d: Unknown result type (might be due to invalid IL or missing references)
		//IL_025f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0264: Unknown result type (might be due to invalid IL or missing references)
		//IL_026a: Expected O, but got Unknown
		LanguageAPI.Add("PALADIN_SPECIAL_LIFEDRAIN_NAME", "Feed The Tar");
		LanguageAPI.Add("PALADIN_SPECIAL_LIFEDRAIN_DESCRIPTION", "Tap into the gifts the tar has given you to drain the health of nearby enemies, <style=cIsDamage>dealing 100% damage per second to enemies tethered</style> while <style=cIsHealing>healing yourself for a quarter the damage you inflict.</style>  Enemies close to you during this attack <style=cIsDamage>take more damage</style>, and flying enemies are grounded.");
		SkillDef val = ScriptableObject.CreateInstance<SkillDef>();
		val.activationState = new SerializableEntityStateType(typeof(TarTetherMove));
		val.activationStateMachineName = "Weapon";
		val.baseMaxStock = 1;
		val.baseRechargeInterval = 13f;
		val.beginSkillCooldownOnSkillEnd = true;
		val.canceledFromSprinting = true;
		val.cancelSprintingOnActivation = true;
		val.fullRestockOnAssign = true;
		val.interruptPriority = (InterruptPriority)2;
		val.isCombatSkill = true;
		val.mustKeyPress = false;
		val.rechargeStock = 1;
		val.requiredStock = 1;
		val.stockToConsume = 1;
		val.icon = Main.LoadTexture2D(Resources.Feed_The_Tar_Icon);
		val.skillDescriptionToken = "PALADIN_SPECIAL_LIFEDRAIN_DESCRIPTION";
		val.skillName = "PALADIN_SPECIAL_LIFEDRAIN_NAME";
		val.skillNameToken = "PALADIN_SPECIAL_LIFEDRAIN_NAME";
		ContentAddition.AddSkillDef(val);
		LanguageAPI.Add("PALADIN_SPECIAL_LIFEDRAIN_SCEPTER_NAME", "Grasping Tar");
		LanguageAPI.Add("PALADIN_SPECIAL_LIFEDRAIN_SCEPTER_DESCRIPTION", "You now gain armor during this move, enemies are tarred, the amount of enemies that can be affected is uncapped and the force flying enemies are pulled down with is doubled. Tap into the gifts the tar has given you to drain the health of nearby enemies, <style=cIsDamage>dealing 100% damage per second to enemies tethered</style> while <style=cIsHealing>healing yourself for a quarter the damage you inflict.</style>  Enemies close to you during this attack <style=cIsDamage>take more damage</style>, and flying enemies are grounded.");
		SkillDef val2 = ScriptableObject.CreateInstance<SkillDef>();
		val2.activationState = new SerializableEntityStateType(typeof(TarTetherMoveScepter));
		val2.activationStateMachineName = "Weapon";
		val2.baseMaxStock = 1;
		val2.baseRechargeInterval = 13f;
		val2.beginSkillCooldownOnSkillEnd = true;
		val2.canceledFromSprinting = true;
		val2.cancelSprintingOnActivation = true;
		val2.fullRestockOnAssign = true;
		val2.interruptPriority = (InterruptPriority)2;
		val2.isCombatSkill = true;
		val2.mustKeyPress = false;
		val2.rechargeStock = 1;
		val2.requiredStock = 1;
		val2.stockToConsume = 1;
		val2.icon = Main.LoadTexture2D(Resources.Grasping_Tar_Icon);
		val2.skillDescriptionToken = "PALADIN_SPECIAL_LIFEDRAIN_SCEPTER_DESCRIPTION";
		val2.skillName = "PALADIN_SPECIAL_LIFEDRAIN_SCEPTER_NAME";
		val2.skillNameToken = "PALADIN_SPECIAL_LIFEDRAIN_SCEPTER_NAME";
		ContentAddition.AddSkillDef(val2);
		SkillLocator component = PaladinPlugin.characterPrefab.GetComponent<SkillLocator>();
		SkillFamily skillFamily = component.special.skillFamily;
		Array.Resize(ref skillFamily.variants, skillFamily.variants.Length + 1);
		Variant[] variants = skillFamily.variants;
		int num = skillFamily.variants.Length - 1;
		Variant val3 = new Variant
		{
			skillDef = val,
			unlockableName = ""
		};
		((Variant)(ref val3)).viewableNode = new Node(val.skillNameToken, false, (Node)null);
		variants[num] = val3;
		if (Chainloader.PluginInfos.ContainsKey("com.DestroyedClone.AncientScepter"))
		{
			ItemBase<AncientScepterItem>.instance.RegisterScepterSkill(val2, "RobPaladinBody", val);
		}
		object obj = _003C_003Ec._003C_003E9__0_0;
		if (obj == null)
		{
			hook_DoDamageTick val4 = delegate(orig_DoDamageTick orig, TarTetherController self, bool mulch)
			{
				//IL_00b8: Unknown result type (might be due to invalid IL or missing references)
				//IL_00bd: Unknown result type (might be due to invalid IL or missing references)
				//IL_00c9: Unknown result type (might be due to invalid IL or missing references)
				//IL_00ce: Unknown result type (might be due to invalid IL or missing references)
				//IL_00d3: Unknown result type (might be due to invalid IL or missing references)
				//IL_00e4: Unknown result type (might be due to invalid IL or missing references)
				//IL_00f0: Unknown result type (might be due to invalid IL or missing references)
				//IL_011a: Unknown result type (might be due to invalid IL or missing references)
				//IL_011c: Unknown result type (might be due to invalid IL or missing references)
				//IL_0121: Unknown result type (might be due to invalid IL or missing references)
				//IL_013c: Unknown result type (might be due to invalid IL or missing references)
				//IL_0141: Unknown result type (might be due to invalid IL or missing references)
				//IL_0152: Unknown result type (might be due to invalid IL or missing references)
				//IL_0153: Unknown result type (might be due to invalid IL or missing references)
				//IL_0158: Unknown result type (might be due to invalid IL or missing references)
				//IL_015d: Unknown result type (might be due to invalid IL or missing references)
				//IL_0163: Unknown result type (might be due to invalid IL or missing references)
				//IL_0169: Unknown result type (might be due to invalid IL or missing references)
				//IL_0176: Expected O, but got Unknown
				//IL_018e: Unknown result type (might be due to invalid IL or missing references)
				//IL_0193: Unknown result type (might be due to invalid IL or missing references)
				//IL_0195: Unknown result type (might be due to invalid IL or missing references)
				//IL_01c1: Unknown result type (might be due to invalid IL or missing references)
				//IL_01c6: Unknown result type (might be due to invalid IL or missing references)
				//IL_01cb: Unknown result type (might be due to invalid IL or missing references)
				//IL_0234: Unknown result type (might be due to invalid IL or missing references)
				//IL_023a: Unknown result type (might be due to invalid IL or missing references)
				//IL_01ee: Unknown result type (might be due to invalid IL or missing references)
				//IL_01f0: Unknown result type (might be due to invalid IL or missing references)
				//IL_01f5: Unknown result type (might be due to invalid IL or missing references)
				//IL_01fa: Unknown result type (might be due to invalid IL or missing references)
				if ((Object)(object)self.ownerBody != (Object)null && (Object)(object)self.ownerBody.master.bodyPrefab == (Object)(object)Prefabs.paladinPrefab)
				{
					if (!Object.op_Implicit((Object)(object)self.targetHealthComponent))
					{
						self.targetHealthComponent = self.targetRoot.GetComponent<HealthComponent>();
					}
					if (!Object.op_Implicit((Object)(object)self.ownerHealthComponent))
					{
						self.ownerHealthComponent = self.ownerRoot.GetComponent<HealthComponent>();
					}
					if (!Object.op_Implicit((Object)(object)self.ownerBody))
					{
						self.ownerBody = self.ownerRoot.GetComponent<CharacterBody>();
					}
					if (Object.op_Implicit((Object)(object)self.ownerRoot))
					{
						DamageInfo val5 = new DamageInfo
						{
							position = self.targetRoot.transform.position,
							attacker = ((Component)self.ownerBody).gameObject,
							inflictor = ((Component)self).gameObject,
							damage = (mulch ? (self.damageCoefficientPerTick * self.mulchDamageScale) : self.damageCoefficientPerTick) * self.ownerBody.damage,
							damageColorIndex = (DamageColorIndex)0,
							damageType = (DamageType)(self.ownerBody.HasBuff(Buffs.ArmorBoost) ? 512 : 0),
							crit = self.ownerBody.RollCrit(),
							force = Vector3.zero,
							procChainMask = default(ProcChainMask),
							procCoefficient = 0.65f
						};
						if ((Object)(object)self.ownerBody.rigidbody != (Object)null)
						{
							Vector3 down = Vector3.down;
							down *= Mathf.Min(4f, Mathf.Max(self.ownerBody.rigidbody.mass / 100f, 1f));
							val5.force += (self.ownerBody.HasBuff(Buffs.ArmorBoost) ? 1600f : 800f) * down;
						}
						self.targetHealthComponent.TakeDamage(val5);
						if (!val5.rejected)
						{
							self.ownerHealthComponent.Heal(val5.damage * 0.25f, default(ProcChainMask), true);
						}
						if (!self.targetHealthComponent.alive)
						{
							self.NetworktargetRoot = null;
						}
					}
				}
				else
				{
					orig.Invoke(self, mulch);
				}
			};
			_003C_003Ec._003C_003E9__0_0 = val4;
			obj = (object)val4;
		}
		TarTetherController.DoDamageTick += (hook_DoDamageTick)obj;
	}
}
