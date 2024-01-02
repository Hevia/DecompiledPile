using EntityStates;

namespace RandomSurvivors;

public class RandomUtility : BaseState
{
	private readonly float totalDuration = 1f;

	public override void OnEnter()
	{
		((BaseState)this).OnEnter();
	}

	public override void OnExit()
	{
		if (((EntityState)this).isAuthority)
		{
			((EntityState)this).OnExit();
		}
	}

	public override void FixedUpdate()
	{
		((EntityState)this).FixedUpdate();
		if (((EntityState)this).fixedAge >= totalDuration && ((EntityState)this).isAuthority)
		{
			((EntityState)this).outer.SetNextStateToMain();
		}
	}
}
