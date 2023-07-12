namespace EntityStates.Captain.Weapon;

public class CallAirstrike3 : CallAirstrikeBase
{
	public override void OnExit()
	{
		PlayAnimation("Gesture, Override", "CallAirstrike3");
		PlayAnimation("Gesture, Additive", "CallAirstrike3");
		AddRecoil(-2f, -2f, -0.5f, 0.5f);
		base.OnExit();
	}
}
