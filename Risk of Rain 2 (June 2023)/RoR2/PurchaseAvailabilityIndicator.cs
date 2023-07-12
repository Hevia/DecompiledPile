using UnityEngine;

namespace RoR2;

[RequireComponent(typeof(PurchaseInteraction))]
public class PurchaseAvailabilityIndicator : MonoBehaviour
{
	public GameObject indicatorObject;

	public GameObject disabledIndicatorObject;

	public Animator animator;

	public string mecanimBool;

	private PurchaseInteraction purchaseInteraction;

	private void Awake()
	{
		purchaseInteraction = ((Component)this).GetComponent<PurchaseInteraction>();
	}

	private void FixedUpdate()
	{
		if (Object.op_Implicit((Object)(object)indicatorObject))
		{
			indicatorObject.SetActive(purchaseInteraction.available);
		}
		if (Object.op_Implicit((Object)(object)disabledIndicatorObject))
		{
			disabledIndicatorObject.SetActive(!purchaseInteraction.available);
		}
		if (Object.op_Implicit((Object)(object)animator))
		{
			animator.SetBool(mecanimBool, purchaseInteraction.available);
		}
	}
}
