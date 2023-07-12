using RoR2.Networking;
using UnityEngine;
using UnityEngine.Networking;

namespace RoR2;

[RequireComponent(typeof(PurchaseInteraction))]
public class ShrineRestackBehavior : NetworkBehaviour
{
	public int maxPurchaseCount;

	public float costMultiplierPerPurchase;

	public Transform symbolTransform;

	private PurchaseInteraction purchaseInteraction;

	private int purchaseCount;

	private float refreshTimer;

	private const float refreshDuration = 2f;

	private bool waitingForRefresh;

	private Xoroshiro128Plus rng;

	public override int GetNetworkChannel()
	{
		return QosChannelIndex.defaultReliable.intVal;
	}

	private void Start()
	{
		//IL_0023: Unknown result type (might be due to invalid IL or missing references)
		//IL_002d: Expected O, but got Unknown
		purchaseInteraction = ((Component)this).GetComponent<PurchaseInteraction>();
		if (NetworkServer.active)
		{
			rng = new Xoroshiro128Plus(Run.instance.stageRng.nextUlong);
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
				purchaseInteraction.Networkcost = (int)((float)purchaseInteraction.cost * costMultiplierPerPurchase);
				waitingForRefresh = false;
			}
		}
	}

	[Server]
	public void AddShrineStack(Interactor interactor)
	{
		//IL_008a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0095: Unknown result type (might be due to invalid IL or missing references)
		//IL_009a: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ba: Unknown result type (might be due to invalid IL or missing references)
		//IL_00bf: Unknown result type (might be due to invalid IL or missing references)
		//IL_00c4: Unknown result type (might be due to invalid IL or missing references)
		if (!NetworkServer.active)
		{
			Debug.LogWarning((object)"[Server] function 'System.Void RoR2.ShrineRestackBehavior::AddShrineStack(RoR2.Interactor)' called on client");
			return;
		}
		waitingForRefresh = true;
		CharacterBody component = ((Component)interactor).GetComponent<CharacterBody>();
		if (Object.op_Implicit((Object)(object)component) && Object.op_Implicit((Object)(object)component.master))
		{
			Inventory inventory = component.master.inventory;
			if (Object.op_Implicit((Object)(object)inventory))
			{
				inventory.ShrineRestackInventory(rng);
				Chat.SendBroadcastChat(new Chat.SubjectFormatChatMessage
				{
					subjectAsCharacterBody = component,
					baseToken = "SHRINE_RESTACK_USE_MESSAGE"
				});
			}
		}
		EffectManager.SpawnEffect(LegacyResourcesAPI.Load<GameObject>("Prefabs/Effects/ShrineUseEffect"), new EffectData
		{
			origin = ((Component)this).transform.position,
			rotation = Quaternion.identity,
			scale = 1f,
			color = Color32.op_Implicit(new Color(1f, 0.23f, 0.6337214f))
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
