using UnityEngine;

namespace RoR2.UI;

public class DifficultyTutorialController : MonoBehaviour
{
	private HUD hud;

	[Tooltip("The tutorial popup object.")]
	public GameObject difficultyTutorialObject;

	[Tooltip("The time at which to trigger the tutorial popup.")]
	public float difficultyTutorialTriggerTime = 60f;

	private void Awake()
	{
		hud = ((Component)this).GetComponentInParent<HUD>();
		if (Object.op_Implicit((Object)(object)difficultyTutorialObject))
		{
			difficultyTutorialObject.SetActive(false);
		}
	}

	private void Update()
	{
		if (Object.op_Implicit((Object)(object)hud))
		{
			UserProfile userProfile = hud.localUserViewer.userProfile;
			CharacterBody cachedBody = hud.localUserViewer.cachedBody;
			if (userProfile != null && Object.op_Implicit((Object)(object)cachedBody) && Object.op_Implicit((Object)(object)difficultyTutorialObject) && userProfile.tutorialDifficulty.shouldShow && Object.op_Implicit((Object)(object)Run.instance) && Run.instance.fixedTime >= difficultyTutorialTriggerTime)
			{
				difficultyTutorialObject.SetActive(true);
				userProfile.tutorialDifficulty.shouldShow = false;
				userProfile.tutorialDifficulty.showCount++;
			}
		}
	}
}
