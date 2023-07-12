using RoR2;
using UnityEngine;

namespace EntityStates.Barrel;

public class ActivateFan : EntityState
{
	public override void OnEnter()
	{
		base.OnEnter();
		PlayAnimation("Base", "IdleToActive");
		Transform modelTransform = GetModelTransform();
		if (Object.op_Implicit((Object)(object)modelTransform))
		{
			ChildLocator component = ((Component)modelTransform).GetComponent<ChildLocator>();
			if (Object.op_Implicit((Object)(object)component))
			{
				((Component)component.FindChild("JumpVolume")).gameObject.SetActive(true);
				((Component)component.FindChild("LightBack")).gameObject.SetActive(true);
				((Component)component.FindChild("LightFront")).gameObject.SetActive(true);
			}
		}
		if (Object.op_Implicit((Object)(object)base.sfxLocator))
		{
			Util.PlaySound(base.sfxLocator.openSound, base.gameObject);
		}
	}
}
