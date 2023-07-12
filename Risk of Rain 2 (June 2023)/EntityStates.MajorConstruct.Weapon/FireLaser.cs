using System;
using EntityStates.EngiTurret.EngiTurretWeapon;
using RoR2;
using RoR2.Audio;
using UnityEngine;

namespace EntityStates.MajorConstruct.Weapon;

public class FireLaser : FireBeam
{
	[SerializeField]
	public float duration;

	[SerializeField]
	public float aimMaxSpeed;

	[SerializeField]
	public string animationLayerName;

	[SerializeField]
	public string animationStateName;

	[SerializeField]
	public string animationPlaybackParameterName;

	[SerializeField]
	public LoopSoundDef loopSoundDef;

	private LoopSoundManager.SoundLoopPtr loopPtr;

	private AimAnimator.DirectionOverrideRequest animatorDirectionOverrideRequest;

	private Vector3 aimDirection;

	private Vector3 aimVelocity;

	public override void OnEnter()
	{
		//IL_0071: Unknown result type (might be due to invalid IL or missing references)
		//IL_0076: Unknown result type (might be due to invalid IL or missing references)
		base.OnEnter();
		PlayAnimation(animationLayerName, animationStateName, animationPlaybackParameterName, duration);
		if (Object.op_Implicit((Object)(object)loopSoundDef))
		{
			loopPtr = LoopSoundManager.PlaySoundLoopLocal(base.gameObject, loopSoundDef);
		}
		AimAnimator component = GetComponent<AimAnimator>();
		if (Object.op_Implicit((Object)(object)component))
		{
			animatorDirectionOverrideRequest = component.RequestDirectionOverride(GetAimDirection);
		}
		aimDirection = GetTargetDirection();
	}

	public override void OnExit()
	{
		animatorDirectionOverrideRequest?.Dispose();
		LoopSoundManager.StopSoundLoopLocal(loopPtr);
		base.OnExit();
	}

	public override void FixedUpdate()
	{
		//IL_0002: Unknown result type (might be due to invalid IL or missing references)
		//IL_0008: Unknown result type (might be due to invalid IL or missing references)
		//IL_0024: Unknown result type (might be due to invalid IL or missing references)
		//IL_0029: Unknown result type (might be due to invalid IL or missing references)
		aimDirection = Vector3.RotateTowards(aimDirection, GetTargetDirection(), aimMaxSpeed * (MathF.PI / 180f) * Time.fixedDeltaTime, float.PositiveInfinity);
		base.FixedUpdate();
	}

	public override void ModifyBullet(BulletAttack bulletAttack)
	{
	}

	protected override EntityState GetNextState()
	{
		//IL_0001: Unknown result type (might be due to invalid IL or missing references)
		return new TerminateLaser(GetBeamEndPoint());
	}

	public override bool ShouldFireLaser()
	{
		return duration > base.fixedAge;
	}

	public override InterruptPriority GetMinimumInterruptPriority()
	{
		return InterruptPriority.Pain;
	}

	private Vector3 GetAimDirection()
	{
		//IL_0001: Unknown result type (might be due to invalid IL or missing references)
		return aimDirection;
	}

	private Vector3 GetTargetDirection()
	{
		//IL_0013: Unknown result type (might be due to invalid IL or missing references)
		//IL_001f: Unknown result type (might be due to invalid IL or missing references)
		if (Object.op_Implicit((Object)(object)base.inputBank))
		{
			return base.inputBank.aimDirection;
		}
		return base.transform.forward;
	}

	public override Ray GetLaserRay()
	{
		//IL_0013: Unknown result type (might be due to invalid IL or missing references)
		//IL_0019: Unknown result type (might be due to invalid IL or missing references)
		//IL_001e: Unknown result type (might be due to invalid IL or missing references)
		//IL_002a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0030: Unknown result type (might be due to invalid IL or missing references)
		//IL_0035: Unknown result type (might be due to invalid IL or missing references)
		if (Object.op_Implicit((Object)(object)base.inputBank))
		{
			return new Ray(base.inputBank.aimOrigin, aimDirection);
		}
		return new Ray(base.transform.position, aimDirection);
	}
}
