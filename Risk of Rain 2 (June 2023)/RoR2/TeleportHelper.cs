using KinematicCharacterController;
using RoR2.Navigation;
using RoR2.Networking;
using UnityEngine;
using UnityEngine.Networking;

namespace RoR2;

public static class TeleportHelper
{
	private class TeleportMessage : MessageBase
	{
		public GameObject gameObject;

		public Vector3 newPosition;

		public Vector3 delta;

		public override void Serialize(NetworkWriter writer)
		{
			//IL_000e: Unknown result type (might be due to invalid IL or missing references)
			//IL_001a: Unknown result type (might be due to invalid IL or missing references)
			writer.Write(gameObject);
			writer.Write(newPosition);
			writer.Write(delta);
		}

		public override void Deserialize(NetworkReader reader)
		{
			//IL_000e: Unknown result type (might be due to invalid IL or missing references)
			//IL_0013: Unknown result type (might be due to invalid IL or missing references)
			//IL_001a: Unknown result type (might be due to invalid IL or missing references)
			//IL_001f: Unknown result type (might be due to invalid IL or missing references)
			gameObject = reader.ReadGameObject();
			newPosition = reader.ReadVector3();
			delta = reader.ReadVector3();
		}
	}

	private static readonly TeleportMessage messageBuffer = new TeleportMessage();

	public static void TeleportGameObject(GameObject gameObject, Vector3 newPosition)
	{
		//IL_0008: Unknown result type (might be due to invalid IL or missing references)
		//IL_0009: Unknown result type (might be due to invalid IL or missing references)
		//IL_0010: Unknown result type (might be due to invalid IL or missing references)
		//IL_0015: Unknown result type (might be due to invalid IL or missing references)
		bool hasEffectiveAuthority = Util.HasEffectiveAuthority(gameObject);
		TeleportGameObject(gameObject, newPosition, newPosition - gameObject.transform.position, hasEffectiveAuthority);
	}

	private static void TeleportGameObject(GameObject gameObject, Vector3 newPosition, Vector3 delta, bool hasEffectiveAuthority)
	{
		//IL_0001: Unknown result type (might be due to invalid IL or missing references)
		//IL_0002: Unknown result type (might be due to invalid IL or missing references)
		//IL_001e: Unknown result type (might be due to invalid IL or missing references)
		//IL_001f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0025: Unknown result type (might be due to invalid IL or missing references)
		//IL_0026: Unknown result type (might be due to invalid IL or missing references)
		OnTeleport(gameObject, newPosition, delta);
		if (NetworkServer.active || hasEffectiveAuthority)
		{
			TeleportMessage teleportMessage = new TeleportMessage
			{
				gameObject = gameObject,
				newPosition = newPosition,
				delta = delta
			};
			QosChannelIndex defaultReliable = QosChannelIndex.defaultReliable;
			if (NetworkServer.active)
			{
				NetworkServer.SendByChannelToAll((short)68, (MessageBase)(object)teleportMessage, defaultReliable.intVal);
			}
			else
			{
				NetworkManager.singleton.client.connection.SendByChannel((short)68, (MessageBase)(object)teleportMessage, defaultReliable.intVal);
			}
		}
	}

	private static void OnTeleport(GameObject gameObject, Vector3 newPosition, Vector3 delta)
	{
		//IL_0015: Unknown result type (might be due to invalid IL or missing references)
		//IL_003c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0041: Unknown result type (might be due to invalid IL or missing references)
		//IL_004e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0062: Unknown result type (might be due to invalid IL or missing references)
		//IL_0063: Unknown result type (might be due to invalid IL or missing references)
		//IL_0064: Unknown result type (might be due to invalid IL or missing references)
		//IL_0069: Unknown result type (might be due to invalid IL or missing references)
		CharacterMotor component = gameObject.GetComponent<CharacterMotor>();
		if (Object.op_Implicit((Object)(object)component))
		{
			((BaseCharacterController)component).Motor.SetPosition(newPosition, true);
			component.velocity = new Vector3(0f, Mathf.Min(component.velocity.y, 0f), 0f);
		}
		else
		{
			gameObject.transform.position = newPosition;
		}
		ITeleportHandler[] componentsInChildren = gameObject.GetComponentsInChildren<ITeleportHandler>();
		for (int i = 0; i < componentsInChildren.Length; i++)
		{
			componentsInChildren[i].OnTeleport(newPosition - delta, newPosition);
		}
	}

	public static void TeleportBody(CharacterBody body, Vector3 targetFootPosition)
	{
		//IL_001f: Unknown result type (might be due to invalid IL or missing references)
		//IL_002a: Unknown result type (might be due to invalid IL or missing references)
		//IL_002f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0034: Unknown result type (might be due to invalid IL or missing references)
		//IL_003b: Unknown result type (might be due to invalid IL or missing references)
		//IL_003c: Unknown result type (might be due to invalid IL or missing references)
		//IL_003d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0042: Unknown result type (might be due to invalid IL or missing references)
		//IL_0043: Unknown result type (might be due to invalid IL or missing references)
		if (Object.op_Implicit((Object)(object)body))
		{
			Vector3 val = default(Vector3);
			((Vector3)(ref val))._002Ector(0f, 0.1f, 0f);
			Vector3 val2 = body.footPosition - ((Component)body).transform.position;
			TeleportGameObject(((Component)body).gameObject, targetFootPosition - val2 + val);
			if (Object.op_Implicit((Object)(object)body.characterMotor))
			{
				body.characterMotor.disableAirControlUntilCollision = false;
			}
		}
	}

	[NetworkMessageHandler(client = true, server = true, msgType = 68)]
	private static void HandleTeleport(NetworkMessage netMsg)
	{
		//IL_004e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0058: Unknown result type (might be due to invalid IL or missing references)
		if (Util.ConnectionIsLocal(netMsg.conn))
		{
			return;
		}
		netMsg.ReadMessage<TeleportMessage>(messageBuffer);
		if (Object.op_Implicit((Object)(object)messageBuffer.gameObject))
		{
			bool flag = Util.HasEffectiveAuthority(messageBuffer.gameObject);
			if (!flag)
			{
				TeleportGameObject(messageBuffer.gameObject, messageBuffer.newPosition, messageBuffer.delta, flag);
			}
		}
	}

	public static Vector3? FindSafeTeleportDestination(Vector3 characterFootPosition, CharacterBody characterBodyOrPrefabComponent, Xoroshiro128Plus rng)
	{
		//IL_0044: Unknown result type (might be due to invalid IL or missing references)
		//IL_0045: Unknown result type (might be due to invalid IL or missing references)
		//IL_0071: Unknown result type (might be due to invalid IL or missing references)
		//IL_0072: Unknown result type (might be due to invalid IL or missing references)
		//IL_0093: Unknown result type (might be due to invalid IL or missing references)
		Vector3? result = null;
		SpawnCard spawnCard = ScriptableObject.CreateInstance<SpawnCard>();
		spawnCard.hullSize = characterBodyOrPrefabComponent.hullClassification;
		spawnCard.nodeGraphType = MapNodeGroup.GraphType.Ground;
		spawnCard.prefab = LegacyResourcesAPI.Load<GameObject>("SpawnCards/HelperPrefab");
		GameObject val = DirectorCore.instance.TrySpawnObject(new DirectorSpawnRequest(spawnCard, new DirectorPlacementRule
		{
			placementMode = DirectorPlacementRule.PlacementMode.NearestNode,
			position = characterFootPosition
		}, rng));
		if (!Object.op_Implicit((Object)(object)val))
		{
			val = DirectorCore.instance.TrySpawnObject(new DirectorSpawnRequest(spawnCard, new DirectorPlacementRule
			{
				placementMode = DirectorPlacementRule.PlacementMode.RandomNormalized,
				position = characterFootPosition
			}, rng));
		}
		if (Object.op_Implicit((Object)(object)val))
		{
			result = val.transform.position;
			Object.Destroy((Object)(object)val);
		}
		Object.Destroy((Object)(object)spawnCard);
		return result;
	}
}
