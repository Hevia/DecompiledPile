namespace EntityStates.AncientWispMonster;

public class FireBomb : BaseState
{
	public static float baseDuration = 4f;

	private float duration;

	public override void OnEnter()
	{
		base.OnEnter();
		duration = baseDuration / attackSpeedStat;
		PlayAnimation("Gesture", "FireBomb", "FireBomb.playbackRate", duration);
	}

	public override void FixedUpdate()
	{
		base.FixedUpdate();
		if (base.fixedAge >= duration && base.isAuthority)
		{
			outer.SetNextStateToMain();
		}
	}

	public override InterruptPriority GetMinimumInterruptPriority()
	{
		return InterruptPriority.Frozen;
	}
}
