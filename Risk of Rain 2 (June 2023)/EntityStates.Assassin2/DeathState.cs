using RoR2;
using UnityEngine;

namespace EntityStates.Assassin2;

public class DeathState : GenericCharacterDeath
{
	public static GameObject deathEffectPrefab;

	public static float duration = 1.333f;

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
	}

	public override void FixedUpdate()
	{
		//IL_0058: Unknown result type (might be due to invalid IL or missing references)
		base.FixedUpdate();
		if (Object.op_Implicit((Object)(object)animator))
		{
			stopwatch += Time.fixedDeltaTime;
			if (!hasPlayedDeathEffect && animator.GetFloat("DeathEffect") > 0.5f)
			{
				hasPlayedDeathEffect = true;
				EffectData effectData = new EffectData();
				effectData.origin = base.transform.position;
				EffectManager.SpawnEffect(deathEffectPrefab, effectData, transmit: false);
			}
			if (stopwatch >= duration)
			{
				DestroyModel();
				EntityState.Destroy((Object)(object)base.gameObject);
			}
		}
	}

	public override void OnExit()
	{
		DestroyModel();
		base.OnExit();
	}
}
