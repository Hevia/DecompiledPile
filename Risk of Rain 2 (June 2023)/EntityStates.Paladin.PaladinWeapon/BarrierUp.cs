using RoR2;
using UnityEngine;

namespace EntityStates.Paladin.PaladinWeapon;

public class BarrierUp : BaseState
{
	public static float duration = 5f;

	public static string soundEffectString;

	private float stopwatch;

	private PaladinBarrierController paladinBarrierController;

	public override void OnEnter()
	{
		base.OnEnter();
		Util.PlaySound(soundEffectString, base.gameObject);
		stopwatch = 0f;
		paladinBarrierController = GetComponent<PaladinBarrierController>();
		if (Object.op_Implicit((Object)(object)paladinBarrierController))
		{
			paladinBarrierController.EnableBarrier();
		}
	}

	public override void OnExit()
	{
		base.OnExit();
		if (Object.op_Implicit((Object)(object)paladinBarrierController))
		{
			paladinBarrierController.DisableBarrier();
		}
	}

	public override void FixedUpdate()
	{
		base.FixedUpdate();
		stopwatch += Time.fixedDeltaTime;
		if (stopwatch >= 0.1f && !base.inputBank.skill2.down && base.isAuthority)
		{
			outer.SetNextStateToMain();
		}
	}

	public override InterruptPriority GetMinimumInterruptPriority()
	{
		return InterruptPriority.PrioritySkill;
	}
}
