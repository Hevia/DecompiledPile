using RoR2;
using UnityEngine;

namespace EntityStates.BeetleQueenMonster;

public class SpawnState : BaseState
{
	public static float duration = 4f;

	public static GameObject burrowPrefab;

	public static string spawnSoundString;

	public override void OnEnter()
	{
		base.OnEnter();
		Util.PlaySound(spawnSoundString, base.gameObject);
		((Component)GetModelTransform()).GetComponent<ChildLocator>();
		PlayAnimation("Body", "Spawn", "Spawn.playbackRate", duration);
		string muzzleName = "BurrowCenter";
		EffectManager.SimpleMuzzleFlash(burrowPrefab, base.gameObject, muzzleName, transmit: false);
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
