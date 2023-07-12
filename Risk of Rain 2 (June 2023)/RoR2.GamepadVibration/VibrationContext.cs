using UnityEngine;

namespace RoR2.GamepadVibration;

public struct VibrationContext
{
	public CameraRigController cameraRigController;

	public LocalUser localUser;

	public float userVibrationScale;

	public float CalcCamDisplacementMagnitude()
	{
		//IL_0019: Unknown result type (might be due to invalid IL or missing references)
		//IL_001e: Unknown result type (might be due to invalid IL or missing references)
		if (!Object.op_Implicit((Object)(object)cameraRigController))
		{
			return 0f;
		}
		Vector3 rawScreenShakeDisplacement = cameraRigController.rawScreenShakeDisplacement;
		return ((Vector3)(ref rawScreenShakeDisplacement)).magnitude;
	}
}
