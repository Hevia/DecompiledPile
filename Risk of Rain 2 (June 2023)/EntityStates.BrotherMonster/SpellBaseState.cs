using System.Collections.Generic;
using RoR2;
using UnityEngine;
using UnityEngine.Networking;

namespace EntityStates.BrotherMonster;

public class SpellBaseState : BaseState
{
	protected ItemStealController itemStealController;

	protected GameObject hammerRendererObject;

	protected virtual bool DisplayWeapon => false;

	public override void OnEnter()
	{
		base.OnEnter();
		FindItemStealer();
		if (NetworkServer.active && !Object.op_Implicit((Object)(object)itemStealController))
		{
			InitItemStealer();
		}
		hammerRendererObject = ((Component)FindModelChild("HammerRenderer")).gameObject;
		if (Object.op_Implicit((Object)(object)hammerRendererObject))
		{
			hammerRendererObject.SetActive(DisplayWeapon);
		}
	}

	public override void OnExit()
	{
		if (Object.op_Implicit((Object)(object)hammerRendererObject))
		{
			hammerRendererObject.SetActive(false);
		}
		base.OnExit();
	}

	public override void FixedUpdate()
	{
		base.FixedUpdate();
		FindItemStealer();
	}

	private void FindItemStealer()
	{
		if (Object.op_Implicit((Object)(object)itemStealController))
		{
			return;
		}
		List<NetworkedBodyAttachment> list = new List<NetworkedBodyAttachment>();
		NetworkedBodyAttachment.FindBodyAttachments(base.characterBody, list);
		foreach (NetworkedBodyAttachment item in list)
		{
			itemStealController = ((Component)item).GetComponent<ItemStealController>();
			if (Object.op_Implicit((Object)(object)itemStealController))
			{
				break;
			}
		}
	}

	private void InitItemStealer()
	{
		//IL_0026: Unknown result type (might be due to invalid IL or missing references)
		//IL_002b: Unknown result type (might be due to invalid IL or missing references)
		if (NetworkServer.active && (Object)(object)itemStealController == (Object)null)
		{
			GameObject val = Object.Instantiate<GameObject>(LegacyResourcesAPI.Load<GameObject>("Prefabs/NetworkedObjects/ItemStealController"), base.transform.position, Quaternion.identity);
			itemStealController = val.GetComponent<ItemStealController>();
			itemStealController.itemLendFilter = ItemStealController.BrotherItemFilter;
			val.GetComponent<NetworkedBodyAttachment>().AttachToGameObjectAndSpawn(base.gameObject);
			base.gameObject.GetComponent<ReturnStolenItemsOnGettingHit>().itemStealController = itemStealController;
			NetworkServer.Spawn(val);
		}
	}
}
