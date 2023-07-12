using UnityEngine.Networking;

namespace RoR2.Networking;

public class SteamNetworkClient : NetworkClient
{
	public SteamNetworkConnection steamConnection => (SteamNetworkConnection)(object)((NetworkClient)this).connection;

	public string status => ((object)(ConnectState)(ref base.m_AsyncConnect)).ToString();

	public void Connect()
	{
		//IL_000e: Unknown result type (might be due to invalid IL or missing references)
		((NetworkClient)this).Connect("localhost", 0);
		base.m_AsyncConnect = (ConnectState)4;
		((NetworkClient)this).connection.ForceInitialize(((NetworkClient)this).hostTopology);
	}

	public SteamNetworkClient(NetworkConnection conn)
		: base(conn)
	{
		((NetworkClient)this).SetNetworkConnectionClass<SteamNetworkConnection>();
	}
}
