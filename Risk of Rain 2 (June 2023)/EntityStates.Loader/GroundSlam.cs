using KinematicCharacterController;
using RoR2;
using UnityEngine;

namespace EntityStates.Loader;

public class GroundSlam : BaseCharacterMain
{
	public static float airControl;

	public static float minimumDuration;

	public static float blastRadius;

	public static float blastProcCoefficient;

	public static float blastDamageCoefficient;

	public static float blastForce;

	public static string enterSoundString;

	public static float initialVerticalVelocity;

	public static float exitVerticalVelocity;

	public static float verticalAcceleration;

	public static float exitSlowdownCoefficient;

	public static Vector3 blastBonusForce;

	public static GameObject blastImpactEffectPrefab;

	public static GameObject blastEffectPrefab;

	public static GameObject fistEffectPrefab;

	private float previousAirControl;

	private GameObject leftFistEffectInstance;

	private GameObject rightFistEffectInstance;

	private bool detonateNextFrame;

	public override void OnEnter()
	{
		base.OnEnter();
		PlayCrossfade("Body", "GroundSlam", 0.2f);
		if (base.isAuthority)
		{
			base.characterMotor.onMovementHit += OnMovementHit;
			base.characterMotor.velocity.y = initialVerticalVelocity;
		}
		Util.PlaySound(enterSoundString, base.gameObject);
		previousAirControl = base.characterMotor.airControl;
		base.characterMotor.airControl = airControl;
		leftFistEffectInstance = Object.Instantiate<GameObject>(fistEffectPrefab, FindModelChild("MechHandR"));
		rightFistEffectInstance = Object.Instantiate<GameObject>(fistEffectPrefab, FindModelChild("MechHandL"));
	}

	public override void FixedUpdate()
	{
		//IL_002d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0043: Unknown result type (might be due to invalid IL or missing references)
		//IL_0048: Unknown result type (might be due to invalid IL or missing references)
		base.FixedUpdate();
		if (base.isAuthority && Object.op_Implicit((Object)(object)base.characterMotor))
		{
			base.characterMotor.moveDirection = base.inputBank.moveVector;
			base.characterDirection.moveVector = base.characterMotor.moveDirection;
			base.characterMotor.velocity.y += verticalAcceleration * Time.fixedDeltaTime;
			if (base.fixedAge >= minimumDuration && (detonateNextFrame || ((BaseCharacterController)base.characterMotor).Motor.GroundingStatus.IsStableOnGround))
			{
				DetonateAuthority();
				outer.SetNextStateToMain();
			}
		}
	}

	public override void OnExit()
	{
		//IL_0036: Unknown result type (might be due to invalid IL or missing references)
		//IL_0040: Unknown result type (might be due to invalid IL or missing references)
		//IL_0045: Unknown result type (might be due to invalid IL or missing references)
		if (base.isAuthority)
		{
			base.characterMotor.onMovementHit -= OnMovementHit;
			((BaseCharacterController)base.characterMotor).Motor.ForceUnground();
			CharacterMotor obj = base.characterMotor;
			obj.velocity *= exitSlowdownCoefficient;
			base.characterMotor.velocity.y = exitVerticalVelocity;
		}
		base.characterMotor.airControl = previousAirControl;
		EntityState.Destroy((Object)(object)leftFistEffectInstance);
		EntityState.Destroy((Object)(object)rightFistEffectInstance);
		base.OnExit();
	}

	private void OnMovementHit(ref CharacterMotor.MovementHitInfo movementHitInfo)
	{
		detonateNextFrame = true;
	}

	protected BlastAttack.Result DetonateAuthority()
	{
		//IL_0006: Unknown result type (might be due to invalid IL or missing references)
		//IL_000b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0017: Unknown result type (might be due to invalid IL or missing references)
		//IL_005d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0062: Unknown result type (might be due to invalid IL or missing references)
		//IL_0099: Unknown result type (might be due to invalid IL or missing references)
		//IL_009a: Unknown result type (might be due to invalid IL or missing references)
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
			crit = RollCrit(),
			damageType = DamageType.Stun1s,
			falloffModel = BlastAttack.FalloffModel.None,
			procCoefficient = blastProcCoefficient,
			radius = blastRadius,
			position = footPosition,
			attackerFiltering = AttackerFiltering.NeverHitSelf,
			impactEffect = EffectCatalog.FindEffectIndexFromPrefab(blastImpactEffectPrefab),
			teamIndex = base.teamComponent.teamIndex
		}.Fire();
	}
}
