using System.Collections.Generic;
using RoR2;
using RoR2.Audio;
using UnityEngine;

namespace EntityStates;

public class BasicMeleeAttack : BaseState
{
	[SerializeField]
	public float baseDuration;

	[SerializeField]
	public float damageCoefficient;

	[SerializeField]
	public string hitBoxGroupName;

	[SerializeField]
	public GameObject hitEffectPrefab;

	[SerializeField]
	public float procCoefficient;

	[SerializeField]
	public float pushAwayForce;

	[SerializeField]
	public Vector3 forceVector;

	[SerializeField]
	public float hitPauseDuration;

	[SerializeField]
	public GameObject swingEffectPrefab;

	[SerializeField]
	public string swingEffectMuzzleString;

	[SerializeField]
	public string mecanimHitboxActiveParameter;

	[SerializeField]
	public float shorthopVelocityFromHit;

	[SerializeField]
	public string beginStateSoundString;

	[SerializeField]
	public string beginSwingSoundString;

	[SerializeField]
	public NetworkSoundEventDef impactSound;

	[SerializeField]
	public bool forceForwardVelocity;

	[SerializeField]
	public AnimationCurve forwardVelocityCurve;

	[SerializeField]
	public bool scaleHitPauseDurationAndVelocityWithAttackSpeed;

	[SerializeField]
	public bool ignoreAttackSpeed;

	protected float duration;

	protected HitBoxGroup hitBoxGroup;

	protected Animator animator;

	private OverlapAttack overlapAttack;

	protected bool authorityHitThisFixedUpdate;

	protected float hitPauseTimer;

	protected Vector3 storedHitPauseVelocity;

	private Run.FixedTimeStamp meleeAttackStartTime = Run.FixedTimeStamp.positiveInfinity;

	private GameObject swingEffectInstance;

	private int meleeAttackTicks;

	protected List<HurtBox> hitResults = new List<HurtBox>();

	private bool forceFire;

	protected bool authorityInHitPause => hitPauseTimer > 0f;

	private bool meleeAttackHasBegun => meleeAttackStartTime.hasPassed;

	protected bool authorityHasFiredAtAll => meleeAttackTicks > 0;

	protected bool isCritAuthority { get; private set; }

	protected virtual bool allowExitFire => true;

	public virtual string GetHitBoxGroupName()
	{
		return hitBoxGroupName;
	}

	public override void OnEnter()
	{
		//IL_00c4: Unknown result type (might be due to invalid IL or missing references)
		//IL_00c9: Unknown result type (might be due to invalid IL or missing references)
		base.OnEnter();
		duration = CalcDuration();
		if (duration <= Time.fixedDeltaTime * 2f)
		{
			forceFire = true;
		}
		StartAimMode();
		Util.PlaySound(beginStateSoundString, base.gameObject);
		animator = GetModelAnimator();
		if (base.isAuthority)
		{
			isCritAuthority = RollCrit();
			hitBoxGroup = FindHitBoxGroup(GetHitBoxGroupName());
			if (Object.op_Implicit((Object)(object)hitBoxGroup))
			{
				overlapAttack = new OverlapAttack
				{
					attacker = base.gameObject,
					damage = damageCoefficient * damageStat,
					damageColorIndex = DamageColorIndex.Default,
					damageType = DamageType.Generic,
					forceVector = forceVector,
					hitBoxGroup = hitBoxGroup,
					hitEffectPrefab = hitEffectPrefab,
					impactSound = (impactSound?.index ?? NetworkSoundEventIndex.Invalid),
					inflictor = base.gameObject,
					isCrit = isCritAuthority,
					procChainMask = default(ProcChainMask),
					pushAwayForce = pushAwayForce,
					procCoefficient = procCoefficient,
					teamIndex = GetTeam()
				};
			}
		}
		PlayAnimation();
	}

	protected virtual float CalcDuration()
	{
		if (ignoreAttackSpeed)
		{
			return baseDuration;
		}
		return baseDuration / attackSpeedStat;
	}

	protected virtual void AuthorityModifyOverlapAttack(OverlapAttack overlapAttack)
	{
	}

	public override void FixedUpdate()
	{
		base.FixedUpdate();
		if (string.IsNullOrEmpty(mecanimHitboxActiveParameter))
		{
			BeginMeleeAttackEffect();
		}
		else if (animator.GetFloat(mecanimHitboxActiveParameter) > 0.5f)
		{
			BeginMeleeAttackEffect();
		}
		if (base.isAuthority)
		{
			AuthorityFixedUpdate();
		}
	}

	protected void AuthorityTriggerHitPause()
	{
		//IL_000f: Unknown result type (might be due to invalid IL or missing references)
		//IL_001a: Unknown result type (might be due to invalid IL or missing references)
		//IL_001f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0024: Unknown result type (might be due to invalid IL or missing references)
		//IL_002f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0034: Unknown result type (might be due to invalid IL or missing references)
		if (Object.op_Implicit((Object)(object)base.characterMotor))
		{
			storedHitPauseVelocity += base.characterMotor.velocity;
			base.characterMotor.velocity = Vector3.zero;
		}
		if (Object.op_Implicit((Object)(object)animator))
		{
			animator.speed = 0f;
		}
		if (Object.op_Implicit((Object)(object)swingEffectInstance))
		{
			ScaleParticleSystemDuration component = swingEffectInstance.GetComponent<ScaleParticleSystemDuration>();
			if (Object.op_Implicit((Object)(object)component))
			{
				component.newDuration = 20f;
			}
		}
		hitPauseTimer = (scaleHitPauseDurationAndVelocityWithAttackSpeed ? (hitPauseDuration / attackSpeedStat) : hitPauseDuration);
	}

	protected virtual void BeginMeleeAttackEffect()
	{
		if (meleeAttackStartTime != Run.FixedTimeStamp.positiveInfinity)
		{
			return;
		}
		meleeAttackStartTime = Run.FixedTimeStamp.now;
		Util.PlaySound(beginSwingSoundString, base.gameObject);
		if (!Object.op_Implicit((Object)(object)swingEffectPrefab))
		{
			return;
		}
		Transform val = FindModelChild(swingEffectMuzzleString);
		if (Object.op_Implicit((Object)(object)val))
		{
			swingEffectInstance = Object.Instantiate<GameObject>(swingEffectPrefab, val);
			ScaleParticleSystemDuration component = swingEffectInstance.GetComponent<ScaleParticleSystemDuration>();
			if (Object.op_Implicit((Object)(object)component))
			{
				component.newDuration = component.initialDuration;
			}
		}
	}

	protected virtual void AuthorityExitHitPause()
	{
		//IL_005c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0061: Unknown result type (might be due to invalid IL or missing references)
		//IL_0067: Unknown result type (might be due to invalid IL or missing references)
		//IL_006c: Unknown result type (might be due to invalid IL or missing references)
		hitPauseTimer = 0f;
		storedHitPauseVelocity.y = Mathf.Max(storedHitPauseVelocity.y, scaleHitPauseDurationAndVelocityWithAttackSpeed ? (shorthopVelocityFromHit / Mathf.Sqrt(attackSpeedStat)) : shorthopVelocityFromHit);
		if (Object.op_Implicit((Object)(object)base.characterMotor))
		{
			base.characterMotor.velocity = storedHitPauseVelocity;
		}
		storedHitPauseVelocity = Vector3.zero;
		if (Object.op_Implicit((Object)(object)animator))
		{
			animator.speed = 1f;
		}
		if (Object.op_Implicit((Object)(object)swingEffectInstance))
		{
			ScaleParticleSystemDuration component = swingEffectInstance.GetComponent<ScaleParticleSystemDuration>();
			if (Object.op_Implicit((Object)(object)component))
			{
				component.newDuration = component.initialDuration;
			}
		}
	}

	protected virtual void PlayAnimation()
	{
	}

	protected virtual void OnMeleeHitAuthority()
	{
	}

	private void AuthorityFireAttack()
	{
		AuthorityModifyOverlapAttack(overlapAttack);
		hitResults.Clear();
		authorityHitThisFixedUpdate = overlapAttack.Fire(hitResults);
		meleeAttackTicks++;
		if (authorityHitThisFixedUpdate)
		{
			AuthorityTriggerHitPause();
			OnMeleeHitAuthority();
		}
	}

	protected virtual void AuthorityFixedUpdate()
	{
		//IL_002d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0032: Unknown result type (might be due to invalid IL or missing references)
		//IL_0084: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a1: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a6: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ad: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b9: Unknown result type (might be due to invalid IL or missing references)
		//IL_00c4: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ca: Unknown result type (might be due to invalid IL or missing references)
		if (authorityInHitPause)
		{
			hitPauseTimer -= Time.fixedDeltaTime;
			if (Object.op_Implicit((Object)(object)base.characterMotor))
			{
				base.characterMotor.velocity = Vector3.zero;
			}
			base.fixedAge -= Time.fixedDeltaTime;
			if (!authorityInHitPause)
			{
				AuthorityExitHitPause();
			}
		}
		else if (forceForwardVelocity && Object.op_Implicit((Object)(object)base.characterMotor) && Object.op_Implicit((Object)(object)base.characterDirection))
		{
			Vector3 val = base.characterDirection.forward * forwardVelocityCurve.Evaluate(base.fixedAge / duration);
			_ = base.characterMotor.velocity;
			base.characterMotor.AddDisplacement(new Vector3(val.x, 0f, val.z));
		}
		authorityHitThisFixedUpdate = false;
		if (overlapAttack != null && (string.IsNullOrEmpty(mecanimHitboxActiveParameter) || animator.GetFloat(mecanimHitboxActiveParameter) > 0.5f || forceFire))
		{
			AuthorityFireAttack();
		}
		if (duration <= base.fixedAge)
		{
			AuthorityOnFinish();
		}
	}

	public override void OnExit()
	{
		if (base.isAuthority)
		{
			if (!outer.destroying && !authorityHasFiredAtAll && allowExitFire)
			{
				BeginMeleeAttackEffect();
				AuthorityFireAttack();
			}
			if (authorityInHitPause)
			{
				AuthorityExitHitPause();
			}
		}
		if (Object.op_Implicit((Object)(object)swingEffectInstance))
		{
			EntityState.Destroy((Object)(object)swingEffectInstance);
		}
		if (Object.op_Implicit((Object)(object)animator))
		{
			animator.speed = 1f;
		}
		base.OnExit();
	}

	protected virtual void AuthorityOnFinish()
	{
		outer.SetNextStateToMain();
	}
}
