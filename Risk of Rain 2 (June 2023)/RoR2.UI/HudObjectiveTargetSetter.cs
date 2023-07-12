using UnityEngine;

namespace RoR2.UI;

public class HudObjectiveTargetSetter : MonoBehaviour
{
	public ObjectivePanelController objectivePanelController;

	private HUD hud;

	private void OnEnable()
	{
		hud = ((Component)this).GetComponentInParent<HUD>();
	}

	private void Update()
	{
		if (Object.op_Implicit((Object)(object)hud) && Object.op_Implicit((Object)(object)objectivePanelController))
		{
			objectivePanelController.SetCurrentMaster(hud.targetMaster);
		}
	}
}
