using UnityEngine;
using UnityEngine.Networking;

namespace RoR2.Networking;

public class NetworkedViewAngles : NetworkBehaviour
{
	public PitchYawPair viewAngles;

	private PitchYawPair networkDesiredViewAngles;

	private PitchYawPair velocity;

	private NetworkIdentity networkIdentity;

	public float sendRate = 0.05f;

	public float bufferMultiplier = 3f;

	public float maxSmoothVelocity = 1440f;

	private float sendTimer;

	private static int kCmdCmdUpdateViewAngles;

	public bool hasEffectiveAuthority { get; private set; }

	private void Awake()
	{
		networkIdentity = ((Component)this).GetComponent<NetworkIdentity>();
	}

	private void Update()
	{
		hasEffectiveAuthority = Util.HasEffectiveAuthority(networkIdentity);
		if (hasEffectiveAuthority)
		{
			networkDesiredViewAngles = viewAngles;
		}
		else
		{
			viewAngles = PitchYawPair.SmoothDamp(viewAngles, networkDesiredViewAngles, ref velocity, ((NetworkBehaviour)this).GetNetworkSendInterval() * bufferMultiplier, maxSmoothVelocity);
		}
	}

	public override float GetNetworkSendInterval()
	{
		return sendRate;
	}

	private void FixedUpdate()
	{
		if (NetworkServer.active)
		{
			((NetworkBehaviour)this).SetDirtyBit(1u);
		}
		hasEffectiveAuthority = Util.HasEffectiveAuthority(networkIdentity);
		if (!hasEffectiveAuthority)
		{
			return;
		}
		networkDesiredViewAngles = viewAngles;
		if (!NetworkServer.active)
		{
			sendTimer -= Time.deltaTime;
			if (sendTimer <= 0f)
			{
				CallCmdUpdateViewAngles(viewAngles.pitch, viewAngles.yaw);
				sendTimer = ((NetworkBehaviour)this).GetNetworkSendInterval();
			}
		}
	}

	[Command(channel = 5)]
	public void CmdUpdateViewAngles(float pitch, float yaw)
	{
		networkDesiredViewAngles = new PitchYawPair(pitch, yaw);
	}

	public override bool OnSerialize(NetworkWriter writer, bool initialState)
	{
		writer.Write(networkDesiredViewAngles);
		return true;
	}

	public override void OnDeserialize(NetworkReader reader, bool initialState)
	{
		PitchYawPair pitchYawPair = reader.ReadPitchYawPair();
		if (!hasEffectiveAuthority)
		{
			networkDesiredViewAngles = pitchYawPair;
			if (initialState)
			{
				viewAngles = pitchYawPair;
				velocity = PitchYawPair.zero;
			}
		}
	}

	private void UNetVersion()
	{
	}

	protected static void InvokeCmdCmdUpdateViewAngles(NetworkBehaviour obj, NetworkReader reader)
	{
		if (!NetworkServer.active)
		{
			Debug.LogError((object)"Command CmdUpdateViewAngles called on client.");
		}
		else
		{
			((NetworkedViewAngles)(object)obj).CmdUpdateViewAngles(reader.ReadSingle(), reader.ReadSingle());
		}
	}

	public void CallCmdUpdateViewAngles(float pitch, float yaw)
	{
		//IL_0031: Unknown result type (might be due to invalid IL or missing references)
		//IL_0037: Expected O, but got Unknown
		//IL_005c: Unknown result type (might be due to invalid IL or missing references)
		if (!NetworkClient.active)
		{
			Debug.LogError((object)"Command function CmdUpdateViewAngles called on server.");
			return;
		}
		if (((NetworkBehaviour)this).isServer)
		{
			CmdUpdateViewAngles(pitch, yaw);
			return;
		}
		NetworkWriter val = new NetworkWriter();
		val.Write((short)0);
		val.Write((short)5);
		val.WritePackedUInt32((uint)kCmdCmdUpdateViewAngles);
		val.Write(((Component)this).GetComponent<NetworkIdentity>().netId);
		val.Write(pitch);
		val.Write(yaw);
		((NetworkBehaviour)this).SendCommandInternal(val, 5, "CmdUpdateViewAngles");
	}

	static NetworkedViewAngles()
	{
		//IL_0020: Unknown result type (might be due to invalid IL or missing references)
		//IL_002a: Expected O, but got Unknown
		kCmdCmdUpdateViewAngles = -1684781536;
		NetworkBehaviour.RegisterCommandDelegate(typeof(NetworkedViewAngles), kCmdCmdUpdateViewAngles, new CmdDelegate(InvokeCmdCmdUpdateViewAngles));
		NetworkCRC.RegisterBehaviour("NetworkedViewAngles", 0);
	}

	public override void PreStartClient()
	{
	}
}
