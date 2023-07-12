using System.Collections.Generic;
using RoR2;
using UnityEngine;
using UnityEngine.Networking;

namespace EntityStates.Missions.BrotherEncounter;

public class BrotherEncounterBaseState : EntityState
{
	protected ChildLocator childLocator;

	protected virtual bool shouldEnableArenaWalls => true;

	protected virtual bool shouldEnableArenaNodes => true;

	public override void OnEnter()
	{
		base.OnEnter();
		childLocator = GetComponent<ChildLocator>();
		Transform val = childLocator.FindChild("ArenaWalls");
		Transform val2 = childLocator.FindChild("ArenaNodes");
		if (Object.op_Implicit((Object)(object)val))
		{
			((Component)val).gameObject.SetActive(shouldEnableArenaWalls);
		}
		if (Object.op_Implicit((Object)(object)val2))
		{
			((Component)val2).gameObject.SetActive(shouldEnableArenaNodes);
		}
	}

	public override void OnExit()
	{
		base.OnExit();
	}

	public void KillAllMonsters()
	{
		if (!NetworkServer.active)
		{
			return;
		}
		foreach (TeamComponent item in new List<TeamComponent>(TeamComponent.GetTeamMembers(TeamIndex.Monster)))
		{
			if (Object.op_Implicit((Object)(object)item))
			{
				HealthComponent component = ((Component)item).GetComponent<HealthComponent>();
				if (Object.op_Implicit((Object)(object)component))
				{
					component.Suicide();
				}
			}
		}
	}
}
