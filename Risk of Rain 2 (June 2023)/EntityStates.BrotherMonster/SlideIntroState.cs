using UnityEngine;

namespace EntityStates.BrotherMonster;

public class SlideIntroState : BaseState
{
	public override void OnEnter()
	{
		//IL_0029: Unknown result type (might be due to invalid IL or missing references)
		//IL_002e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0040: Unknown result type (might be due to invalid IL or missing references)
		//IL_004d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0052: Unknown result type (might be due to invalid IL or missing references)
		//IL_0056: Unknown result type (might be due to invalid IL or missing references)
		//IL_0061: Unknown result type (might be due to invalid IL or missing references)
		//IL_0066: Unknown result type (might be due to invalid IL or missing references)
		//IL_0067: Unknown result type (might be due to invalid IL or missing references)
		//IL_006c: Unknown result type (might be due to invalid IL or missing references)
		//IL_006d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0072: Unknown result type (might be due to invalid IL or missing references)
		//IL_0073: Unknown result type (might be due to invalid IL or missing references)
		//IL_0074: Unknown result type (might be due to invalid IL or missing references)
		//IL_007b: Unknown result type (might be due to invalid IL or missing references)
		//IL_009c: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a1: Unknown result type (might be due to invalid IL or missing references)
		base.OnEnter();
		bool flag = false;
		if (Object.op_Implicit((Object)(object)base.inputBank) && base.isAuthority)
		{
			Vector3 val = ((base.inputBank.moveVector == Vector3.zero) ? base.characterDirection.forward : base.inputBank.moveVector);
			Vector3 normalized = ((Vector3)(ref val)).normalized;
			Vector3 forward = base.characterDirection.forward;
			Vector3 val2 = Vector3.Cross(Vector3.up, forward);
			float num = Vector3.Dot(normalized, forward);
			float num2 = Vector3.Dot(normalized, val2);
			if (Object.op_Implicit((Object)(object)base.characterDirection))
			{
				base.characterDirection.moveVector = base.inputBank.aimDirection;
			}
			if (Mathf.Abs(num2) > Mathf.Abs(num))
			{
				if (num2 <= 0f)
				{
					flag = true;
					outer.SetNextState(new SlideLeftState());
				}
				else
				{
					flag = true;
					outer.SetNextState(new SlideRightState());
				}
			}
			else if (num <= 0f)
			{
				flag = true;
				outer.SetNextState(new SlideBackwardState());
			}
			else
			{
				flag = true;
				outer.SetNextState(new SlideForwardState());
			}
		}
		if (!flag)
		{
			outer.SetNextState(new SlideForwardState());
		}
	}
}
