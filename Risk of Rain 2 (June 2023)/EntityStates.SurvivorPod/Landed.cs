using RoR2;
using UnityEngine;

namespace EntityStates.SurvivorPod;

public class Landed : SurvivorPodBaseState
{
	public override void OnEnter()
	{
		base.OnEnter();
		PlayAnimation("Base", "Idle");
		Util.PlaySound("Play_UI_podSteamLoop", base.gameObject);
		base.survivorPodController.exitAllowed = true;
		base.vehicleSeat.handleVehicleExitRequestServer.AddCallback(HandleVehicleExitRequest);
	}

	private void HandleVehicleExitRequest(GameObject gameObject, ref bool? result)
	{
		base.survivorPodController.exitAllowed = false;
		outer.SetNextState(new PreRelease());
		result = true;
	}

	public override void OnExit()
	{
		base.vehicleSeat.handleVehicleExitRequestServer.RemoveCallback(HandleVehicleExitRequest);
		base.survivorPodController.exitAllowed = false;
		Util.PlaySound("Stop_UI_podSteamLoop", base.gameObject);
	}
}
