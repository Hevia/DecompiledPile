using System.Collections.Generic;
using System.Collections.ObjectModel;
using UnityEngine;
using UnityEngine.Networking;

namespace RoR2.Networking;

public class CharacterNetworkTransformManager : MonoBehaviour
{
	private class CharacterUpdateMessage : MessageBase
	{
		private static readonly int byteSize = 1;

		private static readonly int floatSize = 4;

		private static readonly int vector3Size = floatSize * 3;

		private static readonly int packedUint32MaxSize = 5;

		private static readonly int gameObjectSize = packedUint32MaxSize;

		private static readonly int packedUnitVector3Size = 2;

		private static readonly int quaternionSize = floatSize * 4;

		public static readonly int maxNetworkSize = byteSize + floatSize + gameObjectSize + vector3Size + packedUnitVector3Size + vector3Size + quaternionSize + packedUnitVector3Size;

		private static readonly byte nonIdentityRotationBit = 2;

		private static readonly byte isGroundedBit = 4;

		public float timestamp;

		public GameObject gameObject;

		public Vector3 newPosition;

		public Vector3 aimDirection;

		public Vector3 moveVector;

		public Quaternion rotation;

		public bool isGrounded;

		public Vector3 groundNormal;

		public override void Serialize(NetworkWriter writer)
		{
			//IL_000a: Unknown result type (might be due to invalid IL or missing references)
			//IL_000f: Unknown result type (might be due to invalid IL or missing references)
			//IL_0058: Unknown result type (might be due to invalid IL or missing references)
			//IL_0064: Unknown result type (might be due to invalid IL or missing references)
			//IL_0075: Unknown result type (might be due to invalid IL or missing references)
			//IL_0083: Unknown result type (might be due to invalid IL or missing references)
			//IL_0097: Unknown result type (might be due to invalid IL or missing references)
			((MessageBase)this).Serialize(writer);
			byte b = 0;
			bool num = rotation != Quaternion.identity;
			if (num)
			{
				b = (byte)(b | nonIdentityRotationBit);
			}
			if (isGrounded)
			{
				b = (byte)(b | isGroundedBit);
			}
			writer.Write(b);
			writer.Write(timestamp);
			writer.Write(gameObject);
			writer.Write(newPosition);
			writer.Write(new PackedUnitVector3(aimDirection));
			writer.Write(moveVector);
			if (num)
			{
				writer.Write(rotation);
			}
			if (isGrounded)
			{
				writer.Write(new PackedUnitVector3(groundNormal));
			}
		}

		public override void Deserialize(NetworkReader reader)
		{
			//IL_0042: Unknown result type (might be due to invalid IL or missing references)
			//IL_0047: Unknown result type (might be due to invalid IL or missing references)
			//IL_0056: Unknown result type (might be due to invalid IL or missing references)
			//IL_005b: Unknown result type (might be due to invalid IL or missing references)
			//IL_0062: Unknown result type (might be due to invalid IL or missing references)
			//IL_0067: Unknown result type (might be due to invalid IL or missing references)
			//IL_0070: Unknown result type (might be due to invalid IL or missing references)
			//IL_0075: Unknown result type (might be due to invalid IL or missing references)
			//IL_007d: Unknown result type (might be due to invalid IL or missing references)
			//IL_0082: Unknown result type (might be due to invalid IL or missing references)
			//IL_0099: Unknown result type (might be due to invalid IL or missing references)
			//IL_009e: Unknown result type (might be due to invalid IL or missing references)
			((MessageBase)this).Deserialize(reader);
			byte b = reader.ReadByte();
			bool num = (b & nonIdentityRotationBit) != 0;
			isGrounded = (b & isGroundedBit) != 0;
			timestamp = reader.ReadSingle();
			gameObject = reader.ReadGameObject();
			newPosition = reader.ReadVector3();
			aimDirection = reader.ReadPackedUnitVector3().Unpack();
			moveVector = reader.ReadVector3();
			if (num)
			{
				rotation = reader.ReadQuaternion();
			}
			else
			{
				rotation = Quaternion.identity;
			}
			if (isGrounded)
			{
				groundNormal = reader.ReadPackedUnitVector3().Unpack();
			}
		}
	}

	public struct NetSnapshot
	{
		public GameObject gameObject;

		public CharacterNetworkTransform.Snapshot snapshot;
	}

	private static CharacterNetworkTransformManager instance;

	private CharacterUpdateMessage currentInMessage = new CharacterUpdateMessage();

	private CharacterUpdateMessage currentOutMessage = new CharacterUpdateMessage();

	private readonly Queue<NetSnapshot> snapshotQueue = new Queue<NetSnapshot>();

	private void Awake()
	{
		instance = this;
	}

	[NetworkMessageHandler(msgType = 51, client = true, server = true)]
	private static void HandleTransformUpdates(NetworkMessage netMsg)
	{
		if (Object.op_Implicit((Object)(object)instance))
		{
			instance.HandleTransformUpdatesInternal(netMsg);
		}
	}

	private void HandleTransformUpdatesInternal(NetworkMessage netMsg)
	{
		//IL_00a0: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a5: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b2: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b7: Unknown result type (might be due to invalid IL or missing references)
		//IL_00c4: Unknown result type (might be due to invalid IL or missing references)
		//IL_00c9: Unknown result type (might be due to invalid IL or missing references)
		//IL_00d6: Unknown result type (might be due to invalid IL or missing references)
		//IL_00db: Unknown result type (might be due to invalid IL or missing references)
		//IL_00fa: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ff: Unknown result type (might be due to invalid IL or missing references)
		uint num = netMsg.reader.ReadByte();
		float filteredClientRttFixed = PlatformSystems.networkManager.filteredClientRttFixed;
		for (int i = 0; i < num; i++)
		{
			netMsg.ReadMessage<CharacterUpdateMessage>(currentInMessage);
			GameObject gameObject = currentInMessage.gameObject;
			if (!Object.op_Implicit((Object)(object)gameObject) || (NetworkServer.active && gameObject.GetComponent<NetworkIdentity>().clientAuthorityOwner != netMsg.conn))
			{
				continue;
			}
			CharacterNetworkTransform component = gameObject.GetComponent<CharacterNetworkTransform>();
			if (Object.op_Implicit((Object)(object)component) && !component.hasEffectiveAuthority)
			{
				CharacterNetworkTransform.Snapshot snapshot = default(CharacterNetworkTransform.Snapshot);
				snapshot.serverTime = currentInMessage.timestamp;
				snapshot.position = currentInMessage.newPosition;
				snapshot.moveVector = currentInMessage.moveVector;
				snapshot.aimDirection = currentInMessage.aimDirection;
				snapshot.rotation = currentInMessage.rotation;
				snapshot.isGrounded = currentInMessage.isGrounded;
				snapshot.groundNormal = currentInMessage.groundNormal;
				CharacterNetworkTransform.Snapshot snapshot2 = snapshot;
				if (NetworkClient.active)
				{
					snapshot2.serverTime += filteredClientRttFixed;
				}
				component.PushSnapshot(snapshot2);
				if (NetworkServer.active)
				{
					snapshotQueue.Enqueue(new NetSnapshot
					{
						gameObject = ((Component)component).gameObject,
						snapshot = snapshot2
					});
				}
			}
		}
	}

	private void ProcessQueue()
	{
		//IL_000e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0014: Expected O, but got Unknown
		//IL_007e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0083: Unknown result type (might be due to invalid IL or missing references)
		//IL_0094: Unknown result type (might be due to invalid IL or missing references)
		//IL_0099: Unknown result type (might be due to invalid IL or missing references)
		//IL_00aa: Unknown result type (might be due to invalid IL or missing references)
		//IL_00af: Unknown result type (might be due to invalid IL or missing references)
		//IL_00c0: Unknown result type (might be due to invalid IL or missing references)
		//IL_00c5: Unknown result type (might be due to invalid IL or missing references)
		//IL_0102: Unknown result type (might be due to invalid IL or missing references)
		//IL_0107: Unknown result type (might be due to invalid IL or missing references)
		if (snapshotQueue.Count != 0)
		{
			NetworkWriter val = new NetworkWriter();
			val.StartMessage((short)51);
			int num = Mathf.Min(Mathf.FloorToInt((float)(1000 - val.Position) / (float)CharacterUpdateMessage.maxNetworkSize), snapshotQueue.Count);
			val.Write((byte)num);
			for (int i = 0; i < num; i++)
			{
				NetSnapshot netSnapshot = snapshotQueue.Dequeue();
				currentOutMessage.gameObject = netSnapshot.gameObject;
				currentOutMessage.newPosition = netSnapshot.snapshot.position;
				currentOutMessage.aimDirection = netSnapshot.snapshot.aimDirection;
				currentOutMessage.moveVector = netSnapshot.snapshot.moveVector;
				currentOutMessage.rotation = netSnapshot.snapshot.rotation;
				currentOutMessage.isGrounded = netSnapshot.snapshot.isGrounded;
				currentOutMessage.timestamp = netSnapshot.snapshot.serverTime;
				currentOutMessage.groundNormal = netSnapshot.snapshot.groundNormal;
				val.Write((MessageBase)(object)currentOutMessage);
			}
			val.FinishMessage();
			if (NetworkServer.active)
			{
				NetworkServer.SendWriterToReady((GameObject)null, val, QosChannelIndex.characterTransformUnreliable.intVal);
			}
			else if (ClientScene.readyConnection != null)
			{
				ClientScene.readyConnection.SendWriter(val, QosChannelIndex.characterTransformUnreliable.intVal);
			}
		}
	}

	private void FixedUpdate()
	{
		if (!Object.op_Implicit((Object)(object)NetworkManager.singleton))
		{
			return;
		}
		ReadOnlyCollection<CharacterNetworkTransform> readOnlyInstancesList = CharacterNetworkTransform.readOnlyInstancesList;
		float fixedTime = Time.fixedTime;
		for (int i = 0; i < readOnlyInstancesList.Count; i++)
		{
			CharacterNetworkTransform characterNetworkTransform = readOnlyInstancesList[i];
			if (characterNetworkTransform.hasEffectiveAuthority && fixedTime - characterNetworkTransform.lastPositionTransmitTime > characterNetworkTransform.positionTransmitInterval)
			{
				characterNetworkTransform.lastPositionTransmitTime = fixedTime;
				snapshotQueue.Enqueue(new NetSnapshot
				{
					gameObject = ((Component)characterNetworkTransform).gameObject,
					snapshot = characterNetworkTransform.newestNetSnapshot
				});
			}
		}
		while (snapshotQueue.Count > 0)
		{
			ProcessQueue();
		}
	}
}
