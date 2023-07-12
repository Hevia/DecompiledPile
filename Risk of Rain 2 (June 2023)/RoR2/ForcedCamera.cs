using System.Collections.ObjectModel;
using UnityEngine;

namespace RoR2;

public class ForcedCamera : MonoBehaviour, ICameraStateProvider
{
	public float entryLerpDuration = 1f;

	public float exitLerpDuration = 1f;

	public float fovOverride;

	public bool allowUserLook;

	public bool allowUserHud;

	private void Update()
	{
		ReadOnlyCollection<CameraRigController> readOnlyInstancesList = CameraRigController.readOnlyInstancesList;
		for (int i = 0; i < readOnlyInstancesList.Count; i++)
		{
			CameraRigController cameraRigController = readOnlyInstancesList[i];
			if (!cameraRigController.hasOverride)
			{
				cameraRigController.SetOverrideCam(this, entryLerpDuration);
			}
		}
	}

	private void OnDisable()
	{
		ReadOnlyCollection<CameraRigController> readOnlyInstancesList = CameraRigController.readOnlyInstancesList;
		for (int i = 0; i < readOnlyInstancesList.Count; i++)
		{
			CameraRigController cameraRigController = readOnlyInstancesList[i];
			if (cameraRigController.IsOverrideCam(this))
			{
				cameraRigController.SetOverrideCam(null, exitLerpDuration);
			}
		}
	}

	public void GetCameraState(CameraRigController cameraRigController, ref CameraState cameraState)
	{
		//IL_0007: Unknown result type (might be due to invalid IL or missing references)
		//IL_000c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0018: Unknown result type (might be due to invalid IL or missing references)
		//IL_001d: Unknown result type (might be due to invalid IL or missing references)
		cameraState.position = ((Component)this).transform.position;
		cameraState.rotation = ((Component)this).transform.rotation;
		if (fovOverride > 0f)
		{
			cameraState.fov = fovOverride;
		}
	}

	public bool IsUserLookAllowed(CameraRigController cameraRigController)
	{
		return allowUserLook;
	}

	public bool IsUserControlAllowed(CameraRigController cameraRigController)
	{
		return false;
	}

	public bool IsHudAllowed(CameraRigController cameraRigController)
	{
		return allowUserHud;
	}

	private void OnDrawGizmosSelected()
	{
		//IL_0000: Unknown result type (might be due to invalid IL or missing references)
		//IL_0005: Unknown result type (might be due to invalid IL or missing references)
		//IL_000a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0014: Unknown result type (might be due to invalid IL or missing references)
		//IL_0019: Unknown result type (might be due to invalid IL or missing references)
		//IL_0022: Unknown result type (might be due to invalid IL or missing references)
		//IL_002d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0032: Unknown result type (might be due to invalid IL or missing references)
		//IL_003c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0042: Unknown result type (might be due to invalid IL or missing references)
		Color color = Gizmos.color;
		Matrix4x4 matrix = Gizmos.matrix;
		Gizmos.color = Color.yellow;
		Matrix4x4 identity = Matrix4x4.identity;
		((Matrix4x4)(ref identity)).SetTRS(((Component)this).transform.position, ((Component)this).transform.rotation, Vector3.one);
		Gizmos.matrix = identity;
		Gizmos.DrawFrustum(Vector3.zero, (fovOverride > 0f) ? fovOverride : 60f, 10f, 0.1f, 1.7777778f);
		Gizmos.matrix = matrix;
		Gizmos.color = color;
	}
}
