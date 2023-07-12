using UnityEngine;

namespace EntityStates.HAND.Weapon;

public class ChargeSlam : BaseState
{
	public static float baseDuration = 3.5f;

	private float duration;

	private Animator modelAnimator;

	public override void OnEnter()
	{
		base.OnEnter();
		duration = baseDuration / attackSpeedStat;
		modelAnimator = GetModelAnimator();
		if (Object.op_Implicit((Object)(object)modelAnimator))
		{
			PlayAnimation("Gesture", "ChargeSlam", "ChargeSlam.playbackRate", duration);
		}
		if (Object.op_Implicit((Object)(object)base.characterBody))
		{
			base.characterBody.SetAimTimer(4f);
		}
	}

	public override void FixedUpdate()
	{
		base.FixedUpdate();
		if (base.fixedAge >= duration && base.characterMotor.isGrounded && base.isAuthority)
		{
			outer.SetNextState(new Slam());
		}
	}

	public override InterruptPriority GetMinimumInterruptPriority()
	{
		return InterruptPriority.PrioritySkill;
	}
}
