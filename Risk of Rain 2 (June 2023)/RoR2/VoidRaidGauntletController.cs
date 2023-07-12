using System;
using System.Runtime.InteropServices;
using RoR2.Audio;
using RoR2.Navigation;
using UnityEngine;
using UnityEngine.Networking;

namespace RoR2;

public class VoidRaidGauntletController : NetworkBehaviour
{
	[Serializable]
	public class GauntletInfo
	{
		public Transform startPoint;

		public Transform effectRoot;

		public float startRadius = 100f;

		public MapZone exitZone;

		public MapZone entranceZone;

		public string gateName;
	}

	[Serializable]
	public class DonutInfo
	{
		public GameObject root;

		public Transform returnPoint;

		public float returnRadius = 1000f;

		public Transform crabPosition;

		public CombatDirector combatDirector;
	}

	[SerializeField]
	private DonutInfo initialDonut;

	[SerializeField]
	private DonutInfo[] followingDonuts;

	[SerializeField]
	private GauntletInfo[] gauntlets;

	[SerializeField]
	private BuffDef requiredBuffToKill;

	[SerializeField]
	private GameObject donutSkyboxObject;

	[SerializeField]
	private GameObject gauntletSkyboxObject;

	[SerializeField]
	private GameObject gauntletEffectPrefab;

	[SerializeField]
	private int initialSpawnSpiderSteps = 4;

	[SerializeField]
	private int maxInitialSpawnPoints = 16;

	[SerializeField]
	public InteractableSpawnCard outroPortalSpawnCard;

	[SerializeField]
	public float minOutroPortalDistance;

	[SerializeField]
	public float maxOutroPortalDistance;

	[SerializeField]
	private InteractableSpawnCard gauntletExtranceSpawnCard;

	[SerializeField]
	private ScriptedCombatEncounter[] phaseEncounters;

	[SerializeField]
	private LoopSoundDef gauntletActiveLoop;

	private GauntletInfo previousGauntlet;

	private DonutInfo previousDonut;

	private GauntletInfo currentGauntlet;

	private DonutInfo currentDonut;

	private CharacterBody bossBody;

	private LoopSoundManager.SoundLoopPtr gauntletActiveLoopPtr;

	[SyncVar(hook = "TryShuffleData")]
	private ulong rngSeed;

	private bool hasShuffled;

	private int gauntletIndex;

	private static int kRpcRpcStartActiveSoundLoop;

	private static int kRpcRpcActivateGate;

	private static int kRpcRpcActivateDonut;

	private static int kRpcRpcTryShuffleData;

	public static VoidRaidGauntletController instance { get; private set; }

	public ulong NetworkrngSeed
	{
		get
		{
			return rngSeed;
		}
		[param: In]
		set
		{
			if (NetworkServer.localClientActive && !((NetworkBehaviour)this).syncVarHookGuard)
			{
				((NetworkBehaviour)this).syncVarHookGuard = true;
				TryShuffleData(value);
				((NetworkBehaviour)this).syncVarHookGuard = false;
			}
			((NetworkBehaviour)this).SetSyncVar<ulong>(value, ref rngSeed, 1u);
		}
	}

	public void SetCurrentDonutCombatDirectorEnabled(bool isEnabled)
	{
		if (Object.op_Implicit((Object)(object)currentDonut?.combatDirector))
		{
			((Behaviour)currentDonut.combatDirector).enabled = isEnabled;
		}
	}

	public bool TryOpenGauntlet(Vector3 entrancePosition, NetworkInstanceId bossMasterId)
	{
		//IL_001f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0020: Unknown result type (might be due to invalid IL or missing references)
		//IL_0059: Unknown result type (might be due to invalid IL or missing references)
		//IL_01c9: Unknown result type (might be due to invalid IL or missing references)
		//IL_01df: Unknown result type (might be due to invalid IL or missing references)
		//IL_01e4: Unknown result type (might be due to invalid IL or missing references)
		//IL_0231: Unknown result type (might be due to invalid IL or missing references)
		//IL_0237: Expected O, but got Unknown
		//IL_0244: Unknown result type (might be due to invalid IL or missing references)
		//IL_0245: Unknown result type (might be due to invalid IL or missing references)
		if (gauntletIndex >= phaseEncounters.Length)
		{
			return false;
		}
		if (bossMasterId != NetworkInstanceId.Invalid)
		{
			ScriptedCombatEncounter scriptedCombatEncounter = phaseEncounters[gauntletIndex];
			if (!Object.op_Implicit((Object)(object)scriptedCombatEncounter) || scriptedCombatEncounter.combatSquad.memberCount != 0 || !scriptedCombatEncounter.combatSquad.HasContainedMember(bossMasterId))
			{
				return false;
			}
		}
		int destinationGauntletIndex = gauntletIndex;
		previousGauntlet = currentGauntlet;
		previousDonut = currentDonut;
		if (previousDonut != null && Object.op_Implicit((Object)(object)previousDonut.combatDirector))
		{
			previousDonut.combatDirector.monsterCredit = 0f;
			((Behaviour)previousDonut.combatDirector).enabled = false;
		}
		int num = gauntletIndex % followingDonuts.Length;
		currentDonut = followingDonuts[num];
		currentGauntlet = gauntlets[gauntletIndex % gauntlets.Length];
		gauntletIndex++;
		CallRpcTryShuffleData(rngSeed);
		if (Object.op_Implicit((Object)(object)currentDonut.root))
		{
			currentDonut.root.SetActive(true);
			CallRpcActivateDonut(num);
		}
		if ((Object)(object)SceneInfo.instance != (Object)null && !string.IsNullOrEmpty(currentGauntlet?.gateName))
		{
			SceneInfo.instance.SetGateState(currentGauntlet.gateName, gateEnabled: true);
			CallRpcActivateGate(currentGauntlet?.gateName);
		}
		if (Object.op_Implicit((Object)(object)currentGauntlet.effectRoot) && Object.op_Implicit((Object)(object)gauntletEffectPrefab))
		{
			EffectData effectData = new EffectData
			{
				origin = currentGauntlet.effectRoot.position,
				rotation = currentGauntlet.effectRoot.rotation
			};
			EffectManager.SpawnEffect(gauntletEffectPrefab, effectData, transmit: false);
		}
		if (Object.op_Implicit((Object)(object)previousDonut?.combatDirector))
		{
			((Behaviour)previousDonut.combatDirector).enabled = false;
		}
		Xoroshiro128Plus rng = new Xoroshiro128Plus(rngSeed + (ulong)gauntletIndex);
		DirectorPlacementRule placementRule = new DirectorPlacementRule
		{
			placementMode = DirectorPlacementRule.PlacementMode.NearestNode,
			position = entrancePosition
		};
		DirectorSpawnRequest directorSpawnRequest = new DirectorSpawnRequest(gauntletExtranceSpawnCard, placementRule, rng);
		directorSpawnRequest.onSpawnedServer = delegate(SpawnCard.SpawnResult result)
		{
			OnEntranceSpawned(result, destinationGauntletIndex);
		};
		DirectorCore.instance.TrySpawnObject(directorSpawnRequest);
		return true;
	}

	private void OnEntranceSpawned(SpawnCard.SpawnResult result, int destinationGauntletIndex)
	{
		if (result.success)
		{
			CallRpcStartActiveSoundLoop();
			currentGauntlet.entranceZone = result.spawnedInstance.GetComponentInChildren<MapZone>();
			result.spawnedInstance.GetComponent<VoidRaidGauntletEntranceController>().SetGauntletIndex(destinationGauntletIndex);
		}
	}

	private void OnEnable()
	{
		instance = SingletonHelper.Assign<VoidRaidGauntletController>(instance, this);
		SceneDirector.onPreGeneratePlayerSpawnPointsServer += OnPreGeneratePlayerSpawnPointsServer;
	}

	private void OnDisable()
	{
		StopActiveSoundLoop();
		SceneDirector.onPreGeneratePlayerSpawnPointsServer -= OnPreGeneratePlayerSpawnPointsServer;
		instance = SingletonHelper.Unassign<VoidRaidGauntletController>(instance, this);
	}

	private void OnPreGeneratePlayerSpawnPointsServer(SceneDirector sceneDirector, ref Action generationMethod)
	{
		generationMethod = GeneratePlayerSpawnPointsServer;
	}

	private void GeneratePlayerSpawnPointsServer()
	{
		//IL_003c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0041: Unknown result type (might be due to invalid IL or missing references)
		//IL_006b: Unknown result type (might be due to invalid IL or missing references)
		//IL_00d0: Unknown result type (might be due to invalid IL or missing references)
		//IL_00d5: Unknown result type (might be due to invalid IL or missing references)
		//IL_00d7: Unknown result type (might be due to invalid IL or missing references)
		//IL_00d9: Unknown result type (might be due to invalid IL or missing references)
		if (!Object.op_Implicit((Object)(object)SceneInfo.instance) || initialDonut == null || !Object.op_Implicit((Object)(object)initialDonut.returnPoint))
		{
			return;
		}
		Transform returnPoint = initialDonut.returnPoint;
		Vector3 position = returnPoint.position;
		NodeGraph groundNodes = SceneInfo.instance.groundNodes;
		if (!Object.op_Implicit((Object)(object)groundNodes))
		{
			Debug.LogError((object)"VoidRaidGauntletController.GeneratePlayerSpawnPointsServer: No ground nodegraph found to place spawn points.", (Object)(object)this);
			return;
		}
		NodeGraphSpider nodeGraphSpider = new NodeGraphSpider(groundNodes, HullMask.Human);
		nodeGraphSpider.AddNodeForNextStep(groundNodes.FindClosestNode(position, HullClassification.Human));
		for (int i = 0; i < initialSpawnSpiderSteps; i++)
		{
			nodeGraphSpider.PerformStep();
			if (nodeGraphSpider.collectedSteps.Count > maxInitialSpawnPoints)
			{
				break;
			}
		}
		for (int j = 0; j < nodeGraphSpider.collectedSteps.Count; j++)
		{
			NodeGraphSpider.StepInfo stepInfo = nodeGraphSpider.collectedSteps[j];
			groundNodes.GetNodePosition(stepInfo.node, out var position2);
			Quaternion rotation = returnPoint.rotation;
			SpawnPoint.AddSpawnPoint(position2, rotation);
		}
	}

	private void Start()
	{
		previousDonut = initialDonut;
		DonutInfo[] array = followingDonuts;
		foreach (DonutInfo donutInfo in array)
		{
			if (Object.op_Implicit((Object)(object)donutInfo?.root))
			{
				donutInfo.root.SetActive(false);
			}
		}
		if (NetworkServer.active)
		{
			TryShuffleData(Run.instance.stageRng.nextUlong);
			CallRpcTryShuffleData(rngSeed);
		}
	}

	public override void OnStartServer()
	{
		((NetworkBehaviour)this).OnStartServer();
		CallRpcTryShuffleData(rngSeed);
	}

	private void TryShuffleData(ulong seed)
	{
		//IL_0013: Unknown result type (might be due to invalid IL or missing references)
		//IL_0019: Expected O, but got Unknown
		NetworkrngSeed = seed;
		if (hasShuffled)
		{
			return;
		}
		Xoroshiro128Plus rng = new Xoroshiro128Plus(seed);
		Debug.Log((object)$"Shuffling with seed {seed}");
		Util.ShuffleArray(followingDonuts, rng);
		Util.ShuffleArray(gauntlets, rng);
		for (int i = 1; i < phaseEncounters.Length && i - 1 < followingDonuts.Length; i++)
		{
			DonutInfo donutInfo = followingDonuts[i - 1];
			ScriptedCombatEncounter scriptedCombatEncounter = phaseEncounters[i];
			int encounterIndex = i;
			scriptedCombatEncounter.onBeginEncounter += delegate(ScriptedCombatEncounter argEncounter)
			{
				OnBeginEncounter(argEncounter, encounterIndex);
			};
			if (scriptedCombatEncounter.spawns.Length != 0)
			{
				scriptedCombatEncounter.spawns[0].explicitSpawnPosition = donutInfo.crabPosition;
			}
		}
		for (int j = 0; j < gauntlets.Length && j < followingDonuts.Length; j++)
		{
			GauntletInfo gauntletInfo = gauntlets[j];
			DonutInfo donutInfo2 = followingDonuts[j];
			Debug.Log((object)("Pointing gauntlet " + gauntletInfo.gateName + " to " + ((Object)donutInfo2.root).name));
			if (Object.op_Implicit((Object)(object)gauntletInfo.exitZone) && Object.op_Implicit((Object)(object)donutInfo2.returnPoint))
			{
				gauntletInfo.exitZone.explicitDestination = donutInfo2.returnPoint;
				gauntletInfo.exitZone.destinationIdealRadius = donutInfo2.returnRadius;
			}
		}
		hasShuffled = true;
	}

	private void OnBeginEncounter(ScriptedCombatEncounter encounter, int encounterIndex)
	{
		//IL_000e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0013: Unknown result type (might be due to invalid IL or missing references)
		while (gauntletIndex < encounterIndex)
		{
			TryOpenGauntlet(currentDonut.crabPosition.position, NetworkInstanceId.Invalid);
		}
	}

	public void SpawnOutroPortal()
	{
		//IL_002a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0030: Expected O, but got Unknown
		if (NetworkServer.active && currentDonut != null && Object.op_Implicit((Object)(object)currentDonut.returnPoint))
		{
			Xoroshiro128Plus rng = new Xoroshiro128Plus(rngSeed + 1);
			DirectorPlacementRule placementRule = new DirectorPlacementRule
			{
				placementMode = DirectorPlacementRule.PlacementMode.Approximate,
				minDistance = minOutroPortalDistance,
				maxDistance = maxOutroPortalDistance,
				spawnOnTarget = currentDonut.returnPoint
			};
			DirectorSpawnRequest directorSpawnRequest = new DirectorSpawnRequest(outroPortalSpawnCard, placementRule, rng);
			DirectorCore.instance.TrySpawnObject(directorSpawnRequest);
		}
	}

	[ClientRpc]
	private void RpcStartActiveSoundLoop()
	{
		if (Object.op_Implicit((Object)(object)gauntletActiveLoop))
		{
			gauntletActiveLoopPtr = LoopSoundManager.PlaySoundLoopLocal(((Component)RoR2Application.instance).gameObject, gauntletActiveLoop);
		}
	}

	[ClientRpc]
	private void RpcActivateGate(string gateName)
	{
		if (!string.IsNullOrEmpty(gateName))
		{
			SceneInfo.instance.SetGateState(gateName, gateEnabled: true);
		}
	}

	[ClientRpc]
	private void RpcActivateDonut(int donutIndex)
	{
		if (donutIndex < followingDonuts.Length)
		{
			DonutInfo donutInfo = followingDonuts[donutIndex];
			if (donutInfo != null)
			{
				Debug.Log((object)("Setting " + ((Object)donutInfo.root).name + " to active"));
				donutInfo.root.SetActive(true);
			}
		}
	}

	[ClientRpc]
	private void RpcTryShuffleData(ulong seed)
	{
		TryShuffleData(seed);
	}

	private void StopActiveSoundLoop()
	{
		if (Object.op_Implicit((Object)(object)gauntletActiveLoop))
		{
			LoopSoundManager.StopSoundLoopLocal(gauntletActiveLoopPtr);
		}
	}

	private void SetSkyboxToGauntlet()
	{
		if (Object.op_Implicit((Object)(object)donutSkyboxObject))
		{
			donutSkyboxObject.SetActive(false);
		}
		if (Object.op_Implicit((Object)(object)gauntletSkyboxObject))
		{
			gauntletSkyboxObject.SetActive(true);
		}
	}

	private void SetSkyboxToDonut()
	{
		if (Object.op_Implicit((Object)(object)donutSkyboxObject))
		{
			donutSkyboxObject.SetActive(true);
		}
		if (Object.op_Implicit((Object)(object)gauntletSkyboxObject))
		{
			gauntletSkyboxObject.SetActive(false);
		}
	}

	public void OnAuthorityPlayerEnter()
	{
		SetSkyboxToGauntlet();
	}

	public void OnAuthorityPlayerExit()
	{
		SetSkyboxToDonut();
		StopActiveSoundLoop();
	}

	public void PointZoneToGauntlet(int destinationGauntletIndex, MapZone zone)
	{
		if (destinationGauntletIndex < gauntlets.Length)
		{
			GauntletInfo gauntletInfo = gauntlets[destinationGauntletIndex];
			zone.explicitDestination = gauntletInfo.startPoint;
			zone.destinationIdealRadius = gauntletInfo.startRadius;
		}
	}

	private void UNetVersion()
	{
	}

	protected static void InvokeRpcRpcStartActiveSoundLoop(NetworkBehaviour obj, NetworkReader reader)
	{
		if (!NetworkClient.active)
		{
			Debug.LogError((object)"RPC RpcStartActiveSoundLoop called on server.");
		}
		else
		{
			((VoidRaidGauntletController)(object)obj).RpcStartActiveSoundLoop();
		}
	}

	protected static void InvokeRpcRpcActivateGate(NetworkBehaviour obj, NetworkReader reader)
	{
		if (!NetworkClient.active)
		{
			Debug.LogError((object)"RPC RpcActivateGate called on server.");
		}
		else
		{
			((VoidRaidGauntletController)(object)obj).RpcActivateGate(reader.ReadString());
		}
	}

	protected static void InvokeRpcRpcActivateDonut(NetworkBehaviour obj, NetworkReader reader)
	{
		if (!NetworkClient.active)
		{
			Debug.LogError((object)"RPC RpcActivateDonut called on server.");
		}
		else
		{
			((VoidRaidGauntletController)(object)obj).RpcActivateDonut((int)reader.ReadPackedUInt32());
		}
	}

	protected static void InvokeRpcRpcTryShuffleData(NetworkBehaviour obj, NetworkReader reader)
	{
		if (!NetworkClient.active)
		{
			Debug.LogError((object)"RPC RpcTryShuffleData called on server.");
		}
		else
		{
			((VoidRaidGauntletController)(object)obj).RpcTryShuffleData(reader.ReadPackedUInt64());
		}
	}

	public void CallRpcStartActiveSoundLoop()
	{
		//IL_0016: Unknown result type (might be due to invalid IL or missing references)
		//IL_001c: Expected O, but got Unknown
		//IL_0041: Unknown result type (might be due to invalid IL or missing references)
		if (!NetworkServer.active)
		{
			Debug.LogError((object)"RPC Function RpcStartActiveSoundLoop called on client.");
			return;
		}
		NetworkWriter val = new NetworkWriter();
		val.Write((short)0);
		val.Write((short)2);
		val.WritePackedUInt32((uint)kRpcRpcStartActiveSoundLoop);
		val.Write(((Component)this).GetComponent<NetworkIdentity>().netId);
		((NetworkBehaviour)this).SendRPCInternal(val, 0, "RpcStartActiveSoundLoop");
	}

	public void CallRpcActivateGate(string gateName)
	{
		//IL_0016: Unknown result type (might be due to invalid IL or missing references)
		//IL_001c: Expected O, but got Unknown
		//IL_0041: Unknown result type (might be due to invalid IL or missing references)
		if (!NetworkServer.active)
		{
			Debug.LogError((object)"RPC Function RpcActivateGate called on client.");
			return;
		}
		NetworkWriter val = new NetworkWriter();
		val.Write((short)0);
		val.Write((short)2);
		val.WritePackedUInt32((uint)kRpcRpcActivateGate);
		val.Write(((Component)this).GetComponent<NetworkIdentity>().netId);
		val.Write(gateName);
		((NetworkBehaviour)this).SendRPCInternal(val, 0, "RpcActivateGate");
	}

	public void CallRpcActivateDonut(int donutIndex)
	{
		//IL_0016: Unknown result type (might be due to invalid IL or missing references)
		//IL_001c: Expected O, but got Unknown
		//IL_0041: Unknown result type (might be due to invalid IL or missing references)
		if (!NetworkServer.active)
		{
			Debug.LogError((object)"RPC Function RpcActivateDonut called on client.");
			return;
		}
		NetworkWriter val = new NetworkWriter();
		val.Write((short)0);
		val.Write((short)2);
		val.WritePackedUInt32((uint)kRpcRpcActivateDonut);
		val.Write(((Component)this).GetComponent<NetworkIdentity>().netId);
		val.WritePackedUInt32((uint)donutIndex);
		((NetworkBehaviour)this).SendRPCInternal(val, 0, "RpcActivateDonut");
	}

	public void CallRpcTryShuffleData(ulong seed)
	{
		//IL_0016: Unknown result type (might be due to invalid IL or missing references)
		//IL_001c: Expected O, but got Unknown
		//IL_0041: Unknown result type (might be due to invalid IL or missing references)
		if (!NetworkServer.active)
		{
			Debug.LogError((object)"RPC Function RpcTryShuffleData called on client.");
			return;
		}
		NetworkWriter val = new NetworkWriter();
		val.Write((short)0);
		val.Write((short)2);
		val.WritePackedUInt32((uint)kRpcRpcTryShuffleData);
		val.Write(((Component)this).GetComponent<NetworkIdentity>().netId);
		val.WritePackedUInt64(seed);
		((NetworkBehaviour)this).SendRPCInternal(val, 0, "RpcTryShuffleData");
	}

	static VoidRaidGauntletController()
	{
		//IL_0020: Unknown result type (might be due to invalid IL or missing references)
		//IL_002a: Expected O, but got Unknown
		//IL_004a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0054: Expected O, but got Unknown
		//IL_0074: Unknown result type (might be due to invalid IL or missing references)
		//IL_007e: Expected O, but got Unknown
		//IL_009e: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a8: Expected O, but got Unknown
		kRpcRpcStartActiveSoundLoop = -755317963;
		NetworkBehaviour.RegisterRpcDelegate(typeof(VoidRaidGauntletController), kRpcRpcStartActiveSoundLoop, new CmdDelegate(InvokeRpcRpcStartActiveSoundLoop));
		kRpcRpcActivateGate = -1148984728;
		NetworkBehaviour.RegisterRpcDelegate(typeof(VoidRaidGauntletController), kRpcRpcActivateGate, new CmdDelegate(InvokeRpcRpcActivateGate));
		kRpcRpcActivateDonut = -1261146843;
		NetworkBehaviour.RegisterRpcDelegate(typeof(VoidRaidGauntletController), kRpcRpcActivateDonut, new CmdDelegate(InvokeRpcRpcActivateDonut));
		kRpcRpcTryShuffleData = 20528402;
		NetworkBehaviour.RegisterRpcDelegate(typeof(VoidRaidGauntletController), kRpcRpcTryShuffleData, new CmdDelegate(InvokeRpcRpcTryShuffleData));
		NetworkCRC.RegisterBehaviour("VoidRaidGauntletController", 0);
	}

	public override bool OnSerialize(NetworkWriter writer, bool forceAll)
	{
		if (forceAll)
		{
			writer.WritePackedUInt64(rngSeed);
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
			writer.WritePackedUInt64(rngSeed);
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
			rngSeed = reader.ReadPackedUInt64();
			return;
		}
		int num = (int)reader.ReadPackedUInt32();
		if (((uint)num & (true ? 1u : 0u)) != 0)
		{
			TryShuffleData(reader.ReadPackedUInt64());
		}
	}

	public override void PreStartClient()
	{
	}
}
