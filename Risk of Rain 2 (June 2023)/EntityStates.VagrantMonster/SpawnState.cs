using RoR2;
using UnityEngine;

namespace EntityStates.VagrantMonster;

public class SpawnState : BaseState
{
	private float stopwatch;

	public static float duration = 4f;

	public static string spawnSoundString;

	public static GameObject spawnEffectPrefab;

	public override void OnEnter()
	{
		base.OnEnter();
		PlayAnimation("Body", "Spawn", "Spawn.playbackRate", duration);
		Util.PlaySound(spawnSoundString, base.gameObject);
		if (Object.op_Implicit((Object)(object)spawnEffectPrefab))
		{
			EffectManager.SimpleMuzzleFlash(spawnEffectPrefab, base.gameObject, "SpawnOrigin", transmit: false);
		}
		Transform modelTransform = GetModelTransform();
		if (Object.op_Implicit((Object)(object)modelTransform))
		{
			((Behaviour)((Component)modelTransform).GetComponent<PrintController>()).enabled = true;
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
