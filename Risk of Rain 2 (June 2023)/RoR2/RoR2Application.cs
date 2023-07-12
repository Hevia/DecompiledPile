using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using Facepunch.Steamworks;
using HG;
using JetBrains.Annotations;
using Rewired;
using RoR2.ContentManagement;
using RoR2.ConVar;
using RoR2.Modding;
using RoR2.UI;
using TMPro;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.Networking;
using UnityEngine.Rendering.PostProcessing;
using UnityEngine.SceneManagement;
using Zio;
using Zio.FileSystems;

namespace RoR2;

public class RoR2Application : MonoBehaviour
{
	[SerializeField]
	[HideInInspector]
	private bool loaded;

	public static readonly string messageForModders = "We don't officially support modding at this time but if you're going to mod the game please change this value to true if you're modding the game. This will disable some things like Prismatic Trials and put players into a separate matchmaking queue from vanilla users to protect their game experience.";

	public static bool isModded = false;

	public GameObject networkManagerPrefab;

	public GameObject platformManagerPrefab;

	public GameObject networkSessionPrefab;

	public PostProcessVolume postProcessSettingsController;

	public Canvas mainCanvas;

	public Stopwatch stopwatch = new Stopwatch();

	public const string gameName = "Risk of Rain 2";

	private const uint ror1AppId = 248820u;

	public const uint ror2AppId = 632360u;

	private const uint ror2DedicatedServerAppId = 1180760u;

	public const bool isDedicatedServer = false;

	public const uint appId = 632360u;

	public static string steamBuildId = "STEAM_UNINITIALIZED";

	private static string buildId;

	public static readonly int hardMaxPlayers = 16;

	public static readonly int maxPlayers = 4;

	public static readonly int maxLocalPlayers = 4;

	private static IntConVar waitMsConVar = new IntConVar("wait_ms", ConVarFlags.None, "-1", "How many milliseconds to sleep between each frame. -1 for no sleeping between frames.");

	public static readonly TimerQueue timeTimers = new TimerQueue();

	public static readonly TimerQueue fixedTimeTimers = new TimerQueue();

	public static readonly TimerQueue unscaledTimeTimers = new TimerQueue();

	public static FileSystem cloudStorage;

	public static Func<bool> loadSteamworksClient;

	public static Action unloadSteamworksClient;

	public static Action onLoad;

	public static Action onStart;

	public static Action onShutDown;

	public static readonly Xoroshiro128Plus rng = new Xoroshiro128Plus((ulong)DateTime.UtcNow.Ticks);

	public bool IsFullyLoaded => loaded;

	public Client steamworksClient { get; private set; }

	public static RoR2Application instance { get; private set; }

	public static FileSystem fileSystem { get; private set; }

	public static bool loadFinished { get; private set; } = false;


	public static bool loadStarted { get; private set; } = false;


	public static bool isInSinglePlayer
	{
		get
		{
			if (NetworkServer.active && NetworkServer.dontListen)
			{
				return LocalUserManager.readOnlyLocalUsersList.Count == 1;
			}
			return false;
		}
	}

	public static bool isInMultiPlayer
	{
		get
		{
			if (NetworkClient.active)
			{
				if (NetworkServer.active && NetworkServer.dontListen)
				{
					return LocalUserManager.readOnlyLocalUsersList.Count != 1;
				}
				return true;
			}
			return false;
		}
	}

	public static event Action onUpdate;

	public static event Action onFixedUpdate;

	public static event Action onLateUpdate;

	public static event Action onNextUpdate;

	private static void AssignBuildId()
	{
		buildId = Application.version;
	}

	public static string GetBuildId()
	{
		return buildId;
	}

	private void Awake()
	{
		if (maxPlayers != 4 || (Application.genuineCheckAvailable && !Application.genuine))
		{
			isModded = true;
		}
		stopwatch.Start();
		Object.DontDestroyOnLoad((Object)(object)((Component)this).gameObject);
		if (Object.op_Implicit((Object)(object)instance))
		{
			Object.Destroy((Object)(object)((Component)this).gameObject);
			return;
		}
		instance = this;
		AssignBuildId();
		Debug.Log((object)("buildId = " + buildId));
		if (!loadStarted)
		{
			loadStarted = true;
			((MonoBehaviour)this).StartCoroutine(OnLoad());
		}
	}

	private void Start()
	{
		if ((Object)(object)instance == (Object)(object)this && onStart != null)
		{
			onStart();
			onStart = null;
		}
	}

	private void Update()
	{
		if (waitMsConVar.value >= 0)
		{
			Thread.Sleep(waitMsConVar.value);
		}
		if (!Application.isBatchMode && Object.op_Implicit((Object)(object)MPEventSystemManager.kbmEventSystem))
		{
			Cursor.lockState = (CursorLockMode)((!MPEventSystemManager.kbmEventSystem.isCursorVisible && !MPEventSystemManager.combinedEventSystem.isCursorVisible) ? 1 : 2);
			Cursor.visible = false;
		}
		RoR2Application.onUpdate?.Invoke();
		Interlocked.Exchange(ref RoR2Application.onNextUpdate, null)?.Invoke();
		timeTimers.Update(Time.deltaTime);
		unscaledTimeTimers.Update(Time.unscaledDeltaTime);
	}

	private void FixedUpdate()
	{
		RoR2Application.onFixedUpdate?.Invoke();
		fixedTimeTimers.Update(Time.fixedDeltaTime);
	}

	private void LateUpdate()
	{
		RoR2Application.onLateUpdate?.Invoke();
	}

	private IEnumerator OnLoad()
	{
		return InitializeGameRoutine();
	}

	private IEnumerator InitializeGameRoutine()
	{
		loadStarted = true;
		UnitySystemConsoleRedirector.Redirect();
		SceneManager.sceneLoaded += delegate(Scene scene, LoadSceneMode loadSceneMode)
		{
			//IL_0017: Unknown result type (might be due to invalid IL or missing references)
			Debug.LogFormat("Loaded scene {0} loadSceneMode={1}", new object[2]
			{
				((Scene)(ref scene)).name,
				loadSceneMode
			});
		};
		SceneManager.sceneUnloaded += delegate(Scene scene)
		{
			Debug.LogFormat("Unloaded scene {0}", new object[1] { ((Scene)(ref scene)).name });
		};
		SceneManager.activeSceneChanged += delegate(Scene oldScene, Scene newScene)
		{
			Debug.LogFormat("Active scene changed from {0} to {1}", new object[2]
			{
				((Scene)(ref oldScene)).name,
				((Scene)(ref newScene)).name
			});
		};
		WwiseIntegrationManager.Init();
		Scene activeScene;
		while (true)
		{
			activeScene = SceneManager.GetActiveScene();
			if (!(((Scene)(ref activeScene)).name != "loadingbasic"))
			{
				break;
			}
			yield return (object)new WaitForEndOfFrame();
		}
		yield return (object)new WaitForEndOfFrame();
		PhysicalFileSystem val = new PhysicalFileSystem();
		SubFileSystem val2 = new SubFileSystem((IFileSystem)(object)val, ((FileSystem)val).ConvertPathFromInternal(Application.dataPath), true);
		Debug.Log((object)("application data path is" + Application.dataPath));
		fileSystem = (FileSystem)val2;
		cloudStorage = fileSystem;
		if (!(loadSteamworksClient?.Invoke() ?? false))
		{
			Application.Quit();
			yield break;
		}
		RewiredIntegrationManager.Init();
		Object.Instantiate<GameObject>(LegacyResourcesAPI.Load<GameObject>("Prefabs/UI/MPEventSystemManager"));
		PlatformSystems.InitNetworkManagerSystem(Object.Instantiate<GameObject>(networkManagerPrefab));
		if ((Object)(object)platformManagerPrefab != (Object)null)
		{
			Object.Instantiate<GameObject>(platformManagerPrefab);
		}
		PlatformSystems.InitPlatformManagerObject(((Component)this).gameObject);
		if (PlatformSystems.textDataManager != null)
		{
			while (!PlatformSystems.textDataManager.InitializedConfigFiles)
			{
				Debug.Log((object)"Text config stuff still happening...");
				yield return null;
			}
		}
		GameObject val3 = new GameObject("Console");
		val3.AddComponent<SetDontDestroyOnLoad>();
		val3.AddComponent<Console>();
		yield return LoadGameContent();
		yield return (object)new WaitForEndOfFrame();
		Language.collectLanguageRootFolders += delegate(List<string> list)
		{
			list.Add(System.IO.Path.Combine(Application.streamingAssetsPath, "Language"));
		};
		Language.Init();
		SystemInitializerAttribute.Execute();
		LocalUserManager.Init();
		PlatformSystems.saveSystem.LoadUserProfiles();
		if (onLoad != null)
		{
			onLoad();
			onLoad = null;
		}
		bool hasStartupError = false;
		try
		{
			AkSoundEngine.IsInitialized();
		}
		catch (DllNotFoundException)
		{
			IssueStartupError(new SimpleDialogBox.TokenParamsPair("STARTUP_FAILURE_DIALOG_TITLE"), new SimpleDialogBox.TokenParamsPair("MSVCR_2015_BAD_INSTALL_DIALOG_BODY"), new(string, Action)[1] { ("MSVCR_2015_BAD_INSTALL_DIALOG_DOWNLOAD_BUTTON", OpenVCR2015DownloadPage) });
		}
		if (!hasStartupError)
		{
			while ((Object)(object)Console.instance == (Object)null)
			{
				Debug.Log((object)"Console not initialized yet...");
				yield return null;
			}
			((NetworkManager)PlatformSystems.networkManager).ServerChangeScene("splash");
		}
		while (true)
		{
			activeScene = SceneManager.GetActiveScene();
			if (!(((Scene)(ref activeScene)).name == "loadingbasic"))
			{
				break;
			}
			yield return (object)new WaitForEndOfFrame();
		}
		loadStarted = false;
		loadFinished = true;
		void IssueStartupError(SimpleDialogBox.TokenParamsPair headerToken, SimpleDialogBox.TokenParamsPair descriptionToken, (string token, Action action)[] buttons)
		{
			//IL_009f: Unknown result type (might be due to invalid IL or missing references)
			//IL_00b5: Expected O, but got Unknown
			if (!hasStartupError)
			{
				hasStartupError = true;
				LocalUserManager.ClearUsers();
				NetworkManager.singleton.ServerChangeScene("title");
				SimpleDialogBox simpleDialogBox = SimpleDialogBox.Create();
				simpleDialogBox.headerToken = headerToken;
				simpleDialogBox.descriptionToken = descriptionToken;
				simpleDialogBox.rootObject.transform.SetParent(((Component)mainCanvas).transform);
				OnDestroyCallback.AddCallback(simpleDialogBox.rootObject, delegate
				{
					Console.instance.SubmitCmd(null, "quit");
				});
				if (buttons != null)
				{
					for (int i = 0; i < buttons.Length; i++)
					{
						(string, Action) tuple = buttons[i];
						simpleDialogBox.AddActionButton(new UnityAction(tuple.Item2.Invoke), tuple.Item1, true);
					}
				}
				simpleDialogBox.AddCancelButton("PAUSE_QUIT_TO_DESKTOP");
			}
		}
		static void OpenVCR2015DownloadPage()
		{
			Application.OpenURL("https://www.microsoft.com/en-us/download/details.aspx?id=48145");
		}
	}

	private IEnumerator LoadGameContent()
	{
		TMP_Text loadingPercentIndicatorLabel = (from label in Object.FindObjectsOfType<TMP_Text>()
			where ((Object)label).name.Equals("LoadingPercentIndicator", StringComparison.Ordinal)
			select label).FirstOrDefault();
		Animation loadingPercentAnimation = (Object.op_Implicit((Object)(object)loadingPercentIndicatorLabel) ? ((Component)loadingPercentIndicatorLabel).GetComponentInChildren<Animation>() : null);
		StringBuilder loadingTextStringBuilder = new StringBuilder();
		TimeSpan maxLoadTimePerFrame = TimeSpan.FromSeconds(1.0 / 120.0);
		ContentManager.collectContentPackProviders += delegate(ContentManager.AddContentPackProviderDelegate addContentPackProvider)
		{
			addContentPackProvider(new RoR2Content());
			addContentPackProvider(new JunkContent());
			addContentPackProvider(new DLC1Content());
			foreach (IContentPackProvider item3 in from mod in ModLoader.instance.allEnabledModsReadOnly
				select mod.GetGameData<RoR2Mod>().contentPackProvider into contentPackProvider
				where contentPackProvider != null
				select contentPackProvider)
			{
				addContentPackProvider(item3);
			}
		};
		ReadableProgress<float> contentLoadProgressReceiver = new ReadableProgress<float>();
		IEnumerator item = ContentManager.LoadContentPacks((IProgress<float>)contentLoadProgressReceiver);
		Stopwatch totalLoadStopwatch = new Stopwatch();
		totalLoadStopwatch.Start();
		Stopwatch thisFrameLoadStopwatch = new Stopwatch();
		int previousProgressPercent = 0;
		Stack<IEnumerator> coroutineStack = new Stack<IEnumerator>();
		coroutineStack.Push(item);
		while (coroutineStack.Count > 0)
		{
			thisFrameLoadStopwatch.Restart();
			int num = 0;
			do
			{
				num++;
				IEnumerator enumerator = coroutineStack.Peek();
				if (enumerator.Current is IEnumerator item2)
				{
					coroutineStack.Push(item2);
					continue;
				}
				while (!enumerator.MoveNext())
				{
					coroutineStack.Pop();
					if (coroutineStack.Count == 0)
					{
						break;
					}
					enumerator = coroutineStack.Peek();
				}
			}
			while (coroutineStack.Count > 0 && thisFrameLoadStopwatch.Elapsed < maxLoadTimePerFrame);
			thisFrameLoadStopwatch.Stop();
			int num2 = Mathf.FloorToInt(contentLoadProgressReceiver.value * 100f);
			if (previousProgressPercent != num2)
			{
				previousProgressPercent = num2;
				loadingTextStringBuilder.Clear();
				loadingTextStringBuilder.AppendInt(num2);
				loadingTextStringBuilder.Append("%");
				if (Object.op_Implicit((Object)(object)loadingPercentIndicatorLabel))
				{
					loadingPercentIndicatorLabel.SetText(loadingTextStringBuilder);
				}
			}
			if (Object.op_Implicit((Object)(object)loadingPercentAnimation))
			{
				AnimationClip clip = loadingPercentAnimation.clip;
				clip.SampleAnimation(((Component)loadingPercentAnimation).gameObject, contentLoadProgressReceiver.value * 0.99f * clip.length);
			}
			yield return (object)new WaitForEndOfFrame();
		}
		Console.instance.SubmitCmd(null, IntroCutsceneController.shouldSkip ? "set_scene title" : "set_scene intro");
		yield return null;
		Debug.LogFormat("Game content load completed in {0}ms.", new object[1] { totalLoadStopwatch.ElapsedMilliseconds });
	}

	private void OnDestroy()
	{
		if ((Object)(object)instance == (Object)(object)this && PlatformSystems.EgsToggleConVar.value != 1)
		{
			unloadSteamworksClient?.Invoke();
		}
	}

	private void OnApplicationQuit()
	{
		onShutDown?.Invoke();
		if (Object.op_Implicit((Object)(object)Console.instance))
		{
			Console.instance.SaveArchiveConVars();
		}
		unloadSteamworksClient?.Invoke();
		UnitySystemConsoleRedirector.Disengage();
	}

	[ConCommand(commandName = "quit", flags = ConVarFlags.None, helpText = "Close the application.")]
	private static void CCQuit(ConCommandArgs args)
	{
		Application.Quit();
	}

	[NotNull]
	public static string GetBestUserName()
	{
		string text = null;
		object obj = text;
		if (obj == null)
		{
			Client obj2 = Client.Instance;
			obj = ((obj2 != null) ? obj2.Username : null);
		}
		text = (string)obj;
		if (LocalUserManager.readOnlyLocalUsersList.Count > 0)
		{
			text = text ?? LocalUserManager.readOnlyLocalUsersList[0].userProfile.name;
		}
		return text ?? "???";
	}

	[ConCommand(commandName = "app_info", flags = ConVarFlags.None, helpText = "Get information about the application, including build and version info.")]
	private static void CCAppInfo(ConCommandArgs args)
	{
		//IL_009c: Unknown result type (might be due to invalid IL or missing references)
		StringBuilder stringBuilder = StringBuilderPool.RentStringBuilder();
		stringBuilder.Append("====================").AppendLine();
		stringBuilder.Append("= Application Info =").AppendLine();
		stringBuilder.Append("====================").AppendLine();
		stringBuilder.Append("> Product: ").AppendLine();
		stringBuilder.Append(">   Product Name: ").Append(Application.productName).AppendLine();
		stringBuilder.Append(">   Company Name: ").Append(Application.companyName).AppendLine();
		stringBuilder.Append("> Build: ").AppendLine();
		stringBuilder.Append(">   Platform: ").Append(Application.platform).AppendLine();
		stringBuilder.Append(">   Version: ").Append(Application.version).AppendLine();
		stringBuilder.Append(">   Unity Version: ").Append(Application.unityVersion).AppendLine();
		stringBuilder.Append(">   Build GUID: ").Append(Application.buildGUID).AppendLine();
		stringBuilder.Append(">   Build Type: ").Append("Production").AppendLine();
		stringBuilder.Append(">   Is Dedicated Server: ").Append(value: false).AppendLine();
		stringBuilder.Append("> Environment: ").AppendLine();
		stringBuilder.Append(">   Command Line: ").Append(Environment.CommandLine).AppendLine();
		stringBuilder.Append(">   Console Log Path: ").Append(Application.consoleLogPath).AppendLine();
		Client val = Client.Instance;
		if (val != null && ((BaseSteamworks)val).IsValid)
		{
			stringBuilder.Append("> Steamworks Client: ").AppendLine();
			stringBuilder.Append(">   App ID: ").AppendUint(((BaseSteamworks)val).AppId).AppendLine();
			stringBuilder.Append(">   Build ID: ").AppendInt(val.BuildId).AppendLine();
			stringBuilder.Append(">   Branch: ").Append(val.BetaName).AppendLine();
		}
		Server val2 = Server.Instance;
		if (val2 != null && ((BaseSteamworks)val2).IsValid)
		{
			stringBuilder.Append("> Steamworks Game Server: ").AppendLine();
			stringBuilder.Append(">   App ID: ").AppendUint(((BaseSteamworks)val2).AppId).AppendLine();
		}
		args.Log(stringBuilder.ToString());
		stringBuilder = StringBuilderPool.ReturnStringBuilder(stringBuilder);
	}

	public static void LoadRewiredUIMap()
	{
		//IL_0053: Unknown result type (might be due to invalid IL or missing references)
		//IL_0077: Unknown result type (might be due to invalid IL or missing references)
		foreach (Player player in ReInput.players.Players)
		{
			player.controllers.maps.ClearAllMaps(false);
			foreach (Controller controller in player.controllers.Controllers)
			{
				try
				{
					player.controllers.maps.LoadMap(controller.type, controller.id, 2, 0);
				}
				catch (FormatException ex)
				{
					Debug.LogWarning((object)$"Excepting loading controller mapping (type:{controller.type},id:{controller.id}) for player (name:{player.name},id:{player.id}).");
					Debug.LogException((Exception)ex);
				}
			}
			player.controllers.maps.SetAllMapsEnabled(true);
		}
	}

	public static void DebugPrintRewired()
	{
		//IL_00b9: Unknown result type (might be due to invalid IL or missing references)
		//IL_01bd: Unknown result type (might be due to invalid IL or missing references)
		Debug.Log((object)"---Rewired Start---");
		Debug.Log((object)("Rewired found " + ReInput.controllers.joystickCount + " joysticks attached."));
		for (int i = 0; i < ReInput.controllers.joystickCount; i++)
		{
			Joystick val = ReInput.controllers.Joysticks[i];
			Debug.Log((object)("[" + i + "] Joystick: " + ((Controller)val).name + "\nHardware Name: " + ((Controller)val).hardwareName + "\nIs Recognized: " + ((((Controller)val).hardwareTypeGuid != Guid.Empty) ? "Yes" : "No") + "\nIs Assigned: " + (ReInput.controllers.IsControllerAssigned(((Controller)val).type, (Controller)(object)val) ? "Yes" : "No")));
		}
		foreach (Player player in ReInput.players.Players)
		{
			Debug.Log((object)("PlayerId = " + player.id + " is assigned " + player.controllers.joystickCount + " joysticks."));
			foreach (Joystick joystick in player.controllers.Joysticks)
			{
				Debug.Log((object)("Joystick: " + ((Controller)joystick).name + "\nIs Recognized: " + ((((Controller)joystick).hardwareTypeGuid != Guid.Empty) ? "Yes" : "No")));
				foreach (ControllerMap map in player.controllers.maps.GetMaps(((Controller)joystick).type, ((Controller)joystick).id))
				{
					string name = ((InputCategory)ReInput.mapping.GetMapCategory(map.categoryId)).name;
					string name2 = ReInput.mapping.GetJoystickLayout(map.layoutId).name;
					Debug.Log((object)("Controller Map:\nCategory = " + name + "\nLayout = " + name2 + "\nenabled = " + map.enabled));
					ActionElementMap[] elementMaps = map.GetElementMaps();
					foreach (ActionElementMap val2 in elementMaps)
					{
						InputAction action = ReInput.mapping.GetAction(val2.actionId);
						if (action != null)
						{
							Debug.Log((object)("Action \"" + action.name + "\" is bound to \"" + val2.elementIdentifierName + "\""));
						}
					}
				}
			}
		}
		Debug.Log((object)"---Rewired End---");
	}

	private static void AssignJoystickToAvailablePlayer(Controller controller)
	{
		IList<Player> players = ReInput.players.Players;
		for (int i = 0; i < players.Count; i++)
		{
			Player val = players[i];
			if (val.name != "PlayerMain" && val.controllers.joystickCount == 0 && !val.controllers.hasKeyboard && !val.controllers.hasMouse)
			{
				val.controllers.AddController(controller, false);
				break;
			}
		}
	}

	private static void AssignNewController(ControllerStatusChangedEventArgs args)
	{
		//IL_0006: Unknown result type (might be due to invalid IL or missing references)
		AssignNewController(ReInput.controllers.GetController(args.controllerType, args.controllerId));
	}

	public static void ClearControllers()
	{
		ReInput.players.GetPlayer("PlayerMain").controllers.ClearAllControllers();
	}

	private static void AssignNewController(Controller controller)
	{
		//IL_001c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0022: Invalid comparison between Unknown and I4
		ReInput.players.GetPlayer("PlayerMain").controllers.AddController(controller, false);
		if ((int)controller.type == 2)
		{
			AssignJoystickToAvailablePlayer(controller);
		}
	}
}
