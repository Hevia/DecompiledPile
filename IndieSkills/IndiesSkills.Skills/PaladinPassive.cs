using System;
using IndiesSkills.Properties;
using On.RoR2;
using PaladinMod;
using PaladinMod.Modules;
using R2API;
using RoR2;
using RoR2.Skills;
using UnityEngine;

namespace IndiesSkills.Skills;

internal class PaladinPassive
{
	public static void addPaladinPassives()
	{
		//IL_00e1: Unknown result type (might be due to invalid IL or missing references)
		//IL_010f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0119: Expected O, but got Unknown
		//IL_011a: Unknown result type (might be due to invalid IL or missing references)
		//IL_011b: Unknown result type (might be due to invalid IL or missing references)
		//IL_01b8: Unknown result type (might be due to invalid IL or missing references)
		//IL_01e6: Unknown result type (might be due to invalid IL or missing references)
		//IL_01f0: Expected O, but got Unknown
		//IL_01f1: Unknown result type (might be due to invalid IL or missing references)
		//IL_01f2: Unknown result type (might be due to invalid IL or missing references)
		//IL_01fe: Unknown result type (might be due to invalid IL or missing references)
		//IL_0208: Expected O, but got Unknown
		LanguageAPI.Add("PALADIN_PASSIVE_DESCRIPTION_MODIFIED", "While above <style=cIsHealth>90% health</style> or while having active <style=cIsHealth>barrier</style>, the Paladin is <style=cIsHealing>blessed</style>, empowering some sword skills.");
		PaladinPlugin.characterPrefab.GetComponent<SkillLocator>().passiveSkill.skillDescriptionToken = "PALADIN_PASSIVE_DESCRIPTION_MODIFIED";
		GenericSkill val = Prefabs.paladinPrefab.AddComponent<GenericSkill>();
		val.skillName = "Passive";
		SkillFamily val2 = (val._skillFamily = ScriptableObject.CreateInstance<SkillFamily>());
		val2.variants = (Variant[])(object)new Variant[1];
		LanguageAPI.Add("PALADIN_PASSIVE_BULWARK_NAME", "Stalwart");
		LanguageAPI.Add("PALADIN_PASSIVE_BULWARK_DESCRIPTION", "Gain <style=cIsHealth>+1 armor</style> per level.");
		SkillDef bulwark = ScriptableObject.CreateInstance<SkillDef>();
		bulwark.icon = Main.LoadTexture2D(Resources.Stalwart_Icon);
		bulwark.skillDescriptionToken = "PALADIN_PASSIVE_BULWARK_DESCRIPTION";
		bulwark.skillName = "PALADIN_PASSIVE_BULWARK_NAME";
		bulwark.skillNameToken = "PALADIN_PASSIVE_BULWARK_NAME";
		Variant[] variants = val2.variants;
		int num = val2.variants.Length - 1;
		Variant val3 = new Variant
		{
			skillDef = bulwark,
			unlockableName = ""
		};
		((Variant)(ref val3)).viewableNode = new Node(bulwark.skillNameToken, false, (Node)null);
		variants[num] = val3;
		LanguageAPI.Add("PALADIN_PASSIVE_KINGSBEQUEATHMENT_NAME", "Speed is War");
		LanguageAPI.Add("PALADIN_PASSIVE_KINGSBEQUEATHMENT_DESCRIPTION", "Gain <style=cIsDamage>1% speed and attack speed</style> per level.");
		SkillDef kingsgift = ScriptableObject.CreateInstance<SkillDef>();
		kingsgift.icon = Resources.Load<Sprite>("NotAnActualPath");
		kingsgift.skillDescriptionToken = "PALADIN_PASSIVE_KINGSBEQUEATHMENT_DESCRIPTION";
		kingsgift.skillName = "PALADIN_PASSIVE_KINGSBEQUEATHMENT_NAME";
		kingsgift.skillNameToken = "PALADIN_PASSIVE_KINGSBEQUEATHMENT_NAME";
		Array.Resize(ref val2.variants, val2.variants.Length + 1);
		Variant[] variants2 = val2.variants;
		int num2 = val2.variants.Length - 1;
		val3 = new Variant
		{
			skillDef = kingsgift,
			unlockableName = ""
		};
		((Variant)(ref val3)).viewableNode = new Node(kingsgift.skillNameToken, false, (Node)null);
		variants2[num2] = val3;
		CharacterMaster.SpawnBody += (hook_SpawnBody)delegate(orig_SpawnBody orig, CharacterMaster self, Vector3 pos, Quaternion rot)
		{
			//IL_0003: Unknown result type (might be due to invalid IL or missing references)
			//IL_0004: Unknown result type (might be due to invalid IL or missing references)
			orig.Invoke(self, pos, rot);
			if ((Object)(object)self.bodyPrefab == (Object)(object)Prefabs.paladinPrefab && !(self.GetBody().skillLocator.FindSkill("Passive").skillDef.skillName == bulwark.skillName) && self.GetBody().skillLocator.FindSkill("Passive").skillDef.skillName == kingsgift.skillName)
			{
				self.GetBody().levelArmor = 0f;
				self.GetBody().levelAttackSpeed = self.GetBody().baseAttackSpeed * 0.01f;
				self.GetBody().levelMoveSpeed = self.GetBody().baseMoveSpeed * 0.01f;
				self.GetBody().RecalculateStats();
			}
			return null;
		};
	}
}
