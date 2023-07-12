using UnityEngine;

namespace RoR2;

public class WheelVehicleMotor : MonoBehaviour
{
	[HideInInspector]
	public Vector3 moveVector;

	public WheelCollider[] driveWheels;

	public WheelCollider[] steerWheels;

	public float motorTorque;

	public float maxSteerAngle;

	public float wheelMass = 20f;

	public float wheelRadius = 0.5f;

	public float wheelWellDistance = 2.7f;

	public float wheelSuspensionDistance = 0.3f;

	public float wheelForceAppPointDistance;

	public float wheelSuspensionSpringSpring = 35000f;

	public float wheelSuspensionSpringDamper = 4500f;

	public float wheelSuspensionSpringTargetPosition = 0.5f;

	public float forwardFrictionExtremumSlip = 0.4f;

	public float forwardFrictionValue = 1f;

	public float forwardFrictionAsymptoticSlip = 0.8f;

	public float forwardFrictionAsymptoticValue = 0.5f;

	public float forwardFrictionStiffness = 1f;

	public float sidewaysFrictionExtremumSlip = 0.2f;

	public float sidewaysFrictionValue = 1f;

	public float sidewaysFrictionAsymptoticSlip = 0.5f;

	public float sidewaysFrictionAsymptoticValue = 0.75f;

	public float sidewaysFrictionStiffness = 1f;

	private InputBankTest inputBank;

	private void Start()
	{
		inputBank = ((Component)this).GetComponent<InputBankTest>();
	}

	private void UpdateWheelParameter(WheelCollider wheel)
	{
		//IL_003c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0053: Unknown result type (might be due to invalid IL or missing references)
		//IL_005d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0069: Unknown result type (might be due to invalid IL or missing references)
		//IL_0097: Unknown result type (might be due to invalid IL or missing references)
		//IL_009f: Unknown result type (might be due to invalid IL or missing references)
		//IL_00e7: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ef: Unknown result type (might be due to invalid IL or missing references)
		//IL_0137: Unknown result type (might be due to invalid IL or missing references)
		wheel.mass = wheelMass;
		wheel.radius = wheelRadius;
		wheel.suspensionDistance = wheelSuspensionDistance;
		wheel.forceAppPointDistance = wheelForceAppPointDistance;
		((Component)wheel).transform.localPosition = new Vector3(((Component)wheel).transform.localPosition.x, 0f - wheelWellDistance, ((Component)wheel).transform.localPosition.z);
		JointSpring suspensionSpring = default(JointSpring);
		suspensionSpring.spring = wheelSuspensionSpringSpring;
		suspensionSpring.damper = wheelSuspensionSpringDamper;
		suspensionSpring.targetPosition = wheelSuspensionSpringTargetPosition;
		wheel.suspensionSpring = suspensionSpring;
		WheelFrictionCurve forwardFriction = default(WheelFrictionCurve);
		((WheelFrictionCurve)(ref forwardFriction)).extremumSlip = forwardFrictionExtremumSlip;
		((WheelFrictionCurve)(ref forwardFriction)).extremumValue = forwardFrictionValue;
		((WheelFrictionCurve)(ref forwardFriction)).asymptoteSlip = forwardFrictionAsymptoticSlip;
		((WheelFrictionCurve)(ref forwardFriction)).asymptoteValue = forwardFrictionAsymptoticValue;
		((WheelFrictionCurve)(ref forwardFriction)).stiffness = forwardFrictionStiffness;
		wheel.forwardFriction = forwardFriction;
		WheelFrictionCurve sidewaysFriction = default(WheelFrictionCurve);
		((WheelFrictionCurve)(ref sidewaysFriction)).extremumSlip = sidewaysFrictionExtremumSlip;
		((WheelFrictionCurve)(ref sidewaysFriction)).extremumValue = sidewaysFrictionValue;
		((WheelFrictionCurve)(ref sidewaysFriction)).asymptoteSlip = sidewaysFrictionAsymptoticSlip;
		((WheelFrictionCurve)(ref sidewaysFriction)).asymptoteValue = sidewaysFrictionAsymptoticValue;
		((WheelFrictionCurve)(ref sidewaysFriction)).stiffness = sidewaysFrictionStiffness;
		wheel.sidewaysFriction = sidewaysFriction;
	}

	private void UpdateAllWheelParameters()
	{
		WheelCollider[] array = driveWheels;
		foreach (WheelCollider wheel in array)
		{
			UpdateWheelParameter(wheel);
		}
		array = steerWheels;
		foreach (WheelCollider wheel2 in array)
		{
			UpdateWheelParameter(wheel2);
		}
	}

	private void FixedUpdate()
	{
		//IL_001d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0022: Unknown result type (might be due to invalid IL or missing references)
		//IL_0045: Unknown result type (might be due to invalid IL or missing references)
		//IL_004b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0050: Unknown result type (might be due to invalid IL or missing references)
		UpdateAllWheelParameters();
		if (Object.op_Implicit((Object)(object)inputBank))
		{
			moveVector = inputBank.moveVector;
			float num = 0f;
			if (((Vector3)(ref moveVector)).sqrMagnitude > 0f)
			{
				num = Util.AngleSigned(((Component)this).transform.forward, moveVector, Vector3.up);
			}
			WheelCollider[] array = steerWheels;
			for (int i = 0; i < array.Length; i++)
			{
				array[i].steerAngle = Mathf.Min(maxSteerAngle, Mathf.Abs(num)) * Mathf.Sign(num);
			}
			array = driveWheels;
			for (int i = 0; i < array.Length; i++)
			{
				array[i].motorTorque = ((Vector3)(ref moveVector)).magnitude * motorTorque;
			}
		}
	}
}
