using RoR2;
using UnityEngine;

namespace EntityStates.Missions.Arena.NullWard;

public class WardOnAndReady : NullWardBaseState
{
	public static string soundLoopStartEvent;

	public static string soundLoopEndEvent;

	public override void OnEnter()
	{
		base.OnEnter();
		sphereZone.Networkradius = NullWardBaseState.wardWaitingRadius;
		purchaseInteraction.SetAvailable(newAvailable: true);
		((Component)childLocator.FindChild("WardOnEffect")).gameObject.SetActive(true);
		((Behaviour)sphereZone).enabled = true;
		Util.PlaySound(soundLoopStartEvent, base.gameObject);
	}

	public override void OnExit()
	{
		Util.PlaySound(soundLoopEndEvent, base.gameObject);
		base.OnExit();
	}
}
