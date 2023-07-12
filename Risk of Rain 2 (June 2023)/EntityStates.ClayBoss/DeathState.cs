using RoR2;
using UnityEngine;
using UnityEngine.Networking;

namespace EntityStates.ClayBoss;

public class DeathState : GenericCharacterDeath
{
	public static GameObject initialEffect;

	public static GameObject deathEffect;

	public static float duration = 2f;

	private float stopwatch;

	private Transform modelBaseTransform;

	private Transform centerTransform;

	private bool attemptedDeathBehavior;

	public override void OnEnter()
	{
		//IL_0057: Unknown result type (might be due to invalid IL or missing references)
		//IL_0062: Unknown result type (might be due to invalid IL or missing references)
		base.OnEnter();
		if (!Object.op_Implicit((Object)(object)base.modelLocator))
		{
			return;
		}
		ChildLocator component = ((Component)base.modelLocator.modelTransform).GetComponent<ChildLocator>();
		if (Object.op_Implicit((Object)(object)component))
		{
			centerTransform = component.FindChild("Center");
			if (Object.op_Implicit((Object)(object)initialEffect))
			{
				GameObject obj = Object.Instantiate<GameObject>(initialEffect, centerTransform.position, centerTransform.rotation);
				obj.GetComponent<ScaleParticleSystemDuration>().newDuration = duration;
				obj.transform.parent = centerTransform;
			}
		}
		modelBaseTransform = base.modelLocator.modelBaseTransform;
	}

	private void AttemptDeathBehavior()
	{
		//IL_0041: Unknown result type (might be due to invalid IL or missing references)
		if (!attemptedDeathBehavior)
		{
			attemptedDeathBehavior = true;
			if (Object.op_Implicit((Object)(object)deathEffect) && NetworkServer.active && Object.op_Implicit((Object)(object)centerTransform))
			{
				EffectManager.SpawnEffect(deathEffect, new EffectData
				{
					origin = centerTransform.position
				}, transmit: true);
			}
			if (Object.op_Implicit((Object)(object)modelBaseTransform))
			{
				EntityState.Destroy((Object)(object)((Component)modelBaseTransform).gameObject);
				modelBaseTransform = null;
			}
			if (NetworkServer.active)
			{
				EntityState.Destroy((Object)(object)base.gameObject);
			}
		}
	}

	public override void FixedUpdate()
	{
		stopwatch += Time.fixedDeltaTime;
		if (stopwatch >= duration)
		{
			AttemptDeathBehavior();
		}
	}

	public override void OnExit()
	{
		if (!outer.destroying)
		{
			AttemptDeathBehavior();
		}
		base.OnExit();
	}
}
