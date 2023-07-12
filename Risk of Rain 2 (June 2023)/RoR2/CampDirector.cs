using RoR2.Navigation;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.Networking;

namespace RoR2;

public class CampDirector : MonoBehaviour
{
	private struct NodeDistanceSqrPair
	{
		public NodeGraph.NodeIndex nodeIndex;

		public float distanceSqr;
	}

	[Header("Main Properties")]
	[Tooltip("Which interactables the camp can spawn. If left blank, will fall back to the stage's.")]
	public DirectorCardCategorySelection interactableDirectorCards;

	public int baseMonsterCredit;

	public int baseInteractableCredit;

	public float campMinimumRadius;

	public float campMaximumRadius;

	public Transform campCenterTransform;

	public CombatDirector combatDirector;

	[Header("Combat Director Properties")]
	public EliteDef eliteDef;

	[Header("Optional Properties")]
	public bool scaleMonsterCreditWithDifficultyCoefficient;

	[Tooltip("The amount of credits to take away from the scenedirector's monster credits. A value of 1 takes away all the credits the camp spends - a value of 0 takes away none.")]
	[Range(0f, 1f)]
	public float monsterCreditPenaltyCoefficient = 0.5f;

	private Xoroshiro128Plus rng;

	private int monsterCredit;

	private static readonly WeightedSelection<DirectorCard> cardSelector = new WeightedSelection<DirectorCard>();

	private void Start()
	{
		//IL_0018: Unknown result type (might be due to invalid IL or missing references)
		//IL_0022: Expected O, but got Unknown
		if (NetworkServer.active)
		{
			rng = new Xoroshiro128Plus((ulong)Run.instance.stageRng.nextUint);
			CalculateCredits();
			PopulateCamp();
		}
	}

	private void OnEnable()
	{
		SceneDirector.onPrePopulateMonstersSceneServer += OnSceneDirectorPrePopulate;
	}

	private void OnDisable()
	{
		SceneDirector.onPrePopulateMonstersSceneServer -= OnSceneDirectorPrePopulate;
	}

	private void OnSceneDirectorPrePopulate(SceneDirector sceneDirector)
	{
		CalculateCredits();
		sceneDirector.ReduceMonsterCredits((int)((float)monsterCredit * monsterCreditPenaltyCoefficient));
	}

	private WeightedSelection<DirectorCard> GenerateInteractableCardSelection()
	{
		DirectorCardCategorySelection directorCardCategorySelection = ScriptableObject.CreateInstance<DirectorCardCategorySelection>();
		if (Object.op_Implicit((Object)(object)interactableDirectorCards))
		{
			directorCardCategorySelection.CopyFrom(interactableDirectorCards);
		}
		else if (Object.op_Implicit((Object)(object)ClassicStageInfo.instance) && Object.op_Implicit((Object)(object)ClassicStageInfo.instance.interactableCategories))
		{
			directorCardCategorySelection.CopyFrom(ClassicStageInfo.instance.interactableCategories);
		}
		WeightedSelection<DirectorCard> result = directorCardCategorySelection.GenerateDirectorCardWeightedSelection();
		Object.Destroy((Object)(object)directorCardCategorySelection);
		return result;
	}

	private void PopulateCamp()
	{
		//IL_007f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0084: Unknown result type (might be due to invalid IL or missing references)
		WeightedSelection<DirectorCard> deck = GenerateInteractableCardSelection();
		while (baseInteractableCredit > 0)
		{
			DirectorCard directorCard = SelectCard(deck, baseInteractableCredit);
			if (directorCard == null)
			{
				break;
			}
			if (!directorCard.IsAvailable())
			{
				continue;
			}
			baseInteractableCredit -= directorCard.cost;
			if (!Object.op_Implicit((Object)(object)Run.instance))
			{
				continue;
			}
			for (int i = 0; i < 10; i++)
			{
				DirectorPlacementRule placementRule = new DirectorPlacementRule
				{
					placementMode = DirectorPlacementRule.PlacementMode.Approximate,
					minDistance = campMinimumRadius,
					maxDistance = campMaximumRadius,
					position = campCenterTransform.position,
					spawnOnTarget = campCenterTransform
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
		if (Object.op_Implicit((Object)(object)Run.instance) && CombatDirector.cvDirectorCombatDisable.value)
		{
			monsterCredit = 0;
		}
		if (Object.op_Implicit((Object)(object)combatDirector))
		{
			combatDirector.monsterCredit += monsterCredit;
			monsterCredit = 0;
			((UnityEvent<GameObject>)combatDirector.onSpawnedServer).AddListener((UnityAction<GameObject>)OnMonsterSpawnedServer);
			combatDirector.SpendAllCreditsOnMapSpawns(campCenterTransform);
			((UnityEvent<GameObject>)combatDirector.onSpawnedServer).RemoveListener((UnityAction<GameObject>)OnMonsterSpawnedServer);
		}
		void OnMonsterSpawnedServer(GameObject masterObject)
		{
			EquipmentIndex equipmentIndex = eliteDef?.eliteEquipmentDef?.equipmentIndex ?? EquipmentIndex.None;
			CharacterMaster component2 = masterObject.GetComponent<CharacterMaster>();
			GameObject bodyObject = component2.GetBodyObject();
			if (Object.op_Implicit((Object)(object)bodyObject))
			{
				EntityStateMachine[] components = bodyObject.GetComponents<EntityStateMachine>();
				foreach (EntityStateMachine obj in components)
				{
					obj.initialStateType = obj.mainStateType;
				}
			}
			if (equipmentIndex != EquipmentIndex.None)
			{
				component2.inventory.SetEquipmentIndex(equipmentIndex);
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

	private void CalculateCredits()
	{
		if (scaleMonsterCreditWithDifficultyCoefficient)
		{
			monsterCredit = Mathf.CeilToInt((float)baseMonsterCredit * Run.instance.difficultyCoefficient);
		}
		else
		{
			monsterCredit = baseMonsterCredit;
		}
	}
}
