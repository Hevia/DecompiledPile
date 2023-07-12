using Unity;
using UnityEngine.Networking;

namespace RoR2.Networking;

public class ClientAuthData : MessageBase
{
	public CSteamID steamId;

	public byte[] authTicket;

	public string[] entitlements;

	public string password;

	public string version;

	public string modHash;

	public override void Serialize(NetworkWriter writer)
	{
		GeneratedNetworkCode._WriteCSteamID_None(writer, steamId);
		writer.WriteBytesFull(authTicket);
		GeneratedNetworkCode._WriteArrayString_None(writer, entitlements);
		writer.Write(password);
		writer.Write(version);
		writer.Write(modHash);
	}

	public override void Deserialize(NetworkReader reader)
	{
		steamId = GeneratedNetworkCode._ReadCSteamID_None(reader);
		authTicket = reader.ReadBytesAndSize();
		entitlements = GeneratedNetworkCode._ReadArrayString_None(reader);
		password = reader.ReadString();
		version = reader.ReadString();
		modHash = reader.ReadString();
	}
}
