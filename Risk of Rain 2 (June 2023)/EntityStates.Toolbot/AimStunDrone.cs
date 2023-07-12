using RoR2;

namespace EntityStates.Toolbot;

public class AimStunDrone : AimGrenade
{
	public static string enterSoundString;

	public static string exitSoundString;

	public override void OnEnter()
	{
		base.OnEnter();
		Util.PlaySound(enterSoundString, base.gameObject);
		PlayAnimation("Gesture, Additive", "PrepBomb", "PrepBomb.playbackRate", minimumDuration);
		PlayAnimation("Stance, Override", "PutAwayGun");
	}

	public override void OnExit()
	{
		base.OnExit();
		outer.SetNextState(new RecoverAimStunDrone());
		Util.PlaySound(exitSoundString, base.gameObject);
	}

	public override InterruptPriority GetMinimumInterruptPriority()
	{
		return InterruptPriority.PrioritySkill;
	}
}
