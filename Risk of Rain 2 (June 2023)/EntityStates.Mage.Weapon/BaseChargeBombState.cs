using RoR2;
using RoR2.UI;
using UnityEngine;

namespace EntityStates.Mage.Weapon;

public abstract class BaseChargeBombState : BaseSkillState
{
	[SerializeField]
	public GameObject chargeEffectPrefab;

	[SerializeField]
	public string chargeSoundString;

	[SerializeField]
	public float baseDuration = 1.5f;

	[SerializeField]
	public float minBloomRadius;

	[SerializeField]
	public float maxBloomRadius;

	[SerializeField]
	public GameObject crosshairOverridePrefab;

	[SerializeField]
	public float minChargeDuration = 0.5f;

	private CrosshairUtils.OverrideRequest crosshairOverrideRequest;

	private uint loopSoundInstanceId;

	protected float duration { get; private set; }

	protected Animator animator { get; private set; }

	protected ChildLocator childLocator { get; private set; }

	protected GameObject chargeEffectInstance { get; private set; }

	public override void OnEnter()
	{
		//IL_007e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0084: Unknown result type (might be due to invalid IL or missing references)
		base.OnEnter();
		duration = baseDuration / attackSpeedStat;
		animator = GetModelAnimator();
		childLocator = GetModelChildLocator();
		if (Object.op_Implicit((Object)(object)childLocator))
		{
			Transform val = childLocator.FindChild("MuzzleBetween") ?? base.characterBody.coreTransform;
			if (Object.op_Implicit((Object)(object)val) && Object.op_Implicit((Object)(object)chargeEffectPrefab))
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
		}
		PlayChargeAnimation();
		loopSoundInstanceId = Util.PlayAttackSpeedSound(chargeSoundString, base.gameObject, attackSpeedStat);
		if (Object.op_Implicit((Object)(object)crosshairOverridePrefab))
		{
			crosshairOverrideRequest = CrosshairUtils.RequestOverrideForBody(base.characterBody, crosshairOverridePrefab, CrosshairUtils.OverridePriority.Skill);
		}
		StartAimMode(duration + 2f);
	}

	public override void OnExit()
	{
		crosshairOverrideRequest?.Dispose();
		AkSoundEngine.StopPlayingID(loopSoundInstanceId);
		if (!outer.destroying)
		{
			PlayAnimation("Gesture, Additive", "Empty");
		}
		EntityState.Destroy((Object)(object)chargeEffectInstance);
		base.OnExit();
	}

	protected float CalcCharge()
	{
		return Mathf.Clamp01(base.fixedAge / duration);
	}

	public override void FixedUpdate()
	{
		base.FixedUpdate();
		float charge = CalcCharge();
		if (base.isAuthority && ((!IsKeyDownAuthority() && base.fixedAge >= minChargeDuration) || base.fixedAge >= duration))
		{
			BaseThrowBombState nextState = GetNextState();
			nextState.charge = charge;
			outer.SetNextState(nextState);
		}
	}

	public override void Update()
	{
		base.Update();
		base.characterBody.SetSpreadBloom(Util.Remap(CalcCharge(), 0f, 1f, minBloomRadius, maxBloomRadius));
	}

	public override InterruptPriority GetMinimumInterruptPriority()
	{
		return InterruptPriority.PrioritySkill;
	}

	protected abstract BaseThrowBombState GetNextState();

	protected virtual void PlayChargeAnimation()
	{
		PlayAnimation("Gesture, Additive", "ChargeNovaBomb", "ChargeNovaBomb.playbackRate", duration);
	}
}
