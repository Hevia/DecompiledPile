using System;
using EntityStates;
using EntityStates.Mage;
using On.EntityStates.Mage;
using On.RoR2;
using R2API;
using RoR2;
using RoR2.Skills;
using UnityEngine;
using UnityEngine.Networking;

namespace IndiesSkills.Skills;

internal class ArtificerPassives
{
	public static void addArtificerPassives()
	{
		//IL_00d7: Unknown result type (might be due to invalid IL or missing references)
		//IL_0105: Unknown result type (might be due to invalid IL or missing references)
		//IL_010f: Expected O, but got Unknown
		//IL_0110: Unknown result type (might be due to invalid IL or missing references)
		//IL_0112: Unknown result type (might be due to invalid IL or missing references)
		//IL_01b3: Unknown result type (might be due to invalid IL or missing references)
		//IL_01e1: Unknown result type (might be due to invalid IL or missing references)
		//IL_01eb: Expected O, but got Unknown
		//IL_01ec: Unknown result type (might be due to invalid IL or missing references)
		//IL_01ee: Unknown result type (might be due to invalid IL or missing references)
		//IL_028f: Unknown result type (might be due to invalid IL or missing references)
		//IL_02bd: Unknown result type (might be due to invalid IL or missing references)
		//IL_02c7: Expected O, but got Unknown
		//IL_02c8: Unknown result type (might be due to invalid IL or missing references)
		//IL_02ca: Unknown result type (might be due to invalid IL or missing references)
		//IL_02d6: Unknown result type (might be due to invalid IL or missing references)
		//IL_02e0: Expected O, but got Unknown
		//IL_02e8: Unknown result type (might be due to invalid IL or missing references)
		//IL_02f2: Expected O, but got Unknown
		//IL_02fa: Unknown result type (might be due to invalid IL or missing references)
		//IL_0304: Expected O, but got Unknown
		//IL_030c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0316: Expected O, but got Unknown
		GameObject val = Resources.Load<GameObject>("prefabs/characterbodies/MageBody");
		SkillLocator component = val.GetComponent<SkillLocator>();
		component.passiveSkill.enabled = false;
		GenericSkill val2 = val.AddComponent<GenericSkill>();
		val2.skillName = "Passive";
		SkillFamily val3 = (val2._skillFamily = ScriptableObject.CreateInstance<SkillFamily>());
		val3.variants = (Variant[])(object)new Variant[1];
		LanguageAPI.Add("ARTIFICER_PASSIVE_JETPACK_NAME", "ENV Suit");
		LanguageAPI.Add("ARTIFICER_PASSIVE_JETPACK_DESCRIPTION", "Holding the jump key causes Artificer to hover in the air.");
		SkillDef jetpack = ScriptableObject.CreateInstance<SkillDef>();
		jetpack.icon = Resources.Load<Sprite>("NotAnActualPath");
		jetpack.skillDescriptionToken = "ARTIFICER_PASSIVE_JETPACK_DESCRIPTION";
		jetpack.skillName = "ARTIFICER_PASSIVE_JETPACK_NAME";
		jetpack.skillNameToken = "ARTIFICER_PASSIVE_JETPACK_NAME";
		Variant[] variants = val3.variants;
		int num = val3.variants.Length - 1;
		Variant val4 = new Variant
		{
			skillDef = jetpack,
			unlockableName = ""
		};
		((Variant)(ref val4)).viewableNode = new Node(jetpack.skillNameToken, false, (Node)null);
		variants[num] = val4;
		LanguageAPI.Add("ARTIFICER_PASSIVE_BOOSTERSUIT_NAME", "Booster Suit");
		LanguageAPI.Add("ARTIFICER_PASSIVE_BOOSTERSUIT_DESCRIPTION", "Artificer can jump twice and jumps 50% higher.");
		SkillDef boostersuit = ScriptableObject.CreateInstance<SkillDef>();
		boostersuit.icon = Resources.Load<Sprite>("NotAnActualPath");
		boostersuit.skillDescriptionToken = "ARTIFICER_PASSIVE_BOOSTERSUIT_DESCRIPTION";
		boostersuit.skillName = "ARTIFICER_PASSIVE_BOOSTERSUIT_NAME";
		boostersuit.skillNameToken = "ARTIFICER_PASSIVE_BOOSTERSUIT_NAME";
		Array.Resize(ref val3.variants, val3.variants.Length + 1);
		Variant[] variants2 = val3.variants;
		int num2 = val3.variants.Length - 1;
		val4 = new Variant
		{
			skillDef = boostersuit,
			unlockableName = ""
		};
		((Variant)(ref val4)).viewableNode = new Node(boostersuit.skillNameToken, false, (Node)null);
		variants2[num2] = val4;
		LanguageAPI.Add("ARTIFICER_PASSIVE_ELITEFINDER_NAME", "Elementally Tuned");
		LanguageAPI.Add("ARTIFICER_PASSIVE_ELITEFINDER_DESCRIPTION", "Slain elites have a 5% chance to drop their aspect.");
		SkillDef elitefinder = ScriptableObject.CreateInstance<SkillDef>();
		elitefinder.icon = Resources.Load<Sprite>("NotAnActualPath");
		elitefinder.skillDescriptionToken = "ARTIFICER_PASSIVE_ELITEFINDER_DESCRIPTION";
		elitefinder.skillName = "ARTIFICER_PASSIVE_ELITEFINDER_NAME";
		elitefinder.skillNameToken = "ARTIFICER_PASSIVE_ELITEFINDER_NAME";
		Array.Resize(ref val3.variants, val3.variants.Length + 1);
		Variant[] variants3 = val3.variants;
		int num3 = val3.variants.Length - 1;
		val4 = new Variant
		{
			skillDef = elitefinder,
			unlockableName = ""
		};
		((Variant)(ref val4)).viewableNode = new Node(elitefinder.skillNameToken, false, (Node)null);
		variants3[num3] = val4;
		JetpackOn.OnEnter += (hook_OnEnter)delegate(orig_OnEnter orig, JetpackOn self)
		{
			if (((EntityState)self).characterBody.skillLocator.FindSkill("Passive").skillDef.skillName != jetpack.skillName)
			{
				((EntityState)self).outer.SetNextStateToMain();
			}
			else
			{
				orig.Invoke(self);
			}
		};
		JetpackOn.FixedUpdate += (hook_FixedUpdate)delegate(orig_FixedUpdate orig, JetpackOn self)
		{
			if (((EntityState)self).characterBody.skillLocator.FindSkill("Passive").skillDef.skillName != jetpack.skillName)
			{
				((EntityState)self).outer.SetNextStateToMain();
			}
			else
			{
				orig.Invoke(self);
			}
		};
		CharacterMaster.SpawnBody += (hook_SpawnBody)delegate(orig_SpawnBody orig, CharacterMaster self, Vector3 pos, Quaternion rot)
		{
			//IL_0003: Unknown result type (might be due to invalid IL or missing references)
			//IL_0004: Unknown result type (might be due to invalid IL or missing references)
			orig.Invoke(self, pos, rot);
			if ((Object)(object)self.bodyPrefab == (Object)(object)BodyCatalog.FindBodyPrefab("MageBody") && self.GetBody().skillLocator.FindSkill("Passive").skillDef.skillName == boostersuit.skillName)
			{
				self.GetBody().baseJumpCount = 2;
				self.GetBody().baseJumpPower = Resources.Load<GameObject>("prefabs/characterbodies/MageBody").GetComponent<CharacterBody>().baseJumpPower * 1.5f;
			}
			return null;
		};
		CharacterBody.HandleOnKillEffectsServer += (hook_HandleOnKillEffectsServer)delegate(orig_HandleOnKillEffectsServer orig, CharacterBody self, DamageReport damageReport)
		{
			//IL_00a9: Unknown result type (might be due to invalid IL or missing references)
			//IL_00ae: Unknown result type (might be due to invalid IL or missing references)
			//IL_00be: Unknown result type (might be due to invalid IL or missing references)
			//IL_00c5: Unknown result type (might be due to invalid IL or missing references)
			//IL_00cb: Unknown result type (might be due to invalid IL or missing references)
			orig.Invoke(self, damageReport);
			if (NetworkServer.active && (Object)(object)damageReport.attackerBody.master.bodyPrefab == (Object)(object)BodyCatalog.FindBodyPrefab("MageBody") && damageReport.victimBody.isElite && damageReport.attackerBody.skillLocator.FindSkill("Passive").skillDef.skillName == elitefinder.skillName)
			{
				Random random = new Random();
				if (random.NextDouble() <= 0.05)
				{
					PickupDropletController.CreatePickupDroplet(PickupCatalog.FindPickupIndex(damageReport.victimBody.equipmentSlot.equipmentIndex), damageReport.victimBody.transform.position, default(Vector3));
				}
			}
		};
	}
}
