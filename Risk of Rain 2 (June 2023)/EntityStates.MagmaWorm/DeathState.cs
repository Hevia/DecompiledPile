using RoR2;
using UnityEngine;
using UnityEngine.Networking;

namespace EntityStates.MagmaWorm;

public class DeathState : GenericCharacterDeath
{
	public static GameObject initialDeathExplosionEffect;

	public static string deathSoundString;

	public static float duration;

	private float stopwatch;

	public override void OnEnter()
	{
		base.OnEnter();
		WormBodyPositions2 component = GetComponent<WormBodyPositions2>();
		WormBodyPositionsDriver component2 = GetComponent<WormBodyPositionsDriver>();
		if (Object.op_Implicit((Object)(object)component))
		{
			component2.yDamperConstant = 0f;
			component2.ySpringConstant = 0f;
			component2.maxTurnSpeed = 0f;
			component.meatballCount = 0;
			Util.PlaySound(deathSoundString, ((Component)component.bones[0]).gameObject);
		}
		Transform modelTransform = GetModelTransform();
		if (!Object.op_Implicit((Object)(object)modelTransform))
		{
			return;
		}
		PrintController printController = ((Component)modelTransform).gameObject.AddComponent<PrintController>();
		printController.printTime = duration;
		((Behaviour)printController).enabled = true;
		printController.startingPrintHeight = 99999f;
		printController.maxPrintHeight = 99999f;
		printController.startingPrintBias = 1f;
		printController.maxPrintBias = 3.5f;
		printController.animateFlowmapPower = true;
		printController.startingFlowmapPower = 1.14f;
		printController.maxFlowmapPower = 30f;
		printController.disableWhenFinished = false;
		printController.printCurve = AnimationCurve.EaseInOut(0f, 0f, 1f, 1f);
		ParticleSystem[] componentsInChildren = ((Component)modelTransform).GetComponentsInChildren<ParticleSystem>();
		for (int i = 0; i < componentsInChildren.Length; i++)
		{
			componentsInChildren[i].Stop();
		}
		ChildLocator component3 = ((Component)modelTransform).GetComponent<ChildLocator>();
		if (Object.op_Implicit((Object)(object)component3))
		{
			Transform val = component3.FindChild("PP");
			if (Object.op_Implicit((Object)(object)val))
			{
				PostProcessDuration component4 = ((Component)val).GetComponent<PostProcessDuration>();
				if (Object.op_Implicit((Object)(object)component4))
				{
					((Behaviour)component4).enabled = true;
					component4.maxDuration = duration;
				}
			}
		}
		if (NetworkServer.active)
		{
			EffectManager.SimpleMuzzleFlash(initialDeathExplosionEffect, base.gameObject, "HeadCenter", transmit: true);
		}
	}

	public override void FixedUpdate()
	{
		stopwatch += Time.fixedDeltaTime;
		if (NetworkServer.active && stopwatch > duration)
		{
			EntityState.Destroy((Object)(object)base.gameObject);
		}
	}
}
