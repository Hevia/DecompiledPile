using UnityEngine;

namespace EntityStates.Jellyfish;

public class Dash : BaseState
{
	public static float duration = 1.8f;

	public static float speedCoefficient = 2f;

	private Animator modelAnimator;

	public override void OnEnter()
	{
		//IL_0035: Unknown result type (might be due to invalid IL or missing references)
		//IL_0045: Unknown result type (might be due to invalid IL or missing references)
		//IL_004f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0054: Unknown result type (might be due to invalid IL or missing references)
		base.OnEnter();
		modelAnimator = GetModelAnimator();
		if (Object.op_Implicit((Object)(object)base.rigidbodyMotor))
		{
			base.rigidbodyMotor.moveVector = ((Component)base.rigidbodyMotor.rigid).transform.forward * base.characterBody.moveSpeed * speedCoefficient;
		}
	}

	public override void FixedUpdate()
	{
		//IL_0036: Unknown result type (might be due to invalid IL or missing references)
		base.FixedUpdate();
		if (Object.op_Implicit((Object)(object)base.rigidbodyMotor) && Object.op_Implicit((Object)(object)modelAnimator))
		{
			modelAnimator.SetFloat("swim.rate", Vector3.Magnitude(base.rigidbodyMotor.rigid.velocity));
		}
		if (base.fixedAge >= duration)
		{
			outer.SetNextState(new SwimState());
		}
	}
}
