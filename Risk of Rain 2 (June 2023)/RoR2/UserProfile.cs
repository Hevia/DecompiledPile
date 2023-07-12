using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Xml.Linq;
using Facepunch.Steamworks;
using JetBrains.Annotations;
using Rewired;
using RoR2.Stats;
using UnityEngine;
using Zio;

namespace RoR2;

public class UserProfile
{
	public struct TutorialProgression
	{
		public uint showCount;

		public bool shouldShow;
	}

	public bool isClaimed;

	public bool canSave;

	public string fileName;

	public IFileSystem fileSystem;

	public UPath filePath = UPath.Empty;

	[SaveField]
	public string name;

	[SaveField]
	public uint coins;

	[SaveField]
	public uint totalCollectedCoins;

	[SaveField]
	public string version = "2";

	[SaveField]
	public float screenShakeScale = 1f;

	[SaveField(explicitSetupMethod = "SetupKeyboardMap")]
	public KeyboardMap keyboardMap = new KeyboardMap(DefaultControllerMaps.defaultKeyboardMap);

	[SaveField(explicitSetupMethod = "SetupMouseMap")]
	public MouseMap mouseMap = new MouseMap(DefaultControllerMaps.defaultMouseMap);

	[SaveField(explicitSetupMethod = "SetupJoystickMap")]
	public JoystickMap joystickMap = new JoystickMap(DefaultControllerMaps.defaultJoystickMap);

	[SaveField]
	public float mouseLookSensitivity = 0.25f;

	[SaveField]
	public float mouseLookScaleX = 1f;

	[SaveField]
	public float mouseLookScaleY = 1f;

	[SaveField]
	public bool mouseLookInvertX;

	[SaveField]
	public bool mouseLookInvertY;

	[SaveField]
	public float stickLookSensitivity = 4f;

	[SaveField]
	public float stickLookScaleX = 1f;

	[SaveField]
	public float stickLookScaleY = 1f;

	[SaveField]
	public bool stickLookInvertX;

	[SaveField]
	public bool stickLookInvertY;

	[SaveField]
	public float gamepadVibrationScale = 1f;

	public bool saveRequestPending;

	private static string[] saveFieldNames;

	public static SaveFieldAttribute[] saveFields;

	private static readonly Dictionary<string, SaveFieldAttribute> nameToSaveFieldMap = new Dictionary<string, SaveFieldAttribute>();

	public static UserProfile defaultProfile;

	[SaveField(explicitSetupMethod = "SetupTokenList")]
	public List<string> viewedUnlockablesList = new List<string>();

	[SaveField(explicitSetupMethod = "SetupPickupsSet")]
	private readonly bool[] discoveredPickups = PickupCatalog.GetPerPickupBuffer<bool>();

	public Action<PickupIndex> onPickupDiscovered;

	public Action onStatsReceived;

	[SaveField(explicitSetupMethod = "SetupTokenList")]
	private List<string> achievementsList = new List<string>();

	[SaveField(explicitSetupMethod = "SetupTokenList")]
	private List<string> unviewedAchievementsList = new List<string>();

	public StatSheet statSheet = StatSheet.New();

	private const uint maxShowCount = 3u;

	public TutorialProgression tutorialDifficulty;

	public TutorialProgression tutorialSprint;

	public TutorialProgression tutorialEquipment;

	[SaveField]
	private SurvivorDef survivorPreference = SurvivorCatalog.defaultSurvivor;

	public readonly Loadout loadout = new Loadout();

	[SaveField]
	public uint totalLoginSeconds;

	[SaveField]
	public uint totalRunSeconds;

	[SaveField]
	public uint totalAliveSeconds;

	[SaveField]
	public uint totalRunCount;

	private bool ownsPortrait;

	private const string defaultProfileContents = "<UserProfile>\r\n  <name>Survivor</name>\r\n  <mouseLookSensitivity>0.2</mouseLookSensitivity>\r\n  <mouseLookScaleX>1</mouseLookScaleX>\r\n  <mouseLookScaleY>1</mouseLookScaleY>\r\n  <stickLookSensitivity>5</stickLookSensitivity>\r\n  <stickLookScaleX>1</stickLookScaleX>\r\n  <stickLookScaleY>1</stickLookScaleY>\r\n</UserProfile>";

	[SaveField(defaultValue = "", explicitSetupMethod = "SetupTokenList", fieldName = "viewedViewables")]
	private readonly List<string> viewedViewables = new List<string>();

	public bool isCorrupted { get; set; }

	public bool hasUnviewedAchievement => unviewedAchievementsList.Count > 0;

	public bool loggedIn { get; set; }

	public Texture portraitTexture { get; private set; }

	public event Action onSurvivorPreferenceChanged;

	public static event Action<UserProfile> onSurvivorPreferenceChangedGlobal;

	public event Action onLoadoutChanged;

	public static event Action<UserProfile> onLoadoutChangedGlobal;

	public static event Action<UserProfile, UnlockableDef> onUnlockableGranted;

	public static event Action<UserProfile> onUserProfileViewedViewablesChanged;

	public static void GenerateSaveFieldFunctions()
	{
		nameToSaveFieldMap.Clear();
		FieldInfo[] fields = typeof(UserProfile).GetFields(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
		foreach (FieldInfo fieldInfo in fields)
		{
			SaveFieldAttribute customAttribute = fieldInfo.GetCustomAttribute<SaveFieldAttribute>();
			if (customAttribute != null)
			{
				customAttribute.Setup(fieldInfo);
				nameToSaveFieldMap[fieldInfo.Name] = customAttribute;
			}
		}
		saveFieldNames = nameToSaveFieldMap.Keys.ToArray();
		Array.Sort(saveFieldNames);
		saveFields = saveFieldNames.Select((string name) => nameToSaveFieldMap[name]).ToArray();
	}

	public void SetSaveFieldString([NotNull] string fieldName, [NotNull] string value)
	{
		if (nameToSaveFieldMap.TryGetValue(fieldName, out var value2))
		{
			value2.setter(this, value);
			return;
		}
		Debug.LogErrorFormat("Save field {0} is not defined.", new object[1] { fieldName });
	}

	public string GetSaveFieldString([NotNull] string fieldName)
	{
		if (nameToSaveFieldMap.TryGetValue(fieldName, out var value))
		{
			return value.getter(this);
		}
		Debug.LogErrorFormat("Save field {0} is not defined.", new object[1] { fieldName });
		return string.Empty;
	}

	public static XDocument ToXml(UserProfile userProfile)
	{
		return XmlUtility.ToXml(userProfile);
	}

	public static UserProfile FromXml(XDocument doc)
	{
		return XmlUtility.FromXml(doc);
	}

	public bool HasUnlockable([NotNull] string unlockableToken)
	{
		UnlockableDef unlockableDef = UnlockableCatalog.GetUnlockableDef(unlockableToken);
		if (!((Object)(object)unlockableDef == (Object)null))
		{
			return HasUnlockable(unlockableDef);
		}
		return true;
	}

	public bool HasUnlockable([CanBeNull] UnlockableDef unlockableDef)
	{
		return statSheet.HasUnlockable(unlockableDef);
	}

	public void AddUnlockToken(string unlockableToken)
	{
		UnlockableDef unlockableDef = UnlockableCatalog.GetUnlockableDef(unlockableToken);
		if ((Object)(object)unlockableDef != (Object)null)
		{
			GrantUnlockable(unlockableDef);
		}
	}

	public void GrantUnlockable(UnlockableDef unlockableDef)
	{
		if (!statSheet.HasUnlockable(unlockableDef))
		{
			statSheet.AddUnlockable(unlockableDef);
			Debug.LogFormat("{0} unlocked {1}", new object[2] { name, unlockableDef.nameToken });
			RequestEventualSave();
			UserProfile.onUnlockableGranted?.Invoke(this, unlockableDef);
		}
	}

	public void RequestEventualSave()
	{
		if (canSave)
		{
			saveRequestPending = true;
		}
	}

	public void RevokeUnlockable(UnlockableDef unlockableDef)
	{
		if (statSheet.HasUnlockable(unlockableDef))
		{
			statSheet.RemoveUnlockable(unlockableDef.index);
		}
	}

	public bool HasSurvivorUnlocked(SurvivorIndex survivorIndex)
	{
		SurvivorDef survivorDef = SurvivorCatalog.GetSurvivorDef(survivorIndex);
		if ((Object)(object)survivorDef == (Object)null)
		{
			return false;
		}
		if (!Object.op_Implicit((Object)(object)survivorDef.unlockableDef))
		{
			return true;
		}
		return HasUnlockable(survivorDef.unlockableDef);
	}

	public bool HasDiscoveredPickup(PickupIndex pickupIndex)
	{
		if (pickupIndex.isValid)
		{
			return discoveredPickups[pickupIndex.value];
		}
		return false;
	}

	public void DiscoverPickup(PickupIndex pickupIndex)
	{
		SetPickupDiscovered(pickupIndex, newDiscovered: true);
	}

	private void SetPickupDiscovered(PickupIndex pickupIndex, bool newDiscovered)
	{
		if (!pickupIndex.isValid)
		{
			return;
		}
		ref bool reference = ref discoveredPickups[pickupIndex.value];
		if (reference != newDiscovered)
		{
			reference = newDiscovered;
			if (newDiscovered)
			{
				onPickupDiscovered?.Invoke(pickupIndex);
				RequestEventualSave();
			}
		}
	}

	[ConCommand(commandName = "user_profile_set_pickup_discovered", flags = ConVarFlags.Cheat, helpText = "Sets the pickup discovery state for the sender's profile.")]
	private static void CCUserProfileSetPickupDiscovered(ConCommandArgs args)
	{
		UserProfile userProfile = args.GetSenderLocalUser().userProfile;
		IEnumerable<PickupIndex> enumerable = null;
		enumerable = (IEnumerable<PickupIndex>)((!(args.TryGetArgString(0) == "all")) ? new PickupIndex[1] { args.GetArgPickupIndex(0) } : ((object)PickupCatalog.allPickupIndices));
		bool argBool = args.GetArgBool(1);
		foreach (PickupIndex item in enumerable)
		{
			userProfile.SetPickupDiscovered(item, argBool);
		}
	}

	public bool HasAchievement(string achievementName)
	{
		return achievementsList.Contains(achievementName);
	}

	public bool CanSeeAchievement(string achievementName)
	{
		if (HasAchievement(achievementName))
		{
			return true;
		}
		AchievementDef achievementDef = AchievementManager.GetAchievementDef(achievementName);
		if (achievementDef != null)
		{
			if (string.IsNullOrEmpty(achievementDef.prerequisiteAchievementIdentifier))
			{
				return true;
			}
			return HasAchievement(achievementDef.prerequisiteAchievementIdentifier);
		}
		return false;
	}

	public void AddAchievement(string achievementName, bool isExternal)
	{
		achievementsList.Add(achievementName);
		unviewedAchievementsList.Add(achievementName);
		if (isExternal)
		{
			PlatformSystems.achievementSystem.AddAchievement(achievementName);
		}
		RequestEventualSave();
	}

	public void RevokeAchievement(string achievementName)
	{
		achievementsList.Remove(achievementName);
		unviewedAchievementsList.Remove(achievementName);
		RequestEventualSave();
	}

	public string PopNextUnviewedAchievementName()
	{
		if (unviewedAchievementsList.Count == 0)
		{
			return null;
		}
		string result = unviewedAchievementsList[0];
		unviewedAchievementsList.RemoveAt(0);
		return result;
	}

	public void ApplyDeltaStatSheet(StatSheet deltaStatSheet)
	{
		int i = 0;
		for (int unlockableCount = deltaStatSheet.GetUnlockableCount(); i < unlockableCount; i++)
		{
			GrantUnlockable(deltaStatSheet.GetUnlockable(i));
		}
		statSheet.ApplyDelta(deltaStatSheet);
		onStatsReceived?.Invoke();
	}

	[ConCommand(commandName = "user_profile_stats_stress_test", flags = ConVarFlags.Cheat, helpText = "Sets the stats of the sender's user profile to the maximum their datatypes support for stress-testing purposes.")]
	private static void CCUserProfileStatsStressTest(ConCommandArgs args)
	{
		LocalUser senderLocalUser = args.GetSenderLocalUser();
		senderLocalUser.userProfile.statSheet.SetAllFieldsToMaxValue();
		PlatformSystems.saveSystem.Save(senderLocalUser.userProfile, blocking: true);
	}

	private void ResetShouldShowTutorial(ref TutorialProgression tutorialProgression)
	{
		tutorialProgression.shouldShow = tutorialProgression.showCount < 3;
	}

	private void RebuildTutorialProgressions()
	{
		ResetShouldShowTutorial(ref tutorialDifficulty);
		ResetShouldShowTutorial(ref tutorialSprint);
		ResetShouldShowTutorial(ref tutorialEquipment);
	}

	[NotNull]
	public SurvivorDef GetSurvivorPreference()
	{
		return survivorPreference;
	}

	public void SetSurvivorPreference([NotNull] SurvivorDef newSurvivorPreference)
	{
		if (!Object.op_Implicit((Object)(object)newSurvivorPreference))
		{
			throw new ArgumentException("Provided object is null or invalid", "newSurvivorPreference");
		}
		if (survivorPreference != newSurvivorPreference)
		{
			survivorPreference = newSurvivorPreference;
			this.onSurvivorPreferenceChanged?.Invoke();
			UserProfile.onSurvivorPreferenceChangedGlobal?.Invoke(this);
		}
	}

	private void OnLoadoutChanged()
	{
		this.onLoadoutChanged?.Invoke();
		UserProfile.onLoadoutChangedGlobal?.Invoke(this);
		RequestEventualSave();
	}

	public void CopyLoadout(Loadout dest)
	{
		loadout.Copy(dest);
	}

	public void SetLoadout(Loadout newLoadout)
	{
		if (!loadout.ValueEquals(newLoadout))
		{
			newLoadout.Copy(loadout);
			OnLoadoutChanged();
		}
	}

	[ConCommand(commandName = "loadout_set_skill_variant", flags = (ConVarFlags.ExecuteOnServer | ConVarFlags.Cheat), helpText = "loadout_set_skill_variant [body_name] [skill_slot_index] [skill_variant_index]\nSets the skill variant for the sender's user profile.")]
	private static void CCLoadoutSetSkillVariant(ConCommandArgs args)
	{
		BodyIndex argBodyIndex = args.GetArgBodyIndex(0);
		int argInt = args.GetArgInt(1);
		int argInt2 = args.GetArgInt(2);
		UserProfile userProfile = args.GetSenderLocalUser().userProfile;
		Loadout loadout = new Loadout();
		userProfile.loadout.Copy(loadout);
		loadout.bodyLoadoutManager.SetSkillVariant(argBodyIndex, argInt, (uint)argInt2);
		userProfile.SetLoadout(loadout);
		if (Object.op_Implicit((Object)(object)args.senderMaster))
		{
			args.senderMaster.SetLoadoutServer(loadout);
		}
		if (Object.op_Implicit((Object)(object)args.senderBody))
		{
			args.senderBody.SetLoadoutServer(loadout);
		}
	}

	public void OnLogin()
	{
		if (loggedIn)
		{
			Debug.LogErrorFormat("Profile {0} is already logged in!", new object[1] { fileName });
			return;
		}
		loggedIn = true;
		PlatformSystems.saveSystem.loggedInProfiles.Add(this);
		RebuildTutorialProgressions();
		foreach (string achievements in achievementsList)
		{
			PlatformSystems.achievementSystem.AddAchievement(achievements);
		}
		loadout.EnforceUnlockables(this);
		LoadPortrait();
	}

	public void OnLogout()
	{
		if (!loggedIn)
		{
			Debug.LogErrorFormat("Profile {0} is already logged out!", new object[1] { fileName });
			return;
		}
		UnloadPortrait();
		loggedIn = false;
		PlatformSystems.saveSystem.Save(this, blocking: true);
		PlatformSystems.saveSystem.loggedInProfiles.Remove(this);
	}

	private void LoadPortrait()
	{
		//IL_001b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0025: Expected O, but got Unknown
		Texture2D newPortraitTexture = new Texture2D(184, 184, (TextureFormat)5, false, false);
		portraitTexture = (Texture)(object)newPortraitTexture;
		ownsPortrait = true;
		Client.Instance.Friends.GetAvatar((AvatarSize)2, Client.Instance.SteamId, (Action<Image>)LoadSteamImage);
		void LoadSteamImage(Image image)
		{
			//IL_009f: Unknown result type (might be due to invalid IL or missing references)
			//IL_00a4: Unknown result type (might be due to invalid IL or missing references)
			if (((image != null) ? image.Data : null) != null && Object.op_Implicit((Object)(object)newPortraitTexture) && !((Object)(object)newPortraitTexture != (Object)(object)portraitTexture))
			{
				byte[] data = image.Data;
				Color32[] array = (Color32[])(object)new Color32[data.Length / 4];
				int width = image.Width;
				int height = image.Height;
				newPortraitTexture.Resize(width, height);
				for (int i = 0; i < height; i++)
				{
					int num = height - 1 - i;
					for (int j = 0; j < width; j++)
					{
						int num2 = (i * width + j) * 4;
						array[num * width + j] = new Color32(data[num2], data[num2 + 1], data[num2 + 2], data[num2 + 3]);
					}
				}
				newPortraitTexture.SetPixels32(array);
				newPortraitTexture.Apply();
			}
		}
	}

	private void UnloadPortrait()
	{
		if (ownsPortrait)
		{
			Object.Destroy((Object)(object)portraitTexture);
			portraitTexture = null;
			ownsPortrait = false;
		}
	}

	public static void LoadDefaultProfile()
	{
		defaultProfile = XmlUtility.FromXml(XDocument.Parse("<UserProfile>\r\n  <name>Survivor</name>\r\n  <mouseLookSensitivity>0.2</mouseLookSensitivity>\r\n  <mouseLookScaleX>1</mouseLookScaleX>\r\n  <mouseLookScaleY>1</mouseLookScaleY>\r\n  <stickLookSensitivity>5</stickLookSensitivity>\r\n  <stickLookScaleX>1</stickLookScaleX>\r\n  <stickLookScaleY>1</stickLookScaleY>\r\n</UserProfile>"));
		defaultProfile.canSave = false;
	}

	public bool HasViewedViewable(string viewableName)
	{
		return viewedViewables.Contains(viewableName);
	}

	public void MarkViewableAsViewed(string viewableName)
	{
		if (!HasViewedViewable(viewableName))
		{
			viewedViewables.Add(viewableName);
			UserProfile.onUserProfileViewedViewablesChanged?.Invoke(this);
			RequestEventualSave();
		}
	}

	public void ClearAllViewablesAsViewed()
	{
		viewedViewables.Clear();
		UserProfile.onUserProfileViewedViewablesChanged?.Invoke(this);
		RequestEventualSave();
	}
}
