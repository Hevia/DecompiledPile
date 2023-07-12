using System;
using System.Collections.Generic;
using JetBrains.Annotations;
using RoR2.Navigation;
using UnityEngine;

namespace RoR2;

public class DirectorCore : MonoBehaviour
{
	private struct NodeReference : IEquatable<NodeReference>
	{
		public readonly NodeGraph nodeGraph;

		public readonly NodeGraph.NodeIndex nodeIndex;

		public NodeReference(NodeGraph nodeGraph, NodeGraph.NodeIndex nodeIndex)
		{
			this.nodeGraph = nodeGraph;
			this.nodeIndex = nodeIndex;
		}

		public bool Equals(NodeReference other)
		{
			if (object.Equals(nodeGraph, other.nodeGraph))
			{
				return nodeIndex.Equals(other.nodeIndex);
			}
			return false;
		}

		public override bool Equals(object obj)
		{
			if (obj == null)
			{
				return false;
			}
			if (obj is NodeReference other)
			{
				return Equals(other);
			}
			return false;
		}

		public override int GetHashCode()
		{
			int num = (((Object)(object)nodeGraph != (Object)null) ? ((object)nodeGraph).GetHashCode() : 0) * 397;
			NodeGraph.NodeIndex nodeIndex = this.nodeIndex;
			return num ^ nodeIndex.GetHashCode();
		}
	}

	public enum MonsterSpawnDistance
	{
		Standard,
		Close,
		Far
	}

	public static List<GameObject> spawnedObjects = new List<GameObject>();

	private NodeReference[] occupiedNodes = Array.Empty<NodeReference>();

	public static DirectorCore instance { get; private set; }

	public GameObject[] GetObjectsOfTeam(TeamIndex _teamIndex)
	{
		List<GameObject> list = new List<GameObject>();
		for (int i = 0; i < spawnedObjects.Count; i++)
		{
			CharacterMaster component = spawnedObjects[i].GetComponent<CharacterMaster>();
			if (Object.op_Implicit((Object)(object)component) && component.teamIndex == _teamIndex)
			{
				spawnedObjects.Add(spawnedObjects[i]);
			}
		}
		return list.ToArray();
	}

	private void OnEnable()
	{
		if (!Object.op_Implicit((Object)(object)instance))
		{
			instance = this;
			return;
		}
		Debug.LogErrorFormat((Object)(object)this, "Duplicate instance of singleton class {0}. Only one should exist at a time.", new object[1] { ((object)this).GetType().Name });
	}

	private void OnDisable()
	{
		if ((Object)(object)instance == (Object)(object)this)
		{
			instance = null;
		}
	}

	public void AddOccupiedNode(NodeGraph nodeGraph, NodeGraph.NodeIndex nodeIndex)
	{
		Array.Resize(ref occupiedNodes, occupiedNodes.Length + 1);
		occupiedNodes[occupiedNodes.Length - 1] = new NodeReference(nodeGraph, nodeIndex);
	}

	private bool CheckPositionFree(NodeGraph nodeGraph, NodeGraph.NodeIndex nodeIndex, SpawnCard spawnCard)
	{
		//IL_0043: Unknown result type (might be due to invalid IL or missing references)
		//IL_0044: Unknown result type (might be due to invalid IL or missing references)
		//IL_0050: Unknown result type (might be due to invalid IL or missing references)
		//IL_0055: Unknown result type (might be due to invalid IL or missing references)
		//IL_005a: Unknown result type (might be due to invalid IL or missing references)
		//IL_005b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0065: Unknown result type (might be due to invalid IL or missing references)
		//IL_0077: Unknown result type (might be due to invalid IL or missing references)
		//IL_008a: Unknown result type (might be due to invalid IL or missing references)
		if (Array.IndexOf(value: new NodeReference(nodeGraph, nodeIndex), array: occupiedNodes) != -1)
		{
			return false;
		}
		float num = HullDef.Find(spawnCard.hullSize).radius * 0.7f;
		nodeGraph.GetNodePosition(nodeIndex, out var position);
		if (spawnCard.nodeGraphType == MapNodeGroup.GraphType.Ground)
		{
			position += Vector3.up * (num + 0.25f);
		}
		return Physics.OverlapSphere(position, num, LayerMask.op_Implicit(LayerIndex.world.mask) | LayerMask.op_Implicit(LayerIndex.defaultLayer.mask) | LayerMask.op_Implicit(LayerIndex.fakeActor.mask)).Length == 0;
	}

	public GameObject TrySpawnObject([NotNull] DirectorSpawnRequest directorSpawnRequest)
	{
		//IL_00aa: Unknown result type (might be due to invalid IL or missing references)
		//IL_00af: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ca: Unknown result type (might be due to invalid IL or missing references)
		//IL_00dc: Unknown result type (might be due to invalid IL or missing references)
		//IL_00f3: Unknown result type (might be due to invalid IL or missing references)
		//IL_0102: Unknown result type (might be due to invalid IL or missing references)
		//IL_0120: Unknown result type (might be due to invalid IL or missing references)
		//IL_018c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0239: Unknown result type (might be due to invalid IL or missing references)
		//IL_023d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0242: Unknown result type (might be due to invalid IL or missing references)
		//IL_0245: Unknown result type (might be due to invalid IL or missing references)
		//IL_0247: Unknown result type (might be due to invalid IL or missing references)
		//IL_029b: Unknown result type (might be due to invalid IL or missing references)
		//IL_02d0: Unknown result type (might be due to invalid IL or missing references)
		//IL_02d4: Unknown result type (might be due to invalid IL or missing references)
		//IL_02d9: Unknown result type (might be due to invalid IL or missing references)
		//IL_02dc: Unknown result type (might be due to invalid IL or missing references)
		//IL_02de: Unknown result type (might be due to invalid IL or missing references)
		//IL_0322: Unknown result type (might be due to invalid IL or missing references)
		//IL_0380: Unknown result type (might be due to invalid IL or missing references)
		//IL_03c1: Unknown result type (might be due to invalid IL or missing references)
		//IL_03c5: Unknown result type (might be due to invalid IL or missing references)
		//IL_03ca: Unknown result type (might be due to invalid IL or missing references)
		//IL_03cd: Unknown result type (might be due to invalid IL or missing references)
		//IL_03cf: Unknown result type (might be due to invalid IL or missing references)
		//IL_0422: Unknown result type (might be due to invalid IL or missing references)
		//IL_053a: Unknown result type (might be due to invalid IL or missing references)
		//IL_053f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0542: Unknown result type (might be due to invalid IL or missing references)
		//IL_0544: Unknown result type (might be due to invalid IL or missing references)
		//IL_05bc: Unknown result type (might be due to invalid IL or missing references)
		//IL_05c1: Unknown result type (might be due to invalid IL or missing references)
		//IL_05c8: Unknown result type (might be due to invalid IL or missing references)
		//IL_05d4: Unknown result type (might be due to invalid IL or missing references)
		//IL_05e6: Unknown result type (might be due to invalid IL or missing references)
		//IL_05f2: Unknown result type (might be due to invalid IL or missing references)
		//IL_0604: Unknown result type (might be due to invalid IL or missing references)
		//IL_0610: Unknown result type (might be due to invalid IL or missing references)
		//IL_0625: Unknown result type (might be due to invalid IL or missing references)
		//IL_0663: Unknown result type (might be due to invalid IL or missing references)
		//IL_0667: Unknown result type (might be due to invalid IL or missing references)
		//IL_066c: Unknown result type (might be due to invalid IL or missing references)
		//IL_066f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0671: Unknown result type (might be due to invalid IL or missing references)
		//IL_06b5: Unknown result type (might be due to invalid IL or missing references)
		SpawnCard spawnCard = directorSpawnRequest.spawnCard;
		DirectorPlacementRule placementRule = directorSpawnRequest.placementRule;
		Xoroshiro128Plus rng = directorSpawnRequest.rng;
		NodeGraph nodeGraph = SceneInfo.instance.GetNodeGraph(spawnCard.nodeGraphType);
		if ((Object)(object)nodeGraph == (Object)null)
		{
			Debug.LogError((object)$"Unable to find nodegraph for {SceneInfo.instance.sceneDef.cachedName} of type {spawnCard.nodeGraphType}.");
			return null;
		}
		GameObject result = null;
		switch (placementRule.placementMode)
		{
		case DirectorPlacementRule.PlacementMode.Direct:
		{
			Quaternion val = Quaternion.Euler(0f, rng.nextNormalizedFloat * 360f, 0f);
			result = spawnCard.DoSpawn(Object.op_Implicit((Object)(object)placementRule.spawnOnTarget) ? placementRule.spawnOnTarget.position : directorSpawnRequest.placementRule.position, Object.op_Implicit((Object)(object)placementRule.spawnOnTarget) ? placementRule.spawnOnTarget.rotation : val, directorSpawnRequest).spawnedInstance;
			break;
		}
		case DirectorPlacementRule.PlacementMode.Approximate:
		{
			List<NodeGraph.NodeIndex> list = nodeGraph.FindNodesInRangeWithFlagConditions(placementRule.targetPosition, placementRule.minDistance, placementRule.maxDistance, (HullMask)(1 << (int)spawnCard.hullSize), spawnCard.requiredFlags, spawnCard.forbiddenFlags, placementRule.preventOverhead);
			if (list.Count == 0)
			{
				Debug.Log((object)$"PlacementMode.Approximate:  could not find nodes satisfying conditions for {((Object)spawnCard).name}.  targetPosition={placementRule.targetPosition}, minDistance={placementRule.minDistance}, maxDistance={placementRule.maxDistance}, hullSize ={spawnCard.hullSize}, requiredFlags={spawnCard.requiredFlags}, forbiddenFlags={spawnCard.forbiddenFlags}, preventOverhead={placementRule.preventOverhead}");
			}
			while (list.Count > 0)
			{
				int index2 = rng.RangeInt(0, list.Count);
				NodeGraph.NodeIndex nodeIndex5 = list[index2];
				if (nodeGraph.GetNodePosition(nodeIndex5, out var position5) && CheckPositionFree(nodeGraph, nodeIndex5, spawnCard))
				{
					Quaternion rotation5 = GetRotationFacingTargetPositionFromPoint(position5);
					result = spawnCard.DoSpawn(position5, rotation5, directorSpawnRequest).spawnedInstance;
					if (spawnCard.occupyPosition)
					{
						AddOccupiedNode(nodeGraph, nodeIndex5);
					}
					break;
				}
				Debug.Log((object)"Position not free or not found.");
				list.RemoveAt(index2);
			}
			break;
		}
		case DirectorPlacementRule.PlacementMode.NearestNode:
		{
			NodeGraph.NodeIndex nodeIndex4 = nodeGraph.FindClosestNodeWithFlagConditions(placementRule.targetPosition, spawnCard.hullSize, spawnCard.requiredFlags, spawnCard.forbiddenFlags, placementRule.preventOverhead);
			if (nodeGraph.GetNodePosition(nodeIndex4, out var position4))
			{
				Quaternion rotation4 = GetRotationFacingTargetPositionFromPoint(position4);
				result = spawnCard.DoSpawn(position4, rotation4, directorSpawnRequest).spawnedInstance;
				if (spawnCard.occupyPosition)
				{
					AddOccupiedNode(nodeGraph, nodeIndex4);
				}
			}
			else
			{
				Debug.Log((object)$"PlacementMode.NearestNode:  could not find nodes satisfying conditions for {((Object)spawnCard).name}.  targetPosition={placementRule.targetPosition}, hullSize ={spawnCard.hullSize}, requiredFlags={spawnCard.requiredFlags}, forbiddenFlags={spawnCard.forbiddenFlags}, preventOverhead={placementRule.preventOverhead}");
			}
			break;
		}
		case DirectorPlacementRule.PlacementMode.ApproximateSimple:
		{
			NodeGraph.NodeIndex nodeIndex2 = nodeGraph.FindClosestNodeWithFlagConditions(placementRule.targetPosition, spawnCard.hullSize, spawnCard.requiredFlags, spawnCard.forbiddenFlags, placementRule.preventOverhead);
			if (nodeGraph.GetNodePosition(nodeIndex2, out var position2))
			{
				if (CheckPositionFree(nodeGraph, nodeIndex2, spawnCard))
				{
					Quaternion rotation2 = GetRotationFacingTargetPositionFromPoint(position2);
					result = spawnCard.DoSpawn(position2, rotation2, directorSpawnRequest).spawnedInstance;
					if (spawnCard.occupyPosition)
					{
						AddOccupiedNode(nodeGraph, nodeIndex2);
					}
				}
				else
				{
					Debug.Log((object)"Position not free.");
				}
			}
			else
			{
				Debug.Log((object)$"PlacementMode.ApproximateSimple:  could not find nodes satisfying conditions for {((Object)spawnCard).name}.  targetPosition={placementRule.targetPosition}, hullSize ={spawnCard.hullSize}, requiredFlags={spawnCard.requiredFlags}, forbiddenFlags={spawnCard.forbiddenFlags}, preventOverhead={placementRule.preventOverhead}");
			}
			break;
		}
		case DirectorPlacementRule.PlacementMode.Random:
		{
			List<NodeGraph.NodeIndex> activeNodesForHullMaskWithFlagConditions = nodeGraph.GetActiveNodesForHullMaskWithFlagConditions((HullMask)(1 << (int)spawnCard.hullSize), spawnCard.requiredFlags, spawnCard.forbiddenFlags);
			if (activeNodesForHullMaskWithFlagConditions.Count == 0)
			{
				Debug.Log((object)$"PlacementMode.Random:  could not find nodes satisfying conditions for {((Object)spawnCard).name}.  hullSize={spawnCard.hullSize}, requiredFlags={spawnCard.requiredFlags}, forbiddenFlags={spawnCard.forbiddenFlags}");
			}
			while (activeNodesForHullMaskWithFlagConditions.Count > 0)
			{
				int index = rng.RangeInt(0, activeNodesForHullMaskWithFlagConditions.Count);
				NodeGraph.NodeIndex nodeIndex3 = activeNodesForHullMaskWithFlagConditions[index];
				if (nodeGraph.GetNodePosition(nodeIndex3, out var position3) && CheckPositionFree(nodeGraph, nodeIndex3, spawnCard))
				{
					Quaternion rotation3 = Quaternion.Euler(0f, rng.nextNormalizedFloat * 360f, 0f);
					result = spawnCard.DoSpawn(position3, rotation3, directorSpawnRequest).spawnedInstance;
					if (spawnCard.occupyPosition)
					{
						AddOccupiedNode(nodeGraph, nodeIndex3);
					}
					break;
				}
				Debug.Log((object)"Position not free or not found.");
				activeNodesForHullMaskWithFlagConditions.RemoveAt(index);
			}
			break;
		}
		case DirectorPlacementRule.PlacementMode.RandomNormalized:
		{
			if ((Object)(object)SceneInfo.instance.approximateMapBoundMesh == (Object)null)
			{
				Debug.Log((object)"Approximate Map Bound is missing. Aborting.");
				break;
			}
			Bounds bounds = ((Renderer)SceneInfo.instance.approximateMapBoundMesh).bounds;
			Vector3 position = default(Vector3);
			((Vector3)(ref position))._002Ector(rng.RangeFloat(((Bounds)(ref bounds)).min.x, ((Bounds)(ref bounds)).max.x), rng.RangeFloat(((Bounds)(ref bounds)).min.y, ((Bounds)(ref bounds)).max.y), rng.RangeFloat(((Bounds)(ref bounds)).min.z, ((Bounds)(ref bounds)).max.z));
			NodeGraph.NodeIndex nodeIndex = nodeGraph.FindClosestNodeWithFlagConditions(position, spawnCard.hullSize, spawnCard.requiredFlags, spawnCard.forbiddenFlags, placementRule.preventOverhead);
			if (nodeGraph.GetNodePosition(nodeIndex, out position))
			{
				if (CheckPositionFree(nodeGraph, nodeIndex, spawnCard))
				{
					Quaternion rotation = GetRotationFacingTargetPositionFromPoint(position);
					result = spawnCard.DoSpawn(position, rotation, directorSpawnRequest).spawnedInstance;
					if (spawnCard.occupyPosition)
					{
						AddOccupiedNode(nodeGraph, nodeIndex);
					}
				}
				else
				{
					Debug.Log((object)"Position not free.");
				}
			}
			else
			{
				Debug.Log((object)$"PlacementMode.RandomNormalized:  could not find nodes satisfying conditions for {((Object)spawnCard).name}.  targetPosition={position}, hullSize ={spawnCard.hullSize}, requiredFlags={spawnCard.requiredFlags}, forbiddenFlags={spawnCard.forbiddenFlags}, preventOverhead={placementRule.preventOverhead}");
			}
			break;
		}
		}
		return result;
		Quaternion GetRotationFacingTargetPositionFromPoint(Vector3 point)
		{
			//IL_0006: Unknown result type (might be due to invalid IL or missing references)
			//IL_000b: Unknown result type (might be due to invalid IL or missing references)
			//IL_000e: Unknown result type (might be due to invalid IL or missing references)
			//IL_001f: Unknown result type (might be due to invalid IL or missing references)
			//IL_0024: Unknown result type (might be due to invalid IL or missing references)
			//IL_0025: Unknown result type (might be due to invalid IL or missing references)
			//IL_002a: Unknown result type (might be due to invalid IL or missing references)
			Vector3 targetPosition = placementRule.targetPosition;
			point.y = targetPosition.y;
			return Util.QuaternionSafeLookRotation(placementRule.targetPosition - point);
		}
	}

	public static void GetMonsterSpawnDistance(MonsterSpawnDistance input, out float minimumDistance, out float maximumDistance)
	{
		minimumDistance = 0f;
		maximumDistance = 0f;
		switch (input)
		{
		case MonsterSpawnDistance.Close:
			minimumDistance = 8f;
			maximumDistance = 20f;
			break;
		case MonsterSpawnDistance.Standard:
			minimumDistance = 25f;
			maximumDistance = 40f;
			break;
		case MonsterSpawnDistance.Far:
			minimumDistance = 70f;
			maximumDistance = 120f;
			break;
		}
	}
}
