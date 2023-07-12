using UnityEngine;

namespace EntityStates.Turret1;

public class SpawnState : BaseState
{
	public static float duration = 4f;

	public override void OnEnter()
	{
		base.OnEnter();
		if (Object.op_Implicit((Object)(object)GetModelAnimator()))
		{
			PlayAnimation("Body", "Spawn", "Spawn.playbackRate", 1.5f);
		}
	}

	public override void FixedUpdate()
	{
		base.FixedUpdate();
		if (base.fixedAge >= duration && base.isAuthority)
		{
			outer.SetNextStateToMain();
		}
	}

	public override InterruptPriority GetMinimumInterruptPriority()
	{
		return InterruptPriority.Death;
	}
}
