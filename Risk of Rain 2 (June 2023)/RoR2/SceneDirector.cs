using System;
using System.Collections.Generic;
using RoR2.Navigation;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.Networking;

namespace RoR2;

public class SceneDirector : MonoBehaviour
{
	private struct NodeDistanceSqrPair
	{
		public NodeGraph.NodeIndex nodeIndex;

		public float distanceSqr;
	}

	public delegate void GenerateSpawnPointsDelegate(SceneDirector sceneDirector, ref Action generationMethod);

	public SpawnCard teleporterSpawnCard;

	public float expRewardCoefficient;

	public float eliteBias;

	public float spawnDistanceMultiplier;

	private int monsterCredit;

	public GameObject teleporterInstance;

	private Xoroshiro128Plus rng;

	private static readonly WeightedSelection<DirectorCard> cardSelector = new WeightedSelection<DirectorCard>();

	public int interactableCredit { get; set; }

	public float onPopulateCreditMultiplier { get; set; } = 1f;


	public static event Action<SceneDirector, DirectorCardCategorySelection> onGenerateInteractableCardSelection;

	public static event GenerateSpawnPointsDelegate onPreGeneratePlayerSpawnPointsServer;

	public static event Action<SceneDirector> onPrePopulateSceneServer;

	public static event Action<SceneDirector> onPrePopulateMonstersSceneServer;

	public static event Action<SceneDirector> onPostPopulateSceneServer;

	private void Start()
	{
		//IL_001b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0025: Expected O, but got Unknown
		if (!NetworkServer.active)
		{
			return;
		}
		rng = new Xoroshiro128Plus((ulong)Run.instance.stageRng.nextUint);
		float num = 0.5f + (float)Run.instance.participatingPlayerCount * 0.5f;
		ClassicStageInfo component = ((Component)SceneInfo.instance).GetComponent<ClassicStageInfo>();
		if (Object.op_Implicit((Object)(object)component))
		{
			interactableCredit = (int)((float)component.sceneDirectorInteractibleCredits * num);
			if (component.bonusInteractibleCreditObjects != null)
			{
				for (int i = 0; i < component.bonusInteractibleCreditObjects.Length; i++)
				{
					ClassicStageInfo.BonusInteractibleCreditObject bonusInteractibleCreditObject = component.bonusInteractibleCreditObjects[i];
					if (Object.op_Implicit((Object)(object)bonusInteractibleCreditObject.objectThatGrantsPointsIfEnabled) && bonusInteractibleCreditObject.objectThatGrantsPointsIfEnabled.activeSelf)
					{
						interactableCredit += bonusInteractibleCreditObject.points;
					}
				}
			}
			Debug.LogFormat("Spending {0} credits on interactables...", new object[1] { interactableCredit });
			monsterCredit = (int)((float)component.sceneDirectorMonsterCredits * Run.instance.difficultyCoefficient);
		}
		SceneDirector.onPrePopulateSceneServer?.Invoke(this);
		PopulateScene();
		SceneDirector.onPostPopulateSceneServer?.Invoke(this);
	}

	private void PlaceTeleporter()
	{
		if (!Object.op_Implicit((Object)(object)teleporterInstance) && Object.op_Implicit((Object)(object)teleporterSpawnCard))
		{
			teleporterInstance = DirectorCore.instance.TrySpawnObject(new DirectorSpawnRequest(teleporterSpawnCard, new DirectorPlacementRule
			{
				placementMode = DirectorPlacementRule.PlacementMode.Random
			}, rng));
			Run.instance.OnServerTeleporterPlaced(this, teleporterInstance);
		}
	}

	private static bool IsNodeSuitableForPod(NodeGraph nodeGraph, NodeGraph.NodeIndex nodeIndex)
	{
		if (nodeGraph.GetNodeFlags(nodeIndex, out var flags) && (flags & NodeFlags.NoCeiling) != 0)
		{
			return true;
		}
		return false;
	}

	private void PlacePlayerSpawnsViaNodegraph()
	{
		//IL_011b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0120: Unknown result type (might be due to invalid IL or missing references)
		//IL_015c: Unknown result type (might be due to invalid IL or missing references)
		//IL_015e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0160: Unknown result type (might be due to invalid IL or missing references)
		//IL_0165: Unknown result type (might be due to invalid IL or missing references)
		bool usePod = Stage.instance.usePod;
		NodeGraph groundNodes = SceneInfo.instance.groundNodes;
		NodeFlags requiredFlags = NodeFlags.None;
		NodeFlags nodeFlags = NodeFlags.None;
		nodeFlags |= NodeFlags.NoCharacterSpawn;
		List<NodeGraph.NodeIndex> list = groundNodes.GetActiveNodesForHullMaskWithFlagConditions(HullMask.Golem, requiredFlags, nodeFlags);
		if (usePod)
		{
			int num = list.Count - 1;
			while (num >= 0 && list.Count > 1)
			{
				if (!IsNodeSuitableForPod(groundNodes, list[num]))
				{
					list.RemoveAt(num);
				}
				num--;
			}
		}
		if (PlayerSpawnInhibitor.readOnlyInstancesList.Count > 0)
		{
			List<NodeGraph.NodeIndex> list2 = new List<NodeGraph.NodeIndex>();
			for (int i = 0; i < list.Count; i++)
			{
				bool flag = false;
				foreach (PlayerSpawnInhibitor readOnlyInstances in PlayerSpawnInhibitor.readOnlyInstancesList)
				{
					if (readOnlyInstances.IsInhibiting(groundNodes, list[i]))
					{
						flag = true;
						break;
					}
				}
				if (!flag)
				{
					list2.Add(list[i]);
				}
			}
			if (list2.Count > 0)
			{
				list = list2;
			}
		}
		NodeGraph.NodeIndex nodeIndex;
		if (Object.op_Implicit((Object)(object)teleporterInstance))
		{
			Vector3 position = teleporterInstance.transform.position;
			List<NodeDistanceSqrPair> list3 = new List<NodeDistanceSqrPair>();
			for (int j = 0; j < list.Count; j++)
			{
				groundNodes.GetNodePosition(list[j], out var position2);
				NodeDistanceSqrPair item = new NodeDistanceSqrPair
				{
					nodeIndex = list[j]
				};
				Vector3 val = position - position2;
				item.distanceSqr = ((Vector3)(ref val)).sqrMagnitude;
				list3.Add(item);
			}
			list3.Sort((NodeDistanceSqrPair a, NodeDistanceSqrPair b) => a.distanceSqr.CompareTo(b.distanceSqr));
			int index = rng.RangeInt(list3.Count * 3 / 4, list3.Count);
			nodeIndex = list3[index].nodeIndex;
		}
		else
		{
			nodeIndex = rng.NextElementUniform<NodeGraph.NodeIndex>(list);
		}
		NodeGraphSpider nodeGraphSpider = new NodeGraphSpider(groundNodes, HullMask.Human);
		nodeGraphSpider.AddNodeForNextStep(nodeIndex);
		while (nodeGraphSpider.PerformStep())
		{
			List<NodeGraphSpider.StepInfo> collectedSteps = nodeGraphSpider.collectedSteps;
			for (int num2 = collectedSteps.Count - 1; num2 >= 0; num2--)
			{
				if ((RoR2Application.maxPlayers <= list.Count && !list.Contains(collectedSteps[num2].node)) || (usePod && !IsNodeSuitableForPod(groundNodes, collectedSteps[num2].node)))
				{
					collectedSteps.RemoveAt(num2);
				}
			}
			if (collectedSteps.Count >= RoR2Application.maxPlayers)
			{
				break;
			}
		}
		List<NodeGraphSpider.StepInfo> collectedSteps2 = nodeGraphSpider.collectedSteps;
		Util.ShuffleList(collectedSteps2, Run.instance.stageRng);
		int num3 = Math.Min(nodeGraphSpider.collectedSteps.Count, RoR2Application.maxPlayers);
		for (int k = 0; k < num3; k++)
		{
			SpawnPoint.AddSpawnPoint(groundNodes, collectedSteps2[k].node, rng);
		}
	}

	private void RemoveAllExistingSpawnPoints()
	{
		List<SpawnPoint> list = new List<SpawnPoint>(SpawnPoint.readOnlyInstancesList);
		for (int i = 0; i < list.Count; i++)
		{
			Object.Destroy((Object)(object)((Component)list[i]).gameObject);
		}
	}

	private void CullExistingSpawnPoints()
	{
		//IL_002a: Unknown result type (might be due to invalid IL or missing references)
		//IL_002f: Unknown result type (might be due to invalid IL or missing references)
		List<SpawnPoint> list = new List<SpawnPoint>(SpawnPoint.readOnlyInstancesList);
		if (!Object.op_Implicit((Object)(object)teleporterInstance))
		{
			return;
		}
		Vector3 teleporterPosition = teleporterInstance.transform.position;
		list.Sort(delegate(SpawnPoint a, SpawnPoint b)
		{
			//IL_0001: Unknown result type (might be due to invalid IL or missing references)
			//IL_000c: Unknown result type (might be due to invalid IL or missing references)
			//IL_0011: Unknown result type (might be due to invalid IL or missing references)
			//IL_0016: Unknown result type (might be due to invalid IL or missing references)
			//IL_0022: Unknown result type (might be due to invalid IL or missing references)
			//IL_002d: Unknown result type (might be due to invalid IL or missing references)
			//IL_0032: Unknown result type (might be due to invalid IL or missing references)
			//IL_0037: Unknown result type (might be due to invalid IL or missing references)
			Vector3 val = teleporterPosition - ((Component)a).transform.position;
			float sqrMagnitude = ((Vector3)(ref val)).sqrMagnitude;
			val = teleporterPosition - ((Component)b).transform.position;
			return sqrMagnitude.CompareTo(((Vector3)(ref val)).sqrMagnitude);
		});
		Debug.Log((object)"reorder list");
		for (int num = list.Count; num >= 0; num--)
		{
			if (num < list.Count - RoR2Application.maxPlayers)
			{
				Object.Destroy((Object)(object)((Component)list[num]).gameObject);
			}
		}
	}

	private void DefaultPlayerSpawnPointGenerator()
	{
		bool num = SpawnPoint.readOnlyInstancesList.Count == 0;
		bool flag = Run.instance.autoGenerateSpawnPoints && Object.op_Implicit((Object)(object)Stage.instance) && !Stage.instance.usePod;
		if (num || flag)
		{
			RemoveAllExistingSpawnPoints();
			PlacePlayerSpawnsViaNodegraph();
		}
		else
		{
			CullExistingSpawnPoints();
		}
	}

	private WeightedSelection<DirectorCard> GenerateInteractableCardSelection()
	{
		DirectorCardCategorySelection directorCardCategorySelection = ScriptableObject.CreateInstance<DirectorCardCategorySelection>();
		if (Object.op_Implicit((Object)(object)ClassicStageInfo.instance) && Object.op_Implicit((Object)(object)ClassicStageInfo.instance.interactableCategories))
		{
			directorCardCategorySelection.CopyFrom(ClassicStageInfo.instance.interactableCategories);
		}
		SceneDirector.onGenerateInteractableCardSelection?.Invoke(this, directorCardCategorySelection);
		WeightedSelection<DirectorCard> result = directorCardCategorySelection.GenerateDirectorCardWeightedSelection();
		Object.Destroy((Object)(object)directorCardCategorySelection);
		return result;
	}

	private void PopulateScene()
	{
		//IL_02bf: Unknown result type (might be due to invalid IL or missing references)
		//IL_02c6: Expected O, but got Unknown
		//IL_0351: Unknown result type (might be due to invalid IL or missing references)
		//IL_0358: Expected O, but got Unknown
		//IL_03e3: Unknown result type (might be due to invalid IL or missing references)
		//IL_03ea: Expected O, but got Unknown
		WeightedSelection<DirectorCard> deck = GenerateInteractableCardSelection();
		PlaceTeleporter();
		Dictionary<DirectorCard, int> dictionary = new Dictionary<DirectorCard, int>();
		interactableCredit = (int)Mathf.Floor((float)interactableCredit * onPopulateCreditMultiplier);
		while (interactableCredit > 0)
		{
			DirectorCard directorCard = SelectCard(deck, interactableCredit);
			if (directorCard == null)
			{
				break;
			}
			if (!directorCard.IsAvailable())
			{
				continue;
			}
			if (!dictionary.ContainsKey(directorCard))
			{
				InteractableSpawnCard interactableSpawnCard = directorCard.spawnCard as InteractableSpawnCard;
				if (Object.op_Implicit((Object)(object)interactableSpawnCard))
				{
					int value = int.MaxValue;
					if (interactableSpawnCard.maxSpawnsPerStage >= 0)
					{
						value = interactableSpawnCard.maxSpawnsPerStage;
					}
					dictionary[directorCard] = value;
				}
			}
			if (!dictionary.TryGetValue(directorCard, out var value2) || value2 <= 0)
			{
				continue;
			}
			dictionary[directorCard] = value2 - 1;
			interactableCredit -= directorCard.cost;
			if (!Object.op_Implicit((Object)(object)Run.instance))
			{
				continue;
			}
			for (int i = 0; i < 10; i++)
			{
				DirectorPlacementRule placementRule = new DirectorPlacementRule
				{
					placementMode = DirectorPlacementRule.PlacementMode.Random
				};
				GameObject val = DirectorCore.instance.TrySpawnObject(new DirectorSpawnRequest(directorCard.spawnCard, placementRule, rng));
				if (Object.op_Implicit((Object)(object)val))
				{
					PurchaseInteraction component = val.GetComponent<PurchaseInteraction>();
					if (Object.op_Implicit((Object)(object)component) && component.costType == CostTypeIndex.Money)
					{
						component.Networkcost = Run.instance.GetDifficultyScaledCost(component.cost);
					}
					break;
				}
			}
		}
		Action generationMethod = DefaultPlayerSpawnPointGenerator;
		SceneDirector.onPreGeneratePlayerSpawnPointsServer?.Invoke(this, ref generationMethod);
		generationMethod?.Invoke();
		Run.instance.OnPlayerSpawnPointsPlaced(this);
		SceneDirector.onPrePopulateMonstersSceneServer?.Invoke(this);
		if (Object.op_Implicit((Object)(object)Run.instance) && CombatDirector.cvDirectorCombatDisable.value)
		{
			monsterCredit = 0;
		}
		CombatDirector component2 = ((Component)this).GetComponent<CombatDirector>();
		if (Object.op_Implicit((Object)(object)component2))
		{
			float num = component2.expRewardCoefficient;
			float num2 = component2.eliteBias;
			float num3 = component2.spawnDistanceMultiplier;
			component2.monsterCredit += monsterCredit;
			component2.expRewardCoefficient = expRewardCoefficient;
			component2.eliteBias = eliteBias;
			component2.spawnDistanceMultiplier = spawnDistanceMultiplier;
			monsterCredit = 0;
			((UnityEvent<GameObject>)component2.onSpawnedServer).AddListener((UnityAction<GameObject>)OnMonsterSpawnedServer);
			component2.SpendAllCreditsOnMapSpawns(Object.op_Implicit((Object)(object)TeleporterInteraction.instance) ? ((Component)TeleporterInteraction.instance).transform : null);
			((UnityEvent<GameObject>)component2.onSpawnedServer).RemoveListener((UnityAction<GameObject>)OnMonsterSpawnedServer);
			component2.expRewardCoefficient = num;
			component2.eliteBias = num2;
			component2.spawnDistanceMultiplier = num3;
		}
		if (!SceneInfo.instance.countsAsStage)
		{
			return;
		}
		Xoroshiro128Plus val2 = new Xoroshiro128Plus(rng.nextUlong);
		int num4 = 0;
		foreach (CharacterMaster readOnlyInstances in CharacterMaster.readOnlyInstancesList)
		{
			if (readOnlyInstances.inventory.GetItemCount(RoR2Content.Items.TreasureCache) > 0)
			{
				num4++;
			}
		}
		for (int j = 0; j < num4; j++)
		{
			DirectorCore.instance.TrySpawnObject(new DirectorSpawnRequest(LegacyResourcesAPI.Load<SpawnCard>("SpawnCards/InteractableSpawnCard/iscLockbox"), new DirectorPlacementRule
			{
				placementMode = DirectorPlacementRule.PlacementMode.Random
			}, val2));
		}
		Xoroshiro128Plus val3 = new Xoroshiro128Plus(rng.nextUlong);
		int num5 = 0;
		foreach (CharacterMaster readOnlyInstances2 in CharacterMaster.readOnlyInstancesList)
		{
			if (readOnlyInstances2.inventory.GetItemCount(DLC1Content.Items.TreasureCacheVoid) > 0)
			{
				num5++;
			}
		}
		for (int k = 0; k < num5; k++)
		{
			DirectorCore.instance.TrySpawnObject(new DirectorSpawnRequest(LegacyResourcesAPI.Load<SpawnCard>("SpawnCards/InteractableSpawnCard/iscLockboxVoid"), new DirectorPlacementRule
			{
				placementMode = DirectorPlacementRule.PlacementMode.Random
			}, val3));
		}
		Xoroshiro128Plus val4 = new Xoroshiro128Plus(rng.nextUlong);
		int num6 = 0;
		foreach (CharacterMaster readOnlyInstances3 in CharacterMaster.readOnlyInstancesList)
		{
			if (readOnlyInstances3.inventory.GetItemCount(DLC1Content.Items.FreeChest) > 0)
			{
				num6++;
			}
		}
		for (int l = 0; l < num6; l++)
		{
			DirectorCore.instance.TrySpawnObject(new DirectorSpawnRequest(LegacyResourcesAPI.Load<SpawnCard>("SpawnCards/InteractableSpawnCard/iscFreeChest"), new DirectorPlacementRule
			{
				placementMode = DirectorPlacementRule.PlacementMode.Random
			}, val4));
		}
		static void OnMonsterSpawnedServer(GameObject masterObject)
		{
			GameObject bodyObject = masterObject.GetComponent<CharacterMaster>().GetBodyObject();
			if (Object.op_Implicit((Object)(object)bodyObject))
			{
				EntityStateMachine[] components = bodyObject.GetComponents<EntityStateMachine>();
				foreach (EntityStateMachine obj in components)
				{
					obj.initialStateType = obj.mainStateType;
				}
			}
		}
	}

	private DirectorCard SelectCard(WeightedSelection<DirectorCard> deck, int maxCost)
	{
		cardSelector.Clear();
		int i = 0;
		for (int count = deck.Count; i < count; i++)
		{
			WeightedSelection<DirectorCard>.ChoiceInfo choice = deck.GetChoice(i);
			if (choice.value.cost <= maxCost)
			{
				cardSelector.AddChoice(choice);
			}
		}
		if (cardSelector.Count == 0)
		{
			return null;
		}
		return cardSelector.Evaluate(rng.nextNormalizedFloat);
	}

	public void ReduceMonsterCredits(int creditReduction)
	{
		monsterCredit = Mathf.Max(0, monsterCredit - creditReduction);
	}
}
