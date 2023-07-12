using System.Linq;
using KinematicCharacterController;
using RoR2;
using RoR2.Projectile;
using UnityEngine;

namespace EntityStates.AcidLarva;

public class LarvaLeap : BaseCharacterMain
{
	[SerializeField]
	public float minimumDuration;

	[SerializeField]
	public float blastRadius;

	[SerializeField]
	public float blastProcCoefficient;

	[SerializeField]
	public float blastDamageCoefficient;

	[SerializeField]
	public float blastForce;

	[SerializeField]
	public string leapSoundString;

	[SerializeField]
	public GameObject projectilePrefab;

	[SerializeField]
	public Vector3 blastBonusForce;

	[SerializeField]
	public GameObject blastImpactEffectPrefab;

	[SerializeField]
	public GameObject blastEffectPrefab;

	[SerializeField]
	public float airControl;

	[SerializeField]
	public float aimVelocity;

	[SerializeField]
	public float upwardVelocity;

	[SerializeField]
	public float forwardVelocity;

	[SerializeField]
	public float minimumY;

	[SerializeField]
	public float minYVelocityForAnim;

	[SerializeField]
	public float maxYVelocityForAnim;

	[SerializeField]
	public float knockbackForce;

	[SerializeField]
	public float maxRadiusToConfirmDetonate;

	[SerializeField]
	public bool confirmDetonate;

	[SerializeField]
	public GameObject spinEffectPrefab;

	[SerializeField]
	public string spinEffectMuzzleString;

	[SerializeField]
	public string soundLoopStartEvent;

	[SerializeField]
	public string soundLoopStopEvent;

	[SerializeField]
	public NetworkSoundEventDef landingSound;

	[SerializeField]
	public float detonateSelfDamageFraction;

	private float previousAirControl;

	private GameObject spinEffectInstance;

	protected bool isCritAuthority;

	protected CrocoDamageTypeController crocoDamageTypeController;

	private bool detonateNextFrame;

	protected virtual DamageType GetBlastDamageType()
	{
		return DamageType.Generic;
	}

	public override void OnEnter()
	{
		//IL_0029: Unknown result type (might be due to invalid IL or missing references)
		//IL_002e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0031: Unknown result type (might be due to invalid IL or missing references)
		//IL_0036: Unknown result type (might be due to invalid IL or missing references)
		//IL_0050: Unknown result type (might be due to invalid IL or missing references)
		//IL_0068: Unknown result type (might be due to invalid IL or missing references)
		//IL_0073: Unknown result type (might be due to invalid IL or missing references)
		//IL_007e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0083: Unknown result type (might be due to invalid IL or missing references)
		//IL_0084: Unknown result type (might be due to invalid IL or missing references)
		//IL_008f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0094: Unknown result type (might be due to invalid IL or missing references)
		//IL_0095: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a0: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a6: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ab: Unknown result type (might be due to invalid IL or missing references)
		//IL_00af: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ba: Unknown result type (might be due to invalid IL or missing references)
		//IL_00bf: Unknown result type (might be due to invalid IL or missing references)
		//IL_00d7: Unknown result type (might be due to invalid IL or missing references)
		//IL_00d8: Unknown result type (might be due to invalid IL or missing references)
		//IL_00d9: Unknown result type (might be due to invalid IL or missing references)
		//IL_00de: Unknown result type (might be due to invalid IL or missing references)
		//IL_00e0: Unknown result type (might be due to invalid IL or missing references)
		//IL_00e5: Unknown result type (might be due to invalid IL or missing references)
		//IL_0123: Unknown result type (might be due to invalid IL or missing references)
		//IL_0124: Unknown result type (might be due to invalid IL or missing references)
		base.OnEnter();
		previousAirControl = base.characterMotor.airControl;
		base.characterMotor.airControl = airControl;
		Ray aimRay = GetAimRay();
		Vector3 direction = ((Ray)(ref aimRay)).direction;
		if (base.isAuthority)
		{
			base.characterBody.isSprinting = true;
			direction.y = Mathf.Max(direction.y, minimumY);
			Vector3 val = ((Vector3)(ref direction)).normalized * aimVelocity * moveSpeedStat;
			Vector3 val2 = Vector3.up * upwardVelocity;
			Vector3 val3 = new Vector3(direction.x, 0f, direction.z);
			Vector3 val4 = ((Vector3)(ref val3)).normalized * forwardVelocity;
			((BaseCharacterController)base.characterMotor).Motor.ForceUnground();
			base.characterMotor.velocity = val + val2 + val4;
			isCritAuthority = RollCrit();
		}
		PlayCrossfade("Gesture, Override", "LarvaLeap", 0.1f);
		Util.PlaySound(leapSoundString, base.gameObject);
		base.characterDirection.moveVector = direction;
		spinEffectInstance = Object.Instantiate<GameObject>(spinEffectPrefab, FindModelChild(spinEffectMuzzleString));
		if (base.isAuthority)
		{
			base.characterMotor.onMovementHit += OnMovementHit;
		}
		Util.PlaySound(soundLoopStartEvent, base.gameObject);
	}

	private void OnMovementHit(ref CharacterMotor.MovementHitInfo movementHitInfo)
	{
		detonateNextFrame = true;
	}

	protected override void UpdateAnimationParameters()
	{
		base.UpdateAnimationParameters();
	}

	public override void FixedUpdate()
	{
		//IL_002d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0043: Unknown result type (might be due to invalid IL or missing references)
		//IL_0048: Unknown result type (might be due to invalid IL or missing references)
		//IL_0125: Unknown result type (might be due to invalid IL or missing references)
		//IL_012a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0136: Unknown result type (might be due to invalid IL or missing references)
		//IL_013b: Unknown result type (might be due to invalid IL or missing references)
		base.FixedUpdate();
		if (!base.isAuthority || !Object.op_Implicit((Object)(object)base.characterMotor))
		{
			return;
		}
		base.characterMotor.moveDirection = base.inputBank.moveVector;
		base.characterDirection.moveVector = base.characterMotor.velocity;
		base.characterMotor.disableAirControlUntilCollision = base.characterMotor.velocity.y < 0f;
		if (base.fixedAge >= minimumDuration && (detonateNextFrame || (((BaseCharacterController)base.characterMotor).Motor.GroundingStatus.IsStableOnGround && !((BaseCharacterController)base.characterMotor).Motor.LastGroundingStatus.IsStableOnGround)))
		{
			bool flag = true;
			if (confirmDetonate)
			{
				BullseyeSearch bullseyeSearch = new BullseyeSearch();
				bullseyeSearch.viewer = base.characterBody;
				bullseyeSearch.teamMaskFilter = TeamMask.allButNeutral;
				bullseyeSearch.teamMaskFilter.RemoveTeam(base.characterBody.teamComponent.teamIndex);
				bullseyeSearch.sortMode = BullseyeSearch.SortMode.Distance;
				bullseyeSearch.minDistanceFilter = 0f;
				bullseyeSearch.maxDistanceFilter = maxRadiusToConfirmDetonate;
				bullseyeSearch.searchOrigin = base.inputBank.aimOrigin;
				bullseyeSearch.searchDirection = base.inputBank.aimDirection;
				bullseyeSearch.maxAngleFilter = 180f;
				bullseyeSearch.filterByLoS = false;
				bullseyeSearch.RefreshCandidates();
				flag = Object.op_Implicit((Object)(object)bullseyeSearch.GetResults().FirstOrDefault());
			}
			if (flag)
			{
				DoImpactAuthority();
			}
			outer.SetNextStateToMain();
		}
	}

	protected virtual void DoImpactAuthority()
	{
		//IL_0025: Unknown result type (might be due to invalid IL or missing references)
		//IL_006b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0070: Unknown result type (might be due to invalid IL or missing references)
		DetonateAuthority();
		if (Object.op_Implicit((Object)(object)landingSound))
		{
			EffectManager.SimpleSoundEffect(landingSound.index, base.characterBody.footPosition, transmit: true);
		}
		base.healthComponent.TakeDamage(new DamageInfo
		{
			damage = base.healthComponent.fullCombinedHealth * detonateSelfDamageFraction,
			attacker = ((Component)base.characterBody).gameObject,
			position = base.characterBody.corePosition,
			damageType = DamageType.Generic
		});
	}

	protected BlastAttack.Result DetonateAuthority()
	{
		//IL_0006: Unknown result type (might be due to invalid IL or missing references)
		//IL_000b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0018: Unknown result type (might be due to invalid IL or missing references)
		//IL_0062: Unknown result type (might be due to invalid IL or missing references)
		//IL_0067: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a4: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a5: Unknown result type (might be due to invalid IL or missing references)
		Vector3 footPosition = base.characterBody.footPosition;
		EffectManager.SpawnEffect(blastEffectPrefab, new EffectData
		{
			origin = footPosition,
			scale = blastRadius
		}, transmit: true);
		return new BlastAttack
		{
			attacker = base.gameObject,
			baseDamage = damageStat * blastDamageCoefficient,
			baseForce = blastForce,
			bonusForce = blastBonusForce,
			crit = isCritAuthority,
			damageType = GetBlastDamageType(),
			falloffModel = BlastAttack.FalloffModel.None,
			procCoefficient = blastProcCoefficient,
			radius = blastRadius,
			position = footPosition,
			attackerFiltering = AttackerFiltering.NeverHitSelf,
			impactEffect = EffectCatalog.FindEffectIndexFromPrefab(blastImpactEffectPrefab),
			teamIndex = base.teamComponent.teamIndex
		}.Fire();
	}

	protected void FireProjectile()
	{
		//IL_0006: Unknown result type (might be due to invalid IL or missing references)
		//IL_000b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0056: Unknown result type (might be due to invalid IL or missing references)
		//IL_005b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0062: Unknown result type (might be due to invalid IL or missing references)
		//IL_0063: Unknown result type (might be due to invalid IL or missing references)
		Vector3 footPosition = base.characterBody.footPosition;
		FireProjectileInfo fireProjectileInfo = default(FireProjectileInfo);
		fireProjectileInfo.projectilePrefab = projectilePrefab;
		fireProjectileInfo.crit = isCritAuthority;
		fireProjectileInfo.force = 0f;
		fireProjectileInfo.damage = damageStat;
		fireProjectileInfo.owner = base.gameObject;
		fireProjectileInfo.rotation = Quaternion.identity;
		fireProjectileInfo.position = footPosition;
		FireProjectileInfo fireProjectileInfo2 = fireProjectileInfo;
		ProjectileManager.instance.FireProjectile(fireProjectileInfo2);
	}

	public override void OnExit()
	{
		Util.PlaySound(soundLoopStopEvent, base.gameObject);
		if (base.isAuthority)
		{
			base.characterMotor.onMovementHit -= OnMovementHit;
		}
		base.characterMotor.airControl = previousAirControl;
		base.characterBody.isSprinting = false;
		if (Object.op_Implicit((Object)(object)spinEffectInstance))
		{
			EntityState.Destroy((Object)(object)spinEffectInstance);
		}
		PlayAnimation("Gesture, Override", "Empty");
		base.OnExit();
	}

	public override InterruptPriority GetMinimumInterruptPriority()
	{
		return InterruptPriority.PrioritySkill;
	}
}
