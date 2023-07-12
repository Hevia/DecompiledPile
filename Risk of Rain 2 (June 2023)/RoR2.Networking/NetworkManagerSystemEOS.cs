using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using Epic.OnlineServices;
using Epic.OnlineServices.Lobby;
using Epic.OnlineServices.P2P;
using Facepunch.Steamworks;
using UnityEngine;
using UnityEngine.Networking;

namespace RoR2.Networking;

public class NetworkManagerSystemEOS : NetworkManagerSystem
{
	public ProductUserId myUserId;

	private int packetsRecieved;

	private Dictionary<ulong, byte> sequenceNumber = new Dictionary<ulong, byte>();

	private Dictionary<ulong, List<Tuple<byte, byte[], uint>>> outOfOrderPackets = new Dictionary<ulong, List<Tuple<byte, byte[], uint>>>();

	private List<ProductUserId> epicIdBanList = new List<ProductUserId>();

	public static SocketId socketId = new SocketId
	{
		SocketName = "RoR2EOS"
	};

	public static P2PInterface P2pInterface { get; private set; }

	protected override void Start()
	{
		//IL_0017: Unknown result type (might be due to invalid IL or missing references)
		//IL_001c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0023: Unknown result type (might be due to invalid IL or missing references)
		myUserId = null;
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
		UpdateNetworkReceiveLoop();
	}

	public void ResetSequencing()
	{
		packetsRecieved = 0;
		sequenceNumber.Clear();
		outOfOrderPackets.Clear();
	}

	private void UpdateNetworkReceiveLoop()
	{
		//IL_002c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0031: Unknown result type (might be due to invalid IL or missing references)
		//IL_003c: Unknown result type (might be due to invalid IL or missing references)
		//IL_005a: Expected O, but got Unknown
		//IL_0055: Unknown result type (might be due to invalid IL or missing references)
		//IL_005a: Unknown result type (might be due to invalid IL or missing references)
		//IL_005d: Unknown result type (might be due to invalid IL or missing references)
		if ((Handle)(object)P2pInterface == (Handle)null)
		{
			return;
		}
		byte b = byte.MaxValue;
		packetsRecieved = 0;
		int num = 0;
		bool flag;
		ProductUserId incomingUserId = default(ProductUserId);
		SocketId val = default(SocketId);
		byte[] array = default(byte[]);
		do
		{
			num++;
			flag = false;
			Result result = P2pInterface.ReceivePacket(new ReceivePacketOptions
			{
				LocalUserId = EOSLoginManager.loggedInProductId,
				MaxDataSizeBytes = ((NetworkManager)this).connectionConfig.PacketSize
			}, ref incomingUserId, ref val, ref b, ref array);
			if (ProcessData(result, incomingUserId, array, array.Length, 0))
			{
				flag = true;
			}
		}
		while (flag);
	}

	private bool ProcessData(Result result, ProductUserId incomingUserId, byte[] buf, int dataSize, int channelId)
	{
		//IL_0000: Unknown result type (might be due to invalid IL or missing references)
		//IL_003f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0042: Invalid comparison between Unknown and I4
		//IL_0044: Unknown result type (might be due to invalid IL or missing references)
		//IL_0047: Invalid comparison between Unknown and I4
		if ((int)result == 0)
		{
			EOSNetworkConnection eOSNetworkConnection = EOSNetworkConnection.Find(myUserId, incomingUserId);
			if (eOSNetworkConnection != null)
			{
				((NetworkConnection)eOSNetworkConnection).TransportReceive(buf, dataSize, 0);
			}
			else
			{
				Debug.LogFormat("Rejecting data from sender: Not associated with a registered connection. id={0} dataLength={1}", new object[2] { incomingUserId, dataSize });
			}
			return true;
		}
		if ((int)result == 18 || (int)result == 10)
		{
			return false;
		}
		Debug.LogError((object)("P2PInterface ReceivePacket returned a failure: " + ((object)(Result)(ref result)).ToString()));
		return false;
	}

	protected override void EnsureDesiredHost()
	{
		if (false | serverShuttingDown | base.clientIsConnecting | (NetworkServer.active && NetworkManagerSystem.isLoadingScene) | (!NetworkClient.active && NetworkManagerSystem.isLoadingScene))
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
			int maxPlayers = RoR2Application.maxPlayers;
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
					((NetworkManager)this).StartServer(((NetworkManager)this).connectionConfig, maxPlayers);
				}
				else
				{
					((NetworkManager)this).StartHost(((NetworkManager)this).connectionConfig, maxPlayers);
				}
			}
			if (base.desiredHost.hostType == HostDescription.HostType.EOS && Time.unscaledTime - lastDesiredHostSetTime >= 0f)
			{
				actedUponDesiredHost = true;
				StartClient(base.desiredHost.userID.CID.egsValue);
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
				((NetworkManager)NetworkManagerSystem.singleton).StartClient(((NetworkManager)this).matchInfo, ((NetworkManager)this).connectionConfig);
			}
		}
	}

	public override void ForceCloseAllConnections()
	{
		//IL_0021: Unknown result type (might be due to invalid IL or missing references)
		//IL_0026: Unknown result type (might be due to invalid IL or missing references)
		//IL_0032: Unknown result type (might be due to invalid IL or missing references)
		//IL_003e: Unknown result type (might be due to invalid IL or missing references)
		//IL_004e: Expected O, but got Unknown
		//IL_0049: Unknown result type (might be due to invalid IL or missing references)
		//IL_0083: Unknown result type (might be due to invalid IL or missing references)
		//IL_0088: Unknown result type (might be due to invalid IL or missing references)
		//IL_0094: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a0: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b0: Expected O, but got Unknown
		//IL_00ab: Unknown result type (might be due to invalid IL or missing references)
		foreach (NetworkConnection connection in NetworkServer.connections)
		{
			if (connection is EOSNetworkConnection eOSNetworkConnection)
			{
				P2pInterface.CloseConnection(new CloseConnectionOptions
				{
					LocalUserId = eOSNetworkConnection.LocalUserID,
					RemoteUserId = eOSNetworkConnection.RemoteUserID,
					SocketId = socketId
				});
			}
		}
		NetworkClient client = ((NetworkManager)this).client;
		if (((client != null) ? client.connection : null) is EOSNetworkConnection eOSNetworkConnection2)
		{
			P2pInterface.CloseConnection(new CloseConnectionOptions
			{
				LocalUserId = eOSNetworkConnection2.LocalUserID,
				RemoteUserId = eOSNetworkConnection2.RemoteUserID,
				SocketId = socketId
			});
		}
		myUserId = null;
	}

	public override NetworkConnection GetClient(UserID clientID)
	{
		if (!NetworkServer.active)
		{
			return null;
		}
		if ((Handle)(object)clientID.CID.egsValue == (Handle)(object)myUserId && NetworkServer.connections.Count > 0)
		{
			return NetworkServer.connections[0];
		}
		foreach (NetworkConnection connection in NetworkServer.connections)
		{
			if (connection is EOSNetworkConnection eOSNetworkConnection && (Handle)(object)eOSNetworkConnection.RemoteUserID == (Handle)(object)clientID.CID.egsValue)
			{
				return (NetworkConnection)(object)eOSNetworkConnection;
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
		if (conn is EOSNetworkConnection eOSNetworkConnection)
		{
			Debug.Log((object)$"Closing connection with RemoteUserID: {eOSNetworkConnection.RemoteUserID}");
		}
		base.OnServerDisconnect(conn);
	}

	public override void OnStartServer()
	{
		base.OnStartServer();
		ServerManagerBase<EOSServerManager>.StartServer();
		InitP2P();
		NetworkMessageHandlerAttribute.RegisterServerMessages();
		InitializeTime();
		serverNetworkSessionInstance = Object.Instantiate<GameObject>(LegacyResourcesAPI.Load<GameObject>("Prefabs/NetworkSession"));
		FireStartServerGlobalEvent();
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
		ServerManagerBase<EOSServerManager>.StopServer();
		myUserId = null;
		base.OnStopServer();
	}

	public override void ServerBanClient(NetworkConnection conn)
	{
		if (conn is EOSNetworkConnection eOSNetworkConnection)
		{
			epicIdBanList.Add(eOSNetworkConnection.RemoteUserID);
		}
	}

	protected override NetworkUserId AddPlayerIdFromPlatform(NetworkConnection conn, AddPlayerMessage message, byte playerControllerId)
	{
		NetworkUserId result = NetworkUserId.FromIp(conn.address, playerControllerId);
		CSteamID? cSteamID = ServerAuthManager.FindAuthData(conn)?.steamId;
		if (cSteamID.HasValue)
		{
			if (cSteamID.Value.isSteam)
			{
				result = NetworkUserId.FromId(cSteamID.Value.steamValue, playerControllerId);
			}
			if (cSteamID.Value.isEGS)
			{
				result = new NetworkUserId(((object)cSteamID.Value.egsValue).ToString(), playerControllerId);
			}
		}
		return result;
	}

	protected override void KickClient(NetworkConnection conn, BaseKickReason reason)
	{
		if (conn is EOSNetworkConnection eOSNetworkConnection)
		{
			eOSNetworkConnection.ignore = true;
		}
	}

	public override void ServerHandleClientDisconnect(NetworkConnection conn)
	{
		((NetworkManager)this).OnServerDisconnect(conn);
		conn.InvokeHandlerNoData((short)33);
		conn.Disconnect();
		conn.Dispose();
		if (conn is EOSNetworkConnection)
		{
			NetworkServer.RemoveExternalConnection(conn.connectionId);
		}
	}

	protected override void UpdateServer()
	{
	}

	public override void OnStartClient(NetworkClient newClient)
	{
		base.OnStartClient(newClient);
		InitP2P();
	}

	public override void OnClientDisconnect(NetworkConnection conn)
	{
		if (conn is EOSNetworkConnection eOSNetworkConnection)
		{
			Debug.LogFormat("Closing connection with remote ID: {0}", new object[1] { eOSNetworkConnection.RemoteUserID });
		}
		base.OnClientDisconnect(conn);
	}

	protected override AddPlayerMessage CreateClientAddPlayerMessage()
	{
		if (((NetworkManager)this).client != null)
		{
			return new AddPlayerMessage
			{
				id = new UserID(new CSteamID(EOSLoginManager.loggedInProductId)),
				steamAuthTicketData = Array.Empty<byte>()
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
	}

	protected override void StartClient(UserID serverID)
	{
		StartClient(serverID.CID.egsValue);
	}

	protected override void PlatformAuth(ref ClientAuthData authData, NetworkConnection conn)
	{
		authData.steamId = new CSteamID(EOSLoginManager.loggedInProductId);
		authData.authTicket = Client.Instance.Auth.GetAuthSessionTicket().Data;
	}

	private void StartClient(ProductUserId remoteUserId)
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
		EOSNetworkConnection eOSNetworkConnection = new EOSNetworkConnection(EOSLoginManager.loggedInProductId, remoteUserId);
		EOSNetworkClient eOSNetworkClient = new EOSNetworkClient((NetworkConnection)(object)eOSNetworkConnection);
		((NetworkClient)eOSNetworkClient).Configure(((NetworkManager)this).connectionConfig, 1);
		((NetworkManager)this).UseExternalClient((NetworkClient)(object)eOSNetworkClient);
		eOSNetworkClient.Connect();
		Debug.LogFormat("Initiating connection to server {0}...", new object[1] { remoteUserId });
		if (!eOSNetworkConnection.SendConnectionRequest())
		{
			Debug.LogFormat("Failed to send connection request to server {0}.", new object[1] { remoteUserId });
		}
	}

	public override bool IsConnectedToServer(UserID serverID)
	{
		if (((NetworkManager)this).client == null || !((NetworkManager)this).client.connection.isConnected)
		{
			return false;
		}
		if (((NetworkManager)this).client.connection is EOSNetworkConnection eOSNetworkConnection)
		{
			return (Handle)(object)eOSNetworkConnection.RemoteUserID == (Handle)(object)serverID.CID.egsValue;
		}
		if (((NetworkManager)this).client.connection.address == "localServer")
		{
			return serverID.CID == base.serverP2PId;
		}
		return false;
	}

	private void OnConnectionRequested(OnIncomingConnectionRequestInfo connectionRequestInfo)
	{
		//IL_00a5: Unknown result type (might be due to invalid IL or missing references)
		//IL_00aa: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b6: Unknown result type (might be due to invalid IL or missing references)
		//IL_00c2: Unknown result type (might be due to invalid IL or missing references)
		//IL_00d2: Expected O, but got Unknown
		//IL_00cd: Unknown result type (might be due to invalid IL or missing references)
		bool flag = false;
		if (connectionRequestInfo.SocketId.SocketName == socketId.SocketName)
		{
			if (NetworkServer.active)
			{
				flag = !NetworkServer.dontListen && !epicIdBanList.Contains(connectionRequestInfo.RemoteUserId) && !IsServerAtMaxConnections();
			}
			else if (((NetworkManager)this).client is EOSNetworkClient eOSNetworkClient && (Handle)(object)eOSNetworkClient.eosConnection.RemoteUserID == (Handle)(object)connectionRequestInfo.RemoteUserId)
			{
				flag = true;
			}
		}
		string arg = (flag ? "accepted" : "rejected");
		Debug.Log((object)$"Incoming connection from Product User ID {connectionRequestInfo.RemoteUserId}: {arg}");
		if (flag)
		{
			P2pInterface.AcceptConnection(new AcceptConnectionOptions
			{
				LocalUserId = connectionRequestInfo.LocalUserId,
				RemoteUserId = connectionRequestInfo.RemoteUserId,
				SocketId = socketId
			});
			CreateServerP2PConnectionWithPeer(connectionRequestInfo.RemoteUserId);
		}
	}

	public void CreateServerP2PConnectionWithPeer(ProductUserId peer)
	{
		//IL_005d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0063: Expected O, but got Unknown
		EOSNetworkConnection eOSNetworkConnection = new EOSNetworkConnection(myUserId, peer);
		((NetworkConnection)(object)eOSNetworkConnection).ForceInitialize(NetworkServer.hostTopology);
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
		((NetworkConnection)eOSNetworkConnection).connectionId = num;
		NetworkServer.AddExternalConnection((NetworkConnection)(object)eOSNetworkConnection);
		NetworkWriter val = new NetworkWriter();
		val.StartMessage((short)32);
		val.FinishMessage();
		((NetworkConnection)eOSNetworkConnection).SendWriter(val, QosChannelIndex.defaultReliable.intVal);
	}

	protected void InitP2P()
	{
		//IL_001c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0021: Unknown result type (might be due to invalid IL or missing references)
		//IL_002c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0038: Expected O, but got Unknown
		//IL_003f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0045: Expected O, but got Unknown
		//IL_0053: Unknown result type (might be due to invalid IL or missing references)
		//IL_0058: Unknown result type (might be due to invalid IL or missing references)
		//IL_0063: Unknown result type (might be due to invalid IL or missing references)
		//IL_006f: Expected O, but got Unknown
		//IL_0076: Unknown result type (might be due to invalid IL or missing references)
		//IL_007c: Expected O, but got Unknown
		if ((Handle)(object)P2pInterface == (Handle)null)
		{
			P2pInterface = EOSPlatformManager.GetPlatformInterface().GetP2PInterface();
			AddNotifyPeerConnectionRequestOptions val = new AddNotifyPeerConnectionRequestOptions
			{
				LocalUserId = EOSLoginManager.loggedInProductId,
				SocketId = socketId
			};
			OnIncomingConnectionRequestCallback val2 = new OnIncomingConnectionRequestCallback(OnConnectionRequested);
			P2pInterface.AddNotifyPeerConnectionRequest(val, (object)null, val2);
			AddNotifyPeerConnectionClosedOptions val3 = new AddNotifyPeerConnectionClosedOptions
			{
				LocalUserId = EOSLoginManager.loggedInProductId,
				SocketId = socketId
			};
			OnRemoteConnectionClosedCallback val4 = new OnRemoteConnectionClosedCallback(OnConnectionClosed);
			P2pInterface.AddNotifyPeerConnectionClosed(val3, (object)null, val4);
		}
		myUserId = EOSLoginManager.loggedInProductId;
		base.serverP2PId = new CSteamID(myUserId);
	}

	private void OnConnectionClosed(OnRemoteConnectionClosedInfo connectionRequestInfo)
	{
		//IL_000c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0011: Unknown result type (might be due to invalid IL or missing references)
		ProductUserId remoteUserId = connectionRequestInfo.RemoteUserId;
		ConnectionClosedReason reason = connectionRequestInfo.Reason;
		Debug.Log((object)$"Close connection mesasge received for Product User ID {remoteUserId}, reason = {((object)(ConnectionClosedReason)(ref reason)).ToString()}");
		EOSNetworkConnection eOSNetworkConnection = EOSNetworkConnection.Find(myUserId, connectionRequestInfo.RemoteUserId);
		if (eOSNetworkConnection == null)
		{
			Debug.Log((object)$"Unable to find connection for remote user id: {connectionRequestInfo.RemoteUserId} when attempting to close!");
			return;
		}
		if (((NetworkManager)this).client != null && ((NetworkManager)this).client.connection == eOSNetworkConnection)
		{
			((NetworkConnection)eOSNetworkConnection).InvokeHandlerNoData((short)33);
			((NetworkConnection)eOSNetworkConnection).Disconnect();
			((NetworkConnection)eOSNetworkConnection).Dispose();
		}
		if (NetworkServer.active && NetworkServer.connections.IndexOf((NetworkConnection)(object)eOSNetworkConnection) != -1)
		{
			ServerHandleClientDisconnect((NetworkConnection)(object)eOSNetworkConnection);
		}
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
		//IL_006d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0079: Expected O, but got Unknown
		//IL_0074: Unknown result type (might be due to invalid IL or missing references)
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
				LobbyDetailsInfo val = default(LobbyDetailsInfo);
				maxPlayers = (int)(((int)(PlatformSystems.lobbyManager as EOSLobbyManager).CurrentLobbyDetails.CopyInfo(new LobbyDetailsCopyInfoOptions(), ref val) == 0) ? val.MaxMembers : 0);
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
		CheckSteamworks();
		args.GetArgSteamID(0);
		Object.op_Implicit((Object)(object)NetworkManagerSystem.singleton);
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
