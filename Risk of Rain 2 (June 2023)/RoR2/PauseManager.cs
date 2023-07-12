using System;
using UnityEngine;
using UnityEngine.Networking;

namespace RoR2;

public static class PauseManager
{
	private static GameObject pauseScreenInstance;

	public static Action onPauseStartGlobal;

	public static Action onPauseEndGlobal;

	public static bool isPaused => Object.op_Implicit((Object)(object)pauseScreenInstance);

	[ConCommand(commandName = "pause", flags = ConVarFlags.None, helpText = "Toggles game pause state.")]
	private static void CCTogglePause(ConCommandArgs args)
	{
		if (Object.op_Implicit((Object)(object)pauseScreenInstance))
		{
			Object.Destroy((Object)(object)pauseScreenInstance);
			pauseScreenInstance = null;
		}
		else if (NetworkManager.singleton.isNetworkActive)
		{
			pauseScreenInstance = Object.Instantiate<GameObject>(LegacyResourcesAPI.Load<GameObject>("Prefabs/UI/PauseScreen"), ((Component)RoR2Application.instance).transform);
		}
	}
}
