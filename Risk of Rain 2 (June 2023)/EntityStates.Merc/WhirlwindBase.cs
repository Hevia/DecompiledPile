using RoR2;
using UnityEngine;

namespace EntityStates.Merc;

public class WhirlwindBase : BaseState
{
	public static GameObject swingEffectPrefab;

	public static GameObject hitEffectPrefab;

	public static string attackSoundString;

	public static string hitSoundString;

	public static float slashPitch;

	public static float hitPauseDuration;

	[SerializeField]
	public float baseDuration;

	[SerializeField]
	public float baseDamageCoefficient;

	[SerializeField]
	public string slashChildName;

	[SerializeField]
	public float selfForceMagnitude;

	[SerializeField]
	public float moveSpeedBonusCoefficient;

	[SerializeField]
	public float smallHopVelocity;

	[SerializeField]
	public string hitboxString;

	protected Animator animator;

	protected float duration;

	protected float hitInterval;

	protected int swingCount;

	protected float hitPauseTimer;

	protected bool isInHitPause;

	protected OverlapAttack overlapAttack;

	protected HitStopCachedState hitStopCachedState;

	public override void OnEnter()
	{
		//IL_006e: Unknown result type (might be due to invalid IL or missing references)
		base.OnEnter();
		animator = GetModelAnimator();
		duration = baseDuration / attackSpeedStat;
		overlapAttack = InitMeleeOverlap(baseDamageCoefficient, hitEffectPrefab, GetModelTransform(), hitboxString);
		if (Object.op_Implicit((Object)(object)base.characterDirection) && Object.op_Implicit((Object)(object)base.inputBank))
		{
			base.characterDirection.forward = base.inputBank.aimDirection;
		}
		SmallHop(base.characterMotor, smallHopVelocity);
		PlayAnim();
	}

	protected virtual void PlayAnim()
	{
	}

	public override void OnExit()
	{
		base.OnExit();
	}

	public override void FixedUpdate()
	{
		//IL_009e: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a3: Unknown result type (might be due to invalid IL or missing references)
		//IL_018a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0195: Unknown result type (might be due to invalid IL or missing references)
		//IL_01b7: Unknown result type (might be due to invalid IL or missing references)
		//IL_01bc: Unknown result type (might be due to invalid IL or missing references)
		//IL_01da: Unknown result type (might be due to invalid IL or missing references)
		//IL_01db: Unknown result type (might be due to invalid IL or missing references)
		//IL_01e8: Unknown result type (might be due to invalid IL or missing references)
		//IL_01ed: Unknown result type (might be due to invalid IL or missing references)
		base.FixedUpdate();
		hitPauseTimer -= Time.fixedDeltaTime;
		if (animator.GetFloat("Sword.active") > (float)swingCount)
		{
			swingCount++;
			Util.PlayAttackSpeedSound(attackSoundString, base.gameObject, slashPitch);
			EffectManager.SimpleMuzzleFlash(swingEffectPrefab, base.gameObject, slashChildName, transmit: false);
			if (base.isAuthority)
			{
				overlapAttack.ResetIgnoredHealthComponents();
				if (Object.op_Implicit((Object)(object)base.characterMotor))
				{
					base.characterMotor.ApplyForce(selfForceMagnitude * base.characterDirection.forward, alwaysApply: true);
				}
			}
		}
		if (!base.isAuthority)
		{
			return;
		}
		if (FireMeleeOverlap(overlapAttack, animator, "Sword.active", 0f))
		{
			Util.PlaySound(hitSoundString, base.gameObject);
			if (!isInHitPause)
			{
				hitStopCachedState = CreateHitStopCachedState(base.characterMotor, animator, "Whirlwind.playbackRate");
				hitPauseTimer = hitPauseDuration / attackSpeedStat;
				isInHitPause = true;
			}
		}
		if (hitPauseTimer <= 0f && isInHitPause)
		{
			ConsumeHitStopCachedState(hitStopCachedState, base.characterMotor, animator);
			isInHitPause = false;
		}
		if (!isInHitPause)
		{
			if (Object.op_Implicit((Object)(object)base.characterMotor) && Object.op_Implicit((Object)(object)base.characterDirection))
			{
				Vector3 velocity = base.characterDirection.forward * moveSpeedStat * Mathf.Lerp(moveSpeedBonusCoefficient, 1f, base.age / duration);
				velocity.y = base.characterMotor.velocity.y;
				base.characterMotor.velocity = velocity;
			}
		}
		else
		{
			base.characterMotor.velocity = Vector3.zero;
			hitPauseTimer -= Time.fixedDeltaTime;
			animator.SetFloat("Whirlwind.playbackRate", 0f);
		}
		if (base.fixedAge >= duration)
		{
			while (swingCount < 2)
			{
				swingCount++;
				overlapAttack.Fire();
			}
			outer.SetNextStateToMain();
		}
	}
}
