namespace EntityStates.BrotherMonster;

public class StaggerLoop : StaggerBaseState
{
	public override EntityState nextState => new StaggerExit();

	public override void OnEnter()
	{
		base.OnEnter();
		PlayCrossfade("Body", "StaggerLoop", 0.2f);
	}
}
