using System;
using RoR2.Navigation;
using UnityEngine;
using UnityEngine.Networking;

namespace RoR2;

public class MoonMissionController : NetworkBehaviour
{
	private Xoroshiro128Plus rng;

	public static MoonMissionController instance { get; private set; }

	private void OnEnable()
	{
		instance = SingletonHelper.Assign<MoonMissionController>(instance, this);
		SceneDirector.onPreGeneratePlayerSpawnPointsServer += OnPreGeneratePlayerSpawnPointsServer;
	}

	private void OnDisable()
	{
		SceneDirector.onPreGeneratePlayerSpawnPointsServer -= OnPreGeneratePlayerSpawnPointsServer;
		instance = SingletonHelper.Unassign<MoonMissionController>(instance, this);
	}

	[Server]
	public override void OnStartServer()
	{
		//IL_002c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0036: Expected O, but got Unknown
		if (!NetworkServer.active)
		{
			Debug.LogWarning((object)"[Server] function 'System.Void RoR2.MoonMissionController::OnStartServer()' called on client");
			return;
		}
		((NetworkBehaviour)this).OnStartServer();
		rng = new Xoroshiro128Plus((ulong)Run.instance.stageRng.nextUint);
	}

	private void OnPreGeneratePlayerSpawnPointsServer(SceneDirector sceneDirector, ref Action generationMethod)
	{
		generationMethod = GeneratePlayerSpawnPointsServer;
	}

	private void GeneratePlayerSpawnPointsServer()
	{
		//IL_002e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0033: Unknown result type (might be due to invalid IL or missing references)
		//IL_005f: Unknown result type (might be due to invalid IL or missing references)
		//IL_00be: Unknown result type (might be due to invalid IL or missing references)
		//IL_00c3: Unknown result type (might be due to invalid IL or missing references)
		//IL_00c5: Unknown result type (might be due to invalid IL or missing references)
		//IL_00c7: Unknown result type (might be due to invalid IL or missing references)
		if (!Object.op_Implicit((Object)(object)SceneInfo.instance))
		{
			return;
		}
		ChildLocator component = ((Component)SceneInfo.instance).GetComponent<ChildLocator>();
		if (!Object.op_Implicit((Object)(object)component))
		{
			return;
		}
		Transform val = component.FindChild("PlayerSpawnOrigin");
		Vector3 position = val.position;
		NodeGraph groundNodes = SceneInfo.instance.groundNodes;
		if (!Object.op_Implicit((Object)(object)groundNodes))
		{
			Debug.LogError((object)"MoonMissionController.GeneratePlayerSpawnPointsServer: No ground nodegraph found to place spawn points.", (Object)(object)this);
			return;
		}
		NodeGraphSpider nodeGraphSpider = new NodeGraphSpider(groundNodes, HullMask.Human);
		nodeGraphSpider.AddNodeForNextStep(groundNodes.FindClosestNode(position, HullClassification.Human));
		for (int i = 0; i < 4; i++)
		{
			nodeGraphSpider.PerformStep();
			if (nodeGraphSpider.collectedSteps.Count > 16)
			{
				break;
			}
		}
		for (int j = 0; j < nodeGraphSpider.collectedSteps.Count; j++)
		{
			NodeGraphSpider.StepInfo stepInfo = nodeGraphSpider.collectedSteps[j];
			groundNodes.GetNodePosition(stepInfo.node, out var position2);
			Quaternion rotation = val.rotation;
			SpawnPoint.AddSpawnPoint(position2, rotation);
		}
	}

	private void UNetVersion()
	{
	}

	public override bool OnSerialize(NetworkWriter writer, bool forceAll)
	{
		bool result = default(bool);
		return result;
	}

	public override void OnDeserialize(NetworkReader reader, bool initialState)
	{
	}

	public override void PreStartClient()
	{
	}
}
