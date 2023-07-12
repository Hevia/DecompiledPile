using RoR2;

namespace EntityStates.MoonElevator;

public class Inactive : MoonElevatorBaseState
{
	public override Interactability interactability => Interactability.ConditionsNotMet;

	public override bool goToNextStateAutomatically => false;

	public override bool showBaseEffects => false;

	public override void OnEnter()
	{
		base.OnEnter();
		PlayAnimation("Base", "Inactive");
	}
}
