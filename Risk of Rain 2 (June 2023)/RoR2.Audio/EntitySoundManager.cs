using RoR2.Networking;
using UnityEngine;
using UnityEngine.Networking;

namespace RoR2.Audio;

public static class EntitySoundManager
{
	private class EntitySoundMessage : MessageBase
	{
		public NetworkSoundEventIndex networkSoundEventIndex;

		public NetworkIdentity networkIdentity;

		public override void Serialize(NetworkWriter writer)
		{
			((MessageBase)this).Serialize(writer);
			writer.WriteNetworkSoundEventIndex(networkSoundEventIndex);
			writer.Write(networkIdentity);
		}

		public override void Deserialize(NetworkReader reader)
		{
			((MessageBase)this).Deserialize(reader);
			networkSoundEventIndex = reader.ReadNetworkSoundEventIndex();
			networkIdentity = reader.ReadNetworkIdentity();
		}

		public void Clear()
		{
			networkSoundEventIndex = NetworkSoundEventIndex.Invalid;
			networkIdentity = null;
		}
	}

	private static readonly EntitySoundMessage sharedMessage = new EntitySoundMessage();

	private static readonly QosChannelIndex channel = QosChannelIndex.defaultReliable;

	private static readonly short messageType = 73;

	[NetworkMessageHandler(client = true, server = false, msgType = 73)]
	private static void HandleMessage(NetworkMessage netMsg)
	{
		netMsg.ReadMessage<EntitySoundMessage>(sharedMessage);
		if (Object.op_Implicit((Object)(object)sharedMessage.networkIdentity))
		{
			EmitSoundLocal(NetworkSoundEventCatalog.GetAkIdFromNetworkSoundEventIndex(sharedMessage.networkSoundEventIndex), ((Component)sharedMessage.networkIdentity).gameObject);
		}
		sharedMessage.Clear();
	}

	public static uint EmitSoundLocal(AkEventIdArg akEventId, GameObject gameObject)
	{
		if ((uint)akEventId == 0)
		{
			return 0u;
		}
		return AkSoundEngine.PostEvent((uint)akEventId, gameObject);
	}

	public static void EmitSoundServer(AkEventIdArg akEventId, GameObject gameObject)
	{
		NetworkSoundEventIndex networkSoundEventIndex = NetworkSoundEventCatalog.FindNetworkSoundEventIndex(akEventId);
		if (networkSoundEventIndex == NetworkSoundEventIndex.Invalid)
		{
			Debug.LogWarningFormat("Cannot emit sound \"{0}\" on object \"{1}\": Event is not registered in NetworkSoundEventCatalog.", new object[2] { akEventId.id, gameObject });
		}
		else
		{
			EmitSoundServer(networkSoundEventIndex, gameObject);
		}
	}

	public static void EmitSoundServer(NetworkSoundEventIndex networkSoundEventIndex, GameObject gameObject)
	{
		NetworkIdentity component = gameObject.GetComponent<NetworkIdentity>();
		if (!Object.op_Implicit((Object)(object)component))
		{
			Debug.LogWarningFormat("Cannot emit sound \"{0}\" on object \"{1}\": Object has no NetworkIdentity.", new object[2]
			{
				NetworkSoundEventCatalog.GetAkIdFromNetworkSoundEventIndex(networkSoundEventIndex),
				gameObject
			});
		}
		else
		{
			EmitSoundServer(networkSoundEventIndex, component);
		}
	}

	public static void EmitSoundServer(NetworkSoundEventIndex networkSoundEventIndex, NetworkIdentity networkIdentity)
	{
		sharedMessage.networkIdentity = networkIdentity;
		sharedMessage.networkSoundEventIndex = networkSoundEventIndex;
		NetworkServer.SendByChannelToAll(messageType, (MessageBase)(object)sharedMessage, channel.intVal);
		sharedMessage.Clear();
	}
}
