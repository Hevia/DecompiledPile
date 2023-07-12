using System;
using System.Collections.Generic;
using System.Linq;
using RoR2.UI;
using RoR2.WwiseUtils;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.Rendering;
using UnityEngine.Serialization;

namespace RoR2;

public class CharacterModel : MonoBehaviour
{
	[Serializable]
	public struct RendererInfo : IEquatable<RendererInfo>
	{
		[PrefabReference]
		public Renderer renderer;

		public Material defaultMaterial;

		public ShadowCastingMode defaultShadowCastingMode;

		public bool ignoreOverlays;

		public bool hideOnDeath;

		public bool Equals(RendererInfo other)
		{
			//IL_001d: Unknown result type (might be due to invalid IL or missing references)
			//IL_0028: Unknown result type (might be due to invalid IL or missing references)
			if (renderer == other.renderer && defaultMaterial == other.defaultMaterial && object.Equals(defaultShadowCastingMode, other.defaultShadowCastingMode) && object.Equals(ignoreOverlays, other.ignoreOverlays))
			{
				return object.Equals(hideOnDeath, other.hideOnDeath);
			}
			return false;
		}
	}

	[Serializable]
	public struct LightInfo
	{
		public Light light;

		public Color defaultColor;

		public LightInfo(Light light)
		{
			//IL_0009: Unknown result type (might be due to invalid IL or missing references)
			//IL_000e: Unknown result type (might be due to invalid IL or missing references)
			this.light = light;
			defaultColor = light.color;
		}
	}

	private struct HurtBoxInfo
	{
		public readonly Transform transform;

		public readonly float estimatedRadius;

		public HurtBoxInfo(HurtBox hurtBox)
		{
			transform = ((Component)hurtBox).transform;
			estimatedRadius = Util.SphereVolumeToRadius(hurtBox.volume);
		}
	}

	private struct ParentedPrefabDisplay
	{
		public ItemIndex itemIndex;

		public EquipmentIndex equipmentIndex;

		public GameObject instance { get; private set; }

		public ItemDisplay itemDisplay { get; private set; }

		public void Apply(CharacterModel characterModel, GameObject prefab, Transform parent, Vector3 localPosition, Quaternion localRotation, Vector3 localScale)
		{
			//IL_001d: Unknown result type (might be due to invalid IL or missing references)
			//IL_002f: Unknown result type (might be due to invalid IL or missing references)
			//IL_0041: Unknown result type (might be due to invalid IL or missing references)
			instance = Object.Instantiate<GameObject>(prefab.gameObject, parent);
			instance.transform.localPosition = localPosition;
			instance.transform.localRotation = localRotation;
			instance.transform.localScale = localScale;
			LimbMatcher component = instance.GetComponent<LimbMatcher>();
			if (Object.op_Implicit((Object)(object)component) && Object.op_Implicit((Object)(object)characterModel.childLocator))
			{
				component.SetChildLocator(characterModel.childLocator);
			}
			itemDisplay = instance.GetComponent<ItemDisplay>();
		}

		public void Undo()
		{
			if (Object.op_Implicit((Object)(object)instance))
			{
				Object.Destroy((Object)(object)instance);
				instance = null;
			}
		}
	}

	private struct LimbMaskDisplay
	{
		public ItemIndex itemIndex;

		public EquipmentIndex equipmentIndex;

		public LimbFlags maskValue;

		public void Apply(CharacterModel characterModel, LimbFlags mask)
		{
			maskValue = mask;
			characterModel.limbFlagSet.AddFlags(mask);
		}

		public void Undo(CharacterModel characterModel)
		{
			characterModel.limbFlagSet.RemoveFlags(maskValue);
		}
	}

	[Serializable]
	private class LimbFlagSet
	{
		private readonly byte[] flagCounts = new byte[5];

		private LimbFlags flags;

		private static readonly int[] primeConversionTable;

		public int materialMaskValue { get; private set; }

		public LimbFlagSet()
		{
			materialMaskValue = 1;
		}

		static LimbFlagSet()
		{
			int[] array = new int[5] { 2, 3, 5, 11, 17 };
			primeConversionTable = new int[31];
			for (int i = 0; i < primeConversionTable.Length; i++)
			{
				int num = 1;
				for (int j = 0; j < 5; j++)
				{
					if ((i & (1 << j)) != 0)
					{
						num *= array[j];
					}
				}
				primeConversionTable[i] = num;
			}
		}

		private static int ConvertLimbFlagsToMaterialMask(LimbFlags limbFlags)
		{
			return primeConversionTable[(int)limbFlags];
		}

		public void AddFlags(LimbFlags addedFlags)
		{
			LimbFlags limbFlags = flags;
			flags |= addedFlags;
			for (int i = 0; i < 5; i++)
			{
				if (((uint)addedFlags & (uint)(1 << i)) != 0)
				{
					flagCounts[i]++;
				}
			}
			if (flags != limbFlags)
			{
				materialMaskValue = ConvertLimbFlagsToMaterialMask(flags);
			}
		}

		public void RemoveFlags(LimbFlags removedFlags)
		{
			LimbFlags limbFlags = flags;
			for (int i = 0; i < 5; i++)
			{
				if (((uint)removedFlags & (uint)(1 << i)) != 0 && --flagCounts[i] == 0)
				{
					flags &= (LimbFlags)(~(1 << i));
				}
			}
			if (flags != limbFlags)
			{
				materialMaskValue = ConvertLimbFlagsToMaterialMask(flags);
			}
		}
	}

	public CharacterBody body;

	public ItemDisplayRuleSet itemDisplayRuleSet;

	public bool autoPopulateLightInfos = true;

	[FormerlySerializedAs("rendererInfos")]
	public RendererInfo[] baseRendererInfos = Array.Empty<RendererInfo>();

	public LightInfo[] baseLightInfos = Array.Empty<LightInfo>();

	private ChildLocator childLocator;

	private GameObject goldAffixEffect;

	private HurtBoxInfo[] hurtBoxInfos = Array.Empty<HurtBoxInfo>();

	private Transform coreTransform;

	private static readonly Color hitFlashBaseColor;

	private static readonly Color hitFlashShieldColor;

	private static readonly Color healFlashColor;

	private static readonly float hitFlashDuration;

	private static readonly float healFlashDuration;

	private VisibilityLevel _visibility = VisibilityLevel.Visible;

	private bool _isGhost;

	private bool _isDoppelganger;

	private bool _isEcho;

	[NonSerialized]
	[HideInInspector]
	public int invisibilityCount;

	[NonSerialized]
	public List<TemporaryOverlay> temporaryOverlays = new List<TemporaryOverlay>();

	private bool materialsDirty = true;

	private MaterialPropertyBlock propertyStorage;

	private EquipmentIndex inventoryEquipmentIndex = EquipmentIndex.None;

	private EliteIndex myEliteIndex = EliteIndex.None;

	private float fade = 1f;

	private float firstPersonFade = 1f;

	private SkinnedMeshRenderer mainSkinnedMeshRenderer;

	private static readonly Color poisonEliteLightColor;

	private static readonly Color hauntedEliteLightColor;

	private static readonly Color lunarEliteLightColor;

	private static readonly Color voidEliteLightColor;

	private Color? lightColorOverride;

	private Material particleMaterialOverride;

	private GameObject poisonAffixEffect;

	private GameObject hauntedAffixEffect;

	private GameObject voidAffixEffect;

	private float affixHauntedCloakLockoutDuration = 3f;

	private EquipmentIndex currentEquipmentDisplayIndex = EquipmentIndex.None;

	private ItemMask enabledItemDisplays;

	private List<ParentedPrefabDisplay> parentedPrefabDisplays = new List<ParentedPrefabDisplay>();

	private List<LimbMaskDisplay> limbMaskDisplays = new List<LimbMaskDisplay>();

	private LimbFlagSet limbFlagSet = new LimbFlagSet();

	public static Material revealedMaterial;

	public static Material cloakedMaterial;

	public static Material ghostMaterial;

	public static Material bellBuffMaterial;

	public static Material wolfhatMaterial;

	public static Material energyShieldMaterial;

	public static Material fullCritMaterial;

	public static Material beetleJuiceMaterial;

	public static Material brittleMaterial;

	public static Material clayGooMaterial;

	public static Material slow80Material;

	public static Material immuneMaterial;

	public static Material elitePoisonOverlayMaterial;

	public static Material elitePoisonParticleReplacementMaterial;

	public static Material eliteHauntedOverlayMaterial;

	public static Material eliteJustHauntedOverlayMaterial;

	public static Material eliteHauntedParticleReplacementMaterial;

	public static Material eliteLunarParticleReplacementMaterial;

	public static Material eliteVoidParticleReplacementMaterial;

	public static Material eliteVoidOverlayMaterial;

	public static Material weakMaterial;

	public static Material pulverizedMaterial;

	public static Material doppelgangerMaterial;

	public static Material ghostParticleReplacementMaterial;

	public static Material lunarGolemShieldMaterial;

	public static Material echoMaterial;

	public static Material gummyCloneMaterial;

	public static Material voidSurvivorCorruptMaterial;

	public static Material voidShieldMaterial;

	private static readonly int maxOverlays;

	private Material[] currentOverlays = (Material[])(object)new Material[maxOverlays];

	private int activeOverlayCount;

	private bool wasPreviouslyClayGooed;

	private bool wasPreviouslyHaunted;

	private RtpcSetter rtpcEliteEnemy;

	private int shaderEliteRampIndex = -1;

	private static Material[][] sharedMaterialArrays;

	private static readonly int maxMaterials;

	public VisibilityLevel visibility
	{
		get
		{
			return _visibility;
		}
		set
		{
			if (_visibility != value)
			{
				_visibility = value;
				materialsDirty = true;
			}
		}
	}

	public bool isGhost
	{
		get
		{
			return _isGhost;
		}
		set
		{
			if (_isGhost != value)
			{
				_isGhost = value;
				materialsDirty = true;
			}
		}
	}

	public bool isDoppelganger
	{
		get
		{
			return _isDoppelganger;
		}
		set
		{
			if (_isDoppelganger != value)
			{
				_isDoppelganger = value;
				materialsDirty = true;
			}
		}
	}

	public bool isEcho
	{
		get
		{
			return _isEcho;
		}
		set
		{
			if (_isEcho != value)
			{
				_isEcho = value;
				materialsDirty = true;
			}
		}
	}

	private void Awake()
	{
		//IL_0090: Unknown result type (might be due to invalid IL or missing references)
		//IL_009a: Expected O, but got Unknown
		//IL_00c6: Unknown result type (might be due to invalid IL or missing references)
		//IL_00d0: Expected O, but got Unknown
		enabledItemDisplays = ItemMask.Rent();
		childLocator = ((Component)this).GetComponent<ChildLocator>();
		HurtBoxGroup component = ((Component)this).GetComponent<HurtBoxGroup>();
		coreTransform = ((Component)this).transform;
		if (Object.op_Implicit((Object)(object)component))
		{
			HurtBox mainHurtBox = component.mainHurtBox;
			coreTransform = ((mainHurtBox != null) ? ((Component)mainHurtBox).transform : null) ?? coreTransform;
			HurtBox[] hurtBoxes = component.hurtBoxes;
			if (hurtBoxes.Length != 0)
			{
				hurtBoxInfos = new HurtBoxInfo[hurtBoxes.Length];
				for (int i = 0; i < hurtBoxes.Length; i++)
				{
					hurtBoxInfos[i] = new HurtBoxInfo(hurtBoxes[i]);
				}
			}
		}
		propertyStorage = new MaterialPropertyBlock();
		RendererInfo[] array = baseRendererInfos;
		for (int j = 0; j < array.Length; j++)
		{
			RendererInfo rendererInfo = array[j];
			if (rendererInfo.renderer is SkinnedMeshRenderer)
			{
				mainSkinnedMeshRenderer = (SkinnedMeshRenderer)rendererInfo.renderer;
				break;
			}
		}
		if (Object.op_Implicit((Object)(object)body) && Util.IsPrefab(((Component)body).gameObject) && !Util.IsPrefab(((Component)this).gameObject))
		{
			body = null;
		}
	}

	private void Start()
	{
		visibility = VisibilityLevel.Invisible;
		UpdateMaterials();
	}

	private void OnEnable()
	{
		InstanceTracker.Add(this);
		if (body != null)
		{
			rtpcEliteEnemy = new RtpcSetter("eliteEnemy", ((Component)body).gameObject);
			body.onInventoryChanged += OnInventoryChanged;
		}
	}

	private void OnDisable()
	{
		InstanceUpdate();
		if (body != null)
		{
			body.onInventoryChanged -= OnInventoryChanged;
		}
		InstanceTracker.Remove(this);
	}

	private void OnDestroy()
	{
		ItemMask.Return(enabledItemDisplays);
	}

	private void OnInventoryChanged()
	{
		if (Object.op_Implicit((Object)(object)body))
		{
			Inventory inventory = body.inventory;
			if (Object.op_Implicit((Object)(object)inventory))
			{
				UpdateItemDisplay(inventory);
				inventoryEquipmentIndex = inventory.GetEquipmentIndex();
				SetEquipmentDisplay(inventoryEquipmentIndex);
			}
		}
	}

	private void InstanceUpdate()
	{
		//IL_002b: Unknown result type (might be due to invalid IL or missing references)
		//IL_005d: Unknown result type (might be due to invalid IL or missing references)
		//IL_008f: Unknown result type (might be due to invalid IL or missing references)
		//IL_00dd: Unknown result type (might be due to invalid IL or missing references)
		if (isGhost)
		{
			particleMaterialOverride = ghostParticleReplacementMaterial;
		}
		else if (myEliteIndex == RoR2Content.Elites.Poison.eliteIndex)
		{
			lightColorOverride = poisonEliteLightColor;
			particleMaterialOverride = elitePoisonParticleReplacementMaterial;
		}
		else if (myEliteIndex == RoR2Content.Elites.Haunted.eliteIndex)
		{
			lightColorOverride = hauntedEliteLightColor;
			particleMaterialOverride = eliteHauntedParticleReplacementMaterial;
		}
		else if (myEliteIndex == RoR2Content.Elites.Lunar.eliteIndex)
		{
			lightColorOverride = lunarEliteLightColor;
			particleMaterialOverride = eliteLunarParticleReplacementMaterial;
		}
		else if (myEliteIndex == DLC1Content.Elites.Void.eliteIndex && Object.op_Implicit((Object)(object)body) && body.healthComponent.alive)
		{
			lightColorOverride = voidEliteLightColor;
			particleMaterialOverride = eliteVoidParticleReplacementMaterial;
		}
		else
		{
			lightColorOverride = null;
			particleMaterialOverride = null;
		}
		UpdateGoldAffix();
		UpdatePoisonAffix();
		UpdateHauntedAffix();
		UpdateVoidAffix();
		UpdateLights();
	}

	private void UpdateLights()
	{
		//IL_001f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0024: Unknown result type (might be due to invalid IL or missing references)
		//IL_0035: Unknown result type (might be due to invalid IL or missing references)
		//IL_005c: Unknown result type (might be due to invalid IL or missing references)
		LightInfo[] array = baseLightInfos;
		if (array.Length == 0)
		{
			return;
		}
		if (lightColorOverride.HasValue)
		{
			Color value = lightColorOverride.Value;
			for (int i = 0; i < array.Length; i++)
			{
				array[i].light.color = value;
			}
		}
		else
		{
			for (int j = 0; j < array.Length; j++)
			{
				ref LightInfo reference = ref array[j];
				reference.light.color = reference.defaultColor;
			}
		}
	}

	[RuntimeInitializeOnLoadMethod]
	private static void Init()
	{
		RoR2Application.onLateUpdate += StaticUpdate;
	}

	private static void StaticUpdate()
	{
		foreach (CharacterModel instances in InstanceTracker.GetInstancesList<CharacterModel>())
		{
			instances.InstanceUpdate();
		}
	}

	private bool IsCurrentEliteType(EliteDef eliteDef)
	{
		if (eliteDef == null || eliteDef.eliteIndex == EliteIndex.None)
		{
			return false;
		}
		return eliteDef.eliteIndex == myEliteIndex;
	}

	private void UpdateGoldAffix()
	{
		//IL_004b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0050: Unknown result type (might be due to invalid IL or missing references)
		if (IsCurrentEliteType(JunkContent.Elites.Gold) == Object.op_Implicit((Object)(object)goldAffixEffect))
		{
			return;
		}
		if (!Object.op_Implicit((Object)(object)goldAffixEffect))
		{
			goldAffixEffect = Object.Instantiate<GameObject>(LegacyResourcesAPI.Load<GameObject>("Prefabs/GoldAffixEffect"), ((Component)this).transform);
			ShapeModule shape = goldAffixEffect.GetComponent<ParticleSystem>().shape;
			if (Object.op_Implicit((Object)(object)mainSkinnedMeshRenderer))
			{
				((ShapeModule)(ref shape)).shapeType = (ParticleSystemShapeType)14;
				((ShapeModule)(ref shape)).skinnedMeshRenderer = mainSkinnedMeshRenderer;
			}
		}
		else
		{
			Object.Destroy((Object)(object)goldAffixEffect);
			goldAffixEffect = null;
		}
	}

	private void UpdatePoisonAffix()
	{
		if ((myEliteIndex == RoR2Content.Elites.Poison.eliteIndex && body.healthComponent.alive) == Object.op_Implicit((Object)(object)poisonAffixEffect))
		{
			return;
		}
		if (!Object.op_Implicit((Object)(object)poisonAffixEffect))
		{
			poisonAffixEffect = Object.Instantiate<GameObject>(LegacyResourcesAPI.Load<GameObject>("Prefabs/PoisonAffixEffect"), ((Component)this).transform);
			if (Object.op_Implicit((Object)(object)mainSkinnedMeshRenderer))
			{
				JitterBones[] components = poisonAffixEffect.GetComponents<JitterBones>();
				for (int i = 0; i < components.Length; i++)
				{
					components[i].skinnedMeshRenderer = mainSkinnedMeshRenderer;
				}
			}
		}
		else
		{
			Object.Destroy((Object)(object)poisonAffixEffect);
			poisonAffixEffect = null;
		}
	}

	private void UpdateHauntedAffix()
	{
		if ((myEliteIndex == RoR2Content.Elites.Haunted.eliteIndex && body.healthComponent.alive) == Object.op_Implicit((Object)(object)hauntedAffixEffect))
		{
			return;
		}
		if (!Object.op_Implicit((Object)(object)hauntedAffixEffect))
		{
			hauntedAffixEffect = Object.Instantiate<GameObject>(LegacyResourcesAPI.Load<GameObject>("Prefabs/HauntedAffixEffect"), ((Component)this).transform);
			if (Object.op_Implicit((Object)(object)mainSkinnedMeshRenderer))
			{
				JitterBones[] components = hauntedAffixEffect.GetComponents<JitterBones>();
				for (int i = 0; i < components.Length; i++)
				{
					components[i].skinnedMeshRenderer = mainSkinnedMeshRenderer;
				}
			}
		}
		else
		{
			Object.Destroy((Object)(object)hauntedAffixEffect);
			hauntedAffixEffect = null;
		}
	}

	private void UpdateVoidAffix()
	{
		//IL_0045: Unknown result type (might be due to invalid IL or missing references)
		//IL_004a: Unknown result type (might be due to invalid IL or missing references)
		if ((myEliteIndex == DLC1Content.Elites.Void.eliteIndex && body.healthComponent.alive) == Object.op_Implicit((Object)(object)voidAffixEffect))
		{
			return;
		}
		if (!Object.op_Implicit((Object)(object)voidAffixEffect))
		{
			voidAffixEffect = Object.Instantiate<GameObject>(Addressables.LoadAssetAsync<GameObject>((object)"RoR2/DLC1/EliteVoid/VoidAffixEffect.prefab").WaitForCompletion(), ((Component)this).transform);
			if (Object.op_Implicit((Object)(object)mainSkinnedMeshRenderer))
			{
				JitterBones[] components = voidAffixEffect.GetComponents<JitterBones>();
				for (int i = 0; i < components.Length; i++)
				{
					components[i].skinnedMeshRenderer = mainSkinnedMeshRenderer;
				}
			}
		}
		else
		{
			Object.Destroy((Object)(object)voidAffixEffect);
			voidAffixEffect = null;
		}
	}

	private void OnValidate()
	{
		//IL_002d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0032: Unknown result type (might be due to invalid IL or missing references)
		if (Application.isPlaying)
		{
			return;
		}
		for (int i = 0; i < baseLightInfos.Length; i++)
		{
			ref LightInfo reference = ref baseLightInfos[i];
			if (Object.op_Implicit((Object)(object)reference.light))
			{
				reference.defaultColor = reference.light.color;
			}
		}
		if (!Object.op_Implicit((Object)(object)itemDisplayRuleSet))
		{
			Debug.LogErrorFormat("CharacterModel \"{0}\" does not have the itemDisplayRuleSet field assigned.", new object[1] { ((Component)this).gameObject });
		}
		if (autoPopulateLightInfos)
		{
			LightInfo[] first = (from light in ((Component)this).GetComponentsInChildren<Light>()
				select new LightInfo(light)).ToArray();
			if (!first.SequenceEqual(baseLightInfos))
			{
				baseLightInfos = first;
			}
		}
	}

	private static void RefreshObstructorsForCamera(CameraRigController cameraRigController)
	{
		//IL_0006: Unknown result type (might be due to invalid IL or missing references)
		//IL_000b: Unknown result type (might be due to invalid IL or missing references)
		//IL_002a: Unknown result type (might be due to invalid IL or missing references)
		Vector3 position = ((Component)cameraRigController).transform.position;
		foreach (CharacterModel instances in InstanceTracker.GetInstancesList<CharacterModel>())
		{
			if (cameraRigController.enableFading)
			{
				float nearestHurtBoxDistance = instances.GetNearestHurtBoxDistance(position);
				instances.fade = Mathf.Clamp01(Util.Remap(nearestHurtBoxDistance, cameraRigController.fadeStartDistance, cameraRigController.fadeEndDistance, 0f, 1f));
			}
			else
			{
				instances.fade = 1f;
			}
		}
	}

	private float GetNearestHurtBoxDistance(Vector3 cameraPosition)
	{
		//IL_001b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0020: Unknown result type (might be due to invalid IL or missing references)
		float num = float.PositiveInfinity;
		for (int i = 0; i < hurtBoxInfos.Length; i++)
		{
			float num2 = Vector3.Distance(hurtBoxInfos[i].transform.position, cameraPosition) - hurtBoxInfos[i].estimatedRadius;
			if (num2 < num)
			{
				num = Mathf.Min(num2, num);
			}
		}
		return num;
	}

	private void UpdateForCamera(CameraRigController cameraRigController)
	{
		visibility = VisibilityLevel.Visible;
		float num = 1f;
		if (Object.op_Implicit((Object)(object)body))
		{
			if (cameraRigController.firstPersonTarget == ((Component)body).gameObject)
			{
				num = 0f;
			}
			visibility = body.GetVisibilityLevel(cameraRigController.targetTeamIndex);
		}
		firstPersonFade = Mathf.MoveTowards(firstPersonFade, num, Time.deltaTime / 0.25f);
		fade *= firstPersonFade;
		if (fade <= 0f || invisibilityCount > 0)
		{
			visibility = VisibilityLevel.Invisible;
		}
		UpdateOverlays();
		if (materialsDirty)
		{
			UpdateMaterials();
			materialsDirty = false;
		}
	}

	static CharacterModel()
	{
		//IL_000e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0013: Unknown result type (might be due to invalid IL or missing references)
		//IL_0018: Unknown result type (might be due to invalid IL or missing references)
		//IL_0031: Unknown result type (might be due to invalid IL or missing references)
		//IL_0036: Unknown result type (might be due to invalid IL or missing references)
		//IL_003b: Unknown result type (might be due to invalid IL or missing references)
		//IL_004e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0053: Unknown result type (might be due to invalid IL or missing references)
		//IL_0058: Unknown result type (might be due to invalid IL or missing references)
		//IL_0082: Unknown result type (might be due to invalid IL or missing references)
		//IL_0087: Unknown result type (might be due to invalid IL or missing references)
		//IL_008c: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a5: Unknown result type (might be due to invalid IL or missing references)
		//IL_00aa: Unknown result type (might be due to invalid IL or missing references)
		//IL_00af: Unknown result type (might be due to invalid IL or missing references)
		//IL_00c5: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ca: Unknown result type (might be due to invalid IL or missing references)
		//IL_00cf: Unknown result type (might be due to invalid IL or missing references)
		//IL_00e5: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ea: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ef: Unknown result type (might be due to invalid IL or missing references)
		hitFlashBaseColor = Color32.op_Implicit(new Color32((byte)193, (byte)108, (byte)51, byte.MaxValue));
		hitFlashShieldColor = Color32.op_Implicit(new Color32((byte)132, (byte)159, byte.MaxValue, byte.MaxValue));
		healFlashColor = Color32.op_Implicit(new Color32((byte)104, (byte)196, (byte)49, byte.MaxValue));
		hitFlashDuration = 0.15f;
		healFlashDuration = 0.35f;
		poisonEliteLightColor = Color32.op_Implicit(new Color32((byte)90, byte.MaxValue, (byte)193, (byte)204));
		hauntedEliteLightColor = Color32.op_Implicit(new Color32((byte)152, (byte)228, (byte)217, (byte)204));
		lunarEliteLightColor = Color32.op_Implicit(new Color32(byte.MaxValue, byte.MaxValue, byte.MaxValue, (byte)127));
		voidEliteLightColor = Color32.op_Implicit(new Color32((byte)151, (byte)78, (byte)132, (byte)204));
		maxOverlays = 6;
		maxMaterials = 1 + maxOverlays;
		SceneCamera.onSceneCameraPreRender += OnSceneCameraPreRender;
	}

	private static void OnSceneCameraPreRender(SceneCamera sceneCamera)
	{
		if (Object.op_Implicit((Object)(object)sceneCamera.cameraRigController))
		{
			RefreshObstructorsForCamera(sceneCamera.cameraRigController);
		}
		if (!Object.op_Implicit((Object)(object)sceneCamera.cameraRigController))
		{
			return;
		}
		foreach (CharacterModel instances in InstanceTracker.GetInstancesList<CharacterModel>())
		{
			instances.UpdateForCamera(sceneCamera.cameraRigController);
		}
	}

	private void InstantiateDisplayRuleGroup(DisplayRuleGroup displayRuleGroup, ItemIndex itemIndex, EquipmentIndex equipmentIndex)
	{
		//IL_0089: Unknown result type (might be due to invalid IL or missing references)
		//IL_008f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0094: Unknown result type (might be due to invalid IL or missing references)
		//IL_009a: Unknown result type (might be due to invalid IL or missing references)
		if (displayRuleGroup.rules == null)
		{
			return;
		}
		for (int i = 0; i < displayRuleGroup.rules.Length; i++)
		{
			ItemDisplayRule itemDisplayRule = displayRuleGroup.rules[i];
			switch (itemDisplayRule.ruleType)
			{
			case ItemDisplayRuleType.ParentedPrefab:
				if (Object.op_Implicit((Object)(object)childLocator))
				{
					Transform val = childLocator.FindChild(itemDisplayRule.childName);
					if (Object.op_Implicit((Object)(object)val))
					{
						ParentedPrefabDisplay parentedPrefabDisplay = default(ParentedPrefabDisplay);
						parentedPrefabDisplay.itemIndex = itemIndex;
						parentedPrefabDisplay.equipmentIndex = equipmentIndex;
						ParentedPrefabDisplay item2 = parentedPrefabDisplay;
						item2.Apply(this, itemDisplayRule.followerPrefab, val, itemDisplayRule.localPos, Quaternion.Euler(itemDisplayRule.localAngles), itemDisplayRule.localScale);
						parentedPrefabDisplays.Add(item2);
					}
				}
				break;
			case ItemDisplayRuleType.LimbMask:
			{
				LimbMaskDisplay limbMaskDisplay = default(LimbMaskDisplay);
				limbMaskDisplay.itemIndex = itemIndex;
				limbMaskDisplay.equipmentIndex = equipmentIndex;
				LimbMaskDisplay item = limbMaskDisplay;
				item.Apply(this, itemDisplayRule.limbMask);
				limbMaskDisplays.Add(item);
				break;
			}
			}
		}
	}

	private void SetEquipmentDisplay(EquipmentIndex newEquipmentIndex)
	{
		if (newEquipmentIndex == currentEquipmentDisplayIndex)
		{
			return;
		}
		for (int num = parentedPrefabDisplays.Count - 1; num >= 0; num--)
		{
			if (parentedPrefabDisplays[num].equipmentIndex != EquipmentIndex.None)
			{
				parentedPrefabDisplays[num].Undo();
				parentedPrefabDisplays.RemoveAt(num);
			}
		}
		for (int num2 = limbMaskDisplays.Count - 1; num2 >= 0; num2--)
		{
			if (limbMaskDisplays[num2].equipmentIndex != EquipmentIndex.None)
			{
				limbMaskDisplays[num2].Undo(this);
				limbMaskDisplays.RemoveAt(num2);
			}
		}
		currentEquipmentDisplayIndex = newEquipmentIndex;
		if (Object.op_Implicit((Object)(object)itemDisplayRuleSet))
		{
			DisplayRuleGroup equipmentDisplayRuleGroup = itemDisplayRuleSet.GetEquipmentDisplayRuleGroup(newEquipmentIndex);
			InstantiateDisplayRuleGroup(equipmentDisplayRuleGroup, ItemIndex.None, newEquipmentIndex);
		}
	}

	private void EnableItemDisplay(ItemIndex itemIndex)
	{
		if (!enabledItemDisplays.Contains(itemIndex))
		{
			enabledItemDisplays.Add(itemIndex);
			if (Object.op_Implicit((Object)(object)itemDisplayRuleSet))
			{
				DisplayRuleGroup itemDisplayRuleGroup = itemDisplayRuleSet.GetItemDisplayRuleGroup(itemIndex);
				InstantiateDisplayRuleGroup(itemDisplayRuleGroup, itemIndex, EquipmentIndex.None);
			}
		}
	}

	private void DisableItemDisplay(ItemIndex itemIndex)
	{
		if (!enabledItemDisplays.Contains(itemIndex))
		{
			return;
		}
		enabledItemDisplays.Remove(itemIndex);
		for (int num = parentedPrefabDisplays.Count - 1; num >= 0; num--)
		{
			if (parentedPrefabDisplays[num].itemIndex == itemIndex)
			{
				parentedPrefabDisplays[num].Undo();
				parentedPrefabDisplays.RemoveAt(num);
			}
		}
		for (int num2 = limbMaskDisplays.Count - 1; num2 >= 0; num2--)
		{
			if (limbMaskDisplays[num2].itemIndex == itemIndex)
			{
				limbMaskDisplays[num2].Undo(this);
				limbMaskDisplays.RemoveAt(num2);
			}
		}
	}

	public void UpdateItemDisplay(Inventory inventory)
	{
		ItemIndex itemIndex = (ItemIndex)0;
		for (ItemIndex itemCount = (ItemIndex)ItemCatalog.itemCount; itemIndex < itemCount; itemIndex++)
		{
			if (inventory.GetItemCount(itemIndex) > 0)
			{
				EnableItemDisplay(itemIndex);
			}
			else
			{
				DisableItemDisplay(itemIndex);
			}
		}
	}

	public void HighlightItemDisplay(ItemIndex itemIndex)
	{
		if (!enabledItemDisplays.Contains(itemIndex))
		{
			return;
		}
		ItemTierDef itemTierDef = ItemTierCatalog.GetItemTierDef(ItemCatalog.GetItemDef(itemIndex).tier);
		GameObject val = null;
		val = ((!Object.op_Implicit((Object)(object)itemTierDef) || !Object.op_Implicit((Object)(object)itemTierDef.highlightPrefab)) ? LegacyResourcesAPI.Load<GameObject>("Prefabs/UI/HighlightTier1Item") : itemTierDef.highlightPrefab);
		for (int num = parentedPrefabDisplays.Count - 1; num >= 0; num--)
		{
			if (parentedPrefabDisplays[num].itemIndex == itemIndex)
			{
				GameObject instance = parentedPrefabDisplays[num].instance;
				if (Object.op_Implicit((Object)(object)instance))
				{
					Renderer componentInChildren = instance.GetComponentInChildren<Renderer>();
					if (Object.op_Implicit((Object)(object)componentInChildren) && Object.op_Implicit((Object)(object)body))
					{
						HighlightRect.CreateHighlight(((Component)body).gameObject, componentInChildren, val);
					}
				}
			}
		}
	}

	public List<GameObject> GetEquipmentDisplayObjects(EquipmentIndex equipmentIndex)
	{
		List<GameObject> list = new List<GameObject>();
		for (int num = parentedPrefabDisplays.Count - 1; num >= 0; num--)
		{
			if (parentedPrefabDisplays[num].equipmentIndex == equipmentIndex)
			{
				GameObject instance = parentedPrefabDisplays[num].instance;
				list.Add(instance);
			}
		}
		return list;
	}

	public List<GameObject> GetItemDisplayObjects(ItemIndex itemIndex)
	{
		List<GameObject> list = new List<GameObject>();
		for (int num = parentedPrefabDisplays.Count - 1; num >= 0; num--)
		{
			if (parentedPrefabDisplays[num].itemIndex == itemIndex)
			{
				GameObject instance = parentedPrefabDisplays[num].instance;
				list.Add(instance);
			}
		}
		return list;
	}

	[RuntimeInitializeOnLoadMethod]
	private static void InitMaterials()
	{
		//IL_017c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0181: Unknown result type (might be due to invalid IL or missing references)
		//IL_0193: Unknown result type (might be due to invalid IL or missing references)
		//IL_0198: Unknown result type (might be due to invalid IL or missing references)
		//IL_01aa: Unknown result type (might be due to invalid IL or missing references)
		//IL_01af: Unknown result type (might be due to invalid IL or missing references)
		//IL_01c1: Unknown result type (might be due to invalid IL or missing references)
		//IL_01c6: Unknown result type (might be due to invalid IL or missing references)
		revealedMaterial = LegacyResourcesAPI.Load<Material>("Materials/matRevealedEffect");
		cloakedMaterial = LegacyResourcesAPI.Load<Material>("Materials/matCloakedEffect");
		ghostMaterial = LegacyResourcesAPI.Load<Material>("Materials/matGhostEffect");
		ghostParticleReplacementMaterial = LegacyResourcesAPI.Load<Material>("Materials/matGhostParticleReplacement");
		wolfhatMaterial = LegacyResourcesAPI.Load<Material>("Materials/matWolfhatOverlay");
		energyShieldMaterial = LegacyResourcesAPI.Load<Material>("Materials/matEnergyShield");
		beetleJuiceMaterial = LegacyResourcesAPI.Load<Material>("Materials/matBeetleJuice");
		brittleMaterial = LegacyResourcesAPI.Load<Material>("Materials/matBrittle");
		fullCritMaterial = LegacyResourcesAPI.Load<Material>("Materials/matFullCrit");
		clayGooMaterial = LegacyResourcesAPI.Load<Material>("Materials/matClayGooDebuff");
		slow80Material = LegacyResourcesAPI.Load<Material>("Materials/matSlow80Debuff");
		immuneMaterial = LegacyResourcesAPI.Load<Material>("Materials/matImmune");
		bellBuffMaterial = LegacyResourcesAPI.Load<Material>("Materials/matBellBuff");
		elitePoisonOverlayMaterial = LegacyResourcesAPI.Load<Material>("Materials/matElitePoisonOverlay");
		elitePoisonParticleReplacementMaterial = LegacyResourcesAPI.Load<Material>("Materials/matElitePoisonParticleReplacement");
		eliteHauntedOverlayMaterial = LegacyResourcesAPI.Load<Material>("Materials/matEliteHauntedOverlay");
		eliteHauntedParticleReplacementMaterial = LegacyResourcesAPI.Load<Material>("Materials/matEliteHauntedParticleReplacement");
		eliteJustHauntedOverlayMaterial = LegacyResourcesAPI.Load<Material>("Materials/matEliteJustHauntedOverlay");
		eliteLunarParticleReplacementMaterial = LegacyResourcesAPI.Load<Material>("Materials/matEliteLunarParticleReplacement");
		doppelgangerMaterial = LegacyResourcesAPI.Load<Material>("Materials/matDoppelganger");
		weakMaterial = LegacyResourcesAPI.Load<Material>("Materials/matWeakOverlay");
		pulverizedMaterial = LegacyResourcesAPI.Load<Material>("Materials/matPulverizedOverlay");
		lunarGolemShieldMaterial = LegacyResourcesAPI.Load<Material>("Materials/matLunarGolemShield");
		echoMaterial = LegacyResourcesAPI.Load<Material>("Materials/matEcho");
		gummyCloneMaterial = LegacyResourcesAPI.Load<Material>("Materials/matGummyClone");
		eliteVoidParticleReplacementMaterial = Addressables.LoadAssetAsync<Material>((object)"RoR2/DLC1/EliteVoid/matEliteVoidParticleReplacement.mat").WaitForCompletion();
		eliteVoidOverlayMaterial = Addressables.LoadAssetAsync<Material>((object)"RoR2/DLC1/EliteVoid/matEliteVoidOverlay.mat").WaitForCompletion();
		voidSurvivorCorruptMaterial = Addressables.LoadAssetAsync<Material>((object)"RoR2/DLC1/VoidSurvivor/matVoidSurvivorCorruptOverlay.mat").WaitForCompletion();
		voidShieldMaterial = Addressables.LoadAssetAsync<Material>((object)"RoR2/DLC1/MissileVoid/matEnergyShieldVoid.mat").WaitForCompletion();
	}

	private void UpdateOverlays()
	{
		for (int i = 0; i < activeOverlayCount; i++)
		{
			currentOverlays[i] = null;
		}
		activeOverlayCount = 0;
		if (visibility == VisibilityLevel.Invisible)
		{
			return;
		}
		EquipmentDef equipmentDef = EquipmentCatalog.GetEquipmentDef(inventoryEquipmentIndex);
		myEliteIndex = equipmentDef?.passiveBuffDef?.eliteDef?.eliteIndex ?? EliteIndex.None;
		shaderEliteRampIndex = equipmentDef?.passiveBuffDef?.eliteDef?.shaderEliteRampIndex ?? (-1);
		bool flag = false;
		bool flag2 = false;
		if (Object.op_Implicit((Object)(object)body))
		{
			flag = body.HasBuff(RoR2Content.Buffs.ClayGoo);
			flag2 = body.HasBuff(RoR2Content.Buffs.AffixHauntedRecipient);
			rtpcEliteEnemy.value = ((myEliteIndex != EliteIndex.None) ? 1f : 0f);
			rtpcEliteEnemy.FlushIfChanged();
			Inventory inventory = body.inventory;
			isGhost = inventory != null && inventory.GetItemCount(RoR2Content.Items.Ghost) > 0;
			Inventory inventory2 = body.inventory;
			isDoppelganger = inventory2 != null && inventory2.GetItemCount(RoR2Content.Items.InvadingDoppelganger) > 0;
			Inventory inventory3 = body.inventory;
			isEcho = inventory3 != null && inventory3.GetItemCount(RoR2Content.Items.SummonedEcho) > 0;
			Inventory inventory4 = body.inventory;
			bool flag3 = inventory4 != null && inventory4.GetItemCount(DLC1Content.Items.MissileVoid) > 0;
			AddOverlay(ghostMaterial, isGhost);
			AddOverlay(doppelgangerMaterial, isDoppelganger);
			AddOverlay(clayGooMaterial, flag);
			AddOverlay(elitePoisonOverlayMaterial, myEliteIndex == RoR2Content.Elites.Poison.eliteIndex || body.HasBuff(RoR2Content.Buffs.HealingDisabled));
			AddOverlay(eliteHauntedOverlayMaterial, body.HasBuff(RoR2Content.Buffs.AffixHaunted));
			AddOverlay(eliteVoidOverlayMaterial, body.HasBuff(DLC1Content.Buffs.EliteVoid) && Object.op_Implicit((Object)(object)body.healthComponent) && body.healthComponent.alive);
			AddOverlay(pulverizedMaterial, body.HasBuff(RoR2Content.Buffs.Pulverized));
			AddOverlay(weakMaterial, body.HasBuff(RoR2Content.Buffs.Weak));
			AddOverlay(fullCritMaterial, body.HasBuff(RoR2Content.Buffs.FullCrit));
			AddOverlay(wolfhatMaterial, body.HasBuff(RoR2Content.Buffs.AttackSpeedOnCrit));
			AddOverlay(flag3 ? voidShieldMaterial : energyShieldMaterial, Object.op_Implicit((Object)(object)body.healthComponent) && body.healthComponent.shield > 0f);
			AddOverlay(beetleJuiceMaterial, body.HasBuff(RoR2Content.Buffs.BeetleJuice));
			AddOverlay(immuneMaterial, body.HasBuff(RoR2Content.Buffs.Immune));
			AddOverlay(slow80Material, body.HasBuff(RoR2Content.Buffs.Slow80));
			AddOverlay(brittleMaterial, Object.op_Implicit((Object)(object)body.inventory) && body.inventory.GetItemCount(RoR2Content.Items.LunarDagger) > 0);
			AddOverlay(lunarGolemShieldMaterial, body.HasBuff(RoR2Content.Buffs.LunarShell));
			AddOverlay(echoMaterial, isEcho);
			AddOverlay(gummyCloneMaterial, IsGummyClone());
			AddOverlay(voidSurvivorCorruptMaterial, body.HasBuff(DLC1Content.Buffs.VoidSurvivorCorruptMode));
		}
		if (wasPreviouslyClayGooed && !flag)
		{
			TemporaryOverlay temporaryOverlay = ((Component)this).gameObject.AddComponent<TemporaryOverlay>();
			temporaryOverlay.duration = 0.6f;
			temporaryOverlay.animateShaderAlpha = true;
			temporaryOverlay.alphaCurve = AnimationCurve.EaseInOut(0f, 1f, 1f, 0f);
			temporaryOverlay.destroyComponentOnEnd = true;
			temporaryOverlay.originalMaterial = clayGooMaterial;
			temporaryOverlay.AddToCharacerModel(this);
		}
		if (wasPreviouslyHaunted != flag2)
		{
			TemporaryOverlay temporaryOverlay2 = ((Component)this).gameObject.AddComponent<TemporaryOverlay>();
			temporaryOverlay2.duration = 0.5f;
			temporaryOverlay2.animateShaderAlpha = true;
			temporaryOverlay2.alphaCurve = AnimationCurve.EaseInOut(0f, 1f, 1f, 0f);
			temporaryOverlay2.destroyComponentOnEnd = true;
			temporaryOverlay2.originalMaterial = eliteJustHauntedOverlayMaterial;
			temporaryOverlay2.AddToCharacerModel(this);
		}
		wasPreviouslyClayGooed = flag;
		wasPreviouslyHaunted = flag2;
		for (int j = 0; j < temporaryOverlays.Count; j++)
		{
			if (activeOverlayCount >= maxOverlays)
			{
				break;
			}
			currentOverlays[activeOverlayCount++] = temporaryOverlays[j].materialInstance;
		}
		wasPreviouslyClayGooed = flag;
		wasPreviouslyHaunted = flag2;
		materialsDirty = true;
		void AddOverlay(Material overlayMaterial, bool condition)
		{
			if (activeOverlayCount < maxOverlays && condition)
			{
				currentOverlays[activeOverlayCount++] = overlayMaterial;
			}
		}
	}

	[RuntimeInitializeOnLoadMethod]
	private static void InitSharedMaterialsArrays()
	{
		sharedMaterialArrays = new Material[maxMaterials + 1][];
		if (maxMaterials > 0)
		{
			sharedMaterialArrays[0] = Array.Empty<Material>();
			for (int i = 1; i < sharedMaterialArrays.Length; i++)
			{
				sharedMaterialArrays[i] = (Material[])(object)new Material[i];
			}
		}
	}

	private void UpdateRendererMaterials(Renderer renderer, Material defaultMaterial, bool ignoreOverlays)
	{
		Material val = null;
		switch (visibility)
		{
		case VisibilityLevel.Invisible:
			renderer.sharedMaterial = null;
			return;
		case VisibilityLevel.Cloaked:
			if (!ignoreOverlays)
			{
				ignoreOverlays = true;
				val = cloakedMaterial;
			}
			break;
		case VisibilityLevel.Revealed:
			if (!ignoreOverlays)
			{
				val = revealedMaterial;
			}
			break;
		case VisibilityLevel.Visible:
			val = (ignoreOverlays ? (Object.op_Implicit((Object)(object)particleMaterialOverride) ? particleMaterialOverride : defaultMaterial) : ((!isDoppelganger) ? ((!isGhost) ? ((!IsGummyClone()) ? defaultMaterial : gummyCloneMaterial) : ghostMaterial) : doppelgangerMaterial));
			break;
		}
		int num = ((!ignoreOverlays) ? activeOverlayCount : 0);
		if (Object.op_Implicit((Object)(object)val))
		{
			num++;
		}
		Material[] array = sharedMaterialArrays[num];
		int num2 = 0;
		if (Object.op_Implicit((Object)(object)val))
		{
			array[num2++] = val;
		}
		if (!ignoreOverlays)
		{
			for (int i = 0; i < activeOverlayCount; i++)
			{
				array[num2++] = currentOverlays[i];
			}
		}
		renderer.sharedMaterials = array;
	}

	private void UpdateMaterials()
	{
		//IL_0000: Unknown result type (might be due to invalid IL or missing references)
		//IL_0005: Unknown result type (might be due to invalid IL or missing references)
		//IL_007d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0083: Unknown result type (might be due to invalid IL or missing references)
		//IL_0088: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a2: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a9: Unknown result type (might be due to invalid IL or missing references)
		//IL_00af: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b4: Unknown result type (might be due to invalid IL or missing references)
		//IL_0153: Unknown result type (might be due to invalid IL or missing references)
		//IL_017d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0244: Unknown result type (might be due to invalid IL or missing references)
		Color val = Color.black;
		if (Object.op_Implicit((Object)(object)body) && Object.op_Implicit((Object)(object)body.healthComponent))
		{
			float num = Mathf.Clamp01(1f - body.healthComponent.timeSinceLastHit / hitFlashDuration);
			float num2 = Mathf.Pow(Mathf.Clamp01(1f - body.healthComponent.timeSinceLastHeal / healFlashDuration), 0.5f);
			val = ((!(num2 > num)) ? (((body.healthComponent.shield > 0f) ? hitFlashShieldColor : hitFlashBaseColor) * num) : (healFlashColor * num2));
		}
		if (visibility == VisibilityLevel.Invisible)
		{
			for (int num3 = baseRendererInfos.Length - 1; num3 >= 0; num3--)
			{
				RendererInfo rendererInfo = baseRendererInfos[num3];
				rendererInfo.renderer.shadowCastingMode = (ShadowCastingMode)0;
				rendererInfo.renderer.enabled = false;
			}
		}
		else
		{
			for (int num4 = baseRendererInfos.Length - 1; num4 >= 0; num4--)
			{
				RendererInfo rendererInfo2 = baseRendererInfos[num4];
				Renderer renderer = rendererInfo2.renderer;
				UpdateRendererMaterials(renderer, baseRendererInfos[num4].defaultMaterial, baseRendererInfos[num4].ignoreOverlays);
				renderer.shadowCastingMode = rendererInfo2.defaultShadowCastingMode;
				renderer.enabled = true;
				renderer.GetPropertyBlock(propertyStorage);
				propertyStorage.SetColor(CommonShaderProperties._FlashColor, val);
				propertyStorage.SetFloat(CommonShaderProperties._EliteIndex, (float)(shaderEliteRampIndex + 1));
				propertyStorage.SetInt(CommonShaderProperties._LimbPrimeMask, limbFlagSet.materialMaskValue);
				propertyStorage.SetFloat(CommonShaderProperties._Fade, fade);
				renderer.SetPropertyBlock(propertyStorage);
			}
		}
		for (int i = 0; i < parentedPrefabDisplays.Count; i++)
		{
			ItemDisplay itemDisplay = parentedPrefabDisplays[i].itemDisplay;
			itemDisplay.SetVisibilityLevel(visibility);
			for (int j = 0; j < itemDisplay.rendererInfos.Length; j++)
			{
				Renderer renderer2 = itemDisplay.rendererInfos[j].renderer;
				renderer2.GetPropertyBlock(propertyStorage);
				propertyStorage.SetColor(CommonShaderProperties._FlashColor, val);
				propertyStorage.SetFloat(CommonShaderProperties._Fade, fade);
				renderer2.SetPropertyBlock(propertyStorage);
			}
		}
	}

	private bool IsGummyClone()
	{
		CharacterBody characterBody = body;
		if (characterBody == null)
		{
			return false;
		}
		return characterBody.inventory?.GetItemCount(DLC1Content.Items.GummyCloneIdentifier) > 0;
	}

	public void OnDeath()
	{
		for (int i = 0; i < parentedPrefabDisplays.Count; i++)
		{
			parentedPrefabDisplays[i].itemDisplay.OnDeath();
		}
		InstanceUpdate();
		UpdateOverlays();
		UpdateMaterials();
	}
}
