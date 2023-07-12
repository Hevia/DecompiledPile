using System;
using System.Collections.Generic;
using HG;
using RoR2.Navigation;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.Serialization;

namespace RoR2;

public class SceneInfo : MonoBehaviour
{
	private static class NodeGraphOverlay
	{
		private static Dictionary<(MapNodeGroup.GraphType graphType, HullMask hullMask), (DebugOverlay.MeshDrawer drawer, Action updater)> drawers;

		private static void StaticUpdate()
		{
			foreach (KeyValuePair<(MapNodeGroup.GraphType, HullMask), (DebugOverlay.MeshDrawer, Action)> drawer in drawers)
			{
				drawer.Value.Item2();
			}
		}

		private static void SetGraphDrawEnabled(bool shouldDraw, MapNodeGroup.GraphType graphType, HullMask hullMask)
		{
			//IL_00a4: Unknown result type (might be due to invalid IL or missing references)
			//IL_00a9: Unknown result type (might be due to invalid IL or missing references)
			(MapNodeGroup.GraphType, HullMask) key = (graphType, hullMask);
			DebugOverlay.MeshDrawer drawer;
			GenericMemoizer<(DebugOverlay.MeshDrawer, SceneInfo, MapNodeGroup.GraphType, HullMask), Mesh> getNodeGraphMeshMemoizer;
			if (shouldDraw)
			{
				if (drawers == null)
				{
					drawers = new Dictionary<(MapNodeGroup.GraphType, HullMask), (DebugOverlay.MeshDrawer, Action)>();
					RoR2Application.onUpdate += StaticUpdate;
				}
				if (!drawers.ContainsKey(key))
				{
					drawer = DebugOverlay.GetMeshDrawer();
					drawer.hasMeshOwnership = true;
					drawer.material = DebugOverlay.defaultWireMaterial;
					getNodeGraphMeshMemoizer = new GenericMemoizer<(DebugOverlay.MeshDrawer, SceneInfo, MapNodeGroup.GraphType, HullMask), Mesh>((Func<(DebugOverlay.MeshDrawer, SceneInfo, MapNodeGroup.GraphType, HullMask), Mesh>)GetNodeGraphMesh);
					drawers.Add(key, (drawer, Updater));
				}
			}
			else if (drawers != null)
			{
				if (drawers.TryGetValue(key, out (DebugOverlay.MeshDrawer, Action) value))
				{
					value.Item1.Dispose();
					drawers.Remove(key);
				}
				if (drawers.Count == 0)
				{
					drawers = null;
					RoR2Application.onUpdate -= StaticUpdate;
				}
			}
			static Mesh GetNodeGraphMesh(in (DebugOverlay.MeshDrawer, SceneInfo sceneInfo, MapNodeGroup.GraphType graphType, HullMask hullMask) input)
			{
				return input.sceneInfo?.GetNodeGraph(input.graphType).GenerateLinkDebugMesh(input.hullMask);
			}
			void Updater()
			{
				DebugOverlay.MeshDrawer meshDrawer = drawer;
				ref GenericMemoizer<(DebugOverlay.MeshDrawer, SceneInfo, MapNodeGroup.GraphType, HullMask), Mesh> reference = ref getNodeGraphMeshMemoizer;
				(DebugOverlay.MeshDrawer, SceneInfo, MapNodeGroup.GraphType, HullMask) tuple = (drawer, instance, graphType, hullMask);
				meshDrawer.mesh = reference.Evaluate(ref tuple);
			}
		}

		[ConCommand(commandName = "debug_scene_draw_nodegraph", flags = ConVarFlags.Cheat, helpText = "Enables/disables overlay drawing of the specified nodegraph. Format: {shouldDraw} {graphType} {hullClassification, ...}")]
		private static void CCDebugSceneDrawNodegraph(ConCommandArgs args)
		{
			bool argBool = args.GetArgBool(0);
			MapNodeGroup.GraphType argEnum = args.GetArgEnum<MapNodeGroup.GraphType>(1);
			HullMask hullMask = (HullMask)(1 << (int)args.GetArgEnum<HullClassification>(2));
			if (hullMask == HullMask.None)
			{
				throw new ConCommandException("Cannot use HullMask.None.");
			}
			for (int i = 3; i < args.Count; i++)
			{
				HullClassification? hullClassification = args.TryGetArgEnum<HullClassification>(i);
				if (hullClassification.HasValue)
				{
					hullMask = (HullMask)((int)hullMask | (1 << (int)hullClassification.Value));
				}
			}
			SetGraphDrawEnabled(argBool, argEnum, hullMask);
		}
	}

	private static SceneInfo _instance;

	[FormerlySerializedAs("groundNodes")]
	public MapNodeGroup groundNodeGroup;

	[FormerlySerializedAs("airNodes")]
	public MapNodeGroup airNodeGroup;

	[FormerlySerializedAs("railNodes")]
	public MapNodeGroup railNodeGroup;

	public MeshRenderer approximateMapBoundMesh;

	[SerializeField]
	private NodeGraph groundNodesAsset;

	[SerializeField]
	private NodeGraph airNodesAsset;

	public static SceneInfo instance => _instance;

	public NodeGraph groundNodes { get; private set; }

	public NodeGraph airNodes { get; private set; }

	public NodeGraph railNodes { get; private set; }

	public SceneDef sceneDef { get; private set; }

	public bool countsAsStage
	{
		get
		{
			if (!Object.op_Implicit((Object)(object)sceneDef))
			{
				return false;
			}
			return sceneDef.sceneType == SceneType.Stage;
		}
	}

	private void Awake()
	{
		//IL_0026: Unknown result type (might be due to invalid IL or missing references)
		//IL_002b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0068: Unknown result type (might be due to invalid IL or missing references)
		//IL_006d: Unknown result type (might be due to invalid IL or missing references)
		//IL_008b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0090: Unknown result type (might be due to invalid IL or missing references)
		Scene scene;
		if (Object.op_Implicit((Object)(object)groundNodesAsset))
		{
			groundNodes = Object.Instantiate<NodeGraph>(groundNodesAsset);
		}
		else
		{
			scene = ((Component)this).gameObject.scene;
			Debug.LogWarning((object)(((Scene)(ref scene)).name + " has no groundNodesAsset"));
		}
		if (Object.op_Implicit((Object)(object)airNodesAsset))
		{
			airNodes = Object.Instantiate<NodeGraph>(airNodesAsset);
		}
		else
		{
			scene = ((Component)this).gameObject.scene;
			Debug.LogWarning((object)(((Scene)(ref scene)).name + " has no airNodesAsset"));
		}
		scene = ((Component)this).gameObject.scene;
		sceneDef = SceneCatalog.GetSceneDefFromSceneName(((Scene)(ref scene)).name);
	}

	private void OnDestroy()
	{
		Object.Destroy((Object)(object)groundNodes);
		Object.Destroy((Object)(object)airNodes);
		Object.Destroy((Object)(object)railNodes);
	}

	public MapNodeGroup GetNodeGroup(MapNodeGroup.GraphType nodeGraphType)
	{
		return nodeGraphType switch
		{
			MapNodeGroup.GraphType.Ground => groundNodeGroup, 
			MapNodeGroup.GraphType.Air => airNodeGroup, 
			MapNodeGroup.GraphType.Rail => railNodeGroup, 
			_ => null, 
		};
	}

	public NodeGraph GetNodeGraph(MapNodeGroup.GraphType nodeGraphType)
	{
		return nodeGraphType switch
		{
			MapNodeGroup.GraphType.Ground => groundNodes, 
			MapNodeGroup.GraphType.Air => airNodes, 
			MapNodeGroup.GraphType.Rail => railNodes, 
			_ => null, 
		};
	}

	public void SetGateState(string gateName, bool gateEnabled)
	{
		//IL_0067: Unknown result type (might be due to invalid IL or missing references)
		//IL_006c: Unknown result type (might be due to invalid IL or missing references)
		bool flag = false;
		if (Object.op_Implicit((Object)(object)groundNodes))
		{
			flag = groundNodes.TrySetGateState(gateName, gateEnabled) || flag;
		}
		if (Object.op_Implicit((Object)(object)airNodes))
		{
			flag = airNodes.TrySetGateState(gateName, gateEnabled) || flag;
		}
		if (Object.op_Implicit((Object)(object)railNodes))
		{
			flag = railNodes.TrySetGateState(gateName, gateEnabled) || flag;
		}
		if (!flag)
		{
			Scene scene = ((Component)this).gameObject.scene;
			Debug.LogError((object)$"Unable to set gate state for {((Scene)(ref scene)).name}: {gateName}={gateEnabled}");
		}
	}

	private void OnEnable()
	{
		if (!Object.op_Implicit((Object)(object)_instance))
		{
			_instance = this;
		}
	}

	private void OnDisable()
	{
		if ((Object)(object)_instance == (Object)(object)this)
		{
			_instance = null;
		}
	}

	private void OnValidate()
	{
		if (Object.op_Implicit((Object)(object)groundNodeGroup))
		{
			groundNodesAsset = groundNodeGroup.nodeGraph;
		}
		if (Object.op_Implicit((Object)(object)airNodeGroup))
		{
			airNodesAsset = airNodeGroup.nodeGraph;
		}
	}
}
