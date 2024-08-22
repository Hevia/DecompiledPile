using System;
using System.IO;
using BanditsPrimaryRevolver.MyEntityStates;
using BepInEx;
using EntityStates;
using R2API;
using RoR2;
using RoR2.Skills;
using UnityEngine;
using UnityEngine.AddressableAssets;

namespace BanditsPrimaryRevolver;

[BepInDependency(/*Could not decode attribute arguments.*/)]
[BepInDependency(/*Could not decode attribute arguments.*/)]
[BepInPlugin("Icarus", "Bandit's Primary Revolver", "1.0.0")]
public class BanditsPrimaryRevolver : BaseUnityPlugin
{
	public static class Assets
	{
		public static AssetBundle mainBundle;

		public const string bundleName = "asseticon";

		public static string AssetBundlePath
		{
			get
			{
				string directoryName = Path.GetDirectoryName(PInfo.Location);
				return Path.Combine(directoryName, "asseticon");
			}
		}

		public static void Init()
		{
			mainBundle = AssetBundle.LoadFromFile(AssetBundlePath);
		}
	}

	public static PluginInfo PInfo { get; private set; }

	public void Awake()
	{
		//IL_0028: Unknown result type (might be due to invalid IL or missing references)
		//IL_002d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0068: Unknown result type (might be due to invalid IL or missing references)
		//IL_006d: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ad: Unknown result type (might be due to invalid IL or missing references)
		//IL_0152: Unknown result type (might be due to invalid IL or missing references)
		//IL_016a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0174: Expected O, but got Unknown
		//IL_0175: Unknown result type (might be due to invalid IL or missing references)
		//IL_0177: Unknown result type (might be due to invalid IL or missing references)
		PInfo = ((BaseUnityPlugin)this).Info;
		Assets.Init();
		Sprite icon = Assets.mainBundle.LoadAsset<Sprite>("SkillIcon");
		GameObject val = Addressables.LoadAssetAsync<GameObject>((object)"RoR2/Base/Bandit2/Bandit2Body.prefab").WaitForCompletion();
		LanguageAPI.Add("BANDIT2_PRIMARY_REVOLVER_NAME", "Revolver");
		LanguageAPI.Add("BANDIT2_PRIMARY_REVOLVER_DESCRIPTION", "Fire a bullet from your Revolver for <style=cIsDamage>600% damage</style>.");
		SkillDef val2 = ScriptableObject.CreateInstance<SkillDef>();
		val2.activationState = new SerializableEntityStateType(typeof(Revolver));
		val2.activationStateMachineName = "Weapon";
		val2.baseMaxStock = 6;
		val2.baseRechargeInterval = 2.5f;
		val2.beginSkillCooldownOnSkillEnd = false;
		val2.canceledFromSprinting = false;
		val2.cancelSprintingOnActivation = true;
		val2.fullRestockOnAssign = true;
		val2.interruptPriority = (InterruptPriority)0;
		val2.isCombatSkill = true;
		val2.mustKeyPress = false;
		val2.rechargeStock = 6;
		val2.requiredStock = 1;
		val2.stockToConsume = 1;
		val2.resetCooldownTimerOnUse = true;
		val2.beginSkillCooldownOnSkillEnd = true;
		val2.icon = icon;
		val2.skillDescriptionToken = "BANDIT2_PRIMARY_REVOLVER_DESCRIPTION";
		val2.skillName = "BANDIT2_PRIMARY_REVOLVER_NAME";
		val2.skillNameToken = "BANDIT2_PRIMARY_REVOLVER_NAME";
		ContentAddition.AddSkillDef(val2);
		SkillLocator component = val.GetComponent<SkillLocator>();
		SkillFamily skillFamily = component.primary.skillFamily;
		Array.Resize(ref skillFamily.variants, skillFamily.variants.Length + 1);
		Variant[] variants = skillFamily.variants;
		int num = skillFamily.variants.Length - 1;
		Variant val3 = new Variant
		{
			skillDef = val2
		};
		((Variant)(ref val3)).viewableNode = new Node(val2.skillNameToken, false, (Node)null);
		variants[num] = val3;
	}
}
