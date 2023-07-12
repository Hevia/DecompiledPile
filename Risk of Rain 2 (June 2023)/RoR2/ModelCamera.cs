using System;
using System.Collections.Generic;
using UnityEngine;

namespace RoR2;

[RequireComponent(typeof(Camera))]
public class ModelCamera : MonoBehaviour
{
	private struct ObjectRestoreInfo
	{
		public GameObject obj;

		public int layer;
	}

	[NonSerialized]
	public RenderSettingsState renderSettings;

	public Color ambientLight;

	private readonly List<Light> lights = new List<Light>();

	public static ModelCamera instance { get; private set; }

	public Camera attachedCamera { get; private set; }

	private void OnEnable()
	{
		if (Object.op_Implicit((Object)(object)instance) && (Object)(object)instance != (Object)(object)this)
		{
			Debug.LogErrorFormat("Only one {0} instance can be active at a time.", new object[1] { ((object)this).GetType().Name });
		}
		else
		{
			instance = this;
		}
	}

	private void OnDisable()
	{
		if ((Object)(object)instance == (Object)(object)this)
		{
			instance = null;
		}
	}

	private void Awake()
	{
		//IL_0026: Unknown result type (might be due to invalid IL or missing references)
		attachedCamera = ((Component)this).GetComponent<Camera>();
		((Behaviour)attachedCamera).enabled = false;
		attachedCamera.cullingMask = LayerMask.op_Implicit(LayerIndex.manualRender.mask);
		Object.Destroy((Object)(object)((Component)this).GetComponent<AkAudioListener>());
	}

	private static void PrepareObjectForRendering(Transform objTransform, List<ObjectRestoreInfo> objectRestorationList)
	{
		GameObject gameObject = ((Component)objTransform).gameObject;
		objectRestorationList.Add(new ObjectRestoreInfo
		{
			obj = gameObject,
			layer = gameObject.layer
		});
		gameObject.layer = LayerIndex.manualRender.intVal;
		int childCount = objTransform.childCount;
		for (int i = 0; i < childCount; i++)
		{
			PrepareObjectForRendering(objTransform.GetChild(i), objectRestorationList);
		}
	}

	public void RenderItem(GameObject obj, RenderTexture targetTexture)
	{
		//IL_0018: Unknown result type (might be due to invalid IL or missing references)
		for (int i = 0; i < lights.Count; i++)
		{
			lights[i].cullingMask = LayerMask.op_Implicit(LayerIndex.manualRender.mask);
		}
		RenderSettingsState renderSettingsState = RenderSettingsState.FromCurrent();
		renderSettings.Apply();
		List<ObjectRestoreInfo> list = new List<ObjectRestoreInfo>();
		if (Object.op_Implicit((Object)(object)obj))
		{
			PrepareObjectForRendering(obj.transform, list);
		}
		attachedCamera.targetTexture = targetTexture;
		attachedCamera.Render();
		for (int j = 0; j < list.Count; j++)
		{
			list[j].obj.layer = list[j].layer;
		}
		for (int k = 0; k < lights.Count; k++)
		{
			lights[k].cullingMask = 0;
		}
		renderSettingsState.Apply();
	}

	public void AddLight(Light light)
	{
		lights.Add(light);
	}

	public void RemoveLight(Light light)
	{
		lights.Remove(light);
	}
}
