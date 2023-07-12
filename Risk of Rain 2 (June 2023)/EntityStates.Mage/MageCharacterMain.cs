using RoR2;
using UnityEngine;

namespace EntityStates.Mage;

public class MageCharacterMain : GenericCharacterMain
{
	private EntityStateMachine jetpackStateMachine;

	public override void OnEnter()
	{
		base.OnEnter();
		jetpackStateMachine = EntityStateMachine.FindByCustomName(base.gameObject, "Jet");
	}

	public override void ProcessJump()
	{
		base.ProcessJump();
		if (hasCharacterMotor && hasInputBank && base.isAuthority)
		{
			bool num = base.inputBank.jump.down && base.characterMotor.velocity.y < 0f && !base.characterMotor.isGrounded;
			bool flag = jetpackStateMachine.state.GetType() == typeof(JetpackOn);
			if (num && !flag)
			{
				jetpackStateMachine.SetNextState(new JetpackOn());
			}
			if (!num && flag)
			{
				jetpackStateMachine.SetNextState(new Idle());
			}
		}
	}

	public override void OnExit()
	{
		if (base.isAuthority && Object.op_Implicit((Object)(object)jetpackStateMachine))
		{
			jetpackStateMachine.SetNextState(new Idle());
		}
		base.OnExit();
	}
}
