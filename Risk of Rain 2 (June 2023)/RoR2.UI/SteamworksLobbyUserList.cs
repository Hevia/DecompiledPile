using System;
using System.Collections.Generic;
using System.Globalization;
using Facepunch.Steamworks;
using RoR2.Networking;
using TMPro;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

namespace RoR2.UI;

public class SteamworksLobbyUserList : MonoBehaviour
{
	public struct LobbyStateChoice
	{
		public Type lobbyType;

		public string token;
	}

	private class Element
	{
		public ulong steamId;

		public GameObject gameObject;

		public SocialUserIcon userIcon;

		public SteamUsernameLabel usernameLabel;

		public GameObject lobbyLeaderCrown;

		public ChildLocator elementChildLocator;

		public void SetUser(ulong steamId, int subPlayerIndex)
		{
			this.steamId = steamId;
			userIcon.RefreshWithUser(new UserID(steamId));
			usernameLabel.subPlayerIndex = subPlayerIndex;
			Refresh();
		}

		public void Refresh()
		{
			if (steamId == 0L)
			{
				((Component)elementChildLocator.FindChild("UserIcon")).gameObject.SetActive(false);
				((Component)elementChildLocator.FindChild("InviteButton")).gameObject.SetActive(true);
			}
			else
			{
				((Component)elementChildLocator.FindChild("UserIcon")).gameObject.SetActive(true);
				((Component)elementChildLocator.FindChild("InviteButton")).gameObject.SetActive(false);
			}
			userIcon.Refresh();
			usernameLabel.Refresh();
			RefreshCrownAndPromoteButton();
		}

		private void RefreshCrownAndPromoteButton()
		{
			//IL_0100: Unknown result type (might be due to invalid IL or missing references)
			//IL_010a: Expected O, but got Unknown
			if (Client.Instance == null)
			{
				return;
			}
			bool flag = Client.Instance.Lobby.Owner == steamId && steamId != 0;
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
			if (!Object.op_Implicit((Object)(object)elementChildLocator))
			{
				return;
			}
			bool flag2 = !flag && PlatformSystems.lobbyManager.ownsLobby && steamId != 0L && !SteamLobbyFinder.running && !Object.op_Implicit((Object)(object)NetworkSession.instance);
			GameObject val = ((Component)elementChildLocator.FindChild("PromoteButton")).gameObject;
			if (!Object.op_Implicit((Object)(object)val))
			{
				return;
			}
			val.SetActive(flag2);
			if (!flag2)
			{
				return;
			}
			MPButton component = val.GetComponent<MPButton>();
			if (Object.op_Implicit((Object)(object)component))
			{
				((UnityEventBase)((Button)component).onClick).RemoveAllListeners();
				((UnityEvent)((Button)component).onClick).AddListener((UnityAction)delegate
				{
					Console.instance.SubmitCmd(null, string.Format(CultureInfo.InvariantCulture, "steam_lobby_assign_owner {0}", TextSerialization.ToStringInvariant(steamId)));
				});
			}
		}
	}

	[Tooltip("The panel which acts as a container for UI that's only valid if there's an active lobby, like the lobby type or the copy-ID-to-clipboard button.")]
	public GameObject lobbyControlPanel;

	public MPButton createLobbyButton;

	public MPButton leaveLobbyButton;

	public MPButton copyLobbyButton;

	public MPDropdown lobbyTypeDropdown;

	public RectTransform playerListContainer;

	public static readonly LobbyStateChoice[] lobbyStateChoices = new LobbyStateChoice[3]
	{
		new LobbyStateChoice
		{
			lobbyType = (Type)0,
			token = "STEAM_LOBBY_PRIVATE"
		},
		new LobbyStateChoice
		{
			lobbyType = (Type)1,
			token = "STEAM_LOBBY_FRIENDSONLY"
		},
		new LobbyStateChoice
		{
			lobbyType = (Type)2,
			token = "STEAM_LOBBY_PUBLIC"
		}
	};

	private static readonly List<string> optionsBuffer = new List<string>();

	private List<Element> elements = new List<Element>();

	private bool validLobbyExists => PlatformSystems.lobbyManager.isInLobby;

	private void Awake()
	{
		((UnityEvent<int>)(object)((TMP_Dropdown)lobbyTypeDropdown).onValueChanged).AddListener((UnityAction<int>)OnLobbyStateDropdownValueChanged);
		if (Client.Instance == null)
		{
			((Behaviour)this).enabled = false;
		}
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

	public void Refresh()
	{
		//IL_0104: Unknown result type (might be due to invalid IL or missing references)
		//IL_0109: Unknown result type (might be due to invalid IL or missing references)
		//IL_010e: Unknown result type (might be due to invalid IL or missing references)
		//IL_011a: Unknown result type (might be due to invalid IL or missing references)
		if (lobbyControlPanel.activeSelf != validLobbyExists)
		{
			lobbyControlPanel.SetActive(validLobbyExists);
		}
		if (Object.op_Implicit((Object)(object)createLobbyButton) && ((Component)createLobbyButton).gameObject.activeSelf == validLobbyExists)
		{
			((Component)createLobbyButton).gameObject.SetActive(!validLobbyExists);
		}
		if (Object.op_Implicit((Object)(object)leaveLobbyButton) && ((Component)leaveLobbyButton).gameObject.activeSelf != validLobbyExists)
		{
			((Component)leaveLobbyButton).gameObject.SetActive(validLobbyExists);
		}
		if (Object.op_Implicit((Object)(object)copyLobbyButton) && ((Component)copyLobbyButton).gameObject.activeSelf != validLobbyExists)
		{
			((Component)copyLobbyButton).gameObject.SetActive(validLobbyExists);
		}
		if (lobbyControlPanel.activeInHierarchy)
		{
			((Selectable)lobbyTypeDropdown).interactable = PlatformSystems.lobbyManager.ownsLobby;
			Type lobbyType = Client.Instance.Lobby.LobbyType;
			for (int i = 0; i < lobbyStateChoices.Length; i++)
			{
				if (lobbyType == lobbyStateChoices[i].lobbyType)
				{
					((TMP_Dropdown)lobbyTypeDropdown).SetValueWithoutNotify(i);
					break;
				}
			}
		}
		RebuildPlayers();
	}

	private void RebuildLobbyStateDropdownOptions()
	{
		for (int i = 0; i < lobbyStateChoices.Length; i++)
		{
			optionsBuffer.Add(Language.GetString(lobbyStateChoices[i].token));
		}
		((TMP_Dropdown)lobbyTypeDropdown).ClearOptions();
		((TMP_Dropdown)lobbyTypeDropdown).AddOptions(optionsBuffer);
		optionsBuffer.Clear();
	}

	private void OnLobbyStateDropdownValueChanged(int newValue)
	{
		//IL_001e: Unknown result type (might be due to invalid IL or missing references)
		if (validLobbyExists)
		{
			Client.Instance.Lobby.LobbyType = lobbyStateChoices[newValue].lobbyType;
			Refresh();
		}
	}

	public void RebuildPlayers()
	{
		if (!Object.op_Implicit((Object)(object)playerListContainer) || !Object.op_Implicit((Object)(object)((Component)playerListContainer).gameObject) || !((Component)playerListContainer).gameObject.activeInHierarchy)
		{
			return;
		}
		bool num = validLobbyExists;
		Client instance = Client.Instance;
		object obj;
		if (instance == null)
		{
			obj = null;
		}
		else
		{
			Lobby lobby = instance.Lobby;
			obj = ((lobby != null) ? lobby.GetMemberIDs() : null);
		}
		ulong[] array = (ulong[])obj;
		int num2 = Math.Max(num ? RoR2Application.maxPlayers : 0, 0);
		while (elements.Count > num2)
		{
			int index = elements.Count - 1;
			Object.Destroy((Object)(object)elements[index].gameObject);
			elements.RemoveAt(index);
		}
		while (elements.Count < num2)
		{
			GameObject val = Object.Instantiate<GameObject>(LegacyResourcesAPI.Load<GameObject>("Prefabs/UI/SteamLobbyUserListElement"), (Transform)(object)playerListContainer);
			val.SetActive(true);
			SocialUserIcon componentInChildren = val.GetComponentInChildren<SocialUserIcon>();
			SteamUsernameLabel componentInChildren2 = val.GetComponentInChildren<SteamUsernameLabel>();
			ChildLocator component = val.GetComponent<ChildLocator>();
			elements.Add(new Element
			{
				gameObject = val,
				userIcon = componentInChildren,
				usernameLabel = componentInChildren2,
				elementChildLocator = component
			});
		}
		if (array == null)
		{
			return;
		}
		int i = 0;
		for (int j = 0; j < array.Length; j++)
		{
			int lobbyMemberPlayerCountByIndex = PlatformSystems.lobbyManager.GetLobbyMemberPlayerCountByIndex(j);
			for (int k = 0; k < lobbyMemberPlayerCountByIndex; k++)
			{
				if (i >= elements.Count)
				{
					break;
				}
				elements[i++].SetUser(array[j], k);
			}
		}
		for (; i < num2 && i < elements.Count; i++)
		{
			elements[i].SetUser(0uL, 0);
		}
	}

	private void UpdateUser(ulong userId)
	{
		for (int i = 0; i < elements.Count; i++)
		{
			if (elements[i].steamId == userId)
			{
				elements[i].Refresh();
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
		UpdateUser(steamId.ID);
	}

	private void OnLobbyChanged()
	{
		Refresh();
	}
}
