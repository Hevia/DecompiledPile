using RoR2;
using UnityEngine;
using UnityEngine.Networking;

namespace EntityStates.GameOver;

public class RoR2MainEndingPlayCutscene : BaseGameOverControllerState
{
	public override void FixedUpdate()
	{
		base.FixedUpdate();
		if (NetworkServer.active && (!Object.op_Implicit((Object)(object)OutroCutsceneController.instance) || OutroCutsceneController.instance.cutsceneIsFinished))
		{
			outer.SetNextStateToMain();
		}
	}

	public override void OnExit()
	{
		if (Object.op_Implicit((Object)(object)OutroCutsceneController.instance) && Object.op_Implicit((Object)(object)OutroCutsceneController.instance.playableDirector))
		{
			OutroCutsceneController.instance.playableDirector.time = OutroCutsceneController.instance.playableDirector.duration;
		}
		base.OnExit();
	}
}
