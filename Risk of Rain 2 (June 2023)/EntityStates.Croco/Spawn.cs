using RoR2;
using UnityEngine;

namespace EntityStates.Croco;

public class Spawn : BaseState
{
	public static float minimumSleepDuration;

	public static GameObject spawnEffectPrefab;

	private Animator modelAnimator;

	public override void OnEnter()
	{
		//IL_0061: Unknown result type (might be due to invalid IL or missing references)
		base.OnEnter();
		base.modelLocator.normalizeToFloor = true;
		modelAnimator = GetModelAnimator();
		if (Object.op_Implicit((Object)(object)modelAnimator))
		{
			modelAnimator.SetFloat(AnimationParameters.aimWeight, 0f);
		}
		PlayAnimation("Body", "SleepLoop");
		EffectManager.SpawnEffect(spawnEffectPrefab, new EffectData
		{
			origin = base.characterBody.footPosition
		}, transmit: false);
	}

	public override void OnExit()
	{
		if (Object.op_Implicit((Object)(object)modelAnimator))
		{
			modelAnimator.SetFloat(AnimationParameters.aimWeight, 1f);
		}
		base.OnExit();
	}

	public override void FixedUpdate()
	{
		base.FixedUpdate();
		if (base.isAuthority && base.fixedAge >= minimumSleepDuration && (((Vector3)(ref base.inputBank.moveVector)).sqrMagnitude >= Mathf.Epsilon || base.inputBank.CheckAnyButtonDown()))
		{
			outer.SetNextState(new WakeUp());
		}
	}
}
