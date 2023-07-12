using RoR2;
using RoR2.Projectile;
using UnityEngine;

namespace EntityStates.AncientWispMonster;

public class FireRHCannon : BaseState
{
	public static GameObject projectilePrefab;

	public static GameObject effectPrefab;

	public static float baseDuration = 2f;

	public static float baseDurationBetweenShots = 0.5f;

	public static float damageCoefficient = 1.2f;

	public static float force = 20f;

	public static int bulletCount;

	private float duration;

	private float durationBetweenShots;

	public int bulletCountCurrent = 1;

	public override void OnEnter()
	{
		//IL_0007: Unknown result type (might be due to invalid IL or missing references)
		//IL_000c: Unknown result type (might be due to invalid IL or missing references)
		//IL_00d1: Unknown result type (might be due to invalid IL or missing references)
		//IL_00d6: Unknown result type (might be due to invalid IL or missing references)
		//IL_00d8: Unknown result type (might be due to invalid IL or missing references)
		//IL_00e4: Unknown result type (might be due to invalid IL or missing references)
		//IL_00f8: Unknown result type (might be due to invalid IL or missing references)
		//IL_00fe: Unknown result type (might be due to invalid IL or missing references)
		//IL_0103: Unknown result type (might be due to invalid IL or missing references)
		//IL_0108: Unknown result type (might be due to invalid IL or missing references)
		//IL_0115: Unknown result type (might be due to invalid IL or missing references)
		//IL_011a: Unknown result type (might be due to invalid IL or missing references)
		//IL_011c: Unknown result type (might be due to invalid IL or missing references)
		base.OnEnter();
		Ray aimRay = GetAimRay();
		string text = "MuzzleRight";
		duration = baseDuration / attackSpeedStat;
		durationBetweenShots = baseDurationBetweenShots / attackSpeedStat;
		if (Object.op_Implicit((Object)(object)effectPrefab))
		{
			EffectManager.SimpleMuzzleFlash(effectPrefab, base.gameObject, text, transmit: false);
		}
		PlayAnimation("Gesture", "FireRHCannon", "FireRHCannon.playbackRate", duration);
		if (!base.isAuthority || !Object.op_Implicit((Object)(object)base.modelLocator) || !Object.op_Implicit((Object)(object)base.modelLocator.modelTransform))
		{
			return;
		}
		ChildLocator component = ((Component)base.modelLocator.modelTransform).GetComponent<ChildLocator>();
		if (!Object.op_Implicit((Object)(object)component))
		{
			return;
		}
		Transform val = component.FindChild(text);
		if (Object.op_Implicit((Object)(object)val))
		{
			Vector3 forward = ((Ray)(ref aimRay)).direction;
			RaycastHit val2 = default(RaycastHit);
			if (Physics.Raycast(aimRay, ref val2, (float)LayerMask.op_Implicit(LayerIndex.world.mask)))
			{
				forward = ((RaycastHit)(ref val2)).point - val.position;
			}
			ProjectileManager.instance.FireProjectile(projectilePrefab, val.position, Util.QuaternionSafeLookRotation(forward), base.gameObject, damageStat * damageCoefficient, force, Util.CheckRoll(critStat, base.characterBody.master));
		}
	}

	public override void OnExit()
	{
		base.OnExit();
	}

	public override void FixedUpdate()
	{
		base.FixedUpdate();
		if (base.isAuthority)
		{
			if (bulletCountCurrent == bulletCount && base.fixedAge >= duration)
			{
				outer.SetNextStateToMain();
			}
			else if (bulletCountCurrent < bulletCount && base.fixedAge >= durationBetweenShots)
			{
				FireRHCannon fireRHCannon = new FireRHCannon();
				fireRHCannon.bulletCountCurrent = bulletCountCurrent + 1;
				outer.SetNextState(fireRHCannon);
			}
		}
	}

	public override InterruptPriority GetMinimumInterruptPriority()
	{
		return InterruptPriority.Skill;
	}
}
