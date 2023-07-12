using EntityStates.SurvivorPod;
using RoR2;
using UnityEngine;

namespace EntityStates.VoidSurvivorPod;

public class Landed : SurvivorPodBaseState
{
	[SerializeField]
	public string animationLayerName;

	[SerializeField]
	public string animationStateName;

	[SerializeField]
	public string enterSoundString;

	[SerializeField]
	public string effectMuzzle;

	[SerializeField]
	public GameObject openEffect;

	[SerializeField]
	public string particleChildName;

	private ParticleSystem[] particles;

	public override void OnEnter()
	{
		base.OnEnter();
		PlayAnimation(animationLayerName, animationStateName);
		Util.PlaySound(enterSoundString, base.gameObject);
		base.survivorPodController.exitAllowed = true;
		base.vehicleSeat.handleVehicleExitRequestServer.AddCallback(HandleVehicleExitRequest);
		ModelLocator component = GetComponent<ModelLocator>();
		if (!Object.op_Implicit((Object)(object)component) || !Object.op_Implicit((Object)(object)component.modelTransform))
		{
			return;
		}
		ChildLocator component2 = ((Component)component.modelTransform).GetComponent<ChildLocator>();
		if (Object.op_Implicit((Object)(object)component2))
		{
			Transform val = component2.FindChild(particleChildName);
			if (Object.op_Implicit((Object)(object)val))
			{
				particles = ((Component)val).GetComponentsInChildren<ParticleSystem>();
				((Component)val).gameObject.SetActive(true);
			}
		}
	}

	private void HandleVehicleExitRequest(GameObject gameObject, ref bool? result)
	{
		base.survivorPodController.exitAllowed = false;
		outer.SetNextState(new Release());
		result = true;
	}

	public override void OnExit()
	{
		base.vehicleSeat.handleVehicleExitRequestServer.RemoveCallback(HandleVehicleExitRequest);
		base.survivorPodController.exitAllowed = false;
		EffectManager.SimpleMuzzleFlash(openEffect, base.gameObject, effectMuzzle, transmit: false);
		ParticleSystem[] array = particles;
		for (int i = 0; i < array.Length; i++)
		{
			array[i].Stop();
		}
	}
}
