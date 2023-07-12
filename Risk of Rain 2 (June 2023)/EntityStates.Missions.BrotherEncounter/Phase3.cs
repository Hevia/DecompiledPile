using EntityStates.BrotherMonster;
using RoR2;
using UnityEngine;

namespace EntityStates.Missions.BrotherEncounter;

public class Phase3 : BrotherEncounterPhaseBaseState
{
	protected override string phaseControllerChildString => "Phase3";

	protected override EntityState nextState => new Phase4();

	public override void OnEnter()
	{
		base.OnEnter();
	}

	public override void OnExit()
	{
		KillAllMonsters();
		base.OnExit();
	}

	protected override void OnMemberAddedServer(CharacterMaster master)
	{
		base.OnMemberAddedServer(master);
		if (!master.hasBody)
		{
			return;
		}
		CharacterBody body = master.GetBody();
		if (Object.op_Implicit((Object)(object)body))
		{
			CharacterDeathBehavior component = ((Component)body).GetComponent<CharacterDeathBehavior>();
			if (Object.op_Implicit((Object)(object)component))
			{
				component.deathState = new SerializableEntityStateType(typeof(InstantDeathState));
			}
		}
	}
}
