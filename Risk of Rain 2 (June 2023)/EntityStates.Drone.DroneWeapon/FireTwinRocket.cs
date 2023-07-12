using RoR2;
using RoR2.Projectile;
using UnityEngine;

namespace EntityStates.Drone.DroneWeapon;

public class FireTwinRocket : BaseState
{
	public static GameObject projectilePrefab;

	public static GameObject muzzleEffectPrefab;

	public static float damageCoefficient;

	public static float force;

	public static float baseDuration = 2f;

	private ChildLocator childLocator;

	private float stopwatch;

	private float duration;

	public override void OnEnter()
	{
		//IL_0024: Unknown result type (might be due to invalid IL or missing references)
		base.OnEnter();
		stopwatch = 0f;
		duration = baseDuration / attackSpeedStat;
		GetAimRay();
		Transform modelTransform = GetModelTransform();
		if (Object.op_Implicit((Object)(object)modelTransform))
		{
			childLocator = ((Component)modelTransform).GetComponent<ChildLocator>();
		}
		FireProjectile("GatLeft");
		FireProjectile("GatRight");
	}

	public override void OnExit()
	{
		base.OnExit();
	}

	private void FireProjectile(string muzzleString)
	{
		//IL_0001: Unknown result type (might be due to invalid IL or missing references)
		//IL_0054: Unknown result type (might be due to invalid IL or missing references)
		//IL_0059: Unknown result type (might be due to invalid IL or missing references)
		//IL_005c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0061: Unknown result type (might be due to invalid IL or missing references)
		//IL_0064: Unknown result type (might be due to invalid IL or missing references)
		//IL_0069: Unknown result type (might be due to invalid IL or missing references)
		//IL_0074: Unknown result type (might be due to invalid IL or missing references)
		//IL_0079: Unknown result type (might be due to invalid IL or missing references)
		//IL_007b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0088: Unknown result type (might be due to invalid IL or missing references)
		//IL_009b: Unknown result type (might be due to invalid IL or missing references)
		//IL_00af: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b5: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ba: Unknown result type (might be due to invalid IL or missing references)
		//IL_00bf: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ca: Unknown result type (might be due to invalid IL or missing references)
		//IL_00cc: Unknown result type (might be due to invalid IL or missing references)
		//IL_00cd: Unknown result type (might be due to invalid IL or missing references)
		GetAimRay();
		Transform val = childLocator.FindChild(muzzleString);
		if (Object.op_Implicit((Object)(object)muzzleEffectPrefab))
		{
			EffectManager.SimpleMuzzleFlash(muzzleEffectPrefab, base.gameObject, muzzleString, transmit: false);
		}
		if (!base.isAuthority || !((Object)(object)projectilePrefab != (Object)null))
		{
			return;
		}
		float num = 1000f;
		Ray aimRay = GetAimRay();
		Vector3 forward = ((Ray)(ref aimRay)).direction;
		Vector3 position = ((Ray)(ref aimRay)).origin;
		if (Object.op_Implicit((Object)(object)val))
		{
			position = val.position;
			RaycastHit val2 = default(RaycastHit);
			if (Physics.Raycast(aimRay, ref val2, num, LayerMask.op_Implicit(LayerIndex.world.mask) | LayerMask.op_Implicit(LayerIndex.entityPrecise.mask)))
			{
				forward = ((RaycastHit)(ref val2)).point - val.position;
			}
		}
		ProjectileManager.instance.FireProjectile(projectilePrefab, position, Util.QuaternionSafeLookRotation(forward), base.gameObject, damageStat * damageCoefficient, force, Util.CheckRoll(critStat, base.characterBody.master));
	}

	public override void FixedUpdate()
	{
		base.FixedUpdate();
		stopwatch += Time.fixedDeltaTime;
		if (stopwatch >= duration / attackSpeedStat && base.isAuthority)
		{
			outer.SetNextStateToMain();
		}
	}

	public override InterruptPriority GetMinimumInterruptPriority()
	{
		return InterruptPriority.Skill;
	}
}
