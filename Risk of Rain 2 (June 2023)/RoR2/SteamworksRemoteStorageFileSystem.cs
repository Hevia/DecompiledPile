using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using Facepunch.Steamworks;
using JetBrains.Annotations;
using UnityEngine;
using Zio;
using Zio.FileSystems;

namespace RoR2;

public class SteamworksRemoteStorageFileSystem : FileSystem
{
	private struct SteamworksRemoteStoragePath : IEquatable<SteamworksRemoteStoragePath>
	{
		public readonly string str;

		public SteamworksRemoteStoragePath(string path)
		{
			str = path;
		}

		public static implicit operator SteamworksRemoteStoragePath(string str)
		{
			return new SteamworksRemoteStoragePath(str);
		}

		public bool Equals(SteamworksRemoteStoragePath other)
		{
			return string.Equals(str, other.str);
		}

		public override bool Equals(object obj)
		{
			if (obj == null)
			{
				return false;
			}
			if (obj is SteamworksRemoteStoragePath other)
			{
				return Equals(other);
			}
			return false;
		}

		public override int GetHashCode()
		{
			if (str == null)
			{
				return 0;
			}
			return str.GetHashCode();
		}
	}

	private class Node
	{
		public readonly UPath path;

		public Node parent;

		public Node(UPath path)
		{
			//IL_0007: Unknown result type (might be due to invalid IL or missing references)
			//IL_0008: Unknown result type (might be due to invalid IL or missing references)
			//IL_000d: Unknown result type (might be due to invalid IL or missing references)
			this.path = UPathExtensions.ToAbsolute(path);
		}
	}

	private class FileNode : Node
	{
		public readonly SteamworksRemoteStoragePath steamworksRemoteStoragePath;

		private RemoteFile file => remoteStorage.OpenFile(steamworksRemoteStoragePath.str);

		public FileNode(SteamworksRemoteStoragePath steamworksRemoteStoragePath)
			: base(UPath.op_Implicit(steamworksRemoteStoragePath.str))
		{
			//IL_0007: Unknown result type (might be due to invalid IL or missing references)
			this.steamworksRemoteStoragePath = steamworksRemoteStoragePath;
		}

		public int GetLength()
		{
			return file.SizeInBytes;
		}

		public Stream OpenWrite()
		{
			return (Stream)(object)file.OpenWrite();
		}

		public Stream OpenRead()
		{
			return file.OpenRead();
		}

		public void Delete()
		{
			file.Delete();
		}
	}

	private class DirectoryNode : Node
	{
		private Node[] childNodes = Array.Empty<Node>();

		public int childCount { get; private set; }

		public Node GetChild(int i)
		{
			return childNodes[i];
		}

		public void AddChild(Node node)
		{
			int num = childCount + 1;
			childCount = num;
			if (childCount > childNodes.Length)
			{
				Array.Resize(ref childNodes, childCount);
			}
			childNodes[childCount - 1] = node;
			node.parent = this;
		}

		public void RemoveChildAt(int i)
		{
			if (childCount > 0)
			{
				childNodes[i].parent = null;
			}
			int num = childCount - 1;
			while (i < num)
			{
				childNodes[i] = childNodes[i + 1];
				i++;
			}
			if (childCount > 0)
			{
				childNodes[childCount - 1] = null;
			}
			int num2 = childCount - 1;
			childCount = num2;
		}

		public void RemoveChild(Node node)
		{
			int num = Array.IndexOf(childNodes, node);
			if (num >= 0)
			{
				RemoveChildAt(num);
			}
		}

		public void RemoveAllChildren()
		{
			for (int i = 0; i < childCount; i++)
			{
				childNodes[i].parent = null;
				childNodes[i] = null;
			}
			childCount = 0;
		}

		public DirectoryNode(UPath path)
			: base(path)
		{
		}//IL_000c: Unknown result type (might be due to invalid IL or missing references)

	}

	private static readonly object globalLock = new object();

	private string[] allFilePaths = Array.Empty<string>();

	private readonly DirectoryNode rootNode = new DirectoryNode(UPath.Root);

	private readonly Dictionary<UPath, Node> pathToNodeMap = new Dictionary<UPath, Node>();

	private bool treeIsDirty = true;

	private static Client steamworksClient => Client.Instance;

	private static RemoteStorage remoteStorage => steamworksClient.RemoteStorage;

	public SteamworksRemoteStorageFileSystem()
	{
		//IL_000c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0039: Unknown result type (might be due to invalid IL or missing references)
		pathToNodeMap[UPath.Root] = rootNode;
	}

	protected override void CreateDirectoryImpl(UPath path)
	{
	}

	protected override bool DirectoryExistsImpl(UPath path)
	{
		return true;
	}

	protected override void MoveDirectoryImpl(UPath srcPath, UPath destPath)
	{
		treeIsDirty = true;
		throw new NotImplementedException();
	}

	protected override void DeleteDirectoryImpl(UPath path, bool isRecursive)
	{
		throw new NotImplementedException();
	}

	protected override void CopyFileImpl(UPath srcPath, UPath destPath, bool overwrite)
	{
		treeIsDirty = true;
		throw new NotImplementedException();
	}

	protected override void ReplaceFileImpl(UPath srcPath, UPath destPath, UPath destBackupPath, bool ignoreMetadataErrors)
	{
		throw new NotImplementedException();
	}

	protected override long GetFileLengthImpl(UPath path)
	{
		//IL_000e: Unknown result type (might be due to invalid IL or missing references)
		int num = 0;
		EnterFileSystemShared();
		try
		{
			UpdateDirectories();
			num = GetFileNode(path)?.GetLength() ?? 0;
		}
		finally
		{
			ExitFileSystemShared();
		}
		return num;
	}

	protected override bool FileExistsImpl(UPath path)
	{
		//IL_0007: Unknown result type (might be due to invalid IL or missing references)
		UpdateDirectories();
		return GetFileNode(path) != null;
	}

	protected override void MoveFileImpl(UPath srcPath, UPath destPath)
	{
		treeIsDirty = true;
		throw new NotImplementedException();
	}

	protected override void DeleteFileImpl(UPath path)
	{
		//IL_000d: Unknown result type (might be due to invalid IL or missing references)
		EnterFileSystemShared();
		try
		{
			treeIsDirty = true;
			GetFileNode(path)?.Delete();
		}
		finally
		{
			ExitFileSystemShared();
		}
	}

	protected override Stream OpenFileImpl(UPath path, FileMode mode, FileAccess access, FileShare share)
	{
		//IL_0018: Unknown result type (might be due to invalid IL or missing references)
		//IL_0070: Unknown result type (might be due to invalid IL or missing references)
		//IL_0071: Unknown result type (might be due to invalid IL or missing references)
		//IL_0076: Unknown result type (might be due to invalid IL or missing references)
		//IL_0096: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ac: Unknown result type (might be due to invalid IL or missing references)
		EnterFileSystemShared();
		if (!((UPath)(ref path)).IsAbsolute)
		{
			throw new ArgumentException(string.Format("'{0}' must be absolute. {0} = {1}", "path", path));
		}
		try
		{
			bool flag = false;
			switch (mode)
			{
			case FileMode.Append:
				throw new NotImplementedException();
			case FileMode.Create:
				flag = true;
				break;
			case FileMode.CreateNew:
				flag = true;
				break;
			}
			if (flag && access == FileAccess.Write)
			{
				treeIsDirty = true;
				RemoteStorage obj = remoteStorage;
				UPath val = UPathExtensions.ToRelative(path);
				return (Stream)(object)obj.CreateFile(((UPath)(ref val)).FullName).OpenWrite();
			}
			return access switch
			{
				FileAccess.Read => GetFileNode(path)?.OpenRead(), 
				FileAccess.Write => GetFileNode(path)?.OpenWrite(), 
				_ => throw new NotImplementedException(), 
			};
		}
		finally
		{
			ExitFileSystemShared();
		}
	}

	protected override FileAttributes GetAttributesImpl(UPath path)
	{
		throw new NotImplementedException();
	}

	protected override void SetAttributesImpl(UPath path, FileAttributes attributes)
	{
	}

	protected override DateTime GetCreationTimeImpl(UPath path)
	{
		throw new NotImplementedException();
	}

	protected override void SetCreationTimeImpl(UPath path, DateTime time)
	{
	}

	protected override DateTime GetLastAccessTimeImpl(UPath path)
	{
		throw new NotImplementedException();
	}

	protected override void SetLastAccessTimeImpl(UPath path, DateTime time)
	{
	}

	protected override DateTime GetLastWriteTimeImpl(UPath path)
	{
		//IL_0005: Unknown result type (might be due to invalid IL or missing references)
		//IL_0006: Unknown result type (might be due to invalid IL or missing references)
		//IL_000b: Unknown result type (might be due to invalid IL or missing references)
		RemoteStorage obj = remoteStorage;
		UPath val = UPathExtensions.ToRelative(path);
		return DateTime.FromFileTimeUtc(obj.OpenFile(((UPath)(ref val)).FullName).FileTimestamp);
	}

	protected override void SetLastWriteTimeImpl(UPath path, DateTime time)
	{
	}

	private FileNode AddFileToTree(string path)
	{
		FileNode fileNode = new FileNode(path);
		AddNodeToTree(fileNode);
		return fileNode;
	}

	private DirectoryNode AddDirectoryToTree(UPath path)
	{
		//IL_0000: Unknown result type (might be due to invalid IL or missing references)
		DirectoryNode directoryNode = new DirectoryNode(path);
		AddNodeToTree(directoryNode);
		return directoryNode;
	}

	private void AddNodeToTree(Node newNode)
	{
		//IL_0001: Unknown result type (might be due to invalid IL or missing references)
		//IL_0006: Unknown result type (might be due to invalid IL or missing references)
		//IL_000b: Unknown result type (might be due to invalid IL or missing references)
		//IL_000d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0020: Unknown result type (might be due to invalid IL or missing references)
		UPath directory = UPathExtensions.GetDirectory(newNode.path);
		GetDirectoryNode(directory).AddChild(newNode);
		pathToNodeMap[newNode.path] = newNode;
	}

	[CanBeNull]
	private DirectoryNode GetDirectoryNode(UPath directoryPath)
	{
		//IL_0006: Unknown result type (might be due to invalid IL or missing references)
		//IL_0018: Unknown result type (might be due to invalid IL or missing references)
		if (pathToNodeMap.TryGetValue(directoryPath, out var value))
		{
			return value as DirectoryNode;
		}
		return AddDirectoryToTree(directoryPath);
	}

	[CanBeNull]
	private FileNode GetFileNode(UPath filePath)
	{
		//IL_0006: Unknown result type (might be due to invalid IL or missing references)
		if (pathToNodeMap.TryGetValue(filePath, out var value))
		{
			return value as FileNode;
		}
		return null;
	}

	private void UpdateDirectories()
	{
		//IL_0075: Unknown result type (might be due to invalid IL or missing references)
		EnterFileSystemShared();
		try
		{
			if (!treeIsDirty)
			{
				return;
			}
			treeIsDirty = false;
			IEnumerable<string> enumerable = remoteStorage.Files.Select((RemoteFile file) => file.FileName);
			if (!enumerable.SequenceEqual(allFilePaths))
			{
				allFilePaths = enumerable.ToArray();
				pathToNodeMap.Clear();
				pathToNodeMap[UPath.Root] = rootNode;
				rootNode.RemoveAllChildren();
				string[] array = allFilePaths;
				foreach (string path in array)
				{
					AddFileToTree(path);
				}
			}
		}
		finally
		{
			ExitFileSystemShared();
		}
	}

	private void AssertDirectory(Node node, UPath srcPath)
	{
		//IL_000d: Unknown result type (might be due to invalid IL or missing references)
		if (node is FileNode)
		{
			throw new IOException($"The source directory `{srcPath}` is a file");
		}
	}

	protected override IEnumerable<UPath> EnumeratePathsImpl(UPath path, string searchPattern, SearchOption searchOption, SearchTarget searchTarget)
	{
		//IL_000f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0010: Unknown result type (might be due to invalid IL or missing references)
		//IL_0024: Unknown result type (might be due to invalid IL or missing references)
		//IL_0026: Unknown result type (might be due to invalid IL or missing references)
		UpdateDirectories();
		SearchPattern search = SearchPattern.Parse(ref path, ref searchPattern);
		List<UPath> foldersToProcess = new List<UPath> { path };
		SortedSet<UPath> entries = new SortedSet<UPath>(UPath.DefaultComparerIgnoreCase);
		while (foldersToProcess.Count > 0)
		{
			UPath val = foldersToProcess[0];
			foldersToProcess.RemoveAt(0);
			int num = 0;
			entries.Clear();
			EnterFileSystemShared();
			try
			{
				Node directoryNode = GetDirectoryNode(val);
				if (val == path)
				{
					AssertDirectory(directoryNode, val);
					goto IL_00cb;
				}
				if (directoryNode is DirectoryNode)
				{
					goto IL_00cb;
				}
				goto end_IL_009b;
				IL_00cb:
				DirectoryNode directoryNode2 = (DirectoryNode)directoryNode;
				for (int i = 0; i < directoryNode2.childCount; i++)
				{
					Node child = directoryNode2.GetChild(i);
					if (!(child is FileNode) || (int)searchTarget != 2)
					{
						bool flag = ((SearchPattern)(ref search)).Match(child.path);
						bool num2 = searchOption == SearchOption.AllDirectories && child is DirectoryNode;
						bool flag2 = (child is FileNode && (int)searchTarget != 2 && flag) || (child is DirectoryNode && (int)searchTarget != 1 && flag);
						UPath item = val / child.path;
						if (num2)
						{
							foldersToProcess.Insert(num++, item);
						}
						if (flag2)
						{
							entries.Add(item);
						}
					}
				}
				goto IL_01b6;
				end_IL_009b:;
			}
			finally
			{
				ExitFileSystemShared();
			}
			continue;
			IL_01b6:
			foreach (UPath item2 in entries)
			{
				yield return item2;
			}
		}
	}

	private static void EnterFileSystemShared()
	{
		Monitor.Enter(globalLock);
	}

	private static void ExitFileSystemShared()
	{
		Monitor.Exit(globalLock);
	}

	protected override IFileSystemWatcher WatchImpl(UPath path)
	{
		throw new NotImplementedException();
	}

	protected override string ConvertPathToInternalImpl(UPath path)
	{
		return ((UPath)(ref path)).FullName;
	}

	protected override UPath ConvertPathFromInternalImpl(string innerPath)
	{
		//IL_0001: Unknown result type (might be due to invalid IL or missing references)
		return new UPath(innerPath);
	}

	[ConCommand(commandName = "steam_remote_storage_list_files", flags = ConVarFlags.None, helpText = "Lists the files currently being managed by Steamworks remote storage.")]
	private static void CCSteamRemoteStorageListFiles(ConCommandArgs args)
	{
		Debug.Log((object)string.Join("\n", remoteStorage.Files.Select((RemoteFile file) => $"{file.FileName} .. {file.SizeInBytes}b").ToArray()));
	}
}
