using UnityEngine;

namespace EntityStates.Merc;

public class WhirlwindEntry : BaseState
{
	public override void OnEnter()
	{
		base.OnEnter();
		if (base.isAuthority)
		{
			EntityState nextState = ((Object.op_Implicit((Object)(object)base.characterMotor) && base.characterMotor.isGrounded) ? ((WhirlwindBase)new WhirlwindGround()) : ((WhirlwindBase)new WhirlwindAir()));
			outer.SetNextState(nextState);
		}
	}
}
