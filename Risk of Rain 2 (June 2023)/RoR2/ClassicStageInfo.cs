using System;
using System.Collections;
using HG;
using JetBrains.Annotations;
using UnityEngine;
using UnityEngine.Networking;

namespace RoR2;

[RequireComponent(typeof(SceneInfo))]
public class ClassicStageInfo : MonoBehaviour
{
	[Serializable]
	public struct BonusInteractibleCreditObject
	{
		public GameObject objectThatGrantsPointsIfEnabled;

		public int points;
	}

	[Serializable]
	public struct MonsterFamily
	{
		[SerializeField]
		public DirectorCardCategorySelection monsterFamilyCategories;

		public string familySelectionChatString;

		public float selectionWeight;

		public int minimumStageCompletion;

		public int maximumStageCompletion;
	}

	private DirectorCardCategorySelection modifiableMonsterCategories;

	[Tooltip("We'll select a single DCCS from this pool when we enter the stage to determine which monsters can spawn.")]
	[SerializeField]
	private DccsPool monsterDccsPool;

	[Tooltip("We'll select a single DCCS from this pool when we enter the stage to determine which interactables can spawn.")]
	[SerializeField]
	private DccsPool interactableDccsPool;

	private ulong seedServer;

	private Xoroshiro128Plus rng;

	public int sceneDirectorInteractibleCredits = 200;

	public int sceneDirectorMonsterCredits = 20;

	public BonusInteractibleCreditObject[] bonusInteractibleCreditObjects;

	public static float monsterFamilyChance = 0.02f;

	[HideInInspector]
	[SerializeField]
	private DirectorCard[] monsterCards;

	[HideInInspector]
	[SerializeField]
	public DirectorCard[] interactableCards;

	[CanBeNull]
	[Tooltip("Deprecated.  Use MonsterDccsPool instead.")]
	[ShowFieldObsolete]
	public DirectorCardCategorySelection interactableCategories;

	[SerializeField]
	[CanBeNull]
	[ShowFieldObsolete]
	[Tooltip("Deprecated.  Use MonsterDccsPool instead.")]
	private DirectorCardCategorySelection monsterCategories;

	[ShowFieldObsolete]
	[Tooltip("Deprecated.  Use MonsterDccsPool instead.")]
	public MonsterFamily[] possibleMonsterFamilies;

	public WeightedSelection<DirectorCard> monsterSelection { get; private set; }

	public static ClassicStageInfo instance { get; private set; }

	private static void HandleSingleMonsterTypeArtifact(DirectorCardCategorySelection monsterCategories, Xoroshiro128Plus rng)
	{
		ScriptableObject.CreateInstance<DirectorCardCategorySelection>().CopyFrom(monsterCategories);
		float baseCredits = 40f * Run.instance.difficultyCoefficient;
		WeightedSelection<DirectorCard> candidatesSelection = new WeightedSelection<DirectorCard>();
		AddCardsWhichPassCondition(CardIsAffordable);
		if (candidatesSelection.Count == 0)
		{
			AddCardsWhichPassCondition(ReturnTrue);
		}
		if (candidatesSelection.Count == 0)
		{
			Debug.LogWarning((object)"Could not collapse director card selection down to one, no cards passed the filters!");
			return;
		}
		DirectorCard directorCard = candidatesSelection.Evaluate(rng.nextNormalizedFloat);
		monsterCategories.Clear();
		int categoryIndex = monsterCategories.AddCategory("Basic Monsters", 1f);
		monsterCategories.AddCard(categoryIndex, directorCard);
		BodyIndex bodyIndex = directorCard.spawnCard.prefab.GetComponent<CharacterMaster>().bodyPrefab.GetComponent<CharacterBody>().bodyIndex;
		if (Object.op_Implicit((Object)(object)Stage.instance))
		{
			SetStageSingleMonsterType(Stage.instance);
		}
		else
		{
			Stage.onServerStageBegin += SetStageSingleMonsterType;
		}
		void AddCardsWhichPassCondition(Predicate<DirectorCard> predicate)
		{
			for (int i = 0; i < monsterCategories.categories.Length; i++)
			{
				ref DirectorCardCategorySelection.Category reference = ref monsterCategories.categories[i];
				DirectorCard[] cards = reference.cards;
				float selectionWeight = reference.selectionWeight;
				for (int j = 0; j < cards.Length; j++)
				{
					DirectorCard directorCard2 = cards[j];
					if (predicate(directorCard2))
					{
						candidatesSelection.AddChoice(cards[j], selectionWeight * (float)directorCard2.selectionWeight);
					}
				}
			}
		}
		bool CardIsAffordable(DirectorCard card)
		{
			return Util.DirectorCardIsReasonableChoice(baseCredits, 5, 1, card, CombatDirector.CalcHighestEliteCostMultiplier(card.spawnCard.eliteRules));
		}
		static bool ReturnTrue(DirectorCard card)
		{
			return true;
		}
		void SetStageSingleMonsterType(Stage stage)
		{
			Stage.instance.singleMonsterTypeBodyIndex = bodyIndex;
			Stage.onServerStageBegin -= SetStageSingleMonsterType;
		}
	}

	private static void HandleMixEnemyArtifact(DirectorCardCategorySelection monsterCategories, Xoroshiro128Plus rng)
	{
		monsterCategories.CopyFrom(RoR2Content.mixEnemyMonsterCards);
		if (monsterCategories.categories.Length == 0)
		{
			Debug.LogError((object)"MixEnemy monster cards are size 0!");
		}
		TrimCategory("Basic Monsters", 3);
		TrimCategory("Minibosses", 3);
		TrimCategory("Champions", 3);
		void TrimCategory(string categoryName, int requiredCount)
		{
			DirectorCardCategorySelection.Category[] categories = monsterCategories.categories;
			for (int j = 0; j < categories.Length; j++)
			{
				if (string.CompareOrdinal(categoryName, categories[j].name) == 0)
				{
					Debug.LogFormat("Trimming {0} from {1} to {2}", new object[3]
					{
						categoryName,
						categories[j].cards.Length,
						requiredCount
					});
					TrimSelection(ref categories[j].cards, requiredCount);
				}
			}
		}
		void TrimSelection(ref DirectorCard[] cards, int requiredCount)
		{
			if (cards.Length > requiredCount)
			{
				DirectorCard[] array = ArrayUtils.Clone<DirectorCard>(cards);
				Util.ShuffleArray(array, rng);
				int num = array.Length - 1;
				while (num >= 0 && array.Length > requiredCount)
				{
					if (!array[num].IsAvailable())
					{
						ArrayUtils.ArrayRemoveAtAndResize<DirectorCard>(ref array, num, 1);
					}
					num--;
				}
				if (array.Length > requiredCount)
				{
					Array.Resize(ref array, requiredCount);
				}
				cards = array;
				DirectorCard[] array2 = cards;
				foreach (DirectorCard directorCard in array2)
				{
					Debug.LogFormat("Selected {0}", new object[1] { ((Object)directorCard.spawnCard).name });
				}
			}
		}
	}

	private static bool DirectorCardDoesNotForbidElite(DirectorCard directorCard)
	{
		CharacterSpawnCard characterSpawnCard = directorCard.spawnCard as CharacterSpawnCard;
		if (!Object.op_Implicit((Object)(object)characterSpawnCard))
		{
			return true;
		}
		return !characterSpawnCard.noElites;
	}

	private void Awake()
	{
		//IL_0023: Unknown result type (might be due to invalid IL or missing references)
		//IL_002d: Expected O, but got Unknown
		if (NetworkServer.active)
		{
			seedServer = Run.instance.stageRng.nextUlong;
			rng = new Xoroshiro128Plus(seedServer);
		}
	}

	private void Start()
	{
		RebuildCards();
		RunArtifactManager.onArtifactEnabledGlobal += OnArtifactEnabled;
		RunArtifactManager.onArtifactDisabledGlobal += OnArtifactDisabled;
	}

	private void OnDestroy()
	{
		RunArtifactManager.onArtifactEnabledGlobal -= OnArtifactEnabled;
		RunArtifactManager.onArtifactDisabledGlobal -= OnArtifactDisabled;
		if (Object.op_Implicit((Object)(object)modifiableMonsterCategories))
		{
			Object.Destroy((Object)(object)modifiableMonsterCategories);
		}
	}

	public IEnumerator BroadcastFamilySelection(string familySelectionChatString)
	{
		yield return (object)new WaitForSeconds(1f);
		Chat.SendBroadcastChat(new Chat.SimpleChatMessage
		{
			baseToken = familySelectionChatString
		});
	}

	private void OnEnable()
	{
		instance = this;
	}

	private void OnDisable()
	{
		instance = null;
	}

	private static float CalculateTotalWeight(DirectorCard[] cards)
	{
		float num = 0f;
		foreach (DirectorCard directorCard in cards)
		{
			num += (float)directorCard.selectionWeight;
		}
		return num;
	}

	private void RebuildCards()
	{
		//IL_0006: Unknown result type (might be due to invalid IL or missing references)
		//IL_000c: Expected O, but got Unknown
		//IL_0012: Unknown result type (might be due to invalid IL or missing references)
		//IL_0018: Expected O, but got Unknown
		//IL_001e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0024: Expected O, but got Unknown
		//IL_002a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0030: Expected O, but got Unknown
		//IL_0036: Unknown result type (might be due to invalid IL or missing references)
		//IL_003d: Expected O, but got Unknown
		Xoroshiro128Plus val = new Xoroshiro128Plus(seedServer);
		Xoroshiro128Plus val2 = new Xoroshiro128Plus(val.nextUlong);
		Xoroshiro128Plus val3 = new Xoroshiro128Plus(val.nextUlong);
		Xoroshiro128Plus val4 = new Xoroshiro128Plus(val.nextUlong);
		Xoroshiro128Plus val5 = new Xoroshiro128Plus(val.nextUlong);
		if (Object.op_Implicit((Object)(object)interactableDccsPool))
		{
			DirectorCardCategorySelection directorCardCategorySelection = interactableDccsPool.GenerateWeightedSelection().Evaluate(val5.nextNormalizedFloat);
			if ((Object)(object)directorCardCategorySelection != (Object)null)
			{
				directorCardCategorySelection.OnSelected(this);
				interactableCategories = directorCardCategorySelection;
			}
		}
		DirectorCardCategorySelection directorCardCategorySelection2 = null;
		if (Object.op_Implicit((Object)(object)monsterDccsPool))
		{
			DirectorCardCategorySelection directorCardCategorySelection3 = monsterDccsPool.GenerateWeightedSelection().Evaluate(val5.nextNormalizedFloat);
			if ((Object)(object)directorCardCategorySelection3 != (Object)null)
			{
				directorCardCategorySelection3.OnSelected(this);
				directorCardCategorySelection2 = Object.Instantiate<DirectorCardCategorySelection>(directorCardCategorySelection3);
			}
		}
		else if (Object.op_Implicit((Object)(object)monsterCategories))
		{
			directorCardCategorySelection2 = Object.Instantiate<DirectorCardCategorySelection>(monsterCategories);
		}
		if (!Object.op_Implicit((Object)(object)directorCardCategorySelection2))
		{
			return;
		}
		Object.Destroy((Object)(object)modifiableMonsterCategories);
		bool flag = RunArtifactManager.instance?.IsArtifactEnabled(RoR2Content.Artifacts.singleMonsterTypeArtifactDef) ?? false;
		bool num = RunArtifactManager.instance?.IsArtifactEnabled(RoR2Content.Artifacts.mixEnemyArtifactDef) ?? false;
		bool flag2 = RunArtifactManager.instance?.IsArtifactEnabled(RoR2Content.Artifacts.eliteOnlyArtifactDef) ?? false;
		modifiableMonsterCategories = directorCardCategorySelection2;
		if (num)
		{
			HandleMixEnemyArtifact(modifiableMonsterCategories, val3);
		}
		else if (!Object.op_Implicit((Object)(object)monsterDccsPool) && val4.nextNormalizedFloat <= monsterFamilyChance && possibleMonsterFamilies != null)
		{
			Run run = Run.instance;
			if (run == null || run.canFamilyEventTrigger)
			{
				Debug.Log((object)"Trying to find family selection...");
				WeightedSelection<MonsterFamily> weightedSelection = new WeightedSelection<MonsterFamily>();
				for (int i = 0; i < possibleMonsterFamilies.Length; i++)
				{
					MonsterFamily value = possibleMonsterFamilies[i];
					if ((Object)(object)Run.instance != (Object)null && value.minimumStageCompletion <= Run.instance.stageClearCount && value.maximumStageCompletion > Run.instance.stageClearCount)
					{
						weightedSelection.AddChoice(value, value.selectionWeight);
					}
				}
				if (weightedSelection.Count > 0)
				{
					MonsterFamily monsterFamily = weightedSelection.Evaluate(val.nextNormalizedFloat);
					modifiableMonsterCategories.CopyFrom(monsterFamily.monsterFamilyCategories);
					((MonoBehaviour)this).StartCoroutine("BroadcastFamilySelection", (object)monsterFamily.familySelectionChatString);
				}
			}
		}
		if (flag2)
		{
			modifiableMonsterCategories.RemoveCardsThatFailFilter(DirectorCardDoesNotForbidElite);
		}
		if (flag)
		{
			HandleSingleMonsterTypeArtifact(modifiableMonsterCategories, val2);
		}
		monsterSelection = modifiableMonsterCategories.GenerateDirectorCardWeightedSelection();
	}

	private void OnArtifactDisabled([NotNull] RunArtifactManager runArtifactManager, [NotNull] ArtifactDef artifactDef)
	{
		if (artifactDef == RoR2Content.Artifacts.mixEnemyArtifactDef || artifactDef == RoR2Content.Artifacts.singleMonsterTypeArtifactDef)
		{
			RebuildCards();
		}
	}

	private void OnArtifactEnabled([NotNull] RunArtifactManager runArtifactManager, [NotNull] ArtifactDef artifactDef)
	{
		if (artifactDef == RoR2Content.Artifacts.mixEnemyArtifactDef || artifactDef == RoR2Content.Artifacts.singleMonsterTypeArtifactDef)
		{
			RebuildCards();
		}
	}
}
