using System;
using System.Text;
using Epic.OnlineServices;
using UnityEngine.Networking;

namespace RoR2.Networking;

public struct HostDescription : IEquatable<HostDescription>
{
	public enum HostType
	{
		None,
		Self,
		Steam,
		IPv4,
		Pia,
		PS4,
		EOS
	}

	public struct HostingParameters : IEquatable<HostingParameters>
	{
		public bool listen;

		public int maxPlayers;

		public bool Equals(HostingParameters other)
		{
			if (listen == other.listen)
			{
				return maxPlayers == other.maxPlayers;
			}
			return false;
		}

		public override bool Equals(object obj)
		{
			if (obj == null)
			{
				return false;
			}
			if (obj is HostingParameters other)
			{
				return Equals(other);
			}
			return false;
		}

		public override int GetHashCode()
		{
			return (listen.GetHashCode() * 397) ^ maxPlayers;
		}
	}

	public readonly HostType hostType;

	public readonly UserID userID;

	public readonly AddressPortPair addressPortPair;

	public readonly HostingParameters hostingParameters;

	public static readonly HostDescription none = new HostDescription(null);

	private static readonly StringBuilder sharedStringBuilder = new StringBuilder();

	public bool isRemote
	{
		get
		{
			if (hostType != 0)
			{
				return hostType != HostType.Self;
			}
			return false;
		}
	}

	public HostDescription(UserID id, HostType type)
	{
		this = default(HostDescription);
		hostType = type;
		userID = id;
	}

	public HostDescription(AddressPortPair addressPortPair)
	{
		this = default(HostDescription);
		hostType = HostType.IPv4;
		this.addressPortPair = addressPortPair;
	}

	public HostDescription(HostingParameters hostingParameters)
	{
		this = default(HostDescription);
		hostType = HostType.Self;
		this.hostingParameters = hostingParameters;
	}

	public bool DescribesCurrentHost()
	{
		switch (hostType)
		{
		case HostType.None:
			return !((NetworkManager)NetworkManagerSystem.singleton).isNetworkActive;
		case HostType.Self:
			if (NetworkServer.active && hostingParameters.listen != NetworkServer.dontListen)
			{
				return true;
			}
			return false;
		case HostType.Steam:
		{
			NetworkClient client3 = ((NetworkManager)NetworkManagerSystem.singleton).client;
			if (((client3 != null) ? client3.connection : null) is SteamNetworkConnection steamNetworkConnection && steamNetworkConnection.steamId == userID.CID)
			{
				return true;
			}
			return false;
		}
		case HostType.EOS:
		{
			NetworkClient client2 = ((NetworkManager)NetworkManagerSystem.singleton).client;
			if (((client2 != null) ? client2.connection : null) is EOSNetworkConnection eOSNetworkConnection && (Handle)(object)eOSNetworkConnection.RemoteUserID == (Handle)(object)userID.CID.egsValue)
			{
				return true;
			}
			return false;
		}
		case HostType.IPv4:
		{
			NetworkClient client = ((NetworkManager)NetworkManagerSystem.singleton).client;
			NetworkConnection val;
			if ((val = ((client != null) ? client.connection : null)) != null && val.address == addressPortPair.address)
			{
				return true;
			}
			return false;
		}
		default:
			throw new ArgumentOutOfRangeException();
		}
	}

	private HostDescription(object o)
	{
		this = default(HostDescription);
		hostType = HostType.None;
	}

	public bool Equals(HostDescription other)
	{
		if (hostType == other.hostType)
		{
			UserID userID = this.userID;
			if (userID.Equals(other.userID) && addressPortPair.Equals(other.addressPortPair))
			{
				return hostingParameters.Equals(other.hostingParameters);
			}
		}
		return false;
	}

	public override bool Equals(object obj)
	{
		if (obj == null)
		{
			return false;
		}
		if (obj is HostDescription other)
		{
			return Equals(other);
		}
		return false;
	}

	public override int GetHashCode()
	{
		int num = (int)hostType * 397;
		CSteamID cID = userID.CID;
		int num2 = (num ^ cID.GetHashCode()) * 397;
		AddressPortPair addressPortPair = this.addressPortPair;
		int num3 = (num2 ^ addressPortPair.GetHashCode()) * 397;
		HostingParameters hostingParameters = this.hostingParameters;
		return num3 ^ hostingParameters.GetHashCode();
	}

	public override string ToString()
	{
		sharedStringBuilder.Clear();
		sharedStringBuilder.Append("{ hostType=").Append(hostType);
		switch (hostType)
		{
		case HostType.Self:
			sharedStringBuilder.Append(" listen=").Append(hostingParameters.listen);
			sharedStringBuilder.Append(" maxPlayers=").Append(hostingParameters.maxPlayers);
			break;
		case HostType.Steam:
			sharedStringBuilder.Append(" steamId=").Append(this.userID.CID);
			break;
		case HostType.IPv4:
			sharedStringBuilder.Append(" address=").Append(addressPortPair.address);
			sharedStringBuilder.Append(" port=").Append(addressPortPair.port);
			break;
		case HostType.EOS:
		{
			StringBuilder stringBuilder = sharedStringBuilder.Append(" steamId=");
			UserID userID = this.userID;
			stringBuilder.Append(userID.ToString());
			break;
		}
		default:
			throw new ArgumentOutOfRangeException();
		case HostType.None:
			break;
		}
		sharedStringBuilder.Append(" }");
		return sharedStringBuilder.ToString();
	}
}
