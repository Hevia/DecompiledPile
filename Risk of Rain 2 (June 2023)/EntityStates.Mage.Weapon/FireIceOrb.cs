using RoR2;
using RoR2.Projectile;
using UnityEngine;

namespace EntityStates.Mage.Weapon;

public class FireIceOrb : BaseState
{
	public enum Gauntlet
	{
		Left,
		Right
	}

	public static GameObject projectilePrefab;

	public static GameObject effectPrefab;

	public static float baseDuration = 2f;

	public static float damageCoefficient = 1.2f;

	public static string attackString;

	private float duration;

	private bool hasFiredGauntlet;

	private string muzzleString;

	private Animator animator;

	public Gauntlet gauntlet;

	public override void OnEnter()
	{
		base.OnEnter();
		duration = baseDuration / attackSpeedStat;
		switch (gauntlet)
		{
		case Gauntlet.Left:
			muzzleString = "MuzzleLeft";
			PlayAnimation("Gesture Left, Additive", "FireGauntletLeft", "FireGauntlet.playbackRate", duration);
			break;
		case Gauntlet.Right:
			muzzleString = "MuzzleRight";
			PlayAnimation("Gesture Right, Additive", "FireGauntletRight", "FireGauntlet.playbackRate", duration);
			break;
		}
		PlayAnimation("Gesture, Additive", "HoldGauntletsUp", "FireGauntlet.playbackRate", duration);
		Util.PlaySound(attackString, base.gameObject);
		animator = GetModelAnimator();
		base.characterBody.SetAimTimer(2f);
		FireGauntlet();
	}

	public override void OnExit()
	{
		base.OnExit();
	}

	private void FireGauntlet()
	{
		//IL_0008: Unknown result type (might be due to invalid IL or missing references)
		//IL_000d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0045: Unknown result type (might be due to invalid IL or missing references)
		//IL_004c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0051: Unknown result type (might be due to invalid IL or missing references)
		hasFiredGauntlet = true;
		Ray aimRay = GetAimRay();
		if (Object.op_Implicit((Object)(object)effectPrefab))
		{
			EffectManager.SimpleMuzzleFlash(effectPrefab, base.gameObject, muzzleString, transmit: false);
		}
		if (base.isAuthority)
		{
			ProjectileManager.instance.FireProjectile(projectilePrefab, aimRay.origin, Util.QuaternionSafeLookRotation(aimRay.direction), base.gameObject, damageStat * damageCoefficient, 0f, Util.CheckRoll(critStat, base.characterBody.master));
		}
	}

	public override void FixedUpdate()
	{
		base.FixedUpdate();
		if (animator.GetFloat("FireGauntlet.fire") > 0f && !hasFiredGauntlet)
		{
			FireGauntlet();
		}
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
