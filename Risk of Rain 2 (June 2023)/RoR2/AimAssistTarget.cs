using System.Collections.Generic;
using RoR2.ConVar;
using UnityEngine;

namespace RoR2;

public class AimAssistTarget : MonoBehaviour
{
	public Transform point0;

	public Transform point1;

	public float assistScale = 1f;

	public HealthComponent healthComponent;

	public TeamComponent teamComponent;

	public static List<AimAssistTarget> instancesList = new List<AimAssistTarget>();

	public static FloatConVar debugAimAssistVisualCoefficient = new FloatConVar("debug_aim_assist_visual_coefficient", ConVarFlags.None, "2", "Magic for debug visuals. Don't touch.");

	private void OnEnable()
	{
		instancesList.Add(this);
	}

	private void OnDisable()
	{
		instancesList.Remove(this);
	}

	private void FixedUpdate()
	{
		if (Object.op_Implicit((Object)(object)healthComponent) && !healthComponent.alive)
		{
			Object.Destroy((Object)(object)((Component)this).gameObject);
		}
	}

	private void OnDrawGizmos()
	{
		//IL_0010: Unknown result type (might be due to invalid IL or missing references)
		//IL_0020: Unknown result type (might be due to invalid IL or missing references)
		//IL_004c: Unknown result type (might be due to invalid IL or missing references)
		//IL_005c: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a3: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b3: Unknown result type (might be due to invalid IL or missing references)
		//IL_00df: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ef: Unknown result type (might be due to invalid IL or missing references)
		//IL_0146: Unknown result type (might be due to invalid IL or missing references)
		//IL_0151: Unknown result type (might be due to invalid IL or missing references)
		if (Object.op_Implicit((Object)(object)point0))
		{
			Gizmos.color = Color.red;
			Gizmos.DrawWireSphere(point0.position, 1f * assistScale * CameraRigController.aimStickAssistMinSize.value * debugAimAssistVisualCoefficient.value);
			Gizmos.color = Color.white;
			Gizmos.DrawWireSphere(point0.position, 1f * assistScale * CameraRigController.aimStickAssistMaxSize.value * CameraRigController.aimStickAssistMinSize.value * debugAimAssistVisualCoefficient.value);
		}
		if (Object.op_Implicit((Object)(object)point1))
		{
			Gizmos.color = Color.red;
			Gizmos.DrawWireSphere(point1.position, 1f * assistScale * CameraRigController.aimStickAssistMinSize.value * debugAimAssistVisualCoefficient.value);
			Gizmos.color = Color.white;
			Gizmos.DrawWireSphere(point1.position, 1f * assistScale * CameraRigController.aimStickAssistMaxSize.value * CameraRigController.aimStickAssistMinSize.value * debugAimAssistVisualCoefficient.value);
		}
		if (Object.op_Implicit((Object)(object)point0) && Object.op_Implicit((Object)(object)point1))
		{
			Gizmos.DrawLine(point0.position, point1.position);
		}
	}
}
