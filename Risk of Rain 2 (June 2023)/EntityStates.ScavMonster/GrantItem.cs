using RoR2;
using UnityEngine;
using UnityEngine.Networking;

namespace EntityStates.ScavMonster;

public class GrantItem : BaseState
{
	public PickupIndex dropPickup;

	public int itemsToGrant;

	public override void OnEnter()
	{
		base.OnEnter();
		if (NetworkServer.active)
		{
			GrantPickupServer(dropPickup, itemsToGrant);
		}
		if (base.isAuthority)
		{
			outer.SetNextState(new ExitSit());
		}
	}

	public override void OnSerialize(NetworkWriter writer)
	{
		base.OnSerialize(writer);
		writer.Write(dropPickup);
		writer.WritePackedUInt32((uint)itemsToGrant);
	}

	public override void OnDeserialize(NetworkReader reader)
	{
		base.OnDeserialize(reader);
		dropPickup = reader.ReadPickupIndex();
		itemsToGrant = (int)reader.ReadPackedUInt32();
	}

	private void GrantPickupServer(PickupIndex pickupIndex, int countToGrant)
	{
		//IL_005d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0062: Unknown result type (might be due to invalid IL or missing references)
		//IL_0067: Unknown result type (might be due to invalid IL or missing references)
		PickupDef pickupDef = PickupCatalog.GetPickupDef(pickupIndex);
		if (pickupDef != null)
		{
			ItemIndex itemIndex = pickupDef.itemIndex;
			if (!((Object)(object)ItemCatalog.GetItemDef(itemIndex) == (Object)null))
			{
				base.characterBody.inventory.GiveItem(itemIndex, countToGrant);
				Chat.SendBroadcastChat(new Chat.PlayerPickupChatMessage
				{
					subjectAsCharacterBody = base.characterBody,
					baseToken = "MONSTER_PICKUP",
					pickupToken = pickupDef.nameToken,
					pickupColor = Color32.op_Implicit(pickupDef.baseColor),
					pickupQuantity = (uint)itemsToGrant
				});
			}
		}
	}
}
