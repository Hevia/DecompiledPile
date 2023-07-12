using RoR2;
using UnityEngine;

namespace EntityStates.LunarTeleporter;

public class LunarTeleporterBaseState : BaseState
{
	protected GenericInteraction genericInteraction;

	protected Interactability preferredInteractability = Interactability.Available;

	protected TeleporterInteraction teleporterInteraction;

	public override void OnEnter()
	{
		base.OnEnter();
		genericInteraction = GetComponent<GenericInteraction>();
		teleporterInteraction = ((Component)base.transform.root).GetComponent<TeleporterInteraction>();
	}

	public override void FixedUpdate()
	{
		base.FixedUpdate();
		if (Object.op_Implicit((Object)(object)TeleporterInteraction.instance))
		{
			if (TeleporterInteraction.instance.isIdle)
			{
				genericInteraction.Networkinteractability = preferredInteractability;
			}
			else
			{
				genericInteraction.Networkinteractability = Interactability.Disabled;
			}
		}
	}
}
