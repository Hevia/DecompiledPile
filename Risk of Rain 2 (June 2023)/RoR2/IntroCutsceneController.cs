using RoR2.ConVar;
using UnityEngine;

namespace RoR2;

public class IntroCutsceneController : MonoBehaviour
{
	private static BoolConVar cvIntroSkip = new BoolConVar("intro_skip", ConVarFlags.Archive, "0", "Whether or not to skip the opening cutscene.");

	public static bool shouldSkip => cvIntroSkip.value;

	public void Finish()
	{
		Console.instance.SubmitCmd(null, "set_scene title");
	}
}
