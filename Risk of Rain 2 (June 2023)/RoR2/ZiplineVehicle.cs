using System.Runtime.InteropServices;
using UnityEngine;
using UnityEngine.Networking;

namespace RoR2;

[RequireComponent(typeof(VehicleSeat))]
[RequireComponent(typeof(Rigidbody))]
public class ZiplineVehicle : NetworkBehaviour
{
	public float maxSpeed = 30f;

	public float acceleration = 2f;

	private Rigidbody rigidbody;

	private Vector3 startPoint;

	[SyncVar]
	public Vector3 endPoint;

	private Vector3 travelDirection;

	private GameObject currentPassenger;

	private Run.FixedTimeStamp startTravelFixedTime = Run.FixedTimeStamp.positiveInfinity;

	private Run.TimeStamp startTravelTime = Run.TimeStamp.positiveInfinity;

	public VehicleSeat vehicleSeat { get; private set; }

	public Vector3 NetworkendPoint
	{
		get
		{
			//IL_0001: Unknown result type (might be due to invalid IL or missing references)
			return endPoint;
		}
		[param: In]
		set
		{
			//IL_0001: Unknown result type (might be due to invalid IL or missing references)
			((NetworkBehaviour)this).SetSyncVar<Vector3>(value, ref endPoint, 1u);
		}
	}

	private void Awake()
	{
		vehicleSeat = ((Component)this).GetComponent<VehicleSeat>();
		rigidbody = ((Component)this).GetComponent<Rigidbody>();
		vehicleSeat.onPassengerEnter += OnPassengerEnter;
		vehicleSeat.onPassengerExit += OnPassengerExit;
	}

	private void OnPassengerEnter(GameObject passenger)
	{
		//IL_000e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0013: Unknown result type (might be due to invalid IL or missing references)
		currentPassenger = passenger;
		startPoint = ((Component)this).transform.position;
		startTravelFixedTime = Run.FixedTimeStamp.now;
		startTravelTime = Run.TimeStamp.now;
	}

	private void SetTravelDistance(float time)
	{
		//IL_0001: Unknown result type (might be due to invalid IL or missing references)
		//IL_0007: Unknown result type (might be due to invalid IL or missing references)
		//IL_000c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0011: Unknown result type (might be due to invalid IL or missing references)
		//IL_001a: Unknown result type (might be due to invalid IL or missing references)
		//IL_001c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0021: Unknown result type (might be due to invalid IL or missing references)
		//IL_0047: Unknown result type (might be due to invalid IL or missing references)
		//IL_004c: Unknown result type (might be due to invalid IL or missing references)
		//IL_004e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0053: Unknown result type (might be due to invalid IL or missing references)
		//IL_0063: Unknown result type (might be due to invalid IL or missing references)
		//IL_006c: Unknown result type (might be due to invalid IL or missing references)
		Vector3 val = endPoint - startPoint;
		float magnitude = ((Vector3)(ref val)).magnitude;
		Vector3 val2 = val / magnitude;
		float num = HGPhysics.CalculateDistance(0f, acceleration, time);
		bool flag = false;
		if (num > magnitude)
		{
			num = magnitude;
			flag = true;
		}
		rigidbody.MovePosition(startPoint + val2 * num);
		rigidbody.velocity = val2 * (acceleration * time);
		if (NetworkServer.active && flag)
		{
			vehicleSeat.EjectPassenger(currentPassenger);
		}
	}

	private void Update()
	{
		_ = startTravelTime.hasPassed;
	}

	private void FixedUpdate()
	{
		//IL_0039: Unknown result type (might be due to invalid IL or missing references)
		//IL_0044: Unknown result type (might be due to invalid IL or missing references)
		//IL_0049: Unknown result type (might be due to invalid IL or missing references)
		//IL_004e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0051: Unknown result type (might be due to invalid IL or missing references)
		//IL_0056: Unknown result type (might be due to invalid IL or missing references)
		//IL_0057: Unknown result type (might be due to invalid IL or missing references)
		//IL_0059: Unknown result type (might be due to invalid IL or missing references)
		//IL_0088: Unknown result type (might be due to invalid IL or missing references)
		//IL_008d: Unknown result type (might be due to invalid IL or missing references)
		//IL_008e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0090: Unknown result type (might be due to invalid IL or missing references)
		//IL_009d: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a2: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a7: Unknown result type (might be due to invalid IL or missing references)
		//IL_00cb: Unknown result type (might be due to invalid IL or missing references)
		//IL_00d5: Unknown result type (might be due to invalid IL or missing references)
		//IL_00da: Unknown result type (might be due to invalid IL or missing references)
		//IL_00e1: Unknown result type (might be due to invalid IL or missing references)
		//IL_00e8: Unknown result type (might be due to invalid IL or missing references)
		//IL_00e9: Unknown result type (might be due to invalid IL or missing references)
		if (startTravelFixedTime.hasPassed)
		{
			SetTravelDistance(startTravelFixedTime.timeSince);
		}
		if (!NetworkServer.active || !Object.op_Implicit((Object)(object)currentPassenger))
		{
			return;
		}
		Vector3 val = endPoint - ((Component)this).transform.position;
		Vector3 normalized = ((Vector3)(ref val)).normalized;
		if (Vector3.Dot(normalized, travelDirection) < 0f)
		{
			vehicleSeat.EjectPassenger(currentPassenger);
			return;
		}
		float fixedDeltaTime = Time.fixedDeltaTime;
		Vector3 val2 = rigidbody.velocity;
		val2 += travelDirection * (acceleration * fixedDeltaTime);
		float sqrMagnitude = ((Vector3)(ref val2)).sqrMagnitude;
		if (sqrMagnitude > maxSpeed * maxSpeed)
		{
			float num = Mathf.Sqrt(sqrMagnitude);
			val2 *= maxSpeed / num;
		}
		rigidbody.velocity = val2;
		travelDirection = normalized;
	}

	private void OnPassengerExit(GameObject passenger)
	{
		currentPassenger = null;
		((Behaviour)vehicleSeat).enabled = false;
		if (NetworkServer.active)
		{
			((Component)this).gameObject.AddComponent<DestroyOnTimer>().duration = 0.1f;
		}
	}

	private void UNetVersion()
	{
	}

	public override bool OnSerialize(NetworkWriter writer, bool forceAll)
	{
		//IL_0008: Unknown result type (might be due to invalid IL or missing references)
		//IL_003f: Unknown result type (might be due to invalid IL or missing references)
		if (forceAll)
		{
			writer.Write(endPoint);
			return true;
		}
		bool flag = false;
		if ((((NetworkBehaviour)this).syncVarDirtyBits & (true ? 1u : 0u)) != 0)
		{
			if (!flag)
			{
				writer.WritePackedUInt32(((NetworkBehaviour)this).syncVarDirtyBits);
				flag = true;
			}
			writer.Write(endPoint);
		}
		if (!flag)
		{
			writer.WritePackedUInt32(((NetworkBehaviour)this).syncVarDirtyBits);
		}
		return flag;
	}

	public override void OnDeserialize(NetworkReader reader, bool initialState)
	{
		//IL_0008: Unknown result type (might be due to invalid IL or missing references)
		//IL_000d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0029: Unknown result type (might be due to invalid IL or missing references)
		//IL_002e: Unknown result type (might be due to invalid IL or missing references)
		if (initialState)
		{
			endPoint = reader.ReadVector3();
			return;
		}
		int num = (int)reader.ReadPackedUInt32();
		if (((uint)num & (true ? 1u : 0u)) != 0)
		{
			endPoint = reader.ReadVector3();
		}
	}

	public override void PreStartClient()
	{
	}
}
