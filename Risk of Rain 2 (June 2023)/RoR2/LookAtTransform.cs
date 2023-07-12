using System;
using UnityEngine;

namespace RoR2;

[ExecuteAlways]
public class LookAtTransform : MonoBehaviour
{
	public enum Axis
	{
		Right,
		Left,
		Up,
		Down,
		Forward,
		Backward
	}

	public Transform target;

	public Axis axis = Axis.Forward;

	private void LateUpdate()
	{
		//IL_0016: Unknown result type (might be due to invalid IL or missing references)
		//IL_0021: Unknown result type (might be due to invalid IL or missing references)
		//IL_0026: Unknown result type (might be due to invalid IL or missing references)
		//IL_002b: Unknown result type (might be due to invalid IL or missing references)
		//IL_002c: Unknown result type (might be due to invalid IL or missing references)
		//IL_002d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0067: Unknown result type (might be due to invalid IL or missing references)
		//IL_0074: Unknown result type (might be due to invalid IL or missing references)
		//IL_0075: Unknown result type (might be due to invalid IL or missing references)
		//IL_0086: Unknown result type (might be due to invalid IL or missing references)
		//IL_0093: Unknown result type (might be due to invalid IL or missing references)
		//IL_0094: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a5: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b2: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b3: Unknown result type (might be due to invalid IL or missing references)
		if (!Object.op_Implicit((Object)(object)target))
		{
			return;
		}
		Vector3 val = target.position - ((Component)this).transform.position;
		if (!(val == Vector3.zero))
		{
			switch (axis)
			{
			case Axis.Right:
				((Component)this).transform.right = val;
				break;
			case Axis.Left:
				((Component)this).transform.right = -val;
				break;
			case Axis.Up:
				((Component)this).transform.up = val;
				break;
			case Axis.Down:
				((Component)this).transform.right = -val;
				break;
			case Axis.Forward:
				((Component)this).transform.forward = val;
				break;
			case Axis.Backward:
				((Component)this).transform.forward = -val;
				break;
			default:
				throw new ArgumentOutOfRangeException();
			}
		}
	}
}
