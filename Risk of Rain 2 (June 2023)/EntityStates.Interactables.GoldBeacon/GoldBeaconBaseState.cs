using RoR2;
using RoR2.Hologram;
using UnityEngine;

namespace EntityStates.Interactables.GoldBeacon;

public class GoldBeaconBaseState : BaseState
{
	protected void SetReady(bool ready)
	{
		Transform modelTransform = GetModelTransform();
		if (Object.op_Implicit((Object)(object)modelTransform))
		{
			PrintController component = ((Component)modelTransform).GetComponent<PrintController>();
			component.paused = !ready;
			if (!ready)
			{
				component.age = 0f;
			}
			ChildLocator component2 = ((Component)modelTransform).GetComponent<ChildLocator>();
			if (Object.op_Implicit((Object)(object)component2))
			{
				((Component)component2.FindChild("Purchased")).gameObject.SetActive(ready);
			}
		}
		PurchaseInteraction component3 = GetComponent<PurchaseInteraction>();
		if (Object.op_Implicit((Object)(object)component3))
		{
			component3.SetAvailable(!ready);
		}
		HologramProjector component4 = GetComponent<HologramProjector>();
		if (Object.op_Implicit((Object)(object)component4))
		{
			((Component)component4.hologramPivot).gameObject.SetActive(!ready);
		}
	}
}
