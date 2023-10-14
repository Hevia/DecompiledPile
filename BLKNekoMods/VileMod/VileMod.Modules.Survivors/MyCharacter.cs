using System;
using System.Collections.Generic;
using BepInEx.Configuration;
using EntityStates;
using RoR2;
using RoR2.Skills;
using UnityEngine;
using VileMod.Modules.Characters;
using VileMod.SkillStates;
using VileMod.SkillStates.BaseStates;

namespace VileMod.Modules.Survivors;

internal class MyCharacter : SurvivorBase
{
	public const string VILEV3_PREFIX = "BLKNeko_VILEV3_BODY_";

	private static UnlockableDef masterySkinUnlockableDef;

	public override string prefabBodyName => "VileV3";

	public override string survivorTokenPrefix => "BLKNeko_VILEV3_BODY_";

	public override BodyInfo bodyInfo { get; set; } = new BodyInfo
	{
		bodyName = "VileV3Body",
		bodyNameToken = "BLKNeko_VILEV3_BODY_NAME",
		subtitleNameToken = "BLKNeko_VILEV3_BODY_SUBTITLE",
		characterPortrait = Assets.VIcon,
		bodyColor = new Color(0.35f, 0.05f, 0.4f),
		crosshair = Resources.Load<GameObject>("Prefabs/Crosshair/SMGCrosshair"),
		podPrefab = LegacyResourcesAPI.Load<GameObject>("Prefabs/NetworkedObjects/SurvivorPod"),
		armor = 30f,
		armorGrowth = 1.8f,
		shieldGrowth = 0.25f,
		damage = 25f,
		healthGrowth = 25f,
		healthRegen = 1.8f,
		jumpCount = 1,
		maxHealth = 150f,
		attackSpeed = 0.85f,
		jumpPowerGrowth = 0.5f,
		jumpPower = 23f,
		moveSpeed = 5.5f
	};


	public override CustomRendererInfo[] customRendererInfos { get; set; } = new CustomRendererInfo[7]
	{
		new CustomRendererInfo
		{
			childName = "BodyMesh_C",
			material = Materials.CreateHopooMaterial("MatVile")
		},
		new CustomRendererInfo
		{
			childName = "BodyMesh_M",
			material = Materials.CreateHopooMaterial("MatVile")
		},
		new CustomRendererInfo
		{
			childName = "HandMesh_L_C",
			material = Materials.CreateHopooMaterial("MatVile")
		},
		new CustomRendererInfo
		{
			childName = "HandMesh_L_M",
			material = Materials.CreateHopooMaterial("MatVile")
		},
		new CustomRendererInfo
		{
			childName = "HandMesh_R_C",
			material = Materials.CreateHopooMaterial("MatVile")
		},
		new CustomRendererInfo
		{
			childName = "HandMesh_R_M",
			material = Materials.CreateHopooMaterial("MatVile")
		},
		new CustomRendererInfo
		{
			childName = "WeaponMesh_M",
			material = Materials.CreateHopooMaterial("MatVile")
		}
	};


	public override UnlockableDef characterUnlockableDef => null;

	public override Type characterMainState => typeof(Fury);

	public override Type characterDeathState => typeof(DeathState);

	public override Type characterSpawnState => typeof(SpawnState);

	public override ItemDisplaysBase itemDisplays => new HenryItemDisplays();

	public override ConfigEntry<bool> characterEnabledConfig => null;

	public override void InitializeCharacter()
	{
		base.InitializeCharacter();
	}

	public override void InitializeUnlockables()
	{
	}

	public override void InitializeHitboxes()
	{
		//IL_003d: Unknown result type (might be due to invalid IL or missing references)
		ChildLocator componentInChildren = bodyPrefab.GetComponentInChildren<ChildLocator>();
		GameObject gameObject = ((Component)componentInChildren).gameObject;
		Transform val = componentInChildren.FindChild("GroundBox");
		Prefabs.SetupHitbox(gameObject, val, "GroundBox");
		val.localScale = new Vector3(4f, 4f, 4f);
	}

	public override void InitializeSkills()
	{
		//IL_0075: Unknown result type (might be due to invalid IL or missing references)
		//IL_007a: Unknown result type (might be due to invalid IL or missing references)
		//IL_00c2: Unknown result type (might be due to invalid IL or missing references)
		//IL_0172: Unknown result type (might be due to invalid IL or missing references)
		//IL_0177: Unknown result type (might be due to invalid IL or missing references)
		//IL_01bf: Unknown result type (might be due to invalid IL or missing references)
		//IL_026f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0274: Unknown result type (might be due to invalid IL or missing references)
		//IL_02bc: Unknown result type (might be due to invalid IL or missing references)
		//IL_036c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0371: Unknown result type (might be due to invalid IL or missing references)
		//IL_03b9: Unknown result type (might be due to invalid IL or missing references)
		//IL_046b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0470: Unknown result type (might be due to invalid IL or missing references)
		//IL_04b8: Unknown result type (might be due to invalid IL or missing references)
		//IL_056a: Unknown result type (might be due to invalid IL or missing references)
		//IL_056f: Unknown result type (might be due to invalid IL or missing references)
		//IL_05b7: Unknown result type (might be due to invalid IL or missing references)
		//IL_0669: Unknown result type (might be due to invalid IL or missing references)
		//IL_066e: Unknown result type (might be due to invalid IL or missing references)
		//IL_06b6: Unknown result type (might be due to invalid IL or missing references)
		//IL_0768: Unknown result type (might be due to invalid IL or missing references)
		//IL_076d: Unknown result type (might be due to invalid IL or missing references)
		//IL_07b5: Unknown result type (might be due to invalid IL or missing references)
		//IL_0867: Unknown result type (might be due to invalid IL or missing references)
		//IL_086c: Unknown result type (might be due to invalid IL or missing references)
		//IL_08b4: Unknown result type (might be due to invalid IL or missing references)
		Skills.CreateSkillFamilies(bodyPrefab);
		string text = "BLKNeko_VILEV3_BODY_";
		Skills.PassiveSetup(bodyPrefab);
		SkillDefInfo skillDefInfo = new SkillDefInfo();
		skillDefInfo.skillName = text + "CHERRYBLAST_NAME";
		skillDefInfo.skillNameToken = text + "CHERRYBLAST_NAME";
		skillDefInfo.skillDescriptionToken = text + "CHERRYBLAST_DESCRIPTION";
		skillDefInfo.skillIcon = Assets.VCB;
		skillDefInfo.activationState = new SerializableEntityStateType(typeof(CherryBlast));
		skillDefInfo.activationStateMachineName = "Weapon";
		skillDefInfo.baseMaxStock = 1;
		skillDefInfo.baseRechargeInterval = 0f;
		skillDefInfo.beginSkillCooldownOnSkillEnd = false;
		skillDefInfo.canceledFromSprinting = false;
		skillDefInfo.forceSprintDuringState = false;
		skillDefInfo.fullRestockOnAssign = true;
		skillDefInfo.interruptPriority = (InterruptPriority)1;
		skillDefInfo.resetCooldownTimerOnUse = false;
		skillDefInfo.isCombatSkill = true;
		skillDefInfo.mustKeyPress = true;
		skillDefInfo.cancelSprintingOnActivation = false;
		skillDefInfo.rechargeStock = 0;
		skillDefInfo.requiredStock = 0;
		skillDefInfo.stockToConsume = 0;
		SkillDef val = Skills.CreateSkillDef(skillDefInfo);
		Skills.AddPrimarySkills(bodyPrefab, val);
		skillDefInfo = new SkillDefInfo();
		skillDefInfo.skillName = text + "TRIPLE7_NAME";
		skillDefInfo.skillNameToken = text + "TRIPLE7_NAME";
		skillDefInfo.skillDescriptionToken = text + "TRIPLE7_DESCRIPTION";
		skillDefInfo.skillIcon = Assets.VT7;
		skillDefInfo.activationState = new SerializableEntityStateType(typeof(Triple7));
		skillDefInfo.activationStateMachineName = "Weapon";
		skillDefInfo.baseMaxStock = 1;
		skillDefInfo.baseRechargeInterval = 0f;
		skillDefInfo.beginSkillCooldownOnSkillEnd = false;
		skillDefInfo.canceledFromSprinting = false;
		skillDefInfo.forceSprintDuringState = false;
		skillDefInfo.fullRestockOnAssign = true;
		skillDefInfo.interruptPriority = (InterruptPriority)1;
		skillDefInfo.resetCooldownTimerOnUse = false;
		skillDefInfo.isCombatSkill = true;
		skillDefInfo.mustKeyPress = true;
		skillDefInfo.cancelSprintingOnActivation = false;
		skillDefInfo.rechargeStock = 0;
		skillDefInfo.requiredStock = 0;
		skillDefInfo.stockToConsume = 0;
		SkillDef val2 = Skills.CreateSkillDef(skillDefInfo);
		Skills.AddPrimarySkills(bodyPrefab, val2);
		skillDefInfo = new SkillDefInfo();
		skillDefInfo.skillName = text + "BUMPITYBOOM_NAME";
		skillDefInfo.skillNameToken = text + "BUMPITYBOOM_NAME";
		skillDefInfo.skillDescriptionToken = text + "BUMPITYBOOM_DESCRIPTION";
		skillDefInfo.skillIcon = Assets.VBB;
		skillDefInfo.activationState = new SerializableEntityStateType(typeof(BumpityBoom));
		skillDefInfo.activationStateMachineName = "Weapon";
		skillDefInfo.baseMaxStock = 3;
		skillDefInfo.baseRechargeInterval = 6f;
		skillDefInfo.beginSkillCooldownOnSkillEnd = false;
		skillDefInfo.canceledFromSprinting = false;
		skillDefInfo.forceSprintDuringState = false;
		skillDefInfo.fullRestockOnAssign = true;
		skillDefInfo.interruptPriority = (InterruptPriority)1;
		skillDefInfo.resetCooldownTimerOnUse = false;
		skillDefInfo.isCombatSkill = false;
		skillDefInfo.mustKeyPress = true;
		skillDefInfo.cancelSprintingOnActivation = false;
		skillDefInfo.rechargeStock = 1;
		skillDefInfo.requiredStock = 1;
		skillDefInfo.stockToConsume = 1;
		SkillDef val3 = Skills.CreateSkillDef(skillDefInfo);
		Skills.AddSecondarySkills(bodyPrefab, val3);
		skillDefInfo = new SkillDefInfo();
		skillDefInfo.skillName = text + "FRONTRUNNER_NAME";
		skillDefInfo.skillNameToken = text + "FRONTRUNNER_NAME";
		skillDefInfo.skillDescriptionToken = text + "FRONTRUNNER_DESCRIPTION";
		skillDefInfo.skillIcon = Assets.VFR;
		skillDefInfo.activationState = new SerializableEntityStateType(typeof(FrontRunner));
		skillDefInfo.activationStateMachineName = "Weapon";
		skillDefInfo.baseMaxStock = 5;
		skillDefInfo.baseRechargeInterval = 7f;
		skillDefInfo.beginSkillCooldownOnSkillEnd = false;
		skillDefInfo.canceledFromSprinting = false;
		skillDefInfo.forceSprintDuringState = false;
		skillDefInfo.fullRestockOnAssign = true;
		skillDefInfo.interruptPriority = (InterruptPriority)1;
		skillDefInfo.resetCooldownTimerOnUse = false;
		skillDefInfo.isCombatSkill = true;
		skillDefInfo.mustKeyPress = true;
		skillDefInfo.cancelSprintingOnActivation = false;
		skillDefInfo.rechargeStock = 1;
		skillDefInfo.requiredStock = 1;
		skillDefInfo.stockToConsume = 1;
		SkillDef val4 = Skills.CreateSkillDef(skillDefInfo);
		Skills.AddSecondarySkills(bodyPrefab, val4);
		skillDefInfo = new SkillDefInfo();
		skillDefInfo.skillName = text + "NAPALMBOMB_NAME";
		skillDefInfo.skillNameToken = text + "NAPALMBOMB_NAME";
		skillDefInfo.skillDescriptionToken = text + "NAPALMBOMB_DESCRIPTION";
		skillDefInfo.skillIcon = Assets.VNB;
		skillDefInfo.activationState = new SerializableEntityStateType(typeof(NapalmBomb));
		skillDefInfo.activationStateMachineName = "Weapon";
		skillDefInfo.baseMaxStock = 2;
		skillDefInfo.baseRechargeInterval = 8f;
		skillDefInfo.beginSkillCooldownOnSkillEnd = false;
		skillDefInfo.canceledFromSprinting = false;
		skillDefInfo.forceSprintDuringState = false;
		skillDefInfo.fullRestockOnAssign = true;
		skillDefInfo.interruptPriority = (InterruptPriority)1;
		skillDefInfo.resetCooldownTimerOnUse = false;
		skillDefInfo.isCombatSkill = true;
		skillDefInfo.mustKeyPress = true;
		skillDefInfo.cancelSprintingOnActivation = false;
		skillDefInfo.rechargeStock = 1;
		skillDefInfo.requiredStock = 1;
		skillDefInfo.stockToConsume = 1;
		SkillDef val5 = Skills.CreateSkillDef(skillDefInfo);
		Skills.AddSecondarySkills(bodyPrefab, val5);
		skillDefInfo = new SkillDefInfo();
		skillDefInfo.skillName = text + "ELETRICSPARK_NAME";
		skillDefInfo.skillNameToken = text + "ELETRICSPARK_NAME";
		skillDefInfo.skillDescriptionToken = text + "ELETRICSPARK_DESCRIPTION";
		skillDefInfo.skillIcon = Assets.VES;
		skillDefInfo.activationState = new SerializableEntityStateType(typeof(EletricSpark));
		skillDefInfo.activationStateMachineName = "Weapon";
		skillDefInfo.baseMaxStock = 1;
		skillDefInfo.baseRechargeInterval = 9f;
		skillDefInfo.beginSkillCooldownOnSkillEnd = false;
		skillDefInfo.canceledFromSprinting = true;
		skillDefInfo.forceSprintDuringState = false;
		skillDefInfo.fullRestockOnAssign = true;
		skillDefInfo.interruptPriority = (InterruptPriority)1;
		skillDefInfo.resetCooldownTimerOnUse = false;
		skillDefInfo.isCombatSkill = true;
		skillDefInfo.mustKeyPress = true;
		skillDefInfo.cancelSprintingOnActivation = true;
		skillDefInfo.rechargeStock = 1;
		skillDefInfo.requiredStock = 1;
		skillDefInfo.stockToConsume = 1;
		SkillDef val6 = Skills.CreateSkillDef(skillDefInfo);
		Skills.AddUtilitySkills(bodyPrefab, val6);
		skillDefInfo = new SkillDefInfo();
		skillDefInfo.skillName = text + "SHOTGUNICE_NAME";
		skillDefInfo.skillNameToken = text + "SHOTGUNICE_NAME";
		skillDefInfo.skillDescriptionToken = text + "SHOTGUNICE_DESCRIPTION";
		skillDefInfo.skillIcon = Assets.VSI;
		skillDefInfo.activationState = new SerializableEntityStateType(typeof(ShotgunIce));
		skillDefInfo.activationStateMachineName = "Weapon";
		skillDefInfo.baseMaxStock = 2;
		skillDefInfo.baseRechargeInterval = 8f;
		skillDefInfo.beginSkillCooldownOnSkillEnd = false;
		skillDefInfo.canceledFromSprinting = true;
		skillDefInfo.forceSprintDuringState = false;
		skillDefInfo.fullRestockOnAssign = true;
		skillDefInfo.interruptPriority = (InterruptPriority)1;
		skillDefInfo.resetCooldownTimerOnUse = false;
		skillDefInfo.isCombatSkill = true;
		skillDefInfo.mustKeyPress = true;
		skillDefInfo.cancelSprintingOnActivation = true;
		skillDefInfo.rechargeStock = 1;
		skillDefInfo.requiredStock = 1;
		skillDefInfo.stockToConsume = 1;
		SkillDef val7 = Skills.CreateSkillDef(skillDefInfo);
		Skills.AddUtilitySkills(bodyPrefab, val7);
		skillDefInfo = new SkillDefInfo();
		skillDefInfo.skillName = text + "BURNINGDRIVE_NAME";
		skillDefInfo.skillNameToken = text + "BURNINGDRIVE_NAME";
		skillDefInfo.skillDescriptionToken = text + "BURNINGDRIVE_DESCRIPTION";
		skillDefInfo.skillIcon = Assets.VBD;
		skillDefInfo.activationState = new SerializableEntityStateType(typeof(BurningDrive));
		skillDefInfo.activationStateMachineName = "Body";
		skillDefInfo.baseMaxStock = 1;
		skillDefInfo.baseRechargeInterval = 7f;
		skillDefInfo.beginSkillCooldownOnSkillEnd = true;
		skillDefInfo.canceledFromSprinting = true;
		skillDefInfo.forceSprintDuringState = false;
		skillDefInfo.fullRestockOnAssign = true;
		skillDefInfo.interruptPriority = (InterruptPriority)1;
		skillDefInfo.resetCooldownTimerOnUse = false;
		skillDefInfo.isCombatSkill = true;
		skillDefInfo.mustKeyPress = true;
		skillDefInfo.cancelSprintingOnActivation = true;
		skillDefInfo.rechargeStock = 1;
		skillDefInfo.requiredStock = 1;
		skillDefInfo.stockToConsume = 1;
		SkillDef val8 = Skills.CreateSkillDef(skillDefInfo);
		Skills.AddSpecialSkills(bodyPrefab, val8);
		skillDefInfo = new SkillDefInfo();
		skillDefInfo.skillName = text + "CERBERUSPHANTOM_NAME";
		skillDefInfo.skillNameToken = text + "CERBERUSPHANTOM_NAME";
		skillDefInfo.skillDescriptionToken = text + "CERBERUSPHANTOM_DESCRIPTION";
		skillDefInfo.skillIcon = Assets.VCP;
		skillDefInfo.activationState = new SerializableEntityStateType(typeof(CerberusPhantom));
		skillDefInfo.activationStateMachineName = "Weapon";
		skillDefInfo.baseMaxStock = 3;
		skillDefInfo.baseRechargeInterval = 6f;
		skillDefInfo.beginSkillCooldownOnSkillEnd = false;
		skillDefInfo.canceledFromSprinting = false;
		skillDefInfo.forceSprintDuringState = false;
		skillDefInfo.fullRestockOnAssign = true;
		skillDefInfo.interruptPriority = (InterruptPriority)1;
		skillDefInfo.resetCooldownTimerOnUse = false;
		skillDefInfo.isCombatSkill = true;
		skillDefInfo.mustKeyPress = true;
		skillDefInfo.cancelSprintingOnActivation = true;
		skillDefInfo.rechargeStock = 1;
		skillDefInfo.requiredStock = 1;
		skillDefInfo.stockToConsume = 1;
		SkillDef val9 = Skills.CreateSkillDef(skillDefInfo);
		Skills.AddSpecialSkills(bodyPrefab, val9);
	}

	public override void InitializeSkins()
	{
		ModelSkinController val = ((Component)prefabCharacterModel).gameObject.AddComponent<ModelSkinController>();
		ChildLocator component = ((Component)prefabCharacterModel).GetComponent<ChildLocator>();
		RendererInfo[] baseRendererInfos = prefabCharacterModel.baseRendererInfos;
		List<SkinDef> list = new List<SkinDef>();
		string text = "BLKNeko_VILEV3_BODY_";
		SkinDef item = Skins.CreateSkinDef(text + "DEFAULT_SKIN_NAME", Assets.VSkin, baseRendererInfos, ((Component)prefabCharacterModel).gameObject);
		list.Add(item);
		val.skins = list.ToArray();
	}
}
