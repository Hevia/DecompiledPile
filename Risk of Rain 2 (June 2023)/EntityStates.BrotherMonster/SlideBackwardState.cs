using UnityEngine;

namespace EntityStates.BrotherMonster;

public class SlideBackwardState : BaseSlideState
{
	public override void OnEnter()
	{
		//IL_0006: Unknown result type (might be due to invalid IL or missing references)
		//IL_000b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0010: Unknown result type (might be due to invalid IL or missing references)
		slideRotation = Quaternion.AngleAxis(-180f, Vector3.up);
		base.OnEnter();
		PlayCrossfade("FullBody Override", "SlideBackward", "Slide.playbackRate", BaseSlideState.duration, 0.05f);
	}
}
