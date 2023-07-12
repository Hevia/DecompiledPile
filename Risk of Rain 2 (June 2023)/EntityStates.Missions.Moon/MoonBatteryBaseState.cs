using RoR2;
using UnityEngine;

namespace EntityStates.Missions.Moon;

public abstract class MoonBatteryBaseState : BaseState
{
	protected PurchaseInteraction purchaseInteraction;

	protected Animator[] animators;

	public override void OnEnter()
	{
		base.OnEnter();
		purchaseInteraction = GetComponent<PurchaseInteraction>();
		animators = ((Component)outer).GetComponentsInChildren<Animator>();
	}
}
