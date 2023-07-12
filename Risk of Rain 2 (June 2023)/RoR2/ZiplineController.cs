using System.Runtime.InteropServices;
using UnityEngine;
using UnityEngine.Networking;

namespace RoR2;

public class ZiplineController : NetworkBehaviour
{
	[SyncVar(hook = "SetPointAPosition")]
	private Vector3 pointAPosition;

	[SyncVar(hook = "SetPointBPosition")]
	private Vector3 pointBPosition;

	[SerializeField]
	private Transform pointATransform;

	[SerializeField]
	private Transform pointBTransform;

	public GameObject ziplineVehiclePrefab;

	private ZiplineVehicle currentZiplineA;

	private ZiplineVehicle currentZiplineB;

	public Vector3 NetworkpointAPosition
	{
		get
		{
			//IL_0001: Unknown result type (might be due to invalid IL or missing references)
			return pointAPosition;
		}
		[param: In]
		set
		{
			//IL_001d: Unknown result type (might be due to invalid IL or missing references)
			//IL_002c: Unknown result type (might be due to invalid IL or missing references)
			if (NetworkServer.localClientActive && !((NetworkBehaviour)this).syncVarHookGuard)
			{
				((NetworkBehaviour)this).syncVarHookGuard = true;
				SetPointAPosition(value);
				((NetworkBehaviour)this).syncVarHookGuard = false;
			}
			((NetworkBehaviour)this).SetSyncVar<Vector3>(value, ref pointAPosition, 1u);
		}
	}

	public Vector3 NetworkpointBPosition
	{
		get
		{
			//IL_0001: Unknown result type (might be due to invalid IL or missing references)
			return pointBPosition;
		}
		[param: In]
		set
		{
			//IL_001d: Unknown result type (might be due to invalid IL or missing references)
			//IL_002c: Unknown result type (might be due to invalid IL or missing references)
			if (NetworkServer.localClientActive && !((NetworkBehaviour)this).syncVarHookGuard)
			{
				((NetworkBehaviour)this).syncVarHookGuard = true;
				SetPointBPosition(value);
				((NetworkBehaviour)this).syncVarHookGuard = false;
			}
			((NetworkBehaviour)this).SetSyncVar<Vector3>(value, ref pointBPosition, 2u);
		}
	}

	public void SetPointAPosition(Vector3 position)
	{
		//IL_0001: Unknown result type (might be due to invalid IL or missing references)
		//IL_000e: Unknown result type (might be due to invalid IL or missing references)
		NetworkpointAPosition = position;
		pointATransform.position = pointAPosition;
		pointATransform.LookAt(pointBTransform);
		pointBTransform.LookAt(pointATransform);
	}

	public void SetPointBPosition(Vector3 position)
	{
		//IL_0001: Unknown result type (might be due to invalid IL or missing references)
		//IL_000e: Unknown result type (might be due to invalid IL or missing references)
		NetworkpointBPosition = position;
		pointBTransform.position = pointBPosition;
		pointATransform.LookAt(pointBTransform);
		pointBTransform.LookAt(pointATransform);
	}

	private void RebuildZiplineVehicle(ref ZiplineVehicle ziplineVehicle, Vector3 startPos, Vector3 endPos)
	{
		//IL_0029: Unknown result type (might be due to invalid IL or missing references)
		//IL_002a: Unknown result type (might be due to invalid IL or missing references)
		//IL_003f: Unknown result type (might be due to invalid IL or missing references)
		if (Object.op_Implicit((Object)(object)ziplineVehicle) && ziplineVehicle.vehicleSeat.hasPassenger)
		{
			ziplineVehicle = null;
		}
		if (!Object.op_Implicit((Object)(object)ziplineVehicle))
		{
			GameObject val = Object.Instantiate<GameObject>(ziplineVehiclePrefab, startPos, Quaternion.identity);
			ziplineVehicle = val.GetComponent<ZiplineVehicle>();
			ziplineVehicle.NetworkendPoint = endPos;
			NetworkServer.Spawn(val);
		}
	}

	private void FixedUpdate()
	{
		//IL_000f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0015: Unknown result type (might be due to invalid IL or missing references)
		//IL_0027: Unknown result type (might be due to invalid IL or missing references)
		//IL_002d: Unknown result type (might be due to invalid IL or missing references)
		if (NetworkServer.active)
		{
			RebuildZiplineVehicle(ref currentZiplineA, pointAPosition, pointBPosition);
			RebuildZiplineVehicle(ref currentZiplineB, pointBPosition, pointAPosition);
		}
	}

	private void OnDestroy()
	{
		if (NetworkServer.active)
		{
			if (Object.op_Implicit((Object)(object)currentZiplineA))
			{
				Object.Destroy((Object)(object)((Component)currentZiplineA).gameObject);
			}
			if (Object.op_Implicit((Object)(object)currentZiplineB))
			{
				Object.Destroy((Object)(object)((Component)currentZiplineB).gameObject);
			}
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
			writer.Write(pointAPosition);
			writer.Write(pointBPosition);
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
			writer.Write(pointAPosition);
		}
		if ((((NetworkBehaviour)this).syncVarDirtyBits & 2u) != 0)
		{
			if (!flag)
			{
				writer.WritePackedUInt32(((NetworkBehaviour)this).syncVarDirtyBits);
				flag = true;
			}
			writer.Write(pointBPosition);
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
			pointAPosition = reader.ReadVector3();
			pointBPosition = reader.ReadVector3();
			return;
		}
		int num = (int)reader.ReadPackedUInt32();
		if (((uint)num & (true ? 1u : 0u)) != 0)
		{
			SetPointAPosition(reader.ReadVector3());
		}
		if (((uint)num & 2u) != 0)
		{
			SetPointBPosition(reader.ReadVector3());
		}
	}

	public override void PreStartClient()
	{
	}
}
