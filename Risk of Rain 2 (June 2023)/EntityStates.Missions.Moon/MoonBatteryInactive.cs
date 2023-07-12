using UnityEngine;

namespace EntityStates.Missions.Moon;

public class MoonBatteryInactive : MoonBatteryBaseState
{
	public override void OnEnter()
	{
		base.OnEnter();
		purchaseInteraction.SetAvailable(newAvailable: true);
		((Component)FindModelChild("InactiveFX")).gameObject.SetActive(true);
	}

	public override void OnExit()
	{
		((Component)FindModelChild("InactiveFX")).gameObject.SetActive(false);
		purchaseInteraction.SetAvailable(newAvailable: false);
		base.OnExit();
	}
}
