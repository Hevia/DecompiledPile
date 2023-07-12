using System;
using UnityEngine;

namespace RoR2;

[RequireComponent(typeof(Rigidbody))]
public class HoverVehicleMotor : MonoBehaviour
{
	private enum WheelLateralAxis
	{
		Left,
		Right
	}

	public enum WheelLongitudinalAxis
	{
		Front,
		Back
	}

	[Serializable]
	public struct AxleGroup
	{
		public HoverEngine leftWheel;

		public HoverEngine rightWheel;

		public WheelLongitudinalAxis wheelLongitudinalAxis;

		public bool isDriven;

		public AnimationCurve slidingTractionCurve;
	}

	[HideInInspector]
	public Vector3 targetSteerVector;

	public Vector3 centerOfMassOffset;

	public AxleGroup[] staticAxles;

	public AxleGroup[] steerAxles;

	public float wheelWellDepth;

	public float wheelBase;

	public float trackWidth;

	public float rollingFrictionCoefficient;

	public float slidingTractionCoefficient;

	public float motorForce;

	public float maxSteerAngle;

	public float maxTurningRadius;

	public float hoverForce = 33f;

	public float hoverHeight = 2f;

	public float hoverDamping = 0.5f;

	public float hoverRadius = 0.5f;

	public Vector3 hoverOffsetVector = Vector3.up;

	private InputBankTest inputBank;

	private Vector3 steerVector = Vector3.forward;

	private Rigidbody rigidbody;

	private void Start()
	{
		inputBank = ((Component)this).GetComponent<InputBankTest>();
		rigidbody = ((Component)this).GetComponent<Rigidbody>();
	}

	private void ApplyWheelForces(HoverEngine wheel, float gas, bool driveWheel, AnimationCurve slidingWheelTractionCurve)
	{
		//IL_001f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0024: Unknown result type (might be due to invalid IL or missing references)
		//IL_002b: Unknown result type (might be due to invalid IL or missing references)
		//IL_002c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0031: Unknown result type (might be due to invalid IL or missing references)
		//IL_0033: Unknown result type (might be due to invalid IL or missing references)
		//IL_0036: Unknown result type (might be due to invalid IL or missing references)
		//IL_003b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0040: Unknown result type (might be due to invalid IL or missing references)
		//IL_0043: Unknown result type (might be due to invalid IL or missing references)
		//IL_0048: Unknown result type (might be due to invalid IL or missing references)
		//IL_004d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0052: Unknown result type (might be due to invalid IL or missing references)
		//IL_0054: Unknown result type (might be due to invalid IL or missing references)
		//IL_0055: Unknown result type (might be due to invalid IL or missing references)
		//IL_0057: Unknown result type (might be due to invalid IL or missing references)
		//IL_0061: Unknown result type (might be due to invalid IL or missing references)
		//IL_0066: Unknown result type (might be due to invalid IL or missing references)
		//IL_006c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0072: Unknown result type (might be due to invalid IL or missing references)
		//IL_007d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0082: Unknown result type (might be due to invalid IL or missing references)
		//IL_008b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0091: Unknown result type (might be due to invalid IL or missing references)
		//IL_009c: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a2: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a7: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ad: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ae: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b1: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b6: Unknown result type (might be due to invalid IL or missing references)
		//IL_00c0: Unknown result type (might be due to invalid IL or missing references)
		//IL_00cb: Unknown result type (might be due to invalid IL or missing references)
		//IL_00d1: Unknown result type (might be due to invalid IL or missing references)
		//IL_00d6: Unknown result type (might be due to invalid IL or missing references)
		//IL_00d8: Unknown result type (might be due to invalid IL or missing references)
		//IL_00dd: Unknown result type (might be due to invalid IL or missing references)
		//IL_00e5: Unknown result type (might be due to invalid IL or missing references)
		//IL_00e7: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ed: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ee: Unknown result type (might be due to invalid IL or missing references)
		//IL_00f1: Unknown result type (might be due to invalid IL or missing references)
		//IL_00f6: Unknown result type (might be due to invalid IL or missing references)
		//IL_0100: Unknown result type (might be due to invalid IL or missing references)
		//IL_0113: Unknown result type (might be due to invalid IL or missing references)
		//IL_011e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0124: Unknown result type (might be due to invalid IL or missing references)
		//IL_0129: Unknown result type (might be due to invalid IL or missing references)
		//IL_012b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0130: Unknown result type (might be due to invalid IL or missing references)
		//IL_0138: Unknown result type (might be due to invalid IL or missing references)
		//IL_013a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0140: Unknown result type (might be due to invalid IL or missing references)
		//IL_0141: Unknown result type (might be due to invalid IL or missing references)
		//IL_0144: Unknown result type (might be due to invalid IL or missing references)
		//IL_0149: Unknown result type (might be due to invalid IL or missing references)
		//IL_0153: Unknown result type (might be due to invalid IL or missing references)
		//IL_0154: Unknown result type (might be due to invalid IL or missing references)
		//IL_0156: Unknown result type (might be due to invalid IL or missing references)
		//IL_0158: Unknown result type (might be due to invalid IL or missing references)
		//IL_015d: Unknown result type (might be due to invalid IL or missing references)
		//IL_015f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0165: Unknown result type (might be due to invalid IL or missing references)
		//IL_016a: Unknown result type (might be due to invalid IL or missing references)
		if (wheel.isGrounded)
		{
			float num = 0.005f;
			Transform transform = ((Component)wheel).transform;
			float num2 = 1f;
			Vector3 position = transform.position;
			Vector3 pointVelocity = rigidbody.GetPointVelocity(position);
			Vector3 val = Vector3.Project(pointVelocity, transform.right);
			Vector3 val2 = Vector3.Project(pointVelocity, transform.forward);
			Vector3 up = Vector3.up;
			Debug.DrawRay(position, pointVelocity, Color.blue);
			Vector3 val3 = Vector3.zero;
			if (driveWheel)
			{
				val3 = transform.forward * gas * motorForce;
				rigidbody.AddForceAtPosition(transform.forward * gas * motorForce * num2, position);
				Debug.DrawRay(position, val3 * num, Color.yellow);
			}
			Vector3 val4 = Vector3.ProjectOnPlane(-val2 * rollingFrictionCoefficient * num2, up);
			rigidbody.AddForceAtPosition(val4, position);
			Debug.DrawRay(position, val4 * num, Color.red);
			Vector3 val5 = Vector3.ProjectOnPlane(-val * slidingWheelTractionCurve.Evaluate(((Vector3)(ref pointVelocity)).magnitude) * slidingTractionCoefficient * num2, up);
			rigidbody.AddForceAtPosition(val5, position);
			Debug.DrawRay(position, val5 * num, Color.red);
			Debug.DrawRay(position, (val3 + val4 + val5) * num, Color.green);
		}
	}

	private void UpdateCenterOfMass()
	{
		//IL_0017: Unknown result type (might be due to invalid IL or missing references)
		//IL_001d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0022: Unknown result type (might be due to invalid IL or missing references)
		rigidbody.ResetCenterOfMass();
		rigidbody.centerOfMass += centerOfMassOffset;
	}

	private void UpdateWheelParameter(HoverEngine wheel, WheelLateralAxis wheelLateralAxis, WheelLongitudinalAxis wheelLongitudinalAxis)
	{
		//IL_0026: Unknown result type (might be due to invalid IL or missing references)
		//IL_002b: Unknown result type (might be due to invalid IL or missing references)
		//IL_003c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0041: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ba: Unknown result type (might be due to invalid IL or missing references)
		wheel.hoverForce = hoverForce;
		wheel.hoverDamping = hoverDamping;
		wheel.hoverHeight = hoverHeight;
		wheel.offsetVector = hoverOffsetVector;
		wheel.hoverRadius = hoverRadius;
		Vector3 zero = Vector3.zero;
		zero.y = 0f - wheelWellDepth;
		switch (wheelLateralAxis)
		{
		case WheelLateralAxis.Left:
			zero.x = (0f - trackWidth) / 2f;
			break;
		case WheelLateralAxis.Right:
			zero.x = trackWidth / 2f;
			break;
		}
		switch (wheelLongitudinalAxis)
		{
		case WheelLongitudinalAxis.Front:
			zero.z = wheelBase / 2f;
			break;
		case WheelLongitudinalAxis.Back:
			zero.z = (0f - wheelBase) / 2f;
			break;
		}
		((Component)wheel).transform.localPosition = zero;
	}

	private void UpdateAllWheelParameters()
	{
		AxleGroup[] array = staticAxles;
		for (int i = 0; i < array.Length; i++)
		{
			AxleGroup axleGroup = array[i];
			HoverEngine leftWheel = axleGroup.leftWheel;
			HoverEngine rightWheel = axleGroup.rightWheel;
			UpdateWheelParameter(leftWheel, WheelLateralAxis.Left, axleGroup.wheelLongitudinalAxis);
			UpdateWheelParameter(rightWheel, WheelLateralAxis.Right, axleGroup.wheelLongitudinalAxis);
		}
		array = steerAxles;
		for (int i = 0; i < array.Length; i++)
		{
			AxleGroup axleGroup2 = array[i];
			HoverEngine leftWheel2 = axleGroup2.leftWheel;
			HoverEngine rightWheel2 = axleGroup2.rightWheel;
			UpdateWheelParameter(leftWheel2, WheelLateralAxis.Left, axleGroup2.wheelLongitudinalAxis);
			UpdateWheelParameter(rightWheel2, WheelLateralAxis.Right, axleGroup2.wheelLongitudinalAxis);
		}
	}

	private void FixedUpdate()
	{
		//IL_0022: Unknown result type (might be due to invalid IL or missing references)
		//IL_0027: Unknown result type (might be due to invalid IL or missing references)
		//IL_002e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0039: Unknown result type (might be due to invalid IL or missing references)
		//IL_003e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0043: Unknown result type (might be due to invalid IL or missing references)
		//IL_0047: Unknown result type (might be due to invalid IL or missing references)
		//IL_004c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0053: Unknown result type (might be due to invalid IL or missing references)
		//IL_0058: Unknown result type (might be due to invalid IL or missing references)
		//IL_005f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0186: Unknown result type (might be due to invalid IL or missing references)
		//IL_018b: Unknown result type (might be due to invalid IL or missing references)
		//IL_01a0: Unknown result type (might be due to invalid IL or missing references)
		//IL_01a5: Unknown result type (might be due to invalid IL or missing references)
		//IL_01b6: Unknown result type (might be due to invalid IL or missing references)
		//IL_01c4: Unknown result type (might be due to invalid IL or missing references)
		//IL_01d4: Unknown result type (might be due to invalid IL or missing references)
		//IL_01e2: Unknown result type (might be due to invalid IL or missing references)
		//IL_022e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0233: Unknown result type (might be due to invalid IL or missing references)
		//IL_0239: Unknown result type (might be due to invalid IL or missing references)
		//IL_023e: Unknown result type (might be due to invalid IL or missing references)
		UpdateCenterOfMass();
		UpdateAllWheelParameters();
		if (!Object.op_Implicit((Object)(object)inputBank))
		{
			return;
		}
		Vector3 moveVector = inputBank.moveVector;
		Vector3 val = Vector3.ProjectOnPlane(inputBank.aimDirection, ((Component)this).transform.up);
		Vector3 normalized = ((Vector3)(ref val)).normalized;
		float num = Mathf.Clamp(Util.AngleSigned(((Component)this).transform.forward, normalized, ((Component)this).transform.up), 0f - maxSteerAngle, maxSteerAngle);
		float magnitude = ((Vector3)(ref moveVector)).magnitude;
		AxleGroup[] array = staticAxles;
		for (int i = 0; i < array.Length; i++)
		{
			AxleGroup axleGroup = array[i];
			HoverEngine leftWheel = axleGroup.leftWheel;
			HoverEngine rightWheel = axleGroup.rightWheel;
			ApplyWheelForces(leftWheel, magnitude, axleGroup.isDriven, axleGroup.slidingTractionCurve);
			ApplyWheelForces(rightWheel, magnitude, axleGroup.isDriven, axleGroup.slidingTractionCurve);
		}
		array = steerAxles;
		for (int i = 0; i < array.Length; i++)
		{
			AxleGroup axleGroup2 = array[i];
			HoverEngine leftWheel2 = axleGroup2.leftWheel;
			HoverEngine rightWheel2 = axleGroup2.rightWheel;
			float num2 = maxTurningRadius / Mathf.Abs(num / maxSteerAngle);
			float num3 = Mathf.Atan(wheelBase / (num2 - trackWidth / 2f)) * 57.29578f;
			float num4 = Mathf.Atan(wheelBase / (num2 + trackWidth / 2f)) * 57.29578f;
			Quaternion localRotation = Quaternion.Euler(0f, num3 * Mathf.Sign(num), 0f);
			Quaternion localRotation2 = Quaternion.Euler(0f, num4 * Mathf.Sign(num), 0f);
			if (num <= 0f)
			{
				((Component)leftWheel2).transform.localRotation = localRotation;
				((Component)rightWheel2).transform.localRotation = localRotation2;
			}
			else
			{
				((Component)leftWheel2).transform.localRotation = localRotation2;
				((Component)rightWheel2).transform.localRotation = localRotation;
			}
			ApplyWheelForces(leftWheel2, magnitude, axleGroup2.isDriven, axleGroup2.slidingTractionCurve);
			ApplyWheelForces(rightWheel2, magnitude, axleGroup2.isDriven, axleGroup2.slidingTractionCurve);
		}
		Debug.DrawRay(((Component)this).transform.position, normalized * 5f, Color.blue);
	}

	private void OnDrawGizmos()
	{
		//IL_000d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0023: Unknown result type (might be due to invalid IL or missing references)
		//IL_0028: Unknown result type (might be due to invalid IL or missing references)
		if (Object.op_Implicit((Object)(object)rigidbody))
		{
			Gizmos.color = Color.red;
			Gizmos.DrawSphere(((Component)this).transform.TransformPoint(rigidbody.centerOfMass), 0.3f);
		}
	}
}
