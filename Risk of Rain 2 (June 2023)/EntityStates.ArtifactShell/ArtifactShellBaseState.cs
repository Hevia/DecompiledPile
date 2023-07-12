using RoR2;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.Rendering.PostProcessing;

namespace EntityStates.ArtifactShell;

public class ArtifactShellBaseState : BaseState
{
	protected Light light;

	private string stopLoopSound;

	protected PurchaseInteraction purchaseInteraction { get; private set; }

	protected virtual CostTypeIndex interactionCostType => CostTypeIndex.None;

	protected virtual bool interactionAvailable => false;

	protected virtual int interactionCost => 0;

	protected ParticleSystem rayParticleSystem { get; private set; }

	protected PostProcessVolume postProcessVolume { get; private set; }

	public override void OnEnter()
	{
		base.OnEnter();
		purchaseInteraction = GetComponent<PurchaseInteraction>();
		if (purchaseInteraction != null)
		{
			purchaseInteraction.costType = interactionCostType;
			purchaseInteraction.Networkcost = interactionCost;
			purchaseInteraction.Networkavailable = interactionAvailable;
			((UnityEvent<Interactor>)purchaseInteraction.onPurchase).AddListener((UnityAction<Interactor>)DoOnPurchase);
		}
		Transform obj = FindModelChild("RayParticles");
		rayParticleSystem = ((obj != null) ? ((Component)obj).GetComponent<ParticleSystem>() : null);
		Transform obj2 = FindModelChild("PP");
		postProcessVolume = ((obj2 != null) ? ((Component)obj2).GetComponent<PostProcessVolume>() : null);
		light = ((Component)FindModelChild("Light")).GetComponent<Light>();
		CalcLoopSounds(base.healthComponent.combinedHealthFraction, out var startLoopSound, out stopLoopSound);
		Util.PlaySound(startLoopSound, base.gameObject);
	}

	private static void CalcLoopSounds(float currentHealthFraction, out string startLoopSound, out string stopLoopSound)
	{
		startLoopSound = null;
		stopLoopSound = null;
		float num = 0.05f;
		if (currentHealthFraction > 0.75f + num)
		{
			startLoopSound = "Play_artifactBoss_loop_level1";
			stopLoopSound = "Stop_artifactBoss_loop_level1";
		}
		else if (currentHealthFraction > 0.25f + num)
		{
			startLoopSound = "Play_artifactBoss_loop_level2";
			stopLoopSound = "Stop_artifactBoss_loop_level2";
		}
		else if (currentHealthFraction > 0f + num)
		{
			startLoopSound = "Play_artifactBoss_loop_level2";
			stopLoopSound = "Stop_artifactBoss_loop_level2";
		}
	}

	public override void OnExit()
	{
		Util.PlaySound(stopLoopSound, base.gameObject);
		if (purchaseInteraction != null)
		{
			((UnityEvent<Interactor>)purchaseInteraction.onPurchase).RemoveListener((UnityAction<Interactor>)DoOnPurchase);
			purchaseInteraction = null;
		}
		base.OnExit();
	}

	public override void FixedUpdate()
	{
		base.FixedUpdate();
		UpdateVisuals();
	}

	private void DoOnPurchase(Interactor activator)
	{
		OnPurchase(activator);
	}

	protected virtual void OnPurchase(Interactor activator)
	{
	}

	protected void UpdateVisuals()
	{
		//IL_0025: Unknown result type (might be due to invalid IL or missing references)
		//IL_002a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0047: Unknown result type (might be due to invalid IL or missing references)
		//IL_0057: Unknown result type (might be due to invalid IL or missing references)
		//IL_005c: Unknown result type (might be due to invalid IL or missing references)
		float num = 1f - base.healthComponent.combinedHealthFraction;
		if (Object.op_Implicit((Object)(object)rayParticleSystem))
		{
			EmissionModule emission = rayParticleSystem.emission;
			((EmissionModule)(ref emission)).rateOverTime = MinMaxCurve.op_Implicit(Util.Remap(num, 0f, 1f, 0f, 10f));
			MainModule main = rayParticleSystem.main;
			((MainModule)(ref main)).simulationSpeed = Util.Remap(num, 0f, 1f, 1f, 5f);
		}
		if (Object.op_Implicit((Object)(object)postProcessVolume))
		{
			postProcessVolume.weight = num;
		}
		if (Object.op_Implicit((Object)(object)light))
		{
			light.range = 10f + num * 150f;
		}
	}
}
