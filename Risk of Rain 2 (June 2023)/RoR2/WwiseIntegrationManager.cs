using UnityEngine;

namespace RoR2;

public static class WwiseIntegrationManager
{
	private static GameObject wwiseGlobalObjectInstance;

	private static GameObject wwiseAudioObjectInstance;

	public static bool noAudio => false;

	public static void Init()
	{
		if (!noAudio)
		{
			if (Object.op_Implicit((Object)(object)Object.FindObjectOfType<AkInitializer>()))
			{
				Debug.LogError((object)"Attempting to initialize wwise when AkInitializer already exists! This will cause a crash!");
				return;
			}
			wwiseGlobalObjectInstance = Object.Instantiate<GameObject>(LegacyResourcesAPI.Load<GameObject>("Prefabs/WwiseGlobal"));
			wwiseAudioObjectInstance = Object.Instantiate<GameObject>(LegacyResourcesAPI.Load<GameObject>("Prefabs/AudioManager"));
		}
	}
}
