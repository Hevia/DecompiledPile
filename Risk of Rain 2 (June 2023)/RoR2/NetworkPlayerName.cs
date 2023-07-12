using Facepunch.Steamworks;
using UnityEngine.Networking;

namespace RoR2;

public struct NetworkPlayerName
{
	public CSteamID steamId;

	public string nameOverride;

	public void Deserialize(NetworkReader reader)
	{
		if (reader.ReadBoolean())
		{
			steamId = CSteamID.nil;
			nameOverride = reader.ReadString();
		}
		else
		{
			steamId = new CSteamID(reader.ReadUInt64());
			nameOverride = null;
		}
	}

	public void Serialize(NetworkWriter writer)
	{
		bool flag = nameOverride != null;
		writer.Write(flag);
		if (flag)
		{
			writer.Write(nameOverride);
		}
		else
		{
			writer.Write((ulong)steamId.value);
		}
	}

	public string GetResolvedName()
	{
		if (!string.IsNullOrEmpty(nameOverride))
		{
			return (PlatformSystems.lobbyManager as EOSLobbyManager).GetUserDisplayNameFromProductIdString(nameOverride);
		}
		if (PlatformSystems.ShouldUseEpicOnlineSystems)
		{
			EOSLobbyManager obj = PlatformSystems.lobbyManager as EOSLobbyManager;
			UserID user = new UserID(steamId);
			return obj.GetUserDisplayName(user);
		}
		Client instance = Client.Instance;
		if (instance != null)
		{
			return instance.Friends.GetName(steamId.steamValue);
		}
		return "???";
	}
}
