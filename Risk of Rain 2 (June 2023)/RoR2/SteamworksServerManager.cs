using System;
using System.Net;
using Facepunch.Steamworks;
using RoR2.ConVar;
using RoR2.Networking;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.SceneManagement;

namespace RoR2;

internal sealed class SteamworksServerManager : ServerManagerBase<SteamworksServerManager>, IDisposable
{
	private sealed class SteamServerHeartbeatEnabledConVar : BaseConVar
	{
		public static readonly SteamServerHeartbeatEnabledConVar instance = new SteamServerHeartbeatEnabledConVar("steam_server_heartbeat_enabled", ConVarFlags.Engine, null, "Whether or not this server issues any heartbeats to the Steam master server and effectively advertises it in the master server list. Default is 1 for dedicated servers, 0 for client builds.");

		public bool value { get; private set; }

		public SteamServerHeartbeatEnabledConVar(string name, ConVarFlags flags, string defaultValue, string helpText)
			: base(name, flags, defaultValue, helpText)
		{
		}

		public override void SetString(string newValueString)
		{
			if (!TextSerialization.TryParseInvariant(newValueString, out int result))
			{
				return;
			}
			bool flag = result != 0;
			if (flag != value)
			{
				value = flag;
				if (ServerManagerBase<SteamworksServerManager>.instance != null)
				{
					ServerManagerBase<SteamworksServerManager>.instance.steamworksServer.AutomaticHeartbeats = value;
				}
			}
		}

		public override string GetString()
		{
			if (!value)
			{
				return "0";
			}
			return "1";
		}
	}

	public class SteamServerPortConVar : BaseConVar
	{
		public ushort value { get; private set; }

		public SteamServerPortConVar(string name, ConVarFlags flags, string defaultValue, string helpText)
			: base(name, flags, defaultValue, helpText)
		{
		}

		public override void SetString(string newValueString)
		{
			if (NetworkServer.active)
			{
				throw new ConCommandException("Cannot change this convar while the server is running.");
			}
			if (TextSerialization.TryParseInvariant(newValueString, out ushort result))
			{
				value = result;
			}
		}

		public override string GetString()
		{
			return value.ToString();
		}
	}

	private Server steamworksServer;

	private IPAddress address;

	private static readonly SteamServerPortConVar cvSteamServerQueryPort = new SteamServerPortConVar("steam_server_query_port", ConVarFlags.Engine, "27016", "The port for queries.");

	private static readonly SteamServerPortConVar cvSteamServerSteamPort = new SteamServerPortConVar("steam_server_steam_port", ConVarFlags.Engine, "0", "The port for steam. 0 for a random port in the range 10000-60000.");

	public SteamworksServerManager()
	{
		//IL_0012: Unknown result type (might be due to invalid IL or missing references)
		//IL_0018: Expected O, but got Unknown
		//IL_0086: Unknown result type (might be due to invalid IL or missing references)
		//IL_0090: Expected O, but got Unknown
		//IL_011b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0120: Unknown result type (might be due to invalid IL or missing references)
		string text = "Risk of Rain 2";
		ServerInit val = new ServerInit("Risk of Rain 2", text);
		val.IpAddress = IPAddress.Any;
		val.Secure = true;
		val.VersionString = RoR2Application.GetBuildId();
		val.GamePort = NetworkManagerSystem.SvPortConVar.instance.value;
		val.QueryPort = cvSteamServerQueryPort.value;
		val.SteamPort = cvSteamServerSteamPort.value;
		val.GameData = ServerManagerBase<SteamworksServerManager>.GetVersionGameDataString() + "," + NetworkModCompatibilityHelper.steamworksGameserverGameDataValue;
		steamworksServer = new Server(632360u, val);
		Debug.LogFormat("steamworksServer.IsValid={0}", new object[1] { ((BaseSteamworks)steamworksServer).IsValid });
		if (!((BaseSteamworks)steamworksServer).IsValid)
		{
			Dispose();
			return;
		}
		steamworksServer.Auth.OnAuthChange = OnAuthChange;
		steamworksServer.MaxPlayers = GetMaxPlayers();
		UpdateHostName(NetworkManagerSystem.SvHostNameConVar.instance.GetString());
		NetworkManagerSystem.SvHostNameConVar.instance.onValueChanged += UpdateHostName;
		Scene activeScene = SceneManager.GetActiveScene();
		UpdateMapName(((Scene)(ref activeScene)).name);
		NetworkManagerSystem.onServerSceneChangedGlobal += UpdateMapName;
		UpdatePassword(NetworkManagerSystem.SvPasswordConVar.instance.value);
		NetworkManagerSystem.SvPasswordConVar.instance.onValueChanged += UpdatePassword;
		steamworksServer.DedicatedServer = false;
		steamworksServer.AutomaticHeartbeats = SteamServerHeartbeatEnabledConVar.instance.value;
		steamworksServer.LogOnAnonymous();
		Debug.LogFormat("steamworksServer.LoggedOn={0}", new object[1] { steamworksServer.LoggedOn });
		RoR2Application.onUpdate += Update;
		NetworkManagerSystem.onServerConnectGlobal += OnServerConnectClient;
		NetworkManagerSystem.onServerDisconnectGlobal += OnServerDisconnectClient;
		ServerAuthManager.onAuthDataReceivedFromClient += OnAuthDataReceivedFromClient;
		ServerAuthManager.onAuthExpired += OnAuthExpired;
		Run.onServerRunSetRuleBookGlobal += base.OnServerRunSetRuleBookGlobal;
		PreGameController.onPreGameControllerSetRuleBookServerGlobal += base.OnPreGameControllerSetRuleBookServerGlobal;
		NetworkUser.onNetworkUserDiscovered += OnNetworkUserDiscovered;
		NetworkUser.onNetworkUserLost += OnNetworkUserLost;
		steamworksServer.SetKey("Test", "Value");
		steamworksServer.SetKey("gameMode", PreGameController.GameModeConVar.instance.GetString());
		steamworksServer.SetKey("buildId", RoR2Application.GetBuildId());
		steamworksServer.SetKey("modHash", NetworkModCompatibilityHelper.networkModHash);
		ruleBookKvHelper = new KeyValueSplitter("ruleBook", 2048, 2048, (Action<string, string>)steamworksServer.SetKey);
		modListKvHelper = new KeyValueSplitter(NetworkModCompatibilityHelper.steamworksGameserverRulesBaseName, 2048, 2048, (Action<string, string>)steamworksServer.SetKey);
		modListKvHelper.SetValue(NetworkModCompatibilityHelper.steamworksGameserverGameRulesValue);
		steamworksServer.ForceHeartbeat();
		UpdateServerRuleBook();
	}

	protected override void TagsStringUpdated()
	{
		base.TagsStringUpdated();
		steamworksServer.GameTags = base.tagsString;
	}

	private void OnAuthExpired(NetworkConnection conn, ClientAuthData authData)
	{
		SteamNetworkConnection obj = conn as SteamNetworkConnection;
		CSteamID? cSteamID = ((obj != null) ? new CSteamID?(obj.steamId) : authData?.steamId);
		if (cSteamID.HasValue)
		{
			steamworksServer.Auth.EndSession(cSteamID.Value.steamValue);
		}
	}

	private void OnAuthDataReceivedFromClient(NetworkConnection conn, ClientAuthData authData)
	{
		CSteamID steamId = authData.steamId;
		if (conn is SteamNetworkConnection steamNetworkConnection)
		{
			steamId = steamNetworkConnection.steamId;
		}
		steamworksServer.Auth.StartSession(authData.authTicket, steamId.steamValue);
	}

	private void OnServerConnectClient(NetworkConnection conn)
	{
	}

	private void OnServerDisconnectClient(NetworkConnection conn)
	{
	}

	public override void Dispose()
	{
		if (!disposed)
		{
			disposed = true;
			Server obj = steamworksServer;
			if (obj != null)
			{
				((BaseSteamworks)obj).Dispose();
			}
			steamworksServer = null;
			RoR2Application.onUpdate -= Update;
			NetworkManagerSystem.SvHostNameConVar.instance.onValueChanged -= UpdateHostName;
			NetworkManagerSystem.SvPasswordConVar.instance.onValueChanged -= UpdatePassword;
			NetworkManagerSystem.onServerSceneChangedGlobal -= UpdateMapName;
			NetworkManagerSystem.onServerConnectGlobal -= OnServerConnectClient;
			NetworkManagerSystem.onServerDisconnectGlobal -= OnServerDisconnectClient;
			ServerAuthManager.onAuthDataReceivedFromClient -= OnAuthDataReceivedFromClient;
			ServerAuthManager.onAuthExpired -= OnAuthExpired;
			Run.onServerRunSetRuleBookGlobal -= base.OnServerRunSetRuleBookGlobal;
			PreGameController.onPreGameControllerSetRuleBookServerGlobal -= base.OnPreGameControllerSetRuleBookServerGlobal;
			NetworkUser.onNetworkUserDiscovered -= OnNetworkUserDiscovered;
			NetworkUser.onNetworkUserLost -= OnNetworkUserLost;
		}
	}

	private int GetMaxPlayers()
	{
		return ((NetworkManager)NetworkManagerSystem.singleton).maxConnections;
	}

	private void OnNetworkUserLost(NetworkUser networkUser)
	{
		UpdateBotPlayerCount();
	}

	private void OnNetworkUserDiscovered(NetworkUser networkUser)
	{
		UpdateBotPlayerCount();
	}

	private void UpdateBotPlayerCount()
	{
		int num = 0;
		foreach (NetworkUser readOnlyInstances in NetworkUser.readOnlyInstancesList)
		{
			if (readOnlyInstances.isSplitScreenExtraPlayer)
			{
				num++;
			}
		}
		steamworksServer.BotCount = num;
	}

	private void UpdateHostName(string newHostName)
	{
		steamworksServer.ServerName = newHostName;
	}

	private void UpdateMapName(string sceneName)
	{
		steamworksServer.MapName = sceneName;
	}

	private void UpdatePassword(string newPassword)
	{
		steamworksServer.Passworded = newPassword.Length > 0;
	}

	private void OnAddressDiscovered()
	{
		Debug.Log((object)"Steamworks Server IP discovered.");
	}

	private void RefreshSteamServerPlayers()
	{
		foreach (NetworkUser readOnlyInstances in NetworkUser.readOnlyInstancesList)
		{
			ClientAuthData clientAuthData = ServerAuthManager.FindAuthData(((NetworkBehaviour)readOnlyInstances).connectionToClient);
			if (clientAuthData != null)
			{
				steamworksServer.UpdatePlayer(clientAuthData.steamId.steamValue, readOnlyInstances.userName, 0);
			}
		}
	}

	protected override void Update()
	{
		((BaseSteamworks)steamworksServer).Update();
		playerUpdateTimer -= Time.unscaledDeltaTime;
		if (playerUpdateTimer <= 0f)
		{
			playerUpdateTimer = playerUpdateInterval;
			RefreshSteamServerPlayers();
		}
		if (address == null)
		{
			address = steamworksServer.PublicIp;
			if (address != null)
			{
				OnAddressDiscovered();
			}
		}
	}

	private void OnAuthChange(ulong steamId, ulong ownerId, Status status)
	{
		//IL_002e: Unknown result type (might be due to invalid IL or missing references)
		//IL_003b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0069: Expected I4, but got Unknown
		//IL_0071: Unknown result type (might be due to invalid IL or missing references)
		NetworkConnection val = ServerAuthManager.FindConnectionForSteamID(new CSteamID(steamId));
		if (val == null)
		{
			Debug.LogWarningFormat("SteamworksServerManager.OnAuthChange(steamId={0}, ownerId={1}, status={2}): Could not find connection for steamId.", new object[3] { steamId, ownerId, status });
			return;
		}
		switch ((int)status)
		{
		case 0:
			break;
		default:
			throw new ArgumentOutOfRangeException("status", status, null);
		case 1:
		case 2:
		case 3:
		case 4:
		case 5:
		case 6:
		case 7:
		case 8:
		case 9:
			NetworkManagerSystem.singleton.ServerKickClient(val, new NetworkManagerSystem.SimpleLocalizedKickReason("KICK_REASON_STEAMWORKS_AUTH_FAILURE", ((object)(Status)(ref status)).ToString()));
			break;
		}
	}

	[ConCommand(commandName = "steam_server_force_heartbeat", flags = ConVarFlags.None, helpText = "Forces the server to issue a heartbeat to the master server.")]
	private static void CCSteamServerForceHeartbeat(ConCommandArgs args)
	{
		(ServerManagerBase<SteamworksServerManager>.instance?.steamworksServer ?? throw new ConCommandException("No Steamworks server is running.")).ForceHeartbeat();
	}

	[ConCommand(commandName = "steam_server_print_info", flags = ConVarFlags.None, helpText = "Prints debug info about the currently hosted Steamworks server.")]
	private static void CCSteamServerPrintInfo(ConCommandArgs args)
	{
		Server val = ServerManagerBase<SteamworksServerManager>.instance?.steamworksServer;
		if (val == null)
		{
			throw new ConCommandException("No Steamworks server is running.");
		}
		Debug.Log((object)string.Concat(string.Concat(string.Concat(string.Concat(string.Concat(string.Concat(string.Concat(string.Concat(string.Concat(string.Concat(string.Concat(string.Concat(string.Concat("" + $"IsValid={((BaseSteamworks)val).IsValid}\n", $"Product={val.Product}\n"), $"ModDir={val.ModDir}\n"), $"SteamId={val.SteamId}\n"), $"DedicatedServer={val.DedicatedServer}\n"), $"LoggedOn={val.LoggedOn}\n"), $"ServerName={val.ServerName}\n"), $"PublicIp={val.PublicIp}\n"), $"Passworded={val.Passworded}\n"), $"MaxPlayers={val.MaxPlayers}\n"), $"BotCount={val.BotCount}\n"), $"MapName={val.MapName}\n"), $"GameDescription={val.GameDescription}\n"), $"GameTags={val.GameTags}\n"));
	}
}
