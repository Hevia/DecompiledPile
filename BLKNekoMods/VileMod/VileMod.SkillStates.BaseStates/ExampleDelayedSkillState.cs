using EntityStates;

namespace VileMod.SkillStates.BaseStates;

public class ExampleDelayedSkillState : BaseTimedSkillState
{
	public static float SkillBaseDuration = 1.5f;

	public static float SkillStartTime = 0.2f;

	public override void OnEnter()
	{
		((BaseState)this).OnEnter();
		InitDurationValues(SkillBaseDuration, SkillStartTime);
	}

	protected override void OnCastEnter()
	{
	}
}
