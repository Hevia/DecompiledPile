using System;
using UnityEngine;
using UnityEngine.Networking;

namespace RoR2;

[RequireComponent(typeof(PurchaseInteraction))]
public class ShrineChanceBehavior : NetworkBehaviour
{
	public int maxPurchaseCount;

	public float costMultiplierPerPurchase;

	public float failureChance;

	public PickupDropTable dropTable;

	public Transform symbolTransform;

	public Transform dropletOrigin;

	public Color shrineColor;

	private PurchaseInteraction purchaseInteraction;

	private int successfulPurchaseCount;

	private float refreshTimer;

	private const float refreshDuration = 2f;

	private bool waitingForRefresh;

	private Xoroshiro128Plus rng;

	[Header("Deprecated")]
	public float failureWeight;

	public float equipmentWeight;

	public float tier1Weight;

	public float tier2Weight;

	public float tier3Weight;

	public static event Action<bool, Interactor> onShrineChancePurchaseGlobal;

	private void Awake()
	{
		purchaseInteraction = ((Component)this).GetComponent<PurchaseInteraction>();
	}

	public void Start()
	{
		//IL_0017: Unknown result type (might be due to invalid IL or missing references)
		//IL_0021: Expected O, but got Unknown
		if (NetworkServer.active)
		{
			rng = new Xoroshiro128Plus(Run.instance.treasureRng.nextUlong);
		}
	}

	public void FixedUpdate()
	{
		if (waitingForRefresh)
		{
			refreshTimer -= Time.fixedDeltaTime;
			if (refreshTimer <= 0f && successfulPurchaseCount < maxPurchaseCount)
			{
				purchaseInteraction.SetAvailable(newAvailable: true);
				purchaseInteraction.Networkcost = (int)((float)purchaseInteraction.cost * costMultiplierPerPurchase);
				waitingForRefresh = false;
			}
		}
	}

	[Server]
	public void AddShrineStack(Interactor activator)
	{
		//IL_0145: Unknown result type (might be due to invalid IL or missing references)
		//IL_0150: Unknown result type (might be due to invalid IL or missing references)
		//IL_015a: Unknown result type (might be due to invalid IL or missing references)
		//IL_01bb: Unknown result type (might be due to invalid IL or missing references)
		//IL_01c6: Unknown result type (might be due to invalid IL or missing references)
		//IL_01cb: Unknown result type (might be due to invalid IL or missing references)
		//IL_01dd: Unknown result type (might be due to invalid IL or missing references)
		//IL_01e2: Unknown result type (might be due to invalid IL or missing references)
		//IL_01e7: Unknown result type (might be due to invalid IL or missing references)
		if (!NetworkServer.active)
		{
			Debug.LogWarning((object)"[Server] function 'System.Void RoR2.ShrineChanceBehavior::AddShrineStack(RoR2.Interactor)' called on client");
			return;
		}
		PickupIndex pickupIndex = PickupIndex.none;
		if (Object.op_Implicit((Object)(object)dropTable))
		{
			if (rng.nextNormalizedFloat > failureChance)
			{
				pickupIndex = dropTable.GenerateDrop(rng);
			}
		}
		else
		{
			PickupIndex none = PickupIndex.none;
			PickupIndex value = rng.NextElementUniform<PickupIndex>(Run.instance.availableTier1DropList);
			PickupIndex value2 = rng.NextElementUniform<PickupIndex>(Run.instance.availableTier2DropList);
			PickupIndex value3 = rng.NextElementUniform<PickupIndex>(Run.instance.availableTier3DropList);
			PickupIndex value4 = rng.NextElementUniform<PickupIndex>(Run.instance.availableEquipmentDropList);
			WeightedSelection<PickupIndex> weightedSelection = new WeightedSelection<PickupIndex>();
			weightedSelection.AddChoice(none, failureWeight);
			weightedSelection.AddChoice(value, tier1Weight);
			weightedSelection.AddChoice(value2, tier2Weight);
			weightedSelection.AddChoice(value3, tier3Weight);
			weightedSelection.AddChoice(value4, equipmentWeight);
			pickupIndex = weightedSelection.Evaluate(rng.nextNormalizedFloat);
		}
		bool flag = pickupIndex == PickupIndex.none;
		string baseToken;
		if (flag)
		{
			baseToken = "SHRINE_CHANCE_FAIL_MESSAGE";
		}
		else
		{
			baseToken = "SHRINE_CHANCE_SUCCESS_MESSAGE";
			successfulPurchaseCount++;
			PickupDropletController.CreatePickupDroplet(pickupIndex, dropletOrigin.position, dropletOrigin.forward * 20f);
		}
		Chat.SendBroadcastChat(new Chat.SubjectFormatChatMessage
		{
			subjectAsCharacterBody = ((Component)activator).GetComponent<CharacterBody>(),
			baseToken = baseToken
		});
		ShrineChanceBehavior.onShrineChancePurchaseGlobal?.Invoke(flag, activator);
		waitingForRefresh = true;
		refreshTimer = 2f;
		EffectManager.SpawnEffect(LegacyResourcesAPI.Load<GameObject>("Prefabs/Effects/ShrineUseEffect"), new EffectData
		{
			origin = ((Component)this).transform.position,
			rotation = Quaternion.identity,
			scale = 1f,
			color = Color32.op_Implicit(shrineColor)
		}, transmit: true);
		if (successfulPurchaseCount >= maxPurchaseCount)
		{
			((Component)symbolTransform).gameObject.SetActive(false);
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
