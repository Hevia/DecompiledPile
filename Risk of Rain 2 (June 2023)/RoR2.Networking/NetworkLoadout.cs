using System;
using UnityEngine;
using UnityEngine.Networking;

namespace RoR2.Networking;

public class NetworkLoadout : NetworkBehaviour
{
	private static readonly Loadout temp;

	private readonly Loadout loadout = new Loadout();

	private static int kCmdCmdSendLoadout;

	public event Action onLoadoutUpdated;

	public void CopyLoadout(Loadout dest)
	{
		loadout.Copy(dest);
	}

	public void SetLoadout(Loadout src)
	{
		src.Copy(loadout);
		if (NetworkServer.active)
		{
			((NetworkBehaviour)this).SetDirtyBit(1u);
		}
		else if (((NetworkBehaviour)this).isLocalPlayer)
		{
			SendLoadoutClient();
		}
		OnLoadoutUpdated();
	}

	[Command]
	private void CmdSendLoadout(byte[] bytes)
	{
		//IL_0001: Unknown result type (might be due to invalid IL or missing references)
		//IL_0007: Expected O, but got Unknown
		NetworkReader reader = new NetworkReader(bytes);
		temp.Deserialize(reader);
		SetLoadout(temp);
	}

	[Client]
	private void SendLoadoutClient()
	{
		//IL_0015: Unknown result type (might be due to invalid IL or missing references)
		//IL_001b: Expected O, but got Unknown
		if (!NetworkClient.active)
		{
			Debug.LogWarning((object)"[Client] function 'System.Void RoR2.Networking.NetworkLoadout::SendLoadoutClient()' called on server");
			return;
		}
		NetworkWriter val = new NetworkWriter();
		loadout.Serialize(val);
		CallCmdSendLoadout(val.ToArray());
	}

	private void OnLoadoutUpdated()
	{
		this.onLoadoutUpdated?.Invoke();
	}

	public override bool OnSerialize(NetworkWriter writer, bool initialState)
	{
		uint num = ((NetworkBehaviour)this).syncVarDirtyBits;
		if (initialState)
		{
			num = 1u;
		}
		writer.WritePackedUInt32(num);
		if ((num & (true ? 1u : 0u)) != 0)
		{
			loadout.Serialize(writer);
		}
		return num != 0;
	}

	public override void OnDeserialize(NetworkReader reader, bool initialState)
	{
		if ((reader.ReadPackedUInt32() & (true ? 1u : 0u)) != 0)
		{
			temp.Deserialize(reader);
			if (!((NetworkBehaviour)this).isLocalPlayer)
			{
				temp.Copy(loadout);
				OnLoadoutUpdated();
			}
		}
	}

	static NetworkLoadout()
	{
		//IL_002a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0034: Expected O, but got Unknown
		temp = new Loadout();
		kCmdCmdSendLoadout = 1217513894;
		NetworkBehaviour.RegisterCommandDelegate(typeof(NetworkLoadout), kCmdCmdSendLoadout, new CmdDelegate(InvokeCmdCmdSendLoadout));
		NetworkCRC.RegisterBehaviour("NetworkLoadout", 0);
	}

	private void UNetVersion()
	{
	}

	protected static void InvokeCmdCmdSendLoadout(NetworkBehaviour obj, NetworkReader reader)
	{
		if (!NetworkServer.active)
		{
			Debug.LogError((object)"Command CmdSendLoadout called on client.");
		}
		else
		{
			((NetworkLoadout)(object)obj).CmdSendLoadout(reader.ReadBytesAndSize());
		}
	}

	public void CallCmdSendLoadout(byte[] bytes)
	{
		//IL_002d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0033: Expected O, but got Unknown
		//IL_0058: Unknown result type (might be due to invalid IL or missing references)
		if (!NetworkClient.active)
		{
			Debug.LogError((object)"Command function CmdSendLoadout called on server.");
			return;
		}
		if (((NetworkBehaviour)this).isServer)
		{
			CmdSendLoadout(bytes);
			return;
		}
		NetworkWriter val = new NetworkWriter();
		val.Write((short)0);
		val.Write((short)5);
		val.WritePackedUInt32((uint)kCmdCmdSendLoadout);
		val.Write(((Component)this).GetComponent<NetworkIdentity>().netId);
		val.WriteBytesFull(bytes);
		((NetworkBehaviour)this).SendCommandInternal(val, 0, "CmdSendLoadout");
	}

	public override void PreStartClient()
	{
	}
}
