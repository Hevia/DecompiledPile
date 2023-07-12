using UnityEngine;

namespace EntityStates.ClayBoss;

public class RecoverExit : BaseState
{
	public static float exitDuration = 1f;

	private float stopwatch;

	public override void OnEnter()
	{
		base.OnEnter();
		stopwatch = 0f;
		PlayAnimation("Body", "ExitSiphon", "ExitSiphon.playbackRate", exitDuration);
	}

	public override void FixedUpdate()
	{
		base.FixedUpdate();
		stopwatch += Time.fixedDeltaTime;
		if (stopwatch >= exitDuration && base.isAuthority)
		{
			outer.SetNextStateToMain();
		}
	}
}
