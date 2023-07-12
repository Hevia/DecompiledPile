using UnityEngine;

namespace RoR2;

public struct CharacterAnimParamAvailability
{
	public bool isMoving;

	public bool turnAngle;

	public bool isGrounded;

	public bool mainRootPlaybackRate;

	public bool forwardSpeed;

	public bool rightSpeed;

	public bool upSpeed;

	public bool walkSpeed;

	public bool isSprinting;

	public static CharacterAnimParamAvailability FromAnimator(Animator animator)
	{
		CharacterAnimParamAvailability result = default(CharacterAnimParamAvailability);
		result.isMoving = Util.HasAnimationParameter(AnimationParameters.isMoving, animator);
		result.turnAngle = Util.HasAnimationParameter(AnimationParameters.turnAngle, animator);
		result.isGrounded = Util.HasAnimationParameter(AnimationParameters.isGrounded, animator);
		result.mainRootPlaybackRate = Util.HasAnimationParameter(AnimationParameters.mainRootPlaybackRate, animator);
		result.forwardSpeed = Util.HasAnimationParameter(AnimationParameters.forwardSpeed, animator);
		result.rightSpeed = Util.HasAnimationParameter(AnimationParameters.rightSpeed, animator);
		result.upSpeed = Util.HasAnimationParameter(AnimationParameters.upSpeed, animator);
		result.walkSpeed = Util.HasAnimationParameter(AnimationParameters.walkSpeed, animator);
		result.isSprinting = Util.HasAnimationParameter(AnimationParameters.isSprinting, animator);
		return result;
	}
}
