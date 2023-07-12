using System.Collections.Generic;
using UnityEngine;

namespace RoR2;

public class ConstrainToScreen : MonoBehaviour
{
	private static float boundaryUVSize;

	private static List<Transform> instanceTransformsList;

	static ConstrainToScreen()
	{
		boundaryUVSize = 0.05f;
		instanceTransformsList = new List<Transform>();
		SceneCamera.onSceneCameraPreCull += OnSceneCameraPreCull;
	}

	private static void OnSceneCameraPreCull(SceneCamera sceneCamera)
	{
		//IL_0019: Unknown result type (might be due to invalid IL or missing references)
		//IL_001e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0023: Unknown result type (might be due to invalid IL or missing references)
		//IL_0026: Unknown result type (might be due to invalid IL or missing references)
		//IL_0048: Unknown result type (might be due to invalid IL or missing references)
		//IL_006a: Unknown result type (might be due to invalid IL or missing references)
		//IL_006b: Unknown result type (might be due to invalid IL or missing references)
		Camera camera = sceneCamera.camera;
		for (int i = 0; i < instanceTransformsList.Count; i++)
		{
			Transform val = instanceTransformsList[i];
			Vector3 val2 = camera.WorldToViewportPoint(val.position);
			val2.x = Mathf.Clamp(val2.x, boundaryUVSize, 1f - boundaryUVSize);
			val2.y = Mathf.Clamp(val2.y, boundaryUVSize, 1f - boundaryUVSize);
			val.position = camera.ViewportToWorldPoint(val2);
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
