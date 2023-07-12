using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using RoR2.ConVar;
using RoR2.Networking;
using RoR2.RemoteGameBrowser;
using RoR2.UI;
using TMPro;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

namespace RoR2;

[RequireComponent(typeof(RectTransform))]
public class RemoteGameDetailsPanelController : MonoBehaviour
{
	[Header("Main Info")]
	public TextMeshProUGUI nameLabel;

	public TextMeshProUGUI typeLabel;

	public TextMeshProUGUI lobbyPlayerCountLabel;

	public TextMeshProUGUI serverPlayerCountLabel;

	public TextMeshProUGUI pingLabel;

	public TextMeshProUGUI tagsLabel;

	public TextMeshProUGUI gameModeLabel;

	public TextMeshProUGUI mapLabel;

	public TextMeshProUGUI statusLabel;

	[Header("Password")]
	public RectTransform passwordPanel;

	public TMP_InputField passwordTextBox;

	[Header("Rules")]
	public RectTransform rulesPanel;

	public RectTransform rulesContainer;

	public GameObject ruleIconPrefab;

	[Header("Players")]
	public RectTransform playersPanel;

	public RectTransform playersContainer;

	public GameObject playerStripPrefab;

	[Header("Favorite/Blacklist")]
	public MPToggle favoriteToggle;

	public MPToggle blacklistToggle;

	[Header("Buttons")]
	public MPButton refreshButton;

	public MPButton joinLobbyButton;

	public MPButton joinServerButton;

	[Header("Warning Panels")]
	public GameObject highCapacityWarningPanel;

	public GameObject versionMismatchWarningPanel;

	public GameObject modMismatchWarningPanel;

	private UIElementAllocator<ChildLocator> playerStripAllocator;

	private UIElementAllocator<RuleChoiceController> ruleIconAllocator;

	private RuleBook ruleBook;

	private RectTransform rectTransform;

	private RemoteGameInfo currentGameInfo;

	private int cookie;

	private void Awake()
	{
		//IL_0007: Unknown result type (might be due to invalid IL or missing references)
		//IL_0011: Expected O, but got Unknown
		rectTransform = (RectTransform)((Component)this).transform;
		playerStripAllocator = new UIElementAllocator<ChildLocator>(playersContainer, playerStripPrefab);
		ruleIconAllocator = new UIElementAllocator<RuleChoiceController>(rulesContainer, ruleIconPrefab);
		ruleBook = new RuleBook();
		OnChangeEvent onValueChanged = passwordTextBox.onValueChanged;
		StringConVar cvClPassword = NetworkManagerSystem.cvClPassword;
		((UnityEvent<string>)(object)onValueChanged).AddListener((UnityAction<string>)cvClPassword.SetString);
	}

	private void OnEnable()
	{
		passwordTextBox.SetTextWithoutNotify(NetworkManagerSystem.cvClPassword.value);
	}

	private void Update()
	{
		int num = LocalUserManager.readOnlyLocalUsersList.Count;
		if (PlatformSystems.lobbyManager.isInLobby)
		{
			num = PlatformSystems.lobbyManager.calculatedTotalPlayerCount;
		}
		((Selectable)joinLobbyButton).interactable = currentGameInfo.availableLobbySlots >= num;
		((Selectable)joinServerButton).interactable = currentGameInfo.availableServerSlots >= num;
	}

	public void SetGameInfo(RemoteGameInfo newGameInfo)
	{
		SetGameInfoInternal(in newGameInfo);
		RequestRefresh();
	}

	private void SetGameInfoInternal(in RemoteGameInfo newGameInfo)
	{
		currentGameInfo = newGameInfo;
		SceneDef sceneDef = SceneCatalog.GetSceneDef(newGameInfo.currentSceneIndex ?? SceneIndex.Invalid);
		string value = null;
		if (newGameInfo.didRespond.HasValue)
		{
			value = Language.GetString(newGameInfo.didRespond.Value ? "REMOTE_GAME_STATUS_RESPONSIVE" : "REMOTE_GAME_STATUS_UNRESPONSIVE");
		}
		GameModeIndex gameModeIndex = GameModeIndex.Invalid;
		string text = null;
		if (currentGameInfo.gameModeName != null)
		{
			gameModeIndex = GameModeCatalog.FindGameModeIndex(currentGameInfo.gameModeName);
		}
		if (gameModeIndex != GameModeIndex.Invalid)
		{
			text = GameModeCatalog.GetGameModePrefabComponent(gameModeIndex).nameToken;
		}
		if (PlatformSystems.ShouldUseEpicOnlineSystems)
		{
			SetLabelInfo(nameLabel, "REMOTE_GAME_DETAILS_NAME", currentGameInfo.serverName);
		}
		else
		{
			SetLabelInfo(nameLabel, "REMOTE_GAME_DETAILS_NAME", currentGameInfo.name);
		}
		SetLabelInfo(pingLabel, "REMOTE_GAME_DETAILS_PING", currentGameInfo.ping);
		SetLabelInfo(tagsLabel, "REMOTE_GAME_DETAILS_TAGS", (currentGameInfo.tags != null) ? string.Join(",", currentGameInfo.tags) : null);
		SetLabelInfo(gameModeLabel, "REMOTE_GAME_DETAILS_GAMEMODE", (text != null) ? Language.GetString(text) : null);
		SetLabelInfo(mapLabel, "REMOTE_GAME_DETAILS_MAP", Object.op_Implicit((Object)(object)sceneDef) ? Language.GetString(sceneDef.nameToken) : null);
		SetLabelInfo(statusLabel, "REMOTE_GAME_DETAILS_STATUS", value);
		SetLabelInfo(lobbyPlayerCountLabel, "REMOTE_GAME_DETAILS_LOBBY_PLAYER_COUNT", currentGameInfo.lobbyPlayerCount, currentGameInfo.lobbyMaxPlayers);
		SetLabelInfo(serverPlayerCountLabel, "REMOTE_GAME_DETAILS_SERVER_PLAYER_COUNT", currentGameInfo.serverPlayerCount, currentGameInfo.serverMaxPlayers);
		List<RemotePlayerInfo> list = new List<RemotePlayerInfo>();
		newGameInfo.GetPlayers(list);
		playerStripAllocator.AllocateElements(list.Count);
		ReadOnlyCollection<ChildLocator> elements = playerStripAllocator.elements;
		for (int i = 0; i < playerStripAllocator.elements.Count; i++)
		{
			ChildLocator childLocator = elements[i];
			RemotePlayerInfo remotePlayerInfo = list[i];
			((TMP_Text)((Component)childLocator.FindChild("NameLabel")).GetComponent<TextMeshProUGUI>()).SetText(remotePlayerInfo.name, true);
		}
		if (newGameInfo.GetRuleBook(ruleBook))
		{
			List<RuleChoiceDef> list2 = new List<RuleChoiceDef>();
			foreach (RuleChoiceDef choice in ruleBook.choices)
			{
				if (!choice.onlyShowInGameBrowserIfNonDefault || !choice.isDefaultChoice)
				{
					list2.Add(choice);
				}
			}
			ruleIconAllocator.AllocateElements(list2.Count);
			ReadOnlyCollection<RuleChoiceController> elements2 = ruleIconAllocator.elements;
			for (int j = 0; j < list2.Count; j++)
			{
				elements2[j].SetChoice(list2[j]);
			}
		}
		else
		{
			ruleIconAllocator.AllocateElements(0);
		}
		SetWarningEnabled(highCapacityWarningPanel, currentGameInfo.greaterMaxPlayers > 4);
		SetWarningEnabled(versionMismatchWarningPanel, newGameInfo.buildId != null && !string.Equals(newGameInfo.buildId, RoR2Application.GetBuildId(), StringComparison.OrdinalIgnoreCase));
		SetWarningEnabled(modMismatchWarningPanel, newGameInfo.modHash != null && !string.Equals(newGameInfo.modHash, NetworkModCompatibilityHelper.networkModHash, StringComparison.OrdinalIgnoreCase));
		bool flag = newGameInfo.IsLobbyIdValid();
		bool flag2 = newGameInfo.IsServerIdValid() || newGameInfo.serverAddress.HasValue;
		if (flag && flag2)
		{
			flag2 = false;
		}
		((Component)joinLobbyButton).gameObject.SetActive(flag);
		((Component)joinServerButton).gameObject.SetActive(flag2);
		((Component)passwordPanel).gameObject.SetActive(currentGameInfo.hasPassword ?? false);
	}

	private void SetLabelInfo<T>(TextMeshProUGUI label, string formatToken, T? value, GameObject explicitContainer = null) where T : struct
	{
		if (explicitContainer == null)
		{
			explicitContainer = ((Component)label).gameObject;
		}
		bool hasValue = value.HasValue;
		explicitContainer.SetActive(hasValue);
		if (hasValue)
		{
			((TMP_Text)label).SetText(Language.GetStringFormatted(formatToken, value.Value), true);
		}
	}

	private void SetLabelInfo(TextMeshProUGUI label, string formatToken, string value, GameObject explicitContainer = null)
	{
		if (explicitContainer == null)
		{
			explicitContainer = ((Component)label).gameObject;
		}
		bool flag = !string.IsNullOrEmpty(value);
		explicitContainer.SetActive(flag);
		if (flag)
		{
			((TMP_Text)label).SetText(Language.GetStringFormatted(formatToken, value), true);
		}
	}

	private void SetLabelInfo(TextMeshProUGUI label, string formatToken, int? value1, int? value2, GameObject explicitContainer = null)
	{
		if (explicitContainer == null)
		{
			explicitContainer = ((Component)label).gameObject;
		}
		bool flag = value1.HasValue && value2.HasValue;
		explicitContainer.SetActive(flag);
		if (flag)
		{
			((TMP_Text)label).SetText(Language.GetStringFormatted(formatToken, value1.Value, value2.Value), true);
		}
	}

	private void SetWarningEnabled(GameObject labelObject, bool shouldEnable)
	{
		if (Object.op_Implicit((Object)(object)labelObject))
		{
			labelObject.SetActive(shouldEnable);
		}
	}

	public void JoinCurrentLobby()
	{
		if (currentGameInfo.IsLobbyIdValid())
		{
			string text = null;
			text = ((!PlatformSystems.ShouldUseEpicOnlineSystems) ? currentGameInfo.lobbyId.ToString() : currentGameInfo.lobbyIdStr);
			Console.instance.SubmitCmd(null, "steam_lobby_join " + text);
		}
	}

	public void JoinCurrentServer()
	{
		if (currentGameInfo.serverAddress.HasValue)
		{
			Console.instance.SubmitCmd(null, "connect " + currentGameInfo.serverAddress.Value);
		}
		else if (currentGameInfo.IsServerIdValid())
		{
			Console.instance.SubmitCmd(null, "connect_steamworks_p2p " + currentGameInfo.serverId);
		}
	}

	public void RequestRefresh()
	{
		int capturedCookie = ++cookie;
		currentGameInfo.RequestRefresh(delegate(in RemoteGameInfo refreshedGameInfo)
		{
			if (Object.op_Implicit((Object)(object)this) && capturedCookie == cookie)
			{
				RemoteGameInfo newGameInfo = refreshedGameInfo;
				newGameInfo.didRespond = true;
				SetGameInfoInternal(in newGameInfo);
			}
		}, delegate
		{
			currentGameInfo.didRespond = false;
			SetGameInfoInternal(in currentGameInfo);
		}, fetchDetails: true);
	}
}
