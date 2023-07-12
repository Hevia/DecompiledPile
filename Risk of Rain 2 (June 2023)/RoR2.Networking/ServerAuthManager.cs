using System;
using HG;
using JetBrains.Annotations;
using UnityEngine;
using UnityEngine.Networking;

namespace RoR2.Networking;

public static class ServerAuthManager
{
	public struct KeyValue
	{
		public readonly NetworkConnection conn;

		public readonly ClientAuthData authData;

		public KeyValue(NetworkConnection conn, ClientAuthData authData)
		{
			this.conn = conn;
			this.authData = authData;
		}
	}

	private static readonly int initialSize = 16;

	public static KeyValue[] instances = new KeyValue[initialSize];

	private static int instanceCount = 0;

	public static event Action<NetworkConnection, ClientAuthData> onAuthDataReceivedFromClient;

	public static event Action<NetworkConnection, ClientAuthData> onAuthExpired;

	[RuntimeInitializeOnLoadMethod]
	private static void Init()
	{
		NetworkManagerSystem.onServerConnectGlobal += OnConnectionDiscovered;
		NetworkManagerSystem.onServerDisconnectGlobal += OnConnectionLost;
	}

	private static void OnConnectionDiscovered(NetworkConnection conn)
	{
	}

	private static void OnConnectionLost(NetworkConnection conn)
	{
		for (int i = 0; i < instanceCount; i++)
		{
			if (instances[i].conn == conn)
			{
				ServerAuthManager.onAuthExpired?.Invoke(conn, instances[i].authData);
				ArrayUtils.ArrayRemoveAt<KeyValue>(instances, ref instanceCount, i, 1);
				break;
			}
		}
	}

	public static ClientAuthData FindAuthData(NetworkConnection conn)
	{
		for (int i = 0; i < instanceCount; i++)
		{
			if (instances[i].conn == conn)
			{
				return instances[i].authData;
			}
		}
		return null;
	}

	public static NetworkConnection FindConnectionForSteamID(CSteamID steamId)
	{
		for (int i = 0; i < instanceCount; i++)
		{
			if (instances[i].authData.steamId == steamId)
			{
				return instances[i].conn;
			}
		}
		return null;
	}

	[NetworkMessageHandler(client = false, server = true, msgType = 74)]
	private static void HandleSetClientAuth(NetworkMessage netMsg)
	{
		if (netMsg.conn == null)
		{
			Debug.LogWarning((object)"ServerAuthManager.HandleSetClientAuth(): Connection is null.");
		}
		else
		{
			if (FindAuthData(netMsg.conn) != null)
			{
				return;
			}
			bool flag = Util.ConnectionIsLocal(netMsg.conn);
			NetworkManagerSystem.BaseKickReason baseKickReason = null;
			try
			{
				ClientAuthData clientAuthData = netMsg.ReadMessage<ClientAuthData>();
				NetworkConnection val = FindConnectionForSteamID(clientAuthData.steamId);
				if (val != null)
				{
					Debug.LogFormat("SteamID {0} is already claimed by connection [{1}]. Connection [{2}] rejected.", new object[3] { clientAuthData.steamId, val, netMsg.conn });
					PlatformSystems.networkManager.ServerKickClient(netMsg.conn, new NetworkManagerSystem.SimpleLocalizedKickReason("KICK_REASON_ACCOUNT_ALREADY_ON_SERVER"));
					return;
				}
				KeyValue keyValue = new KeyValue(netMsg.conn, clientAuthData);
				ArrayUtils.ArrayAppend<KeyValue>(ref instances, ref instanceCount, ref keyValue);
				string value = NetworkManagerSystem.SvPasswordConVar.instance.value;
				if (!flag && value.Length != 0 && !(clientAuthData.password == value))
				{
					Debug.LogFormat("Rejecting connection from [{0}]: {1}", new object[2] { netMsg.conn, "Bad password." });
					baseKickReason = new NetworkManagerSystem.SimpleLocalizedKickReason("KICK_REASON_BADPASSWORD");
				}
				string version = clientAuthData.version;
				string buildId = RoR2Application.GetBuildId();
				if (!string.Equals(version, buildId, StringComparison.OrdinalIgnoreCase))
				{
					Debug.LogFormat("Rejecting connection from [{0}]: {1}", new object[2] { netMsg.conn, "Bad version." });
					baseKickReason = new NetworkManagerSystem.SimpleLocalizedKickReason("KICK_REASON_BADVERSION", version, buildId);
				}
				string modHash = clientAuthData.modHash;
				string networkModHash = NetworkModCompatibilityHelper.networkModHash;
				if (!string.Equals(modHash, networkModHash, StringComparison.OrdinalIgnoreCase))
				{
					Debug.LogFormat("Rejecting connection from [{0}]: {1}", new object[2] { netMsg.conn, "Mod mismatch." });
					baseKickReason = new NetworkManagerSystem.ModMismatchKickReason(NetworkModCompatibilityHelper.networkModList);
				}
				ServerAuthManager.onAuthDataReceivedFromClient?.Invoke(keyValue.conn, keyValue.authData);
			}
			catch
			{
				Debug.LogFormat("Rejecting connection from [{0}]: {1}", new object[2] { netMsg.conn, "Malformed auth data." });
				baseKickReason = new NetworkManagerSystem.SimpleLocalizedKickReason("KICK_REASON_MALFORMED_AUTH_DATA");
			}
			if (baseKickReason != null)
			{
				PlatformSystems.networkManager.ServerKickClient(netMsg.conn, baseKickReason);
			}
		}
	}

	[CanBeNull]
	public static ClientAuthData GetClientAuthData(NetworkConnection networkConnection)
	{
		for (int i = 0; i < instances.Length; i++)
		{
			if (instances[i].conn == networkConnection)
			{
				return instances[i].authData;
			}
		}
		return null;
	}
}
