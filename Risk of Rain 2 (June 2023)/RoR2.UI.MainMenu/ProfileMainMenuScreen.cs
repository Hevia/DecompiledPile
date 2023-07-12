using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using Zio;

namespace RoR2.UI.MainMenu;

[RequireComponent(typeof(MPEventSystemLocator))]
public class ProfileMainMenuScreen : BaseMainMenuScreen
{
	public GameObject createProfilePanel;

	public TMP_InputField createProfileNameInputField;

	public MPButton submitProfileNameButton;

	public GameObject gotoSelectProfilePanelButtonContainer;

	public GameObject selectProfilePanel;

	public MPButton gotoCreateProfilePanelButton;

	public UserProfileListController existingProfileListController;

	private MPEventSystemLocator eventSystemLocator;

	private bool firstTimeConfiguration;

	private const string defaultName = "Nameless Survivor";

	private string GuessDefaultProfileName()
	{
		return PlatformSystems.saveSystem.GetPlatformUsernameOrDefault("Nameless Survivor");
	}

	public UserProfile Editor_TryOpenTestProfile()
	{
		List<string> availableProfileNames = PlatformSystems.saveSystem.GetAvailableProfileNames();
		if (availableProfileNames.Count > 0)
		{
			Debug.LogWarning((object)"Got existing profile!");
			return PlatformSystems.saveSystem.GetProfile(availableProfileNames[0]);
		}
		return null;
	}

	public void OpenDefaultProfile()
	{
		string name = GuessDefaultProfileName();
		UserProfile userProfile = null;
		userProfile = PlatformSystems.saveSystem.CreateProfile((IFileSystem)(object)RoR2Application.cloudStorage, name);
		SetMainProfile(userProfile);
	}

	protected new void Awake()
	{
		eventSystemLocator = ((Component)this).GetComponent<MPEventSystemLocator>();
		existingProfileListController.onProfileSelected += SetMainProfile;
		existingProfileListController.onListRebuilt += OnListRebuilt;
	}

	protected void OnEnable()
	{
		firstTimeConfiguration = true;
		List<string> availableProfileNames = PlatformSystems.saveSystem.GetAvailableProfileNames();
		for (int i = 0; i < availableProfileNames.Count; i++)
		{
			if (IsProfileCustom(PlatformSystems.saveSystem.GetProfile(availableProfileNames[i])))
			{
				firstTimeConfiguration = false;
				break;
			}
		}
		if (firstTimeConfiguration)
		{
			Debug.Log((object)"First-Time Profile Configuration");
			OpenCreateProfileMenu(firstTime: true);
			return;
		}
		createProfilePanel.SetActive(false);
		selectProfilePanel.SetActive(true);
		OnListRebuilt();
		gotoSelectProfilePanelButtonContainer.SetActive(true);
	}

	public void OpenCreateProfileMenu(bool firstTime)
	{
		selectProfilePanel.SetActive(false);
		createProfilePanel.SetActive(true);
		createProfileNameInputField.text = GuessDefaultProfileName();
		if (firstTime)
		{
			gotoSelectProfilePanelButtonContainer.SetActive(false);
		}
	}

	private void OnListRebuilt()
	{
		existingProfileListController.GetReadOnlyElementsList();
	}

	protected void OnDisable()
	{
	}

	private void SetMainProfile(UserProfile profile)
	{
		LocalUserManager.SetLocalUsers(new LocalUserManager.LocalUserInitializationInfo[1]
		{
			new LocalUserManager.LocalUserInitializationInfo
			{
				profile = profile
			}
		});
		if ((Object)(object)myMainMenuController != (Object)null)
		{
			myMainMenuController.desiredMenuScreen = myMainMenuController.titleMenuScreen;
		}
		else
		{
			Debug.LogError((object)"myMainMenuController reference null on ProfileMainMenuScreen.cs while trying to run SetMainProfile(UserProfile profile)");
		}
	}

	private static bool IsProfileCustom(UserProfile profile)
	{
		return profile.fileName != "default";
	}

	private static bool IsNewProfileNameAcceptable(string newProfileName)
	{
		if (PlatformSystems.saveSystem.GetProfile(newProfileName) != null)
		{
			return false;
		}
		if (newProfileName == "")
		{
			return false;
		}
		return true;
	}

	public void OnAddProfilePressed()
	{
		if ((Object)(object)((EventSystem)eventSystemLocator.eventSystem).currentSelectedGameObject == (Object)(object)((Component)createProfileNameInputField).gameObject && !Input.GetKeyDown((KeyCode)13) && !Input.GetKeyDown((KeyCode)271))
		{
			return;
		}
		string text = createProfileNameInputField.text;
		if (IsNewProfileNameAcceptable(text))
		{
			createProfileNameInputField.text = "";
			UserProfile userProfile = PlatformSystems.saveSystem.CreateProfile((IFileSystem)(object)RoR2Application.cloudStorage, text);
			if (userProfile != null)
			{
				SetMainProfile(userProfile);
			}
		}
	}

	protected new void Update()
	{
		if (!Object.op_Implicit((Object)(object)eventSystemLocator.eventSystem) || eventSystemLocator.eventSystem.player == null || !eventSystemLocator.eventSystem.player.GetButton(31))
		{
			return;
		}
		GameObject currentSelectedGameObject = ((EventSystem)MPEventSystemManager.combinedEventSystem).currentSelectedGameObject;
		if (!Object.op_Implicit((Object)(object)currentSelectedGameObject))
		{
			return;
		}
		UserProfileListElementController component = currentSelectedGameObject.GetComponent<UserProfileListElementController>();
		if (Object.op_Implicit((Object)(object)component))
		{
			if (component.userProfile == null)
			{
				Debug.LogError((object)"!!!???");
				return;
			}
			SimpleDialogBox simpleDialogBox = SimpleDialogBox.Create();
			string consoleString = "user_profile_delete \"" + component.userProfile.fileName + "\"";
			simpleDialogBox.headerToken = new SimpleDialogBox.TokenParamsPair
			{
				token = "USER_PROFILE_DELETE_HEADER",
				formatParams = null
			};
			simpleDialogBox.descriptionToken = new SimpleDialogBox.TokenParamsPair
			{
				token = "USER_PROFILE_DELETE_DESCRIPTION",
				formatParams = new object[1] { component.userProfile.name }
			};
			simpleDialogBox.AddCommandButton(consoleString, "USER_PROFILE_DELETE_YES");
			simpleDialogBox.AddCancelButton("USER_PROFILE_DELETE_NO");
		}
	}
}
