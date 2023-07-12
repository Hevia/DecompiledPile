using UnityEngine;

namespace EntityStates.Missions.Arena.NullWard;

public class Off : NullWardBaseState
{
	public override void OnEnter()
	{
		base.OnEnter();
		sphereZone.Networkradius = NullWardBaseState.wardRadiusOff;
		purchaseInteraction.SetAvailable(newAvailable: false);
		((Behaviour)sphereZone).enabled = false;
	}
}
