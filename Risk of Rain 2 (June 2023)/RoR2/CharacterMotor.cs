using System;
using System.Collections.Generic;
using System.Globalization;
using KinematicCharacterController;
using RoR2.ConVar;
using Unity;
using UnityEngine;
using UnityEngine.Networking;

namespace RoR2;

[RequireComponent(typeof(CharacterBody))]
public class CharacterMotor : BaseCharacterController, IPhysMotor, ILifeBehavior, IDisplacementReceiver, ICharacterGravityParameterProvider, ICharacterFlightParameterProvider
{
	[Serializable]
	public struct HitGroundInfo
	{
		public Vector3 velocity;

		public Vector3 position;

		public override string ToString()
		{
			//IL_0006: Unknown result type (might be due to invalid IL or missing references)
			//IL_0011: Unknown result type (might be due to invalid IL or missing references)
			return $"velocity={velocity} position={position}";
		}
	}

	public delegate void HitGroundDelegate(ref HitGroundInfo hitGroundInfo);

	public struct MovementHitInfo
	{
		public Vector3 velocity;

		public Collider hitCollider;
	}

	public delegate void MovementHitDelegate(ref MovementHitInfo movementHitInfo);

	public static readonly List<CharacterMotor> instancesList;

	[HideInInspector]
	public float walkSpeedPenaltyCoefficient = 1f;

	[Tooltip("The character direction component to supply a move vector to.")]
	public CharacterDirection characterDirection;

	[Tooltip("Whether or not a move vector supplied to this component can cause movement. Use this when the object is driven by root motion.")]
	public bool muteWalkMotion;

	[Tooltip("The mass of this character.")]
	public float mass = 1f;

	[Tooltip("The air control value of this character as a fraction of ground control.")]
	public float airControl = 0.25f;

	[Tooltip("Disables Air Control for things like jumppads")]
	public bool disableAirControlUntilCollision;

	[Tooltip("Auto-assigns parameters skin width, slope angle, and step offset as a function of the Character Motor's radius and height")]
	public bool generateParametersOnAwake = true;

	private NetworkIdentity networkIdentity;

	private CharacterBody body;

	private CapsuleCollider capsuleCollider;

	private static readonly bool enableMotorWithoutAuthority;

	private bool alive = true;

	private const float restDuration = 1f;

	private const float restVelocityThreshold = 0.025f;

	private const float restVelocityThresholdSqr = 0.00062500004f;

	public const float slipStartAngle = 70f;

	public const float slipEndAngle = 55f;

	private float restStopwatch;

	private Vector3 previousPosition;

	private bool isAirControlForced;

	[NonSerialized]
	public int jumpCount;

	[NonSerialized]
	public bool netIsGrounded;

	[NonSerialized]
	public Vector3 netGroundNormal;

	[NonSerialized]
	public Vector3 velocity;

	private Vector3 lastVelocity;

	[NonSerialized]
	public Vector3 rootMotion;

	private Vector3 _moveDirection;

	private static readonly FloatConVar cvCMotorSafeCollisionStepThreshold;

	private int _safeCollisionEnableCount;

	[SerializeField]
	[Tooltip("Determins how gravity affects this character.")]
	private CharacterGravityParameters _gravityParameters;

	[SerializeField]
	[Tooltip("Determines whether this character has three-dimensional or two-dimensional movement capabilities.")]
	private CharacterFlightParameters _flightParameters;

	private static int kRpcRpcApplyForceImpulse;

	private static int kCmdCmdReportHitGround;

	public float walkSpeed => body.moveSpeed * walkSpeedPenaltyCoefficient;

	public float acceleration => body.acceleration;

	public bool atRest => restStopwatch > 1f;

	public bool hasEffectiveAuthority => Util.HasEffectiveAuthority(networkIdentity);

	public Vector3 estimatedGroundNormal
	{
		get
		{
			//IL_0009: Unknown result type (might be due to invalid IL or missing references)
			//IL_001a: Unknown result type (might be due to invalid IL or missing references)
			if (!hasEffectiveAuthority)
			{
				return netGroundNormal;
			}
			return ((BaseCharacterController)this).Motor.GroundingStatus.GroundNormal;
		}
	}

	private bool canWalk
	{
		get
		{
			if (!muteWalkMotion)
			{
				return alive;
			}
			return false;
		}
	}

	public bool isGrounded
	{
		get
		{
			if (!hasEffectiveAuthority)
			{
				return netIsGrounded;
			}
			return ((BaseCharacterController)this).Motor.GroundingStatus.IsStableOnGround;
		}
	}

	float IPhysMotor.mass => mass;

	Vector3 IPhysMotor.velocity => velocity;

	Vector3 IPhysMotor.velocityAuthority
	{
		get
		{
			//IL_0001: Unknown result type (might be due to invalid IL or missing references)
			return velocity;
		}
		set
		{
			//IL_0001: Unknown result type (might be due to invalid IL or missing references)
			//IL_0002: Unknown result type (might be due to invalid IL or missing references)
			velocity = value;
		}
	}

	public Vector3 moveDirection
	{
		get
		{
			//IL_0001: Unknown result type (might be due to invalid IL or missing references)
			return _moveDirection;
		}
		set
		{
			//IL_0001: Unknown result type (might be due to invalid IL or missing references)
			//IL_0002: Unknown result type (might be due to invalid IL or missing references)
			_moveDirection = value;
		}
	}

	private float slopeLimit
	{
		get
		{
			return ((BaseCharacterController)this).Motor.MaxStableSlopeAngle;
		}
		set
		{
			((BaseCharacterController)this).Motor.MaxStableSlopeAngle = value;
		}
	}

	public float stepOffset
	{
		get
		{
			return ((BaseCharacterController)this).Motor.MaxStepHeight;
		}
		set
		{
			((BaseCharacterController)this).Motor.MaxStepHeight = value;
		}
	}

	public float capsuleHeight => capsuleCollider.height;

	public float capsuleRadius => capsuleCollider.radius;

	public StepHandlingMethod stepHandlingMethod
	{
		get
		{
			//IL_0008: Unknown result type (might be due to invalid IL or missing references)
			//IL_0009: Unknown result type (might be due to invalid IL or missing references)
			//IL_000e: Unknown result type (might be due to invalid IL or missing references)
			KinematicCharacterMotor motor = ((BaseCharacterController)this).Motor;
			StepHandlingMethod result = (StepHandlingMethod)0;
			motor.StepHandling = (StepHandlingMethod)0;
			return result;
		}
		set
		{
			//IL_0006: Unknown result type (might be due to invalid IL or missing references)
			//IL_0007: Unknown result type (might be due to invalid IL or missing references)
			((BaseCharacterController)this).Motor.StepHandling = value;
		}
	}

	public bool ledgeHandling
	{
		get
		{
			return ((BaseCharacterController)this).Motor.LedgeHandling;
		}
		set
		{
			((BaseCharacterController)this).Motor.LedgeHandling = value;
		}
	}

	public bool interactiveRigidbodyHandling
	{
		get
		{
			return ((BaseCharacterController)this).Motor.InteractiveRigidbodyHandling;
		}
		set
		{
			((BaseCharacterController)this).Motor.InteractiveRigidbodyHandling = value;
		}
	}

	public Run.FixedTimeStamp lastGroundedTime { get; private set; } = Run.FixedTimeStamp.negativeInfinity;


	public CharacterGravityParameters gravityParameters
	{
		get
		{
			return _gravityParameters;
		}
		set
		{
			if (!_gravityParameters.Equals(value))
			{
				_gravityParameters = value;
				useGravity = _gravityParameters.CheckShouldUseGravity();
			}
		}
	}

	public bool useGravity { get; private set; }

	public CharacterFlightParameters flightParameters
	{
		get
		{
			return _flightParameters;
		}
		set
		{
			if (!_flightParameters.Equals(value))
			{
				_flightParameters = value;
				isFlying = _flightParameters.CheckShouldUseFlight();
			}
		}
	}

	public bool isFlying { get; private set; }

	[Obsolete("Use '.onHitGroundServer' instead, which this is just a backwards-compatibility wrapper for. Or, use '.onHitGroundAuthority' if that is more appropriate to your use case.", false)]
	public event HitGroundDelegate onHitGround
	{
		add
		{
			onHitGroundServer += value;
		}
		remove
		{
			onHitGroundServer -= value;
		}
	}

	public event HitGroundDelegate onHitGroundServer;

	public event HitGroundDelegate onHitGroundAuthority;

	public event MovementHitDelegate onMovementHit;

	private void UpdateInnerMotorEnabled()
	{
		((Behaviour)((BaseCharacterController)this).Motor).enabled = enableMotorWithoutAuthority || hasEffectiveAuthority;
	}

	private void UpdateAuthority()
	{
		UpdateInnerMotorEnabled();
	}

	private void Awake()
	{
		//IL_002b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0030: Unknown result type (might be due to invalid IL or missing references)
		networkIdentity = ((Component)this).GetComponent<NetworkIdentity>();
		body = ((Component)this).GetComponent<CharacterBody>();
		capsuleCollider = ((Component)this).GetComponent<CapsuleCollider>();
		previousPosition = ((Component)this).transform.position;
		((BaseCharacterController)this).Motor.Rigidbody.mass = mass;
		((BaseCharacterController)this).Motor.MaxStableSlopeAngle = 70f;
		((BaseCharacterController)this).Motor.MaxStableDenivelationAngle = 55f;
		((BaseCharacterController)this).Motor.RebuildCollidableLayers();
		if (generateParametersOnAwake)
		{
			GenerateParameters();
		}
		useGravity = gravityParameters.CheckShouldUseGravity();
		isFlying = flightParameters.CheckShouldUseFlight();
	}

	private void Start()
	{
		UpdateAuthority();
	}

	public override void OnStartAuthority()
	{
		UpdateAuthority();
	}

	public override void OnStopAuthority()
	{
		UpdateAuthority();
	}

	private void OnEnable()
	{
		instancesList.Add(this);
		UpdateInnerMotorEnabled();
	}

	private void OnDisable()
	{
		((Behaviour)((BaseCharacterController)this).Motor).enabled = false;
		instancesList.Remove(this);
	}

	private void PreMove(float deltaTime)
	{
		//IL_003b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0040: Unknown result type (might be due to invalid IL or missing references)
		//IL_0089: Unknown result type (might be due to invalid IL or missing references)
		//IL_008c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0091: Unknown result type (might be due to invalid IL or missing references)
		//IL_0092: Unknown result type (might be due to invalid IL or missing references)
		//IL_0099: Unknown result type (might be due to invalid IL or missing references)
		//IL_009e: Unknown result type (might be due to invalid IL or missing references)
		//IL_00bb: Unknown result type (might be due to invalid IL or missing references)
		//IL_00c0: Unknown result type (might be due to invalid IL or missing references)
		//IL_00c4: Unknown result type (might be due to invalid IL or missing references)
		//IL_00c9: Unknown result type (might be due to invalid IL or missing references)
		//IL_00e8: Unknown result type (might be due to invalid IL or missing references)
		if (!hasEffectiveAuthority)
		{
			return;
		}
		float num = acceleration;
		if (isAirControlForced || !isGrounded)
		{
			num *= (disableAirControlUntilCollision ? 0f : airControl);
		}
		Vector3 val = moveDirection;
		if (!isFlying)
		{
			val.y = 0f;
		}
		if (body.isSprinting)
		{
			float magnitude = ((Vector3)(ref val)).magnitude;
			if (magnitude < 1f && magnitude > 0f)
			{
				float num2 = 1f / ((Vector3)(ref val)).magnitude;
				val *= num2;
			}
		}
		Vector3 val2 = val * walkSpeed;
		if (!isFlying)
		{
			val2.y = velocity.y;
		}
		velocity = Vector3.MoveTowards(velocity, val2, num * deltaTime);
		if (useGravity)
		{
			ref float y = ref velocity.y;
			y += Physics.gravity.y * deltaTime;
			if (isGrounded)
			{
				y = Mathf.Max(y, 0f);
			}
		}
	}

	public void OnDeathStart()
	{
		alive = false;
	}

	private void FixedUpdate()
	{
		//IL_0015: Unknown result type (might be due to invalid IL or missing references)
		//IL_001a: Unknown result type (might be due to invalid IL or missing references)
		//IL_001c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0021: Unknown result type (might be due to invalid IL or missing references)
		//IL_0022: Unknown result type (might be due to invalid IL or missing references)
		//IL_0027: Unknown result type (might be due to invalid IL or missing references)
		//IL_0054: Unknown result type (might be due to invalid IL or missing references)
		//IL_0055: Unknown result type (might be due to invalid IL or missing references)
		float fixedDeltaTime = Time.fixedDeltaTime;
		if (fixedDeltaTime != 0f)
		{
			Vector3 position = ((Component)this).transform.position;
			Vector3 val = previousPosition - position;
			if (((Vector3)(ref val)).sqrMagnitude < 0.00062500004f * fixedDeltaTime)
			{
				restStopwatch += fixedDeltaTime;
			}
			else
			{
				restStopwatch = 0f;
			}
			previousPosition = position;
			if (netIsGrounded)
			{
				lastGroundedTime = Run.FixedTimeStamp.now;
			}
		}
	}

	private void GenerateParameters()
	{
		slopeLimit = 70f;
		stepOffset = Mathf.Min(capsuleHeight * 0.1f, 0.2f);
		stepHandlingMethod = (StepHandlingMethod)0;
		ledgeHandling = false;
		interactiveRigidbodyHandling = true;
	}

	public void ApplyForce(Vector3 force, bool alwaysApply = false, bool disableAirControlUntilCollision = false)
	{
		//IL_0008: Unknown result type (might be due to invalid IL or missing references)
		//IL_0009: Unknown result type (might be due to invalid IL or missing references)
		PhysForceInfo forceInfo = PhysForceInfo.Create();
		forceInfo.force = force;
		forceInfo.ignoreGroundStick = alwaysApply;
		forceInfo.disableAirControlUntilCollision = disableAirControlUntilCollision;
		forceInfo.massIsOne = false;
		ApplyForceImpulse(in forceInfo);
	}

	public void ApplyForceImpulse(in PhysForceInfo forceInfo)
	{
		//IL_001d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0022: Unknown result type (might be due to invalid IL or missing references)
		//IL_003b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0048: Unknown result type (might be due to invalid IL or missing references)
		//IL_004d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0079: Unknown result type (might be due to invalid IL or missing references)
		//IL_00aa: Unknown result type (might be due to invalid IL or missing references)
		//IL_00c4: Unknown result type (might be due to invalid IL or missing references)
		//IL_00c9: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ca: Unknown result type (might be due to invalid IL or missing references)
		//IL_00cf: Unknown result type (might be due to invalid IL or missing references)
		if (NetworkServer.active && !hasEffectiveAuthority)
		{
			CallRpcApplyForceImpulse(forceInfo);
			return;
		}
		Vector3 val = forceInfo.force;
		if (!forceInfo.massIsOne)
		{
			_ = ((Vector3)(ref val)).magnitude;
			val *= 1f / mass;
		}
		else
		{
			Debug.Log((object)$"addedVelocity.magnitude = {((Vector3)(ref val)).magnitude}");
		}
		if (mass != 0f)
		{
			if (val.y < 6f && isGrounded && !forceInfo.ignoreGroundStick)
			{
				val.y = 0f;
			}
			if (val.y > 0f)
			{
				((BaseCharacterController)this).Motor.ForceUnground();
			}
			velocity += val;
			if (forceInfo.disableAirControlUntilCollision)
			{
				disableAirControlUntilCollision = true;
			}
		}
	}

	[ClientRpc]
	private void RpcApplyForceImpulse(PhysForceInfo physForceInfo)
	{
		if (!NetworkServer.active)
		{
			ApplyForceImpulse(in physForceInfo);
		}
	}

	public override void UpdateRotation(ref Quaternion currentRotation, float deltaTime)
	{
		//IL_0001: Unknown result type (might be due to invalid IL or missing references)
		//IL_0006: Unknown result type (might be due to invalid IL or missing references)
		currentRotation = Quaternion.identity;
	}

	public override void UpdateVelocity(ref Vector3 currentVelocity, float deltaTime)
	{
		//IL_0002: Unknown result type (might be due to invalid IL or missing references)
		//IL_0007: Unknown result type (might be due to invalid IL or missing references)
		currentVelocity = velocity;
	}

	public override void BeforeCharacterUpdate(float deltaTime)
	{
		//IL_0017: Unknown result type (might be due to invalid IL or missing references)
		//IL_001c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0029: Unknown result type (might be due to invalid IL or missing references)
		//IL_002e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0030: Unknown result type (might be due to invalid IL or missing references)
		//IL_0035: Unknown result type (might be due to invalid IL or missing references)
		//IL_005e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0063: Unknown result type (might be due to invalid IL or missing references)
		//IL_0064: Unknown result type (might be due to invalid IL or missing references)
		//IL_0076: Unknown result type (might be due to invalid IL or missing references)
		//IL_0080: Unknown result type (might be due to invalid IL or missing references)
		//IL_0085: Unknown result type (might be due to invalid IL or missing references)
		float num = cvCMotorSafeCollisionStepThreshold.value * cvCMotorSafeCollisionStepThreshold.value;
		if (rootMotion != Vector3.zero)
		{
			Vector3 val = rootMotion;
			rootMotion = Vector3.zero;
			((BaseCharacterController)this).Motor.SafeMovement = ((Vector3)(ref val)).sqrMagnitude >= num;
			((BaseCharacterController)this).Motor.MoveCharacter(((Component)this).transform.position + val);
		}
		PreMove(deltaTime);
		Vector3 val2 = velocity * Time.fixedDeltaTime;
		float sqrMagnitude = ((Vector3)(ref val2)).sqrMagnitude;
		((BaseCharacterController)this).Motor.SafeMovement = sqrMagnitude >= num;
	}

	public override void PostGroundingUpdate(float deltaTime)
	{
		if (((BaseCharacterController)this).Motor.GroundingStatus.IsStableOnGround != ((BaseCharacterController)this).Motor.LastGroundingStatus.IsStableOnGround)
		{
			netIsGrounded = ((BaseCharacterController)this).Motor.GroundingStatus.IsStableOnGround;
			if (((BaseCharacterController)this).Motor.GroundingStatus.IsStableOnGround)
			{
				OnLanded();
			}
			else
			{
				OnLeaveStableGround();
			}
		}
	}

	private void OnLanded()
	{
		//IL_0012: Unknown result type (might be due to invalid IL or missing references)
		//IL_0017: Unknown result type (might be due to invalid IL or missing references)
		//IL_0029: Unknown result type (might be due to invalid IL or missing references)
		//IL_002e: Unknown result type (might be due to invalid IL or missing references)
		jumpCount = 0;
		HitGroundInfo hitGroundInfo = default(HitGroundInfo);
		hitGroundInfo.velocity = lastVelocity;
		hitGroundInfo.position = ((BaseCharacterController)this).Motor.GroundingStatus.GroundPoint;
		HitGroundInfo hitGroundInfo2 = hitGroundInfo;
		if (hasEffectiveAuthority)
		{
			try
			{
				this.onHitGroundAuthority?.Invoke(ref hitGroundInfo2);
			}
			catch (Exception ex)
			{
				Debug.LogError((object)ex);
			}
			if (NetworkServer.active)
			{
				OnHitGroundServer(hitGroundInfo2);
			}
			else
			{
				CallCmdReportHitGround(hitGroundInfo2);
			}
		}
	}

	[Server]
	private void OnHitGroundServer(HitGroundInfo hitGroundInfo)
	{
		//IL_0021: Unknown result type (might be due to invalid IL or missing references)
		if (!NetworkServer.active)
		{
			Debug.LogWarning((object)"[Server] function 'System.Void RoR2.CharacterMotor::OnHitGroundServer(RoR2.CharacterMotor/HitGroundInfo)' called on client");
			return;
		}
		GlobalEventManager.instance.OnCharacterHitGroundServer(body, hitGroundInfo.velocity);
		try
		{
			this.onHitGroundServer?.Invoke(ref hitGroundInfo);
		}
		catch (Exception ex)
		{
			Debug.LogError((object)ex);
		}
	}

	[Command]
	private void CmdReportHitGround(HitGroundInfo hitGroundInfo)
	{
		OnHitGroundServer(hitGroundInfo);
	}

	private void OnLeaveStableGround()
	{
		if (jumpCount < 1)
		{
			jumpCount = 1;
		}
	}

	public override void AfterCharacterUpdate(float deltaTime)
	{
		//IL_0002: Unknown result type (might be due to invalid IL or missing references)
		//IL_0007: Unknown result type (might be due to invalid IL or missing references)
		//IL_0013: Unknown result type (might be due to invalid IL or missing references)
		//IL_0018: Unknown result type (might be due to invalid IL or missing references)
		lastVelocity = velocity;
		velocity = ((BaseCharacterController)this).Motor.BaseVelocity;
	}

	public override bool IsColliderValidForCollisions(Collider coll)
	{
		if (!coll.isTrigger)
		{
			return (Object)(object)coll != (Object)(object)((BaseCharacterController)this).Motor.Capsule;
		}
		return false;
	}

	public override void OnGroundHit(Collider hitCollider, Vector3 hitNormal, Vector3 hitPoint, ref HitStabilityReport hitStabilityReport)
	{
		//IL_0008: Unknown result type (might be due to invalid IL or missing references)
		isAirControlForced = false;
		SurfaceDef objectSurfaceDef = SurfaceDefProvider.GetObjectSurfaceDef(hitCollider, hitPoint);
		if (Object.op_Implicit((Object)(object)objectSurfaceDef))
		{
			isAirControlForced = objectSurfaceDef.isSlippery;
		}
	}

	public override void OnMovementHit(Collider hitCollider, Vector3 hitNormal, Vector3 hitPoint, ref HitStabilityReport hitStabilityReport)
	{
		//IL_001a: Unknown result type (might be due to invalid IL or missing references)
		//IL_001f: Unknown result type (might be due to invalid IL or missing references)
		disableAirControlUntilCollision = false;
		if (this.onMovementHit != null)
		{
			MovementHitInfo movementHitInfo = default(MovementHitInfo);
			movementHitInfo.velocity = velocity;
			movementHitInfo.hitCollider = hitCollider;
			MovementHitInfo movementHitInfo2 = movementHitInfo;
			this.onMovementHit(ref movementHitInfo2);
		}
	}

	public override void ProcessHitStabilityReport(Collider hitCollider, Vector3 hitNormal, Vector3 hitPoint, Vector3 atCharacterPosition, Quaternion atCharacterRotation, ref HitStabilityReport hitStabilityReport)
	{
	}

	public void Jump(float horizontalMultiplier, float verticalMultiplier, bool vault = false)
	{
		//IL_0001: Unknown result type (might be due to invalid IL or missing references)
		//IL_0006: Unknown result type (might be due to invalid IL or missing references)
		//IL_000b: Unknown result type (might be due to invalid IL or missing references)
		//IL_000c: Unknown result type (might be due to invalid IL or missing references)
		//IL_002f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0031: Unknown result type (might be due to invalid IL or missing references)
		//IL_0036: Unknown result type (might be due to invalid IL or missing references)
		//IL_0037: Unknown result type (might be due to invalid IL or missing references)
		//IL_0043: Unknown result type (might be due to invalid IL or missing references)
		//IL_0049: Unknown result type (might be due to invalid IL or missing references)
		//IL_004e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0064: Unknown result type (might be due to invalid IL or missing references)
		//IL_0065: Unknown result type (might be due to invalid IL or missing references)
		Vector3 val = moveDirection;
		if (vault)
		{
			velocity = val;
		}
		else
		{
			val.y = 0f;
			float magnitude = ((Vector3)(ref val)).magnitude;
			if (magnitude > 0f)
			{
				val /= magnitude;
			}
			Vector3 val2 = val * body.moveSpeed * horizontalMultiplier;
			val2.y = body.jumpPower * verticalMultiplier;
			velocity = val2;
		}
		((BaseCharacterController)this).Motor.ForceUnground();
	}

	public void AddDisplacement(Vector3 displacement)
	{
		//IL_0002: Unknown result type (might be due to invalid IL or missing references)
		//IL_0007: Unknown result type (might be due to invalid IL or missing references)
		//IL_0008: Unknown result type (might be due to invalid IL or missing references)
		//IL_000d: Unknown result type (might be due to invalid IL or missing references)
		rootMotion += displacement;
	}

	static CharacterMotor()
	{
		//IL_0057: Unknown result type (might be due to invalid IL or missing references)
		//IL_0061: Expected O, but got Unknown
		//IL_0081: Unknown result type (might be due to invalid IL or missing references)
		//IL_008b: Expected O, but got Unknown
		instancesList = new List<CharacterMotor>();
		enableMotorWithoutAuthority = false;
		cvCMotorSafeCollisionStepThreshold = new FloatConVar("cmotor_safe_collision_step_threshold", ConVarFlags.Cheat, 1.0833334f.ToString(CultureInfo.InvariantCulture), "How large of a movement in meters/fixedTimeStep is needed to trigger more expensive \"safe\" collisions to prevent tunneling.");
		kCmdCmdReportHitGround = 1796547162;
		NetworkBehaviour.RegisterCommandDelegate(typeof(CharacterMotor), kCmdCmdReportHitGround, new CmdDelegate(InvokeCmdCmdReportHitGround));
		kRpcRpcApplyForceImpulse = 1042934326;
		NetworkBehaviour.RegisterRpcDelegate(typeof(CharacterMotor), kRpcRpcApplyForceImpulse, new CmdDelegate(InvokeRpcRpcApplyForceImpulse));
		NetworkCRC.RegisterBehaviour("CharacterMotor", 0);
	}

	void IPhysMotor.ApplyForceImpulse(in PhysForceInfo physForceInfo)
	{
		ApplyForceImpulse(in physForceInfo);
	}

	private void UNetVersion()
	{
	}

	protected static void InvokeCmdCmdReportHitGround(NetworkBehaviour obj, NetworkReader reader)
	{
		if (!NetworkServer.active)
		{
			Debug.LogError((object)"Command CmdReportHitGround called on client.");
		}
		else
		{
			((CharacterMotor)(object)obj).CmdReportHitGround(GeneratedNetworkCode._ReadHitGroundInfo_CharacterMotor(reader));
		}
	}

	public void CallCmdReportHitGround(HitGroundInfo hitGroundInfo)
	{
		//IL_002d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0033: Expected O, but got Unknown
		//IL_0058: Unknown result type (might be due to invalid IL or missing references)
		if (!NetworkClient.active)
		{
			Debug.LogError((object)"Command function CmdReportHitGround called on server.");
			return;
		}
		if (((NetworkBehaviour)this).isServer)
		{
			CmdReportHitGround(hitGroundInfo);
			return;
		}
		NetworkWriter val = new NetworkWriter();
		val.Write((short)0);
		val.Write((short)5);
		val.WritePackedUInt32((uint)kCmdCmdReportHitGround);
		val.Write(((Component)this).GetComponent<NetworkIdentity>().netId);
		GeneratedNetworkCode._WriteHitGroundInfo_CharacterMotor(val, hitGroundInfo);
		((NetworkBehaviour)this).SendCommandInternal(val, 0, "CmdReportHitGround");
	}

	protected static void InvokeRpcRpcApplyForceImpulse(NetworkBehaviour obj, NetworkReader reader)
	{
		if (!NetworkClient.active)
		{
			Debug.LogError((object)"RPC RpcApplyForceImpulse called on server.");
		}
		else
		{
			((CharacterMotor)(object)obj).RpcApplyForceImpulse(GeneratedNetworkCode._ReadPhysForceInfo_None(reader));
		}
	}

	public void CallRpcApplyForceImpulse(PhysForceInfo physForceInfo)
	{
		//IL_0016: Unknown result type (might be due to invalid IL or missing references)
		//IL_001c: Expected O, but got Unknown
		//IL_0041: Unknown result type (might be due to invalid IL or missing references)
		if (!NetworkServer.active)
		{
			Debug.LogError((object)"RPC Function RpcApplyForceImpulse called on client.");
			return;
		}
		NetworkWriter val = new NetworkWriter();
		val.Write((short)0);
		val.Write((short)2);
		val.WritePackedUInt32((uint)kRpcRpcApplyForceImpulse);
		val.Write(((Component)this).GetComponent<NetworkIdentity>().netId);
		GeneratedNetworkCode._WritePhysForceInfo_None(val, physForceInfo);
		((NetworkBehaviour)this).SendRPCInternal(val, 0, "RpcApplyForceImpulse");
	}

	public override bool OnSerialize(NetworkWriter writer, bool forceAll)
	{
		bool flag = ((BaseCharacterController)this).OnSerialize(writer, forceAll);
		bool flag2 = default(bool);
		return flag2 || flag;
	}

	public override void OnDeserialize(NetworkReader reader, bool initialState)
	{
		((BaseCharacterController)this).OnDeserialize(reader, initialState);
	}

	public override void PreStartClient()
	{
		((BaseCharacterController)this).PreStartClient();
	}
}
