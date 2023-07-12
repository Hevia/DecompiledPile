using System;
using System.Collections.Generic;
using HG;
using UnityEngine;

namespace RoR2;

public static class BackstabManager
{
	private class BackstabVisualizer
	{
		private class IndicatorInfo
		{
			public GameObject gameObject;

			public Transform transform;

			public ParticleSystem particleSystem;

			public ParticleSystemRenderer renderer;

			public float lastDisplayTime;
		}

		private CameraRigController camera;

		public CharacterBody targetBody;

		private readonly Dictionary<CharacterBody, IndicatorInfo> bodyToIndicator = new Dictionary<CharacterBody, IndicatorInfo>();

		private static readonly Dictionary<CharacterBody, IndicatorInfo> buffer = new Dictionary<CharacterBody, IndicatorInfo>();

		private readonly Stack<IndicatorInfo> indicatorPool = new Stack<IndicatorInfo>();

		private static GameObject indicatorPrefab;

		public void Install(CameraRigController newCamera)
		{
			camera = newCamera;
			RoR2Application.onLateUpdate += UpdateIndicators;
		}

		public void Uninstall()
		{
			RoR2Application.onLateUpdate -= UpdateIndicators;
			camera = null;
			foreach (KeyValuePair<CharacterBody, IndicatorInfo> item in bodyToIndicator)
			{
				Object.Destroy((Object)(object)item.Value.gameObject);
			}
			bodyToIndicator.Clear();
			foreach (IndicatorInfo item2 in indicatorPool)
			{
				Object.Destroy((Object)(object)item2.gameObject);
			}
			indicatorPool.Clear();
		}

		[SystemInitializer(new Type[] { })]
		private static void OnLoad()
		{
			indicatorPrefab = LegacyResourcesAPI.Load<GameObject>("Prefabs/VFX/BackstabIndicator");
		}

		private void UpdateIndicators()
		{
			//IL_0006: Unknown result type (might be due to invalid IL or missing references)
			//IL_000b: Unknown result type (might be due to invalid IL or missing references)
			//IL_005c: Unknown result type (might be due to invalid IL or missing references)
			//IL_0061: Unknown result type (might be due to invalid IL or missing references)
			//IL_0062: Unknown result type (might be due to invalid IL or missing references)
			Vector3 corePosition = targetBody.corePosition;
			TeamIndex teamIndex = targetBody.teamComponent.teamIndex;
			buffer.Clear();
			float unscaledTime = Time.unscaledTime;
			foreach (CharacterBody readOnlyInstances in CharacterBody.readOnlyInstancesList)
			{
				bool num = TeamManager.IsTeamEnemy(teamIndex, readOnlyInstances.teamComponent.teamIndex);
				bool flag = false;
				if (num)
				{
					flag = IsBackstab(readOnlyInstances.corePosition - corePosition, readOnlyInstances);
				}
				if (num && flag)
				{
					IndicatorInfo value = null;
					if (!bodyToIndicator.TryGetValue(readOnlyInstances, out value))
					{
						value = AddIndicator(readOnlyInstances);
					}
					value.lastDisplayTime = unscaledTime;
				}
			}
			List<CharacterBody> list = CollectionPool<CharacterBody, List<CharacterBody>>.RentCollection();
			foreach (KeyValuePair<CharacterBody, IndicatorInfo> item in bodyToIndicator)
			{
				if (item.Value.lastDisplayTime != unscaledTime)
				{
					list.Add(item.Key);
				}
			}
			foreach (CharacterBody item2 in list)
			{
				RemoveIndicator(item2);
			}
			list = CollectionPool<CharacterBody, List<CharacterBody>>.ReturnCollection(list);
		}

		private IndicatorInfo AddIndicator(CharacterBody victimBody)
		{
			IndicatorInfo indicatorInfo = null;
			if (indicatorPool.Count > 0)
			{
				indicatorInfo = indicatorPool.Pop();
			}
			else
			{
				indicatorInfo = new IndicatorInfo();
				indicatorInfo.gameObject = Object.Instantiate<GameObject>(indicatorPrefab);
				indicatorInfo.transform = indicatorInfo.gameObject.transform;
				indicatorInfo.particleSystem = indicatorInfo.gameObject.GetComponent<ParticleSystem>();
				indicatorInfo.renderer = indicatorInfo.gameObject.GetComponent<ParticleSystemRenderer>();
				((Renderer)indicatorInfo.renderer).enabled = false;
			}
			indicatorInfo.gameObject.SetActive(true);
			indicatorInfo.particleSystem.Play();
			bodyToIndicator[victimBody] = indicatorInfo;
			return indicatorInfo;
		}

		private void RemoveIndicator(CharacterBody victimBody)
		{
			IndicatorInfo indicatorInfo = bodyToIndicator[victimBody];
			if (Object.op_Implicit((Object)(object)indicatorInfo.gameObject))
			{
				indicatorInfo.particleSystem.Stop();
				indicatorInfo.gameObject.SetActive(false);
			}
			bodyToIndicator.Remove(victimBody);
			indicatorPool.Push(indicatorInfo);
		}

		public void OnPreCull()
		{
			//IL_0047: Unknown result type (might be due to invalid IL or missing references)
			//IL_004c: Unknown result type (might be due to invalid IL or missing references)
			//IL_0058: Unknown result type (might be due to invalid IL or missing references)
			//IL_005e: Unknown result type (might be due to invalid IL or missing references)
			//IL_0069: Unknown result type (might be due to invalid IL or missing references)
			//IL_006e: Unknown result type (might be due to invalid IL or missing references)
			foreach (KeyValuePair<CharacterBody, IndicatorInfo> item in bodyToIndicator)
			{
				CharacterBody key = item.Key;
				Transform transform = item.Value.transform;
				if (Object.op_Implicit((Object)(object)key))
				{
					Vector3? bodyForward = GetBodyForward(key);
					if (bodyForward.HasValue)
					{
						transform.forward = -bodyForward.Value;
					}
					transform.position = key.corePosition - transform.forward * key.radius;
					((Renderer)item.Value.renderer).enabled = true;
				}
			}
		}

		public void OnPostRender()
		{
			foreach (KeyValuePair<CharacterBody, IndicatorInfo> item in bodyToIndicator)
			{
				((Renderer)item.Value.renderer).enabled = false;
			}
		}

		private void Update()
		{
			UpdateIndicators();
		}
	}

	public static GameObject backstabImpactEffectPrefab = null;

	private static readonly bool enableVisualizerSystem = false;

	private static readonly float showBackstabThreshold = Mathf.Cos(MathF.PI / 4f);

	private static readonly Dictionary<CameraRigController, BackstabVisualizer> camToVisualizer = new Dictionary<CameraRigController, BackstabVisualizer>();

	public static bool IsBackstab(Vector3 attackerCorePositionToHitPosition, CharacterBody victimBody)
	{
		//IL_001a: Unknown result type (might be due to invalid IL or missing references)
		//IL_001d: Unknown result type (might be due to invalid IL or missing references)
		if (!victimBody.canReceiveBackstab)
		{
			return false;
		}
		Vector3? bodyForward = GetBodyForward(victimBody);
		if (bodyForward.HasValue)
		{
			return Vector3.Dot(attackerCorePositionToHitPosition, bodyForward.Value) > 0f;
		}
		return false;
	}

	private static Vector3? GetBodyForward(CharacterBody characterBody)
	{
		//IL_001d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0031: Unknown result type (might be due to invalid IL or missing references)
		Vector3? val = null;
		return (!Object.op_Implicit((Object)(object)characterBody.characterDirection)) ? new Vector3?(((Component)characterBody).transform.forward) : new Vector3?(characterBody.characterDirection.forward);
	}

	[SystemInitializer(new Type[] { })]
	private static void Init()
	{
		if (enableVisualizerSystem)
		{
			InitVisualizerSystem();
		}
		backstabImpactEffectPrefab = LegacyResourcesAPI.Load<GameObject>("Effects/ImpactEffects/BackstabSpark");
	}

	private static bool ShouldShowBackstab(Vector3 attackerCorePosition, CharacterBody victimBody)
	{
		//IL_001a: Unknown result type (might be due to invalid IL or missing references)
		//IL_001c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0021: Unknown result type (might be due to invalid IL or missing references)
		//IL_0026: Unknown result type (might be due to invalid IL or missing references)
		//IL_0029: Unknown result type (might be due to invalid IL or missing references)
		//IL_0030: Unknown result type (might be due to invalid IL or missing references)
		if (!victimBody.canReceiveBackstab)
		{
			return false;
		}
		Vector3? bodyForward = GetBodyForward(victimBody);
		if (bodyForward.HasValue)
		{
			Vector3 val = attackerCorePosition - victimBody.corePosition;
			return Vector3.Dot(((Vector3)(ref val)).normalized, bodyForward.Value) > showBackstabThreshold;
		}
		return false;
	}

	private static void InitVisualizerSystem()
	{
		CameraRigController.onCameraTargetChanged += OnCameraTargetChanged;
		CameraRigController.onCameraEnableGlobal += OnCameraDiscovered;
		CameraRigController.onCameraDisableGlobal += OnCameraLost;
		SceneCamera.onSceneCameraPreCull += OnSceneCameraPreCull;
		SceneCamera.onSceneCameraPostRender += OnSceneCameraPostRender;
	}

	private static void OnCameraTargetChanged(CameraRigController camera, GameObject target)
	{
		RefreshCamera(camera);
	}

	private static void OnCameraDiscovered(CameraRigController camera)
	{
		RefreshCamera(camera);
	}

	private static void OnCameraLost(CameraRigController camera)
	{
		RefreshCamera(camera);
	}

	private static void OnSceneCameraPreCull(SceneCamera sceneCam)
	{
		if (camToVisualizer.TryGetValue(sceneCam.cameraRigController, out var value))
		{
			value.OnPreCull();
		}
	}

	private static void OnSceneCameraPostRender(SceneCamera sceneCam)
	{
		if (camToVisualizer.TryGetValue(sceneCam.cameraRigController, out var value))
		{
			value.OnPostRender();
		}
	}

	private static void RefreshCamera(CameraRigController camera)
	{
		BackstabVisualizer value;
		bool num = camToVisualizer.TryGetValue(camera, out value);
		GameObject val = (((Behaviour)camera).isActiveAndEnabled ? camera.target : null);
		CharacterBody characterBody = (Object.op_Implicit((Object)(object)val) ? val.GetComponent<CharacterBody>() : null);
		bool flag = Object.op_Implicit((Object)(object)characterBody) && characterBody.canPerformBackstab;
		if (num != flag)
		{
			if (flag)
			{
				value = new BackstabVisualizer();
				camToVisualizer.Add(camera, value);
				value.Install(camera);
			}
			else
			{
				value.Uninstall();
				camToVisualizer.Remove(camera);
				value = null;
			}
		}
		if (value != null)
		{
			value.targetBody = characterBody;
		}
	}
}
