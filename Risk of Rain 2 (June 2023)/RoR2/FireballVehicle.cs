using UnityEngine;
using UnityEngine.Networking;

namespace RoR2;

[RequireComponent(typeof(Rigidbody))]
[RequireComponent(typeof(VehicleSeat))]
public class FireballVehicle : MonoBehaviour, ICameraStateProvider
{
	[Header("Vehicle Parameters")]
	public float duration = 3f;

	public float initialSpeed = 120f;

	public float targetSpeed = 40f;

	public float acceleration = 20f;

	public float cameraLerpTime = 1f;

	[Header("Blast Parameters")]
	public bool detonateOnCollision;

	public GameObject explosionEffectPrefab;

	public float blastDamageCoefficient;

	public float blastRadius;

	public float blastForce;

	public BlastAttack.FalloffModel blastFalloffModel;

	public DamageType blastDamageType;

	public Vector3 blastBonusForce;

	public float blastProcCoefficient;

	public string explosionSoundString;

	[Header("Overlap Parameters")]
	public float overlapDamageCoefficient;

	public float overlapProcCoefficient;

	public float overlapForce;

	public float overlapFireFrequency;

	public float overlapResetFrequency;

	public float overlapVehicleDurationBonusPerHit;

	public GameObject overlapHitEffectPrefab;

	private float age;

	private bool hasDetonatedServer;

	private VehicleSeat vehicleSeat;

	private Rigidbody rigidbody;

	private OverlapAttack overlapAttack;

	private float overlapFireAge;

	private float overlapResetAge;

	private void Awake()
	{
		vehicleSeat = ((Component)this).GetComponent<VehicleSeat>();
		vehicleSeat.onPassengerEnter += OnPassengerEnter;
		vehicleSeat.onPassengerExit += OnPassengerExit;
		rigidbody = ((Component)this).GetComponent<Rigidbody>();
	}

	private void OnPassengerExit(GameObject passenger)
	{
		if (NetworkServer.active)
		{
			DetonateServer();
		}
		foreach (CameraRigController readOnlyInstances in CameraRigController.readOnlyInstancesList)
		{
			if ((Object)(object)readOnlyInstances.target == (Object)(object)passenger)
			{
				readOnlyInstances.SetOverrideCam(this, 0f);
				readOnlyInstances.SetOverrideCam(null, cameraLerpTime);
			}
		}
	}

	private void OnPassengerEnter(GameObject passenger)
	{
		//IL_001e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0023: Unknown result type (might be due to invalid IL or missing references)
		//IL_002a: Unknown result type (might be due to invalid IL or missing references)
		//IL_002b: Unknown result type (might be due to invalid IL or missing references)
		//IL_003b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0042: Unknown result type (might be due to invalid IL or missing references)
		if (Object.op_Implicit((Object)(object)vehicleSeat.currentPassengerInputBank))
		{
			Vector3 aimDirection = vehicleSeat.currentPassengerInputBank.aimDirection;
			rigidbody.rotation = Quaternion.LookRotation(aimDirection);
			rigidbody.velocity = aimDirection * initialSpeed;
			CharacterBody currentPassengerBody = vehicleSeat.currentPassengerBody;
			overlapAttack = new OverlapAttack
			{
				attacker = ((Component)currentPassengerBody).gameObject,
				damage = overlapDamageCoefficient * currentPassengerBody.damage,
				pushAwayForce = overlapForce,
				isCrit = currentPassengerBody.RollCrit(),
				damageColorIndex = DamageColorIndex.Item,
				inflictor = ((Component)this).gameObject,
				procChainMask = default(ProcChainMask),
				procCoefficient = overlapProcCoefficient,
				teamIndex = currentPassengerBody.teamComponent.teamIndex,
				hitBoxGroup = ((Component)this).gameObject.GetComponent<HitBoxGroup>(),
				hitEffectPrefab = overlapHitEffectPrefab
			};
		}
	}

	private void DetonateServer()
	{
		//IL_0033: Unknown result type (might be due to invalid IL or missing references)
		//IL_0089: Unknown result type (might be due to invalid IL or missing references)
		//IL_008e: Unknown result type (might be due to invalid IL or missing references)
		//IL_00d8: Unknown result type (might be due to invalid IL or missing references)
		//IL_00dd: Unknown result type (might be due to invalid IL or missing references)
		if (!hasDetonatedServer)
		{
			hasDetonatedServer = true;
			CharacterBody currentPassengerBody = vehicleSeat.currentPassengerBody;
			if (Object.op_Implicit((Object)(object)currentPassengerBody))
			{
				EffectData effectData = new EffectData
				{
					origin = ((Component)this).transform.position,
					scale = blastRadius
				};
				EffectManager.SpawnEffect(explosionEffectPrefab, effectData, transmit: true);
				BlastAttack blastAttack = new BlastAttack();
				blastAttack.attacker = ((Component)currentPassengerBody).gameObject;
				blastAttack.baseDamage = blastDamageCoefficient * currentPassengerBody.damage;
				blastAttack.baseForce = blastForce;
				blastAttack.bonusForce = blastBonusForce;
				blastAttack.attackerFiltering = AttackerFiltering.NeverHitSelf;
				blastAttack.crit = currentPassengerBody.RollCrit();
				blastAttack.damageColorIndex = DamageColorIndex.Item;
				blastAttack.damageType = blastDamageType;
				blastAttack.falloffModel = blastFalloffModel;
				blastAttack.inflictor = ((Component)this).gameObject;
				blastAttack.position = ((Component)this).transform.position;
				blastAttack.procChainMask = default(ProcChainMask);
				blastAttack.procCoefficient = blastProcCoefficient;
				blastAttack.radius = blastRadius;
				blastAttack.teamIndex = currentPassengerBody.teamComponent.teamIndex;
				blastAttack.Fire();
			}
			Util.PlaySound(explosionSoundString, ((Component)this).gameObject);
			Object.Destroy((Object)(object)((Component)this).gameObject);
		}
	}

	private void FixedUpdate()
	{
		//IL_00dd: Unknown result type (might be due to invalid IL or missing references)
		//IL_00e2: Unknown result type (might be due to invalid IL or missing references)
		//IL_00e3: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ec: Unknown result type (might be due to invalid IL or missing references)
		//IL_00f1: Unknown result type (might be due to invalid IL or missing references)
		//IL_00f8: Unknown result type (might be due to invalid IL or missing references)
		//IL_00fd: Unknown result type (might be due to invalid IL or missing references)
		//IL_0100: Unknown result type (might be due to invalid IL or missing references)
		//IL_010b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0110: Unknown result type (might be due to invalid IL or missing references)
		//IL_0111: Unknown result type (might be due to invalid IL or missing references)
		//IL_0112: Unknown result type (might be due to invalid IL or missing references)
		//IL_011f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0124: Unknown result type (might be due to invalid IL or missing references)
		//IL_012e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0133: Unknown result type (might be due to invalid IL or missing references)
		//IL_0143: Unknown result type (might be due to invalid IL or missing references)
		//IL_0145: Unknown result type (might be due to invalid IL or missing references)
		//IL_0146: Unknown result type (might be due to invalid IL or missing references)
		if (!Object.op_Implicit((Object)(object)vehicleSeat) || !Object.op_Implicit((Object)(object)vehicleSeat.currentPassengerInputBank))
		{
			return;
		}
		age += Time.fixedDeltaTime;
		overlapFireAge += Time.fixedDeltaTime;
		overlapResetAge += Time.fixedDeltaTime;
		if (NetworkServer.active)
		{
			if (overlapFireAge > 1f / overlapFireFrequency)
			{
				if (overlapAttack.Fire())
				{
					age = Mathf.Max(0f, age - overlapVehicleDurationBonusPerHit);
				}
				overlapFireAge = 0f;
			}
			if (overlapResetAge >= 1f / overlapResetFrequency)
			{
				overlapAttack.ResetIgnoredHealthComponents();
				overlapResetAge = 0f;
			}
		}
		Ray originalAimRay = vehicleSeat.currentPassengerInputBank.GetAimRay();
		originalAimRay = CameraRigController.ModifyAimRayIfApplicable(originalAimRay, ((Component)this).gameObject, out var _);
		Vector3 velocity = rigidbody.velocity;
		Vector3 val = ((Ray)(ref originalAimRay)).direction * targetSpeed;
		Vector3 val2 = Vector3.MoveTowards(velocity, val, acceleration * Time.fixedDeltaTime);
		rigidbody.MoveRotation(Quaternion.LookRotation(((Ray)(ref originalAimRay)).direction));
		rigidbody.AddForce(val2 - velocity, (ForceMode)2);
		if (NetworkServer.active && duration <= age)
		{
			DetonateServer();
		}
	}

	private void OnCollisionEnter(Collision collision)
	{
		if (detonateOnCollision && NetworkServer.active)
		{
			DetonateServer();
		}
	}

	public void GetCameraState(CameraRigController cameraRigController, ref CameraState cameraState)
	{
	}

	public bool IsUserLookAllowed(CameraRigController cameraRigController)
	{
		return true;
	}

	public bool IsUserControlAllowed(CameraRigController cameraRigController)
	{
		return true;
	}

	public bool IsHudAllowed(CameraRigController cameraRigController)
	{
		return true;
	}
}
