using System.Collections.Generic;
using RoR2.Items;
using RoR2.Networking;
using Unity;
using UnityEngine;
using UnityEngine.Networking;

namespace RoR2;

[RequireComponent(typeof(PurchaseInteraction))]
public class VoidSuppressorBehavior : NetworkBehaviour
{
	public class SyncListPickupIndex : SyncListStruct<PickupIndex>
	{
		public override void SerializeItem(NetworkWriter writer, PickupIndex item)
		{
			writer.WritePackedUInt32((uint)item.value);
		}

		public override PickupIndex DeserializeItem(NetworkReader reader)
		{
			PickupIndex result = default(PickupIndex);
			result.value = (int)reader.ReadPackedUInt32();
			return result;
		}
	}

	[SerializeField]
	private int maxPurchaseCount;

	[SerializeField]
	private int itemsSuppressedPerPurchase;

	[SerializeField]
	private float costMultiplierPerPurchase;

	[SerializeField]
	private Transform symbolTransform;

	[SerializeField]
	private PurchaseInteraction purchaseInteraction;

	[SerializeField]
	private GameObject effectPrefab;

	[SerializeField]
	private Color effectColor;

	[SerializeField]
	private PickupDisplay[] pickupDisplays;

	[SerializeField]
	private int numItemsToReveal;

	[SerializeField]
	private bool transformItems;

	[SerializeField]
	private string animatorSuppressName;

	[SerializeField]
	private string animatorIsAvailableName;

	[SerializeField]
	private string animatorIsPlayerNearbyName;

	[SerializeField]
	private Animator animator;

	[SerializeField]
	private NetworkAnimator networkAnimator;

	[SerializeField]
	private string revealedName;

	[SerializeField]
	private string revealedContext;

	[SerializeField]
	private float useRefreshDelay = 2f;

	[SerializeField]
	private float itemRefreshDelay = 1f;

	private int purchaseCount;

	private float timeUntilUseRefresh;

	private float timeUntilItemRefresh;

	private Xoroshiro128Plus rng;

	private SyncListPickupIndex nextItemsToSuppress = new SyncListPickupIndex();

	private ItemIndex transformItemIndex;

	private bool hasRevealed;

	private static int kListnextItemsToSuppress;

	public override int GetNetworkChannel()
	{
		return QosChannelIndex.defaultReliable.intVal;
	}

	private void Start()
	{
		//IL_0017: Unknown result type (might be due to invalid IL or missing references)
		//IL_0021: Expected O, but got Unknown
		if (NetworkServer.active)
		{
			rng = new Xoroshiro128Plus(Run.instance.treasureRng.nextUlong);
		}
	}

	public override void OnStartClient()
	{
		((SyncList<PickupIndex>)(object)nextItemsToSuppress).Callback = OnItemsUpdated;
	}

	public void FixedUpdate()
	{
		if (purchaseCount >= maxPurchaseCount)
		{
			return;
		}
		if (timeUntilUseRefresh > 0f)
		{
			timeUntilUseRefresh -= Time.fixedDeltaTime;
			if (timeUntilUseRefresh <= 0f)
			{
				animator.SetBool(animatorSuppressName, false);
				purchaseInteraction.SetAvailable(newAvailable: true);
				purchaseInteraction.Networkcost = (int)((float)purchaseInteraction.cost * costMultiplierPerPurchase);
			}
		}
		if (timeUntilItemRefresh > 0f)
		{
			timeUntilItemRefresh -= Time.fixedDeltaTime;
			if (timeUntilItemRefresh <= 0f)
			{
				((SyncList<PickupIndex>)(object)nextItemsToSuppress).Clear();
				RefreshItems();
			}
		}
	}

	[Server]
	public void OnInteraction(Interactor interactor)
	{
		//IL_00cd: Unknown result type (might be due to invalid IL or missing references)
		//IL_00d2: Unknown result type (might be due to invalid IL or missing references)
		//IL_0113: Unknown result type (might be due to invalid IL or missing references)
		//IL_011e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0123: Unknown result type (might be due to invalid IL or missing references)
		//IL_0135: Unknown result type (might be due to invalid IL or missing references)
		//IL_013a: Unknown result type (might be due to invalid IL or missing references)
		//IL_013f: Unknown result type (might be due to invalid IL or missing references)
		if (!NetworkServer.active)
		{
			Debug.LogWarning((object)"[Server] function 'System.Void RoR2.VoidSuppressorBehavior::OnInteraction(RoR2.Interactor)' called on client");
			return;
		}
		timeUntilUseRefresh = useRefreshDelay;
		if (hasRevealed)
		{
			timeUntilItemRefresh = itemRefreshDelay;
			if (nextItemsToSuppress != null)
			{
				CharacterBody component = ((Component)interactor).GetComponent<CharacterBody>();
				for (int i = 0; i < itemsSuppressedPerPurchase && i < ((SyncListStruct<PickupIndex>)nextItemsToSuppress).Count; i++)
				{
					ItemIndex itemIndex = ((SyncList<PickupIndex>)(object)nextItemsToSuppress)[i].itemIndex;
					if (SuppressedItemManager.SuppressItem(itemIndex, transformItemIndex))
					{
						ItemDef itemDef = ItemCatalog.GetItemDef(itemIndex);
						ItemTierDef itemTierDef = ItemTierCatalog.GetItemTierDef(itemDef.tier);
						ColoredTokenChatMessage coloredTokenChatMessage = new ColoredTokenChatMessage();
						coloredTokenChatMessage.subjectAsCharacterBody = component;
						coloredTokenChatMessage.baseToken = "VOID_SUPPRESSOR_USE_MESSAGE";
						coloredTokenChatMessage.paramTokens = new string[1] { itemDef.nameToken };
						coloredTokenChatMessage.paramColors = (Color32[])(object)new Color32[1] { ColorCatalog.GetColor(itemTierDef.colorIndex) };
						Chat.SendBroadcastChat(coloredTokenChatMessage);
					}
				}
			}
			EffectManager.SpawnEffect(effectPrefab, new EffectData
			{
				origin = ((Component)this).transform.position,
				rotation = Quaternion.identity,
				scale = 1f,
				color = Color32.op_Implicit(effectColor)
			}, transmit: true);
			purchaseCount++;
			if (purchaseCount >= maxPurchaseCount)
			{
				if (Object.op_Implicit((Object)(object)symbolTransform))
				{
					((Component)symbolTransform).gameObject.SetActive(false);
				}
				animator.SetBool(animatorIsAvailableName, false);
			}
			animator.SetBool(animatorSuppressName, true);
		}
		else
		{
			hasRevealed = true;
			animator.SetBool(animatorIsAvailableName, true);
			purchaseInteraction.NetworkcontextToken = revealedContext;
			purchaseInteraction.NetworkdisplayNameToken = revealedName;
			RefreshItems();
		}
	}

	[Server]
	public void RefreshItems()
	{
		if (!NetworkServer.active)
		{
			Debug.LogWarning((object)"[Server] function 'System.Void RoR2.VoidSuppressorBehavior::RefreshItems()' called on client");
		}
		else
		{
			if (timeUntilItemRefresh > 0f)
			{
				return;
			}
			for (int num = ((SyncListStruct<PickupIndex>)nextItemsToSuppress).Count - 1; num >= 0; num--)
			{
				if (SuppressedItemManager.HasItemBeenSuppressed(((SyncList<PickupIndex>)(object)nextItemsToSuppress)[num].itemIndex))
				{
					((SyncList<PickupIndex>)(object)nextItemsToSuppress).RemoveAt(num);
				}
			}
			int num2 = itemsSuppressedPerPurchase - ((SyncListStruct<PickupIndex>)nextItemsToSuppress).Count;
			if (num2 > 0)
			{
				List<PickupIndex> list = null;
				PickupIndex item = PickupIndex.none;
				switch (purchaseCount)
				{
				case 0:
					list = new List<PickupIndex>(Run.instance.availableTier1DropList);
					item = PickupCatalog.FindPickupIndex(DLC1Content.Items.ScrapWhiteSuppressed.itemIndex);
					transformItemIndex = (transformItems ? DLC1Content.Items.ScrapWhiteSuppressed.itemIndex : ItemIndex.None);
					break;
				case 1:
					list = new List<PickupIndex>(Run.instance.availableTier2DropList);
					item = PickupCatalog.FindPickupIndex(DLC1Content.Items.ScrapGreenSuppressed.itemIndex);
					transformItemIndex = (transformItems ? DLC1Content.Items.ScrapGreenSuppressed.itemIndex : ItemIndex.None);
					break;
				case 2:
					list = new List<PickupIndex>(Run.instance.availableTier3DropList);
					item = PickupCatalog.FindPickupIndex(DLC1Content.Items.ScrapRedSuppressed.itemIndex);
					transformItemIndex = (transformItems ? DLC1Content.Items.ScrapRedSuppressed.itemIndex : ItemIndex.None);
					break;
				}
				if (list != null && list.Count > 0)
				{
					list.Remove(item);
					foreach (PickupIndex item2 in (SyncList<PickupIndex>)(object)nextItemsToSuppress)
					{
						list.Remove(item2);
					}
					Util.ShuffleList(list, rng);
					int num3 = list.Count - num2;
					if (num3 > 0)
					{
						list.RemoveRange(num2, num3);
					}
					foreach (PickupIndex item3 in list)
					{
						((SyncList<PickupIndex>)(object)nextItemsToSuppress).Add(item3);
					}
				}
			}
			RefreshPickupDisplays();
			if (((SyncListStruct<PickupIndex>)nextItemsToSuppress).Count != 0)
			{
				return;
			}
			if (purchaseCount < maxPurchaseCount)
			{
				purchaseCount++;
				RefreshItems();
				return;
			}
			if (Object.op_Implicit((Object)(object)symbolTransform))
			{
				((Component)symbolTransform).gameObject.SetActive(false);
			}
			purchaseInteraction.SetAvailable(newAvailable: false);
			animator.SetBool(animatorIsAvailableName, false);
		}
	}

	public void OnPlayerNearby()
	{
		animator.SetBool(animatorIsPlayerNearbyName, true);
	}

	public void OnPlayerFar()
	{
		animator.SetBool(animatorIsPlayerNearbyName, false);
	}

	private void OnItemsUpdated(Operation<PickupIndex> op, int index)
	{
		RefreshPickupDisplays();
	}

	private void RefreshPickupDisplays()
	{
		for (int i = 0; i < pickupDisplays.Length; i++)
		{
			if (i < ((SyncListStruct<PickupIndex>)nextItemsToSuppress).Count)
			{
				((Component)pickupDisplays[i]).gameObject.SetActive(true);
				pickupDisplays[i].SetPickupIndex(((SyncList<PickupIndex>)(object)nextItemsToSuppress)[i], i >= numItemsToReveal);
			}
			else
			{
				((Component)pickupDisplays[i]).gameObject.SetActive(false);
			}
		}
	}

	private void UNetVersion()
	{
	}

	protected static void InvokeSyncListnextItemsToSuppress(NetworkBehaviour obj, NetworkReader reader)
	{
		if (!NetworkClient.active)
		{
			Debug.LogError((object)"SyncList nextItemsToSuppress called on server.");
		}
		else
		{
			((SyncList<PickupIndex>)(object)((VoidSuppressorBehavior)(object)obj).nextItemsToSuppress).HandleMsg(reader);
		}
	}

	static VoidSuppressorBehavior()
	{
		//IL_0020: Unknown result type (might be due to invalid IL or missing references)
		//IL_002a: Expected O, but got Unknown
		kListnextItemsToSuppress = 164192093;
		NetworkBehaviour.RegisterSyncListDelegate(typeof(VoidSuppressorBehavior), kListnextItemsToSuppress, new CmdDelegate(InvokeSyncListnextItemsToSuppress));
		NetworkCRC.RegisterBehaviour("VoidSuppressorBehavior", 0);
	}

	private void Awake()
	{
		((SyncList<PickupIndex>)(object)nextItemsToSuppress).InitializeBehaviour((NetworkBehaviour)(object)this, kListnextItemsToSuppress);
	}

	public override bool OnSerialize(NetworkWriter writer, bool forceAll)
	{
		if (forceAll)
		{
			GeneratedNetworkCode._WriteStructSyncListPickupIndex_VoidSuppressorBehavior(writer, nextItemsToSuppress);
			return true;
		}
		bool flag = false;
		if ((((NetworkBehaviour)this).syncVarDirtyBits & (true ? 1u : 0u)) != 0)
		{
			if (!flag)
			{
				writer.WritePackedUInt32(((NetworkBehaviour)this).syncVarDirtyBits);
				flag = true;
			}
			GeneratedNetworkCode._WriteStructSyncListPickupIndex_VoidSuppressorBehavior(writer, nextItemsToSuppress);
		}
		if (!flag)
		{
			writer.WritePackedUInt32(((NetworkBehaviour)this).syncVarDirtyBits);
		}
		return flag;
	}

	public override void OnDeserialize(NetworkReader reader, bool initialState)
	{
		if (initialState)
		{
			GeneratedNetworkCode._ReadStructSyncListPickupIndex_VoidSuppressorBehavior(reader, nextItemsToSuppress);
			return;
		}
		int num = (int)reader.ReadPackedUInt32();
		if (((uint)num & (true ? 1u : 0u)) != 0)
		{
			GeneratedNetworkCode._ReadStructSyncListPickupIndex_VoidSuppressorBehavior(reader, nextItemsToSuppress);
		}
	}

	public override void PreStartClient()
	{
	}
}
