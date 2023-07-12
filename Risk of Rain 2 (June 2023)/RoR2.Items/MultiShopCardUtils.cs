using RoR2.Orbs;
using UnityEngine;
using UnityEngine.Networking;

namespace RoR2.Items;

public static class MultiShopCardUtils
{
	private const float refundPercentage = 0.1f;

	public static void OnNonMoneyPurchase(CostTypeDef.PayCostContext context)
	{
		OnPurchase(context, 0);
	}

	public static void OnMoneyPurchase(CostTypeDef.PayCostContext context)
	{
		OnPurchase(context, context.cost);
	}

	private static void OnPurchase(CostTypeDef.PayCostContext context, int moneyCost)
	{
		//IL_009e: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b4: Unknown result type (might be due to invalid IL or missing references)
		//IL_00bd: Unknown result type (might be due to invalid IL or missing references)
		//IL_00c2: Unknown result type (might be due to invalid IL or missing references)
		CharacterMaster activatorMaster = context.activatorMaster;
		if (!Object.op_Implicit((Object)(object)activatorMaster) || !activatorMaster.hasBody || !Object.op_Implicit((Object)(object)activatorMaster.inventory) || activatorMaster.inventory.currentEquipmentIndex != DLC1Content.Equipment.MultiShopCard.equipmentIndex)
		{
			return;
		}
		CharacterBody body = activatorMaster.GetBody();
		if (body.equipmentSlot.stock <= 0)
		{
			return;
		}
		bool flag = false;
		if (moneyCost > 0)
		{
			flag = true;
			GoldOrb goldOrb = new GoldOrb();
			GameObject purchasedObject = context.purchasedObject;
			Vector3? obj;
			if (purchasedObject == null)
			{
				obj = null;
			}
			else
			{
				Transform transform = purchasedObject.transform;
				obj = ((transform != null) ? new Vector3?(transform.position) : null);
			}
			goldOrb.origin = (Vector3)(((_003F?)obj) ?? body.corePosition);
			goldOrb.target = body.mainHurtBox;
			goldOrb.goldAmount = (uint)(0.1f * (float)moneyCost);
			OrbManager.instance.AddOrb(goldOrb);
		}
		GameObject purchasedObject2 = context.purchasedObject;
		ShopTerminalBehavior shopTerminalBehavior = ((purchasedObject2 != null) ? purchasedObject2.GetComponent<ShopTerminalBehavior>() : null);
		if (Object.op_Implicit((Object)(object)shopTerminalBehavior) && Object.op_Implicit((Object)(object)shopTerminalBehavior.serverMultiShopController))
		{
			flag = true;
			shopTerminalBehavior.serverMultiShopController.SetCloseOnTerminalPurchase(context.purchasedObject.GetComponent<PurchaseInteraction>(), doCloseMultiShop: false);
		}
		if (flag)
		{
			if (((NetworkBehaviour)body).hasAuthority)
			{
				body.equipmentSlot.OnEquipmentExecuted();
			}
			else
			{
				body.equipmentSlot.CallCmdOnEquipmentExecuted();
			}
		}
	}
}
