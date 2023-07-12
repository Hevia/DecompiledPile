namespace EntityStates.BrotherMonster;

public class StaggerExit : StaggerBaseState
{
	public override EntityState nextState => new GenericCharacterMain();

	public override void OnEnter()
	{
		base.OnEnter();
		PlayCrossfade("Body", "StaggerExit", "Stagger.playbackRate", duration, 0.1f);
	}

	public override InterruptPriority GetMinimumInterruptPriority()
	{
		return InterruptPriority.Pain;
	}
}
