using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Reflection;
using System.Text;
using HG;
using RoR2.ContentManagement;
using RoR2.ConVar;
using RoR2.UI;
using Unity;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.AddressableAssets.ResourceLocators;
using UnityEngine.Networking;
using UnityEngine.ResourceManagement.AsyncOperations;
using UnityEngine.ResourceManagement.ResourceLocations;
using UnityEngine.ResourceManagement.ResourceProviders;
using UnityEngine.SceneManagement;

namespace RoR2.Networking;

public abstract class NetworkManagerSystem : NetworkManager
{
	private class NetLogLevelConVar : BaseConVar
	{
		private static NetLogLevelConVar cvNetLogLevel = new NetLogLevelConVar("net_loglevel", ConVarFlags.Engine, null, "Network log verbosity.");

		public NetLogLevelConVar(string name, ConVarFlags flags, string defaultValue, string helpText)
			: base(name, flags, defaultValue, helpText)
		{
		}

		public override void SetString(string newValue)
		{
			if (TextSerialization.TryParseInvariant(newValue, out int result))
			{
				LogFilter.currentLogLevel = result;
			}
		}

		public override string GetString()
		{
			return TextSerialization.ToStringInvariant(LogFilter.currentLogLevel);
		}
	}

	private class SvListenConVar : BaseConVar
	{
		private static SvListenConVar cvSvListen = new SvListenConVar("sv_listen", ConVarFlags.Engine, null, "Whether or not the server will accept connections from other players.");

		public SvListenConVar(string name, ConVarFlags flags, string defaultValue, string helpText)
			: base(name, flags, defaultValue, helpText)
		{
		}

		public override void SetString(string newValue)
		{
			int result;
			if (NetworkServer.active)
			{
				Debug.Log((object)"Can't change value of sv_listen while server is running.");
			}
			else if (TextSerialization.TryParseInvariant(newValue, out result))
			{
				NetworkServer.dontListen = result == 0;
			}
		}

		public override string GetString()
		{
			if (!NetworkServer.dontListen)
			{
				return "1";
			}
			return "0";
		}
	}

	public class SvMaxPlayersConVar : BaseConVar
	{
		public static readonly SvMaxPlayersConVar instance = new SvMaxPlayersConVar("sv_maxplayers", ConVarFlags.Engine, null, "Maximum number of players allowed.");

		public int intValue => NetworkManager.singleton.maxConnections;

		public SvMaxPlayersConVar(string name, ConVarFlags flags, string defaultValue, string helpText)
			: base(name, flags, defaultValue, helpText)
		{
		}

		public override void SetString(string newValue)
		{
			if (NetworkServer.active)
			{
				throw new ConCommandException("Cannot change this convar while the server is running.");
			}
			if (Object.op_Implicit((Object)(object)NetworkManager.singleton) && TextSerialization.TryParseInvariant(newValue, out int result))
			{
				NetworkManager.singleton.maxConnections = Math.Min(Math.Max(result, 1), RoR2Application.hardMaxPlayers);
			}
		}

		public override string GetString()
		{
			if (!Object.op_Implicit((Object)(object)NetworkManager.singleton))
			{
				return "1";
			}
			return TextSerialization.ToStringInvariant(NetworkManager.singleton.maxConnections);
		}
	}

	private class KickMessage : MessageBase
	{
		public BaseKickReason kickReason;

		public KickMessage()
		{
		}

		public KickMessage(BaseKickReason kickReason)
		{
			this.kickReason = kickReason;
		}

		public override void Serialize(NetworkWriter writer)
		{
			((MessageBase)this).Serialize(writer);
			string text = ((object)kickReason)?.GetType().FullName ?? string.Empty;
			writer.Write(text);
			if (!(text == string.Empty))
			{
				((MessageBase)kickReason).Serialize(writer);
			}
		}

		public override void Deserialize(NetworkReader reader)
		{
			((MessageBase)this).Deserialize(reader);
			string typeName = reader.ReadString();
			Type type = null;
			try
			{
				type = Type.GetType(typeName);
			}
			catch
			{
			}
			if (type == null || !typeof(BaseKickReason).IsAssignableFrom(type) || type.IsAbstract)
			{
				kickReason = null;
				return;
			}
			kickReason = (BaseKickReason)Activator.CreateInstance(type);
			((MessageBase)kickReason).Deserialize(reader);
		}

		public bool TryGetDisplayTokenAndFormatParams(out string token, out object[] formatArgs)
		{
			if (kickReason == null)
			{
				token = null;
				formatArgs = null;
				return false;
			}
			kickReason.GetDisplayTokenAndFormatParams(out token, out formatArgs);
			return true;
		}
	}

	public abstract class BaseKickReason : MessageBase
	{
		public BaseKickReason()
		{
		}

		public abstract void GetDisplayTokenAndFormatParams(out string token, out object[] formatArgs);
	}

	public class SimpleLocalizedKickReason : BaseKickReason
	{
		public string baseToken;

		public string[] formatArgs;

		public SimpleLocalizedKickReason()
		{
		}

		public SimpleLocalizedKickReason(string baseToken, params string[] formatArgs)
		{
			this.baseToken = baseToken;
			this.formatArgs = formatArgs;
		}

		public override void GetDisplayTokenAndFormatParams(out string token, out object[] formatArgs)
		{
			token = baseToken;
			object[] array = this.formatArgs;
			formatArgs = array;
		}

		public override void Serialize(NetworkWriter writer)
		{
			writer.Write(baseToken);
			GeneratedNetworkCode._WriteArrayString_None(writer, formatArgs);
		}

		public override void Deserialize(NetworkReader reader)
		{
			baseToken = reader.ReadString();
			formatArgs = GeneratedNetworkCode._ReadArrayString_None(reader);
		}
	}

	public class ModMismatchKickReason : BaseKickReason
	{
		public string[] serverModList = Array.Empty<string>();

		public ModMismatchKickReason()
		{
		}

		public ModMismatchKickReason(IEnumerable<string> serverModList)
		{
			this.serverModList = serverModList.ToArray();
		}

		public override void GetDisplayTokenAndFormatParams(out string token, out object[] formatArgs)
		{
			IEnumerable<string> networkModList = NetworkModCompatibilityHelper.networkModList;
			IEnumerable<string> values = networkModList.Except(serverModList);
			IEnumerable<string> values2 = serverModList.Except(networkModList);
			token = "KICK_REASON_MOD_MISMATCH";
			object[] array = new string[2]
			{
				string.Join("\n", values),
				string.Join("\n", values2)
			};
			formatArgs = array;
		}

		public override void Serialize(NetworkWriter writer)
		{
			GeneratedNetworkCode._WriteArrayString_None(writer, serverModList);
		}

		public override void Deserialize(NetworkReader reader)
		{
			serverModList = GeneratedNetworkCode._ReadArrayString_None(reader);
		}
	}

	protected class AddPlayerMessage : MessageBase
	{
		public UserID id;

		public byte[] steamAuthTicketData;

		public override void Serialize(NetworkWriter writer)
		{
			GeneratedNetworkCode._WriteUserID_None(writer, id);
			writer.WriteBytesFull(steamAuthTicketData);
		}

		public override void Deserialize(NetworkReader reader)
		{
			id = GeneratedNetworkCode._ReadUserID_None(reader);
			steamAuthTicketData = reader.ReadBytesAndSize();
		}
	}

	public class SvHostNameConVar : BaseConVar
	{
		public static readonly SvHostNameConVar instance = new SvHostNameConVar("sv_hostname", ConVarFlags.None, "", "The public name to use for the server if hosting.");

		private string value = "NAME";

		public event Action<string> onValueChanged;

		public SvHostNameConVar(string name, ConVarFlags flags, string defaultValue, string helpText)
			: base(name, flags, defaultValue, helpText)
		{
		}

		public override void SetString(string newValue)
		{
			value = newValue;
			this.onValueChanged?.Invoke(newValue);
		}

		public override string GetString()
		{
			return value;
		}
	}

	public class SvPortConVar : BaseConVar
	{
		public static readonly SvPortConVar instance = new SvPortConVar("sv_port", ConVarFlags.Engine, null, "The port to use for the server if hosting.");

		public ushort value
		{
			get
			{
				if (!Object.op_Implicit((Object)(object)singleton))
				{
					return 0;
				}
				return (ushort)((NetworkManager)singleton).networkPort;
			}
		}

		public SvPortConVar(string name, ConVarFlags flags, string defaultValue, string helpText)
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
				((NetworkManager)singleton).networkPort = result;
			}
		}

		public override string GetString()
		{
			return value.ToString();
		}
	}

	public class SvIPConVar : BaseConVar
	{
		public static readonly SvIPConVar instance = new SvIPConVar("sv_ip", ConVarFlags.Engine, null, "The IP for the server to bind to if hosting.");

		public SvIPConVar(string name, ConVarFlags flags, string defaultValue, string helpText)
			: base(name, flags, defaultValue, helpText)
		{
		}

		public override void SetString(string newValueString)
		{
			if (NetworkServer.active)
			{
				throw new ConCommandException("Cannot change this convar while the server is running.");
			}
			((NetworkManager)singleton).serverBindAddress = newValueString;
		}

		public override string GetString()
		{
			if (!Object.op_Implicit((Object)(object)singleton))
			{
				return string.Empty;
			}
			return ((NetworkManager)singleton).serverBindAddress;
		}
	}

	public class SvPasswordConVar : BaseConVar
	{
		public static readonly SvPasswordConVar instance = new SvPasswordConVar("sv_password", ConVarFlags.None, "", "The password to use for the server if hosting.");

		public string value { get; private set; }

		public event Action<string> onValueChanged;

		public SvPasswordConVar(string name, ConVarFlags flags, string defaultValue, string helpText)
			: base(name, flags, defaultValue, helpText)
		{
		}

		public override void SetString(string newValue)
		{
			if (newValue == null)
			{
				newValue = "";
			}
			if (!(value == newValue))
			{
				value = newValue;
				this.onValueChanged?.Invoke(value);
			}
		}

		public override string GetString()
		{
			return value;
		}
	}

	protected class AddressablesChangeSceneAsyncOperation : IChangeSceneAsyncOperation
	{
		private static AsyncOperationHandle<SceneInstance>? previousLoadOperation;

		private AsyncOperationHandle<SceneInstance> srcOperation;

		private bool srcOperationIsDone;

		private SceneInstance sceneInstance;

		private bool isActivated;

		private bool _allowSceneActivation;

		public bool isDone { get; private set; }

		public bool allowSceneActivation
		{
			get
			{
				return _allowSceneActivation;
			}
			set
			{
				if (_allowSceneActivation != value)
				{
					_allowSceneActivation = value;
					ActivateIfReady();
				}
			}
		}

		public AddressablesChangeSceneAsyncOperation(object key, LoadSceneMode loadMode, bool activateOnLoad)
		{
			//IL_0008: Unknown result type (might be due to invalid IL or missing references)
			//IL_000c: Unknown result type (might be due to invalid IL or missing references)
			//IL_0011: Unknown result type (might be due to invalid IL or missing references)
			//IL_0017: Unknown result type (might be due to invalid IL or missing references)
			srcOperation = Addressables.LoadSceneAsync(key, loadMode, activateOnLoad, 100);
			previousLoadOperation = srcOperation;
			srcOperation.Completed += delegate(AsyncOperationHandle<SceneInstance> v)
			{
				//IL_000a: Unknown result type (might be due to invalid IL or missing references)
				//IL_000f: Unknown result type (might be due to invalid IL or missing references)
				srcOperationIsDone = true;
				sceneInstance = v.Result;
				ActivateIfReady();
			};
			allowSceneActivation = activateOnLoad;
		}

		private void ActivateIfReady()
		{
			if (srcOperationIsDone && !isActivated)
			{
				((SceneInstance)(ref sceneInstance)).ActivateAsync().completed += delegate
				{
					isDone = true;
				};
			}
		}
	}

	protected static readonly FieldInfo loadingSceneAsyncFieldInfo;

	protected float _unpredictedServerFixedTime;

	protected float _unpredictedServerFixedTimeSmoothed;

	protected float unpredictedServerFixedTimeVelocity;

	protected float _unpredictedServerFrameTime;

	protected float _unpredictedServerFrameTimeSmoothed;

	protected float unpredictedServerFrameTimeVelocity;

	protected static FloatConVar cvNetTimeSmoothRate;

	public float debugServerTime;

	public float debugRTT;

	protected bool isSinglePlayer;

	protected bool actedUponDesiredHost;

	protected float lastDesiredHostSetTime = float.NegativeInfinity;

	protected HostDescription _desiredHost;

	private static bool wasFading;

	protected static readonly string[] sceneWhiteList;

	public static readonly StringConVar cvClPassword;

	protected GameObject serverNetworkSessionInstance;

	private static NetworkReader DefaultNetworkReader;

	private static readonly FloatConVar svTimeTransmitInterval;

	private float timeTransmitTimer;

	protected bool serverShuttingDown;

	private static readonly Queue<NetworkConnection> clientsReadyDuringLevelTransition;

	public static readonly StringConVar cvSvCustomTags;

	protected static bool isLoadingScene
	{
		get
		{
			//IL_000b: Unknown result type (might be due to invalid IL or missing references)
			//IL_0011: Expected O, but got Unknown
			IChangeSceneAsyncOperation val = (IChangeSceneAsyncOperation)loadingSceneAsyncFieldInfo.GetValue(null);
			if (val != null)
			{
				return !val.isDone;
			}
			return false;
		}
	}

	public static NetworkManagerSystem singleton => (NetworkManagerSystem)(object)NetworkManager.singleton;

	public float unpredictedServerFixedTime => _unpredictedServerFixedTime;

	public float unpredictedServerFixedTimeSmoothed => _unpredictedServerFixedTimeSmoothed;

	public float serverFixedTime => unpredictedServerFixedTimeSmoothed + filteredClientRttFixed;

	public float unpredictedServerFrameTime => _unpredictedServerFrameTime;

	public float unpredictedServerFrameTimeSmoothed => _unpredictedServerFrameTimeSmoothed;

	public float serverFrameTime => unpredictedServerFrameTimeSmoothed + filteredClientRttFrame;

	public HostDescription desiredHost
	{
		get
		{
			return _desiredHost;
		}
		set
		{
			if (!_desiredHost.Equals(value))
			{
				_desiredHost = value;
				actedUponDesiredHost = false;
				lastDesiredHostSetTime = Time.unscaledTime;
				Debug.LogFormat("NetworkManagerSystem.desiredHost={0}", new object[1] { _desiredHost.ToString() });
			}
		}
	}

	public bool clientHasConfirmedQuit { get; private set; }

	protected bool clientIsConnecting
	{
		get
		{
			NetworkClient client = base.client;
			if (((client != null) ? client.connection : null) != null)
			{
				return !base.client.isConnected;
			}
			return false;
		}
	}

	public float clientRttFixed { get; private set; }

	public float clientRttFrame { get; private set; }

	public float filteredClientRttFixed { get; private set; }

	public float filteredClientRttFrame { get; private set; }

	public bool isHost { get; private set; }

	public CSteamID clientP2PId { get; protected set; } = CSteamID.nil;


	public CSteamID serverP2PId { get; protected set; } = CSteamID.nil;


	public static event Action onStartGlobal;

	public static event Action<NetworkClient> onStartClientGlobal;

	public static event Action onStopClientGlobal;

	public static event Action<NetworkConnection> onClientConnectGlobal;

	public static event Action<NetworkConnection> onClientDisconnectGlobal;

	public static event Action onStartHostGlobal;

	public static event Action onStopHostGlobal;

	public static event Action onStartServerGlobal;

	public static event Action onStopServerGlobal;

	public static event Action<NetworkConnection> onServerConnectGlobal;

	public static event Action<NetworkConnection> onServerDisconnectGlobal;

	public static event Action<string> onServerSceneChangedGlobal;

	static NetworkManagerSystem()
	{
		//IL_005d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0067: Expected O, but got Unknown
		cvNetTimeSmoothRate = new FloatConVar("net_time_smooth_rate", ConVarFlags.None, "1.05", "The smoothing rate for the network time.");
		wasFading = false;
		sceneWhiteList = new string[3] { "title", "crystalworld", "logbook" };
		cvClPassword = new StringConVar("cl_password", ConVarFlags.None, "", "The password to use when joining a passworded server.");
		DefaultNetworkReader = new NetworkReader();
		svTimeTransmitInterval = new FloatConVar("sv_time_transmit_interval", ConVarFlags.Cheat, (1f / 60f).ToString(), "How long it takes for the server to issue a time update to clients.");
		clientsReadyDuringLevelTransition = new Queue<NetworkConnection>();
		cvSvCustomTags = new StringConVar("sv_custom_tags", ConVarFlags.None, "", "Comma-delimited custom tags to report to the server browser.");
		loadingSceneAsyncFieldInfo = typeof(NetworkManager).GetField("s_LoadingSceneAsync", BindingFlags.Static | BindingFlags.NonPublic);
		if (loadingSceneAsyncFieldInfo == null)
		{
			Debug.LogError((object)"NetworkManager.s_LoadingSceneAsync field could not be found! Make sure to provide a proper implementation for this version of Unity.");
		}
	}

	public virtual void Init(NetworkManagerConfiguration configurationComponent)
	{
		//IL_001a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0056: Unknown result type (might be due to invalid IL or missing references)
		((NetworkManager)this).dontDestroyOnLoad = configurationComponent.DontDestroyOnLoad;
		((NetworkManager)this).runInBackground = configurationComponent.RunInBackground;
		((NetworkManager)this).logLevel = configurationComponent.LogLevel;
		((NetworkManager)this).offlineScene = configurationComponent.OfflineScene;
		((NetworkManager)this).onlineScene = configurationComponent.OnlineScene;
		((NetworkManager)this).playerPrefab = configurationComponent.PlayerPrefab;
		((NetworkManager)this).autoCreatePlayer = configurationComponent.AutoCreatePlayer;
		((NetworkManager)this).playerSpawnMethod = configurationComponent.PlayerSpawnMethod;
		((NetworkManager)this).spawnPrefabs.Clear();
		((NetworkManager)this).spawnPrefabs.AddRange(configurationComponent.SpawnPrefabs);
		((NetworkManager)this).customConfig = configurationComponent.CustomConfig;
		((NetworkManager)this).maxConnections = configurationComponent.MaxConnections;
		((NetworkManager)this).channels.Clear();
		((NetworkManager)this).channels.AddRange(configurationComponent.QosChannels);
	}

	protected void InitializeTime()
	{
		_unpredictedServerFixedTime = 0f;
		_unpredictedServerFixedTimeSmoothed = 0f;
		unpredictedServerFixedTimeVelocity = 1f;
		_unpredictedServerFrameTime = 0f;
		_unpredictedServerFrameTimeSmoothed = 0f;
		unpredictedServerFrameTimeVelocity = 1f;
	}

	protected void UpdateTime(ref float targetValue, ref float currentValue, ref float velocity, float deltaTime)
	{
		if (!(deltaTime <= 0f))
		{
			targetValue += deltaTime;
			float num = (targetValue - currentValue) / deltaTime;
			float num2 = 1f;
			if (velocity == 0f || Mathf.Abs(num) > num2 * 3f)
			{
				currentValue = targetValue;
				velocity = num2;
			}
			else
			{
				currentValue += velocity * deltaTime;
				velocity = Mathf.MoveTowards(velocity, num, cvNetTimeSmoothRate.value * deltaTime);
			}
		}
	}

	protected static NetworkUser[] GetConnectionNetworkUsers(NetworkConnection conn)
	{
		List<PlayerController> playerControllers = conn.playerControllers;
		NetworkUser[] array = new NetworkUser[playerControllers.Count];
		for (int i = 0; i < playerControllers.Count; i++)
		{
			array[i] = playerControllers[i].gameObject.GetComponent<NetworkUser>();
		}
		return array;
	}

	protected abstract void Start();

	protected void OnDestroy()
	{
		typeof(NetworkManager).GetMethod("OnDestroy", BindingFlags.Instance | BindingFlags.NonPublic).Invoke(this, null);
	}

	public void FireOnStartGlobalEvent()
	{
		NetworkManagerSystem.onStartGlobal?.Invoke();
	}

	protected void FixedUpdate()
	{
		UpdateTime(ref _unpredictedServerFixedTime, ref _unpredictedServerFixedTimeSmoothed, ref unpredictedServerFixedTimeVelocity, Time.fixedDeltaTime);
		FixedUpdateServer();
		FixedUpdateClient();
		debugServerTime = unpredictedServerFixedTime;
		debugRTT = clientRttFrame;
	}

	protected abstract void Update();

	protected abstract void EnsureDesiredHost();

	public abstract void ForceCloseAllConnections();

	public void StartSinglePlayer()
	{
		Debug.Log((object)"Setting isSinglePlayer to true");
		NetworkServer.dontListen = true;
		desiredHost = default(HostDescription);
		isSinglePlayer = true;
	}

	public void ResetDesiredHost()
	{
		actedUponDesiredHost = false;
		singleton.desiredHost = default(HostDescription);
	}

	public abstract void CreateLocalLobby();

	public override void OnStartClient(NetworkClient newClient)
	{
		((NetworkManager)this).OnStartClient(newClient);
		InitializeTime();
		RegisterPrefabs(ContentManager.bodyPrefabs);
		RegisterPrefabs(ContentManager.masterPrefabs);
		RegisterPrefabs(ContentManager.projectilePrefabs);
		RegisterPrefabs(ContentManager.networkedObjectPrefabs);
		RegisterPrefabs(ContentManager.gameModePrefabs);
		ClientScene.RegisterPrefab(LegacyResourcesAPI.Load<GameObject>("Prefabs/NetworkSession"));
		ClientScene.RegisterPrefab(LegacyResourcesAPI.Load<GameObject>("Prefabs/Stage"));
		NetworkMessageHandlerAttribute.RegisterClientMessages(newClient);
		NetworkManagerSystem.onStartClientGlobal?.Invoke(newClient);
		static void RegisterPrefabs(GameObject[] prefabs)
		{
			for (int i = 0; i < prefabs.Length; i++)
			{
				ClientScene.RegisterPrefab(prefabs[i]);
			}
		}
	}

	public override void OnStopClient()
	{
		try
		{
			NetworkManagerSystem.onStopClientGlobal?.Invoke();
		}
		catch (Exception ex)
		{
			Debug.LogError((object)ex);
		}
		foreach (NetworkClient allClient in NetworkClient.allClients)
		{
			if (allClient != null)
			{
				NetworkConnection connection = allClient.connection;
				if (connection != null)
				{
					connection.Disconnect();
				}
			}
		}
		ForceCloseAllConnections();
		if (actedUponDesiredHost)
		{
			singleton.desiredHost = HostDescription.none;
		}
		((NetworkManager)this).OnStopClient();
	}

	public override void OnClientConnect(NetworkConnection conn)
	{
		((NetworkManager)this).OnClientConnect(conn);
		clientRttFrame = 0f;
		filteredClientRttFixed = 0f;
		ClientSendAuth(conn);
		ClientSetPlayers(conn);
		NetworkManagerSystem.onClientConnectGlobal?.Invoke(conn);
	}

	public override void OnClientDisconnect(NetworkConnection conn)
	{
		((NetworkManager)this).OnClientDisconnect(conn);
		NetworkManagerSystem.onClientDisconnectGlobal?.Invoke(conn);
	}

	public void ClientAddPlayer(short playerControllerId, NetworkConnection connection = null)
	{
		foreach (PlayerController localPlayer in ClientScene.localPlayers)
		{
			if (localPlayer.playerControllerId == playerControllerId && localPlayer.IsValid && Object.op_Implicit((Object)(object)localPlayer.gameObject))
			{
				Debug.LogFormat("Player {0} already added, aborting.", new object[1] { playerControllerId });
				return;
			}
		}
		Debug.LogFormat("Adding local player controller {0} on connection {1}", new object[2] { playerControllerId, connection });
		AddPlayerMessage addPlayerMessage = CreateClientAddPlayerMessage();
		ClientScene.AddPlayer(connection, playerControllerId, (MessageBase)(object)addPlayerMessage);
	}

	protected abstract AddPlayerMessage CreateClientAddPlayerMessage();

	protected void UpdateClient()
	{
		UpdateCheckInactiveConnections();
		NetworkClient client = base.client;
		if (((client != null) ? client.connection : null) != null)
		{
			filteredClientRttFrame = RttManager.GetConnectionFrameSmoothedRtt(base.client.connection);
			clientRttFrame = RttManager.GetConnectionRTT(base.client.connection);
		}
		bool flag = (base.client != null && !ClientScene.ready) || isLoadingScene;
		if (wasFading != flag)
		{
			if (flag)
			{
				FadeToBlackManager.fadeCount++;
			}
			else
			{
				FadeToBlackManager.fadeCount--;
			}
			wasFading = flag;
		}
	}

	protected abstract void UpdateCheckInactiveConnections();

	protected abstract void StartClient(UserID serverID);

	public abstract bool IsConnectedToServer(UserID serverID);

	private void FixedUpdateClient()
	{
		if (!NetworkClient.active || base.client == null)
		{
			return;
		}
		NetworkClient client = base.client;
		if (((client != null) ? client.connection : null) != null && base.client.connection.isConnected)
		{
			NetworkConnection connection = base.client.connection;
			filteredClientRttFixed = RttManager.GetConnectionFixedSmoothedRtt(connection);
			clientRttFixed = RttManager.GetConnectionRTT(connection);
			if (!Util.ConnectionIsLocal(connection))
			{
				RttManager.Ping(connection, QosChannelIndex.ping.intVal);
			}
		}
	}

	public override void OnClientSceneChanged(NetworkConnection conn)
	{
		//IL_0013: Unknown result type (might be due to invalid IL or missing references)
		//IL_0018: Unknown result type (might be due to invalid IL or missing references)
		string networkSceneName = NetworkManager.networkSceneName;
		List<string> list = new List<string>();
		bool flag = false;
		for (int i = 0; i < SceneManager.sceneCount; i++)
		{
			Scene sceneAt = SceneManager.GetSceneAt(i);
			string name = ((Scene)(ref sceneAt)).name;
			list.Add(name);
			if (name == networkSceneName)
			{
				flag = true;
			}
		}
		Debug.Log((object)("OnClientSceneChanged networkSceneName=" + networkSceneName + " loadedScenes=" + string.Join(", ", list)));
		if (!flag)
		{
			Debug.Log((object)"OnClientSceneChanged skipped: scene specified by networkSceneName is not loaded.");
			return;
		}
		((NetworkManager)this).autoCreatePlayer = false;
		((NetworkManager)this).OnClientSceneChanged(conn);
		ClientSetPlayers(conn);
		FadeToBlackManager.ForceFullBlack();
	}

	private void ClientSendAuth(NetworkConnection conn)
	{
		ClientAuthData data = new ClientAuthData();
		PlatformAuth(ref data, conn);
		data.password = cvClPassword.value;
		data.version = RoR2Application.GetBuildId();
		data.modHash = NetworkModCompatibilityHelper.networkModHash;
		data.entitlements = PlatformSystems.entitlementsSystem.BuildEntitlements();
		conn.Send((short)74, (MessageBase)(object)data);
	}

	protected abstract void PlatformAuth(ref ClientAuthData data, NetworkConnection conn);

	protected void ClientSetPlayers(NetworkConnection conn)
	{
		ReadOnlyCollection<LocalUser> readOnlyLocalUsersList = LocalUserManager.readOnlyLocalUsersList;
		for (int i = 0; i < readOnlyLocalUsersList.Count; i++)
		{
			ClientAddPlayer((short)readOnlyLocalUsersList[i].id, conn);
		}
	}

	[RuntimeInitializeOnLoadMethod(/*Could not decode attribute arguments.*/)]
	private static void ClientInit()
	{
		SceneCatalog.onMostRecentSceneDefChanged += ClientUpdateOfflineScene;
	}

	private static void ClientUpdateOfflineScene(SceneDef sceneDef)
	{
		if (Object.op_Implicit((Object)(object)singleton) && sceneDef.isOfflineScene)
		{
			((NetworkManager)singleton).offlineScene = sceneDef.cachedName;
		}
	}

	protected static void EnsureNetworkManagerNotBusy()
	{
		if (!Object.op_Implicit((Object)(object)singleton) || (!singleton.serverShuttingDown && !isLoadingScene))
		{
			return;
		}
		throw new ConCommandException("NetworkManager is busy and cannot receive commands.");
	}

	[ConCommand(commandName = "client_set_players", flags = ConVarFlags.None, helpText = "Adds network players for all local players. Debug only.")]
	private static void CCClientSetPlayers(ConCommandArgs args)
	{
		if (Object.op_Implicit((Object)(object)singleton))
		{
			singleton.PlatformClientSetPlayers(args);
		}
	}

	protected abstract void PlatformClientSetPlayers(ConCommandArgs args);

	[ConCommand(commandName = "ping", flags = ConVarFlags.None, helpText = "Prints the current round trip time from this client to the server and back.")]
	private static void CCPing(ConCommandArgs args)
	{
		if (Object.op_Implicit((Object)(object)singleton))
		{
			NetworkManagerSystem networkManagerSystem = singleton;
			object obj;
			if (networkManagerSystem == null)
			{
				obj = null;
			}
			else
			{
				NetworkClient client = ((NetworkManager)networkManagerSystem).client;
				obj = ((client != null) ? client.connection : null);
			}
			NetworkConnection val = (NetworkConnection)obj;
			if (val != null)
			{
				Debug.LogFormat("rtt={0}ms smoothedFrame={1} smoothedFixed={2}", new object[3]
				{
					RttManager.GetConnectionRTTInMilliseconds(val),
					RttManager.GetConnectionFrameSmoothedRtt(val),
					RttManager.GetConnectionFixedSmoothedRtt(val)
				});
			}
			else
			{
				Debug.Log((object)"No connection to server.");
			}
		}
	}

	[ConCommand(commandName = "set_scene", flags = ConVarFlags.None, helpText = "Changes to the named scene.")]
	private static void CCSetScene(ConCommandArgs args)
	{
		string argString = args.GetArgString(0);
		if (!Object.op_Implicit((Object)(object)singleton))
		{
			throw new ConCommandException("set_scene failed: NetworkManagerSystem is not available.");
		}
		SceneCatalog.GetSceneDefForCurrentScene();
		SceneDef sceneDefFromSceneName = SceneCatalog.GetSceneDefFromSceneName(argString);
		if (!Object.op_Implicit((Object)(object)sceneDefFromSceneName))
		{
			throw new ConCommandException("\"" + argString + "\" is not a valid scene.");
		}
		bool boolValue = Console.CheatsConVar.instance.boolValue;
		if (!Object.op_Implicit((Object)(object)NetworkManager.singleton))
		{
			_ = 1;
		}
		else
			_ = NetworkManager.singleton.isNetworkActive;
		if (NetworkManager.singleton.isNetworkActive)
		{
			if (sceneDefFromSceneName.isOfflineScene)
			{
				throw new ConCommandException("Cannot switch to scene \"" + argString + "\": Cannot switch to offline-only scene while in a network session.");
			}
			if (!boolValue)
			{
				throw new ConCommandException("Cannot switch to scene \"" + argString + "\": Cheats must be enabled to switch between online-only scenes.");
			}
		}
		else if (!sceneDefFromSceneName.isOfflineScene)
		{
			throw new ConCommandException("Cannot switch to scene \"" + argString + "\": Cannot switch to online-only scene while not in a network session.");
		}
		if (NetworkServer.active)
		{
			Debug.LogFormat("Setting server scene to {0}", new object[1] { argString });
			((NetworkManager)singleton).ServerChangeScene(argString);
			return;
		}
		if (!NetworkClient.active)
		{
			Debug.LogFormat("Setting offline scene to {0}", new object[1] { argString });
			((NetworkManager)singleton).ServerChangeScene(argString);
			return;
		}
		throw new ConCommandException("Cannot change scene while connected to a remote server.");
	}

	[ConCommand(commandName = "scene_list", flags = ConVarFlags.None, helpText = "Prints a list of all available scene names.")]
	private static void CCSceneList(ConCommandArgs args)
	{
		if (Object.op_Implicit((Object)(object)singleton))
		{
			string[] array = new string[SceneManager.sceneCountInBuildSettings];
			for (int i = 0; i < SceneManager.sceneCountInBuildSettings; i++)
			{
				array[i] = $"[{i}]={SceneUtility.GetScenePathByBuildIndex(i)}";
			}
			Debug.Log((object)string.Join("\n", array));
		}
	}

	[ConCommand(commandName = "dump_network_ids", flags = ConVarFlags.None, helpText = "Lists the network ids of all currently networked game objects.")]
	private static void CCDumpNetworkIDs(ConCommandArgs args)
	{
		//IL_0071: Unknown result type (might be due to invalid IL or missing references)
		//IL_0076: Unknown result type (might be due to invalid IL or missing references)
		if (Object.op_Implicit((Object)(object)singleton))
		{
			List<NetworkIdentity> list = new List<NetworkIdentity>(Object.FindObjectsOfType<NetworkIdentity>());
			Debug.Log((object)$"Found {list.Count} NetworkIdentity components");
			list.Sort(delegate(NetworkIdentity lhs, NetworkIdentity rhs)
			{
				//IL_0001: Unknown result type (might be due to invalid IL or missing references)
				//IL_0006: Unknown result type (might be due to invalid IL or missing references)
				//IL_000f: Unknown result type (might be due to invalid IL or missing references)
				//IL_0014: Unknown result type (might be due to invalid IL or missing references)
				NetworkInstanceId netId2 = lhs.netId;
				uint value = ((NetworkInstanceId)(ref netId2)).Value;
				netId2 = rhs.netId;
				return (int)(value - ((NetworkInstanceId)(ref netId2)).Value);
			});
			for (int i = 0; i < list.Count; i++)
			{
				object[] array = new object[2];
				NetworkInstanceId netId = list[i].netId;
				array[0] = ((NetworkInstanceId)(ref netId)).Value;
				array[1] = ((Object)((Component)list[i]).gameObject).name;
				Debug.LogFormat("{0}={1}", array);
			}
		}
	}

	[ConCommand(commandName = "disconnect", flags = ConVarFlags.None, helpText = "Disconnect from a server or shut down the current server.")]
	private static void CCDisconnect(ConCommandArgs args)
	{
		if (Object.op_Implicit((Object)(object)singleton))
		{
			singleton.PlatformDisconnect(args);
		}
	}

	protected abstract void PlatformDisconnect(ConCommandArgs args);

	protected void Disconnect()
	{
		if (!serverShuttingDown && ((NetworkManager)singleton).isNetworkActive)
		{
			Debug.Log((object)"Network shutting down...");
			if (NetworkServer.active)
			{
				singleton.RequestServerShutdown();
			}
			else
			{
				((NetworkManager)singleton).StopClient();
			}
		}
	}

	[ConCommand(commandName = "connect", flags = ConVarFlags.None, helpText = "Connect to a server.")]
	private static void CCConnect(ConCommandArgs args)
	{
		if (Object.op_Implicit((Object)(object)singleton))
		{
			singleton.PlatformConnect(args);
		}
	}

	protected abstract void PlatformConnect(ConCommandArgs args);

	private static void CCConnectP2P(ConCommandArgs args)
	{
		if (Object.op_Implicit((Object)(object)singleton))
		{
			singleton.PlatformConnect(args);
		}
	}

	protected abstract void PlatformConnectP2P(ConCommandArgs args);

	[ConCommand(commandName = "host", flags = ConVarFlags.None, helpText = "Host a server. First argument is whether or not to listen for incoming connections.")]
	private static void CCHost(ConCommandArgs args)
	{
		if (Object.op_Implicit((Object)(object)singleton))
		{
			singleton.PlatformHost(args);
		}
	}

	protected abstract void PlatformHost(ConCommandArgs args);

	[ConCommand(commandName = "steam_get_p2p_session_state")]
	private static void CCSGetP2PSessionState(ConCommandArgs args)
	{
		if (Object.op_Implicit((Object)(object)singleton))
		{
			singleton.PlatformGetP2PSessionState(args);
		}
	}

	protected abstract void PlatformGetP2PSessionState(ConCommandArgs args);

	[ConCommand(commandName = "kick_steam", flags = ConVarFlags.SenderMustBeServer, helpText = "Kicks the user with the specified steam id from the server.")]
	private static void CCKickSteam(ConCommandArgs args)
	{
		if (Object.op_Implicit((Object)(object)singleton))
		{
			singleton.PlatformKick(args);
		}
	}

	protected abstract void PlatformKick(ConCommandArgs args);

	[ConCommand(commandName = "ban_steam", flags = ConVarFlags.SenderMustBeServer, helpText = "Bans the user with the specified steam id from the server.")]
	private static void CCBan(ConCommandArgs args)
	{
		if (Object.op_Implicit((Object)(object)singleton))
		{
			singleton.PlatformBan(args);
		}
	}

	protected abstract void PlatformBan(ConCommandArgs args);

	public virtual bool IsHost()
	{
		return isHost;
	}

	public override void OnStartHost()
	{
		((NetworkManager)this).OnStartHost();
		isHost = true;
		NetworkManagerSystem.onStartHostGlobal?.Invoke();
	}

	public override void OnStopHost()
	{
		NetworkManagerSystem.onStopHostGlobal?.Invoke();
		isHost = false;
		((NetworkManager)this).OnStopHost();
	}

	[NetworkMessageHandler(client = true, server = false, msgType = 67)]
	private static void HandleKick(NetworkMessage netMsg)
	{
		KickMessage kickMessage = netMsg.ReadMessage<KickMessage>();
		Debug.LogFormat("Received kick message. Reason={0}", new object[1] { kickMessage.kickReason });
		((NetworkManager)singleton).StopClient();
		kickMessage.TryGetDisplayTokenAndFormatParams(out var token, out var formatArgs);
		SimpleDialogBox simpleDialogBox = SimpleDialogBox.Create();
		simpleDialogBox.headerToken = new SimpleDialogBox.TokenParamsPair("DISCONNECTED");
		simpleDialogBox.descriptionToken = new SimpleDialogBox.TokenParamsPair(token ?? string.Empty, formatArgs);
		simpleDialogBox.AddCancelButton(CommonLanguageTokens.ok);
		simpleDialogBox.rootObject.transform.SetParent(((Component)RoR2Application.instance.mainCanvas).transform);
	}

	public static void HandleKick(string displayToken)
	{
		((NetworkManager)singleton).StopClient();
		SimpleDialogBox simpleDialogBox = SimpleDialogBox.Create();
		simpleDialogBox.headerToken = new SimpleDialogBox.TokenParamsPair("DISCONNECTED");
		simpleDialogBox.descriptionToken = new SimpleDialogBox.TokenParamsPair(displayToken);
		simpleDialogBox.AddCancelButton(CommonLanguageTokens.ok);
		simpleDialogBox.rootObject.transform.SetParent(((Component)RoR2Application.instance.mainCanvas).transform);
	}

	[NetworkMessageHandler(msgType = 54, client = true)]
	private static void HandleUpdateTime(NetworkMessage netMsg)
	{
		float num = netMsg.reader.ReadSingle();
		singleton._unpredictedServerFixedTime = num;
		float num2 = Time.time - Time.fixedTime;
		singleton._unpredictedServerFrameTime = num + num2;
	}

	[NetworkMessageHandler(msgType = 64, client = true, server = true)]
	private static void HandleTest(NetworkMessage netMsg)
	{
		int num = netMsg.reader.ReadInt32();
		Debug.LogFormat("Received test packet. value={0}", new object[1] { num });
	}

	public abstract NetworkConnection GetClient(UserID clientId);

	public virtual void InitPlatformServer()
	{
	}

	public override void OnStartServer()
	{
		((NetworkManager)this).OnStartServer();
		NetworkMessageHandlerAttribute.RegisterServerMessages();
		InitializeTime();
		serverNetworkSessionInstance = Object.Instantiate<GameObject>(RoR2Application.instance.networkSessionPrefab);
		InitPlatformServer();
		NetworkManagerSystem.onStartServerGlobal?.Invoke();
	}

	public void FireStartServerGlobalEvent()
	{
		NetworkManagerSystem.onStartServerGlobal?.Invoke();
	}

	public override void OnStopServer()
	{
		((NetworkManager)this).OnStopServer();
	}

	public void FireStopServerGlobalEvent()
	{
		NetworkManagerSystem.onStopServerGlobal?.Invoke();
	}

	public override void OnServerConnect(NetworkConnection conn)
	{
		((NetworkManager)this).OnServerConnect(conn);
	}

	public void FireServerConnectGlobalEvent(NetworkConnection conn)
	{
		NetworkManagerSystem.onServerConnectGlobal?.Invoke(conn);
	}

	public override void OnServerDisconnect(NetworkConnection conn)
	{
		((NetworkManager)this).OnServerDisconnect(conn);
	}

	public void FireServerDisconnectGlobalEvent(NetworkConnection conn)
	{
		NetworkManagerSystem.onServerDisconnectGlobal?.Invoke(conn);
	}

	public abstract void ServerHandleClientDisconnect(NetworkConnection conn);

	public abstract void ServerBanClient(NetworkConnection conn);

	public void ServerKickClient(NetworkConnection conn, BaseKickReason reason)
	{
		Debug.LogFormat("Kicking client on connection {0}: Reason {1}", new object[2] { conn.connectionId, reason });
		conn.SendByChannel((short)67, (MessageBase)(object)new KickMessage(reason), QosChannelIndex.defaultReliable.intVal);
		conn.FlushChannels();
		KickClient(conn, reason);
	}

	protected abstract void KickClient(NetworkConnection conn, BaseKickReason reason);

	public override void OnServerAddPlayer(NetworkConnection conn, short playerControllerId)
	{
		((NetworkManager)this).OnServerAddPlayer(conn, playerControllerId, (NetworkReader)null);
	}

	public override void OnServerAddPlayer(NetworkConnection conn, short playerControllerId, NetworkReader extraMessageReader)
	{
		OnServerAddPlayerInternal(conn, playerControllerId, extraMessageReader);
	}

	private void OnServerAddPlayerInternal(NetworkConnection conn, short playerControllerId, NetworkReader extraMessageReader)
	{
		//IL_00dd: Unknown result type (might be due to invalid IL or missing references)
		//IL_00e3: Unknown result type (might be due to invalid IL or missing references)
		//IL_00f6: Unknown result type (might be due to invalid IL or missing references)
		//IL_00fb: Unknown result type (might be due to invalid IL or missing references)
		if ((Object)(object)((NetworkManager)this).playerPrefab == (Object)null)
		{
			if (LogFilter.logError)
			{
				Debug.LogError((object)"The PlayerPrefab is empty on the NetworkManager. Please setup a PlayerPrefab object.");
			}
			return;
		}
		if ((Object)(object)((NetworkManager)this).playerPrefab.GetComponent<NetworkIdentity>() == (Object)null)
		{
			if (LogFilter.logError)
			{
				Debug.LogError((object)"The PlayerPrefab does not have a NetworkIdentity. Please add a NetworkIdentity to the player prefab.");
			}
			return;
		}
		if (playerControllerId < conn.playerControllers.Count && conn.playerControllers[playerControllerId].IsValid && (Object)(object)conn.playerControllers[playerControllerId].gameObject != (Object)null)
		{
			if (LogFilter.logError)
			{
				Debug.LogError((object)"There is already a player at that playerControllerId for this connections.");
			}
			return;
		}
		if (NetworkUser.readOnlyInstancesList.Count >= ((NetworkManager)this).maxConnections)
		{
			if (LogFilter.logError)
			{
				Debug.LogError((object)"Cannot add any more players.)");
			}
			return;
		}
		if (extraMessageReader == null)
		{
			extraMessageReader = DefaultNetworkReader;
		}
		AddPlayerMessage message = extraMessageReader.ReadMessage<AddPlayerMessage>();
		Transform startPosition = ((NetworkManager)this).GetStartPosition();
		GameObject val = ((!((Object)(object)startPosition != (Object)null)) ? Object.Instantiate<GameObject>(((NetworkManager)this).playerPrefab, Vector3.zero, Quaternion.identity) : Object.Instantiate<GameObject>(((NetworkManager)this).playerPrefab, startPosition.position, startPosition.rotation));
		Debug.LogFormat("NetworkManagerSystem.AddPlayerInternal(conn={0}, playerControllerId={1}, extraMessageReader={2}", new object[3] { conn, playerControllerId, extraMessageReader });
		NetworkUser component = val.GetComponent<NetworkUser>();
		Util.ConnectionIsLocal(conn);
		component.id = AddPlayerIdFromPlatform(conn, message, (byte)playerControllerId);
		Chat.SendPlayerConnectedMessage(component);
		NetworkServer.AddPlayerForConnection(conn, val, playerControllerId);
	}

	protected abstract NetworkUserId AddPlayerIdFromPlatform(NetworkConnection conn, AddPlayerMessage message, byte playerControllerId);

	protected abstract void UpdateServer();

	private void FixedUpdateServer()
	{
		//IL_0027: Unknown result type (might be due to invalid IL or missing references)
		//IL_002d: Expected O, but got Unknown
		if (!NetworkServer.active)
		{
			return;
		}
		timeTransmitTimer -= Time.fixedDeltaTime;
		if (timeTransmitTimer <= 0f)
		{
			NetworkWriter val = new NetworkWriter();
			val.StartMessage((short)54);
			val.Write(unpredictedServerFixedTime);
			val.FinishMessage();
			NetworkServer.SendWriterToReady((GameObject)null, val, QosChannelIndex.time.intVal);
			timeTransmitTimer += svTimeTransmitInterval.value;
		}
		foreach (NetworkConnection connection in NetworkServer.connections)
		{
			if (connection != null && !Util.ConnectionIsLocal(connection))
			{
				RttManager.Ping(connection, QosChannelIndex.ping.intVal);
			}
		}
	}

	public override void OnServerSceneChanged(string sceneName)
	{
		((NetworkManager)this).OnServerSceneChanged(sceneName);
		if (Object.op_Implicit((Object)(object)Run.instance))
		{
			Run.instance.OnServerSceneChanged(sceneName);
		}
		NetworkManagerSystem.onServerSceneChangedGlobal?.Invoke(sceneName);
		while (clientsReadyDuringLevelTransition.Count > 0)
		{
			NetworkConnection val = clientsReadyDuringLevelTransition.Dequeue();
			try
			{
				if (val.isConnected)
				{
					((NetworkManager)this).OnServerReady(val);
				}
			}
			catch (Exception ex)
			{
				Debug.LogErrorFormat("OnServerReady could not be called for client: {0}", new object[1] { ex.Message });
			}
		}
	}

	protected bool IsServerAtMaxConnections()
	{
		ReadOnlyCollection<NetworkConnection> connections = NetworkServer.connections;
		if (connections.Count >= ((NetworkManager)this).maxConnections)
		{
			int num = 0;
			for (int i = 0; i < connections.Count; i++)
			{
				if (connections[i] != null)
				{
					num++;
				}
			}
			return num >= ((NetworkManager)this).maxConnections;
		}
		return false;
	}

	private NetworkUser FindNetworkUserForConnectionServer(NetworkConnection connection)
	{
		ReadOnlyCollection<NetworkUser> readOnlyInstancesList = NetworkUser.readOnlyInstancesList;
		int count = readOnlyInstancesList.Count;
		for (int i = 0; i < count; i++)
		{
			NetworkUser networkUser = readOnlyInstancesList[i];
			if (((NetworkBehaviour)networkUser).connectionToClient == connection)
			{
				return networkUser;
			}
		}
		return null;
	}

	public int GetConnectingClientCount()
	{
		int num = 0;
		ReadOnlyCollection<NetworkConnection> connections = NetworkServer.connections;
		int count = connections.Count;
		for (int i = 0; i < count; i++)
		{
			NetworkConnection val = connections[i];
			if (val != null && !Object.op_Implicit((Object)(object)FindNetworkUserForConnectionServer(val)))
			{
				num++;
			}
		}
		return num;
	}

	public void RequestServerShutdown()
	{
		if (!serverShuttingDown)
		{
			serverShuttingDown = true;
			((MonoBehaviour)this).StartCoroutine(ServerShutdownCoroutine());
		}
	}

	private IEnumerator ServerShutdownCoroutine()
	{
		Debug.Log((object)"Server shutting down...");
		ReadOnlyCollection<NetworkConnection> connections = NetworkServer.connections;
		for (int num = connections.Count - 1; num >= 0; num--)
		{
			NetworkConnection val = connections[num];
			if (val != null && !Util.ConnectionIsLocal(val))
			{
				ServerKickClient(val, new SimpleLocalizedKickReason("KICK_REASON_SERVERSHUTDOWN"));
			}
		}
		Debug.Log((object)"Issued kick message to all remote clients.");
		float maxWait = 0.2f;
		for (float t = 0f; t < maxWait; t += Time.unscaledDeltaTime)
		{
			if (CheckConnectionsEmpty())
			{
				break;
			}
			yield return (object)new WaitForEndOfFrame();
		}
		Debug.Log((object)"Finished waiting for clients to disconnect.");
		if (base.client != null)
		{
			Debug.Log((object)"StopHost()");
			((NetworkManager)this).StopHost();
		}
		else
		{
			Debug.Log((object)"StopServer()");
			((NetworkManager)this).StopServer();
		}
		serverShuttingDown = false;
		Debug.Log((object)"Server shutdown complete.");
		static bool CheckConnectionsEmpty()
		{
			foreach (NetworkConnection connection in NetworkServer.connections)
			{
				if (connection != null && !Util.ConnectionIsLocal(connection))
				{
					return false;
				}
			}
			return true;
		}
	}

	private static void ServerHandleReady(NetworkMessage netMsg)
	{
		if (isLoadingScene)
		{
			clientsReadyDuringLevelTransition.Enqueue(netMsg.conn);
			Debug.Log((object)"Client readied during a level transition! Queuing their request.");
		}
		else
		{
			((NetworkManager)singleton).OnServerReady(netMsg.conn);
			Debug.Log((object)"Client ready.");
		}
	}

	private void RegisterServerOverrideMessages()
	{
		//IL_0009: Unknown result type (might be due to invalid IL or missing references)
		//IL_0013: Expected O, but got Unknown
		NetworkServer.RegisterHandler((short)35, new NetworkMessageDelegate(ServerHandleReady));
	}

	public override void ServerChangeScene(string newSceneName)
	{
		RegisterServerOverrideMessages();
		((NetworkManager)this).ServerChangeScene(newSceneName);
	}

	private static bool IsAddressablesKeyValid(string key, Type type)
	{
		IList<IResourceLocation> list = default(IList<IResourceLocation>);
		foreach (IResourceLocator resourceLocator in Addressables.ResourceLocators)
		{
			if (resourceLocator.Locate((object)key, typeof(SceneInstance), ref list) && list.Count > 0)
			{
				return true;
			}
		}
		return false;
	}

	protected override IChangeSceneAsyncOperation ChangeSceneImplementation(string newSceneName)
	{
		IChangeSceneAsyncOperation val = null;
		StringBuilder stringBuilder = StringBuilderPool.RentStringBuilder();
		try
		{
			SceneDef sceneDef = SceneCatalog.FindSceneDef(newSceneName);
			if (Object.op_Implicit((Object)(object)sceneDef))
			{
				AssetReferenceScene sceneAddress = sceneDef.sceneAddress;
				string text = ((sceneAddress != null) ? ((AssetReference)sceneAddress).AssetGUID : null);
				if (!string.IsNullOrEmpty(text))
				{
					if (IsAddressablesKeyValid(text, typeof(SceneInstance)))
					{
						val = (IChangeSceneAsyncOperation)(object)new AddressablesChangeSceneAsyncOperation(text, (LoadSceneMode)0, activateOnLoad: false);
					}
					else
					{
						stringBuilder.AppendLine("Scene address is invalid. sceneName=\"").Append(newSceneName).Append("\" sceneAddress=\"")
							.Append(text)
							.Append("\"")
							.AppendLine();
					}
				}
			}
			if (val == null)
			{
				val = ((NetworkManager)this).ChangeSceneImplementation(newSceneName);
				if (val == null)
				{
					stringBuilder.Append("SceneManager.LoadSceneAsync(\"").Append(newSceneName).Append("\" failed.")
						.AppendLine();
				}
			}
			return val;
		}
		finally
		{
			if (val == null)
			{
				Debug.LogError((object)stringBuilder.ToString());
			}
			StringBuilderPool.ReturnStringBuilder(stringBuilder);
		}
	}
}
