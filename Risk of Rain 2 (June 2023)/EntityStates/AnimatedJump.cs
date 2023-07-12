using RoR2;
using UnityEngine;

namespace EntityStates;

public class AnimatedJump : BaseState
{
	private float duration;

	private bool hasInputJump;

	public override void OnEnter()
	{
		//IL_003e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0043: Unknown result type (might be due to invalid IL or missing references)
		base.OnEnter();
		Animator modelAnimator = GetModelAnimator();
		if (Object.op_Implicit((Object)(object)modelAnimator))
		{
			int layerIndex = modelAnimator.GetLayerIndex("Body");
			modelAnimator.CrossFadeInFixedTime("AnimatedJump", 0.25f);
			modelAnimator.Update(0f);
			AnimatorStateInfo nextAnimatorStateInfo = modelAnimator.GetNextAnimatorStateInfo(layerIndex);
			duration = ((AnimatorStateInfo)(ref nextAnimatorStateInfo)).length;
			AimAnimator component = ((Component)modelAnimator).GetComponent<AimAnimator>();
			if (Object.op_Implicit((Object)(object)component))
			{
				((Behaviour)component).enabled = true;
			}
		}
	}

	public override void FixedUpdate()
	{
		//IL_003d: Unknown result type (might be due to invalid IL or missing references)
		base.FixedUpdate();
		if (base.fixedAge >= duration / 2f && base.isAuthority && !hasInputJump)
		{
			hasInputJump = true;
			base.characterMotor.moveDirection = base.inputBank.moveVector;
			GenericCharacterMain.ApplyJumpVelocity(base.characterMotor, base.characterBody, 1f, 1f);
		}
		if (base.fixedAge >= duration && base.isAuthority)
		{
			outer.SetNextStateToMain();
		}
	}
}
