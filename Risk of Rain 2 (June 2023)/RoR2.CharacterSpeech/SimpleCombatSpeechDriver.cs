using System;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.Networking;

namespace RoR2.CharacterSpeech;

[RequireComponent(typeof(CharacterSpeechController))]
public class SimpleCombatSpeechDriver : BaseCharacterSpeechDriver
{
	[Serializable]
	public struct SpeechCategory
	{
		public string refName;

		public float chanceToTrigger;

		public SpeechInfo[] speechInfos;

		public WeightedSelection<SpeechInfo> speechSelection;
	}

	[Serializable]
	public struct SpeechInfo
	{
		public string nameToken;

		public float weight;
	}

	[Header("Speech Info")]
	public float minimumDurationBetweenSpeech;

	public bool removeRepeats;

	[SerializeField]
	public SpeechCategory[] speechCategories;

	[Tooltip("The refname to automatically submit speech for when damage is dealt.")]
	public string damageDealtRefName;

	[Header("Combat Parameters")]
	public bool scaleChanceOnDamageDealtByHealthFractionDealt = true;

	public float scaleChanceOnDamageDealtMultiplier = 1f;

	[Header("Events")]
	public DamageReportUnityEvent onBodyDamageDealt;

	public UnityEvent onStart;

	public DamageReportUnityEvent onBodyKill;

	public DamageReportUnityEvent onBodyDeath;

	private float lastSpeechStopwatch = float.PositiveInfinity;

	protected new void Awake()
	{
		base.Awake();
	}

	protected void Start()
	{
		if (NetworkServer.active)
		{
			Initialize();
			UnityEvent obj = onStart;
			if (obj != null)
			{
				obj.Invoke();
			}
		}
	}

	protected new void OnDestroy()
	{
		base.OnDestroy();
	}

	protected void FixedUpdate()
	{
		lastSpeechStopwatch += Time.fixedDeltaTime;
	}

	public void TriggerSpeech(string refName)
	{
		if (lastSpeechStopwatch < minimumDurationBetweenSpeech)
		{
			return;
		}
		int num = FindSpeechCategoryIndexByName(refName);
		if (num == -1)
		{
			Debug.LogWarningFormat("Speech category \"{0}\" could not be found.", new object[1] { refName });
			return;
		}
		ref SpeechCategory reference = ref speechCategories[num];
		if (Random.value < reference.chanceToTrigger)
		{
			SubmitSpeechRequestFromCategory(ref reference);
		}
	}

	private void SubmitSpeechRequestFromCategory(ref SpeechCategory category)
	{
		if (category.speechSelection == null || category.speechSelection.Count <= 0)
		{
			if (category.speechInfos.Length == 0)
			{
				return;
			}
			InitializeCategory(ref category);
		}
		int num = category.speechSelection.EvaluateToChoiceIndex(Random.value);
		SpeechInfo value = category.speechSelection.GetChoice(num).value;
		if (removeRepeats)
		{
			category.speechSelection.RemoveChoice(num);
		}
		lastSpeechStopwatch = 0f;
		CharacterSpeechController obj = base.characterSpeechController;
		CharacterSpeechController.SpeechInfo speechInfo = new CharacterSpeechController.SpeechInfo
		{
			token = value.nameToken,
			duration = minimumDurationBetweenSpeech,
			maxWait = minimumDurationBetweenSpeech,
			mustPlay = false
		};
		obj.EnqueueSpeech(in speechInfo);
	}

	private int FindSpeechCategoryIndexByName(string refName)
	{
		for (int i = 0; i < speechCategories.Length; i++)
		{
			if (string.Equals(speechCategories[i].refName, refName, StringComparison.Ordinal))
			{
				return i;
			}
		}
		return -1;
	}

	private void Initialize()
	{
		for (int i = 0; i < speechCategories.Length; i++)
		{
			InitializeCategory(ref speechCategories[i]);
		}
	}

	private void InitializeCategory(ref SpeechCategory speechCategory)
	{
		WeightedSelection<SpeechInfo> weightedSelection = new WeightedSelection<SpeechInfo>();
		for (int i = 0; i < speechCategory.speechInfos.Length; i++)
		{
			SpeechInfo value = speechCategory.speechInfos[i];
			weightedSelection.AddChoice(value, value.weight);
		}
		speechCategory.speechSelection = weightedSelection;
	}

	protected override void OnCharacterBodyDiscovered(CharacterBody characterBody)
	{
		base.OnCharacterBodyDiscovered(characterBody);
		GlobalEventManager.onServerDamageDealt += OnServerDamageDealt;
		GlobalEventManager.onCharacterDeathGlobal += OnCharacterDeathGlobal;
	}

	protected override void OnCharacterBodyLost(CharacterBody characterBody)
	{
		GlobalEventManager.onCharacterDeathGlobal -= OnCharacterDeathGlobal;
		GlobalEventManager.onServerDamageDealt -= OnServerDamageDealt;
		base.OnCharacterBodyLost(characterBody);
	}

	private void OnServerDamageDealt(DamageReport damageReport)
	{
		if (damageReport.attackerBody == base.currentCharacterBody)
		{
			OnBodyDamageDealt(damageReport);
		}
		if (damageReport.victimBody == base.currentCharacterBody)
		{
			OnBodyDamageTaken(damageReport);
		}
	}

	private void OnCharacterDeathGlobal(DamageReport damageReport)
	{
		if (damageReport.attackerBody == base.currentCharacterBody)
		{
			OnBodyKill(damageReport);
		}
		if (damageReport.victimBody == base.currentCharacterBody)
		{
			OnBodyDeath(damageReport);
		}
	}

	private void OnBodyDamageDealt(DamageReport damageReport)
	{
		if (scaleChanceOnDamageDealtByHealthFractionDealt && Object.op_Implicit((Object)(object)damageReport.victim) && !string.IsNullOrEmpty(damageDealtRefName))
		{
			int num = FindSpeechCategoryIndexByName(damageDealtRefName);
			if (num == -1)
			{
				Debug.LogWarningFormat("Speech category \"{0}\" could not be found.", new object[1] { damageDealtRefName });
			}
			else
			{
				ref SpeechCategory reference = ref speechCategories[num];
				float num2 = reference.chanceToTrigger;
				if (scaleChanceOnDamageDealtByHealthFractionDealt)
				{
					float fullCombinedHealth = damageReport.victim.fullCombinedHealth;
					float num3 = damageReport.damageDealt / fullCombinedHealth;
					num2 *= num3;
				}
				num2 *= scaleChanceOnDamageDealtMultiplier;
				if (Random.value < num2)
				{
					SubmitSpeechRequestFromCategory(ref reference);
				}
			}
		}
		((UnityEvent<DamageReport>)onBodyDamageDealt)?.Invoke(damageReport);
	}

	private void OnBodyDamageTaken(DamageReport damageReport)
	{
	}

	private void OnBodyKill(DamageReport damageReport)
	{
		((UnityEvent<DamageReport>)onBodyKill)?.Invoke(damageReport);
	}

	private void OnBodyDeath(DamageReport damageReport)
	{
		((UnityEvent<DamageReport>)onBodyDeath)?.Invoke(damageReport);
	}
}
