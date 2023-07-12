using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.Playables;

namespace RoR2;

[CreateAssetMenu(menuName = "RoR2/Singletons/GlobalEventMethodLibrary")]
public class GlobalEventMethodLibrary : ScriptableObject
{
	public void RunAdvanceStageServer(SceneDef nextScene)
	{
		if (NetworkServer.active && Object.op_Implicit((Object)(object)Run.instance))
		{
			Run.instance.AdvanceStage(nextScene);
		}
	}

	public void RunBeginGameOverServer(GameEndingDef endingDef)
	{
		if (NetworkServer.active && Object.op_Implicit((Object)(object)Run.instance))
		{
			Run.instance.BeginGameOver(endingDef);
		}
	}

	public void LogMessage(string message)
	{
		Debug.Log((object)message);
	}

	public void ForcePlayableDirectorFinish(PlayableDirector playableDirector)
	{
		playableDirector.time = playableDirector.duration;
	}

	public void DestroyObject(Object obj)
	{
		Object.Destroy(obj);
	}

	public void DisableAllSiblings(Transform transform)
	{
		if (!Object.op_Implicit((Object)(object)transform))
		{
			return;
		}
		Transform parent = transform.parent;
		if (!Object.op_Implicit((Object)(object)parent))
		{
			return;
		}
		int i = 0;
		for (int childCount = parent.childCount; i < childCount; i++)
		{
			Transform child = parent.GetChild(i);
			if (Object.op_Implicit((Object)(object)child) && (Object)(object)child != (Object)(object)transform)
			{
				((Component)child).gameObject.SetActive(false);
			}
		}
	}

	public void DisableAllSiblings(GameObject gameObject)
	{
		if (Object.op_Implicit((Object)(object)gameObject))
		{
			DisableAllSiblings(gameObject.transform);
		}
	}

	public void ActivateGameObjectIfServer(GameObject gameObject)
	{
		if (Object.op_Implicit((Object)(object)gameObject) && NetworkServer.active)
		{
			gameObject.SetActive(true);
		}
	}

	public void DeactivateGameObjectIfServer(GameObject gameObject)
	{
		if (Object.op_Implicit((Object)(object)gameObject) && NetworkServer.active)
		{
			gameObject.SetActive(false);
		}
	}
}
