using System;
using System.Collections.Generic;
using System.Globalization;
using System.Runtime.InteropServices;
using Facepunch.Steamworks;
using HG;
using RoR2.ExpansionManagement;
using UnityEngine;
using UnityEngine.Networking;

namespace RoR2;

public class WeeklyRun : Run
{
	private Xoroshiro128Plus bossAffixRng;

	public static readonly DateTime startDate = new DateTime(2018, 8, 27, 0, 0, 0, 0, DateTimeKind.Utc);

	public const int cycleLength = 3;

	private string leaderboardName;

	[SyncVar]
	private uint serverSeedCycle;

	private EquipmentIndex[] bossAffixes = Array.Empty<EquipmentIndex>();

	public SpawnCard crystalSpawnCard;

	public uint crystalCount = 3u;

	public uint crystalRewardValue = 50u;

	public uint crystalsRequiredToKill = 3u;

	private List<OnDestroyCallback> crystalActiveList = new List<OnDestroyCallback>();

	public SpawnCard equipmentBarrelSpawnCard;

	public uint equipmentBarrelCount = 3u;

	public float equipmentBarrelRadius = 10f;

	public static DateTime now => Util.UnixTimeStampToDateTimeUtc(Client.Instance.Utils.GetServerRealTime());

	public uint crystalsKilled => (uint)(crystalCount - crystalActiveList.Count);

	public uint NetworkserverSeedCycle
	{
		get
		{
			return serverSeedCycle;
		}
		[param: In]
		set
		{
			((NetworkBehaviour)this).SetSyncVar<uint>(value, ref serverSeedCycle, 64u);
		}
	}

	public static uint GetCurrentSeedCycle()
	{
		return (uint)((now - startDate).Days / 3);
	}

	public static DateTime GetSeedCycleStartDateTime(uint seedCycle)
	{
		DateTime dateTime = startDate;
		return dateTime.AddDays(seedCycle * 3);
	}

	public static DateTime GetSeedCycleStartDateTime()
	{
		return GetSeedCycleStartDateTime(GetCurrentSeedCycle());
	}

	public static DateTime GetSeedCycleEndDateTime()
	{
		return GetSeedCycleStartDateTime(GetCurrentSeedCycle() + 1);
	}

	protected new void Start()
	{
		//IL_0019: Unknown result type (might be due to invalid IL or missing references)
		//IL_0023: Expected O, but got Unknown
		base.Start();
		if (NetworkServer.active)
		{
			bossAffixRng = new Xoroshiro128Plus(runRNG.nextUlong);
			NetworkserverSeedCycle = GetCurrentSeedCycle();
		}
		bossAffixes = new EquipmentIndex[2]
		{
			RoR2Content.Equipment.AffixRed.equipmentIndex,
			RoR2Content.Equipment.AffixBlue.equipmentIndex
		};
	}

	protected override void OnFixedUpdate()
	{
		//IL_009b: Unknown result type (might be due to invalid IL or missing references)
		base.OnFixedUpdate();
		if (!Object.op_Implicit((Object)(object)TeleporterInteraction.instance))
		{
			return;
		}
		bool flag = crystalsRequiredToKill > crystalsKilled;
		if (flag == TeleporterInteraction.instance.locked)
		{
			return;
		}
		if (flag)
		{
			if (NetworkServer.active)
			{
				TeleporterInteraction.instance.locked = true;
			}
			return;
		}
		if (NetworkServer.active)
		{
			TeleporterInteraction.instance.locked = false;
		}
		ChildLocator component = ((Component)((Component)TeleporterInteraction.instance).GetComponent<ModelLocator>().modelTransform).GetComponent<ChildLocator>();
		if (Object.op_Implicit((Object)(object)component))
		{
			Transform val = component.FindChild("TimeCrystalBeaconBlocker");
			EffectManager.SpawnEffect(LegacyResourcesAPI.Load<GameObject>("Prefabs/Effects/TimeCrystalDeath"), new EffectData
			{
				origin = ((Component)val).transform.position
			}, transmit: false);
			((Component)val).gameObject.SetActive(false);
		}
	}

	public override ulong GenerateSeedForNewRun()
	{
		return (ulong)GetCurrentSeedCycle() << 32;
	}

	public override void HandlePlayerFirstEntryAnimation(CharacterBody body, Vector3 spawnPosition, Quaternion spawnRotation)
	{
	}

	public override void AdvanceStage(SceneDef nextScene)
	{
		if (stageClearCount == 1 && SceneInfo.instance.countsAsStage)
		{
			BeginGameOver(RoR2Content.GameEndings.PrismaticTrialEnding);
		}
		else
		{
			base.AdvanceStage(nextScene);
		}
	}

	public override void OnClientGameOver(RunReport runReport)
	{
		base.OnClientGameOver(runReport);
		ClientSubmitLeaderboardScore(runReport);
	}

	public override void OnServerBossAdded(BossGroup bossGroup, CharacterMaster characterMaster)
	{
		base.OnServerBossAdded(bossGroup, characterMaster);
		if (stageClearCount >= 1)
		{
			if (characterMaster.inventory.GetEquipmentIndex() == EquipmentIndex.None)
			{
				characterMaster.inventory.SetEquipmentIndex(bossAffixRng.NextElementUniform<EquipmentIndex>(bossAffixes));
			}
			characterMaster.inventory.GiveItem(RoR2Content.Items.BoostHp, 5);
			characterMaster.inventory.GiveItem(RoR2Content.Items.BoostDamage);
		}
	}

	public override void OnServerBossDefeated(BossGroup bossGroup)
	{
		base.OnServerBossDefeated(bossGroup);
		if (Object.op_Implicit((Object)(object)TeleporterInteraction.instance))
		{
			TeleporterInteraction.instance.holdoutZoneController.FullyChargeHoldoutZone();
		}
	}

	public override GameObject GetTeleportEffectPrefab(GameObject objectToTeleport)
	{
		return LegacyResourcesAPI.Load<GameObject>("Prefabs/Effects/TeleportOutCrystalBoom");
	}

	public override void OnServerTeleporterPlaced(SceneDirector sceneDirector, GameObject teleporter)
	{
		base.OnServerTeleporterPlaced(sceneDirector, teleporter);
		DirectorPlacementRule directorPlacementRule = new DirectorPlacementRule();
		directorPlacementRule.placementMode = DirectorPlacementRule.PlacementMode.Random;
		for (int i = 0; i < crystalCount; i++)
		{
			GameObject val = DirectorCore.instance.TrySpawnObject(new DirectorSpawnRequest(crystalSpawnCard, directorPlacementRule, stageRng));
			if (Object.op_Implicit((Object)(object)val))
			{
				DeathRewards component2 = val.GetComponent<DeathRewards>();
				if (Object.op_Implicit((Object)(object)component2))
				{
					component2.goldReward = crystalRewardValue;
				}
			}
			crystalActiveList.Add(OnDestroyCallback.AddCallback(val, delegate(OnDestroyCallback component)
			{
				crystalActiveList.Remove(component);
			}));
		}
		if (Object.op_Implicit((Object)(object)TeleporterInteraction.instance))
		{
			ChildLocator component3 = ((Component)((Component)TeleporterInteraction.instance).GetComponent<ModelLocator>().modelTransform).GetComponent<ChildLocator>();
			if (Object.op_Implicit((Object)(object)component3))
			{
				((Component)component3.FindChild("TimeCrystalProps")).gameObject.SetActive(true);
				((Component)component3.FindChild("TimeCrystalBeaconBlocker")).gameObject.SetActive(true);
			}
		}
	}

	public override void OnPlayerSpawnPointsPlaced(SceneDirector sceneDirector)
	{
		//IL_003c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0041: Unknown result type (might be due to invalid IL or missing references)
		//IL_0046: Unknown result type (might be due to invalid IL or missing references)
		//IL_0051: Unknown result type (might be due to invalid IL or missing references)
		//IL_0056: Unknown result type (might be due to invalid IL or missing references)
		//IL_005b: Unknown result type (might be due to invalid IL or missing references)
		//IL_008b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0090: Unknown result type (might be due to invalid IL or missing references)
		//IL_0091: Unknown result type (might be due to invalid IL or missing references)
		//IL_0096: Unknown result type (might be due to invalid IL or missing references)
		if (stageClearCount != 0)
		{
			return;
		}
		SpawnPoint spawnPoint = SpawnPoint.readOnlyInstancesList[0];
		if (Object.op_Implicit((Object)(object)spawnPoint))
		{
			float num = 360f / (float)equipmentBarrelCount;
			for (int i = 0; i < equipmentBarrelCount; i++)
			{
				Vector3 val = Quaternion.AngleAxis(num * (float)i, Vector3.up) * (Vector3.forward * equipmentBarrelRadius);
				DirectorPlacementRule directorPlacementRule = new DirectorPlacementRule();
				directorPlacementRule.minDistance = 0f;
				directorPlacementRule.maxDistance = 3f;
				directorPlacementRule.placementMode = DirectorPlacementRule.PlacementMode.NearestNode;
				directorPlacementRule.position = ((Component)spawnPoint).transform.position + val;
				DirectorCore.instance.TrySpawnObject(new DirectorSpawnRequest(equipmentBarrelSpawnCard, directorPlacementRule, stageRng));
			}
		}
	}

	public static string GetLeaderboardName(int playerCount, uint seedCycle)
	{
		if (Console.sessionCheatsEnabled)
		{
			return null;
		}
		return string.Format(CultureInfo.InvariantCulture, "weekly{0}p{1}", playerCount, seedCycle);
	}

	protected void ClientSubmitLeaderboardScore(RunReport runReport)
	{
		Debug.LogFormat("Attempting to submit leaderboard score.", Array.Empty<object>());
		Debug.Log((object)runReport.gameEnding.cachedName);
		if (!runReport.gameEnding.isWin)
		{
			Debug.Log((object)"Didn't win - aborting");
			return;
		}
		bool flag = false;
		foreach (NetworkUser readOnlyLocalPlayers in NetworkUser.readOnlyLocalPlayersList)
		{
			if (readOnlyLocalPlayers.isParticipating)
			{
				flag = true;
				break;
			}
		}
		if (!flag)
		{
			return;
		}
		int num = PlayerCharacterMasterController.instances.Count;
		switch (num)
		{
		default:
			return;
		case 3:
		case 4:
			num = 4;
			break;
		case 1:
		case 2:
			break;
		}
		string text = GetLeaderboardName(num, serverSeedCycle);
		if (string.IsNullOrEmpty(text))
		{
			return;
		}
		int[] subScores = new int[64];
		GameObject bodyPrefab = BodyCatalog.GetBodyPrefab(NetworkUser.readOnlyLocalPlayersList[0].bodyIndexPreference);
		if (!Object.op_Implicit((Object)(object)bodyPrefab))
		{
			return;
		}
		SurvivorDef survivorDef = SurvivorCatalog.FindSurvivorDefFromBody(bodyPrefab);
		if (!((Object)(object)survivorDef == (Object)null))
		{
			subScores[1] = (int)survivorDef.survivorIndex;
			Leaderboard leaderboard = Client.Instance.GetLeaderboard(text, (LeaderboardSortMethod)1, (LeaderboardDisplayType)3);
			leaderboard.OnBoardInformation = delegate
			{
				leaderboard.AddScore(true, (int)Math.Ceiling((double)runReport.runStopwatchValue * 1000.0), subScores);
			};
		}
	}

	public override void OverrideRuleChoices(RuleChoiceMask mustInclude, RuleChoiceMask mustExclude, ulong runSeed)
	{
		//IL_006b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0071: Expected O, but got Unknown
		//IL_01ba: Unknown result type (might be due to invalid IL or missing references)
		//IL_01bf: Unknown result type (might be due to invalid IL or missing references)
		//IL_01c3: Unknown result type (might be due to invalid IL or missing references)
		//IL_01c8: Unknown result type (might be due to invalid IL or missing references)
		base.OverrideRuleChoices(mustInclude, mustExclude, base.seed);
		ForceChoice(mustInclude, mustExclude, "Difficulty.Normal");
		ForceChoice(mustInclude, mustExclude, "Misc.StartingMoney.50");
		ForceChoice(mustInclude, mustExclude, "Misc.StageOrder.Random");
		ForceChoice(mustInclude, mustExclude, "Misc.KeepMoneyBetweenStages.Off");
		for (int i = 0; i < ArtifactCatalog.artifactCount; i++)
		{
			ForceChoice(mustInclude, mustExclude, FindRuleForArtifact((ArtifactIndex)i).FindChoice("Off"));
		}
		Xoroshiro128Plus val = new Xoroshiro128Plus(runSeed);
		Debug.LogFormat("Weekly Run Seed: {0}", new object[1] { runSeed });
		if (val.nextNormalizedFloat < 1f)
		{
			int num = val.RangeInt(2, 7);
			ArtifactIndex[] array = new ArtifactIndex[ArtifactCatalog.artifactCount];
			for (int j = 0; j < array.Length; j++)
			{
				array[j] = (ArtifactIndex)j;
			}
			Util.ShuffleArray(array, val);
			for (int k = 0; k < num; k++)
			{
				if ((Object)(object)ArtifactCatalog.GetArtifactDef(array[k]) != (Object)(object)RoR2Content.Artifacts.randomSurvivorOnRespawnArtifactDef)
				{
					ForceChoice(mustInclude, mustExclude, FindRuleForArtifact(array[k]).FindChoice("On"));
				}
			}
		}
		ItemIndex itemIndex = (ItemIndex)0;
		for (ItemIndex itemCount = (ItemIndex)ItemCatalog.itemCount; itemIndex < itemCount; itemIndex++)
		{
			ItemDef itemDef = ItemCatalog.GetItemDef(itemIndex);
			RuleChoiceDef ruleChoiceDef = RuleCatalog.FindRuleDef("Items." + ((Object)itemDef).name)?.FindChoice("On");
			if (ruleChoiceDef != null)
			{
				ForceChoice(mustInclude, mustExclude, ruleChoiceDef);
			}
		}
		EquipmentIndex equipmentIndex = (EquipmentIndex)0;
		for (EquipmentIndex equipmentCount = (EquipmentIndex)EquipmentCatalog.equipmentCount; equipmentIndex < equipmentCount; equipmentIndex++)
		{
			EquipmentDef equipmentDef = EquipmentCatalog.GetEquipmentDef(equipmentIndex);
			RuleChoiceDef ruleChoiceDef2 = RuleCatalog.FindRuleDef("Equipment." + ((Object)equipmentDef).name)?.FindChoice("On");
			if (ruleChoiceDef2 != null)
			{
				ForceChoice(mustInclude, mustExclude, ruleChoiceDef2);
			}
		}
		Enumerator<ExpansionDef> enumerator = ExpansionCatalog.expansionDefs.GetEnumerator();
		try
		{
			while (enumerator.MoveNext())
			{
				ExpansionDef current = enumerator.Current;
				RuleChoiceDef ruleChoiceDef3 = RuleCatalog.FindRuleDef("Expansions." + ((Object)current).name)?.FindChoice("On");
				if (ruleChoiceDef3 != null)
				{
					ForceChoice(mustInclude, mustExclude, ruleChoiceDef3);
				}
			}
		}
		finally
		{
			((IDisposable)enumerator).Dispose();
		}
		static RuleDef FindRuleForArtifact(ArtifactIndex artifactIndex)
		{
			ArtifactDef artifactDef = ArtifactCatalog.GetArtifactDef(artifactIndex);
			return RuleCatalog.FindRuleDef("Artifacts." + artifactDef.cachedName);
		}
	}

	public override bool IsUnlockableUnlocked(UnlockableDef unlockableDef)
	{
		return true;
	}

	public override bool CanUnlockableBeGrantedThisRun(UnlockableDef unlockableDef)
	{
		return false;
	}

	public override bool DoesEveryoneHaveThisUnlockableUnlocked(UnlockableDef unlockableDef)
	{
		return true;
	}

	protected override void HandlePostRunDestination()
	{
		Console.instance.SubmitCmd(null, "transition_command \"disconnect\";");
	}

	protected override bool ShouldUpdateRunStopwatch()
	{
		return base.livingPlayerCount > 0;
	}

	public override bool ShouldAllowNonChampionBossSpawn()
	{
		return true;
	}

	private void UNetVersion()
	{
	}

	public override bool OnSerialize(NetworkWriter writer, bool forceAll)
	{
		bool flag = base.OnSerialize(writer, forceAll);
		if (forceAll)
		{
			writer.WritePackedUInt32(serverSeedCycle);
			return true;
		}
		bool flag2 = false;
		if ((((NetworkBehaviour)this).syncVarDirtyBits & 0x40u) != 0)
		{
			if (!flag2)
			{
				writer.WritePackedUInt32(((NetworkBehaviour)this).syncVarDirtyBits);
				flag2 = true;
			}
			writer.WritePackedUInt32(serverSeedCycle);
		}
		if (!flag2)
		{
			writer.WritePackedUInt32(((NetworkBehaviour)this).syncVarDirtyBits);
		}
		return flag2 || flag;
	}

	public override void OnDeserialize(NetworkReader reader, bool initialState)
	{
		base.OnDeserialize(reader, initialState);
		if (initialState)
		{
			serverSeedCycle = reader.ReadPackedUInt32();
			return;
		}
		int num = (int)reader.ReadPackedUInt32();
		if (((uint)num & 0x40u) != 0)
		{
			serverSeedCycle = reader.ReadPackedUInt32();
		}
	}

	public override void PreStartClient()
	{
		base.PreStartClient();
	}
}
