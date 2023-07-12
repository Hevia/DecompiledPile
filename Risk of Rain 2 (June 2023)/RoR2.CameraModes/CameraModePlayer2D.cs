using UnityEngine;

namespace RoR2.CameraModes;

public class CameraModePlayer2D : CameraModeBase
{
	private class InstanceData
	{
		public float facing = 1f;
	}

	public static readonly CameraModePlayer2D instance = new CameraModePlayer2D();

	public override bool IsSpectating(CameraRigController cameraRigController)
	{
		return false;
	}

	protected override void ApplyLookInputInternal(object rawInstanceData, in CameraModeContext context, in ApplyLookInputArgs input)
	{
	}

	protected override void CollectLookInputInternal(object rawInstanceData, in CameraModeContext context, out CollectLookInputResult result)
	{
		//IL_0001: Unknown result type (might be due to invalid IL or missing references)
		//IL_0006: Unknown result type (might be due to invalid IL or missing references)
		result.lookInput = Vector2.zero;
	}

	protected override object CreateInstanceData(CameraRigController cameraRigController)
	{
		return new InstanceData();
	}

	protected override void MatchStateInternal(object rawInstanceData, in CameraModeContext context, in CameraState cameraStateToMatch)
	{
	}

	protected override void OnTargetChangedInternal(object rawInstanceData, CameraRigController cameraRigController, in OnTargetChangedArgs args)
	{
	}

	protected override void UpdateInternal(object rawInstanceData, in CameraModeContext context, out UpdateResult result)
	{
		//IL_002d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0032: Unknown result type (might be due to invalid IL or missing references)
		//IL_004c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0051: Unknown result type (might be due to invalid IL or missing references)
		//IL_0052: Unknown result type (might be due to invalid IL or missing references)
		//IL_0062: Unknown result type (might be due to invalid IL or missing references)
		//IL_0067: Unknown result type (might be due to invalid IL or missing references)
		//IL_006c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0071: Unknown result type (might be due to invalid IL or missing references)
		//IL_0076: Unknown result type (might be due to invalid IL or missing references)
		//IL_00bf: Unknown result type (might be due to invalid IL or missing references)
		//IL_00c4: Unknown result type (might be due to invalid IL or missing references)
		//IL_00d7: Unknown result type (might be due to invalid IL or missing references)
		//IL_00dc: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ed: Unknown result type (might be due to invalid IL or missing references)
		//IL_00f2: Unknown result type (might be due to invalid IL or missing references)
		//IL_0106: Unknown result type (might be due to invalid IL or missing references)
		//IL_0112: Unknown result type (might be due to invalid IL or missing references)
		//IL_013a: Unknown result type (might be due to invalid IL or missing references)
		//IL_013f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0141: Unknown result type (might be due to invalid IL or missing references)
		//IL_0151: Unknown result type (might be due to invalid IL or missing references)
		//IL_0156: Unknown result type (might be due to invalid IL or missing references)
		//IL_0162: Unknown result type (might be due to invalid IL or missing references)
		//IL_0163: Unknown result type (might be due to invalid IL or missing references)
		//IL_017e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0192: Unknown result type (might be due to invalid IL or missing references)
		//IL_0197: Unknown result type (might be due to invalid IL or missing references)
		//IL_019c: Unknown result type (might be due to invalid IL or missing references)
		//IL_019d: Unknown result type (might be due to invalid IL or missing references)
		//IL_01a2: Unknown result type (might be due to invalid IL or missing references)
		//IL_01ac: Unknown result type (might be due to invalid IL or missing references)
		//IL_01b1: Unknown result type (might be due to invalid IL or missing references)
		//IL_01b6: Unknown result type (might be due to invalid IL or missing references)
		//IL_01cc: Unknown result type (might be due to invalid IL or missing references)
		//IL_01ce: Unknown result type (might be due to invalid IL or missing references)
		//IL_01d5: Unknown result type (might be due to invalid IL or missing references)
		//IL_01df: Unknown result type (might be due to invalid IL or missing references)
		//IL_01e4: Unknown result type (might be due to invalid IL or missing references)
		//IL_01e9: Unknown result type (might be due to invalid IL or missing references)
		InstanceData instanceData = (InstanceData)rawInstanceData;
		result.cameraState = context.cameraInfo.previousCameraState;
		result.firstPersonTarget = null;
		result.showSprintParticles = false;
		result.crosshairWorldPosition = result.cameraState.position;
		if (!Object.op_Implicit((Object)(object)context.targetInfo.target))
		{
			return;
		}
		Quaternion identity = Quaternion.identity;
		Vector3 val = identity * Quaternion.Euler(0f, 90f, 0f) * Vector3.forward;
		Quaternion val2 = ((!Object.op_Implicit((Object)(object)context.targetInfo.body) || !Object.op_Implicit((Object)(object)context.targetInfo.body.characterDirection)) ? context.targetInfo.target.transform.rotation : Quaternion.Euler(0f, context.targetInfo.body.characterDirection.yaw, 0f));
		Vector3 val3 = context.targetInfo.target.transform.position;
		if (Object.op_Implicit((Object)(object)context.targetInfo.inputBank))
		{
			float num = Vector3.Dot(val, context.targetInfo.inputBank.moveVector);
			if (num != 0f)
			{
				instanceData.facing = num;
			}
			val3 = context.targetInfo.inputBank.aimOrigin;
		}
		_ = val2 * Quaternion.Euler(0f, 90f, 0f);
		result.cameraState.rotation = identity;
		result.cameraState.position = context.targetInfo.target.transform.position + new Vector3(0f, 10f, 0f) + identity * Vector3.forward * -30f;
		result.cameraState.fov = 60f;
		result.crosshairWorldPosition = val3 + val * instanceData.facing * 30f;
	}
}
