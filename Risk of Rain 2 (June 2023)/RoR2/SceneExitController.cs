using System;
using System.Collections.ObjectModel;
using UnityEngine;
using UnityEngine.Networking;

namespace RoR2;

public class SceneExitController : MonoBehaviour
{
	public enum ExitState
	{
		Idle,
		ExtractExp,
		TeleportOut,
		Finished
	}

	public bool useRunNextStageScene;

	public SceneDef destinationScene;

	private const float teleportOutDuration = 4f;

	private float teleportOutTimer;

	private ExitState exitState;

	private ConvertPlayerMoneyToExperience experienceCollector;

	public static bool isRunning { get; private set; }

	public static event Action<SceneExitController> onBeginExit;

	public static event Action<SceneExitController> onFinishExit;

	public void Begin()
	{
		if (NetworkServer.active && exitState == ExitState.Idle)
		{
			SetState((!Run.instance.ruleBook.keepMoneyBetweenStages) ? ExitState.ExtractExp : ExitState.TeleportOut);
		}
	}

	public void SetState(ExitState newState)
	{
		//IL_00a5: Unknown result type (might be due to invalid IL or missing references)
		//IL_00aa: Unknown result type (might be due to invalid IL or missing references)
		if (newState == exitState)
		{
			return;
		}
		exitState = newState;
		switch (exitState)
		{
		case ExitState.ExtractExp:
			if (!isRunning)
			{
				SceneExitController.onBeginExit?.Invoke(this);
			}
			isRunning = true;
			experienceCollector = ((Component)this).gameObject.AddComponent<ConvertPlayerMoneyToExperience>();
			break;
		case ExitState.TeleportOut:
		{
			ReadOnlyCollection<CharacterMaster> readOnlyInstancesList = CharacterMaster.readOnlyInstancesList;
			for (int i = 0; i < readOnlyInstancesList.Count; i++)
			{
				CharacterMaster component = ((Component)readOnlyInstancesList[i]).GetComponent<CharacterMaster>();
				if (Object.op_Implicit((Object)(object)((Component)component).GetComponent<SetDontDestroyOnLoad>()))
				{
					GameObject bodyObject = component.GetBodyObject();
					if (Object.op_Implicit((Object)(object)bodyObject))
					{
						GameObject obj = Object.Instantiate<GameObject>(LegacyResourcesAPI.Load<GameObject>("Prefabs/NetworkedObjects/TeleportOutController"), bodyObject.transform.position, Quaternion.identity);
						obj.GetComponent<TeleportOutController>().Networktarget = bodyObject;
						NetworkServer.Spawn(obj);
					}
				}
			}
			teleportOutTimer = 4f;
			break;
		}
		case ExitState.Finished:
			SceneExitController.onFinishExit?.Invoke(this);
			if (!Object.op_Implicit((Object)(object)Run.instance) || !Run.instance.isGameOverServer)
			{
				if (useRunNextStageScene)
				{
					Stage.instance.BeginAdvanceStage(Run.instance.nextStageScene);
				}
				else if (Object.op_Implicit((Object)(object)destinationScene))
				{
					Stage.instance.BeginAdvanceStage(destinationScene);
				}
				else
				{
					Debug.Log((object)"SceneExitController: destinationScene not set!");
				}
			}
			break;
		case ExitState.Idle:
			break;
		}
	}

	private void FixedUpdate()
	{
		if (NetworkServer.active)
		{
			UpdateServer();
		}
	}

	private void UpdateServer()
	{
		switch (exitState)
		{
		case ExitState.ExtractExp:
			if (!Object.op_Implicit((Object)(object)experienceCollector))
			{
				SetState(ExitState.TeleportOut);
			}
			break;
		case ExitState.TeleportOut:
			teleportOutTimer -= Time.fixedDeltaTime;
			if (teleportOutTimer <= 0f)
			{
				SetState(ExitState.Finished);
			}
			break;
		default:
			throw new ArgumentOutOfRangeException();
		case ExitState.Idle:
		case ExitState.Finished:
			break;
		}
	}

	private void OnDestroy()
	{
		isRunning = false;
	}

	private void OnEnable()
	{
		InstanceTracker.Add(this);
	}

	private void OnDisable()
	{
		InstanceTracker.Remove(this);
	}
}
