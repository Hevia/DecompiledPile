using System.Collections.ObjectModel;
using Rewired;
using UnityEngine;

namespace RoR2;

[RequireComponent(typeof(CameraRigController))]
public class CameraRigControllerSpectateControls : MonoBehaviour
{
	private CameraRigController cameraRigController;

	private void Awake()
	{
		cameraRigController = ((Component)this).GetComponent<CameraRigController>();
	}

	private static bool CanUserSpectateBody(NetworkUser viewer, CharacterBody body)
	{
		return Object.op_Implicit((Object)(object)Util.LookUpBodyNetworkUser(((Component)body).gameObject));
	}

	public static GameObject GetNextSpectateGameObject(NetworkUser viewer, GameObject currentGameObject)
	{
		ReadOnlyCollection<CharacterBody> readOnlyInstancesList = CharacterBody.readOnlyInstancesList;
		if (readOnlyInstancesList.Count == 0)
		{
			return null;
		}
		CharacterBody characterBody = (Object.op_Implicit((Object)(object)currentGameObject) ? currentGameObject.GetComponent<CharacterBody>() : null);
		int num = (Object.op_Implicit((Object)(object)characterBody) ? readOnlyInstancesList.IndexOf(characterBody) : 0);
		for (int i = num + 1; i < readOnlyInstancesList.Count; i++)
		{
			if (CanUserSpectateBody(viewer, readOnlyInstancesList[i]))
			{
				return ((Component)readOnlyInstancesList[i]).gameObject;
			}
		}
		for (int j = 0; j <= num; j++)
		{
			if (CanUserSpectateBody(viewer, readOnlyInstancesList[j]))
			{
				return ((Component)readOnlyInstancesList[j]).gameObject;
			}
		}
		return null;
	}

	public static GameObject GetPreviousSpectateGameObject(NetworkUser viewer, GameObject currentGameObject)
	{
		ReadOnlyCollection<CharacterBody> readOnlyInstancesList = CharacterBody.readOnlyInstancesList;
		if (readOnlyInstancesList.Count == 0)
		{
			return null;
		}
		CharacterBody characterBody = (Object.op_Implicit((Object)(object)currentGameObject) ? currentGameObject.GetComponent<CharacterBody>() : null);
		int num = (Object.op_Implicit((Object)(object)characterBody) ? readOnlyInstancesList.IndexOf(characterBody) : 0);
		for (int num2 = num - 1; num2 >= 0; num2--)
		{
			if (CanUserSpectateBody(viewer, readOnlyInstancesList[num2]))
			{
				return ((Component)readOnlyInstancesList[num2]).gameObject;
			}
		}
		for (int num3 = readOnlyInstancesList.Count - 1; num3 >= num; num3--)
		{
			if (CanUserSpectateBody(viewer, readOnlyInstancesList[num3]))
			{
				return ((Component)readOnlyInstancesList[num3]).gameObject;
			}
		}
		return null;
	}

	private void Update()
	{
		Player val = cameraRigController.localUserViewer?.inputPlayer;
		if (cameraRigController.cameraMode != null && cameraRigController.cameraMode.IsSpectating(cameraRigController) && val != null)
		{
			if (val.GetButtonDown(7))
			{
				cameraRigController.nextTarget = GetNextSpectateGameObject(cameraRigController.viewer, cameraRigController.target);
			}
			if (val.GetButtonDown(8))
			{
				cameraRigController.nextTarget = GetPreviousSpectateGameObject(cameraRigController.viewer, cameraRigController.target);
			}
		}
	}
}
