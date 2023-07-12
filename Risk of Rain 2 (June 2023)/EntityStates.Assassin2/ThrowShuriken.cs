using RoR2;
using UnityEngine;

namespace EntityStates.Assassin2;

public class ThrowShuriken : GenericProjectileBaseState
{
	public static string attackString;

	private Transform muzzleTransform;

	public override void OnEnter()
	{
		//IL_003b: Unknown result type (might be due to invalid IL or missing references)
		base.OnEnter();
		duration = baseDuration / attackSpeedStat;
		PlayAnimation("Gesture", "ThrowShuriken");
		Util.PlaySound(attackString, base.gameObject);
		GetAimRay();
		string text = "ShurikenTag";
		muzzleTransform = FindModelChild(text);
		if (Object.op_Implicit((Object)(object)effectPrefab))
		{
			EffectManager.SimpleMuzzleFlash(effectPrefab, base.gameObject, text, transmit: false);
		}
		if (base.isAuthority)
		{
			FireProjectile();
		}
	}

	public override void OnExit()
	{
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
