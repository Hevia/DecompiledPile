using System.Collections.Generic;
using RoR2;
using RoR2.Orbs;
using UnityEngine;
using UnityEngine.Networking;

namespace EntityStates.Huntress.HuntressWeapon;

public class ThrowGlaive : BaseState
{
	public static float baseDuration = 3f;

	public static GameObject chargePrefab;

	public static GameObject muzzleFlashPrefab;

	public static float smallHopStrength;

	public static float antigravityStrength;

	public static float damageCoefficient = 1.2f;

	public static float damageCoefficientPerBounce = 1.1f;

	public static float glaiveProcCoefficient;

	public static int maxBounceCount;

	public static float glaiveTravelSpeed;

	public static float glaiveBounceRange;

	public static string attackSoundString;

	private float duration;

	private float stopwatch;

	private Animator animator;

	private GameObject chargeEffect;

	private Transform modelTransform;

	private HuntressTracker huntressTracker;

	private ChildLocator childLocator;

	private bool hasTriedToThrowGlaive;

	private bool hasSuccessfullyThrownGlaive;

	private HurtBox initialOrbTarget;

	public override void OnEnter()
	{
		//IL_0124: Unknown result type (might be due to invalid IL or missing references)
		//IL_012a: Unknown result type (might be due to invalid IL or missing references)
		base.OnEnter();
		stopwatch = 0f;
		duration = baseDuration / attackSpeedStat;
		modelTransform = GetModelTransform();
		animator = GetModelAnimator();
		huntressTracker = GetComponent<HuntressTracker>();
		Util.PlayAttackSpeedSound(attackSoundString, base.gameObject, attackSpeedStat);
		if (Object.op_Implicit((Object)(object)huntressTracker) && base.isAuthority)
		{
			initialOrbTarget = huntressTracker.GetTrackingTarget();
		}
		if (Object.op_Implicit((Object)(object)base.characterMotor) && smallHopStrength != 0f)
		{
			base.characterMotor.velocity.y = smallHopStrength;
		}
		PlayAnimation("FullBody, Override", "ThrowGlaive", "ThrowGlaive.playbackRate", duration);
		if (Object.op_Implicit((Object)(object)modelTransform))
		{
			childLocator = ((Component)modelTransform).GetComponent<ChildLocator>();
			if (Object.op_Implicit((Object)(object)childLocator))
			{
				Transform val = childLocator.FindChild("HandR");
				if (Object.op_Implicit((Object)(object)val) && Object.op_Implicit((Object)(object)chargePrefab))
				{
					chargeEffect = Object.Instantiate<GameObject>(chargePrefab, val.position, val.rotation);
					chargeEffect.transform.parent = val;
				}
			}
		}
		if (Object.op_Implicit((Object)(object)base.characterBody))
		{
			base.characterBody.SetAimTimer(duration);
		}
	}

	public override void OnExit()
	{
		base.OnExit();
		if (Object.op_Implicit((Object)(object)chargeEffect))
		{
			EntityState.Destroy((Object)(object)chargeEffect);
		}
		int layerIndex = animator.GetLayerIndex("Impact");
		if (layerIndex >= 0)
		{
			animator.SetLayerWeight(layerIndex, 1.5f);
			animator.PlayInFixedTime("LightImpact", layerIndex, 0f);
		}
		if (!hasTriedToThrowGlaive)
		{
			FireOrbGlaive();
		}
		if (!hasSuccessfullyThrownGlaive && NetworkServer.active)
		{
			base.skillLocator.secondary.AddOneStock();
		}
	}

	public override void FixedUpdate()
	{
		base.FixedUpdate();
		stopwatch += Time.fixedDeltaTime;
		if (!hasTriedToThrowGlaive && animator.GetFloat("ThrowGlaive.fire") > 0f)
		{
			if (Object.op_Implicit((Object)(object)chargeEffect))
			{
				EntityState.Destroy((Object)(object)chargeEffect);
			}
			FireOrbGlaive();
		}
		base.characterMotor.velocity.y += antigravityStrength * Time.fixedDeltaTime * (1f - stopwatch / duration);
		if (stopwatch >= duration && base.isAuthority)
		{
			outer.SetNextStateToMain();
		}
	}

	private void FireOrbGlaive()
	{
		//IL_00fa: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ff: Unknown result type (might be due to invalid IL or missing references)
		if (NetworkServer.active && !hasTriedToThrowGlaive)
		{
			hasTriedToThrowGlaive = true;
			LightningOrb lightningOrb = new LightningOrb();
			lightningOrb.lightningType = LightningOrb.LightningType.HuntressGlaive;
			lightningOrb.damageValue = base.characterBody.damage * damageCoefficient;
			lightningOrb.isCrit = Util.CheckRoll(base.characterBody.crit, base.characterBody.master);
			lightningOrb.teamIndex = TeamComponent.GetObjectTeam(base.gameObject);
			lightningOrb.attacker = base.gameObject;
			lightningOrb.procCoefficient = glaiveProcCoefficient;
			lightningOrb.bouncesRemaining = maxBounceCount;
			lightningOrb.speed = glaiveTravelSpeed;
			lightningOrb.bouncedObjects = new List<HealthComponent>();
			lightningOrb.range = glaiveBounceRange;
			lightningOrb.damageCoefficientPerBounce = damageCoefficientPerBounce;
			HurtBox hurtBox = initialOrbTarget;
			if (Object.op_Implicit((Object)(object)hurtBox))
			{
				hasSuccessfullyThrownGlaive = true;
				Transform val = childLocator.FindChild("HandR");
				EffectManager.SimpleMuzzleFlash(muzzleFlashPrefab, base.gameObject, "HandR", transmit: true);
				lightningOrb.origin = val.position;
				lightningOrb.target = hurtBox;
				OrbManager.instance.AddOrb(lightningOrb);
			}
		}
	}

	public override InterruptPriority GetMinimumInterruptPriority()
	{
		return InterruptPriority.PrioritySkill;
	}

	public override void OnSerialize(NetworkWriter writer)
	{
		writer.Write(HurtBoxReference.FromHurtBox(initialOrbTarget));
	}

	public override void OnDeserialize(NetworkReader reader)
	{
		initialOrbTarget = reader.ReadHurtBoxReference().ResolveHurtBox();
	}
}
