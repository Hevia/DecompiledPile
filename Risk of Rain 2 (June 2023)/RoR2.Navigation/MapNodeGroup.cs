using System.Collections.Generic;
using System.Collections.ObjectModel;
using UnityEngine;

namespace RoR2.Navigation;

public class MapNodeGroup : MonoBehaviour
{
	public enum GraphType
	{
		Ground,
		Air,
		Rail
	}

	public NodeGraph nodeGraph;

	public Transform testPointA;

	public Transform testPointB;

	public HullClassification debugHullDef;

	public GraphType graphType;

	public void Clear()
	{
		for (int num = ((Component)this).transform.childCount - 1; num >= 0; num--)
		{
			Object.DestroyImmediate((Object)(object)((Component)((Component)this).transform.GetChild(num)).gameObject);
		}
	}

	public GameObject AddNode(Vector3 position)
	{
		//IL_0000: Unknown result type (might be due to invalid IL or missing references)
		//IL_0005: Unknown result type (might be due to invalid IL or missing references)
		//IL_000b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0011: Unknown result type (might be due to invalid IL or missing references)
		//IL_0022: Unknown result type (might be due to invalid IL or missing references)
		//IL_0029: Unknown result type (might be due to invalid IL or missing references)
		//IL_0035: Expected O, but got Unknown
		GameObject val = new GameObject();
		val.transform.position = position;
		val.transform.parent = ((Component)this).transform;
		val.AddComponent<MapNode>();
		((Object)val).name = "MapNode";
		return val;
	}

	public List<MapNode> GetNodes()
	{
		List<MapNode> list = new List<MapNode>();
		((Component)this).GetComponentsInChildren<MapNode>(false, list);
		return list;
	}

	public void UpdateNoCeilingMasks()
	{
		int num = 0;
		foreach (MapNode node in GetNodes())
		{
			node.flags &= ~NodeFlags.NoCeiling;
			if (node.TestNoCeiling())
			{
				num++;
				node.flags |= NodeFlags.NoCeiling;
			}
		}
		Debug.LogFormat("{0} successful ceiling masks baked.", new object[1] { num });
	}

	public void UpdateTeleporterMasks()
	{
		int num = 0;
		foreach (MapNode node in GetNodes())
		{
			node.flags &= ~NodeFlags.TeleporterOK;
			if (node.TestTeleporterOK())
			{
				num++;
				node.flags |= NodeFlags.TeleporterOK;
			}
		}
		Debug.LogFormat("{0} successful teleporter masks baked.", new object[1] { num });
	}

	public void Bake(NodeGraph nodeGraph)
	{
		List<MapNode> nodes = GetNodes();
		ReadOnlyCollection<MapNode> readOnlyCollection = nodes.AsReadOnly();
		for (int i = 0; i < nodes.Count; i++)
		{
			nodes[i].BuildLinks(readOnlyCollection, graphType);
		}
		List<SerializableBitArray> list = new List<SerializableBitArray>();
		for (int j = 0; j < nodes.Count; j++)
		{
			MapNode mapNode = nodes[j];
			SerializableBitArray serializableBitArray = new SerializableBitArray(nodes.Count);
			for (int k = 0; k < nodes.Count; k++)
			{
				MapNode other = nodes[k];
				serializableBitArray[k] = mapNode.TestLineOfSight(other);
			}
			list.Add(serializableBitArray);
		}
		nodeGraph.SetNodes(readOnlyCollection, list.AsReadOnly());
	}
}
