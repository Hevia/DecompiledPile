using Unity;
using UnityEngine;
using UnityEngine.Networking;

namespace RoR2.Achievements;

[RequireComponent(typeof(NetworkUser))]
public class ServerAchievementTracker : NetworkBehaviour
{
	private BaseServerAchievement[] achievementTrackers;

	private SerializableBitArray maskBitArrayConverter;

	private byte[] maskBuffer;

	private static int kCmdCmdSetAchievementTrackerRequests;

	private static int kRpcRpcGrantAchievement;

	public NetworkUser networkUser { get; private set; }

	private void Awake()
	{
		networkUser = ((Component)this).GetComponent<NetworkUser>();
		maskBitArrayConverter = new SerializableBitArray(AchievementManager.serverAchievementCount);
		if (NetworkServer.active)
		{
			achievementTrackers = new BaseServerAchievement[AchievementManager.serverAchievementCount];
		}
		if (NetworkClient.active)
		{
			maskBuffer = new byte[maskBitArrayConverter.byteCount];
		}
	}

	private void Start()
	{
		if (networkUser.localUser != null)
		{
			AchievementManager.GetUserAchievementManager(networkUser.localUser)?.TransmitAchievementRequestsToServer();
		}
	}

	private void OnDestroy()
	{
		if (achievementTrackers != null)
		{
			int serverAchievementCount = AchievementManager.serverAchievementCount;
			for (int i = 0; i < serverAchievementCount; i++)
			{
				SetAchievementTracked(new ServerAchievementIndex
				{
					intValue = i
				}, shouldTrack: false);
			}
		}
	}

	[Client]
	public void SendAchievementTrackerRequestsMaskToServer(bool[] serverAchievementsToTrackMask)
	{
		if (!NetworkClient.active)
		{
			Debug.LogWarning((object)"[Client] function 'System.Void RoR2.Achievements.ServerAchievementTracker::SendAchievementTrackerRequestsMaskToServer(System.Boolean[])' called on server");
			return;
		}
		int serverAchievementCount = AchievementManager.serverAchievementCount;
		for (int i = 0; i < serverAchievementCount; i++)
		{
			maskBitArrayConverter[i] = serverAchievementsToTrackMask[i];
		}
		maskBitArrayConverter.GetBytes(maskBuffer);
		CallCmdSetAchievementTrackerRequests(maskBuffer);
	}

	[Command]
	private void CmdSetAchievementTrackerRequests(byte[] packedServerAchievementsToTrackMask)
	{
		int serverAchievementCount = AchievementManager.serverAchievementCount;
		if (packedServerAchievementsToTrackMask.Length << 3 >= serverAchievementCount)
		{
			for (int i = 0; i < serverAchievementCount; i++)
			{
				int num = i >> 3;
				int num2 = i & 7;
				SetAchievementTracked(new ServerAchievementIndex
				{
					intValue = i
				}, ((packedServerAchievementsToTrackMask[num] >> num2) & 1) != 0);
			}
		}
	}

	private void SetAchievementTracked(ServerAchievementIndex serverAchievementIndex, bool shouldTrack)
	{
		BaseServerAchievement baseServerAchievement = achievementTrackers[serverAchievementIndex.intValue];
		if (shouldTrack != (baseServerAchievement != null))
		{
			if (shouldTrack)
			{
				BaseServerAchievement baseServerAchievement2 = BaseServerAchievement.Instantiate(serverAchievementIndex);
				baseServerAchievement2.serverAchievementTracker = this;
				achievementTrackers[serverAchievementIndex.intValue] = baseServerAchievement2;
				baseServerAchievement2.OnInstall();
			}
			else
			{
				baseServerAchievement.OnUninstall();
				achievementTrackers[serverAchievementIndex.intValue] = null;
			}
		}
	}

	[ClientRpc]
	public void RpcGrantAchievement(ServerAchievementIndex serverAchievementIndex)
	{
		LocalUser localUser = networkUser.localUser;
		if (localUser != null)
		{
			AchievementManager.GetUserAchievementManager(localUser)?.HandleServerAchievementCompleted(serverAchievementIndex);
		}
	}

	private void UNetVersion()
	{
	}

	protected static void InvokeCmdCmdSetAchievementTrackerRequests(NetworkBehaviour obj, NetworkReader reader)
	{
		if (!NetworkServer.active)
		{
			Debug.LogError((object)"Command CmdSetAchievementTrackerRequests called on client.");
		}
		else
		{
			((ServerAchievementTracker)(object)obj).CmdSetAchievementTrackerRequests(reader.ReadBytesAndSize());
		}
	}

	public void CallCmdSetAchievementTrackerRequests(byte[] packedServerAchievementsToTrackMask)
	{
		//IL_002d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0033: Expected O, but got Unknown
		//IL_0058: Unknown result type (might be due to invalid IL or missing references)
		if (!NetworkClient.active)
		{
			Debug.LogError((object)"Command function CmdSetAchievementTrackerRequests called on server.");
			return;
		}
		if (((NetworkBehaviour)this).isServer)
		{
			CmdSetAchievementTrackerRequests(packedServerAchievementsToTrackMask);
			return;
		}
		NetworkWriter val = new NetworkWriter();
		val.Write((short)0);
		val.Write((short)5);
		val.WritePackedUInt32((uint)kCmdCmdSetAchievementTrackerRequests);
		val.Write(((Component)this).GetComponent<NetworkIdentity>().netId);
		val.WriteBytesFull(packedServerAchievementsToTrackMask);
		((NetworkBehaviour)this).SendCommandInternal(val, 0, "CmdSetAchievementTrackerRequests");
	}

	protected static void InvokeRpcRpcGrantAchievement(NetworkBehaviour obj, NetworkReader reader)
	{
		if (!NetworkClient.active)
		{
			Debug.LogError((object)"RPC RpcGrantAchievement called on server.");
		}
		else
		{
			((ServerAchievementTracker)(object)obj).RpcGrantAchievement(GeneratedNetworkCode._ReadServerAchievementIndex_None(reader));
		}
	}

	public void CallRpcGrantAchievement(ServerAchievementIndex serverAchievementIndex)
	{
		//IL_0016: Unknown result type (might be due to invalid IL or missing references)
		//IL_001c: Expected O, but got Unknown
		//IL_0041: Unknown result type (might be due to invalid IL or missing references)
		if (!NetworkServer.active)
		{
			Debug.LogError((object)"RPC Function RpcGrantAchievement called on client.");
			return;
		}
		NetworkWriter val = new NetworkWriter();
		val.Write((short)0);
		val.Write((short)2);
		val.WritePackedUInt32((uint)kRpcRpcGrantAchievement);
		val.Write(((Component)this).GetComponent<NetworkIdentity>().netId);
		GeneratedNetworkCode._WriteServerAchievementIndex_None(val, serverAchievementIndex);
		((NetworkBehaviour)this).SendRPCInternal(val, 0, "RpcGrantAchievement");
	}

	static ServerAchievementTracker()
	{
		//IL_0020: Unknown result type (might be due to invalid IL or missing references)
		//IL_002a: Expected O, but got Unknown
		//IL_004a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0054: Expected O, but got Unknown
		kCmdCmdSetAchievementTrackerRequests = 387052099;
		NetworkBehaviour.RegisterCommandDelegate(typeof(ServerAchievementTracker), kCmdCmdSetAchievementTrackerRequests, new CmdDelegate(InvokeCmdCmdSetAchievementTrackerRequests));
		kRpcRpcGrantAchievement = -1713740939;
		NetworkBehaviour.RegisterRpcDelegate(typeof(ServerAchievementTracker), kRpcRpcGrantAchievement, new CmdDelegate(InvokeRpcRpcGrantAchievement));
		NetworkCRC.RegisterBehaviour("ServerAchievementTracker", 0);
	}

	public override bool OnSerialize(NetworkWriter writer, bool forceAll)
	{
		bool result = default(bool);
		return result;
	}

	public override void OnDeserialize(NetworkReader reader, bool initialState)
	{
	}

	public override void PreStartClient()
	{
	}
}
