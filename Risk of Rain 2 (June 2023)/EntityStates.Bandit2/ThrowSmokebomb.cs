namespace EntityStates.Bandit2;

public class ThrowSmokebomb : BaseState
{
	public static float duration;

	public override void OnEnter()
	{
		base.OnEnter();
		PlayAnimation("Gesture, Additive", "ThrowSmokebomb", "ThrowSmokebomb.playbackRate", duration);
	}

	public override void FixedUpdate()
	{
		base.FixedUpdate();
		if (base.fixedAge > duration)
		{
			outer.SetNextState(new StealthMode());
		}
	}

	public override InterruptPriority GetMinimumInterruptPriority()
	{
		return InterruptPriority.PrioritySkill;
	}
}
