using System.Collections.Generic;
using Facepunch.Steamworks;
using JetBrains.Annotations;
using RoR2.ConVar;
using UnityEngine;
using UnityEngine.Networking;

namespace RoR2.Networking;

public class SteamNetworkConnection : NetworkConnection
{
	private BaseSteamworks steamworks;

	private Networking networking;

	private static List<SteamNetworkConnection> instancesList = new List<SteamNetworkConnection>();

	public bool ignore;

	public uint rtt;

	public static BoolConVar cvNetP2PDebugTransport = new BoolConVar("net_p2p_debug_transport", ConVarFlags.None, "0", "Allows p2p transport information to print to the console.");

	private static BoolConVar cvNetP2PLogMessages = new BoolConVar("net_p2p_log_messages", ConVarFlags.None, "0", "Enables logging of network messages.");

	public CSteamID myId { get; private set; }

	public CSteamID steamId { get; private set; }

	public SteamNetworkConnection()
	{
	}

	public SteamNetworkConnection([NotNull] BaseSteamworks steamworks, CSteamID endpointId)
	{
		BaseSteamworks obj = ((steamworks is Client) ? steamworks : null);
		long value;
		if (obj == null)
		{
			BaseSteamworks obj2 = ((steamworks is Server) ? steamworks : null);
			value = (long)((obj2 != null) ? ((Server)obj2).SteamId : 0);
		}
		else
		{
			value = (long)((Client)obj).SteamId;
		}
		myId = new CSteamID((ulong)value);
		steamId = endpointId;
		this.steamworks = steamworks;
		networking = steamworks.Networking;
		networking.CloseSession(endpointId.steamValue);
		instancesList.Add(this);
	}

	public bool SendConnectionRequest()
	{
		return networking.SendP2PPacket(steamId.steamValue, (byte[])null, 0, (SendType)2, 0);
	}

	public override bool TransportSend(byte[] bytes, int numBytes, int channelId, out byte error)
	{
		//IL_006f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0085: Unknown result type (might be due to invalid IL or missing references)
		//IL_008a: Unknown result type (might be due to invalid IL or missing references)
		//IL_008b: Unknown result type (might be due to invalid IL or missing references)
		//IL_008e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0090: Invalid comparison between Unknown and I4
		//IL_0092: Unknown result type (might be due to invalid IL or missing references)
		//IL_0094: Invalid comparison between Unknown and I4
		//IL_0097: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ab: Unknown result type (might be due to invalid IL or missing references)
		if (ignore)
		{
			error = 0;
			return true;
		}
		base.logNetworkMessages = cvNetP2PLogMessages.value;
		if (steamId == myId)
		{
			((NetworkConnection)this).TransportReceive(bytes, numBytes, channelId);
			error = 0;
			if (cvNetP2PDebugTransport.value)
			{
				Debug.LogFormat("SteamNetworkConnection.TransportSend steamId=self numBytes={1} channelId={2}", new object[2] { numBytes, channelId });
			}
			return true;
		}
		SendType val = (SendType)2;
		QosType qOS = ((NetworkManager)PlatformSystems.networkManager).connectionConfig.Channels[channelId].QOS;
		if ((int)qOS == 0 || (int)qOS == 1 || (int)qOS == 2)
		{
			val = (SendType)0;
		}
		if (networking.SendP2PPacket(steamId.steamValue, bytes, numBytes, val, 0))
		{
			error = 0;
			if (cvNetP2PDebugTransport.value)
			{
				Debug.LogFormat("SteamNetworkConnection.TransportSend steamId={0} numBytes={1} channelId={2} error={3}", new object[4] { steamId.value, numBytes, channelId, error });
			}
			return true;
		}
		error = 1;
		if (cvNetP2PDebugTransport.value)
		{
			Debug.LogFormat("SteamNetworkConnection.TransportSend steamId={0} numBytes={1} channelId={2} error={3}", new object[4] { steamId.value, numBytes, channelId, error });
		}
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
		instancesList.Remove(this);
		if (networking != null && steamId.steamValue != 0L)
		{
			networking.CloseSession(steamId.steamValue);
			steamId = CSteamID.nil;
		}
		((NetworkConnection)this).Dispose(disposing);
	}

	public static SteamNetworkConnection Find(BaseSteamworks owner, CSteamID endpoint)
	{
		for (int i = 0; i < instancesList.Count; i++)
		{
			SteamNetworkConnection steamNetworkConnection = instancesList[i];
			if (steamNetworkConnection.steamId == endpoint && owner == steamNetworkConnection.steamworks)
			{
				return steamNetworkConnection;
			}
		}
		return null;
	}
}
