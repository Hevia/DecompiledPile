using UnityEngine.Networking;

namespace RoR2.Networking;

public class EOSNetworkClient : NetworkClient
{
	public EOSNetworkConnection eosConnection => (EOSNetworkConnection)(object)((NetworkClient)this).connection;

	public string status => ((object)(ConnectState)(ref base.m_AsyncConnect)).ToString();

	public void Connect()
	{
		//IL_000e: Unknown result type (might be due to invalid IL or missing references)
		((NetworkClient)this).Connect("localhost", 0);
		base.m_AsyncConnect = (ConnectState)4;
		((NetworkClient)this).connection.ForceInitialize(((NetworkClient)this).hostTopology);
	}

	public EOSNetworkClient(NetworkConnection conn)
		: base(conn)
	{
		((NetworkClient)this).SetNetworkConnectionClass<EOSNetworkConnection>();
	}
}
