using System.Linq;
using RoR2;
using RoR2.Projectile;
using RoR2.Skills;
using UnityEngine;

namespace EntityStates.GrandParentBoss;

public class GroundSwipe : BasicMeleeAttack, SteppedSkillDef.IStepSetter
{
	[SerializeField]
	public GameObject chargeEffectPrefab;

	[SerializeField]
	public GameObject projectilePrefab;

	[SerializeField]
	public float baseChargeStartDelay;

	[SerializeField]
	public float baseFireProjectileDelay;

	[SerializeField]
	public float projectileDamageCoefficient;

	[SerializeField]
	public float projectileForce;

	[SerializeField]
	public float maxBullseyeDistance;

	[SerializeField]
	public float maxBullseyeAngle;

	[SerializeField]
	public float minProjectileDistance;

	[SerializeField]
	public float maxProjectileDistance;

	[SerializeField]
	public float projectileHorizontalSpeed;

	[SerializeField]
	public string chargeSoundName;

	[SerializeField]
	public string hitBoxGroupNameLeft;

	[SerializeField]
	public string animationLayerName = "Body";

	[SerializeField]
	public string muzzleNameLeft = "SecondaryProjectileMuzzle";

	[SerializeField]
	public string muzzleNameRight = "SecondaryProjectileMuzzle";

	[SerializeField]
	public string animationStateNameLeft = "FireSecondaryProjectile";

	[SerializeField]
	public string animationStateNameRight = "FireSecondaryProjectile";

	[SerializeField]
	public string playbackRateParam = "GroundSwipe.playbackRate";

	private GameObject chargeEffectInstance;

	private bool hasFired;

	private float chargeStartDelay;

	private float fireProjectileDelay;

	private int step;

	protected ChildLocator childLocator { get; private set; }

	void SteppedSkillDef.IStepSetter.SetStep(int i)
	{
		step = i;
	}

	public override void OnEnter()
	{
		base.OnEnter();
		hasFired = false;
		chargeStartDelay = baseChargeStartDelay / attackSpeedStat;
		fireProjectileDelay = baseFireProjectileDelay / attackSpeedStat;
		childLocator = GetModelChildLocator();
	}

	public override void OnExit()
	{
		EntityState.Destroy((Object)(object)chargeEffectInstance);
		base.OnExit();
	}

	protected override void PlayAnimation()
	{
		PlayCrossfade(animationLayerName, GetAnimationStateName(), playbackRateParam, duration, 1f);
	}

	public string GetMuzzleName()
	{
		if (step % 2 == 0)
		{
			return muzzleNameLeft;
		}
		return muzzleNameRight;
	}

	public string GetAnimationStateName()
	{
		if (step % 2 == 0)
		{
			return animationStateNameLeft;
		}
		return animationStateNameRight;
	}

	public override string GetHitBoxGroupName()
	{
		if (step % 2 == 0)
		{
			return hitBoxGroupNameLeft;
		}
		return hitBoxGroupName;
	}

	public override void FixedUpdate()
	{
		//IL_0078: Unknown result type (might be due to invalid IL or missing references)
		//IL_007e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0137: Unknown result type (might be due to invalid IL or missing references)
		//IL_013c: Unknown result type (might be due to invalid IL or missing references)
		//IL_013d: Unknown result type (might be due to invalid IL or missing references)
		//IL_013e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0160: Unknown result type (might be due to invalid IL or missing references)
		//IL_0182: Unknown result type (might be due to invalid IL or missing references)
		//IL_0187: Unknown result type (might be due to invalid IL or missing references)
		//IL_0193: Unknown result type (might be due to invalid IL or missing references)
		//IL_0198: Unknown result type (might be due to invalid IL or missing references)
		//IL_01f9: Unknown result type (might be due to invalid IL or missing references)
		//IL_0205: Unknown result type (might be due to invalid IL or missing references)
		//IL_0216: Unknown result type (might be due to invalid IL or missing references)
		//IL_022c: Unknown result type (might be due to invalid IL or missing references)
		//IL_024f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0254: Unknown result type (might be due to invalid IL or missing references)
		//IL_02a6: Unknown result type (might be due to invalid IL or missing references)
		//IL_02ad: Unknown result type (might be due to invalid IL or missing references)
		//IL_02b2: Unknown result type (might be due to invalid IL or missing references)
		//IL_02b7: Unknown result type (might be due to invalid IL or missing references)
		//IL_02bb: Unknown result type (might be due to invalid IL or missing references)
		//IL_02c2: Unknown result type (might be due to invalid IL or missing references)
		//IL_02d7: Unknown result type (might be due to invalid IL or missing references)
		//IL_02db: Unknown result type (might be due to invalid IL or missing references)
		//IL_02e0: Unknown result type (might be due to invalid IL or missing references)
		//IL_0300: Unknown result type (might be due to invalid IL or missing references)
		//IL_0310: Unknown result type (might be due to invalid IL or missing references)
		//IL_0320: Unknown result type (might be due to invalid IL or missing references)
		//IL_0343: Unknown result type (might be due to invalid IL or missing references)
		//IL_035d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0362: Unknown result type (might be due to invalid IL or missing references)
		//IL_0367: Unknown result type (might be due to invalid IL or missing references)
		base.FixedUpdate();
		if (!Object.op_Implicit((Object)(object)chargeEffectInstance) && Object.op_Implicit((Object)(object)childLocator) && base.fixedAge >= chargeStartDelay)
		{
			Transform val = childLocator.FindChild(GetMuzzleName()) ?? base.characterBody.coreTransform;
			if (Object.op_Implicit((Object)(object)val))
			{
				if (Object.op_Implicit((Object)(object)chargeEffectPrefab))
				{
					chargeEffectInstance = Object.Instantiate<GameObject>(chargeEffectPrefab, val.position, val.rotation);
					chargeEffectInstance.transform.parent = val;
					ScaleParticleSystemDuration component = chargeEffectInstance.GetComponent<ScaleParticleSystemDuration>();
					ObjectScaleCurve component2 = chargeEffectInstance.GetComponent<ObjectScaleCurve>();
					if (Object.op_Implicit((Object)(object)component))
					{
						component.newDuration = duration;
					}
					if (Object.op_Implicit((Object)(object)component2))
					{
						component2.timeMax = duration;
					}
				}
				if (chargeSoundName != null)
				{
					Util.PlaySound(chargeSoundName, base.gameObject);
				}
			}
		}
		if (base.isAuthority && !hasFired && Object.op_Implicit((Object)(object)projectilePrefab) && base.fixedAge >= fireProjectileDelay)
		{
			hasFired = true;
			Ray aimRay = GetAimRay();
			Ray val2 = aimRay;
			Transform val3 = childLocator.FindChild(GetMuzzleName());
			if (Object.op_Implicit((Object)(object)val3))
			{
				((Ray)(ref val2)).origin = val3.position;
			}
			BullseyeSearch bullseyeSearch = new BullseyeSearch();
			bullseyeSearch.viewer = base.characterBody;
			bullseyeSearch.searchOrigin = base.characterBody.corePosition;
			bullseyeSearch.searchDirection = base.characterBody.corePosition;
			bullseyeSearch.maxDistanceFilter = maxBullseyeDistance;
			bullseyeSearch.maxAngleFilter = maxBullseyeAngle;
			bullseyeSearch.teamMaskFilter = TeamMask.GetEnemyTeams(GetTeam());
			bullseyeSearch.sortMode = BullseyeSearch.SortMode.DistanceAndAngle;
			bullseyeSearch.RefreshCandidates();
			HurtBox hurtBox = bullseyeSearch.GetResults().FirstOrDefault();
			Vector3? val4 = null;
			RaycastHit val5 = default(RaycastHit);
			if (Object.op_Implicit((Object)(object)hurtBox))
			{
				val4 = ((Component)hurtBox).transform.position;
			}
			else if (Physics.Raycast(aimRay, ref val5, float.PositiveInfinity, LayerMask.op_Implicit(LayerIndex.world.mask), (QueryTriggerInteraction)1))
			{
				val4 = ((RaycastHit)(ref val5)).point;
			}
			FireProjectileInfo fireProjectileInfo = default(FireProjectileInfo);
			fireProjectileInfo.projectilePrefab = projectilePrefab;
			fireProjectileInfo.position = ((Ray)(ref val2)).origin;
			fireProjectileInfo.owner = base.gameObject;
			fireProjectileInfo.damage = damageStat * projectileDamageCoefficient;
			fireProjectileInfo.force = projectileForce;
			fireProjectileInfo.crit = RollCrit();
			FireProjectileInfo fireProjectileInfo2 = fireProjectileInfo;
			if (val4.HasValue)
			{
				Vector3 val6 = val4.Value - ((Ray)(ref val2)).origin;
				Vector2 val7 = default(Vector2);
				((Vector2)(ref val7))._002Ector(val6.x, val6.z);
				float magnitude = ((Vector2)(ref val7)).magnitude;
				Vector2 val8 = val7 / magnitude;
				magnitude = Mathf.Clamp(magnitude, minProjectileDistance, maxProjectileDistance);
				float num = Trajectory.CalculateInitialYSpeed(magnitude / projectileHorizontalSpeed, val6.y);
				Vector3 direction = default(Vector3);
				((Vector3)(ref direction))._002Ector(val8.x * projectileHorizontalSpeed, num, val8.y * projectileHorizontalSpeed);
				fireProjectileInfo2.speedOverride = ((Vector3)(ref direction)).magnitude;
				((Ray)(ref val2)).direction = direction;
			}
			else
			{
				fireProjectileInfo2.speedOverride = projectileHorizontalSpeed;
			}
			fireProjectileInfo2.rotation = Util.QuaternionSafeLookRotation(((Ray)(ref val2)).direction);
			ProjectileManager.instance.FireProjectile(fireProjectileInfo2);
		}
	}
}
