using System;
using RoR2.Networking;
using UnityEngine;
using UnityEngine.Networking;

namespace RoR2;

[RequireComponent(typeof(PurchaseInteraction))]
public class ShrineHealingBehavior : NetworkBehaviour
{
	public GameObject wardPrefab;

	private GameObject wardInstance;

	public float baseRadius;

	public float radiusBonusPerPurchase;

	public int maxPurchaseCount;

	public float costMultiplierPerPurchase;

	public Transform symbolTransform;

	private PurchaseInteraction purchaseInteraction;

	private float refreshTimer;

	private const float refreshDuration = 2f;

	private bool waitingForRefresh;

	private HealingWard healingWard;

	public int purchaseCount { get; private set; }

	public static event Action<ShrineHealingBehavior, Interactor> onActivated;

	public override int GetNetworkChannel()
	{
		return QosChannelIndex.defaultReliable.intVal;
	}

	private void Awake()
	{
		purchaseInteraction = ((Component)this).GetComponent<PurchaseInteraction>();
	}

	public void FixedUpdate()
	{
		if (waitingForRefresh)
		{
			refreshTimer -= Time.fixedDeltaTime;
			if (refreshTimer <= 0f && purchaseCount < maxPurchaseCount)
			{
				purchaseInteraction.SetAvailable(newAvailable: true);
				purchaseInteraction.Networkcost = (int)((float)purchaseInteraction.cost * costMultiplierPerPurchase);
				waitingForRefresh = false;
			}
		}
	}

	[Server]
	private void SetWardEnabled(bool enableWard)
	{
		//IL_0033: Unknown result type (might be due to invalid IL or missing references)
		//IL_003e: Unknown result type (might be due to invalid IL or missing references)
		if (!NetworkServer.active)
		{
			Debug.LogWarning((object)"[Server] function 'System.Void RoR2.ShrineHealingBehavior::SetWardEnabled(System.Boolean)' called on client");
		}
		else if (enableWard != Object.op_Implicit((Object)(object)wardInstance))
		{
			if (enableWard)
			{
				wardInstance = Object.Instantiate<GameObject>(wardPrefab, ((Component)this).transform.position, ((Component)this).transform.rotation);
				wardInstance.GetComponent<TeamFilter>().teamIndex = TeamIndex.Player;
				healingWard = wardInstance.GetComponent<HealingWard>();
				NetworkServer.Spawn(wardInstance);
			}
			else
			{
				Object.Destroy((Object)(object)wardInstance);
				wardInstance = null;
				healingWard = null;
			}
		}
	}

	[Server]
	public void AddShrineStack(Interactor activator)
	{
		//IL_009e: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a9: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ae: Unknown result type (might be due to invalid IL or missing references)
		//IL_00bf: Unknown result type (might be due to invalid IL or missing references)
		//IL_00c4: Unknown result type (might be due to invalid IL or missing references)
		//IL_00c9: Unknown result type (might be due to invalid IL or missing references)
		if (!NetworkServer.active)
		{
			Debug.LogWarning((object)"[Server] function 'System.Void RoR2.ShrineHealingBehavior::AddShrineStack(RoR2.Interactor)' called on client");
			return;
		}
		SetWardEnabled(enableWard: true);
		Chat.SendBroadcastChat(new Chat.SubjectFormatChatMessage
		{
			subjectAsCharacterBody = ((Component)activator).gameObject.GetComponent<CharacterBody>(),
			baseToken = "SHRINE_HEALING_USE_MESSAGE"
		});
		waitingForRefresh = true;
		purchaseCount++;
		float networkradius = baseRadius + radiusBonusPerPurchase * (float)(purchaseCount - 1);
		healingWard.Networkradius = networkradius;
		refreshTimer = 2f;
		EffectManager.SpawnEffect(LegacyResourcesAPI.Load<GameObject>("Prefabs/Effects/ShrineUseEffect"), new EffectData
		{
			origin = ((Component)this).transform.position,
			rotation = Quaternion.identity,
			scale = 1f,
			color = Color32.op_Implicit(Color.green)
		}, transmit: true);
		if (purchaseCount >= maxPurchaseCount)
		{
			((Component)symbolTransform).gameObject.SetActive(false);
		}
		ShrineHealingBehavior.onActivated?.Invoke(this, activator);
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
