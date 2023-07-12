using RoR2;
using UnityEngine;
using UnityEngine.Networking;

namespace EntityStates.GameOver;

public class ShowCredits : BaseGameOverControllerState
{
	private GameObject creditsControllerInstance;

	public override void OnEnter()
	{
		base.OnEnter();
		if (NetworkServer.active)
		{
			creditsControllerInstance = Object.Instantiate<GameObject>(LegacyResourcesAPI.Load<GameObject>("Prefabs/NetworkedObjects/CreditsController"));
			NetworkServer.Spawn(creditsControllerInstance);
		}
	}

	public override void OnExit()
	{
		if (NetworkServer.active && Object.op_Implicit((Object)(object)creditsControllerInstance))
		{
			EntityState.Destroy((Object)(object)creditsControllerInstance);
		}
		base.OnExit();
	}

	public override void FixedUpdate()
	{
		base.FixedUpdate();
		if (NetworkServer.active && !Object.op_Implicit((Object)(object)creditsControllerInstance))
		{
			outer.SetNextState(new ShowReport());
		}
	}
}
