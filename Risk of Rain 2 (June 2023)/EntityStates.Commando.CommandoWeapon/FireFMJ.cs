using UnityEngine;

namespace EntityStates.Commando.CommandoWeapon;

public class FireFMJ : GenericProjectileBaseState
{
	protected override void PlayAnimation(float duration)
	{
		base.PlayAnimation(duration);
		if (Object.op_Implicit((Object)(object)GetModelAnimator()))
		{
			PlayAnimation("Gesture, Additive", "FireFMJ", "FireFMJ.playbackRate", duration);
			PlayAnimation("Gesture, Override", "FireFMJ", "FireFMJ.playbackRate", duration);
		}
	}
}
