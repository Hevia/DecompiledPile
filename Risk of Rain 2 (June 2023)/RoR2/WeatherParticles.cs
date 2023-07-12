using System.Collections.Generic;
using UnityEngine;

namespace RoR2;

public class WeatherParticles : MonoBehaviour
{
	public bool resetPositionToZero;

	public bool lockPosition = true;

	public bool lockRotation = true;

	private static List<WeatherParticles> instancesList;

	static WeatherParticles()
	{
		instancesList = new List<WeatherParticles>();
		SceneCamera.onSceneCameraPreRender += OnSceneCameraPreRender;
	}

	private void UpdateForCamera(CameraRigController cameraRigController, bool lockPosition, bool lockRotation)
	{
		//IL_0016: Unknown result type (might be due to invalid IL or missing references)
		//IL_001e: Unknown result type (might be due to invalid IL or missing references)
		//IL_002c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0034: Unknown result type (might be due to invalid IL or missing references)
		Transform transform = ((Component)cameraRigController).transform;
		((Component)this).transform.SetPositionAndRotation(lockPosition ? transform.position : ((Component)this).transform.position, lockRotation ? transform.rotation : ((Component)this).transform.rotation);
	}

	private static void OnSceneCameraPreRender(SceneCamera sceneCamera)
	{
		if (Object.op_Implicit((Object)(object)sceneCamera.cameraRigController))
		{
			for (int i = 0; i < instancesList.Count; i++)
			{
				WeatherParticles weatherParticles = instancesList[i];
				weatherParticles.UpdateForCamera(sceneCamera.cameraRigController, weatherParticles.lockPosition, weatherParticles.lockRotation);
			}
		}
	}

	private void OnEnable()
	{
		//IL_0019: Unknown result type (might be due to invalid IL or missing references)
		instancesList.Add(this);
		if (resetPositionToZero)
		{
			((Component)this).transform.position = Vector3.zero;
		}
	}

	private void OnDisable()
	{
		instancesList.Remove(this);
	}
}
