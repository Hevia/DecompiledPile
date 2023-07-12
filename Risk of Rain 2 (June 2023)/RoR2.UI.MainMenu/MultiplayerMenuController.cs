using System;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using RoR2.Networking;
using TMPro;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.Networking;
using UnityEngine.UI;

namespace RoR2.UI.MainMenu;

[RequireComponent(typeof(FirstSelectedObjectProvider))]
public class MultiplayerMenuController : BaseMainMenuScreen
{
	public enum Subview
	{
		Main,
		FindGame,
		HostGame
	}

	[Serializable]
	[CompilerGenerated]
	private sealed class _003C_003Ec
	{
		public static readonly _003C_003Ec _003C_003E9 = new _003C_003Ec();

		public static UnityAction _003C_003E9__64_2;

		public static UnityAction _003C_003E9__64_3;

		internal void _003CAwake_003Eb__64_2()
		{
			PlatformSystems.lobbyManager.ToggleQuickplay();
		}

		internal void _003CAwake_003Eb__64_3()
		{
			PlatformSystems.lobbyManager.SetNetworkType(isInternet: true);
			PlatformSystems.lobbyManager.OnStartPrivateGame();
		}
	}

	private const float titleTransitionDuration = 0.5f;

	private const float titleTransitionBuffer = 0.1f;

	public Image fadeImage;

	public LerpUIRect[] uiToLerp;

	private float titleStopwatch;

	private Subview _curSubView;

	[Header("Subviews")]
	public GameObject MainMultiplayerMenu;

	public GameObject JoinGameMenu;

	public GameObject HostGameMenu;

	[Header("Buttons")]
	public MPButton hostGame;

	public MPButton findGame;

	public MPButton quickplayButton;

	public MPButton startPrivateGameButton;

	public MPButton inviteButton;

	public MPButton backButton;

	public GameObject[] lobbyActions;

	[Header("Quickplay Logic")]
	public LanguageTextMeshController quickplayLabelController;

	public string quickplayStartToken;

	public string quickplayStopToken;

	public RectTransform quickplaySpinnerRectTransform;

	public TextMeshProUGUI quickplayStateText;

	private Coroutine updateCutoffTime;

	private bool hasCutoffTime;

	private double cutoffTime;

	public static float QuickplayWaitTime = 45f;

	[Header("Sub-Views")]
	public SteamJoinClipboardLobby joinClipboardLobbyButtonController;

	public LobbyUserList lobbyUserList;

	[Header("Platform Specific Components")]
	public CarouselController lobbyVisibilityController;

	public static MultiplayerMenuController instance { get; private set; }

	public bool isInHostingState => PlatformSystems.lobbyManager.state == LobbyManager.State.Hosting;

	public bool IsQuickPlaySearching { get; set; }

	public bool IsQuickPlayButtonLocked { get; set; }

	public bool CanLeave { get; set; } = true;


	public Subview GetCurrentSubview()
	{
		return _curSubView;
	}

	public void SetSubview(Subview targetSubview)
	{
		_curSubView = targetSubview;
		MainMultiplayerMenu.SetActive(targetSubview == Subview.Main);
		JoinGameMenu.SetActive(targetSubview == Subview.FindGame);
		HostGameMenu.SetActive(targetSubview == Subview.HostGame);
	}

	public void ReturnToMainSubview()
	{
		SetSubview(Subview.Main);
	}

	private void ToggleMPFeatures(MPFeatures featureFlags)
	{
		Toggle((MonoBehaviour)(object)hostGame, featureFlags.HasFlag(MPFeatures.HostGame));
		Toggle((MonoBehaviour)(object)findGame, featureFlags.HasFlag(MPFeatures.FindGame));
		Toggle((MonoBehaviour)(object)quickplayButton, featureFlags.HasFlag(MPFeatures.Quickplay));
		Toggle((MonoBehaviour)(object)startPrivateGameButton, featureFlags.HasFlag(MPFeatures.PrivateGame));
		Toggle((MonoBehaviour)(object)inviteButton, featureFlags.HasFlag(MPFeatures.Invite));
	}

	private void ToggleMPLobbyFeatures(MPLobbyFeatures lobbyFlags)
	{
		if (PlatformSystems.lobbyManager.HasMPLobbyUI())
		{
			Toggle((MonoBehaviour)(object)lobbyUserList.createLobbyButton, lobbyFlags.HasFlag(MPLobbyFeatures.CreateLobby));
			Toggle((MonoBehaviour)(object)lobbyUserList.leaveLobbyButton, lobbyFlags.HasFlag(MPLobbyFeatures.LeaveLobby));
			Toggle((MonoBehaviour)(object)lobbyUserList.copyLobbyButton, lobbyFlags.HasFlag(MPLobbyFeatures.Clipboard));
			Toggle((MonoBehaviour)(object)lobbyUserList.joinLobbyButton, lobbyFlags.HasFlag(MPLobbyFeatures.Clipboard));
		}
	}

	private void ToggleMPFeaturesInteractable(MPFeatures featureFlags)
	{
		((Selectable)hostGame).interactable = featureFlags.HasFlag(MPFeatures.HostGame);
		((Selectable)findGame).interactable = featureFlags.HasFlag(MPFeatures.FindGame);
		((Selectable)quickplayButton).interactable = featureFlags.HasFlag(MPFeatures.Quickplay);
		((Selectable)startPrivateGameButton).interactable = featureFlags.HasFlag(MPFeatures.PrivateGame);
		((Selectable)inviteButton).interactable = featureFlags.HasFlag(MPFeatures.Invite);
	}

	private void ToggleMPLobbyFeaturesInteractable(MPLobbyFeatures lobbyFlags)
	{
		((Selectable)lobbyUserList.createLobbyButton).interactable = lobbyFlags.HasFlag(MPLobbyFeatures.CreateLobby);
		((Selectable)lobbyUserList.leaveLobbyButton).interactable = lobbyFlags.HasFlag(MPLobbyFeatures.LeaveLobby);
		((Selectable)lobbyUserList.copyLobbyButton).interactable = lobbyFlags.HasFlag(MPLobbyFeatures.Clipboard);
		((Selectable)lobbyUserList.joinLobbyButton).interactable = lobbyFlags.HasFlag(MPLobbyFeatures.Clipboard);
		((Selectable)lobbyUserList.lobbyTypeDropdown).interactable = lobbyFlags.HasFlag(MPLobbyFeatures.LobbyDropdownOptions);
	}

	private void Toggle(MonoBehaviour button, bool val)
	{
		if (Object.op_Implicit((Object)(object)button))
		{
			((Component)button).gameObject.SetActive(val);
		}
		else
		{
			Debug.LogError((object)("Nullref on Toggle of Button in MultiplayerMenuController on \"" + ((Object)((Component)this).gameObject).name + "\""));
		}
	}

	private void Toggle(GameObject[] goToToggle, bool val)
	{
		int num = goToToggle.Length;
		for (int i = 0; i < num; i++)
		{
			goToToggle[i].gameObject.SetActive(val);
		}
	}

	private void Toggle(MonoBehaviour[] buttons, bool val)
	{
		int num = buttons.Length;
		for (int i = 0; i < num; i++)
		{
			((Component)buttons[i]).gameObject.SetActive(val);
		}
	}

	public void SetQuickplayButtonStateText(string textKey, object[] formatArgs = null)
	{
		if (formatArgs == null)
		{
			formatArgs = Array.Empty<object>();
		}
		TextMeshProUGUI val = quickplayStateText;
		if (Object.op_Implicit((Object)(object)val))
		{
			((TMP_Text)val).text = Language.GetStringFormatted(textKey, formatArgs);
		}
	}

	public void SetButtonText(MPButton button, string textKey, object[] formatArgs = null)
	{
		if (formatArgs == null)
		{
			formatArgs = Array.Empty<object>();
		}
		TextMeshProUGUI component = ((Component)button).GetComponent<TextMeshProUGUI>();
		if (Object.op_Implicit((Object)(object)component))
		{
			((TMP_Text)component).text = Language.GetStringFormatted(textKey, formatArgs);
		}
	}

	public void SetNetworkType(bool isInternet)
	{
		PlatformSystems.lobbyManager.SetNetworkType(isInternet);
	}

	public void RefreshFirstObjectSelectedProvider()
	{
		if (Object.op_Implicit((Object)(object)firstSelectedObjectProvider))
		{
			firstSelectedObjectProvider.firstSelectedObject = ((Component)hostGame).gameObject;
			firstSelectedObjectProvider.fallBackFirstSelectedObjects = (GameObject[])(object)new GameObject[6]
			{
				((Component)hostGame).gameObject,
				((Component)findGame).gameObject,
				((Component)quickplayButton).gameObject,
				((Component)startPrivateGameButton).gameObject,
				((Component)inviteButton).gameObject,
				((Component)backButton).gameObject
			};
		}
	}

	public void OnEnable()
	{
		ResetState();
		RefreshFirstObjectSelectedProvider();
		ToggleMPFeatures(PlatformSystems.lobbyManager.GetPlatformMPFeatureFlags());
		ToggleMPLobbyFeatures(PlatformSystems.lobbyManager.GetPlatformMPLobbyFeatureFlags());
		LerpAllUI(LerpUIRect.LerpState.Entering);
		if (!Object.op_Implicit((Object)(object)instance))
		{
			instance = SingletonHelper.Assign<MultiplayerMenuController>(instance, this);
		}
		PlatformSystems.lobbyManager.OnMultiplayerMenuEnabled(OnLobbyLeave);
		firstSelectedObjectProvider?.ResetLastSelected();
		firstSelectedObjectProvider?.EnsureSelectedObject();
	}

	public void OnDisable()
	{
		LobbyManager lobbyManager = PlatformSystems.lobbyManager;
		lobbyManager.onLobbyLeave = (Action<UserID>)Delegate.Remove(lobbyManager.onLobbyLeave, new Action<UserID>(OnLobbyLeave));
		if (!((NetworkManager)NetworkManagerSystem.singleton).isNetworkActive)
		{
			PlatformSystems.lobbyManager.LeaveLobby();
		}
		instance = SingletonHelper.Unassign<MultiplayerMenuController>(instance, this);
	}

	private void OnLobbyLeave(UserID lobbyId)
	{
		if (!(PlatformSystems.lobbyManager as SteamworksLobbyManager).isInLobbyDelayed && !PlatformSystems.lobbyManager.awaitingJoin)
		{
			PlatformSystems.lobbyManager.CreateLobby();
		}
	}

	public new void Awake()
	{
		//IL_001f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0029: Expected O, but got Unknown
		//IL_003b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0045: Expected O, but got Unknown
		//IL_0064: Unknown result type (might be due to invalid IL or missing references)
		//IL_0069: Unknown result type (might be due to invalid IL or missing references)
		//IL_006f: Expected O, but got Unknown
		//IL_0093: Unknown result type (might be due to invalid IL or missing references)
		//IL_0098: Unknown result type (might be due to invalid IL or missing references)
		//IL_009e: Expected O, but got Unknown
		base.Awake();
		LerpAllUI(LerpUIRect.LerpState.Entering);
		((UnityEvent)((Button)hostGame).onClick).AddListener((UnityAction)delegate
		{
			PlatformSystems.lobbyManager.SetNetworkType(isInternet: true);
			SetSubview(Subview.HostGame);
		});
		((UnityEvent)((Button)findGame).onClick).AddListener((UnityAction)delegate
		{
			SetSubview(Subview.FindGame);
		});
		ButtonClickedEvent onClick = ((Button)quickplayButton).onClick;
		object obj = _003C_003Ec._003C_003E9__64_2;
		if (obj == null)
		{
			UnityAction val = delegate
			{
				PlatformSystems.lobbyManager.ToggleQuickplay();
			};
			obj = (object)val;
			_003C_003Ec._003C_003E9__64_2 = val;
		}
		((UnityEvent)onClick).AddListener((UnityAction)obj);
		ButtonClickedEvent onClick2 = ((Button)startPrivateGameButton).onClick;
		object obj2 = _003C_003Ec._003C_003E9__64_3;
		if (obj2 == null)
		{
			UnityAction val2 = delegate
			{
				PlatformSystems.lobbyManager.SetNetworkType(isInternet: true);
				PlatformSystems.lobbyManager.OnStartPrivateGame();
			};
			obj2 = (object)val2;
			_003C_003Ec._003C_003E9__64_3 = val2;
		}
		((UnityEvent)onClick2).AddListener((UnityAction)obj2);
		SetLobbyVisibilityChoices();
	}

	private void SetLobbyVisibilityChoices()
	{
		if (!((Object)(object)lobbyVisibilityController != (Object)null) || !PlatformSystems.ShouldUseEpicOnlineSystems)
		{
			return;
		}
		List<CarouselController.Choice> list = new List<CarouselController.Choice>(lobbyVisibilityController.choices);
		foreach (CarouselController.Choice item in list)
		{
			if (item.convarValue == "Private")
			{
				list.Remove(item);
				break;
			}
		}
		lobbyVisibilityController.choices = list.ToArray();
	}

	public void LerpAllUI(LerpUIRect.LerpState lerpState)
	{
		LerpUIRect[] array = uiToLerp;
		for (int i = 0; i < array.Length; i++)
		{
			array[i].lerpState = lerpState;
		}
	}

	private void ResetState()
	{
		if (PlatformSystems.lobbyManager.GetPlatformMPFeatureFlags().HasFlag(MPFeatures.Quickplay))
		{
			SetQuickplayText("", new object[0]);
			ResetCutoffTime();
		}
	}

	public void BackButtonPressed()
	{
		Debug.LogError((object)"BackButtonPressed");
		PlatformSystems.lobbyManager.LeaveLobby();
		lobbyUserList.ClearUserList();
		ReturnToMainMenu();
	}

	public void ReturnToMainMenu()
	{
		Debug.Log((object)"ReturnToMainMenu");
		myMainMenuController.SetDesiredMenuScreen(myMainMenuController.titleMenuScreen);
	}

	public void OnEnterGameButtonPressed()
	{
	}

	private new void Update()
	{
		//IL_0048: Unknown result type (might be due to invalid IL or missing references)
		//IL_004d: Unknown result type (might be due to invalid IL or missing references)
		//IL_006a: Unknown result type (might be due to invalid IL or missing references)
		base.Update();
		titleStopwatch += Time.deltaTime;
		PlatformSystems.lobbyManager.CheckBusyTimer();
		if (IsQuickPlaySearching)
		{
			ToggleMPFeaturesInteractable(MPFeatures.Quickplay);
			((Component)quickplaySpinnerRectTransform).gameObject.SetActive(true);
			Vector3 localEulerAngles = ((Transform)quickplaySpinnerRectTransform).localEulerAngles;
			localEulerAngles.z += Time.deltaTime * 360f;
			((Transform)quickplaySpinnerRectTransform).localEulerAngles = localEulerAngles;
			quickplayLabelController.token = quickplayStopToken;
		}
		else if (PlatformSystems.lobbyManager.state == LobbyManager.State.Hosting)
		{
			ToggleMPFeaturesInteractable(MPFeatures.None);
			if (Object.op_Implicit((Object)(object)inviteButton))
			{
				((Selectable)inviteButton).interactable = ShouldEnableInviteButton();
			}
			((Component)quickplaySpinnerRectTransform).gameObject.SetActive(false);
			quickplayLabelController.token = quickplayStartToken;
		}
		else
		{
			ToggleMPFeaturesInteractable(PlatformSystems.lobbyManager.GetPlatformMPFeatureFlags());
			((Component)quickplaySpinnerRectTransform).gameObject.SetActive(false);
			quickplayLabelController.token = quickplayStartToken;
			if (Object.op_Implicit((Object)(object)quickplayButton))
			{
				((Selectable)quickplayButton).interactable = ShouldEnableQuickplayButton();
			}
			if (Object.op_Implicit((Object)(object)startPrivateGameButton))
			{
				((Selectable)startPrivateGameButton).interactable = ShouldEnableStartPrivateGameButton();
			}
			if (Object.op_Implicit((Object)(object)joinClipboardLobbyButtonController) && Object.op_Implicit((Object)(object)joinClipboardLobbyButtonController.mpButton))
			{
				((Selectable)joinClipboardLobbyButtonController.mpButton).interactable = ShouldEnableJoinClipboardLobbyButton();
			}
			if (Object.op_Implicit((Object)(object)inviteButton))
			{
				((Selectable)inviteButton).interactable = ShouldEnableInviteButton();
			}
			((Selectable)backButton).interactable = ShouldEnableBackButton();
		}
		firstSelectedObjectProvider?.EnsureSelectedObject();
	}

	public void CreateLocalLobby()
	{
		NetworkManagerSystem.singleton.CreateLocalLobby();
	}

	public void InviteButtonPressed()
	{
		if (!PlatformSystems.lobbyManager.isInLobby)
		{
			CreateLocalLobby();
		}
	}

	private bool ShouldEnableQuickplayButton()
	{
		return PlatformSystems.lobbyManager.ShouldEnableQuickplayButton();
	}

	private bool ShouldEnableStartPrivateGameButton()
	{
		return PlatformSystems.lobbyManager.ShouldEnableStartPrivateGameButton();
	}

	private bool ShouldEnableJoinClipboardLobbyButton()
	{
		if (!PlatformSystems.lobbyManager.newestLobbyData.quickplayQueued)
		{
			return joinClipboardLobbyButtonController.validClipboardLobbyID;
		}
		return false;
	}

	private bool ShouldEnableInviteButton()
	{
		return PlatformSystems.lobbyManager.CanInvite();
	}

	public void ToggleQuickplay()
	{
		PlatformSystems.lobbyManager.ToggleQuickplay();
	}

	public override bool IsReadyToLeave()
	{
		return true;
	}

	private bool ShouldEnableBackButton()
	{
		if (!PlatformSystems.lobbyManager.IsBusy)
		{
			return _curSubView == Subview.Main;
		}
		return false;
	}

	private bool ShouldEnableEnterGameButton()
	{
		return !PlatformSystems.lobbyManager.IsBusy;
	}

	public void GoBack()
	{
		if (PlatformSystems.lobbyManager.newestLobbyData.quickplayQueued)
		{
			quickplayButton.InvokeClick();
		}
		else if (ShouldEnableBackButton())
		{
			backButton.InvokeClick();
		}
	}

	public void ResetLastSelected()
	{
	}

	public void ResetCutoffTime()
	{
		if (PlatformSystems.lobbyManager.HasMPFeature(MPFeatures.Quickplay) && updateCutoffTime != null)
		{
			((MonoBehaviour)this).StopCoroutine(updateCutoffTime);
			updateCutoffTime = null;
		}
	}

	private void OnLobbyPropertyCutoffTimeChanged(double cutoffTime)
	{
		hasCutoffTime = true;
		this.cutoffTime = cutoffTime;
	}

	private IEnumerator UpdateCutoffTime()
	{
		if (!PlatformSystems.lobbyManager.HasMPFeature(MPFeatures.Quickplay))
		{
			yield break;
		}
		Debug.LogError((object)"UpdateCutoffTime coroutine start");
		float fullDuration = QuickplayWaitTime;
		float age = 0f;
		hasCutoffTime = false;
		if (PlatformSystems.networkManager.IsHost())
		{
			Debug.LogError((object)"UpdateCutoffTime coroutine host");
			TimeSpan timeSpan = DateTime.Now + TimeSpan.FromSeconds(fullDuration) - Util.dateZero;
			PlatformSystems.lobbyManager.SetQuickplayCutoffTime(timeSpan.TotalSeconds);
		}
		else
		{
			Debug.LogError((object)"UpdateCutoffTime coroutine client");
			cutoffTime = PlatformSystems.lobbyManager.GetQuickplayCutoffTime();
			if (cutoffTime == 0.0)
			{
				while (!hasCutoffTime)
				{
					yield return null;
				}
			}
			Debug.LogError((object)$"Client cutoffTime = {cutoffTime}");
			Debug.LogError((object)$"Client cutoffTime = {cutoffTime}");
			fullDuration = CalculateCutoffTimerDuration();
		}
		while (age < fullDuration)
		{
			if (!PlatformSystems.networkManager.isHost)
			{
				double quickplayCutoffTime = PlatformSystems.lobbyManager.GetQuickplayCutoffTime();
				if (quickplayCutoffTime != cutoffTime)
				{
					cutoffTime = quickplayCutoffTime;
					fullDuration = CalculateCutoffTimerDuration();
				}
			}
			UpdateCutoffTimeText(PlatformSystems.lobbyManager.calculatedTotalPlayerCount, fullDuration, age);
			age += Time.unscaledDeltaTime;
			yield return null;
		}
		PlatformSystems.lobbyManager.OnCutoffTimerComplete();
	}

	private float CalculateCutoffTimerDuration()
	{
		DateTime now = DateTime.Now;
		return (float)(Util.dateZero + TimeSpan.FromSeconds(cutoffTime) - now).TotalSeconds;
	}

	public void StartUpdateCutoffTimer()
	{
		if (PlatformSystems.lobbyManager.HasMPFeature(MPFeatures.Quickplay))
		{
			updateCutoffTime = ((MonoBehaviour)this).StartCoroutine(UpdateCutoffTime());
		}
	}

	public void SetQuickplayTextBelowMinPlayers()
	{
		if (PlatformSystems.lobbyManager.HasMPFeature(MPFeatures.Quickplay))
		{
			object[] formatArgs = new object[2]
			{
				PlatformSystems.lobbyManager.calculatedTotalPlayerCount,
				RoR2Application.maxPlayers
			};
			SetQuickplayText("STEAM_LOBBY_STATUS_QUICKPLAY_WAITING_BELOW_MINIMUM_PLAYERS", formatArgs);
		}
	}

	private void UpdateCutoffTimeText(int numberOfPlayers, float fullDuration, float age)
	{
		object[] formatArgs = new object[3]
		{
			numberOfPlayers,
			RoR2Application.maxPlayers,
			(int)Math.Max(0.0, fullDuration - age)
		};
		SetQuickplayText("STEAM_LOBBY_STATUS_QUICKPLAY_WAITING_ABOVE_MINIMUM_PLAYERS", formatArgs);
	}

	public void SetQuickplayText(string text, object[] formatArgs = null)
	{
		if (formatArgs == null)
		{
			formatArgs = Array.Empty<object>();
		}
		((TMP_Text)quickplayStateText).text = Language.GetStringFormatted(text, formatArgs);
	}

	public IEnumerator LockQuickPlayButton(float duration)
	{
		IsQuickPlayButtonLocked = true;
		yield return (object)new WaitForSeconds(duration);
		IsQuickPlayButtonLocked = false;
	}
}
