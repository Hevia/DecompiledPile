using System.Runtime.InteropServices;
using Unity;
using UnityEngine;
using UnityEngine.Networking;

namespace RoR2;

public class PickupIndexNetworker : NetworkBehaviour
{
	[SyncVar(hook = "SyncPickupIndex")]
	public PickupIndex pickupIndex;

	public PickupDisplay pickupDisplay;

	public PickupIndex NetworkpickupIndex
	{
		get
		{
			return pickupIndex;
		}
		[param: In]
		set
		{
			if (NetworkServer.localClientActive && !((NetworkBehaviour)this).syncVarHookGuard)
			{
				((NetworkBehaviour)this).syncVarHookGuard = true;
				SyncPickupIndex(value);
				((NetworkBehaviour)this).syncVarHookGuard = false;
			}
			((NetworkBehaviour)this).SetSyncVar<PickupIndex>(value, ref pickupIndex, 1u);
		}
	}

	private void SyncPickupIndex(PickupIndex newPickupIndex)
	{
		NetworkpickupIndex = newPickupIndex;
		UpdatePickupDisplay();
	}

	private void UpdatePickupDisplay()
	{
		if (Object.op_Implicit((Object)(object)pickupDisplay))
		{
			pickupDisplay.SetPickupIndex(pickupIndex);
		}
	}

	private void UNetVersion()
	{
	}

	public override bool OnSerialize(NetworkWriter writer, bool forceAll)
	{
		if (forceAll)
		{
			GeneratedNetworkCode._WritePickupIndex_None(writer, pickupIndex);
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
			GeneratedNetworkCode._WritePickupIndex_None(writer, pickupIndex);
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
			pickupIndex = GeneratedNetworkCode._ReadPickupIndex_None(reader);
			return;
		}
		int num = (int)reader.ReadPackedUInt32();
		if (((uint)num & (true ? 1u : 0u)) != 0)
		{
			SyncPickupIndex(GeneratedNetworkCode._ReadPickupIndex_None(reader));
		}
	}

	public override void PreStartClient()
	{
	}
}
