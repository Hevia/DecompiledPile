namespace EntityStates.Barrel;

public class Opened : EntityState
{
	public override void OnEnter()
	{
		base.OnEnter();
		PlayAnimation("Body", "Opened");
	}
}
