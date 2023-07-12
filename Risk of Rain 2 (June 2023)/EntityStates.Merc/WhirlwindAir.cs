namespace EntityStates.Merc;

public class WhirlwindAir : WhirlwindBase
{
	protected override void PlayAnim()
	{
		PlayCrossfade("FullBody, Override", "WhirlwindAir", "Whirlwind.playbackRate", duration, 0.1f);
	}

	public override void OnExit()
	{
		base.OnExit();
		PlayAnimation("FullBody, Override", "WhirlwindAirExit");
	}
}
