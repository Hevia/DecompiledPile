namespace EntityStates.Engi.SpiderMine;

public class WaitForStick : BaseSpiderMineState
{
	protected override bool shouldStick => true;

	public override void OnEnter()
	{
		base.OnEnter();
		PlayAnimation("Base", "Idle");
	}

	public override void FixedUpdate()
	{
		base.FixedUpdate();
		if (base.isAuthority && base.projectileStickOnImpact.stuck)
		{
			outer.SetNextState(new Burrow());
		}
	}
}
