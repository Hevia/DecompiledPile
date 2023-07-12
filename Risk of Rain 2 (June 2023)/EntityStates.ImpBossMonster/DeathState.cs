using RoR2;
using UnityEngine;
using UnityEngine.Networking;

namespace EntityStates.ImpBossMonster;

public class DeathState : GenericCharacterDeath
{
	public static GameObject initialEffect;

	public static GameObject deathEffect;

	private static float duration = 3.3166666f;

	private float stopwatch;

	private Animator animator;

	private bool hasPlayedDeathEffect;

	private bool attemptedDeathBehavior;

	public override void OnEnter()
	{
		base.OnEnter();
		animator = GetModelAnimator();
		if (Object.op_Implicit((Object)(object)base.characterMotor))
		{
			((Behaviour)base.characterMotor).enabled = false;
		}
		if (Object.op_Implicit((Object)(object)base.modelLocator))
		{
			Transform modelTransform = base.modelLocator.modelTransform;
			ChildLocator component = ((Component)modelTransform).GetComponent<ChildLocator>();
			CharacterModel component2 = ((Component)modelTransform).GetComponent<CharacterModel>();
			if (Object.op_Implicit((Object)(object)component))
			{
				((Component)component.FindChild("DustCenter")).gameObject.SetActive(false);
				if (Object.op_Implicit((Object)(object)initialEffect))
				{
					EffectManager.SimpleMuzzleFlash(initialEffect, base.gameObject, "DeathCenter", transmit: false);
				}
			}
			if (Object.op_Implicit((Object)(object)component2))
			{
				for (int i = 0; i < component2.baseRendererInfos.Length; i++)
				{
					component2.baseRendererInfos[i].ignoreOverlays = true;
				}
			}
		}
		PlayAnimation("Fullbody Override", "Death");
	}

	public override void FixedUpdate()
	{
		if (Object.op_Implicit((Object)(object)animator))
		{
			stopwatch += Time.fixedDeltaTime;
			if (!hasPlayedDeathEffect && animator.GetFloat("DeathEffect") > 0.5f)
			{
				hasPlayedDeathEffect = true;
				EffectManager.SimpleMuzzleFlash(deathEffect, base.gameObject, "DeathCenter", transmit: false);
			}
			if (stopwatch >= duration)
			{
				AttemptDeathBehavior();
			}
		}
	}

	private void AttemptDeathBehavior()
	{
		if (!attemptedDeathBehavior)
		{
			attemptedDeathBehavior = true;
			if (Object.op_Implicit((Object)(object)base.modelLocator.modelBaseTransform))
			{
				EntityState.Destroy((Object)(object)((Component)base.modelLocator.modelBaseTransform).gameObject);
			}
			if (NetworkServer.active)
			{
				EntityState.Destroy((Object)(object)base.gameObject);
			}
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
