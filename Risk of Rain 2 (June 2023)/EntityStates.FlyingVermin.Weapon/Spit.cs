using UnityEngine;

namespace EntityStates.FlyingVermin.Weapon;

public class Spit : GenericProjectileBaseState
{
	public override void OnEnter()
	{
		base.OnEnter();
		if (Object.op_Implicit((Object)(object)base.characterBody))
		{
			base.characterBody.SetAimTimer(5f);
		}
	}

	protected override void PlayAnimation(float duration)
	{
		base.PlayAnimation(duration);
		PlayAnimation("Gesture, Additive", "Spit", "Spit.playbackRate", duration);
	}

	public override InterruptPriority GetMinimumInterruptPriority()
	{
		return InterruptPriority.Skill;
	}
}
