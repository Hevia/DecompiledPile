using UnityEngine;

namespace EntityStates;

public class GenericCharacterPod : BaseState
{
	public override void OnEnter()
	{
		base.OnEnter();
		if (Object.op_Implicit((Object)(object)base.characterMotor))
		{
			((Behaviour)base.characterMotor).enabled = false;
		}
		if (Object.op_Implicit((Object)(object)base.rigidbodyMotor))
		{
			((Behaviour)base.rigidbodyMotor).enabled = false;
		}
	}

	public override void OnExit()
	{
		if (Object.op_Implicit((Object)(object)base.characterMotor))
		{
			((Behaviour)base.characterMotor).enabled = true;
		}
		if (Object.op_Implicit((Object)(object)base.rigidbodyMotor))
		{
			((Behaviour)base.rigidbodyMotor).enabled = true;
		}
		base.OnExit();
	}
}
