using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Threading.Tasks;
using System.Xml;
using System.Xml.Linq;
using Facepunch.Steamworks;
using UnityEngine;
using Zio;

namespace RoR2;

public class SaveSystemSteam : SaveSystem
{
	private new readonly Dictionary<FileReference, DateTime> latestWrittenRequestTimesByFile = new Dictionary<FileReference, DateTime>();

	protected override LoadUserProfileOperationResult LoadUserProfileFromDisk(IFileSystem fileSystem, UPath path)
	{
		//IL_000d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0053: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a9: Unknown result type (might be due to invalid IL or missing references)
		//IL_00c6: Unknown result type (might be due to invalid IL or missing references)
		//IL_00c7: Unknown result type (might be due to invalid IL or missing references)
		//IL_0124: Unknown result type (might be due to invalid IL or missing references)
		//IL_0125: Unknown result type (might be due to invalid IL or missing references)
		//IL_012c: Unknown result type (might be due to invalid IL or missing references)
		//IL_018d: Unknown result type (might be due to invalid IL or missing references)
		Debug.LogFormat("Attempting to load user profile {0}", new object[1] { path });
		LoadUserProfileOperationResult loadUserProfileOperationResult = default(LoadUserProfileOperationResult);
		loadUserProfileOperationResult.fileName = ((UPath)(ref path)).FullName;
		loadUserProfileOperationResult.fileLength = 0L;
		loadUserProfileOperationResult.userProfile = null;
		loadUserProfileOperationResult.exception = null;
		loadUserProfileOperationResult.failureContents = null;
		LoadUserProfileOperationResult result = loadUserProfileOperationResult;
		try
		{
			using Stream stream = fileSystem.OpenFile(path, FileMode.Open, FileAccess.Read, FileShare.Read);
			SaveSystem.SkipBOM(stream);
			result.fileLength = stream.Length;
			using TextReader textReader = new StreamReader(stream, Encoding.UTF8);
			Debug.LogFormat("stream.Length={0}", new object[1] { stream.Length });
			try
			{
				UserProfile userProfile = XmlUtility.FromXml(XDocument.Load(textReader));
				userProfile.fileName = UPathExtensions.GetNameWithoutExtension(path);
				userProfile.canSave = true;
				userProfile.fileSystem = fileSystem;
				userProfile.filePath = path;
				result.userProfile = userProfile;
				return result;
			}
			catch (XmlException)
			{
				stream.Position = 0L;
				byte[] array = new byte[stream.Length];
				stream.Read(array, 0, (int)stream.Length);
				result.failureContents = Convert.ToBase64String(array);
				UserProfile userProfile2 = CreateGuestProfile();
				userProfile2.fileSystem = fileSystem;
				userProfile2.filePath = path;
				userProfile2.fileName = UPathExtensions.GetNameWithoutExtension(path);
				userProfile2.name = $"<color=#FF7F7FFF>Corrupted Profile: {userProfile2.fileName}</color>";
				userProfile2.canSave = false;
				userProfile2.isCorrupted = true;
				result.userProfile = userProfile2;
				throw;
			}
		}
		catch (Exception ex2)
		{
			Debug.LogFormat("Failed to load user profile {0}: {1}\nStack Trace:\n{2}", new object[3] { path, ex2.Message, ex2.StackTrace });
			result.exception = ex2;
			return result;
		}
	}

	protected override void ProcessFileOutputQueue()
	{
		lock (pendingOutputQueue)
		{
			while (pendingOutputQueue.Count > 0)
			{
				FileOutput fileOutput = pendingOutputQueue.Dequeue();
				if (CanWrite(fileOutput))
				{
					WriteToDisk(fileOutput);
				}
			}
		}
	}

	protected override void StartSave(UserProfile userProfile, bool blocking)
	{
		//IL_003b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0040: Unknown result type (might be due to invalid IL or missing references)
		UserProfile tempCopy = new UserProfile();
		SaveSystem.Copy(userProfile, tempCopy);
		FileOutput fileOutput = new FileOutput
		{
			fileReference = new FileReference
			{
				path = tempCopy.filePath,
				fileSystem = tempCopy.fileSystem
			},
			requestTime = DateTime.UtcNow,
			contents = Array.Empty<byte>()
		};
		Task task = null;
		task = new Task(PayloadGeneratorAction);
		AddActiveTask(task);
		task.Start(TaskScheduler.Default);
		if (blocking)
		{
			task.Wait();
			ProcessFileOutputQueue();
		}
		void PayloadGeneratorAction()
		{
			try
			{
				FileIoIndicatorManager.IncrementActiveWriteCount();
				XDocument xDocument = XmlUtility.ToXml(tempCopy);
				using (MemoryStream memoryStream = new MemoryStream())
				{
					using XmlTextWriter xmlTextWriter = new XmlTextWriter(memoryStream, new UTF8Encoding(encoderShouldEmitUTF8Identifier: false));
					xDocument.Save(xmlTextWriter);
					xmlTextWriter.Flush();
					fileOutput.contents = new byte[memoryStream.Length];
					memoryStream.Seek(0L, SeekOrigin.Begin);
					memoryStream.Read(fileOutput.contents, 0, fileOutput.contents.Length);
				}
				EnqueueFileOutput(fileOutput);
			}
			catch (Exception ex)
			{
				Exception ex2 = ex;
				Exception e = ex2;
				RoR2Application.onNextUpdate += delegate
				{
					Debug.Log((object)e);
				};
				throw;
			}
			finally
			{
				FileIoIndicatorManager.DecrementActiveWriteCount();
				RemoveActiveTask(task);
			}
		}
	}

	private new bool CanWrite(FileOutput fileOutput)
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

	private void WriteToDisk(FileOutput fileOutput)
	{
		//IL_0016: Unknown result type (might be due to invalid IL or missing references)
		//IL_0058: Unknown result type (might be due to invalid IL or missing references)
		FileIoIndicatorManager.IncrementActiveWriteCount();
		try
		{
			using (Stream stream = fileOutput.fileReference.fileSystem.OpenFile(fileOutput.fileReference.path, FileMode.Create, FileAccess.Write, FileShare.None))
			{
				stream.Write(fileOutput.contents, 0, fileOutput.contents.Length);
				stream.Flush();
				stream.Close();
				Debug.LogFormat("Saved file \"{0}\" ({1} bytes)", new object[2]
				{
					UPathExtensions.GetName(fileOutput.fileReference.path),
					fileOutput.contents.Length
				});
			}
			latestWrittenRequestTimesByFile[fileOutput.fileReference] = fileOutput.requestTime;
		}
		catch (Exception ex)
		{
			Debug.Log((object)ex);
		}
		finally
		{
			FileIoIndicatorManager.DecrementActiveWriteCount();
		}
	}

	public override UserProfile LoadPrimaryProfile()
	{
		return null;
	}

	public override string GetPlatformUsernameOrDefault(string defaultName)
	{
		Client instance = Client.Instance;
		string text = ((instance != null) ? instance.Username : null);
		if (!string.IsNullOrEmpty(text))
		{
			return text;
		}
		return defaultName;
	}

	public override void SaveHistory(byte[] data, string fileName)
	{
	}

	public override Dictionary<string, byte[]> LoadHistory()
	{
		return new Dictionary<string, byte[]>();
	}
}
