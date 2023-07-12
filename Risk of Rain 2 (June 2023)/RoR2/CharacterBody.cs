using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Runtime.CompilerServices;
using EntityStates;
using HG;
using RoR2.Audio;
using RoR2.Items;
using RoR2.Navigation;
using RoR2.Networking;
using RoR2.Projectile;
using RoR2.Skills;
using Unity;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.Events;
using UnityEngine.Networking;
using UnityEngine.Serialization;

namespace RoR2;

[DisallowMultipleComponent]
[RequireComponent(typeof(SkillLocator))]
[RequireComponent(typeof(TeamComponent))]
public class CharacterBody : NetworkBehaviour, ILifeBehavior, IDisplayNameProvider, IOnTakeDamageServerReceiver, IOnKilledOtherServerReceiver
{
	private static class CommonAssets
	{
		public static SkillDef lunarUtilityReplacementSkillDef;

		public static SkillDef lunarPrimaryReplacementSkillDef;

		public static SkillDef lunarSecondaryReplacementSkillDef;

		public static SkillDef lunarSpecialReplacementSkillDef;

		public static NetworkSoundEventDef nullifiedBuffAppliedSound;

		public static NetworkSoundEventDef pulverizeBuildupBuffAppliedSound;

		public static NetworkSoundEventDef[] procCritAttackSpeedSounds;

		public static void Load()
		{
			nullifiedBuffAppliedSound = LegacyResourcesAPI.Load<NetworkSoundEventDef>("NetworkSoundEventDefs/nseNullifiedBuffApplied");
			pulverizeBuildupBuffAppliedSound = LegacyResourcesAPI.Load<NetworkSoundEventDef>("NetworkSoundEventDefs/nsePulverizeBuildupBuffApplied");
			procCritAttackSpeedSounds = new NetworkSoundEventDef[3]
			{
				LegacyResourcesAPI.Load<NetworkSoundEventDef>("NetworkSoundEventDefs/nseProcCritAttackSpeed1"),
				LegacyResourcesAPI.Load<NetworkSoundEventDef>("NetworkSoundEventDefs/nseProcCritAttackSpeed2"),
				LegacyResourcesAPI.Load<NetworkSoundEventDef>("NetworkSoundEventDefs/nseProcCritAttackSpeed3")
			};
			SkillCatalog.skillsDefined.CallWhenAvailable(delegate
			{
				lunarUtilityReplacementSkillDef = SkillCatalog.GetSkillDef(SkillCatalog.FindSkillIndexByName("LunarUtilityReplacement"));
				lunarPrimaryReplacementSkillDef = SkillCatalog.GetSkillDef(SkillCatalog.FindSkillIndexByName("LunarPrimaryReplacement"));
				lunarSecondaryReplacementSkillDef = SkillCatalog.GetSkillDef(SkillCatalog.FindSkillIndexByName("LunarSecondaryReplacement"));
				lunarSpecialReplacementSkillDef = SkillCatalog.GetSkillDef(SkillCatalog.FindSkillIndexByName("LunarDetonatorSpecialReplacement"));
			});
		}
	}

	private class TimedBuff
	{
		public BuffIndex buffIndex;

		public float timer;
	}

	[Flags]
	public enum BodyFlags : uint
	{
		None = 0u,
		IgnoreFallDamage = 1u,
		Mechanical = 2u,
		Masterless = 4u,
		ImmuneToGoo = 8u,
		ImmuneToExecutes = 0x10u,
		SprintAnyDirection = 0x20u,
		ResistantToAOE = 0x40u,
		HasBackstabPassive = 0x80u,
		HasBackstabImmunity = 0x100u,
		OverheatImmune = 0x200u,
		Void = 0x400u,
		ImmuneToVoidDeath = 0x800u
	}

	public class ItemBehavior : MonoBehaviour
	{
		public CharacterBody body;

		public int stack;
	}

	public class AffixHauntedBehavior : ItemBehavior
	{
		private GameObject affixHauntedWard;

		private void FixedUpdate()
		{
			if (!NetworkServer.active)
			{
				return;
			}
			bool flag = stack > 0;
			if (Object.op_Implicit((Object)(object)affixHauntedWard) != flag)
			{
				if (flag)
				{
					affixHauntedWard = Object.Instantiate<GameObject>(LegacyResourcesAPI.Load<GameObject>("Prefabs/NetworkedObjects/AffixHauntedWard"));
					affixHauntedWard.GetComponent<TeamFilter>().teamIndex = body.teamComponent.teamIndex;
					affixHauntedWard.GetComponent<BuffWard>().Networkradius = 30f + body.radius;
					affixHauntedWard.GetComponent<NetworkedBodyAttachment>().AttachToGameObjectAndSpawn(((Component)body).gameObject);
				}
				else
				{
					Object.Destroy((Object)(object)affixHauntedWard);
					affixHauntedWard = null;
				}
			}
		}

		private void OnDisable()
		{
			if (Object.op_Implicit((Object)(object)affixHauntedWard))
			{
				Object.Destroy((Object)(object)affixHauntedWard);
			}
		}
	}

	public class QuestVolatileBatteryBehaviorServer : ItemBehavior
	{
		private NetworkedBodyAttachment attachment;

		private void Start()
		{
			attachment = Object.Instantiate<GameObject>(LegacyResourcesAPI.Load<GameObject>("Prefabs/NetworkedObjects/QuestVolatileBatteryAttachment")).GetComponent<NetworkedBodyAttachment>();
			attachment.AttachToGameObjectAndSpawn(((Component)body).gameObject);
		}

		private void OnDestroy()
		{
			if (Object.op_Implicit((Object)(object)attachment))
			{
				Object.Destroy((Object)(object)((Component)attachment).gameObject);
				attachment = null;
			}
		}
	}

	public class TimeBubbleItemBehaviorServer : ItemBehavior
	{
		private void OnDestroy()
		{
			if (Object.op_Implicit((Object)(object)body.timeBubbleWardInstance))
			{
				Object.Destroy((Object)(object)body.timeBubbleWardInstance);
			}
		}
	}

	public class ElementalRingsBehavior : ItemBehavior
	{
		private void OnDisable()
		{
			if (Object.op_Implicit((Object)(object)body))
			{
				if (body.HasBuff(RoR2Content.Buffs.ElementalRingsReady))
				{
					body.RemoveBuff(RoR2Content.Buffs.ElementalRingsReady);
				}
				if (body.HasBuff(RoR2Content.Buffs.ElementalRingsCooldown))
				{
					body.RemoveBuff(RoR2Content.Buffs.ElementalRingsCooldown);
				}
			}
		}

		private void FixedUpdate()
		{
			bool flag = body.HasBuff(RoR2Content.Buffs.ElementalRingsCooldown);
			bool flag2 = body.HasBuff(RoR2Content.Buffs.ElementalRingsReady);
			if (!flag && !flag2)
			{
				body.AddBuff(RoR2Content.Buffs.ElementalRingsReady);
			}
			if (flag2 && flag)
			{
				body.RemoveBuff(RoR2Content.Buffs.ElementalRingsReady);
			}
		}
	}

	public class AffixEchoBehavior : ItemBehavior
	{
		private DeployableMinionSpawner echoSpawner1;

		private DeployableMinionSpawner echoSpawner2;

		private CharacterSpawnCard spawnCard;

		private List<CharacterMaster> spawnedEchoes = new List<CharacterMaster>();

		private void FixedUpdate()
		{
			spawnCard.nodeGraphType = (body.isFlying ? MapNodeGroup.GraphType.Air : MapNodeGroup.GraphType.Ground);
		}

		private void Awake()
		{
			((Behaviour)this).enabled = false;
		}

		private void OnEnable()
		{
			MasterCatalog.MasterIndex masterIndex = MasterCatalog.FindAiMasterIndexForBody(body.bodyIndex);
			spawnCard = ScriptableObject.CreateInstance<CharacterSpawnCard>();
			spawnCard.prefab = MasterCatalog.GetMasterPrefab(masterIndex);
			spawnCard.inventoryToCopy = body.inventory;
			spawnCard.equipmentToGrant = new EquipmentDef[1];
			spawnCard.itemsToGrant = new ItemCountPair[1]
			{
				new ItemCountPair
				{
					itemDef = RoR2Content.Items.SummonedEcho,
					count = 1
				}
			};
			CreateSpawners();
		}

		private void OnDisable()
		{
			Object.Destroy((Object)(object)spawnCard);
			spawnCard = null;
			for (int num = spawnedEchoes.Count - 1; num >= 0; num--)
			{
				if (Object.op_Implicit((Object)(object)spawnedEchoes[num]))
				{
					spawnedEchoes[num].TrueKill();
				}
			}
			DestroySpawners();
		}

		private void CreateSpawners()
		{
			//IL_001c: Unknown result type (might be due to invalid IL or missing references)
			//IL_0026: Expected O, but got Unknown
			Xoroshiro128Plus rng = new Xoroshiro128Plus(Run.instance.seed ^ (ulong)((Object)this).GetInstanceID());
			CreateSpawner(ref echoSpawner1, DeployableSlot.RoboBallRedBuddy, spawnCard);
			CreateSpawner(ref echoSpawner2, DeployableSlot.RoboBallGreenBuddy, spawnCard);
			void CreateSpawner(ref DeployableMinionSpawner buddySpawner, DeployableSlot deployableSlot, SpawnCard spawnCard)
			{
				buddySpawner = new DeployableMinionSpawner(body.master, deployableSlot, rng)
				{
					respawnInterval = 30f,
					spawnCard = spawnCard
				};
				buddySpawner.onMinionSpawnedServer += OnMinionSpawnedServer;
			}
		}

		private void DestroySpawners()
		{
			echoSpawner1?.Dispose();
			echoSpawner1 = null;
			echoSpawner2?.Dispose();
			echoSpawner2 = null;
		}

		private void OnMinionSpawnedServer(SpawnCard.SpawnResult spawnResult)
		{
			GameObject spawnedInstance = spawnResult.spawnedInstance;
			if (!Object.op_Implicit((Object)(object)spawnedInstance))
			{
				return;
			}
			CharacterMaster spawnedMaster = spawnedInstance.GetComponent<CharacterMaster>();
			if (Object.op_Implicit((Object)(object)spawnedMaster))
			{
				spawnedEchoes.Add(spawnedMaster);
				OnDestroyCallback.AddCallback(((Component)spawnedMaster).gameObject, delegate
				{
					spawnedEchoes.Remove(spawnedMaster);
				});
			}
		}
	}

	private static class AssetReferences
	{
		public static GameObject engiShieldTempEffectPrefab;

		public static GameObject bucklerShieldTempEffectPrefab;

		public static GameObject slowDownTimeTempEffectPrefab;

		public static GameObject crippleEffectPrefab;

		public static GameObject tonicBuffEffectPrefab;

		public static GameObject weakTempEffectPrefab;

		public static GameObject energizedTempEffectPrefab;

		public static GameObject barrierTempEffectPrefab;

		public static GameObject nullifyStack1EffectPrefab;

		public static GameObject nullifyStack2EffectPrefab;

		public static GameObject nullifyStack3EffectPrefab;

		public static GameObject regenBoostEffectPrefab;

		public static GameObject elephantDefenseEffectPrefab;

		public static GameObject healingDisabledEffectPrefab;

		public static GameObject noCooldownEffectPrefab;

		public static GameObject doppelgangerEffectPrefab;

		public static GameObject deathmarkEffectPrefab;

		public static GameObject crocoRegenEffectPrefab;

		public static GameObject mercExposeEffectPrefab;

		public static GameObject lifestealOnHitEffectPrefab;

		public static GameObject teamWarCryEffectPrefab;

		public static GameObject randomDamageEffectPrefab;

		public static GameObject lunarGolemShieldEffectPrefab;

		public static GameObject warbannerEffectPrefab;

		public static GameObject teslaFieldEffectPrefab;

		public static GameObject lunarSecondaryRootEffectPrefab;

		public static GameObject lunarDetonatorEffectPrefab;

		public static GameObject fruitingEffectPrefab;

		public static GameObject mushroomVoidTempEffectPrefab;

		public static GameObject bearVoidTempEffectPrefab;

		public static GameObject outOfCombatArmorEffectPrefab;

		public static GameObject voidFogMildEffectPrefab;

		public static GameObject voidFogStrongEffectPrefab;

		public static GameObject voidJailerSlowEffectPrefab;

		public static GameObject voidRaidcrabWardWipeFogEffectPrefab;

		public static GameObject permanentDebuffEffectPrefab;

		public static void Resolve()
		{
			//IL_01c7: Unknown result type (might be due to invalid IL or missing references)
			//IL_01cc: Unknown result type (might be due to invalid IL or missing references)
			//IL_01fc: Unknown result type (might be due to invalid IL or missing references)
			//IL_0201: Unknown result type (might be due to invalid IL or missing references)
			//IL_0213: Unknown result type (might be due to invalid IL or missing references)
			//IL_0218: Unknown result type (might be due to invalid IL or missing references)
			engiShieldTempEffectPrefab = LegacyResourcesAPI.Load<GameObject>("Prefabs/TemporaryVisualEffects/EngiShield");
			bucklerShieldTempEffectPrefab = LegacyResourcesAPI.Load<GameObject>("Prefabs/TemporaryVisualEffects/BucklerDefense");
			slowDownTimeTempEffectPrefab = LegacyResourcesAPI.Load<GameObject>("Prefabs/TemporaryVisualEffects/SlowDownTime");
			crippleEffectPrefab = LegacyResourcesAPI.Load<GameObject>("Prefabs/TemporaryVisualEffects/CrippleEffect");
			tonicBuffEffectPrefab = LegacyResourcesAPI.Load<GameObject>("Prefabs/TemporaryVisualEffects/TonicBuffEffect");
			weakTempEffectPrefab = LegacyResourcesAPI.Load<GameObject>("Prefabs/TemporaryVisualEffects/WeakEffect");
			energizedTempEffectPrefab = LegacyResourcesAPI.Load<GameObject>("Prefabs/TemporaryVisualEffects/EnergizedEffect");
			barrierTempEffectPrefab = LegacyResourcesAPI.Load<GameObject>("Prefabs/TemporaryVisualEffects/BarrierEffect");
			regenBoostEffectPrefab = LegacyResourcesAPI.Load<GameObject>("Prefabs/TemporaryVisualEffects/RegenBoostEffect");
			elephantDefenseEffectPrefab = LegacyResourcesAPI.Load<GameObject>("Prefabs/TemporaryVisualEffects/ElephantDefense");
			healingDisabledEffectPrefab = LegacyResourcesAPI.Load<GameObject>("Prefabs/TemporaryVisualEffects/HealingDisabledEffect");
			noCooldownEffectPrefab = LegacyResourcesAPI.Load<GameObject>("Prefabs/TemporaryVisualEffects/NoCooldownEffect");
			doppelgangerEffectPrefab = LegacyResourcesAPI.Load<GameObject>("Prefabs/TemporaryVisualEffects/DoppelgangerEffect");
			nullifyStack1EffectPrefab = LegacyResourcesAPI.Load<GameObject>("Prefabs/TemporaryVisualEffects/NullifyStack1Effect");
			nullifyStack2EffectPrefab = LegacyResourcesAPI.Load<GameObject>("Prefabs/TemporaryVisualEffects/NullifyStack2Effect");
			nullifyStack3EffectPrefab = LegacyResourcesAPI.Load<GameObject>("Prefabs/TemporaryVisualEffects/NullifyStack3Effect");
			deathmarkEffectPrefab = LegacyResourcesAPI.Load<GameObject>("Prefabs/TemporaryVisualEffects/DeathMarkEffect");
			crocoRegenEffectPrefab = LegacyResourcesAPI.Load<GameObject>("Prefabs/TemporaryVisualEffects/CrocoRegenEffect");
			mercExposeEffectPrefab = LegacyResourcesAPI.Load<GameObject>("Prefabs/TemporaryVisualEffects/MercExposeEffect");
			lifestealOnHitEffectPrefab = LegacyResourcesAPI.Load<GameObject>("Prefabs/TemporaryVisualEffects/LifeStealOnHitAura");
			teamWarCryEffectPrefab = LegacyResourcesAPI.Load<GameObject>("Prefabs/TemporaryVisualEffects/TeamWarCryAura");
			lunarGolemShieldEffectPrefab = LegacyResourcesAPI.Load<GameObject>("Prefabs/TemporaryVisualEffects/LunarDefense");
			randomDamageEffectPrefab = LegacyResourcesAPI.Load<GameObject>("Prefabs/TemporaryVisualEffects/RandomDamageBuffEffect");
			warbannerEffectPrefab = LegacyResourcesAPI.Load<GameObject>("Prefabs/TemporaryVisualEffects/WarbannerBuffEffect");
			teslaFieldEffectPrefab = LegacyResourcesAPI.Load<GameObject>("Prefabs/TemporaryVisualEffects/TeslaFieldBuffEffect");
			lunarSecondaryRootEffectPrefab = LegacyResourcesAPI.Load<GameObject>("Prefabs/TemporaryVisualEffects/LunarSecondaryRootEffect");
			lunarDetonatorEffectPrefab = LegacyResourcesAPI.Load<GameObject>("Prefabs/TemporaryVisualEffects/LunarDetonatorEffect");
			fruitingEffectPrefab = LegacyResourcesAPI.Load<GameObject>("Prefabs/TemporaryVisualEffects/FruitingEffect");
			mushroomVoidTempEffectPrefab = LegacyResourcesAPI.Load<GameObject>("Prefabs/TemporaryVisualEffects/MushroomVoidEffect");
			bearVoidTempEffectPrefab = LegacyResourcesAPI.Load<GameObject>("Prefabs/TemporaryVisualEffects/BearVoidEffect");
			outOfCombatArmorEffectPrefab = Addressables.LoadAssetAsync<GameObject>((object)"RoR2/DLC1/OutOfCombatArmor/OutOfCombatArmorEffect.prefab").WaitForCompletion();
			voidFogMildEffectPrefab = LegacyResourcesAPI.Load<GameObject>("Prefabs/TemporaryVisualEffects/VoidFogMildEffect");
			voidFogStrongEffectPrefab = LegacyResourcesAPI.Load<GameObject>("Prefabs/TemporaryVisualEffects/VoidFogStrongEffect");
			voidRaidcrabWardWipeFogEffectPrefab = Addressables.LoadAssetAsync<GameObject>((object)"RoR2/DLC1/VoidRaidCrab/VoidRaidCrabWardWipeFogEffect.prefab").WaitForCompletion();
			voidJailerSlowEffectPrefab = Addressables.LoadAssetAsync<GameObject>((object)"RoR2/DLC1/VoidJailer/VoidJailerTetherDebuff.prefab").WaitForCompletion();
		}
	}

	private class ConstructTurretMessage : MessageBase
	{
		public GameObject builder;

		public Vector3 position;

		public Quaternion rotation;

		public MasterCatalog.NetworkMasterIndex turretMasterIndex;

		public override void Serialize(NetworkWriter writer)
		{
			//IL_000e: Unknown result type (might be due to invalid IL or missing references)
			//IL_001a: Unknown result type (might be due to invalid IL or missing references)
			writer.Write(builder);
			writer.Write(position);
			writer.Write(rotation);
			GeneratedNetworkCode._WriteNetworkMasterIndex_MasterCatalog(writer, turretMasterIndex);
		}

		public override void Deserialize(NetworkReader reader)
		{
			//IL_000e: Unknown result type (might be due to invalid IL or missing references)
			//IL_0013: Unknown result type (might be due to invalid IL or missing references)
			//IL_001a: Unknown result type (might be due to invalid IL or missing references)
			//IL_001f: Unknown result type (might be due to invalid IL or missing references)
			builder = reader.ReadGameObject();
			position = reader.ReadVector3();
			rotation = reader.ReadQuaternion();
			turretMasterIndex = GeneratedNetworkCode._ReadNetworkMasterIndex_MasterCatalog(reader);
		}
	}

	[Serializable]
	public class CharacterBodyUnityEvent : UnityEvent<CharacterBody>
	{
	}

	[HideInInspector]
	[Tooltip("This is assigned to the prefab automatically by BodyCatalog at runtime. Do not set this value manually.")]
	public BodyIndex bodyIndex = BodyIndex.None;

	[Tooltip("The language token to use as the base name of this character.")]
	public string baseNameToken;

	public string subtitleNameToken;

	private BuffIndex[] activeBuffsList;

	private int activeBuffsListCount;

	private int[] buffs;

	private int eliteBuffCount;

	private List<TimedBuff> timedBuffs = new List<TimedBuff>();

	[NonSerialized]
	public int pendingTonicAfflictionCount;

	private GameObject warCryEffectInstance;

	[EnumMask(typeof(BodyFlags))]
	public BodyFlags bodyFlags;

	private NetworkInstanceId masterObjectId;

	private GameObject _masterObject;

	private CharacterMaster _master;

	private bool linkedToMaster;

	private bool disablingHurtBoxes;

	private EquipmentIndex previousEquipmentIndex = EquipmentIndex.None;

	private Transform transform;

	private SfxLocator sfxLocator;

	private static List<CharacterBody> instancesList;

	public static readonly ReadOnlyCollection<CharacterBody> readOnlyInstancesList;

	private bool _isSprinting;

	private const float outOfCombatDelay = 5f;

	private const float outOfDangerDelay = 7f;

	private float outOfCombatStopwatch;

	private float outOfDangerStopwatch;

	private bool _outOfDanger = true;

	private Vector3 previousPosition;

	private const float notMovingWait = 1f;

	private float notMovingStopwatch;

	public bool rootMotionInMainState;

	public float mainRootSpeed;

	public float baseMaxHealth;

	public float baseRegen;

	public float baseMaxShield;

	public float baseMoveSpeed;

	public float baseAcceleration;

	public float baseJumpPower;

	public float baseDamage;

	public float baseAttackSpeed;

	public float baseCrit;

	public float baseArmor;

	public float baseVisionDistance = float.PositiveInfinity;

	public int baseJumpCount = 1;

	public float sprintingSpeedMultiplier = 1.45f;

	public bool autoCalculateLevelStats;

	public float levelMaxHealth;

	public float levelRegen;

	public float levelMaxShield;

	public float levelMoveSpeed;

	public float levelJumpPower;

	public float levelDamage;

	public float levelAttackSpeed;

	public float levelCrit;

	public float levelArmor;

	private bool statsDirty;

	private float aimTimer;

	private const uint masterDirtyBit = 1u;

	private const uint buffsDirtyBit = 2u;

	private const uint outOfCombatBit = 4u;

	private const uint outOfDangerBit = 8u;

	private const uint sprintingBit = 16u;

	private const uint allDirtyBits = 31u;

	private HelfireController helfireController;

	private float helfireLifetime;

	private DamageTrail fireTrail;

	public bool wasLucky;

	private const float poisonballAngle = 25f;

	private const float poisonballDamageCoefficient = 1f;

	private const float poisonballRefreshTime = 6f;

	private float poisonballTimer;

	private const float lunarMissileDamageCoefficient = 0.3f;

	private const float lunarMissileRefreshTime = 10f;

	private const float lunarMissileDelayBetweenShots = 0.1f;

	private float lunarMissileRechargeTimer = 10f;

	private float lunarMissileTimerBetweenShots;

	private int remainingMissilesToFire;

	private GameObject lunarMissilePrefab;

	private GameObject timeBubbleWardInstance;

	private TemporaryVisualEffect engiShieldTempEffectInstance;

	private TemporaryVisualEffect bucklerShieldTempEffectInstance;

	private TemporaryVisualEffect slowDownTimeTempEffectInstance;

	private TemporaryVisualEffect crippleEffectInstance;

	private TemporaryVisualEffect tonicBuffEffectInstance;

	private TemporaryVisualEffect weakTempEffectInstance;

	private TemporaryVisualEffect energizedTempEffectInstance;

	private TemporaryVisualEffect barrierTempEffectInstance;

	private TemporaryVisualEffect nullifyStack1EffectInstance;

	private TemporaryVisualEffect nullifyStack2EffectInstance;

	private TemporaryVisualEffect nullifyStack3EffectInstance;

	private TemporaryVisualEffect regenBoostEffectInstance;

	private TemporaryVisualEffect elephantDefenseEffectInstance;

	private TemporaryVisualEffect healingDisabledEffectInstance;

	private TemporaryVisualEffect noCooldownEffectInstance;

	private TemporaryVisualEffect doppelgangerEffectInstance;

	private TemporaryVisualEffect deathmarkEffectInstance;

	private TemporaryVisualEffect crocoRegenEffectInstance;

	private TemporaryVisualEffect mercExposeEffectInstance;

	private TemporaryVisualEffect lifestealOnHitEffectInstance;

	private TemporaryVisualEffect teamWarCryEffectInstance;

	private TemporaryVisualEffect randomDamageEffectInstance;

	private TemporaryVisualEffect lunarGolemShieldEffectInstance;

	private TemporaryVisualEffect warbannerEffectInstance;

	private TemporaryVisualEffect teslaFieldEffectInstance;

	private TemporaryVisualEffect lunarSecondaryRootEffectInstance;

	private TemporaryVisualEffect lunarDetonatorEffectInstance;

	private TemporaryVisualEffect fruitingEffectInstance;

	private TemporaryVisualEffect mushroomVoidTempEffectInstance;

	private TemporaryVisualEffect bearVoidTempEffectInstance;

	private TemporaryVisualEffect outOfCombatArmorEffectInstance;

	private TemporaryVisualEffect voidFogMildEffectInstance;

	private TemporaryVisualEffect voidFogStrongEffectInstance;

	private TemporaryVisualEffect voidJailerSlowEffectInstance;

	private TemporaryVisualEffect voidRaidcrabWardWipeFogEffectInstance;

	[Tooltip("How long it takes for spread bloom to reset from full.")]
	public float spreadBloomDecayTime = 0.45f;

	[Tooltip("The spread bloom interpretation curve.")]
	public AnimationCurve spreadBloomCurve;

	private float spreadBloomInternal;

	[Tooltip("The crosshair prefab used for this body.")]
	[SerializeField]
	[FormerlySerializedAs("crosshairPrefab")]
	private GameObject _defaultCrosshairPrefab;

	[HideInInspector]
	public bool hideCrosshair;

	private const float multiKillMaxInterval = 1f;

	private float multiKillTimer;

	private const int multiKillThresholdForWarcry = 4;

	[Tooltip("The child transform to be used as the aiming origin.")]
	public Transform aimOriginTransform;

	[Tooltip("The hull size to use when pathfinding for this object.")]
	public HullClassification hullClassification;

	[Tooltip("The icon displayed for ally healthbars")]
	public Texture portraitIcon;

	[Tooltip("The main color of the body. Currently only used in the logbook.")]
	public Color bodyColor = Color.clear;

	[FormerlySerializedAs("isBoss")]
	[Tooltip("Whether or not this is a boss for dropping items on death.")]
	public bool isChampion;

	public VehicleSeat currentVehicle;

	[Tooltip("The pod prefab to use for handling this character's first-time spawn animation.")]
	public GameObject preferredPodPrefab;

	[Tooltip("The preferred state to use for handling the character's first-time spawn animation. Only used with no preferred pod prefab.")]
	public SerializableEntityStateType preferredInitialStateType = new SerializableEntityStateType(typeof(Uninitialized));

	public uint skinIndex;

	public string customKillTotalStatName;

	public Transform overrideCoreTransform;

	private static int kCmdCmdAddTimedBuff;

	private static int kCmdCmdUpdateSprint;

	private static int kCmdCmdOnSkillActivated;

	private static int kRpcRpcBark;

	private static int kCmdCmdRequestVehicleEjection;

	private static int kRpcRpcUsePreferredInitialStateType;

	public CharacterMaster master
	{
		get
		{
			if (!Object.op_Implicit((Object)(object)masterObject))
			{
				return null;
			}
			return _master;
		}
	}

	public Inventory inventory { get; private set; }

	public bool isPlayerControlled { get; private set; }

	public float executeEliteHealthFraction { get; private set; }

	public GameObject masterObject
	{
		get
		{
			//IL_0019: Unknown result type (might be due to invalid IL or missing references)
			//IL_0033: Unknown result type (might be due to invalid IL or missing references)
			if (!Object.op_Implicit((Object)(object)_masterObject))
			{
				if (NetworkServer.active)
				{
					_masterObject = NetworkServer.FindLocalObject(masterObjectId);
				}
				else if (NetworkClient.active)
				{
					_masterObject = ClientScene.FindLocalObject(masterObjectId);
				}
				_master = (Object.op_Implicit((Object)(object)_masterObject) ? _masterObject.GetComponent<CharacterMaster>() : null);
				if (Object.op_Implicit((Object)(object)_master))
				{
					isPlayerControlled = Object.op_Implicit((Object)(object)_masterObject.GetComponent<PlayerCharacterMasterController>());
					if (Object.op_Implicit((Object)(object)inventory))
					{
						inventory.onInventoryChanged -= OnInventoryChanged;
					}
					inventory = _master.inventory;
					if (Object.op_Implicit((Object)(object)inventory))
					{
						inventory.onInventoryChanged += OnInventoryChanged;
						OnInventoryChanged();
					}
					statsDirty = true;
				}
			}
			return _masterObject;
		}
		set
		{
			//IL_0007: Unknown result type (might be due to invalid IL or missing references)
			//IL_000c: Unknown result type (might be due to invalid IL or missing references)
			masterObjectId = value.GetComponent<NetworkIdentity>().netId;
			statsDirty = true;
		}
	}

	public Rigidbody rigidbody { get; private set; }

	public NetworkIdentity networkIdentity { get; private set; }

	public CharacterMotor characterMotor { get; private set; }

	public CharacterDirection characterDirection { get; private set; }

	public TeamComponent teamComponent { get; private set; }

	public HealthComponent healthComponent { get; private set; }

	public EquipmentSlot equipmentSlot { get; private set; }

	public InputBankTest inputBank { get; private set; }

	public SkillLocator skillLocator { get; private set; }

	public ModelLocator modelLocator { get; private set; }

	public HurtBoxGroup hurtBoxGroup { get; private set; }

	public HurtBox mainHurtBox { get; private set; }

	public Transform coreTransform { get; private set; }

	public bool hasEffectiveAuthority { get; private set; }

	public bool isSprinting
	{
		get
		{
			return _isSprinting;
		}
		set
		{
			if (_isSprinting != value)
			{
				_isSprinting = value;
				RecalculateStats();
				if (value)
				{
					OnSprintStart();
				}
				else
				{
					OnSprintStop();
				}
				if (NetworkServer.active)
				{
					((NetworkBehaviour)this).SetDirtyBit(16u);
				}
				else if (hasEffectiveAuthority)
				{
					CallCmdUpdateSprint(value);
				}
			}
		}
	}

	public bool outOfCombat { get; private set; } = true;


	public bool outOfDanger
	{
		get
		{
			return _outOfDanger;
		}
		private set
		{
			if (_outOfDanger != value)
			{
				_outOfDanger = value;
				OnOutOfDangerChanged();
			}
		}
	}

	public float experience { get; private set; }

	public float level { get; private set; }

	public float maxHealth { get; private set; }

	public float maxBarrier { get; private set; }

	public float barrierDecayRate { get; private set; }

	public float regen { get; private set; }

	public float maxShield { get; private set; }

	public float moveSpeed { get; private set; }

	public float acceleration { get; private set; }

	public float jumpPower { get; private set; }

	public int maxJumpCount { get; private set; }

	public float maxJumpHeight { get; private set; }

	public float damage { get; private set; }

	public float attackSpeed { get; private set; }

	public float crit { get; private set; }

	public float critMultiplier { get; private set; }

	public float bleedChance { get; private set; }

	public float armor { get; private set; }

	public float visionDistance { get; private set; }

	public float critHeal { get; private set; }

	public float cursePenalty { get; private set; }

	public bool hasOneShotProtection { get; private set; }

	public bool isGlass { get; private set; }

	public float oneShotProtectionFraction { get; private set; }

	public bool canPerformBackstab { get; private set; }

	public bool canReceiveBackstab { get; private set; }

	public bool shouldAim
	{
		get
		{
			if (aimTimer > 0f)
			{
				return !isSprinting;
			}
			return false;
		}
	}

	public int killCountServer { get; private set; }

	public float bestFitRadius => Mathf.Max(radius, Object.op_Implicit((Object)(object)characterMotor) ? characterMotor.capsuleHeight : 1f);

	public bool hasCloakBuff
	{
		get
		{
			if (!HasBuff(RoR2Content.Buffs.Cloak))
			{
				return HasBuff(RoR2Content.Buffs.AffixHauntedRecipient);
			}
			return true;
		}
	}

	public float spreadBloomAngle => spreadBloomCurve.Evaluate(spreadBloomInternal);

	public GameObject defaultCrosshairPrefab => _defaultCrosshairPrefab;

	public int multiKillCount { get; private set; }

	public Vector3 corePosition
	{
		get
		{
			//IL_0013: Unknown result type (might be due to invalid IL or missing references)
			//IL_001f: Unknown result type (might be due to invalid IL or missing references)
			if (!Object.op_Implicit((Object)(object)coreTransform))
			{
				return transform.position;
			}
			return coreTransform.position;
		}
	}

	public Vector3 footPosition
	{
		get
		{
			//IL_0006: Unknown result type (might be due to invalid IL or missing references)
			//IL_000b: Unknown result type (might be due to invalid IL or missing references)
			//IL_0035: Unknown result type (might be due to invalid IL or missing references)
			Vector3 position = transform.position;
			if (Object.op_Implicit((Object)(object)characterMotor))
			{
				position.y -= characterMotor.capsuleHeight * 0.5f;
			}
			return position;
		}
	}

	public float radius { get; private set; }

	public Vector3 aimOrigin
	{
		get
		{
			//IL_000e: Unknown result type (might be due to invalid IL or missing references)
			//IL_001a: Unknown result type (might be due to invalid IL or missing references)
			if (!Object.op_Implicit((Object)(object)aimOriginTransform))
			{
				return corePosition;
			}
			return aimOriginTransform.position;
		}
	}

	public bool isElite { get; private set; }

	public bool isBoss
	{
		get
		{
			if (Object.op_Implicit((Object)(object)master))
			{
				return master.isBoss;
			}
			return false;
		}
	}

	public bool isFlying
	{
		get
		{
			if (Object.op_Implicit((Object)(object)characterMotor))
			{
				return characterMotor.isFlying;
			}
			return true;
		}
	}

	public Run.FixedTimeStamp localStartTime { get; private set; } = Run.FixedTimeStamp.positiveInfinity;


	public bool isEquipmentActivationAllowed
	{
		get
		{
			if (Object.op_Implicit((Object)(object)currentVehicle))
			{
				return currentVehicle.isEquipmentActivationAllowed;
			}
			return true;
		}
	}

	public event Action onInventoryChanged;

	public event Action<GenericSkill> onSkillActivatedServer;

	public event Action<GenericSkill> onSkillActivatedAuthority;

	public static event Action<CharacterBody> onBodyAwakeGlobal;

	public static event Action<CharacterBody> onBodyDestroyGlobal;

	public static event Action<CharacterBody> onBodyStartGlobal;

	public static event Action<CharacterBody> onBodyInventoryChangedGlobal;

	[RuntimeInitializeOnLoadMethod]
	private static void LoadCommonAssets()
	{
		CommonAssets.Load();
	}

	public string GetDisplayName()
	{
		return Language.GetString(baseNameToken);
	}

	public string GetSubtitle()
	{
		return Language.GetString(subtitleNameToken);
	}

	public string GetUserName()
	{
		string text = "";
		if (Object.op_Implicit((Object)(object)master))
		{
			PlayerCharacterMasterController component = ((Component)master).GetComponent<PlayerCharacterMasterController>();
			if (Object.op_Implicit((Object)(object)component))
			{
				text = component.GetDisplayName();
			}
		}
		if (string.IsNullOrEmpty(text))
		{
			text = GetDisplayName();
		}
		return text;
	}

	public string GetColoredUserName()
	{
		//IL_0057: Unknown result type (might be due to invalid IL or missing references)
		//IL_005c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0070: Unknown result type (might be due to invalid IL or missing references)
		Color32 userColor = default(Color32);
		((Color32)(ref userColor))._002Ector((byte)127, (byte)127, (byte)127, byte.MaxValue);
		string text = null;
		if (Object.op_Implicit((Object)(object)master))
		{
			PlayerCharacterMasterController component = ((Component)master).GetComponent<PlayerCharacterMasterController>();
			if (Object.op_Implicit((Object)(object)component))
			{
				GameObject networkUserObject = component.networkUserObject;
				if (Object.op_Implicit((Object)(object)networkUserObject))
				{
					NetworkUser component2 = networkUserObject.GetComponent<NetworkUser>();
					if (Object.op_Implicit((Object)(object)component2))
					{
						userColor = component2.userColor;
						text = component2.userName;
					}
				}
			}
		}
		if (text == null)
		{
			text = GetDisplayName();
		}
		return Util.GenerateColoredString(text, userColor);
	}

	[Server]
	private void WriteBuffs(NetworkWriter writer)
	{
		if (!NetworkServer.active)
		{
			Debug.LogWarning((object)"[Server] function 'System.Void RoR2.CharacterBody::WriteBuffs(UnityEngine.Networking.NetworkWriter)' called on client");
			return;
		}
		writer.Write((byte)activeBuffsListCount);
		for (int i = 0; i < activeBuffsListCount; i++)
		{
			BuffIndex buffIndex = activeBuffsList[i];
			BuffDef buffDef = BuffCatalog.GetBuffDef(buffIndex);
			writer.WriteBuffIndex(buffIndex);
			if (buffDef.canStack)
			{
				writer.WritePackedUInt32((uint)buffs[(int)buffIndex]);
			}
		}
	}

	[Client]
	private void ReadBuffs(NetworkReader reader)
	{
		//IL_00c0: Unknown result type (might be due to invalid IL or missing references)
		if (!NetworkClient.active)
		{
			Debug.LogWarning((object)"[Client] function 'System.Void RoR2.CharacterBody::ReadBuffs(UnityEngine.Networking.NetworkReader)' called on server");
			return;
		}
		if (activeBuffsList == null)
		{
			Debug.LogError((object)"Trying to ReadBuffs, but our activeBuffsList is null");
			return;
		}
		int activeBuffsIndexToCheck = 0;
		int num = reader.ReadByte();
		BuffIndex buffIndex = BuffIndex.None;
		for (int i = 0; i < num; i++)
		{
			BuffIndex buffIndex2 = reader.ReadBuffIndex();
			BuffDef buffDef = BuffCatalog.GetBuffDef(buffIndex2);
			if ((Object)(object)buffDef != (Object)null)
			{
				int num2 = 1;
				if (buffDef.canStack)
				{
					num2 = (int)reader.ReadPackedUInt32();
				}
				if (num2 > 0 && !NetworkServer.active)
				{
					ZeroBuffIndexRange(buffIndex + 1, buffIndex2);
					SetBuffCount(buffIndex2, num2);
				}
				buffIndex = buffIndex2;
			}
			else
			{
				Debug.LogErrorFormat("No BuffDef for index {0}. body={1}, netID={2}", new object[3]
				{
					buffIndex2,
					((Component)this).gameObject,
					((NetworkBehaviour)this).netId
				});
			}
		}
		if (!NetworkServer.active)
		{
			ZeroBuffIndexRange(buffIndex + 1, (BuffIndex)BuffCatalog.buffCount);
		}
		void ZeroBuffIndexRange(BuffIndex start, BuffIndex end)
		{
			while (activeBuffsIndexToCheck < activeBuffsListCount)
			{
				BuffIndex buffIndex3 = activeBuffsList[activeBuffsIndexToCheck];
				if (end <= buffIndex3)
				{
					break;
				}
				int num3;
				if (start <= buffIndex3)
				{
					SetBuffCount(buffIndex3, 0);
					num3 = activeBuffsIndexToCheck - 1;
					activeBuffsIndexToCheck = num3;
				}
				num3 = activeBuffsIndexToCheck + 1;
				activeBuffsIndexToCheck = num3;
			}
		}
	}

	[Server]
	public void AddBuff(BuffIndex buffType)
	{
		if (!NetworkServer.active)
		{
			Debug.LogWarning((object)"[Server] function 'System.Void RoR2.CharacterBody::AddBuff(RoR2.BuffIndex)' called on client");
		}
		else if (buffType != BuffIndex.None)
		{
			SetBuffCount(buffType, buffs[(int)buffType] + 1);
		}
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	[Server]
	public void AddBuff(BuffDef buffDef)
	{
		if (!NetworkServer.active)
		{
			Debug.LogWarning((object)"[Server] function 'System.Void RoR2.CharacterBody::AddBuff(RoR2.BuffDef)' called on client");
		}
		else
		{
			AddBuff(buffDef?.buffIndex ?? BuffIndex.None);
		}
	}

	[Server]
	public void RemoveBuff(BuffIndex buffType)
	{
		//IL_009d: Unknown result type (might be due to invalid IL or missing references)
		if (!NetworkServer.active)
		{
			Debug.LogWarning((object)"[Server] function 'System.Void RoR2.CharacterBody::RemoveBuff(RoR2.BuffIndex)' called on client");
		}
		else
		{
			if (buffType == BuffIndex.None)
			{
				return;
			}
			SetBuffCount(buffType, buffs[(int)buffType] - 1);
			if (buffType == RoR2Content.Buffs.MedkitHeal.buffIndex)
			{
				if (GetBuffCount(RoR2Content.Buffs.MedkitHeal.buffIndex) == 0)
				{
					int itemCount = inventory.GetItemCount(RoR2Content.Items.Medkit);
					float num = 20f;
					float num2 = maxHealth * 0.05f * (float)itemCount;
					healthComponent.Heal(num + num2, default(ProcChainMask));
					EffectData effectData = new EffectData
					{
						origin = transform.position
					};
					effectData.SetNetworkedObjectReference(((Component)this).gameObject);
					EffectManager.SpawnEffect(LegacyResourcesAPI.Load<GameObject>("Prefabs/Effects/MedkitHealEffect"), effectData, transmit: true);
				}
			}
			else if (buffType == RoR2Content.Buffs.TonicBuff.buffIndex && Object.op_Implicit((Object)(object)inventory) && GetBuffCount(RoR2Content.Buffs.TonicBuff) == 0 && pendingTonicAfflictionCount > 0)
			{
				inventory.GiveItem(RoR2Content.Items.TonicAffliction, pendingTonicAfflictionCount);
				PickupIndex pickupIndex = PickupCatalog.FindPickupIndex(RoR2Content.Items.TonicAffliction.itemIndex);
				GenericPickupController.SendPickupMessage(master, pickupIndex);
				pendingTonicAfflictionCount = 0;
			}
		}
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	[Server]
	public void RemoveBuff(BuffDef buffDef)
	{
		if (!NetworkServer.active)
		{
			Debug.LogWarning((object)"[Server] function 'System.Void RoR2.CharacterBody::RemoveBuff(RoR2.BuffDef)' called on client");
		}
		else
		{
			RemoveBuff(buffDef?.buffIndex ?? BuffIndex.None);
		}
	}

	private void SetBuffCount(BuffIndex buffType, int newCount)
	{
		ref int reference = ref buffs[(int)buffType];
		if (newCount == reference)
		{
			return;
		}
		int num = reference;
		reference = newCount;
		BuffDef buffDef = BuffCatalog.GetBuffDef(buffType);
		bool flag = true;
		if (!buffDef.canStack)
		{
			flag = num == 0 != (newCount == 0);
		}
		if (flag)
		{
			if (newCount == 0)
			{
				ArrayUtils.ArrayRemoveAt<BuffIndex>(activeBuffsList, ref activeBuffsListCount, Array.IndexOf(activeBuffsList, buffType), 1);
				OnBuffFinalStackLost(buffDef);
			}
			else if (num == 0)
			{
				int i;
				for (i = 0; i < activeBuffsListCount && buffType >= activeBuffsList[i]; i++)
				{
				}
				ArrayUtils.ArrayInsert<BuffIndex>(ref activeBuffsList, ref activeBuffsListCount, i, ref buffType);
				OnBuffFirstStackGained(buffDef);
			}
			if (NetworkServer.active)
			{
				((NetworkBehaviour)this).SetDirtyBit(2u);
			}
		}
		statsDirty = true;
		if (NetworkClient.active)
		{
			OnClientBuffsChanged();
		}
	}

	private void OnBuffFirstStackGained(BuffDef buffDef)
	{
		if (buffDef.isElite)
		{
			eliteBuffCount++;
		}
		if ((Object)(object)buffDef == (Object)(object)RoR2Content.Buffs.Intangible)
		{
			UpdateHurtBoxesEnabled();
		}
		else if ((Object)(object)buffDef == (Object)(object)RoR2Content.Buffs.WarCryBuff)
		{
			if (HasBuff(RoR2Content.Buffs.TeamWarCry))
			{
				ClearTimedBuffs(RoR2Content.Buffs.TeamWarCry);
			}
		}
		else if ((Object)(object)buffDef == (Object)(object)RoR2Content.Buffs.TeamWarCry)
		{
			if (HasBuff(RoR2Content.Buffs.WarCryBuff))
			{
				ClearTimedBuffs(RoR2Content.Buffs.WarCryBuff);
			}
		}
		else if ((Object)(object)buffDef == (Object)(object)RoR2Content.Buffs.AffixEcho && NetworkServer.active)
		{
			AddItemBehavior<AffixEchoBehavior>(1);
		}
	}

	private void OnBuffFinalStackLost(BuffDef buffDef)
	{
		if (buffDef.isElite)
		{
			eliteBuffCount--;
		}
		if (buffDef.buffIndex == RoR2Content.Buffs.Intangible.buffIndex)
		{
			UpdateHurtBoxesEnabled();
		}
		else if ((Object)(object)buffDef == (Object)(object)RoR2Content.Buffs.AffixEcho && NetworkServer.active)
		{
			AddItemBehavior<AffixEchoBehavior>(0);
		}
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public int GetBuffCount(BuffIndex buffType)
	{
		return ArrayUtils.GetSafe<int>(buffs, (int)buffType);
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public int GetBuffCount(BuffDef buffDef)
	{
		return GetBuffCount(buffDef?.buffIndex ?? BuffIndex.None);
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public bool HasBuff(BuffIndex buffType)
	{
		return GetBuffCount(buffType) > 0;
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public bool HasBuff(BuffDef buffDef)
	{
		return HasBuff(buffDef?.buffIndex ?? BuffIndex.None);
	}

	public void AddTimedBuffAuthority(BuffIndex buffType, float duration)
	{
		if (NetworkServer.active)
		{
			AddTimedBuff(buffType, duration);
		}
		else
		{
			CallCmdAddTimedBuff(buffType, duration);
		}
	}

	[Command]
	public void CmdAddTimedBuff(BuffIndex buffType, float duration)
	{
		AddTimedBuff(buffType, duration);
	}

	[Server]
	public void AddTimedBuff(BuffDef buffDef, float duration, int maxStacks)
	{
		if (!NetworkServer.active)
		{
			Debug.LogWarning((object)"[Server] function 'System.Void RoR2.CharacterBody::AddTimedBuff(RoR2.BuffDef,System.Single,System.Int32)' called on client");
		}
		else
		{
			if (ImmuneToDebuffBehavior.OverrideDebuff(buffDef, this))
			{
				return;
			}
			if (GetBuffCount(buffDef) < maxStacks)
			{
				AddTimedBuff(buffDef, duration);
				return;
			}
			int num = -1;
			float num2 = duration;
			for (int i = 0; i < timedBuffs.Count; i++)
			{
				if (timedBuffs[i].buffIndex == buffDef.buffIndex && timedBuffs[i].timer < num2)
				{
					num = i;
					num2 = timedBuffs[i].timer;
				}
			}
			if (num >= 0)
			{
				timedBuffs[num].timer = duration;
			}
		}
	}

	[Server]
	public void AddTimedBuff(BuffDef buffDef, float duration)
	{
		BuffIndex buffType;
		if (!NetworkServer.active)
		{
			Debug.LogWarning((object)"[Server] function 'System.Void RoR2.CharacterBody::AddTimedBuff(RoR2.BuffDef,System.Single)' called on client");
		}
		else
		{
			if (buffDef == null || ImmuneToDebuffBehavior.OverrideDebuff(buffDef, this))
			{
				return;
			}
			buffType = buffDef.buffIndex;
			if (buffType == BuffIndex.None)
			{
				return;
			}
			if ((Object)(object)buffDef == (Object)(object)RoR2Content.Buffs.AttackSpeedOnCrit)
			{
				int num = (Object.op_Implicit((Object)(object)inventory) ? inventory.GetItemCount(RoR2Content.Items.AttackSpeedOnCrit) : 0);
				int num2 = 1 + num * 2;
				int num3 = 0;
				int num4 = -1;
				float num5 = 999f;
				for (int i = 0; i < timedBuffs.Count; i++)
				{
					if (timedBuffs[i].buffIndex == buffType)
					{
						num3++;
						if (timedBuffs[i].timer < num5)
						{
							num4 = i;
							num5 = timedBuffs[i].timer;
						}
					}
				}
				if (num3 < num2)
				{
					timedBuffs.Add(new TimedBuff
					{
						buffIndex = buffType,
						timer = duration
					});
					AddBuff(buffType);
					ChildLocator component = ((Component)modelLocator.modelTransform).GetComponent<ChildLocator>();
					if (Object.op_Implicit((Object)(object)component))
					{
						Transform obj = component.FindChild("HandL");
						Transform val = component.FindChild("HandR");
						GameObject effectPrefab = LegacyResourcesAPI.Load<GameObject>("Prefabs/Effects/WolfProcEffect");
						if (Object.op_Implicit((Object)(object)obj))
						{
							EffectManager.SimpleMuzzleFlash(effectPrefab, ((Component)this).gameObject, "HandL", transmit: true);
						}
						if (Object.op_Implicit((Object)(object)val))
						{
							EffectManager.SimpleMuzzleFlash(effectPrefab, ((Component)this).gameObject, "HandR", transmit: true);
						}
					}
				}
				else if (num4 > -1)
				{
					timedBuffs[num4].timer = duration;
				}
				EntitySoundManager.EmitSoundServer(CommonAssets.procCritAttackSpeedSounds[Mathf.Min(CommonAssets.procCritAttackSpeedSounds.Length - 1, num3)].index, networkIdentity);
			}
			else if ((Object)(object)buffDef == (Object)(object)RoR2Content.Buffs.BeetleJuice)
			{
				if (RefreshStacks() < 10)
				{
					timedBuffs.Add(new TimedBuff
					{
						buffIndex = buffType,
						timer = duration
					});
					AddBuff(buffType);
				}
			}
			else if ((Object)(object)buffDef == (Object)(object)RoR2Content.Buffs.NullifyStack)
			{
				if (HasBuff(RoR2Content.Buffs.Nullified))
				{
					return;
				}
				int num6 = 0;
				for (int j = 0; j < timedBuffs.Count; j++)
				{
					if (timedBuffs[j].buffIndex == buffType)
					{
						num6++;
						if (timedBuffs[j].timer < duration)
						{
							timedBuffs[j].timer = duration;
						}
					}
				}
				if (num6 < 2)
				{
					timedBuffs.Add(new TimedBuff
					{
						buffIndex = buffType,
						timer = duration
					});
					AddBuff(buffType);
				}
				else
				{
					ClearTimedBuffs(RoR2Content.Buffs.NullifyStack.buffIndex);
					AddTimedBuff(RoR2Content.Buffs.Nullified.buffIndex, 3f);
				}
			}
			else if ((Object)(object)buffDef == (Object)(object)RoR2Content.Buffs.AffixHauntedRecipient)
			{
				if (!HasBuff(RoR2Content.Buffs.AffixHaunted))
				{
					DefaultBehavior();
				}
			}
			else if ((Object)(object)buffDef == (Object)(object)RoR2Content.Buffs.LunarDetonationCharge)
			{
				RefreshStacks();
				DefaultBehavior();
			}
			else if ((Object)(object)buffDef == (Object)(object)RoR2Content.Buffs.Overheat)
			{
				RefreshStacks();
				DefaultBehavior();
			}
			else
			{
				DefaultBehavior();
			}
		}
		void DefaultBehavior()
		{
			bool flag = false;
			if (!buffDef.canStack)
			{
				for (int k = 0; k < timedBuffs.Count; k++)
				{
					if (timedBuffs[k].buffIndex == buffType)
					{
						flag = true;
						timedBuffs[k].timer = Mathf.Max(timedBuffs[k].timer, duration);
						break;
					}
				}
			}
			if (!flag)
			{
				timedBuffs.Add(new TimedBuff
				{
					buffIndex = buffType,
					timer = duration
				});
				AddBuff(buffType);
			}
			if (Object.op_Implicit((Object)(object)buffDef.startSfx))
			{
				EntitySoundManager.EmitSoundServer(buffDef.startSfx.index, networkIdentity);
			}
		}
		int RefreshStacks()
		{
			int num7 = 0;
			for (int l = 0; l < timedBuffs.Count; l++)
			{
				TimedBuff timedBuff = timedBuffs[l];
				if (timedBuff.buffIndex == buffType)
				{
					num7++;
					if (timedBuff.timer < duration)
					{
						timedBuff.timer = duration;
					}
				}
			}
			return num7;
		}
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	[Server]
	public void AddTimedBuff(BuffIndex buffIndex, float duration)
	{
		if (!NetworkServer.active)
		{
			Debug.LogWarning((object)"[Server] function 'System.Void RoR2.CharacterBody::AddTimedBuff(RoR2.BuffIndex,System.Single)' called on client");
		}
		else
		{
			AddTimedBuff(BuffCatalog.GetBuffDef(buffIndex), duration);
		}
	}

	[Server]
	public void ClearTimedBuffs(BuffIndex buffType)
	{
		if (!NetworkServer.active)
		{
			Debug.LogWarning((object)"[Server] function 'System.Void RoR2.CharacterBody::ClearTimedBuffs(RoR2.BuffIndex)' called on client");
			return;
		}
		for (int num = timedBuffs.Count - 1; num >= 0; num--)
		{
			TimedBuff timedBuff = timedBuffs[num];
			if (timedBuff.buffIndex == buffType)
			{
				timedBuffs.RemoveAt(num);
				RemoveBuff(timedBuff.buffIndex);
			}
		}
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	[Server]
	public void ClearTimedBuffs(BuffDef buffDef)
	{
		if (!NetworkServer.active)
		{
			Debug.LogWarning((object)"[Server] function 'System.Void RoR2.CharacterBody::ClearTimedBuffs(RoR2.BuffDef)' called on client");
		}
		else
		{
			ClearTimedBuffs(buffDef?.buffIndex ?? BuffIndex.None);
		}
	}

	[Server]
	public void RemoveOldestTimedBuff(BuffIndex buffType)
	{
		if (!NetworkServer.active)
		{
			Debug.LogWarning((object)"[Server] function 'System.Void RoR2.CharacterBody::RemoveOldestTimedBuff(RoR2.BuffIndex)' called on client");
			return;
		}
		float num = float.NegativeInfinity;
		int num2 = -1;
		for (int num3 = timedBuffs.Count - 1; num3 >= 0; num3--)
		{
			TimedBuff timedBuff = timedBuffs[num3];
			if (timedBuff.buffIndex == buffType && num < timedBuff.timer)
			{
				num = timedBuff.timer;
				num2 = num3;
			}
		}
		if (num2 > 0)
		{
			timedBuffs.RemoveAt(num2);
			RemoveBuff(buffType);
		}
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	[Server]
	public void RemoveOldestTimedBuff(BuffDef buffDef)
	{
		if (!NetworkServer.active)
		{
			Debug.LogWarning((object)"[Server] function 'System.Void RoR2.CharacterBody::RemoveOldestTimedBuff(RoR2.BuffDef)' called on client");
		}
		else
		{
			RemoveOldestTimedBuff(buffDef?.buffIndex ?? BuffIndex.None);
		}
	}

	[Server]
	private void UpdateBuffs(float deltaTime)
	{
		if (!NetworkServer.active)
		{
			Debug.LogWarning((object)"[Server] function 'System.Void RoR2.CharacterBody::UpdateBuffs(System.Single)' called on client");
			return;
		}
		for (int num = timedBuffs.Count - 1; num >= 0; num--)
		{
			TimedBuff timedBuff = timedBuffs[num];
			timedBuff.timer -= deltaTime;
			if (timedBuff.timer <= 0f)
			{
				timedBuffs.RemoveAt(num);
				RemoveBuff(timedBuff.buffIndex);
			}
		}
	}

	[Client]
	private void OnClientBuffsChanged()
	{
		//IL_007f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0084: Unknown result type (might be due to invalid IL or missing references)
		if (!NetworkClient.active)
		{
			Debug.LogWarning((object)"[Client] function 'System.Void RoR2.CharacterBody::OnClientBuffsChanged()' called on server");
			return;
		}
		bool num = HasBuff(RoR2Content.Buffs.WarCryBuff);
		if (!num && Object.op_Implicit((Object)(object)warCryEffectInstance))
		{
			Object.Destroy((Object)(object)warCryEffectInstance);
		}
		if (num && !Object.op_Implicit((Object)(object)warCryEffectInstance))
		{
			Transform val = (Object.op_Implicit((Object)(object)mainHurtBox) ? ((Component)mainHurtBox).transform : transform);
			if (Object.op_Implicit((Object)(object)val))
			{
				warCryEffectInstance = Object.Instantiate<GameObject>(LegacyResourcesAPI.Load<GameObject>("Prefabs/Effects/WarCryEffect"), val.position, Quaternion.identity, val);
			}
		}
	}

	private void UpdateHurtBoxesEnabled()
	{
		bool flag = (Object.op_Implicit((Object)(object)inventory) && inventory.GetItemCount(RoR2Content.Items.Ghost) > 0) || HasBuff(RoR2Content.Buffs.Intangible);
		if (flag == disablingHurtBoxes)
		{
			return;
		}
		if (Object.op_Implicit((Object)(object)hurtBoxGroup))
		{
			if (flag)
			{
				HurtBoxGroup obj = hurtBoxGroup;
				int hurtBoxesDeactivatorCounter = obj.hurtBoxesDeactivatorCounter + 1;
				obj.hurtBoxesDeactivatorCounter = hurtBoxesDeactivatorCounter;
			}
			else
			{
				HurtBoxGroup obj2 = hurtBoxGroup;
				int hurtBoxesDeactivatorCounter = obj2.hurtBoxesDeactivatorCounter - 1;
				obj2.hurtBoxesDeactivatorCounter = hurtBoxesDeactivatorCounter;
			}
		}
		disablingHurtBoxes = flag;
	}

	private void OnInventoryChanged()
	{
		EquipmentIndex currentEquipmentIndex = inventory.currentEquipmentIndex;
		if (currentEquipmentIndex != previousEquipmentIndex)
		{
			EquipmentDef equipmentDef = EquipmentCatalog.GetEquipmentDef(previousEquipmentIndex);
			EquipmentDef equipmentDef2 = EquipmentCatalog.GetEquipmentDef(currentEquipmentIndex);
			if ((Object)(object)equipmentDef != (Object)null)
			{
				OnEquipmentLost(equipmentDef);
			}
			if ((Object)(object)equipmentDef2 != (Object)null)
			{
				OnEquipmentGained(equipmentDef2);
			}
			previousEquipmentIndex = currentEquipmentIndex;
		}
		statsDirty = true;
		UpdateHurtBoxesEnabled();
		AddItemBehavior<AffixHauntedBehavior>(HasBuff(RoR2Content.Buffs.AffixHaunted) ? 1 : 0);
		AddItemBehavior<AffixEarthBehavior>(HasBuff(DLC1Content.Buffs.EliteEarth) ? 1 : 0);
		AddItemBehavior<AffixVoidBehavior>(HasBuff(DLC1Content.Buffs.EliteVoid) ? 1 : 0);
		if (NetworkServer.active)
		{
			AddItemBehavior<QuestVolatileBatteryBehaviorServer>(((Object)(object)inventory.GetEquipment(inventory.activeEquipmentSlot).equipmentDef == (Object)(object)RoR2Content.Equipment.QuestVolatileBattery) ? 1 : 0);
			AddItemBehavior<ElementalRingsBehavior>(inventory.GetItemCount(RoR2Content.Items.IceRing) + inventory.GetItemCount(RoR2Content.Items.FireRing));
			AddItemBehavior<ElementalRingVoidBehavior>(inventory.GetItemCount(DLC1Content.Items.ElementalRingVoid));
			AddItemBehavior<OutOfCombatArmorBehavior>(inventory.GetItemCount(DLC1Content.Items.OutOfCombatArmor));
			AddItemBehavior<PrimarySkillShurikenBehavior>(inventory.GetItemCount(DLC1Content.Items.PrimarySkillShuriken));
			AddItemBehavior<MushroomVoidBehavior>(inventory.GetItemCount(DLC1Content.Items.MushroomVoid));
			AddItemBehavior<BearVoidBehavior>(inventory.GetItemCount(DLC1Content.Items.BearVoid));
			AddItemBehavior<LunarSunBehavior>(inventory.GetItemCount(DLC1Content.Items.LunarSun));
			AddItemBehavior<VoidMegaCrabItemBehavior>(inventory.GetItemCount(DLC1Content.Items.VoidMegaCrabItem));
			AddItemBehavior<DroneWeaponsBehavior>(inventory.GetItemCount(DLC1Content.Items.DroneWeapons));
			AddItemBehavior<DroneWeaponsBoostBehavior>(inventory.GetItemCount(DLC1Content.Items.DroneWeaponsBoost));
		}
		executeEliteHealthFraction = Util.ConvertAmplificationPercentageIntoReductionPercentage(13f * (float)inventory.GetItemCount(RoR2Content.Items.ExecuteLowHealthElite)) / 100f;
		if (Object.op_Implicit((Object)(object)skillLocator))
		{
			ReplaceSkillIfItemPresent(skillLocator.primary, RoR2Content.Items.LunarPrimaryReplacement.itemIndex, CommonAssets.lunarPrimaryReplacementSkillDef);
			ReplaceSkillIfItemPresent(skillLocator.secondary, RoR2Content.Items.LunarSecondaryReplacement.itemIndex, CommonAssets.lunarSecondaryReplacementSkillDef);
			ReplaceSkillIfItemPresent(skillLocator.special, RoR2Content.Items.LunarSpecialReplacement.itemIndex, CommonAssets.lunarSpecialReplacementSkillDef);
			ReplaceSkillIfItemPresent(skillLocator.utility, RoR2Content.Items.LunarUtilityReplacement.itemIndex, CommonAssets.lunarUtilityReplacementSkillDef);
		}
		this.onInventoryChanged?.Invoke();
		CharacterBody.onBodyInventoryChangedGlobal?.Invoke(this);
	}

	private void ReplaceSkillIfItemPresent(GenericSkill skill, ItemIndex itemIndex, SkillDef skillDef)
	{
		if (Object.op_Implicit((Object)(object)skill))
		{
			if (inventory.GetItemCount(itemIndex) > 0 && Object.op_Implicit((Object)(object)skillDef))
			{
				skill.SetSkillOverride(this, skillDef, GenericSkill.SkillOverridePriority.Replacement);
			}
			else
			{
				skill.UnsetSkillOverride(this, skillDef, GenericSkill.SkillOverridePriority.Replacement);
			}
		}
	}

	private void OnEquipmentLost(EquipmentDef equipmentDef)
	{
		if (NetworkServer.active && equipmentDef.passiveBuffDef != null)
		{
			RemoveBuff(equipmentDef.passiveBuffDef);
		}
	}

	private void OnEquipmentGained(EquipmentDef equipmentDef)
	{
		if (NetworkServer.active && equipmentDef.passiveBuffDef != null)
		{
			AddBuff(equipmentDef.passiveBuffDef);
		}
	}

	private void UpdateMasterLink()
	{
		if (!bodyFlags.HasFlag(BodyFlags.Masterless) && !linkedToMaster && Object.op_Implicit((Object)(object)master) && Object.op_Implicit((Object)(object)master))
		{
			master.OnBodyStart(this);
			linkedToMaster = true;
			skinIndex = master.loadout.bodyLoadoutManager.GetSkinIndex(bodyIndex);
		}
	}

	[RuntimeInitializeOnLoadMethod]
	private static void Init()
	{
		AssetReferences.Resolve();
	}

	private void Awake()
	{
		transform = ((Component)this).transform;
		rigidbody = ((Component)this).GetComponent<Rigidbody>();
		networkIdentity = ((Component)this).GetComponent<NetworkIdentity>();
		teamComponent = ((Component)this).GetComponent<TeamComponent>();
		healthComponent = ((Component)this).GetComponent<HealthComponent>();
		equipmentSlot = ((Component)this).GetComponent<EquipmentSlot>();
		skillLocator = ((Component)this).GetComponent<SkillLocator>();
		modelLocator = ((Component)this).GetComponent<ModelLocator>();
		characterMotor = ((Component)this).GetComponent<CharacterMotor>();
		characterDirection = ((Component)this).GetComponent<CharacterDirection>();
		inputBank = ((Component)this).GetComponent<InputBankTest>();
		sfxLocator = ((Component)this).GetComponent<SfxLocator>();
		activeBuffsList = BuffCatalog.GetPerBuffBuffer<BuffIndex>();
		buffs = BuffCatalog.GetPerBuffBuffer<int>();
		if (Object.op_Implicit((Object)(object)modelLocator))
		{
			modelLocator.onModelChanged += OnModelChanged;
			OnModelChanged(modelLocator.modelTransform);
		}
		radius = 1f;
		CapsuleCollider component = ((Component)this).GetComponent<CapsuleCollider>();
		if (Object.op_Implicit((Object)(object)component))
		{
			radius = component.radius;
		}
		else
		{
			SphereCollider component2 = ((Component)this).GetComponent<SphereCollider>();
			if (Object.op_Implicit((Object)(object)component2))
			{
				radius = component2.radius;
			}
		}
		try
		{
			CharacterBody.onBodyAwakeGlobal?.Invoke(this);
		}
		catch (Exception ex)
		{
			Debug.LogError((object)ex);
		}
	}

	private void OnModelChanged(Transform modelTransform)
	{
		hurtBoxGroup = null;
		mainHurtBox = null;
		coreTransform = transform;
		if (Object.op_Implicit((Object)(object)modelTransform))
		{
			hurtBoxGroup = ((Component)modelTransform).GetComponent<HurtBoxGroup>();
			if (Object.op_Implicit((Object)(object)hurtBoxGroup))
			{
				mainHurtBox = hurtBoxGroup.mainHurtBox;
				if (Object.op_Implicit((Object)(object)mainHurtBox))
				{
					coreTransform = ((Component)mainHurtBox).transform;
				}
			}
		}
		if (Object.op_Implicit((Object)(object)overrideCoreTransform))
		{
			coreTransform = overrideCoreTransform;
		}
	}

	private void Start()
	{
		UpdateAuthority();
		localStartTime = Run.FixedTimeStamp.now;
		bool num = (bodyFlags & BodyFlags.Masterless) != 0;
		outOfCombatStopwatch = float.PositiveInfinity;
		outOfDangerStopwatch = float.PositiveInfinity;
		notMovingStopwatch = 0f;
		if (NetworkServer.active)
		{
			outOfCombat = true;
			outOfDanger = true;
		}
		RecalculateStats();
		UpdateMasterLink();
		if (num)
		{
			healthComponent.Networkhealth = maxHealth;
		}
		if (Object.op_Implicit((Object)(object)sfxLocator) && healthComponent.alive)
		{
			Util.PlaySound(sfxLocator.aliveLoopStart, ((Component)this).gameObject);
		}
		CharacterBody.onBodyStartGlobal?.Invoke(this);
	}

	public void Update()
	{
		UpdateSpreadBloom(Time.deltaTime);
	}

	public void FixedUpdate()
	{
		//IL_010b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0110: Unknown result type (might be due to invalid IL or missing references)
		//IL_011f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0122: Unknown result type (might be due to invalid IL or missing references)
		//IL_0127: Unknown result type (might be due to invalid IL or missing references)
		//IL_012c: Unknown result type (might be due to invalid IL or missing references)
		//IL_015c: Unknown result type (might be due to invalid IL or missing references)
		//IL_015e: Unknown result type (might be due to invalid IL or missing references)
		outOfCombatStopwatch += Time.fixedDeltaTime;
		outOfDangerStopwatch += Time.fixedDeltaTime;
		aimTimer = Mathf.Max(aimTimer - Time.fixedDeltaTime, 0f);
		if (NetworkServer.active)
		{
			UpdateMultiKill(Time.fixedDeltaTime);
		}
		UpdateMasterLink();
		bool flag = outOfCombat;
		bool flag2 = flag;
		if (NetworkServer.active || hasEffectiveAuthority)
		{
			flag2 = outOfCombatStopwatch >= 5f;
			if (outOfCombat != flag2)
			{
				if (NetworkServer.active)
				{
					((NetworkBehaviour)this).SetDirtyBit(4u);
				}
				outOfCombat = flag2;
				statsDirty = true;
			}
		}
		if (NetworkServer.active)
		{
			UpdateBuffs(Time.fixedDeltaTime);
			bool flag3 = outOfDangerStopwatch >= 7f;
			bool flag4 = outOfDanger;
			bool flag5 = flag && flag4;
			bool num = flag2 && flag3;
			if (outOfDanger != flag3)
			{
				((NetworkBehaviour)this).SetDirtyBit(8u);
				outOfDanger = flag3;
				statsDirty = true;
			}
			if (num && !flag5)
			{
				OnOutOfCombatAndDangerServer();
			}
			Vector3 position = transform.position;
			float num2 = 0.1f * Time.fixedDeltaTime;
			Vector3 val = position - previousPosition;
			if (((Vector3)(ref val)).sqrMagnitude <= num2 * num2)
			{
				notMovingStopwatch += Time.fixedDeltaTime;
			}
			else
			{
				notMovingStopwatch = 0f;
			}
			previousPosition = position;
			UpdateHelfire();
			UpdateAffixPoison(Time.fixedDeltaTime);
			UpdateAffixLunar(Time.fixedDeltaTime);
		}
		if (statsDirty)
		{
			RecalculateStats();
		}
		UpdateFireTrail();
	}

	public void OnDeathStart()
	{
		((Behaviour)this).enabled = false;
		if (Object.op_Implicit((Object)(object)sfxLocator))
		{
			Util.PlaySound(sfxLocator.aliveLoopStop, ((Component)this).gameObject);
		}
		if (NetworkServer.active && Object.op_Implicit((Object)(object)currentVehicle))
		{
			currentVehicle.EjectPassenger(((Component)this).gameObject);
		}
		if (Object.op_Implicit((Object)(object)master))
		{
			master.OnBodyDeath(this);
		}
		ModelLocator component = ((Component)this).GetComponent<ModelLocator>();
		if (!Object.op_Implicit((Object)(object)component))
		{
			return;
		}
		Transform modelTransform = component.modelTransform;
		if (Object.op_Implicit((Object)(object)modelTransform))
		{
			CharacterModel component2 = ((Component)modelTransform).GetComponent<CharacterModel>();
			if (Object.op_Implicit((Object)(object)component2))
			{
				component2.OnDeath();
			}
		}
	}

	public void OnTakeDamageServer(DamageReport damageReport)
	{
		if (damageReport.damageDealt > 0f)
		{
			outOfDangerStopwatch = 0f;
		}
		if (Object.op_Implicit((Object)(object)master))
		{
			master.OnBodyDamaged(damageReport);
		}
	}

	public void OnSkillActivated(GenericSkill skill)
	{
		if (skill.isCombatSkill)
		{
			outOfCombatStopwatch = 0f;
		}
		if (hasEffectiveAuthority)
		{
			this.onSkillActivatedAuthority?.Invoke(skill);
		}
		if (NetworkServer.active)
		{
			this.onSkillActivatedServer?.Invoke(skill);
		}
		else
		{
			CallCmdOnSkillActivated((sbyte)skillLocator.FindSkillSlot(skill));
		}
	}

	public void OnDestroy()
	{
		try
		{
			CharacterBody.onBodyDestroyGlobal?.Invoke(this);
		}
		catch (Exception ex)
		{
			Debug.LogError((object)ex);
		}
		if (Object.op_Implicit((Object)(object)sfxLocator))
		{
			Util.PlaySound(sfxLocator.aliveLoopStop, ((Component)this).gameObject);
		}
		if (modelLocator != null)
		{
			modelLocator.onModelChanged -= OnModelChanged;
		}
		if (Object.op_Implicit((Object)(object)inventory))
		{
			inventory.onInventoryChanged -= OnInventoryChanged;
		}
		if (Object.op_Implicit((Object)(object)master))
		{
			master.OnBodyDestroyed(this);
		}
	}

	public float GetNormalizedThreatValue()
	{
		if (Object.op_Implicit((Object)(object)Run.instance))
		{
			return (Object.op_Implicit((Object)(object)master) ? ((float)master.money) : 0f) / Mathf.Pow(Run.instance.compensatedDifficultyCoefficient, 2f);
		}
		return 0f;
	}

	private void OnEnable()
	{
		instancesList.Add(this);
	}

	private void OnDisable()
	{
		instancesList.Remove(this);
	}

	private void OnValidate()
	{
		if (autoCalculateLevelStats)
		{
			PerformAutoCalculateLevelStats();
		}
		if (!Application.isPlaying && bodyIndex != BodyIndex.None)
		{
			bodyIndex = BodyIndex.None;
		}
	}

	private void UpdateAuthority()
	{
		hasEffectiveAuthority = Util.HasEffectiveAuthority(((Component)this).gameObject);
	}

	public override void OnStartAuthority()
	{
		UpdateAuthority();
	}

	public override void OnStopAuthority()
	{
		UpdateAuthority();
	}

	private void OnSprintStart()
	{
		if (Object.op_Implicit((Object)(object)sfxLocator))
		{
			Util.PlaySound(sfxLocator.sprintLoopStart, ((Component)this).gameObject);
		}
	}

	private void OnSprintStop()
	{
		if (Object.op_Implicit((Object)(object)sfxLocator))
		{
			Util.PlaySound(sfxLocator.sprintLoopStop, ((Component)this).gameObject);
		}
	}

	[Command]
	private void CmdUpdateSprint(bool newIsSprinting)
	{
		isSprinting = newIsSprinting;
	}

	[Command]
	private void CmdOnSkillActivated(sbyte skillIndex)
	{
		OnSkillActivated(skillLocator.GetSkill((SkillSlot)skillIndex));
	}

	private void OnOutOfDangerChanged()
	{
		if (outOfDanger && healthComponent.shield != healthComponent.fullShield)
		{
			Util.PlaySound("Play_item_proc_personal_shield_recharge", ((Component)this).gameObject);
		}
	}

	[Server]
	private void OnOutOfCombatAndDangerServer()
	{
		if (!NetworkServer.active)
		{
			Debug.LogWarning((object)"[Server] function 'System.Void RoR2.CharacterBody::OnOutOfCombatAndDangerServer()' called on client");
		}
	}

	[Server]
	public bool GetNotMoving()
	{
		if (!NetworkServer.active)
		{
			Debug.LogWarning((object)"[Server] function 'System.Boolean RoR2.CharacterBody::GetNotMoving()' called on client");
			return false;
		}
		return notMovingStopwatch >= 1f;
	}

	public void PerformAutoCalculateLevelStats()
	{
		levelMaxHealth = Mathf.Round(baseMaxHealth * 0.3f);
		levelMaxShield = Mathf.Round(baseMaxShield * 0.3f);
		levelRegen = baseRegen * 0.2f;
		levelMoveSpeed = 0f;
		levelJumpPower = 0f;
		levelDamage = baseDamage * 0.2f;
		levelAttackSpeed = 0f;
		levelCrit = 0f;
		levelArmor = 0f;
	}

	public void MarkAllStatsDirty()
	{
		statsDirty = true;
	}

	public void RecalculateStats()
	{
		float num = level;
		TeamManager.instance.GetTeamExperience(teamComponent.teamIndex);
		float num2 = TeamManager.instance.GetTeamLevel(teamComponent.teamIndex);
		int num3 = 0;
		int num4 = 0;
		int num5 = 0;
		int num6 = 0;
		int num7 = 0;
		int num8 = 0;
		int num9 = 0;
		int num10 = 0;
		int num11 = 0;
		int num12 = 0;
		int num13 = 0;
		int num14 = 0;
		int num15 = 0;
		int num16 = 0;
		int num17 = 0;
		int num18 = 0;
		int num19 = 0;
		int num20 = 0;
		int bonusStockFromBody = 0;
		int num21 = 0;
		int num22 = 0;
		int num23 = 0;
		int num24 = 0;
		int num25 = 0;
		int num26 = 0;
		int num27 = 0;
		int num28 = 0;
		int num29 = 0;
		int num30 = 0;
		int num31 = 0;
		int num32 = 0;
		int num33 = 0;
		int num34 = 0;
		int num35 = 0;
		int num36 = 0;
		int num37 = 0;
		int num38 = 0;
		int num39 = 0;
		int num40 = 0;
		int num41 = 0;
		int num42 = 0;
		int num43 = 0;
		int num44 = 0;
		int num45 = 0;
		int num46 = 0;
		int num47 = 0;
		int num48 = 0;
		int num49 = 0;
		EquipmentIndex equipmentIndex = EquipmentIndex.None;
		uint num50 = 0u;
		if (Object.op_Implicit((Object)(object)inventory))
		{
			num3 = inventory.GetItemCount(RoR2Content.Items.LevelBonus);
			num4 = inventory.GetItemCount(RoR2Content.Items.Infusion);
			num5 = inventory.GetItemCount(RoR2Content.Items.HealWhileSafe);
			num6 = inventory.GetItemCount(RoR2Content.Items.PersonalShield);
			num7 = inventory.GetItemCount(RoR2Content.Items.Hoof);
			num8 = inventory.GetItemCount(RoR2Content.Items.SprintOutOfCombat);
			num9 = inventory.GetItemCount(RoR2Content.Items.Feather);
			num10 = inventory.GetItemCount(RoR2Content.Items.Syringe);
			num11 = inventory.GetItemCount(RoR2Content.Items.CritGlasses);
			num12 = inventory.GetItemCount(RoR2Content.Items.AttackSpeedOnCrit);
			num13 = inventory.GetItemCount(JunkContent.Items.CooldownOnCrit);
			num14 = inventory.GetItemCount(RoR2Content.Items.HealOnCrit);
			num15 = inventory.GetItemCount(RoR2Content.Items.ShieldOnly);
			num16 = inventory.GetItemCount(RoR2Content.Items.AlienHead);
			num17 = inventory.GetItemCount(RoR2Content.Items.Knurl);
			num18 = inventory.GetItemCount(RoR2Content.Items.BoostHp);
			num19 = inventory.GetItemCount(JunkContent.Items.CritHeal);
			num20 = inventory.GetItemCount(RoR2Content.Items.SprintBonus);
			bonusStockFromBody = inventory.GetItemCount(RoR2Content.Items.SecondarySkillMagazine);
			num22 = inventory.GetItemCount(RoR2Content.Items.SprintArmor);
			num23 = inventory.GetItemCount(RoR2Content.Items.UtilitySkillMagazine);
			num24 = inventory.GetItemCount(RoR2Content.Items.HealthDecay);
			num26 = inventory.GetItemCount(RoR2Content.Items.TonicAffliction);
			num27 = inventory.GetItemCount(RoR2Content.Items.LunarDagger);
			num25 = inventory.GetItemCount(RoR2Content.Items.DrizzlePlayerHelper);
			num28 = inventory.GetItemCount(RoR2Content.Items.MonsoonPlayerHelper);
			num29 = inventory.GetItemCount(RoR2Content.Items.Pearl);
			num30 = inventory.GetItemCount(RoR2Content.Items.ShinyPearl);
			num31 = inventory.GetItemCount(RoR2Content.Items.InvadingDoppelganger);
			num32 = inventory.GetItemCount(RoR2Content.Items.CutHp);
			num33 = inventory.GetItemCount(RoR2Content.Items.BoostAttackSpeed);
			num34 = inventory.GetItemCount(RoR2Content.Items.BleedOnHitAndExplode);
			num35 = inventory.GetItemCount(RoR2Content.Items.LunarBadLuck);
			num36 = inventory.GetItemCount(RoR2Content.Items.FlatHealth);
			num37 = inventory.GetItemCount(RoR2Content.Items.TeamSizeDamageBonus);
			num38 = inventory.GetItemCount(RoR2Content.Items.SummonedEcho);
			num39 = inventory.GetItemCount(RoR2Content.Items.UseAmbientLevel);
			num21 = inventory.GetItemCount(DLC1Content.Items.EquipmentMagazineVoid);
			num43 = inventory.GetItemCount(DLC1Content.Items.HalfAttackSpeedHalfCooldowns);
			num44 = inventory.GetItemCount(DLC1Content.Items.HalfSpeedDoubleHealth);
			num40 = inventory.GetItemCount(RoR2Content.Items.BleedOnHit);
			num41 = inventory.GetItemCount(DLC1Content.Items.AttackSpeedAndMoveSpeed);
			num42 = inventory.GetItemCount(DLC1Content.Items.CritDamage);
			num45 = inventory.GetItemCount(DLC1Content.Items.ConvertCritChanceToCritDamage);
			num46 = inventory.GetItemCount(DLC1Content.Items.DroneWeaponsBoost);
			num47 = inventory.GetItemCount(DLC1Content.Items.MissileVoid);
			equipmentIndex = inventory.currentEquipmentIndex;
			num50 = inventory.infusionBonus;
			num48 = ((equipmentIndex == DLC1Content.Equipment.EliteVoidEquipment.equipmentIndex) ? 1 : 0);
			num49 = inventory.GetItemCount(DLC1Content.Items.OutOfCombatArmor);
			inventory.GetItemCount(DLC1Content.Items.VoidmanPassiveItem);
		}
		level = num2;
		if (num39 > 0)
		{
			level = Math.Max(level, Run.instance.ambientLevelFloor);
		}
		level += num3;
		EquipmentDef equipmentDef = EquipmentCatalog.GetEquipmentDef(equipmentIndex);
		float num51 = level - 1f;
		isElite = eliteBuffCount > 0;
		bool flag = HasBuff(RoR2Content.Buffs.TonicBuff);
		bool num52 = HasBuff(RoR2Content.Buffs.Entangle);
		bool flag2 = HasBuff(RoR2Content.Buffs.Nullified);
		bool flag3 = HasBuff(RoR2Content.Buffs.LunarSecondaryRoot);
		bool flag4 = teamComponent.teamIndex == TeamIndex.Player && RunArtifactManager.instance.IsArtifactEnabled(RoR2Content.Artifacts.glassArtifactDef);
		bool num53 = num15 > 0 || HasBuff(RoR2Content.Buffs.AffixLunar);
		bool flag5 = equipmentDef != null && equipmentDef == JunkContent.Equipment.EliteYellowEquipment;
		hasOneShotProtection = isPlayerControlled;
		int buffCount = GetBuffCount(RoR2Content.Buffs.BeetleJuice);
		isGlass = flag4 || num27 > 0;
		canPerformBackstab = (bodyFlags & BodyFlags.HasBackstabPassive) == BodyFlags.HasBackstabPassive;
		canReceiveBackstab = (bodyFlags & BodyFlags.HasBackstabImmunity) != BodyFlags.HasBackstabImmunity;
		float num54 = maxHealth;
		float num55 = maxShield;
		float num56 = baseMaxHealth + levelMaxHealth * num51;
		float num57 = 1f;
		num57 += (float)num18 * 0.1f;
		num57 += (float)(num29 + num30) * 0.1f;
		num57 += (float)num48 * 0.5f;
		num57 += (float)num44 * 1f;
		if (num4 > 0)
		{
			num56 += (float)num50;
		}
		num56 += (float)num36 * 25f;
		num56 += (float)num17 * 40f;
		num56 *= num57;
		num56 /= (float)(num32 + 1);
		if (num31 > 0)
		{
			num56 *= 10f;
		}
		if (num38 > 0)
		{
			num56 *= 0.1f;
		}
		maxHealth = num56;
		float num58 = baseMaxShield + levelMaxShield * num51;
		num58 += (float)num6 * 0.08f * maxHealth;
		if (HasBuff(RoR2Content.Buffs.EngiShield))
		{
			num58 += maxHealth * 1f;
		}
		if (HasBuff(JunkContent.Buffs.EngiTeamShield))
		{
			num58 += maxHealth * 0.5f;
		}
		if (num47 > 0)
		{
			num58 += maxHealth * 0.1f;
		}
		if (num53)
		{
			num58 += maxHealth * (1.5f + (float)(num15 - 1) * 0.25f);
			maxHealth = 1f;
		}
		if (HasBuff(RoR2Content.Buffs.AffixBlue))
		{
			float num59 = maxHealth * 0.5f;
			maxHealth -= num59;
			num58 += maxHealth;
		}
		maxShield = num58;
		float num60 = baseRegen + levelRegen * num51;
		float num61 = 1f + num51 * 0.2f;
		float num62 = (float)num17 * 1.6f * num61;
		float num63 = ((outOfDanger && num5 > 0) ? (3f * (float)num5) : 0f) * num61;
		float num64 = (HasBuff(JunkContent.Buffs.MeatRegenBoost) ? 2f : 0f) * num61;
		float num65 = (float)GetBuffCount(RoR2Content.Buffs.CrocoRegen) * maxHealth * 0.1f;
		float num66 = (float)num30 * 0.1f * num61;
		float num67 = 1f;
		if (num25 > 0)
		{
			num67 += 0.5f;
		}
		if (num28 > 0)
		{
			num67 -= 0.4f;
		}
		float num68 = (num60 + num62 + num63 + num64 + num66) * num67;
		if (HasBuff(RoR2Content.Buffs.OnFire) || HasBuff(DLC1Content.Buffs.StrongerBurn))
		{
			num68 = Mathf.Min(0f, num68);
		}
		num68 += num65;
		if (num53)
		{
			num68 = Mathf.Max(num68, 0f);
		}
		if (num24 > 0)
		{
			num68 = Mathf.Min(num68, 0f) - maxHealth / cursePenalty / (float)num24;
		}
		regen = num68;
		float num69 = baseMoveSpeed + levelMoveSpeed * num51;
		float num70 = 1f;
		if (flag5)
		{
			num69 += 2f;
		}
		if (isSprinting)
		{
			num69 *= sprintingSpeedMultiplier;
		}
		num70 += (float)num7 * 0.14f;
		num70 += (float)num41 * 0.07f;
		num70 += (float)num30 * 0.1f;
		num70 += 0.25f * (float)GetBuffCount(DLC1Content.Buffs.KillMoveSpeed);
		if (teamComponent.teamIndex == TeamIndex.Monster && Run.instance.selectedDifficulty >= DifficultyIndex.Eclipse4)
		{
			num70 += 0.4f;
		}
		if (isSprinting && num20 > 0)
		{
			num70 += 0.25f * (float)num20 / sprintingSpeedMultiplier;
		}
		if (num8 > 0 && HasBuff(RoR2Content.Buffs.WhipBoost))
		{
			num70 += (float)num8 * 0.3f;
		}
		if (num38 > 0)
		{
			num70 += 0.66f;
		}
		if (HasBuff(RoR2Content.Buffs.BugWings))
		{
			num70 += 0.2f;
		}
		if (HasBuff(RoR2Content.Buffs.Warbanner))
		{
			num70 += 0.3f;
		}
		if (HasBuff(JunkContent.Buffs.EnrageAncientWisp))
		{
			num70 += 0.4f;
		}
		if (HasBuff(RoR2Content.Buffs.CloakSpeed))
		{
			num70 += 0.4f;
		}
		if (HasBuff(RoR2Content.Buffs.WarCryBuff) || HasBuff(RoR2Content.Buffs.TeamWarCry))
		{
			num70 += 0.5f;
		}
		if (HasBuff(JunkContent.Buffs.EngiTeamShield))
		{
			num70 += 0.3f;
		}
		if (HasBuff(RoR2Content.Buffs.AffixLunar))
		{
			num70 += 0.3f;
		}
		float num71 = 1f;
		if (HasBuff(RoR2Content.Buffs.Slow50))
		{
			num71 += 0.5f;
		}
		if (HasBuff(RoR2Content.Buffs.Slow60))
		{
			num71 += 0.6f;
		}
		if (HasBuff(RoR2Content.Buffs.Slow80))
		{
			num71 += 0.8f;
		}
		if (HasBuff(RoR2Content.Buffs.ClayGoo))
		{
			num71 += 0.5f;
		}
		if (HasBuff(JunkContent.Buffs.Slow30))
		{
			num71 += 0.3f;
		}
		if (HasBuff(RoR2Content.Buffs.Cripple))
		{
			num71 += 1f;
		}
		if (HasBuff(DLC1Content.Buffs.JailerSlow))
		{
			num71 += 1f;
		}
		num71 += (float)num44 * 1f;
		num69 *= num70 / num71;
		if (buffCount > 0)
		{
			num69 *= 1f - 0.05f * (float)buffCount;
		}
		moveSpeed = num69;
		acceleration = moveSpeed / baseMoveSpeed * baseAcceleration;
		if (num52 || flag2 || flag3)
		{
			moveSpeed = 0f;
			acceleration = 80f;
		}
		float num72 = baseJumpPower + levelJumpPower * num51;
		jumpPower = num72;
		maxJumpHeight = Trajectory.CalculateApex(jumpPower);
		maxJumpCount = baseJumpCount + num9;
		oneShotProtectionFraction = 0.1f;
		float num73 = baseDamage + levelDamage * num51;
		float num74 = 1f;
		int num75 = (Object.op_Implicit((Object)(object)inventory) ? inventory.GetItemCount(RoR2Content.Items.BoostDamage) : 0);
		if (num75 > 0)
		{
			num74 += (float)num75 * 0.1f;
		}
		if (num37 > 0)
		{
			int num76 = Math.Max(TeamComponent.GetTeamMembers(teamComponent.teamIndex).Count - 1, 0);
			num74 += (float)(num76 * num37) * 1f;
		}
		if (buffCount > 0)
		{
			num74 -= 0.05f * (float)buffCount;
		}
		if (HasBuff(JunkContent.Buffs.GoldEmpowered))
		{
			num74 += 1f;
		}
		if (HasBuff(RoR2Content.Buffs.PowerBuff))
		{
			num74 += 0.5f;
		}
		num74 += (float)num30 * 0.1f;
		num74 += Mathf.Pow(2f, (float)num27) - 1f;
		num74 -= (float)num48 * 0.3f;
		num73 *= num74;
		if (num31 > 0)
		{
			num73 *= 0.04f;
		}
		if (flag4)
		{
			num73 *= 5f;
		}
		damage = num73;
		float num77 = baseAttackSpeed + levelAttackSpeed * num51;
		float num78 = 1f;
		num78 += (float)num33 * 0.1f;
		num78 += (float)num10 * 0.15f;
		num78 += (float)num41 * 0.075f;
		num78 += (float)num46 * 0.5f;
		if (flag5)
		{
			num78 += 0.5f;
		}
		num78 += (float)GetBuffCount(RoR2Content.Buffs.AttackSpeedOnCrit) * 0.12f;
		if (HasBuff(RoR2Content.Buffs.Warbanner))
		{
			num78 += 0.3f;
		}
		if (HasBuff(RoR2Content.Buffs.Energized))
		{
			num78 += 0.7f;
		}
		if (HasBuff(RoR2Content.Buffs.WarCryBuff) || HasBuff(RoR2Content.Buffs.TeamWarCry))
		{
			num78 += 1f;
		}
		num78 += (float)num30 * 0.1f;
		num78 /= (float)(num43 + 1);
		num78 = Mathf.Max(num78, 0.1f);
		num77 *= num78;
		if (buffCount > 0)
		{
			num77 *= 1f - 0.05f * (float)buffCount;
		}
		attackSpeed = num77;
		critMultiplier = 2f + 1f * (float)num42;
		float num79 = baseCrit + levelCrit * num51;
		num79 += (float)num11 * 10f;
		if (num12 > 0)
		{
			num79 += 5f;
		}
		if (num34 > 0)
		{
			num79 += 5f;
		}
		if (num13 > 0)
		{
			num79 += 5f;
		}
		if (num14 > 0)
		{
			num79 += 5f;
		}
		if (num19 > 0)
		{
			num79 += 5f;
		}
		if (HasBuff(RoR2Content.Buffs.FullCrit))
		{
			num79 += 100f;
		}
		num79 += (float)num30 * 10f;
		if (num45 == 0)
		{
			crit = num79;
		}
		else
		{
			critMultiplier += num79 * 0.01f;
			crit = 0f;
		}
		armor = baseArmor + levelArmor * num51;
		if (num30 > 0)
		{
			armor *= 1f + 0.1f * (float)num30;
		}
		armor += (float)num25 * 70f;
		armor += (HasBuff(RoR2Content.Buffs.ArmorBoost) ? 200f : 0f);
		armor += (HasBuff(RoR2Content.Buffs.SmallArmorBoost) ? 100f : 0f);
		armor += (HasBuff(DLC1Content.Buffs.OutOfCombatArmorBuff) ? (100f * (float)num49) : 0f);
		armor += (HasBuff(RoR2Content.Buffs.ElephantArmorBoost) ? 500f : 0f);
		armor += (HasBuff(DLC1Content.Buffs.VoidSurvivorCorruptMode) ? 100f : 0f);
		if (HasBuff(RoR2Content.Buffs.Cripple))
		{
			armor -= 20f;
		}
		if (HasBuff(RoR2Content.Buffs.Pulverized))
		{
			armor -= 60f;
		}
		if (isSprinting && num22 > 0)
		{
			armor += num22 * 30;
		}
		int buffCount2 = GetBuffCount(DLC1Content.Buffs.PermanentDebuff);
		armor -= (float)buffCount2 * 2f;
		float num80 = 0f;
		if (num35 > 0)
		{
			num80 += 2f + 1f * (float)(num35 - 1);
		}
		float num81 = 1f;
		if (HasBuff(JunkContent.Buffs.GoldEmpowered))
		{
			num81 *= 0.25f;
		}
		for (int i = 0; i < num16; i++)
		{
			num81 *= 0.75f;
		}
		for (int j = 0; j < num43; j++)
		{
			num81 *= 0.5f;
		}
		for (int k = 0; k < num46; k++)
		{
			num81 *= 0.5f;
		}
		if (teamComponent.teamIndex == TeamIndex.Monster && Run.instance.selectedDifficulty >= DifficultyIndex.Eclipse7)
		{
			num81 *= 0.5f;
		}
		if (HasBuff(RoR2Content.Buffs.NoCooldowns))
		{
			num81 = 0f;
		}
		if (Object.op_Implicit((Object)(object)skillLocator.primary))
		{
			skillLocator.primary.cooldownScale = num81;
			skillLocator.primary.flatCooldownReduction = num80;
		}
		if (Object.op_Implicit((Object)(object)skillLocator.secondaryBonusStockSkill))
		{
			skillLocator.secondaryBonusStockSkill.cooldownScale = num81;
			skillLocator.secondaryBonusStockSkill.SetBonusStockFromBody(bonusStockFromBody);
			skillLocator.secondaryBonusStockSkill.flatCooldownReduction = num80;
		}
		if (Object.op_Implicit((Object)(object)skillLocator.utilityBonusStockSkill))
		{
			float num82 = num81;
			if (num23 > 0)
			{
				num82 *= 2f / 3f;
			}
			skillLocator.utilityBonusStockSkill.cooldownScale = num82;
			skillLocator.utilityBonusStockSkill.flatCooldownReduction = num80;
			skillLocator.utilityBonusStockSkill.SetBonusStockFromBody(num23 * 2);
		}
		if (Object.op_Implicit((Object)(object)skillLocator.specialBonusStockSkill))
		{
			skillLocator.specialBonusStockSkill.cooldownScale = num81;
			if (num21 > 0)
			{
				skillLocator.specialBonusStockSkill.cooldownScale *= 0.67f;
			}
			skillLocator.specialBonusStockSkill.flatCooldownReduction = num80;
			skillLocator.specialBonusStockSkill.SetBonusStockFromBody(num21);
		}
		critHeal = 0f;
		if (num19 > 0)
		{
			float num83 = crit;
			crit /= num19 + 1;
			critHeal = num83 - crit;
		}
		cursePenalty = 1f;
		if (num27 > 0)
		{
			cursePenalty = Mathf.Pow(2f, (float)num27);
		}
		if (flag4)
		{
			cursePenalty *= 10f;
		}
		int buffCount3 = GetBuffCount(RoR2Content.Buffs.PermanentCurse);
		if (buffCount3 > 0)
		{
			cursePenalty += (float)buffCount3 * 0.01f;
		}
		if (HasBuff(RoR2Content.Buffs.Weak))
		{
			armor -= 30f;
			damage *= 0.6f;
			moveSpeed *= 0.6f;
		}
		if (flag)
		{
			maxHealth *= 1.5f;
			maxShield *= 1.5f;
			attackSpeed *= 1.7f;
			moveSpeed *= 1.3f;
			armor += 20f;
			damage *= 2f;
			regen *= 4f;
		}
		else if (num26 > 0)
		{
			float num84 = Mathf.Pow(0.95f, (float)num26);
			attackSpeed *= num84;
			moveSpeed *= num84;
			damage *= num84;
			regen *= num84;
			cursePenalty += 0.1f * (float)num26;
		}
		maxHealth /= cursePenalty;
		maxShield /= cursePenalty;
		oneShotProtectionFraction = Mathf.Max(0f, oneShotProtectionFraction - (1f - 1f / cursePenalty));
		maxBarrier = maxHealth + maxShield;
		barrierDecayRate = maxBarrier / 30f;
		if (NetworkServer.active)
		{
			float num85 = maxHealth - num54;
			float num86 = maxShield - num55;
			if (num85 > 0f)
			{
				healthComponent.Heal(num85, default(ProcChainMask), nonRegen: false);
			}
			else if (healthComponent.health > maxHealth)
			{
				healthComponent.Networkhealth = Mathf.Max(healthComponent.health + num85, maxHealth);
			}
			if (num86 > 0f)
			{
				healthComponent.RechargeShield(num86);
			}
			else if (healthComponent.shield > maxShield)
			{
				healthComponent.Networkshield = Mathf.Max(healthComponent.shield + num86, maxShield);
			}
		}
		bleedChance = 10f * (float)num40;
		visionDistance = baseVisionDistance;
		if (HasBuff(DLC1Content.Buffs.Blinded))
		{
			visionDistance = Mathf.Min(visionDistance, 15f);
		}
		if (level != num)
		{
			OnCalculatedLevelChanged(num, level);
		}
		UpdateAllTemporaryVisualEffects();
		statsDirty = false;
	}

	public void OnTeamLevelChanged()
	{
		statsDirty = true;
	}

	private void OnCalculatedLevelChanged(float oldLevel, float newLevel)
	{
		if (newLevel > oldLevel)
		{
			int num = Mathf.FloorToInt(oldLevel);
			if (Mathf.FloorToInt(newLevel) > num && num != 0)
			{
				OnLevelUp();
			}
		}
	}

	private void OnLevelUp()
	{
		GlobalEventManager.OnCharacterLevelUp(this);
	}

	public void SetAimTimer(float duration)
	{
		aimTimer = duration;
	}

	public override void OnDeserialize(NetworkReader reader, bool initialState)
	{
		//IL_000c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0011: Unknown result type (might be due to invalid IL or missing references)
		//IL_0012: Unknown result type (might be due to invalid IL or missing references)
		//IL_0014: Unknown result type (might be due to invalid IL or missing references)
		//IL_0021: Unknown result type (might be due to invalid IL or missing references)
		//IL_0022: Unknown result type (might be due to invalid IL or missing references)
		byte num = reader.ReadByte();
		if (((uint)num & (true ? 1u : 0u)) != 0)
		{
			NetworkInstanceId val = reader.ReadNetworkId();
			if (val != masterObjectId)
			{
				masterObjectId = val;
				statsDirty = true;
			}
		}
		if ((num & 2u) != 0)
		{
			ReadBuffs(reader);
		}
		if ((num & 4u) != 0)
		{
			bool flag = reader.ReadBoolean();
			if (!hasEffectiveAuthority && flag != outOfCombat)
			{
				outOfCombat = flag;
				statsDirty = true;
			}
		}
		if ((num & 8u) != 0)
		{
			bool flag2 = reader.ReadBoolean();
			if (flag2 != outOfDanger)
			{
				outOfDanger = flag2;
				statsDirty = true;
			}
		}
		if ((num & 0x10u) != 0)
		{
			bool flag3 = reader.ReadBoolean();
			if (flag3 != isSprinting && !hasEffectiveAuthority)
			{
				statsDirty = true;
				isSprinting = flag3;
			}
		}
	}

	public override bool OnSerialize(NetworkWriter writer, bool initialState)
	{
		//IL_003d: Unknown result type (might be due to invalid IL or missing references)
		uint num = ((NetworkBehaviour)this).syncVarDirtyBits;
		if (initialState)
		{
			num = 31u;
		}
		bool num2 = (num & 1) != 0;
		bool flag = (num & 2) != 0;
		bool flag2 = (num & 4) != 0;
		bool flag3 = (num & 8) != 0;
		bool flag4 = (num & 0x10) != 0;
		writer.Write((byte)num);
		if (num2)
		{
			writer.Write(masterObjectId);
		}
		if (flag)
		{
			WriteBuffs(writer);
		}
		if (flag2)
		{
			writer.Write(outOfCombat);
		}
		if (flag3)
		{
			writer.Write(outOfDanger);
		}
		if (flag4)
		{
			writer.Write(isSprinting);
		}
		if (!initialState)
		{
			return num != 0;
		}
		return false;
	}

	public T AddItemBehavior<T>(int stack) where T : ItemBehavior
	{
		T val = ((Component)this).GetComponent<T>();
		if (stack > 0)
		{
			if (!Object.op_Implicit((Object)(object)val))
			{
				val = ((Component)this).gameObject.AddComponent<T>();
				val.body = this;
				((Behaviour)val).enabled = true;
			}
			val.stack = stack;
			return val;
		}
		if (Object.op_Implicit((Object)(object)val))
		{
			Object.Destroy((Object)(object)val);
		}
		return null;
	}

	public void HandleOnKillEffectsServer(DamageReport damageReport)
	{
		int num = killCountServer + 1;
		killCountServer = num;
		AddMultiKill(1);
	}

	public void OnKilledOtherServer(DamageReport damageReport)
	{
	}

	public void AddHelfireDuration(float duration)
	{
		helfireLifetime = duration;
	}

	[Server]
	private void UpdateHelfire()
	{
		if (!NetworkServer.active)
		{
			Debug.LogWarning((object)"[Server] function 'System.Void RoR2.CharacterBody::UpdateHelfire()' called on client");
			return;
		}
		helfireLifetime -= Time.fixedDeltaTime;
		bool flag = false;
		if (Object.op_Implicit((Object)(object)inventory))
		{
			flag = inventory.GetItemCount(JunkContent.Items.BurnNearby) > 0 || helfireLifetime > 0f;
		}
		if (Object.op_Implicit((Object)(object)helfireController) != flag)
		{
			if (flag)
			{
				helfireController = Object.Instantiate<GameObject>(LegacyResourcesAPI.Load<GameObject>("Prefabs/NetworkedObjects/HelfireController")).GetComponent<HelfireController>();
				helfireController.networkedBodyAttachment.AttachToGameObjectAndSpawn(((Component)this).gameObject);
			}
			else
			{
				Object.Destroy((Object)(object)((Component)helfireController).gameObject);
				helfireController = null;
			}
		}
	}

	private void UpdateFireTrail()
	{
		//IL_0049: Unknown result type (might be due to invalid IL or missing references)
		bool flag = HasBuff(RoR2Content.Buffs.AffixRed);
		if (flag != Object.op_Implicit((Object)(object)fireTrail))
		{
			if (flag)
			{
				fireTrail = Object.Instantiate<GameObject>(LegacyResourcesAPI.Load<GameObject>("Prefabs/FireTrail"), transform).GetComponent<DamageTrail>();
				((Component)fireTrail).transform.position = footPosition;
				fireTrail.owner = ((Component)this).gameObject;
				fireTrail.radius *= radius;
			}
			else
			{
				Object.Destroy((Object)(object)((Component)fireTrail).gameObject);
				fireTrail = null;
			}
		}
		if (Object.op_Implicit((Object)(object)fireTrail))
		{
			fireTrail.damagePerSecond = damage * 1.5f;
		}
	}

	private void UpdateAffixPoison(float deltaTime)
	{
		//IL_0043: Unknown result type (might be due to invalid IL or missing references)
		//IL_0048: Unknown result type (might be due to invalid IL or missing references)
		//IL_0058: Unknown result type (might be due to invalid IL or missing references)
		//IL_005d: Unknown result type (might be due to invalid IL or missing references)
		//IL_005e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0063: Unknown result type (might be due to invalid IL or missing references)
		//IL_0067: Unknown result type (might be due to invalid IL or missing references)
		//IL_006c: Unknown result type (might be due to invalid IL or missing references)
		//IL_006d: Unknown result type (might be due to invalid IL or missing references)
		//IL_006e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0079: Unknown result type (might be due to invalid IL or missing references)
		//IL_007e: Unknown result type (might be due to invalid IL or missing references)
		//IL_008a: Unknown result type (might be due to invalid IL or missing references)
		//IL_008b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0090: Unknown result type (might be due to invalid IL or missing references)
		//IL_0092: Unknown result type (might be due to invalid IL or missing references)
		//IL_0097: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a9: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ae: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b0: Unknown result type (might be due to invalid IL or missing references)
		if (!HasBuff(RoR2Content.Buffs.AffixPoison))
		{
			return;
		}
		poisonballTimer += deltaTime;
		if (poisonballTimer >= 6f)
		{
			int num = 3 + (int)radius;
			poisonballTimer = 0f;
			Vector3 up = Vector3.up;
			float num2 = 360f / (float)num;
			Vector3 val = Vector3.ProjectOnPlane(transform.forward, up);
			Vector3 normalized = ((Vector3)(ref val)).normalized;
			Vector3 val2 = Vector3.RotateTowards(up, normalized, 0.43633232f, float.PositiveInfinity);
			for (int i = 0; i < num; i++)
			{
				Vector3 forward = Quaternion.AngleAxis(num2 * (float)i, up) * val2;
				ProjectileManager.instance.FireProjectile(LegacyResourcesAPI.Load<GameObject>("Prefabs/Projectiles/PoisonOrbProjectile"), corePosition, Util.QuaternionSafeLookRotation(forward), ((Component)this).gameObject, damage * 1f, 0f, Util.CheckRoll(crit, master));
			}
		}
	}

	private void UpdateAffixLunar(float deltaTime)
	{
		//IL_00b6: Unknown result type (might be due to invalid IL or missing references)
		//IL_00c3: Unknown result type (might be due to invalid IL or missing references)
		//IL_00c8: Unknown result type (might be due to invalid IL or missing references)
		//IL_00f5: Unknown result type (might be due to invalid IL or missing references)
		//IL_00f6: Unknown result type (might be due to invalid IL or missing references)
		//IL_00fb: Unknown result type (might be due to invalid IL or missing references)
		//IL_0115: Unknown result type (might be due to invalid IL or missing references)
		//IL_0116: Unknown result type (might be due to invalid IL or missing references)
		//IL_011b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0120: Unknown result type (might be due to invalid IL or missing references)
		//IL_0126: Unknown result type (might be due to invalid IL or missing references)
		//IL_012b: Unknown result type (might be due to invalid IL or missing references)
		//IL_012e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0133: Unknown result type (might be due to invalid IL or missing references)
		//IL_0135: Unknown result type (might be due to invalid IL or missing references)
		//IL_013a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0153: Unknown result type (might be due to invalid IL or missing references)
		//IL_0155: Unknown result type (might be due to invalid IL or missing references)
		//IL_015c: Unknown result type (might be due to invalid IL or missing references)
		//IL_015e: Unknown result type (might be due to invalid IL or missing references)
		if (!outOfCombat && HasBuff(RoR2Content.Buffs.AffixLunar))
		{
			lunarMissileRechargeTimer += deltaTime;
			lunarMissileTimerBetweenShots += deltaTime;
			int num = 4;
			if (!Object.op_Implicit((Object)(object)lunarMissilePrefab))
			{
				lunarMissilePrefab = LegacyResourcesAPI.Load<GameObject>("Prefabs/Projectiles/LunarMissileProjectile");
			}
			if (lunarMissileRechargeTimer >= 10f)
			{
				lunarMissileRechargeTimer = 0f;
				remainingMissilesToFire += num;
			}
			if (remainingMissilesToFire > 0 && lunarMissileTimerBetweenShots > 0.1f)
			{
				lunarMissileTimerBetweenShots = 0f;
				Vector3 val = (Object.op_Implicit((Object)(object)inputBank) ? inputBank.aimDirection : transform.forward);
				float num2 = 180f / (float)num;
				float num3 = 3f + (float)(int)radius * 1f;
				float num4 = damage * 0.3f;
				Quaternion rotation = Util.QuaternionSafeLookRotation(val);
				Vector3 val2 = Quaternion.AngleAxis((float)(remainingMissilesToFire - 1) * num2 - num2 * (float)(num - 1) / 2f, val) * Vector3.up * num3;
				Vector3 position = aimOrigin + val2;
				FireProjectileInfo fireProjectileInfo = default(FireProjectileInfo);
				fireProjectileInfo.projectilePrefab = lunarMissilePrefab;
				fireProjectileInfo.position = position;
				fireProjectileInfo.rotation = rotation;
				fireProjectileInfo.owner = ((Component)this).gameObject;
				fireProjectileInfo.damage = num4;
				fireProjectileInfo.crit = Util.CheckRoll(crit, master);
				fireProjectileInfo.force = 200f;
				FireProjectileInfo fireProjectileInfo2 = fireProjectileInfo;
				ProjectileManager.instance.FireProjectile(fireProjectileInfo2);
				remainingMissilesToFire--;
			}
		}
	}

	private void UpdateAllTemporaryVisualEffects()
	{
		int buffCount = GetBuffCount(RoR2Content.Buffs.NullifyStack);
		UpdateSingleTemporaryVisualEffect(ref engiShieldTempEffectInstance, AssetReferences.engiShieldTempEffectPrefab, bestFitRadius, healthComponent.shield > 0f && HasBuff(RoR2Content.Buffs.EngiShield));
		ref TemporaryVisualEffect tempEffect = ref bucklerShieldTempEffectInstance;
		GameObject bucklerShieldTempEffectPrefab = AssetReferences.bucklerShieldTempEffectPrefab;
		float effectRadius = radius;
		int active;
		if (isSprinting)
		{
			Inventory obj = inventory;
			active = ((obj != null && obj.GetItemCount(RoR2Content.Items.SprintArmor) > 0) ? 1 : 0);
		}
		else
		{
			active = 0;
		}
		UpdateSingleTemporaryVisualEffect(ref tempEffect, bucklerShieldTempEffectPrefab, effectRadius, (byte)active != 0);
		UpdateSingleTemporaryVisualEffect(ref slowDownTimeTempEffectInstance, AssetReferences.slowDownTimeTempEffectPrefab, radius, HasBuff(RoR2Content.Buffs.Slow60));
		UpdateSingleTemporaryVisualEffect(ref crippleEffectInstance, AssetReferences.crippleEffectPrefab, radius, HasBuff(RoR2Content.Buffs.Cripple));
		UpdateSingleTemporaryVisualEffect(ref tonicBuffEffectInstance, AssetReferences.tonicBuffEffectPrefab, radius, HasBuff(RoR2Content.Buffs.TonicBuff));
		UpdateSingleTemporaryVisualEffect(ref weakTempEffectInstance, AssetReferences.weakTempEffectPrefab, radius, HasBuff(RoR2Content.Buffs.Weak));
		UpdateSingleTemporaryVisualEffect(ref energizedTempEffectInstance, AssetReferences.energizedTempEffectPrefab, radius, HasBuff(RoR2Content.Buffs.Energized));
		UpdateSingleTemporaryVisualEffect(ref barrierTempEffectInstance, AssetReferences.barrierTempEffectPrefab, bestFitRadius, healthComponent.barrier > 0f);
		UpdateSingleTemporaryVisualEffect(ref regenBoostEffectInstance, AssetReferences.regenBoostEffectPrefab, bestFitRadius, HasBuff(JunkContent.Buffs.MeatRegenBoost));
		UpdateSingleTemporaryVisualEffect(ref elephantDefenseEffectInstance, AssetReferences.elephantDefenseEffectPrefab, radius, HasBuff(RoR2Content.Buffs.ElephantArmorBoost));
		UpdateSingleTemporaryVisualEffect(ref healingDisabledEffectInstance, AssetReferences.healingDisabledEffectPrefab, radius, HasBuff(RoR2Content.Buffs.HealingDisabled));
		UpdateSingleTemporaryVisualEffect(ref noCooldownEffectInstance, AssetReferences.noCooldownEffectPrefab, radius, HasBuff(RoR2Content.Buffs.NoCooldowns), "Head");
		ref TemporaryVisualEffect tempEffect2 = ref doppelgangerEffectInstance;
		GameObject doppelgangerEffectPrefab = AssetReferences.doppelgangerEffectPrefab;
		float effectRadius2 = radius;
		Inventory obj2 = inventory;
		UpdateSingleTemporaryVisualEffect(ref tempEffect2, doppelgangerEffectPrefab, effectRadius2, obj2 != null && obj2.GetItemCount(RoR2Content.Items.InvadingDoppelganger) > 0, "Head");
		UpdateSingleTemporaryVisualEffect(ref nullifyStack1EffectInstance, AssetReferences.nullifyStack1EffectPrefab, radius, buffCount == 1);
		UpdateSingleTemporaryVisualEffect(ref nullifyStack2EffectInstance, AssetReferences.nullifyStack2EffectPrefab, radius, buffCount == 2);
		UpdateSingleTemporaryVisualEffect(ref nullifyStack3EffectInstance, AssetReferences.nullifyStack3EffectPrefab, radius, HasBuff(RoR2Content.Buffs.Nullified));
		UpdateSingleTemporaryVisualEffect(ref deathmarkEffectInstance, AssetReferences.deathmarkEffectPrefab, radius, HasBuff(RoR2Content.Buffs.DeathMark));
		UpdateSingleTemporaryVisualEffect(ref crocoRegenEffectInstance, AssetReferences.crocoRegenEffectPrefab, bestFitRadius, HasBuff(RoR2Content.Buffs.CrocoRegen));
		UpdateSingleTemporaryVisualEffect(ref mercExposeEffectInstance, AssetReferences.mercExposeEffectPrefab, radius, HasBuff(RoR2Content.Buffs.MercExpose));
		UpdateSingleTemporaryVisualEffect(ref lifestealOnHitEffectInstance, AssetReferences.lifestealOnHitEffectPrefab, bestFitRadius, HasBuff(RoR2Content.Buffs.LifeSteal));
		UpdateSingleTemporaryVisualEffect(ref teamWarCryEffectInstance, AssetReferences.teamWarCryEffectPrefab, bestFitRadius, HasBuff(RoR2Content.Buffs.TeamWarCry), "HeadCenter");
		UpdateSingleTemporaryVisualEffect(ref lunarGolemShieldEffectInstance, AssetReferences.lunarGolemShieldEffectPrefab, bestFitRadius, HasBuff(RoR2Content.Buffs.LunarShell));
		UpdateSingleTemporaryVisualEffect(ref randomDamageEffectInstance, AssetReferences.randomDamageEffectPrefab, radius, HasBuff(RoR2Content.Buffs.PowerBuff));
		UpdateSingleTemporaryVisualEffect(ref warbannerEffectInstance, AssetReferences.warbannerEffectPrefab, radius, HasBuff(RoR2Content.Buffs.Warbanner));
		UpdateSingleTemporaryVisualEffect(ref teslaFieldEffectInstance, AssetReferences.teslaFieldEffectPrefab, bestFitRadius, HasBuff(RoR2Content.Buffs.TeslaField));
		UpdateSingleTemporaryVisualEffect(ref lunarSecondaryRootEffectInstance, AssetReferences.lunarSecondaryRootEffectPrefab, radius, HasBuff(RoR2Content.Buffs.LunarSecondaryRoot));
		UpdateSingleTemporaryVisualEffect(ref lunarDetonatorEffectInstance, AssetReferences.lunarDetonatorEffectPrefab, radius, HasBuff(RoR2Content.Buffs.LunarDetonationCharge));
		UpdateSingleTemporaryVisualEffect(ref fruitingEffectInstance, AssetReferences.fruitingEffectPrefab, radius, HasBuff(RoR2Content.Buffs.Fruiting));
		UpdateSingleTemporaryVisualEffect(ref mushroomVoidTempEffectInstance, AssetReferences.mushroomVoidTempEffectPrefab, radius, HasBuff(DLC1Content.Buffs.MushroomVoidActive));
		UpdateSingleTemporaryVisualEffect(ref bearVoidTempEffectInstance, AssetReferences.bearVoidTempEffectPrefab, radius, HasBuff(DLC1Content.Buffs.BearVoidReady));
		UpdateSingleTemporaryVisualEffect(ref outOfCombatArmorEffectInstance, AssetReferences.outOfCombatArmorEffectPrefab, radius, HasBuff(DLC1Content.Buffs.OutOfCombatArmorBuff));
		UpdateSingleTemporaryVisualEffect(ref voidFogMildEffectInstance, AssetReferences.voidFogMildEffectPrefab, radius, HasBuff(RoR2Content.Buffs.VoidFogMild));
		UpdateSingleTemporaryVisualEffect(ref voidFogStrongEffectInstance, AssetReferences.voidFogStrongEffectPrefab, radius, HasBuff(RoR2Content.Buffs.VoidFogStrong));
		UpdateSingleTemporaryVisualEffect(ref voidRaidcrabWardWipeFogEffectInstance, AssetReferences.voidRaidcrabWardWipeFogEffectPrefab, radius, HasBuff(DLC1Content.Buffs.VoidRaidCrabWardWipeFog));
		UpdateSingleTemporaryVisualEffect(ref voidJailerSlowEffectInstance, AssetReferences.voidJailerSlowEffectPrefab, radius, HasBuff(DLC1Content.Buffs.JailerSlow));
	}

	private void UpdateSingleTemporaryVisualEffect(ref TemporaryVisualEffect tempEffect, string resourceString, float effectRadius, bool active, string childLocatorOverride = "")
	{
		//IL_0025: Unknown result type (might be due to invalid IL or missing references)
		//IL_002a: Unknown result type (might be due to invalid IL or missing references)
		bool flag = (Object)(object)tempEffect != (Object)null;
		if (flag == active)
		{
			return;
		}
		if (active)
		{
			if (flag)
			{
				return;
			}
			GameObject val = Object.Instantiate<GameObject>(LegacyResourcesAPI.Load<GameObject>(resourceString), corePosition, Quaternion.identity);
			tempEffect = val.GetComponent<TemporaryVisualEffect>();
			tempEffect.parentTransform = coreTransform;
			tempEffect.visualState = TemporaryVisualEffect.VisualState.Enter;
			tempEffect.healthComponent = healthComponent;
			tempEffect.radius = effectRadius;
			LocalCameraEffect component = val.GetComponent<LocalCameraEffect>();
			if (Object.op_Implicit((Object)(object)component))
			{
				component.targetCharacter = ((Component)this).gameObject;
			}
			if (string.IsNullOrEmpty(childLocatorOverride))
			{
				return;
			}
			ModelLocator obj = modelLocator;
			object obj2;
			if (obj == null)
			{
				obj2 = null;
			}
			else
			{
				Transform modelTransform = obj.modelTransform;
				obj2 = ((modelTransform != null) ? ((Component)modelTransform).GetComponent<ChildLocator>() : null);
			}
			ChildLocator childLocator = (ChildLocator)obj2;
			if (Object.op_Implicit((Object)(object)childLocator))
			{
				Transform val2 = childLocator.FindChild(childLocatorOverride);
				if (Object.op_Implicit((Object)(object)val2))
				{
					tempEffect.parentTransform = val2;
				}
			}
		}
		else if (Object.op_Implicit((Object)(object)tempEffect))
		{
			tempEffect.visualState = TemporaryVisualEffect.VisualState.Exit;
		}
	}

	private void UpdateSingleTemporaryVisualEffect(ref TemporaryVisualEffect tempEffect, GameObject tempEffectPrefab, float effectRadius, bool active, string childLocatorOverride = "")
	{
		//IL_002b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0030: Unknown result type (might be due to invalid IL or missing references)
		bool flag = (Object)(object)tempEffect != (Object)null;
		if (flag == active)
		{
			return;
		}
		if (active)
		{
			if (flag)
			{
				return;
			}
			if (Object.op_Implicit((Object)(object)tempEffectPrefab))
			{
				GameObject val = Object.Instantiate<GameObject>(tempEffectPrefab, corePosition, Quaternion.identity);
				tempEffect = val.GetComponent<TemporaryVisualEffect>();
				tempEffect.parentTransform = coreTransform;
				tempEffect.visualState = TemporaryVisualEffect.VisualState.Enter;
				tempEffect.healthComponent = healthComponent;
				tempEffect.radius = effectRadius;
				LocalCameraEffect component = val.GetComponent<LocalCameraEffect>();
				if (Object.op_Implicit((Object)(object)component))
				{
					component.targetCharacter = ((Component)this).gameObject;
				}
				if (string.IsNullOrEmpty(childLocatorOverride))
				{
					return;
				}
				ModelLocator obj = modelLocator;
				object obj2;
				if (obj == null)
				{
					obj2 = null;
				}
				else
				{
					Transform modelTransform = obj.modelTransform;
					obj2 = ((modelTransform != null) ? ((Component)modelTransform).GetComponent<ChildLocator>() : null);
				}
				ChildLocator childLocator = (ChildLocator)obj2;
				if (Object.op_Implicit((Object)(object)childLocator))
				{
					Transform val2 = childLocator.FindChild(childLocatorOverride);
					if (Object.op_Implicit((Object)(object)val2))
					{
						tempEffect.parentTransform = val2;
					}
				}
			}
			else
			{
				Debug.LogError((object)"Can't instantiate null temporary visual effect");
			}
		}
		else if (Object.op_Implicit((Object)(object)tempEffect))
		{
			tempEffect.visualState = TemporaryVisualEffect.VisualState.Exit;
		}
	}

	public VisibilityLevel GetVisibilityLevel(CharacterBody observer)
	{
		return GetVisibilityLevel(Object.op_Implicit((Object)(object)observer) ? observer.teamComponent.teamIndex : TeamIndex.None);
	}

	public VisibilityLevel GetVisibilityLevel(TeamIndex observerTeam)
	{
		if (!hasCloakBuff)
		{
			return VisibilityLevel.Visible;
		}
		if (teamComponent.teamIndex != observerTeam)
		{
			return VisibilityLevel.Cloaked;
		}
		return VisibilityLevel.Revealed;
	}

	public void AddSpreadBloom(float value)
	{
		spreadBloomInternal = Mathf.Min(spreadBloomInternal + value, 1f);
	}

	public void SetSpreadBloom(float value, bool canOnlyIncreaseBloom = true)
	{
		if (canOnlyIncreaseBloom)
		{
			spreadBloomInternal = Mathf.Clamp(value, spreadBloomInternal, 1f);
		}
		else
		{
			spreadBloomInternal = Mathf.Min(value, 1f);
		}
	}

	private void UpdateSpreadBloom(float dt)
	{
		float num = 1f / spreadBloomDecayTime;
		spreadBloomInternal = Mathf.Max(spreadBloomInternal - num * dt, 0f);
	}

	[Client]
	public void SendConstructTurret(CharacterBody builder, Vector3 position, Quaternion rotation, MasterCatalog.MasterIndex masterIndex)
	{
		//IL_0027: Unknown result type (might be due to invalid IL or missing references)
		//IL_0028: Unknown result type (might be due to invalid IL or missing references)
		//IL_002e: Unknown result type (might be due to invalid IL or missing references)
		//IL_002f: Unknown result type (might be due to invalid IL or missing references)
		if (!NetworkClient.active)
		{
			Debug.LogWarning((object)"[Client] function 'System.Void RoR2.CharacterBody::SendConstructTurret(RoR2.CharacterBody,UnityEngine.Vector3,UnityEngine.Quaternion,RoR2.MasterCatalog/MasterIndex)' called on server");
			return;
		}
		ConstructTurretMessage constructTurretMessage = new ConstructTurretMessage
		{
			builder = ((Component)builder).gameObject,
			position = position,
			rotation = rotation,
			turretMasterIndex = masterIndex
		};
		ClientScene.readyConnection.Send((short)62, (MessageBase)(object)constructTurretMessage);
	}

	[NetworkMessageHandler(msgType = 62, server = true)]
	private static void HandleConstructTurret(NetworkMessage netMsg)
	{
		//IL_005d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0062: Unknown result type (might be due to invalid IL or missing references)
		//IL_0069: Unknown result type (might be due to invalid IL or missing references)
		//IL_006e: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a7: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b1: Expected O, but got Unknown
		//IL_00bf: Unknown result type (might be due to invalid IL or missing references)
		//IL_00c9: Expected O, but got Unknown
		ConstructTurretMessage constructTurretMessage = netMsg.ReadMessage<ConstructTurretMessage>();
		if (!Object.op_Implicit((Object)(object)constructTurretMessage.builder))
		{
			return;
		}
		CharacterBody component = constructTurretMessage.builder.GetComponent<CharacterBody>();
		if (Object.op_Implicit((Object)(object)component))
		{
			CharacterMaster characterMaster = component.master;
			if (Object.op_Implicit((Object)(object)characterMaster))
			{
				CharacterMaster characterMaster2 = new MasterSummon
				{
					masterPrefab = MasterCatalog.GetMasterPrefab(constructTurretMessage.turretMasterIndex),
					position = constructTurretMessage.position,
					rotation = constructTurretMessage.rotation,
					summonerBodyObject = ((Component)component).gameObject,
					ignoreTeamMemberLimit = true,
					inventoryToCopy = characterMaster.inventory
				}.Perform();
				Deployable deployable = ((Component)characterMaster2).gameObject.AddComponent<Deployable>();
				deployable.onUndeploy = new UnityEvent();
				deployable.onUndeploy.AddListener(new UnityAction(characterMaster2.TrueKill));
				characterMaster.AddDeployable(deployable, DeployableSlot.EngiTurret);
			}
		}
	}

	[Server]
	public void AddMultiKill(int kills)
	{
		if (!NetworkServer.active)
		{
			Debug.LogWarning((object)"[Server] function 'System.Void RoR2.CharacterBody::AddMultiKill(System.Int32)' called on client");
			return;
		}
		multiKillTimer = 1f;
		multiKillCount += kills;
		int num = (Object.op_Implicit((Object)(object)inventory) ? inventory.GetItemCount(RoR2Content.Items.WarCryOnMultiKill) : 0);
		if (num > 0 && multiKillCount >= 4)
		{
			AddTimedBuff(RoR2Content.Buffs.WarCryBuff, 2f + 4f * (float)num);
		}
	}

	[Server]
	private void UpdateMultiKill(float deltaTime)
	{
		if (!NetworkServer.active)
		{
			Debug.LogWarning((object)"[Server] function 'System.Void RoR2.CharacterBody::UpdateMultiKill(System.Single)' called on client");
			return;
		}
		multiKillTimer -= deltaTime;
		if (multiKillTimer <= 0f)
		{
			multiKillTimer = 0f;
			multiKillCount = 0;
		}
	}

	[ClientRpc]
	public void RpcBark()
	{
		if (Object.op_Implicit((Object)(object)sfxLocator))
		{
			Util.PlaySound(sfxLocator.barkSound, ((Component)this).gameObject);
		}
	}

	[Command]
	public void CmdRequestVehicleEjection()
	{
		if (Object.op_Implicit((Object)(object)currentVehicle))
		{
			currentVehicle.EjectPassenger(((Component)this).gameObject);
		}
	}

	public bool RollCrit()
	{
		if (Object.op_Implicit((Object)(object)master))
		{
			return Util.CheckRoll(crit, master);
		}
		return false;
	}

	[ClientRpc]
	private void RpcUsePreferredInitialStateType()
	{
		if (hasEffectiveAuthority)
		{
			SetBodyStateToPreferredInitialState();
		}
	}

	public void SetBodyStateToPreferredInitialState()
	{
		if (!hasEffectiveAuthority)
		{
			if (NetworkServer.active)
			{
				CallRpcUsePreferredInitialStateType();
			}
			return;
		}
		Type stateType = preferredInitialStateType.stateType;
		if (!(stateType == null) && !(stateType == typeof(Uninitialized)))
		{
			EntityStateMachine.FindByCustomName(((Component)this).gameObject, "Body")?.SetState(EntityStateCatalog.InstantiateState(stateType));
		}
	}

	[Server]
	public void SetLoadoutServer(Loadout loadout)
	{
		if (!NetworkServer.active)
		{
			Debug.LogWarning((object)"[Server] function 'System.Void RoR2.CharacterBody::SetLoadoutServer(RoR2.Loadout)' called on client");
		}
		else
		{
			skillLocator.ApplyLoadoutServer(loadout, bodyIndex);
		}
	}

	static CharacterBody()
	{
		//IL_0039: Unknown result type (might be due to invalid IL or missing references)
		//IL_0043: Expected O, but got Unknown
		//IL_0063: Unknown result type (might be due to invalid IL or missing references)
		//IL_006d: Expected O, but got Unknown
		//IL_008d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0097: Expected O, but got Unknown
		//IL_00b7: Unknown result type (might be due to invalid IL or missing references)
		//IL_00c1: Expected O, but got Unknown
		//IL_00e1: Unknown result type (might be due to invalid IL or missing references)
		//IL_00eb: Expected O, but got Unknown
		//IL_010b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0115: Expected O, but got Unknown
		instancesList = new List<CharacterBody>();
		readOnlyInstancesList = new ReadOnlyCollection<CharacterBody>(instancesList);
		kCmdCmdAddTimedBuff = -160178508;
		NetworkBehaviour.RegisterCommandDelegate(typeof(CharacterBody), kCmdCmdAddTimedBuff, new CmdDelegate(InvokeCmdCmdAddTimedBuff));
		kCmdCmdUpdateSprint = -1006016914;
		NetworkBehaviour.RegisterCommandDelegate(typeof(CharacterBody), kCmdCmdUpdateSprint, new CmdDelegate(InvokeCmdCmdUpdateSprint));
		kCmdCmdOnSkillActivated = 384138986;
		NetworkBehaviour.RegisterCommandDelegate(typeof(CharacterBody), kCmdCmdOnSkillActivated, new CmdDelegate(InvokeCmdCmdOnSkillActivated));
		kCmdCmdRequestVehicleEjection = 1803737791;
		NetworkBehaviour.RegisterCommandDelegate(typeof(CharacterBody), kCmdCmdRequestVehicleEjection, new CmdDelegate(InvokeCmdCmdRequestVehicleEjection));
		kRpcRpcBark = -76716871;
		NetworkBehaviour.RegisterRpcDelegate(typeof(CharacterBody), kRpcRpcBark, new CmdDelegate(InvokeRpcRpcBark));
		kRpcRpcUsePreferredInitialStateType = 638695010;
		NetworkBehaviour.RegisterRpcDelegate(typeof(CharacterBody), kRpcRpcUsePreferredInitialStateType, new CmdDelegate(InvokeRpcRpcUsePreferredInitialStateType));
		NetworkCRC.RegisterBehaviour("CharacterBody", 0);
	}

	private void UNetVersion()
	{
	}

	protected static void InvokeCmdCmdAddTimedBuff(NetworkBehaviour obj, NetworkReader reader)
	{
		if (!NetworkServer.active)
		{
			Debug.LogError((object)"Command CmdAddTimedBuff called on client.");
		}
		else
		{
			((CharacterBody)(object)obj).CmdAddTimedBuff((BuffIndex)reader.ReadInt32(), reader.ReadSingle());
		}
	}

	protected static void InvokeCmdCmdUpdateSprint(NetworkBehaviour obj, NetworkReader reader)
	{
		if (!NetworkServer.active)
		{
			Debug.LogError((object)"Command CmdUpdateSprint called on client.");
		}
		else
		{
			((CharacterBody)(object)obj).CmdUpdateSprint(reader.ReadBoolean());
		}
	}

	protected static void InvokeCmdCmdOnSkillActivated(NetworkBehaviour obj, NetworkReader reader)
	{
		if (!NetworkServer.active)
		{
			Debug.LogError((object)"Command CmdOnSkillActivated called on client.");
		}
		else
		{
			((CharacterBody)(object)obj).CmdOnSkillActivated((sbyte)reader.ReadPackedUInt32());
		}
	}

	protected static void InvokeCmdCmdRequestVehicleEjection(NetworkBehaviour obj, NetworkReader reader)
	{
		if (!NetworkServer.active)
		{
			Debug.LogError((object)"Command CmdRequestVehicleEjection called on client.");
		}
		else
		{
			((CharacterBody)(object)obj).CmdRequestVehicleEjection();
		}
	}

	public void CallCmdAddTimedBuff(BuffIndex buffType, float duration)
	{
		//IL_0031: Unknown result type (might be due to invalid IL or missing references)
		//IL_0037: Expected O, but got Unknown
		//IL_005c: Unknown result type (might be due to invalid IL or missing references)
		if (!NetworkClient.active)
		{
			Debug.LogError((object)"Command function CmdAddTimedBuff called on server.");
			return;
		}
		if (((NetworkBehaviour)this).isServer)
		{
			CmdAddTimedBuff(buffType, duration);
			return;
		}
		NetworkWriter val = new NetworkWriter();
		val.Write((short)0);
		val.Write((short)5);
		val.WritePackedUInt32((uint)kCmdCmdAddTimedBuff);
		val.Write(((Component)this).GetComponent<NetworkIdentity>().netId);
		val.Write((int)buffType);
		val.Write(duration);
		((NetworkBehaviour)this).SendCommandInternal(val, 0, "CmdAddTimedBuff");
	}

	public void CallCmdUpdateSprint(bool newIsSprinting)
	{
		//IL_002d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0033: Expected O, but got Unknown
		//IL_0058: Unknown result type (might be due to invalid IL or missing references)
		if (!NetworkClient.active)
		{
			Debug.LogError((object)"Command function CmdUpdateSprint called on server.");
			return;
		}
		if (((NetworkBehaviour)this).isServer)
		{
			CmdUpdateSprint(newIsSprinting);
			return;
		}
		NetworkWriter val = new NetworkWriter();
		val.Write((short)0);
		val.Write((short)5);
		val.WritePackedUInt32((uint)kCmdCmdUpdateSprint);
		val.Write(((Component)this).GetComponent<NetworkIdentity>().netId);
		val.Write(newIsSprinting);
		((NetworkBehaviour)this).SendCommandInternal(val, 0, "CmdUpdateSprint");
	}

	public void CallCmdOnSkillActivated(sbyte skillIndex)
	{
		//IL_002d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0033: Expected O, but got Unknown
		//IL_0058: Unknown result type (might be due to invalid IL or missing references)
		if (!NetworkClient.active)
		{
			Debug.LogError((object)"Command function CmdOnSkillActivated called on server.");
			return;
		}
		if (((NetworkBehaviour)this).isServer)
		{
			CmdOnSkillActivated(skillIndex);
			return;
		}
		NetworkWriter val = new NetworkWriter();
		val.Write((short)0);
		val.Write((short)5);
		val.WritePackedUInt32((uint)kCmdCmdOnSkillActivated);
		val.Write(((Component)this).GetComponent<NetworkIdentity>().netId);
		val.WritePackedUInt32((uint)skillIndex);
		((NetworkBehaviour)this).SendCommandInternal(val, 0, "CmdOnSkillActivated");
	}

	public void CallCmdRequestVehicleEjection()
	{
		//IL_0029: Unknown result type (might be due to invalid IL or missing references)
		//IL_002f: Expected O, but got Unknown
		//IL_0054: Unknown result type (might be due to invalid IL or missing references)
		if (!NetworkClient.active)
		{
			Debug.LogError((object)"Command function CmdRequestVehicleEjection called on server.");
			return;
		}
		if (((NetworkBehaviour)this).isServer)
		{
			CmdRequestVehicleEjection();
			return;
		}
		NetworkWriter val = new NetworkWriter();
		val.Write((short)0);
		val.Write((short)5);
		val.WritePackedUInt32((uint)kCmdCmdRequestVehicleEjection);
		val.Write(((Component)this).GetComponent<NetworkIdentity>().netId);
		((NetworkBehaviour)this).SendCommandInternal(val, 0, "CmdRequestVehicleEjection");
	}

	protected static void InvokeRpcRpcBark(NetworkBehaviour obj, NetworkReader reader)
	{
		if (!NetworkClient.active)
		{
			Debug.LogError((object)"RPC RpcBark called on server.");
		}
		else
		{
			((CharacterBody)(object)obj).RpcBark();
		}
	}

	protected static void InvokeRpcRpcUsePreferredInitialStateType(NetworkBehaviour obj, NetworkReader reader)
	{
		if (!NetworkClient.active)
		{
			Debug.LogError((object)"RPC RpcUsePreferredInitialStateType called on server.");
		}
		else
		{
			((CharacterBody)(object)obj).RpcUsePreferredInitialStateType();
		}
	}

	public void CallRpcBark()
	{
		//IL_0016: Unknown result type (might be due to invalid IL or missing references)
		//IL_001c: Expected O, but got Unknown
		//IL_0041: Unknown result type (might be due to invalid IL or missing references)
		if (!NetworkServer.active)
		{
			Debug.LogError((object)"RPC Function RpcBark called on client.");
			return;
		}
		NetworkWriter val = new NetworkWriter();
		val.Write((short)0);
		val.Write((short)2);
		val.WritePackedUInt32((uint)kRpcRpcBark);
		val.Write(((Component)this).GetComponent<NetworkIdentity>().netId);
		((NetworkBehaviour)this).SendRPCInternal(val, 0, "RpcBark");
	}

	public void CallRpcUsePreferredInitialStateType()
	{
		//IL_0016: Unknown result type (might be due to invalid IL or missing references)
		//IL_001c: Expected O, but got Unknown
		//IL_0041: Unknown result type (might be due to invalid IL or missing references)
		if (!NetworkServer.active)
		{
			Debug.LogError((object)"RPC Function RpcUsePreferredInitialStateType called on client.");
			return;
		}
		NetworkWriter val = new NetworkWriter();
		val.Write((short)0);
		val.Write((short)2);
		val.WritePackedUInt32((uint)kRpcRpcUsePreferredInitialStateType);
		val.Write(((Component)this).GetComponent<NetworkIdentity>().netId);
		((NetworkBehaviour)this).SendRPCInternal(val, 0, "RpcUsePreferredInitialStateType");
	}

	public override void PreStartClient()
	{
	}
}
