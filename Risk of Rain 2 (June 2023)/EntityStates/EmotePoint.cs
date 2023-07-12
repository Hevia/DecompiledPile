using UnityEngine;

namespace EntityStates;

public class EmotePoint : BaseState
{
	public static float duration = 0.5f;

	public override void OnEnter()
	{
		base.OnEnter();
		Animator modelAnimator = GetModelAnimator();
		if (Object.op_Implicit((Object)(object)modelAnimator))
		{
			int layerIndex = modelAnimator.GetLayerIndex("Gesture");
			modelAnimator.SetFloat("EmotePoint.playbackRate", 1f);
			modelAnimator.PlayInFixedTime("EmotePoint", layerIndex, 0f);
			modelAnimator.Update(0f);
			modelAnimator.SetFloat("EmotePoint.playbackRate", attackSpeedStat);
		}
	}

	public override void FixedUpdate()
	{
		base.FixedUpdate();
		if (base.fixedAge > duration && base.isAuthority)
		{
			outer.SetNextStateToMain();
		}
	}
}
