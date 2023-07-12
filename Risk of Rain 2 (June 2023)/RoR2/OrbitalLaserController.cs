using System;
using EntityStates;
using UnityEngine;
using UnityEngine.Networking;

namespace RoR2;

public class OrbitalLaserController : MonoBehaviour
{
	private abstract class OrbitalLaserBaseState : BaseState
	{
		protected OrbitalLaserController controller;

		public override void OnEnter()
		{
			base.OnEnter();
			controller = GetComponent<OrbitalLaserController>();
		}
	}

	private class OrbitalLaserChargeState : OrbitalLaserBaseState
	{
		public override void OnEnter()
		{
			base.OnEnter();
			controller.chargeEffect.SetActive(true);
			controller.maxSpeed = controller.chargeMaxVelocity;
		}

		public override void OnExit()
		{
			controller.chargeEffect.SetActive(false);
			base.OnExit();
		}

		public override void FixedUpdate()
		{
			base.FixedUpdate();
			if (NetworkServer.active && base.fixedAge >= controller.chargeDuration)
			{
				outer.SetNextState(new OrbitalLaserFireState());
			}
		}
	}

	private class OrbitalLaserFireState : OrbitalLaserBaseState
	{
		private float bulletAttackTimer;

		public override void OnEnter()
		{
			base.OnEnter();
			controller.fireEffect.SetActive(true);
			controller.maxSpeed = controller.fireMaxVelocity;
		}

		public override void OnExit()
		{
			controller.fireEffect.SetActive(false);
			base.OnExit();
		}

		public override void FixedUpdate()
		{
			//IL_00c9: Unknown result type (might be due to invalid IL or missing references)
			//IL_00ce: Unknown result type (might be due to invalid IL or missing references)
			//IL_00d8: Unknown result type (might be due to invalid IL or missing references)
			//IL_00dd: Unknown result type (might be due to invalid IL or missing references)
			//IL_00e2: Unknown result type (might be due to invalid IL or missing references)
			//IL_00f3: Unknown result type (might be due to invalid IL or missing references)
			//IL_01d5: Unknown result type (might be due to invalid IL or missing references)
			//IL_01da: Unknown result type (might be due to invalid IL or missing references)
			base.FixedUpdate();
			if (!NetworkServer.active)
			{
				return;
			}
			if (base.fixedAge >= controller.fireDuration || !Object.op_Implicit((Object)(object)controller.ownerBody))
			{
				outer.SetNextState(new OrbitalLaserDecayState());
				return;
			}
			bulletAttackTimer -= Time.fixedDeltaTime;
			if (Object.op_Implicit((Object)(object)controller.ownerBody) && bulletAttackTimer < 0f)
			{
				bulletAttackTimer += 1f / controller.fireFrequency;
				BulletAttack bulletAttack = new BulletAttack();
				bulletAttack.owner = ((Component)controller.ownerBody).gameObject;
				bulletAttack.weapon = base.gameObject;
				bulletAttack.origin = base.transform.position + Vector3.up * 600f;
				bulletAttack.maxDistance = 1200f;
				bulletAttack.aimVector = Vector3.down;
				bulletAttack.minSpread = 0f;
				bulletAttack.maxSpread = 0f;
				bulletAttack.damage = Mathf.Lerp(controller.damageCoefficientInitial, controller.damageCoefficientFinal, base.fixedAge / controller.fireDuration) * controller.ownerBody.damage / controller.fireFrequency;
				bulletAttack.force = controller.force;
				bulletAttack.tracerEffectPrefab = controller.tracerEffectPrefab;
				bulletAttack.muzzleName = "";
				bulletAttack.hitEffectPrefab = controller.hitEffectPrefab;
				bulletAttack.isCrit = Util.CheckRoll(controller.ownerBody.crit, controller.ownerBody.master);
				bulletAttack.stopperMask = LayerIndex.world.mask;
				bulletAttack.damageColorIndex = DamageColorIndex.Item;
				bulletAttack.procCoefficient = controller.procCoefficient / controller.fireFrequency;
				bulletAttack.radius = 2f;
				bulletAttack.Fire();
			}
		}
	}

	private class OrbitalLaserDecayState : OrbitalLaserBaseState
	{
		public override void OnEnter()
		{
			base.OnEnter();
			controller.maxSpeed = 0f;
		}

		public override void FixedUpdate()
		{
			base.FixedUpdate();
			if (NetworkServer.active && base.fixedAge >= controller.decayDuration)
			{
				EntityState.Destroy((Object)(object)base.gameObject);
			}
		}
	}

	[NonSerialized]
	public CharacterBody ownerBody;

	private InputBankTest ownerInputBank;

	[Header("Movement Parameters")]
	public float smoothDampTime = 0.3f;

	private Vector3 velocity;

	private float maxSpeed;

	[Header("Attack Parameters")]
	public float fireFrequency = 5f;

	public float damageCoefficientInitial = 6f;

	public float damageCoefficientFinal = 6f;

	public float procCoefficient = 0.5f;

	public float force;

	[Header("Charge")]
	public GameObject chargeEffect;

	public float chargeDuration = 3f;

	public float chargeMaxVelocity = 20f;

	private Transform chargeEffectTransform;

	[Header("Fire")]
	public GameObject fireEffect;

	public float fireDuration = 6f;

	public float fireMaxVelocity = 1f;

	public GameObject tracerEffectPrefab;

	public GameObject hitEffectPrefab;

	[Header("Decay")]
	public float decayDuration = 1.5f;

	[Tooltip("The transform of the child laser pointer effect.")]
	[Header("Laser Pointer")]
	public Transform laserPointerEffectTransform;

	[Tooltip("The transform of the muzzle effect.")]
	public Transform muzzleEffectTransform;

	private Vector3 mostRecentPointerPosition;

	private Vector3 mostRecentPointerNormal;

	private Vector3 mostRecentMuzzlePosition;

	private void Start()
	{
		//IL_0029: Unknown result type (might be due to invalid IL or missing references)
		//IL_002e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0034: Unknown result type (might be due to invalid IL or missing references)
		//IL_0039: Unknown result type (might be due to invalid IL or missing references)
		chargeEffect.SetActive(true);
		chargeEffect.GetComponent<ObjectScaleCurve>().timeMax = chargeDuration;
		mostRecentPointerPosition = ((Component)this).transform.position;
		mostRecentPointerNormal = Vector3.up;
	}

	private void UpdateLaserPointer()
	{
		//IL_0023: Unknown result type (might be due to invalid IL or missing references)
		//IL_0031: Unknown result type (might be due to invalid IL or missing references)
		//IL_0043: Unknown result type (might be due to invalid IL or missing references)
		//IL_004d: Unknown result type (might be due to invalid IL or missing references)
		//IL_004e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0052: Unknown result type (might be due to invalid IL or missing references)
		//IL_0057: Unknown result type (might be due to invalid IL or missing references)
		//IL_0062: Unknown result type (might be due to invalid IL or missing references)
		//IL_006f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0082: Unknown result type (might be due to invalid IL or missing references)
		//IL_0098: Unknown result type (might be due to invalid IL or missing references)
		//IL_009d: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a8: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ad: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b5: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ba: Unknown result type (might be due to invalid IL or missing references)
		//IL_00bf: Unknown result type (might be due to invalid IL or missing references)
		if (Object.op_Implicit((Object)(object)ownerBody))
		{
			ownerInputBank = ((Component)ownerBody).GetComponent<InputBankTest>();
			Ray val = default(Ray);
			((Ray)(ref val)).origin = ownerInputBank.aimOrigin;
			((Ray)(ref val)).direction = ownerInputBank.aimDirection;
			Ray val2 = val;
			mostRecentMuzzlePosition = ((Ray)(ref val2)).origin;
			float num = 900f;
			RaycastHit val3 = default(RaycastHit);
			if (Physics.Raycast(val2, ref val3, num, LayerMask.op_Implicit(LayerIndex.world.mask) | LayerMask.op_Implicit(LayerIndex.entityPrecise.mask), (QueryTriggerInteraction)1))
			{
				mostRecentPointerPosition = ((RaycastHit)(ref val3)).point;
			}
			else
			{
				mostRecentPointerPosition = ((Ray)(ref val2)).GetPoint(num);
			}
			mostRecentPointerNormal = -((Ray)(ref val2)).direction;
		}
	}

	private void Update()
	{
		//IL_000d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0013: Unknown result type (might be due to invalid IL or missing references)
		//IL_0018: Unknown result type (might be due to invalid IL or missing references)
		//IL_0029: Unknown result type (might be due to invalid IL or missing references)
		//IL_002e: Unknown result type (might be due to invalid IL or missing references)
		UpdateLaserPointer();
		laserPointerEffectTransform.SetPositionAndRotation(mostRecentPointerPosition, Quaternion.LookRotation(mostRecentPointerNormal));
		muzzleEffectTransform.SetPositionAndRotation(mostRecentMuzzlePosition, Quaternion.identity);
	}

	private void FixedUpdate()
	{
		//IL_0019: Unknown result type (might be due to invalid IL or missing references)
		//IL_001f: Unknown result type (might be due to invalid IL or missing references)
		//IL_003b: Unknown result type (might be due to invalid IL or missing references)
		UpdateLaserPointer();
		if (NetworkServer.active)
		{
			((Component)this).transform.position = Vector3.SmoothDamp(((Component)this).transform.position, mostRecentPointerPosition, ref velocity, smoothDampTime, maxSpeed, Time.fixedDeltaTime);
		}
	}
}
