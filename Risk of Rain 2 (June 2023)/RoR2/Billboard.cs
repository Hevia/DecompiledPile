using System.Collections.Generic;
using UnityEngine;

namespace RoR2;

public class Billboard : MonoBehaviour
{
	private static List<Transform> instanceTransformsList;

	static Billboard()
	{
		instanceTransformsList = new List<Transform>();
		SceneCamera.onSceneCameraPreCull += OnSceneCameraPreCull;
	}

	private static void OnSceneCameraPreCull(SceneCamera sceneCamera)
	{
		//IL_0006: Unknown result type (might be due to invalid IL or missing references)
		//IL_000b: Unknown result type (might be due to invalid IL or missing references)
		//IL_001b: Unknown result type (might be due to invalid IL or missing references)
		Quaternion rotation = ((Component)sceneCamera).transform.rotation;
		for (int i = 0; i < instanceTransformsList.Count; i++)
		{
			instanceTransformsList[i].rotation = rotation;
		}
	}

	private void OnEnable()
	{
		instanceTransformsList.Add(((Component)this).transform);
	}

	private void OnDisable()
	{
		instanceTransformsList.Remove(((Component)this).transform);
	}
}
