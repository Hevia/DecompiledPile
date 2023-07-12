using RoR2;
using UnityEngine;

namespace EntityStates.BrotherMonster;

public class ThroneSpawnState : BaseState
{
	public static GameObject spawnEffectPrefab;

	public static string muzzleString;

	public static float initialDelay;

	public static float duration;

	private bool hasPlayedAnimation;

	public override void OnEnter()
	{
		base.OnEnter();
		PlayAnimation("Body", "Throne");
		PlayAnimation("FullBody Override", "BufferEmpty");
		EffectManager.SimpleMuzzleFlash(spawnEffectPrefab, base.gameObject, muzzleString, transmit: false);
	}

	private void PlayAnimation()
	{
		hasPlayedAnimation = true;
		PlayAnimation("Body", "ThroneToIdle", "ThroneToIdle.playbackRate", duration);
	}

	public override void FixedUpdate()
	{
		base.FixedUpdate();
		if (base.fixedAge > initialDelay && !hasPlayedAnimation)
		{
			PlayAnimation();
		}
		if (base.fixedAge > initialDelay + duration)
		{
			outer.SetNextStateToMain();
		}
	}
}
