using UnityEngine;

namespace EntityStates.Missions.ArtifactWorld.TrialController;

public class BeforeTrial : ArtifactTrialControllerBaseState
{
	public override void OnEnter()
	{
		base.OnEnter();
		((Behaviour)purchaseInteraction).enabled = true;
		((Component)childLocator.FindChild("BeforeTrial")).gameObject.SetActive(true);
	}

	public override void OnExit()
	{
		((Component)childLocator.FindChild("BeforeTrial")).gameObject.SetActive(false);
		base.OnExit();
	}
}
