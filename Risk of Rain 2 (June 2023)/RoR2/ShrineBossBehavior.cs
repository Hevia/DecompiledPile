using RoR2.Networking;
using UnityEngine;
using UnityEngine.Networking;

namespace RoR2;

[RequireComponent(typeof(PurchaseInteraction))]
public class ShrineBossBehavior : NetworkBehaviour
{
	public int maxPurchaseCount;

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
				purchaseInteraction.Networkcost = (int)((float)purchaseInteraction.cost * costMultiplierPerPurchase);
				waitingForRefresh = false;
			}
		}
	}

	[Server]
	public void AddShrineStack(Interactor interactor)
	{
		//IL_006b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0076: Unknown result type (might be due to invalid IL or missing references)
		//IL_007b: Unknown result type (might be due to invalid IL or missing references)
		//IL_009b: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a0: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a5: Unknown result type (might be due to invalid IL or missing references)
		if (!NetworkServer.active)
		{
			Debug.LogWarning((object)"[Server] function 'System.Void RoR2.ShrineBossBehavior::AddShrineStack(RoR2.Interactor)' called on client");
			return;
		}
		waitingForRefresh = true;
		if (Object.op_Implicit((Object)(object)TeleporterInteraction.instance))
		{
			TeleporterInteraction.instance.AddShrineStack();
		}
		CharacterBody component = ((Component)interactor).GetComponent<CharacterBody>();
		Chat.SendBroadcastChat(new Chat.SubjectFormatChatMessage
		{
			subjectAsCharacterBody = component,
			baseToken = "SHRINE_BOSS_USE_MESSAGE"
		});
		EffectManager.SpawnEffect(LegacyResourcesAPI.Load<GameObject>("Prefabs/Effects/ShrineUseEffect"), new EffectData
		{
			origin = ((Component)this).transform.position,
			rotation = Quaternion.identity,
			scale = 1f,
			color = Color32.op_Implicit(new Color(0.7372549f, 77f / 85f, 0.94509804f))
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
