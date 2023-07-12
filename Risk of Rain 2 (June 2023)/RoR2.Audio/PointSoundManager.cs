using System.Collections.Generic;
using RoR2.ConVar;
using RoR2.Networking;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.SceneManagement;

namespace RoR2.Audio;

public static class PointSoundManager
{
	private class NetworkSoundEventMessage : MessageBase
	{
		public NetworkSoundEventIndex soundEventIndex;

		public Vector3 position;

		public override void Serialize(NetworkWriter writer)
		{
			//IL_0015: Unknown result type (might be due to invalid IL or missing references)
			((MessageBase)this).Serialize(writer);
			writer.WriteNetworkSoundEventIndex(soundEventIndex);
			writer.Write(position);
		}

		public override void Deserialize(NetworkReader reader)
		{
			//IL_0015: Unknown result type (might be due to invalid IL or missing references)
			//IL_001a: Unknown result type (might be due to invalid IL or missing references)
			((MessageBase)this).Deserialize(reader);
			soundEventIndex = reader.ReadNetworkSoundEventIndex();
			position = reader.ReadVector3();
		}
	}

	private struct TimeoutInfo
	{
		public readonly AkGameObj emitter;

		public readonly float startTime;

		public TimeoutInfo(AkGameObj emitter, float startTime)
		{
			this.emitter = emitter;
			this.startTime = startTime;
		}
	}

	private static readonly Stack<AkGameObj> emitterPool = new Stack<AkGameObj>();

	private const uint EMPTY_AK_EVENT_SOUND = 2166136261u;

	private static readonly NetworkSoundEventMessage sharedMessage = new NetworkSoundEventMessage();

	private static readonly QosChannelIndex channel = QosChannelIndex.effects;

	private const short messageType = 72;

	private static readonly FloatConVar cvPointSoundManagerTimeout = new FloatConVar("pointsoundmanager_timeout", ConVarFlags.None, "3", "Timeout value in seconds to use for sound emitters dispatched by PointSoundManager. -1 for end-of-playback callbacks instead, which we suspect may have thread-safety issues.");

	private static readonly Queue<TimeoutInfo> timeouts = new Queue<TimeoutInfo>();

	private static AkGameObj RequestEmitter()
	{
		//IL_001d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0022: Unknown result type (might be due to invalid IL or missing references)
		if (emitterPool.Count > 0)
		{
			return emitterPool.Pop();
		}
		GameObject val = new GameObject("SoundEmitter");
		val.AddComponent<Rigidbody>().isKinematic = true;
		return val.AddComponent<AkGameObj>();
	}

	private static void FreeEmitter(AkGameObj emitter)
	{
		if (Object.op_Implicit((Object)(object)emitter))
		{
			emitterPool.Push(emitter);
		}
	}

	public static uint EmitSoundLocal(AkEventIdArg akEventId, Vector3 position)
	{
		//IL_002a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0055: Unknown result type (might be due to invalid IL or missing references)
		//IL_0060: Expected O, but got Unknown
		if (WwiseIntegrationManager.noAudio || akEventId.id == 0 || akEventId.id == 2166136261u)
		{
			return 0u;
		}
		AkGameObj val = RequestEmitter();
		((Component)val).transform.position = position;
		uint result;
		if (cvPointSoundManagerTimeout.value < 0f)
		{
			result = AkSoundEngine.PostEvent((uint)akEventId, ((Component)val).gameObject, 1u, new EventCallback(Callback), (object)val);
		}
		else
		{
			result = AkSoundEngine.PostEvent((uint)akEventId, ((Component)val).gameObject);
			timeouts.Enqueue(new TimeoutInfo(val, Time.unscaledTime));
		}
		return result;
	}

	private static void Callback(object cookie, AkCallbackType in_type, AkCallbackInfo in_info)
	{
		//IL_0000: Unknown result type (might be due to invalid IL or missing references)
		//IL_0002: Invalid comparison between Unknown and I4
		//IL_0005: Unknown result type (might be due to invalid IL or missing references)
		//IL_000f: Expected O, but got Unknown
		if ((int)in_type == 1)
		{
			FreeEmitter((AkGameObj)cookie);
		}
	}

	[RuntimeInitializeOnLoadMethod]
	private static void Init()
	{
		SceneManager.sceneUnloaded += OnSceneUnloaded;
		RoR2Application.onUpdate += StaticUpdate;
	}

	private static void StaticUpdate()
	{
		ProcessTimeoutQueue();
	}

	private static void OnSceneUnloaded(Scene scene)
	{
		ClearEmitterPool();
	}

	private static void ClearEmitterPool()
	{
		foreach (AkGameObj item in emitterPool)
		{
			if (Object.op_Implicit((Object)(object)item))
			{
				Object.Destroy((Object)(object)((Component)item).gameObject);
			}
		}
		emitterPool.Clear();
	}

	public static void EmitSoundServer(NetworkSoundEventIndex networkSoundEventIndex, Vector3 position)
	{
		//IL_0010: Unknown result type (might be due to invalid IL or missing references)
		//IL_0011: Unknown result type (might be due to invalid IL or missing references)
		sharedMessage.soundEventIndex = networkSoundEventIndex;
		sharedMessage.position = position;
		NetworkServer.SendByChannelToAll((short)72, (MessageBase)(object)sharedMessage, channel.intVal);
	}

	[NetworkMessageHandler(client = true, server = false, msgType = 72)]
	private static void HandleMessage(NetworkMessage netMsg)
	{
		//IL_0024: Unknown result type (might be due to invalid IL or missing references)
		netMsg.ReadMessage<NetworkSoundEventMessage>(sharedMessage);
		EmitSoundLocal(NetworkSoundEventCatalog.GetAkIdFromNetworkSoundEventIndex(sharedMessage.soundEventIndex), sharedMessage.position);
	}

	private static void ProcessTimeoutQueue()
	{
		float num = Time.unscaledTime - cvPointSoundManagerTimeout.value;
		while (timeouts.Count > 0)
		{
			TimeoutInfo timeoutInfo = timeouts.Peek();
			if (!(num <= timeoutInfo.startTime))
			{
				timeouts.Dequeue();
				FreeEmitter(timeoutInfo.emitter);
				continue;
			}
			break;
		}
	}
}
