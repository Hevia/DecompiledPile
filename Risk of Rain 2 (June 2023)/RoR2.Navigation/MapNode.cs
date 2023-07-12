using System.Collections.Generic;
using System.Collections.ObjectModel;
using UnityEngine;

namespace RoR2.Navigation;

[ExecuteInEditMode]
public class MapNode : MonoBehaviour
{
	public struct Link
	{
		public MapNode nodeB;

		public float distanceScore;

		public float minJumpHeight;

		public int hullMask;

		public int jumpHullMask;

		public string gateName;
	}

	public class MoveProbe
	{
		public CharacterController testCharacterController;

		private float testTimeStep = 1f / 15f;

		public void Init()
		{
			//IL_0000: Unknown result type (might be due to invalid IL or missing references)
			//IL_0006: Expected O, but got Unknown
			GameObject val = new GameObject();
			((Object)val).name = "NodeGraphProbe";
			_ = val.transform;
			testCharacterController = val.AddComponent<CharacterController>();
			testCharacterController.stepOffset = 0.5f;
			testCharacterController.slopeLimit = 60f;
		}

		public void SetHull(HullClassification hullClassification)
		{
			HullDef hullDef = HullDef.Find(hullClassification);
			testCharacterController.radius = hullDef.radius;
			testCharacterController.height = hullDef.height;
		}

		public void Destroy()
		{
			Object.DestroyImmediate((Object)(object)((Component)testCharacterController).gameObject);
		}

		private static float DistanceXZ(Vector3 a, Vector3 b)
		{
			//IL_0018: Unknown result type (might be due to invalid IL or missing references)
			//IL_0019: Unknown result type (might be due to invalid IL or missing references)
			a.y = 0f;
			b.y = 0f;
			return Vector3.Distance(a, b);
		}

		public static Vector3 GetGroundPosition(Vector3 footPosition, float height, float radius)
		{
			//IL_0000: Unknown result type (might be due to invalid IL or missing references)
			//IL_000e: Unknown result type (might be due to invalid IL or missing references)
			//IL_0013: Unknown result type (might be due to invalid IL or missing references)
			//IL_0014: Unknown result type (might be due to invalid IL or missing references)
			//IL_0020: Unknown result type (might be due to invalid IL or missing references)
			//IL_0025: Unknown result type (might be due to invalid IL or missing references)
			//IL_0026: Unknown result type (might be due to invalid IL or missing references)
			//IL_0027: Unknown result type (might be due to invalid IL or missing references)
			//IL_0028: Unknown result type (might be due to invalid IL or missing references)
			//IL_003b: Unknown result type (might be due to invalid IL or missing references)
			//IL_003c: Unknown result type (might be due to invalid IL or missing references)
			//IL_0042: Unknown result type (might be due to invalid IL or missing references)
			//IL_0047: Unknown result type (might be due to invalid IL or missing references)
			//IL_004c: Unknown result type (might be due to invalid IL or missing references)
			//IL_004d: Unknown result type (might be due to invalid IL or missing references)
			//IL_0053: Unknown result type (might be due to invalid IL or missing references)
			//IL_0058: Unknown result type (might be due to invalid IL or missing references)
			//IL_005d: Unknown result type (might be due to invalid IL or missing references)
			//IL_005f: Unknown result type (might be due to invalid IL or missing references)
			//IL_0061: Unknown result type (might be due to invalid IL or missing references)
			//IL_0062: Unknown result type (might be due to invalid IL or missing references)
			//IL_0067: Unknown result type (might be due to invalid IL or missing references)
			//IL_0069: Unknown result type (might be due to invalid IL or missing references)
			//IL_006a: Unknown result type (might be due to invalid IL or missing references)
			//IL_0070: Unknown result type (might be due to invalid IL or missing references)
			//IL_008d: Unknown result type (might be due to invalid IL or missing references)
			//IL_00a6: Unknown result type (might be due to invalid IL or missing references)
			//IL_00ab: Unknown result type (might be due to invalid IL or missing references)
			//IL_00b0: Unknown result type (might be due to invalid IL or missing references)
			//IL_00b2: Unknown result type (might be due to invalid IL or missing references)
			//IL_00b3: Unknown result type (might be due to invalid IL or missing references)
			//IL_00b5: Unknown result type (might be due to invalid IL or missing references)
			//IL_00bb: Unknown result type (might be due to invalid IL or missing references)
			Vector3 val = Vector3.up * (height * 0.5f - radius);
			Vector3 val2 = Vector3.up * (height * 0.5f);
			Vector3 val3 = footPosition + val2;
			float num = radius * 0.5f + 0.005f;
			Vector3 val4 = footPosition + Vector3.up * num;
			Vector3 val5 = val3 + Vector3.up * num;
			RaycastHit val6 = default(RaycastHit);
			if (Physics.CapsuleCast(val5 + val, val5 - val, radius, Vector3.down, ref val6, num * 2f + 0.005f, LayerMask.op_Implicit(LayerIndex.world.mask), (QueryTriggerInteraction)1))
			{
				Vector3 val7 = ((RaycastHit)(ref val6)).distance * Vector3.down;
				return val4 + val7;
			}
			return footPosition;
		}

		public Vector3 GetGroundPosition(Vector3 footPosition)
		{
			//IL_0000: Unknown result type (might be due to invalid IL or missing references)
			//IL_0017: Unknown result type (might be due to invalid IL or missing references)
			return GetGroundPosition(footPosition, testCharacterController.height, testCharacterController.radius);
		}

		public bool CapsuleOverlapTest(Vector3 centerOfCapsule)
		{
			//IL_0000: Unknown result type (might be due to invalid IL or missing references)
			//IL_0022: Unknown result type (might be due to invalid IL or missing references)
			//IL_0027: Unknown result type (might be due to invalid IL or missing references)
			//IL_0028: Unknown result type (might be due to invalid IL or missing references)
			//IL_003e: Unknown result type (might be due to invalid IL or missing references)
			//IL_0044: Unknown result type (might be due to invalid IL or missing references)
			//IL_0045: Unknown result type (might be due to invalid IL or missing references)
			//IL_0046: Unknown result type (might be due to invalid IL or missing references)
			//IL_004b: Unknown result type (might be due to invalid IL or missing references)
			//IL_004c: Unknown result type (might be due to invalid IL or missing references)
			//IL_004d: Unknown result type (might be due to invalid IL or missing references)
			//IL_0065: Unknown result type (might be due to invalid IL or missing references)
			//IL_0077: Unknown result type (might be due to invalid IL or missing references)
			Vector3 val = Vector3.up * (testCharacterController.height * 0.5f - testCharacterController.radius);
			_ = Vector3.up * (testCharacterController.height * 0.5f);
			return Physics.OverlapCapsule(centerOfCapsule + val, centerOfCapsule - val, testCharacterController.radius, LayerMask.op_Implicit(LayerIndex.world.mask) | LayerMask.op_Implicit(LayerIndex.defaultLayer.mask), (QueryTriggerInteraction)1).Length == 0;
		}

		public bool FlyTest(Vector3 startPos, Vector3 endPos, float flySpeed)
		{
			//IL_0000: Unknown result type (might be due to invalid IL or missing references)
			//IL_0022: Unknown result type (might be due to invalid IL or missing references)
			//IL_0027: Unknown result type (might be due to invalid IL or missing references)
			//IL_0028: Unknown result type (might be due to invalid IL or missing references)
			//IL_0029: Unknown result type (might be due to invalid IL or missing references)
			//IL_002a: Unknown result type (might be due to invalid IL or missing references)
			//IL_002f: Unknown result type (might be due to invalid IL or missing references)
			//IL_0030: Unknown result type (might be due to invalid IL or missing references)
			//IL_0031: Unknown result type (might be due to invalid IL or missing references)
			//IL_0041: Unknown result type (might be due to invalid IL or missing references)
			//IL_0042: Unknown result type (might be due to invalid IL or missing references)
			//IL_0043: Unknown result type (might be due to invalid IL or missing references)
			//IL_0048: Unknown result type (might be due to invalid IL or missing references)
			//IL_004b: Unknown result type (might be due to invalid IL or missing references)
			//IL_0050: Unknown result type (might be due to invalid IL or missing references)
			//IL_0051: Unknown result type (might be due to invalid IL or missing references)
			//IL_0052: Unknown result type (might be due to invalid IL or missing references)
			//IL_0057: Unknown result type (might be due to invalid IL or missing references)
			//IL_0067: Unknown result type (might be due to invalid IL or missing references)
			Vector3 val = Vector3.up * (testCharacterController.height * 0.5f - testCharacterController.radius);
			Vector3 val2 = startPos + val;
			Vector3 val3 = startPos - val;
			float radius = testCharacterController.radius;
			Vector3 val4 = endPos - startPos;
			Vector3 normalized = ((Vector3)(ref val4)).normalized;
			val4 = endPos - startPos;
			return !Physics.CapsuleCast(val2, val3, radius, normalized, ((Vector3)(ref val4)).magnitude, LayerMask.op_Implicit(LayerIndex.world.mask));
		}

		private void MoveCapsule(Vector3 displacement)
		{
			//IL_0006: Unknown result type (might be due to invalid IL or missing references)
			//IL_0007: Unknown result type (might be due to invalid IL or missing references)
			testCharacterController.Move(displacement);
		}

		private void SetCapsulePosition(Vector3 position)
		{
			//IL_000b: Unknown result type (might be due to invalid IL or missing references)
			((Component)testCharacterController).transform.position = position;
			Physics.SyncTransforms();
		}

		public bool GroundTest(Vector3 startCenterOfCapsulePos, Vector3 endCenterOfCapsulePos, float hSpeed)
		{
			//IL_0001: Unknown result type (might be due to invalid IL or missing references)
			//IL_000b: Unknown result type (might be due to invalid IL or missing references)
			//IL_0010: Unknown result type (might be due to invalid IL or missing references)
			//IL_0011: Unknown result type (might be due to invalid IL or missing references)
			//IL_0012: Unknown result type (might be due to invalid IL or missing references)
			//IL_0019: Unknown result type (might be due to invalid IL or missing references)
			//IL_001a: Unknown result type (might be due to invalid IL or missing references)
			//IL_001f: Unknown result type (might be due to invalid IL or missing references)
			//IL_0049: Unknown result type (might be due to invalid IL or missing references)
			//IL_004e: Unknown result type (might be due to invalid IL or missing references)
			//IL_0056: Unknown result type (might be due to invalid IL or missing references)
			//IL_0062: Unknown result type (might be due to invalid IL or missing references)
			//IL_0067: Unknown result type (might be due to invalid IL or missing references)
			//IL_006c: Unknown result type (might be due to invalid IL or missing references)
			//IL_007e: Unknown result type (might be due to invalid IL or missing references)
			//IL_0080: Unknown result type (might be due to invalid IL or missing references)
			//IL_0097: Unknown result type (might be due to invalid IL or missing references)
			//IL_00a7: Unknown result type (might be due to invalid IL or missing references)
			//IL_00b5: Unknown result type (might be due to invalid IL or missing references)
			//IL_00b6: Unknown result type (might be due to invalid IL or missing references)
			//IL_00c1: Unknown result type (might be due to invalid IL or missing references)
			//IL_00c6: Unknown result type (might be due to invalid IL or missing references)
			//IL_00cb: Unknown result type (might be due to invalid IL or missing references)
			//IL_00cd: Unknown result type (might be due to invalid IL or missing references)
			//IL_00d4: Unknown result type (might be due to invalid IL or missing references)
			//IL_00e9: Unknown result type (might be due to invalid IL or missing references)
			//IL_00ee: Unknown result type (might be due to invalid IL or missing references)
			//IL_00f0: Unknown result type (might be due to invalid IL or missing references)
			//IL_00f2: Unknown result type (might be due to invalid IL or missing references)
			//IL_00fc: Unknown result type (might be due to invalid IL or missing references)
			//IL_00fe: Unknown result type (might be due to invalid IL or missing references)
			MoveCapsule(Vector3.zero);
			Vector3 val = Vector3.zero;
			float num = DistanceXZ(startCenterOfCapsulePos, endCenterOfCapsulePos);
			SetCapsulePosition(startCenterOfCapsulePos + Vector3.up);
			int num2 = Mathf.CeilToInt(num * 1.5f / hSpeed / testTimeStep);
			Vector3 val2 = ((Component)testCharacterController).transform.position;
			for (int i = 0; i < num2; i++)
			{
				Vector3 val3 = endCenterOfCapsulePos - ((Component)testCharacterController).transform.position;
				if (((Vector3)(ref val3)).sqrMagnitude <= 0.25f)
				{
					return true;
				}
				Vector3 val4 = val3;
				val4.y = 0f;
				((Vector3)(ref val4)).Normalize();
				val.x = val4.x * hSpeed;
				val.z = val4.z * hSpeed;
				val += Physics.gravity * testTimeStep;
				MoveCapsule(val * testTimeStep);
				Vector3 position = ((Component)testCharacterController).transform.position;
				if (position == val2)
				{
					return false;
				}
				val2 = position;
			}
			return false;
		}

		public float JumpTest(Vector3 startCenterOfCapsulePos, Vector3 endCenterOfCapsulePos, float hSpeed)
		{
			//IL_0001: Unknown result type (might be due to invalid IL or missing references)
			//IL_0002: Unknown result type (might be due to invalid IL or missing references)
			//IL_000d: Unknown result type (might be due to invalid IL or missing references)
			//IL_0013: Unknown result type (might be due to invalid IL or missing references)
			//IL_0026: Unknown result type (might be due to invalid IL or missing references)
			//IL_002b: Unknown result type (might be due to invalid IL or missing references)
			//IL_0031: Unknown result type (might be due to invalid IL or missing references)
			//IL_0032: Unknown result type (might be due to invalid IL or missing references)
			//IL_0033: Unknown result type (might be due to invalid IL or missing references)
			//IL_0038: Unknown result type (might be due to invalid IL or missing references)
			//IL_004c: Unknown result type (might be due to invalid IL or missing references)
			//IL_004e: Unknown result type (might be due to invalid IL or missing references)
			//IL_0053: Unknown result type (might be due to invalid IL or missing references)
			//IL_005c: Unknown result type (might be due to invalid IL or missing references)
			//IL_005d: Unknown result type (might be due to invalid IL or missing references)
			//IL_0064: Unknown result type (might be due to invalid IL or missing references)
			//IL_0090: Unknown result type (might be due to invalid IL or missing references)
			//IL_0095: Unknown result type (might be due to invalid IL or missing references)
			//IL_009f: Unknown result type (might be due to invalid IL or missing references)
			//IL_00ab: Unknown result type (might be due to invalid IL or missing references)
			//IL_00b0: Unknown result type (might be due to invalid IL or missing references)
			//IL_00b5: Unknown result type (might be due to invalid IL or missing references)
			//IL_00c6: Unknown result type (might be due to invalid IL or missing references)
			//IL_00d9: Unknown result type (might be due to invalid IL or missing references)
			//IL_00ea: Unknown result type (might be due to invalid IL or missing references)
			//IL_00ec: Unknown result type (might be due to invalid IL or missing references)
			//IL_0103: Unknown result type (might be due to invalid IL or missing references)
			//IL_0113: Unknown result type (might be due to invalid IL or missing references)
			//IL_0121: Unknown result type (might be due to invalid IL or missing references)
			//IL_0122: Unknown result type (might be due to invalid IL or missing references)
			//IL_012d: Unknown result type (might be due to invalid IL or missing references)
			//IL_0132: Unknown result type (might be due to invalid IL or missing references)
			//IL_0137: Unknown result type (might be due to invalid IL or missing references)
			//IL_013e: Unknown result type (might be due to invalid IL or missing references)
			//IL_0145: Unknown result type (might be due to invalid IL or missing references)
			//IL_014a: Unknown result type (might be due to invalid IL or missing references)
			//IL_015b: Unknown result type (might be due to invalid IL or missing references)
			//IL_0160: Unknown result type (might be due to invalid IL or missing references)
			//IL_0162: Unknown result type (might be due to invalid IL or missing references)
			//IL_0164: Unknown result type (might be due to invalid IL or missing references)
			//IL_0173: Unknown result type (might be due to invalid IL or missing references)
			//IL_0175: Unknown result type (might be due to invalid IL or missing references)
			float y = Trajectory.CalculateInitialYSpeed(Trajectory.CalculateGroundTravelTime(hSpeed, DistanceXZ(startCenterOfCapsulePos, endCenterOfCapsulePos)), endCenterOfCapsulePos.y - startCenterOfCapsulePos.y);
			testCharacterController.Move(Vector3.zero);
			Vector3 val = endCenterOfCapsulePos - startCenterOfCapsulePos;
			val.y = 0f;
			((Vector3)(ref val)).Normalize();
			val *= hSpeed;
			val.y = y;
			float num = DistanceXZ(startCenterOfCapsulePos, endCenterOfCapsulePos);
			SetCapsulePosition(startCenterOfCapsulePos);
			int num2 = Mathf.CeilToInt(num * 1.5f / hSpeed / testTimeStep);
			float num3 = float.NegativeInfinity;
			Vector3 val2 = ((Component)testCharacterController).transform.position;
			for (int i = 0; i < num2; i++)
			{
				Vector3 val3 = endCenterOfCapsulePos - ((Component)testCharacterController).transform.position;
				if (((Vector3)(ref val3)).sqrMagnitude <= 4f)
				{
					return num3 - startCenterOfCapsulePos.y;
				}
				num3 = Mathf.Max(((Component)testCharacterController).transform.position.y, num3);
				Vector3 val4 = val3;
				val4.y = 0f;
				((Vector3)(ref val4)).Normalize();
				val.x = val4.x * hSpeed;
				val.z = val4.z * hSpeed;
				val += Physics.gravity * testTimeStep;
				testCharacterController.Move(val * testTimeStep);
				Vector3 position = ((Component)testCharacterController).transform.position;
				if (position == val2)
				{
					return 0f;
				}
				val2 = position;
			}
			return 0f;
		}
	}

	private static List<MapNode> _instances = new List<MapNode>();

	private static ReadOnlyCollection<MapNode> instancesReadOnly = _instances.AsReadOnly();

	public static readonly float maxConnectionDistance = 15f;

	public List<Link> links = new List<Link>();

	[EnumMask(typeof(HullMask))]
	public HullMask forbiddenHulls;

	[EnumMask(typeof(NodeFlags))]
	public NodeFlags flags;

	[Tooltip("The name of the nodegraph gate associated with this node. If the named gate is closed this node will be treated as though it does not exist.")]
	public string gateName = "";

	public static ReadOnlyCollection<MapNode> instances => instancesReadOnly;

	public void OnEnable()
	{
		_instances.Add(this);
	}

	public void OnDisable()
	{
		_instances.Remove(this);
	}

	private void AddLink(MapNode nodeB, float distanceScore, float minJumpHeight, HullClassification hullClassification)
	{
		int num = links.FindIndex((Link item) => (Object)(object)item.nodeB == (Object)(object)nodeB);
		if (num == -1)
		{
			links.Add(new Link
			{
				nodeB = nodeB
			});
			num = links.Count - 1;
		}
		Link value = links[num];
		value.distanceScore = Mathf.Max(value.distanceScore, distanceScore);
		value.minJumpHeight = Mathf.Max(value.minJumpHeight, minJumpHeight);
		value.hullMask |= 1 << (int)hullClassification;
		if (minJumpHeight > 0f)
		{
			value.jumpHullMask |= 1 << (int)hullClassification;
		}
		if (string.IsNullOrEmpty(value.gateName))
		{
			value.gateName = nodeB.gateName;
		}
		links[num] = value;
	}

	private void BuildGroundLinks(ReadOnlyCollection<MapNode> nodes, MoveProbe moveProbe)
	{
		//IL_0006: Unknown result type (might be due to invalid IL or missing references)
		//IL_000b: Unknown result type (might be due to invalid IL or missing references)
		//IL_002d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0032: Unknown result type (might be due to invalid IL or missing references)
		//IL_003c: Unknown result type (might be due to invalid IL or missing references)
		//IL_003d: Unknown result type (might be due to invalid IL or missing references)
		//IL_003e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0043: Unknown result type (might be due to invalid IL or missing references)
		//IL_0096: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ac: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b1: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b3: Unknown result type (might be due to invalid IL or missing references)
		//IL_00bd: Unknown result type (might be due to invalid IL or missing references)
		//IL_00c2: Unknown result type (might be due to invalid IL or missing references)
		//IL_00c5: Unknown result type (might be due to invalid IL or missing references)
		//IL_00c6: Unknown result type (might be due to invalid IL or missing references)
		//IL_00cb: Unknown result type (might be due to invalid IL or missing references)
		//IL_00cd: Unknown result type (might be due to invalid IL or missing references)
		//IL_00d2: Unknown result type (might be due to invalid IL or missing references)
		//IL_00d5: Unknown result type (might be due to invalid IL or missing references)
		//IL_00d6: Unknown result type (might be due to invalid IL or missing references)
		//IL_00db: Unknown result type (might be due to invalid IL or missing references)
		//IL_00dd: Unknown result type (might be due to invalid IL or missing references)
		//IL_00e2: Unknown result type (might be due to invalid IL or missing references)
		//IL_00e4: Unknown result type (might be due to invalid IL or missing references)
		//IL_00e6: Unknown result type (might be due to invalid IL or missing references)
		//IL_00eb: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ed: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ef: Unknown result type (might be due to invalid IL or missing references)
		//IL_00f4: Unknown result type (might be due to invalid IL or missing references)
		//IL_00f7: Unknown result type (might be due to invalid IL or missing references)
		//IL_0101: Unknown result type (might be due to invalid IL or missing references)
		//IL_010b: Unknown result type (might be due to invalid IL or missing references)
		//IL_010d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0124: Unknown result type (might be due to invalid IL or missing references)
		//IL_0126: Unknown result type (might be due to invalid IL or missing references)
		Vector3 position = ((Component)this).transform.position;
		for (int i = 0; i < nodes.Count; i++)
		{
			MapNode mapNode = nodes[i];
			if ((Object)(object)mapNode == (Object)(object)this)
			{
				continue;
			}
			Vector3 position2 = ((Component)mapNode).transform.position;
			float num = maxConnectionDistance;
			float num2 = num * num;
			Vector3 val = position2 - position;
			float sqrMagnitude = ((Vector3)(ref val)).sqrMagnitude;
			if (!(sqrMagnitude < num2))
			{
				continue;
			}
			float distanceScore = Mathf.Sqrt(sqrMagnitude);
			for (int j = 0; j < 3; j++)
			{
				moveProbe.SetHull((HullClassification)j);
				if (((uint)forbiddenHulls & (uint)(1 << j)) != 0 || ((uint)mapNode.forbiddenHulls & (uint)(1 << j)) != 0)
				{
					continue;
				}
				Vector3 val2 = Vector3.up * (moveProbe.testCharacterController.height * 0.5f);
				Vector3 val3 = Vector3.up * 0.01f;
				Vector3 val4 = moveProbe.GetGroundPosition(position) + val3;
				Vector3 val5 = moveProbe.GetGroundPosition(position2) + val3;
				Vector3 val6 = val4 + val2;
				Vector3 val7 = val5 + val2;
				if (moveProbe.CapsuleOverlapTest(val6) && moveProbe.CapsuleOverlapTest(val7))
				{
					bool num3 = moveProbe.GroundTest(val6, val7, 6f);
					float num4 = ((!num3) ? moveProbe.JumpTest(val6, val7, 7.5f) : 0f);
					if (num3 || (num4 > 0f && num4 < 10f))
					{
						AddLink(mapNode, distanceScore, num4, (HullClassification)j);
					}
				}
			}
		}
	}

	private void BuildAirLinks(ReadOnlyCollection<MapNode> nodes, MoveProbe moveProbe)
	{
		//IL_0006: Unknown result type (might be due to invalid IL or missing references)
		//IL_000b: Unknown result type (might be due to invalid IL or missing references)
		//IL_002d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0032: Unknown result type (might be due to invalid IL or missing references)
		//IL_0042: Unknown result type (might be due to invalid IL or missing references)
		//IL_0043: Unknown result type (might be due to invalid IL or missing references)
		//IL_0044: Unknown result type (might be due to invalid IL or missing references)
		//IL_0049: Unknown result type (might be due to invalid IL or missing references)
		//IL_0090: Unknown result type (might be due to invalid IL or missing references)
		//IL_0091: Unknown result type (might be due to invalid IL or missing references)
		//IL_0093: Unknown result type (might be due to invalid IL or missing references)
		//IL_0094: Unknown result type (might be due to invalid IL or missing references)
		//IL_0097: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a1: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ab: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ad: Unknown result type (might be due to invalid IL or missing references)
		Vector3 position = ((Component)this).transform.position;
		for (int i = 0; i < nodes.Count; i++)
		{
			MapNode mapNode = nodes[i];
			if ((Object)(object)mapNode == (Object)(object)this)
			{
				continue;
			}
			Vector3 position2 = ((Component)mapNode).transform.position;
			float num = maxConnectionDistance * 2f;
			float num2 = num * num;
			Vector3 val = position2 - position;
			float sqrMagnitude = ((Vector3)(ref val)).sqrMagnitude;
			if (!(sqrMagnitude < num2))
			{
				continue;
			}
			float distanceScore = Mathf.Sqrt(sqrMagnitude);
			for (int j = 0; j < 3; j++)
			{
				if (((uint)forbiddenHulls & (uint)(1 << j)) == 0 && ((uint)mapNode.forbiddenHulls & (uint)(1 << j)) == 0)
				{
					moveProbe.SetHull((HullClassification)j);
					Vector3 val2 = position;
					Vector3 val3 = position2;
					if (moveProbe.CapsuleOverlapTest(val2) && moveProbe.CapsuleOverlapTest(val3) && moveProbe.FlyTest(val2, val3, 6f))
					{
						AddLink(mapNode, distanceScore, 0f, (HullClassification)j);
					}
				}
			}
		}
	}

	private void BuildRailLinks(ReadOnlyCollection<MapNode> nodes, MoveProbe moveProbe)
	{
		//IL_0006: Unknown result type (might be due to invalid IL or missing references)
		//IL_000b: Unknown result type (might be due to invalid IL or missing references)
		//IL_002d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0032: Unknown result type (might be due to invalid IL or missing references)
		//IL_0042: Unknown result type (might be due to invalid IL or missing references)
		//IL_0043: Unknown result type (might be due to invalid IL or missing references)
		//IL_0044: Unknown result type (might be due to invalid IL or missing references)
		//IL_0049: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a5: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a6: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a8: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a9: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ab: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b0: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b2: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b4: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ea: Unknown result type (might be due to invalid IL or missing references)
		//IL_00f4: Unknown result type (might be due to invalid IL or missing references)
		//IL_00fe: Unknown result type (might be due to invalid IL or missing references)
		//IL_0100: Unknown result type (might be due to invalid IL or missing references)
		Vector3 position = ((Component)this).transform.position;
		for (int i = 0; i < nodes.Count; i++)
		{
			MapNode mapNode = nodes[i];
			if ((Object)(object)mapNode == (Object)(object)this)
			{
				continue;
			}
			Vector3 position2 = ((Component)mapNode).transform.position;
			float num = maxConnectionDistance * 2f;
			float num2 = num * num;
			Vector3 val = position2 - position;
			float sqrMagnitude = ((Vector3)(ref val)).sqrMagnitude;
			if (!(sqrMagnitude < num2))
			{
				continue;
			}
			float distanceScore = Mathf.Sqrt(sqrMagnitude);
			for (int j = 0; j < 3; j++)
			{
				HullDef hullDef = HullDef.Find((HullClassification)j);
				if (((uint)forbiddenHulls & (uint)(1 << j)) != 0 || ((uint)mapNode.forbiddenHulls & (uint)(1 << j)) != 0)
				{
					continue;
				}
				moveProbe.SetHull((HullClassification)j);
				Vector3 val2 = position;
				Vector3 val3 = position2;
				if (!(Vector3.Angle(Vector3.up, val3 - val2) <= 50f))
				{
					val2.y += hullDef.height;
					val3.y += hullDef.height;
					if (moveProbe.CapsuleOverlapTest(val2) && moveProbe.CapsuleOverlapTest(val3) && moveProbe.FlyTest(val2, val3, 6f))
					{
						AddLink(mapNode, distanceScore, 0f, (HullClassification)j);
					}
				}
			}
		}
	}

	public void BuildLinks(ReadOnlyCollection<MapNode> nodes, MapNodeGroup.GraphType graphType)
	{
		//IL_0011: Unknown result type (might be due to invalid IL or missing references)
		//IL_0016: Unknown result type (might be due to invalid IL or missing references)
		//IL_008f: Unknown result type (might be due to invalid IL or missing references)
		//IL_009c: Unknown result type (might be due to invalid IL or missing references)
		links.Clear();
		Vector3 position = ((Component)this).transform.position;
		MoveProbe moveProbe = new MoveProbe();
		moveProbe.Init();
		switch (graphType)
		{
		case MapNodeGroup.GraphType.Ground:
			BuildGroundLinks(nodes, moveProbe);
			break;
		case MapNodeGroup.GraphType.Air:
			BuildAirLinks(nodes, moveProbe);
			break;
		case MapNodeGroup.GraphType.Rail:
			BuildRailLinks(nodes, moveProbe);
			break;
		}
		MapNodeLink[] components = ((Component)this).GetComponents<MapNodeLink>();
		foreach (MapNodeLink mapNodeLink in components)
		{
			if (!Object.op_Implicit((Object)(object)mapNodeLink.other))
			{
				continue;
			}
			Link link = default(Link);
			link.nodeB = mapNodeLink.other;
			link.distanceScore = Vector3.Distance(position, ((Component)mapNodeLink.other).transform.position);
			link.minJumpHeight = mapNodeLink.minJumpHeight;
			link.gateName = mapNodeLink.gateName;
			link.hullMask = -1;
			Link link2 = link;
			bool flag = false;
			for (int j = 0; j < links.Count; j++)
			{
				if ((Object)(object)links[j].nodeB == (Object)(object)mapNodeLink.other)
				{
					links[j] = link2;
					flag = true;
					break;
				}
			}
			if (!flag)
			{
				links.Add(link2);
			}
		}
		moveProbe.Destroy();
	}

	public bool TestLineOfSight(MapNode other)
	{
		//IL_0006: Unknown result type (might be due to invalid IL or missing references)
		//IL_000b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0010: Unknown result type (might be due to invalid IL or missing references)
		//IL_001b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0020: Unknown result type (might be due to invalid IL or missing references)
		//IL_0025: Unknown result type (might be due to invalid IL or missing references)
		//IL_0032: Unknown result type (might be due to invalid IL or missing references)
		return !Physics.Linecast(((Component)this).transform.position + Vector3.up, ((Component)other).transform.position + Vector3.up, LayerMask.op_Implicit(LayerIndex.world.mask));
	}

	public bool TestNoCeiling()
	{
		//IL_0006: Unknown result type (might be due to invalid IL or missing references)
		//IL_000b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0010: Unknown result type (might be due to invalid IL or missing references)
		//IL_0022: Unknown result type (might be due to invalid IL or missing references)
		return !Physics.Raycast(new Ray(((Component)this).transform.position, Vector3.up), float.PositiveInfinity, LayerMask.op_Implicit(LayerIndex.world.mask), (QueryTriggerInteraction)1);
	}

	public bool TestTeleporterOK()
	{
		//IL_002a: Unknown result type (might be due to invalid IL or missing references)
		//IL_002f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0034: Unknown result type (might be due to invalid IL or missing references)
		//IL_003a: Unknown result type (might be due to invalid IL or missing references)
		//IL_003f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0044: Unknown result type (might be due to invalid IL or missing references)
		//IL_004c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0051: Unknown result type (might be due to invalid IL or missing references)
		//IL_0053: Unknown result type (might be due to invalid IL or missing references)
		//IL_0058: Unknown result type (might be due to invalid IL or missing references)
		//IL_005e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0063: Unknown result type (might be due to invalid IL or missing references)
		//IL_0068: Unknown result type (might be due to invalid IL or missing references)
		//IL_006a: Unknown result type (might be due to invalid IL or missing references)
		//IL_006c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0071: Unknown result type (might be due to invalid IL or missing references)
		//IL_0084: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a9: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b4: Unknown result type (might be due to invalid IL or missing references)
		//IL_00be: Unknown result type (might be due to invalid IL or missing references)
		//IL_00c3: Unknown result type (might be due to invalid IL or missing references)
		float num = 15f;
		int num2 = 20;
		float num3 = 7f;
		float num4 = 3f;
		float num5 = 360f / (float)num2;
		RaycastHit val3 = default(RaycastHit);
		for (int i = 0; i < num2; i++)
		{
			Vector3 val = Quaternion.AngleAxis(num5 * (float)i, Vector3.up) * (Vector3.forward * num);
			Vector3 val2 = ((Component)this).transform.position + val + Vector3.up * num3;
			if (!Physics.Raycast(new Ray(val2, Vector3.down), ref val3, num4 + num3, LayerMask.op_Implicit(LayerIndex.world.mask), (QueryTriggerInteraction)1))
			{
				return false;
			}
		}
		Debug.DrawRay(((Component)this).transform.position, ((Component)this).transform.up * 20f, Color.green, 15f);
		return true;
	}
}
