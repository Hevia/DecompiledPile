using RoR2;
using UnityEngine;

namespace EntityStates.Assassin2;

public class SpawnState : BaseState
{
	private float stopwatch;

	public static float animDuration = 1.5f;

	public static float duration = 4f;

	public static string spawnSoundString;

	public static GameObject spawnEffectPrefab;

	public override void OnEnter()
	{
		base.OnEnter();
		PlayAnimation("Body", "Spawn");
		Util.PlaySound(spawnSoundString, base.gameObject);
		if (Object.op_Implicit((Object)(object)spawnEffectPrefab))
		{
			EffectManager.SimpleMuzzleFlash(spawnEffectPrefab, base.gameObject, "Base", transmit: false);
		}
	}

	public override void FixedUpdate()
	{
		base.FixedUpdate();
		stopwatch += Time.fixedDeltaTime;
		if (stopwatch >= duration && base.isAuthority)
		{
			outer.SetNextStateToMain();
		}
	}

	public override InterruptPriority GetMinimumInterruptPriority()
	{
		return InterruptPriority.Death;
	}
}
