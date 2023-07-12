using EntityStates;
using UnityEngine;

namespace RoR2.UI;

[RequireComponent(typeof(HUD))]
public class HUDTutorialController : MonoBehaviour
{
	private HUD hud;

	[Header("Equipment Tutorial")]
	[Tooltip("The tutorial popup object.")]
	public GameObject equipmentTutorialObject;

	[Tooltip("The equipment icon to monitor for a change to trigger the tutorial popup.")]
	public EquipmentIcon equipmentIcon;

	[Header("Sprint Tutorial")]
	[Tooltip("The tutorial popup object.")]
	public GameObject sprintTutorialObject;

	[Tooltip("How long to wait for the player to sprint before showing the tutorial popup.")]
	public float sprintTutorialTriggerTime = 30f;

	private float sprintTutorialStopwatch;

	private void Awake()
	{
		hud = ((Component)this).GetComponent<HUD>();
		if (Object.op_Implicit((Object)(object)equipmentTutorialObject))
		{
			equipmentTutorialObject.SetActive(false);
		}
		if (Object.op_Implicit((Object)(object)sprintTutorialObject))
		{
			sprintTutorialObject.SetActive(false);
		}
	}

	private UserProfile GetUserProfile()
	{
		if (Object.op_Implicit((Object)(object)hud) && hud.localUserViewer != null)
		{
			return hud.localUserViewer.userProfile;
		}
		return null;
	}

	private void HandleTutorial(GameObject tutorialPopup, ref UserProfile.TutorialProgression tutorialProgression, bool dismiss = false, bool progress = true)
	{
		if (Object.op_Implicit((Object)(object)tutorialPopup) && !dismiss)
		{
			tutorialPopup.SetActive(true);
		}
		tutorialProgression.shouldShow = false;
		if (progress)
		{
			tutorialProgression.showCount++;
		}
	}

	private void Update()
	{
		if (!Object.op_Implicit((Object)(object)hud) || hud.localUserViewer == null)
		{
			return;
		}
		UserProfile userProfile = GetUserProfile();
		CharacterBody cachedBody = hud.localUserViewer.cachedBody;
		if (userProfile == null || !Object.op_Implicit((Object)(object)cachedBody))
		{
			return;
		}
		if (userProfile.tutorialEquipment.shouldShow && equipmentIcon.hasEquipment)
		{
			HandleTutorial(equipmentTutorialObject, ref userProfile.tutorialEquipment);
		}
		if (userProfile.tutorialSprint.shouldShow)
		{
			if (cachedBody.isSprinting)
			{
				HandleTutorial(null, ref userProfile.tutorialSprint, dismiss: true);
				return;
			}
			if (((Component)cachedBody).GetComponent<EntityStateMachine>()?.state is GenericCharacterMain)
			{
				sprintTutorialStopwatch += Time.deltaTime;
			}
			if (sprintTutorialStopwatch >= sprintTutorialTriggerTime)
			{
				HandleTutorial(sprintTutorialObject, ref userProfile.tutorialSprint, dismiss: false, progress: false);
			}
		}
		else if (Object.op_Implicit((Object)(object)sprintTutorialObject) && sprintTutorialObject.activeInHierarchy && cachedBody.isSprinting)
		{
			Object.Destroy((Object)(object)sprintTutorialObject);
			sprintTutorialObject = null;
			userProfile.tutorialSprint.showCount++;
		}
	}
}
