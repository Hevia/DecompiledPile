using RoR2;
using UnityEngine;
using UnityEngine.Networking;

namespace EntityStates.Paladin;

public class DashSlam : BaseState
{
	private float stopwatch;

	public static float damageCoefficient = 4f;

	public static float baseForceMagnitude = 16f;

	public static float bonusImpactForce;

	public static string initialAttackSoundString;

	public static GameObject chargeEffectPrefab;

	public static GameObject slamEffectPrefab;

	public static GameObject hitEffectPrefab;

	public static float initialSpeedCoefficient;

	public static float finalSpeedCoefficient;

	public static float duration;

	public static float overlapSphereRadius;

	public static float blastAttackRadius;

	private BlastAttack attack;

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
		//IL_0024: Unknown result type (might be due to invalid IL or missing references)
		//IL_0029: Unknown result type (might be due to invalid IL or missing references)
		//IL_002d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0032: Unknown result type (might be due to invalid IL or missing references)
		//IL_0037: Unknown result type (might be due to invalid IL or missing references)
		//IL_003c: Unknown result type (might be due to invalid IL or missing references)
		//IL_005d: Unknown result type (might be due to invalid IL or missing references)
		base.OnEnter();
		modelTransform = GetModelTransform();
		Util.PlaySound(initialAttackSoundString, base.gameObject);
		Ray aimRay = GetAimRay();
		initialAimVector = Vector3.ProjectOnPlane(((Ray)(ref aimRay)).direction, Vector3.up);
		base.characterMotor.velocity.y = 0f;
		base.characterDirection.forward = initialAimVector;
		attack = new BlastAttack();
		attack.attacker = base.gameObject;
		attack.inflictor = base.gameObject;
		attack.teamIndex = TeamComponent.GetObjectTeam(attack.attacker);
		attack.baseDamage = damageCoefficient * damageStat;
		attack.damageType = DamageType.Stun1s;
		attack.baseForce = baseForceMagnitude;
		attack.radius = blastAttackRadius + base.characterBody.radius;
		attack.falloffModel = BlastAttack.FalloffModel.None;
		attack.attackerFiltering = AttackerFiltering.NeverHitSelf;
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
		//IL_0013: Unknown result type (might be due to invalid IL or missing references)
		//IL_0018: Unknown result type (might be due to invalid IL or missing references)
		//IL_0024: Unknown result type (might be due to invalid IL or missing references)
		//IL_0029: Unknown result type (might be due to invalid IL or missing references)
		//IL_0033: Unknown result type (might be due to invalid IL or missing references)
		//IL_0038: Unknown result type (might be due to invalid IL or missing references)
		//IL_0042: Unknown result type (might be due to invalid IL or missing references)
		//IL_0047: Unknown result type (might be due to invalid IL or missing references)
		if (NetworkServer.active)
		{
			attack.position = base.transform.position;
			attack.bonusForce = (initialAimVector + Vector3.up * 0.3f) * bonusImpactForce;
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
		//IL_0026: Unknown result type (might be due to invalid IL or missing references)
		//IL_0044: Unknown result type (might be due to invalid IL or missing references)
		//IL_00f6: Unknown result type (might be due to invalid IL or missing references)
		//IL_00f8: Unknown result type (might be due to invalid IL or missing references)
		//IL_0104: Unknown result type (might be due to invalid IL or missing references)
		base.FixedUpdate();
		stopwatch += Time.fixedDeltaTime;
		if (base.isAuthority)
		{
			Collider[] array = Physics.OverlapSphere(base.transform.position, base.characterBody.radius + overlapSphereRadius, LayerMask.op_Implicit(LayerIndex.entityPrecise.mask));
			for (int i = 0; i < array.Length; i++)
			{
				HurtBox component = ((Component)array[i]).GetComponent<HurtBox>();
				if (Object.op_Implicit((Object)(object)component) && (Object)(object)component.healthComponent != (Object)(object)base.healthComponent)
				{
					outer.SetNextStateToMain();
					return;
				}
			}
		}
		if (Object.op_Implicit((Object)(object)base.characterMotor))
		{
			float num = Mathf.Lerp(initialSpeedCoefficient, finalSpeedCoefficient, stopwatch / duration) * base.characterBody.moveSpeed;
			Vector3 velocity = default(Vector3);
			((Vector3)(ref velocity))._002Ector(initialAimVector.x * num, 0f, initialAimVector.z * num);
			base.characterMotor.velocity = velocity;
			base.characterMotor.moveDirection = initialAimVector;
		}
		if (stopwatch > duration && base.isAuthority)
		{
			outer.SetNextStateToMain();
		}
	}

	public override InterruptPriority GetMinimumInterruptPriority()
	{
		return InterruptPriority.PrioritySkill;
	}
}
