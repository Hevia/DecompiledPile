using System;
using System.Collections.Generic;
using System.Linq;
using BepInEx;
using BepInEx.Configuration;
using EntityStates;
using KinematicCharacterController;
using R2API;
using R2API.Utils;
using RoR2;
using RoR2.Skills;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.Rendering;

namespace Rorschach;

[BepInDependency(/*Could not decode attribute arguments.*/)]
[BepInPlugin("com.Dragonyck.Rorschach", "Rorschach", "1.0.0")]
[NetworkCompatibility(/*Could not decode attribute arguments.*/)]
public class MainPlugin : BaseUnityPlugin
{
	public const string MODUID = "com.Dragonyck.Rorschach";

	public const string MODNAME = "Rorschach";

	public const string VERSION = "1.0.0";

	public const string SURVIVORNAME = "Rorschach";

	public const string SURVIVORNAMEKEY = "RORSCHACH";

	public static GameObject characterPrefab;

	public GameObject characterDisplay;

	public GameObject doppelganger;

	private static readonly Color characterColor = new Color(0.55686f, 0.38824f, 0.27451f);

	internal static SkillDef PrimaryRageDef;

	internal static SkillDef SecondaryRageDef;

	internal static SkillDef UtilityRageDef;

	internal static SkillDef SpecialRageDef;

	internal static ConfigEntry<KeyCode> emote1Key;

	internal static ConfigEntry<KeyCode> emote2Key;

	private void Awake()
	{
		//IL_0011: Unknown result type (might be due to invalid IL or missing references)
		//IL_0023: Unknown result type (might be due to invalid IL or missing references)
		//IL_002d: Expected O, but got Unknown
		//IL_002d: Expected O, but got Unknown
		//IL_0042: Unknown result type (might be due to invalid IL or missing references)
		//IL_0054: Unknown result type (might be due to invalid IL or missing references)
		//IL_005e: Expected O, but got Unknown
		//IL_005e: Expected O, but got Unknown
		emote1Key = ((BaseUnityPlugin)this).Config.Bind<KeyCode>(new ConfigDefinition("Emote 1 Keybind", "Keybind"), (KeyCode)49, new ConfigDescription("", (AcceptableValueBase)null, Array.Empty<object>()));
		emote2Key = ((BaseUnityPlugin)this).Config.Bind<KeyCode>(new ConfigDefinition("Emote 2 Keybind", "Keybind"), (KeyCode)50, new ConfigDescription("", (AcceptableValueBase)null, Array.Empty<object>()));
		Assets.PopulateAssets();
		Prefabs.CreatePrefabs();
		CreatePrefab();
		RegisterStates();
		RegisterCharacter();
		CreateDoppelganger();
		Hook.Hooks();
	}

	private static GameObject CreateModel(GameObject main)
	{
		Object.Destroy((Object)(object)((Component)main.transform.Find("ModelBase")).gameObject);
		Object.Destroy((Object)(object)((Component)main.transform.Find("CameraPivot")).gameObject);
		Object.Destroy((Object)(object)((Component)main.transform.Find("AimOrigin")).gameObject);
		return Assets.MainAssetBundle.LoadAsset<GameObject>("rors");
	}

	internal static void CreatePrefab()
	{
		//IL_0052: Unknown result type (might be due to invalid IL or missing references)
		//IL_0058: Expected O, but got Unknown
		//IL_0083: Unknown result type (might be due to invalid IL or missing references)
		//IL_0094: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b4: Unknown result type (might be due to invalid IL or missing references)
		//IL_00c4: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ca: Expected O, but got Unknown
		//IL_00f1: Unknown result type (might be due to invalid IL or missing references)
		//IL_0102: Unknown result type (might be due to invalid IL or missing references)
		//IL_0113: Unknown result type (might be due to invalid IL or missing references)
		//IL_014e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0264: Unknown result type (might be due to invalid IL or missing references)
		//IL_0299: Unknown result type (might be due to invalid IL or missing references)
		//IL_029e: Unknown result type (might be due to invalid IL or missing references)
		//IL_02bb: Unknown result type (might be due to invalid IL or missing references)
		//IL_02d7: Unknown result type (might be due to invalid IL or missing references)
		//IL_02e4: Unknown result type (might be due to invalid IL or missing references)
		//IL_02fd: Unknown result type (might be due to invalid IL or missing references)
		//IL_0302: Unknown result type (might be due to invalid IL or missing references)
		//IL_03a4: Unknown result type (might be due to invalid IL or missing references)
		//IL_03a9: Unknown result type (might be due to invalid IL or missing references)
		//IL_03df: Unknown result type (might be due to invalid IL or missing references)
		//IL_03e4: Unknown result type (might be due to invalid IL or missing references)
		//IL_0466: Unknown result type (might be due to invalid IL or missing references)
		//IL_05bf: Unknown result type (might be due to invalid IL or missing references)
		//IL_05fa: Unknown result type (might be due to invalid IL or missing references)
		//IL_060e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0622: Unknown result type (might be due to invalid IL or missing references)
		//IL_0636: Unknown result type (might be due to invalid IL or missing references)
		//IL_0691: Unknown result type (might be due to invalid IL or missing references)
		//IL_0888: Unknown result type (might be due to invalid IL or missing references)
		//IL_089f: Unknown result type (might be due to invalid IL or missing references)
		//IL_08a4: Unknown result type (might be due to invalid IL or missing references)
		//IL_08ac: Unknown result type (might be due to invalid IL or missing references)
		//IL_090e: Unknown result type (might be due to invalid IL or missing references)
		//IL_098e: Unknown result type (might be due to invalid IL or missing references)
		//IL_09b3: Unknown result type (might be due to invalid IL or missing references)
		//IL_0a20: Unknown result type (might be due to invalid IL or missing references)
		//IL_0a25: Unknown result type (might be due to invalid IL or missing references)
		//IL_0ace: Unknown result type (might be due to invalid IL or missing references)
		//IL_0ad3: Unknown result type (might be due to invalid IL or missing references)
		//IL_04fa: Unknown result type (might be due to invalid IL or missing references)
		//IL_0520: Unknown result type (might be due to invalid IL or missing references)
		//IL_052d: Unknown result type (might be due to invalid IL or missing references)
		//IL_052f: Unknown result type (might be due to invalid IL or missing references)
		//IL_048e: Unknown result type (might be due to invalid IL or missing references)
		//IL_04ac: Unknown result type (might be due to invalid IL or missing references)
		//IL_04c5: Unknown result type (might be due to invalid IL or missing references)
		//IL_04e3: Unknown result type (might be due to invalid IL or missing references)
		//IL_04f0: Unknown result type (might be due to invalid IL or missing references)
		//IL_04f2: Unknown result type (might be due to invalid IL or missing references)
		//IL_0534: Unknown result type (might be due to invalid IL or missing references)
		characterPrefab = PrefabAPI.InstantiateClone(LegacyResourcesAPI.Load<GameObject>("Prefabs/CharacterBodies/CommandoBody"), "RorschachBody", true);
		characterPrefab.GetComponent<NetworkIdentity>().localPlayerAuthority = true;
		characterPrefab.AddComponent<RorschachRageBarBehaviour>();
		characterPrefab.AddComponent<GrabBehaviour>();
		GameObject val = CreateModel(characterPrefab);
		GameObject val2 = new GameObject("ModelBase");
		val2.transform.parent = characterPrefab.transform;
		val2.transform.localPosition = new Vector3(0f, -0.94f, 0f);
		val2.transform.localRotation = Quaternion.identity;
		val2.transform.localScale = new Vector3(1f, 1f, 1f);
		GameObject val3 = new GameObject("AimOrigin");
		val3.transform.parent = val2.transform;
		val3.transform.localPosition = new Vector3(0f, 2f, 0f);
		val3.transform.localRotation = Quaternion.identity;
		val3.transform.localScale = Vector3.one;
		CharacterBody component = characterPrefab.GetComponent<CharacterBody>();
		((Object)component).name = "RorschachBody";
		component.baseNameToken = "RORSCHACH_NAME";
		component.subtitleNameToken = "RORSCHACH_SUBTITLE";
		component.bodyFlags = (BodyFlags)16;
		component.rootMotionInMainState = false;
		component.mainRootSpeed = 0f;
		component.baseMaxHealth = 130f;
		component.levelMaxHealth = 35f;
		component.baseRegen = 1f;
		component.levelRegen = 0.33f;
		component.baseMaxShield = 0f;
		component.levelMaxShield = 0f;
		component.baseMoveSpeed = 7f;
		component.levelMoveSpeed = 0f;
		component.baseAcceleration = 110f;
		component.baseJumpPower = 15f;
		component.levelJumpPower = 0f;
		component.baseDamage = 12f;
		component.levelDamage = 2.4f;
		component.baseAttackSpeed = 1f;
		component.levelAttackSpeed = 0f;
		component.baseCrit = 1f;
		component.levelCrit = 0f;
		component.baseArmor = 20f;
		component.levelArmor = 0f;
		component.baseJumpCount = 1;
		component.sprintingSpeedMultiplier = 1.45f;
		component.wasLucky = false;
		component.hideCrosshair = false;
		component.aimOriginTransform = val3.transform;
		component.hullClassification = (HullClassification)0;
		component.portraitIcon = (Texture)(object)Assets.MainAssetBundle.LoadAsset<Sprite>("portrait").texture;
		component.isChampion = false;
		component.currentVehicle = null;
		component.skinIndex = 0u;
		component.bodyColor = characterColor;
		Transform transform = val.transform;
		transform.parent = val2.transform;
		transform.localPosition = Vector3.zero;
		transform.localScale = new Vector3(1f, 1f, 1f);
		transform.localRotation = Quaternion.identity;
		CharacterDirection component2 = characterPrefab.GetComponent<CharacterDirection>();
		component2.moveVector = Vector3.zero;
		component2.targetTransform = val2.transform;
		component2.overrideAnimatorForwardTransform = null;
		component2.rootMotionAccumulator = null;
		component2.modelAnimator = val.GetComponentInChildren<Animator>();
		component2.driveFromRootRotation = false;
		component2.turnSpeed = 720f;
		CharacterMotor component3 = characterPrefab.GetComponent<CharacterMotor>();
		component3.walkSpeedPenaltyCoefficient = 1f;
		component3.characterDirection = component2;
		component3.muteWalkMotion = false;
		component3.mass = 160f;
		component3.airControl = 0.25f;
		component3.disableAirControlUntilCollision = false;
		component3.generateParametersOnAwake = true;
		InputBankTest component4 = characterPrefab.GetComponent<InputBankTest>();
		component4.moveVector = Vector3.zero;
		CameraTargetParams component5 = characterPrefab.GetComponent<CameraTargetParams>();
		component5.cameraParams = LegacyResourcesAPI.Load<GameObject>("Prefabs/CharacterBodies/MercBody").GetComponent<CameraTargetParams>().cameraParams;
		component5.cameraPivotTransform = null;
		component5.recoil = Vector2.zero;
		component5.dontRaycastToPivot = false;
		ModelLocator component6 = characterPrefab.GetComponent<ModelLocator>();
		component6.modelTransform = transform;
		component6.modelBaseTransform = val2.transform;
		component6.dontReleaseModelOnDeath = false;
		component6.autoUpdateModelTransform = true;
		component6.dontDetatchFromParent = false;
		component6.noCorpse = false;
		component6.normalizeToFloor = false;
		component6.preserveModel = false;
		ChildLocator component7 = val.GetComponent<ChildLocator>();
		SkinnedMeshRenderer[] componentsInChildren = val.GetComponentsInChildren<SkinnedMeshRenderer>();
		List<RendererInfo> list = new List<RendererInfo>();
		for (int i = 0; i < componentsInChildren.Length; i++)
		{
			RendererInfo val4 = default(RendererInfo);
			if (((Object)((Component)componentsInChildren[i]).gameObject).name.Contains("outline"))
			{
				RendererInfo val5 = default(RendererInfo);
				val5.renderer = (Renderer)(object)componentsInChildren[i];
				val5.defaultMaterial = Utils.InstantiateMaterial(((Renderer)componentsInChildren[i]).material.color, ((Renderer)componentsInChildren[i]).material.GetTexture("_MainTex"), Color.black, 0f, null, 1f, null);
				val5.defaultShadowCastingMode = (ShadowCastingMode)1;
				val5.ignoreOverlays = false;
				val4 = val5;
			}
			else
			{
				RendererInfo val5 = default(RendererInfo);
				val5.renderer = (Renderer)(object)componentsInChildren[i];
				val5.defaultMaterial = ((Renderer)componentsInChildren[i]).material;
				val5.defaultShadowCastingMode = (ShadowCastingMode)0;
				val5.ignoreOverlays = true;
				val4 = val5;
			}
			list.Add(val4);
		}
		CharacterModel val6 = val.AddComponent<CharacterModel>();
		val6.baseRendererInfos = list.ToArray();
		val6.autoPopulateLightInfos = true;
		val6.invisibilityCount = 0;
		val6.temporaryOverlays = new List<TemporaryOverlay>();
		Reflection.SetFieldValue<SkinnedMeshRenderer>((object)val6, "mainSkinnedMeshRenderer", componentsInChildren[0]);
		SkinnedMeshRenderer fieldValue = Reflection.GetFieldValue<SkinnedMeshRenderer>((object)val6, "mainSkinnedMeshRenderer");
		ModelSkinController val7 = val.AddComponent<ModelSkinController>();
		LanguageAPI.Add("RORSCHACHMODBODY_DEFAULT_SKIN_NAME", "Default");
		SkinDefInfo val8 = default(SkinDefInfo);
		val8.BaseSkins = Array.Empty<SkinDef>();
		val8.MinionSkinReplacements = (MinionSkinReplacement[])(object)new MinionSkinReplacement[0];
		val8.ProjectileGhostReplacements = (ProjectileGhostReplacement[])(object)new ProjectileGhostReplacement[0];
		val8.GameObjectActivations = (GameObjectActivation[])(object)new GameObjectActivation[0];
		val8.Icon = LoadoutAPI.CreateSkinIcon(Color.white, new Color(0.55686f, 0.38824f, 0.27451f), new Color(0.3451f, 0.21569f, 0.18431f), new Color(0.37255f, 0.25098f, 0.27059f));
		RendererInfo[] baseRendererInfos = val6.baseRendererInfos;
		val8.MeshReplacements = (MeshReplacement[])(object)new MeshReplacement[0];
		val8.Name = "RORSCHACHMODBODY_DEFAULT_SKIN_NAME";
		val8.NameToken = "RORSCHACHMODBODY_DEFAULT_SKIN_NAME";
		val8.RendererInfos = val6.baseRendererInfos;
		val8.RootObject = val;
		val8.UnlockableDef = null;
		SkinDef val9 = LoadoutAPI.CreateNewSkinDef(val8);
		val7.skins = (SkinDef[])(object)new SkinDef[1] { val9 };
		HealthComponent component8 = characterPrefab.GetComponent<HealthComponent>();
		component8.health = 110f;
		component8.shield = 0f;
		component8.barrier = 0f;
		component8.magnetiCharge = 0f;
		component8.body = null;
		component8.dontShowHealthbar = false;
		component8.globalDeathEventChanceCoefficient = 1f;
		characterPrefab.GetComponent<Interactor>().maxInteractionDistance = 3f;
		characterPrefab.GetComponent<InteractionDriver>().highlightInteractor = true;
		SfxLocator component9 = characterPrefab.GetComponent<SfxLocator>();
		component9.deathSound = "Play_ui_player_death";
		component9.barkSound = "";
		component9.openSound = "";
		component9.landingSound = "Play_char_land";
		component9.fallDamageSound = "Play_char_land_fall_damage";
		component9.aliveLoopStart = "";
		component9.aliveLoopStop = "";
		Rigidbody component10 = characterPrefab.GetComponent<Rigidbody>();
		component10.mass = 100f;
		component10.drag = 0f;
		component10.angularDrag = 0f;
		component10.useGravity = false;
		component10.isKinematic = true;
		component10.interpolation = (RigidbodyInterpolation)0;
		component10.collisionDetectionMode = (CollisionDetectionMode)0;
		component10.constraints = (RigidbodyConstraints)0;
		CapsuleCollider component11 = characterPrefab.GetComponent<CapsuleCollider>();
		((Collider)component11).isTrigger = false;
		((Collider)component11).material = null;
		KinematicCharacterMotor component12 = characterPrefab.GetComponent<KinematicCharacterMotor>();
		component12.CharacterController = (BaseCharacterController)(object)component3;
		component12.Capsule = component11;
		component12.Rigidbody = component10;
		component12.DetectDiscreteCollisions = false;
		component12.GroundDetectionExtraDistance = 0f;
		component12.MaxStepHeight = 0.2f;
		component12.MinRequiredStepDepth = 0.1f;
		component12.MaxStableSlopeAngle = 55f;
		component12.MaxStableDistanceFromLedge = 0.5f;
		component12.PreventSnappingOnLedges = false;
		component12.MaxStableDenivelationAngle = 55f;
		component12.RigidbodyInteractionType = (RigidbodyInteractionType)0;
		component12.PreserveAttachedRigidbodyMomentum = true;
		component12.HasPlanarConstraint = false;
		component12.PlanarConstraintAxis = Vector3.up;
		component12.StepHandling = (StepHandlingMethod)0;
		component12.LedgeHandling = true;
		component12.InteractiveRigidbodyHandling = true;
		component12.SafeMovement = false;
		HurtBoxGroup val10 = val.AddComponent<HurtBoxGroup>();
		HurtBox val11 = ((Component)val.GetComponentInChildren<CapsuleCollider>()).gameObject.AddComponent<HurtBox>();
		((Component)val11).gameObject.layer = LayerIndex.entityPrecise.intVal;
		val11.healthComponent = component8;
		val11.isBullseye = true;
		val11.damageModifier = (DamageModifier)0;
		val11.hurtBoxGroup = val10;
		val11.indexInGroup = 0;
		val10.hurtBoxes = (HurtBox[])(object)new HurtBox[1] { val11 };
		val10.mainHurtBox = val11;
		val10.bullseyeCount = 1;
		GameObject gameObject = ((Component)component7.FindChild("cubeCollider")).gameObject;
		gameObject.AddComponent<CollisionCaller>();
		gameObject.layer = LayerIndex.projectile.intVal;
		Utils.CreateHitbox("Punch", val.transform, new Vector3(4.5f, 4.5f, 4.8f));
		Utils.CreateHitbox("Kick", val.transform, new Vector3(4.8f, 4.8f, 5.2f));
		FootstepHandler val12 = val.AddComponent<FootstepHandler>();
		val12.baseFootstepString = "Play_player_footstep";
		val12.sprintFootstepOverrideString = "";
		val12.enableFootstepDust = true;
		val12.footstepDustPrefab = LegacyResourcesAPI.Load<GameObject>("Prefabs/GenericFootstepDust");
		CharacterDeathBehavior component13 = characterPrefab.GetComponent<CharacterDeathBehavior>();
		component13.deathStateMachine = characterPrefab.GetComponent<EntityStateMachine>();
		component13.deathState = new SerializableEntityStateType(typeof(GenericCharacterDeath));
		EntityStateMachine item = Utils.NewStateMachine(characterPrefab, "Hook");
		EntityStateMachine item2 = Utils.NewStateMachine(characterPrefab, "secondary");
		EntityStateMachine item3 = Utils.NewStateMachine(characterPrefab, "utility");
		EntityStateMachine item4 = Utils.NewStateMachine(characterPrefab, "special");
		NetworkStateMachine component14 = ((Component)component).GetComponent<NetworkStateMachine>();
		List<EntityStateMachine> list2 = component14.stateMachines.ToList();
		list2.Add(item);
		list2.Add(item2);
		list2.Add(item3);
		list2.Add(item4);
		component14.stateMachines = list2.ToArray();
		EntityStateMachine component15 = ((Component)component).GetComponent<EntityStateMachine>();
		component15.mainStateType = new SerializableEntityStateType(typeof(CharacterMain));
		ContentAddition.AddBody(characterPrefab);
	}

	private void RegisterCharacter()
	{
		//IL_016f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0174: Unknown result type (might be due to invalid IL or missing references)
		characterDisplay = PrefabAPI.InstantiateClone(((Component)characterPrefab.GetComponent<ModelLocator>().modelTransform).gameObject, "RorschachDisplay", true);
		characterDisplay.AddComponent<NetworkIdentity>();
		CharacterModel component = characterDisplay.GetComponent<CharacterModel>();
		RendererInfo[] baseRendererInfos = component.baseRendererInfos;
		for (int i = 0; i < baseRendererInfos.Length; i++)
		{
			Material val = Object.Instantiate<Material>(baseRendererInfos[i].defaultMaterial);
			val.shaderKeywords = null;
			baseRendererInfos[i].defaultMaterial = val;
		}
		string text = "The face is finished at last - Wonderful black and white, as all things should be. I am glad I decided to keep the dress these past two years. The face is perfect, a thing of true beauty... A face that can shelter me from the world and hide my weary senses. A face which I can finally stare down in the mirror." + Environment.NewLine + "From this point on, I've decided to write down everything I see and experience which might possibly have a bearing upon my nocturnal mission. This journal will be a complete record of my deeds which I can refer back to and a voucher to show the angels when they come looking for me on Judgement Day.";
		LanguageAPI.Add("RORSCHACH_LORE", text);
		string text2 = "<style=cSub>\r\n\r\n< ! > " + Environment.NewLine + "<style=cSub>\r\n\r\n< ! > " + Environment.NewLine + "<style=cSub>\r\n\r\n< ! > " + Environment.NewLine + "<style=cSub>\r\n\r\n< ! > ";
		string text3 = "..and so he left, without compromise.";
		string text4 = "..and so he vanished.";
		LanguageAPI.Add("RORSCHACH_NAME", "Rorschach");
		LanguageAPI.Add("RORSCHACH_DESCRIPTION", text2);
		LanguageAPI.Add("RORSCHACH_SUBTITLE", "");
		LanguageAPI.Add("RORSCHACH_OUTRO", text3);
		LanguageAPI.Add("RORSCHACH_FAIL", text4);
		SurvivorDef val2 = ScriptableObject.CreateInstance<SurvivorDef>();
		val2.cachedName = "RORSCHACH_NAME";
		val2.unlockableDef = null;
		val2.descriptionToken = "RORSCHACH_DESCRIPTION";
		val2.primaryColor = characterColor;
		val2.bodyPrefab = characterPrefab;
		val2.displayPrefab = characterDisplay;
		val2.outroFlavorToken = "RORSCHACH_OUTRO";
		val2.desiredSortPosition = 8.2f;
		val2.mainEndingEscapeFailureFlavorToken = "RORSCHACH_FAIL";
		ContentAddition.AddSurvivorDef(val2);
		SkillSetup();
	}

	private void RegisterStates()
	{
		//IL_0003: Unknown result type (might be due to invalid IL or missing references)
		//IL_000b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0013: Unknown result type (might be due to invalid IL or missing references)
		//IL_001b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0023: Unknown result type (might be due to invalid IL or missing references)
		//IL_002b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0033: Unknown result type (might be due to invalid IL or missing references)
		//IL_003b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0043: Unknown result type (might be due to invalid IL or missing references)
		bool flag = default(bool);
		ContentAddition.AddEntityState<Primary>(ref flag);
		ContentAddition.AddEntityState<Secondary>(ref flag);
		ContentAddition.AddEntityState<Utility>(ref flag);
		ContentAddition.AddEntityState<Special>(ref flag);
		ContentAddition.AddEntityState<PrimaryRage>(ref flag);
		ContentAddition.AddEntityState<SecondaryRage>(ref flag);
		ContentAddition.AddEntityState<UtilityRage>(ref flag);
		ContentAddition.AddEntityState<SpecialRage>(ref flag);
		ContentAddition.AddEntityState<CharacterMain>(ref flag);
	}

	private void SkillSetup()
	{
		GenericSkill[] componentsInChildren = characterPrefab.GetComponentsInChildren<GenericSkill>();
		foreach (GenericSkill val in componentsInChildren)
		{
			Object.DestroyImmediate((Object)(object)val);
		}
		PassiveSetup();
		PrimarySetup();
		SecondarySetup();
		UtilitySetup();
		SpecialSetup();
	}

	private void PassiveSetup()
	{
		SkillLocator component = characterPrefab.GetComponent<SkillLocator>();
		LanguageAPI.Add("RORSCHACH_PASSIVE_NAME", "Rage");
		LanguageAPI.Add("RORSCHACH_PASSIVE_DESCRIPTION", "Rorscach builds up rage when attacking enemies. Reaching maximum rage, Rorschach abilities are upgraded to new and more powerful moves.");
		component.passiveSkill.enabled = true;
		component.passiveSkill.skillNameToken = "RORSCHACH_PASSIVE_NAME";
		component.passiveSkill.skillDescriptionToken = "RORSCHACH_PASSIVE_DESCRIPTION";
		component.passiveSkill.icon = Assets.MainAssetBundle.LoadAsset<Sprite>("passive");
	}

	private void PrimarySetup()
	{
		//IL_005d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0062: Unknown result type (might be due to invalid IL or missing references)
		//IL_009b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0150: Unknown result type (might be due to invalid IL or missing references)
		//IL_0168: Unknown result type (might be due to invalid IL or missing references)
		//IL_0172: Expected O, but got Unknown
		//IL_0173: Unknown result type (might be due to invalid IL or missing references)
		//IL_0175: Unknown result type (might be due to invalid IL or missing references)
		//IL_019a: Unknown result type (might be due to invalid IL or missing references)
		//IL_019f: Unknown result type (might be due to invalid IL or missing references)
		//IL_01f4: Unknown result type (might be due to invalid IL or missing references)
		SkillLocator component = characterPrefab.GetComponent<SkillLocator>();
		LanguageAPI.Add("RORSCHACH_M1", "Jab & Cross");
		LanguageAPI.Add("RORSCHACH_M1_DESCRIPTION", "Rorschach performs a light five hit combo dealing <style=cIsDamage>1000% damage</style> and can be chained into <style=cIsDamage>Hook & Overhead</style>.");
		LanguageAPI.Add("RORSCHACH_M1RAGE", "Rolling Punches");
		LanguageAPI.Add("RORSCHACH_M1RAGE_DESCRIPTION", "Rorschach performs consecutive rolling punches dealing <style=cIsDamage>250% damage</style> every hit.");
		RSkillDef rSkillDef = ScriptableObject.CreateInstance<RSkillDef>();
		((SkillDef)rSkillDef).activationState = new SerializableEntityStateType(typeof(Primary));
		((SkillDef)rSkillDef).activationStateMachineName = "Weapon";
		((SkillDef)rSkillDef).baseMaxStock = 0;
		((SkillDef)rSkillDef).baseRechargeInterval = 0f;
		((SkillDef)rSkillDef).beginSkillCooldownOnSkillEnd = true;
		((SkillDef)rSkillDef).canceledFromSprinting = false;
		((SkillDef)rSkillDef).fullRestockOnAssign = true;
		((SkillDef)rSkillDef).interruptPriority = (InterruptPriority)0;
		((SkillDef)rSkillDef).isCombatSkill = true;
		((SkillDef)rSkillDef).mustKeyPress = false;
		((SkillDef)rSkillDef).cancelSprintingOnActivation = true;
		((SkillDef)rSkillDef).rechargeStock = 0;
		((SkillDef)rSkillDef).requiredStock = 0;
		((SkillDef)rSkillDef).stockToConsume = 0;
		((SkillDef)rSkillDef).icon = Assets.MainAssetBundle.LoadAsset<Sprite>("m1");
		((SkillDef)rSkillDef).skillDescriptionToken = "RORSCHACH_M1_DESCRIPTION";
		((SkillDef)rSkillDef).skillName = "RORSCHACH_M1";
		((SkillDef)rSkillDef).skillNameToken = "RORSCHACH_M1";
		ContentAddition.AddSkillDef((SkillDef)(object)rSkillDef);
		component.primary = characterPrefab.AddComponent<GenericSkill>();
		SkillFamily val = ScriptableObject.CreateInstance<SkillFamily>();
		val.variants = (Variant[])(object)new Variant[1];
		Reflection.SetFieldValue<SkillFamily>((object)component.primary, "_skillFamily", val);
		SkillFamily skillFamily = component.primary.skillFamily;
		Variant[] variants = skillFamily.variants;
		Variant val2 = new Variant
		{
			skillDef = (SkillDef)(object)rSkillDef
		};
		((Variant)(ref val2)).viewableNode = new Node(((SkillDef)rSkillDef).skillNameToken, false, (Node)null);
		variants[0] = val2;
		ContentAddition.AddSkillFamily(skillFamily);
		PrimaryRageDef = (SkillDef)(object)ScriptableObject.CreateInstance<RSkillDef>();
		PrimaryRageDef.activationState = new SerializableEntityStateType(typeof(PrimaryRage));
		PrimaryRageDef.activationStateMachineName = "Weapon";
		PrimaryRageDef.baseMaxStock = 1;
		PrimaryRageDef.baseRechargeInterval = 3f;
		PrimaryRageDef.beginSkillCooldownOnSkillEnd = true;
		PrimaryRageDef.canceledFromSprinting = false;
		PrimaryRageDef.fullRestockOnAssign = true;
		PrimaryRageDef.interruptPriority = (InterruptPriority)0;
		PrimaryRageDef.isCombatSkill = true;
		PrimaryRageDef.mustKeyPress = false;
		PrimaryRageDef.cancelSprintingOnActivation = true;
		PrimaryRageDef.rechargeStock = 1;
		PrimaryRageDef.requiredStock = 1;
		PrimaryRageDef.stockToConsume = 1;
		PrimaryRageDef.icon = Assets.MainAssetBundle.LoadAsset<Sprite>("ragem1");
		PrimaryRageDef.skillDescriptionToken = "RORSCHACH_M1RAGE_DESCRIPTION";
		PrimaryRageDef.skillName = "RORSCHACH_M1RAGE";
		PrimaryRageDef.skillNameToken = "RORSCHACH_M1RAGE";
		ContentAddition.AddSkillDef(PrimaryRageDef);
	}

	private void SecondarySetup()
	{
		//IL_005d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0062: Unknown result type (might be due to invalid IL or missing references)
		//IL_009b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0150: Unknown result type (might be due to invalid IL or missing references)
		//IL_0168: Unknown result type (might be due to invalid IL or missing references)
		//IL_0172: Expected O, but got Unknown
		//IL_0173: Unknown result type (might be due to invalid IL or missing references)
		//IL_0175: Unknown result type (might be due to invalid IL or missing references)
		//IL_019a: Unknown result type (might be due to invalid IL or missing references)
		//IL_019f: Unknown result type (might be due to invalid IL or missing references)
		//IL_01f4: Unknown result type (might be due to invalid IL or missing references)
		SkillLocator component = characterPrefab.GetComponent<SkillLocator>();
		LanguageAPI.Add("RORSCHACH_M2", "Hook & Overhead");
		LanguageAPI.Add("RORSCHACH_M2_DESCRIPTION", "Rorschach performs a heavy three hit combo dealing <style=cIsDamage>900% damage</style> can be chained into <style=cIsDamage>Jab & Cross</style>.");
		LanguageAPI.Add("RORSCHACH_M2RAGE", "Superman Punch");
		LanguageAPI.Add("RORSCHACH_M2RAGE_DESCRIPTION", "Rorschach performs a strong lunging punch dealing <style=cIsDamage>450% damage</style>.");
		RSkillDef rSkillDef = ScriptableObject.CreateInstance<RSkillDef>();
		((SkillDef)rSkillDef).activationState = new SerializableEntityStateType(typeof(Secondary));
		((SkillDef)rSkillDef).activationStateMachineName = "Slide";
		((SkillDef)rSkillDef).baseMaxStock = 3;
		((SkillDef)rSkillDef).baseRechargeInterval = 4f;
		((SkillDef)rSkillDef).beginSkillCooldownOnSkillEnd = true;
		((SkillDef)rSkillDef).canceledFromSprinting = false;
		((SkillDef)rSkillDef).fullRestockOnAssign = false;
		((SkillDef)rSkillDef).interruptPriority = (InterruptPriority)0;
		((SkillDef)rSkillDef).isCombatSkill = true;
		((SkillDef)rSkillDef).mustKeyPress = false;
		((SkillDef)rSkillDef).cancelSprintingOnActivation = true;
		((SkillDef)rSkillDef).rechargeStock = 1;
		((SkillDef)rSkillDef).requiredStock = 1;
		((SkillDef)rSkillDef).stockToConsume = 1;
		((SkillDef)rSkillDef).icon = Assets.MainAssetBundle.LoadAsset<Sprite>("m2");
		((SkillDef)rSkillDef).skillDescriptionToken = "RORSCHACH_M2_DESCRIPTION";
		((SkillDef)rSkillDef).skillName = "RORSCHACH_M2";
		((SkillDef)rSkillDef).skillNameToken = "RORSCHACH_M2";
		ContentAddition.AddSkillDef((SkillDef)(object)rSkillDef);
		component.secondary = characterPrefab.AddComponent<GenericSkill>();
		SkillFamily val = ScriptableObject.CreateInstance<SkillFamily>();
		val.variants = (Variant[])(object)new Variant[1];
		Reflection.SetFieldValue<SkillFamily>((object)component.secondary, "_skillFamily", val);
		SkillFamily skillFamily = component.secondary.skillFamily;
		Variant[] variants = skillFamily.variants;
		Variant val2 = new Variant
		{
			skillDef = (SkillDef)(object)rSkillDef
		};
		((Variant)(ref val2)).viewableNode = new Node(((SkillDef)rSkillDef).skillNameToken, false, (Node)null);
		variants[0] = val2;
		ContentAddition.AddSkillFamily(skillFamily);
		SecondaryRageDef = (SkillDef)(object)ScriptableObject.CreateInstance<RSkillDef>();
		SecondaryRageDef.activationState = new SerializableEntityStateType(typeof(SecondaryRage));
		SecondaryRageDef.activationStateMachineName = "Weapon";
		SecondaryRageDef.baseMaxStock = 1;
		SecondaryRageDef.baseRechargeInterval = 4f;
		SecondaryRageDef.beginSkillCooldownOnSkillEnd = true;
		SecondaryRageDef.canceledFromSprinting = false;
		SecondaryRageDef.fullRestockOnAssign = false;
		SecondaryRageDef.interruptPriority = (InterruptPriority)0;
		SecondaryRageDef.isCombatSkill = true;
		SecondaryRageDef.mustKeyPress = false;
		SecondaryRageDef.cancelSprintingOnActivation = true;
		SecondaryRageDef.rechargeStock = 1;
		SecondaryRageDef.requiredStock = 1;
		SecondaryRageDef.stockToConsume = 1;
		SecondaryRageDef.icon = Assets.MainAssetBundle.LoadAsset<Sprite>("ragem2");
		SecondaryRageDef.skillDescriptionToken = "RORSCHACH_M2RAGE_DESCRIPTION";
		SecondaryRageDef.skillName = "RORSCHACH_M2RAGE";
		SecondaryRageDef.skillNameToken = "RORSCHACH_M2RAGE";
		ContentAddition.AddSkillDef(SecondaryRageDef);
	}

	private void UtilitySetup()
	{
		//IL_005d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0062: Unknown result type (might be due to invalid IL or missing references)
		//IL_009b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0150: Unknown result type (might be due to invalid IL or missing references)
		//IL_0168: Unknown result type (might be due to invalid IL or missing references)
		//IL_0172: Expected O, but got Unknown
		//IL_0173: Unknown result type (might be due to invalid IL or missing references)
		//IL_0175: Unknown result type (might be due to invalid IL or missing references)
		//IL_019a: Unknown result type (might be due to invalid IL or missing references)
		//IL_019f: Unknown result type (might be due to invalid IL or missing references)
		//IL_01f4: Unknown result type (might be due to invalid IL or missing references)
		SkillLocator component = characterPrefab.GetComponent<SkillLocator>();
		LanguageAPI.Add("RORSCHACH_UTIL", "Hook Shot");
		LanguageAPI.Add("RORSCHACH_UTIL_DESCRIPTION", "Rorschach fires his grapple, pulling himself forward, hitting an enemy deal <style=cIsDamage>350% damage</style>.");
		LanguageAPI.Add("RORSCHACH_UTILRAGE", "Bull Rush ");
		LanguageAPI.Add("RORSCHACH_UTILRAGE_DESCRIPTION", "Rorschach sprints forward, grabbing the first nearby enemy and slams them on terrain for <style=cIsDamage>350% damage</style>.");
		RSkillDef rSkillDef = ScriptableObject.CreateInstance<RSkillDef>();
		((SkillDef)rSkillDef).activationState = new SerializableEntityStateType(typeof(Utility));
		((SkillDef)rSkillDef).activationStateMachineName = "Hook";
		((SkillDef)rSkillDef).baseMaxStock = 1;
		((SkillDef)rSkillDef).baseRechargeInterval = 6f;
		((SkillDef)rSkillDef).beginSkillCooldownOnSkillEnd = true;
		((SkillDef)rSkillDef).canceledFromSprinting = false;
		((SkillDef)rSkillDef).fullRestockOnAssign = false;
		((SkillDef)rSkillDef).interruptPriority = (InterruptPriority)0;
		((SkillDef)rSkillDef).isCombatSkill = false;
		((SkillDef)rSkillDef).mustKeyPress = true;
		((SkillDef)rSkillDef).cancelSprintingOnActivation = true;
		((SkillDef)rSkillDef).rechargeStock = 1;
		((SkillDef)rSkillDef).requiredStock = 1;
		((SkillDef)rSkillDef).stockToConsume = 1;
		((SkillDef)rSkillDef).icon = Assets.MainAssetBundle.LoadAsset<Sprite>("utility");
		((SkillDef)rSkillDef).skillDescriptionToken = "RORSCHACH_UTIL_DESCRIPTION";
		((SkillDef)rSkillDef).skillName = "RORSCHACH_UTIL";
		((SkillDef)rSkillDef).skillNameToken = "RORSCHACH_UTIL";
		ContentAddition.AddSkillDef((SkillDef)(object)rSkillDef);
		component.utility = characterPrefab.AddComponent<GenericSkill>();
		SkillFamily val = ScriptableObject.CreateInstance<SkillFamily>();
		val.variants = (Variant[])(object)new Variant[1];
		Reflection.SetFieldValue<SkillFamily>((object)component.utility, "_skillFamily", val);
		SkillFamily skillFamily = component.utility.skillFamily;
		Variant[] variants = skillFamily.variants;
		Variant val2 = new Variant
		{
			skillDef = (SkillDef)(object)rSkillDef
		};
		((Variant)(ref val2)).viewableNode = new Node(((SkillDef)rSkillDef).skillNameToken, false, (Node)null);
		variants[0] = val2;
		ContentAddition.AddSkillFamily(skillFamily);
		UtilityRageDef = (SkillDef)(object)ScriptableObject.CreateInstance<RSkillDef>();
		UtilityRageDef.activationState = new SerializableEntityStateType(typeof(UtilityRage));
		UtilityRageDef.activationStateMachineName = "Hook";
		UtilityRageDef.baseMaxStock = 1;
		UtilityRageDef.baseRechargeInterval = 5f;
		UtilityRageDef.beginSkillCooldownOnSkillEnd = true;
		UtilityRageDef.canceledFromSprinting = false;
		UtilityRageDef.fullRestockOnAssign = false;
		UtilityRageDef.interruptPriority = (InterruptPriority)0;
		UtilityRageDef.isCombatSkill = false;
		UtilityRageDef.mustKeyPress = true;
		UtilityRageDef.cancelSprintingOnActivation = false;
		UtilityRageDef.rechargeStock = 1;
		UtilityRageDef.requiredStock = 1;
		UtilityRageDef.stockToConsume = 1;
		UtilityRageDef.icon = Assets.MainAssetBundle.LoadAsset<Sprite>("rageutility");
		UtilityRageDef.skillDescriptionToken = "RORSCHACH_UTILRAGE_DESCRIPTION";
		UtilityRageDef.skillName = "RORSCHACH_UTILRAGE";
		UtilityRageDef.skillNameToken = "RORSCHACH_UTILRAGE";
		ContentAddition.AddSkillDef(UtilityRageDef);
	}

	private void SpecialSetup()
	{
		//IL_005d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0062: Unknown result type (might be due to invalid IL or missing references)
		//IL_009b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0150: Unknown result type (might be due to invalid IL or missing references)
		//IL_0168: Unknown result type (might be due to invalid IL or missing references)
		//IL_0172: Expected O, but got Unknown
		//IL_0173: Unknown result type (might be due to invalid IL or missing references)
		//IL_0175: Unknown result type (might be due to invalid IL or missing references)
		//IL_019a: Unknown result type (might be due to invalid IL or missing references)
		//IL_019f: Unknown result type (might be due to invalid IL or missing references)
		//IL_01f4: Unknown result type (might be due to invalid IL or missing references)
		SkillLocator component = characterPrefab.GetComponent<SkillLocator>();
		LanguageAPI.Add("RORSCHACH_SPEC", "Makeshift Flamethrower");
		LanguageAPI.Add("RORSCHACH_SPEC_DESCRIPTION", "Rorschach uses lighter and spray can to ignite area in front for <style=cIsDamage>1000% damage</style> per second.");
		LanguageAPI.Add("RORSCHACH_SPECRAGE", "Cleaver");
		LanguageAPI.Add("RORSCHACH_SPECRAGE_DESCRIPTION", "Rorschach performs a flurry of cleaver slashes for <style=cIsDamage>750% damage</style>.");
		RSkillDef rSkillDef = ScriptableObject.CreateInstance<RSkillDef>();
		((SkillDef)rSkillDef).activationState = new SerializableEntityStateType(typeof(Special));
		((SkillDef)rSkillDef).activationStateMachineName = "Weapon";
		((SkillDef)rSkillDef).baseMaxStock = 1;
		((SkillDef)rSkillDef).baseRechargeInterval = 8f;
		((SkillDef)rSkillDef).beginSkillCooldownOnSkillEnd = true;
		((SkillDef)rSkillDef).canceledFromSprinting = false;
		((SkillDef)rSkillDef).fullRestockOnAssign = false;
		((SkillDef)rSkillDef).interruptPriority = (InterruptPriority)0;
		((SkillDef)rSkillDef).isCombatSkill = true;
		((SkillDef)rSkillDef).mustKeyPress = true;
		((SkillDef)rSkillDef).cancelSprintingOnActivation = false;
		((SkillDef)rSkillDef).rechargeStock = 1;
		((SkillDef)rSkillDef).requiredStock = 1;
		((SkillDef)rSkillDef).stockToConsume = 1;
		((SkillDef)rSkillDef).icon = Assets.MainAssetBundle.LoadAsset<Sprite>("special");
		((SkillDef)rSkillDef).skillDescriptionToken = "RORSCHACH_SPEC_DESCRIPTION";
		((SkillDef)rSkillDef).skillName = "RORSCHACH_SPEC";
		((SkillDef)rSkillDef).skillNameToken = "RORSCHACH_SPEC";
		ContentAddition.AddSkillDef((SkillDef)(object)rSkillDef);
		component.special = characterPrefab.AddComponent<GenericSkill>();
		SkillFamily val = ScriptableObject.CreateInstance<SkillFamily>();
		val.variants = (Variant[])(object)new Variant[1];
		Reflection.SetFieldValue<SkillFamily>((object)component.special, "_skillFamily", val);
		SkillFamily skillFamily = component.special.skillFamily;
		Variant[] variants = skillFamily.variants;
		Variant val2 = new Variant
		{
			skillDef = (SkillDef)(object)rSkillDef
		};
		((Variant)(ref val2)).viewableNode = new Node(((SkillDef)rSkillDef).skillNameToken, false, (Node)null);
		variants[0] = val2;
		ContentAddition.AddSkillFamily(skillFamily);
		SpecialRageDef = (SkillDef)(object)ScriptableObject.CreateInstance<RSkillDef>();
		SpecialRageDef.activationState = new SerializableEntityStateType(typeof(SpecialRage));
		SpecialRageDef.activationStateMachineName = "Weapon";
		SpecialRageDef.baseMaxStock = 1;
		SpecialRageDef.baseRechargeInterval = 8f;
		SpecialRageDef.beginSkillCooldownOnSkillEnd = true;
		SpecialRageDef.canceledFromSprinting = false;
		SpecialRageDef.fullRestockOnAssign = false;
		SpecialRageDef.interruptPriority = (InterruptPriority)0;
		SpecialRageDef.isCombatSkill = true;
		SpecialRageDef.mustKeyPress = true;
		SpecialRageDef.cancelSprintingOnActivation = false;
		SpecialRageDef.rechargeStock = 1;
		SpecialRageDef.requiredStock = 1;
		SpecialRageDef.stockToConsume = 1;
		SpecialRageDef.icon = Assets.MainAssetBundle.LoadAsset<Sprite>("ragespecial");
		SpecialRageDef.skillDescriptionToken = "RORSCHACH_SPECRAGE_DESCRIPTION";
		SpecialRageDef.skillName = "RORSCHACH_SPECRAGE";
		SpecialRageDef.skillNameToken = "RORSCHACH_SPECRAGE";
		ContentAddition.AddSkillDef(SpecialRageDef);
	}

	private void CreateDoppelganger()
	{
		doppelganger = PrefabAPI.InstantiateClone(LegacyResourcesAPI.Load<GameObject>("Prefabs/CharacterMasters/MercMonsterMaster"), "RorschachMaster", true);
		ContentAddition.AddMaster(doppelganger);
		CharacterMaster component = doppelganger.GetComponent<CharacterMaster>();
		component.bodyPrefab = characterPrefab;
	}
}
