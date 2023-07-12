using System.Collections.Generic;
using UnityEngine;

namespace EntityStates.VoidRaidCrab.Joint;

public class PreDeathState : BaseState
{
	[SerializeField]
	public float minDuration;

	[SerializeField]
	public string joint1Name;

	[SerializeField]
	public string joint2Name;

	[SerializeField]
	public string joint3Name;

	[SerializeField]
	public GameObject jointEffectPrefab;

	public bool canProceed;

	private List<GameObject> jointEffects = new List<GameObject>();

	public override void OnEnter()
	{
		base.OnEnter();
		canProceed = false;
		jointEffects.Clear();
		ChildLocator modelChildLocator = GetModelChildLocator();
		if (Object.op_Implicit((Object)(object)modelChildLocator))
		{
			SpawnJointEffect(joint1Name, modelChildLocator);
			SpawnJointEffect(joint2Name, modelChildLocator);
			SpawnJointEffect(joint3Name, modelChildLocator);
		}
	}

	public override void OnExit()
	{
		base.OnExit();
		foreach (GameObject jointEffect in jointEffects)
		{
			Object.Destroy((Object)(object)jointEffect);
		}
		jointEffects.Clear();
	}

	public override void FixedUpdate()
	{
		base.FixedUpdate();
		if (base.isAuthority && base.fixedAge > minDuration && canProceed)
		{
			outer.SetNextState(new DeathState());
		}
	}

	private void SpawnJointEffect(string jointName, ChildLocator childLocator)
	{
		Transform val = childLocator.FindChild(jointName);
		if (Object.op_Implicit((Object)(object)val))
		{
			GameObject item = Object.Instantiate<GameObject>(jointEffectPrefab, val);
			jointEffects.Add(item);
		}
	}
}
