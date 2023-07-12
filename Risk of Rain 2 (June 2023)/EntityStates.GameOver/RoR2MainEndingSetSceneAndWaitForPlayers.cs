using RoR2;
using UnityEngine;
using UnityEngine.Networking;

namespace EntityStates.GameOver;

public class RoR2MainEndingSetSceneAndWaitForPlayers : BaseGameOverControllerState
{
	private SceneDef desiredSceneDef;

	public override void OnEnter()
	{
		base.OnEnter();
		FadeToBlackManager.ForceFullBlack();
		FadeToBlackManager.fadeCount++;
		desiredSceneDef = SceneCatalog.GetSceneDefFromSceneName("outro");
		if (NetworkServer.active)
		{
			Run.instance.AdvanceStage(desiredSceneDef);
		}
	}

	public override void FixedUpdate()
	{
		base.FixedUpdate();
		if (NetworkServer.active && NetworkUser.AllParticipatingNetworkUsersReady() && (Object)(object)SceneCatalog.mostRecentSceneDef == (Object)(object)desiredSceneDef)
		{
			outer.SetNextState(new RoR2MainEndingPlayCutscene());
		}
	}

	public override void OnExit()
	{
		FadeToBlackManager.fadeCount--;
		base.OnExit();
	}
}
