using UnityEngine;

namespace EntityStates.Missions.Moon;

public class MoonBatteryComplete : MoonBatteryBaseState
{
	public override void OnEnter()
	{
		base.OnEnter();
		((Component)FindModelChild("ChargedFX")).gameObject.SetActive(true);
	}
}
