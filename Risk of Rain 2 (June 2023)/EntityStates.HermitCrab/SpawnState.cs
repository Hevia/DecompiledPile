using RoR2;
using UnityEngine;

namespace EntityStates.HermitCrab;

public class SpawnState : BaseState
{
	private float stopwatch;

	public static GameObject burrowPrefab;

	public static float duration = 4f;

	public static string spawnSoundString;

	public override void OnEnter()
	{
		base.OnEnter();
		Util.PlaySound(spawnSoundString, base.gameObject);
		PlayAnimation("Body", "Spawn", "Spawn.playbackRate", duration);
		EffectManager.SimpleMuzzleFlash(burrowPrefab, base.gameObject, "BurrowCenter", transmit: false);
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
