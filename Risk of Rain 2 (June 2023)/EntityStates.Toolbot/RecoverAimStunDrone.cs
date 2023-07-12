using RoR2;
using UnityEngine;

namespace EntityStates.Toolbot;

public class RecoverAimStunDrone : BaseState
{
	public static GameObject muzzleEffectPrefab;

	public static string fireSoundString;

	public static float baseDuration;

	private float duration;

	public override void OnEnter()
	{
		base.OnEnter();
		duration = baseDuration / attackSpeedStat;
		PlayAnimation("Gesture, Additive", "FireBomb", "FireBomb.playbackRate", duration);
		Util.PlaySound(fireSoundString, base.gameObject);
		if (Object.op_Implicit((Object)(object)muzzleEffectPrefab))
		{
			EffectManager.SimpleMuzzleFlash(muzzleEffectPrefab, base.gameObject, "MuzzleNailgun", transmit: false);
		}
	}

	public override void OnExit()
	{
		base.OnExit();
		PlayCrossfade("Stance, Override", "Empty", 0.1f);
	}

	public override void FixedUpdate()
	{
		base.FixedUpdate();
		if (base.isAuthority && base.fixedAge >= duration)
		{
			outer.SetNextStateToMain();
		}
	}

	public override InterruptPriority GetMinimumInterruptPriority()
	{
		return InterruptPriority.Skill;
	}
}
