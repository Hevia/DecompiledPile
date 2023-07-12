using System;
using System.Collections.Generic;
using System.Globalization;
using TMPro;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

namespace RoR2.UI;

public class LobbyUserList : MonoBehaviour
{
	public struct LobbyStateChoice
	{
		public LobbyType lobbyType;

		public string token;
	}

	private class UserElement
	{
		public UserID id;

		public GameObject gameObject;

		public SocialUserIcon userIcon;

		public SocialUsernameLabel usernameLabel;

		public GameObject lobbyLeaderCrown;

		public ChildLocator elementChildLocator;

		public void SetUser(UserID playerUserID, int subPlayerIndex)
		{
			id = playerUserID;
			userIcon.RefreshWithUser(playerUserID);
			usernameLabel.userId = playerUserID;
			usernameLabel.subPlayerIndex = subPlayerIndex;
			Refresh();
		}

		public void Refresh()
		{
			if (id == default(UserID))
			{
				((Component)elementChildLocator.FindChild("UserIcon")).gameObject.SetActive(false);
				((Component)elementChildLocator.FindChild("InviteButton")).gameObject.SetActive(PlatformSystems.lobbyManager.HasMPLobbyFeature(MPLobbyFeatures.Invite));
			}
			else
			{
				((Component)elementChildLocator.FindChild("UserIcon")).gameObject.SetActive(PlatformSystems.lobbyManager.HasMPLobbyFeature(MPLobbyFeatures.UserIcon));
				((Component)elementChildLocator.FindChild("InviteButton")).gameObject.SetActive(false);
			}
			userIcon.Refresh();
			usernameLabel.Refresh();
			RefreshCrownAndPromoteButton();
		}

		private void RefreshCrownAndPromoteButton()
		{
			//IL_010a: Unknown result type (might be due to invalid IL or missing references)
			//IL_0114: Expected O, but got Unknown
			if (!PlatformSystems.lobbyManager.isInLobby)
			{
				return;
			}
			bool flag = PlatformSystems.lobbyManager.IsLobbyOwner(id);
			if (Object.op_Implicit((Object)(object)lobbyLeaderCrown) != flag)
			{
				if (flag)
				{
					lobbyLeaderCrown = Object.Instantiate<GameObject>(LegacyResourcesAPI.Load<GameObject>("Prefabs/UI/LobbyLeaderCrown"), gameObject.transform);
				}
				else
				{
					Object.Destroy((Object)(object)lobbyLeaderCrown);
					lobbyLeaderCrown = null;
				}
			}
			if (!Object.op_Implicit((Object)(object)elementChildLocator) || !PlatformSystems.lobbyManager.HasMPLobbyFeature(MPLobbyFeatures.HostPromotion))
			{
				return;
			}
			bool flag2 = !PlatformSystems.lobbyManager.ShouldShowPromoteButton() && !flag && PlatformSystems.lobbyManager.ownsLobby && id != default(UserID) && !Object.op_Implicit((Object)(object)NetworkSession.instance);
			GameObject val = ((Component)elementChildLocator.FindChild("PromoteButton")).gameObject;
			if (!Object.op_Implicit((Object)(object)val))
			{
				return;
			}
			val.SetActive(flag2);
			if (flag2)
			{
				MPButton component = val.GetComponent<MPButton>();
				((UnityEventBase)((Button)component).onClick).RemoveAllListeners();
				((UnityEvent)((Button)component).onClick).AddListener((UnityAction)delegate
				{
					Console.instance.SubmitCmd(null, string.Format(CultureInfo.InvariantCulture, "steam_lobby_assign_owner {0}", id));
				});
			}
		}
	}

	public TextMeshProUGUI lobbyStateText;

	public GameObject lobbyControlPanel;

	public GameObject contentArea;

	public RectTransform playerListContainer;

	[Tooltip("The panel which acts as a container for UI that's only valid if there's an active lobby, like the lobby type or the copy-ID-to-clipboard button.")]
	public MPButton createLobbyButton;

	public MPButton leaveLobbyButton;

	public MPButton copyLobbyButton;

	public MPButton joinLobbyButton;

	public MPDropdown lobbyTypeDropdown;

	private List<UserElement> userList = new List<UserElement>();

	public static readonly LobbyStateChoice[] lobbyStateChoices = new LobbyStateChoice[3]
	{
		new LobbyStateChoice
		{
			lobbyType = LobbyType.Private,
			token = "STEAM_LOBBY_PRIVATE"
		},
		new LobbyStateChoice
		{
			lobbyType = LobbyType.FriendsOnly,
			token = "STEAM_LOBBY_FRIENDSONLY"
		},
		new LobbyStateChoice
		{
			lobbyType = LobbyType.Public,
			token = "STEAM_LOBBY_PUBLIC"
		}
	};

	private static readonly List<string> optionsBuffer = new List<string>();

	public int NumberOfParticipants { get; private set; }

	private bool isInLobby => PlatformSystems.lobbyManager.isInLobby;

	private void Awake()
	{
		((UnityEvent<int>)(object)((TMP_Dropdown)lobbyTypeDropdown).onValueChanged).AddListener((UnityAction<int>)OnLobbyStateDropdownValueChanged);
	}

	private void OnEnable()
	{
		LobbyManager lobbyManager = PlatformSystems.lobbyManager;
		lobbyManager.onLobbyChanged = (Action)Delegate.Combine(lobbyManager.onLobbyChanged, new Action(OnLobbyChanged));
		LobbyManager lobbyManager2 = PlatformSystems.lobbyManager;
		lobbyManager2.onLobbyDataUpdated = (Action)Delegate.Combine(lobbyManager2.onLobbyDataUpdated, new Action(RebuildPlayers));
		LobbyManager lobbyManager3 = PlatformSystems.lobbyManager;
		lobbyManager3.onLobbyStateChanged = (Action)Delegate.Combine(lobbyManager3.onLobbyStateChanged, new Action(OnLobbyStateChanged));
		LobbyManager lobbyManager4 = PlatformSystems.lobbyManager;
		lobbyManager4.onLobbyMemberDataUpdated = (Action<UserID>)Delegate.Combine(lobbyManager4.onLobbyMemberDataUpdated, new Action<UserID>(OnLobbyMemberDataUpdated));
		LobbyManager lobbyManager5 = PlatformSystems.lobbyManager;
		lobbyManager5.onPlayerCountUpdated = (Action)Delegate.Combine(lobbyManager5.onPlayerCountUpdated, new Action(RebuildPlayers));
		RebuildLobbyStateDropdownOptions();
		Refresh();
	}

	private void OnDisable()
	{
		LobbyManager lobbyManager = PlatformSystems.lobbyManager;
		lobbyManager.onLobbyChanged = (Action)Delegate.Remove(lobbyManager.onLobbyChanged, new Action(OnLobbyChanged));
		LobbyManager lobbyManager2 = PlatformSystems.lobbyManager;
		lobbyManager2.onLobbyDataUpdated = (Action)Delegate.Remove(lobbyManager2.onLobbyDataUpdated, new Action(RebuildPlayers));
		LobbyManager lobbyManager3 = PlatformSystems.lobbyManager;
		lobbyManager3.onLobbyStateChanged = (Action)Delegate.Remove(lobbyManager3.onLobbyStateChanged, new Action(OnLobbyStateChanged));
		LobbyManager lobbyManager4 = PlatformSystems.lobbyManager;
		lobbyManager4.onLobbyMemberDataUpdated = (Action<UserID>)Delegate.Remove(lobbyManager4.onLobbyMemberDataUpdated, new Action<UserID>(OnLobbyMemberDataUpdated));
		LobbyManager lobbyManager5 = PlatformSystems.lobbyManager;
		lobbyManager5.onPlayerCountUpdated = (Action)Delegate.Remove(lobbyManager5.onPlayerCountUpdated, new Action(RebuildPlayers));
	}

	private void Update()
	{
		Refresh();
	}

	internal void ToggleActions(bool val)
	{
		((Component)createLobbyButton).gameObject.SetActive(val);
		((Component)leaveLobbyButton).gameObject.SetActive(val);
		((Component)copyLobbyButton).gameObject.SetActive(val);
	}

	public void Refresh()
	{
		if (PlatformSystems.lobbyManager.HasMPLobbyUI())
		{
			if (lobbyControlPanel.activeSelf != isInLobby)
			{
				lobbyControlPanel.SetActive(isInLobby);
			}
			if (Object.op_Implicit((Object)(object)createLobbyButton) && PlatformSystems.lobbyManager.HasMPLobbyFeature(MPLobbyFeatures.CreateLobby) && ((Component)createLobbyButton).gameObject.activeSelf == isInLobby)
			{
				((Component)createLobbyButton).gameObject.SetActive(!isInLobby);
			}
			if (Object.op_Implicit((Object)(object)leaveLobbyButton) && PlatformSystems.lobbyManager.HasMPLobbyFeature(MPLobbyFeatures.LeaveLobby) && ((Component)leaveLobbyButton).gameObject.activeSelf != isInLobby)
			{
				((Component)leaveLobbyButton).gameObject.SetActive(isInLobby);
			}
			if (Object.op_Implicit((Object)(object)copyLobbyButton) && PlatformSystems.lobbyManager.HasMPLobbyFeature(MPLobbyFeatures.Clipboard))
			{
				((Selectable)copyLobbyButton).interactable = isInLobby;
				((Component)copyLobbyButton).gameObject.SetActive(isInLobby);
			}
			if (lobbyControlPanel.activeInHierarchy && PlatformSystems.lobbyManager.HasMPLobbyFeature(MPLobbyFeatures.LobbyDropdownOptions))
			{
				((Selectable)lobbyTypeDropdown).interactable = PlatformSystems.lobbyManager.ownsLobby;
				LobbyType currentLobbyType = PlatformSystems.lobbyManager.currentLobbyType;
				for (int i = 0; i < lobbyStateChoices.Length; i++)
				{
					if (currentLobbyType == lobbyStateChoices[i].lobbyType)
					{
						((TMP_Dropdown)lobbyTypeDropdown).SetValueWithoutNotify(i);
						break;
					}
				}
			}
		}
		RebuildPlayers();
	}

	private void RebuildLobbyStateDropdownOptions()
	{
		if (PlatformSystems.lobbyManager.HasMPLobbyUI())
		{
			for (int i = 0; i < lobbyStateChoices.Length; i++)
			{
				optionsBuffer.Add(Language.GetString(lobbyStateChoices[i].token));
			}
			((TMP_Dropdown)lobbyTypeDropdown).ClearOptions();
			((TMP_Dropdown)lobbyTypeDropdown).AddOptions(optionsBuffer);
			optionsBuffer.Clear();
		}
	}

	private void OnLobbyStateDropdownValueChanged(int newValue)
	{
		if (isInLobby)
		{
			PlatformSystems.lobbyManager.currentLobbyType = lobbyStateChoices[newValue].lobbyType;
			Refresh();
		}
	}

	public void ClearUserList()
	{
		while (userList.Count > 0)
		{
			int index = userList.Count - 1;
			Object.Destroy((Object)(object)userList[index].gameObject);
			userList.RemoveAt(index);
		}
	}

	public void RebuildPlayers()
	{
		if (!Object.op_Implicit((Object)(object)playerListContainer) || !Object.op_Implicit((Object)(object)((Component)playerListContainer).gameObject) || !((Component)playerListContainer).gameObject.activeInHierarchy)
		{
			return;
		}
		bool num = isInLobby;
		UserID[] lobbyMembers = PlatformSystems.lobbyManager.GetLobbyMembers();
		int num2 = Math.Min(num ? (lobbyMembers.Length + 1) : 0, RoR2Application.maxPlayers);
		while (userList.Count > num2)
		{
			int index = userList.Count - 1;
			Object.Destroy((Object)(object)userList[index].gameObject);
			userList.RemoveAt(index);
		}
		while (userList.Count < num2)
		{
			GameObject val = Object.Instantiate<GameObject>(LegacyResourcesAPI.Load<GameObject>("Prefabs/UI/SteamLobbyUserListElement"), (Transform)(object)playerListContainer);
			val.SetActive(true);
			SocialUserIcon componentInChildren = val.GetComponentInChildren<SocialUserIcon>();
			SteamUsernameLabel componentInChildren2 = val.GetComponentInChildren<SteamUsernameLabel>();
			ChildLocator component = val.GetComponent<ChildLocator>();
			userList.Add(new UserElement
			{
				gameObject = val,
				userIcon = componentInChildren,
				usernameLabel = componentInChildren2,
				elementChildLocator = component
			});
		}
		if (lobbyMembers != null)
		{
			int i;
			for (i = 0; i < lobbyMembers.Length && i < userList.Count; i++)
			{
				userList[i].SetUser(lobbyMembers[i], i);
			}
			for (; i < num2; i++)
			{
				userList[i].SetUser(default(UserID), 0);
			}
		}
	}

	private void UpdateUser(UserID userId)
	{
		for (int i = 0; i < userList.Count; i++)
		{
			if (userList[i].id == userId)
			{
				userList[i].Refresh();
			}
		}
	}

	private void OnLobbyStateChanged()
	{
		((Selectable)lobbyTypeDropdown).interactable = PlatformSystems.lobbyManager.ownsLobby;
		Refresh();
	}

	private void OnLobbyMemberDataUpdated(UserID steamId)
	{
		UpdateUser(steamId);
	}

	private void OnLobbyChanged()
	{
		Refresh();
	}
}
