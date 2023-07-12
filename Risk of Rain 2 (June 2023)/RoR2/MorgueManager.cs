using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Xml.Linq;
using HG;
using RoR2.ConVar;
using UnityEngine;
using Zio;

namespace RoR2;

public static class MorgueManager
{
	private struct HistoryFileInfo
	{
		public UPath path;

		public DateTime lastModified;

		public FileEntry fileEntry;

		public void LoadRunReport(RunReport dest)
		{
			HGXml.FromXml(XDocument.Parse(fileEntry.ReadAllText()).Root, ref dest);
		}

		public void Delete()
		{
			((FileSystemEntry)fileEntry).Delete();
		}
	}

	private static readonly IntConVar morgueHistoryLimit = new IntConVar("morgue_history_limit", ConVarFlags.Archive, "30", "How many non-favorited entries we can store in the morgue before old ones are deleted.");

	private static readonly UPath historyDirectory = new UPath("/RunReports/History/");

	private static IFileSystem storage => (IFileSystem)(object)RoR2Application.fileSystem;

	[SystemInitializer(new Type[] { })]
	private static void Init()
	{
		Run.onClientGameOverGlobal += OnClientGameOverGlobal;
	}

	private static void OnClientGameOverGlobal(Run run, RunReport runReport)
	{
		AddRunReportToHistory(runReport);
	}

	private static void AddRunReportToHistory(RunReport runReport)
	{
		EnforceHistoryLimit();
		StringBuilder stringBuilder = StringBuilderPool.RentStringBuilder();
		string fileName = stringBuilder.Append(runReport.runGuid).Append(".xml").ToString();
		StringBuilderPool.ReturnStringBuilder(stringBuilder);
		using Stream stream = GetHistoryFile(fileName);
		if (stream != null)
		{
			XDocument xDocument = new XDocument();
			xDocument.Add(HGXml.ToXml("RunReport", runReport));
			xDocument.Save(stream);
			stream.Flush();
			stream.Dispose();
		}
	}

	private static void EnforceHistoryLimit()
	{
		List<HistoryFileInfo> list = CollectionPool<HistoryFileInfo, List<HistoryFileInfo>>.RentCollection();
		GetHistoryFiles(list);
		int num = list.Count - 1;
		int num2 = Math.Max(morgueHistoryLimit.value, 0);
		while (num >= num2)
		{
			list[num].Delete();
			num--;
		}
		CollectionPool<HistoryFileInfo, List<HistoryFileInfo>>.ReturnCollection(list);
	}

	private static bool RemoveOldestHistoryFile()
	{
		List<HistoryFileInfo> list = CollectionPool<HistoryFileInfo, List<HistoryFileInfo>>.RentCollection();
		GetHistoryFiles(list);
		DateTime dateTime = DateTime.MaxValue;
		int num = -1;
		for (int i = 0; i < list.Count; i++)
		{
			HistoryFileInfo historyFileInfo = list[i];
			if (historyFileInfo.lastModified < dateTime)
			{
				num = i;
				dateTime = historyFileInfo.lastModified;
			}
		}
		if (num != -1)
		{
			list[num].Delete();
			return true;
		}
		CollectionPool<HistoryFileInfo, List<HistoryFileInfo>>.ReturnCollection(list);
		return false;
	}

	public static void LoadHistoryRunReports(List<RunReport> dest)
	{
		List<HistoryFileInfo> list = CollectionPool<HistoryFileInfo, List<HistoryFileInfo>>.RentCollection();
		GetHistoryFiles(list);
		list.Sort(CompareHistoryFileLastModified);
		for (int i = 0; i < list.Count; i++)
		{
			HistoryFileInfo historyFileInfo = list[i];
			try
			{
				RunReport runReport = new RunReport();
				historyFileInfo.LoadRunReport(runReport);
				dest.Add(runReport);
			}
			catch (Exception ex)
			{
				Debug.LogFormat("Could not load RunReport \"{0}\": {1}", new object[2] { historyFileInfo, ex });
			}
		}
		CollectionPool<HistoryFileInfo, List<HistoryFileInfo>>.ReturnCollection(list);
		static int CompareHistoryFileLastModified(HistoryFileInfo a, HistoryFileInfo b)
		{
			return b.lastModified.CompareTo(a.lastModified);
		}
	}

	private static Stream GetHistoryFile(string fileName)
	{
		//IL_0000: Unknown result type (might be due to invalid IL or missing references)
		//IL_0006: Unknown result type (might be due to invalid IL or missing references)
		//IL_000b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0010: Unknown result type (might be due to invalid IL or missing references)
		//IL_0016: Unknown result type (might be due to invalid IL or missing references)
		//IL_0025: Unknown result type (might be due to invalid IL or missing references)
		UPath val = historyDirectory / UPath.op_Implicit(fileName);
		storage.CreateDirectory(historyDirectory);
		return storage.OpenFile(val, FileMode.Create, FileAccess.Write, FileShare.None);
	}

	private static void GetHistoryFiles(List<HistoryFileInfo> dest)
	{
		//IL_0005: Unknown result type (might be due to invalid IL or missing references)
		//IL_001d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0030: Unknown result type (might be due to invalid IL or missing references)
		//IL_0035: Unknown result type (might be due to invalid IL or missing references)
		//IL_003b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0043: Unknown result type (might be due to invalid IL or missing references)
		//IL_0061: Unknown result type (might be due to invalid IL or missing references)
		//IL_0062: Unknown result type (might be due to invalid IL or missing references)
		//IL_006e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0080: Unknown result type (might be due to invalid IL or missing references)
		foreach (UPath item in storage.DirectoryExists(historyDirectory) ? FileSystemExtensions.EnumeratePaths(storage, historyDirectory) : Enumerable.Empty<UPath>())
		{
			if (storage.FileExists(item) && UPathExtensions.GetExtensionWithDot(item).Equals(".xml", StringComparison.OrdinalIgnoreCase))
			{
				dest.Add(new HistoryFileInfo
				{
					path = item,
					lastModified = storage.GetLastWriteTime(item),
					fileEntry = FileSystemExtensions.GetFileEntry(storage, item)
				});
			}
		}
	}
}
