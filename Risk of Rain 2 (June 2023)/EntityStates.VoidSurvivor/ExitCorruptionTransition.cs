using EntityStates.VoidSurvivor.CorruptMode;
using UnityEngine;
using UnityEngine.Networking;

namespace EntityStates.VoidSurvivor;

public class ExitCorruptionTransition : CorruptionTransitionBase
{
	public override void FixedUpdate()
	{
		base.FixedUpdate();
		if (Object.op_Implicit((Object)(object)voidSurvivorController) && NetworkServer.active)
		{
			voidSurvivorController.AddCorruption(-100f);
		}
	}

	public override void OnFinishAuthority()
	{
		base.OnFinishAuthority();
		if (Object.op_Implicit((Object)(object)voidSurvivorController))
		{
			voidSurvivorController.corruptionModeStateMachine.SetNextState(new UncorruptedMode());
		}
	}
}
