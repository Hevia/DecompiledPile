using RoR2;
using UnityEngine;

namespace EntityStates.BrotherMonster;

public class SprintBash : BasicMeleeAttack
{
	public static float durationBeforePriorityReduces;

	protected override void PlayAnimation()
	{
		PlayCrossfade("FullBody Override", "SprintBash", "SprintBash.playbackRate", duration, 0.05f);
	}

	public override void OnEnter()
	{
		//IL_0035: Unknown result type (might be due to invalid IL or missing references)
		base.OnEnter();
		AimAnimator aimAnimator = GetAimAnimator();
		if (Object.op_Implicit((Object)(object)aimAnimator))
		{
			((Behaviour)aimAnimator).enabled = true;
		}
		if (Object.op_Implicit((Object)(object)base.characterDirection))
		{
			base.characterDirection.forward = base.inputBank.aimDirection;
		}
	}

	public override void FixedUpdate()
	{
		base.FixedUpdate();
		if (base.isAuthority && Object.op_Implicit((Object)(object)base.inputBank) && Object.op_Implicit((Object)(object)base.skillLocator) && base.skillLocator.utility.IsReady() && base.inputBank.skill3.justPressed)
		{
			base.skillLocator.utility.ExecuteIfReady();
		}
	}

	public override void OnExit()
	{
		Transform val = FindModelChild("SpinnyFX");
		if (Object.op_Implicit((Object)(object)val))
		{
			((Component)val).gameObject.SetActive(false);
		}
		PlayCrossfade("FullBody Override", "BufferEmpty", 0.1f);
		base.OnExit();
	}

	public override InterruptPriority GetMinimumInterruptPriority()
	{
		if (!(base.fixedAge > durationBeforePriorityReduces))
		{
			return InterruptPriority.PrioritySkill;
		}
		return InterruptPriority.Skill;
	}
}
