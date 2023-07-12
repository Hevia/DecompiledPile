using RoR2;
using UnityEngine;

namespace EntityStates;

public class HurtState : BaseState
{
	private float stopwatch;

	private float duration = 0.35f;

	public override void OnEnter()
	{
		//IL_0089: Unknown result type (might be due to invalid IL or missing references)
		//IL_008e: Unknown result type (might be due to invalid IL or missing references)
		base.OnEnter();
		if (Object.op_Implicit((Object)(object)base.sfxLocator) && base.sfxLocator.barkSound != "")
		{
			Util.PlaySound(base.sfxLocator.barkSound, base.gameObject);
		}
		Animator modelAnimator = GetModelAnimator();
		if (Object.op_Implicit((Object)(object)modelAnimator))
		{
			int layerIndex = modelAnimator.GetLayerIndex("Body");
			modelAnimator.CrossFadeInFixedTime((Random.Range(0, 2) == 0) ? "Hurt1" : "Hurt2", 0.1f);
			modelAnimator.Update(0f);
			AnimatorStateInfo nextAnimatorStateInfo = modelAnimator.GetNextAnimatorStateInfo(layerIndex);
			duration = ((AnimatorStateInfo)(ref nextAnimatorStateInfo)).length;
		}
		if (Object.op_Implicit((Object)(object)base.characterBody))
		{
			base.characterBody.isSprinting = false;
		}
	}

	public override void FixedUpdate()
	{
		base.FixedUpdate();
		stopwatch += Time.fixedDeltaTime;
		if (stopwatch >= duration && base.isAuthority)
		{
			outer.SetNextStateToMain();
		}
	}
}
