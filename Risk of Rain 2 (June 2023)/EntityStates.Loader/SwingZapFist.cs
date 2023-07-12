using RoR2;
using RoR2.Projectile;
using UnityEngine;

namespace EntityStates.Loader;

public class SwingZapFist : BaseSwingChargedFist
{
	public static float selfKnockback;

	private bool hasHit;

	protected override void OnMeleeHitAuthority()
	{
		//IL_003f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0044: Unknown result type (might be due to invalid IL or missing references)
		//IL_004c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0051: Unknown result type (might be due to invalid IL or missing references)
		//IL_0056: Unknown result type (might be due to invalid IL or missing references)
		if (!hasHit)
		{
			base.OnMeleeHitAuthority();
			hasHit = true;
			if (Object.op_Implicit((Object)(object)FindModelChild(swingEffectMuzzleString)))
			{
				FireProjectileInfo fireProjectileInfo = default(FireProjectileInfo);
				fireProjectileInfo.position = FindModelChild(swingEffectMuzzleString).position;
				fireProjectileInfo.rotation = Quaternion.LookRotation(punchVelocity);
				fireProjectileInfo.crit = base.isCritAuthority;
				fireProjectileInfo.damage = 1f * damageStat;
				fireProjectileInfo.owner = base.gameObject;
				fireProjectileInfo.projectilePrefab = LegacyResourcesAPI.Load<GameObject>("Prefabs/Projectiles/LoaderZapCone");
				ProjectileManager.instance.FireProjectile(fireProjectileInfo);
			}
		}
	}

	protected override void AuthorityExitHitPause()
	{
		base.AuthorityExitHitPause();
		outer.SetNextStateToMain();
	}

	public override void OnExit()
	{
		//IL_0024: Unknown result type (might be due to invalid IL or missing references)
		//IL_0029: Unknown result type (might be due to invalid IL or missing references)
		//IL_002c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0043: Unknown result type (might be due to invalid IL or missing references)
		//IL_0048: Unknown result type (might be due to invalid IL or missing references)
		//IL_0049: Unknown result type (might be due to invalid IL or missing references)
		//IL_0050: Unknown result type (might be due to invalid IL or missing references)
		//IL_0055: Unknown result type (might be due to invalid IL or missing references)
		//IL_0069: Unknown result type (might be due to invalid IL or missing references)
		base.OnExit();
		if (base.isAuthority && hasHit && Object.op_Implicit((Object)(object)base.healthComponent))
		{
			Vector3 val = punchVelocity;
			val.y = Mathf.Min(val.y, 0f);
			val = ((Vector3)(ref val)).normalized;
			val *= 0f - selfKnockback;
			if (Object.op_Implicit((Object)(object)base.characterMotor))
			{
				base.characterMotor.ApplyForce(val, alwaysApply: true);
			}
		}
	}
}
