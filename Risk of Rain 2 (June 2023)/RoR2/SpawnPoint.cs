using System.Collections.Generic;
using System.Collections.ObjectModel;
using HG;
using RoR2.Navigation;
using UnityEngine;

namespace RoR2;

public class SpawnPoint : MonoBehaviour
{
	private static List<SpawnPoint> instancesList = new List<SpawnPoint>();

	private static ReadOnlyCollection<SpawnPoint> _readOnlyInstancesList = new ReadOnlyCollection<SpawnPoint>(instancesList);

	[Tooltip("Flagged when a player spawns on this position, to stop overlapping spawn positions")]
	public bool consumed;

	private static GameObject prefab;

	private static Mesh commandoMesh;

	private static bool attemptedCommandoMeshLoad = false;

	public static ReadOnlyCollection<SpawnPoint> readOnlyInstancesList => _readOnlyInstancesList;

	private void OnEnable()
	{
		instancesList.Add(this);
	}

	public static SpawnPoint ConsumeSpawnPoint()
	{
		if (instancesList.Count == 0)
		{
			return null;
		}
		SpawnPoint spawnPoint = null;
		for (int i = 0; i < readOnlyInstancesList.Count; i++)
		{
			if (!readOnlyInstancesList[i].consumed)
			{
				spawnPoint = readOnlyInstancesList[i];
				readOnlyInstancesList[i].consumed = true;
				break;
			}
		}
		if (!Object.op_Implicit((Object)(object)spawnPoint))
		{
			for (int j = 0; j < readOnlyInstancesList.Count; j++)
			{
				readOnlyInstancesList[j].consumed = false;
			}
			spawnPoint = readOnlyInstancesList[0];
		}
		return spawnPoint;
	}

	private void OnDisable()
	{
		instancesList.Remove(this);
	}

	private static Mesh GetCommandoMesh()
	{
		if (!Object.op_Implicit((Object)(object)commandoMesh))
		{
			LoadCommandoMesh();
		}
		return commandoMesh;
	}

	private static void LoadCommandoMesh()
	{
		if (attemptedCommandoMeshLoad)
		{
			return;
		}
		attemptedCommandoMeshLoad = true;
		GameObject val = null;
		val = LegacyResourcesAPI.Load<GameObject>("Prefabs/CharacterBodies/CommandoBody");
		if (!Object.op_Implicit((Object)(object)val))
		{
			return;
		}
		CharacterModel component = ((Component)val.GetComponent<ModelLocator>().modelTransform).GetComponent<CharacterModel>();
		SkinnedMeshRenderer val2 = null;
		for (int i = 0; i < component.baseRendererInfos.Length; i++)
		{
			Renderer renderer = component.baseRendererInfos[i].renderer;
			val2 = (SkinnedMeshRenderer)(object)((renderer is SkinnedMeshRenderer) ? renderer : null);
			if (Object.op_Implicit((Object)(object)val2))
			{
				break;
			}
		}
		commandoMesh = val2.sharedMesh;
	}

	[RuntimeInitializeOnLoadMethod]
	private static void Init()
	{
		prefab = LegacyResourcesAPI.Load<GameObject>("Prefabs/SpawnPoint");
	}

	public static void AddSpawnPoint(NodeGraph nodeGraph, NodeGraph.NodeIndex nodeIndex, Xoroshiro128Plus rng)
	{
		//IL_0039: Unknown result type (might be due to invalid IL or missing references)
		//IL_003b: Unknown result type (might be due to invalid IL or missing references)
		//IL_003c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0041: Unknown result type (might be due to invalid IL or missing references)
		//IL_0046: Unknown result type (might be due to invalid IL or missing references)
		//IL_005f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0064: Unknown result type (might be due to invalid IL or missing references)
		//IL_0065: Unknown result type (might be due to invalid IL or missing references)
		//IL_0066: Unknown result type (might be due to invalid IL or missing references)
		nodeGraph.GetNodePosition(nodeIndex, out var position);
		List<NodeGraph.LinkIndex> list = CollectionPool<NodeGraph.LinkIndex, List<NodeGraph.LinkIndex>>.RentCollection();
		nodeGraph.GetActiveNodeLinks(nodeIndex, list);
		Quaternion rotation;
		if (list.Count > 0)
		{
			NodeGraph.LinkIndex linkIndex = rng.NextElementUniform<NodeGraph.LinkIndex>(list);
			nodeGraph.GetNodePosition(nodeGraph.GetLinkEndNode(linkIndex), out var position2);
			rotation = Util.QuaternionSafeLookRotation(position2 - position);
		}
		else
		{
			rotation = Quaternion.Euler(0f, rng.nextNormalizedFloat * 360f, 0f);
		}
		AddSpawnPoint(position, rotation);
		list = CollectionPool<NodeGraph.LinkIndex, List<NodeGraph.LinkIndex>>.ReturnCollection(list);
	}

	public static void AddSpawnPoint(Vector3 position, Quaternion rotation)
	{
		//IL_0005: Unknown result type (might be due to invalid IL or missing references)
		//IL_0006: Unknown result type (might be due to invalid IL or missing references)
		Object.Instantiate<GameObject>(prefab, position, rotation);
	}

	public void OnDrawGizmos()
	{
		//IL_0006: Unknown result type (might be due to invalid IL or missing references)
		//IL_0010: Unknown result type (might be due to invalid IL or missing references)
		Gizmos.matrix = ((Component)this).transform.localToWorldMatrix;
		Gizmos.color = Color.green;
		Gizmos.DrawWireMesh(GetCommandoMesh());
	}
}
