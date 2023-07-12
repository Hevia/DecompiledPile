using RoR2;
using UnityEngine;
using UnityEngine.Networking;

namespace EntityStates.DeepVoidPortalBattery;

public class Charged : BaseDeepVoidPortalBatteryState
{
	public override void OnEnter()
	{
		base.OnEnter();
		if (Object.op_Implicit((Object)(object)VoidStageMissionController.instance) && NetworkServer.active)
		{
			VoidStageMissionController.instance.OnBatteryActivated();
		}
	}
}
