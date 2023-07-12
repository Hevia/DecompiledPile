using System;
using System.Collections.Generic;
using HG;
using UnityEngine;

namespace RoR2;

public class CameraTargetParams : MonoBehaviour
{
	public enum AimType
	{
		Standard,
		FirstPerson,
		Aura,
		Sprinting,
		OverTheShoulder
	}

	public class AimRequest : IDisposable
	{
		public readonly AimType aimType;

		private Action<AimRequest> disposeCallback;

		public AimRequest(AimType type, Action<AimRequest> onDispose)
		{
			disposeCallback = onDispose;
			aimType = type;
		}

		public void Dispose()
		{
			disposeCallback?.Invoke(this);
			disposeCallback = null;
		}
	}

	public struct CameraParamsOverrideRequest
	{
		public CharacterCameraParamsData cameraParamsData;

		public float priority;
	}

	internal class CameraParamsOverride
	{
		public CharacterCameraParamsData cameraParamsData;

		public float priority;

		public float enterStartTime;

		public float enterEndTime;

		public float exitStartTime;

		public float exitEndTime;

		public float CalculateAlpha(float t)
		{
			float num = 1f;
			if (t < enterEndTime)
			{
				num *= Mathf.Clamp01(Mathf.InverseLerp(enterStartTime, enterEndTime, t));
			}
			if (t > exitStartTime)
			{
				num *= Mathf.Clamp01(Mathf.InverseLerp(exitEndTime, exitStartTime, t));
			}
			return easeCurve.Evaluate(num);
		}
	}

	public struct CameraParamsOverrideHandle
	{
		internal readonly CameraParamsOverride target;

		public bool isValid => target != null;

		internal CameraParamsOverrideHandle(CameraParamsOverride target)
		{
			this.target = target;
		}
	}

	public CharacterCameraParams cameraParams;

	public Transform cameraPivotTransform;

	[ShowFieldObsolete]
	[Obsolete]
	public float fovOverride;

	[HideInInspector]
	public Vector2 recoil;

	[HideInInspector]
	public bool dontRaycastToPivot;

	private static float targetRecoilDampTime = 0.08f;

	private static float recoilDampTime = 0.05f;

	private Vector2 targetRecoil;

	private Vector2 recoilVelocity;

	private Vector2 targetRecoilVelocity;

	private List<AimRequest> aimRequestStack = new List<AimRequest>();

	private static readonly AnimationCurve easeCurve = AnimationCurve.EaseInOut(0f, 0f, 1f, 1f);

	private List<CameraParamsOverride> cameraParamsOverrides;

	private CharacterCameraParamsData _currentCameraParamsData;

	public ref CharacterCameraParamsData currentCameraParamsData => ref _currentCameraParamsData;

	public void AddRecoil(float verticalMin, float verticalMax, float horizontalMin, float horizontalMax)
	{
		//IL_0002: Unknown result type (might be due to invalid IL or missing references)
		//IL_0016: Unknown result type (might be due to invalid IL or missing references)
		//IL_001b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0020: Unknown result type (might be due to invalid IL or missing references)
		targetRecoil += new Vector2(Random.Range(horizontalMin, horizontalMax), Random.Range(verticalMin, verticalMax));
	}

	public AimRequest RequestAimType(AimType aimType)
	{
		//IL_002d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0041: Unknown result type (might be due to invalid IL or missing references)
		//IL_0046: Unknown result type (might be due to invalid IL or missing references)
		//IL_004b: Unknown result type (might be due to invalid IL or missing references)
		if (aimType == AimType.Aura)
		{
			CharacterCameraParamsData data = cameraParams.data;
			ref Vector3 value = ref data.idealLocalCameraPos.value;
			value += new Vector3(0f, 1.5f, -7f);
			CameraParamsOverrideHandle overrideHandle = AddParamsOverride(new CameraParamsOverrideRequest
			{
				cameraParamsData = data,
				priority = 0.1f
			}, 0.5f);
			AimRequest aimRequest2 = new AimRequest(aimType, delegate(AimRequest aimRequest)
			{
				RemoveRequest(aimRequest);
				RemoveParamsOverride(overrideHandle, 0.5f);
			});
			aimRequestStack.Add(aimRequest2);
			return aimRequest2;
		}
		return null;
	}

	private void RemoveRequest(AimRequest request)
	{
		aimRequestStack.Remove(request);
	}

	private void Awake()
	{
		CharacterBody component = ((Component)this).GetComponent<CharacterBody>();
		if (Object.op_Implicit((Object)(object)component) && (Object)(object)cameraPivotTransform == (Object)null)
		{
			cameraPivotTransform = component.aimOriginTransform;
		}
		cameraParamsOverrides = CollectionPool<CameraParamsOverride, List<CameraParamsOverride>>.RentCollection();
	}

	private void OnDestroy()
	{
		cameraParamsOverrides = CollectionPool<CameraParamsOverride, List<CameraParamsOverride>>.ReturnCollection(cameraParamsOverrides);
	}

	private void Update()
	{
		//IL_0002: Unknown result type (might be due to invalid IL or missing references)
		//IL_0007: Unknown result type (might be due to invalid IL or missing references)
		//IL_0021: Unknown result type (might be due to invalid IL or missing references)
		//IL_0026: Unknown result type (might be due to invalid IL or missing references)
		//IL_002d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0033: Unknown result type (might be due to invalid IL or missing references)
		//IL_004d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0052: Unknown result type (might be due to invalid IL or missing references)
		targetRecoil = Vector2.SmoothDamp(targetRecoil, Vector2.zero, ref targetRecoilVelocity, targetRecoilDampTime, 180f, Time.deltaTime);
		recoil = Vector2.SmoothDamp(recoil, targetRecoil, ref recoilVelocity, recoilDampTime, 180f, Time.deltaTime);
		CalcParams(out currentCameraParamsData);
		float time = Time.time;
		for (int num = cameraParamsOverrides.Count - 1; num >= 0; num--)
		{
			if (cameraParamsOverrides[num].exitEndTime <= time)
			{
				cameraParamsOverrides.RemoveAt(num);
			}
		}
	}

	public CameraParamsOverrideHandle AddParamsOverride(CameraParamsOverrideRequest request, float transitionDuration = 0.2f)
	{
		float time = Time.time;
		CameraParamsOverride cameraParamsOverride = new CameraParamsOverride
		{
			cameraParamsData = request.cameraParamsData,
			enterStartTime = time,
			enterEndTime = time + transitionDuration,
			exitStartTime = float.PositiveInfinity,
			exitEndTime = float.PositiveInfinity,
			priority = request.priority
		};
		int i;
		for (i = 0; i < cameraParamsOverrides.Count && !(request.priority <= cameraParamsOverrides[i].priority); i++)
		{
		}
		cameraParamsOverrides.Insert(i, cameraParamsOverride);
		return new CameraParamsOverrideHandle(cameraParamsOverride);
	}

	public CameraParamsOverrideHandle RemoveParamsOverride(CameraParamsOverrideHandle handle, float transitionDuration = 0.2f)
	{
		if (cameraParamsOverrides == null)
		{
			return default(CameraParamsOverrideHandle);
		}
		CameraParamsOverride cameraParamsOverride = null;
		for (int i = 0; i < cameraParamsOverrides.Count; i++)
		{
			if (handle.target == cameraParamsOverrides[i])
			{
				cameraParamsOverride = cameraParamsOverrides[i];
				break;
			}
		}
		if (cameraParamsOverride == null || cameraParamsOverride.exitStartTime != float.PositiveInfinity)
		{
			return default(CameraParamsOverrideHandle);
		}
		cameraParamsOverride.exitEndTime = (cameraParamsOverride.exitStartTime = Time.time) + transitionDuration;
		return default(CameraParamsOverrideHandle);
	}

	public void CalcParams(out CharacterCameraParamsData dest)
	{
		dest = (Object.op_Implicit((Object)(object)cameraParams) ? cameraParams.data : CharacterCameraParamsData.basic);
		float time = Time.time;
		for (int i = 0; i < cameraParamsOverrides.Count; i++)
		{
			CameraParamsOverride cameraParamsOverride = cameraParamsOverrides[i];
			CharacterCameraParamsData.Blend(in cameraParamsOverride.cameraParamsData, ref dest, cameraParamsOverride.CalculateAlpha(time));
		}
	}
}
