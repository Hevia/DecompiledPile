using UnityEngine;

namespace EntityStates;

public class SleepState : EntityState
{
	public override void OnEnter()
	{
		base.OnEnter();
		Animator modelAnimator = GetModelAnimator();
		if (Object.op_Implicit((Object)(object)modelAnimator))
		{
			int layerIndex = modelAnimator.GetLayerIndex("Body");
			modelAnimator.Play("Sleep", layerIndex, 0f);
		}
	}

	public override void FixedUpdate()
	{
		base.FixedUpdate();
	}

	public override InterruptPriority GetMinimumInterruptPriority()
	{
		return InterruptPriority.Any;
	}
}
