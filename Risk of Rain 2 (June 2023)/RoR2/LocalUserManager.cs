using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using Rewired;
using RoR2.ConVar;
using RoR2.UI;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.SceneManagement;

namespace RoR2;

public static class LocalUserManager
{
	public struct LocalUserInitializationInfo
	{
		public Player player;

		public UserProfile profile;
	}

	private class UserProfileMainConVar : BaseConVar
	{
		public static UserProfileMainConVar instance = new UserProfileMainConVar("user_profile_main", ConVarFlags.Archive, null, "The current user profile.");

		public string lastReceivedValue;

		public UserProfileMainConVar(string name, ConVarFlags flags, string defaultValue, string helpText)
			: base(name, flags, defaultValue, helpText)
		{
		}

		public override void SetString(string newValue)
		{
			lastReceivedValue = newValue;
			if (isAnyUserSignedIn)
			{
				Debug.Log((object)"Can't change user_profile_main while there are users signed in.");
			}
			else if (ready)
			{
				AddMainUser(newValue);
			}
		}

		public override string GetString()
		{
			int num = FindUserIndex(GetRewiredMainPlayer());
			if (num == -1)
			{
				return "";
			}
			return localUsersList[num].userProfile.fileName;
		}
	}

	private static readonly List<LocalUser> localUsersList = new List<LocalUser>();

	public static readonly ReadOnlyCollection<LocalUser> readOnlyLocalUsersList = localUsersList.AsReadOnly();

	public static Player startPlayer;

	private static bool ready = false;

	public static bool isAnyUserSignedIn => localUsersList.Count > 0;

	public static event Action<LocalUser> onUserSignIn;

	public static event Action<LocalUser> onUserSignOut;

	public static event Action onLocalUsersUpdated;

	public static bool UserExists(Player inputPlayer)
	{
		for (int i = 0; i < localUsersList.Count; i++)
		{
			if (localUsersList[i].inputPlayer == inputPlayer)
			{
				return true;
			}
		}
		return false;
	}

	private static int FindUserIndex(int userId)
	{
		for (int i = 0; i < localUsersList.Count; i++)
		{
			if (localUsersList[i].id == userId)
			{
				return i;
			}
		}
		return -1;
	}

	public static LocalUser FindLocalUser(int userId)
	{
		for (int i = 0; i < localUsersList.Count; i++)
		{
			if (localUsersList[i].id == userId)
			{
				return localUsersList[i];
			}
		}
		return null;
	}

	public static LocalUser FindLocalUser(Player inputPlayer)
	{
		for (int i = 0; i < localUsersList.Count; i++)
		{
			if (localUsersList[i].inputPlayer == inputPlayer)
			{
				return localUsersList[i];
			}
		}
		return null;
	}

	public static LocalUser GetFirstLocalUser()
	{
		if (localUsersList.Count <= 0)
		{
			return null;
		}
		return localUsersList[0];
	}

	private static int FindUserIndex(Player inputPlayer)
	{
		for (int i = 0; i < localUsersList.Count; i++)
		{
			if (localUsersList[i].inputPlayer == inputPlayer)
			{
				return i;
			}
		}
		return -1;
	}

	private static int GetFirstAvailableId()
	{
		int i;
		for (i = 0; i < localUsersList.Count; i++)
		{
			if (FindUserIndex(i) == -1)
			{
				return i;
			}
		}
		return i;
	}

	private static void AddUser(Player inputPlayer, UserProfile userProfile)
	{
		if (!UserExists(inputPlayer))
		{
			int firstAvailableId = GetFirstAvailableId();
			LocalUser localUser = new LocalUser
			{
				inputPlayer = inputPlayer,
				id = firstAvailableId,
				userProfile = userProfile
			};
			localUsersList.Add(localUser);
			userProfile.OnLogin();
			MPEventSystem.FindByPlayer(inputPlayer).localUser = localUser;
			if (LocalUserManager.onUserSignIn != null)
			{
				LocalUserManager.onUserSignIn(localUser);
			}
			if (LocalUserManager.onLocalUsersUpdated != null)
			{
				LocalUserManager.onLocalUsersUpdated();
			}
		}
	}

	public static bool IsUserChangeSafe()
	{
		//IL_0000: Unknown result type (might be due to invalid IL or missing references)
		//IL_0005: Unknown result type (might be due to invalid IL or missing references)
		Scene activeScene = SceneManager.GetActiveScene();
		if (((Scene)(ref activeScene)).name == "title")
		{
			return true;
		}
		return false;
	}

	public static void SetLocalUsers(LocalUserInitializationInfo[] initializationInfo)
	{
		if (localUsersList.Count > 0)
		{
			Debug.LogError((object)"Cannot call LocalUserManager.SetLocalUsers while users are already signed in!");
			return;
		}
		if (!IsUserChangeSafe())
		{
			Debug.LogError((object)"Cannot call LocalUserManager.SetLocalUsers at this time, user login changes are not safe at this time.");
			return;
		}
		if (initializationInfo.Length == 1)
		{
			initializationInfo[0].player = GetRewiredMainPlayer();
		}
		for (int i = 0; i < initializationInfo.Length; i++)
		{
			AddUser(initializationInfo[i].player, initializationInfo[i].profile);
		}
	}

	private static Player GetRewiredMainPlayer()
	{
		return ReInput.players.GetPlayer("PlayerMain");
	}

	private static void AddMainUser(UserProfile userProfile)
	{
		AddUser(GetRewiredMainPlayer(), userProfile);
	}

	private static bool AddMainUser(string userProfileName)
	{
		UserProfile profile = PlatformSystems.saveSystem.GetProfile(userProfileName);
		if (profile != null && !profile.isCorrupted)
		{
			AddMainUser(profile);
			return true;
		}
		return false;
	}

	private static void RemoveUser(Player inputPlayer)
	{
		int num = FindUserIndex(inputPlayer);
		if (num != -1)
		{
			RemoveUser(num);
		}
	}

	private static void RemoveUser(int userIndex)
	{
		LocalUser localUser = localUsersList[userIndex];
		if (LocalUserManager.onUserSignOut != null)
		{
			LocalUserManager.onUserSignOut(localUser);
		}
		localUser.userProfile.OnLogout();
		MPEventSystem.FindByPlayer(localUser.inputPlayer).localUser = null;
		localUsersList.RemoveAt(userIndex);
		if (LocalUserManager.onLocalUsersUpdated != null)
		{
			LocalUserManager.onLocalUsersUpdated();
		}
	}

	public static void ClearUsers()
	{
		if (!IsUserChangeSafe())
		{
			Debug.LogError((object)"Cannot call LocalUserManager.SetLocalUsers at this time, user login changes are not safe at this time.");
			return;
		}
		for (int num = localUsersList.Count - 1; num >= 0; num--)
		{
			RemoveUser(num);
		}
	}

	private static Player ListenForStartSignIn()
	{
		IList<Player> players = ReInput.players.Players;
		for (int i = 0; i < players.Count; i++)
		{
			Player val = players[i];
			if (!(val.name == "PlayerMain") && !UserExists(val) && val.GetButtonDown(11))
			{
				return val;
			}
		}
		return null;
	}

	public static void Init()
	{
		ready = true;
		RoR2Application.onUpdate += Update;
		SaveSystem.onAvailableUserProfilesChanged += LogInWithPreviousSessionUser;
		static void LogInWithPreviousSessionUser()
		{
			if (UserProfileMainConVar.instance.lastReceivedValue != null)
			{
				AddMainUser(UserProfileMainConVar.instance.lastReceivedValue);
			}
			SaveSystem.onAvailableUserProfilesChanged -= LogInWithPreviousSessionUser;
		}
	}

	private static void Update()
	{
		for (int i = 0; i < localUsersList.Count; i++)
		{
			localUsersList[i].RebuildControlChain();
		}
	}

	[ConCommand(commandName = "remove_all_local_users", flags = ConVarFlags.None, helpText = "Removes all local users.")]
	private static void CCRemoveAllLocalUsers(ConCommandArgs args)
	{
		ClearUsers();
	}

	[ConCommand(commandName = "print_local_users", flags = ConVarFlags.None, helpText = "Prints a list of all local users.")]
	private static void CCPrintLocalUsers(ConCommandArgs args)
	{
		string[] array = new string[localUsersList.Count];
		for (int i = 0; i < localUsersList.Count; i++)
		{
			if (localUsersList[i] != null)
			{
				array[i] = string.Format("localUsersList[{0}] id={1} userProfile={2}", i, localUsersList[i].id, (localUsersList[i].userProfile != null) ? localUsersList[i].userProfile.fileName : "null");
			}
			else
			{
				array[i] = $"localUsersList[{i}] null";
			}
		}
		Debug.Log((object)string.Join("\n", array));
	}

	[ConCommand(commandName = "test_splitscreen", flags = ConVarFlags.None, helpText = "Logs in the specified number of guest users, or two by default.")]
	private static void CCTestSplitscreen(ConCommandArgs args)
	{
		int num = 2;
		if (args.Count >= 1 && TextSerialization.TryParseInvariant(args[0], out int result))
		{
			num = Mathf.Clamp(result, 1, 4);
		}
		if (!NetworkClient.active)
		{
			ClearUsers();
			LocalUserInitializationInfo[] array = new LocalUserInitializationInfo[num];
			for (int i = 0; i < num; i++)
			{
				array[i] = new LocalUserInitializationInfo
				{
					player = ReInput.players.GetPlayer(2 + i),
					profile = PlatformSystems.saveSystem.CreateGuestProfile()
				};
			}
			SetLocalUsers(array);
		}
	}

	[ConCommand(commandName = "export_controller_maps", flags = ConVarFlags.None, helpText = "Prints all Rewired ControllerMaps of the first player as xml.")]
	public static void CCExportControllerMaps(ConCommandArgs args)
	{
		if (localUsersList.Count <= 0)
		{
			return;
		}
		foreach (string item in from v in localUsersList[0].inputPlayer.controllers.maps.GetAllMaps()
			select v.ToXmlString())
		{
			Debug.Log((object)item);
		}
	}
}
