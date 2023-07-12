using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Runtime.InteropServices;
using HG;
using RoR2.CharacterAI;
using RoR2.ConVar;
using RoR2.Navigation;
using UnityEngine;
using UnityEngine.Networking;

namespace RoR2;

public class Stage : NetworkBehaviour
{
	[SyncVar]
	private float _entryTime;

	[SyncVar]
	private float _entryStopwatchValue;

	[SyncVar]
	private float _entryDifficultyCoefficient;

	[SyncVar]
	private int _singleMonsterTypeBodyIndex = -1;

	private bool spawnedAnyPlayer;

	[NonSerialized]
	public bool usePod = Object.op_Implicit((Object)(object)Run.instance) && Run.instance.spawnWithPod && stage1PodConVar.value;

	private static BoolConVar stage1PodConVar = new BoolConVar("stage1_pod", ConVarFlags.Cheat, "1", "Whether or not to use the pod when spawning on the first stage.");

	[SyncVar]
	private float _stageAdvanceTime = float.PositiveInfinity;

	public const float stageAdvanceTransitionDuration = 0.5f;

	public const float stageAdvanceTransitionDelay = 0.75f;

	private SceneDef nextStage;

	public static Stage instance { get; private set; }

	public Run.FixedTimeStamp entryTime
	{
		get
		{
			return Run.FixedTimeStamp.zero + _entryTime;
		}
		private set
		{
			Network_entryTime = value - Run.FixedTimeStamp.zero;
		}
	}

	public float entryStopwatchValue
	{
		get
		{
			return _entryStopwatchValue;
		}
		private set
		{
			Network_entryStopwatchValue = value;
		}
	}

	public float entryDifficultyCoefficient
	{
		get
		{
			return _entryDifficultyCoefficient;
		}
		private set
		{
			Network_entryDifficultyCoefficient = value;
		}
	}

	public BodyIndex singleMonsterTypeBodyIndex
	{
		get
		{
			return (BodyIndex)_singleMonsterTypeBodyIndex;
		}
		set
		{
			Network_singleMonsterTypeBodyIndex = (int)value;
		}
	}

	public SceneDef sceneDef { get; private set; }

	public Run.FixedTimeStamp stageAdvanceTime
	{
		get
		{
			return Run.FixedTimeStamp.zero + _stageAdvanceTime;
		}
		private set
		{
			Network_stageAdvanceTime = value - Run.FixedTimeStamp.zero;
		}
	}

	public bool completed { get; private set; }

	public bool scavPackDroppedServer { get; set; }

	public float Network_entryTime
	{
		get
		{
			return _entryTime;
		}
		[param: In]
		set
		{
			((NetworkBehaviour)this).SetSyncVar<float>(value, ref _entryTime, 1u);
		}
	}

	public float Network_entryStopwatchValue
	{
		get
		{
			return _entryStopwatchValue;
		}
		[param: In]
		set
		{
			((NetworkBehaviour)this).SetSyncVar<float>(value, ref _entryStopwatchValue, 2u);
		}
	}

	public float Network_entryDifficultyCoefficient
	{
		get
		{
			return _entryDifficultyCoefficient;
		}
		[param: In]
		set
		{
			((NetworkBehaviour)this).SetSyncVar<float>(value, ref _entryDifficultyCoefficient, 4u);
		}
	}

	public int Network_singleMonsterTypeBodyIndex
	{
		get
		{
			return _singleMonsterTypeBodyIndex;
		}
		[param: In]
		set
		{
			((NetworkBehaviour)this).SetSyncVar<int>(value, ref _singleMonsterTypeBodyIndex, 8u);
		}
	}

	public float Network_stageAdvanceTime
	{
		get
		{
			return _stageAdvanceTime;
		}
		[param: In]
		set
		{
			((NetworkBehaviour)this).SetSyncVar<float>(value, ref _stageAdvanceTime, 16u);
		}
	}

	public static event Action<Stage> onServerStageBegin;

	public static event Action<Stage> onServerStageComplete;

	public static event Action<Stage> onStageStartGlobal;

	private void Start()
	{
		sceneDef = SceneCatalog.GetSceneDefForCurrentScene();
		if (NetworkServer.active)
		{
			entryTime = Run.FixedTimeStamp.now;
			entryStopwatchValue = Run.instance.GetRunStopwatch();
			entryDifficultyCoefficient = Run.instance.difficultyCoefficient;
			RespawnAllNPCs();
			BeginServer();
		}
		if (NetworkClient.active)
		{
			RespawnLocalPlayers();
		}
		Stage.onStageStartGlobal?.Invoke(this);
	}

	private void RespawnAllNPCs()
	{
		//IL_0065: Unknown result type (might be due to invalid IL or missing references)
		//IL_006a: Unknown result type (might be due to invalid IL or missing references)
		//IL_006c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0071: Unknown result type (might be due to invalid IL or missing references)
		//IL_007f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0084: Unknown result type (might be due to invalid IL or missing references)
		//IL_0087: Unknown result type (might be due to invalid IL or missing references)
		//IL_008c: Unknown result type (might be due to invalid IL or missing references)
		//IL_00dc: Unknown result type (might be due to invalid IL or missing references)
		//IL_0138: Unknown result type (might be due to invalid IL or missing references)
		//IL_013a: Unknown result type (might be due to invalid IL or missing references)
		if (sceneDef.suppressNpcEntry)
		{
			return;
		}
		ReadOnlyCollection<CharacterMaster> readOnlyInstancesList = CharacterMaster.readOnlyInstancesList;
		Transform playerSpawnTransform = GetPlayerSpawnTransform();
		for (int i = 0; i < readOnlyInstancesList.Count; i++)
		{
			CharacterMaster characterMaster = readOnlyInstancesList[i];
			if (!Object.op_Implicit((Object)(object)characterMaster) || Object.op_Implicit((Object)(object)((Component)characterMaster).GetComponent<PlayerCharacterMasterController>()) || Object.op_Implicit((Object)(object)characterMaster.GetBodyObject()) || !Util.IsDontDestroyOnLoad(((Component)characterMaster).gameObject))
			{
				continue;
			}
			Vector3 position = Vector3.zero;
			Quaternion rotation = Quaternion.identity;
			if (Object.op_Implicit((Object)(object)playerSpawnTransform))
			{
				position = playerSpawnTransform.position;
				rotation = playerSpawnTransform.rotation;
				BaseAI component = ((Component)readOnlyInstancesList[i]).GetComponent<BaseAI>();
				CharacterBody component2 = readOnlyInstancesList[i].bodyPrefab.GetComponent<CharacterBody>();
				if (Object.op_Implicit((Object)(object)component) && Object.op_Implicit((Object)(object)component2))
				{
					NodeGraph desiredSpawnNodeGraph = component.GetDesiredSpawnNodeGraph();
					if (Object.op_Implicit((Object)(object)desiredSpawnNodeGraph))
					{
						List<NodeGraph.NodeIndex> list = CollectionPool<NodeGraph.NodeIndex, List<NodeGraph.NodeIndex>>.RentCollection();
						desiredSpawnNodeGraph.FindNodesInRange(position, 10f, 100f, (HullMask)(1 << (int)component2.hullClassification), list);
						if ((float)list.Count > 0f)
						{
							desiredSpawnNodeGraph.GetNodePosition(list[Random.Range(0, list.Count)], out position);
						}
						list = CollectionPool<NodeGraph.NodeIndex, List<NodeGraph.NodeIndex>>.ReturnCollection(list);
					}
				}
			}
			readOnlyInstancesList[i].Respawn(position, rotation);
		}
	}

	[Client]
	public void RespawnLocalPlayers()
	{
		if (!NetworkClient.active)
		{
			Debug.LogWarning((object)"[Client] function 'System.Void RoR2.Stage::RespawnLocalPlayers()' called on server");
		}
		else
		{
			if (sceneDef.suppressPlayerEntry)
			{
				return;
			}
			ReadOnlyCollection<NetworkUser> readOnlyInstancesList = NetworkUser.readOnlyInstancesList;
			for (int i = 0; i < readOnlyInstancesList.Count; i++)
			{
				NetworkUser networkUser = readOnlyInstancesList[i];
				CharacterMaster characterMaster = null;
				if (((NetworkBehaviour)networkUser).isLocalPlayer && Object.op_Implicit((Object)(object)networkUser.masterObject))
				{
					characterMaster = networkUser.masterObject.GetComponent<CharacterMaster>();
				}
				if (Object.op_Implicit((Object)(object)characterMaster))
				{
					characterMaster.CallCmdRespawn("");
				}
			}
		}
	}

	private void OnEnable()
	{
		instance = SingletonHelper.Assign<Stage>(instance, this);
	}

	private void OnDisable()
	{
		instance = SingletonHelper.Unassign<Stage>(instance, this);
	}

	[Server]
	public Transform GetPlayerSpawnTransform()
	{
		//IL_0016: Unknown result type (might be due to invalid IL or missing references)
		if (!NetworkServer.active)
		{
			Debug.LogWarning((object)"[Server] function 'UnityEngine.Transform RoR2.Stage::GetPlayerSpawnTransform()' called on client");
			return default(Transform);
		}
		SpawnPoint spawnPoint = SpawnPoint.ConsumeSpawnPoint();
		if (Object.op_Implicit((Object)(object)spawnPoint))
		{
			return ((Component)spawnPoint).transform;
		}
		return null;
	}

	[Server]
	public void RespawnCharacter(CharacterMaster characterMaster)
	{
		//IL_0025: Unknown result type (might be due to invalid IL or missing references)
		//IL_002a: Unknown result type (might be due to invalid IL or missing references)
		//IL_002b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0030: Unknown result type (might be due to invalid IL or missing references)
		//IL_003a: Unknown result type (might be due to invalid IL or missing references)
		//IL_003f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0041: Unknown result type (might be due to invalid IL or missing references)
		//IL_0046: Unknown result type (might be due to invalid IL or missing references)
		//IL_0048: Unknown result type (might be due to invalid IL or missing references)
		//IL_0049: Unknown result type (might be due to invalid IL or missing references)
		//IL_0077: Unknown result type (might be due to invalid IL or missing references)
		//IL_0078: Unknown result type (might be due to invalid IL or missing references)
		if (!NetworkServer.active)
		{
			Debug.LogWarning((object)"[Server] function 'System.Void RoR2.Stage::RespawnCharacter(RoR2.CharacterMaster)' called on client");
		}
		else if (Object.op_Implicit((Object)(object)characterMaster))
		{
			Transform playerSpawnTransform = GetPlayerSpawnTransform();
			Vector3 val = Vector3.zero;
			Quaternion val2 = Quaternion.identity;
			if (Object.op_Implicit((Object)(object)playerSpawnTransform))
			{
				val = playerSpawnTransform.position;
				val2 = playerSpawnTransform.rotation;
			}
			characterMaster.Respawn(val, val2);
			if (Object.op_Implicit((Object)(object)((Component)characterMaster).GetComponent<PlayerCharacterMasterController>()))
			{
				spawnedAnyPlayer = true;
			}
			if (usePod)
			{
				Run.instance.HandlePlayerFirstEntryAnimation(characterMaster.GetBody(), val, val2);
			}
		}
	}

	[Server]
	public void BeginAdvanceStage(SceneDef destinationStage)
	{
		if (!NetworkServer.active)
		{
			Debug.LogWarning((object)"[Server] function 'System.Void RoR2.Stage::BeginAdvanceStage(RoR2.SceneDef)' called on client");
			return;
		}
		stageAdvanceTime = Run.FixedTimeStamp.now + 0.75f;
		nextStage = destinationStage;
	}

	[Server]
	private void BeginServer()
	{
		if (!NetworkServer.active)
		{
			Debug.LogWarning((object)"[Server] function 'System.Void RoR2.Stage::BeginServer()' called on client");
		}
		else
		{
			Stage.onServerStageBegin?.Invoke(this);
		}
	}

	[Server]
	public void CompleteServer()
	{
		if (!NetworkServer.active)
		{
			Debug.LogWarning((object)"[Server] function 'System.Void RoR2.Stage::CompleteServer()' called on client");
		}
		else if (!completed)
		{
			completed = true;
			Stage.onServerStageComplete?.Invoke(this);
		}
	}

	private void FixedUpdate()
	{
		if (!NetworkServer.active)
		{
			return;
		}
		if (Object.op_Implicit((Object)(object)nextStage) && stageAdvanceTime.hasPassed)
		{
			SceneDef nextScene = nextStage;
			nextStage = null;
			Run.instance.AdvanceStage(nextScene);
		}
		if (!spawnedAnyPlayer || !stageAdvanceTime.isInfinity || Run.instance.isGameOverServer)
		{
			return;
		}
		ReadOnlyCollection<PlayerCharacterMasterController> instances = PlayerCharacterMasterController.instances;
		bool flag = false;
		for (int i = 0; i < instances.Count; i++)
		{
			PlayerCharacterMasterController playerCharacterMasterController = instances[i];
			if (playerCharacterMasterController.isConnected && playerCharacterMasterController.preventGameOver)
			{
				flag = true;
				break;
			}
		}
		if (!flag)
		{
			Run.instance.BeginGameOver(RoR2Content.GameEndings.StandardLoss);
		}
	}

	private void UNetVersion()
	{
	}

	public override bool OnSerialize(NetworkWriter writer, bool forceAll)
	{
		if (forceAll)
		{
			writer.Write(_entryTime);
			writer.Write(_entryStopwatchValue);
			writer.Write(_entryDifficultyCoefficient);
			writer.WritePackedUInt32((uint)_singleMonsterTypeBodyIndex);
			writer.Write(_stageAdvanceTime);
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
			writer.Write(_entryTime);
		}
		if ((((NetworkBehaviour)this).syncVarDirtyBits & 2u) != 0)
		{
			if (!flag)
			{
				writer.WritePackedUInt32(((NetworkBehaviour)this).syncVarDirtyBits);
				flag = true;
			}
			writer.Write(_entryStopwatchValue);
		}
		if ((((NetworkBehaviour)this).syncVarDirtyBits & 4u) != 0)
		{
			if (!flag)
			{
				writer.WritePackedUInt32(((NetworkBehaviour)this).syncVarDirtyBits);
				flag = true;
			}
			writer.Write(_entryDifficultyCoefficient);
		}
		if ((((NetworkBehaviour)this).syncVarDirtyBits & 8u) != 0)
		{
			if (!flag)
			{
				writer.WritePackedUInt32(((NetworkBehaviour)this).syncVarDirtyBits);
				flag = true;
			}
			writer.WritePackedUInt32((uint)_singleMonsterTypeBodyIndex);
		}
		if ((((NetworkBehaviour)this).syncVarDirtyBits & 0x10u) != 0)
		{
			if (!flag)
			{
				writer.WritePackedUInt32(((NetworkBehaviour)this).syncVarDirtyBits);
				flag = true;
			}
			writer.Write(_stageAdvanceTime);
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
			_entryTime = reader.ReadSingle();
			_entryStopwatchValue = reader.ReadSingle();
			_entryDifficultyCoefficient = reader.ReadSingle();
			_singleMonsterTypeBodyIndex = (int)reader.ReadPackedUInt32();
			_stageAdvanceTime = reader.ReadSingle();
			return;
		}
		int num = (int)reader.ReadPackedUInt32();
		if (((uint)num & (true ? 1u : 0u)) != 0)
		{
			_entryTime = reader.ReadSingle();
		}
		if (((uint)num & 2u) != 0)
		{
			_entryStopwatchValue = reader.ReadSingle();
		}
		if (((uint)num & 4u) != 0)
		{
			_entryDifficultyCoefficient = reader.ReadSingle();
		}
		if (((uint)num & 8u) != 0)
		{
			_singleMonsterTypeBodyIndex = (int)reader.ReadPackedUInt32();
		}
		if (((uint)num & 0x10u) != 0)
		{
			_stageAdvanceTime = reader.ReadSingle();
		}
	}

	public override void PreStartClient()
	{
	}
}
