using System;
using System.Linq;
using RoR2.ConVar;
using RoR2.EntitlementManagement;
using RoR2.Networking;
using UnityEngine;

namespace RoR2;

public static class PlatformSystems
{
	public static SaveSystem saveSystem;

	public static UserManager userManager;

	public static AchievementSystem achievementSystem;

	public static LobbyManager lobbyManager;

	public static TextDataManager textDataManager;

	public static PlatformManager platformManager;

	public static NetworkManagerSystem networkManager;

	public static IUserEntitementsResolverNetworkAndLocal entitlementsSystem;

	public static object statManager;

	public static object friendsManager;

	public static bool crossPlayEnabledOnStartup = false;

	public static PlayerPrefsIntConVar EgsToggleConVar = new PlayerPrefsIntConVar("egsToggle", ConVarFlags.Engine, "0", "If EGS is used. If false, use Steam.");

	public static bool ShouldUseEpicOnlineSystems
	{
		get
		{
			return crossPlayEnabledOnStartup;
		}
		set
		{
		}
	}

	[RuntimeInitializeOnLoadMethod(/*Could not decode attribute arguments.*/)]
	private static void Init()
	{
		SteamworksClientManager.Init();
		string[] commandLineArgs = Environment.GetCommandLineArgs();
		if (commandLineArgs.Contains("--disableCrossplay"))
		{
			EgsToggleConVar.value = 0;
		}
		else if (commandLineArgs.Contains("--enableCrossplay"))
		{
			EgsToggleConVar.value = 1;
		}
		if (EgsToggleConVar.value == 1)
		{
			try
			{
				crossPlayEnabledOnStartup = true;
				platformManager = new EOSPlatformManager();
				userManager = new UserManagerEOS();
				(userManager as UserManagerEOS).InitializeUserManager();
				textDataManager = new StreamingAssetsTextDataManager();
				lobbyManager = new EOSLobbyManager();
				(lobbyManager as EOSLobbyManager).Init();
				new EOSLoginManager().TryLogin();
			}
			catch
			{
				EgsToggleConVar.value = 0;
				Application.Quit();
			}
		}
		else
		{
			lobbyManager = new SteamworksLobbyManager();
			userManager = new SteamUserManager();
			textDataManager = new StreamingAssetsTextDataManager();
		}
		SteamworksRichPresenceManager.Init();
		saveSystem = new SaveSystemSteam();
		achievementSystem = new AchievementSystemSteam();
		entitlementsSystem = new SteamworksEntitlementResolver();
		platformManager?.InitializePlatformManager();
		UserProfile.GenerateSaveFieldFunctions();
		RoR2Application.onUpdate += saveSystem.StaticUpdate;
		if (entitlementsSystem != null)
		{
			EntitlementManager.collectLocalUserEntitlementResolvers += delegate(Action<IUserEntitlementResolver<LocalUser>> add)
			{
				add(entitlementsSystem);
			};
			EntitlementManager.collectNetworkUserEntitlementResolvers += delegate(Action<IUserEntitlementResolver<NetworkUser>> add)
			{
				add(entitlementsSystem);
			};
		}
		RoR2Application.onShutDown = (Action)Delegate.Combine(RoR2Application.onShutDown, new Action(Shutdown));
	}

	private static void Shutdown()
	{
		saveSystem.HandleShutDown();
		lobbyManager.Shutdown();
	}

	private static T BuildMonoSingleton<T>() where T : MonoBehaviour
	{
		//IL_0005: Unknown result type (might be due to invalid IL or missing references)
		//IL_000a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0016: Expected O, but got Unknown
		GameObject val = new GameObject("T");
		T result = val.AddComponent<T>();
		Object.DontDestroyOnLoad((Object)val);
		return result;
	}

	public static void InitNetworkManagerSystem(GameObject networkManagerPrefabObject)
	{
		Debug.Log((object)"PlatformSystems:InitNetworkManagerSystem");
		if (!((Object)(object)networkManagerPrefabObject.GetComponent<NetworkManagerSystem>() != (Object)null))
		{
			if (EgsToggleConVar.value == 1)
			{
				networkManager = networkManagerPrefabObject.AddComponent<NetworkManagerSystemEOS>();
			}
			else
			{
				networkManager = networkManagerPrefabObject.AddComponent<NetworkManagerSystemSteam>();
			}
			NetworkManagerConfiguration component = networkManagerPrefabObject.GetComponent<NetworkManagerConfiguration>();
			if ((Object)(object)component == (Object)null || (Object)(object)networkManager == (Object)null)
			{
				Debug.LogError((object)"Missing NetworkManagerConfiguration on NetworkManagerPrefab or platform NetworkManagerSystem not found");
			}
			else
			{
				networkManager.Init(component);
			}
		}
	}

	public static void InitPlatformManagerObject(GameObject platformManagerPrefabObject)
	{
	}
}
