using RoR2;
using UnityEngine;

namespace EntityStates.MiniMushroom;

public class InPlant : BaseState
{
	public static GameObject burrowPrefab;

	public static float baseDuration;

	public static string burrowInSoundString;

	private float duration;

	public override void OnEnter()
	{
		base.OnEnter();
		duration = baseDuration / attackSpeedStat;
		Util.PlaySound(burrowInSoundString, base.gameObject);
		EffectManager.SimpleMuzzleFlash(burrowPrefab, base.gameObject, "BurrowCenter", transmit: false);
		PlayAnimation("Plant", "PlantStart", "PlantStart.playbackRate", duration);
	}

	public override void OnExit()
	{
		PlayAnimation("Plant", "Empty");
		base.OnExit();
	}

	public override void FixedUpdate()
	{
		base.FixedUpdate();
		if (base.isAuthority && base.fixedAge >= duration)
		{
			outer.SetNextState(new Plant());
		}
	}

	public override InterruptPriority GetMinimumInterruptPriority()
	{
		return InterruptPriority.Skill;
	}
}
