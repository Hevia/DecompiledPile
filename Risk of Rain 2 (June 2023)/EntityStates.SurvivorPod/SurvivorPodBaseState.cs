using RoR2;
using UnityEngine;

namespace EntityStates.SurvivorPod;

public abstract class SurvivorPodBaseState : EntityState
{
	protected SurvivorPodController survivorPodController { get; private set; }

	protected VehicleSeat vehicleSeat { get; private set; }

	public override void OnEnter()
	{
		base.OnEnter();
		survivorPodController = GetComponent<SurvivorPodController>();
		vehicleSeat = survivorPodController?.vehicleSeat;
		if (!Object.op_Implicit((Object)(object)survivorPodController) && base.isAuthority)
		{
			outer.SetNextStateToMain();
		}
	}
}
