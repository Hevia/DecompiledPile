using RoR2.Networking;
using UnityEngine;
using UnityEngine.Networking;

namespace RoR2;

[RequireComponent(typeof(PurchaseInteraction))]
public class ShrineBloodBehavior : NetworkBehaviour
{
	public int maxPurchaseCount;

	public float goldToPaidHpRatio = 0.5f;

	public float costMultiplierPerPurchase;

	public Transform symbolTransform;

	private PurchaseInteraction purchaseInteraction;

	private int purchaseCount;

	private float refreshTimer;

	private const float refreshDuration = 2f;

	private bool waitingForRefresh;

	public override int GetNetworkChannel()
	{
		return QosChannelIndex.defaultReliable.intVal;
	}

	private void Start()
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
				purchaseInteraction.Networkcost = (int)(100f * (1f - Mathf.Pow(1f - (float)purchaseInteraction.cost / 100f, costMultiplierPerPurchase)));
				waitingForRefresh = false;
			}
		}
	}

	[Server]
	public void AddShrineStack(Interactor interactor)
	{
		//IL_00b5: Unknown result type (might be due to invalid IL or missing references)
		//IL_00c0: Unknown result type (might be due to invalid IL or missing references)
		//IL_00c5: Unknown result type (might be due to invalid IL or missing references)
		//IL_00d6: Unknown result type (might be due to invalid IL or missing references)
		//IL_00db: Unknown result type (might be due to invalid IL or missing references)
		//IL_00e0: Unknown result type (might be due to invalid IL or missing references)
		if (!NetworkServer.active)
		{
			Debug.LogWarning((object)"[Server] function 'System.Void RoR2.ShrineBloodBehavior::AddShrineStack(RoR2.Interactor)' called on client");
			return;
		}
		waitingForRefresh = true;
		CharacterBody component = ((Component)interactor).GetComponent<CharacterBody>();
		if (Object.op_Implicit((Object)(object)component))
		{
			uint amount = (uint)(component.healthComponent.fullCombinedHealth * (float)purchaseInteraction.cost / 100f * goldToPaidHpRatio);
			if (Object.op_Implicit((Object)(object)component.master))
			{
				component.master.GiveMoney(amount);
				Chat.SubjectFormatChatMessage subjectFormatChatMessage = new Chat.SubjectFormatChatMessage();
				subjectFormatChatMessage.subjectAsCharacterBody = component;
				subjectFormatChatMessage.baseToken = "SHRINE_BLOOD_USE_MESSAGE";
				subjectFormatChatMessage.paramTokens = new string[1] { amount.ToString() };
				Chat.SendBroadcastChat(subjectFormatChatMessage);
			}
		}
		EffectManager.SpawnEffect(LegacyResourcesAPI.Load<GameObject>("Prefabs/Effects/ShrineUseEffect"), new EffectData
		{
			origin = ((Component)this).transform.position,
			rotation = Quaternion.identity,
			scale = 1f,
			color = Color32.op_Implicit(Color.red)
		}, transmit: true);
		purchaseCount++;
		refreshTimer = 2f;
		if (purchaseCount >= maxPurchaseCount)
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
