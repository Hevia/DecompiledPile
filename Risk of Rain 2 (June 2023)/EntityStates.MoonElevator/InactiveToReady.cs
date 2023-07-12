using RoR2;

namespace EntityStates.MoonElevator;

public class InactiveToReady : MoonElevatorBaseState
{
	public override Interactability interactability => Interactability.Disabled;

	public override bool goToNextStateAutomatically => true;

	public override EntityState nextState => new Ready();

	public override bool showBaseEffects => true;

	public override void OnEnter()
	{
		base.OnEnter();
		PlayAnimation("Base", "InactiveToActive", "playbackRate", duration);
	}
}
