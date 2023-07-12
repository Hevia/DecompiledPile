using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using HG;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.Networking;

namespace RoR2;

public static class GoldTitanManager
{
	private class RemoveItemStealOnDeath : MonoBehaviour
	{
	}

	private static CharacterSpawnCard goldTitanSpawnCard;

	private static ItemIndex goldTitanItemIndex;

	private static MasterCatalog.MasterIndex brotherHurtMasterIndex;

	private static readonly Xoroshiro128Plus rng = new Xoroshiro128Plus(0uL);

	private static object currentChanneler;

	private static readonly List<CharacterMaster> currentTitans = new List<CharacterMaster>();

	private static readonly Func<ItemIndex, bool> goldTitanItemFilterDelegate = GoldTitanItemFilter;

	private static readonly Func<ItemIndex, bool> noItemFilterDelegate = NoItemFilter;

	private static readonly Func<CharacterMaster, bool> allCharacterMastersFilterDelegate = AllCharacterMastersFilter;

	private static event Action onChannelEnd;

	[SystemInitializer(new Type[]
	{
		typeof(ItemCatalog),
		typeof(MasterCatalog)
	})]
	private static void Init()
	{
		Run.onRunStartGlobal += OnRunStartGlobal;
		Run.onRunDestroyGlobal += OnRunDestroyGlobal;
		TeleporterInteraction.onTeleporterBeginChargingGlobal += OnTeleporterBeginChargingGlobal;
		TeleporterInteraction.onTeleporterChargedGlobal += OnTeleporterChargedGlobal;
		BossGroup.onBossGroupStartServer += OnBossGroupStartServer;
		goldTitanSpawnCard = LegacyResourcesAPI.Load<CharacterSpawnCard>("SpawnCards/CharacterSpawnCards/cscTitanGoldAlly");
		goldTitanItemIndex = RoR2Content.Items.TitanGoldDuringTP?.itemIndex ?? ItemIndex.None;
		brotherHurtMasterIndex = MasterCatalog.FindMasterIndex("BrotherHurtMaster");
	}

	private static void CalcTitanPowerAndBestTeam(out int totalItemCount, out TeamIndex teamIndex)
	{
		TeamIndex teamIndex2 = TeamIndex.None;
		int num = 0;
		totalItemCount = 0;
		for (TeamIndex teamIndex3 = TeamIndex.Neutral; teamIndex3 < TeamIndex.Count; teamIndex3++)
		{
			int itemCountForTeam = Util.GetItemCountForTeam(teamIndex3, goldTitanItemIndex, requiresAlive: true);
			if (itemCountForTeam > num)
			{
				num = itemCountForTeam;
				teamIndex2 = teamIndex3;
			}
			totalItemCount += itemCountForTeam;
		}
		teamIndex = teamIndex2;
	}

	private static void KillTitansInList(List<CharacterMaster> titansList)
	{
		try
		{
			foreach (CharacterMaster titans in titansList)
			{
				if (Object.op_Implicit((Object)(object)titans))
				{
					titans.TrueKill();
				}
			}
		}
		catch (Exception ex)
		{
			Debug.LogError((object)ex);
		}
	}

	private static bool GoldTitanItemFilter(ItemIndex itemIndex)
	{
		return itemIndex == goldTitanItemIndex;
	}

	private static bool NoItemFilter(ItemIndex itemIndex)
	{
		return false;
	}

	private static bool AllCharacterMastersFilter(CharacterMaster characterMaster)
	{
		return true;
	}

	private static bool TryStartChannelingTitansServer(object channeler, Vector3 approximatePosition, Vector3? lookAtPosition = null, Action channelEndCallback = null)
	{
		//IL_0057: Unknown result type (might be due to invalid IL or missing references)
		//IL_0058: Unknown result type (might be due to invalid IL or missing references)
		CalcTitanPowerAndBestTeam(out var totalItemCount, out var _);
		if (totalItemCount <= 0)
		{
			return false;
		}
		List<CharacterMaster> newTitans = CollectionPool<CharacterMaster, List<CharacterMaster>>.RentCollection();
		try
		{
			DirectorPlacementRule placementRule = new DirectorPlacementRule
			{
				placementMode = DirectorPlacementRule.PlacementMode.NearestNode,
				minDistance = 20f,
				maxDistance = 130f,
				position = approximatePosition
			};
			DirectorSpawnRequest directorSpawnRequest = new DirectorSpawnRequest(goldTitanSpawnCard, placementRule, rng);
			directorSpawnRequest.ignoreTeamMemberLimit = true;
			directorSpawnRequest.teamIndexOverride = TeamIndex.Player;
			float currentBoostHpCoefficient = 1f;
			float currentBoostDamageCoefficient = 1f;
			currentBoostHpCoefficient *= Mathf.Pow((float)totalItemCount, 1f);
			currentBoostDamageCoefficient *= Mathf.Pow((float)totalItemCount, 0.5f);
			directorSpawnRequest.onSpawnedServer = OnSpawnedServer;
			DirectorCore.instance.TrySpawnObject(directorSpawnRequest);
			if (newTitans.Count > 0)
			{
				EndChannelingTitansServer(currentChanneler);
				GoldTitanManager.onChannelEnd = channelEndCallback;
				currentChanneler = channeler;
				ListUtils.AddRange<CharacterMaster, List<CharacterMaster>>(currentTitans, newTitans);
			}
			return true;
			void OnSpawnedServer(SpawnCard.SpawnResult spawnResult)
			{
				//IL_00c8: Unknown result type (might be due to invalid IL or missing references)
				//IL_00ce: Unknown result type (might be due to invalid IL or missing references)
				//IL_00d3: Unknown result type (might be due to invalid IL or missing references)
				//IL_00fb: Unknown result type (might be due to invalid IL or missing references)
				//IL_0101: Unknown result type (might be due to invalid IL or missing references)
				//IL_0106: Unknown result type (might be due to invalid IL or missing references)
				//IL_0170: Unknown result type (might be due to invalid IL or missing references)
				//IL_017a: Expected O, but got Unknown
				GameObject spawnedInstance = spawnResult.spawnedInstance;
				ItemStealController titanItemStealController;
				if (Object.op_Implicit((Object)(object)spawnedInstance))
				{
					CharacterMaster component = spawnedInstance.GetComponent<CharacterMaster>();
					if (Object.op_Implicit((Object)(object)component))
					{
						newTitans.Add(component);
						component.inventory.GiveItem(RoR2Content.Items.BoostHp, Mathf.RoundToInt((currentBoostHpCoefficient - 1f) * 10f));
						component.inventory.GiveItem(RoR2Content.Items.BoostDamage, Mathf.RoundToInt((currentBoostDamageCoefficient - 1f) * 10f));
						if (lookAtPosition.HasValue)
						{
							CharacterBody body = component.GetBody();
							if (Object.op_Implicit((Object)(object)body))
							{
								if (Object.op_Implicit((Object)(object)body.characterDirection))
								{
									body.characterDirection.forward = lookAtPosition.Value - body.corePosition;
								}
								if (Object.op_Implicit((Object)(object)body.inputBank))
								{
									body.inputBank.aimDirection = lookAtPosition.Value - body.aimOrigin;
								}
							}
						}
						titanItemStealController = ((Component)component).gameObject.AddComponent<ItemStealController>();
						titanItemStealController.itemStealFilter = goldTitanItemFilterDelegate;
						titanItemStealController.itemLendFilter = noItemFilterDelegate;
						titanItemStealController.stealInterval = 0f;
						component.onBodyStart += OnBodyDiscovered;
						component.onBodyDeath.AddListener(new UnityAction(OnBodyLost));
						CharacterBody body2 = component.GetBody();
						if (Object.op_Implicit((Object)(object)body2))
						{
							OnBodyDiscovered(body2);
						}
					}
				}
				void OnBodyDiscovered(CharacterBody titanBody)
				{
					titanItemStealController.orbDestinationHurtBoxOverride = titanBody.mainHurtBox;
					titanItemStealController.StartSteal(allCharacterMastersFilterDelegate);
				}
				void OnBodyLost()
				{
					titanItemStealController?.ReclaimAllItems();
				}
			}
		}
		catch (Exception ex)
		{
			Debug.LogError((object)ex);
			KillTitansInList(newTitans);
			return false;
		}
		finally
		{
			CollectionPool<CharacterMaster, List<CharacterMaster>>.ReturnCollection(newTitans);
		}
	}

	private static void EndChannelingTitansServer(object channeler)
	{
		if (channeler == null || channeler != currentChanneler)
		{
			return;
		}
		currentChanneler = null;
		KillTitansInList(currentTitans);
		currentTitans.Clear();
		Action action = GoldTitanManager.onChannelEnd;
		GoldTitanManager.onChannelEnd = null;
		try
		{
			action?.Invoke();
		}
		catch (Exception ex)
		{
			Debug.LogError((object)ex);
		}
	}

	private static bool TryStartChannelingAgainstCombatSquadServer(CombatSquad combatSquad)
	{
		//IL_007f: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ce: Unknown result type (might be due to invalid IL or missing references)
		//IL_0118: Unknown result type (might be due to invalid IL or missing references)
		//IL_011d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0120: Unknown result type (might be due to invalid IL or missing references)
		//IL_0128: Unknown result type (might be due to invalid IL or missing references)
		//IL_0153: Unknown result type (might be due to invalid IL or missing references)
		//IL_0158: Unknown result type (might be due to invalid IL or missing references)
		//IL_015a: Unknown result type (might be due to invalid IL or missing references)
		//IL_015f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0165: Unknown result type (might be due to invalid IL or missing references)
		//IL_016a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0172: Unknown result type (might be due to invalid IL or missing references)
		//IL_0174: Unknown result type (might be due to invalid IL or missing references)
		if (!Object.op_Implicit((Object)(object)combatSquad))
		{
			return false;
		}
		List<Vector3> list = CollectionPool<Vector3, List<Vector3>>.RentCollection();
		List<Vector3> list2 = CollectionPool<Vector3, List<Vector3>>.RentCollection();
		try
		{
			combatSquad.onDefeatedServer += EndChannelingWhenDefeated;
			foreach (CharacterMaster readOnlyInstances in CharacterMaster.readOnlyInstancesList)
			{
				CharacterBody body = readOnlyInstances.GetBody();
				if (Object.op_Implicit((Object)(object)body) && readOnlyInstances.inventory.GetItemCount(goldTitanItemIndex) > 0)
				{
					list2.Add(body.corePosition);
				}
			}
			foreach (CharacterMaster readOnlyMembers in combatSquad.readOnlyMembersList)
			{
				CharacterBody body2 = readOnlyMembers.GetBody();
				if (Object.op_Implicit((Object)(object)body2))
				{
					list.Add(body2.corePosition);
				}
			}
			if (list2.Count == 0 || list.Count == 0)
			{
				if (list2.Count == list.Count)
				{
					Vector3 position = ((Component)combatSquad).transform.position;
					list2.Add(position);
					list.Add(position);
				}
				else
				{
					List<Vector3> obj = ((list2.Count == 0) ? list2 : list);
					List<Vector3> list3 = ((list2.Count != 0) ? list2 : list);
					ListUtils.AddRange<Vector3, List<Vector3>>(obj, list3);
				}
			}
			Vector3 val = Vector3Utils.AveragePrecise<List<Vector3>>(list);
			Vector3 approximatePosition = Vector3.Lerp(Vector3Utils.AveragePrecise<List<Vector3>>(list2), val, 0.15f);
			return TryStartChannelingTitansServer(combatSquad, approximatePosition, val, delegate
			{
				combatSquad.onDefeatedServer -= EndChannelingWhenDefeated;
			});
		}
		catch (Exception ex)
		{
			Debug.LogError((object)ex);
			return false;
		}
		finally
		{
			CollectionPool<Vector3, List<Vector3>>.ReturnCollection(list2);
			CollectionPool<Vector3, List<Vector3>>.ReturnCollection(list);
		}
		void EndChannelingWhenDefeated()
		{
			EndChannelingTitansServer(combatSquad);
		}
	}

	private static void OnRunStartGlobal(Run run)
	{
		if (NetworkServer.active)
		{
			rng.ResetSeed(run.seed + 88888888);
		}
	}

	private static void OnRunDestroyGlobal(Run run)
	{
		if (NetworkServer.active)
		{
			EndChannelingTitansServer(currentChanneler);
		}
	}

	private static void OnTeleporterBeginChargingGlobal(TeleporterInteraction teleporter)
	{
		//IL_000f: Unknown result type (might be due to invalid IL or missing references)
		if (NetworkServer.active)
		{
			TryStartChannelingTitansServer(teleporter, ((Component)teleporter).transform.position);
		}
	}

	private static void OnTeleporterChargedGlobal(TeleporterInteraction teleporter)
	{
		if (NetworkServer.active)
		{
			EndChannelingTitansServer(teleporter);
		}
	}

	private static void OnBossGroupStartServer(BossGroup bossGroup)
	{
		CombatSquad combatSquad = bossGroup.combatSquad;
		bool flag = false;
		foreach (CharacterMaster readOnlyMembers in combatSquad.readOnlyMembersList)
		{
			if (readOnlyMembers.masterIndex == brotherHurtMasterIndex)
			{
				flag = true;
				break;
			}
		}
		float timer;
		if (flag)
		{
			timer = 2f;
			RoR2Application.onFixedUpdate += Check;
		}
		void Check()
		{
			bool flag2 = true;
			try
			{
				if (Object.op_Implicit((Object)(object)combatSquad))
				{
					ReadOnlyCollection<CharacterMaster> readOnlyMembersList = combatSquad.readOnlyMembersList;
					for (int i = 0; i < readOnlyMembersList.Count; i++)
					{
						CharacterMaster characterMaster = readOnlyMembersList[i];
						if (Object.op_Implicit((Object)(object)characterMaster))
						{
							CharacterBody body = characterMaster.GetBody();
							if (body.HasBuff(RoR2Content.Buffs.Immune) || body.outOfCombat)
							{
								flag2 = false;
							}
							else
							{
								timer -= Time.fixedDeltaTime;
								if (timer > 0f)
								{
									flag2 = false;
								}
							}
						}
					}
					if (flag2)
					{
						TryStartChannelingAgainstCombatSquadServer(combatSquad);
					}
				}
			}
			catch (Exception ex)
			{
				Debug.Log((object)ex);
			}
			if (flag2)
			{
				RoR2Application.onFixedUpdate -= Check;
			}
		}
	}
}
