using RoR2;
using UnityEngine;

namespace EntityStates.ImpMonster;

public class DeathState : GenericCharacterDeath
{
	public static GameObject initialEffect;

	public static GameObject deathEffect;

	private static float duration = 1.333f;

	private float stopwatch;

	private Animator animator;

	private bool hasPlayedDeathEffect;

	public override void OnEnter()
	{
		base.OnEnter();
		animator = GetModelAnimator();
		if (Object.op_Implicit((Object)(object)base.characterMotor))
		{
			((Behaviour)base.characterMotor).enabled = false;
		}
		if (Object.op_Implicit((Object)(object)base.modelLocator) && Object.op_Implicit((Object)(object)((Component)base.modelLocator.modelTransform).GetComponent<ChildLocator>()) && Object.op_Implicit((Object)(object)initialEffect))
		{
			EffectManager.SimpleMuzzleFlash(initialEffect, base.gameObject, "Base", transmit: false);
		}
	}

	public override void FixedUpdate()
	{
		base.FixedUpdate();
		if (Object.op_Implicit((Object)(object)animator))
		{
			stopwatch += Time.fixedDeltaTime;
			if (!hasPlayedDeathEffect && animator.GetFloat("DeathEffect") > 0.5f)
			{
				hasPlayedDeathEffect = true;
				EffectManager.SimpleMuzzleFlash(deathEffect, base.gameObject, "Center", transmit: false);
			}
			if (stopwatch >= duration)
			{
				EntityState.Destroy((Object)(object)base.gameObject);
			}
		}
	}
}
