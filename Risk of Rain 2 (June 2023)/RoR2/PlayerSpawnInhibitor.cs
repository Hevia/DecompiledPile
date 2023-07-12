using System.Collections.Generic;
using System.Collections.ObjectModel;
using RoR2.Navigation;
using UnityEngine;

namespace RoR2;

public class PlayerSpawnInhibitor : MonoBehaviour
{
	private static List<PlayerSpawnInhibitor> instancesList = new List<PlayerSpawnInhibitor>();

	private static ReadOnlyCollection<PlayerSpawnInhibitor> _readOnlyInstancesList = new ReadOnlyCollection<PlayerSpawnInhibitor>(instancesList);

	[SerializeField]
	private float radius;

	[SerializeField]
	private Transform center;

	public static ReadOnlyCollection<PlayerSpawnInhibitor> readOnlyInstancesList => _readOnlyInstancesList;

	private void OnEnable()
	{
		instancesList.Add(this);
	}

	private void OnDisable()
	{
		instancesList.Remove(this);
	}

	public bool IsInhibiting(NodeGraph nodeGraph, NodeGraph.NodeIndex nodeIndex)
	{
		//IL_0019: Unknown result type (might be due to invalid IL or missing references)
		//IL_001e: Unknown result type (might be due to invalid IL or missing references)
		//IL_001f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0024: Unknown result type (might be due to invalid IL or missing references)
		if (Object.op_Implicit((Object)(object)nodeGraph) && nodeGraph.GetNodePosition(nodeIndex, out var position))
		{
			Vector3 val = center.position - position;
			return ((Vector3)(ref val)).sqrMagnitude < radius * radius;
		}
		return false;
	}
}
