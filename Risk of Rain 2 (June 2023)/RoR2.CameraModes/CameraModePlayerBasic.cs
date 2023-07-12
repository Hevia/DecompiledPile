using HG.BlendableTypes;
using Rewired;
using UnityEngine;

namespace RoR2.CameraModes;

public class CameraModePlayerBasic : CameraModeBase
{
	protected class InstanceData
	{
		public float currentCameraDistance;

		public float cameraDistanceVelocity;

		public float stickAimPreviousAcceleratedMagnitude;

		public float minPitch;

		public float maxPitch;

		public PitchYawPair pitchYaw;

		public CameraRigController.AimAssistInfo lastAimAssist;

		public CameraRigController.AimAssistInfo aimAssist;

		public HurtBox lastCrosshairHurtBox;

		public bool hasEverHadTarget;

		public float neutralFov;

		public float neutralFovVelocity;

		public void SetPitchYawFromLookVector(Vector3 lookVector)
		{
			//IL_0000: Unknown result type (might be due to invalid IL or missing references)
			//IL_0001: Unknown result type (might be due to invalid IL or missing references)
			//IL_0002: Unknown result type (might be due to invalid IL or missing references)
			//IL_0008: Unknown result type (might be due to invalid IL or missing references)
			//IL_000f: Unknown result type (might be due to invalid IL or missing references)
			//IL_0015: Unknown result type (might be due to invalid IL or missing references)
			//IL_0029: Unknown result type (might be due to invalid IL or missing references)
			//IL_0047: Unknown result type (might be due to invalid IL or missing references)
			//IL_004d: Unknown result type (might be due to invalid IL or missing references)
			float num = Mathf.Sqrt(lookVector.x * lookVector.x + lookVector.z * lookVector.z);
			pitchYaw.pitch = Mathf.Atan2(0f - lookVector.y, num) * 57.29578f;
			pitchYaw.yaw = Mathf.Repeat(Mathf.Atan2(lookVector.x, lookVector.z) * 57.29578f, 360f);
		}
	}

	public bool isSpectatorMode;

	public static CameraModePlayerBasic playerBasic = new CameraModePlayerBasic
	{
		isSpectatorMode = false
	};

	public static CameraModePlayerBasic spectator = new CameraModePlayerBasic
	{
		isSpectatorMode = true
	};

	protected override object CreateInstanceData(CameraRigController cameraRigController)
	{
		return new InstanceData();
	}

	protected override void OnInstallInternal(object rawInstancedata, CameraRigController cameraRigController)
	{
		base.OnInstallInternal(rawInstancedata, cameraRigController);
		((InstanceData)rawInstancedata).neutralFov = cameraRigController.baseFov;
	}

	public override bool IsSpectating(CameraRigController cameraRigController)
	{
		return isSpectatorMode;
	}

	protected override void UpdateInternal(object rawInstanceData, in CameraModeContext context, out UpdateResult result)
	{
		//IL_0036: Unknown result type (might be due to invalid IL or missing references)
		//IL_003b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0048: Unknown result type (might be due to invalid IL or missing references)
		//IL_004d: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a8: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ad: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b2: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b7: Unknown result type (might be due to invalid IL or missing references)
		//IL_00d5: Unknown result type (might be due to invalid IL or missing references)
		//IL_00e1: Unknown result type (might be due to invalid IL or missing references)
		//IL_00e6: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ea: Unknown result type (might be due to invalid IL or missing references)
		//IL_0106: Unknown result type (might be due to invalid IL or missing references)
		//IL_0118: Unknown result type (might be due to invalid IL or missing references)
		//IL_0143: Unknown result type (might be due to invalid IL or missing references)
		//IL_014f: Unknown result type (might be due to invalid IL or missing references)
		//IL_017e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0183: Unknown result type (might be due to invalid IL or missing references)
		//IL_01a3: Unknown result type (might be due to invalid IL or missing references)
		//IL_01a8: Unknown result type (might be due to invalid IL or missing references)
		//IL_01aa: Unknown result type (might be due to invalid IL or missing references)
		//IL_01ac: Unknown result type (might be due to invalid IL or missing references)
		//IL_01b0: Unknown result type (might be due to invalid IL or missing references)
		//IL_01b5: Unknown result type (might be due to invalid IL or missing references)
		//IL_01ba: Unknown result type (might be due to invalid IL or missing references)
		//IL_01bf: Unknown result type (might be due to invalid IL or missing references)
		//IL_01c4: Unknown result type (might be due to invalid IL or missing references)
		//IL_01c6: Unknown result type (might be due to invalid IL or missing references)
		//IL_01cb: Unknown result type (might be due to invalid IL or missing references)
		//IL_0210: Unknown result type (might be due to invalid IL or missing references)
		//IL_0212: Unknown result type (might be due to invalid IL or missing references)
		//IL_021a: Unknown result type (might be due to invalid IL or missing references)
		//IL_021c: Unknown result type (might be due to invalid IL or missing references)
		//IL_021e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0227: Unknown result type (might be due to invalid IL or missing references)
		//IL_0240: Unknown result type (might be due to invalid IL or missing references)
		//IL_0247: Unknown result type (might be due to invalid IL or missing references)
		//IL_024e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0253: Unknown result type (might be due to invalid IL or missing references)
		//IL_0264: Unknown result type (might be due to invalid IL or missing references)
		//IL_026b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0272: Unknown result type (might be due to invalid IL or missing references)
		//IL_0277: Unknown result type (might be due to invalid IL or missing references)
		//IL_02c3: Unknown result type (might be due to invalid IL or missing references)
		//IL_02c7: Unknown result type (might be due to invalid IL or missing references)
		//IL_02d2: Unknown result type (might be due to invalid IL or missing references)
		//IL_02d7: Unknown result type (might be due to invalid IL or missing references)
		//IL_02dc: Unknown result type (might be due to invalid IL or missing references)
		//IL_02e4: Unknown result type (might be due to invalid IL or missing references)
		//IL_02e6: Unknown result type (might be due to invalid IL or missing references)
		//IL_02f1: Unknown result type (might be due to invalid IL or missing references)
		//IL_02f3: Unknown result type (might be due to invalid IL or missing references)
		InstanceData instanceData = (InstanceData)rawInstanceData;
		CameraRigController cameraRigController = context.cameraInfo.cameraRigController;
		CameraTargetParams targetParams = context.targetInfo.targetParams;
		float fov = context.cameraInfo.baseFov;
		Quaternion val = context.cameraInfo.previousCameraState.rotation;
		Vector3 position = context.cameraInfo.previousCameraState.position;
		GameObject firstPersonTarget = null;
		float num = cameraRigController.baseFov;
		if (context.targetInfo.isSprinting)
		{
			num *= 1.3f;
		}
		instanceData.neutralFov = Mathf.SmoothDamp(instanceData.neutralFov, num, ref instanceData.neutralFovVelocity, 0.2f, float.PositiveInfinity, Time.deltaTime);
		CharacterCameraParamsData dest = CharacterCameraParamsData.basic;
		dest.fov = BlendableFloat.op_Implicit(instanceData.neutralFov);
		Vector2 val2 = Vector2.zero;
		if (Object.op_Implicit((Object)(object)targetParams))
		{
			CharacterCameraParamsData.Blend(in targetParams.currentCameraParamsData, ref dest, 1f);
			fov = dest.fov.value;
			val2 = targetParams.recoil;
		}
		if (dest.isFirstPerson.value)
		{
			firstPersonTarget = context.targetInfo.target;
		}
		instanceData.minPitch = dest.minPitch.value;
		instanceData.maxPitch = dest.maxPitch.value;
		float pitch = instanceData.pitchYaw.pitch;
		float yaw = instanceData.pitchYaw.yaw;
		pitch += val2.y;
		yaw += val2.x;
		pitch = Mathf.Clamp(pitch, instanceData.minPitch, instanceData.maxPitch);
		yaw = Mathf.Repeat(yaw, 360f);
		Vector3 targetPivotPosition = CalculateTargetPivotPosition(in context);
		if (Object.op_Implicit((Object)(object)context.targetInfo.target))
		{
			val = Quaternion.Euler(pitch, yaw, 0f);
			Vector3 val3 = targetPivotPosition + val * dest.idealLocalCameraPos.value - targetPivotPosition;
			float magnitude = ((Vector3)(ref val3)).magnitude;
			float num2 = (1f + pitch / -90f) * 0.5f;
			magnitude *= Mathf.Sqrt(1f - num2);
			if (magnitude < 0.25f)
			{
				magnitude = 0.25f;
			}
			Ray val4 = default(Ray);
			((Ray)(ref val4))._002Ector(targetPivotPosition, val3);
			float num3 = cameraRigController.Raycast(new Ray(targetPivotPosition, val3), magnitude, dest.wallCushion.value - 0.01f);
			Debug.DrawRay(((Ray)(ref val4)).origin, ((Ray)(ref val4)).direction * magnitude, Color.yellow, Time.deltaTime);
			Debug.DrawRay(((Ray)(ref val4)).origin, ((Ray)(ref val4)).direction * num3, Color.red, Time.deltaTime);
			if (instanceData.currentCameraDistance >= num3)
			{
				instanceData.currentCameraDistance = num3;
				instanceData.cameraDistanceVelocity = 0f;
			}
			else
			{
				instanceData.currentCameraDistance = Mathf.SmoothDamp(instanceData.currentCameraDistance, num3, ref instanceData.cameraDistanceVelocity, 0.5f);
			}
			position = targetPivotPosition + ((Vector3)(ref val3)).normalized * instanceData.currentCameraDistance;
		}
		result.cameraState.position = position;
		result.cameraState.rotation = val;
		result.cameraState.fov = fov;
		result.showSprintParticles = context.targetInfo.isSprinting;
		result.firstPersonTarget = firstPersonTarget;
		UpdateCrosshair(rawInstanceData, in context, in result.cameraState, in targetPivotPosition, out result.crosshairWorldPosition);
	}

	protected override void CollectLookInputInternal(object rawInstanceData, in CameraModeContext context, out CollectLookInputResult output)
	{
		//IL_0039: Unknown result type (might be due to invalid IL or missing references)
		//IL_003e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0043: Unknown result type (might be due to invalid IL or missing references)
		//IL_006c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0076: Expected O, but got Unknown
		//IL_0159: Unknown result type (might be due to invalid IL or missing references)
		//IL_015b: Unknown result type (might be due to invalid IL or missing references)
		//IL_015f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0164: Unknown result type (might be due to invalid IL or missing references)
		//IL_0169: Unknown result type (might be due to invalid IL or missing references)
		//IL_016b: Unknown result type (might be due to invalid IL or missing references)
		//IL_016d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0171: Unknown result type (might be due to invalid IL or missing references)
		//IL_0176: Unknown result type (might be due to invalid IL or missing references)
		//IL_017b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0186: Unknown result type (might be due to invalid IL or missing references)
		//IL_018d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0192: Unknown result type (might be due to invalid IL or missing references)
		//IL_0195: Unknown result type (might be due to invalid IL or missing references)
		//IL_0197: Unknown result type (might be due to invalid IL or missing references)
		//IL_0199: Unknown result type (might be due to invalid IL or missing references)
		//IL_019e: Unknown result type (might be due to invalid IL or missing references)
		//IL_01aa: Unknown result type (might be due to invalid IL or missing references)
		//IL_01c1: Unknown result type (might be due to invalid IL or missing references)
		//IL_01c6: Unknown result type (might be due to invalid IL or missing references)
		//IL_01e5: Unknown result type (might be due to invalid IL or missing references)
		//IL_01ef: Unknown result type (might be due to invalid IL or missing references)
		//IL_01f4: Unknown result type (might be due to invalid IL or missing references)
		InstanceData instanceData = (InstanceData)rawInstanceData;
		ref readonly CameraInfo cameraInfo = ref context.cameraInfo;
		ref readonly ViewerInfo viewerInfo = ref context.viewerInfo;
		ref readonly TargetInfo targetInfo = ref context.targetInfo;
		bool hasCursor = viewerInfo.hasCursor;
		Player inputPlayer = viewerInfo.inputPlayer;
		UserProfile userProfile = viewerInfo.userProfile;
		ICameraStateProvider overrideCam = cameraInfo.overrideCam;
		output.lookInput = Vector2.op_Implicit(Vector3.zero);
		if (!hasCursor && viewerInfo.inputPlayer != null && userProfile != null && !viewerInfo.isUIFocused && (!Object.op_Implicit((Object)overrideCam) || overrideCam.IsUserLookAllowed(cameraInfo.cameraRigController)))
		{
			Vector2 val = default(Vector2);
			((Vector2)(ref val))._002Ector(inputPlayer.GetAxisRaw(2), inputPlayer.GetAxisRaw(3));
			Vector2 aimStickVector = default(Vector2);
			((Vector2)(ref aimStickVector))._002Ector(inputPlayer.GetAxisRaw(16), inputPlayer.GetAxisRaw(17));
			ConditionalNegate(ref val.x, userProfile.mouseLookInvertX);
			ConditionalNegate(ref val.y, userProfile.mouseLookInvertY);
			ConditionalNegate(ref aimStickVector.x, userProfile.stickLookInvertX);
			ConditionalNegate(ref aimStickVector.y, userProfile.stickLookInvertY);
			PerformStickPostProcessing(instanceData, in context, ref aimStickVector);
			float mouseLookSensitivity = userProfile.mouseLookSensitivity;
			float num = userProfile.stickLookSensitivity * CameraRigController.aimStickGlobalScale.value * 45f;
			Vector2 val2 = default(Vector2);
			((Vector2)(ref val2))._002Ector(userProfile.mouseLookScaleX, userProfile.mouseLookScaleY);
			Vector2 val3 = default(Vector2);
			((Vector2)(ref val3))._002Ector(userProfile.stickLookScaleX, userProfile.stickLookScaleY);
			val *= val2 * mouseLookSensitivity;
			aimStickVector *= val3 * num;
			PerformAimAssist(in context, ref aimStickVector);
			aimStickVector *= Time.deltaTime;
			output.lookInput = val + aimStickVector;
		}
		ref Vector2 lookInput = ref output.lookInput;
		lookInput *= cameraInfo.previousCameraState.fov / cameraInfo.baseFov;
		if (targetInfo.isSprinting && CameraRigController.enableSprintSensitivitySlowdown.value)
		{
			ref Vector2 lookInput2 = ref output.lookInput;
			lookInput2 *= 0.5f;
		}
		static void ConditionalNegate(ref float value, bool condition)
		{
			value = (condition ? (0f - value) : value);
		}
	}

	private void PerformStickPostProcessing(InstanceData instanceData, in CameraModeContext context, ref Vector2 aimStickVector)
	{
		//IL_0009: Unknown result type (might be due to invalid IL or missing references)
		//IL_000f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0015: Unknown result type (might be due to invalid IL or missing references)
		//IL_0060: Unknown result type (might be due to invalid IL or missing references)
		//IL_0069: Unknown result type (might be due to invalid IL or missing references)
		//IL_0071: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b5: Unknown result type (might be due to invalid IL or missing references)
		//IL_00be: Unknown result type (might be due to invalid IL or missing references)
		//IL_00c6: Unknown result type (might be due to invalid IL or missing references)
		//IL_00e5: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ee: Unknown result type (might be due to invalid IL or missing references)
		//IL_00f6: Unknown result type (might be due to invalid IL or missing references)
		//IL_0106: Unknown result type (might be due to invalid IL or missing references)
		//IL_010e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0113: Unknown result type (might be due to invalid IL or missing references)
		float magnitude = ((Vector2)(ref aimStickVector)).magnitude;
		float num = magnitude;
		_ = Vector2.zero;
		_ = Vector2.zero;
		_ = Vector2.zero;
		if (CameraRigController.aimStickDualZoneSmoothing.value != 0f)
		{
			float num2 = Time.deltaTime / CameraRigController.aimStickDualZoneSmoothing.value;
			num = (instanceData.stickAimPreviousAcceleratedMagnitude = Mathf.Min(Mathf.MoveTowards(instanceData.stickAimPreviousAcceleratedMagnitude, magnitude, num2), magnitude));
			if (magnitude == 0f)
			{
				_ = Vector2.zero;
			}
			else
			{
				_ = aimStickVector * (num / magnitude);
			}
		}
		float num3 = num;
		float value = CameraRigController.aimStickDualZoneSlope.value;
		float num4 = ((!(num3 <= CameraRigController.aimStickDualZoneThreshold.value)) ? (1f - value) : 0f);
		num = value * num3 + num4;
		if (magnitude == 0f)
		{
			_ = Vector2.zero;
		}
		else
		{
			_ = aimStickVector * (num / magnitude);
		}
		num = Mathf.Pow(num, CameraRigController.aimStickExponent.value);
		if (magnitude == 0f)
		{
			_ = Vector2.zero;
		}
		else
		{
			_ = aimStickVector * (num / magnitude);
		}
		if (magnitude != 0f)
		{
			aimStickVector *= num / magnitude;
		}
	}

	private void PerformAimAssist(in CameraModeContext context, ref Vector2 aimStickVector)
	{
		//IL_007d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0082: Unknown result type (might be due to invalid IL or missing references)
		//IL_0084: Unknown result type (might be due to invalid IL or missing references)
		//IL_008b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0090: Unknown result type (might be due to invalid IL or missing references)
		//IL_00d8: Unknown result type (might be due to invalid IL or missing references)
		//IL_00dd: Unknown result type (might be due to invalid IL or missing references)
		//IL_00e2: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ec: Unknown result type (might be due to invalid IL or missing references)
		//IL_00f1: Unknown result type (might be due to invalid IL or missing references)
		//IL_00f6: Unknown result type (might be due to invalid IL or missing references)
		//IL_00f8: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ff: Unknown result type (might be due to invalid IL or missing references)
		//IL_0128: Unknown result type (might be due to invalid IL or missing references)
		//IL_012a: Unknown result type (might be due to invalid IL or missing references)
		//IL_012c: Unknown result type (might be due to invalid IL or missing references)
		//IL_012e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0133: Unknown result type (might be due to invalid IL or missing references)
		//IL_0138: Unknown result type (might be due to invalid IL or missing references)
		//IL_013d: Unknown result type (might be due to invalid IL or missing references)
		//IL_013f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0144: Unknown result type (might be due to invalid IL or missing references)
		//IL_017d: Unknown result type (might be due to invalid IL or missing references)
		//IL_017f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0184: Unknown result type (might be due to invalid IL or missing references)
		//IL_0186: Unknown result type (might be due to invalid IL or missing references)
		//IL_01b9: Unknown result type (might be due to invalid IL or missing references)
		//IL_01bb: Unknown result type (might be due to invalid IL or missing references)
		//IL_01d5: Unknown result type (might be due to invalid IL or missing references)
		//IL_01da: Unknown result type (might be due to invalid IL or missing references)
		//IL_0207: Unknown result type (might be due to invalid IL or missing references)
		//IL_020b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0210: Unknown result type (might be due to invalid IL or missing references)
		//IL_021a: Unknown result type (might be due to invalid IL or missing references)
		//IL_021c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0221: Unknown result type (might be due to invalid IL or missing references)
		//IL_0223: Unknown result type (might be due to invalid IL or missing references)
		//IL_0252: Unknown result type (might be due to invalid IL or missing references)
		//IL_0257: Unknown result type (might be due to invalid IL or missing references)
		//IL_025c: Unknown result type (might be due to invalid IL or missing references)
		//IL_025f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0261: Unknown result type (might be due to invalid IL or missing references)
		ref readonly TargetInfo targetInfo = ref context.targetInfo;
		ref readonly CameraInfo cameraInfo = ref context.cameraInfo;
		if (!context.targetInfo.isViewerControlled || context.targetInfo.isSprinting)
		{
			return;
		}
		Camera sceneCam = cameraInfo.sceneCam;
		AimAssistTarget aimAssistTarget = null;
		AimAssistTarget aimAssistTarget2 = null;
		float value = CameraRigController.aimStickAssistMinSize.value;
		float num = value * CameraRigController.aimStickAssistMaxSize.value;
		float value2 = CameraRigController.aimStickAssistMaxSlowdownScale.value;
		float value3 = CameraRigController.aimStickAssistMinSlowdownScale.value;
		float num2 = 0f;
		float value4 = 0f;
		float num3 = 0f;
		Vector2 val = Vector2.zero;
		_ = Vector2.zero;
		Vector2 normalized = ((Vector2)(ref aimStickVector)).normalized;
		Vector2 val2 = default(Vector2);
		((Vector2)(ref val2))._002Ector(0.5f, 0.5f);
		for (int i = 0; i < AimAssistTarget.instancesList.Count; i++)
		{
			AimAssistTarget aimAssistTarget3 = AimAssistTarget.instancesList[i];
			if (aimAssistTarget3.teamComponent.teamIndex == targetInfo.teamIndex)
			{
				continue;
			}
			Vector3 val3 = sceneCam.WorldToViewportPoint(aimAssistTarget3.point0.position);
			Vector3 val4 = sceneCam.WorldToViewportPoint(aimAssistTarget3.point1.position);
			float num4 = Mathf.Lerp(val3.z, val4.z, 0.5f);
			if (!(num4 <= 3f))
			{
				float num5 = 1f / num4;
				Vector2 val5 = Vector2.op_Implicit(Util.ClosestPointOnLine(val3, val4, Vector2.op_Implicit(val2))) - val2;
				float num6 = Mathf.Clamp01(Util.Remap(((Vector2)(ref val5)).magnitude, value * aimAssistTarget3.assistScale * num5, num * aimAssistTarget3.assistScale * num5, 1f, 0f));
				float num7 = Mathf.Clamp01(Vector3.Dot(Vector2.op_Implicit(val5), Vector2.op_Implicit(normalized)));
				float num8 = num7 * num6;
				if (num2 < num6)
				{
					num2 = num6;
					aimAssistTarget2 = aimAssistTarget3;
				}
				if (num8 > num3)
				{
					num2 = num6;
					value4 = num7;
					aimAssistTarget = aimAssistTarget3;
					val = val5;
				}
			}
		}
		Vector2 val6 = aimStickVector;
		if (Object.op_Implicit((Object)(object)aimAssistTarget2))
		{
			float num9 = Mathf.Clamp01(Util.Remap(1f - num2, 0f, 1f, value2, value3));
			val6 *= num9;
		}
		if (Object.op_Implicit((Object)(object)aimAssistTarget))
		{
			val6 = Vector2.op_Implicit(Vector3.RotateTowards(Vector2.op_Implicit(val6), Vector2.op_Implicit(val), Util.Remap(value4, 1f, 0f, CameraRigController.aimStickAssistMaxDelta.value, CameraRigController.aimStickAssistMinDelta.value), 0f));
		}
		aimStickVector = val6;
	}

	protected override void ApplyLookInputInternal(object rawInstanceData, in CameraModeContext context, in ApplyLookInputArgs input)
	{
		//IL_00c6: Unknown result type (might be due to invalid IL or missing references)
		InstanceData instanceData = (InstanceData)rawInstanceData;
		ref readonly TargetInfo targetInfo = ref context.targetInfo;
		if (targetInfo.isViewerControlled)
		{
			float minPitch = instanceData.minPitch;
			float maxPitch = instanceData.maxPitch;
			instanceData.pitchYaw.pitch = Mathf.Clamp(instanceData.pitchYaw.pitch - input.lookInput.y, minPitch, maxPitch);
			instanceData.pitchYaw.yaw += input.lookInput.x;
			if (Object.op_Implicit((Object)(object)targetInfo.networkedViewAngles) && targetInfo.networkedViewAngles.hasEffectiveAuthority)
			{
				targetInfo.networkedViewAngles.viewAngles = instanceData.pitchYaw;
			}
		}
		else if (Object.op_Implicit((Object)(object)targetInfo.networkedViewAngles))
		{
			instanceData.pitchYaw = targetInfo.networkedViewAngles.viewAngles;
		}
		else if (Object.op_Implicit((Object)(object)targetInfo.inputBank))
		{
			instanceData.SetPitchYawFromLookVector(targetInfo.inputBank.aimDirection);
		}
	}

	protected void UpdateCrosshair(object rawInstanceData, in CameraModeContext context, in CameraState cameraState, in Vector3 targetPivotPosition, out Vector3 crosshairWorldPosition)
	{
		//IL_0015: Unknown result type (might be due to invalid IL or missing references)
		//IL_001c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0022: Unknown result type (might be due to invalid IL or missing references)
		//IL_0027: Unknown result type (might be due to invalid IL or missing references)
		//IL_0033: Unknown result type (might be due to invalid IL or missing references)
		//IL_0039: Unknown result type (might be due to invalid IL or missing references)
		//IL_0053: Unknown result type (might be due to invalid IL or missing references)
		//IL_0066: Unknown result type (might be due to invalid IL or missing references)
		//IL_008f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0094: Unknown result type (might be due to invalid IL or missing references)
		//IL_0182: Unknown result type (might be due to invalid IL or missing references)
		//IL_0187: Unknown result type (might be due to invalid IL or missing references)
		//IL_019e: Unknown result type (might be due to invalid IL or missing references)
		//IL_01a3: Unknown result type (might be due to invalid IL or missing references)
		//IL_0255: Unknown result type (might be due to invalid IL or missing references)
		//IL_025a: Unknown result type (might be due to invalid IL or missing references)
		//IL_026e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0273: Unknown result type (might be due to invalid IL or missing references)
		//IL_0278: Unknown result type (might be due to invalid IL or missing references)
		//IL_0292: Unknown result type (might be due to invalid IL or missing references)
		//IL_0297: Unknown result type (might be due to invalid IL or missing references)
		InstanceData instanceData = (InstanceData)rawInstanceData;
		instanceData.lastAimAssist = instanceData.aimAssist;
		Ray crosshairRaycastRay = GetCrosshairRaycastRay(in context, Vector2.zero, targetPivotPosition, in cameraState);
		bool flag = false;
		instanceData.lastCrosshairHurtBox = null;
		RaycastHit val = default(RaycastHit);
		RaycastHit[] array = Physics.RaycastAll(crosshairRaycastRay, context.cameraInfo.cameraRigController.maxAimRaycastDistance, LayerMask.op_Implicit(LayerIndex.world.mask) | LayerMask.op_Implicit(LayerIndex.entityPrecise.mask), (QueryTriggerInteraction)1);
		float num = float.PositiveInfinity;
		int num2 = -1;
		for (int i = 0; i < array.Length; i++)
		{
			RaycastHit val2 = array[i];
			HurtBox hurtBox = ((Component)((RaycastHit)(ref val2)).collider).GetComponent<HurtBox>();
			EntityLocator component = ((Component)((RaycastHit)(ref val2)).collider).GetComponent<EntityLocator>();
			float distance = ((RaycastHit)(ref val2)).distance;
			if (distance <= 3f || num <= distance)
			{
				continue;
			}
			if (Object.op_Implicit((Object)(object)hurtBox))
			{
				if (hurtBox.teamIndex == context.targetInfo.teamIndex)
				{
					continue;
				}
				if (Object.op_Implicit((Object)(object)hurtBox.healthComponent) && hurtBox.healthComponent.dontShowHealthbar)
				{
					hurtBox = null;
				}
			}
			if (Object.op_Implicit((Object)(object)component))
			{
				VehicleSeat vehicleSeat = (Object.op_Implicit((Object)(object)component.entity) ? component.entity.GetComponent<VehicleSeat>() : null);
				if (Object.op_Implicit((Object)(object)vehicleSeat) && (Object)(object)vehicleSeat.currentPassengerBody == (Object)(object)context.targetInfo.body)
				{
					continue;
				}
			}
			num = distance;
			num2 = i;
			instanceData.lastCrosshairHurtBox = hurtBox;
		}
		if (num2 != -1)
		{
			flag = true;
			val = array[num2];
		}
		instanceData.aimAssist.aimAssistHurtbox = null;
		if (flag)
		{
			crosshairWorldPosition = ((RaycastHit)(ref val)).point;
			float num3 = 1000f;
			if (!(((RaycastHit)(ref val)).distance < num3))
			{
				return;
			}
			HurtBox component2 = ((Component)((RaycastHit)(ref val)).collider).GetComponent<HurtBox>();
			if (!Object.op_Implicit((Object)(object)component2))
			{
				return;
			}
			HealthComponent healthComponent = component2.healthComponent;
			if (!Object.op_Implicit((Object)(object)healthComponent))
			{
				return;
			}
			TeamComponent component3 = ((Component)healthComponent).GetComponent<TeamComponent>();
			if (Object.op_Implicit((Object)(object)component3) && component3.teamIndex != context.targetInfo.teamIndex && component3.teamIndex != TeamIndex.None)
			{
				HurtBox hurtBox2 = healthComponent.body?.mainHurtBox;
				if (Object.op_Implicit((Object)(object)hurtBox2))
				{
					instanceData.aimAssist.aimAssistHurtbox = hurtBox2;
					instanceData.aimAssist.worldPosition = ((RaycastHit)(ref val)).point;
					instanceData.aimAssist.localPositionOnHurtbox = ((Component)hurtBox2).transform.InverseTransformPoint(((RaycastHit)(ref val)).point);
				}
			}
		}
		else
		{
			crosshairWorldPosition = ((Ray)(ref crosshairRaycastRay)).GetPoint(context.cameraInfo.cameraRigController.maxAimRaycastDistance);
		}
	}

	private Ray GetCrosshairRaycastRay(in CameraModeContext context, Vector2 crosshairOffset, Vector3 raycastStartPlanePoint, in CameraState cameraState)
	{
		//IL_0014: Unknown result type (might be due to invalid IL or missing references)
		//IL_001a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0037: Unknown result type (might be due to invalid IL or missing references)
		//IL_003f: Unknown result type (might be due to invalid IL or missing references)
		//IL_004c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0051: Unknown result type (might be due to invalid IL or missing references)
		//IL_0054: Unknown result type (might be due to invalid IL or missing references)
		//IL_0059: Unknown result type (might be due to invalid IL or missing references)
		//IL_005a: Unknown result type (might be due to invalid IL or missing references)
		//IL_005f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0062: Unknown result type (might be due to invalid IL or missing references)
		//IL_0067: Unknown result type (might be due to invalid IL or missing references)
		//IL_0068: Unknown result type (might be due to invalid IL or missing references)
		//IL_006f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0074: Unknown result type (might be due to invalid IL or missing references)
		//IL_0079: Unknown result type (might be due to invalid IL or missing references)
		//IL_007e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0083: Unknown result type (might be due to invalid IL or missing references)
		//IL_0084: Unknown result type (might be due to invalid IL or missing references)
		//IL_0089: Unknown result type (might be due to invalid IL or missing references)
		//IL_008a: Unknown result type (might be due to invalid IL or missing references)
		//IL_008f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0094: Unknown result type (might be due to invalid IL or missing references)
		if (!Object.op_Implicit((Object)(object)context.cameraInfo.sceneCam))
		{
			return default(Ray);
		}
		float fov = cameraState.fov;
		float num = fov * context.cameraInfo.sceneCam.aspect;
		Quaternion val = Quaternion.Euler(crosshairOffset.y * fov, crosshairOffset.x * num, 0f);
		val = cameraState.rotation * val;
		return new Ray(Vector3.ProjectOnPlane(cameraState.position - raycastStartPlanePoint, cameraState.rotation * Vector3.forward) + raycastStartPlanePoint, val * Vector3.forward);
	}

	private Vector3 CalculateTargetPivotPosition(in CameraModeContext context)
	{
		//IL_0023: Unknown result type (might be due to invalid IL or missing references)
		//IL_0028: Unknown result type (might be due to invalid IL or missing references)
		//IL_003a: Unknown result type (might be due to invalid IL or missing references)
		//IL_003f: Unknown result type (might be due to invalid IL or missing references)
		//IL_004d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0056: Unknown result type (might be due to invalid IL or missing references)
		//IL_0075: Unknown result type (might be due to invalid IL or missing references)
		//IL_007a: Unknown result type (might be due to invalid IL or missing references)
		//IL_007f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0089: Unknown result type (might be due to invalid IL or missing references)
		//IL_008b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0091: Unknown result type (might be due to invalid IL or missing references)
		//IL_0093: Unknown result type (might be due to invalid IL or missing references)
		//IL_0094: Unknown result type (might be due to invalid IL or missing references)
		//IL_0099: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a6: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a7: Unknown result type (might be due to invalid IL or missing references)
		//IL_00af: Unknown result type (might be due to invalid IL or missing references)
		//IL_00cc: Unknown result type (might be due to invalid IL or missing references)
		//IL_00d3: Unknown result type (might be due to invalid IL or missing references)
		//IL_00da: Unknown result type (might be due to invalid IL or missing references)
		//IL_00df: Unknown result type (might be due to invalid IL or missing references)
		//IL_00f0: Unknown result type (might be due to invalid IL or missing references)
		//IL_00f7: Unknown result type (might be due to invalid IL or missing references)
		//IL_00fe: Unknown result type (might be due to invalid IL or missing references)
		//IL_0103: Unknown result type (might be due to invalid IL or missing references)
		//IL_0116: Unknown result type (might be due to invalid IL or missing references)
		//IL_011b: Unknown result type (might be due to invalid IL or missing references)
		//IL_011c: Unknown result type (might be due to invalid IL or missing references)
		CameraRigController cameraRigController = context.cameraInfo.cameraRigController;
		CameraTargetParams targetParams = context.targetInfo.targetParams;
		Vector3 result = context.cameraInfo.previousCameraState.position;
		if (Object.op_Implicit((Object)(object)targetParams))
		{
			Vector3 position = ((Component)targetParams).transform.position;
			Vector3 val = (Object.op_Implicit((Object)(object)targetParams.cameraPivotTransform) ? targetParams.cameraPivotTransform.position : position) + new Vector3(0f, targetParams.currentCameraParamsData.pivotVerticalOffset.value, 0f);
			if (targetParams.dontRaycastToPivot)
			{
				result = val;
			}
			else
			{
				Vector3 val2 = val - position;
				float magnitude = ((Vector3)(ref val2)).magnitude;
				Ray ray = default(Ray);
				((Ray)(ref ray))._002Ector(position, val2);
				float num = cameraRigController.Raycast(ray, magnitude, targetParams.currentCameraParamsData.wallCushion.value);
				Debug.DrawRay(((Ray)(ref ray)).origin, ((Ray)(ref ray)).direction * magnitude, Color.green, Time.deltaTime);
				Debug.DrawRay(((Ray)(ref ray)).origin, ((Ray)(ref ray)).direction * num, Color.red, Time.deltaTime);
				result = ((Ray)(ref ray)).GetPoint(num);
			}
		}
		return result;
	}

	protected override void OnTargetChangedInternal(object rawInstanceData, CameraRigController cameraRigController, in OnTargetChangedArgs args)
	{
		InstanceData instanceData = (InstanceData)rawInstanceData;
		if (instanceData.hasEverHadTarget || !Object.op_Implicit((Object)(object)args.newTarget))
		{
			return;
		}
		CharacterBody component = args.newTarget.GetComponent<CharacterBody>();
		if (Object.op_Implicit((Object)(object)component))
		{
			CharacterDirection characterDirection = component.characterDirection;
			if (Object.op_Implicit((Object)(object)characterDirection))
			{
				instanceData.pitchYaw = new PitchYawPair(0f, characterDirection.yaw);
			}
		}
		instanceData.hasEverHadTarget = true;
	}

	protected override void MatchStateInternal(object rawInstanceData, in CameraModeContext context, in CameraState cameraStateToMatch)
	{
		//IL_0007: Unknown result type (might be due to invalid IL or missing references)
		//IL_000c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0011: Unknown result type (might be due to invalid IL or missing references)
		((InstanceData)rawInstanceData).SetPitchYawFromLookVector(cameraStateToMatch.rotation * Vector3.forward);
	}
}
