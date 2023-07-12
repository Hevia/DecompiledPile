using System.Collections.Generic;
using HG;
using RoR2;
using UnityEngine;

namespace EntityStates.CaptainSupplyDrop;

public class HackingMainState : BaseMainState
{
	public static float baseRadius = 7f;

	public static float scanInterval = 5f;

	private float radius;

	private float scanTimer;

	private SphereSearch sphereSearch;

	public override void OnEnter()
	{
		//IL_0030: Unknown result type (might be due to invalid IL or missing references)
		//IL_0035: Unknown result type (might be due to invalid IL or missing references)
		//IL_0040: Unknown result type (might be due to invalid IL or missing references)
		//IL_0045: Unknown result type (might be due to invalid IL or missing references)
		//IL_0051: Unknown result type (might be due to invalid IL or missing references)
		base.OnEnter();
		radius = baseRadius;
		if (base.isAuthority)
		{
			sphereSearch = new SphereSearch();
			sphereSearch.origin = base.transform.position;
			sphereSearch.mask = LayerIndex.CommonMasks.interactable;
			sphereSearch.queryTriggerInteraction = (QueryTriggerInteraction)2;
			sphereSearch.radius = radius;
		}
	}

	public override void OnExit()
	{
		base.OnExit();
	}

	private PurchaseInteraction ScanForTarget()
	{
		List<Collider> list = CollectionPool<Collider, List<Collider>>.RentCollection();
		sphereSearch.ClearCandidates();
		sphereSearch.RefreshCandidates();
		sphereSearch.FilterCandidatesByColliderEntities();
		sphereSearch.OrderCandidatesByDistance();
		sphereSearch.FilterCandidatesByDistinctColliderEntities();
		sphereSearch.GetColliders(list);
		PurchaseInteraction result = null;
		int i = 0;
		for (int count = list.Count; i < count; i++)
		{
			PurchaseInteraction component = ((Component)list[i]).GetComponent<EntityLocator>().entity.GetComponent<PurchaseInteraction>();
			if (PurchaseInteractionIsValidTarget(component))
			{
				result = component;
				break;
			}
		}
		CollectionPool<Collider, List<Collider>>.ReturnCollection(list);
		return result;
	}

	public override void FixedUpdate()
	{
		base.FixedUpdate();
		if (!base.isAuthority)
		{
			return;
		}
		scanTimer -= Time.fixedDeltaTime;
		if (scanTimer <= 0f)
		{
			scanTimer = scanInterval;
			PurchaseInteraction purchaseInteraction = ScanForTarget();
			if (Object.op_Implicit((Object)(object)purchaseInteraction))
			{
				outer.SetNextState(new HackingInProgressState
				{
					target = purchaseInteraction
				});
			}
		}
	}

	public static bool PurchaseInteractionIsValidTarget(PurchaseInteraction purchaseInteraction)
	{
		if (Object.op_Implicit((Object)(object)purchaseInteraction))
		{
			if (purchaseInteraction.costType == CostTypeIndex.Money && purchaseInteraction.cost > 0)
			{
				return purchaseInteraction.available;
			}
			return false;
		}
		return false;
	}
}
