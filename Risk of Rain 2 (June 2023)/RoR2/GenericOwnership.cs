using System;
using System.Runtime.InteropServices;
using UnityEngine;
using UnityEngine.Networking;

namespace RoR2;

public class GenericOwnership : NetworkBehaviour
{
	[SyncVar(hook = "SetOwnerClient")]
	private NetworkInstanceId ownerInstanceId;

	private GameObject cachedOwnerObject;

	public GameObject ownerObject
	{
		get
		{
			return cachedOwnerObject;
		}
		[Server]
		set
		{
			//IL_005b: Unknown result type (might be due to invalid IL or missing references)
			//IL_006f: Unknown result type (might be due to invalid IL or missing references)
			//IL_0078: Unknown result type (might be due to invalid IL or missing references)
			if (!NetworkServer.active)
			{
				Debug.LogWarning((object)"[Server] function 'System.Void RoR2.GenericOwnership::set_ownerObject(UnityEngine.GameObject)' called on client");
				return;
			}
			if (!Object.op_Implicit((Object)(object)value))
			{
				value = null;
			}
			if (cachedOwnerObject != value)
			{
				cachedOwnerObject = value;
				GameObject obj = cachedOwnerObject;
				NetworkInstanceId? obj2;
				if (obj == null)
				{
					obj2 = null;
				}
				else
				{
					NetworkIdentity component = obj.GetComponent<NetworkIdentity>();
					obj2 = ((component != null) ? new NetworkInstanceId?(component.netId) : null);
				}
				NetworkownerInstanceId = (NetworkInstanceId)(((_003F?)obj2) ?? NetworkInstanceId.Invalid);
				this.onOwnerChanged?.Invoke(cachedOwnerObject);
			}
		}
	}

	public NetworkInstanceId NetworkownerInstanceId
	{
		get
		{
			//IL_0001: Unknown result type (might be due to invalid IL or missing references)
			return ownerInstanceId;
		}
		[param: In]
		set
		{
			//IL_001d: Unknown result type (might be due to invalid IL or missing references)
			//IL_002c: Unknown result type (might be due to invalid IL or missing references)
			if (NetworkServer.localClientActive && !((NetworkBehaviour)this).syncVarHookGuard)
			{
				((NetworkBehaviour)this).syncVarHookGuard = true;
				SetOwnerClient(value);
				((NetworkBehaviour)this).syncVarHookGuard = false;
			}
			((NetworkBehaviour)this).SetSyncVar<NetworkInstanceId>(value, ref ownerInstanceId, 1u);
		}
	}

	public event Action<GameObject> onOwnerChanged;

	private void SetOwnerClient(NetworkInstanceId id)
	{
		//IL_0009: Unknown result type (might be due to invalid IL or missing references)
		if (!NetworkServer.active)
		{
			cachedOwnerObject = ClientScene.FindLocalObject(id);
			this.onOwnerChanged?.Invoke(cachedOwnerObject);
		}
	}

	public override void OnStartClient()
	{
		//IL_0008: Unknown result type (might be due to invalid IL or missing references)
		((NetworkBehaviour)this).OnStartClient();
		SetOwnerClient(ownerInstanceId);
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
			writer.Write(ownerInstanceId);
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
			writer.Write(ownerInstanceId);
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
		if (initialState)
		{
			ownerInstanceId = reader.ReadNetworkId();
			return;
		}
		int num = (int)reader.ReadPackedUInt32();
		if (((uint)num & (true ? 1u : 0u)) != 0)
		{
			SetOwnerClient(reader.ReadNetworkId());
		}
	}

	public override void PreStartClient()
	{
	}
}
