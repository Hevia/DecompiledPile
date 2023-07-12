using System;
using System.Collections.Generic;
using JetBrains.Annotations;
using UnityEngine;

namespace RoR2;

public class Indicator
{
	private static class IndicatorManager
	{
		private static readonly List<Indicator> runningIndicators;

		public static void AddIndicator([NotNull] Indicator indicator)
		{
			runningIndicators.Add(indicator);
			RebuildVisualizer(indicator);
		}

		public static void RemoveIndicator([NotNull] Indicator indicator)
		{
			indicator.SetVisualizerInstantiated(newVisualizerInstantiated: false);
			runningIndicators.Remove(indicator);
		}

		static IndicatorManager()
		{
			runningIndicators = new List<Indicator>();
			CameraRigController.onCameraTargetChanged += delegate
			{
				RebuildVisualizerForAll();
			};
			UICamera.onUICameraPreRender += OnPreRenderUI;
			UICamera.onUICameraPostRender += OnPostRenderUI;
			RoR2Application.onUpdate += Update;
		}

		private static void RebuildVisualizerForAll()
		{
			foreach (Indicator runningIndicator in runningIndicators)
			{
				RebuildVisualizer(runningIndicator);
			}
		}

		private static void Update()
		{
			foreach (Indicator runningIndicator in runningIndicators)
			{
				if (runningIndicator.hasVisualizer)
				{
					runningIndicator.UpdateVisualizer();
				}
			}
		}

		private static void RebuildVisualizer(Indicator indicator)
		{
			bool visualizerInstantiated = false;
			foreach (CameraRigController readOnlyInstances in CameraRigController.readOnlyInstancesList)
			{
				if ((Object)(object)readOnlyInstances.target == (Object)(object)indicator.owner)
				{
					visualizerInstantiated = true;
					break;
				}
			}
			indicator.SetVisualizerInstantiated(visualizerInstantiated);
		}

		private static void OnPreRenderUI(UICamera uiCam)
		{
			GameObject target = uiCam.cameraRigController.target;
			Camera sceneCam = uiCam.cameraRigController.sceneCam;
			foreach (Indicator runningIndicator in runningIndicators)
			{
				bool num = (Object)(object)target == (Object)(object)runningIndicator.owner;
				runningIndicator.SetVisible((Object)(object)target == (Object)(object)runningIndicator.owner);
				if (num)
				{
					runningIndicator.PositionForUI(sceneCam, uiCam.camera);
				}
			}
		}

		private static void OnPostRenderUI(UICamera uiCamera)
		{
			foreach (Indicator runningIndicator in runningIndicators)
			{
				runningIndicator.SetVisible(newVisible: true);
			}
		}
	}

	private GameObject _visualizerPrefab;

	public readonly GameObject owner;

	public Transform targetTransform;

	private bool _active;

	private bool visible = true;

	public GameObject visualizerPrefab
	{
		get
		{
			return _visualizerPrefab;
		}
		set
		{
			if (!((Object)(object)_visualizerPrefab == (Object)(object)value))
			{
				_visualizerPrefab = value;
				if (Object.op_Implicit((Object)(object)visualizerInstance))
				{
					DestroyVisualizer();
					InstantiateVisualizer();
				}
			}
		}
	}

	protected GameObject visualizerInstance { get; private set; }

	protected Transform visualizerTransform { get; private set; }

	protected Renderer[] visualizerRenderers { get; private set; }

	public bool hasVisualizer => Object.op_Implicit((Object)(object)visualizerInstance);

	public bool active
	{
		get
		{
			return _active;
		}
		set
		{
			if (_active != value)
			{
				_active = value;
				if (active)
				{
					IndicatorManager.AddIndicator(this);
				}
				else
				{
					IndicatorManager.RemoveIndicator(this);
				}
			}
		}
	}

	public Indicator(GameObject owner, GameObject visualizerPrefab)
	{
		this.owner = owner;
		_visualizerPrefab = visualizerPrefab;
		visualizerRenderers = Array.Empty<Renderer>();
	}

	public void SetVisualizerInstantiated(bool newVisualizerInstantiated)
	{
		if (Object.op_Implicit((Object)(object)visualizerInstance) != newVisualizerInstantiated)
		{
			if (newVisualizerInstantiated)
			{
				InstantiateVisualizer();
			}
			else
			{
				DestroyVisualizer();
			}
		}
	}

	private void InstantiateVisualizer()
	{
		visualizerInstance = Object.Instantiate<GameObject>(visualizerPrefab);
		OnInstantiateVisualizer();
	}

	private void DestroyVisualizer()
	{
		OnDestroyVisualizer();
		Object.Destroy((Object)(object)visualizerInstance);
		visualizerInstance = null;
	}

	public void OnInstantiateVisualizer()
	{
		visualizerTransform = visualizerInstance.transform;
		visualizerRenderers = visualizerInstance.GetComponentsInChildren<Renderer>();
		SetVisibleInternal(visible);
	}

	public virtual void OnDestroyVisualizer()
	{
		visualizerTransform = null;
		visualizerRenderers = Array.Empty<Renderer>();
	}

	public virtual void UpdateVisualizer()
	{
	}

	public virtual void PositionForUI(Camera sceneCamera, Camera uiCamera)
	{
		//IL_0013: Unknown result type (might be due to invalid IL or missing references)
		//IL_0018: Unknown result type (might be due to invalid IL or missing references)
		//IL_001a: Unknown result type (might be due to invalid IL or missing references)
		//IL_001b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0020: Unknown result type (might be due to invalid IL or missing references)
		//IL_0023: Unknown result type (might be due to invalid IL or missing references)
		//IL_0042: Unknown result type (might be due to invalid IL or missing references)
		//IL_0043: Unknown result type (might be due to invalid IL or missing references)
		//IL_0048: Unknown result type (might be due to invalid IL or missing references)
		//IL_005d: Unknown result type (might be due to invalid IL or missing references)
		if (Object.op_Implicit((Object)(object)targetTransform))
		{
			Vector3 position = targetTransform.position;
			Vector3 val = sceneCamera.WorldToScreenPoint(position);
			val.z = ((val.z > 0f) ? 1f : (-1f));
			Vector3 position2 = uiCamera.ScreenToWorldPoint(val);
			if ((Object)(object)visualizerTransform != (Object)null)
			{
				visualizerTransform.position = position2;
			}
		}
	}

	public void SetVisible(bool newVisible)
	{
		newVisible &= Object.op_Implicit((Object)(object)targetTransform);
		if (visible != newVisible)
		{
			SetVisibleInternal(newVisible);
		}
	}

	private void SetVisibleInternal(bool newVisible)
	{
		visible = newVisible;
		Renderer[] array = visualizerRenderers;
		for (int i = 0; i < array.Length; i++)
		{
			array[i].enabled = newVisible;
		}
	}
}
