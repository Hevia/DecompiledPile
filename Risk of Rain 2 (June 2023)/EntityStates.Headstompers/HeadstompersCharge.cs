using UnityEngine;

namespace EntityStates.Headstompers;

public class HeadstompersCharge : BaseHeadstompersState
{
	private float inputStopwatch;

	public static float maxChargeDuration = 0.5f;

	public static float minVelocityY = 1f;

	public static float accelerationY = 10f;

	public override void FixedUpdate()
	{
		base.FixedUpdate();
		if (base.isAuthority)
		{
			FixedUpdateAuthority();
		}
	}

	private void FixedUpdateAuthority()
	{
		//IL_0074: Unknown result type (might be due to invalid IL or missing references)
		//IL_0079: Unknown result type (might be due to invalid IL or missing references)
		//IL_007a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0089: Unknown result type (might be due to invalid IL or missing references)
		//IL_00af: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b0: Unknown result type (might be due to invalid IL or missing references)
		if (ReturnToIdleIfGroundedAuthority())
		{
			return;
		}
		inputStopwatch = (base.slamButtonDown ? (inputStopwatch + Time.deltaTime) : 0f);
		if (inputStopwatch >= maxChargeDuration)
		{
			outer.SetNextState(new HeadstompersFall());
		}
		else if (!base.slamButtonDown)
		{
			outer.SetNextState(new HeadstompersIdle());
		}
		else if (Object.op_Implicit((Object)(object)bodyMotor))
		{
			Vector3 velocity = bodyMotor.velocity;
			if (velocity.y < minVelocityY)
			{
				velocity.y = Mathf.MoveTowards(velocity.y, minVelocityY, accelerationY * Time.deltaTime);
				bodyMotor.velocity = velocity;
			}
		}
	}
}
