using RoR2;

namespace EntityStates.Treebot.Weapon;

public class AimMortar : AimThrowableBase
{
	public static string enterSoundString;

	public static string exitSoundString;

	public override void OnEnter()
	{
		base.OnEnter();
		Util.PlaySound(enterSoundString, base.gameObject);
		PlayAnimation("Gesture, Additive", "PrepBomb", "PrepBomb.playbackRate", minimumDuration);
	}

	public override void OnExit()
	{
		base.OnExit();
		outer.SetNextState(new FireMortar());
		if (!outer.destroying)
		{
			Util.PlaySound(exitSoundString, base.gameObject);
		}
	}

	public override InterruptPriority GetMinimumInterruptPriority()
	{
		return InterruptPriority.PrioritySkill;
	}
}
