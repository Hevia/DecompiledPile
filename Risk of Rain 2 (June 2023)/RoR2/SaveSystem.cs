using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using Facepunch.Steamworks;
using JetBrains.Annotations;
using RoR2.Stats;
using UnityEngine;
using Zio;
using Zio.FileSystems;

namespace RoR2;

public abstract class SaveSystem : SaveSystemBase
{
	public bool isXmlReady;

	protected readonly Dictionary<FileReference, DateTime> latestWrittenRequestTimesByFile = new Dictionary<FileReference, DateTime>();

	public readonly Dictionary<string, UserProfile> loadedUserProfiles = new Dictionary<string, UserProfile>(StringComparer.OrdinalIgnoreCase);

	public readonly List<LoadUserProfileOperationResult> badFileResults = new List<LoadUserProfileOperationResult>();

	public readonly List<UserProfile> loggedInProfiles = new List<UserProfile>();

	private static float secondAccumulator;

	private const string userProfilesFolder = "/UserProfiles";

	protected readonly Queue<FileOutput> pendingOutputQueue = new Queue<FileOutput>();

	private readonly List<Task> activeTasks = new List<Task>();

	public static event Action onAvailableUserProfilesChanged;

	public static void SkipBOM(Stream stream)
	{
		long position = stream.Position;
		if (stream.Length - position < 3)
		{
			return;
		}
		int num = stream.ReadByte();
		int num2 = stream.ReadByte();
		if (num == 255 && num2 == 254)
		{
			Debug.Log((object)"Skipping UTF-8 BOM");
			return;
		}
		int num3 = stream.ReadByte();
		if (num == 239 && num2 == 187 && num3 == 191)
		{
			Debug.Log((object)"Skipping UTF-16 BOM");
		}
		else
		{
			stream.Position = position;
		}
	}

	public virtual void InitializeSaveSystem()
	{
		Debug.Log((object)"Should call child class initialize");
	}

	protected bool CanWrite(FileOutput fileOutput)
	{
		if (fileOutput.contents.Length == 0)
		{
			Debug.LogErrorFormat("Cannot write UserProfile \"{0}\" with zero-length contents. This would erase the file.", Array.Empty<object>());
			return false;
		}
		if (latestWrittenRequestTimesByFile.TryGetValue(fileOutput.fileReference, out var value))
		{
			return value < fileOutput.requestTime;
		}
		return true;
	}

	protected UserProfile AttemptToRecoverUserData(string profileXML)
	{
		string value = "<a";
		string value2 = "<c";
		int num = profileXML.IndexOf(value);
		int num2 = profileXML.IndexOf(value2);
		string[] array = null;
		string text = null;
		if (num != -1 && num + 1 < profileXML.Length)
		{
			int num3 = profileXML.IndexOf("<", num + 1);
			int num4 = profileXML.IndexOf(">", num);
			if (num4 != -1)
			{
				num3 = ((num3 != -1) ? (num3 - 1) : (profileXML.Length - 1));
				string text2 = profileXML.Substring(num4 + 1, num3 - num4);
				array = text2.Split(new char[1] { ' ' }, StringSplitOptions.RemoveEmptyEntries);
				Debug.LogError((object)("Recovered achievements: " + text2));
			}
		}
		if (num2 != -1 && num2 + 1 < profileXML.Length)
		{
			int num5 = profileXML.IndexOf("<", num2 + 1);
			int num6 = profileXML.IndexOf(">", num2);
			if (num6 != -1)
			{
				num5 = ((num5 != -1) ? (num5 - 1) : (profileXML.Length - 1));
				text = profileXML.Substring(num6 + 1, num5 - num6);
				Debug.LogError((object)("Recovered coins: " + text));
			}
		}
		UserProfile userProfile = CreateGuestProfile();
		if (array != null && array.Length != 0)
		{
			string[] array2 = array;
			foreach (string achievementName in array2)
			{
				userProfile.AddAchievement(achievementName, isExternal: true);
			}
		}
		else
		{
			Debug.LogError((object)"XML didn't contain achievements!");
		}
		uint result = 0u;
		if (text != null && uint.TryParse(text, out result))
		{
			userProfile.coins = result;
		}
		else
		{
			Debug.LogError((object)"XML didn't contain coins!");
		}
		return userProfile;
	}

	protected void AddActiveTask(Task task)
	{
		lock (activeTasks)
		{
			activeTasks.Add(task);
		}
	}

	protected void RemoveActiveTask(Task task)
	{
		lock (activeTasks)
		{
			activeTasks.Remove(task);
		}
	}

	public void StaticUpdate()
	{
		secondAccumulator += Time.unscaledDeltaTime;
		if (secondAccumulator > 1f)
		{
			secondAccumulator -= 1f;
			foreach (UserProfile loggedInProfile in loggedInProfiles)
			{
				loggedInProfile.totalLoginSeconds++;
			}
		}
		foreach (UserProfile loggedInProfile2 in loggedInProfiles)
		{
			if (loggedInProfile2.saveRequestPending && Save(loggedInProfile2, blocking: false))
			{
				loggedInProfile2.saveRequestPending = false;
			}
		}
		ProcessFileOutputQueue();
	}

	public abstract void SaveHistory(byte[] data, string fileName);

	public abstract Dictionary<string, byte[]> LoadHistory();

	public void RequestSave(UserProfile profile, bool immediate = false)
	{
		if (profile.canSave)
		{
			if (immediate)
			{
				Save(profile, blocking: true);
			}
			else
			{
				profile.saveRequestPending = true;
			}
		}
	}

	public bool Save(UserProfile data, bool blocking)
	{
		try
		{
			StartSave(data, blocking);
			return true;
		}
		catch
		{
			return false;
		}
	}

	public void AddLoadedUserProfile(string name, UserProfile profile)
	{
		if (profile != null && !loadedUserProfiles.ContainsKey(name))
		{
			loadedUserProfiles.Add(name, profile);
		}
	}

	public List<string> GetAvailableProfileNames()
	{
		List<string> list = new List<string>();
		foreach (KeyValuePair<string, UserProfile> loadedUserProfile in loadedUserProfiles)
		{
			if (!loadedUserProfile.Value.isClaimed)
			{
				list.Add(loadedUserProfile.Key);
			}
		}
		list.Sort();
		return list;
	}

	public UserProfile GetProfile(string profileName)
	{
		profileName = profileName.ToLower(CultureInfo.InvariantCulture);
		if (loadedUserProfiles.TryGetValue(profileName, out var value))
		{
			return value;
		}
		return null;
	}

	public virtual UserProfile CreateProfile(IFileSystem fileSystem, string name)
	{
		UserProfile newProfile = UserProfile.FromXml(UserProfile.ToXml(UserProfile.defaultProfile));
		PlatformInitProfile(ref newProfile, fileSystem, name);
		loadedUserProfiles.Add(newProfile.fileName, newProfile);
		Save(newProfile, blocking: true);
		SaveSystem.onAvailableUserProfilesChanged?.Invoke();
		return newProfile;
	}

	protected virtual void PlatformInitProfile(ref UserProfile newProfile, IFileSystem fileSystem, string name)
	{
		//IL_003a: Unknown result type (might be due to invalid IL or missing references)
		//IL_003f: Unknown result type (might be due to invalid IL or missing references)
		newProfile.fileName = Guid.NewGuid().ToString();
		newProfile.fileSystem = fileSystem;
		newProfile.filePath = UPath.op_Implicit("/UserProfiles/" + newProfile.fileName + ".xml");
		newProfile.name = name;
		newProfile.canSave = true;
	}

	public UserProfile CreateGuestProfile()
	{
		UserProfile userProfile = new UserProfile();
		Copy(UserProfile.defaultProfile, userProfile);
		userProfile.name = "Guest";
		return userProfile;
	}

	public void LoadUserProfiles()
	{
		//IL_002d: Unknown result type (might be due to invalid IL or missing references)
		//IL_003f: Unknown result type (might be due to invalid IL or missing references)
		//IL_004f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0062: Unknown result type (might be due to invalid IL or missing references)
		//IL_0067: Unknown result type (might be due to invalid IL or missing references)
		//IL_0069: Unknown result type (might be due to invalid IL or missing references)
		//IL_0071: Unknown result type (might be due to invalid IL or missing references)
		//IL_0085: Unknown result type (might be due to invalid IL or missing references)
		badFileResults.Clear();
		loadedUserProfiles.Clear();
		UserProfile.LoadDefaultProfile();
		FileSystem cloudStorage = RoR2Application.cloudStorage;
		if (cloudStorage != null)
		{
			if (!cloudStorage.DirectoryExists(UPath.op_Implicit("/UserProfiles")))
			{
				cloudStorage.CreateDirectory(UPath.op_Implicit("/UserProfiles"));
			}
			foreach (UPath item2 in FileSystemExtensions.EnumeratePaths((IFileSystem)(object)cloudStorage, UPath.op_Implicit("/UserProfiles")))
			{
				if (cloudStorage.FileExists(item2) && string.CompareOrdinal(UPathExtensions.GetExtensionWithDot(item2), ".xml") == 0)
				{
					LoadUserProfileOperationResult item = LoadUserProfileFromDisk((IFileSystem)(object)cloudStorage, item2);
					UserProfile userProfile = item.userProfile;
					if (userProfile != null)
					{
						loadedUserProfiles[userProfile.fileName] = userProfile;
					}
					if (item.exception != null)
					{
						badFileResults.Add(item);
					}
				}
			}
			OutputBadFileResults();
			SaveSystem.onAvailableUserProfilesChanged?.Invoke();
		}
		else
		{
			Debug.LogError((object)"cloud storage is null");
		}
	}

	private void OutputBadFileResults()
	{
		//IL_0019: Unknown result type (might be due to invalid IL or missing references)
		if (badFileResults.Count == 0)
		{
			return;
		}
		try
		{
			using Stream stream = FileSystemExtensions.CreateFile((IFileSystem)(object)RoR2Application.fileSystem, new UPath("/bad_profiles.log"));
			using TextWriter textWriter = new StreamWriter(stream);
			foreach (LoadUserProfileOperationResult badFileResult in badFileResults)
			{
				textWriter.WriteLine("Failed to load file \"{0}\" ({1}B)", badFileResult.fileName, badFileResult.fileLength);
				textWriter.WriteLine("Exception: {0}", badFileResult.exception);
				textWriter.Write("Base64 Contents: ");
				textWriter.WriteLine(badFileResult.failureContents ?? string.Empty);
				textWriter.WriteLine(string.Empty);
			}
		}
		catch (Exception ex)
		{
			Debug.LogFormat("Could not write bad UserProfile load log! Reason: {0}", new object[1] { ex.Message });
		}
	}

	public virtual void HandleShutDown()
	{
		foreach (UserProfile loggedInProfile in loggedInProfiles)
		{
			RequestSave(loggedInProfile, immediate: true);
		}
	}

	public static void Copy(UserProfile src, UserProfile dest)
	{
		//IL_000e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0013: Unknown result type (might be due to invalid IL or missing references)
		dest.fileSystem = src.fileSystem;
		dest.filePath = src.filePath;
		StatSheet.Copy(src.statSheet, dest.statSheet);
		src.loadout.Copy(dest.loadout);
		dest.tutorialSprint = src.tutorialSprint;
		dest.tutorialDifficulty = src.tutorialDifficulty;
		dest.tutorialEquipment = src.tutorialEquipment;
		SaveFieldAttribute[] saveFields = UserProfile.saveFields;
		for (int i = 0; i < saveFields.Length; i++)
		{
			saveFields[i].copier(src, dest);
		}
		dest.isClaimed = false;
		dest.canSave = false;
		dest.fileName = src.fileName;
		dest.onPickupDiscovered = null;
		dest.onStatsReceived = null;
		dest.loggedIn = false;
	}

	protected void EnqueueFileOutput(FileOutput fileOutput)
	{
		lock (pendingOutputQueue)
		{
			pendingOutputQueue.Enqueue(fileOutput);
		}
	}

	private static bool ProfileNameIsReserved([NotNull] string profileName)
	{
		return string.Equals("default", profileName, StringComparison.OrdinalIgnoreCase);
	}

	[ConCommand(commandName = "user_profile_save", flags = ConVarFlags.None, helpText = "Saves the named profile to disk, if it exists.")]
	private static void CCUserProfileSave(ConCommandArgs args)
	{
		args.CheckArgumentCount(1);
		string text = args[0];
		if (ProfileNameIsReserved(text))
		{
			Debug.LogFormat("Cannot save profile \"{0}\", it is a reserved profile.", new object[1] { text });
			return;
		}
		UserProfile profile = PlatformSystems.saveSystem.GetProfile(text);
		if (profile == null)
		{
			Debug.LogFormat("Could not find profile \"{0}\" to save.", new object[1] { text });
		}
		else
		{
			profile.RequestEventualSave();
		}
	}

	[ConCommand(commandName = "user_profile_copy", flags = ConVarFlags.None, helpText = "Copies the profile named by the first argument to a new profile named by the second argument. This does not save the profile.")]
	private static void CCUserProfileCopy(ConCommandArgs args)
	{
		//IL_00a7: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ac: Unknown result type (might be due to invalid IL or missing references)
		args.CheckArgumentCount(2);
		string text = args[0].ToLower(CultureInfo.InvariantCulture);
		string text2 = args[1].ToLower(CultureInfo.InvariantCulture);
		UserProfile profile = PlatformSystems.saveSystem.GetProfile(text);
		if (profile == null)
		{
			Debug.LogFormat("Profile {0} does not exist, so it cannot be copied.", new object[1] { text });
			return;
		}
		if (PlatformSystems.saveSystem.GetProfile(text2) != null)
		{
			Debug.LogFormat("Profile {0} already exists, and cannot be copied to.", new object[1] { text2 });
			return;
		}
		UserProfile userProfile = new UserProfile();
		Copy(profile, userProfile);
		userProfile.fileSystem = (IFileSystem)(((object)profile.fileSystem) ?? ((object)RoR2Application.cloudStorage));
		userProfile.filePath = UPath.op_Implicit("/UserProfiles/" + text2 + ".xml");
		userProfile.fileName = text2;
		userProfile.canSave = true;
		PlatformSystems.saveSystem.loadedUserProfiles[text2] = userProfile;
		SaveSystem.onAvailableUserProfilesChanged?.Invoke();
	}

	[ConCommand(commandName = "user_profile_delete", flags = ConVarFlags.None, helpText = "Unloads the named user profile and deletes it from the disk if it exists.")]
	private static void CCUserProfileDelete(ConCommandArgs args)
	{
		args.CheckArgumentCount(1);
		string text = args[0];
		if (ProfileNameIsReserved(text))
		{
			Debug.LogFormat("Cannot delete profile \"{0}\", it is a reserved profile.", new object[1] { text });
		}
		else
		{
			DeleteUserProfile(text);
		}
	}

	private static void DeleteUserProfile(string fileName)
	{
		//IL_004e: Unknown result type (might be due to invalid IL or missing references)
		fileName = fileName.ToLower(CultureInfo.InvariantCulture);
		UserProfile profile = PlatformSystems.saveSystem.GetProfile(fileName);
		if (PlatformSystems.saveSystem.loadedUserProfiles.ContainsKey(fileName))
		{
			PlatformSystems.saveSystem.loadedUserProfiles.Remove(fileName);
		}
		if (profile != null && profile.fileSystem != null)
		{
			profile.fileSystem.DeleteFile(profile.filePath);
		}
		SaveSystem.onAvailableUserProfilesChanged?.Invoke();
	}

	[ConCommand(commandName = "create_corrupted_profiles", flags = ConVarFlags.None, helpText = "Creates corrupted user profiles.")]
	private static void CCCreateCorruptedProfiles(ConCommandArgs args)
	{
		FileSystem fileSystem = RoR2Application.cloudStorage;
		WriteFile("empty", "");
		WriteFile("truncated", "<?xml version=\"1.0\" encoding=\"utf-8\"?>\r\n<UserProfile>\r\n");
		WriteFile("multiroot", "<?xml version=\"1.0\" encoding=\"utf-8\"?>\r\n<UserProfile>\r\n</UserProfile>\r\n<UserProfile>\r\n</UserProfile>");
		WriteFile("outoforder", "<?xml version=\"1.0\" encodi=\"utf-8\"ng?>\r\n<Userrofile>\r\n<UserProfile>\r\n</UserProfileProfile>\r\n</UserP>");
		void WriteFile(string fileName, string contents)
		{
			//IL_0016: Unknown result type (might be due to invalid IL or missing references)
			using Stream stream = fileSystem.OpenFile(UPath.op_Implicit("/UserProfiles/" + fileName + ".xml"), FileMode.Create, FileAccess.Write, FileShare.None);
			using (TextWriter textWriter = new StreamWriter(stream))
			{
				textWriter.Write(contents.ToCharArray());
				textWriter.Flush();
			}
			stream.Flush();
		}
	}

	[ConCommand(commandName = "userprofile_test_buffer_overflow", flags = ConVarFlags.None, helpText = "")]
	private static void CCUserProfileTestBufferOverflow(ConCommandArgs args)
	{
		args.CheckArgumentCount(1);
		int num = 128;
		_ = RoR2Application.cloudStorage;
		RemoteFile val = Client.Instance.RemoteStorage.OpenFile(args[0]);
		_ = val.SizeInBytes;
		FieldInfo field = ((object)val).GetType().GetField("_sizeInBytes", BindingFlags.Instance | BindingFlags.NonPublic);
		int num2 = (int)field.GetValue(val);
		field.SetValue(val, num2 + num);
		byte[] array = val.ReadAllBytes();
		byte[] array2 = new byte[num];
		for (int i = 0; i < num; i++)
		{
			Debug.Log((object)array[num2 + i]);
			array2[i] = array[num2 + i];
		}
		GUIUtility.systemCopyBuffer = Encoding.UTF8.GetString(array2);
		field.SetValue(val, num2);
	}
}
