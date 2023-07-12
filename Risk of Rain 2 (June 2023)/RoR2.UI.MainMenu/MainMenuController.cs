using System;
using Epic.OnlineServices;
using RoR2.ConVar;
using RoR2.Networking;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.Networking;
using UnityEngine.UI;

namespace RoR2.UI.MainMenu;

public sealed class MainMenuController : MonoBehaviour
{
	[NonSerialized]
	public BaseMainMenuScreen desiredMenuScreen;

	public BaseMainMenuScreen LoadingScreen;

	public BaseMainMenuScreen EngagementScreen;

	public BaseMainMenuScreen profileMenuScreen;

	public BaseMainMenuScreen EAwarningProfileMenu;

	public BaseMainMenuScreen multiplayerMenuScreen;

	public BaseMainMenuScreen titleMenuScreen;

	public BaseMainMenuScreen settingsMenuScreen;

	public BaseMainMenuScreen moreMenuScreen;

	public BaseMainMenuScreen extraGameModeMenuScreen;

	[HideInInspector]
	public BaseMainMenuScreen currentMenuScreen;

	public static MainMenuController instance;

	public HGButton exitButtonTransition;

	public HGButton profileButtonTransition;

	public HGButton onlineMultiplayerButtonTransition;

	public HGButton localMultiplayerButtonTransition;

	public GameObject steamBuildLabel;

	public HGButton continueButtonTransition;

	public GameObject EA_Panel;

	public GameObject mainMenuButtonPanel;

	public Transform cameraTransform;

	public float camRotationSmoothDampTime;

	public float camTranslationSmoothDampTime;

	private Vector3 camSmoothDampPositionVelocity;

	private Vector3 camSmoothDampRotationVelocity;

	public float camTransitionDuration;

	private float camTransitionTimer;

	private static bool wasInMultiplayer = false;

	private static EOSLoginManager.EOSLoginState lastEOSLoginState = EOSLoginManager.EOSLoginState.None;

	private static bool eaWarningShown = false;

	private static BoolConVar eaMessageSkipConVar = new BoolConVar("ea_message_skip", ConVarFlags.None, "0", "Whether or not to skip the early access splash screen.");

	private bool isInitialized;

	public static bool IsOnMultiplayerScreen { get; set; } = false;


	[SystemInitializer(new Type[] { })]
	private static void Init()
	{
		NetworkManagerSystem.onStartClientGlobal += delegate
		{
			if (!NetworkServer.active || !NetworkServer.dontListen)
			{
				wasInMultiplayer = true;
			}
		};
	}

	private void Awake()
	{
		RoR2Application.onStart = (Action)Delegate.Combine(RoR2Application.onStart, new Action(StartManaged));
	}

	private void Start()
	{
		if ((Object)(object)instance != (Object)null)
		{
			Object.Destroy((Object)(object)((Component)this).gameObject);
			return;
		}
		instance = this;
		StartWaitForLoad();
		wasInMultiplayer = false;
		if (Object.op_Implicit((Object)(object)LoadingScreen))
		{
			((Component)LoadingScreen).gameObject.SetActive(true);
		}
		if (Object.op_Implicit((Object)(object)EngagementScreen))
		{
			((Component)EngagementScreen).gameObject.SetActive(false);
		}
		((Component)titleMenuScreen).gameObject.SetActive(false);
		((Component)multiplayerMenuScreen).gameObject.SetActive(false);
		((Component)settingsMenuScreen).gameObject.SetActive(false);
		((Component)moreMenuScreen).gameObject.SetActive(false);
		((Component)extraGameModeMenuScreen).gameObject.SetActive(false);
		desiredMenuScreen = (wasInMultiplayer ? multiplayerMenuScreen : titleMenuScreen);
		StartManaged();
	}

	private void StartManaged()
	{
		//IL_0061: Unknown result type (might be due to invalid IL or missing references)
		isInitialized = true;
		CheckWarningScreen();
		currentMenuScreen = desiredMenuScreen;
		if (!((Component)currentMenuScreen).gameObject.activeInHierarchy)
		{
			((Component)currentMenuScreen).gameObject.SetActive(true);
		}
		if ((Object)(object)currentMenuScreen.desiredCameraTransform != (Object)null)
		{
			cameraTransform.rotation = currentMenuScreen.desiredCameraTransform.rotation;
		}
		if (Object.op_Implicit((Object)(object)currentMenuScreen))
		{
			currentMenuScreen.OnEnter(this);
		}
		IsOnMultiplayerScreen = (Object)(object)currentMenuScreen == (Object)(object)multiplayerMenuScreen;
	}

	private bool CheckWarningScreen()
	{
		return false;
	}

	private static bool IsMainUserSignedIn()
	{
		return LocalUserManager.FindLocalUser(0) != null;
	}

	private bool IsInLobby()
	{
		return PlatformSystems.lobbyManager.isInLobby;
	}

	private void Update()
	{
		if (isInitialized)
		{
			if ((Object)(object)currentMenuScreen == (Object)(object)multiplayerMenuScreen && PlayerPrefs.GetInt("HasShownFirstTimePopup", 0) == 0)
			{
				PlayerPrefs.SetInt("HasShownFirstTimePopup", 1);
				ShowFirstTimeCrossPlayPopup();
			}
			if (IsInLobby() && (Object)(object)currentMenuScreen != (Object)(object)multiplayerMenuScreen)
			{
				desiredMenuScreen = multiplayerMenuScreen;
			}
			if (!IsMainUserSignedIn() && (Object)(object)currentMenuScreen != (Object)(object)EAwarningProfileMenu && (Object)(object)currentMenuScreen != (Object)(object)EngagementScreen)
			{
				desiredMenuScreen = profileMenuScreen;
			}
			if (!((Object)(object)currentMenuScreen == (Object)null))
			{
				UpdateMenuTransition();
			}
		}
	}

	private void UpdateMenuTransition()
	{
		//IL_00d9: Unknown result type (might be due to invalid IL or missing references)
		//IL_00e9: Unknown result type (might be due to invalid IL or missing references)
		//IL_00fa: Unknown result type (might be due to invalid IL or missing references)
		//IL_010a: Unknown result type (might be due to invalid IL or missing references)
		//IL_010f: Unknown result type (might be due to invalid IL or missing references)
		//IL_011b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0120: Unknown result type (might be due to invalid IL or missing references)
		//IL_0123: Unknown result type (might be due to invalid IL or missing references)
		//IL_0129: Unknown result type (might be due to invalid IL or missing references)
		//IL_0156: Unknown result type (might be due to invalid IL or missing references)
		//IL_015c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0189: Unknown result type (might be due to invalid IL or missing references)
		//IL_018f: Unknown result type (might be due to invalid IL or missing references)
		//IL_01c0: Unknown result type (might be due to invalid IL or missing references)
		if ((Object)(object)desiredMenuScreen != (Object)(object)currentMenuScreen)
		{
			currentMenuScreen.shouldDisplay = false;
			if (currentMenuScreen.IsReadyToLeave())
			{
				MPEventSystemLocator component = ((Component)this).GetComponent<MPEventSystemLocator>();
				if (Object.op_Implicit((Object)(object)component))
				{
					component.eventSystem.SetSelectedObject(null);
				}
				currentMenuScreen.OnExit(this);
				((Component)currentMenuScreen).gameObject.SetActive(false);
				currentMenuScreen = desiredMenuScreen;
				camTransitionTimer = camTransitionDuration;
				currentMenuScreen.OnEnter(this);
				IsOnMultiplayerScreen = (Object)(object)currentMenuScreen == (Object)(object)multiplayerMenuScreen;
			}
		}
		else if ((Object)(object)currentMenuScreen.desiredCameraTransform != (Object)null)
		{
			camTransitionTimer -= Time.deltaTime;
			cameraTransform.position = Vector3.SmoothDamp(cameraTransform.position, currentMenuScreen.desiredCameraTransform.position, ref camSmoothDampPositionVelocity, camTranslationSmoothDampTime);
			Vector3 eulerAngles = cameraTransform.eulerAngles;
			Vector3 eulerAngles2 = currentMenuScreen.desiredCameraTransform.eulerAngles;
			eulerAngles.x = Mathf.SmoothDampAngle(eulerAngles.x, eulerAngles2.x, ref camSmoothDampRotationVelocity.x, camRotationSmoothDampTime, float.PositiveInfinity, Time.unscaledDeltaTime);
			eulerAngles.y = Mathf.SmoothDampAngle(eulerAngles.y, eulerAngles2.y, ref camSmoothDampRotationVelocity.y, camRotationSmoothDampTime, float.PositiveInfinity, Time.unscaledDeltaTime);
			eulerAngles.z = Mathf.SmoothDampAngle(eulerAngles.z, eulerAngles2.z, ref camSmoothDampRotationVelocity.z, camRotationSmoothDampTime, float.PositiveInfinity, Time.unscaledDeltaTime);
			cameraTransform.eulerAngles = eulerAngles;
			if (camTransitionTimer <= 0f)
			{
				((Component)currentMenuScreen).gameObject.SetActive(true);
				currentMenuScreen.shouldDisplay = true;
			}
		}
	}

	public void SetDesiredMenuScreen(BaseMainMenuScreen newDesiredMenuScreen)
	{
		desiredMenuScreen = newDesiredMenuScreen;
	}

	public void ClearEngagementScreen()
	{
		if (!CheckWarningScreen())
		{
			desiredMenuScreen = titleMenuScreen;
		}
	}

	private void UpdateEoSLoginState()
	{
		if (lastEOSLoginState == EOSLoginManager.loginState)
		{
			return;
		}
		switch (EOSLoginManager.loginState)
		{
		case EOSLoginManager.EOSLoginState.None:
			Debug.Log((object)"EOSLoginState: None");
			break;
		case EOSLoginManager.EOSLoginState.AttemptingLogin:
			Debug.Log((object)"EOSLoginState: AttemptingLogin");
			break;
		case EOSLoginManager.EOSLoginState.AttemptingLink:
			Debug.Log((object)"EOSLoginState: AttemptingLink");
			AddAccountLinkPopup();
			break;
		case EOSLoginManager.EOSLoginState.FailedLogin:
			Debug.Log((object)"EOSLoginState: FailedLogin");
			AddAccountLinkPopup();
			break;
		case EOSLoginManager.EOSLoginState.FailedLink:
			Debug.Log((object)"EOSLoginState: FailedLink");
			AddAccountLinkPopup();
			break;
		case EOSLoginManager.EOSLoginState.Success:
			Debug.Log((object)"EOSLoginState: Success");
			if (((Behaviour)titleMenuScreen).isActiveAndEnabled)
			{
				GameObject obj = mainMenuButtonPanel;
				if (obj != null)
				{
					obj.SetActive(true);
				}
			}
			break;
		}
		lastEOSLoginState = EOSLoginManager.loginState;
	}

	private void AddAccountLinkPopup()
	{
		//IL_00a2: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b7: Expected O, but got Unknown
		//IL_00c5: Unknown result type (might be due to invalid IL or missing references)
		//IL_00da: Expected O, but got Unknown
		SimpleDialogBox dialogBox = SimpleDialogBox.Create();
		Action retryLoginFunction = delegate
		{
			if (Object.op_Implicit((Object)(object)dialogBox))
			{
				RetryLogin();
			}
		};
		Action deactiveCrossplayAndRestartFunction = delegate
		{
			if (Object.op_Implicit((Object)(object)dialogBox))
			{
				DeactivateCrossplayAndRestart();
			}
		};
		dialogBox.headerToken = new SimpleDialogBox.TokenParamsPair
		{
			token = "EOS_NOT_LINKED_TITLE",
			formatParams = Array.Empty<object>()
		};
		dialogBox.descriptionToken = new SimpleDialogBox.TokenParamsPair
		{
			token = "EOS_NOT_LINKED_MESSAGE",
			formatParams = Array.Empty<object>()
		};
		dialogBox.AddActionButton((UnityAction)delegate
		{
			retryLoginFunction();
		}, "EOS_RETRY_LOGIN", true);
		dialogBox.AddActionButton((UnityAction)delegate
		{
			deactiveCrossplayAndRestartFunction();
		}, "EOS_DEACTIVATE_CROSSPLAY_RESTART", true);
	}

	private void RetryLogin()
	{
		if ((Handle)(object)EOSLoginManager.loggedInAuthId == (Handle)null)
		{
			new EOSLoginManager().TryLogin();
		}
		else if (((Behaviour)titleMenuScreen).isActiveAndEnabled)
		{
			GameObject obj = mainMenuButtonPanel;
			if (obj != null)
			{
				obj.SetActive(true);
			}
		}
	}

	private void DeactivateCrossplayAndRestart()
	{
		Console.instance.FindConVar("egsToggle")?.AttemptSetString("0");
		Console.instance.SubmitCmd(null, "quit");
	}

	private void ShowFirstTimeCrossPlayPopup()
	{
		//IL_00ad: Unknown result type (might be due to invalid IL or missing references)
		//IL_00c2: Expected O, but got Unknown
		//IL_00d0: Unknown result type (might be due to invalid IL or missing references)
		//IL_00e5: Expected O, but got Unknown
		//IL_00f3: Unknown result type (might be due to invalid IL or missing references)
		//IL_0108: Expected O, but got Unknown
		SimpleDialogBox dialogBox = SimpleDialogBox.Create();
		Action activateCrossplayFunction = delegate
		{
			if (Object.op_Implicit((Object)(object)dialogBox))
			{
				ActivateCrossPlay();
			}
		};
		Action deactivateCrossplayFunction = delegate
		{
			if (Object.op_Implicit((Object)(object)dialogBox))
			{
				DeactivateCrossPlay();
			}
		};
		Action ShowCrossplayInfo = delegate
		{
			if (Object.op_Implicit((Object)(object)dialogBox))
			{
				Application.OpenURL("https://support.gearboxsoftware.com/hc/en-us/articles/4440999200269");
			}
		};
		dialogBox.headerToken = new SimpleDialogBox.TokenParamsPair
		{
			token = "FIRST_TIME_CROSSPLAY_POPUP_HEADER",
			formatParams = Array.Empty<object>()
		};
		dialogBox.descriptionToken = new SimpleDialogBox.TokenParamsPair
		{
			token = "FIRST_TIME_CROSSPLAY_POPUP_DESCRIPTION",
			formatParams = Array.Empty<object>()
		};
		dialogBox.AddActionButton((UnityAction)delegate
		{
			activateCrossplayFunction();
		}, "STAT_CONTINUE", true);
		dialogBox.AddActionButton((UnityAction)delegate
		{
			ShowCrossplayInfo();
		}, "FIRST_TIME_CROSSPLAY_POPUP_INFO", false);
		dialogBox.AddActionButton((UnityAction)delegate
		{
			deactivateCrossplayFunction();
		}, "FIRST_TIME_CROSSPLAY_POPUP_DISABLE_CROSSPLAY", true);
		static void ActivateCrossPlay()
		{
			PlayerPrefs.SetInt("ShownFirstTimePopup", 1);
		}
		static void DeactivateCrossPlay()
		{
			PlayerPrefs.SetInt("ShownFirstTimePopup", 1);
			Console.instance.FindConVar("egsToggle")?.AttemptSetString("1");
			Console.instance.SubmitCmd(null, "quit");
		}
	}

	public void StartWaitForLoad()
	{
		IsMainUserSignedIn();
	}

	public void EnableContinueEAWarningButton()
	{
		((Selectable)continueButtonTransition).interactable = true;
	}

	public void StartSinglePlayer()
	{
		PlatformSystems.networkManager.StartSinglePlayer();
	}
}
