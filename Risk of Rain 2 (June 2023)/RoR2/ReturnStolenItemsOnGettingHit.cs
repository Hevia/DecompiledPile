using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.Networking;

namespace RoR2;

public class ReturnStolenItemsOnGettingHit : MonoBehaviour, IOnTakeDamageServerReceiver, IOnKilledServerReceiver
{
	public HealthComponent healthComponent;

	[SerializeField]
	[Range(0.01f, 100f)]
	private float minPercentagePerItem;

	[Range(0.01f, 100f)]
	[SerializeField]
	private float maxPercentagePerItem;

	[SerializeField]
	[Range(0f, 100f)]
	private float initialPercentageToFirstItem;

	private List<Inventory> returnOrder;

	private int nextReturnIndex;

	private float damagePerItem;

	private float accumulatedDamage;

	private ItemStealController _itemStealController;

	private bool damageTrackingInitialized;

	public ItemStealController itemStealController
	{
		get
		{
			return _itemStealController;
		}
		set
		{
			//IL_001a: Unknown result type (might be due to invalid IL or missing references)
			//IL_0024: Expected O, but got Unknown
			//IL_0039: Unknown result type (might be due to invalid IL or missing references)
			//IL_0043: Expected O, but got Unknown
			if (_itemStealController != null)
			{
				_itemStealController.onLendingFinishServer.RemoveListener(new UnityAction(InitializeDamageTracking));
			}
			if (Object.op_Implicit((Object)(object)value))
			{
				value.onLendingFinishServer.AddListener(new UnityAction(InitializeDamageTracking));
				_itemStealController = value;
			}
			else
			{
				_itemStealController = null;
			}
		}
	}

	public void OnTakeDamageServer(DamageReport damageReport)
	{
		if (Object.op_Implicit((Object)(object)itemStealController) && itemStealController.hasStolen && !damageReport.damageInfo.rejected)
		{
			accumulatedDamage += damageReport.damageDealt;
		}
	}

	private void Awake()
	{
		maxPercentagePerItem = Mathf.Max(minPercentagePerItem, maxPercentagePerItem);
	}

	private void Update()
	{
		if (!NetworkServer.active || !damageTrackingInitialized)
		{
			return;
		}
		if (damagePerItem <= 0f)
		{
			damageTrackingInitialized = false;
			Debug.LogError((object)"ReturnStolenItemsOnGettingHit.damagePerItem is 0!");
			return;
		}
		while (accumulatedDamage > damagePerItem)
		{
			accumulatedDamage -= damagePerItem;
			bool flag = itemStealController.ReclaimItemForInventory(returnOrder[nextReturnIndex]);
			nextReturnIndex = (nextReturnIndex + 1) % returnOrder.Count;
			int num = 0;
			while (!flag && num < returnOrder.Count - 1)
			{
				flag = itemStealController.ReclaimItemForInventory(returnOrder[nextReturnIndex]);
				num++;
				nextReturnIndex = (nextReturnIndex + 1) % returnOrder.Count;
			}
			if (!flag)
			{
				break;
			}
		}
	}

	private void OnDestroy()
	{
		//IL_001a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0024: Expected O, but got Unknown
		if (_itemStealController != null)
		{
			_itemStealController.onLendingFinishServer.RemoveListener(new UnityAction(InitializeDamageTracking));
			_itemStealController = null;
		}
	}

	public void OnKilledServer(DamageReport damageReport)
	{
		Object.op_Implicit((Object)(object)itemStealController);
	}

	private void InitializeDamageTracking()
	{
		//IL_00fa: Unknown result type (might be due to invalid IL or missing references)
		//IL_0104: Expected O, but got Unknown
		returnOrder = new List<Inventory>();
		if (Object.op_Implicit((Object)(object)itemStealController))
		{
			int num = 0;
			List<Inventory> list = new List<Inventory>();
			itemStealController.AddValidStolenInventoriesToList(list);
			foreach (Inventory item in list)
			{
				if (!Object.op_Implicit((Object)(object)((Component)item).GetComponent<CharacterMaster>().minionOwnership.ownerMaster))
				{
					returnOrder.Add(item);
					num += itemStealController.GetStolenItemCount(item);
				}
			}
			float num2 = Mathf.Clamp(100f / (float)Math.Max(num, 1), minPercentagePerItem, maxPercentagePerItem) / 100f;
			damagePerItem = healthComponent.fullCombinedHealth * num2;
			accumulatedDamage += damagePerItem * initialPercentageToFirstItem / 100f;
			_itemStealController.onLendingFinishServer.RemoveListener(new UnityAction(InitializeDamageTracking));
		}
		damageTrackingInitialized = true;
	}
}
