using UnityEngine;

namespace EntityStates;

public class BaseMelee : BaseState
{
	protected RootMotionAccumulator rootMotionAccumulator;

	public RootMotionAccumulator InitMeleeRootMotion()
	{
		//IL_001f: Unknown result type (might be due to invalid IL or missing references)
		//IL_003e: Unknown result type (might be due to invalid IL or missing references)
		//IL_005b: Unknown result type (might be due to invalid IL or missing references)
		rootMotionAccumulator = GetModelRootMotionAccumulator();
		if (Object.op_Implicit((Object)(object)rootMotionAccumulator))
		{
			rootMotionAccumulator.ExtractRootMotion();
		}
		if (Object.op_Implicit((Object)(object)base.characterDirection))
		{
			base.characterDirection.forward = base.inputBank.aimDirection;
		}
		if (Object.op_Implicit((Object)(object)base.characterMotor))
		{
			base.characterMotor.moveDirection = Vector3.zero;
		}
		return rootMotionAccumulator;
	}

	public void UpdateMeleeRootMotion(float scale)
	{
		//IL_0013: Unknown result type (might be due to invalid IL or missing references)
		//IL_0018: Unknown result type (might be due to invalid IL or missing references)
		//IL_002c: Unknown result type (might be due to invalid IL or missing references)
		//IL_002e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0033: Unknown result type (might be due to invalid IL or missing references)
		if (Object.op_Implicit((Object)(object)rootMotionAccumulator))
		{
			Vector3 val = rootMotionAccumulator.ExtractRootMotion();
			if (Object.op_Implicit((Object)(object)base.characterMotor))
			{
				base.characterMotor.rootMotion = val * scale;
			}
		}
	}
}
