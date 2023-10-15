using System;
using RoR2;
using UnityEngine;
using UnityEngine.Networking;

namespace EntityStates.Paladin;

public class LeapSlam : BaseState
{
	private float stopwatch;

	public static float damageCoefficient = 4f;

	public static float forceMagnitude = 16f;

	public static float yBias;

	public static string initialAttackSoundString;

	public static GameObject chargeEffectPrefab;

	public static GameObject slamEffectPrefab;

	public static GameObject hitEffectPrefab;

	public static float leapVelocityCoefficient;

	public static float verticalLeapBonusCoefficient;

	public static float minimumDuration;

	private float leapVelocity;

	private OverlapAttack attack;

	private Transform modelTransform;

	private GameObject leftHandChargeEffect;

	private GameObject rightHandChargeEffect;

	private ChildLocator modelChildLocator;

	private Vector3 initialAimVector;

	private void EnableIndicator(string childLocatorName, ChildLocator childLocator = null)
	{
		if (!Object.op_Implicit((Object)(object)childLocator))
		{
			childLocator = ((Component)GetModelTransform()).GetComponent<ChildLocator>();
		}
		Transform val = childLocator.FindChild(childLocatorName);
		if (Object.op_Implicit((Object)(object)val))
		{
			((Component)val).gameObject.SetActive(true);
			ObjectScaleCurve component = ((Component)val).gameObject.GetComponent<ObjectScaleCurve>();
			if (Object.op_Implicit((Object)(object)component))
			{
				component.time = 0f;
			}
		}
	}

	private void DisableIndicator(string childLocatorName, ChildLocator childLocator = null)
	{
		if (!Object.op_Implicit((Object)(object)childLocator))
		{
			childLocator = ((Component)GetModelTransform()).GetComponent<ChildLocator>();
		}
		Transform val = childLocator.FindChild(childLocatorName);
		if (Object.op_Implicit((Object)(object)val))
		{
			((Component)val).gameObject.SetActive(false);
		}
	}

	public override void OnEnter()
	{
		//IL_003b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0040: Unknown result type (might be due to invalid IL or missing references)
		//IL_0044: Unknown result type (might be due to invalid IL or missing references)
		//IL_0049: Unknown result type (might be due to invalid IL or missing references)
		//IL_0089: Unknown result type (might be due to invalid IL or missing references)
		//IL_008e: Unknown result type (might be due to invalid IL or missing references)
		//IL_013d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0147: Unknown result type (might be due to invalid IL or missing references)
		//IL_014c: Unknown result type (might be due to invalid IL or missing references)
		base.OnEnter();
		leapVelocity = base.characterBody.moveSpeed * leapVelocityCoefficient;
		modelTransform = GetModelTransform();
		Util.PlaySound(initialAttackSoundString, base.gameObject);
		Ray aimRay = GetAimRay();
		initialAimVector = aimRay.direction;
		initialAimVector.y = Mathf.Max(initialAimVector.y, 0f);
		initialAimVector.y += yBias;
		initialAimVector = ((Vector3)(ref initialAimVector)).normalized;
		base.characterMotor.velocity.y = leapVelocity * initialAimVector.y * verticalLeapBonusCoefficient;
		attack = new OverlapAttack();
		attack.attacker = base.gameObject;
		attack.inflictor = base.gameObject;
		attack.teamIndex = TeamComponent.GetObjectTeam(attack.attacker);
		attack.damage = damageCoefficient * damageStat;
		attack.hitEffectPrefab = hitEffectPrefab;
		attack.damageType = DamageType.Stun1s;
		attack.forceVector = Vector3.up * forceMagnitude;
		if (Object.op_Implicit((Object)(object)modelTransform))
		{
			attack.hitBoxGroup = Array.Find(((Component)modelTransform).GetComponents<HitBoxGroup>(), (HitBoxGroup element) => element.groupName == "GroundSlam");
		}
		if (!Object.op_Implicit((Object)(object)modelTransform))
		{
			return;
		}
		modelChildLocator = ((Component)modelTransform).GetComponent<ChildLocator>();
		if (Object.op_Implicit((Object)(object)modelChildLocator))
		{
			GameObject val = chargeEffectPrefab;
			Transform val2 = modelChildLocator.FindChild("HandL");
			Transform val3 = modelChildLocator.FindChild("HandR");
			if (Object.op_Implicit((Object)(object)val2))
			{
				leftHandChargeEffect = Object.Instantiate<GameObject>(val, val2);
			}
			if (Object.op_Implicit((Object)(object)val3))
			{
				rightHandChargeEffect = Object.Instantiate<GameObject>(val, val3);
			}
			EnableIndicator("GroundSlamIndicator", modelChildLocator);
		}
	}

	public override void OnExit()
	{
		if (NetworkServer.active)
		{
			attack.Fire();
		}
		if (base.isAuthority && Object.op_Implicit((Object)(object)modelTransform))
		{
			EffectManager.SimpleMuzzleFlash(slamEffectPrefab, base.gameObject, "SlamZone", transmit: true);
		}
		EntityState.Destroy((Object)(object)leftHandChargeEffect);
		EntityState.Destroy((Object)(object)rightHandChargeEffect);
		DisableIndicator("GroundSlamIndicator", modelChildLocator);
		base.OnExit();
	}

	public override void FixedUpdate()
	{
		//IL_002b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0030: Unknown result type (might be due to invalid IL or missing references)
		//IL_0045: Unknown result type (might be due to invalid IL or missing references)
		//IL_0068: Unknown result type (might be due to invalid IL or missing references)
		//IL_0069: Unknown result type (might be due to invalid IL or missing references)
		//IL_0075: Unknown result type (might be due to invalid IL or missing references)
		base.FixedUpdate();
		stopwatch += Time.fixedDeltaTime;
		if (Object.op_Implicit((Object)(object)base.characterMotor))
		{
			Vector3 velocity = base.characterMotor.velocity;
			Vector3 velocity2 = default(Vector3);
			((Vector3)(ref velocity2))._002Ector(initialAimVector.x * leapVelocity, velocity.y, initialAimVector.z * leapVelocity);
			base.characterMotor.velocity = velocity2;
			base.characterMotor.moveDirection = initialAimVector;
		}
		if (base.characterMotor.isGrounded && stopwatch > minimumDuration && base.isAuthority)
		{
			outer.SetNextStateToMain();
		}
	}

	public override InterruptPriority GetMinimumInterruptPriority()
	{
		return InterruptPriority.PrioritySkill;
	}
}
