using System.Runtime.InteropServices;
using UnityEngine;
using UnityEngine.Networking;

namespace RoR2.Projectile;

public class ProjectileNetworkTransform : NetworkBehaviour
{
	private ProjectileController projectileController;

	private Transform transform;

	private Rigidbody rb;

	private InterpolatedTransform interpolatedTransform;

	[Tooltip("The delay in seconds between position network updates.")]
	public float positionTransmitInterval = 1f / 30f;

	[Tooltip("The number of packets of buffers to have.")]
	public float interpolationFactor = 1f;

	public bool allowClientsideCollision;

	[SyncVar(hook = "OnSyncPosition")]
	private Vector3 serverPosition;

	[SyncVar(hook = "OnSyncRotation")]
	private Quaternion serverRotation;

	private NetworkLerpedVector3 interpolatedPosition;

	private NetworkLerpedQuaternion interpolatedRotation;

	private bool isPrediction
	{
		get
		{
			if (!Object.op_Implicit((Object)(object)projectileController))
			{
				return false;
			}
			return projectileController.isPrediction;
		}
	}

	public Vector3 NetworkserverPosition
	{
		get
		{
			//IL_0001: Unknown result type (might be due to invalid IL or missing references)
			return serverPosition;
		}
		[param: In]
		set
		{
			//IL_001d: Unknown result type (might be due to invalid IL or missing references)
			//IL_002c: Unknown result type (might be due to invalid IL or missing references)
			if (NetworkServer.localClientActive && !((NetworkBehaviour)this).syncVarHookGuard)
			{
				((NetworkBehaviour)this).syncVarHookGuard = true;
				OnSyncPosition(value);
				((NetworkBehaviour)this).syncVarHookGuard = false;
			}
			((NetworkBehaviour)this).SetSyncVar<Vector3>(value, ref serverPosition, 1u);
		}
	}

	public Quaternion NetworkserverRotation
	{
		get
		{
			//IL_0001: Unknown result type (might be due to invalid IL or missing references)
			return serverRotation;
		}
		[param: In]
		set
		{
			//IL_001d: Unknown result type (might be due to invalid IL or missing references)
			//IL_002c: Unknown result type (might be due to invalid IL or missing references)
			if (NetworkServer.localClientActive && !((NetworkBehaviour)this).syncVarHookGuard)
			{
				((NetworkBehaviour)this).syncVarHookGuard = true;
				OnSyncRotation(value);
				((NetworkBehaviour)this).syncVarHookGuard = false;
			}
			((NetworkBehaviour)this).SetSyncVar<Quaternion>(value, ref serverRotation, 2u);
		}
	}

	public void SetValuesFromTransform()
	{
		//IL_0007: Unknown result type (might be due to invalid IL or missing references)
		//IL_0018: Unknown result type (might be due to invalid IL or missing references)
		NetworkserverPosition = transform.position;
		NetworkserverRotation = transform.rotation;
	}

	private void Awake()
	{
		//IL_002b: Unknown result type (might be due to invalid IL or missing references)
		//IL_003c: Unknown result type (might be due to invalid IL or missing references)
		projectileController = ((Component)this).GetComponent<ProjectileController>();
		interpolatedTransform = ((Component)this).GetComponent<InterpolatedTransform>();
		transform = ((Component)this).transform;
		NetworkserverPosition = transform.position;
		NetworkserverRotation = transform.rotation;
		rb = ((Component)this).GetComponent<Rigidbody>();
	}

	private void Start()
	{
		//IL_001f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0030: Unknown result type (might be due to invalid IL or missing references)
		interpolatedPosition.interpDelay = ((NetworkBehaviour)this).GetNetworkSendInterval() * interpolationFactor;
		interpolatedPosition.SetValueImmediate(serverPosition);
		interpolatedRotation.SetValueImmediate(serverRotation);
		if (isPrediction)
		{
			((Behaviour)this).enabled = false;
		}
		if (Object.op_Implicit((Object)(object)rb) && !isPrediction && !NetworkServer.active)
		{
			rb.collisionDetectionMode = (CollisionDetectionMode)0;
			rb.detectCollisions = allowClientsideCollision;
			rb.isKinematic = true;
		}
	}

	private void OnSyncPosition(Vector3 newPosition)
	{
		//IL_0006: Unknown result type (might be due to invalid IL or missing references)
		//IL_000d: Unknown result type (might be due to invalid IL or missing references)
		interpolatedPosition.PushValue(newPosition);
		NetworkserverPosition = newPosition;
	}

	private void OnSyncRotation(Quaternion newRotation)
	{
		//IL_0006: Unknown result type (might be due to invalid IL or missing references)
		//IL_000d: Unknown result type (might be due to invalid IL or missing references)
		interpolatedRotation.PushValue(newRotation);
		NetworkserverRotation = newRotation;
	}

	public override float GetNetworkSendInterval()
	{
		return positionTransmitInterval;
	}

	private void FixedUpdate()
	{
		//IL_0027: Unknown result type (might be due to invalid IL or missing references)
		//IL_0038: Unknown result type (might be due to invalid IL or missing references)
		//IL_0049: Unknown result type (might be due to invalid IL or missing references)
		//IL_005a: Unknown result type (might be due to invalid IL or missing references)
		//IL_006c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0071: Unknown result type (might be due to invalid IL or missing references)
		//IL_0079: Unknown result type (might be due to invalid IL or missing references)
		//IL_007e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0080: Unknown result type (might be due to invalid IL or missing references)
		//IL_0081: Unknown result type (might be due to invalid IL or missing references)
		if (((NetworkBehaviour)this).isServer)
		{
			interpolatedPosition.interpDelay = ((NetworkBehaviour)this).GetNetworkSendInterval() * interpolationFactor;
			NetworkserverPosition = transform.position;
			NetworkserverRotation = transform.rotation;
			interpolatedPosition.SetValueImmediate(serverPosition);
			interpolatedRotation.SetValueImmediate(serverRotation);
		}
		else
		{
			Vector3 currentValue = interpolatedPosition.GetCurrentValue(hasAuthority: false);
			Quaternion currentValue2 = interpolatedRotation.GetCurrentValue(hasAuthority: false);
			ApplyPositionAndRotation(currentValue, currentValue2);
		}
	}

	private void ApplyPositionAndRotation(Vector3 position, Quaternion rotation)
	{
		//IL_0020: Unknown result type (might be due to invalid IL or missing references)
		//IL_002c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0039: Unknown result type (might be due to invalid IL or missing references)
		//IL_0045: Unknown result type (might be due to invalid IL or missing references)
		if (Object.op_Implicit((Object)(object)rb) && !Object.op_Implicit((Object)(object)interpolatedTransform))
		{
			rb.MovePosition(position);
			rb.MoveRotation(rotation);
		}
		else
		{
			transform.position = position;
			transform.rotation = rotation;
		}
	}

	private void UNetVersion()
	{
	}

	public override bool OnSerialize(NetworkWriter writer, bool forceAll)
	{
		//IL_0008: Unknown result type (might be due to invalid IL or missing references)
		//IL_0014: Unknown result type (might be due to invalid IL or missing references)
		//IL_004b: Unknown result type (might be due to invalid IL or missing references)
		//IL_007e: Unknown result type (might be due to invalid IL or missing references)
		if (forceAll)
		{
			writer.Write(serverPosition);
			writer.Write(serverRotation);
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
			writer.Write(serverPosition);
		}
		if ((((NetworkBehaviour)this).syncVarDirtyBits & 2u) != 0)
		{
			if (!flag)
			{
				writer.WritePackedUInt32(((NetworkBehaviour)this).syncVarDirtyBits);
				flag = true;
			}
			writer.Write(serverRotation);
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
		//IL_0014: Unknown result type (might be due to invalid IL or missing references)
		//IL_0019: Unknown result type (might be due to invalid IL or missing references)
		//IL_0035: Unknown result type (might be due to invalid IL or missing references)
		//IL_004e: Unknown result type (might be due to invalid IL or missing references)
		if (initialState)
		{
			serverPosition = reader.ReadVector3();
			serverRotation = reader.ReadQuaternion();
			return;
		}
		int num = (int)reader.ReadPackedUInt32();
		if (((uint)num & (true ? 1u : 0u)) != 0)
		{
			OnSyncPosition(reader.ReadVector3());
		}
		if (((uint)num & 2u) != 0)
		{
			OnSyncRotation(reader.ReadQuaternion());
		}
	}

	public override void PreStartClient()
	{
	}
}
