using RoR2;
using UnityEngine;

namespace EntityStates.Commando.CommandoWeapon;

public class PrepBarrage : BaseState
{
	public static float baseDuration = 3f;

	public static string prepBarrageSoundString;

	private float duration;

	private Animator modelAnimator;

	public override void OnEnter()
	{
		base.OnEnter();
		duration = baseDuration / attackSpeedStat;
		modelAnimator = GetModelAnimator();
		if (Object.op_Implicit((Object)(object)modelAnimator))
		{
			PlayAnimation("Gesture", "PrepBarrage", "PrepBarrage.playbackRate", duration);
		}
		Util.PlaySound(prepBarrageSoundString, base.gameObject);
		if (Object.op_Implicit((Object)(object)base.characterBody))
		{
			base.characterBody.SetAimTimer(duration);
		}
	}

	public override void FixedUpdate()
	{
		base.FixedUpdate();
		if (base.fixedAge >= duration && base.isAuthority)
		{
			FireBarrage nextState = new FireBarrage();
			outer.SetNextState(nextState);
		}
	}

	public override InterruptPriority GetMinimumInterruptPriority()
	{
		return InterruptPriority.Skill;
	}
}
