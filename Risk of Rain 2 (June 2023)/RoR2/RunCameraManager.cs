using System.Collections.ObjectModel;
using RoR2.CameraModes;
using UnityEngine;

namespace RoR2;

public class RunCameraManager : MonoBehaviour
{
	private readonly CameraRigController[] cameras = new CameraRigController[RoR2Application.maxLocalPlayers];

	private static readonly Rect[][] screenLayouts = new Rect[5][]
	{
		(Rect[])(object)new Rect[0],
		(Rect[])(object)new Rect[1]
		{
			new Rect(0f, 0f, 1f, 1f)
		},
		(Rect[])(object)new Rect[2]
		{
			new Rect(0f, 0.5f, 1f, 0.5f),
			new Rect(0f, 0f, 1f, 0.5f)
		},
		(Rect[])(object)new Rect[3]
		{
			new Rect(0f, 0.5f, 1f, 0.5f),
			new Rect(0f, 0f, 0.5f, 0.5f),
			new Rect(0.5f, 0f, 0.5f, 0.5f)
		},
		(Rect[])(object)new Rect[4]
		{
			new Rect(0f, 0.5f, 0.5f, 0.5f),
			new Rect(0.5f, 0.5f, 0.5f, 0.5f),
			new Rect(0f, 0f, 0.5f, 0.5f),
			new Rect(0.5f, 0f, 0.5f, 0.5f)
		}
	};

	private static GameObject GetNetworkUserBodyObject(NetworkUser networkUser)
	{
		if (Object.op_Implicit((Object)(object)networkUser.masterObject))
		{
			CharacterMaster component = networkUser.masterObject.GetComponent<CharacterMaster>();
			if (Object.op_Implicit((Object)(object)component))
			{
				return component.GetBodyObject();
			}
		}
		return null;
	}

	private static TeamIndex GetNetworkUserTeamIndex(NetworkUser networkUser)
	{
		if (Object.op_Implicit((Object)(object)networkUser.masterObject))
		{
			CharacterMaster component = networkUser.masterObject.GetComponent<CharacterMaster>();
			if (Object.op_Implicit((Object)(object)component))
			{
				return component.teamIndex;
			}
		}
		return TeamIndex.Neutral;
	}

	private void Update()
	{
		//IL_01b7: Unknown result type (might be due to invalid IL or missing references)
		//IL_01bc: Unknown result type (might be due to invalid IL or missing references)
		bool flag = Object.op_Implicit((Object)(object)Stage.instance);
		if (flag)
		{
			int i = 0;
			for (int count = CameraRigController.readOnlyInstancesList.Count; i < count; i++)
			{
				if (CameraRigController.readOnlyInstancesList[i].suppressPlayerCameras)
				{
					flag = false;
					return;
				}
			}
		}
		if (flag)
		{
			int num = 0;
			ReadOnlyCollection<NetworkUser> readOnlyLocalPlayersList = NetworkUser.readOnlyLocalPlayersList;
			for (int j = 0; j < readOnlyLocalPlayersList.Count; j++)
			{
				NetworkUser networkUser = readOnlyLocalPlayersList[j];
				CameraRigController cameraRigController = cameras[num];
				if (!Object.op_Implicit((Object)(object)cameraRigController))
				{
					cameraRigController = Object.Instantiate<GameObject>(LegacyResourcesAPI.Load<GameObject>("Prefabs/Main Camera")).GetComponent<CameraRigController>();
					cameras[num] = cameraRigController;
				}
				cameraRigController.viewer = networkUser;
				networkUser.cameraRigController = cameraRigController;
				GameObject networkUserBodyObject = GetNetworkUserBodyObject(networkUser);
				ForceSpectate forceSpectate = InstanceTracker.FirstOrNull<ForceSpectate>();
				if (Object.op_Implicit((Object)(object)forceSpectate))
				{
					cameraRigController.nextTarget = forceSpectate.target;
					cameraRigController.cameraMode = CameraModePlayerBasic.spectator;
				}
				else if (Object.op_Implicit((Object)(object)networkUserBodyObject))
				{
					cameraRigController.nextTarget = networkUserBodyObject;
					cameraRigController.cameraMode = CameraModePlayerBasic.playerBasic;
				}
				else if (!cameraRigController.disableSpectating)
				{
					cameraRigController.cameraMode = CameraModePlayerBasic.spectator;
					if (!Object.op_Implicit((Object)(object)cameraRigController.target))
					{
						cameraRigController.nextTarget = CameraRigControllerSpectateControls.GetNextSpectateGameObject(networkUser, null);
					}
				}
				else
				{
					cameraRigController.cameraMode = CameraModeNone.instance;
				}
				num++;
			}
			int num2 = num;
			for (int k = num; k < cameras.Length; k++)
			{
				ref CameraRigController reference = ref cameras[num];
				if (reference != null)
				{
					if (Object.op_Implicit((Object)(object)reference))
					{
						Object.Destroy((Object)(object)((Component)cameras[num]).gameObject);
					}
					reference = null;
				}
			}
			Rect[] array = screenLayouts[num2];
			for (int l = 0; l < num2; l++)
			{
				cameras[l].viewport = array[l];
			}
			return;
		}
		for (int m = 0; m < cameras.Length; m++)
		{
			if (Object.op_Implicit((Object)(object)cameras[m]))
			{
				Object.Destroy((Object)(object)((Component)cameras[m]).gameObject);
			}
		}
	}
}
