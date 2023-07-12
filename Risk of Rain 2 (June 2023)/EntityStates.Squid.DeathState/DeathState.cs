using UnityEngine;

namespace EntityStates.Squid.DeathState;

public class DeathState : BaseState
{
	public static float baseDuration;

	private float duration;

	public override void OnEnter()
	{
		duration = baseDuration;
		base.OnEnter();
		PlayAnimation("Body", "Death");
	}

	public override void FixedUpdate()
	{
		base.FixedUpdate();
		if (base.isAuthority && base.fixedAge >= duration)
		{
			EntityState.Destroy((Object)(object)base.gameObject);
		}
	}

	public override void OnExit()
	{
		if (!outer.destroying && Object.op_Implicit((Object)(object)base.gameObject))
		{
			EntityState.Destroy((Object)(object)base.gameObject);
		}
		base.OnExit();
	}
}
