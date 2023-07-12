using UnityEngine;

namespace EntityStates.Merc.Weapon;

public class ThrowEvisProjectile : GenericProjectileBaseState
{
	public static float shortHopVelocity;

	public override void OnEnter()
	{
		base.OnEnter();
		if (Object.op_Implicit((Object)(object)base.characterMotor))
		{
			base.characterMotor.velocity.y = Mathf.Max(base.characterMotor.velocity.y, shortHopVelocity);
		}
	}

	protected override void PlayAnimation(float duration)
	{
		Animator modelAnimator = GetModelAnimator();
		if (Object.op_Implicit((Object)(object)modelAnimator))
		{
			bool @bool = modelAnimator.GetBool("isMoving");
			bool bool2 = modelAnimator.GetBool("isGrounded");
			if (@bool || !bool2)
			{
				PlayAnimation("Gesture, Additive", "GroundLight3", "GroundLight.playbackRate", duration);
				PlayAnimation("Gesture, Override", "GroundLight3", "GroundLight.playbackRate", duration);
			}
			else
			{
				PlayAnimation("FullBody, Override", "GroundLight3", "GroundLight.playbackRate", duration);
			}
		}
	}
}
