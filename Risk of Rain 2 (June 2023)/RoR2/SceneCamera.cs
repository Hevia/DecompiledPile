using UnityEngine;
using UnityEngine.Rendering;

namespace RoR2;

[RequireComponent(typeof(Camera))]
public class SceneCamera : MonoBehaviour
{
	public delegate void SceneCameraDelegate(SceneCamera sceneCamera);

	public OpaqueSortMode sortMode = (OpaqueSortMode)2;

	public Camera camera { get; private set; }

	public CameraRigController cameraRigController { get; private set; }

	public static event SceneCameraDelegate onSceneCameraPreCull;

	public static event SceneCameraDelegate onSceneCameraPreRender;

	public static event SceneCameraDelegate onSceneCameraPostRender;

	private void Awake()
	{
		camera = ((Component)this).GetComponent<Camera>();
		cameraRigController = ((Component)this).GetComponentInParent<CameraRigController>();
	}

	private void OnPreCull()
	{
		if (SceneCamera.onSceneCameraPreCull != null)
		{
			SceneCamera.onSceneCameraPreCull(this);
		}
	}

	private void OnPreRender()
	{
		//IL_000e: Unknown result type (might be due to invalid IL or missing references)
		if (SceneCamera.onSceneCameraPreRender != null)
		{
			camera.opaqueSortMode = sortMode;
			SceneCamera.onSceneCameraPreRender(this);
		}
	}

	private void OnPostRender()
	{
		if (SceneCamera.onSceneCameraPostRender != null)
		{
			SceneCamera.onSceneCameraPostRender(this);
		}
	}
}
