using RoR2;
using UnityEngine;
using UnityEngine.Networking;

namespace EntityStates.Missions.Goldshores;

public class ActivateBeacons : EntityState
{
	public override void OnEnter()
	{
		base.OnEnter();
		if (Object.op_Implicit((Object)(object)GoldshoresMissionController.instance))
		{
			GoldshoresMissionController.instance.SpawnBeacons();
		}
	}

	public override void OnExit()
	{
		base.OnExit();
		if (!outer.destroying && Object.op_Implicit((Object)(object)GoldshoresMissionController.instance))
		{
			GoldshoresMissionController.instance.BeginTransitionIntoBossfight();
		}
	}

	public override void FixedUpdate()
	{
		base.FixedUpdate();
		if (NetworkServer.active && Object.op_Implicit((Object)(object)GoldshoresMissionController.instance) && GoldshoresMissionController.instance.beaconsActive >= GoldshoresMissionController.instance.beaconsRequiredToSpawnBoss)
		{
			outer.SetNextState(new GoldshoresBossfight());
		}
	}
}
