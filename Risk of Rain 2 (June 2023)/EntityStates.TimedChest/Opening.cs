using System;
using RoR2;
using UnityEngine;

namespace EntityStates.TimedChest;

public class Opening : BaseState
{
	public static float delayUntilUnlockAchievement;

	private bool hasGrantedAchievement;

	public static event Action onOpened;

	public override void OnEnter()
	{
		base.OnEnter();
		PlayAnimation("Body", "Opening");
		TimedChestController component = GetComponent<TimedChestController>();
		if (Object.op_Implicit((Object)(object)component))
		{
			component.purchased = true;
		}
		if (Object.op_Implicit((Object)(object)base.sfxLocator))
		{
			Util.PlaySound(base.sfxLocator.openSound, base.gameObject);
		}
	}

	public override void FixedUpdate()
	{
		base.FixedUpdate();
		if (base.fixedAge >= delayUntilUnlockAchievement && !hasGrantedAchievement)
		{
			Opening.onOpened?.Invoke();
			hasGrantedAchievement = true;
			GenericPickupController componentInChildren = base.gameObject.GetComponentInChildren<GenericPickupController>();
			if (Object.op_Implicit((Object)(object)componentInChildren))
			{
				((Behaviour)componentInChildren).enabled = true;
			}
		}
	}
}
