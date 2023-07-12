using System;
using System.Runtime.InteropServices;
using RoR2.Navigation;
using UnityEngine;
using UnityEngine.Networking;

namespace RoR2;

public class InfiniteTowerRun : Run
{
	[Serializable]
	public struct EnemyItemEntry
	{
		public PickupDropTable dropTable;

		public int stacks;
	}

	private const ulong waveRngSalt = 14312uL;

	private const ulong enemyItemRngSalt = 1535uL;

	private const ulong safeWardRngSalt = 769876uL;

	[Header("Infinite Tower Settings")]
	[Tooltip("If all else fails, use this wave prefab")]
	[SerializeField]
	private GameObject defaultWavePrefab;

	[Tooltip("Selects a wave from the first available category in this list")]
	[SerializeField]
	private InfiniteTowerWaveCategory[] waveCategories;

	[Tooltip("Use this indicator for enemies by default")]
	[SerializeField]
	private GameObject defaultWaveEnemyIndicatorPrefab;

	[Tooltip("The repeating pattern of drop tables to use when selecting items for the enemy team")]
	[SerializeField]
	private EnemyItemEntry[] enemyItemPattern;

	[Tooltip("The number of waves before you give the enemy team the next item in the pattern (e.g., \"every Nth wave\")")]
	[SerializeField]
	private int enemyItemPeriod;

	[SerializeField]
	[Tooltip("The reference inventory we use to store which items enemies should get")]
	private Inventory enemyInventory;

	[SerializeField]
	[Tooltip("The number of waves before you transition to the next stage (e.g., \"every Nth wave\").")]
	private int stageTransitionPeriod;

	[Tooltip("Spawn card for the stage transition portal")]
	[SerializeField]
	private InteractableSpawnCard stageTransitionPortalCard;

	[Tooltip("Maximum spawn distance for the stage transition portal.")]
	[SerializeField]
	private float stageTransitionPortalMaxDistance;

	[SerializeField]
	[Tooltip("The chat message that's broadcasted when spawning the stage transition portal.")]
	private string stageTransitionChatToken;

	[SerializeField]
	[Tooltip("The prefab with the FogDamageController attached")]
	private GameObject fogDamagePrefab;

	[SerializeField]
	[Tooltip("The maximum distance to spawn players from the safe ward")]
	private float spawnMaxRadius;

	[SerializeField]
	[Tooltip("Spawn card for the safe ward that is spawned at the beginning of the run")]
	private InteractableSpawnCard initialSafeWardCard;

	[Tooltip("Spawn card for the safe wards (after the first one)")]
	[SerializeField]
	private InteractableSpawnCard safeWardCard;

	[SerializeField]
	[Tooltip("The effect to spawn when a player is revived at the end of a wave")]
	private GameObject playerRespawnEffectPrefab;

	[SerializeField]
	[Tooltip("The number of credits the SceneDirector uses to spawn interactables")]
	private int interactableCredits;

	[SerializeField]
	[Tooltip("Remove all items with these tags from the item pools")]
	private ItemTag[] blacklistedTags;

	[Tooltip("Remove these items from the pool")]
	[SerializeField]
	private ItemDef[] blacklistedItems;

	[SyncVar]
	private int _waveIndex;

	[SyncVar]
	private NetworkInstanceId waveInstanceId;

	private InfiniteTowerWaveController _waveController;

	private Xoroshiro128Plus waveRng;

	private Xoroshiro128Plus enemyItemRng;

	private Xoroshiro128Plus safeWardRng;

	private int enemyItemPatternIndex;

	private InfiniteTowerSafeWardController safeWardController;

	private FogDamageController fogDamageController;

	public int waveIndex => _waveIndex;

	public InfiniteTowerWaveController waveController => _waveController;

	public override bool spawnWithPod => false;

	public override bool canFamilyEventTrigger => false;

	public override bool autoGenerateSpawnPoints => false;

	public GameObject waveInstance => Util.FindNetworkObject(waveInstanceId);

	public int Network_waveIndex
	{
		get
		{
			return _waveIndex;
		}
		[param: In]
		set
		{
			((NetworkBehaviour)this).SetSyncVar<int>(value, ref _waveIndex, 64u);
		}
	}

	public NetworkInstanceId NetworkwaveInstanceId
	{
		get
		{
			//IL_0001: Unknown result type (might be due to invalid IL or missing references)
			return waveInstanceId;
		}
		[param: In]
		set
		{
			//IL_0001: Unknown result type (might be due to invalid IL or missing references)
			((NetworkBehaviour)this).SetSyncVar<NetworkInstanceId>(value, ref waveInstanceId, 128u);
		}
	}

	public static event Action<InfiniteTowerWaveController> onWaveInitialized;

	public static event Action<InfiniteTowerWaveController> onAllEnemiesDefeatedServer;

	public override GameObject InstantiateUi(Transform uiRoot)
	{
		base.InstantiateUi(uiRoot);
		if (Object.op_Implicit((Object)(object)_waveController))
		{
			_waveController.InstantiateUi(base.uiInstance.transform);
		}
		return base.uiInstance;
	}

	public override void OverrideRuleChoices(RuleChoiceMask mustInclude, RuleChoiceMask mustExclude, ulong runSeed)
	{
		base.OverrideRuleChoices(mustInclude, mustExclude, base.seed);
		ItemIndex itemIndex = (ItemIndex)0;
		for (ItemIndex itemCount = (ItemIndex)ItemCatalog.itemCount; itemIndex < itemCount; itemIndex++)
		{
			ItemDef itemDef = ItemCatalog.GetItemDef(itemIndex);
			bool flag = Array.IndexOf(blacklistedItems, itemDef) != -1;
			if (!flag)
			{
				ItemTag[] array = blacklistedTags;
				foreach (ItemTag tag in array)
				{
					if (itemDef.ContainsTag(tag))
					{
						flag = true;
						break;
					}
				}
			}
			if (flag)
			{
				RuleChoiceDef ruleChoiceDef = RuleCatalog.FindRuleDef("Items." + ((Object)itemDef).name)?.FindChoice("Off");
				if (ruleChoiceDef != null)
				{
					ForceChoice(mustInclude, mustExclude, ruleChoiceDef);
				}
			}
		}
	}

	[Server]
	public void ResetSafeWard()
	{
		if (!NetworkServer.active)
		{
			Debug.LogWarning((object)"[Server] function 'System.Void RoR2.InfiniteTowerRun::ResetSafeWard()' called on client");
			return;
		}
		if (Object.op_Implicit((Object)(object)safeWardController))
		{
			if (Object.op_Implicit((Object)(object)fogDamageController))
			{
				fogDamageController.RemoveSafeZone(safeWardController.safeZone);
			}
			safeWardController.SelfDestruct();
		}
		SpawnSafeWard(safeWardCard, new DirectorPlacementRule
		{
			placementMode = DirectorPlacementRule.PlacementMode.Random
		});
	}

	[Server]
	public void MoveSafeWard()
	{
		if (!NetworkServer.active)
		{
			Debug.LogWarning((object)"[Server] function 'System.Void RoR2.InfiniteTowerRun::MoveSafeWard()' called on client");
		}
		else if (Object.op_Implicit((Object)(object)safeWardController))
		{
			safeWardController.RandomizeLocation(safeWardRng);
			safeWardController.onActivated += OnSafeWardActivated;
		}
	}

	public bool IsStageTransitionWave()
	{
		if (stageTransitionPeriod != 0)
		{
			return waveIndex % stageTransitionPeriod == 0;
		}
		return true;
	}

	public override Vector3 FindSafeTeleportPosition(CharacterBody characterBody, Transform targetDestination)
	{
		//IL_0022: Unknown result type (might be due to invalid IL or missing references)
		//IL_002b: Unknown result type (might be due to invalid IL or missing references)
		if (Object.op_Implicit((Object)(object)safeWardController) && !Object.op_Implicit((Object)(object)targetDestination))
		{
			return base.FindSafeTeleportPosition(characterBody, ((Component)safeWardController).transform);
		}
		return base.FindSafeTeleportPosition(characterBody, targetDestination);
	}

	public override Vector3 FindSafeTeleportPosition(CharacterBody characterBody, Transform targetDestination, float idealMinDistance, float idealMaxDistance)
	{
		//IL_0025: Unknown result type (might be due to invalid IL or missing references)
		//IL_0031: Unknown result type (might be due to invalid IL or missing references)
		if (Object.op_Implicit((Object)(object)safeWardController) && !Object.op_Implicit((Object)(object)targetDestination))
		{
			return base.FindSafeTeleportPosition(characterBody, ((Component)safeWardController).transform, idealMinDistance, idealMaxDistance);
		}
		return base.FindSafeTeleportPosition(characterBody, targetDestination, idealMinDistance, idealMaxDistance);
	}

	[Server]
	private void OnSafeWardActivated(InfiniteTowerSafeWardController safeWard)
	{
		if (!NetworkServer.active)
		{
			Debug.LogWarning((object)"[Server] function 'System.Void RoR2.InfiniteTowerRun::OnSafeWardActivated(RoR2.InfiniteTowerSafeWardController)' called on client");
			return;
		}
		safeWardController.onActivated -= OnSafeWardActivated;
		if (Object.op_Implicit((Object)(object)_waveController))
		{
			_waveController.ForceFinish();
			CleanUpCurrentWave();
		}
		BeginNextWave();
	}

	[Server]
	private void AdvanceWave()
	{
		//IL_00c7: Unknown result type (might be due to invalid IL or missing references)
		//IL_00cc: Unknown result type (might be due to invalid IL or missing references)
		//IL_00d1: Unknown result type (might be due to invalid IL or missing references)
		if (!NetworkServer.active)
		{
			Debug.LogWarning((object)"[Server] function 'System.Void RoR2.InfiniteTowerRun::AdvanceWave()' called on client");
			return;
		}
		Network_waveIndex = _waveIndex + 1;
		if (_waveIndex % enemyItemPeriod != 0)
		{
			return;
		}
		EnemyItemEntry enemyItemEntry = enemyItemPattern[enemyItemPatternIndex++ % enemyItemPattern.Length];
		if (!Object.op_Implicit((Object)(object)enemyItemEntry.dropTable))
		{
			return;
		}
		PickupIndex pickupIndex = enemyItemEntry.dropTable.GenerateDrop(enemyItemRng);
		if (pickupIndex != PickupIndex.none)
		{
			PickupDef pickupDef = PickupCatalog.GetPickupDef(pickupIndex);
			if (pickupDef != null)
			{
				enemyInventory.GiveItem(pickupDef.itemIndex, enemyItemEntry.stacks);
				Chat.SendBroadcastChat(new Chat.PlayerPickupChatMessage
				{
					baseToken = "INFINITETOWER_ADD_ITEM",
					pickupToken = pickupDef.nameToken,
					pickupColor = Color32.op_Implicit(pickupDef.baseColor)
				});
			}
		}
	}

	[Server]
	private void BeginNextWave()
	{
		//IL_0071: Unknown result type (might be due to invalid IL or missing references)
		if (!NetworkServer.active)
		{
			Debug.LogWarning((object)"[Server] function 'System.Void RoR2.InfiniteTowerRun::BeginNextWave()' called on client");
			return;
		}
		AdvanceWave();
		GameObject val = defaultWavePrefab;
		InfiniteTowerWaveCategory[] array = waveCategories;
		foreach (InfiniteTowerWaveCategory infiniteTowerWaveCategory in array)
		{
			if (infiniteTowerWaveCategory.IsAvailable(this))
			{
				val = infiniteTowerWaveCategory.SelectWavePrefab(this, waveRng);
				break;
			}
		}
		GameObject val2 = Object.Instantiate<GameObject>(val, ((Component)this).transform);
		NetworkServer.Spawn(val2);
		NetworkwaveInstanceId = val2.GetComponent<NetworkIdentity>().netId;
		RecalculateDifficultyCoefficentInternal();
	}

	protected override void Start()
	{
		Stage.onServerStageBegin += OnServerStageBegin;
		Stage.onServerStageComplete += OnServerStageComplete;
		SceneDirector.onPrePopulateSceneServer += OnPrePopulateSceneServer;
		base.Start();
	}

	protected override void OnDestroy()
	{
		Stage.onServerStageBegin -= OnServerStageBegin;
		Stage.onServerStageComplete -= OnServerStageComplete;
		SceneDirector.onPrePopulateSceneServer -= OnPrePopulateSceneServer;
		if (Object.op_Implicit((Object)(object)safeWardController))
		{
			Object.Destroy((Object)(object)((Component)safeWardController).gameObject);
			safeWardController = null;
		}
		CleanUpCurrentWave();
		base.OnDestroy();
	}

	private void OnServerStageBegin(Stage stage)
	{
	}

	private void OnServerStageComplete(Stage stage)
	{
		PerformStageCleanUp();
	}

	private void OnPrePopulateSceneServer(SceneDirector sceneDirector)
	{
		//IL_0079: Unknown result type (might be due to invalid IL or missing references)
		//IL_007e: Unknown result type (might be due to invalid IL or missing references)
		//IL_008c: Unknown result type (might be due to invalid IL or missing references)
		//IL_00bf: Unknown result type (might be due to invalid IL or missing references)
		//IL_00c1: Unknown result type (might be due to invalid IL or missing references)
		//IL_00c2: Unknown result type (might be due to invalid IL or missing references)
		//IL_00c7: Unknown result type (might be due to invalid IL or missing references)
		PerformStageCleanUp();
		if (Object.op_Implicit((Object)(object)fogDamagePrefab))
		{
			GameObject val = Object.Instantiate<GameObject>(fogDamagePrefab, ((Component)Stage.instance).transform);
			NetworkServer.Spawn(val);
			fogDamageController = val.GetComponent<FogDamageController>();
		}
		sceneDirector.interactableCredit = interactableCredits;
		DirectorPlacementRule placementRule = new DirectorPlacementRule
		{
			placementMode = DirectorPlacementRule.PlacementMode.Random
		};
		SpawnSafeWard(initialSafeWardCard, placementRule);
		if (!Object.op_Implicit((Object)(object)safeWardController))
		{
			return;
		}
		Vector3 position = ((Component)safeWardController).transform.position;
		NodeGraph nodeGraph = SceneInfo.instance.GetNodeGraph(MapNodeGroup.GraphType.Ground);
		foreach (NodeGraph.NodeIndex item in nodeGraph.FindNodesInRangeWithFlagConditions(position, 0f, spawnMaxRadius, HullMask.Human, NodeFlags.None, NodeFlags.NoCharacterSpawn, preventOverhead: false))
		{
			if (nodeGraph.GetNodePosition(item, out var position2))
			{
				SpawnPoint.AddSpawnPoint(position2, Quaternion.LookRotation(position, Vector3.up));
			}
		}
	}

	protected override void FixedUpdate()
	{
		base.FixedUpdate();
		if (NetworkServer.active && Object.op_Implicit((Object)(object)waveInstance) && Object.op_Implicit((Object)(object)waveController) && waveController.isFinished && !IsStageTransitionWave())
		{
			CleanUpCurrentWave();
			BeginNextWave();
		}
		if (Object.op_Implicit((Object)(object)waveInstance))
		{
			if (!Object.op_Implicit((Object)(object)_waveController))
			{
				InitializeWaveController();
			}
		}
		else
		{
			_waveController = null;
		}
	}

	protected override void RecalculateDifficultyCoefficentInternal()
	{
		DifficultyDef difficultyDef = DifficultyCatalog.GetDifficultyDef(base.selectedDifficulty);
		float num = 1.5f * (float)waveIndex;
		float num2 = 0.0506f * difficultyDef.scalingValue;
		float num3 = Mathf.Pow(1.02f, (float)waveIndex);
		difficultyCoefficient = (1f + num2 * num) * num3;
		compensatedDifficultyCoefficient = difficultyCoefficient;
		base.ambientLevel = Mathf.Min((difficultyCoefficient - 1f) / 0.33f + 1f, 9999f);
		int num4 = base.ambientLevelFloor;
		base.ambientLevelFloor = Mathf.FloorToInt(base.ambientLevel);
		if (num4 != base.ambientLevelFloor && num4 != 0 && base.ambientLevelFloor > num4)
		{
			OnAmbientLevelUp();
		}
	}

	private void InitializeWaveController()
	{
		_waveController = waveInstance.GetComponent<InfiniteTowerWaveController>();
		if (Object.op_Implicit((Object)(object)_waveController))
		{
			if (NetworkServer.active)
			{
				_waveController.Initialize(_waveIndex, enemyInventory, ((Component)safeWardController).gameObject);
			}
			if (Object.op_Implicit((Object)(object)base.uiInstance))
			{
				_waveController.InstantiateUi(base.uiInstance.transform);
			}
			_waveController.PlayBeginSound();
			_waveController.defaultEnemyIndicatorPrefab = defaultWaveEnemyIndicatorPrefab;
			_waveController.onAllEnemiesDefeatedServer += OnWaveAllEnemiesDefeatedServer;
			InfiniteTowerRun.onWaveInitialized?.Invoke(_waveController);
		}
	}

	private void OnWaveAllEnemiesDefeatedServer(InfiniteTowerWaveController wc)
	{
		//IL_004f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0054: Unknown result type (might be due to invalid IL or missing references)
		//IL_006d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0092: Unknown result type (might be due to invalid IL or missing references)
		//IL_0097: Unknown result type (might be due to invalid IL or missing references)
		//IL_009c: Unknown result type (might be due to invalid IL or missing references)
		//IL_009e: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b8: Unknown result type (might be due to invalid IL or missing references)
		//IL_0126: Unknown result type (might be due to invalid IL or missing references)
		//IL_0133: Unknown result type (might be due to invalid IL or missing references)
		//IL_0138: Unknown result type (might be due to invalid IL or missing references)
		//IL_01a5: Unknown result type (might be due to invalid IL or missing references)
		//IL_01aa: Unknown result type (might be due to invalid IL or missing references)
		InfiniteTowerRun.onAllEnemiesDefeatedServer?.Invoke(wc);
		if (base.isGameOverServer)
		{
			return;
		}
		foreach (PlayerCharacterMasterController instance in PlayerCharacterMasterController.instances)
		{
			CharacterMaster master = instance.master;
			if (!instance.isConnected || !master.IsDeadAndOutOfLivesServer())
			{
				continue;
			}
			Vector3 val = master.deathFootPosition;
			if (Object.op_Implicit((Object)(object)safeWardController))
			{
				val = (Vector3)(((_003F?)TeleportHelper.FindSafeTeleportDestination(((Component)safeWardController).transform.position, master.bodyPrefab.GetComponent<CharacterBody>(), RoR2Application.rng)) ?? val);
			}
			master.Respawn(val, Quaternion.Euler(0f, Random.Range(0f, 360f), 0f));
			CharacterBody body = master.GetBody();
			if (Object.op_Implicit((Object)(object)body))
			{
				body.AddTimedBuff(RoR2Content.Buffs.Immune, 3f);
				EntityStateMachine[] components = ((Component)body).GetComponents<EntityStateMachine>();
				foreach (EntityStateMachine obj in components)
				{
					obj.initialStateType = obj.mainStateType;
				}
				if (Object.op_Implicit((Object)(object)playerRespawnEffectPrefab))
				{
					EffectManager.SpawnEffect(playerRespawnEffectPrefab, new EffectData
					{
						origin = val,
						rotation = ((Component)body).transform.rotation
					}, transmit: true);
				}
			}
		}
		if (IsStageTransitionWave())
		{
			PickNextStageSceneFromCurrentSceneDestinations();
			DirectorCore.instance.TrySpawnObject(new DirectorSpawnRequest(stageTransitionPortalCard, new DirectorPlacementRule
			{
				minDistance = 0f,
				maxDistance = stageTransitionPortalMaxDistance,
				placementMode = DirectorPlacementRule.PlacementMode.Approximate,
				position = ((Component)safeWardController).transform.position,
				spawnOnTarget = ((Component)safeWardController).transform
			}, safeWardRng));
			Chat.SendBroadcastChat(new Chat.SimpleChatMessage
			{
				baseToken = stageTransitionChatToken
			});
			if (Object.op_Implicit((Object)(object)safeWardController))
			{
				safeWardController.WaitForPortal();
			}
		}
	}

	protected override bool ShouldUpdateRunStopwatch()
	{
		return true;
	}

	protected override void OnSeedSet()
	{
		//IL_000e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0018: Expected O, but got Unknown
		//IL_0026: Unknown result type (might be due to invalid IL or missing references)
		//IL_0030: Expected O, but got Unknown
		//IL_003e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0048: Expected O, but got Unknown
		waveRng = new Xoroshiro128Plus(base.seed ^ 0x37E8);
		enemyItemRng = new Xoroshiro128Plus(base.seed ^ 0x5FF);
		safeWardRng = new Xoroshiro128Plus(base.seed ^ 0xBBF54);
	}

	private void CleanUpCurrentWave()
	{
		//IL_0044: Unknown result type (might be due to invalid IL or missing references)
		if (Object.op_Implicit((Object)(object)_waveController))
		{
			_waveController.onAllEnemiesDefeatedServer -= OnWaveAllEnemiesDefeatedServer;
			_waveController = null;
		}
		if (Object.op_Implicit((Object)(object)waveInstance))
		{
			Object.Destroy((Object)(object)waveInstance);
		}
		NetworkwaveInstanceId = NetworkInstanceId.Invalid;
	}

	private void SpawnSafeWard(InteractableSpawnCard spawnCard, DirectorPlacementRule placementRule)
	{
		GameObject val = DirectorCore.instance.TrySpawnObject(new DirectorSpawnRequest(spawnCard, placementRule, safeWardRng));
		if (Object.op_Implicit((Object)(object)val))
		{
			NetworkServer.Spawn(val);
			safeWardController = val.GetComponent<InfiniteTowerSafeWardController>();
			if (Object.op_Implicit((Object)(object)safeWardController))
			{
				safeWardController.onActivated += OnSafeWardActivated;
			}
			HoldoutZoneController component = val.GetComponent<HoldoutZoneController>();
			if (Object.op_Implicit((Object)(object)component))
			{
				component.calcAccumulatedCharge += CalcHoldoutZoneCharge;
			}
			if (Object.op_Implicit((Object)(object)fogDamageController))
			{
				fogDamageController.AddSafeZone(safeWardController.safeZone);
			}
		}
		else
		{
			Debug.LogError((object)"Unable to spawn safe ward instance.  Are there any ground nodes?");
		}
	}

	private void CalcHoldoutZoneCharge(ref float charge)
	{
		if (Object.op_Implicit((Object)(object)waveController))
		{
			float num = waveController.GetNormalizedProgress();
			if (waveController.GetSquadCount() > 0)
			{
				num = Mathf.Min(num, 0.99f);
			}
			charge = num;
		}
		else
		{
			charge = 0f;
		}
	}

	[Server]
	public override void HandlePlayerFirstEntryAnimation(CharacterBody body, Vector3 spawnPosition, Quaternion spawnRotation)
	{
		if (!NetworkServer.active)
		{
			Debug.LogWarning((object)"[Server] function 'System.Void RoR2.InfiniteTowerRun::HandlePlayerFirstEntryAnimation(RoR2.CharacterBody,UnityEngine.Vector3,UnityEngine.Quaternion)' called on client");
		}
		else
		{
			body.SetBodyStateToPreferredInitialState();
		}
	}

	private void PerformStageCleanUp()
	{
		safeWardController = null;
		if (Object.op_Implicit((Object)(object)fogDamageController))
		{
			Object.Destroy((Object)(object)((Component)fogDamageController).gameObject);
		}
		CleanUpCurrentWave();
	}

	private void UNetVersion()
	{
	}

	public override bool OnSerialize(NetworkWriter writer, bool forceAll)
	{
		//IL_001d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0087: Unknown result type (might be due to invalid IL or missing references)
		bool flag = base.OnSerialize(writer, forceAll);
		if (forceAll)
		{
			writer.WritePackedUInt32((uint)_waveIndex);
			writer.Write(waveInstanceId);
			return true;
		}
		bool flag2 = false;
		if ((((NetworkBehaviour)this).syncVarDirtyBits & 0x40u) != 0)
		{
			if (!flag2)
			{
				writer.WritePackedUInt32(((NetworkBehaviour)this).syncVarDirtyBits);
				flag2 = true;
			}
			writer.WritePackedUInt32((uint)_waveIndex);
		}
		if ((((NetworkBehaviour)this).syncVarDirtyBits & 0x80u) != 0)
		{
			if (!flag2)
			{
				writer.WritePackedUInt32(((NetworkBehaviour)this).syncVarDirtyBits);
				flag2 = true;
			}
			writer.Write(waveInstanceId);
		}
		if (!flag2)
		{
			writer.WritePackedUInt32(((NetworkBehaviour)this).syncVarDirtyBits);
		}
		return flag2 || flag;
	}

	public override void OnDeserialize(NetworkReader reader, bool initialState)
	{
		//IL_001c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0021: Unknown result type (might be due to invalid IL or missing references)
		//IL_0056: Unknown result type (might be due to invalid IL or missing references)
		//IL_005b: Unknown result type (might be due to invalid IL or missing references)
		base.OnDeserialize(reader, initialState);
		if (initialState)
		{
			_waveIndex = (int)reader.ReadPackedUInt32();
			waveInstanceId = reader.ReadNetworkId();
			return;
		}
		int num = (int)reader.ReadPackedUInt32();
		if (((uint)num & 0x40u) != 0)
		{
			_waveIndex = (int)reader.ReadPackedUInt32();
		}
		if (((uint)num & 0x80u) != 0)
		{
			waveInstanceId = reader.ReadNetworkId();
		}
	}

	public override void PreStartClient()
	{
		base.PreStartClient();
	}
}
