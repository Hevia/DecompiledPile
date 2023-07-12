using RoR2;
using UnityEngine;

namespace EntityStates.VoidRaidCrab.Weapon;

public class ChargeMultiBeam : BaseMultiBeamState
{
	[SerializeField]
	public float baseDuration;

	[SerializeField]
	public GameObject chargeEffectPrefab;

	[SerializeField]
	public GameObject warningLaserVfxPrefab;

	[SerializeField]
	public new string muzzleName;

	[SerializeField]
	public string enterSoundString;

	[SerializeField]
	public bool isSoundScaledByAttackSpeed;

	[SerializeField]
	public string animationLayerName;

	[SerializeField]
	public string animationStateName;

	[SerializeField]
	public string animationPlaybackRateParam;

	private float duration;

	private GameObject chargeEffectInstance;

	private GameObject warningLaserVfxInstance;

	private RayAttackIndicator warningLaserVfxInstanceRayAttackIndicator;

	private bool warningLaserEnabled
	{
		get
		{
			return Object.op_Implicit((Object)(object)warningLaserVfxInstance);
		}
		set
		{
			if (value == warningLaserEnabled)
			{
				return;
			}
			if (value)
			{
				if (Object.op_Implicit((Object)(object)warningLaserVfxPrefab))
				{
					warningLaserVfxInstance = Object.Instantiate<GameObject>(warningLaserVfxPrefab);
					warningLaserVfxInstanceRayAttackIndicator = warningLaserVfxInstance.GetComponent<RayAttackIndicator>();
					UpdateWarningLaser();
				}
			}
			else
			{
				EntityState.Destroy((Object)(object)warningLaserVfxInstance);
				warningLaserVfxInstance = null;
				warningLaserVfxInstanceRayAttackIndicator = null;
			}
		}
	}

	public override void OnEnter()
	{
		//IL_007f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0085: Unknown result type (might be due to invalid IL or missing references)
		base.OnEnter();
		duration = baseDuration / attackSpeedStat;
		PlayAnimation(animationLayerName, animationStateName, animationPlaybackRateParam, duration);
		ChildLocator modelChildLocator = GetModelChildLocator();
		if (Object.op_Implicit((Object)(object)modelChildLocator) && Object.op_Implicit((Object)(object)chargeEffectPrefab))
		{
			Transform val = modelChildLocator.FindChild(muzzleName) ?? base.characterBody.coreTransform;
			if (Object.op_Implicit((Object)(object)val))
			{
				chargeEffectInstance = Object.Instantiate<GameObject>(chargeEffectPrefab, val.position, val.rotation);
				chargeEffectInstance.transform.parent = val;
				ScaleParticleSystemDuration component = chargeEffectInstance.GetComponent<ScaleParticleSystemDuration>();
				if (Object.op_Implicit((Object)(object)component))
				{
					component.newDuration = duration;
				}
			}
		}
		if (!string.IsNullOrEmpty(enterSoundString))
		{
			if (isSoundScaledByAttackSpeed)
			{
				Util.PlayAttackSpeedSound(enterSoundString, base.gameObject, attackSpeedStat);
			}
			else
			{
				Util.PlaySound(enterSoundString, base.gameObject);
			}
		}
		warningLaserEnabled = true;
	}

	public override void OnExit()
	{
		warningLaserEnabled = false;
		EntityState.Destroy((Object)(object)chargeEffectInstance);
		base.OnExit();
	}

	public override void FixedUpdate()
	{
		base.FixedUpdate();
		if (base.isAuthority && base.fixedAge >= duration)
		{
			outer.SetNextState(new FireMultiBeamSmall());
		}
	}

	public override void Update()
	{
		base.Update();
		UpdateWarningLaser();
	}

	public override InterruptPriority GetMinimumInterruptPriority()
	{
		return InterruptPriority.PrioritySkill;
	}

	private void UpdateWarningLaser()
	{
		//IL_002d: Unknown result type (might be due to invalid IL or missing references)
		//IL_002e: Unknown result type (might be due to invalid IL or missing references)
		if (Object.op_Implicit((Object)(object)warningLaserVfxInstanceRayAttackIndicator))
		{
			warningLaserVfxInstanceRayAttackIndicator.attackRange = BaseMultiBeamState.beamMaxDistance;
			CalcBeamPath(out var beamRay, out var _);
			warningLaserVfxInstanceRayAttackIndicator.attackRay = beamRay;
		}
	}
}
