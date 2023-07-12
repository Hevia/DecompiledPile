using System.Collections.Generic;
using RoR2;
using UnityEngine;

namespace EntityStates.VoidRaidCrab.Weapon;

public class ChargeGravityBump : BaseGravityBumpState
{
	[SerializeField]
	public float baseDuration;

	[SerializeField]
	public GameObject chargeEffectPrefab;

	[SerializeField]
	public string muzzleName;

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

	[SerializeField]
	public float horizontalAirborneForce;

	[SerializeField]
	public float verticalAirborneForce;

	[SerializeField]
	public float horizontalGroundedForce;

	[SerializeField]
	public float verticalGroundedForce;

	[SerializeField]
	public GameObject forceIndicatorPrefab;

	private float duration;

	private GameObject chargeEffectInstance;

	private Dictionary<CharacterMotor, Transform> characterMotorsToIndicatorTransforms;

	private Quaternion airborneForceOrientation;

	private Quaternion groundedForceOrientation;

	public override void OnEnter()
	{
		//IL_0033: Unknown result type (might be due to invalid IL or missing references)
		//IL_0038: Unknown result type (might be due to invalid IL or missing references)
		//IL_003d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0042: Unknown result type (might be due to invalid IL or missing references)
		//IL_004b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0051: Unknown result type (might be due to invalid IL or missing references)
		//IL_0056: Unknown result type (might be due to invalid IL or missing references)
		//IL_0058: Unknown result type (might be due to invalid IL or missing references)
		//IL_0062: Unknown result type (might be due to invalid IL or missing references)
		//IL_006d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0072: Unknown result type (might be due to invalid IL or missing references)
		//IL_0079: Unknown result type (might be due to invalid IL or missing references)
		//IL_007e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0083: Unknown result type (might be due to invalid IL or missing references)
		//IL_0089: Unknown result type (might be due to invalid IL or missing references)
		//IL_0094: Unknown result type (might be due to invalid IL or missing references)
		//IL_0099: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a0: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a5: Unknown result type (might be due to invalid IL or missing references)
		//IL_00aa: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b1: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b6: Unknown result type (might be due to invalid IL or missing references)
		//IL_00bb: Unknown result type (might be due to invalid IL or missing references)
		//IL_00c2: Unknown result type (might be due to invalid IL or missing references)
		//IL_00c7: Unknown result type (might be due to invalid IL or missing references)
		//IL_00cc: Unknown result type (might be due to invalid IL or missing references)
		//IL_014d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0153: Unknown result type (might be due to invalid IL or missing references)
		base.OnEnter();
		if (base.isAuthority)
		{
			isLeft = Random.value > 0.5f;
			CharacterDirection component = GetComponent<CharacterDirection>();
			if (Object.op_Implicit((Object)(object)component))
			{
				Vector3 val = Vector3.Cross(component.forward, Vector3.up);
				if (!isLeft)
				{
					val *= -1f;
				}
				airborneForce = Vector3.up * -1f * verticalAirborneForce + val * horizontalAirborneForce;
				groundedForce = Vector3.up * verticalGroundedForce + val * horizontalGroundedForce;
			}
		}
		airborneForceOrientation = Util.QuaternionSafeLookRotation(airborneForce);
		groundedForceOrientation = Util.QuaternionSafeLookRotation(groundedForce);
		duration = baseDuration / attackSpeedStat;
		PlayAnimation(animationLayerName, animationStateName, animationPlaybackRateParam, duration);
		ChildLocator modelChildLocator = GetModelChildLocator();
		if (Object.op_Implicit((Object)(object)modelChildLocator) && Object.op_Implicit((Object)(object)chargeEffectPrefab))
		{
			Transform val2 = modelChildLocator.FindChild(muzzleName) ?? base.characterBody.coreTransform;
			if (Object.op_Implicit((Object)(object)val2))
			{
				chargeEffectInstance = Object.Instantiate<GameObject>(chargeEffectPrefab, val2.position, val2.rotation);
				chargeEffectInstance.transform.parent = val2;
				ScaleParticleSystemDuration component2 = chargeEffectInstance.GetComponent<ScaleParticleSystemDuration>();
				if (Object.op_Implicit((Object)(object)component2))
				{
					component2.newDuration = duration;
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
		characterMotorsToIndicatorTransforms = new Dictionary<CharacterMotor, Transform>();
		AssignIndicators();
	}

	public override void OnExit()
	{
		CleanUpIndicators();
		EntityState.Destroy((Object)(object)chargeEffectInstance);
		base.OnExit();
	}

	public override void FixedUpdate()
	{
		//IL_0032: Unknown result type (might be due to invalid IL or missing references)
		//IL_0046: Unknown result type (might be due to invalid IL or missing references)
		base.FixedUpdate();
		foreach (KeyValuePair<CharacterMotor, Transform> characterMotorsToIndicatorTransform in characterMotorsToIndicatorTransforms)
		{
			if (characterMotorsToIndicatorTransform.Key.isGrounded)
			{
				characterMotorsToIndicatorTransform.Value.rotation = groundedForceOrientation;
			}
			else
			{
				characterMotorsToIndicatorTransform.Value.rotation = airborneForceOrientation;
			}
		}
		if (base.isAuthority && base.fixedAge >= duration)
		{
			outer.SetNextState(new FireGravityBump());
		}
	}

	public override InterruptPriority GetMinimumInterruptPriority()
	{
		return InterruptPriority.PrioritySkill;
	}

	private void AssignIndicators()
	{
		foreach (HurtBox target in GetTargets())
		{
			GameObject val = ((Component)target.healthComponent).gameObject;
			if (Object.op_Implicit((Object)(object)val))
			{
				CharacterMotor component = val.GetComponent<CharacterMotor>();
				if (Object.op_Implicit((Object)(object)component) && !characterMotorsToIndicatorTransforms.ContainsKey(component))
				{
					GameObject val2 = Object.Instantiate<GameObject>(forceIndicatorPrefab, val.transform);
					characterMotorsToIndicatorTransforms.Add(component, val2.transform);
				}
			}
		}
	}

	private void CleanUpIndicators()
	{
		foreach (KeyValuePair<CharacterMotor, Transform> characterMotorsToIndicatorTransform in characterMotorsToIndicatorTransforms)
		{
			Transform value = characterMotorsToIndicatorTransform.Value;
			if (Object.op_Implicit((Object)(object)value))
			{
				EntityState.Destroy((Object)(object)((Component)value).gameObject);
			}
		}
		characterMotorsToIndicatorTransforms.Clear();
	}
}
