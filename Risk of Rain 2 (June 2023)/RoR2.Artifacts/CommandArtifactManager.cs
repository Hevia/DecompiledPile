using System;
using UnityEngine;
using UnityEngine.Networking;

namespace RoR2.Artifacts;

public static class CommandArtifactManager
{
	private static GameObject commandCubePrefab;

	private static ArtifactDef myArtifact => RoR2Content.Artifacts.commandArtifactDef;

	[SystemInitializer(new Type[] { typeof(ArtifactCatalog) })]
	private static void Init()
	{
		commandCubePrefab = LegacyResourcesAPI.Load<GameObject>("Prefabs/NetworkedObjects/CommandCube");
		RunArtifactManager.onArtifactEnabledGlobal += OnArtifactEnabled;
		RunArtifactManager.onArtifactDisabledGlobal += OnArtifactDisabled;
	}

	private static void OnArtifactEnabled(RunArtifactManager runArtifactManager, ArtifactDef artifactDef)
	{
		if (!((Object)(object)artifactDef != (Object)(object)myArtifact) && NetworkServer.active)
		{
			PickupDropletController.onDropletHitGroundServer += OnDropletHitGroundServer;
			SceneDirector.onGenerateInteractableCardSelection += OnGenerateInteractableCardSelection;
		}
	}

	private static void OnArtifactDisabled(RunArtifactManager runArtifactManager, ArtifactDef artifactDef)
	{
		if (!((Object)(object)artifactDef != (Object)(object)myArtifact))
		{
			SceneDirector.onGenerateInteractableCardSelection -= OnGenerateInteractableCardSelection;
			PickupDropletController.onDropletHitGroundServer -= OnDropletHitGroundServer;
		}
	}

	private static void OnDropletHitGroundServer(ref GenericPickupController.CreatePickupInfo createPickupInfo, ref bool shouldSpawn)
	{
		//IL_0033: Unknown result type (might be due to invalid IL or missing references)
		//IL_0039: Unknown result type (might be due to invalid IL or missing references)
		PickupIndex pickupIndex = createPickupInfo.pickupIndex;
		PickupDef pickupDef = PickupCatalog.GetPickupDef(pickupIndex);
		if (pickupDef != null && (pickupDef.itemIndex != ItemIndex.None || pickupDef.equipmentIndex != EquipmentIndex.None || pickupDef.itemTier != ItemTier.NoTier))
		{
			GameObject obj = Object.Instantiate<GameObject>(commandCubePrefab, createPickupInfo.position, createPickupInfo.rotation);
			obj.GetComponent<PickupIndexNetworker>().NetworkpickupIndex = pickupIndex;
			obj.GetComponent<PickupPickerController>().SetOptionsFromPickupForCommandArtifact(pickupIndex);
			NetworkServer.Spawn(obj);
			shouldSpawn = false;
		}
	}

	private static void OnGenerateInteractableCardSelection(SceneDirector sceneDirector, DirectorCardCategorySelection dccs)
	{
		dccs.RemoveCardsThatFailFilter(DoesNotOfferChoice);
		static bool DoesNotOfferChoice(DirectorCard card)
		{
			return !OffersChoice(card);
		}
		static bool OffersChoice(DirectorCard card)
		{
			GameObject prefab = card.spawnCard.prefab;
			if (!Object.op_Implicit((Object)(object)prefab.GetComponent<ShopTerminalBehavior>()) && !Object.op_Implicit((Object)(object)prefab.GetComponent<MultiShopController>()))
			{
				return Object.op_Implicit((Object)(object)prefab.GetComponent<ScrapperController>());
			}
			return true;
		}
	}
}
