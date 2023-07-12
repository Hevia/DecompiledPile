using UnityEngine;
using UnityEngine.Networking;

namespace EntityStates.SurvivorPod;

public class Release : SurvivorPodBaseState
{
	public static float ejectionSpeed = 20f;

	public override void OnEnter()
	{
		base.OnEnter();
		PlayAnimation("Base", "Release");
		Transform modelTransform = GetModelTransform();
		if (Object.op_Implicit((Object)(object)modelTransform))
		{
			ChildLocator component = ((Component)modelTransform).GetComponent<ChildLocator>();
			((Component)component.FindChild("Door")).gameObject.SetActive(false);
			((Component)component.FindChild("ReleaseExhaustFX")).gameObject.SetActive(true);
		}
		if (Object.op_Implicit((Object)(object)base.survivorPodController) && NetworkServer.active && Object.op_Implicit((Object)(object)base.vehicleSeat) && Object.op_Implicit((Object)(object)base.vehicleSeat.currentPassengerBody))
		{
			base.vehicleSeat.EjectPassenger(((Component)base.vehicleSeat.currentPassengerBody).gameObject);
		}
	}

	public override void FixedUpdate()
	{
		base.FixedUpdate();
		if (NetworkServer.active && (!Object.op_Implicit((Object)(object)base.vehicleSeat) || !Object.op_Implicit((Object)(object)base.vehicleSeat.currentPassengerBody)))
		{
			outer.SetNextState(new ReleaseFinished());
		}
	}
}
