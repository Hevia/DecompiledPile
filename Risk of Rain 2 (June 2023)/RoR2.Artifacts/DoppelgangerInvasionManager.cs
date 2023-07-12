using System;
using UnityEngine;
using UnityEngine.Networking;

namespace RoR2.Artifacts;

public class DoppelgangerInvasionManager : MonoBehaviour
{
	private readonly float invasionInterval = 600f;

	private int previousInvasionCycle;

	private ulong seed;

	private Run run;

	private Xoroshiro128Plus treasureRng;

	private PickupDropTable dropTable;

	private bool artifactIsEnabled => RunArtifactManager.instance.IsArtifactEnabled(RoR2Content.Artifacts.shadowCloneArtifactDef);

	public static event Action<DamageReport> onDoppelgangerDeath;

	[SystemInitializer(new Type[] { typeof(ArtifactCatalog) })]
	private static void Init()
	{
		Run.onRunStartGlobal += OnRunStartGlobal;
	}

	private static void OnRunStartGlobal(Run run)
	{
		if (NetworkServer.active)
		{
			((Component)run).gameObject.AddComponent<DoppelgangerInvasionManager>();
		}
	}

	private void Start()
	{
		//IL_0024: Unknown result type (might be due to invalid IL or missing references)
		//IL_002e: Expected O, but got Unknown
		run = ((Component)this).GetComponent<Run>();
		seed = run.seed;
		treasureRng = new Xoroshiro128Plus(seed);
		dropTable = LegacyResourcesAPI.Load<PickupDropTable>("DropTables/dtDoppelganger");
	}

	private void OnEnable()
	{
		GlobalEventManager.onCharacterDeathGlobal += OnCharacterDeathGlobal;
		ArtifactTrialMissionController.onShellTakeDamageServer += OnArtifactTrialShellTakeDamageServer;
	}

	private void OnDisable()
	{
		ArtifactTrialMissionController.onShellTakeDamageServer -= OnArtifactTrialShellTakeDamageServer;
		GlobalEventManager.onCharacterDeathGlobal -= OnCharacterDeathGlobal;
	}

	private void FixedUpdate()
	{
		//IL_0028: Unknown result type (might be due to invalid IL or missing references)
		//IL_0032: Expected O, but got Unknown
		int currentInvasionCycle = GetCurrentInvasionCycle();
		if (previousInvasionCycle < currentInvasionCycle)
		{
			previousInvasionCycle = currentInvasionCycle;
			if (artifactIsEnabled)
			{
				PerformInvasion(new Xoroshiro128Plus(seed + (ulong)currentInvasionCycle));
			}
		}
	}

	private void OnCharacterDeathGlobal(DamageReport damageReport)
	{
		//IL_0097: Unknown result type (might be due to invalid IL or missing references)
		//IL_009c: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a6: Unknown result type (might be due to invalid IL or missing references)
		Inventory inventory = damageReport.victimMaster?.inventory;
		if (!Object.op_Implicit((Object)(object)inventory))
		{
			return;
		}
		bool flag = Object.op_Implicit((Object)(object)damageReport.victimMaster.minionOwnership.ownerMaster);
		if (inventory.GetItemCount(RoR2Content.Items.InvadingDoppelganger) > 0 && inventory.GetItemCount(RoR2Content.Items.ExtraLife) == 0 && inventory.GetItemCount(DLC1Content.Items.ExtraLifeVoid) == 0 && !flag)
		{
			DoppelgangerInvasionManager.onDoppelgangerDeath?.Invoke(damageReport);
			PickupIndex pickupIndex = dropTable.GenerateDrop(treasureRng);
			if (!(pickupIndex == PickupIndex.none))
			{
				PickupDropletController.CreatePickupDroplet(pickupIndex, damageReport.victimBody.corePosition, Vector3.up * 20f);
			}
		}
	}

	private void OnArtifactTrialShellTakeDamageServer(ArtifactTrialMissionController missionController, DamageReport damageReport)
	{
		//IL_0023: Unknown result type (might be due to invalid IL or missing references)
		//IL_002d: Expected O, but got Unknown
		if (artifactIsEnabled && damageReport.victim.alive)
		{
			PerformInvasion(new Xoroshiro128Plus((ulong)damageReport.victim.health));
		}
	}

	private int GetCurrentInvasionCycle()
	{
		return Mathf.FloorToInt(run.GetRunStopwatch() / invasionInterval);
	}

	public static void PerformInvasion(Xoroshiro128Plus rng)
	{
		for (int num = CharacterMaster.readOnlyInstancesList.Count - 1; num >= 0; num--)
		{
			CharacterMaster characterMaster = CharacterMaster.readOnlyInstancesList[num];
			if (characterMaster.teamIndex == TeamIndex.Player && Object.op_Implicit((Object)(object)characterMaster.playerCharacterMasterController))
			{
				CreateDoppelganger(characterMaster, rng);
			}
		}
	}

	private static void CreateDoppelganger(CharacterMaster srcCharacterMaster, Xoroshiro128Plus rng)
	{
		SpawnCard spawnCard = DoppelgangerSpawnCard.FromMaster(srcCharacterMaster);
		if (!Object.op_Implicit((Object)(object)spawnCard))
		{
			return;
		}
		Transform spawnOnTarget;
		DirectorCore.MonsterSpawnDistance input;
		if (Object.op_Implicit((Object)(object)TeleporterInteraction.instance))
		{
			spawnOnTarget = ((Component)TeleporterInteraction.instance).transform;
			input = DirectorCore.MonsterSpawnDistance.Close;
		}
		else
		{
			spawnOnTarget = srcCharacterMaster.GetBody().coreTransform;
			input = DirectorCore.MonsterSpawnDistance.Far;
		}
		DirectorPlacementRule directorPlacementRule = new DirectorPlacementRule
		{
			spawnOnTarget = spawnOnTarget,
			placementMode = DirectorPlacementRule.PlacementMode.NearestNode
		};
		DirectorCore.GetMonsterSpawnDistance(input, out directorPlacementRule.minDistance, out directorPlacementRule.maxDistance);
		DirectorSpawnRequest directorSpawnRequest = new DirectorSpawnRequest(spawnCard, directorPlacementRule, rng);
		directorSpawnRequest.teamIndexOverride = TeamIndex.Monster;
		directorSpawnRequest.ignoreTeamMemberLimit = true;
		CombatSquad combatSquad = null;
		directorSpawnRequest.onSpawnedServer = (Action<SpawnCard.SpawnResult>)Delegate.Combine(directorSpawnRequest.onSpawnedServer, (Action<SpawnCard.SpawnResult>)delegate(SpawnCard.SpawnResult result)
		{
			if (!Object.op_Implicit((Object)(object)combatSquad))
			{
				combatSquad = Object.Instantiate<GameObject>(LegacyResourcesAPI.Load<GameObject>("Prefabs/NetworkedObjects/Encounters/ShadowCloneEncounter")).GetComponent<CombatSquad>();
			}
			combatSquad.AddMember(result.spawnedInstance.GetComponent<CharacterMaster>());
		});
		DirectorCore.instance.TrySpawnObject(directorSpawnRequest);
		if (Object.op_Implicit((Object)(object)combatSquad))
		{
			NetworkServer.Spawn(((Component)combatSquad).gameObject);
		}
		Object.Destroy((Object)(object)spawnCard);
	}
}
