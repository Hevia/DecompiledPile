using System;
using UnityEngine;
using UnityEngine.Networking;

namespace RoR2.Artifacts;

public static class SacrificeArtifactManager
{
	private static PickupDropTable dropTable;

	private static readonly Xoroshiro128Plus treasureRng = new Xoroshiro128Plus(0uL);

	private static ArtifactDef myArtifact => RoR2Content.Artifacts.sacrificeArtifactDef;

	[SystemInitializer(new Type[] { typeof(ArtifactCatalog) })]
	private static void Init()
	{
		dropTable = LegacyResourcesAPI.Load<PickupDropTable>("DropTables/dtSacrificeArtifact");
		RunArtifactManager.onArtifactEnabledGlobal += OnArtifactEnabled;
		RunArtifactManager.onArtifactDisabledGlobal += OnArtifactDisabled;
		Stage.onServerStageBegin += OnServerStageBegin;
	}

	private static void OnServerStageBegin(Stage stage)
	{
		treasureRng.ResetSeed(Run.instance.treasureRng.nextUlong);
	}

	private static void OnArtifactEnabled(RunArtifactManager runArtifactManager, ArtifactDef artifactDef)
	{
		if (NetworkServer.active && !((Object)(object)artifactDef != (Object)(object)myArtifact))
		{
			GlobalEventManager.onCharacterDeathGlobal += OnServerCharacterDeath;
			SceneDirector.onPrePopulateSceneServer += OnPrePopulateSceneServer;
			SceneDirector.onGenerateInteractableCardSelection += OnGenerateInteractableCardSelection;
			DirectorCardCategorySelection.calcCardWeight += CalcCardWeight;
		}
	}

	private static void OnArtifactDisabled(RunArtifactManager runArtifactManager, ArtifactDef artifactDef)
	{
		if (!((Object)(object)artifactDef != (Object)(object)myArtifact))
		{
			SceneDirector.onGenerateInteractableCardSelection -= OnGenerateInteractableCardSelection;
			SceneDirector.onPrePopulateSceneServer -= OnPrePopulateSceneServer;
			GlobalEventManager.onCharacterDeathGlobal -= OnServerCharacterDeath;
			DirectorCardCategorySelection.calcCardWeight -= CalcCardWeight;
		}
	}

	private static void CalcCardWeight(DirectorCard card, ref float weight)
	{
		InteractableSpawnCard interactableSpawnCard = card.spawnCard as InteractableSpawnCard;
		if ((Object)(object)interactableSpawnCard != (Object)null)
		{
			weight *= interactableSpawnCard.weightScalarWhenSacrificeArtifactEnabled;
		}
	}

	private static void OnGenerateInteractableCardSelection(SceneDirector sceneDirector, DirectorCardCategorySelection dccs)
	{
		dccs.RemoveCardsThatFailFilter(IsNotChest);
		static bool IsNotChest(DirectorCard card)
		{
			InteractableSpawnCard obj = card.spawnCard as InteractableSpawnCard;
			if (obj == null)
			{
				return true;
			}
			return !obj.skipSpawnWhenSacrificeArtifactEnabled;
		}
	}

	private static void OnPrePopulateSceneServer(SceneDirector sceneDirector)
	{
		sceneDirector.onPopulateCreditMultiplier *= 0.5f;
	}

	private static void OnServerCharacterDeath(DamageReport damageReport)
	{
		//IL_009e: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a3: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ad: Unknown result type (might be due to invalid IL or missing references)
		if (!Object.op_Implicit((Object)(object)damageReport.victimMaster) || (damageReport.attackerTeamIndex == damageReport.victimTeamIndex && Object.op_Implicit((Object)(object)damageReport.victimMaster.minionOwnership.ownerMaster)))
		{
			return;
		}
		float expAdjustedDropChancePercent = Util.GetExpAdjustedDropChancePercent(5f, ((Component)damageReport.victim).gameObject);
		Debug.LogFormat("Drop chance from {0}: {1}", new object[2] { damageReport.victimBody, expAdjustedDropChancePercent });
		if (Util.CheckRoll(expAdjustedDropChancePercent))
		{
			PickupIndex pickupIndex = dropTable.GenerateDrop(treasureRng);
			if (pickupIndex != PickupIndex.none)
			{
				PickupDropletController.CreatePickupDroplet(pickupIndex, damageReport.victimBody.corePosition, Vector3.up * 20f);
			}
		}
	}
}
