using UnityEngine;

namespace EntityStates.MagmaWorm;

public class Leap : BaseState
{
	private enum LeapState
	{
		Burrow,
		Ascend,
		Fall,
		Resurface
	}

	private Transform modelBaseTransform;

	private readonly float diveDepth = 200f;

	private readonly Vector3 idealDiveVelocity = Vector3.down * 90f;

	private readonly Vector3 idealLeapVelocity = Vector3.up * 90f;

	private float leapAcceleration = 80f;

	private float resurfaceSpeed = 60f;

	private Vector3 velocity;

	private LeapState leapState;

	public override void OnEnter()
	{
		base.OnEnter();
		modelBaseTransform = GetModelBaseTransform();
		leapState = LeapState.Burrow;
	}

	public override void FixedUpdate()
	{
		//IL_003a: Unknown result type (might be due to invalid IL or missing references)
		//IL_004a: Unknown result type (might be due to invalid IL or missing references)
		//IL_005f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0065: Unknown result type (might be due to invalid IL or missing references)
		//IL_0076: Unknown result type (might be due to invalid IL or missing references)
		//IL_007b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0087: Unknown result type (might be due to invalid IL or missing references)
		//IL_008d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0097: Unknown result type (might be due to invalid IL or missing references)
		//IL_009c: Unknown result type (might be due to invalid IL or missing references)
		//IL_00c5: Unknown result type (might be due to invalid IL or missing references)
		//IL_00d5: Unknown result type (might be due to invalid IL or missing references)
		//IL_00e3: Unknown result type (might be due to invalid IL or missing references)
		//IL_00e9: Unknown result type (might be due to invalid IL or missing references)
		//IL_00fa: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ff: Unknown result type (might be due to invalid IL or missing references)
		//IL_010b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0111: Unknown result type (might be due to invalid IL or missing references)
		//IL_011b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0120: Unknown result type (might be due to invalid IL or missing references)
		//IL_0149: Unknown result type (might be due to invalid IL or missing references)
		//IL_0159: Unknown result type (might be due to invalid IL or missing references)
		//IL_016e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0173: Unknown result type (might be due to invalid IL or missing references)
		//IL_017d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0182: Unknown result type (might be due to invalid IL or missing references)
		//IL_0187: Unknown result type (might be due to invalid IL or missing references)
		//IL_0193: Unknown result type (might be due to invalid IL or missing references)
		//IL_0199: Unknown result type (might be due to invalid IL or missing references)
		//IL_01a3: Unknown result type (might be due to invalid IL or missing references)
		//IL_01a8: Unknown result type (might be due to invalid IL or missing references)
		//IL_01bc: Unknown result type (might be due to invalid IL or missing references)
		//IL_01c1: Unknown result type (might be due to invalid IL or missing references)
		//IL_01d2: Unknown result type (might be due to invalid IL or missing references)
		//IL_01dd: Unknown result type (might be due to invalid IL or missing references)
		//IL_01ee: Unknown result type (might be due to invalid IL or missing references)
		//IL_01fe: Unknown result type (might be due to invalid IL or missing references)
		//IL_020e: Unknown result type (might be due to invalid IL or missing references)
		base.FixedUpdate();
		switch (leapState)
		{
		case LeapState.Burrow:
			if (Object.op_Implicit((Object)(object)modelBaseTransform))
			{
				if (modelBaseTransform.position.y >= base.transform.position.y - diveDepth)
				{
					velocity = Vector3.MoveTowards(velocity, idealDiveVelocity, leapAcceleration * Time.fixedDeltaTime);
					Transform obj3 = modelBaseTransform;
					obj3.position += velocity * Time.fixedDeltaTime;
				}
				else
				{
					leapState = LeapState.Ascend;
				}
			}
			break;
		case LeapState.Ascend:
			if (Object.op_Implicit((Object)(object)modelBaseTransform))
			{
				if (modelBaseTransform.position.y <= base.transform.position.y)
				{
					velocity = Vector3.MoveTowards(velocity, idealLeapVelocity, leapAcceleration * Time.fixedDeltaTime);
					Transform obj = modelBaseTransform;
					obj.position += velocity * Time.fixedDeltaTime;
				}
				else
				{
					leapState = LeapState.Fall;
				}
			}
			break;
		case LeapState.Fall:
			if (Object.op_Implicit((Object)(object)modelBaseTransform))
			{
				if (modelBaseTransform.position.y >= base.transform.position.y - diveDepth)
				{
					velocity += Physics.gravity * Time.fixedDeltaTime;
					Transform obj2 = modelBaseTransform;
					obj2.position += velocity * Time.fixedDeltaTime;
				}
				else
				{
					leapState = LeapState.Resurface;
				}
			}
			break;
		case LeapState.Resurface:
			velocity = Vector3.zero;
			modelBaseTransform.position = Vector3.MoveTowards(modelBaseTransform.position, base.transform.position, resurfaceSpeed * Time.fixedDeltaTime);
			if (modelBaseTransform.position.y >= base.transform.position.y)
			{
				outer.SetNextStateToMain();
			}
			break;
		}
	}

	public override InterruptPriority GetMinimumInterruptPriority()
	{
		return InterruptPriority.PrioritySkill;
	}
}
