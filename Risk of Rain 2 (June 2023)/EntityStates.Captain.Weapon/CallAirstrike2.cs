namespace EntityStates.Captain.Weapon;

public class CallAirstrike2 : CallAirstrikeBase
{
	public override void OnExit()
	{
		PlayAnimation("Gesture, Override", "CallAirstrike2");
		PlayAnimation("Gesture, Additive", "CallAirstrike2");
		AddRecoil(0f, 0f, 1f, 1f);
		base.OnExit();
	}
}
