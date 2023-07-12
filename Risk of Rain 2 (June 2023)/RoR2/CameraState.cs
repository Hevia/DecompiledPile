using UnityEngine;

namespace RoR2;

public struct CameraState
{
	public Vector3 position;

	public Quaternion rotation;

	public float fov;

	public static CameraState Lerp(ref CameraState a, ref CameraState b, float t)
	{
		//IL_000b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0011: Unknown result type (might be due to invalid IL or missing references)
		//IL_0017: Unknown result type (might be due to invalid IL or missing references)
		//IL_001c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0024: Unknown result type (might be due to invalid IL or missing references)
		//IL_002a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0030: Unknown result type (might be due to invalid IL or missing references)
		//IL_0035: Unknown result type (might be due to invalid IL or missing references)
		CameraState result = default(CameraState);
		result.position = Vector3.LerpUnclamped(a.position, b.position, t);
		result.rotation = Quaternion.SlerpUnclamped(a.rotation, b.rotation, t);
		result.fov = Mathf.LerpUnclamped(a.fov, b.fov, t);
		return result;
	}

	public static CameraState SmoothDamp(CameraState current, CameraState target, ref Vector3 positionVelocity, ref float angleVelocity, ref float fovVelocity, float smoothTime)
	{
		//IL_000b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0011: Unknown result type (might be due to invalid IL or missing references)
		//IL_0019: Unknown result type (might be due to invalid IL or missing references)
		//IL_001e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0026: Unknown result type (might be due to invalid IL or missing references)
		//IL_002c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0034: Unknown result type (might be due to invalid IL or missing references)
		//IL_0039: Unknown result type (might be due to invalid IL or missing references)
		CameraState result = default(CameraState);
		result.position = Vector3.SmoothDamp(current.position, target.position, ref positionVelocity, smoothTime);
		result.rotation = Util.SmoothDampQuaternion(current.rotation, target.rotation, ref angleVelocity, smoothTime);
		result.fov = Mathf.SmoothDamp(current.fov, target.fov, ref fovVelocity, smoothTime);
		return result;
	}
}
