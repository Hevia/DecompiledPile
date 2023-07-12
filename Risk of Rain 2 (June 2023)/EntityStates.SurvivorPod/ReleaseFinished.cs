namespace EntityStates.SurvivorPod;

public class ReleaseFinished : SurvivorPodBaseState
{
	public override void OnEnter()
	{
		base.OnEnter();
		PlayAnimation("Base", "Release");
	}
}
