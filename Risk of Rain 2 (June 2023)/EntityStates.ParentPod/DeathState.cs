using RoR2;
using UnityEngine;

namespace EntityStates.ParentPod;

public class DeathState : GenericCharacterDeath
{
	public static float deathAnimTimer;

	private float mDeathAnimTimer;

	public static GameObject deathEffect;

	private bool printingStarted;

	public override void OnEnter()
	{
		//IL_0021: Unknown result type (might be due to invalid IL or missing references)
		//IL_002c: Unknown result type (might be due to invalid IL or missing references)
		base.OnEnter();
		mDeathAnimTimer = deathAnimTimer;
		EffectManager.SimpleEffect(deathEffect, base.gameObject.transform.position, base.transform.rotation, transmit: false);
	}

	public override void FixedUpdate()
	{
		mDeathAnimTimer -= Time.deltaTime;
		if (mDeathAnimTimer <= 0f && !printingStarted)
		{
			printingStarted = true;
			PrintController printController = ((Component)GetComponent<ModelLocator>().modelTransform).gameObject.AddComponent<PrintController>();
			((Behaviour)printController).enabled = false;
			printController.printTime = 1f;
			printController.startingPrintHeight = 99999f;
			printController.maxPrintHeight = 99999f;
			printController.startingPrintBias = 0.95f;
			printController.maxPrintBias = 1.95f;
			printController.animateFlowmapPower = true;
			printController.startingFlowmapPower = 1.14f;
			printController.maxFlowmapPower = 30f;
			printController.disableWhenFinished = false;
			printController.printCurve = AnimationCurve.EaseInOut(0f, 0f, 1f, 1f);
			((Behaviour)printController).enabled = true;
		}
		if (printingStarted)
		{
			base.FixedUpdate();
		}
	}
}
