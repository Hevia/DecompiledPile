using RoR2;
using UnityEngine;

namespace EntityStates.Mage.Weapon;

public class FireLaserbolt : BaseState
{
	public enum Gauntlet
	{
		Left,
		Right
	}

	public static GameObject muzzleEffectPrefab;

	public static GameObject tracerEffectPrefab;

	public static GameObject impactEffectPrefab;

	public static float baseDuration = 2f;

	public static float damageCoefficient = 1.2f;

	public static float force = 20f;

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
		//IL_005c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0061: Unknown result type (might be due to invalid IL or missing references)
		//IL_0069: Unknown result type (might be due to invalid IL or missing references)
		hasFiredGauntlet = true;
		Ray aimRay = GetAimRay();
		if (Object.op_Implicit((Object)(object)muzzleEffectPrefab))
		{
			EffectManager.SimpleMuzzleFlash(muzzleEffectPrefab, base.gameObject, muzzleString, transmit: false);
		}
		if (base.isAuthority)
		{
			BulletAttack bulletAttack = new BulletAttack();
			bulletAttack.owner = base.gameObject;
			bulletAttack.weapon = base.gameObject;
			bulletAttack.origin = ((Ray)(ref aimRay)).origin;
			bulletAttack.aimVector = ((Ray)(ref aimRay)).direction;
			bulletAttack.minSpread = 0f;
			bulletAttack.maxSpread = base.characterBody.spreadBloomAngle;
			bulletAttack.damage = damageCoefficient * damageStat;
			bulletAttack.force = force;
			bulletAttack.tracerEffectPrefab = tracerEffectPrefab;
			bulletAttack.muzzleName = muzzleString;
			bulletAttack.hitEffectPrefab = impactEffectPrefab;
			bulletAttack.isCrit = Util.CheckRoll(critStat, base.characterBody.master);
			bulletAttack.radius = 0.1f;
			bulletAttack.smartCollision = false;
			bulletAttack.Fire();
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
			if (base.inputBank.skill1.down)
			{
				FireLaserbolt fireLaserbolt = new FireLaserbolt();
				fireLaserbolt.gauntlet = ((gauntlet == Gauntlet.Left) ? Gauntlet.Right : Gauntlet.Left);
				outer.SetNextState(fireLaserbolt);
			}
			else
			{
				outer.SetNextStateToMain();
			}
		}
	}

	public override InterruptPriority GetMinimumInterruptPriority()
	{
		return InterruptPriority.Skill;
	}
}
