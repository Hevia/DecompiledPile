using System;
using System.Collections.Generic;
using HG;
using Rewired;
using RoR2.Networking;
using RoR2.UI;
using UnityEngine;

namespace RoR2.CameraModes;

public abstract class CameraModeBase
{
	public struct CameraInfo
	{
		public CameraRigController cameraRigController;

		public Camera sceneCam;

		public ICameraStateProvider overrideCam;

		public CameraState previousCameraState;

		public float baseFov;
	}

	public struct TargetInfo
	{
		public GameObject target;

		public CharacterBody body;

		public InputBankTest inputBank;

		public CameraTargetParams targetParams;

		public TeamIndex teamIndex;

		public bool isSprinting;

		public bool isViewerControlled;

		public CharacterMaster master;

		public NetworkUser networkUser;

		public NetworkedViewAngles networkedViewAngles;
	}

	public struct ViewerInfo
	{
		public LocalUser localUser;

		public UserProfile userProfile;

		public Player inputPlayer;

		public MPEventSystem eventSystem;

		public bool hasCursor;

		public bool isUIFocused;
	}

	public struct CameraModeContext
	{
		public CameraInfo cameraInfo;

		public TargetInfo targetInfo;

		public ViewerInfo viewerInfo;
	}

	public struct UpdateArgs
	{
		public CameraInfo cameraInfo;

		public TargetInfo targetInfo;
	}

	public struct UpdateResult
	{
		public GameObject firstPersonTarget;

		public CameraState cameraState;

		public bool showSprintParticles;

		public Vector3 crosshairWorldPosition;
	}

	public struct CollectLookInputResult
	{
		public Vector2 lookInput;
	}

	public struct ApplyLookInputArgs
	{
		public Vector2 lookInput;
	}

	public struct OnTargetChangedArgs
	{
		public GameObject oldTarget;

		public GameObject newTarget;
	}

	private Dictionary<UnityObjectWrapperKey<CameraRigController>, object> camToRawInstanceData = new Dictionary<UnityObjectWrapperKey<CameraRigController>, object>();

	public void OnInstall(CameraRigController cameraRigController)
	{
		//IL_000f: Unknown result type (might be due to invalid IL or missing references)
		object obj = CreateInstanceData(cameraRigController);
		camToRawInstanceData.Add(UnityObjectWrapperKey<CameraRigController>.op_Implicit(cameraRigController), obj);
		try
		{
			OnInstallInternal(obj, cameraRigController);
		}
		catch (Exception ex)
		{
			Debug.LogError((object)ex);
		}
		if (cameraRigController.target != null)
		{
			OnTargetChanged(cameraRigController, new OnTargetChangedArgs
			{
				oldTarget = null,
				newTarget = cameraRigController.target
			});
		}
	}

	protected virtual void OnInstallInternal(object rawInstancedata, CameraRigController cameraRigController)
	{
	}

	public void OnUninstall(CameraRigController cameraRigController)
	{
		//IL_0036: Unknown result type (might be due to invalid IL or missing references)
		//IL_0056: Unknown result type (might be due to invalid IL or missing references)
		if (cameraRigController.target != null)
		{
			OnTargetChanged(cameraRigController, new OnTargetChangedArgs
			{
				oldTarget = cameraRigController.target,
				newTarget = null
			});
		}
		try
		{
			OnUninstallInternal(camToRawInstanceData[UnityObjectWrapperKey<CameraRigController>.op_Implicit(cameraRigController)], cameraRigController);
		}
		catch (Exception ex)
		{
			Debug.LogError((object)ex);
		}
		camToRawInstanceData.Remove(UnityObjectWrapperKey<CameraRigController>.op_Implicit(cameraRigController));
	}

	protected virtual void OnUninstallInternal(object rawInstanceData, CameraRigController cameraRigController)
	{
	}

	protected abstract object CreateInstanceData(CameraRigController cameraRigController);

	public abstract bool IsSpectating(CameraRigController cameraRigController);

	public void Update(in CameraModeContext context, out UpdateResult result)
	{
		//IL_0011: Unknown result type (might be due to invalid IL or missing references)
		object rawInstanceData = camToRawInstanceData[UnityObjectWrapperKey<CameraRigController>.op_Implicit(context.cameraInfo.cameraRigController)];
		UpdateInternal(rawInstanceData, in context, out result);
	}

	protected abstract void UpdateInternal(object rawInstanceData, in CameraModeContext context, out UpdateResult result);

	public void CollectLookInput(in CameraModeContext context, out CollectLookInputResult result)
	{
		//IL_0011: Unknown result type (might be due to invalid IL or missing references)
		object rawInstanceData = camToRawInstanceData[UnityObjectWrapperKey<CameraRigController>.op_Implicit(context.cameraInfo.cameraRigController)];
		CollectLookInputInternal(rawInstanceData, in context, out result);
	}

	protected abstract void CollectLookInputInternal(object rawInstanceData, in CameraModeContext context, out CollectLookInputResult result);

	public void ApplyLookInput(in CameraModeContext context, in ApplyLookInputArgs args)
	{
		//IL_0011: Unknown result type (might be due to invalid IL or missing references)
		object rawInstanceData = camToRawInstanceData[UnityObjectWrapperKey<CameraRigController>.op_Implicit(context.cameraInfo.cameraRigController)];
		ApplyLookInputInternal(rawInstanceData, in context, in args);
	}

	protected abstract void ApplyLookInputInternal(object rawInstanceData, in CameraModeContext context, in ApplyLookInputArgs input);

	public void OnTargetChanged(CameraRigController cameraRigController, OnTargetChangedArgs args)
	{
		//IL_0007: Unknown result type (might be due to invalid IL or missing references)
		object rawInstanceData = camToRawInstanceData[UnityObjectWrapperKey<CameraRigController>.op_Implicit(cameraRigController)];
		OnTargetChangedInternal(rawInstanceData, cameraRigController, in args);
	}

	protected abstract void OnTargetChangedInternal(object rawInstanceData, CameraRigController cameraRigController, in OnTargetChangedArgs args);

	public void MatchState(in CameraModeContext context, in CameraState cameraStateToMatch)
	{
		//IL_0011: Unknown result type (might be due to invalid IL or missing references)
		object rawInstanceData = camToRawInstanceData[UnityObjectWrapperKey<CameraRigController>.op_Implicit(context.cameraInfo.cameraRigController)];
		MatchStateInternal(rawInstanceData, in context, in cameraStateToMatch);
	}

	protected abstract void MatchStateInternal(object rawInstanceData, in CameraModeContext context, in CameraState cameraStateToMatch);

	public object DebugGetInstanceData(CameraRigController cameraRigController)
	{
		//IL_0007: Unknown result type (might be due to invalid IL or missing references)
		camToRawInstanceData.TryGetValue(UnityObjectWrapperKey<CameraRigController>.op_Implicit(cameraRigController), out var value);
		return value;
	}
}
