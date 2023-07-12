using RoR2;
using RoR2.Navigation;
using UnityEngine;
using UnityEngine.Networking;

namespace EntityStates.GrandParent;

public class ChannelSun : ChannelSunBase
{
	public static string animLayerName;

	public static string animStateName;

	public static GameObject sunPrefab;

	public static float sunPrefabDiameter = 10f;

	public static float sunPlacementMinDistance = 100f;

	public static float sunPlacementIdealAltitudeBonus = 200f;

	private GameObject sunInstance;

	public Vector3? sunSpawnPosition;

	public override void OnEnter()
	{
		//IL_0034: Unknown result type (might be due to invalid IL or missing references)
		//IL_005b: Unknown result type (might be due to invalid IL or missing references)
		base.OnEnter();
		PlayAnimation(animLayerName, animStateName);
		if (NetworkServer.active)
		{
			sunSpawnPosition = sunSpawnPosition ?? FindSunSpawnPosition(base.transform.position);
			if (sunSpawnPosition.HasValue)
			{
				sunInstance = CreateSun(sunSpawnPosition.Value);
			}
		}
	}

	public override void OnExit()
	{
		if (NetworkServer.active && Object.op_Implicit((Object)(object)sunInstance))
		{
			sunInstance.GetComponent<GenericOwnership>().ownerObject = null;
			sunInstance = null;
		}
		base.OnExit();
	}

	public override void FixedUpdate()
	{
		base.FixedUpdate();
		if (base.isAuthority && !IsKeyDownAuthority())
		{
			outer.SetNextState(new ChannelSunEnd());
		}
	}

	private GameObject CreateSun(Vector3 sunSpawnPosition)
	{
		//IL_0005: Unknown result type (might be due to invalid IL or missing references)
		//IL_0006: Unknown result type (might be due to invalid IL or missing references)
		GameObject obj = Object.Instantiate<GameObject>(sunPrefab, sunSpawnPosition, Quaternion.identity);
		obj.GetComponent<GenericOwnership>().ownerObject = base.gameObject;
		NetworkServer.Spawn(obj);
		return obj;
	}

	private static Vector3? FindSunNodePosition(Vector3 searchOrigin)
	{
		//IL_000c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0084: Unknown result type (might be due to invalid IL or missing references)
		//IL_0086: Unknown result type (might be due to invalid IL or missing references)
		//IL_0087: Unknown result type (might be due to invalid IL or missing references)
		//IL_008c: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ab: Unknown result type (might be due to invalid IL or missing references)
		NodeGraph airNodes = SceneInfo.instance.airNodes;
		NodeGraph.NodeIndex nodeIndex = airNodes.FindClosestNodeWithFlagConditions(searchOrigin, HullClassification.Golem, NodeFlags.None, NodeFlags.None, preventOverhead: false);
		if (nodeIndex == NodeGraph.NodeIndex.invalid)
		{
			return null;
		}
		float num = sunPlacementMinDistance;
		float num2 = num * num;
		_ = NodeGraph.NodeIndex.invalid;
		float num3 = 0f;
		NodeGraphSpider nodeGraphSpider = new NodeGraphSpider(airNodes, HullMask.Golem);
		nodeGraphSpider.AddNodeForNextStep(nodeIndex);
		int num4 = 0;
		int i = 0;
		while (nodeGraphSpider.PerformStep())
		{
			num4++;
			for (; i < nodeGraphSpider.collectedSteps.Count; i++)
			{
				NodeGraphSpider.StepInfo stepInfo = nodeGraphSpider.collectedSteps[i];
				airNodes.GetNodePosition(stepInfo.node, out var position);
				Vector3 val = position - searchOrigin;
				float sqrMagnitude = ((Vector3)(ref val)).sqrMagnitude;
				if (sqrMagnitude > num3)
				{
					num3 = sqrMagnitude;
					_ = stepInfo.node;
					if (num3 >= num2)
					{
						return position;
					}
				}
			}
		}
		return null;
	}

	public static Vector3? FindSunSpawnPosition(Vector3 searchOrigin)
	{
		//IL_0000: Unknown result type (might be due to invalid IL or missing references)
		//IL_0012: Unknown result type (might be due to invalid IL or missing references)
		//IL_0017: Unknown result type (might be due to invalid IL or missing references)
		//IL_002a: Unknown result type (might be due to invalid IL or missing references)
		//IL_002b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0042: Unknown result type (might be due to invalid IL or missing references)
		//IL_0075: Unknown result type (might be due to invalid IL or missing references)
		Vector3? val = FindSunNodePosition(searchOrigin);
		if (val.HasValue)
		{
			Vector3 value = val.Value;
			float num = sunPlacementIdealAltitudeBonus;
			float num2 = sunPrefabDiameter * 0.5f;
			RaycastHit val2 = default(RaycastHit);
			if (Physics.Raycast(value, Vector3.up, ref val2, sunPlacementIdealAltitudeBonus + num2, LayerMask.op_Implicit(LayerIndex.world.mask), (QueryTriggerInteraction)1))
			{
				num = Mathf.Clamp(((RaycastHit)(ref val2)).distance - num2, 0f, num);
			}
			value.y += num;
			return value;
		}
		return null;
	}
}
