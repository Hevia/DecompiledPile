using RoR2.ConVar;
using UnityEngine;

namespace RoR2;

public class SplashScreenController : MonoBehaviour
{
	private static BoolConVar cvSplashSkip = new BoolConVar("splash_skip", ConVarFlags.Archive, "0", "Whether or not to skip startup splash screens.");

	private void Start()
	{
		if (cvSplashSkip.value)
		{
			Finish();
		}
	}

	public void Finish()
	{
		Console.instance.SubmitCmd(null, IntroCutsceneController.shouldSkip ? "set_scene title" : "set_scene intro");
	}
}
