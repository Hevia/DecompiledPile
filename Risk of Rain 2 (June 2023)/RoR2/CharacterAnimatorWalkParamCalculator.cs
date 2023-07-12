using System;
using UnityEngine;

namespace RoR2;

public struct CharacterAnimatorWalkParamCalculator
{
	private float animatorReferenceMagnitudeVelocity;

	private float animatorReferenceAngleVelocity;

	public Vector2 animatorWalkSpeed { get; private set; }

	public float remainingTurnAngle { get; private set; }

	public void Update(Vector3 worldMoveVector, Vector3 animatorForward, in BodyAnimatorSmoothingParameters.SmoothingParameters smoothingParameters, float deltaTime)
	{
		//IL_0003: Unknown result type (might be due to invalid IL or missing references)
		//IL_0009: Unknown result type (might be due to invalid IL or missing references)
		//IL_000e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0013: Unknown result type (might be due to invalid IL or missing references)
		//IL_0014: Unknown result type (might be due to invalid IL or missing references)
		//IL_0016: Unknown result type (might be due to invalid IL or missing references)
		//IL_0021: Unknown result type (might be due to invalid IL or missing references)
		//IL_0022: Unknown result type (might be due to invalid IL or missing references)
		//IL_004b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0050: Unknown result type (might be due to invalid IL or missing references)
		//IL_005a: Unknown result type (might be due to invalid IL or missing references)
		//IL_005f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0077: Unknown result type (might be due to invalid IL or missing references)
		//IL_007d: Unknown result type (might be due to invalid IL or missing references)
		//IL_00e9: Unknown result type (might be due to invalid IL or missing references)
		//IL_00f0: Unknown result type (might be due to invalid IL or missing references)
		Vector3 val = Vector3.Cross(Vector3.up, animatorForward);
		float num = Vector3.Dot(worldMoveVector, animatorForward);
		float num2 = Vector3.Dot(worldMoveVector, val);
		Vector2 val2 = default(Vector2);
		((Vector2)(ref val2))._002Ector(num, num2);
		float magnitude = ((Vector2)(ref val2)).magnitude;
		float num3 = ((magnitude > 0f) ? Vector2.SignedAngle(Vector2.right, val2) : 0f);
		Vector2 val3 = animatorWalkSpeed;
		float magnitude2 = ((Vector2)(ref val3)).magnitude;
		float num4 = ((magnitude2 > 0f) ? Vector2.SignedAngle(Vector2.right, animatorWalkSpeed) : 0f);
		float num5 = Mathf.SmoothDamp(magnitude2, magnitude, ref animatorReferenceMagnitudeVelocity, smoothingParameters.walkMagnitudeSmoothDamp, float.PositiveInfinity, deltaTime);
		float num6 = Mathf.SmoothDampAngle(num4, num3, ref animatorReferenceAngleVelocity, smoothingParameters.walkAngleSmoothDamp, float.PositiveInfinity, deltaTime);
		remainingTurnAngle = num6 - num3;
		animatorWalkSpeed = new Vector2(Mathf.Cos(num6 * (MathF.PI / 180f)), Mathf.Sin(num6 * (MathF.PI / 180f))) * num5;
	}
}
