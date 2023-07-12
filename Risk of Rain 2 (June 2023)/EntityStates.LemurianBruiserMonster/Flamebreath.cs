using RoR2;
using UnityEngine;

namespace EntityStates.LemurianBruiserMonster;

public class Flamebreath : BaseState
{
	public static GameObject flamethrowerEffectPrefab;

	public static GameObject impactEffectPrefab;

	public static GameObject tracerEffectPrefab;

	public static float maxDistance;

	public static float radius;

	public static float baseEntryDuration = 1f;

	public static float baseExitDuration = 0.5f;

	public static float baseFlamethrowerDuration = 2f;

	public static float totalDamageCoefficient = 1.2f;

	public static float procCoefficientPerTick;

	public static float tickFrequency;

	public static float force = 20f;

	public static string startAttackSoundString;

	public static string endAttackSoundString;

	public static float ignitePercentChance;

	public static float maxSpread;

	private float tickDamageCoefficient;

	private float flamethrowerStopwatch;

	private float stopwatch;

	private float entryDuration;

	private float exitDuration;

	private float flamethrowerDuration;

	private bool hasBegunFlamethrower;

	private ChildLocator childLocator;

	private Transform flamethrowerEffectInstance;

	private Transform muzzleTransform;

	private bool isCrit;

	private const float flamethrowerEffectBaseDistance = 16f;

	public override void OnEnter()
	{
		base.OnEnter();
		stopwatch = 0f;
		entryDuration = baseEntryDuration / attackSpeedStat;
		exitDuration = baseExitDuration / attackSpeedStat;
		flamethrowerDuration = baseFlamethrowerDuration;
		Transform modelTransform = GetModelTransform();
		if (Object.op_Implicit((Object)(object)base.characterBody))
		{
			base.characterBody.SetAimTimer(entryDuration + flamethrowerDuration + 1f);
		}
		if (Object.op_Implicit((Object)(object)modelTransform))
		{
			childLocator = ((Component)modelTransform).GetComponent<ChildLocator>();
			((Behaviour)((Component)modelTransform).GetComponent<AimAnimator>()).enabled = true;
		}
		float num = flamethrowerDuration * tickFrequency;
		tickDamageCoefficient = totalDamageCoefficient / num;
		if (base.isAuthority && Object.op_Implicit((Object)(object)base.characterBody))
		{
			isCrit = Util.CheckRoll(critStat, base.characterBody.master);
		}
		PlayAnimation("Gesture, Override", "PrepFlamebreath", "PrepFlamebreath.playbackRate", entryDuration);
	}

	public override void OnExit()
	{
		Util.PlaySound(endAttackSoundString, base.gameObject);
		PlayCrossfade("Gesture, Override", "BufferEmpty", 0.1f);
		if (Object.op_Implicit((Object)(object)flamethrowerEffectInstance))
		{
			EntityState.Destroy((Object)(object)((Component)flamethrowerEffectInstance).gameObject);
		}
		base.OnExit();
	}

	private void FireFlame(string muzzleString)
	{
		//IL_0001: Unknown result type (might be due to invalid IL or missing references)
		//IL_0046: Unknown result type (might be due to invalid IL or missing references)
		//IL_004b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0057: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ce: Unknown result type (might be due to invalid IL or missing references)
		//IL_00d3: Unknown result type (might be due to invalid IL or missing references)
		GetAimRay();
		if (base.isAuthority && Object.op_Implicit((Object)(object)muzzleTransform))
		{
			BulletAttack bulletAttack = new BulletAttack();
			bulletAttack.owner = base.gameObject;
			bulletAttack.weapon = base.gameObject;
			bulletAttack.origin = muzzleTransform.position;
			bulletAttack.aimVector = muzzleTransform.forward;
			bulletAttack.minSpread = 0f;
			bulletAttack.maxSpread = maxSpread;
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
		}
	}

	public override void FixedUpdate()
	{
		//IL_00c8: Unknown result type (might be due to invalid IL or missing references)
		base.FixedUpdate();
		stopwatch += Time.fixedDeltaTime;
		if (stopwatch >= entryDuration && stopwatch < entryDuration + flamethrowerDuration && !hasBegunFlamethrower)
		{
			hasBegunFlamethrower = true;
			Util.PlaySound(startAttackSoundString, base.gameObject);
			PlayAnimation("Gesture, Override", "Flamebreath", "Flamebreath.playbackRate", flamethrowerDuration);
			if (Object.op_Implicit((Object)(object)childLocator))
			{
				muzzleTransform = childLocator.FindChild("MuzzleMouth");
				flamethrowerEffectInstance = Object.Instantiate<GameObject>(flamethrowerEffectPrefab, muzzleTransform).transform;
				((Component)flamethrowerEffectInstance).transform.localPosition = Vector3.zero;
				((Component)flamethrowerEffectInstance).GetComponent<ScaleParticleSystemDuration>().newDuration = flamethrowerDuration;
			}
		}
		if (stopwatch >= entryDuration + flamethrowerDuration && hasBegunFlamethrower)
		{
			hasBegunFlamethrower = false;
			PlayCrossfade("Gesture, Override", "ExitFlamebreath", "ExitFlamebreath.playbackRate", exitDuration, 0.1f);
		}
		if (hasBegunFlamethrower)
		{
			flamethrowerStopwatch += Time.deltaTime;
			if (flamethrowerStopwatch > 1f / tickFrequency)
			{
				flamethrowerStopwatch -= 1f / tickFrequency;
				FireFlame("MuzzleCenter");
			}
		}
		else if (Object.op_Implicit((Object)(object)flamethrowerEffectInstance))
		{
			EntityState.Destroy((Object)(object)((Component)flamethrowerEffectInstance).gameObject);
		}
		if (stopwatch >= flamethrowerDuration + entryDuration + exitDuration && base.isAuthority)
		{
			outer.SetNextStateToMain();
		}
	}

	public override InterruptPriority GetMinimumInterruptPriority()
	{
		return InterruptPriority.Skill;
	}
}
