using KinematicCharacterController;
using RoR2;
using RoR2.Projectile;
using UnityEngine;

namespace EntityStates.Croco;

public class BaseLeap : BaseCharacterMain
{
	public static float minimumDuration;

	public static float blastRadius;

	public static float blastProcCoefficient;

	[SerializeField]
	public float blastDamageCoefficient;

	[SerializeField]
	public float blastForce;

	public static string leapSoundString;

	public static GameObject projectilePrefab;

	[SerializeField]
	public Vector3 blastBonusForce;

	[SerializeField]
	public GameObject blastImpactEffectPrefab;

	[SerializeField]
	public GameObject blastEffectPrefab;

	public static float airControl;

	public static float aimVelocity;

	public static float upwardVelocity;

	public static float forwardVelocity;

	public static float minimumY;

	public static float minYVelocityForAnim;

	public static float maxYVelocityForAnim;

	public static float knockbackForce;

	[SerializeField]
	public GameObject fistEffectPrefab;

	public static string soundLoopStartEvent;

	public static string soundLoopStopEvent;

	public static NetworkSoundEventDef landingSound;

	private float previousAirControl;

	private GameObject leftFistEffectInstance;

	private GameObject rightFistEffectInstance;

	protected bool isCritAuthority;

	protected CrocoDamageTypeController crocoDamageTypeController;

	private bool detonateNextFrame;

	protected virtual DamageType GetBlastDamageType()
	{
		return DamageType.Generic;
	}

	public override void OnEnter()
	{
		//IL_0034: Unknown result type (might be due to invalid IL or missing references)
		//IL_0039: Unknown result type (might be due to invalid IL or missing references)
		//IL_003c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0041: Unknown result type (might be due to invalid IL or missing references)
		//IL_005b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0072: Unknown result type (might be due to invalid IL or missing references)
		//IL_007c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0087: Unknown result type (might be due to invalid IL or missing references)
		//IL_008c: Unknown result type (might be due to invalid IL or missing references)
		//IL_008d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0097: Unknown result type (might be due to invalid IL or missing references)
		//IL_009c: Unknown result type (might be due to invalid IL or missing references)
		//IL_009d: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a8: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ae: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b3: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b7: Unknown result type (might be due to invalid IL or missing references)
		//IL_00c1: Unknown result type (might be due to invalid IL or missing references)
		//IL_00c6: Unknown result type (might be due to invalid IL or missing references)
		//IL_00de: Unknown result type (might be due to invalid IL or missing references)
		//IL_00df: Unknown result type (might be due to invalid IL or missing references)
		//IL_00e0: Unknown result type (might be due to invalid IL or missing references)
		//IL_00e5: Unknown result type (might be due to invalid IL or missing references)
		//IL_00e7: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ec: Unknown result type (might be due to invalid IL or missing references)
		//IL_0177: Unknown result type (might be due to invalid IL or missing references)
		//IL_0178: Unknown result type (might be due to invalid IL or missing references)
		base.OnEnter();
		crocoDamageTypeController = GetComponent<CrocoDamageTypeController>();
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
		base.characterBody.bodyFlags |= CharacterBody.BodyFlags.IgnoreFallDamage;
		((Behaviour)((Component)GetModelTransform()).GetComponent<AimAnimator>()).enabled = true;
		PlayCrossfade("Gesture, Override", "Leap", 0.1f);
		PlayCrossfade("Gesture, AdditiveHigh", "Leap", 0.1f);
		PlayCrossfade("Gesture, Override", "Leap", 0.1f);
		Util.PlaySound(leapSoundString, base.gameObject);
		base.characterDirection.moveVector = direction;
		leftFistEffectInstance = Object.Instantiate<GameObject>(fistEffectPrefab, FindModelChild("MuzzleHandL"));
		rightFistEffectInstance = Object.Instantiate<GameObject>(fistEffectPrefab, FindModelChild("MuzzleHandR"));
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
		//IL_0007: Unknown result type (might be due to invalid IL or missing references)
		base.UpdateAnimationParameters();
		float num = Mathf.Clamp01(Util.Remap(base.estimatedVelocity.y, minYVelocityForAnim, maxYVelocityForAnim, 0f, 1f)) * 0.97f;
		base.modelAnimator.SetFloat("LeapCycle", num, 0.1f, Time.deltaTime);
	}

	public override void FixedUpdate()
	{
		//IL_0027: Unknown result type (might be due to invalid IL or missing references)
		base.FixedUpdate();
		if (base.isAuthority && Object.op_Implicit((Object)(object)base.characterMotor))
		{
			base.characterMotor.moveDirection = base.inputBank.moveVector;
			if (base.fixedAge >= minimumDuration && (detonateNextFrame || (((BaseCharacterController)base.characterMotor).Motor.GroundingStatus.IsStableOnGround && !((BaseCharacterController)base.characterMotor).Motor.LastGroundingStatus.IsStableOnGround)))
			{
				DoImpactAuthority();
				outer.SetNextStateToMain();
			}
		}
	}

	protected virtual void DoImpactAuthority()
	{
		//IL_001c: Unknown result type (might be due to invalid IL or missing references)
		if (Object.op_Implicit((Object)(object)landingSound))
		{
			EffectManager.SimpleSoundEffect(landingSound.index, base.characterBody.footPosition, transmit: true);
		}
	}

	protected BlastAttack.Result DetonateAuthority()
	{
		//IL_0006: Unknown result type (might be due to invalid IL or missing references)
		//IL_000b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0018: Unknown result type (might be due to invalid IL or missing references)
		//IL_0061: Unknown result type (might be due to invalid IL or missing references)
		//IL_0066: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a1: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a2: Unknown result type (might be due to invalid IL or missing references)
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

	protected void DropAcidPoolAuthority()
	{
		//IL_0006: Unknown result type (might be due to invalid IL or missing references)
		//IL_000b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0055: Unknown result type (might be due to invalid IL or missing references)
		//IL_005a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0061: Unknown result type (might be due to invalid IL or missing references)
		//IL_0062: Unknown result type (might be due to invalid IL or missing references)
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
		base.characterBody.bodyFlags &= ~CharacterBody.BodyFlags.IgnoreFallDamage;
		base.characterMotor.airControl = previousAirControl;
		base.characterBody.isSprinting = false;
		int layerIndex = base.modelAnimator.GetLayerIndex("Impact");
		if (layerIndex >= 0)
		{
			base.modelAnimator.SetLayerWeight(layerIndex, 2f);
			PlayAnimation("Impact", "LightImpact");
		}
		PlayCrossfade("Gesture, Override", "BufferEmpty", 0.1f);
		PlayCrossfade("Gesture, AdditiveHigh", "BufferEmpty", 0.1f);
		EntityState.Destroy((Object)(object)leftFistEffectInstance);
		EntityState.Destroy((Object)(object)rightFistEffectInstance);
		base.OnExit();
	}

	public override InterruptPriority GetMinimumInterruptPriority()
	{
		return InterruptPriority.PrioritySkill;
	}
}
