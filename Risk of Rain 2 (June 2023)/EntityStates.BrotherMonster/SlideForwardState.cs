using UnityEngine;

namespace EntityStates.BrotherMonster;

public class SlideForwardState : BaseSlideState
{
	public override void OnEnter()
	{
		//IL_0001: Unknown result type (might be due to invalid IL or missing references)
		//IL_0006: Unknown result type (might be due to invalid IL or missing references)
		slideRotation = Quaternion.identity;
		base.OnEnter();
		PlayCrossfade("FullBody Override", "SlideForward", "Slide.playbackRate", BaseSlideState.duration, 0.05f);
		PlayCrossfade("Body", "Run", 0.05f);
	}

	public override void OnExit()
	{
		base.OnExit();
	}
}
