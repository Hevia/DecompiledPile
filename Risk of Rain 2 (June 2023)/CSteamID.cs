using System;
using System.Globalization;
using Epic.OnlineServices;
using RoR2;

public struct CSteamID : IEquatable<CSteamID>, IEquatable<UserID>
{
	public enum EAccountType
	{
		k_EAccountTypeInvalid,
		k_EAccountTypeIndividual,
		k_EAccountTypeMultiseat,
		k_EAccountTypeGameServer,
		k_EAccountTypeAnonGameServer,
		k_EAccountTypePending,
		k_EAccountTypeContentServer,
		k_EAccountTypeClan,
		k_EAccountTypeChat,
		k_EAccountTypeConsoleUser,
		k_EAccountTypeAnonUser,
		k_EAccountTypeMax
	}

	public enum EUniverse
	{
		k_EUniverseInvalid,
		k_EUniversePublic,
		k_EUniverseBeta,
		k_EUniverseInternal,
		k_EUniverseDev,
		k_EUniverseMax
	}

	public readonly string stringValue;

	public readonly ulong steamValue;

	public static readonly CSteamID nil;

	public ProductUserId egsValue => ProductUserId.FromString(stringValue);

	public bool isValid
	{
		get
		{
			if (!isSteam)
			{
				return isEGS;
			}
			return true;
		}
	}

	public bool isSteam => steamValue != 0;

	public bool isEGS => stringValue != null;

	public object value
	{
		get
		{
			if (steamValue != 0L)
			{
				return steamValue;
			}
			if (stringValue != null)
			{
				return egsValue;
			}
			return null;
		}
	}

	public uint accountId => GetBitField(0, 32);

	public uint accountInstance => GetBitField(32, 20);

	public EAccountType accountType => (EAccountType)GetBitField(52, 4);

	public EUniverse universe => (EUniverse)GetBitField(56, 8);

	public bool isLobby => accountType == EAccountType.k_EAccountTypeChat;

	public CSteamID(ProductUserId egsValue)
	{
		stringValue = ((object)egsValue).ToString();
		steamValue = 0uL;
	}

	public CSteamID(string egsValue)
	{
		stringValue = egsValue;
		steamValue = 0uL;
	}

	public CSteamID(ulong value)
	{
		steamValue = value;
		stringValue = null;
	}

	public override string ToString()
	{
		if (value != null)
		{
			return value.ToString();
		}
		return string.Empty;
	}

	private static bool ParseFromString(string str, out CSteamID result)
	{
		bool flag = default(bool);
		if (str[0] <= '9')
		{
			flag = TextSerialization.TryParseInvariant(str, out ulong result2);
			if (flag)
			{
				result = new CSteamID(flag ? result2 : 0);
				return flag;
			}
		}
		result = nil;
		if (PlatformSystems.EgsToggleConVar.value == 1 && str.Length <= 32)
		{
			ProductUserId val = ProductUserId.FromString(str, ref flag);
			if (flag && (Handle)(object)val != (Handle)null)
			{
				result = new CSteamID(val);
				return true;
			}
		}
		return false;
	}

	public static bool operator ==(CSteamID a, CSteamID b)
	{
		return a.Equals(b);
	}

	public static bool operator !=(CSteamID a, CSteamID b)
	{
		return !a.Equals(b);
	}

	public static bool operator ==(CSteamID a, UserID b)
	{
		return a.Equals(b);
	}

	public static bool operator !=(CSteamID a, UserID b)
	{
		return !a.Equals(b);
	}

	public override bool Equals(object obj)
	{
		if (!(obj is CSteamID) || !Equals((CSteamID)obj))
		{
			if (obj is UserID)
			{
				return Equals((UserID)obj);
			}
			return false;
		}
		return true;
	}

	public bool Equals(CSteamID other)
	{
		if (steamValue == other.steamValue)
		{
			return stringValue == other.stringValue;
		}
		return false;
	}

	public bool Equals(UserID other)
	{
		if (!isSteam)
		{
			return false;
		}
		return steamValue == other.ID;
	}

	public override int GetHashCode()
	{
		if (value != null)
		{
			return value.GetHashCode();
		}
		return 0;
	}

	public static bool TryParse(string str, out CSteamID result)
	{
		if (!string.IsNullOrEmpty(str))
		{
			return ParseFromString(str, out result);
		}
		result = nil;
		return false;
	}

	private uint GetBitField(int bitOffset, int bitCount)
	{
		uint num = uint.MaxValue >> 32 - bitCount;
		return (uint)(int)(steamValue >> bitOffset) & num;
	}

	public string ToSteamID()
	{
		uint num = accountId;
		return string.Format(CultureInfo.InvariantCulture, "STEAM_{0}:{1}:{2}", (uint)universe, num & 1u, num >> 1);
	}

	public string ToCommunityID()
	{
		char c = 'I';
		switch (accountType)
		{
		case EAccountType.k_EAccountTypeInvalid:
			c = 'I';
			break;
		case EAccountType.k_EAccountTypeIndividual:
			c = 'U';
			break;
		case EAccountType.k_EAccountTypeMultiseat:
			c = 'M';
			break;
		case EAccountType.k_EAccountTypeGameServer:
			c = 'G';
			break;
		case EAccountType.k_EAccountTypeAnonGameServer:
			c = 'A';
			break;
		case EAccountType.k_EAccountTypePending:
			c = 'P';
			break;
		case EAccountType.k_EAccountTypeContentServer:
			c = 'C';
			break;
		case EAccountType.k_EAccountTypeClan:
			c = 'g';
			break;
		case EAccountType.k_EAccountTypeChat:
			c = 'T';
			break;
		case EAccountType.k_EAccountTypeConsoleUser:
			c = 'I';
			break;
		case EAccountType.k_EAccountTypeAnonUser:
			c = 'a';
			break;
		case EAccountType.k_EAccountTypeMax:
			c = 'I';
			break;
		}
		return string.Format(CultureInfo.InvariantCulture, "[{0}:{1}:{2}]", c, 1, accountId);
	}
}
