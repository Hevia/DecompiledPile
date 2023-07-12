using System;
using RoR2;
using UnityEngine;

namespace EntityStates;

public class StunState : BaseState
{
	private float duration;

	private GameObject stunVfxInstance;

	public float stunDuration = 0.35f;

	public static GameObject stunVfxPrefab;

	public float timeRemaining => Math.Max(duration - base.fixedAge, 0f);

	public void ExtendStun(float durationDelta)
	{
		duration += durationDelta;
		PlayStunAnimation();
	}

	public override void OnEnter()
	{
		//IL_0079: Unknown result type (might be due to invalid IL or missing references)
		//IL_007e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0096: Unknown result type (might be due to invalid IL or missing references)
		//IL_009b: Unknown result type (might be due to invalid IL or missing references)
		base.OnEnter();
		if (Object.op_Implicit((Object)(object)base.sfxLocator) && base.sfxLocator.barkSound != "")
		{
			Util.PlaySound(base.sfxLocator.barkSound, base.gameObject);
		}
		PlayStunAnimation();
		if (Object.op_Implicit((Object)(object)base.characterBody))
		{
			base.characterBody.isSprinting = false;
		}
		if (Object.op_Implicit((Object)(object)base.characterDirection))
		{
			base.characterDirection.moveVector = base.characterDirection.forward;
		}
		if (Object.op_Implicit((Object)(object)base.rigidbodyMotor))
		{
			base.rigidbodyMotor.moveVector = Vector3.zero;
		}
	}

	private void PlayStunAnimation()
	{
		//IL_004b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0050: Unknown result type (might be due to invalid IL or missing references)
		Animator modelAnimator = GetModelAnimator();
		if (Object.op_Implicit((Object)(object)modelAnimator))
		{
			int layerIndex = modelAnimator.GetLayerIndex("Body");
			modelAnimator.CrossFadeInFixedTime((Random.Range(0, 2) == 0) ? "Hurt1" : "Hurt2", 0.1f);
			modelAnimator.Update(0f);
			AnimatorStateInfo nextAnimatorStateInfo = modelAnimator.GetNextAnimatorStateInfo(layerIndex);
			duration = Mathf.Max(stunDuration, ((AnimatorStateInfo)(ref nextAnimatorStateInfo)).length);
			if (stunDuration >= 0f)
			{
				stunVfxInstance = Object.Instantiate<GameObject>(stunVfxPrefab, base.transform);
				stunVfxInstance.GetComponent<ScaleParticleSystemDuration>().newDuration = duration;
			}
		}
	}

	public override void OnExit()
	{
		if (Object.op_Implicit((Object)(object)stunVfxInstance))
		{
			EntityState.Destroy((Object)(object)stunVfxInstance);
		}
		base.OnExit();
	}

	public override void FixedUpdate()
	{
		base.FixedUpdate();
		if (base.fixedAge >= duration && base.isAuthority)
		{
			outer.SetNextStateToMain();
		}
	}
}
