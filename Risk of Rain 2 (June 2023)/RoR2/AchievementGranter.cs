using UnityEngine;
using UnityEngine.Networking;

namespace RoR2;

public class AchievementGranter : NetworkBehaviour
{
	private static int kRpcRpcGrantAchievement;

	[ClientRpc]
	public void RpcGrantAchievement(string achievementName)
	{
		foreach (LocalUser readOnlyLocalUsers in LocalUserManager.readOnlyLocalUsersList)
		{
			AchievementManager.GetUserAchievementManager(readOnlyLocalUsers).GrantAchievement(AchievementManager.GetAchievementDef(achievementName));
		}
	}

	private void UNetVersion()
	{
	}

	protected static void InvokeRpcRpcGrantAchievement(NetworkBehaviour obj, NetworkReader reader)
	{
		if (!NetworkClient.active)
		{
			Debug.LogError((object)"RPC RpcGrantAchievement called on server.");
		}
		else
		{
			((AchievementGranter)(object)obj).RpcGrantAchievement(reader.ReadString());
		}
	}

	public void CallRpcGrantAchievement(string achievementName)
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
		val.Write(achievementName);
		((NetworkBehaviour)this).SendRPCInternal(val, 0, "RpcGrantAchievement");
	}

	static AchievementGranter()
	{
		//IL_0020: Unknown result type (might be due to invalid IL or missing references)
		//IL_002a: Expected O, but got Unknown
		kRpcRpcGrantAchievement = -180752285;
		NetworkBehaviour.RegisterRpcDelegate(typeof(AchievementGranter), kRpcRpcGrantAchievement, new CmdDelegate(InvokeRpcRpcGrantAchievement));
		NetworkCRC.RegisterBehaviour("AchievementGranter", 0);
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
