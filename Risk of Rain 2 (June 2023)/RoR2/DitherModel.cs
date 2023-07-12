using System.Collections.Generic;
using UnityEngine;

namespace RoR2;

public class DitherModel : MonoBehaviour
{
	[HideInInspector]
	public float fade;

	public Collider bounds;

	public Renderer[] renderers;

	private MaterialPropertyBlock propertyStorage;

	private static List<DitherModel> instancesList;

	static DitherModel()
	{
		instancesList = new List<DitherModel>();
		SceneCamera.onSceneCameraPreRender += OnSceneCameraPreRender;
	}

	private static void OnSceneCameraPreRender(SceneCamera sceneCamera)
	{
		if (Object.op_Implicit((Object)(object)sceneCamera.cameraRigController))
		{
			RefreshObstructorsForCamera(sceneCamera.cameraRigController);
		}
	}

	private static void RefreshObstructorsForCamera(CameraRigController cameraRigController)
	{
		//IL_0006: Unknown result type (might be due to invalid IL or missing references)
		//IL_000b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0032: Unknown result type (might be due to invalid IL or missing references)
		//IL_0033: Unknown result type (might be due to invalid IL or missing references)
		//IL_0038: Unknown result type (might be due to invalid IL or missing references)
		//IL_0042: Unknown result type (might be due to invalid IL or missing references)
		//IL_0043: Unknown result type (might be due to invalid IL or missing references)
		Vector3 position = ((Component)cameraRigController).transform.position;
		for (int i = 0; i < instancesList.Count; i++)
		{
			DitherModel ditherModel = instancesList[i];
			if (Object.op_Implicit((Object)(object)ditherModel.bounds))
			{
				Vector3 val = ditherModel.bounds.ClosestPointOnBounds(position);
				if (cameraRigController.enableFading)
				{
					ditherModel.fade = Mathf.Clamp01(Util.Remap(Vector3.Distance(val, position), cameraRigController.fadeStartDistance, cameraRigController.fadeEndDistance, 0f, 1f));
				}
				else
				{
					ditherModel.fade = 1f;
				}
				ditherModel.UpdateDither();
			}
			else
			{
				Debug.LogFormat("{0} has missing collider for dither model", new object[1] { ((Component)ditherModel).gameObject });
			}
		}
	}

	private void UpdateDither()
	{
		for (int num = renderers.Length - 1; num >= 0; num--)
		{
			Renderer obj = renderers[num];
			obj.GetPropertyBlock(propertyStorage);
			propertyStorage.SetFloat("_Fade", fade);
			obj.SetPropertyBlock(propertyStorage);
		}
	}

	private void Awake()
	{
		//IL_0001: Unknown result type (might be due to invalid IL or missing references)
		//IL_000b: Expected O, but got Unknown
		propertyStorage = new MaterialPropertyBlock();
	}

	private void OnEnable()
	{
		instancesList.Add(this);
	}

	private void OnDisable()
	{
		instancesList.Remove(this);
	}
}
