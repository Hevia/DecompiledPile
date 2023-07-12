using System;
using System.Collections.Generic;
using RoR2.Networking;
using Unity;
using UnityEngine;
using UnityEngine.Networking;

namespace RoR2;

public class CharacterMasterNotificationQueue : MonoBehaviour
{
	public enum TransformationType
	{
		Default,
		ContagiousVoid,
		CloverVoid,
		Suppressed,
		LunarSun,
		RegeneratingScrapRegen
	}

	public class TransformationInfo
	{
		public readonly TransformationType transformationType;

		public readonly object previousData;

		public static bool operator ==(TransformationInfo lhs, TransformationInfo rhs)
		{
			bool flag = (object)lhs == null;
			bool flag2 = (object)rhs == null;
			if (!(flag && flag2))
			{
				if (!flag && !flag2 && lhs.previousData == rhs.previousData)
				{
					return lhs.transformationType == rhs.transformationType;
				}
				return false;
			}
			return true;
		}

		public static bool operator !=(TransformationInfo lhs, TransformationInfo rhs)
		{
			return !(lhs == rhs);
		}

		public TransformationInfo(TransformationType transformationType, object previousData)
		{
			this.transformationType = transformationType;
			this.previousData = previousData;
		}
	}

	public class NotificationInfo
	{
		public readonly object data;

		public readonly TransformationInfo transformation;

		public static bool operator ==(NotificationInfo lhs, NotificationInfo rhs)
		{
			bool flag = (object)lhs == null;
			bool flag2 = (object)rhs == null;
			if (!(flag && flag2))
			{
				if (!flag && !flag2 && lhs.data == rhs.data)
				{
					return lhs.transformation == rhs.transformation;
				}
				return false;
			}
			return true;
		}

		public static bool operator !=(NotificationInfo lhs, NotificationInfo rhs)
		{
			return !(lhs == rhs);
		}

		public NotificationInfo(object data, TransformationInfo transformation = null)
		{
			this.data = data;
			this.transformation = transformation;
		}
	}

	private class TimedNotificationInfo
	{
		public NotificationInfo notification;

		public float startTime;

		public float duration;
	}

	private class TransformNotificationMessage : MessageBase
	{
		public GameObject masterGameObject;

		public PickupIndex oldIndex;

		public PickupIndex newIndex;

		public TransformationType transformationType;

		public override void Serialize(NetworkWriter writer)
		{
			writer.Write(masterGameObject);
			GeneratedNetworkCode._WritePickupIndex_None(writer, oldIndex);
			GeneratedNetworkCode._WritePickupIndex_None(writer, newIndex);
			writer.Write((int)transformationType);
		}

		public override void Deserialize(NetworkReader reader)
		{
			masterGameObject = reader.ReadGameObject();
			oldIndex = GeneratedNetworkCode._ReadPickupIndex_None(reader);
			newIndex = GeneratedNetworkCode._ReadPickupIndex_None(reader);
			transformationType = (TransformationType)reader.ReadInt32();
		}
	}

	public const float firstNotificationLengthSeconds = 6f;

	public const float repeatNotificationLengthSeconds = 6f;

	private static readonly TransformNotificationMessage transformNotificationMessageInstance = new TransformNotificationMessage();

	private CharacterMaster master;

	private List<TimedNotificationInfo> notifications = new List<TimedNotificationInfo>();

	public event Action<CharacterMasterNotificationQueue> onCurrentNotificationChanged;

	public static void PushPickupNotification(CharacterMaster characterMaster, PickupIndex pickupIndex)
	{
		if (!((NetworkBehaviour)characterMaster).hasAuthority)
		{
			Debug.LogError((object)("Can't PushPickupNotification for " + Util.GetBestMasterName(characterMaster) + " because they aren't local."));
			return;
		}
		PickupDef pickupDef = PickupCatalog.GetPickupDef(pickupIndex);
		ItemIndex itemIndex = pickupDef.itemIndex;
		if (itemIndex != ItemIndex.None)
		{
			PushItemNotification(characterMaster, itemIndex);
			return;
		}
		EquipmentIndex equipmentIndex = pickupDef.equipmentIndex;
		if (equipmentIndex != EquipmentIndex.None)
		{
			PushEquipmentNotification(characterMaster, equipmentIndex);
		}
	}

	public static void PushItemNotification(CharacterMaster characterMaster, ItemIndex itemIndex)
	{
		if (!((NetworkBehaviour)characterMaster).hasAuthority)
		{
			Debug.LogError((object)("Can't PushItemNotification for " + Util.GetBestMasterName(characterMaster) + " because they aren't local."));
			return;
		}
		CharacterMasterNotificationQueue notificationQueueForMaster = GetNotificationQueueForMaster(characterMaster);
		if (!Object.op_Implicit((Object)(object)notificationQueueForMaster) || itemIndex == ItemIndex.None)
		{
			return;
		}
		ItemDef itemDef = ItemCatalog.GetItemDef(itemIndex);
		if (!((Object)(object)itemDef == (Object)null) && !itemDef.hidden)
		{
			float duration = 6f;
			if (characterMaster.inventory.GetItemCount(itemIndex) > 1)
			{
				duration = 6f;
			}
			notificationQueueForMaster.PushNotification(new NotificationInfo(ItemCatalog.GetItemDef(itemIndex)), duration);
		}
	}

	public static void PushEquipmentNotification(CharacterMaster characterMaster, EquipmentIndex equipmentIndex)
	{
		if (!((NetworkBehaviour)characterMaster).hasAuthority)
		{
			Debug.LogError((object)("Can't PushEquipmentNotification for " + Util.GetBestMasterName(characterMaster) + " because they aren't local."));
			return;
		}
		CharacterMasterNotificationQueue notificationQueueForMaster = GetNotificationQueueForMaster(characterMaster);
		if (Object.op_Implicit((Object)(object)notificationQueueForMaster) && equipmentIndex != EquipmentIndex.None)
		{
			notificationQueueForMaster.PushNotification(new NotificationInfo(EquipmentCatalog.GetEquipmentDef(equipmentIndex)), 6f);
		}
	}

	public static void PushArtifactNotification(CharacterMaster characterMaster, ArtifactDef artifactDef)
	{
		if (!((NetworkBehaviour)characterMaster).hasAuthority)
		{
			Debug.LogError((object)("Can't PushArtifactNotification for " + Util.GetBestMasterName(characterMaster) + " because they aren't local."));
			return;
		}
		CharacterMasterNotificationQueue notificationQueueForMaster = GetNotificationQueueForMaster(characterMaster);
		if (Object.op_Implicit((Object)(object)notificationQueueForMaster))
		{
			notificationQueueForMaster.PushNotification(new NotificationInfo(artifactDef), 6f);
		}
	}

	public static void PushEquipmentTransformNotification(CharacterMaster characterMaster, EquipmentIndex oldIndex, EquipmentIndex newIndex, TransformationType transformationType)
	{
		if (!((NetworkBehaviour)characterMaster).hasAuthority)
		{
			Debug.LogError((object)("Can't PushEquipmentTransformNotification for " + Util.GetBestMasterName(characterMaster) + " because they aren't local."));
			return;
		}
		CharacterMasterNotificationQueue notificationQueueForMaster = GetNotificationQueueForMaster(characterMaster);
		if (Object.op_Implicit((Object)(object)notificationQueueForMaster) && oldIndex != EquipmentIndex.None && newIndex != EquipmentIndex.None)
		{
			object equipmentDef = EquipmentCatalog.GetEquipmentDef(oldIndex);
			TransformationInfo transformation = new TransformationInfo(transformationType, equipmentDef);
			NotificationInfo info = new NotificationInfo(EquipmentCatalog.GetEquipmentDef(newIndex), transformation);
			notificationQueueForMaster.PushNotification(info, 6f);
		}
	}

	public static void PushItemTransformNotification(CharacterMaster characterMaster, ItemIndex oldIndex, ItemIndex newIndex, TransformationType transformationType)
	{
		if (!((NetworkBehaviour)characterMaster).hasAuthority)
		{
			Debug.LogError((object)("Can't PushItemTransformNotification for " + Util.GetBestMasterName(characterMaster) + " because they aren't local."));
			return;
		}
		CharacterMasterNotificationQueue notificationQueueForMaster = GetNotificationQueueForMaster(characterMaster);
		if (Object.op_Implicit((Object)(object)notificationQueueForMaster) && oldIndex != ItemIndex.None && newIndex != ItemIndex.None)
		{
			object itemDef = ItemCatalog.GetItemDef(oldIndex);
			TransformationInfo transformation = new TransformationInfo(transformationType, itemDef);
			NotificationInfo info = new NotificationInfo(ItemCatalog.GetItemDef(newIndex), transformation);
			notificationQueueForMaster.PushNotification(info, 6f);
		}
	}

	public static CharacterMasterNotificationQueue GetNotificationQueueForMaster(CharacterMaster master)
	{
		if ((Object)(object)master != (Object)null)
		{
			CharacterMasterNotificationQueue characterMasterNotificationQueue = ((Component)master).GetComponent<CharacterMasterNotificationQueue>();
			if (!Object.op_Implicit((Object)(object)characterMasterNotificationQueue))
			{
				characterMasterNotificationQueue = ((Component)master).gameObject.AddComponent<CharacterMasterNotificationQueue>();
				characterMasterNotificationQueue.master = master;
			}
			return characterMasterNotificationQueue;
		}
		return null;
	}

	public static void SendTransformNotification(CharacterMaster characterMaster, ItemIndex oldIndex, ItemIndex newIndex, TransformationType transformationType)
	{
		SendTransformNotificationInternal(characterMaster, PickupCatalog.FindPickupIndex(oldIndex), PickupCatalog.FindPickupIndex(newIndex), transformationType);
	}

	public static void SendTransformNotification(CharacterMaster characterMaster, EquipmentIndex oldIndex, EquipmentIndex newIndex, TransformationType transformationType)
	{
		SendTransformNotificationInternal(characterMaster, PickupCatalog.FindPickupIndex(oldIndex), PickupCatalog.FindPickupIndex(newIndex), transformationType);
	}

	private static void SendTransformNotificationInternal(CharacterMaster characterMaster, PickupIndex oldIndex, PickupIndex newIndex, TransformationType transformationType)
	{
		if (NetworkServer.active)
		{
			TransformNotificationMessage transformNotificationMessage = new TransformNotificationMessage
			{
				masterGameObject = ((Component)characterMaster).gameObject,
				oldIndex = oldIndex,
				newIndex = newIndex,
				transformationType = transformationType
			};
			NetworkServer.SendByChannelToAll((short)78, (MessageBase)(object)transformNotificationMessage, QosChannelIndex.chat.intVal);
		}
		else
		{
			Debug.LogError((object)"Can't SendTransformNotification if this isn't the server.");
		}
	}

	[NetworkMessageHandler(msgType = 78, client = true)]
	private static void HandleTransformNotification(NetworkMessage netMsg)
	{
		netMsg.ReadMessage<TransformNotificationMessage>(transformNotificationMessageInstance);
		if (Object.op_Implicit((Object)(object)transformNotificationMessageInstance.masterGameObject))
		{
			CharacterMaster component = transformNotificationMessageInstance.masterGameObject.GetComponent<CharacterMaster>();
			if (Object.op_Implicit((Object)(object)component) && ((NetworkBehaviour)component).hasAuthority)
			{
				PickupDef pickupDef = PickupCatalog.GetPickupDef(transformNotificationMessageInstance.oldIndex);
				PickupDef pickupDef2 = PickupCatalog.GetPickupDef(transformNotificationMessageInstance.newIndex);
				if (pickupDef != null && pickupDef2 != null)
				{
					if (pickupDef2.equipmentIndex != EquipmentIndex.None)
					{
						PushEquipmentTransformNotification(component, pickupDef.equipmentIndex, pickupDef2.equipmentIndex, transformNotificationMessageInstance.transformationType);
					}
					else if (pickupDef2.itemIndex != ItemIndex.None)
					{
						PushItemTransformNotification(component, pickupDef.itemIndex, pickupDef2.itemIndex, transformNotificationMessageInstance.transformationType);
					}
				}
				else
				{
					Debug.LogError((object)$"Can't handle transform notification for pickup indices:  {transformNotificationMessageInstance.oldIndex} -> {transformNotificationMessageInstance.newIndex}");
				}
			}
		}
		transformNotificationMessageInstance.masterGameObject = null;
	}

	public void FixedUpdate()
	{
		if (GetCurrentNotificationT() > 1f)
		{
			notifications.RemoveAt(0);
			if (notifications.Count > 0)
			{
				notifications[0].startTime = Run.instance.fixedTime;
			}
			this.onCurrentNotificationChanged?.Invoke(this);
		}
	}

	public NotificationInfo GetCurrentNotification()
	{
		if (notifications.Count > 0)
		{
			return notifications[0].notification;
		}
		return null;
	}

	private void PushNotification(NotificationInfo info, float duration)
	{
		if (notifications.Count == 0 || notifications[notifications.Count - 1].notification != info)
		{
			notifications.Add(new TimedNotificationInfo
			{
				notification = info,
				startTime = Run.instance.fixedTime,
				duration = duration
			});
			if (notifications.Count == 1)
			{
				this.onCurrentNotificationChanged?.Invoke(this);
			}
		}
	}

	public float GetCurrentNotificationT()
	{
		if (notifications.Count > 0)
		{
			TimedNotificationInfo timedNotificationInfo = notifications[0];
			return (Run.instance.fixedTime - timedNotificationInfo.startTime) / timedNotificationInfo.duration;
		}
		return 0f;
	}
}
