namespace EntityStates.Captain.Weapon;

public class CallAirstrike1 : CallAirstrikeBase
{
	public override void OnEnter()
	{
		base.OnEnter();
	}

	public override void OnExit()
	{
		PlayAnimation("Gesture, Override", "CallAirstrike1");
		PlayAnimation("Gesture, Additive", "CallAirstrike1");
		AddRecoil(0f, 0f, -1f, -1f);
		base.OnExit();
	}
}
