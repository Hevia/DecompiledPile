using System;
using System.Collections.Generic;
using Zio;

namespace RoR2;

public struct FileReference : IEquatable<FileReference>
{
	public IFileSystem fileSystem;

	public UPath path;

	public bool Equals(FileReference other)
	{
		//IL_001a: Unknown result type (might be due to invalid IL or missing references)
		if (((object)fileSystem).Equals((object?)other.fileSystem))
		{
			return ((UPath)(ref path)).Equals(other.path);
		}
		return false;
	}

	public override bool Equals(object other)
	{
		if (other is FileReference)
		{
			return Equals((FileReference)other);
		}
		return false;
	}

	public override int GetHashCode()
	{
		//IL_0028: Unknown result type (might be due to invalid IL or missing references)
		return (-990633296 * -1521134295 + EqualityComparer<IFileSystem>.Default.GetHashCode(fileSystem)) * -1521134295 + EqualityComparer<UPath>.Default.GetHashCode(path);
	}
}
