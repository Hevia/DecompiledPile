using RoR2;
using UnityEngine;

namespace EntityStates.MiniMushroom;

public class UnPlant : BaseState
{
	public static GameObject plantEffectPrefab;

	public static float baseDuration;

	public static string UnplantOutSoundString;

	private float stopwatch;

	private Transform modelTransform;

	private ChildLocator childLocator;

	private float duration;

	public override void OnEnter()
	{
		base.OnEnter();
		duration = baseDuration / attackSpeedStat;
		EffectManager.SimpleMuzzleFlash(plantEffectPrefab, base.gameObject, "BurrowCenter", transmit: false);
		Util.PlaySound(UnplantOutSoundString, base.gameObject);
		PlayAnimation("Plant", "PlantEnd", "PlantEnd.playbackRate", duration);
	}

	public override void OnExit()
	{
		PlayAnimation("Plant, Additive", "Empty");
		base.OnExit();
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
		return InterruptPriority.Skill;
	}
}
