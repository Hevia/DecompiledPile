using RoR2;
using UnityEngine;

namespace EntityStates.NewtMonster;

public class SpawnState : EntityState
{
	public static float duration = 2f;

	public static string spawnSoundString;

	public static GameObject spawnEffectPrefab;

	public override void OnEnter()
	{
		base.OnEnter();
		GetModelAnimator();
		Util.PlaySound(spawnSoundString, base.gameObject);
		PlayAnimation("Body", "Spawn", "Spawn.playbackRate", duration);
		if (Object.op_Implicit((Object)(object)spawnEffectPrefab))
		{
			EffectManager.SimpleMuzzleFlash(spawnEffectPrefab, base.gameObject, "SpawnEffectOrigin", transmit: false);
		}
	}

	public override void FixedUpdate()
	{
		base.FixedUpdate();
		if (base.fixedAge >= duration && base.isAuthority)
		{
			outer.SetNextStateToMain();
		}
	}

	public override InterruptPriority GetMinimumInterruptPriority()
	{
		return InterruptPriority.Death;
	}
}
