using UnityEngine;

namespace EntityStates.VoidSurvivor.CorruptMode;

public class UncorruptedMode : CorruptModeBase
{
	public override void FixedUpdate()
	{
		base.FixedUpdate();
		if (base.isAuthority && Object.op_Implicit((Object)(object)voidSurvivorController) && voidSurvivorController.corruption >= voidSurvivorController.maxCorruption && Object.op_Implicit((Object)(object)voidSurvivorController.bodyStateMachine))
		{
			voidSurvivorController.bodyStateMachine.SetInterruptState(new EnterCorruptionTransition(), InterruptPriority.Skill);
		}
	}
}
