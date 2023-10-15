using RoR2;
using RoR2.Projectile;
using UnityEngine;

namespace EntityStates.Huntress.HuntressWeapon;

public class FireGlaive : BaseState
{
	public static GameObject projectilePrefab;

	public static float baseDuration = 2f;

	public static float damageCoefficient = 1.2f;

	public static float force = 20f;

	private float duration;

	public override void OnEnter()
	{
		//IL_0007: Unknown result type (might be due to invalid IL or missing references)
		//IL_000c: Unknown result type (might be due to invalid IL or missing references)
		//IL_000e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0050: Unknown result type (might be due to invalid IL or missing references)
		//IL_0055: Unknown result type (might be due to invalid IL or missing references)
		//IL_0058: Unknown result type (might be due to invalid IL or missing references)
		//IL_005d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0062: Unknown result type (might be due to invalid IL or missing references)
		//IL_0095: Unknown result type (might be due to invalid IL or missing references)
		//IL_009a: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ad: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ae: Unknown result type (might be due to invalid IL or missing references)
		base.OnEnter();
		Ray aimRay = GetAimRay();
		StartAimMode(aimRay);
		Transform modelTransform = GetModelTransform();
		duration = baseDuration / attackSpeedStat;
		PlayAnimation("Gesture", "FireGlaive", "FireGlaive.playbackRate", duration);
		Vector3 position = aimRay.origin;
		Quaternion rotation = Util.QuaternionSafeLookRotation(aimRay.direction);
		if (Object.op_Implicit((Object)(object)modelTransform))
		{
			ChildLocator component = ((Component)modelTransform).GetComponent<ChildLocator>();
			if (Object.op_Implicit((Object)(object)component))
			{
				Transform val = component.FindChild("RightHand");
				if (Object.op_Implicit((Object)(object)val))
				{
					position = val.position;
				}
			}
		}
		if (base.isAuthority)
		{
			ProjectileManager.instance.FireProjectile(projectilePrefab, position, rotation, base.gameObject, damageStat * damageCoefficient, force, Util.CheckRoll(critStat, base.characterBody.master));
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
		return InterruptPriority.PrioritySkill;
	}
}
