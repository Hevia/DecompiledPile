using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Runtime.InteropServices;
using System.Xml.Linq;
using HG;
using JetBrains.Annotations;
using RoR2.ConVar;
using RoR2.ExpansionManagement;
using RoR2.Navigation;
using RoR2.Networking;
using RoR2.Stats;
using Unity;
using UnityEngine;
using UnityEngine.Networking;

namespace RoR2;

[DisallowMultipleComponent]
[RequireComponent(typeof(NetworkRuleBook))]
[RequireComponent(typeof(RunArtifactManager))]
public class Run : NetworkBehaviour
{
	[Serializable]
	public struct RunStopwatch : IEquatable<RunStopwatch>
	{
		public float offsetFromFixedTime;

		public bool isPaused;

		public bool Equals(RunStopwatch other)
		{
			if (offsetFromFixedTime.Equals(other.offsetFromFixedTime))
			{
				return isPaused == other.isPaused;
			}
			return false;
		}

		public override bool Equals(object obj)
		{
			if (obj == null)
			{
				return false;
			}
			if (obj is RunStopwatch other)
			{
				return Equals(other);
			}
			return false;
		}

		public override int GetHashCode()
		{
			return (offsetFromFixedTime.GetHashCode() * 397) ^ isPaused.GetHashCode();
		}
	}

	[Serializable]
	public struct TimeStamp : IEquatable<TimeStamp>, IComparable<TimeStamp>
	{
		public readonly float t;

		private static float tNow;

		public static readonly TimeStamp zero;

		public static readonly TimeStamp positiveInfinity;

		public static readonly TimeStamp negativeInfinity;

		public float timeUntil => t - tNow;

		public float timeSince => tNow - t;

		public float timeUntilClamped => Mathf.Max(timeUntil, 0f);

		public float timeSinceClamped => Mathf.Max(timeSince, 0f);

		public bool hasPassed => t <= tNow;

		public bool isInfinity => float.IsInfinity(t);

		public bool isPositiveInfinity => float.IsPositiveInfinity(t);

		public bool isNegativeInfinity => float.IsNegativeInfinity(t);

		public static TimeStamp now => new TimeStamp(tNow);

		public override int GetHashCode()
		{
			float num = t;
			return num.GetHashCode();
		}

		public static void Update()
		{
			tNow = instance.time;
		}

		private TimeStamp(float t)
		{
			this.t = t;
		}

		public bool Equals(TimeStamp other)
		{
			float num = t;
			return num.Equals(other.t);
		}

		public override bool Equals(object obj)
		{
			if (obj is TimeStamp)
			{
				return Equals((TimeStamp)obj);
			}
			return false;
		}

		public int CompareTo(TimeStamp other)
		{
			float num = t;
			return num.CompareTo(other.t);
		}

		public static TimeStamp operator +(TimeStamp a, float b)
		{
			return new TimeStamp(a.t + b);
		}

		public static TimeStamp operator -(TimeStamp a, float b)
		{
			return new TimeStamp(a.t - b);
		}

		public static float operator -(TimeStamp a, TimeStamp b)
		{
			return a.t - b.t;
		}

		public static bool operator <(TimeStamp a, TimeStamp b)
		{
			return a.t < b.t;
		}

		public static bool operator >(TimeStamp a, TimeStamp b)
		{
			return a.t > b.t;
		}

		public static bool operator <=(TimeStamp a, TimeStamp b)
		{
			return a.t <= b.t;
		}

		public static bool operator >=(TimeStamp a, TimeStamp b)
		{
			return a.t >= b.t;
		}

		public static bool operator ==(TimeStamp a, TimeStamp b)
		{
			return a.Equals(b);
		}

		public static bool operator !=(TimeStamp a, TimeStamp b)
		{
			return !a.Equals(b);
		}

		public static float operator -(TimeStamp a, FixedTimeStamp b)
		{
			return a.t - b.t;
		}

		public static TimeStamp Deserialize(NetworkReader reader)
		{
			return new TimeStamp(reader.ReadSingle());
		}

		public static void Serialize(NetworkWriter writer, TimeStamp timeStamp)
		{
			writer.Write(timeStamp.t);
		}

		public static void ToXml(XElement element, TimeStamp src)
		{
			element.Value = TextSerialization.ToStringInvariant(src.t);
		}

		public static bool FromXml(XElement element, ref TimeStamp dest)
		{
			if (TextSerialization.TryParseInvariant(element.Value, out float result))
			{
				dest = new TimeStamp(result);
				return true;
			}
			return false;
		}

		static TimeStamp()
		{
			zero = new TimeStamp(0f);
			positiveInfinity = new TimeStamp(float.PositiveInfinity);
			negativeInfinity = new TimeStamp(float.NegativeInfinity);
			HGXml.Register<TimeStamp>(ToXml, FromXml);
		}
	}

	[Serializable]
	public struct FixedTimeStamp : IEquatable<FixedTimeStamp>, IComparable<FixedTimeStamp>
	{
		public readonly float t;

		private static float tNow;

		public static readonly FixedTimeStamp zero;

		public static readonly FixedTimeStamp positiveInfinity;

		public static readonly FixedTimeStamp negativeInfinity;

		public float timeUntil => t - tNow;

		public float timeSince => tNow - t;

		public float timeUntilClamped => Mathf.Max(timeUntil, 0f);

		public float timeSinceClamped => Mathf.Max(timeSince, 0f);

		public bool hasPassed => t <= tNow;

		public bool isInfinity => float.IsInfinity(t);

		public bool isPositiveInfinity => float.IsPositiveInfinity(t);

		public bool isNegativeInfinity => float.IsNegativeInfinity(t);

		public static FixedTimeStamp now => new FixedTimeStamp(tNow);

		public override int GetHashCode()
		{
			float num = t;
			return num.GetHashCode();
		}

		public static void Update()
		{
			tNow = instance.fixedTime;
		}

		private FixedTimeStamp(float t)
		{
			this.t = t;
		}

		public bool Equals(FixedTimeStamp other)
		{
			float num = t;
			return num.Equals(other.t);
		}

		public override bool Equals(object obj)
		{
			if (obj is FixedTimeStamp)
			{
				return Equals((FixedTimeStamp)obj);
			}
			return false;
		}

		public int CompareTo(FixedTimeStamp other)
		{
			float num = t;
			return num.CompareTo(other.t);
		}

		public static FixedTimeStamp operator +(FixedTimeStamp a, float b)
		{
			return new FixedTimeStamp(a.t + b);
		}

		public static FixedTimeStamp operator -(FixedTimeStamp a, float b)
		{
			return new FixedTimeStamp(a.t - b);
		}

		public static float operator -(FixedTimeStamp a, FixedTimeStamp b)
		{
			return a.t - b.t;
		}

		public static bool operator <(FixedTimeStamp a, FixedTimeStamp b)
		{
			return a.t < b.t;
		}

		public static bool operator >(FixedTimeStamp a, FixedTimeStamp b)
		{
			return a.t > b.t;
		}

		public static bool operator <=(FixedTimeStamp a, FixedTimeStamp b)
		{
			return a.t <= b.t;
		}

		public static bool operator >=(FixedTimeStamp a, FixedTimeStamp b)
		{
			return a.t >= b.t;
		}

		public static bool operator ==(FixedTimeStamp a, FixedTimeStamp b)
		{
			return a.Equals(b);
		}

		public static bool operator !=(FixedTimeStamp a, FixedTimeStamp b)
		{
			return !a.Equals(b);
		}

		public static FixedTimeStamp Deserialize(NetworkReader reader)
		{
			return new FixedTimeStamp(reader.ReadSingle());
		}

		public static void Serialize(NetworkWriter writer, FixedTimeStamp timeStamp)
		{
			writer.Write(timeStamp.t);
		}

		public static void ToXml(XElement element, FixedTimeStamp src)
		{
			element.Value = TextSerialization.ToStringInvariant(src.t);
		}

		public static bool FromXml(XElement element, ref FixedTimeStamp dest)
		{
			if (TextSerialization.TryParseInvariant(element.Value, out float result))
			{
				dest = new FixedTimeStamp(result);
				return true;
			}
			return false;
		}

		static FixedTimeStamp()
		{
			zero = new FixedTimeStamp(0f);
			positiveInfinity = new FixedTimeStamp(float.PositiveInfinity);
			negativeInfinity = new FixedTimeStamp(float.NegativeInfinity);
			HGXml.Register<FixedTimeStamp>(ToXml, FromXml);
		}
	}

	private NetworkRuleBook networkRuleBookComponent;

	[Tooltip("This is assigned to the prefab automatically by GameModeCatalog at runtime. Do not set this value manually.")]
	[HideInInspector]
	public GameModeIndex gameModeIndex = GameModeIndex.Invalid;

	public string nameToken = "";

	[Tooltip("Whether or not the user can select this game mode for play in the game mode selector UI.")]
	public bool userPickable = true;

	public static int stagesPerLoop = 5;

	public static float baseGravity = -30f;

	[SyncVar]
	private NetworkGuid _uniqueId;

	[SyncVar]
	private NetworkDateTime startTimeUtc;

	[Tooltip("The pool of scenes to select the first scene of the run from.")]
	[ShowFieldObsolete]
	[Obsolete("Use startingSceneGroup instead.")]
	public SceneDef[] startingScenes = Array.Empty<SceneDef>();

	[Tooltip("The pool of scenes to select the first scene of the run from.")]
	public SceneCollection startingSceneGroup;

	public ItemMask availableItems;

	public ItemMask expansionLockedItems;

	public EquipmentMask availableEquipment;

	public EquipmentMask expansionLockedEquipment;

	[SyncVar]
	public float fixedTime;

	public float time;

	[SyncVar]
	private RunStopwatch runStopwatch;

	[SyncVar]
	public int stageClearCount;

	public SceneDef nextStageScene;

	public GameObject gameOverPrefab;

	public GameObject lobbyBackgroundPrefab;

	public GameObject uiPrefab;

	private ulong _seed;

	public Xoroshiro128Plus runRNG;

	public Xoroshiro128Plus nextStageRng;

	public Xoroshiro128Plus stageRngGenerator;

	public Xoroshiro128Plus stageRng;

	public Xoroshiro128Plus bossRewardRng;

	public Xoroshiro128Plus treasureRng;

	public Xoroshiro128Plus spawnRng;

	public Xoroshiro128Plus randomSurvivorOnRespawnRng;

	public float difficultyCoefficient = 1f;

	public float compensatedDifficultyCoefficient = 1f;

	[SyncVar]
	private int selectedDifficultyInternal = 1;

	public int shopPortalCount;

	private static int ambientLevelCap = 99;

	private static readonly StringConVar cvRunSceneOverride = new StringConVar("run_scene_override", ConVarFlags.Cheat, "", "Overrides the first scene to enter in a run.");

	private readonly HashSet<UnlockableDef> unlockablesUnlockedByAnyUser = new HashSet<UnlockableDef>();

	private readonly HashSet<UnlockableDef> unlockablesUnlockedByAllUsers = new HashSet<UnlockableDef>();

	private readonly HashSet<UnlockableDef> unlockablesAlreadyFullyObtained = new HashSet<UnlockableDef>();

	private bool shutdown;

	private Dictionary<NetworkUserId, CharacterMaster> userMasters = new Dictionary<NetworkUserId, CharacterMaster>();

	private bool allowNewParticipants;

	public readonly List<PickupIndex> availableTier1DropList = new List<PickupIndex>();

	public readonly List<PickupIndex> availableTier2DropList = new List<PickupIndex>();

	public readonly List<PickupIndex> availableTier3DropList = new List<PickupIndex>();

	public readonly List<PickupIndex> availableEquipmentDropList = new List<PickupIndex>();

	public readonly List<PickupIndex> availableLunarEquipmentDropList = new List<PickupIndex>();

	public readonly List<PickupIndex> availableLunarItemDropList = new List<PickupIndex>();

	public readonly List<PickupIndex> availableLunarCombinedDropList = new List<PickupIndex>();

	public readonly List<PickupIndex> availableBossDropList = new List<PickupIndex>();

	public readonly List<PickupIndex> availableVoidTier1DropList = new List<PickupIndex>();

	public readonly List<PickupIndex> availableVoidTier2DropList = new List<PickupIndex>();

	public readonly List<PickupIndex> availableVoidTier3DropList = new List<PickupIndex>();

	public readonly List<PickupIndex> availableVoidBossDropList = new List<PickupIndex>();

	public WeightedSelection<List<PickupIndex>> smallChestDropTierSelector = new WeightedSelection<List<PickupIndex>>();

	public WeightedSelection<List<PickupIndex>> mediumChestDropTierSelector = new WeightedSelection<List<PickupIndex>>();

	public WeightedSelection<List<PickupIndex>> largeChestDropTierSelector = new WeightedSelection<List<PickupIndex>>();

	private readonly HashSet<string> eventFlags = new HashSet<string>();

	public static Run instance { get; private set; }

	public RuleBook ruleBook => networkRuleBookComponent.ruleBook;

	public bool isRunStopwatchPaused => runStopwatch.isPaused;

	public virtual int loopClearCount => stageClearCount / stagesPerLoop;

	public virtual bool spawnWithPod => instance.stageClearCount == 0;

	public virtual bool autoGenerateSpawnPoints => true;

	public virtual bool canFamilyEventTrigger => true;

	public GameObject uiInstance { get; protected set; }

	public ulong seed
	{
		get
		{
			return _seed;
		}
		set
		{
			_seed = value;
			OnSeedSet();
		}
	}

	public DifficultyIndex selectedDifficulty
	{
		get
		{
			return (DifficultyIndex)selectedDifficultyInternal;
		}
		set
		{
			NetworkselectedDifficultyInternal = (int)value;
		}
	}

	public int livingPlayerCount => PlayerCharacterMasterController.GetPlayersWithBodiesCount();

	public int participatingPlayerCount => Math.Max(PlayerCharacterMasterController.instances.Count, PlatformSystems.lobbyManager.calculatedTotalPlayerCount);

	public float ambientLevel { get; protected set; }

	public int ambientLevelFloor { get; protected set; }

	public float teamlessDamageCoefficient => difficultyCoefficient;

	public bool isGameOverServer { get; private set; }

	public NetworkGuid Network_uniqueId
	{
		get
		{
			return _uniqueId;
		}
		[param: In]
		set
		{
			((NetworkBehaviour)this).SetSyncVar<NetworkGuid>(value, ref _uniqueId, 1u);
		}
	}

	public NetworkDateTime NetworkstartTimeUtc
	{
		get
		{
			return startTimeUtc;
		}
		[param: In]
		set
		{
			((NetworkBehaviour)this).SetSyncVar<NetworkDateTime>(value, ref startTimeUtc, 2u);
		}
	}

	public float NetworkfixedTime
	{
		get
		{
			return fixedTime;
		}
		[param: In]
		set
		{
			((NetworkBehaviour)this).SetSyncVar<float>(value, ref fixedTime, 4u);
		}
	}

	public RunStopwatch NetworkrunStopwatch
	{
		get
		{
			return runStopwatch;
		}
		[param: In]
		set
		{
			((NetworkBehaviour)this).SetSyncVar<RunStopwatch>(value, ref runStopwatch, 8u);
		}
	}

	public int NetworkstageClearCount
	{
		get
		{
			return stageClearCount;
		}
		[param: In]
		set
		{
			((NetworkBehaviour)this).SetSyncVar<int>(value, ref stageClearCount, 16u);
		}
	}

	public int NetworkselectedDifficultyInternal
	{
		get
		{
			return selectedDifficultyInternal;
		}
		[param: In]
		set
		{
			((NetworkBehaviour)this).SetSyncVar<int>(value, ref selectedDifficultyInternal, 32u);
		}
	}

	public static event Action<Run, RuleBook> onServerRunSetRuleBookGlobal;

	public static event Action<Run, RuleBook> onRunSetRuleBookGlobal;

	public static event Action<Run> onRunAmbientLevelUp;

	public static event Action<Run, PlayerCharacterMasterController> onPlayerFirstCreatedServer;

	public static event Action<Run, GameEndingDef> onServerGameOver;

	public static event Action<Run, RunReport> onClientGameOverGlobal;

	public static event Action<Run> onRunStartGlobal;

	public static event Action<Run> onRunDestroyGlobal;

	public static event Action<Run> onAvailablePickupsModified;

	protected virtual void OnEnable()
	{
		instance = SingletonHelper.Assign<Run>(instance, this);
	}

	protected virtual void OnDisable()
	{
		instance = SingletonHelper.Unassign<Run>(instance, this);
	}

	protected void Awake()
	{
		networkRuleBookComponent = ((Component)this).GetComponent<NetworkRuleBook>();
		networkRuleBookComponent.onRuleBookUpdated += OnRuleBookUpdated;
		availableItems = ItemMask.Rent();
		expansionLockedItems = ItemMask.Rent();
		availableEquipment = EquipmentMask.Rent();
		expansionLockedEquipment = EquipmentMask.Rent();
		if (NetworkServer.active)
		{
			Network_uniqueId = (NetworkGuid)Guid.NewGuid();
			NetworkstartTimeUtc = (NetworkDateTime)DateTime.UtcNow;
		}
	}

	[Server]
	public void SetRuleBook(RuleBook newRuleBook)
	{
		if (!NetworkServer.active)
		{
			Debug.LogWarning((object)"[Server] function 'System.Void RoR2.Run::SetRuleBook(RoR2.RuleBook)' called on client");
		}
		else
		{
			networkRuleBookComponent.SetRuleBook(newRuleBook);
		}
	}

	private void OnRuleBookUpdated(NetworkRuleBook networkRuleBookComponent)
	{
		//IL_0036: Unknown result type (might be due to invalid IL or missing references)
		//IL_003b: Unknown result type (might be due to invalid IL or missing references)
		//IL_003e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0043: Unknown result type (might be due to invalid IL or missing references)
		RuleBook ruleBook = networkRuleBookComponent.ruleBook;
		selectedDifficulty = ruleBook.FindDifficulty();
		ruleBook.GenerateItemMask(availableItems);
		ruleBook.GenerateEquipmentMask(availableEquipment);
		expansionLockedItems.Clear();
		Enumerator<ItemDef> enumerator = ItemCatalog.allItemDefs.GetEnumerator();
		try
		{
			while (enumerator.MoveNext())
			{
				ItemDef current = enumerator.Current;
				if (Object.op_Implicit((Object)(object)current) && Object.op_Implicit((Object)(object)current.requiredExpansion) && !IsExpansionEnabled(current.requiredExpansion))
				{
					expansionLockedItems.Add(current.itemIndex);
				}
			}
		}
		finally
		{
			((IDisposable)enumerator).Dispose();
		}
		expansionLockedEquipment.Clear();
		foreach (EquipmentIndex item in EquipmentCatalog.allEquipment)
		{
			EquipmentDef equipmentDef = EquipmentCatalog.GetEquipmentDef(item);
			if (Object.op_Implicit((Object)(object)equipmentDef) && Object.op_Implicit((Object)(object)equipmentDef.requiredExpansion) && !IsExpansionEnabled(equipmentDef.requiredExpansion))
			{
				expansionLockedEquipment.Add(equipmentDef.equipmentIndex);
			}
		}
		if (NetworkServer.active)
		{
			Run.onServerRunSetRuleBookGlobal?.Invoke(this, ruleBook);
		}
		Run.onRunSetRuleBookGlobal?.Invoke(this, ruleBook);
	}

	public Guid GetUniqueId()
	{
		return (Guid)_uniqueId;
	}

	public DateTime GetStartTimeUtc()
	{
		return (DateTime)startTimeUtc;
	}

	[Server]
	private void SetRunStopwatchPaused(bool isPaused)
	{
		if (!NetworkServer.active)
		{
			Debug.LogWarning((object)"[Server] function 'System.Void RoR2.Run::SetRunStopwatchPaused(System.Boolean)' called on client");
		}
		else if (isPaused != runStopwatch.isPaused)
		{
			RunStopwatch networkrunStopwatch = runStopwatch;
			networkrunStopwatch.isPaused = isPaused;
			float num = GetRunStopwatch();
			if (isPaused)
			{
				networkrunStopwatch.offsetFromFixedTime = num;
			}
			else
			{
				networkrunStopwatch.offsetFromFixedTime = num - fixedTime;
			}
			NetworkrunStopwatch = networkrunStopwatch;
		}
	}

	public float GetRunStopwatch()
	{
		if (runStopwatch.isPaused)
		{
			return runStopwatch.offsetFromFixedTime;
		}
		return fixedTime + runStopwatch.offsetFromFixedTime;
	}

	[Server]
	public void SetRunStopwatch(float t)
	{
		if (!NetworkServer.active)
		{
			Debug.LogWarning((object)"[Server] function 'System.Void RoR2.Run::SetRunStopwatch(System.Single)' called on client");
			return;
		}
		RunStopwatch networkrunStopwatch = runStopwatch;
		if (networkrunStopwatch.isPaused)
		{
			networkrunStopwatch.offsetFromFixedTime = t;
		}
		else
		{
			networkrunStopwatch.offsetFromFixedTime = t - fixedTime;
		}
		NetworkrunStopwatch = networkrunStopwatch;
	}

	public virtual GameObject InstantiateUi(Transform uiRoot)
	{
		if (Object.op_Implicit((Object)(object)uiRoot) && Object.op_Implicit((Object)(object)uiPrefab))
		{
			uiInstance = Object.Instantiate<GameObject>(uiPrefab, uiRoot);
		}
		return uiInstance;
	}

	private void GenerateStageRNG()
	{
		//IL_000c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0016: Expected O, but got Unknown
		//IL_0022: Unknown result type (might be due to invalid IL or missing references)
		//IL_002c: Expected O, but got Unknown
		//IL_0038: Unknown result type (might be due to invalid IL or missing references)
		//IL_0042: Expected O, but got Unknown
		//IL_004e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0058: Expected O, but got Unknown
		//IL_0064: Unknown result type (might be due to invalid IL or missing references)
		//IL_006e: Expected O, but got Unknown
		stageRng = new Xoroshiro128Plus(stageRngGenerator.nextUlong);
		bossRewardRng = new Xoroshiro128Plus(stageRng.nextUlong);
		treasureRng = new Xoroshiro128Plus(stageRng.nextUlong);
		spawnRng = new Xoroshiro128Plus(stageRng.nextUlong);
		randomSurvivorOnRespawnRng = new Xoroshiro128Plus(stageRng.nextUlong);
	}

	protected void OnAmbientLevelUp()
	{
		Run.onRunAmbientLevelUp?.Invoke(this);
	}

	protected virtual void FixedUpdate()
	{
		NetworkfixedTime = fixedTime + Time.fixedDeltaTime;
		FixedTimeStamp.Update();
		if (NetworkServer.active)
		{
			SetRunStopwatchPaused(!ShouldUpdateRunStopwatch());
		}
		OnFixedUpdate();
	}

	public void RecalculateDifficultyCoefficent()
	{
		RecalculateDifficultyCoefficentInternal();
	}

	protected virtual void RecalculateDifficultyCoefficentInternal()
	{
		float num = GetRunStopwatch();
		DifficultyDef difficultyDef = DifficultyCatalog.GetDifficultyDef(selectedDifficulty);
		float num2 = Mathf.Floor(num * (1f / 60f));
		float num3 = (float)participatingPlayerCount * 0.3f;
		float num4 = 0.7f + num3;
		float num5 = 0.7f + num3;
		float num6 = Mathf.Pow((float)participatingPlayerCount, 0.2f);
		float num7 = 0.0506f * difficultyDef.scalingValue * num6;
		float num8 = 0.0506f * difficultyDef.scalingValue * num6;
		float num9 = Mathf.Pow(1.15f, (float)stageClearCount);
		compensatedDifficultyCoefficient = (num5 + num8 * num2) * num9;
		difficultyCoefficient = (num4 + num7 * num2) * num9;
		float num10 = (num4 + num7 * (num * (1f / 60f))) * Mathf.Pow(1.15f, (float)stageClearCount);
		ambientLevel = Mathf.Min((num10 - num4) / 0.33f + 1f, (float)ambientLevelCap);
		int num11 = ambientLevelFloor;
		ambientLevelFloor = Mathf.FloorToInt(ambientLevel);
		if (num11 != ambientLevelFloor && num11 != 0 && ambientLevelFloor > num11)
		{
			OnAmbientLevelUp();
		}
	}

	protected virtual void OnFixedUpdate()
	{
		RecalculateDifficultyCoefficent();
	}

	protected void Update()
	{
		time = Mathf.Clamp(time + Time.deltaTime, fixedTime, fixedTime + Time.fixedDeltaTime);
		TimeStamp.Update();
	}

	protected virtual bool ShouldUpdateRunStopwatch()
	{
		if (SceneCatalog.mostRecentSceneDef.sceneType != SceneType.Stage)
		{
			return false;
		}
		return livingPlayerCount > 0;
	}

	[Server]
	[Obsolete("Use the overload that accepts an UnlockableDef instead. This method may be removed from future releases.", false)]
	public bool CanUnlockableBeGrantedThisRun(string unlockableName)
	{
		if (!NetworkServer.active)
		{
			Debug.LogWarning((object)"[Server] function 'System.Boolean RoR2.Run::CanUnlockableBeGrantedThisRun(System.String)' called on client");
			return false;
		}
		return CanUnlockableBeGrantedThisRun(UnlockableCatalog.GetUnlockableDef(unlockableName));
	}

	[Server]
	public virtual bool CanUnlockableBeGrantedThisRun(UnlockableDef unlockableDef)
	{
		if (!NetworkServer.active)
		{
			Debug.LogWarning((object)"[Server] function 'System.Boolean RoR2.Run::CanUnlockableBeGrantedThisRun(RoR2.UnlockableDef)' called on client");
			return false;
		}
		return !unlockablesAlreadyFullyObtained.Contains(unlockableDef);
	}

	[Server]
	[Obsolete("Use the overload that accepts an UnlockableDef instead. This method may be removed from future releases.", false)]
	public void GrantUnlockToAllParticipatingPlayers(string unlockableName)
	{
		if (!NetworkServer.active)
		{
			Debug.LogWarning((object)"[Server] function 'System.Void RoR2.Run::GrantUnlockToAllParticipatingPlayers(System.String)' called on client");
		}
		else
		{
			GrantUnlockToAllParticipatingPlayers(UnlockableCatalog.GetUnlockableDef(unlockableName));
		}
	}

	[Server]
	public void GrantUnlockToAllParticipatingPlayers(UnlockableDef unlockableDef)
	{
		if (!NetworkServer.active)
		{
			Debug.LogWarning((object)"[Server] function 'System.Void RoR2.Run::GrantUnlockToAllParticipatingPlayers(RoR2.UnlockableDef)' called on client");
		}
		else
		{
			if (!Object.op_Implicit((Object)(object)unlockableDef) || unlockableDef.index == UnlockableIndex.None || unlockablesAlreadyFullyObtained.Contains(unlockableDef))
			{
				return;
			}
			unlockablesAlreadyFullyObtained.Add(unlockableDef);
			foreach (NetworkUser readOnlyInstances in NetworkUser.readOnlyInstancesList)
			{
				if (readOnlyInstances.isParticipating)
				{
					readOnlyInstances.ServerHandleUnlock(unlockableDef);
				}
			}
		}
	}

	[Server]
	[Obsolete("Use the overload that accepts an UnlockableDef instead. This method may be removed from future releases.", false)]
	public void GrantUnlockToSinglePlayer(string unlockableName, CharacterBody body)
	{
		if (!NetworkServer.active)
		{
			Debug.LogWarning((object)"[Server] function 'System.Void RoR2.Run::GrantUnlockToSinglePlayer(System.String,RoR2.CharacterBody)' called on client");
		}
		else
		{
			GrantUnlockToSinglePlayer(UnlockableCatalog.GetUnlockableDef(unlockableName), body);
		}
	}

	[Server]
	public void GrantUnlockToSinglePlayer(UnlockableDef unlockableDef, CharacterBody body)
	{
		if (!NetworkServer.active)
		{
			Debug.LogWarning((object)"[Server] function 'System.Void RoR2.Run::GrantUnlockToSinglePlayer(RoR2.UnlockableDef,RoR2.CharacterBody)' called on client");
		}
		else if (Object.op_Implicit((Object)(object)unlockableDef) && unlockableDef.index != UnlockableIndex.None && Object.op_Implicit((Object)(object)body))
		{
			NetworkUser networkUser = Util.LookUpBodyNetworkUser(body);
			if (Object.op_Implicit((Object)(object)networkUser))
			{
				networkUser.ServerHandleUnlock(unlockableDef);
			}
		}
	}

	[Obsolete("Use the overload that accepts an UnlockableDef instead. This method may be removed from future releases.", false)]
	[Server]
	public bool IsUnlockableUnlocked(string unlockableName)
	{
		if (!NetworkServer.active)
		{
			Debug.LogWarning((object)"[Server] function 'System.Boolean RoR2.Run::IsUnlockableUnlocked(System.String)' called on client");
			return false;
		}
		return IsUnlockableUnlocked(UnlockableCatalog.GetUnlockableDef(unlockableName));
	}

	[Server]
	public virtual bool IsUnlockableUnlocked(UnlockableDef unlockableDef)
	{
		if (!NetworkServer.active)
		{
			Debug.LogWarning((object)"[Server] function 'System.Boolean RoR2.Run::IsUnlockableUnlocked(RoR2.UnlockableDef)' called on client");
			return false;
		}
		return unlockablesUnlockedByAnyUser.Contains(unlockableDef);
	}

	[Server]
	[Obsolete("Use the overload that accepts an UnlockableDef instead. This method may be removed from future releases.", false)]
	public bool DoesEveryoneHaveThisUnlockableUnlocked(string unlockableName)
	{
		if (!NetworkServer.active)
		{
			Debug.LogWarning((object)"[Server] function 'System.Boolean RoR2.Run::DoesEveryoneHaveThisUnlockableUnlocked(System.String)' called on client");
			return false;
		}
		return DoesEveryoneHaveThisUnlockableUnlocked(UnlockableCatalog.GetUnlockableDef(unlockableName));
	}

	[Server]
	public virtual bool DoesEveryoneHaveThisUnlockableUnlocked(UnlockableDef unlockableDef)
	{
		if (!NetworkServer.active)
		{
			Debug.LogWarning((object)"[Server] function 'System.Boolean RoR2.Run::DoesEveryoneHaveThisUnlockableUnlocked(RoR2.UnlockableDef)' called on client");
			return false;
		}
		return unlockablesUnlockedByAllUsers.Contains(unlockableDef);
	}

	[Obsolete("Use the overload that accepts an UnlockableDef instead. This method may be removed from future releases.", false)]
	[Server]
	public void ForceUnlockImmediate(string unlockableName)
	{
		if (!NetworkServer.active)
		{
			Debug.LogWarning((object)"[Server] function 'System.Void RoR2.Run::ForceUnlockImmediate(System.String)' called on client");
		}
		else
		{
			ForceUnlockImmediate(UnlockableCatalog.GetUnlockableDef(unlockableName));
		}
	}

	[Server]
	public void ForceUnlockImmediate(UnlockableDef unlockableDef)
	{
		if (!NetworkServer.active)
		{
			Debug.LogWarning((object)"[Server] function 'System.Void RoR2.Run::ForceUnlockImmediate(RoR2.UnlockableDef)' called on client");
		}
		else
		{
			unlockablesUnlockedByAnyUser.Add(unlockableDef);
		}
	}

	public void PickNextStageSceneFromCurrentSceneDestinations()
	{
		WeightedSelection<SceneDef> weightedSelection = new WeightedSelection<SceneDef>();
		SceneCatalog.mostRecentSceneDef.AddDestinationsToWeightedSelection(weightedSelection, CanPickStage);
		PickNextStageScene(weightedSelection);
	}

	public bool CanPickStage(SceneDef sceneDef)
	{
		if (Object.op_Implicit((Object)(object)sceneDef.requiredExpansion))
		{
			return IsExpansionEnabled(sceneDef.requiredExpansion);
		}
		return true;
	}

	public void PickNextStageScene(WeightedSelection<SceneDef> choices)
	{
		//IL_002e: Unknown result type (might be due to invalid IL or missing references)
		if (choices.Count != 0)
		{
			if (ruleBook.stageOrder == StageOrder.Normal)
			{
				nextStageScene = choices.Evaluate(nextStageRng.nextNormalizedFloat);
				return;
			}
			SceneDef[] array = ((IEnumerable<SceneDef>)(object)SceneCatalog.allStageSceneDefs).Where(IsValidNextStage).ToArray();
			nextStageScene = nextStageRng.NextElementUniform<SceneDef>(array);
		}
		bool IsValidNextStage(SceneDef sceneDef)
		{
			if ((Object)(object)nextStageScene != (Object)null && nextStageScene.baseSceneName == sceneDef.baseSceneName)
			{
				return false;
			}
			if (!sceneDef.hasAnyDestinations)
			{
				return false;
			}
			return sceneDef.validForRandomSelection;
		}
	}

	public virtual ulong GenerateSeedForNewRun()
	{
		return RoR2Application.rng.nextUlong;
	}

	protected virtual void BuildUnlockAvailability()
	{
		unlockablesUnlockedByAnyUser.Clear();
		unlockablesUnlockedByAllUsers.Clear();
		unlockablesAlreadyFullyObtained.Clear();
		int num = 0;
		Dictionary<UnlockableDef, int> dictionary = new Dictionary<UnlockableDef, int>();
		foreach (NetworkUser readOnlyInstances in NetworkUser.readOnlyInstancesList)
		{
			if (!readOnlyInstances.isParticipating)
			{
				continue;
			}
			num++;
			foreach (UnlockableDef unlockable in readOnlyInstances.unlockables)
			{
				unlockablesUnlockedByAnyUser.Add(unlockable);
				if (!dictionary.ContainsKey(unlockable))
				{
					dictionary.Add(unlockable, 0);
				}
				UnlockableDef key = unlockable;
				int value = dictionary[key] + 1;
				dictionary[key] = value;
			}
		}
		if (num <= 0)
		{
			return;
		}
		foreach (KeyValuePair<UnlockableDef, int> item in dictionary)
		{
			if (item.Value == num)
			{
				unlockablesUnlockedByAllUsers.Add(item.Key);
				unlockablesAlreadyFullyObtained.Add(item.Key);
			}
		}
	}

	protected virtual void Start()
	{
		//IL_001a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0024: Expected O, but got Unknown
		//IL_0030: Unknown result type (might be due to invalid IL or missing references)
		//IL_003a: Expected O, but got Unknown
		//IL_0046: Unknown result type (might be due to invalid IL or missing references)
		//IL_0050: Expected O, but got Unknown
		OnRuleBookUpdated(networkRuleBookComponent);
		if (NetworkServer.active)
		{
			runRNG = new Xoroshiro128Plus(seed);
			nextStageRng = new Xoroshiro128Plus(runRNG.nextUlong);
			stageRngGenerator = new Xoroshiro128Plus(runRNG.nextUlong);
			GenerateStageRNG();
		}
		allowNewParticipants = true;
		Object.DontDestroyOnLoad((Object)(object)((Component)this).gameObject);
		ReadOnlyCollection<NetworkUser> readOnlyInstancesList = NetworkUser.readOnlyInstancesList;
		for (int i = 0; i < readOnlyInstancesList.Count; i++)
		{
			OnUserAdded(readOnlyInstancesList[i]);
		}
		allowNewParticipants = false;
		if (NetworkServer.active)
		{
			WeightedSelection<SceneDef> weightedSelection = new WeightedSelection<SceneDef>();
			string @string = cvRunSceneOverride.GetString();
			if (@string != "")
			{
				weightedSelection.AddChoice(SceneCatalog.GetSceneDefFromSceneName(@string), 1f);
			}
			else if (Object.op_Implicit((Object)(object)startingSceneGroup))
			{
				startingSceneGroup.AddToWeightedSelection(weightedSelection, CanPickStage);
			}
			else
			{
				for (int j = 0; j < startingScenes.Length; j++)
				{
					if (CanPickStage(startingScenes[j]))
					{
						weightedSelection.AddChoice(startingScenes[j], 1f);
					}
				}
			}
			PickNextStageScene(weightedSelection);
			if (!Object.op_Implicit((Object)(object)nextStageScene))
			{
				Debug.LogError((object)"Cannot set next scene. nextStageScene is null!");
			}
			NetworkManager.singleton.ServerChangeScene(nextStageScene.cachedName);
		}
		BuildUnlockAvailability();
		BuildDropTable();
		Run.onRunStartGlobal?.Invoke(this);
	}

	protected virtual void OnDestroy()
	{
		Run.onRunDestroyGlobal?.Invoke(this);
		if (Object.op_Implicit((Object)(object)GameOverController.instance))
		{
			Object.Destroy((Object)(object)((Component)GameOverController.instance).gameObject);
		}
		ReadOnlyCollection<CharacterBody> readOnlyInstancesList = CharacterBody.readOnlyInstancesList;
		for (int num = readOnlyInstancesList.Count - 1; num >= 0; num--)
		{
			if (Object.op_Implicit((Object)(object)readOnlyInstancesList[num]))
			{
				Object.Destroy((Object)(object)((Component)readOnlyInstancesList[num]).gameObject);
			}
		}
		ReadOnlyCollection<CharacterMaster> readOnlyInstancesList2 = CharacterMaster.readOnlyInstancesList;
		for (int num2 = readOnlyInstancesList2.Count - 1; num2 >= 0; num2--)
		{
			if (Object.op_Implicit((Object)(object)readOnlyInstancesList2[num2]))
			{
				Object.Destroy((Object)(object)((Component)readOnlyInstancesList2[num2]).gameObject);
			}
		}
		if (Object.op_Implicit((Object)(object)Stage.instance))
		{
			Object.Destroy((Object)(object)((Component)Stage.instance).gameObject);
		}
		Chat.Clear();
		if (!shutdown && ((NetworkManager)PlatformSystems.networkManager).isNetworkActive)
		{
			HandlePostRunDestination();
		}
		ItemMask.Return(availableItems);
		ItemMask.Return(expansionLockedItems);
		EquipmentMask.Return(availableEquipment);
		EquipmentMask.Return(expansionLockedEquipment);
	}

	protected virtual void HandlePostRunDestination()
	{
		if (NetworkServer.active)
		{
			NetworkManager.singleton.ServerChangeScene("lobby");
		}
	}

	protected void OnApplicationQuit()
	{
		shutdown = true;
	}

	[Server]
	public CharacterMaster GetUserMaster(NetworkUserId networkUserId)
	{
		if (!NetworkServer.active)
		{
			Debug.LogWarning((object)"[Server] function 'RoR2.CharacterMaster RoR2.Run::GetUserMaster(RoR2.NetworkUserId)' called on client");
			return null;
		}
		userMasters.TryGetValue(networkUserId, out var value);
		return value;
	}

	[Server]
	public void OnServerSceneChanged(string sceneName)
	{
		if (!NetworkServer.active)
		{
			Debug.LogWarning((object)"[Server] function 'System.Void RoR2.Run::OnServerSceneChanged(System.String)' called on client");
			return;
		}
		BeginStage();
		isGameOverServer = false;
	}

	[Server]
	private void BeginStage()
	{
		if (!NetworkServer.active)
		{
			Debug.LogWarning((object)"[Server] function 'System.Void RoR2.Run::BeginStage()' called on client");
		}
		else
		{
			NetworkServer.Spawn(Object.Instantiate<GameObject>(LegacyResourcesAPI.Load<GameObject>("Prefabs/Stage")));
		}
	}

	[Server]
	private void EndStage()
	{
		if (!NetworkServer.active)
		{
			Debug.LogWarning((object)"[Server] function 'System.Void RoR2.Run::EndStage()' called on client");
		}
		else if (Object.op_Implicit((Object)(object)Stage.instance))
		{
			Object.Destroy((Object)(object)Stage.instance);
		}
	}

	public void OnUserAdded(NetworkUser user)
	{
		if (NetworkServer.active)
		{
			SetupUserCharacterMaster(user);
		}
	}

	public void OnUserRemoved(NetworkUser user)
	{
	}

	public virtual bool ShouldAllowNewParticipant(NetworkUser user)
	{
		if (!allowNewParticipants)
		{
			return (bool)ruleBook.GetRuleChoice(RuleCatalog.FindRuleDef("Misc.AllowDropIn"))?.extraData;
		}
		return true;
	}

	[Server]
	private void SetupUserCharacterMaster(NetworkUser user)
	{
		//IL_0052: Unknown result type (might be due to invalid IL or missing references)
		//IL_0057: Unknown result type (might be due to invalid IL or missing references)
		if (!NetworkServer.active)
		{
			Debug.LogWarning((object)"[Server] function 'System.Void RoR2.Run::SetupUserCharacterMaster(RoR2.NetworkUser)' called on client");
		}
		else
		{
			if (Object.op_Implicit((Object)(object)user.masterObject))
			{
				return;
			}
			CharacterMaster characterMaster = GetUserMaster(user.id);
			int num;
			if (!Object.op_Implicit((Object)(object)characterMaster))
			{
				num = (ShouldAllowNewParticipant(user) ? 1 : 0);
				if (num != 0)
				{
					characterMaster = Object.Instantiate<GameObject>(LegacyResourcesAPI.Load<GameObject>("Prefabs/CharacterMasters/CommandoMaster"), Vector3.zero, Quaternion.identity).GetComponent<CharacterMaster>();
					userMasters[user.id] = characterMaster;
					characterMaster.GiveMoney(ruleBook.startingMoney);
					DifficultyDef difficultyDef = DifficultyCatalog.GetDifficultyDef(selectedDifficulty);
					if (selectedDifficulty == DifficultyIndex.Easy)
					{
						characterMaster.inventory.GiveItem(RoR2Content.Items.DrizzlePlayerHelper);
					}
					else if (difficultyDef.countsAsHardMode)
					{
						characterMaster.inventory.GiveItem(RoR2Content.Items.MonsoonPlayerHelper);
					}
					NetworkServer.Spawn(((Component)characterMaster).gameObject);
				}
			}
			else
			{
				num = 0;
			}
			PlayerCharacterMasterController component = ((Component)characterMaster).GetComponent<PlayerCharacterMasterController>();
			component.LinkToNetworkUserServer(user);
			if (num != 0)
			{
				Run.onPlayerFirstCreatedServer?.Invoke(this, component);
			}
		}
	}

	[Server]
	public virtual void HandlePlayerFirstEntryAnimation(CharacterBody body, Vector3 spawnPosition, Quaternion spawnRotation)
	{
		//IL_0039: Unknown result type (might be due to invalid IL or missing references)
		//IL_003e: Unknown result type (might be due to invalid IL or missing references)
		if (!NetworkServer.active)
		{
			Debug.LogWarning((object)"[Server] function 'System.Void RoR2.Run::HandlePlayerFirstEntryAnimation(RoR2.CharacterBody,UnityEngine.Vector3,UnityEngine.Quaternion)' called on client");
		}
		else if (Object.op_Implicit((Object)(object)body))
		{
			if (Object.op_Implicit((Object)(object)body.preferredPodPrefab))
			{
				GameObject val = Object.Instantiate<GameObject>(body.preferredPodPrefab, ((Component)body).transform.position, spawnRotation);
				VehicleSeat component = val.GetComponent<VehicleSeat>();
				if (Object.op_Implicit((Object)(object)component))
				{
					component.AssignPassenger(((Component)body).gameObject);
				}
				else
				{
					Debug.LogError((object)("Body " + ((Object)body).name + " has preferred pod " + ((Object)body.preferredPodPrefab).name + ", but that object has no VehicleSeat."));
				}
				NetworkServer.Spawn(val);
			}
			else
			{
				body.SetBodyStateToPreferredInitialState();
			}
		}
		else
		{
			Debug.LogError((object)"Can't handle player first entry animation for null body.");
		}
	}

	public virtual void OnServerBossAdded(BossGroup bossGroup, CharacterMaster characterMaster)
	{
	}

	public virtual void OnServerBossDefeated(BossGroup bossGroup)
	{
	}

	public virtual void OnServerCharacterBodySpawned(CharacterBody characterBody)
	{
	}

	public virtual void OnServerTeleporterPlaced(SceneDirector sceneDirector, GameObject teleporter)
	{
	}

	public virtual void OnPlayerSpawnPointsPlaced(SceneDirector sceneDirector)
	{
	}

	public virtual GameObject GetTeleportEffectPrefab(GameObject objectToTeleport)
	{
		return LegacyResourcesAPI.Load<GameObject>("Prefabs/Effects/TeleportOutBoom");
	}

	public int GetDifficultyScaledCost(int baseCost, float difficultyCoefficient)
	{
		return (int)((float)baseCost * Mathf.Pow(difficultyCoefficient, 1.25f));
	}

	public int GetDifficultyScaledCost(int baseCost)
	{
		return GetDifficultyScaledCost(baseCost, instance.difficultyCoefficient);
	}

	public void BuildDropTable()
	{
		availableTier1DropList.Clear();
		availableTier2DropList.Clear();
		availableTier3DropList.Clear();
		availableLunarItemDropList.Clear();
		availableEquipmentDropList.Clear();
		availableBossDropList.Clear();
		availableLunarEquipmentDropList.Clear();
		availableVoidTier1DropList.Clear();
		availableVoidTier2DropList.Clear();
		availableVoidTier3DropList.Clear();
		availableVoidBossDropList.Clear();
		ItemIndex itemIndex = (ItemIndex)0;
		for (ItemIndex itemCount = (ItemIndex)ItemCatalog.itemCount; itemIndex < itemCount; itemIndex++)
		{
			if (availableItems.Contains(itemIndex))
			{
				ItemDef itemDef = ItemCatalog.GetItemDef(itemIndex);
				List<PickupIndex> list = null;
				switch (itemDef.tier)
				{
				case ItemTier.Tier1:
					list = availableTier1DropList;
					break;
				case ItemTier.Tier2:
					list = availableTier2DropList;
					break;
				case ItemTier.Tier3:
					list = availableTier3DropList;
					break;
				case ItemTier.Lunar:
					list = availableLunarItemDropList;
					break;
				case ItemTier.Boss:
					list = availableBossDropList;
					break;
				case ItemTier.VoidTier1:
					list = availableVoidTier1DropList;
					break;
				case ItemTier.VoidTier2:
					list = availableVoidTier2DropList;
					break;
				case ItemTier.VoidTier3:
					list = availableVoidTier3DropList;
					break;
				case ItemTier.VoidBoss:
					list = availableVoidBossDropList;
					break;
				}
				if (list != null && itemDef.DoesNotContainTag(ItemTag.WorldUnique))
				{
					list.Add(PickupCatalog.FindPickupIndex(itemIndex));
				}
			}
		}
		EquipmentIndex equipmentIndex = (EquipmentIndex)0;
		for (EquipmentIndex equipmentCount = (EquipmentIndex)EquipmentCatalog.equipmentCount; equipmentIndex < equipmentCount; equipmentIndex++)
		{
			if (!availableEquipment.Contains(equipmentIndex))
			{
				continue;
			}
			EquipmentDef equipmentDef = EquipmentCatalog.GetEquipmentDef(equipmentIndex);
			if (equipmentDef.canDrop)
			{
				if (!equipmentDef.isLunar)
				{
					availableEquipmentDropList.Add(PickupCatalog.FindPickupIndex(equipmentIndex));
				}
				else
				{
					availableLunarEquipmentDropList.Add(PickupCatalog.FindPickupIndex(equipmentIndex));
				}
			}
		}
		smallChestDropTierSelector.Clear();
		smallChestDropTierSelector.AddChoice(availableTier1DropList, 0.8f);
		smallChestDropTierSelector.AddChoice(availableTier2DropList, 0.2f);
		smallChestDropTierSelector.AddChoice(availableTier3DropList, 0.01f);
		mediumChestDropTierSelector.Clear();
		mediumChestDropTierSelector.AddChoice(availableTier2DropList, 0.8f);
		mediumChestDropTierSelector.AddChoice(availableTier3DropList, 0.2f);
		largeChestDropTierSelector.Clear();
		RefreshLunarCombinedDropList();
	}

	public bool IsItemAvailable(ItemIndex itemIndex)
	{
		return availableItems.Contains(itemIndex);
	}

	public bool IsEquipmentAvailable(EquipmentIndex equipmentIndex)
	{
		return availableEquipment.Contains(equipmentIndex);
	}

	public bool IsItemExpansionLocked(ItemIndex itemIndex)
	{
		return expansionLockedItems.Contains(itemIndex);
	}

	public bool IsEquipmentExpansionLocked(EquipmentIndex equipmentIndex)
	{
		return expansionLockedEquipment.Contains(equipmentIndex);
	}

	public bool IsPickupAvailable(PickupIndex pickupIndex)
	{
		PickupDef pickupDef = PickupCatalog.GetPickupDef(pickupIndex);
		if (pickupDef.itemIndex != ItemIndex.None)
		{
			return IsItemAvailable(pickupDef.itemIndex);
		}
		if (pickupDef.equipmentIndex != EquipmentIndex.None)
		{
			return IsEquipmentAvailable(pickupDef.equipmentIndex);
		}
		return true;
	}

	[Server]
	public void DisablePickupDrop(PickupIndex pickupIndex)
	{
		if (!NetworkServer.active)
		{
			Debug.LogWarning((object)"[Server] function 'System.Void RoR2.Run::DisablePickupDrop(RoR2.PickupIndex)' called on client");
			return;
		}
		PickupDef pickupDef = PickupCatalog.GetPickupDef(pickupIndex);
		if (pickupDef.itemIndex != ItemIndex.None)
		{
			DisableItemDrop(pickupDef.itemIndex);
		}
		if (pickupDef.equipmentIndex != EquipmentIndex.None)
		{
			DisableEquipmentDrop(pickupDef.equipmentIndex);
		}
	}

	[Server]
	public void DisableItemDrop(ItemIndex itemIndex)
	{
		if (!NetworkServer.active)
		{
			Debug.LogWarning((object)"[Server] function 'System.Void RoR2.Run::DisableItemDrop(RoR2.ItemIndex)' called on client");
			return;
		}
		ItemDef itemDef = ItemCatalog.GetItemDef(itemIndex);
		List<PickupIndex> list = null;
		bool flag = false;
		switch (itemDef.tier)
		{
		case ItemTier.Tier1:
			list = availableTier1DropList;
			break;
		case ItemTier.Tier2:
			list = availableTier2DropList;
			break;
		case ItemTier.Tier3:
			list = availableTier3DropList;
			break;
		case ItemTier.Lunar:
			list = availableLunarItemDropList;
			break;
		case ItemTier.Boss:
			list = availableBossDropList;
			break;
		case ItemTier.VoidTier1:
			list = availableVoidTier1DropList;
			break;
		case ItemTier.VoidTier2:
			list = availableVoidTier2DropList;
			break;
		case ItemTier.VoidTier3:
			list = availableVoidTier3DropList;
			break;
		case ItemTier.VoidBoss:
			list = availableVoidBossDropList;
			break;
		}
		PickupIndex pickupIndex = PickupCatalog.FindPickupIndex(itemIndex);
		if (list != null && pickupIndex != PickupIndex.none)
		{
			list.Remove(pickupIndex);
			if (flag)
			{
				RefreshLunarCombinedDropList();
			}
			Run.onAvailablePickupsModified?.Invoke(this);
		}
	}

	[Server]
	public void DisableEquipmentDrop(EquipmentIndex equipmentIndex)
	{
		if (!NetworkServer.active)
		{
			Debug.LogWarning((object)"[Server] function 'System.Void RoR2.Run::DisableEquipmentDrop(RoR2.EquipmentIndex)' called on client");
			return;
		}
		PickupIndex pickupIndex = PickupCatalog.FindPickupIndex(equipmentIndex);
		if (!(pickupIndex != PickupIndex.none))
		{
			return;
		}
		List<PickupIndex> list = null;
		bool flag = false;
		if (PickupCatalog.GetPickupDef(pickupIndex).isLunar)
		{
			flag = true;
			list = availableLunarEquipmentDropList;
		}
		else
		{
			list = availableEquipmentDropList;
		}
		if (list.Contains(pickupIndex))
		{
			list.Remove(pickupIndex);
			if (flag)
			{
				RefreshLunarCombinedDropList();
			}
			Run.onAvailablePickupsModified?.Invoke(this);
		}
	}

	[Server]
	public void EnablePickupDrop(PickupIndex pickupIndex)
	{
		if (!NetworkServer.active)
		{
			Debug.LogWarning((object)"[Server] function 'System.Void RoR2.Run::EnablePickupDrop(RoR2.PickupIndex)' called on client");
			return;
		}
		PickupDef pickupDef = PickupCatalog.GetPickupDef(pickupIndex);
		if (pickupDef.itemIndex != ItemIndex.None)
		{
			EnableItemDrop(pickupDef.itemIndex);
		}
		if (pickupDef.equipmentIndex != EquipmentIndex.None)
		{
			EnableEquipmentDrop(pickupDef.equipmentIndex);
		}
	}

	[Server]
	public void EnableItemDrop(ItemIndex itemIndex)
	{
		if (!NetworkServer.active)
		{
			Debug.LogWarning((object)"[Server] function 'System.Void RoR2.Run::EnableItemDrop(RoR2.ItemIndex)' called on client");
			return;
		}
		ItemDef itemDef = ItemCatalog.GetItemDef(itemIndex);
		List<PickupIndex> list = null;
		bool flag = false;
		switch (itemDef.tier)
		{
		case ItemTier.Tier1:
			list = availableTier1DropList;
			break;
		case ItemTier.Tier2:
			list = availableTier2DropList;
			break;
		case ItemTier.Tier3:
			list = availableTier3DropList;
			break;
		case ItemTier.Lunar:
			list = availableLunarItemDropList;
			break;
		case ItemTier.Boss:
			list = availableBossDropList;
			break;
		case ItemTier.VoidTier1:
			list = availableVoidTier1DropList;
			break;
		case ItemTier.VoidTier2:
			list = availableVoidTier2DropList;
			break;
		case ItemTier.VoidTier3:
			list = availableVoidTier3DropList;
			break;
		case ItemTier.VoidBoss:
			list = availableVoidBossDropList;
			break;
		}
		PickupIndex pickupIndex = PickupCatalog.FindPickupIndex(itemIndex);
		if (list != null && pickupIndex != PickupIndex.none && !list.Contains(pickupIndex))
		{
			list.Add(pickupIndex);
			if (flag)
			{
				RefreshLunarCombinedDropList();
			}
			Run.onAvailablePickupsModified?.Invoke(this);
		}
	}

	[Server]
	public void EnableEquipmentDrop(EquipmentIndex equipmentIndex)
	{
		if (!NetworkServer.active)
		{
			Debug.LogWarning((object)"[Server] function 'System.Void RoR2.Run::EnableEquipmentDrop(RoR2.EquipmentIndex)' called on client");
			return;
		}
		PickupIndex pickupIndex = PickupCatalog.FindPickupIndex(equipmentIndex);
		if (!(pickupIndex != PickupIndex.none))
		{
			return;
		}
		List<PickupIndex> list = null;
		bool flag = false;
		list = ((!PickupCatalog.GetPickupDef(pickupIndex).isLunar) ? availableEquipmentDropList : availableLunarEquipmentDropList);
		if (!list.Contains(pickupIndex))
		{
			list.Add(pickupIndex);
			if (flag)
			{
				RefreshLunarCombinedDropList();
			}
			Run.onAvailablePickupsModified?.Invoke(this);
		}
	}

	[Server]
	private void RefreshLunarCombinedDropList()
	{
		if (!NetworkServer.active)
		{
			Debug.LogWarning((object)"[Server] function 'System.Void RoR2.Run::RefreshLunarCombinedDropList()' called on client");
			return;
		}
		availableLunarCombinedDropList.Clear();
		availableLunarCombinedDropList.AddRange(availableLunarEquipmentDropList);
		availableLunarCombinedDropList.AddRange(availableLunarItemDropList);
	}

	[ConCommand(commandName = "run_end", flags = ConVarFlags.SenderMustBeServer, helpText = "Ends the current run.")]
	private static void CCRunEnd(ConCommandArgs args)
	{
		if (Object.op_Implicit((Object)(object)instance))
		{
			Object.Destroy((Object)(object)((Component)instance).gameObject);
		}
	}

	[ConCommand(commandName = "run_print_unlockables", flags = ConVarFlags.SenderMustBeServer, helpText = "Prints all unlockables available in this run.")]
	private static void CCRunPrintUnlockables(ConCommandArgs args)
	{
		if (!Object.op_Implicit((Object)(object)instance))
		{
			throw new ConCommandException("No run is currently in progress.");
		}
		List<string> list = new List<string>();
		foreach (UnlockableDef item in instance.unlockablesUnlockedByAnyUser)
		{
			list.Add(item.cachedName);
		}
		Debug.Log((object)string.Join("\n", list.ToArray()));
	}

	[ConCommand(commandName = "run_print_seed", flags = ConVarFlags.None, helpText = "Prints the seed of the current run.")]
	private static void CCRunPrintSeed(ConCommandArgs args)
	{
		if (!Object.op_Implicit((Object)(object)instance))
		{
			throw new ConCommandException("No run is currently in progress.");
		}
		Debug.LogFormat("Current run seed: {0}", new object[1] { instance.seed });
	}

	[ConCommand(commandName = "run_set_stages_cleared", flags = (ConVarFlags.ExecuteOnServer | ConVarFlags.Cheat), helpText = "Sets the current number of stages cleared in the run.")]
	private static void CCRunSetStagesCleared(ConCommandArgs args)
	{
		if (!Object.op_Implicit((Object)(object)instance))
		{
			throw new ConCommandException("No run is currently in progress.");
		}
		instance.NetworkstageClearCount = args.GetArgInt(0);
	}

	public virtual void AdvanceStage(SceneDef nextScene)
	{
		if (Object.op_Implicit((Object)(object)Stage.instance))
		{
			Stage.instance.CompleteServer();
			if (SceneCatalog.GetSceneDefForCurrentScene().sceneType == SceneType.Stage)
			{
				NetworkstageClearCount = stageClearCount + 1;
			}
		}
		GenerateStageRNG();
		RecalculateDifficultyCoefficent();
		NetworkManager.singleton.ServerChangeScene(nextScene.cachedName);
	}

	public void BeginGameOver([NotNull] GameEndingDef gameEndingDef)
	{
		if (isGameOverServer)
		{
			return;
		}
		if (Object.op_Implicit((Object)(object)Stage.instance) && gameEndingDef.isWin)
		{
			Stage.instance.CompleteServer();
		}
		isGameOverServer = true;
		if (gameEndingDef.lunarCoinReward != 0)
		{
			for (int i = 0; i < NetworkUser.readOnlyInstancesList.Count; i++)
			{
				NetworkUser networkUser = NetworkUser.readOnlyInstancesList[i];
				if (Object.op_Implicit((Object)(object)networkUser) && networkUser.isParticipating)
				{
					networkUser.AwardLunarCoins(gameEndingDef.lunarCoinReward);
				}
			}
		}
		StatManager.ForceUpdate();
		GameObject val = Object.Instantiate<GameObject>(gameOverPrefab);
		GameOverController component = val.GetComponent<GameOverController>();
		component.SetRunReport(RunReport.Generate(this, gameEndingDef));
		Run.onServerGameOver?.Invoke(this, gameEndingDef);
		NetworkServer.Spawn(val);
		component.CallRpcClientGameOver();
	}

	public virtual void OnClientGameOver(RunReport runReport)
	{
		Run.onClientGameOverGlobal?.Invoke(this, runReport);
	}

	public virtual void OverrideRuleChoices(RuleChoiceMask mustInclude, RuleChoiceMask mustExclude, ulong runSeed)
	{
	}

	protected void ForceChoice(RuleChoiceMask mustInclude, RuleChoiceMask mustExclude, RuleChoiceDef choiceDef)
	{
		foreach (RuleChoiceDef choice in choiceDef.ruleDef.choices)
		{
			mustInclude[choice.globalIndex] = false;
			mustExclude[choice.globalIndex] = true;
		}
		mustInclude[choiceDef.globalIndex] = true;
		mustExclude[choiceDef.globalIndex] = false;
	}

	protected void ForceChoice(RuleChoiceMask mustInclude, RuleChoiceMask mustExclude, string choiceDefGlobalName)
	{
		ForceChoice(mustInclude, mustExclude, RuleCatalog.FindChoiceDef(choiceDefGlobalName));
	}

	public virtual Vector3 FindSafeTeleportPosition(CharacterBody characterBody, Transform targetDestination)
	{
		//IL_000d: Unknown result type (might be due to invalid IL or missing references)
		return FindSafeTeleportPosition(characterBody, targetDestination, float.NegativeInfinity, float.NegativeInfinity);
	}

	public virtual Vector3 FindSafeTeleportPosition(CharacterBody characterBody, Transform targetDestination, float idealMinDistance, float idealMaxDistance)
	{
		//IL_000e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0016: Unknown result type (might be due to invalid IL or missing references)
		//IL_001b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0045: Unknown result type (might be due to invalid IL or missing references)
		//IL_0046: Unknown result type (might be due to invalid IL or missing references)
		//IL_0079: Unknown result type (might be due to invalid IL or missing references)
		//IL_007a: Unknown result type (might be due to invalid IL or missing references)
		//IL_00aa: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ab: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ce: Unknown result type (might be due to invalid IL or missing references)
		//IL_00d3: Unknown result type (might be due to invalid IL or missing references)
		//IL_00e8: Unknown result type (might be due to invalid IL or missing references)
		Vector3 val = (Object.op_Implicit((Object)(object)targetDestination) ? targetDestination.position : ((Component)characterBody).transform.position);
		SpawnCard spawnCard = ScriptableObject.CreateInstance<SpawnCard>();
		spawnCard.hullSize = characterBody.hullClassification;
		spawnCard.nodeGraphType = MapNodeGroup.GraphType.Ground;
		spawnCard.prefab = LegacyResourcesAPI.Load<GameObject>("SpawnCards/HelperPrefab");
		Vector3 result = val;
		GameObject val2 = null;
		if (idealMaxDistance > 0f && idealMinDistance < idealMaxDistance)
		{
			val2 = DirectorCore.instance.TrySpawnObject(new DirectorSpawnRequest(spawnCard, new DirectorPlacementRule
			{
				placementMode = DirectorPlacementRule.PlacementMode.Approximate,
				minDistance = idealMinDistance,
				maxDistance = idealMaxDistance,
				position = val
			}, RoR2Application.rng));
		}
		if (!Object.op_Implicit((Object)(object)val2))
		{
			val2 = DirectorCore.instance.TrySpawnObject(new DirectorSpawnRequest(spawnCard, new DirectorPlacementRule
			{
				placementMode = DirectorPlacementRule.PlacementMode.NearestNode,
				position = val
			}, RoR2Application.rng));
			if (Object.op_Implicit((Object)(object)val2))
			{
				result = val2.transform.position;
			}
		}
		if (Object.op_Implicit((Object)(object)val2))
		{
			Object.Destroy((Object)(object)val2);
		}
		Object.Destroy((Object)(object)spawnCard);
		return result;
	}

	[Server]
	public void SetEventFlag(string name)
	{
		if (!NetworkServer.active)
		{
			Debug.LogWarning((object)"[Server] function 'System.Void RoR2.Run::SetEventFlag(System.String)' called on client");
		}
		else
		{
			eventFlags.Add(name);
		}
	}

	[Server]
	public bool GetEventFlag(string name)
	{
		if (!NetworkServer.active)
		{
			Debug.LogWarning((object)"[Server] function 'System.Boolean RoR2.Run::GetEventFlag(System.String)' called on client");
			return false;
		}
		return eventFlags.Contains(name);
	}

	[Server]
	public void ResetEventFlag(string name)
	{
		if (!NetworkServer.active)
		{
			Debug.LogWarning((object)"[Server] function 'System.Void RoR2.Run::ResetEventFlag(System.String)' called on client");
		}
		else
		{
			eventFlags.Remove(name);
		}
	}

	public virtual bool ShouldAllowNonChampionBossSpawn()
	{
		return stageClearCount > 0;
	}

	public bool IsExpansionEnabled([NotNull] ExpansionDef expansionDef)
	{
		if (expansionDef.enabledChoice != null)
		{
			return ruleBook.IsChoiceActive(expansionDef.enabledChoice);
		}
		return false;
	}

	protected virtual void OnSeedSet()
	{
		Debug.Log((object)$"Run seed:  {seed}");
	}

	private void UNetVersion()
	{
	}

	public override bool OnSerialize(NetworkWriter writer, bool forceAll)
	{
		if (forceAll)
		{
			GeneratedNetworkCode._WriteNetworkGuid_None(writer, _uniqueId);
			GeneratedNetworkCode._WriteNetworkDateTime_None(writer, startTimeUtc);
			writer.Write(fixedTime);
			GeneratedNetworkCode._WriteRunStopwatch_Run(writer, runStopwatch);
			writer.WritePackedUInt32((uint)stageClearCount);
			writer.WritePackedUInt32((uint)selectedDifficultyInternal);
			return true;
		}
		bool flag = false;
		if ((((NetworkBehaviour)this).syncVarDirtyBits & (true ? 1u : 0u)) != 0)
		{
			if (!flag)
			{
				writer.WritePackedUInt32(((NetworkBehaviour)this).syncVarDirtyBits);
				flag = true;
			}
			GeneratedNetworkCode._WriteNetworkGuid_None(writer, _uniqueId);
		}
		if ((((NetworkBehaviour)this).syncVarDirtyBits & 2u) != 0)
		{
			if (!flag)
			{
				writer.WritePackedUInt32(((NetworkBehaviour)this).syncVarDirtyBits);
				flag = true;
			}
			GeneratedNetworkCode._WriteNetworkDateTime_None(writer, startTimeUtc);
		}
		if ((((NetworkBehaviour)this).syncVarDirtyBits & 4u) != 0)
		{
			if (!flag)
			{
				writer.WritePackedUInt32(((NetworkBehaviour)this).syncVarDirtyBits);
				flag = true;
			}
			writer.Write(fixedTime);
		}
		if ((((NetworkBehaviour)this).syncVarDirtyBits & 8u) != 0)
		{
			if (!flag)
			{
				writer.WritePackedUInt32(((NetworkBehaviour)this).syncVarDirtyBits);
				flag = true;
			}
			GeneratedNetworkCode._WriteRunStopwatch_Run(writer, runStopwatch);
		}
		if ((((NetworkBehaviour)this).syncVarDirtyBits & 0x10u) != 0)
		{
			if (!flag)
			{
				writer.WritePackedUInt32(((NetworkBehaviour)this).syncVarDirtyBits);
				flag = true;
			}
			writer.WritePackedUInt32((uint)stageClearCount);
		}
		if ((((NetworkBehaviour)this).syncVarDirtyBits & 0x20u) != 0)
		{
			if (!flag)
			{
				writer.WritePackedUInt32(((NetworkBehaviour)this).syncVarDirtyBits);
				flag = true;
			}
			writer.WritePackedUInt32((uint)selectedDifficultyInternal);
		}
		if (!flag)
		{
			writer.WritePackedUInt32(((NetworkBehaviour)this).syncVarDirtyBits);
		}
		return flag;
	}

	public override void OnDeserialize(NetworkReader reader, bool initialState)
	{
		if (initialState)
		{
			_uniqueId = GeneratedNetworkCode._ReadNetworkGuid_None(reader);
			startTimeUtc = GeneratedNetworkCode._ReadNetworkDateTime_None(reader);
			fixedTime = reader.ReadSingle();
			runStopwatch = GeneratedNetworkCode._ReadRunStopwatch_Run(reader);
			stageClearCount = (int)reader.ReadPackedUInt32();
			selectedDifficultyInternal = (int)reader.ReadPackedUInt32();
			return;
		}
		int num = (int)reader.ReadPackedUInt32();
		if (((uint)num & (true ? 1u : 0u)) != 0)
		{
			_uniqueId = GeneratedNetworkCode._ReadNetworkGuid_None(reader);
		}
		if (((uint)num & 2u) != 0)
		{
			startTimeUtc = GeneratedNetworkCode._ReadNetworkDateTime_None(reader);
		}
		if (((uint)num & 4u) != 0)
		{
			fixedTime = reader.ReadSingle();
		}
		if (((uint)num & 8u) != 0)
		{
			runStopwatch = GeneratedNetworkCode._ReadRunStopwatch_Run(reader);
		}
		if (((uint)num & 0x10u) != 0)
		{
			stageClearCount = (int)reader.ReadPackedUInt32();
		}
		if (((uint)num & 0x20u) != 0)
		{
			selectedDifficultyInternal = (int)reader.ReadPackedUInt32();
		}
	}

	public override void PreStartClient()
	{
	}
}
