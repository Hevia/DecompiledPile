using System.Collections.Generic;
using System.Linq;
using RoR2;
using RoR2.Orbs;
using RoR2.Projectile;
using UnityEngine;

namespace EntityStates.Loader;

public class BigPunch : LoaderMeleeAttack
{
	public static int maxShockCount;

	public static float maxShockFOV;

	public static float maxShockDistance;

	public static float shockDamageCoefficient;

	public static float shockProcCoefficient;

	public static float knockbackForce;

	public static float shorthopVelocityOnEnter;

	private bool hasHit;

	private bool hasKnockbackedSelf;

	private Vector3 punchVector
	{
		get
		{
			//IL_0006: Unknown result type (might be due to invalid IL or missing references)
			//IL_000b: Unknown result type (might be due to invalid IL or missing references)
			//IL_000e: Unknown result type (might be due to invalid IL or missing references)
			Vector3 forward = base.characterDirection.forward;
			return ((Vector3)(ref forward)).normalized;
		}
	}

	public override void OnEnter()
	{
		base.OnEnter();
		base.characterMotor.velocity.y = shorthopVelocityOnEnter;
	}

	protected override void PlayAnimation()
	{
		base.PlayAnimation();
		PlayAnimation("FullBody, Override", "BigPunch", "BigPunch.playbackRate", duration);
	}

	protected override void AuthorityFixedUpdate()
	{
		//IL_002c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0037: Unknown result type (might be due to invalid IL or missing references)
		base.AuthorityFixedUpdate();
		if (hasHit && !hasKnockbackedSelf && !base.authorityInHitPause)
		{
			hasKnockbackedSelf = true;
			base.healthComponent.TakeDamageForce(punchVector * (0f - knockbackForce), alwaysApply: true);
		}
	}

	protected override void AuthorityModifyOverlapAttack(OverlapAttack overlapAttack)
	{
		base.AuthorityModifyOverlapAttack(overlapAttack);
		overlapAttack.maximumOverlapTargets = 1;
	}

	protected override void OnMeleeHitAuthority()
	{
		//IL_0034: Unknown result type (might be due to invalid IL or missing references)
		//IL_0039: Unknown result type (might be due to invalid IL or missing references)
		//IL_003c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0041: Unknown result type (might be due to invalid IL or missing references)
		//IL_0049: Unknown result type (might be due to invalid IL or missing references)
		//IL_004e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0053: Unknown result type (might be due to invalid IL or missing references)
		if (!hasHit)
		{
			base.OnMeleeHitAuthority();
			hasHit = true;
			if (Object.op_Implicit((Object)(object)FindModelChild(swingEffectMuzzleString)))
			{
				FireProjectileInfo fireProjectileInfo = default(FireProjectileInfo);
				Ray aimRay = GetAimRay();
				fireProjectileInfo.position = aimRay.origin;
				fireProjectileInfo.rotation = Quaternion.LookRotation(punchVector);
				fireProjectileInfo.crit = RollCrit();
				fireProjectileInfo.damage = 1f * damageStat;
				fireProjectileInfo.owner = base.gameObject;
				fireProjectileInfo.projectilePrefab = LegacyResourcesAPI.Load<GameObject>("Prefabs/Projectiles/LoaderZapCone");
				ProjectileManager.instance.FireProjectile(fireProjectileInfo);
			}
		}
	}

	private void FireSecondaryRaysServer()
	{
		//IL_0001: Unknown result type (might be due to invalid IL or missing references)
		//IL_0006: Unknown result type (might be due to invalid IL or missing references)
		//IL_003e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0043: Unknown result type (might be due to invalid IL or missing references)
		//IL_004a: Unknown result type (might be due to invalid IL or missing references)
		//IL_004f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0105: Unknown result type (might be due to invalid IL or missing references)
		//IL_010a: Unknown result type (might be due to invalid IL or missing references)
		Ray aimRay = GetAimRay();
		TeamIndex team = GetTeam();
		BullseyeSearch bullseyeSearch = new BullseyeSearch();
		bullseyeSearch.teamMaskFilter = TeamMask.GetEnemyTeams(team);
		bullseyeSearch.maxAngleFilter = maxShockFOV * 0.5f;
		bullseyeSearch.maxDistanceFilter = maxShockDistance;
		bullseyeSearch.searchOrigin = aimRay.origin;
		bullseyeSearch.searchDirection = punchVector;
		bullseyeSearch.sortMode = BullseyeSearch.SortMode.Distance;
		bullseyeSearch.filterByLoS = false;
		bullseyeSearch.RefreshCandidates();
		List<HurtBox> list = bullseyeSearch.GetResults().Where(Util.IsValid).ToList();
		Transform val = FindModelChild(swingEffectMuzzleString);
		if (!Object.op_Implicit((Object)(object)val))
		{
			return;
		}
		for (int i = 0; i < Mathf.Min(list.Count, maxShockCount); i++)
		{
			HurtBox hurtBox = list[i];
			if (Object.op_Implicit((Object)(object)hurtBox))
			{
				LightningOrb lightningOrb = new LightningOrb();
				lightningOrb.bouncedObjects = new List<HealthComponent>();
				lightningOrb.attacker = base.gameObject;
				lightningOrb.teamIndex = team;
				lightningOrb.damageValue = damageStat * shockDamageCoefficient;
				lightningOrb.isCrit = RollCrit();
				lightningOrb.origin = val.position;
				lightningOrb.bouncesRemaining = 0;
				lightningOrb.lightningType = LightningOrb.LightningType.Loader;
				lightningOrb.procCoefficient = shockProcCoefficient;
				lightningOrb.target = hurtBox;
				OrbManager.instance.AddOrb(lightningOrb);
			}
		}
	}
}
