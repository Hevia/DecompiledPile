namespace EntityStates.Merc;

public class WhirlwindGround : WhirlwindBase
{
	protected override void PlayAnim()
	{
		PlayCrossfade("FullBody, Override", "WhirlwindGround", "Whirlwind.playbackRate", duration, 0.1f);
	}

	public override void OnExit()
	{
		base.OnExit();
		int layerIndex = animator.GetLayerIndex("Impact");
		if (layerIndex >= 0)
		{
			animator.SetLayerWeight(layerIndex, 3f);
			PlayAnimation("Impact", "LightImpact");
		}
	}
}
