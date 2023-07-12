using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using Facepunch.Steamworks;
using RoR2.ConVar;
using UnityEngine;
using UnityEngine.Networking;

namespace RoR2.Networking;

public class NetworkManagerSystemSteam : NetworkManagerSystem
{
	private BaseSteamworks clientSteamworks;

	private BaseSteamworks serverSteamworks;

	protected List<CSteamID> steamIdBanList = new List<CSteamID>();

	private static readonly BoolConVar cvSteamP2PUseSteamServer = new BoolConVar("steam_p2p_use_steam_server", ConVarFlags.None, "0", "Whether or not to use the Steam server interface to receive network traffic. Setting to false will cause the traffic to be handled by the Steam client interface instead. Only takes effect on server startup.");

	protected override void Start()
	{
		//IL_0010: Unknown result type (might be due to invalid IL or missing references)
		//IL_0015: Unknown result type (might be due to invalid IL or missing references)
		//IL_001c: Unknown result type (might be due to invalid IL or missing references)
		foreach (QosType channel in ((NetworkManager)this).channels)
		{
			((NetworkManager)this).connectionConfig.AddChannel(channel);
		}
		((NetworkManager)this).connectionConfig.PacketSize = 1200;
		FireOnStartGlobalEvent();
	}

	protected override void Update()
	{
		UpdateTime(ref _unpredictedServerFrameTime, ref _unpredictedServerFrameTimeSmoothed, ref unpredictedServerFrameTimeVelocity, Time.deltaTime);
		EnsureDesiredHost();
		UpdateServer();
		UpdateClient();
	}

	protected override void EnsureDesiredHost()
	{
		if ((false | serverShuttingDown | base.clientIsConnecting | (NetworkServer.active && NetworkManagerSystem.isLoadingScene) | (!NetworkClient.active && NetworkManagerSystem.isLoadingScene)) || !SystemInitializerAttribute.hasExecuted)
		{
			return;
		}
		bool isAnyUserSignedIn = LocalUserManager.isAnyUserSignedIn;
		if (base.desiredHost.isRemote && !isAnyUserSignedIn)
		{
			return;
		}
		if (((NetworkManager)this).isNetworkActive && !actedUponDesiredHost && !base.desiredHost.DescribesCurrentHost())
		{
			Disconnect();
		}
		else
		{
			if (actedUponDesiredHost)
			{
				return;
			}
			if (base.desiredHost.hostType == HostDescription.HostType.Self)
			{
				if (NetworkServer.active)
				{
					return;
				}
				actedUponDesiredHost = true;
				((NetworkManager)this).maxConnections = base.desiredHost.hostingParameters.maxPlayers;
				NetworkServer.dontListen = !base.desiredHost.hostingParameters.listen;
				if (!isAnyUserSignedIn)
				{
					((NetworkManager)this).StartServer(((NetworkManager)this).connectionConfig, 4);
				}
				else
				{
					((NetworkManager)this).StartHost(((NetworkManager)this).connectionConfig, 4);
				}
			}
			if (base.desiredHost.hostType == HostDescription.HostType.Steam && Time.unscaledTime - lastDesiredHostSetTime >= 0f)
			{
				actedUponDesiredHost = true;
				StartClient(base.desiredHost.userID);
			}
			if (base.desiredHost.hostType == HostDescription.HostType.IPv4 && Time.unscaledTime - lastDesiredHostSetTime >= 0f)
			{
				actedUponDesiredHost = true;
				Debug.LogFormat("Attempting connection. ip={0} port={1}", new object[2]
				{
					base.desiredHost.addressPortPair.address,
					base.desiredHost.addressPortPair.port
				});
				((NetworkManager)NetworkManagerSystem.singleton).networkAddress = base.desiredHost.addressPortPair.address;
				((NetworkManager)NetworkManagerSystem.singleton).networkPort = base.desiredHost.addressPortPair.port;
				((NetworkManager)NetworkManagerSystem.singleton).StartClient();
			}
		}
	}

	public override void ForceCloseAllConnections()
	{
		Client instance = Client.Instance;
		Networking val = ((instance != null) ? ((BaseSteamworks)instance).Networking : null);
		if (val == null)
		{
			return;
		}
		foreach (NetworkConnection connection in NetworkServer.connections)
		{
			if (connection is SteamNetworkConnection steamNetworkConnection)
			{
				val.CloseSession(steamNetworkConnection.steamId.steamValue);
			}
		}
		NetworkClient client = ((NetworkManager)this).client;
		if (((client != null) ? client.connection : null) is SteamNetworkConnection steamNetworkConnection2)
		{
			val.CloseSession(steamNetworkConnection2.steamId.steamValue);
		}
	}

	public override NetworkConnection GetClient(UserID clientID)
	{
		if (!NetworkServer.active)
		{
			Debug.Log((object)"Server is not active.");
			return null;
		}
		if (clientID.CID.steamValue == Client.Instance.SteamId && NetworkServer.connections.Count > 0)
		{
			return NetworkServer.connections[0];
		}
		foreach (NetworkConnection connection in NetworkServer.connections)
		{
			if (connection is SteamNetworkConnection steamNetworkConnection)
			{
				if (steamNetworkConnection.steamId.steamValue == clientID.CID.steamValue)
				{
					return (NetworkConnection)(object)steamNetworkConnection;
				}
			}
			else
			{
				Debug.Log((object)$"Skipping connection ({((object)connection).GetType()})");
			}
		}
		Debug.LogError((object)"Client not found");
		return null;
	}

	public override void OnServerConnect(NetworkConnection conn)
	{
		base.OnServerConnect(conn);
		if (NetworkUser.readOnlyInstancesList.Count >= ((NetworkManager)this).maxConnections)
		{
			ServerKickClient(conn, new SimpleLocalizedKickReason("KICK_REASON_SERVERFULL"));
		}
		else
		{
			FireServerConnectGlobalEvent(conn);
		}
	}

	public override void OnServerDisconnect(NetworkConnection conn)
	{
		//IL_0024: Unknown result type (might be due to invalid IL or missing references)
		FireServerDisconnectGlobalEvent(conn);
		if (conn.clientOwnedObjects != null)
		{
			foreach (NetworkInstanceId item in new HashSet<NetworkInstanceId>(conn.clientOwnedObjects))
			{
				GameObject val = NetworkServer.FindLocalObject(item);
				if ((Object)(object)val != (Object)null && Object.op_Implicit((Object)(object)val.GetComponent<CharacterMaster>()))
				{
					NetworkIdentity component = val.GetComponent<NetworkIdentity>();
					if (Object.op_Implicit((Object)(object)component) && component.clientAuthorityOwner == conn)
					{
						component.RemoveClientAuthority(conn);
					}
				}
			}
		}
		List<PlayerController> playerControllers = conn.playerControllers;
		for (int i = 0; i < playerControllers.Count; i++)
		{
			NetworkUser component2 = playerControllers[i].gameObject.GetComponent<NetworkUser>();
			if (Object.op_Implicit((Object)(object)component2))
			{
				Chat.SendPlayerDisconnectedMessage(component2);
			}
		}
		if (conn is SteamNetworkConnection)
		{
			Debug.LogFormat("Closing connection with steamId {0}", new object[1] { ((SteamNetworkConnection)(object)conn).steamId.steamValue });
		}
		base.OnServerDisconnect(conn);
	}

	public override void InitPlatformServer()
	{
		ServerManagerBase<SteamworksServerManager>.StartServer();
		InitP2P();
	}

	public override void OnStopServer()
	{
		FireStopServerGlobalEvent();
		for (int i = 0; i < NetworkServer.connections.Count; i++)
		{
			NetworkConnection val = NetworkServer.connections[i];
			if (val != null)
			{
				((NetworkManager)this).OnServerDisconnect(val);
			}
		}
		Object.Destroy((Object)(object)serverNetworkSessionInstance);
		serverNetworkSessionInstance = null;
		ServerManagerBase<SteamworksServerManager>.StopServer();
		base.OnStopServer();
	}

	public override void ServerBanClient(NetworkConnection conn)
	{
		if (conn is SteamNetworkConnection steamNetworkConnection)
		{
			steamIdBanList.Add(steamNetworkConnection.steamId);
		}
	}

	protected override NetworkUserId AddPlayerIdFromPlatform(NetworkConnection conn, AddPlayerMessage message, byte playerControllerId)
	{
		NetworkUserId result = NetworkUserId.FromIp(conn.address, playerControllerId);
		CSteamID cSteamID = ServerAuthManager.FindAuthData(conn)?.steamId ?? CSteamID.nil;
		if (cSteamID != CSteamID.nil)
		{
			result = NetworkUserId.FromId(cSteamID.steamValue, playerControllerId);
		}
		return result;
	}

	protected override void KickClient(NetworkConnection conn, BaseKickReason reason)
	{
		if (conn is SteamNetworkConnection steamNetworkConnection)
		{
			steamNetworkConnection.ignore = true;
		}
	}

	public override void ServerHandleClientDisconnect(NetworkConnection conn)
	{
		((NetworkManager)this).OnServerDisconnect(conn);
		conn.InvokeHandlerNoData((short)33);
		conn.Disconnect();
		conn.Dispose();
		if (conn is SteamNetworkConnection)
		{
			NetworkServer.RemoveExternalConnection(conn.connectionId);
		}
	}

	protected override void UpdateServer()
	{
		//IL_002a: Unknown result type (might be due to invalid IL or missing references)
		//IL_004e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0056: Unknown result type (might be due to invalid IL or missing references)
		if (!NetworkServer.active)
		{
			return;
		}
		ReadOnlyCollection<NetworkConnection> connections = NetworkServer.connections;
		for (int num = connections.Count - 1; num >= 0; num--)
		{
			if (connections[num] is SteamNetworkConnection steamNetworkConnection)
			{
				P2PSessionState val = default(P2PSessionState);
				if (((BaseSteamworks)Client.Instance).Networking.GetP2PSessionState(steamNetworkConnection.steamId.steamValue, ref val) && val.Connecting == 0 && val.ConnectionActive == 0)
				{
					ServerHandleClientDisconnect((NetworkConnection)(object)steamNetworkConnection);
				}
			}
		}
	}

	public override void OnStartClient(NetworkClient newClient)
	{
		base.OnStartClient(newClient);
		InitP2P();
	}

	public override void OnClientConnect(NetworkConnection conn)
	{
		base.OnClientConnect(conn);
	}

	public override void OnClientDisconnect(NetworkConnection conn)
	{
		if (conn is SteamNetworkConnection steamNetworkConnection)
		{
			Debug.LogFormat("Closing connection with steamId {0}", new object[1] { steamNetworkConnection.steamId });
		}
		base.OnClientDisconnect(conn);
	}

	protected override AddPlayerMessage CreateClientAddPlayerMessage()
	{
		if (Client.Instance != null)
		{
			return new AddPlayerMessage
			{
				id = new UserID(Client.Instance.SteamId),
				steamAuthTicketData = Client.Instance.Auth.GetAuthSessionTicket().Data
			};
		}
		return new AddPlayerMessage
		{
			id = default(UserID),
			steamAuthTicketData = Array.Empty<byte>()
		};
	}

	protected override void UpdateCheckInactiveConnections()
	{
		//IL_001b: Unknown result type (might be due to invalid IL or missing references)
		//IL_004e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0056: Unknown result type (might be due to invalid IL or missing references)
		NetworkClient client = ((NetworkManager)this).client;
		if (((client != null) ? client.connection : null) is SteamNetworkConnection)
		{
			P2PSessionState val = default(P2PSessionState);
			if (((BaseSteamworks)Client.Instance).Networking.GetP2PSessionState(((SteamNetworkConnection)(object)((NetworkManager)this).client.connection).steamId.steamValue, ref val) && val.Connecting == 0 && val.ConnectionActive == 0)
			{
				((NetworkManager)this).client.connection.InvokeHandlerNoData((short)33);
				((NetworkManager)this).StopClient();
			}
		}
	}

	protected override void PlatformAuth(ref ClientAuthData authData, NetworkConnection conn)
	{
		authData.steamId = new CSteamID(Client.Instance.SteamId);
		authData.authTicket = Client.Instance.Auth.GetAuthSessionTicket().Data;
	}

	protected override void StartClient(UserID serverID)
	{
		if (!NetworkServer.active)
		{
			NetworkManager.networkSceneName = "";
		}
		string text = "";
		if (((NetworkManager)this).isNetworkActive)
		{
			text += "isNetworkActive ";
		}
		if (NetworkClient.active)
		{
			text += "NetworkClient.active ";
		}
		if (NetworkServer.active)
		{
			text += "NetworkClient.active ";
		}
		if (NetworkManagerSystem.isLoadingScene)
		{
			text += "isLoadingScene ";
		}
		if (text != "")
		{
			Debug.Log((object)text);
			RoR2Application.onNextUpdate += delegate
			{
			};
		}
		SteamNetworkConnection steamNetworkConnection = new SteamNetworkConnection((BaseSteamworks)(object)Client.Instance, serverID.CID);
		SteamNetworkClient steamNetworkClient = new SteamNetworkClient((NetworkConnection)(object)steamNetworkConnection);
		((NetworkClient)steamNetworkClient).Configure(((NetworkManager)this).connectionConfig, 1);
		((NetworkManager)this).UseExternalClient((NetworkClient)(object)steamNetworkClient);
		steamNetworkClient.Connect();
		Debug.LogFormat("Initiating connection to server {0}...", new object[1] { serverID.CID.steamValue });
		if (!steamNetworkConnection.SendConnectionRequest())
		{
			Debug.LogFormat("Failed to send connection request to server {0}.", new object[1] { serverID.CID.steamValue });
		}
	}

	public override bool IsConnectedToServer(UserID serverID)
	{
		if (((NetworkManager)this).client == null || !((NetworkManager)this).client.connection.isConnected || Client.Instance == null)
		{
			return false;
		}
		if (((NetworkManager)this).client.connection is SteamNetworkConnection steamNetworkConnection)
		{
			return steamNetworkConnection.steamId == serverID.CID;
		}
		if (((NetworkManager)this).client.connection.address == "localServer")
		{
			return serverID.CID == base.serverP2PId;
		}
		return false;
	}

	public static bool IsMemberInSteamLobby(CSteamID steamId)
	{
		return Client.Instance.Lobby.UserIsInCurrentLobby(steamId.steamValue);
	}

	private static CSteamID GetSteamworksSteamId(BaseSteamworks steamworks)
	{
		Client val;
		if ((val = (Client)(object)((steamworks is Client) ? steamworks : null)) != null)
		{
			return new CSteamID(val.SteamId);
		}
		Server val2;
		if ((val2 = (Server)(object)((steamworks is Server) ? steamworks : null)) != null)
		{
			return new CSteamID(val2.SteamId);
		}
		return CSteamID.nil;
	}

	protected void InitP2P()
	{
		//IL_0053: Unknown result type (might be due to invalid IL or missing references)
		//IL_005d: Expected O, but got Unknown
		//IL_00aa: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b4: Expected O, but got Unknown
		Server instance = Server.Instance;
		Networking val = ((instance != null) ? ((BaseSteamworks)instance).Networking : null);
		Client instance2 = Client.Instance;
		Networking val2 = ((instance2 != null) ? ((BaseSteamworks)instance2).Networking : null);
		if (val != null)
		{
			val.OnIncomingConnection = OnSteamServerP2PIncomingConnection;
			val.OnConnectionFailed = OnSteamServerP2PConnectionFailed;
			val.OnP2PData = new OnRecievedP2PData(OnSteamServerP2PData);
			for (int i = 0; i < ((NetworkManager)this).connectionConfig.ChannelCount; i++)
			{
				val.SetListenChannel(i, true);
			}
		}
		if (val2 != null)
		{
			val2.OnIncomingConnection = OnSteamClientP2PIncomingConnection;
			val2.OnConnectionFailed = OnSteamClientP2PConnectionFailed;
			val2.OnP2PData = new OnRecievedP2PData(OnSteamClientP2PData);
			for (int j = 0; j < ((NetworkManager)this).connectionConfig.ChannelCount; j++)
			{
				val2.SetListenChannel(j, true);
			}
		}
		clientSteamworks = (BaseSteamworks)(object)Client.Instance;
		serverSteamworks = null;
		if (NetworkServer.active)
		{
			serverSteamworks = (BaseSteamworks)(((object)(cvSteamP2PUseSteamServer.value ? Server.Instance : null)) ?? ((object)Client.Instance));
		}
		base.clientP2PId = GetSteamworksSteamId(clientSteamworks);
		base.serverP2PId = GetSteamworksSteamId(serverSteamworks);
	}

	private bool OnSteamServerP2PIncomingConnection(ulong senderSteamId)
	{
		return OnIncomingP2PConnection((BaseSteamworks)(object)Server.Instance, new CSteamID(senderSteamId));
	}

	private void OnSteamServerP2PConnectionFailed(ulong senderSteamId, SessionError sessionError)
	{
		//IL_000c: Unknown result type (might be due to invalid IL or missing references)
		OnP2PConnectionFailed((BaseSteamworks)(object)Server.Instance, new CSteamID(senderSteamId), sessionError);
	}

	private void OnSteamServerP2PData(ulong senderSteamId, byte[] data, int dataLength, int channel)
	{
		OnP2PData((BaseSteamworks)(object)Server.Instance, new CSteamID(senderSteamId), data, dataLength, channel);
	}

	private bool OnSteamClientP2PIncomingConnection(ulong senderSteamId)
	{
		return OnIncomingP2PConnection((BaseSteamworks)(object)Client.Instance, new CSteamID(senderSteamId));
	}

	private void OnSteamClientP2PConnectionFailed(ulong senderSteamId, SessionError sessionError)
	{
		//IL_000c: Unknown result type (might be due to invalid IL or missing references)
		OnP2PConnectionFailed((BaseSteamworks)(object)Client.Instance, new CSteamID(senderSteamId), sessionError);
	}

	private void OnSteamClientP2PData(ulong senderSteamId, byte[] data, int dataLength, int channel)
	{
		OnP2PData((BaseSteamworks)(object)Client.Instance, new CSteamID(senderSteamId), data, dataLength, channel);
	}

	private bool OnIncomingP2PConnection(BaseSteamworks receiver, CSteamID senderSteamId)
	{
		bool flag = false;
		if (receiver == serverSteamworks)
		{
			if (NetworkServer.active)
			{
				flag = !NetworkServer.dontListen && !steamIdBanList.Contains(senderSteamId) && !IsServerAtMaxConnections();
			}
			else if (((NetworkManager)this).client is SteamNetworkClient && ((SteamNetworkClient)(object)((NetworkManager)this).client).steamConnection.steamId == senderSteamId)
			{
				flag = true;
			}
		}
		Debug.LogFormat("Incoming Steamworks connection from Steam ID {0}: {1}", new object[2]
		{
			senderSteamId,
			flag ? "accepted" : "rejected"
		});
		if (flag)
		{
			CreateServerP2PConnectionWithPeer(receiver, senderSteamId);
		}
		return flag;
	}

	private void OnP2PConnectionFailed(BaseSteamworks receiver, CSteamID senderSteamId, SessionError sessionError)
	{
		//IL_0016: Unknown result type (might be due to invalid IL or missing references)
		Debug.LogFormat("NetworkManagerSystem.OnClientP2PConnectionFailed steamId={0} sessionError={1}", new object[2] { senderSteamId, sessionError });
		SteamNetworkConnection steamNetworkConnection = SteamNetworkConnection.Find(receiver, senderSteamId);
		if (steamNetworkConnection != null)
		{
			if (((NetworkManager)this).client != null && ((NetworkManager)this).client.connection == steamNetworkConnection)
			{
				((NetworkConnection)steamNetworkConnection).InvokeHandlerNoData((short)33);
				((NetworkConnection)steamNetworkConnection).Disconnect();
				((NetworkConnection)steamNetworkConnection).Dispose();
			}
			if (NetworkServer.active && NetworkServer.connections.IndexOf((NetworkConnection)(object)steamNetworkConnection) != -1)
			{
				ServerHandleClientDisconnect((NetworkConnection)(object)steamNetworkConnection);
			}
		}
	}

	private void OnP2PData(BaseSteamworks receiver, CSteamID senderSteamId, byte[] data, int dataLength, int channel)
	{
		if (SteamNetworkConnection.cvNetP2PDebugTransport.value)
		{
			Debug.LogFormat("Received packet from {0} dataLength={1} channel={2}", new object[3] { senderSteamId, dataLength, channel });
		}
		SteamNetworkConnection steamNetworkConnection = SteamNetworkConnection.Find(receiver, senderSteamId);
		if (steamNetworkConnection != null)
		{
			((NetworkConnection)steamNetworkConnection).TransportReceive(data, dataLength, 0);
			return;
		}
		Debug.LogFormat("Rejecting data from sender: Not associated with a registered connection. steamid={0} dataLength={1}", new object[2] { senderSteamId, data });
	}

	public void CreateServerP2PConnectionWithPeer(BaseSteamworks steamworks, CSteamID peer)
	{
		//IL_0058: Unknown result type (might be due to invalid IL or missing references)
		//IL_005e: Expected O, but got Unknown
		SteamNetworkConnection steamNetworkConnection = new SteamNetworkConnection(steamworks, peer);
		((NetworkConnection)(object)steamNetworkConnection).ForceInitialize(NetworkServer.hostTopology);
		int num = -1;
		ReadOnlyCollection<NetworkConnection> connections = NetworkServer.connections;
		for (int i = 1; i < connections.Count; i++)
		{
			if (connections[i] == null)
			{
				num = i;
				break;
			}
		}
		if (num == -1)
		{
			num = connections.Count;
		}
		((NetworkConnection)steamNetworkConnection).connectionId = num;
		NetworkServer.AddExternalConnection((NetworkConnection)(object)steamNetworkConnection);
		NetworkWriter val = new NetworkWriter();
		val.StartMessage((short)32);
		val.FinishMessage();
		((NetworkConnection)steamNetworkConnection).SendWriter(val, QosChannelIndex.defaultReliable.intVal);
	}

	protected override void PlatformClientSetPlayers(ConCommandArgs args)
	{
		if (((NetworkManager)this).client != null && ((NetworkManager)this).client.connection != null)
		{
			ClientSetPlayers(((NetworkManager)this).client.connection);
		}
	}

	protected override void PlatformConnectP2P(ConCommandArgs args)
	{
		CheckSteamworks();
		UserID userID = default(UserID);
		userID.CID = args.GetArgSteamID(0);
		if (Client.Instance.Lobby.IsValid && !PlatformSystems.lobbyManager.ownsLobby && userID.CID != PlatformSystems.lobbyManager.newestLobbyData.serverId)
		{
			Debug.LogFormat("Cannot connect to server {0}: Server is not the one specified by the current steam lobby.", new object[1] { userID });
		}
		else if (!(base.clientP2PId == userID.CID))
		{
			NetworkManagerSystem.singleton.desiredHost = new HostDescription(userID, HostDescription.HostType.Steam);
		}
	}

	protected override void PlatformDisconnect(ConCommandArgs args)
	{
		NetworkManagerSystem.singleton.desiredHost = HostDescription.none;
	}

	protected override void PlatformConnect(ConCommandArgs args)
	{
		AddressPortPair argAddressPortPair = args.GetArgAddressPortPair(0);
		if (Object.op_Implicit((Object)(object)NetworkManagerSystem.singleton))
		{
			NetworkManagerSystem.EnsureNetworkManagerNotBusy();
			Debug.LogFormat("Parsed address={0} port={1}. Setting desired host.", new object[2] { argAddressPortPair.address, argAddressPortPair.port });
			NetworkManagerSystem.singleton.desiredHost = new HostDescription(argAddressPortPair);
		}
	}

	protected override void PlatformHost(ConCommandArgs args)
	{
		if (!Object.op_Implicit((Object)(object)NetworkManagerSystem.singleton))
		{
			return;
		}
		bool argBool = args.GetArgBool(0);
		if (PlatformSystems.lobbyManager.isInLobby && !PlatformSystems.lobbyManager.ownsLobby)
		{
			return;
		}
		bool flag = false;
		if (NetworkServer.active)
		{
			Debug.Log((object)"Server already running.");
			flag = true;
		}
		if (!flag)
		{
			int maxPlayers = SvMaxPlayersConVar.instance.intValue;
			if (PlatformSystems.lobbyManager.isInLobby)
			{
				maxPlayers = PlatformSystems.lobbyManager.newestLobbyData.totalMaxPlayers;
			}
			NetworkManagerSystem.singleton.desiredHost = new HostDescription(new HostDescription.HostingParameters
			{
				listen = argBool,
				maxPlayers = maxPlayers
			});
		}
	}

	protected override void PlatformGetP2PSessionState(ConCommandArgs args)
	{
		//IL_001d: Unknown result type (might be due to invalid IL or missing references)
		//IL_004c: Unknown result type (might be due to invalid IL or missing references)
		//IL_005a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0068: Unknown result type (might be due to invalid IL or missing references)
		//IL_0076: Unknown result type (might be due to invalid IL or missing references)
		//IL_0084: Unknown result type (might be due to invalid IL or missing references)
		//IL_0092: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a0: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ae: Unknown result type (might be due to invalid IL or missing references)
		CheckSteamworks();
		CSteamID argSteamID = args.GetArgSteamID(0);
		if (Object.op_Implicit((Object)(object)NetworkManagerSystem.singleton))
		{
			P2PSessionState val = default(P2PSessionState);
			if (((BaseSteamworks)Client.Instance).Networking.GetP2PSessionState(argSteamID.steamValue, ref val))
			{
				Debug.LogFormat("ConnectionActive={0}\nConnecting={1}\nP2PSessionError={2}\nUsingRelay={3}\nBytesQueuedForSend={4}\nPacketsQueuedForSend={5}\nRemoteIP={6}\nRemotePort={7}", new object[8] { val.ConnectionActive, val.Connecting, val.P2PSessionError, val.UsingRelay, val.BytesQueuedForSend, val.PacketsQueuedForSend, val.RemoteIP, val.RemotePort });
			}
			else
			{
				Debug.LogFormat("Could not get p2p session info for steamId={0}", new object[1] { argSteamID });
			}
		}
	}

	protected override void PlatformKick(ConCommandArgs args)
	{
		CheckSteamworks();
		UserID clientId = new UserID(args.GetArgSteamID(0));
		NetworkConnection client = NetworkManagerSystem.singleton.GetClient(clientId);
		if (client != null)
		{
			NetworkManagerSystem.singleton.ServerKickClient(client, new SimpleLocalizedKickReason("KICK_REASON_KICK"));
		}
	}

	protected override void PlatformBan(ConCommandArgs args)
	{
		CheckSteamworks();
		UserID clientId = new UserID(args.GetArgSteamID(0));
		NetworkConnection client = NetworkManagerSystem.singleton.GetClient(clientId);
		if (client != null)
		{
			NetworkManagerSystem.singleton.ServerBanClient(client);
			NetworkManagerSystem.singleton.ServerKickClient(client, new SimpleLocalizedKickReason("KICK_REASON_BAN"));
		}
	}

	public static void CheckSteamworks()
	{
		if (Client.Instance == null)
		{
			throw new ConCommandException("Steamworks not available.");
		}
	}

	public override void CreateLocalLobby()
	{
	}
}
