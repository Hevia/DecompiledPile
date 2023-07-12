using HG;

namespace RoR2.GamepadVibration;

public class DefaultGamepadVibrationController : GamepadVibrationController
{
	protected override void CalculateMotorValues(in VibrationContext vibrationContext, float[] motorValues)
	{
		float num = vibrationContext.CalcCamDisplacementMagnitude();
		ArrayUtils.SetRange<float>(motorValues, ref num, 0, motorValues.Length);
	}
}
