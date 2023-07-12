using System;
using HG;
using UnityEngine;
using UnityEngine.Networking;

namespace RoR2.Orbs;

public class ItemTransferOrb : Orb
{
	public ItemIndex itemIndex;

	public int stack;

	public Inventory inventoryToGrantTo;

	public Action<ItemTransferOrb> onArrival = DefaultOnArrivalBehavior;

	public NetworkIdentity orbEffectTargetObjectOverride;

	public float travelDuration = 1f;

	private static GameObject orbEffectPrefab;

	[RuntimeInitializeOnLoadMethod]
	private static void Init()
	{
		orbEffectPrefab = LegacyResourcesAPI.Load<GameObject>("Prefabs/Effects/OrbEffects/ItemTransferOrbEffect");
	}

	public override void Begin()
	{
		//IL_002d: Unknown result type (might be due to invalid IL or missing references)
		base.duration = travelDuration;
		if (Object.op_Implicit((Object)(object)target) || Object.op_Implicit((Object)(object)orbEffectTargetObjectOverride))
		{
			EffectData effectData = new EffectData
			{
				origin = origin,
				genericFloat = base.duration,
				genericUInt = Util.IntToUintPlusOne((int)itemIndex)
			};
			if (Object.op_Implicit((Object)(object)orbEffectTargetObjectOverride))
			{
				effectData.SetNetworkedObjectReference(((Component)orbEffectTargetObjectOverride).gameObject);
			}
			else
			{
				effectData.SetHurtBoxReference(target);
			}
			EffectManager.SpawnEffect(orbEffectPrefab, effectData, transmit: true);
		}
	}

	public override void OnArrival()
	{
		onArrival?.Invoke(this);
	}

	public static void DefaultOnArrivalBehavior(ItemTransferOrb orb)
	{
		if (Object.op_Implicit((Object)(object)orb.inventoryToGrantTo))
		{
			orb.inventoryToGrantTo.GiveItem(orb.itemIndex, orb.stack);
		}
	}

	public static ItemTransferOrb DispatchItemTransferOrb(Vector3 origin, Inventory inventoryToGrantTo, ItemIndex itemIndex, int itemStack, Action<ItemTransferOrb> onArrivalBehavior = null, Either<NetworkIdentity, HurtBox> orbDestinationOverride = default(Either<NetworkIdentity, HurtBox>))
	{
		//IL_0019: Unknown result type (might be due to invalid IL or missing references)
		//IL_001a: Unknown result type (might be due to invalid IL or missing references)
		//IL_003c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0044: Unknown result type (might be due to invalid IL or missing references)
		if (onArrivalBehavior == null)
		{
			onArrivalBehavior = DefaultOnArrivalBehavior;
		}
		ItemTransferOrb itemTransferOrb = new ItemTransferOrb();
		itemTransferOrb.origin = origin;
		itemTransferOrb.inventoryToGrantTo = inventoryToGrantTo;
		itemTransferOrb.itemIndex = itemIndex;
		itemTransferOrb.stack = itemStack;
		itemTransferOrb.onArrival = onArrivalBehavior;
		NetworkIdentity a = orbDestinationOverride.a;
		HurtBox b = orbDestinationOverride.b;
		if (!Object.op_Implicit((Object)(object)b))
		{
			if (Object.op_Implicit((Object)(object)inventoryToGrantTo))
			{
				CharacterMaster component = ((Component)inventoryToGrantTo).GetComponent<CharacterMaster>();
				if (Object.op_Implicit((Object)(object)component))
				{
					CharacterBody body = component.GetBody();
					if (Object.op_Implicit((Object)(object)body))
					{
						itemTransferOrb.target = body.mainHurtBox;
					}
				}
			}
			if (Object.op_Implicit((Object)(object)a))
			{
				itemTransferOrb.orbEffectTargetObjectOverride = a;
			}
		}
		else
		{
			itemTransferOrb.target = b;
		}
		OrbManager.instance.AddOrb(itemTransferOrb);
		return itemTransferOrb;
	}
}
