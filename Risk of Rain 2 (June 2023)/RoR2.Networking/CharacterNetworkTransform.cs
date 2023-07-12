using System.Collections.Generic;
using System.Collections.ObjectModel;
using KinematicCharacterController;
using UnityEngine;
using UnityEngine.Networking;

namespace RoR2.Networking;

public class CharacterNetworkTransform : NetworkBehaviour
{
	public struct Snapshot
	{
		public float serverTime;

		public Vector3 position;

		public Vector3 moveVector;

		public Vector3 aimDirection;

		public Quaternion rotation;

		public bool isGrounded;

		public Vector3 groundNormal;

		private static bool LerpGroundNormal(ref Snapshot a, ref Snapshot b, float t, out Vector3 groundNormal)
		{
			//IL_0001: Unknown result type (might be due to invalid IL or missing references)
			//IL_0006: Unknown result type (might be due to invalid IL or missing references)
			//IL_0036: Unknown result type (might be due to invalid IL or missing references)
			//IL_003c: Unknown result type (might be due to invalid IL or missing references)
			//IL_0042: Unknown result type (might be due to invalid IL or missing references)
			//IL_0047: Unknown result type (might be due to invalid IL or missing references)
			//IL_004f: Unknown result type (might be due to invalid IL or missing references)
			//IL_0054: Unknown result type (might be due to invalid IL or missing references)
			//IL_005c: Unknown result type (might be due to invalid IL or missing references)
			//IL_0061: Unknown result type (might be due to invalid IL or missing references)
			groundNormal = Vector3.zero;
			bool num = ((t > 0f) ? b.isGrounded : a.isGrounded);
			if (num)
			{
				if (b.isGrounded)
				{
					if (a.isGrounded)
					{
						groundNormal = Vector3.Slerp(a.groundNormal, b.groundNormal, t);
						return num;
					}
					groundNormal = b.groundNormal;
					return num;
				}
				groundNormal = a.groundNormal;
			}
			return num;
		}

		public static Snapshot Lerp(Snapshot a, Snapshot b, float t)
		{
			//IL_0018: Unknown result type (might be due to invalid IL or missing references)
			//IL_001e: Unknown result type (might be due to invalid IL or missing references)
			//IL_0024: Unknown result type (might be due to invalid IL or missing references)
			//IL_0029: Unknown result type (might be due to invalid IL or missing references)
			//IL_0031: Unknown result type (might be due to invalid IL or missing references)
			//IL_0037: Unknown result type (might be due to invalid IL or missing references)
			//IL_003d: Unknown result type (might be due to invalid IL or missing references)
			//IL_0042: Unknown result type (might be due to invalid IL or missing references)
			//IL_004a: Unknown result type (might be due to invalid IL or missing references)
			//IL_0050: Unknown result type (might be due to invalid IL or missing references)
			//IL_0056: Unknown result type (might be due to invalid IL or missing references)
			//IL_005b: Unknown result type (might be due to invalid IL or missing references)
			//IL_0063: Unknown result type (might be due to invalid IL or missing references)
			//IL_0069: Unknown result type (might be due to invalid IL or missing references)
			//IL_006f: Unknown result type (might be due to invalid IL or missing references)
			//IL_0074: Unknown result type (might be due to invalid IL or missing references)
			//IL_0083: Unknown result type (might be due to invalid IL or missing references)
			//IL_0084: Unknown result type (might be due to invalid IL or missing references)
			Vector3 val;
			bool flag = LerpGroundNormal(ref a, ref b, t, out val);
			Snapshot result = default(Snapshot);
			result.position = Vector3.Lerp(a.position, b.position, t);
			result.moveVector = Vector3.Lerp(a.moveVector, b.moveVector, t);
			result.aimDirection = Vector3.Slerp(a.aimDirection, b.moveVector, t);
			result.rotation = Quaternion.Lerp(a.rotation, b.rotation, t);
			result.isGrounded = flag;
			result.groundNormal = val;
			return result;
		}

		public static Snapshot Interpolate(Snapshot a, Snapshot b, float serverTime)
		{
			//IL_0037: Unknown result type (might be due to invalid IL or missing references)
			//IL_003d: Unknown result type (might be due to invalid IL or missing references)
			//IL_0043: Unknown result type (might be due to invalid IL or missing references)
			//IL_0048: Unknown result type (might be due to invalid IL or missing references)
			//IL_0050: Unknown result type (might be due to invalid IL or missing references)
			//IL_0056: Unknown result type (might be due to invalid IL or missing references)
			//IL_005c: Unknown result type (might be due to invalid IL or missing references)
			//IL_0061: Unknown result type (might be due to invalid IL or missing references)
			//IL_0069: Unknown result type (might be due to invalid IL or missing references)
			//IL_006f: Unknown result type (might be due to invalid IL or missing references)
			//IL_0075: Unknown result type (might be due to invalid IL or missing references)
			//IL_007a: Unknown result type (might be due to invalid IL or missing references)
			//IL_0082: Unknown result type (might be due to invalid IL or missing references)
			//IL_0088: Unknown result type (might be due to invalid IL or missing references)
			//IL_008e: Unknown result type (might be due to invalid IL or missing references)
			//IL_0093: Unknown result type (might be due to invalid IL or missing references)
			//IL_00a2: Unknown result type (might be due to invalid IL or missing references)
			//IL_00a3: Unknown result type (might be due to invalid IL or missing references)
			float num = (serverTime - a.serverTime) / (b.serverTime - a.serverTime);
			Vector3 val;
			bool flag = LerpGroundNormal(ref a, ref b, num, out val);
			Snapshot result = default(Snapshot);
			result.serverTime = serverTime;
			result.position = Vector3.LerpUnclamped(a.position, b.position, num);
			result.moveVector = Vector3.Lerp(a.moveVector, b.moveVector, num);
			result.aimDirection = Vector3.Slerp(a.aimDirection, b.aimDirection, num);
			result.rotation = Quaternion.Lerp(a.rotation, b.rotation, num);
			result.isGrounded = flag;
			result.groundNormal = val;
			return result;
		}
	}

	private static List<CharacterNetworkTransform> instancesList = new List<CharacterNetworkTransform>();

	private static ReadOnlyCollection<CharacterNetworkTransform> _readOnlyInstancesList = new ReadOnlyCollection<CharacterNetworkTransform>(instancesList);

	[Tooltip("The delay in seconds between position network updates.")]
	public float positionTransmitInterval = 0.1f;

	[HideInInspector]
	public float lastPositionTransmitTime = float.NegativeInfinity;

	[Tooltip("The number of packets of buffers to have.")]
	public float interpolationFactor = 2f;

	public Snapshot newestNetSnapshot;

	private List<Snapshot> snapshots = new List<Snapshot>();

	public bool debugDuplicatePositions;

	public bool debugSnapshotReceived;

	private bool rigidbodyStartedKinematic = true;

	public static ReadOnlyCollection<CharacterNetworkTransform> readOnlyInstancesList => _readOnlyInstancesList;

	public Transform transform { get; private set; }

	public InputBankTest inputBank { get; private set; }

	public CharacterMotor characterMotor { get; set; }

	public CharacterDirection characterDirection { get; private set; }

	private Rigidbody rigidbody { get; set; }

	public float interpolationDelay => positionTransmitInterval * interpolationFactor;

	public bool hasEffectiveAuthority { get; private set; }

	private Snapshot CalcCurrentSnapshot(float time, float interpolationDelay)
	{
		float num = time - interpolationDelay;
		if (snapshots.Count < 2)
		{
			Snapshot result = ((snapshots.Count == 0) ? BuildSnapshot() : snapshots[0]);
			result.serverTime = num;
			return result;
		}
		int i;
		for (i = 0; i < snapshots.Count - 2 && (!(snapshots[i].serverTime <= num) || !(snapshots[i + 1].serverTime >= num)); i++)
		{
		}
		return Snapshot.Interpolate(snapshots[i], snapshots[i + 1], num);
	}

	private Snapshot BuildSnapshot()
	{
		//IL_0021: Unknown result type (might be due to invalid IL or missing references)
		//IL_0026: Unknown result type (might be due to invalid IL or missing references)
		//IL_003a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0047: Unknown result type (might be due to invalid IL or missing references)
		//IL_004c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0060: Unknown result type (might be due to invalid IL or missing references)
		//IL_006d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0072: Unknown result type (might be due to invalid IL or missing references)
		//IL_008c: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a8: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ad: Unknown result type (might be due to invalid IL or missing references)
		//IL_00e3: Unknown result type (might be due to invalid IL or missing references)
		//IL_00f0: Unknown result type (might be due to invalid IL or missing references)
		//IL_00f5: Unknown result type (might be due to invalid IL or missing references)
		Snapshot result = default(Snapshot);
		result.serverTime = PlatformSystems.networkManager.serverFixedTime;
		result.position = transform.position;
		result.moveVector = (Object.op_Implicit((Object)(object)inputBank) ? inputBank.moveVector : Vector3.zero);
		result.aimDirection = (Object.op_Implicit((Object)(object)inputBank) ? inputBank.aimDirection : Vector3.zero);
		result.rotation = (Object.op_Implicit((Object)(object)characterDirection) ? Quaternion.Euler(0f, characterDirection.yaw, 0f) : transform.rotation);
		result.isGrounded = Object.op_Implicit((Object)(object)characterMotor) && characterMotor.isGrounded;
		result.groundNormal = (Object.op_Implicit((Object)(object)characterMotor) ? characterMotor.estimatedGroundNormal : Vector3.zero);
		return result;
	}

	public void PushSnapshot(Snapshot newSnapshot)
	{
		//IL_0075: Unknown result type (might be due to invalid IL or missing references)
		//IL_0092: Unknown result type (might be due to invalid IL or missing references)
		//IL_00f6: Unknown result type (might be due to invalid IL or missing references)
		//IL_00fb: Unknown result type (might be due to invalid IL or missing references)
		//IL_0100: Unknown result type (might be due to invalid IL or missing references)
		//IL_0106: Unknown result type (might be due to invalid IL or missing references)
		//IL_010b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0110: Unknown result type (might be due to invalid IL or missing references)
		//IL_0115: Unknown result type (might be due to invalid IL or missing references)
		if (debugSnapshotReceived)
		{
			Debug.LogFormat("{0} CharacterNetworkTransform snapshot received.", new object[1] { ((Component)this).gameObject });
		}
		if (snapshots.Count > 0 && newSnapshot.serverTime == snapshots[snapshots.Count - 1].serverTime)
		{
			Debug.Log((object)"Received duplicate time!");
		}
		if (debugDuplicatePositions && snapshots.Count > 0 && newSnapshot.position == snapshots[snapshots.Count - 1].position)
		{
			Debug.Log((object)"Received duplicate position!");
		}
		if (((snapshots.Count > 0) ? snapshots[snapshots.Count - 1].serverTime : float.NegativeInfinity) < newSnapshot.serverTime)
		{
			snapshots.Add(newSnapshot);
			newestNetSnapshot = newSnapshot;
			Debug.DrawLine(newSnapshot.position + Vector3.up, newSnapshot.position + Vector3.down, Color.white, 0.25f);
		}
		float num = PlatformSystems.networkManager.serverFixedTime - interpolationDelay * 3f;
		while (snapshots.Count > 2 && snapshots[1].serverTime < num)
		{
			snapshots.RemoveAt(0);
		}
	}

	private void Awake()
	{
		transform = ((Component)this).transform;
		inputBank = ((Component)this).GetComponent<InputBankTest>();
		characterMotor = ((Component)this).GetComponent<CharacterMotor>();
		characterDirection = ((Component)this).GetComponent<CharacterDirection>();
		rigidbody = ((Component)this).GetComponent<Rigidbody>();
		if (Object.op_Implicit((Object)(object)rigidbody))
		{
			rigidbodyStartedKinematic = rigidbody.isKinematic;
		}
	}

	private void Start()
	{
		newestNetSnapshot = BuildSnapshot();
		UpdateAuthority();
	}

	private void OnEnable()
	{
		bool num = instancesList.Contains(this);
		instancesList.Add(this);
		if (num)
		{
			Debug.LogError((object)"Instance already in list!");
		}
	}

	private void OnDisable()
	{
		instancesList.Remove(this);
		if (instancesList.Contains(this))
		{
			Debug.LogError((object)"Instance was not fully removed from list!");
		}
	}

	private void UpdateAuthority()
	{
		hasEffectiveAuthority = Util.HasEffectiveAuthority(((Component)this).gameObject);
		if (Object.op_Implicit((Object)(object)rigidbody))
		{
			rigidbody.isKinematic = !hasEffectiveAuthority || rigidbodyStartedKinematic;
		}
	}

	public override void OnStartAuthority()
	{
		UpdateAuthority();
	}

	public override void OnStopAuthority()
	{
		UpdateAuthority();
	}

	private void ApplyCurrentSnapshot(float currentTime)
	{
		//IL_002a: Unknown result type (might be due to invalid IL or missing references)
		//IL_003d: Unknown result type (might be due to invalid IL or missing references)
		//IL_005b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0060: Unknown result type (might be due to invalid IL or missing references)
		//IL_006c: Unknown result type (might be due to invalid IL or missing references)
		//IL_009b: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a0: Unknown result type (might be due to invalid IL or missing references)
		//IL_00c3: Unknown result type (might be due to invalid IL or missing references)
		//IL_00db: Unknown result type (might be due to invalid IL or missing references)
		//IL_0100: Unknown result type (might be due to invalid IL or missing references)
		//IL_011f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0131: Unknown result type (might be due to invalid IL or missing references)
		Snapshot snapshot = CalcCurrentSnapshot(currentTime, interpolationDelay);
		if (!Object.op_Implicit((Object)(object)characterMotor))
		{
			if (rigidbodyStartedKinematic)
			{
				transform.position = snapshot.position;
			}
			else
			{
				rigidbody.MovePosition(snapshot.position);
			}
		}
		if (Object.op_Implicit((Object)(object)inputBank))
		{
			inputBank.moveVector = snapshot.moveVector;
			inputBank.aimDirection = snapshot.aimDirection;
		}
		if (Object.op_Implicit((Object)(object)characterMotor))
		{
			characterMotor.netIsGrounded = snapshot.isGrounded;
			characterMotor.netGroundNormal = snapshot.groundNormal;
			if (((Behaviour)((BaseCharacterController)characterMotor).Motor).enabled)
			{
				((BaseCharacterController)characterMotor).Motor.MoveCharacter(snapshot.position);
			}
			else
			{
				((BaseCharacterController)characterMotor).Motor.SetPosition(snapshot.position, true);
			}
		}
		if (Object.op_Implicit((Object)(object)characterDirection))
		{
			characterDirection.yaw = ((Quaternion)(ref snapshot.rotation)).eulerAngles.y;
		}
		else if (rigidbodyStartedKinematic)
		{
			transform.rotation = snapshot.rotation;
		}
		else
		{
			rigidbody.MoveRotation(snapshot.rotation);
		}
	}

	private void FixedUpdate()
	{
		if (!hasEffectiveAuthority)
		{
			ApplyCurrentSnapshot(PlatformSystems.networkManager.serverFixedTime);
		}
		else
		{
			newestNetSnapshot = BuildSnapshot();
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
