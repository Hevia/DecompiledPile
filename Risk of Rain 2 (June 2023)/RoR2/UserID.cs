using Epic.OnlineServices;

namespace RoR2;

public struct UserID
{
	private const bool useCIDforComparison = true;

	public CSteamID CID;

	public ulong ID => CID.steamValue;

	public bool isValid => CID.isValid;

	public UserID(ulong id)
	{
		CID = new CSteamID(id);
	}

	public UserID(CSteamID cid)
	{
		CID = cid;
	}

	public static explicit operator UserID(CSteamID cid)
	{
		return new UserID(cid);
	}

	public static bool TryParse(string str, out UserID result)
	{
		if (CSteamID.TryParse(str, out var result2))
		{
			result = new UserID(result2);
			return true;
		}
		result.CID = CSteamID.nil;
		return false;
	}

	public override string ToString()
	{
		return CID.ToString();
	}

	public UserID(ProductUserId id)
	{
		CID = new CSteamID(id);
	}

	public static explicit operator UserID(ulong id)
	{
		return new UserID(id);
	}

	public override bool Equals(object obj)
	{
		if (obj is UserID userID)
		{
			return userID == this;
		}
		return false;
	}

	public override int GetHashCode()
	{
		return ID.GetHashCode();
	}

	public static bool operator ==(UserID first, UserID second)
	{
		return first.CID == second.CID;
	}

	public static bool operator !=(UserID first, UserID second)
	{
		return first.CID != second.CID;
	}

	public static bool operator ==(UserID first, ulong second)
	{
		return first.ID == second;
	}

	public static bool operator !=(UserID first, ulong second)
	{
		return first.ID != second;
	}
}
