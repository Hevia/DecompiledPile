using System.Collections.Generic;
using UnityEngine;

namespace EntityStates.Missions.BrotherEncounter;

public class Phase2 : BrotherEncounterPhaseBaseState
{
	public static float delayBetweenPillarActivation;

	private List<GameObject> pillarsToActive = new List<GameObject>();

	private float pillarActivationStopwatch;

	protected override string phaseControllerChildString => "Phase2";

	protected override EntityState nextState => new Phase3();

	public override void OnEnter()
	{
		base.OnEnter();
		GameObject val = ((Component)childLocator.FindChild("BlockingPillars")).gameObject;
		if (Object.op_Implicit((Object)(object)val))
		{
			val.SetActive(true);
			for (int i = 0; i < val.transform.childCount; i++)
			{
				pillarsToActive.Add(((Component)val.transform.GetChild(i)).gameObject);
			}
		}
	}

	public override void FixedUpdate()
	{
		base.FixedUpdate();
		pillarActivationStopwatch += Time.fixedDeltaTime;
		if (pillarsToActive.Count > 0 && pillarActivationStopwatch > delayBetweenPillarActivation)
		{
			pillarActivationStopwatch = 0f;
			pillarsToActive[0].SetActive(true);
			pillarsToActive.RemoveAt(0);
		}
	}
}
