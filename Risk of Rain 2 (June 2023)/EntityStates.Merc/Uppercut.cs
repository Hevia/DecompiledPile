using KinematicCharacterController;
using RoR2;
using UnityEngine;

namespace EntityStates.Merc;

public class Uppercut : BaseState
{
	public static GameObject swingEffectPrefab;

	public static GameObject hitEffectPrefab;

	public static string enterSoundString;

	public static string attackSoundString;

	public static string hitSoundString;

	public static float slashPitch;

	public static float hitPauseDuration;

	public static float upwardForceStrength;

	public static float baseDuration;

	public static float baseDamageCoefficient;

	public static string slashChildName;

	public static float moveSpeedBonusCoefficient;

	public static string hitboxString;

	public static AnimationCurve yVelocityCurve;

	protected Animator animator;

	protected float duration;

	protected float hitInterval;

	protected bool hasSwung;

	protected float hitPauseTimer;

	protected bool isInHitPause;

	protected OverlapAttack overlapAttack;

	protected HitStopCachedState hitStopCachedState;

	public override void OnEnter()
	{
		//IL_004b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0055: Unknown result type (might be due to invalid IL or missing references)
		//IL_005a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0085: Unknown result type (might be due to invalid IL or missing references)
		base.OnEnter();
		animator = GetModelAnimator();
		duration = baseDuration / attackSpeedStat;
		overlapAttack = InitMeleeOverlap(baseDamageCoefficient, hitEffectPrefab, GetModelTransform(), hitboxString);
		overlapAttack.forceVector = Vector3.up * upwardForceStrength;
		if (Object.op_Implicit((Object)(object)base.characterDirection) && Object.op_Implicit((Object)(object)base.inputBank))
		{
			base.characterDirection.forward = base.inputBank.aimDirection;
		}
		Util.PlaySound(enterSoundString, base.gameObject);
		PlayAnim();
	}

	protected virtual void PlayAnim()
	{
		PlayCrossfade("FullBody, Override", "Uppercut", "Uppercut.playbackRate", duration, 0.1f);
	}

	public override void OnExit()
	{
		base.OnExit();
		PlayAnimation("FullBody, Override", "UppercutExit");
	}

	public override void FixedUpdate()
	{
		//IL_0168: Unknown result type (might be due to invalid IL or missing references)
		//IL_0173: Unknown result type (might be due to invalid IL or missing references)
		//IL_0194: Unknown result type (might be due to invalid IL or missing references)
		//IL_0199: Unknown result type (might be due to invalid IL or missing references)
		//IL_01be: Unknown result type (might be due to invalid IL or missing references)
		//IL_01bf: Unknown result type (might be due to invalid IL or missing references)
		//IL_01de: Unknown result type (might be due to invalid IL or missing references)
		//IL_01e3: Unknown result type (might be due to invalid IL or missing references)
		base.FixedUpdate();
		hitPauseTimer -= Time.fixedDeltaTime;
		if (!base.isAuthority)
		{
			return;
		}
		if (animator.GetFloat("Sword.active") > 0.2f && !hasSwung)
		{
			hasSwung = true;
			((BaseCharacterController)base.characterMotor).Motor.ForceUnground();
			Util.PlayAttackSpeedSound(attackSoundString, base.gameObject, slashPitch);
			EffectManager.SimpleMuzzleFlash(swingEffectPrefab, base.gameObject, slashChildName, transmit: true);
		}
		if (FireMeleeOverlap(overlapAttack, animator, "Sword.active", 0f, calculateForceVector: false))
		{
			Util.PlaySound(hitSoundString, base.gameObject);
			if (!isInHitPause)
			{
				hitStopCachedState = CreateHitStopCachedState(base.characterMotor, animator, "Uppercut.playbackRate");
				hitPauseTimer = hitPauseDuration / attackSpeedStat;
				isInHitPause = true;
			}
		}
		if (hitPauseTimer <= 0f && isInHitPause)
		{
			ConsumeHitStopCachedState(hitStopCachedState, base.characterMotor, animator);
			((BaseCharacterController)base.characterMotor).Motor.ForceUnground();
			isInHitPause = false;
		}
		if (!isInHitPause)
		{
			if (Object.op_Implicit((Object)(object)base.characterMotor) && Object.op_Implicit((Object)(object)base.characterDirection))
			{
				Vector3 velocity = base.characterDirection.forward * moveSpeedStat * Mathf.Lerp(moveSpeedBonusCoefficient, 0f, base.age / duration);
				velocity.y = yVelocityCurve.Evaluate(base.fixedAge / duration);
				base.characterMotor.velocity = velocity;
			}
		}
		else
		{
			base.fixedAge -= Time.fixedDeltaTime;
			base.characterMotor.velocity = Vector3.zero;
			hitPauseTimer -= Time.fixedDeltaTime;
			animator.SetFloat("Uppercut.playbackRate", 0f);
		}
		if (base.fixedAge >= duration)
		{
			if (hasSwung)
			{
				hasSwung = true;
				overlapAttack.Fire();
			}
			outer.SetNextStateToMain();
		}
	}
}
