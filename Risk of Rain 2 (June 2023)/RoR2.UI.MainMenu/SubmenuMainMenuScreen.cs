using UnityEngine;
using UnityEngine.Serialization;

namespace RoR2.UI.MainMenu;

public class SubmenuMainMenuScreen : BaseMainMenuScreen
{
	[FormerlySerializedAs("settingsPanelPrefab")]
	public GameObject submenuPanelPrefab;

	private GameObject submenuPanelInstance;

	public override void OnEnter(MainMenuController mainMenuController)
	{
		base.OnEnter(mainMenuController);
		submenuPanelInstance = Object.Instantiate<GameObject>(submenuPanelPrefab, ((Component)this).transform);
	}

	public override void OnExit(MainMenuController mainMenuController)
	{
		Object.Destroy((Object)(object)submenuPanelInstance);
		base.OnExit(mainMenuController);
	}

	public new void Update()
	{
		if (!Object.op_Implicit((Object)(object)submenuPanelInstance) && Object.op_Implicit((Object)(object)myMainMenuController))
		{
			myMainMenuController.SetDesiredMenuScreen(myMainMenuController.titleMenuScreen);
		}
	}
}
