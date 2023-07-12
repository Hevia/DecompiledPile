using UnityEngine;

namespace RoR2.UI;

public class NotificationUIController : MonoBehaviour
{
	[SerializeField]
	private HUD hud;

	[SerializeField]
	private GameObject genericNotificationPrefab;

	[SerializeField]
	private GameObject genericTransformationNotificationPrefab;

	[SerializeField]
	private GameObject contagiousVoidTransformationNotificationPrefab;

	[SerializeField]
	private GameObject suppressedTransformationNotificationPrefab;

	[SerializeField]
	private GameObject cloverVoidTransformationNotificationPrefab;

	[SerializeField]
	private GameObject lunarSunTransformationNotificationPrefab;

	[SerializeField]
	private GameObject regeneratingScrapRegenTransformationNotificationPrefab;

	private GenericNotification currentNotification;

	private CharacterMasterNotificationQueue notificationQueue;

	private CharacterMaster targetMaster;

	public void OnEnable()
	{
		CharacterMaster.onCharacterMasterLost += OnCharacterMasterLost;
	}

	public void OnDisable()
	{
		CharacterMaster.onCharacterMasterLost -= OnCharacterMasterLost;
		CleanUpCurrentMaster();
	}

	public void Update()
	{
		if ((Object)(object)hud.targetMaster != (Object)(object)targetMaster)
		{
			SetTargetMaster(hud.targetMaster);
		}
		if (Object.op_Implicit((Object)(object)currentNotification) && Object.op_Implicit((Object)(object)notificationQueue))
		{
			currentNotification.SetNotificationT(notificationQueue.GetCurrentNotificationT());
		}
	}

	private void SetUpNotification(CharacterMasterNotificationQueue.NotificationInfo notificationInfo)
	{
		object previousData;
		if (notificationInfo.transformation != null)
		{
			GameObject val = genericTransformationNotificationPrefab;
			switch (notificationInfo.transformation.transformationType)
			{
			case CharacterMasterNotificationQueue.TransformationType.CloverVoid:
				if (Object.op_Implicit((Object)(object)cloverVoidTransformationNotificationPrefab))
				{
					val = cloverVoidTransformationNotificationPrefab;
				}
				break;
			case CharacterMasterNotificationQueue.TransformationType.ContagiousVoid:
				if (Object.op_Implicit((Object)(object)contagiousVoidTransformationNotificationPrefab))
				{
					val = contagiousVoidTransformationNotificationPrefab;
				}
				break;
			case CharacterMasterNotificationQueue.TransformationType.Suppressed:
				if (Object.op_Implicit((Object)(object)suppressedTransformationNotificationPrefab))
				{
					val = suppressedTransformationNotificationPrefab;
				}
				break;
			case CharacterMasterNotificationQueue.TransformationType.LunarSun:
				if (Object.op_Implicit((Object)(object)lunarSunTransformationNotificationPrefab))
				{
					val = lunarSunTransformationNotificationPrefab;
				}
				break;
			case CharacterMasterNotificationQueue.TransformationType.RegeneratingScrapRegen:
				if (Object.op_Implicit((Object)(object)regeneratingScrapRegenTransformationNotificationPrefab))
				{
					val = regeneratingScrapRegenTransformationNotificationPrefab;
				}
				break;
			}
			currentNotification = Object.Instantiate<GameObject>(val).GetComponent<GenericNotification>();
			previousData = notificationInfo.transformation.previousData;
			if (previousData != null)
			{
				if (!(previousData is ItemDef itemDef))
				{
					if (previousData is EquipmentDef equipmentDef)
					{
						EquipmentDef previousEquipment = equipmentDef;
						currentNotification.SetPreviousEquipment(previousEquipment);
					}
				}
				else
				{
					ItemDef previousItem = itemDef;
					currentNotification.SetPreviousItem(previousItem);
				}
			}
		}
		else
		{
			currentNotification = Object.Instantiate<GameObject>(genericNotificationPrefab).GetComponent<GenericNotification>();
		}
		previousData = notificationInfo.data;
		if (previousData != null)
		{
			if (!(previousData is ItemDef itemDef2))
			{
				if (!(previousData is EquipmentDef equipmentDef2))
				{
					if (previousData is ArtifactDef artifactDef)
					{
						ArtifactDef artifact = artifactDef;
						currentNotification.SetArtifact(artifact);
					}
				}
				else
				{
					EquipmentDef equipment = equipmentDef2;
					currentNotification.SetEquipment(equipment);
				}
			}
			else
			{
				ItemDef item = itemDef2;
				currentNotification.SetItem(item);
			}
		}
		((Transform)((Component)currentNotification).GetComponent<RectTransform>()).SetParent((Transform)(object)((Component)this).GetComponent<RectTransform>(), false);
	}

	private void OnCurrentNotificationChanged(CharacterMasterNotificationQueue notificationQueue)
	{
		ShowCurrentNotification(notificationQueue);
	}

	private void ShowCurrentNotification(CharacterMasterNotificationQueue notificationQueue)
	{
		DestroyCurrentNotification();
		CharacterMasterNotificationQueue.NotificationInfo notificationInfo = notificationQueue.GetCurrentNotification();
		if (notificationInfo != null)
		{
			SetUpNotification(notificationInfo);
		}
	}

	private void DestroyCurrentNotification()
	{
		if (Object.op_Implicit((Object)(object)currentNotification))
		{
			Object.Destroy((Object)(object)((Component)currentNotification).gameObject);
			currentNotification = null;
		}
	}

	private void SetTargetMaster(CharacterMaster newMaster)
	{
		DestroyCurrentNotification();
		CleanUpCurrentMaster();
		targetMaster = newMaster;
		if (Object.op_Implicit((Object)(object)newMaster))
		{
			notificationQueue = CharacterMasterNotificationQueue.GetNotificationQueueForMaster(newMaster);
			notificationQueue.onCurrentNotificationChanged += OnCurrentNotificationChanged;
			ShowCurrentNotification(notificationQueue);
		}
	}

	private void OnCharacterMasterLost(CharacterMaster master)
	{
		if (master == targetMaster)
		{
			CleanUpCurrentMaster();
		}
	}

	private void CleanUpCurrentMaster()
	{
		if (Object.op_Implicit((Object)(object)notificationQueue))
		{
			notificationQueue.onCurrentNotificationChanged -= OnCurrentNotificationChanged;
		}
		notificationQueue = null;
		targetMaster = null;
	}
}
