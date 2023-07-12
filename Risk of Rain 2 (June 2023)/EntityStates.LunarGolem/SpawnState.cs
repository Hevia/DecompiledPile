using RoR2;
using UnityEngine;

namespace EntityStates.LunarGolem;

public class SpawnState : BaseState
{
	public static float duration = 1.333f;

	public static string spawnSoundString;

	public static GameObject spawnEffectPrefab;

	public override void OnEnter()
	{
		base.OnEnter();
		PlayAnimation("Body", "Spawn", "Spawn.playbackRate", duration);
		Util.PlaySound(spawnSoundString, base.gameObject);
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
