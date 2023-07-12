using RoR2;

namespace EntityStates.MoonElevator;

public class Ready : MoonElevatorBaseState
{
	public override Interactability interactability => Interactability.Available;

	public override bool goToNextStateAutomatically => false;

	public override bool showBaseEffects => true;

	public override void OnEnter()
	{
		base.OnEnter();
		PlayAnimation("Base", "Ready");
	}

	protected override void OnInteractionBegin(Interactor activator)
	{
		base.OnInteractionBegin(activator);
		_ = base.isAuthority;
	}
}
