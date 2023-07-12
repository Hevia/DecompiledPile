using UnityEngine;

namespace EntityStates.BeetleMonster;

public class MainState : EntityState
{
	private Animator modelAnimator;

	public override void OnEnter()
	{
		base.OnEnter();
		modelAnimator = GetModelAnimator();
		if (Object.op_Implicit((Object)(object)modelAnimator))
		{
			modelAnimator.CrossFadeInFixedTime("Walk", 0.1f);
		}
	}

	public override void FixedUpdate()
	{
		base.FixedUpdate();
		if (Object.op_Implicit((Object)(object)modelAnimator))
		{
			modelAnimator.SetFloat("walkToRunBlend", 1f);
		}
	}
}
