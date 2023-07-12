using RoR2;
using UnityEngine;

namespace EntityStates.Sniper.SniperWeapon;

public class Reload : BaseState
{
	public static float baseDuration = 1f;

	public static float reloadTimeFraction = 0.75f;

	public static string soundString = "";

	private float duration;

	private float reloadTime;

	private Animator modelAnimator;

	private bool reloaded;

	private EntityStateMachine scopeStateMachine;

	public override void OnEnter()
	{
		base.OnEnter();
		duration = baseDuration / attackSpeedStat;
		reloadTime = duration * reloadTimeFraction;
		modelAnimator = GetModelAnimator();
		if (Object.op_Implicit((Object)(object)modelAnimator))
		{
			PlayAnimation("Gesture", "PrepBarrage", "PrepBarrage.playbackRate", duration);
		}
		if (Object.op_Implicit((Object)(object)base.skillLocator))
		{
			GenericSkill secondary = base.skillLocator.secondary;
			if (Object.op_Implicit((Object)(object)secondary))
			{
				scopeStateMachine = secondary.stateMachine;
			}
		}
		if (base.isAuthority && Object.op_Implicit((Object)(object)scopeStateMachine))
		{
			scopeStateMachine.SetNextState(new LockSkill());
		}
	}

	public override void FixedUpdate()
	{
		base.FixedUpdate();
		if (!reloaded && base.fixedAge >= reloadTime)
		{
			if (Object.op_Implicit((Object)(object)base.skillLocator))
			{
				GenericSkill primary = base.skillLocator.primary;
				if (Object.op_Implicit((Object)(object)primary))
				{
					primary.Reset();
					Util.PlaySound(soundString, base.gameObject);
				}
			}
			reloaded = true;
		}
		if (base.fixedAge >= duration && base.isAuthority)
		{
			outer.SetNextStateToMain();
		}
	}

	public override void OnExit()
	{
		if (base.isAuthority && Object.op_Implicit((Object)(object)scopeStateMachine))
		{
			scopeStateMachine.SetNextStateToMain();
		}
		base.OnExit();
	}

	public override InterruptPriority GetMinimumInterruptPriority()
	{
		return InterruptPriority.PrioritySkill;
	}
}
