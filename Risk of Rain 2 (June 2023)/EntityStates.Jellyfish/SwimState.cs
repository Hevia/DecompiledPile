using RoR2;
using UnityEngine;

namespace EntityStates.Jellyfish;

public class SwimState : BaseState
{
	private Animator modelAnimator;

	private bool skill1InputReceived;

	private bool skill2InputReceived;

	private bool skill3InputReceived;

	private bool skill4InputReceived;

	private bool jumpInputReceived;

	public override void OnEnter()
	{
		base.OnEnter();
		modelAnimator = GetModelAnimator();
	}

	public override void Update()
	{
		base.Update();
		if (Object.op_Implicit((Object)(object)base.inputBank))
		{
			skill1InputReceived = base.inputBank.skill1.down;
			skill2InputReceived |= base.inputBank.skill2.down;
			skill3InputReceived |= base.inputBank.skill3.down;
			skill4InputReceived |= base.inputBank.skill4.down;
			jumpInputReceived |= base.inputBank.jump.down;
		}
	}

	public override void FixedUpdate()
	{
		//IL_003a: Unknown result type (might be due to invalid IL or missing references)
		//IL_004a: Unknown result type (might be due to invalid IL or missing references)
		//IL_004f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0077: Unknown result type (might be due to invalid IL or missing references)
		//IL_009a: Unknown result type (might be due to invalid IL or missing references)
		//IL_009f: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a2: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a7: Unknown result type (might be due to invalid IL or missing references)
		base.FixedUpdate();
		if (!base.isAuthority)
		{
			return;
		}
		if (Object.op_Implicit((Object)(object)base.inputBank))
		{
			if (Object.op_Implicit((Object)(object)base.rigidbodyMotor))
			{
				base.rigidbodyMotor.moveVector = base.inputBank.moveVector * base.characterBody.moveSpeed;
				if (Object.op_Implicit((Object)(object)modelAnimator))
				{
					modelAnimator.SetFloat("swim.rate", Vector3.Magnitude(base.rigidbodyMotor.rigid.velocity));
				}
			}
			if (Object.op_Implicit((Object)(object)base.rigidbodyDirection))
			{
				RigidbodyDirection obj = base.rigidbodyDirection;
				Ray aimRay = GetAimRay();
				obj.aimDirection = ((Ray)(ref aimRay)).direction;
			}
		}
		if (Object.op_Implicit((Object)(object)base.skillLocator))
		{
			if (Object.op_Implicit((Object)(object)base.skillLocator.primary) && skill1InputReceived)
			{
				base.skillLocator.primary.ExecuteIfReady();
			}
			if (Object.op_Implicit((Object)(object)base.skillLocator.secondary) && skill2InputReceived)
			{
				base.skillLocator.secondary.ExecuteIfReady();
			}
			if (Object.op_Implicit((Object)(object)base.skillLocator.utility) && skill3InputReceived)
			{
				base.skillLocator.utility.ExecuteIfReady();
			}
			if (Object.op_Implicit((Object)(object)base.skillLocator.special) && skill4InputReceived)
			{
				base.skillLocator.special.ExecuteIfReady();
			}
		}
	}
}
