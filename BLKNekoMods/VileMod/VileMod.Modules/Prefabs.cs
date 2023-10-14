using System.Collections.Generic;
using R2API;
using RoR2;
using UnityEngine;
using UnityEngine.Rendering;
using VileMod.Modules.Characters;

namespace VileMod.Modules;

internal static class Prefabs
{
	private static PhysicMaterial ragdollMaterial;

	public static GameObject CreateDisplayPrefab(string displayModelName, GameObject prefab, BodyInfo bodyInfo)
	{
		GameObject val = Assets.LoadSurvivorModel(displayModelName);
		CharacterModel val2 = val.GetComponent<CharacterModel>();
		if (!Object.op_Implicit((Object)(object)val2))
		{
			val2 = val.AddComponent<CharacterModel>();
		}
		val2.baseRendererInfos = prefab.GetComponentInChildren<CharacterModel>().baseRendererInfos;
		Assets.ConvertAllRenderersToHopooShader(val);
		return val.gameObject;
	}

	public static GameObject CreateBodyPrefab(string bodyName, string modelName, BodyInfo bodyInfo)
	{
		//IL_00d7: Unknown result type (might be due to invalid IL or missing references)
		//IL_00dc: Unknown result type (might be due to invalid IL or missing references)
		//IL_02cf: Unknown result type (might be due to invalid IL or missing references)
		//IL_02d7: Unknown result type (might be due to invalid IL or missing references)
		//IL_02e7: Unknown result type (might be due to invalid IL or missing references)
		GameObject val = LegacyResourcesAPI.Load<GameObject>("Prefabs/CharacterBodies/" + bodyInfo.bodyNameToClone + "Body");
		if (!Object.op_Implicit((Object)(object)val))
		{
			Log.Error(bodyInfo.bodyNameToClone + "Body is not a valid body, character creation failed");
			return null;
		}
		GameObject val2 = PrefabAPI.InstantiateClone(val, bodyName);
		Transform val3 = null;
		GameObject val4 = null;
		if (modelName != "mdl")
		{
			val4 = Assets.LoadSurvivorModel(modelName);
			if ((Object)(object)val4 == (Object)null)
			{
				val4 = ((Component)val2.GetComponentInChildren<CharacterModel>()).gameObject;
			}
			val3 = AddCharacterModelToSurvivorBody(val2, val4.transform, bodyInfo);
		}
		CharacterBody component = val2.GetComponent<CharacterBody>();
		((Object)component).name = bodyInfo.bodyName;
		component.baseNameToken = bodyInfo.bodyNameToken;
		component.subtitleNameToken = bodyInfo.subtitleNameToken;
		component.portraitIcon = bodyInfo.characterPortrait;
		component.bodyColor = bodyInfo.bodyColor;
		component._defaultCrosshairPrefab = bodyInfo.crosshair;
		component.hideCrosshair = false;
		component.preferredPodPrefab = bodyInfo.podPrefab;
		component.baseMaxHealth = bodyInfo.maxHealth;
		component.baseRegen = bodyInfo.healthRegen;
		component.baseArmor = bodyInfo.armor;
		component.baseMaxShield = bodyInfo.shield;
		component.baseDamage = bodyInfo.damage;
		component.baseAttackSpeed = bodyInfo.attackSpeed;
		component.baseCrit = bodyInfo.crit;
		component.baseMoveSpeed = bodyInfo.moveSpeed;
		component.baseJumpPower = bodyInfo.jumpPower;
		component.autoCalculateLevelStats = bodyInfo.autoCalculateLevelStats;
		if (bodyInfo.autoCalculateLevelStats)
		{
			component.levelMaxHealth = Mathf.Round(component.baseMaxHealth * 0.3f);
			component.levelMaxShield = Mathf.Round(component.baseMaxShield * 0.3f);
			component.levelRegen = component.baseRegen * 0.2f;
			component.levelMoveSpeed = 0f;
			component.levelJumpPower = 0f;
			component.levelDamage = component.baseDamage * 0.2f;
			component.levelAttackSpeed = 0f;
			component.levelCrit = 0f;
			component.levelArmor = 0f;
		}
		else
		{
			component.levelMaxHealth = bodyInfo.healthGrowth;
			component.levelMaxShield = bodyInfo.shieldGrowth;
			component.levelRegen = bodyInfo.regenGrowth;
			component.levelMoveSpeed = bodyInfo.moveSpeedGrowth;
			component.levelJumpPower = bodyInfo.jumpPowerGrowth;
			component.levelDamage = bodyInfo.damageGrowth;
			component.levelAttackSpeed = bodyInfo.attackSpeedGrowth;
			component.levelCrit = bodyInfo.critGrowth;
			component.levelArmor = bodyInfo.armorGrowth;
		}
		component.baseAcceleration = bodyInfo.acceleration;
		component.baseJumpCount = bodyInfo.jumpCount;
		component.sprintingSpeedMultiplier = 1.45f;
		component.bodyFlags = (BodyFlags)16;
		component.bodyFlags = (BodyFlags)1;
		component.rootMotionInMainState = false;
		component.hullClassification = (HullClassification)0;
		component.isChampion = false;
		SetupCameraTargetParams(val2, bodyInfo);
		SetupModelLocator(val2, val3, val4.transform);
		SetupCapsuleCollider(val2);
		SetupMainHurtbox(val2, val4);
		SetupAimAnimator(val2, val4);
		if ((Object)(object)val3 != (Object)null)
		{
			SetupCharacterDirection(val2, val3, val4.transform);
		}
		SetupFootstepController(val4);
		SetupRagdoll(val4);
		Content.AddCharacterBodyPrefab(val2);
		return val2;
	}

	public static void CreateGenericDoppelganger(GameObject bodyPrefab, string masterName, string masterToCopy)
	{
		GameObject val = PrefabAPI.InstantiateClone(LegacyResourcesAPI.Load<GameObject>("Prefabs/CharacterMasters/" + masterToCopy + "MonsterMaster"), masterName, true);
		val.GetComponent<CharacterMaster>().bodyPrefab = bodyPrefab;
		Content.AddMasterPrefab(val);
	}

	private static Transform AddCharacterModelToSurvivorBody(GameObject bodyPrefab, Transform modelTransform, BodyInfo bodyInfo)
	{
		//IL_0040: Unknown result type (might be due to invalid IL or missing references)
		//IL_005a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0066: Unknown result type (might be due to invalid IL or missing references)
		//IL_007f: Unknown result type (might be due to invalid IL or missing references)
		//IL_008b: Unknown result type (might be due to invalid IL or missing references)
		//IL_009b: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a1: Expected O, but got Unknown
		//IL_00ba: Unknown result type (might be due to invalid IL or missing references)
		//IL_00cb: Unknown result type (might be due to invalid IL or missing references)
		//IL_00db: Unknown result type (might be due to invalid IL or missing references)
		//IL_00e1: Expected O, but got Unknown
		//IL_00fa: Unknown result type (might be due to invalid IL or missing references)
		//IL_010b: Unknown result type (might be due to invalid IL or missing references)
		for (int num = bodyPrefab.transform.childCount - 1; num >= 0; num--)
		{
			Object.DestroyImmediate((Object)(object)((Component)bodyPrefab.transform.GetChild(num)).gameObject);
		}
		Transform transform = new GameObject("ModelBase").transform;
		transform.parent = bodyPrefab.transform;
		transform.localPosition = bodyInfo.modelBasePosition;
		transform.localRotation = Quaternion.identity;
		modelTransform.parent = ((Component)transform).transform;
		modelTransform.localPosition = Vector3.zero;
		modelTransform.localRotation = Quaternion.identity;
		GameObject val = new GameObject("CameraPivot");
		val.transform.parent = bodyPrefab.transform;
		val.transform.localPosition = bodyInfo.cameraPivotPosition;
		val.transform.localRotation = Quaternion.identity;
		GameObject val2 = new GameObject("AimOrigin");
		val2.transform.parent = bodyPrefab.transform;
		val2.transform.localPosition = bodyInfo.aimOriginPosition;
		val2.transform.localRotation = Quaternion.identity;
		bodyPrefab.GetComponent<CharacterBody>().aimOriginTransform = val2.transform;
		return ((Component)transform).transform;
	}

	public static CharacterModel SetupCharacterModel(GameObject prefab)
	{
		return SetupCharacterModel(prefab, null);
	}

	public static CharacterModel SetupCharacterModel(GameObject prefab, CustomRendererInfo[] customInfos)
	{
		CharacterModel val = ((Component)prefab.GetComponent<ModelLocator>().modelTransform).gameObject.GetComponent<CharacterModel>();
		bool flag = (Object)(object)val != (Object)null;
		if (!flag)
		{
			val = ((Component)prefab.GetComponent<ModelLocator>().modelTransform).gameObject.AddComponent<CharacterModel>();
		}
		val.body = prefab.GetComponent<CharacterBody>();
		val.autoPopulateLightInfos = true;
		val.invisibilityCount = 0;
		val.temporaryOverlays = new List<TemporaryOverlay>();
		if (!flag)
		{
			SetupCustomRendererInfos(val, customInfos);
		}
		else
		{
			SetupPreAttachedRendererInfos(val);
		}
		return val;
	}

	public static void SetupPreAttachedRendererInfos(CharacterModel characterModel)
	{
		for (int i = 0; i < characterModel.baseRendererInfos.Length; i++)
		{
			if ((Object)(object)characterModel.baseRendererInfos[i].defaultMaterial == (Object)null)
			{
				characterModel.baseRendererInfos[i].defaultMaterial = characterModel.baseRendererInfos[i].renderer.sharedMaterial;
			}
			characterModel.baseRendererInfos[i].defaultMaterial.SetHopooMaterial();
		}
	}

	public static void SetupCustomRendererInfos(CharacterModel characterModel, CustomRendererInfo[] customInfos)
	{
		//IL_00de: Unknown result type (might be due to invalid IL or missing references)
		//IL_0108: Unknown result type (might be due to invalid IL or missing references)
		//IL_010d: Unknown result type (might be due to invalid IL or missing references)
		ChildLocator component = ((Component)characterModel).GetComponent<ChildLocator>();
		if (!Object.op_Implicit((Object)(object)component))
		{
			Log.Error("Failed CharacterModel setup: ChildLocator component does not exist on the model");
			return;
		}
		List<RendererInfo> list = new List<RendererInfo>();
		for (int i = 0; i < customInfos.Length; i++)
		{
			if (!Object.op_Implicit((Object)(object)component.FindChild(customInfos[i].childName)))
			{
				Log.Error("Trying to add a RendererInfo for a renderer that does not exist: " + customInfos[i].childName);
				continue;
			}
			Renderer component2 = ((Component)component.FindChild(customInfos[i].childName)).GetComponent<Renderer>();
			if (Object.op_Implicit((Object)(object)component2))
			{
				Material val = customInfos[i].material;
				if ((Object)(object)val == (Object)null)
				{
					val = ((!customInfos[i].dontHotpoo) ? component2.material.SetHopooMaterial() : component2.material);
				}
				list.Add(new RendererInfo
				{
					renderer = component2,
					defaultMaterial = val,
					ignoreOverlays = customInfos[i].ignoreOverlays,
					defaultShadowCastingMode = (ShadowCastingMode)1
				});
			}
		}
		characterModel.baseRendererInfos = list.ToArray();
	}

	private static void SetupCharacterDirection(GameObject prefab, Transform modelBaseTransform, Transform modelTransform)
	{
		if (Object.op_Implicit((Object)(object)prefab.GetComponent<CharacterDirection>()))
		{
			CharacterDirection component = prefab.GetComponent<CharacterDirection>();
			component.targetTransform = modelBaseTransform;
			component.overrideAnimatorForwardTransform = null;
			component.rootMotionAccumulator = null;
			component.modelAnimator = ((Component)modelTransform).GetComponent<Animator>();
			component.driveFromRootRotation = false;
			component.turnSpeed = 720f;
		}
	}

	private static void SetupCameraTargetParams(GameObject prefab, BodyInfo bodyInfo)
	{
		CameraTargetParams component = prefab.GetComponent<CameraTargetParams>();
		component.cameraParams = bodyInfo.cameraParams;
		component.cameraPivotTransform = prefab.transform.Find("CameraPivot");
	}

	private static void SetupModelLocator(GameObject prefab, Transform modelBaseTransform, Transform modelTransform)
	{
		ModelLocator component = prefab.GetComponent<ModelLocator>();
		component.modelTransform = modelTransform;
		component.modelBaseTransform = modelBaseTransform;
	}

	private static void SetupCapsuleCollider(GameObject prefab)
	{
		//IL_0018: Unknown result type (might be due to invalid IL or missing references)
		CapsuleCollider component = prefab.GetComponent<CapsuleCollider>();
		component.center = new Vector3(0f, 0f, 0f);
		component.radius = 0.5f;
		component.height = 1.82f;
		component.direction = 1;
	}

	private static void SetupMainHurtbox(GameObject prefab, GameObject model)
	{
		//IL_0075: Unknown result type (might be due to invalid IL or missing references)
		ChildLocator component = model.GetComponent<ChildLocator>();
		if (!Object.op_Implicit((Object)(object)component.FindChild("MainHurtbox")))
		{
			Debug.LogWarning((object)"Could not set up main hurtbox: make sure you have a transform pair in your prefab's ChildLocator component called 'MainHurtbox'");
			return;
		}
		HurtBoxGroup val = model.AddComponent<HurtBoxGroup>();
		HurtBox val2 = ((Component)component.FindChild("MainHurtbox")).gameObject.AddComponent<HurtBox>();
		((Component)val2).gameObject.layer = LayerIndex.entityPrecise.intVal;
		val2.healthComponent = prefab.GetComponent<HealthComponent>();
		val2.isBullseye = true;
		val2.damageModifier = (DamageModifier)0;
		val2.hurtBoxGroup = val;
		val2.indexInGroup = 0;
		val.hurtBoxes = (HurtBox[])(object)new HurtBox[1] { val2 };
		val.mainHurtBox = val2;
		val.bullseyeCount = 1;
	}

	public static void SetupHurtBoxes(GameObject bodyPrefab)
	{
		HealthComponent component = bodyPrefab.GetComponent<HealthComponent>();
		HurtBoxGroup[] componentsInChildren = bodyPrefab.GetComponentsInChildren<HurtBoxGroup>();
		foreach (HurtBoxGroup val in componentsInChildren)
		{
			val.mainHurtBox.healthComponent = component;
			for (int j = 0; j < val.hurtBoxes.Length; j++)
			{
				val.hurtBoxes[j].healthComponent = component;
			}
		}
	}

	private static void SetupFootstepController(GameObject model)
	{
		FootstepHandler val = model.AddComponent<FootstepHandler>();
		val.baseFootstepString = "Play_player_footstep";
		val.sprintFootstepOverrideString = "";
		val.enableFootstepDust = true;
		val.footstepDustPrefab = LegacyResourcesAPI.Load<GameObject>("Prefabs/GenericFootstepDust");
	}

	private static void SetupRagdoll(GameObject model)
	{
		RagdollController component = model.GetComponent<RagdollController>();
		if (!Object.op_Implicit((Object)(object)component))
		{
			return;
		}
		if ((Object)(object)ragdollMaterial == (Object)null)
		{
			ragdollMaterial = ((Component)LegacyResourcesAPI.Load<GameObject>("Prefabs/CharacterBodies/CommandoBody").GetComponentInChildren<RagdollController>().bones[1]).GetComponent<Collider>().material;
		}
		Transform[] bones = component.bones;
		foreach (Transform val in bones)
		{
			if (Object.op_Implicit((Object)(object)val))
			{
				((Component)val).gameObject.layer = LayerIndex.ragdoll.intVal;
				Collider component2 = ((Component)val).GetComponent<Collider>();
				if (Object.op_Implicit((Object)(object)component2))
				{
					component2.material = ragdollMaterial;
					component2.sharedMaterial = ragdollMaterial;
				}
			}
		}
	}

	private static void SetupAimAnimator(GameObject prefab, GameObject model)
	{
		AimAnimator val = model.AddComponent<AimAnimator>();
		val.directionComponent = prefab.GetComponent<CharacterDirection>();
		val.pitchRangeMax = 60f;
		val.pitchRangeMin = -60f;
		val.yawRangeMin = -80f;
		val.yawRangeMax = 80f;
		val.pitchGiveupRange = 30f;
		val.yawGiveupRange = 10f;
		val.giveupDuration = 3f;
		val.inputBank = prefab.GetComponent<InputBankTest>();
	}

	public static void SetupHitbox(GameObject prefab, Transform hitboxTransform, string hitboxName)
	{
		HitBoxGroup val = prefab.AddComponent<HitBoxGroup>();
		HitBox val2 = ((Component)hitboxTransform).gameObject.AddComponent<HitBox>();
		((Component)hitboxTransform).gameObject.layer = LayerIndex.projectile.intVal;
		val.hitBoxes = (HitBox[])(object)new HitBox[1] { val2 };
		val.groupName = hitboxName;
	}

	public static void SetupHitbox(GameObject prefab, string hitboxName, params Transform[] hitboxTransforms)
	{
		HitBoxGroup val = prefab.AddComponent<HitBoxGroup>();
		List<HitBox> list = new List<HitBox>();
		foreach (Transform val2 in hitboxTransforms)
		{
			HitBox item = ((Component)val2).gameObject.AddComponent<HitBox>();
			((Component)val2).gameObject.layer = LayerIndex.projectile.intVal;
			list.Add(item);
		}
		val.hitBoxes = list.ToArray();
		val.groupName = hitboxName;
	}
}
