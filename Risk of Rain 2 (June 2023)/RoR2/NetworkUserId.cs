using System;
using UnityEngine;

namespace RoR2;

[Serializable]
public struct NetworkUserId : IEquatable<NetworkUserId>
{
	[SerializeField]
	public ulong value;

	[SerializeField]
	public string strValue;

	[SerializeField]
	public readonly byte subId;

	public CSteamID steamId => new CSteamID(value);

	private NetworkUserId(ulong value, byte subId)
	{
		this.value = value;
		this.subId = subId;
		strValue = null;
	}

	public NetworkUserId(string strValue, byte subId)
	{
		value = 0uL;
		this.subId = subId;
		this.strValue = strValue;
	}

	public NetworkUserId(UserID userId, byte subId)
	{
		if (userId.CID.isSteam)
		{
			value = userId.CID.steamValue;
			strValue = null;
		}
		else
		{
			strValue = userId.CID.stringValue;
			value = 0uL;
		}
		this.subId = subId;
	}

	public static NetworkUserId FromIp(string ip, byte subId)
	{
		return new NetworkUserId((ulong)ip.GetHashCode(), subId);
	}

	public static NetworkUserId FromId(ulong id, byte subId)
	{
		return new NetworkUserId(id, subId);
	}

	public bool HasValidValue()
	{
		if (value == 0L)
		{
			return strValue != null;
		}
		return true;
	}

	public bool Equals(NetworkUserId other)
	{
		if (value == other.value && strValue == other.strValue)
		{
			return subId == other.subId;
		}
		return false;
	}

	public override bool Equals(object obj)
	{
		if (obj == null)
		{
			return false;
		}
		if (obj is NetworkUserId)
		{
			return Equals((NetworkUserId)obj);
		}
		return false;
	}

	public override int GetHashCode()
	{
		int num = value.GetHashCode() * 397;
		byte b = subId;
		return num ^ b.GetHashCode();
	}
}
