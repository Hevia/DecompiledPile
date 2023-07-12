using System;
using RoR2;
using UnityEngine;

namespace EntityStates.Missions.ArtifactWorld.TrialController;

public class AfterTrial : ArtifactTrialControllerBaseState
{
	public virtual Type GetNextStateType()
	{
		return typeof(FinishTrial);
	}

	public override void OnEnter()
	{
		base.OnEnter();
		((Behaviour)purchaseInteraction).enabled = true;
		((Component)childLocator.FindChild("AfterTrial")).gameObject.SetActive(true);
		outer.mainStateType = new SerializableEntityStateType(GetNextStateType());
		Highlight component = GetComponent<Highlight>();
		Transform val = childLocator.FindChild("CompletedArtifactMesh");
		if (Object.op_Implicit((Object)(object)component) && Object.op_Implicit((Object)(object)val))
		{
			component.targetRenderer = (Renderer)(object)((Component)val).GetComponent<MeshRenderer>();
		}
	}

	public override void OnExit()
	{
		((Component)childLocator.FindChild("AfterTrial")).gameObject.SetActive(false);
		base.OnExit();
	}
}
