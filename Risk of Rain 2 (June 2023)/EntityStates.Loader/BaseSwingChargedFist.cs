using System;
using KinematicCharacterController;
using RoR2;
using UnityEngine;

namespace EntityStates.Loader;

public class BaseSwingChargedFist : LoaderMeleeAttack
{
	public float charge;

	[SerializeField]
	public float minLungeSpeed;

	[SerializeField]
	public float maxLungeSpeed;

	[SerializeField]
	public float minPunchForce;

	[SerializeField]
	public float maxPunchForce;

	[SerializeField]
	public float minDuration;

	[SerializeField]
	public float maxDuration;

	public static bool disableAirControlUntilCollision;

	public static float speedCoefficientOnExit;

	public static float velocityDamageCoefficient;

	protected Vector3 punchVelocity;

	private float bonusDamage;

	public float punchSpeed { get; private set; }

	public static event Action<BaseSwingChargedFist> onHitAuthorityGlobal;

	public override void OnEnter()
	{
		//IL_003f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0045: Unknown result type (might be due to invalid IL or missing references)
		//IL_004a: Unknown result type (might be due to invalid IL or missing references)
		//IL_004d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0064: Unknown result type (might be due to invalid IL or missing references)
		//IL_0069: Unknown result type (might be due to invalid IL or missing references)
		//IL_0075: Unknown result type (might be due to invalid IL or missing references)
		//IL_007a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0090: Unknown result type (might be due to invalid IL or missing references)
		base.OnEnter();
		if (base.isAuthority)
		{
			((BaseCharacterController)base.characterMotor).Motor.ForceUnground();
			base.characterMotor.disableAirControlUntilCollision |= disableAirControlUntilCollision;
			Vector3 velocity = base.characterMotor.velocity;
			Ray aimRay = GetAimRay();
			punchVelocity = CalculateLungeVelocity(velocity, aimRay.direction, charge, minLungeSpeed, maxLungeSpeed);
			base.characterMotor.velocity = punchVelocity;
			base.characterDirection.forward = ((Vector3)(ref base.characterMotor.velocity)).normalized;
			punchSpeed = ((Vector3)(ref base.characterMotor.velocity)).magnitude;
			bonusDamage = punchSpeed * (velocityDamageCoefficient * damageStat);
		}
	}

	protected override float CalcDuration()
	{
		return Mathf.Lerp(minDuration, maxDuration, charge);
	}

	protected override void PlayAnimation()
	{
		base.PlayAnimation();
		PlayAnimation("FullBody, Override", "ChargePunch", "ChargePunch.playbackRate", duration);
	}

	protected override void AuthorityFixedUpdate()
	{
		//IL_0015: Unknown result type (might be due to invalid IL or missing references)
		//IL_001a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0026: Unknown result type (might be due to invalid IL or missing references)
		base.AuthorityFixedUpdate();
		if (!base.authorityInHitPause)
		{
			base.characterMotor.velocity = punchVelocity;
			base.characterDirection.forward = punchVelocity;
			base.characterBody.isSprinting = true;
		}
	}

	protected override void AuthorityModifyOverlapAttack(OverlapAttack overlapAttack)
	{
		//IL_0028: Unknown result type (might be due to invalid IL or missing references)
		//IL_002e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0033: Unknown result type (might be due to invalid IL or missing references)
		//IL_0036: Unknown result type (might be due to invalid IL or missing references)
		//IL_0052: Unknown result type (might be due to invalid IL or missing references)
		//IL_0057: Unknown result type (might be due to invalid IL or missing references)
		//IL_005c: Unknown result type (might be due to invalid IL or missing references)
		base.AuthorityModifyOverlapAttack(overlapAttack);
		overlapAttack.damage = damageCoefficient * damageStat + bonusDamage;
		Vector3 velocity = base.characterMotor.velocity;
		Ray aimRay = GetAimRay();
		overlapAttack.forceVector = velocity + aimRay.direction * Mathf.Lerp(minPunchForce, maxPunchForce, charge);
		if (base.fixedAge + Time.fixedDeltaTime >= duration)
		{
			HitBoxGroup hitBoxGroup = FindHitBoxGroup("PunchLollypop");
			if (Object.op_Implicit((Object)(object)hitBoxGroup))
			{
				base.hitBoxGroup = hitBoxGroup;
				overlapAttack.hitBoxGroup = hitBoxGroup;
			}
		}
	}

	protected override void OnMeleeHitAuthority()
	{
		base.OnMeleeHitAuthority();
		BaseSwingChargedFist.onHitAuthorityGlobal?.Invoke(this);
	}

	public override void OnExit()
	{
		//IL_000d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0017: Unknown result type (might be due to invalid IL or missing references)
		//IL_001c: Unknown result type (might be due to invalid IL or missing references)
		base.OnExit();
		CharacterMotor obj = base.characterMotor;
		obj.velocity *= speedCoefficientOnExit;
	}

	public static Vector3 CalculateLungeVelocity(Vector3 currentVelocity, Vector3 aimDirection, float charge, float minLungeSpeed, float maxLungeSpeed)
	{
		//IL_0000: Unknown result type (might be due to invalid IL or missing references)
		//IL_0001: Unknown result type (might be due to invalid IL or missing references)
		//IL_000e: Unknown result type (might be due to invalid IL or missing references)
		//IL_000f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0010: Unknown result type (might be due to invalid IL or missing references)
		//IL_0017: Unknown result type (might be due to invalid IL or missing references)
		//IL_001c: Unknown result type (might be due to invalid IL or missing references)
		//IL_001e: Unknown result type (might be due to invalid IL or missing references)
		//IL_001f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0029: Unknown result type (might be due to invalid IL or missing references)
		//IL_002e: Unknown result type (might be due to invalid IL or missing references)
		currentVelocity = ((Vector3.Dot(currentVelocity, aimDirection) < 0f) ? Vector3.zero : Vector3.Project(currentVelocity, aimDirection));
		return currentVelocity + aimDirection * Mathf.Lerp(minLungeSpeed, maxLungeSpeed, charge);
	}

	public override InterruptPriority GetMinimumInterruptPriority()
	{
		return InterruptPriority.PrioritySkill;
	}
}
