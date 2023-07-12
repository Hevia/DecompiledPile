using System.Collections.Generic;
using UnityEngine;

namespace RoR2;

public class ScaleSpriteByCamDistance : MonoBehaviour
{
	public enum ScalingMode
	{
		Direct,
		Square,
		Sqrt,
		Cube,
		CubeRoot
	}

	private static List<ScaleSpriteByCamDistance> instancesList;

	private Transform transform;

	[Tooltip("The amount by which to scale.")]
	public float scaleFactor = 1f;

	public ScalingMode scalingMode;

	static ScaleSpriteByCamDistance()
	{
		instancesList = new List<ScaleSpriteByCamDistance>();
		SceneCamera.onSceneCameraPreCull += OnSceneCameraPreCull;
	}

	private void Awake()
	{
		transform = ((Component)this).transform;
	}

	private void OnEnable()
	{
		instancesList.Add(this);
	}

	private void OnDisable()
	{
		instancesList.Remove(this);
	}

	private static void OnSceneCameraPreCull(SceneCamera sceneCamera)
	{
		//IL_0006: Unknown result type (might be due to invalid IL or missing references)
		//IL_000b: Unknown result type (might be due to invalid IL or missing references)
		//IL_002d: Unknown result type (might be due to invalid IL or missing references)
		//IL_002f: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a6: Unknown result type (might be due to invalid IL or missing references)
		Vector3 position = ((Component)sceneCamera).transform.position;
		for (int i = 0; i < instancesList.Count; i++)
		{
			ScaleSpriteByCamDistance scaleSpriteByCamDistance = instancesList[i];
			Transform val = scaleSpriteByCamDistance.transform;
			float num = 1f;
			float num2 = Vector3.Distance(position, val.position);
			switch (scaleSpriteByCamDistance.scalingMode)
			{
			case ScalingMode.Direct:
				num = num2;
				break;
			case ScalingMode.Square:
				num = num2 * num2;
				break;
			case ScalingMode.Sqrt:
				num = Mathf.Sqrt(num2);
				break;
			case ScalingMode.Cube:
				num = num2 * num2 * num2;
				break;
			case ScalingMode.CubeRoot:
				num = Mathf.Pow(num2, 1f / 3f);
				break;
			}
			num *= scaleSpriteByCamDistance.scaleFactor;
			val.localScale = new Vector3(num, num, num);
		}
	}
}
