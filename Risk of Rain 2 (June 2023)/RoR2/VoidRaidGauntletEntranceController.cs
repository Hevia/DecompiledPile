using System.Runtime.InteropServices;
using UnityEngine;
using UnityEngine.Networking;

namespace RoR2;

public class VoidRaidGauntletEntranceController : NetworkBehaviour
{
	[SerializeField]
	private MapZone entranceZone;

	[SyncVar(hook = "UpdateGauntletIndex")]
	private int gauntletIndex;

	private static int kRpcRpcUpdateGauntletIndex;

	public int NetworkgauntletIndex
	{
		get
		{
			return gauntletIndex;
		}
		[param: In]
		set
		{
			if (NetworkServer.localClientActive && !((NetworkBehaviour)this).syncVarHookGuard)
			{
				((NetworkBehaviour)this).syncVarHookGuard = true;
				UpdateGauntletIndex(value);
				((NetworkBehaviour)this).syncVarHookGuard = false;
			}
			((NetworkBehaviour)this).SetSyncVar<int>(value, ref gauntletIndex, 1u);
		}
	}

	[Server]
	public void SetGauntletIndex(int newGauntletIndex)
	{
		if (!NetworkServer.active)
		{
			Debug.LogWarning((object)"[Server] function 'System.Void RoR2.VoidRaidGauntletEntranceController::SetGauntletIndex(System.Int32)' called on client");
			return;
		}
		NetworkgauntletIndex = newGauntletIndex;
		UpdateGauntletIndex(gauntletIndex);
		CallRpcUpdateGauntletIndex(gauntletIndex);
	}

	private void OnEnable()
	{
		if (Object.op_Implicit((Object)(object)entranceZone))
		{
			entranceZone.onBodyTeleport += OnBodyTeleport;
		}
	}

	private void OnDisable()
	{
		if (Object.op_Implicit((Object)(object)entranceZone))
		{
			entranceZone.onBodyTeleport -= OnBodyTeleport;
		}
	}

	private void OnBodyTeleport(CharacterBody body)
	{
		if (Util.HasEffectiveAuthority(((Component)body).gameObject) && Object.op_Implicit((Object)(object)body.masterObject.GetComponent<PlayerCharacterMasterController>()) && Object.op_Implicit((Object)(object)VoidRaidGauntletController.instance))
		{
			body.healthComponent.CallCmdHealFull();
			body.healthComponent.CallCmdRechargeShieldFull();
			VoidRaidGauntletController.instance.OnAuthorityPlayerEnter();
		}
	}

	private void UpdateGauntletIndex(int newGauntletIndex)
	{
		if (newGauntletIndex != gauntletIndex)
		{
			NetworkgauntletIndex = newGauntletIndex;
		}
		if (Object.op_Implicit((Object)(object)entranceZone) && Object.op_Implicit((Object)(object)VoidRaidGauntletController.instance))
		{
			VoidRaidGauntletController.instance.PointZoneToGauntlet(newGauntletIndex, entranceZone);
		}
	}

	[ClientRpc]
	private void RpcUpdateGauntletIndex(int newGauntletIndex)
	{
		UpdateGauntletIndex(newGauntletIndex);
	}

	private void UNetVersion()
	{
	}

	protected static void InvokeRpcRpcUpdateGauntletIndex(NetworkBehaviour obj, NetworkReader reader)
	{
		if (!NetworkClient.active)
		{
			Debug.LogError((object)"RPC RpcUpdateGauntletIndex called on server.");
		}
		else
		{
			((VoidRaidGauntletEntranceController)(object)obj).RpcUpdateGauntletIndex((int)reader.ReadPackedUInt32());
		}
	}

	public void CallRpcUpdateGauntletIndex(int newGauntletIndex)
	{
		//IL_0016: Unknown result type (might be due to invalid IL or missing references)
		//IL_001c: Expected O, but got Unknown
		//IL_0041: Unknown result type (might be due to invalid IL or missing references)
		if (!NetworkServer.active)
		{
			Debug.LogError((object)"RPC Function RpcUpdateGauntletIndex called on client.");
			return;
		}
		NetworkWriter val = new NetworkWriter();
		val.Write((short)0);
		val.Write((short)2);
		val.WritePackedUInt32((uint)kRpcRpcUpdateGauntletIndex);
		val.Write(((Component)this).GetComponent<NetworkIdentity>().netId);
		val.WritePackedUInt32((uint)newGauntletIndex);
		((NetworkBehaviour)this).SendRPCInternal(val, 0, "RpcUpdateGauntletIndex");
	}

	static VoidRaidGauntletEntranceController()
	{
		//IL_0020: Unknown result type (might be due to invalid IL or missing references)
		//IL_002a: Expected O, but got Unknown
		kRpcRpcUpdateGauntletIndex = -330092625;
		NetworkBehaviour.RegisterRpcDelegate(typeof(VoidRaidGauntletEntranceController), kRpcRpcUpdateGauntletIndex, new CmdDelegate(InvokeRpcRpcUpdateGauntletIndex));
		NetworkCRC.RegisterBehaviour("VoidRaidGauntletEntranceController", 0);
	}

	public override bool OnSerialize(NetworkWriter writer, bool forceAll)
	{
		if (forceAll)
		{
			writer.WritePackedUInt32((uint)gauntletIndex);
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
			writer.WritePackedUInt32((uint)gauntletIndex);
		}
		if (!flag)
		{
			writer.WritePackedUInt32(((NetworkBehaviour)this).syncVarDirtyBits);
		}
		return flag;
	}

	public override void OnDeserialize(NetworkReader reader, bool initialState)
	{
		if (initialState)
		{
			gauntletIndex = (int)reader.ReadPackedUInt32();
			return;
		}
		int num = (int)reader.ReadPackedUInt32();
		if (((uint)num & (true ? 1u : 0u)) != 0)
		{
			UpdateGauntletIndex((int)reader.ReadPackedUInt32());
		}
	}

	public override void PreStartClient()
	{
	}
}
