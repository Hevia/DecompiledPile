using System;
using System.Collections.Generic;
using UnityEngine;

namespace RoR2;

public class RoachController : MonoBehaviour
{
	[Serializable]
	public struct KeyFrame
	{
		public float time;

		public Vector3 position;

		public Quaternion rotation;
	}

	[Serializable]
	public struct Roach
	{
		public KeyFrame[] keyFrames;
	}

	private class SimulatedRoach : IDisposable
	{
		private struct RaycastResult
		{
			public bool didHit;

			public Vector3 point;

			public Vector3 normal;

			public float distance;
		}

		private Vector3 initialFleeNormal;

		private Vector3 desiredMovement;

		private RoachParams roachParams;

		private float reorientTimer;

		private float backupTimer;

		private Vector3 velocity = Vector3.zero;

		private float currentSpeed;

		private float turnVelocity;

		private Vector3 groundNormal;

		private float simulationDuration;

		public Transform transform { get; private set; }

		public float age { get; private set; }

		public bool finished { get; private set; }

		private bool onGround => groundNormal != Vector3.zero;

		public SimulatedRoach(Vector3 position, Vector3 groundNormal, Vector3 initialFleeNormal, RoachParams roachParams)
		{
			//IL_0001: Unknown result type (might be due to invalid IL or missing references)
			//IL_0006: Unknown result type (might be due to invalid IL or missing references)
			//IL_001e: Unknown result type (might be due to invalid IL or missing references)
			//IL_0024: Expected O, but got Unknown
			//IL_0036: Unknown result type (might be due to invalid IL or missing references)
			//IL_0042: Unknown result type (might be due to invalid IL or missing references)
			//IL_0054: Unknown result type (might be due to invalid IL or missing references)
			//IL_0073: Unknown result type (might be due to invalid IL or missing references)
			//IL_007e: Unknown result type (might be due to invalid IL or missing references)
			//IL_007f: Unknown result type (might be due to invalid IL or missing references)
			//IL_0085: Unknown result type (might be due to invalid IL or missing references)
			//IL_0086: Unknown result type (might be due to invalid IL or missing references)
			//IL_008c: Unknown result type (might be due to invalid IL or missing references)
			//IL_0091: Unknown result type (might be due to invalid IL or missing references)
			this.roachParams = roachParams;
			GameObject val = new GameObject("SimulatedRoach");
			transform = val.transform;
			transform.position = position;
			transform.up = groundNormal;
			transform.Rotate(transform.up, Random.Range(0f, 360f));
			transform.forward = Random.onUnitSphere;
			this.groundNormal = groundNormal;
			this.initialFleeNormal = initialFleeNormal;
			desiredMovement = Random.onUnitSphere;
			age = Random.Range(roachParams.minReactionTime, roachParams.maxReactionTime);
			simulationDuration = age + Random.Range(roachParams.minSimulationDuration, roachParams.maxSimulationDuration);
		}

		private void SetUpVector(Vector3 desiredUp)
		{
			//IL_0006: Unknown result type (might be due to invalid IL or missing references)
			//IL_000b: Unknown result type (might be due to invalid IL or missing references)
			//IL_0012: Unknown result type (might be due to invalid IL or missing references)
			//IL_0017: Unknown result type (might be due to invalid IL or missing references)
			//IL_001e: Unknown result type (might be due to invalid IL or missing references)
			//IL_001f: Unknown result type (might be due to invalid IL or missing references)
			//IL_0020: Unknown result type (might be due to invalid IL or missing references)
			//IL_0021: Unknown result type (might be due to invalid IL or missing references)
			Vector3 right = transform.right;
			Vector3 up = transform.up;
			transform.Rotate(right, Vector3.SignedAngle(up, desiredUp, right), (Space)0);
		}

		public void Simulate(float deltaTime)
		{
			//IL_001b: Unknown result type (might be due to invalid IL or missing references)
			//IL_0064: Unknown result type (might be due to invalid IL or missing references)
			//IL_0069: Unknown result type (might be due to invalid IL or missing references)
			//IL_006b: Unknown result type (might be due to invalid IL or missing references)
			//IL_0070: Unknown result type (might be due to invalid IL or missing references)
			//IL_0071: Unknown result type (might be due to invalid IL or missing references)
			//IL_0076: Unknown result type (might be due to invalid IL or missing references)
			//IL_0079: Unknown result type (might be due to invalid IL or missing references)
			//IL_007e: Unknown result type (might be due to invalid IL or missing references)
			//IL_0085: Unknown result type (might be due to invalid IL or missing references)
			//IL_008a: Unknown result type (might be due to invalid IL or missing references)
			//IL_008b: Unknown result type (might be due to invalid IL or missing references)
			//IL_00ea: Unknown result type (might be due to invalid IL or missing references)
			//IL_00ef: Unknown result type (might be due to invalid IL or missing references)
			//IL_00f5: Unknown result type (might be due to invalid IL or missing references)
			//IL_00fa: Unknown result type (might be due to invalid IL or missing references)
			//IL_00ff: Unknown result type (might be due to invalid IL or missing references)
			//IL_0106: Unknown result type (might be due to invalid IL or missing references)
			//IL_012d: Unknown result type (might be due to invalid IL or missing references)
			//IL_0132: Unknown result type (might be due to invalid IL or missing references)
			age += deltaTime;
			if (onGround)
			{
				SetUpVector(groundNormal);
				turnVelocity += Random.Range(0f - roachParams.wiggle, roachParams.wiggle) * deltaTime;
				TurnDesiredMovement(turnVelocity * deltaTime);
				Vector3 up = transform.up;
				Vector3 val = Vector3.ProjectOnPlane(desiredMovement, up);
				Vector3 normalized = ((Vector3)(ref val)).normalized;
				float num = Vector3.SignedAngle(transform.forward, normalized, up);
				TurnBody(Mathf.Clamp(num, (0f - turnVelocity) * deltaTime, turnVelocity * deltaTime));
				currentSpeed = Mathf.MoveTowards(currentSpeed, roachParams.maxSpeed, deltaTime * roachParams.acceleration);
				StepGround(currentSpeed * deltaTime);
			}
			else
			{
				velocity += Physics.gravity * deltaTime;
				StepAir(velocity);
			}
			reorientTimer -= deltaTime;
			if (reorientTimer <= 0f)
			{
				desiredMovement = initialFleeNormal;
				reorientTimer = Random.Range(roachParams.reorientTimerMin, roachParams.reorientTimerMax);
			}
			if (age >= simulationDuration)
			{
				finished = true;
			}
		}

		private void OnBump()
		{
			TurnDesiredMovement(Random.Range(-90f, 90f));
			currentSpeed *= -0.5f;
			if (roachParams.chanceToFinishOnBump < Random.value)
			{
				finished = true;
			}
		}

		private void TurnDesiredMovement(float degrees)
		{
			//IL_0007: Unknown result type (might be due to invalid IL or missing references)
			//IL_000c: Unknown result type (might be due to invalid IL or missing references)
			//IL_0011: Unknown result type (might be due to invalid IL or missing references)
			//IL_0013: Unknown result type (might be due to invalid IL or missing references)
			//IL_0015: Unknown result type (might be due to invalid IL or missing references)
			//IL_001a: Unknown result type (might be due to invalid IL or missing references)
			//IL_001f: Unknown result type (might be due to invalid IL or missing references)
			Quaternion val = Quaternion.AngleAxis(degrees, transform.up);
			desiredMovement = val * desiredMovement;
		}

		private void TurnBody(float degrees)
		{
			//IL_0006: Unknown result type (might be due to invalid IL or missing references)
			transform.Rotate(Vector3.up, degrees, (Space)1);
		}

		private void StepAir(Vector3 movement)
		{
			//IL_0006: Unknown result type (might be due to invalid IL or missing references)
			//IL_000b: Unknown result type (might be due to invalid IL or missing references)
			//IL_000c: Unknown result type (might be due to invalid IL or missing references)
			//IL_0024: Unknown result type (might be due to invalid IL or missing references)
			//IL_002a: Unknown result type (might be due to invalid IL or missing references)
			//IL_002f: Unknown result type (might be due to invalid IL or missing references)
			//IL_0049: Unknown result type (might be due to invalid IL or missing references)
			//IL_004e: Unknown result type (might be due to invalid IL or missing references)
			//IL_0054: Unknown result type (might be due to invalid IL or missing references)
			//IL_0059: Unknown result type (might be due to invalid IL or missing references)
			//IL_0065: Unknown result type (might be due to invalid IL or missing references)
			RaycastResult raycastResult = SimpleRaycast(new Ray(transform.position, movement), ((Vector3)(ref movement)).magnitude);
			Debug.DrawLine(transform.position, raycastResult.point, Color.magenta, 10f, false);
			if (raycastResult.didHit)
			{
				groundNormal = raycastResult.normal;
				velocity = Vector3.zero;
			}
			transform.position = raycastResult.point;
		}

		private void StepGround(float distance)
		{
			//IL_0001: Unknown result type (might be due to invalid IL or missing references)
			//IL_0006: Unknown result type (might be due to invalid IL or missing references)
			//IL_0011: Unknown result type (might be due to invalid IL or missing references)
			//IL_001c: Unknown result type (might be due to invalid IL or missing references)
			//IL_0021: Unknown result type (might be due to invalid IL or missing references)
			//IL_002f: Unknown result type (might be due to invalid IL or missing references)
			//IL_0034: Unknown result type (might be due to invalid IL or missing references)
			//IL_003b: Unknown result type (might be due to invalid IL or missing references)
			//IL_0040: Unknown result type (might be due to invalid IL or missing references)
			//IL_0041: Unknown result type (might be due to invalid IL or missing references)
			//IL_0042: Unknown result type (might be due to invalid IL or missing references)
			//IL_0043: Unknown result type (might be due to invalid IL or missing references)
			//IL_0048: Unknown result type (might be due to invalid IL or missing references)
			//IL_004f: Unknown result type (might be due to invalid IL or missing references)
			//IL_0054: Unknown result type (might be due to invalid IL or missing references)
			//IL_0055: Unknown result type (might be due to invalid IL or missing references)
			//IL_0065: Unknown result type (might be due to invalid IL or missing references)
			//IL_0066: Unknown result type (might be due to invalid IL or missing references)
			//IL_0067: Unknown result type (might be due to invalid IL or missing references)
			//IL_0074: Unknown result type (might be due to invalid IL or missing references)
			//IL_0077: Unknown result type (might be due to invalid IL or missing references)
			//IL_007c: Unknown result type (might be due to invalid IL or missing references)
			//IL_008e: Unknown result type (might be due to invalid IL or missing references)
			//IL_0093: Unknown result type (might be due to invalid IL or missing references)
			//IL_009f: Unknown result type (might be due to invalid IL or missing references)
			//IL_00a4: Unknown result type (might be due to invalid IL or missing references)
			//IL_00ba: Unknown result type (might be due to invalid IL or missing references)
			//IL_00bf: Unknown result type (might be due to invalid IL or missing references)
			//IL_00c6: Unknown result type (might be due to invalid IL or missing references)
			//IL_00c7: Unknown result type (might be due to invalid IL or missing references)
			//IL_00c8: Unknown result type (might be due to invalid IL or missing references)
			//IL_00cd: Unknown result type (might be due to invalid IL or missing references)
			//IL_00e9: Unknown result type (might be due to invalid IL or missing references)
			//IL_00ec: Unknown result type (might be due to invalid IL or missing references)
			//IL_00f1: Unknown result type (might be due to invalid IL or missing references)
			//IL_0103: Unknown result type (might be due to invalid IL or missing references)
			//IL_0108: Unknown result type (might be due to invalid IL or missing references)
			//IL_010c: Unknown result type (might be due to invalid IL or missing references)
			//IL_0111: Unknown result type (might be due to invalid IL or missing references)
			//IL_0118: Unknown result type (might be due to invalid IL or missing references)
			//IL_0119: Unknown result type (might be due to invalid IL or missing references)
			//IL_011a: Unknown result type (might be due to invalid IL or missing references)
			//IL_011b: Unknown result type (might be due to invalid IL or missing references)
			//IL_0120: Unknown result type (might be due to invalid IL or missing references)
			//IL_012f: Unknown result type (might be due to invalid IL or missing references)
			//IL_0130: Unknown result type (might be due to invalid IL or missing references)
			//IL_0131: Unknown result type (might be due to invalid IL or missing references)
			//IL_0136: Unknown result type (might be due to invalid IL or missing references)
			//IL_0138: Unknown result type (might be due to invalid IL or missing references)
			//IL_013d: Unknown result type (might be due to invalid IL or missing references)
			//IL_015a: Unknown result type (might be due to invalid IL or missing references)
			groundNormal = Vector3.zero;
			Vector3 up = transform.up;
			Vector3 forward = transform.forward;
			float stepSize = roachParams.stepSize;
			Vector3 val = up * stepSize;
			Vector3 position = transform.position;
			position += val;
			Debug.DrawLine(transform.position, position, Color.red, 10f, false);
			RaycastResult raycastResult = SimpleRaycast(new Ray(position, forward), distance);
			Debug.DrawLine(position, raycastResult.point, Color.green, 10f, false);
			position = raycastResult.point;
			if (raycastResult.didHit)
			{
				if (Vector3.Dot(raycastResult.normal, forward) < -0.5f)
				{
					OnBump();
				}
				groundNormal = raycastResult.normal;
			}
			else
			{
				RaycastResult raycastResult2 = SimpleRaycast(new Ray(position, -val), stepSize * 2f);
				if (raycastResult2.didHit)
				{
					Debug.DrawLine(position, raycastResult2.point, Color.blue, 10f, false);
					position = raycastResult2.point;
					groundNormal = raycastResult2.normal;
				}
				else
				{
					Debug.DrawLine(position, position - val, Color.white, 10f);
					position -= val;
				}
			}
			if (groundNormal == Vector3.zero)
			{
				currentSpeed = 0f;
			}
			transform.position = position;
		}

		private static RaycastResult SimpleRaycast(Ray ray, float maxDistance)
		{
			//IL_0000: Unknown result type (might be due to invalid IL or missing references)
			//IL_000c: Unknown result type (might be due to invalid IL or missing references)
			//IL_0035: Unknown result type (might be due to invalid IL or missing references)
			//IL_003e: Unknown result type (might be due to invalid IL or missing references)
			//IL_0043: Unknown result type (might be due to invalid IL or missing references)
			//IL_004d: Unknown result type (might be due to invalid IL or missing references)
			//IL_0056: Unknown result type (might be due to invalid IL or missing references)
			//IL_005b: Unknown result type (might be due to invalid IL or missing references)
			RaycastHit val = default(RaycastHit);
			bool flag = Physics.Raycast(ray, ref val, maxDistance, LayerMask.op_Implicit(LayerIndex.world.mask), (QueryTriggerInteraction)1);
			RaycastResult result = default(RaycastResult);
			result.didHit = flag;
			result.point = (flag ? ((RaycastHit)(ref val)).point : ((Ray)(ref ray)).GetPoint(maxDistance));
			result.normal = (flag ? ((RaycastHit)(ref val)).normal : Vector3.zero);
			result.distance = (flag ? ((RaycastHit)(ref val)).distance : maxDistance);
			return result;
		}

		public void Dispose()
		{
			Object.DestroyImmediate((Object)(object)((Component)transform).gameObject);
			transform = null;
		}
	}

	public class RoachPathEditorComponent : MonoBehaviour
	{
		public RoachController roachController;

		public int nodeCount => ((Component)this).transform.childCount;

		public RoachNodeEditorComponent AddNode()
		{
			//IL_0005: Unknown result type (might be due to invalid IL or missing references)
			//IL_000a: Unknown result type (might be due to invalid IL or missing references)
			//IL_0012: Unknown result type (might be due to invalid IL or missing references)
			GameObject val = new GameObject("Roach Path Node (Temporary)")
			{
				hideFlags = (HideFlags)52
			};
			val.transform.SetParent(((Component)this).transform);
			RoachNodeEditorComponent roachNodeEditorComponent = val.AddComponent<RoachNodeEditorComponent>();
			roachNodeEditorComponent.path = this;
			return roachNodeEditorComponent;
		}

		private void OnDrawGizmosSelected()
		{
			//IL_0000: Unknown result type (might be due to invalid IL or missing references)
			//IL_001f: Unknown result type (might be due to invalid IL or missing references)
			//IL_0037: Unknown result type (might be due to invalid IL or missing references)
			//IL_003c: Unknown result type (might be due to invalid IL or missing references)
			//IL_003d: Unknown result type (might be due to invalid IL or missing references)
			Gizmos.color = Color.white;
			for (int i = 0; i + 1 < nodeCount; i++)
			{
				Vector3 position = ((Component)((Component)this).transform.GetChild(i)).transform.position;
				Vector3 position2 = ((Component)((Component)this).transform.GetChild(i + 1)).transform.position;
				Gizmos.DrawLine(position, position2);
			}
		}
	}

	public class RoachNodeEditorComponent : MonoBehaviour
	{
		public RoachPathEditorComponent path;

		public void FacePosition(Vector3 position)
		{
			//IL_0006: Unknown result type (might be due to invalid IL or missing references)
			//IL_000b: Unknown result type (might be due to invalid IL or missing references)
			//IL_0012: Unknown result type (might be due to invalid IL or missing references)
			//IL_0017: Unknown result type (might be due to invalid IL or missing references)
			//IL_0018: Unknown result type (might be due to invalid IL or missing references)
			//IL_0019: Unknown result type (might be due to invalid IL or missing references)
			//IL_001a: Unknown result type (might be due to invalid IL or missing references)
			//IL_001f: Unknown result type (might be due to invalid IL or missing references)
			//IL_0020: Unknown result type (might be due to invalid IL or missing references)
			//IL_0025: Unknown result type (might be due to invalid IL or missing references)
			//IL_002c: Unknown result type (might be due to invalid IL or missing references)
			//IL_0038: Unknown result type (might be due to invalid IL or missing references)
			Vector3 position2 = ((Component)this).transform.position;
			Vector3 up = ((Component)this).transform.up;
			Quaternion rotation = Quaternion.LookRotation(position - position2, up);
			((Component)this).transform.rotation = rotation;
			((Component)this).transform.up = up;
		}
	}

	public RoachParams roachParams;

	public int roachCount;

	public float placementSpreadMin = 1f;

	public float placementSpreadMax = 25f;

	public float placementMaxDistance = 10f;

	public Roach[] roaches;

	private Transform[] roachTransforms;

	private bool scattered;

	private Run.TimeStamp scatterStartTime = Run.TimeStamp.positiveInfinity;

	private const string roachScatterSoundString = "Play_env_roach_scatter";

	private void Awake()
	{
		//IL_0040: Unknown result type (might be due to invalid IL or missing references)
		//IL_005c: Unknown result type (might be due to invalid IL or missing references)
		roachTransforms = (Transform[])(object)new Transform[roaches.Length];
		for (int i = 0; i < roachTransforms.Length; i++)
		{
			roachTransforms[i] = Object.Instantiate<GameObject>(roachParams.roachPrefab, roaches[i].keyFrames[0].position, roaches[i].keyFrames[0].rotation).transform;
		}
	}

	private void OnDestroy()
	{
		for (int i = 0; i < roachTransforms.Length; i++)
		{
			if (Object.op_Implicit((Object)(object)roachTransforms[i]))
			{
				Object.Destroy((Object)(object)((Component)roachTransforms[i]).gameObject);
			}
		}
	}

	public void BakeRoaches2()
	{
		//IL_0015: Unknown result type (might be due to invalid IL or missing references)
		//IL_0020: Unknown result type (might be due to invalid IL or missing references)
		//IL_0045: Unknown result type (might be due to invalid IL or missing references)
		//IL_004f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0061: Unknown result type (might be due to invalid IL or missing references)
		//IL_0077: Unknown result type (might be due to invalid IL or missing references)
		//IL_007e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0088: Unknown result type (might be due to invalid IL or missing references)
		//IL_008d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0094: Unknown result type (might be due to invalid IL or missing references)
		//IL_009b: Unknown result type (might be due to invalid IL or missing references)
		//IL_00df: Unknown result type (might be due to invalid IL or missing references)
		//IL_00e4: Unknown result type (might be due to invalid IL or missing references)
		//IL_00f2: Unknown result type (might be due to invalid IL or missing references)
		//IL_00f7: Unknown result type (might be due to invalid IL or missing references)
		//IL_0134: Unknown result type (might be due to invalid IL or missing references)
		//IL_013b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0140: Unknown result type (might be due to invalid IL or missing references)
		//IL_014a: Unknown result type (might be due to invalid IL or missing references)
		//IL_014f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0154: Unknown result type (might be due to invalid IL or missing references)
		//IL_0159: Unknown result type (might be due to invalid IL or missing references)
		List<Roach> list = new List<Roach>();
		Ray val = default(Ray);
		RaycastHit val2 = default(RaycastHit);
		for (int i = 0; i < roachCount; i++)
		{
			((Ray)(ref val))._002Ector(((Component)this).transform.position, Util.ApplySpread(((Component)this).transform.forward, placementSpreadMin, placementSpreadMax, 1f, 1f));
			if (Physics.Raycast(val, ref val2, placementMaxDistance, LayerMask.op_Implicit(LayerIndex.world.mask)))
			{
				SimulatedRoach simulatedRoach = new SimulatedRoach(((RaycastHit)(ref val2)).point + ((RaycastHit)(ref val2)).normal * 0.01f, ((RaycastHit)(ref val2)).normal, ((Ray)(ref val)).direction, roachParams);
				float keyframeInterval = roachParams.keyframeInterval;
				List<KeyFrame> list2 = new List<KeyFrame>();
				while (!simulatedRoach.finished)
				{
					simulatedRoach.Simulate(keyframeInterval);
					list2.Add(new KeyFrame
					{
						position = simulatedRoach.transform.position,
						rotation = simulatedRoach.transform.rotation,
						time = simulatedRoach.age
					});
				}
				KeyFrame value = list2[list2.Count - 1];
				ref Vector3 position = ref value.position;
				position += value.rotation * (Vector3.down * 0.25f);
				list2[list2.Count - 1] = value;
				simulatedRoach.Dispose();
				list.Add(new Roach
				{
					keyFrames = list2.ToArray()
				});
			}
		}
		roaches = list.ToArray();
	}

	public void BakeRoaches()
	{
		BakeRoaches2();
	}

	private void ClearRoachPathEditors()
	{
		for (int num = ((Component)this).transform.childCount - 1; num > 0; num--)
		{
			Object.DestroyImmediate((Object)(object)((Component)((Component)this).transform.GetChild(num)).gameObject);
		}
	}

	public void DebakeRoaches()
	{
		//IL_003e: Unknown result type (might be due to invalid IL or missing references)
		//IL_004f: Unknown result type (might be due to invalid IL or missing references)
		ClearRoachPathEditors();
		for (int i = 0; i < roaches.Length; i++)
		{
			Roach roach = roaches[i];
			RoachPathEditorComponent roachPathEditorComponent = AddPathEditorObject();
			for (int j = 0; j < roach.keyFrames.Length; j++)
			{
				KeyFrame keyFrame = roach.keyFrames[j];
				RoachNodeEditorComponent roachNodeEditorComponent = roachPathEditorComponent.AddNode();
				((Component)roachNodeEditorComponent).transform.position = keyFrame.position;
				((Component)roachNodeEditorComponent).transform.rotation = keyFrame.rotation;
			}
		}
	}

	public RoachPathEditorComponent AddPathEditorObject()
	{
		//IL_0005: Unknown result type (might be due to invalid IL or missing references)
		//IL_000a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0012: Unknown result type (might be due to invalid IL or missing references)
		GameObject val = new GameObject("Roach Path (Temporary)")
		{
			hideFlags = (HideFlags)52
		};
		val.transform.SetParent(((Component)this).transform, false);
		RoachPathEditorComponent roachPathEditorComponent = val.AddComponent<RoachPathEditorComponent>();
		roachPathEditorComponent.roachController = this;
		return roachPathEditorComponent;
	}

	private void UpdateRoach(int i)
	{
		//IL_0071: Unknown result type (might be due to invalid IL or missing references)
		//IL_0078: Unknown result type (might be due to invalid IL or missing references)
		//IL_007f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0085: Unknown result type (might be due to invalid IL or missing references)
		//IL_008c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0093: Unknown result type (might be due to invalid IL or missing references)
		KeyFrame[] keyFrames = roaches[i].keyFrames;
		float num = Mathf.Min(scatterStartTime.timeSince, keyFrames[^1].time);
		for (int j = 1; j < keyFrames.Length; j++)
		{
			if (num <= keyFrames[j].time)
			{
				KeyFrame keyFrame = keyFrames[j - 1];
				KeyFrame keyFrame2 = keyFrames[j];
				float num2 = Mathf.InverseLerp(keyFrame.time, keyFrame2.time, num);
				SetRoachPosition(i, Vector3.Lerp(keyFrame.position, keyFrame2.position, num2), Quaternion.Slerp(keyFrame.rotation, keyFrame2.rotation, num2));
				break;
			}
		}
	}

	private void SetRoachPosition(int i, Vector3 position, Quaternion rotation)
	{
		//IL_0008: Unknown result type (might be due to invalid IL or missing references)
		//IL_0009: Unknown result type (might be due to invalid IL or missing references)
		roachTransforms[i].SetPositionAndRotation(position, rotation);
	}

	private void Update()
	{
		for (int i = 0; i < roaches.Length; i++)
		{
			UpdateRoach(i);
		}
	}

	private void Scatter()
	{
		if (!scattered)
		{
			Util.PlaySound("Play_env_roach_scatter", ((Component)this).gameObject);
			scattered = true;
			scatterStartTime = Run.TimeStamp.now;
		}
	}

	private void OnTriggerEnter(Collider other)
	{
		CharacterBody component = ((Component)other).GetComponent<CharacterBody>();
		if (component != null && component.isPlayerControlled)
		{
			Scatter();
		}
	}

	private void OnDrawGizmos()
	{
		//IL_0000: Unknown result type (might be due to invalid IL or missing references)
		//IL_0010: Unknown result type (might be due to invalid IL or missing references)
		//IL_001b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0020: Unknown result type (might be due to invalid IL or missing references)
		//IL_0025: Unknown result type (might be due to invalid IL or missing references)
		//IL_002f: Unknown result type (might be due to invalid IL or missing references)
		Gizmos.color = Color.yellow;
		Gizmos.matrix = Matrix4x4.TRS(((Component)this).transform.position, ((Component)this).transform.rotation, Vector3.one);
		Gizmos.DrawFrustum(Vector3.zero, placementSpreadMax * 0.5f, placementMaxDistance, 0f, 1f);
	}
}
