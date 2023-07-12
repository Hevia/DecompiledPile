using EntityStates.VoidSurvivor.CorruptMode;
using RoR2;
using UnityEngine;
using UnityEngine.Networking;

namespace EntityStates.VoidSurvivor;

public class EnterCorruptionTransition : CorruptionTransitionBase
{
	[SerializeField]
	public float lingeringInvincibilityDuration;

	public override void FixedUpdate()
	{
		base.FixedUpdate();
		if (Object.op_Implicit((Object)(object)voidSurvivorController) && NetworkServer.active)
		{
			voidSurvivorController.AddCorruption(100f);
		}
	}

	public override void OnFinishAuthority()
	{
		base.OnFinishAuthority();
		if (Object.op_Implicit((Object)(object)voidSurvivorController))
		{
			voidSurvivorController.corruptionModeStateMachine.SetNextState(new EntityStates.VoidSurvivor.CorruptMode.CorruptMode());
		}
	}

	public override void OnExit()
	{
		base.OnExit();
		if (NetworkServer.active)
		{
			base.characterBody.AddTimedBuff(RoR2Content.Buffs.HiddenInvincibility, lingeringInvincibilityDuration);
		}
	}
}
