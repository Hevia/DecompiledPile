using System;
using System.Collections.Generic;
using System.Linq;
using Epic.OnlineServices;
using Epic.OnlineServices.P2P;
using RoR2.ConVar;
using UnityEngine;
using UnityEngine.Networking;

namespace RoR2.Networking;

public class EOSNetworkConnection : NetworkConnection
{
	private static List<EOSNetworkConnection> instancesList = new List<EOSNetworkConnection>();

	public bool ignore;

	public static BoolConVar cvNetP2PDebugTransport = new BoolConVar("net_p2p_debug_transport", ConVarFlags.None, "0", "Allows p2p transport information to print to the console.");

	private static BoolConVar cvNetP2PLogMessages = new BoolConVar("net_p2p_log_messages", ConVarFlags.None, "0", "Enables logging of network messages.");

	public ProductUserId LocalUserID { get; }

	public ProductUserId RemoteUserID { get; private set; }

	public EOSNetworkConnection()
	{
	}

	public EOSNetworkConnection(ProductUserId localUserID, ProductUserId remoteUserID)
	{
		LocalUserID = localUserID;
		RemoteUserID = remoteUserID;
		instancesList.Add(this);
	}

	public bool SendConnectionRequest()
	{
		//IL_0005: Unknown result type (might be due to invalid IL or missing references)
		//IL_000a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0016: Unknown result type (might be due to invalid IL or missing references)
		//IL_0022: Unknown result type (might be due to invalid IL or missing references)
		//IL_0029: Unknown result type (might be due to invalid IL or missing references)
		//IL_0030: Unknown result type (might be due to invalid IL or missing references)
		//IL_0037: Unknown result type (might be due to invalid IL or missing references)
		//IL_003e: Unknown result type (might be due to invalid IL or missing references)
		//IL_004e: Expected O, but got Unknown
		//IL_0049: Unknown result type (might be due to invalid IL or missing references)
		//IL_004f: Invalid comparison between Unknown and I4
		return (int)NetworkManagerSystemEOS.P2pInterface.SendPacket(new SendPacketOptions
		{
			LocalUserId = LocalUserID,
			RemoteUserId = RemoteUserID,
			AllowDelayedDelivery = true,
			Channel = 0,
			Data = null,
			Reliability = (PacketReliability)2,
			SocketId = NetworkManagerSystemEOS.socketId
		}) == 0;
	}

	public override bool TransportSend(byte[] bytes, int numBytes, int channelId, out byte error)
	{
		//IL_00a0: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a5: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a7: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a8: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ab: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ad: Invalid comparison between Unknown and I4
		//IL_00af: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b1: Invalid comparison between Unknown and I4
		//IL_00b4: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b8: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ca: Unknown result type (might be due to invalid IL or missing references)
		//IL_00cf: Unknown result type (might be due to invalid IL or missing references)
		//IL_00d6: Unknown result type (might be due to invalid IL or missing references)
		//IL_00dd: Unknown result type (might be due to invalid IL or missing references)
		//IL_00e4: Unknown result type (might be due to invalid IL or missing references)
		//IL_00f0: Unknown result type (might be due to invalid IL or missing references)
		//IL_00fc: Unknown result type (might be due to invalid IL or missing references)
		//IL_0107: Unknown result type (might be due to invalid IL or missing references)
		//IL_0108: Unknown result type (might be due to invalid IL or missing references)
		//IL_010f: Expected O, but got Unknown
		//IL_0115: Unknown result type (might be due to invalid IL or missing references)
		if (ignore)
		{
			error = 0;
			return true;
		}
		base.logNetworkMessages = cvNetP2PLogMessages.value;
		if ((Handle)(object)LocalUserID == (Handle)(object)RemoteUserID)
		{
			((NetworkConnection)this).TransportReceive(bytes, numBytes, channelId);
			error = 0;
			if (cvNetP2PDebugTransport.value)
			{
				Debug.Log((object)string.Format("EOSNetworkConnection.TransportSend {0}=self {1}={2} {3}={4}", "RemoteUserID", "numBytes", numBytes, "channelId", channelId));
			}
			return true;
		}
		QosType qOS = ((NetworkManager)NetworkManagerSystem.singleton).connectionConfig.Channels[channelId].QOS;
		PacketReliability val = (PacketReliability)2;
		val = (((int)qOS != 0 && (int)qOS != 1 && (int)qOS != 2) ? ((PacketReliability)2) : ((PacketReliability)0));
		byte[] array = new byte[numBytes];
		Array.Copy(bytes, 0, array, 0, numBytes);
		SendPacketOptions val2 = new SendPacketOptions
		{
			AllowDelayedDelivery = true,
			Data = array,
			Channel = 0,
			LocalUserId = LocalUserID,
			RemoteUserId = RemoteUserID,
			SocketId = NetworkManagerSystemEOS.socketId,
			Reliability = val
		};
		if ((int)NetworkManagerSystemEOS.P2pInterface.SendPacket(val2) == 0)
		{
			error = 0;
			return true;
		}
		error = 1;
		return false;
	}

	public override void TransportReceive(byte[] bytes, int numBytes, int channelId)
	{
		if (!ignore)
		{
			base.logNetworkMessages = cvNetP2PLogMessages.value;
			((NetworkConnection)this).TransportReceive(bytes, numBytes, channelId);
		}
	}

	protected override void Dispose(bool disposing)
	{
		//IL_0021: Unknown result type (might be due to invalid IL or missing references)
		//IL_0026: Unknown result type (might be due to invalid IL or missing references)
		//IL_0032: Unknown result type (might be due to invalid IL or missing references)
		//IL_003e: Unknown result type (might be due to invalid IL or missing references)
		//IL_004e: Expected O, but got Unknown
		//IL_0049: Unknown result type (might be due to invalid IL or missing references)
		instancesList.Remove(this);
		P2PInterface p2PInterface = EOSPlatformManager.GetPlatformInterface().GetP2PInterface();
		if ((Handle)(object)p2PInterface != (Handle)null)
		{
			p2PInterface.CloseConnection(new CloseConnectionOptions
			{
				LocalUserId = LocalUserID,
				RemoteUserId = RemoteUserID,
				SocketId = NetworkManagerSystemEOS.socketId
			});
		}
		((NetworkConnection)this).Dispose(disposing);
	}

	public static EOSNetworkConnection Find(ProductUserId owner, ProductUserId endpoint)
	{
		return instancesList.Where((EOSNetworkConnection instance) => (Handle)(object)instance.RemoteUserID == (Handle)(object)endpoint).FirstOrDefault((EOSNetworkConnection instance) => owner == instance.LocalUserID);
	}
}
