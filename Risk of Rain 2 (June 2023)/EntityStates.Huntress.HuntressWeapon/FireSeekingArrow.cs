using RoR2;
using RoR2.Orbs;
using UnityEngine;
using UnityEngine.Networking;

namespace EntityStates.Huntress.HuntressWeapon;

public class FireSeekingArrow : BaseState
{
	[SerializeField]
	public float orbDamageCoefficient;

	[SerializeField]
	public float orbProcCoefficient;

	[SerializeField]
	public string muzzleString;

	[SerializeField]
	public GameObject muzzleflashEffectPrefab;

	[SerializeField]
	public string attackSoundString;

	[SerializeField]
	public float baseDuration;

	[SerializeField]
	public int maxArrowCount;

	[SerializeField]
	public float baseArrowReloadDuration;

	private float duration;

	protected float arrowReloadDuration;

	private float arrowReloadTimer;

	protected bool isCrit;

	private int firedArrowCount;

	private HurtBox initialOrbTarget;

	private ChildLocator childLocator;

	private HuntressTracker huntressTracker;

	private Animator animator;

	public override void OnEnter()
	{
		base.OnEnter();
		Transform modelTransform = GetModelTransform();
		huntressTracker = GetComponent<HuntressTracker>();
		if (Object.op_Implicit((Object)(object)modelTransform))
		{
			childLocator = ((Component)modelTransform).GetComponent<ChildLocator>();
			animator = ((Component)modelTransform).GetComponent<Animator>();
		}
		Util.PlayAttackSpeedSound(attackSoundString, base.gameObject, attackSpeedStat);
		if (Object.op_Implicit((Object)(object)huntressTracker) && base.isAuthority)
		{
			initialOrbTarget = huntressTracker.GetTrackingTarget();
		}
		duration = baseDuration / attackSpeedStat;
		arrowReloadDuration = baseArrowReloadDuration / attackSpeedStat;
		if (Object.op_Implicit((Object)(object)base.characterBody))
		{
			base.characterBody.SetAimTimer(duration + 1f);
		}
		PlayCrossfade("Gesture, Override", "FireSeekingShot", "FireSeekingShot.playbackRate", duration, duration * 0.2f / attackSpeedStat);
		PlayCrossfade("Gesture, Additive", "FireSeekingShot", "FireSeekingShot.playbackRate", duration, duration * 0.2f / attackSpeedStat);
		isCrit = Util.CheckRoll(base.characterBody.crit, base.characterBody.master);
	}

	public override void OnExit()
	{
		base.OnExit();
		FireOrbArrow();
	}

	protected virtual GenericDamageOrb CreateArrowOrb()
	{
		return new HuntressArrowOrb();
	}

	private void FireOrbArrow()
	{
		//IL_00cc: Unknown result type (might be due to invalid IL or missing references)
		//IL_00d1: Unknown result type (might be due to invalid IL or missing references)
		if (firedArrowCount < maxArrowCount && !(arrowReloadTimer > 0f) && NetworkServer.active)
		{
			firedArrowCount++;
			arrowReloadTimer = arrowReloadDuration;
			GenericDamageOrb genericDamageOrb = CreateArrowOrb();
			genericDamageOrb.damageValue = base.characterBody.damage * orbDamageCoefficient;
			genericDamageOrb.isCrit = isCrit;
			genericDamageOrb.teamIndex = TeamComponent.GetObjectTeam(base.gameObject);
			genericDamageOrb.attacker = base.gameObject;
			genericDamageOrb.procCoefficient = orbProcCoefficient;
			HurtBox hurtBox = initialOrbTarget;
			if (Object.op_Implicit((Object)(object)hurtBox))
			{
				Transform val = childLocator.FindChild(muzzleString);
				EffectManager.SimpleMuzzleFlash(muzzleflashEffectPrefab, base.gameObject, muzzleString, transmit: true);
				genericDamageOrb.origin = val.position;
				genericDamageOrb.target = hurtBox;
				OrbManager.instance.AddOrb(genericDamageOrb);
			}
		}
	}

	public override void FixedUpdate()
	{
		base.FixedUpdate();
		arrowReloadTimer -= Time.fixedDeltaTime;
		if (animator.GetFloat("FireSeekingShot.fire") > 0f)
		{
			FireOrbArrow();
		}
		if (base.fixedAge > duration && base.isAuthority)
		{
			outer.SetNextStateToMain();
		}
	}

	public override InterruptPriority GetMinimumInterruptPriority()
	{
		return InterruptPriority.Skill;
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
