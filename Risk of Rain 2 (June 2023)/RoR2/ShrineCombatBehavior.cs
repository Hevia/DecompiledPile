using System;
using RoR2.Networking;
using UnityEngine;
using UnityEngine.Networking;

namespace RoR2;

[RequireComponent(typeof(PurchaseInteraction))]
[RequireComponent(typeof(CombatDirector))]
[RequireComponent(typeof(CombatSquad))]
public class ShrineCombatBehavior : NetworkBehaviour
{
	public Color shrineEffectColor;

	public int maxPurchaseCount;

	public int baseMonsterCredit;

	public float monsterCreditCoefficientPerPurchase;

	public Transform symbolTransform;

	public GameObject spawnPositionEffectPrefab;

	private CombatDirector combatDirector;

	private PurchaseInteraction purchaseInteraction;

	private int purchaseCount;

	private float refreshTimer;

	private const float refreshDuration = 2f;

	private bool waitingForRefresh;

	private DirectorCard chosenDirectorCard;

	private float monsterCredit => (float)baseMonsterCredit * Stage.instance.entryDifficultyCoefficient * (1f + (float)purchaseCount * (monsterCreditCoefficientPerPurchase - 1f));

	public static event Action<ShrineCombatBehavior> onDefeatedServerGlobal;

	public override int GetNetworkChannel()
	{
		return QosChannelIndex.defaultReliable.intVal;
	}

	private void Awake()
	{
		if (NetworkServer.active)
		{
			purchaseInteraction = ((Component)this).GetComponent<PurchaseInteraction>();
			combatDirector = ((Component)this).GetComponent<CombatDirector>();
			combatDirector.combatSquad.onDefeatedServer += OnDefeatedServer;
		}
	}

	private void OnDefeatedServer()
	{
		ShrineCombatBehavior.onDefeatedServerGlobal?.Invoke(this);
	}

	private void Start()
	{
		if (NetworkServer.active)
		{
			chosenDirectorCard = combatDirector.SelectMonsterCardForCombatShrine(monsterCredit);
			if (chosenDirectorCard == null)
			{
				Debug.Log((object)"Could not find appropriate spawn card for Combat Shrine");
				purchaseInteraction.SetAvailable(newAvailable: false);
			}
		}
	}

	public void FixedUpdate()
	{
		if (waitingForRefresh)
		{
			refreshTimer -= Time.fixedDeltaTime;
			if (refreshTimer <= 0f && purchaseCount < maxPurchaseCount)
			{
				purchaseInteraction.SetAvailable(newAvailable: true);
				waitingForRefresh = false;
			}
		}
	}

	[Server]
	public void AddShrineStack(Interactor interactor)
	{
		//IL_004a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0055: Unknown result type (might be due to invalid IL or missing references)
		//IL_005a: Unknown result type (might be due to invalid IL or missing references)
		//IL_006c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0071: Unknown result type (might be due to invalid IL or missing references)
		//IL_0076: Unknown result type (might be due to invalid IL or missing references)
		if (!NetworkServer.active)
		{
			Debug.LogWarning((object)"[Server] function 'System.Void RoR2.ShrineCombatBehavior::AddShrineStack(RoR2.Interactor)' called on client");
			return;
		}
		waitingForRefresh = true;
		combatDirector.CombatShrineActivation(interactor, monsterCredit, chosenDirectorCard);
		EffectManager.SpawnEffect(LegacyResourcesAPI.Load<GameObject>("Prefabs/Effects/ShrineUseEffect"), new EffectData
		{
			origin = ((Component)this).transform.position,
			rotation = Quaternion.identity,
			scale = 1f,
			color = Color32.op_Implicit(shrineEffectColor)
		}, transmit: true);
		purchaseCount++;
		refreshTimer = 2f;
		if (purchaseCount >= maxPurchaseCount)
		{
			((Component)symbolTransform).gameObject.SetActive(false);
		}
	}

	private void OnValidate()
	{
		if (!Object.op_Implicit((Object)(object)((Component)this).GetComponent<CombatDirector>().combatSquad))
		{
			Debug.LogError((object)"ShrineCombatBehavior's sibling CombatDirector must use a CombatSquad.", (Object)(object)((Component)this).gameObject);
		}
	}

	private void UNetVersion()
	{
	}

	public override bool OnSerialize(NetworkWriter writer, bool forceAll)
	{
		bool result = default(bool);
		return result;
	}

	public override void OnDeserialize(NetworkReader reader, bool initialState)
	{
	}

	public override void PreStartClient()
	{
	}
}
