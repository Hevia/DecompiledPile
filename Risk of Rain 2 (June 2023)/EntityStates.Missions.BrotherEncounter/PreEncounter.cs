using UnityEngine;

namespace EntityStates.Missions.BrotherEncounter;

public class PreEncounter : BaseState
{
	public static float duration;

	private ChildLocator childLocator;

	public override void OnEnter()
	{
		base.OnEnter();
		childLocator = GetComponent<ChildLocator>();
		((Component)childLocator.FindChild("PreEncounter")).gameObject.SetActive(true);
		Debug.Log((object)"Entering pre-encounter");
	}

	public override void FixedUpdate()
	{
		base.FixedUpdate();
		if (base.fixedAge >= duration)
		{
			outer.SetNextState(new Phase1());
		}
	}

	public override void OnExit()
	{
		((Component)childLocator.FindChild("PreEncounter")).gameObject.SetActive(false);
		base.OnExit();
	}
}
