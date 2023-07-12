using UnityEngine;

namespace EntityStates.Missions.ArtifactWorld.TrialController;

public class DuringTrial : ArtifactTrialControllerBaseState
{
	public virtual EntityState GetNextState()
	{
		return new AfterTrial();
	}

	public override void OnEnter()
	{
		base.OnEnter();
		((Behaviour)purchaseInteraction).enabled = false;
		((Component)childLocator.FindChild("DuringTrial")).gameObject.SetActive(true);
	}

	public override void OnExit()
	{
		((Component)childLocator.FindChild("DuringTrial")).gameObject.SetActive(false);
		base.OnExit();
	}
}
