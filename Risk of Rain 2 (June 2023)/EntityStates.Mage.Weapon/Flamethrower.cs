using RoR2;
using UnityEngine;

namespace EntityStates.Mage.Weapon;

public class Flamethrower : BaseState
{
	[SerializeField]
	public GameObject flamethrowerEffectPrefab;

	public static GameObject impactEffectPrefab;

	public static GameObject tracerEffectPrefab;

	[SerializeField]
	public float maxDistance;

	public static float radius;

	public static float baseEntryDuration = 1f;

	public static float baseFlamethrowerDuration = 2f;

	public static float totalDamageCoefficient = 1.2f;

	public static float procCoefficientPerTick;

	public static float tickFrequency;

	public static float force = 20f;

	public static string startAttackSoundString;

	public static string endAttackSoundString;

	public static float ignitePercentChance;

	public static float recoilForce;

	private float tickDamageCoefficient;

	private float flamethrowerStopwatch;

	private float stopwatch;

	private float entryDuration;

	private float flamethrowerDuration;

	private bool hasBegunFlamethrower;

	private ChildLocator childLocator;

	private Transform leftFlamethrowerTransform;

	private Transform rightFlamethrowerTransform;

	private Transform leftMuzzleTransform;

	private Transform rightMuzzleTransform;

	private bool isCrit;

	private const float flamethrowerEffectBaseDistance = 16f;

	public override void OnEnter()
	{
		base.OnEnter();
		stopwatch = 0f;
		entryDuration = baseEntryDuration / attackSpeedStat;
		flamethrowerDuration = baseFlamethrowerDuration;
		Transform modelTransform = GetModelTransform();
		if (Object.op_Implicit((Object)(object)base.characterBody))
		{
			base.characterBody.SetAimTimer(entryDuration + flamethrowerDuration + 1f);
		}
		if (Object.op_Implicit((Object)(object)modelTransform))
		{
			childLocator = ((Component)modelTransform).GetComponent<ChildLocator>();
			leftMuzzleTransform = childLocator.FindChild("MuzzleLeft");
			rightMuzzleTransform = childLocator.FindChild("MuzzleRight");
		}
		int num = Mathf.CeilToInt(flamethrowerDuration * tickFrequency);
		tickDamageCoefficient = totalDamageCoefficient / (float)num;
		if (base.isAuthority && Object.op_Implicit((Object)(object)base.characterBody))
		{
			isCrit = Util.CheckRoll(critStat, base.characterBody.master);
		}
		PlayAnimation("Gesture, Additive", "PrepFlamethrower", "Flamethrower.playbackRate", entryDuration);
	}

	public override void OnExit()
	{
		Util.PlaySound(endAttackSoundString, base.gameObject);
		PlayCrossfade("Gesture, Additive", "ExitFlamethrower", 0.1f);
		if (Object.op_Implicit((Object)(object)leftFlamethrowerTransform))
		{
			EntityState.Destroy((Object)(object)((Component)leftFlamethrowerTransform).gameObject);
		}
		if (Object.op_Implicit((Object)(object)rightFlamethrowerTransform))
		{
			EntityState.Destroy((Object)(object)((Component)rightFlamethrowerTransform).gameObject);
		}
		base.OnExit();
	}

	private void FireGauntlet(string muzzleString)
	{
		//IL_0001: Unknown result type (might be due to invalid IL or missing references)
		//IL_0006: Unknown result type (might be due to invalid IL or missing references)
		//IL_0032: Unknown result type (might be due to invalid IL or missing references)
		//IL_0037: Unknown result type (might be due to invalid IL or missing references)
		//IL_003f: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ab: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b0: Unknown result type (might be due to invalid IL or missing references)
		//IL_0112: Unknown result type (might be due to invalid IL or missing references)
		//IL_011d: Unknown result type (might be due to invalid IL or missing references)
		Ray aimRay = GetAimRay();
		if (base.isAuthority)
		{
			BulletAttack bulletAttack = new BulletAttack();
			bulletAttack.owner = base.gameObject;
			bulletAttack.weapon = base.gameObject;
			bulletAttack.origin = ((Ray)(ref aimRay)).origin;
			bulletAttack.aimVector = ((Ray)(ref aimRay)).direction;
			bulletAttack.minSpread = 0f;
			bulletAttack.damage = tickDamageCoefficient * damageStat;
			bulletAttack.force = force;
			bulletAttack.muzzleName = muzzleString;
			bulletAttack.hitEffectPrefab = impactEffectPrefab;
			bulletAttack.isCrit = isCrit;
			bulletAttack.radius = radius;
			bulletAttack.falloffModel = BulletAttack.FalloffModel.None;
			bulletAttack.stopperMask = LayerIndex.world.mask;
			bulletAttack.procCoefficient = procCoefficientPerTick;
			bulletAttack.maxDistance = maxDistance;
			bulletAttack.smartCollision = true;
			bulletAttack.damageType = (Util.CheckRoll(ignitePercentChance, base.characterBody.master) ? DamageType.IgniteOnHit : DamageType.Generic);
			bulletAttack.Fire();
			if (Object.op_Implicit((Object)(object)base.characterMotor))
			{
				base.characterMotor.ApplyForce(((Ray)(ref aimRay)).direction * (0f - recoilForce));
			}
		}
	}

	public override void FixedUpdate()
	{
		base.FixedUpdate();
		stopwatch += Time.fixedDeltaTime;
		if (stopwatch >= entryDuration && !hasBegunFlamethrower)
		{
			hasBegunFlamethrower = true;
			Util.PlaySound(startAttackSoundString, base.gameObject);
			PlayAnimation("Gesture, Additive", "Flamethrower", "Flamethrower.playbackRate", flamethrowerDuration);
			if (Object.op_Implicit((Object)(object)childLocator))
			{
				Transform val = childLocator.FindChild("MuzzleLeft");
				Transform val2 = childLocator.FindChild("MuzzleRight");
				if (Object.op_Implicit((Object)(object)val))
				{
					leftFlamethrowerTransform = Object.Instantiate<GameObject>(flamethrowerEffectPrefab, val).transform;
				}
				if (Object.op_Implicit((Object)(object)val2))
				{
					rightFlamethrowerTransform = Object.Instantiate<GameObject>(flamethrowerEffectPrefab, val2).transform;
				}
				if (Object.op_Implicit((Object)(object)leftFlamethrowerTransform))
				{
					((Component)leftFlamethrowerTransform).GetComponent<ScaleParticleSystemDuration>().newDuration = flamethrowerDuration;
				}
				if (Object.op_Implicit((Object)(object)rightFlamethrowerTransform))
				{
					((Component)rightFlamethrowerTransform).GetComponent<ScaleParticleSystemDuration>().newDuration = flamethrowerDuration;
				}
			}
			FireGauntlet("MuzzleCenter");
		}
		if (hasBegunFlamethrower)
		{
			flamethrowerStopwatch += Time.deltaTime;
			float num = 1f / tickFrequency / attackSpeedStat;
			if (flamethrowerStopwatch > num)
			{
				flamethrowerStopwatch -= num;
				FireGauntlet("MuzzleCenter");
			}
			UpdateFlamethrowerEffect();
		}
		if (stopwatch >= flamethrowerDuration + entryDuration && base.isAuthority)
		{
			outer.SetNextStateToMain();
		}
	}

	private void UpdateFlamethrowerEffect()
	{
		//IL_0001: Unknown result type (might be due to invalid IL or missing references)
		//IL_0006: Unknown result type (might be due to invalid IL or missing references)
		//IL_0009: Unknown result type (might be due to invalid IL or missing references)
		//IL_000e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0011: Unknown result type (might be due to invalid IL or missing references)
		//IL_0016: Unknown result type (might be due to invalid IL or missing references)
		//IL_002a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0043: Unknown result type (might be due to invalid IL or missing references)
		Ray aimRay = GetAimRay();
		Vector3 direction = ((Ray)(ref aimRay)).direction;
		Vector3 direction2 = ((Ray)(ref aimRay)).direction;
		if (Object.op_Implicit((Object)(object)leftFlamethrowerTransform))
		{
			leftFlamethrowerTransform.forward = direction;
		}
		if (Object.op_Implicit((Object)(object)rightFlamethrowerTransform))
		{
			rightFlamethrowerTransform.forward = direction2;
		}
	}

	public override InterruptPriority GetMinimumInterruptPriority()
	{
		return InterruptPriority.Skill;
	}
}
