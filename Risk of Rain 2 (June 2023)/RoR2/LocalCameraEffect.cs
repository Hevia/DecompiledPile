using System.Collections.Generic;
using UnityEngine;

namespace RoR2;

public class LocalCameraEffect : MonoBehaviour
{
	public GameObject targetCharacter;

	public GameObject effectRoot;

	private static List<LocalCameraEffect> instancesList;

	static LocalCameraEffect()
	{
		instancesList = new List<LocalCameraEffect>();
		UICamera.onUICameraPreCull += OnUICameraPreCull;
	}

	private static void OnUICameraPreCull(UICamera uiCamera)
	{
		for (int i = 0; i < instancesList.Count; i++)
		{
			GameObject target = uiCamera.cameraRigController.target;
			LocalCameraEffect localCameraEffect = instancesList[i];
			if ((Object)(object)localCameraEffect.targetCharacter == (Object)(object)target)
			{
				localCameraEffect.effectRoot.SetActive(true);
			}
			else
			{
				localCameraEffect.effectRoot.SetActive(false);
			}
		}
	}

	private void Start()
	{
		instancesList.Add(this);
	}

	private void OnDestroy()
	{
		instancesList.Remove(this);
	}
}
