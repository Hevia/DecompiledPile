using RoR2;
using UnityEngine;
using UnityEngine.Networking;

namespace EntityStates.Scrapper;

public class ScrappingToIdle : ScrapperBaseState
{
	public static string enterSoundString;

	public static string exitSoundString;

	public static float duration;

	public static float dropUpVelocityStrength;

	public static float dropForwardVelocityStrength;

	public static GameObject muzzleflashEffectPrefab;

	public static string muzzleString;

	private bool foundValidScrap;

	protected override bool enableInteraction => false;

	public override void OnEnter()
	{
		//IL_00f9: Unknown result type (might be due to invalid IL or missing references)
		//IL_00fe: Unknown result type (might be due to invalid IL or missing references)
		//IL_0108: Unknown result type (might be due to invalid IL or missing references)
		//IL_010e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0118: Unknown result type (might be due to invalid IL or missing references)
		//IL_011d: Unknown result type (might be due to invalid IL or missing references)
		base.OnEnter();
		Util.PlaySound(enterSoundString, base.gameObject);
		PlayAnimation("Base", "ScrappingToIdle", "Scrapping.playbackRate", duration);
		if (Object.op_Implicit((Object)(object)muzzleflashEffectPrefab))
		{
			EffectManager.SimpleMuzzleFlash(muzzleflashEffectPrefab, base.gameObject, muzzleString, transmit: false);
		}
		if (!NetworkServer.active)
		{
			return;
		}
		foundValidScrap = false;
		PickupIndex pickupIndex = PickupIndex.none;
		ItemDef itemDef = ItemCatalog.GetItemDef(scrapperController.lastScrappedItemIndex);
		if ((Object)(object)itemDef != (Object)null)
		{
			switch (itemDef.tier)
			{
			case ItemTier.Tier1:
				pickupIndex = PickupCatalog.FindPickupIndex("ItemIndex.ScrapWhite");
				break;
			case ItemTier.Tier2:
				pickupIndex = PickupCatalog.FindPickupIndex("ItemIndex.ScrapGreen");
				break;
			case ItemTier.Tier3:
				pickupIndex = PickupCatalog.FindPickupIndex("ItemIndex.ScrapRed");
				break;
			case ItemTier.Boss:
				pickupIndex = PickupCatalog.FindPickupIndex("ItemIndex.ScrapYellow");
				break;
			}
		}
		if (pickupIndex != PickupIndex.none)
		{
			foundValidScrap = true;
			Transform val = FindModelChild(muzzleString);
			PickupDropletController.CreatePickupDroplet(pickupIndex, val.position, Vector3.up * dropUpVelocityStrength + val.forward * dropForwardVelocityStrength);
			scrapperController.itemsEaten--;
		}
	}

	public override void OnExit()
	{
		Util.PlaySound(exitSoundString, base.gameObject);
		base.OnExit();
	}

	public override void FixedUpdate()
	{
		base.FixedUpdate();
		if (foundValidScrap && scrapperController.itemsEaten > 0 && base.fixedAge > duration / 2f)
		{
			outer.SetNextState(new ScrappingToIdle());
		}
		else if (base.fixedAge > duration)
		{
			outer.SetNextState(new Idle());
		}
	}
}
