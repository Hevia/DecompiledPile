using System;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using Zio;

namespace RoR2;

public class StreamingAssetsTextDataManager : TextDataManager
{
	private readonly string configFolder;

	public override bool InitializedConfigFiles => true;

	public override bool InitializedLocFiles => true;

	public StreamingAssetsTextDataManager()
	{
		configFolder = System.IO.Path.Combine(Application.dataPath, "Config");
	}

	public override string GetConfFile(string fileName, string path)
	{
		//IL_000d: Unknown result type (might be due to invalid IL or missing references)
		if (RoR2Application.fileSystem != null)
		{
			using Stream stream = RoR2Application.fileSystem.OpenFile(UPath.op_Implicit(path), FileMode.Open, FileAccess.Read, FileShare.None);
			if (stream != null)
			{
				using (TextReader textReader = new StreamReader(stream))
				{
					return textReader.ReadToEnd();
				}
			}
		}
		return "";
	}

	public override void GetLocFiles(string folderPath, Action<string[]> callback)
	{
		List<string> list = new List<string>();
		foreach (string item in Directory.EnumerateFiles(folderPath))
		{
			if (string.Compare(System.IO.Path.GetFileName(item), "language.json", StringComparison.OrdinalIgnoreCase) != 0)
			{
				string extension = System.IO.Path.GetExtension(item);
				if (MatchesExtension(extension, ".txt") || MatchesExtension(extension, ".json"))
				{
					list.Add(item);
				}
			}
		}
		callback?.Invoke(list.ConvertAll((string x) => File.ReadAllText(x)).ToArray());
		static bool MatchesExtension(string fileExtension, string testExtension)
		{
			return string.Compare(fileExtension, testExtension, StringComparison.OrdinalIgnoreCase) == 0;
		}
	}
}
