using UnityEngine;

namespace EntityStates.Commando.CommandoWeapon;

public class ThrowGrenade : GenericProjectileBaseState
{
	protected override void PlayAnimation(float duration)
	{
		if (Object.op_Implicit((Object)(object)GetModelAnimator()))
		{
			PlayAnimation("Gesture, Additive", "ThrowGrenade", "FireFMJ.playbackRate", duration * 2f);
			PlayAnimation("Gesture, Override", "ThrowGrenade", "FireFMJ.playbackRate", duration * 2f);
		}
	}

	public override InterruptPriority GetMinimumInterruptPriority()
	{
		return InterruptPriority.PrioritySkill;
	}
}
