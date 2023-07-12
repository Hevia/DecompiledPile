namespace RoR2.Social;

public struct SocialUserId
{
	public readonly CSteamID steamId;

	public SocialUserId(CSteamID steamId)
	{
		this.steamId = steamId;
	}
}
