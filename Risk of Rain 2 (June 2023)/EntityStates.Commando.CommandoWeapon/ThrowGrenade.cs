using UnityEngine;

namespace EntityStates.Commando.CommandoWeapon;

public class ThrowGrenade : GenericProjectileBaseState
{
	public override void PlayAnimation(float duration)
	{
		if (this.GetModelAnimator()))
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
