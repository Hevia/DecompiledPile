using System;
using UnityEngine;

namespace RoR2.Achievements;

[RegisterAchievement("LoopOnce", "Items.BounceNearby", null, null)]
public class LoopOnceAchievement : BaseAchievement
{
	public override void OnInstall()
	{
		base.OnInstall();
		UserProfile obj = base.userProfile;
		obj.onStatsReceived = (Action)Delegate.Combine(obj.onStatsReceived, new Action(Check));
		Check();
	}

	public override void OnUninstall()
	{
		UserProfile obj = base.userProfile;
		obj.onStatsReceived = (Action)Delegate.Remove(obj.onStatsReceived, new Action(Check));
		base.OnUninstall();
	}

	private void Check()
	{
		if (Object.op_Implicit((Object)(object)Run.instance) && ((object)Run.instance).GetType() == typeof(Run) && Run.instance.loopClearCount > 0)
		{
			SceneDef sceneDefForCurrentScene = SceneCatalog.GetSceneDefForCurrentScene();
			if (Object.op_Implicit((Object)(object)sceneDefForCurrentScene) && sceneDefForCurrentScene.sceneType == SceneType.Stage && !sceneDefForCurrentScene.isFinalStage)
			{
				Grant();
			}
		}
	}
}
