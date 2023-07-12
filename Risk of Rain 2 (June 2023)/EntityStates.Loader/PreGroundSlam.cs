using KinematicCharacterController;
using RoR2;

namespace EntityStates.Loader;

public class PreGroundSlam : BaseCharacterMain
{
	public static float baseDuration;

	public static string enterSoundString;

	public static float upwardVelocity;

	private float duration;

	public override void OnEnter()
	{
		base.OnEnter();
		duration = baseDuration / attackSpeedStat;
		PlayAnimation("Body", "PreGroundSlam", "GroundSlam.playbackRate", duration);
		Util.PlaySound(enterSoundString, base.gameObject);
		((BaseCharacterController)base.characterMotor).Motor.ForceUnground();
		base.characterMotor.disableAirControlUntilCollision = false;
		base.characterMotor.velocity.y = upwardVelocity;
	}

	public override void FixedUpdate()
	{
		//IL_0012: Unknown result type (might be due to invalid IL or missing references)
		base.FixedUpdate();
		base.characterMotor.moveDirection = base.inputBank.moveVector;
		if (base.fixedAge > duration)
		{
			outer.SetNextState(new GroundSlam());
		}
	}
}
