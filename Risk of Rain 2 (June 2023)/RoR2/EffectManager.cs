using System.Diagnostics;
using RoR2.Audio;
using RoR2.Networking;
using UnityEngine;
using UnityEngine.Networking;

namespace RoR2;

public static class EffectManager
{
	private class EffectMessage : MessageBase
	{
		public EffectIndex effectIndex;

		public readonly EffectData effectData = new EffectData();

		public override void Serialize(NetworkWriter writer)
		{
			writer.WriteEffectIndex(effectIndex);
			writer.Write(effectData);
		}

		public override void Deserialize(NetworkReader reader)
		{
			effectIndex = reader.ReadEffectIndex();
			reader.ReadEffectData(effectData);
		}
	}

	private static readonly QosChannelIndex qosChannel = QosChannelIndex.effects;

	private static readonly EffectMessage outgoingEffectMessage = new EffectMessage();

	private static readonly EffectMessage incomingEffectMessage = new EffectMessage();

	[NetworkMessageHandler(msgType = 52, server = true)]
	private static void HandleEffectServer(NetworkMessage netMsg)
	{
		HandleEffectServerInternal(netMsg);
	}

	[NetworkMessageHandler(msgType = 52, client = true)]
	private static void HandleEffectClient(NetworkMessage netMsg)
	{
		HandleEffectClientInternal(netMsg);
	}

	public static void SpawnEffect(GameObject effectPrefab, EffectData effectData, bool transmit)
	{
		EffectIndex effectIndex = EffectCatalog.FindEffectIndexFromPrefab(effectPrefab);
		if (effectIndex == EffectIndex.Invalid)
		{
			if (Object.op_Implicit((Object)(object)effectPrefab) && !string.IsNullOrEmpty(((Object)effectPrefab).name))
			{
				Debug.LogError((object)("Unable to SpawnEffect from prefab named '" + ((effectPrefab != null) ? ((Object)effectPrefab).name : null) + "'"));
			}
			else
			{
				Debug.LogError((object)$"Unable to SpawnEffect.  Is null? {(Object)(object)effectPrefab == (Object)null}.  Name = '{((effectPrefab != null) ? ((Object)effectPrefab).name : null)}'.\n{new StackTrace()}");
			}
		}
		else
		{
			SpawnEffect(effectIndex, effectData, transmit);
		}
	}

	public static void SpawnEffect(EffectIndex effectIndex, EffectData effectData, bool transmit)
	{
		//IL_0037: Unknown result type (might be due to invalid IL or missing references)
		//IL_0063: Unknown result type (might be due to invalid IL or missing references)
		//IL_0096: Unknown result type (might be due to invalid IL or missing references)
		//IL_00d4: Unknown result type (might be due to invalid IL or missing references)
		//IL_00da: Unknown result type (might be due to invalid IL or missing references)
		if (transmit)
		{
			TransmitEffect(effectIndex, effectData);
			if (NetworkServer.active)
			{
				return;
			}
		}
		if (!NetworkClient.active)
		{
			return;
		}
		if (effectData.networkSoundEventIndex != NetworkSoundEventIndex.Invalid)
		{
			PointSoundManager.EmitSoundLocal(NetworkSoundEventCatalog.GetAkIdFromNetworkSoundEventIndex(effectData.networkSoundEventIndex), effectData.origin);
		}
		EffectDef effectDef = EffectCatalog.GetEffectDef(effectIndex);
		if (effectDef == null)
		{
			return;
		}
		string spawnSoundEventName = effectDef.spawnSoundEventName;
		if (!string.IsNullOrEmpty(spawnSoundEventName))
		{
			PointSoundManager.EmitSoundLocal((AkEventIdArg)spawnSoundEventName, effectData.origin);
		}
		SurfaceDef surfaceDef = SurfaceDefCatalog.GetSurfaceDef(effectData.surfaceDefIndex);
		if (surfaceDef != null)
		{
			string impactSoundString = surfaceDef.impactSoundString;
			if (!string.IsNullOrEmpty(impactSoundString))
			{
				PointSoundManager.EmitSoundLocal((AkEventIdArg)impactSoundString, effectData.origin);
			}
		}
		if (VFXBudget.CanAffordSpawn(effectDef.prefabVfxAttributes) && (effectDef.cullMethod == null || effectDef.cullMethod(effectData)))
		{
			EffectData effectData2 = effectData.Clone();
			EffectComponent component = Object.Instantiate<GameObject>(effectDef.prefab, effectData2.origin, effectData2.rotation).GetComponent<EffectComponent>();
			if (Object.op_Implicit((Object)(object)component))
			{
				component.effectData = effectData2.Clone();
			}
		}
	}

	private static void TransmitEffect(EffectIndex effectIndex, EffectData effectData, NetworkConnection netOrigin = null)
	{
		outgoingEffectMessage.effectIndex = effectIndex;
		EffectData.Copy(effectData, outgoingEffectMessage.effectData);
		if (NetworkServer.active)
		{
			foreach (NetworkConnection connection in NetworkServer.connections)
			{
				if (connection != null && connection != netOrigin)
				{
					connection.SendByChannel((short)52, (MessageBase)(object)outgoingEffectMessage, qosChannel.intVal);
				}
			}
			return;
		}
		if (ClientScene.readyConnection != null)
		{
			ClientScene.readyConnection.SendByChannel((short)52, (MessageBase)(object)outgoingEffectMessage, qosChannel.intVal);
		}
	}

	private static void HandleEffectClientInternal(NetworkMessage netMsg)
	{
		netMsg.ReadMessage<EffectMessage>(incomingEffectMessage);
		SpawnEffect(incomingEffectMessage.effectIndex, incomingEffectMessage.effectData, transmit: false);
	}

	private static void HandleEffectServerInternal(NetworkMessage netMsg)
	{
		netMsg.ReadMessage<EffectMessage>(incomingEffectMessage);
		TransmitEffect(incomingEffectMessage.effectIndex, incomingEffectMessage.effectData, netMsg.conn);
	}

	public static void SimpleMuzzleFlash(GameObject effectPrefab, GameObject obj, string muzzleName, bool transmit)
	{
		//IL_0058: Unknown result type (might be due to invalid IL or missing references)
		if (!Object.op_Implicit((Object)(object)obj))
		{
			return;
		}
		ModelLocator component = obj.GetComponent<ModelLocator>();
		if (!Object.op_Implicit((Object)(object)component) || !Object.op_Implicit((Object)(object)component.modelTransform))
		{
			return;
		}
		ChildLocator component2 = ((Component)component.modelTransform).GetComponent<ChildLocator>();
		if (Object.op_Implicit((Object)(object)component2))
		{
			int childIndex = component2.FindChildIndex(muzzleName);
			Transform val = component2.FindChild(childIndex);
			if (Object.op_Implicit((Object)(object)val))
			{
				EffectData effectData = new EffectData
				{
					origin = val.position
				};
				effectData.SetChildLocatorTransformReference(obj, childIndex);
				SpawnEffect(effectPrefab, effectData, transmit);
			}
		}
	}

	public static void SimpleImpactEffect(GameObject effectPrefab, Vector3 hitPos, Vector3 normal, bool transmit)
	{
		//IL_0007: Unknown result type (might be due to invalid IL or missing references)
		//IL_000e: Unknown result type (might be due to invalid IL or missing references)
		//IL_000f: Unknown result type (might be due to invalid IL or missing references)
		//IL_001b: Unknown result type (might be due to invalid IL or missing references)
		//IL_001c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0023: Unknown result type (might be due to invalid IL or missing references)
		//IL_0028: Unknown result type (might be due to invalid IL or missing references)
		SpawnEffect(effectPrefab, new EffectData
		{
			origin = hitPos,
			rotation = ((normal == Vector3.zero) ? Quaternion.identity : Util.QuaternionSafeLookRotation(normal))
		}, transmit);
	}

	public static void SimpleImpactEffect(GameObject effectPrefab, Vector3 hitPos, Vector3 normal, Color color, bool transmit)
	{
		//IL_0007: Unknown result type (might be due to invalid IL or missing references)
		//IL_000e: Unknown result type (might be due to invalid IL or missing references)
		//IL_000f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0014: Unknown result type (might be due to invalid IL or missing references)
		//IL_001a: Unknown result type (might be due to invalid IL or missing references)
		//IL_001b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0020: Unknown result type (might be due to invalid IL or missing references)
		SpawnEffect(effectPrefab, new EffectData
		{
			origin = hitPos,
			rotation = Util.QuaternionSafeLookRotation(normal),
			color = Color32.op_Implicit(color)
		}, transmit);
	}

	public static void SimpleEffect(GameObject effectPrefab, Vector3 position, Quaternion rotation, bool transmit)
	{
		//IL_0007: Unknown result type (might be due to invalid IL or missing references)
		//IL_000e: Unknown result type (might be due to invalid IL or missing references)
		//IL_000f: Unknown result type (might be due to invalid IL or missing references)
		SpawnEffect(effectPrefab, new EffectData
		{
			origin = position,
			rotation = rotation
		}, transmit);
	}

	public static void SimpleSoundEffect(NetworkSoundEventIndex soundEventIndex, Vector3 position, bool transmit)
	{
		//IL_0007: Unknown result type (might be due to invalid IL or missing references)
		SpawnEffect(EffectIndex.Invalid, new EffectData
		{
			origin = position,
			networkSoundEventIndex = soundEventIndex
		}, transmit);
	}
}
