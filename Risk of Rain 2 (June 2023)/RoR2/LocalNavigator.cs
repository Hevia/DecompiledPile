using KinematicCharacterController;
using RoR2.ConVar;
using UnityEngine;

namespace RoR2;

public class LocalNavigator
{
	private readonly struct BodyComponents
	{
		public readonly CharacterBody body;

		public readonly Transform transform;

		public readonly CharacterMotor motor;

		public readonly Collider bodyCollider;

		public BodyComponents(CharacterBody body)
		{
			this = default(BodyComponents);
			this.body = body;
			if (Object.op_Implicit((Object)(object)body))
			{
				transform = ((Component)body).transform;
				motor = body.characterMotor;
				if (Object.op_Implicit((Object)(object)motor))
				{
					bodyCollider = (Collider)(object)((BaseCharacterController)body.characterMotor).Motor.Capsule;
				}
			}
		}
	}

	private readonly struct BodySnapshot
	{
		public readonly Vector3 position;

		public readonly Vector3 chestPosition;

		public readonly Vector3 footPosition;

		public readonly Vector3 motorMoveDirection;

		public readonly Vector3 motorVelocity;

		public readonly float maxMoveSpeed;

		public readonly float acceleration;

		public readonly float maxJumpHeight;

		public readonly float maxJumpSpeed;

		public readonly bool isGrounded;

		public readonly float time;

		public readonly float bodyRadius;

		public readonly bool isFlying;

		public readonly bool isJumping;

		private readonly bool hasBody;

		private readonly bool hasBodyCollider;

		private readonly bool hasMotor;

		public bool isValid => hasBody;

		public bool canJump
		{
			get
			{
				if (isGrounded)
				{
					return maxJumpSpeed > 0f;
				}
				return false;
			}
		}

		public BodySnapshot(in BodyComponents bodyComponents, float time)
		{
			//IL_0058: Unknown result type (might be due to invalid IL or missing references)
			//IL_005d: Unknown result type (might be due to invalid IL or missing references)
			//IL_0064: Unknown result type (might be due to invalid IL or missing references)
			//IL_0069: Unknown result type (might be due to invalid IL or missing references)
			//IL_0070: Unknown result type (might be due to invalid IL or missing references)
			//IL_0075: Unknown result type (might be due to invalid IL or missing references)
			//IL_00f3: Unknown result type (might be due to invalid IL or missing references)
			//IL_00f8: Unknown result type (might be due to invalid IL or missing references)
			//IL_00fb: Unknown result type (might be due to invalid IL or missing references)
			//IL_0114: Unknown result type (might be due to invalid IL or missing references)
			//IL_0135: Unknown result type (might be due to invalid IL or missing references)
			//IL_0166: Unknown result type (might be due to invalid IL or missing references)
			//IL_016b: Unknown result type (might be due to invalid IL or missing references)
			this = default(BodySnapshot);
			this.time = time;
			hasBody = Object.op_Implicit((Object)(object)bodyComponents.body);
			hasBodyCollider = Object.op_Implicit((Object)(object)bodyComponents.bodyCollider);
			hasMotor = Object.op_Implicit((Object)(object)bodyComponents.motor);
			if (Object.op_Implicit((Object)(object)bodyComponents.body))
			{
				position = bodyComponents.transform.position;
				chestPosition = position;
				footPosition = position;
				maxMoveSpeed = bodyComponents.body.moveSpeed;
				acceleration = bodyComponents.body.acceleration;
				maxJumpHeight = bodyComponents.body.maxJumpHeight;
				maxJumpSpeed = bodyComponents.body.jumpPower;
				bodyRadius = bodyComponents.body.radius;
				isFlying = bodyComponents.body.isFlying;
			}
			if (Object.op_Implicit((Object)(object)bodyComponents.bodyCollider))
			{
				Bounds bounds = bodyComponents.bodyCollider.bounds;
				((Bounds)(ref bounds)).center = Vector3.zero;
				chestPosition.y += ((Bounds)(ref bounds)).max.y * 0.5f;
				footPosition.y += ((Bounds)(ref bounds)).min.y;
			}
			if (Object.op_Implicit((Object)(object)bodyComponents.motor))
			{
				isGrounded = bodyComponents.motor.isGrounded;
				motorVelocity = bodyComponents.motor.velocity;
				isJumping = !isGrounded;
			}
		}
	}

	private readonly struct SnapshotDelta
	{
		public readonly float deltaTime;

		public readonly Vector3 estimatedVelocity;

		public readonly bool isValid;

		public SnapshotDelta(in BodySnapshot oldSnapshot, in BodySnapshot newSnapshot)
		{
			//IL_0047: Unknown result type (might be due to invalid IL or missing references)
			//IL_004d: Unknown result type (might be due to invalid IL or missing references)
			//IL_0052: Unknown result type (might be due to invalid IL or missing references)
			//IL_005d: Unknown result type (might be due to invalid IL or missing references)
			//IL_0062: Unknown result type (might be due to invalid IL or missing references)
			this = default(SnapshotDelta);
			if (oldSnapshot.isValid && newSnapshot.isValid)
			{
				deltaTime = newSnapshot.time - oldSnapshot.time;
				isValid = deltaTime > 0f;
				if (isValid)
				{
					estimatedVelocity = (newSnapshot.position - oldSnapshot.position) / deltaTime;
				}
			}
		}
	}

	private struct RaycastResults
	{
		public bool forwardObstructed;

		public bool leftObstructed;

		public bool rightObstructed;
	}

	private Vector3 previousMoveVector;

	public Vector3 targetPosition;

	public float avoidanceDuration = 0.5f;

	private float avoidanceTimer;

	public bool allowWalkOffCliff;

	public bool wasObstructedLastUpdate;

	private float localTime;

	private float raycastTimer;

	private static readonly float raycastUpdateInterval = 0.2f;

	private static readonly float avoidanceAngle = 45f;

	private const bool enableFrustration = true;

	private const bool enableWhiskers = true;

	private const bool enableCliffAvoidance = true;

	private BodyComponents bodyComponents;

	private float walkFrustration;

	private const float frustrationLimit = 1f;

	private const float frustrationDecayRate = 0.25f;

	private const float frustrationMinimumSpeed = 0.1f;

	private float aiStopwatch;

	private Vector3 velocityDelta;

	private Vector3 estimatedAcceleration;

	private float accelerationAccuracy;

	private float moveDirectionAccuracy;

	private bool hasMadeSharpTurn;

	private float speedAsFractionOfTopSpeed;

	private bool isAlreadyMovingAtSufficientSpeed;

	private BodySnapshot currentSnapshot;

	private BodySnapshot previousSnapshot;

	private SnapshotDelta currentSnapshotDelta;

	private SnapshotDelta previousSnapshotDelta;

	private RaycastResults raycastResults;

	private static readonly BoolConVar cvLocalNavigatorDebugDraw = new BoolConVar("local_navigator_debug_draw", ConVarFlags.None, "0", "Enables debug drawing of LocalNavigator (drawing visible in editor only).\n  Orange Line: Current position to target position\n  Yellow Line: Raycasts\n  Red Point: Raycast hit position\n  Green Line: Final chosen move vector");

	public Vector3 moveVector { get; private set; }

	public float jumpSpeed { get; private set; }

	public void SetBody(CharacterBody newBody)
	{
		bodyComponents = new BodyComponents(newBody);
		PushSnapshot();
		PushSnapshot();
	}

	private void PushSnapshot()
	{
		previousSnapshot = currentSnapshot;
		currentSnapshot = new BodySnapshot(in bodyComponents, localTime);
		previousSnapshotDelta = currentSnapshotDelta;
		currentSnapshotDelta = new SnapshotDelta(in previousSnapshot, in currentSnapshot);
	}

	public void Update(float deltaTime)
	{
		//IL_0026: Unknown result type (might be due to invalid IL or missing references)
		//IL_002c: Unknown result type (might be due to invalid IL or missing references)
		//IL_003c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0041: Unknown result type (might be due to invalid IL or missing references)
		//IL_0060: Unknown result type (might be due to invalid IL or missing references)
		//IL_0065: Unknown result type (might be due to invalid IL or missing references)
		//IL_006b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0076: Unknown result type (might be due to invalid IL or missing references)
		//IL_007b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0080: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a2: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a4: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a9: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ab: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b0: Unknown result type (might be due to invalid IL or missing references)
		//IL_00bd: Unknown result type (might be due to invalid IL or missing references)
		//IL_00cc: Unknown result type (might be due to invalid IL or missing references)
		//IL_00e4: Unknown result type (might be due to invalid IL or missing references)
		//IL_0152: Unknown result type (might be due to invalid IL or missing references)
		//IL_0158: Unknown result type (might be due to invalid IL or missing references)
		//IL_015d: Unknown result type (might be due to invalid IL or missing references)
		//IL_019f: Unknown result type (might be due to invalid IL or missing references)
		//IL_01a4: Unknown result type (might be due to invalid IL or missing references)
		//IL_01ba: Unknown result type (might be due to invalid IL or missing references)
		//IL_01bf: Unknown result type (might be due to invalid IL or missing references)
		//IL_01d7: Unknown result type (might be due to invalid IL or missing references)
		//IL_01dc: Unknown result type (might be due to invalid IL or missing references)
		//IL_01eb: Unknown result type (might be due to invalid IL or missing references)
		//IL_01f5: Unknown result type (might be due to invalid IL or missing references)
		//IL_026d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0273: Unknown result type (might be due to invalid IL or missing references)
		//IL_027d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0282: Unknown result type (might be due to invalid IL or missing references)
		localTime += deltaTime;
		PushSnapshot();
		if (cvLocalNavigatorDebugDraw.value)
		{
			Debug.DrawLine(currentSnapshot.position, targetPosition, Color32.op_Implicit(new Color32(byte.MaxValue, (byte)127, (byte)39, (byte)127)), deltaTime);
		}
		wasObstructedLastUpdate = false;
		jumpSpeed = 0f;
		previousMoveVector = moveVector;
		Vector3 val = targetPosition - currentSnapshot.position;
		if (!currentSnapshot.isFlying)
		{
			val.y = 0f;
		}
		float magnitude = ((Vector3)(ref val)).magnitude;
		Vector3 positionToTargetNormalized = val / magnitude;
		Vector3 val2 = moveVector;
		CompensateForVelocityByRefinement(deltaTime, ref val2, in positionToTargetNormalized);
		moveVector = val2;
		if (magnitude == 0f)
		{
			moveVector = Vector3.zero;
		}
		else
		{
			raycastResults = GetRaycasts(in currentSnapshot, moveVector, deltaTime);
			if (raycastResults.forwardObstructed)
			{
				int num = ((!raycastResults.leftObstructed) ? (-1) : 0) + ((!raycastResults.rightObstructed) ? 1 : 0);
				if (num == 0)
				{
					num = ((Random.Range(0, 1) != 1) ? 1 : (-1));
				}
				moveVector = Quaternion.Euler(0f, (0f - avoidanceAngle) * (float)num, 0f) * moveVector;
				wasObstructedLastUpdate = raycastResults.leftObstructed || raycastResults.rightObstructed;
			}
			float deltaTime2 = deltaTime + 0.4f;
			ref BodySnapshot reference = ref currentSnapshot;
			ref SnapshotDelta reference2 = ref currentSnapshotDelta;
			Vector3 currentMoveVector = moveVector;
			if (CheckCliffAhead(in reference, in reference2, in currentMoveVector, deltaTime2))
			{
				wasObstructedLastUpdate = true;
				moveVector = -moveVector;
				ref BodySnapshot reference3 = ref currentSnapshot;
				ref SnapshotDelta reference4 = ref currentSnapshotDelta;
				currentMoveVector = moveVector;
				if (CheckCliffAhead(in reference3, in reference4, in currentMoveVector, deltaTime2))
				{
					moveVector *= 0.25f;
				}
			}
			CalculateFrustration(deltaTime, ref walkFrustration);
			if (walkFrustration >= 1f)
			{
				jumpSpeed = currentSnapshot.maxJumpSpeed;
			}
		}
		avoidanceTimer -= deltaTime;
		walkFrustration = Mathf.Clamp(walkFrustration - deltaTime * 0.25f, 0f, 1f);
		if (cvLocalNavigatorDebugDraw.value)
		{
			Debug.DrawRay(currentSnapshot.position, moveVector * 5f, Color.green, deltaTime, false);
		}
	}

	private void CompensateForVelocityByRefinement(float nextExpectedDeltaTime, ref Vector3 moveVector, in Vector3 positionToTargetNormalized)
	{
		//IL_0006: Unknown result type (might be due to invalid IL or missing references)
		//IL_0018: Unknown result type (might be due to invalid IL or missing references)
		//IL_0024: Unknown result type (might be due to invalid IL or missing references)
		//IL_0029: Unknown result type (might be due to invalid IL or missing references)
		//IL_002a: Unknown result type (might be due to invalid IL or missing references)
		//IL_002b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0056: Unknown result type (might be due to invalid IL or missing references)
		//IL_005b: Unknown result type (might be due to invalid IL or missing references)
		//IL_005d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0062: Unknown result type (might be due to invalid IL or missing references)
		//IL_007f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0084: Unknown result type (might be due to invalid IL or missing references)
		//IL_0086: Unknown result type (might be due to invalid IL or missing references)
		//IL_0088: Unknown result type (might be due to invalid IL or missing references)
		//IL_0089: Unknown result type (might be due to invalid IL or missing references)
		//IL_008b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0095: Unknown result type (might be due to invalid IL or missing references)
		//IL_0097: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a1: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a2: Unknown result type (might be due to invalid IL or missing references)
		Vector3 estimatedVelocity = currentSnapshotDelta.estimatedVelocity;
		_ = currentSnapshot.acceleration;
		float num = Vector3.Dot(estimatedVelocity, positionToTargetNormalized);
		Vector3 val = positionToTargetNormalized;
		Vector3 val2 = val;
		if (!currentSnapshot.isJumping)
		{
			for (int i = 0; i < 8; i++)
			{
				EstimateNextMovement(in currentSnapshot, in currentSnapshotDelta, in val2, nextExpectedDeltaTime, out var nextPosition, out var nextVelocity);
				Vector3 val3 = targetPosition - nextPosition;
				if (!currentSnapshot.isFlying)
				{
					val3.y = 0f;
				}
				Vector3 normalized = ((Vector3)(ref val3)).normalized;
				val2 = normalized;
				if (Vector3.Dot(nextVelocity, normalized) > num)
				{
					val = normalized;
				}
			}
		}
		moveVector = val;
	}

	private void CompensateForVelocityByBruteForce(float nextExpectedDeltaTime, ref Vector3 moveVector, in Vector3 positionToTargetNormalized)
	{
		//IL_0006: Unknown result type (might be due to invalid IL or missing references)
		//IL_000b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0015: Unknown result type (might be due to invalid IL or missing references)
		//IL_001b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0027: Unknown result type (might be due to invalid IL or missing references)
		//IL_002c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0045: Unknown result type (might be due to invalid IL or missing references)
		//IL_004a: Unknown result type (might be due to invalid IL or missing references)
		//IL_004f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0054: Unknown result type (might be due to invalid IL or missing references)
		//IL_0059: Unknown result type (might be due to invalid IL or missing references)
		//IL_0074: Unknown result type (might be due to invalid IL or missing references)
		//IL_0079: Unknown result type (might be due to invalid IL or missing references)
		//IL_007b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0080: Unknown result type (might be due to invalid IL or missing references)
		//IL_0084: Unknown result type (might be due to invalid IL or missing references)
		//IL_008b: Unknown result type (might be due to invalid IL or missing references)
		//IL_009f: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a1: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ae: Unknown result type (might be due to invalid IL or missing references)
		//IL_00af: Unknown result type (might be due to invalid IL or missing references)
		Vector3 estimatedVelocity = currentSnapshotDelta.estimatedVelocity;
		_ = currentSnapshot;
		float num = Vector3.Dot(((Vector3)(ref estimatedVelocity)).normalized, positionToTargetNormalized);
		Vector3 val = moveVector;
		int num2 = 16;
		float num3 = 360f / (float)num2;
		for (int i = 0; i < num2; i++)
		{
			Vector3 val2 = Quaternion.AngleAxis((float)i * num3, Vector3.up) * Vector3.forward;
			EstimateNextMovement(in currentSnapshot, in currentSnapshotDelta, in val2, nextExpectedDeltaTime, out var nextPosition, out var nextVelocity);
			Vector3 val3 = targetPosition - nextPosition;
			float num4 = Vector3.Dot(((Vector3)(ref val3)).normalized, ((Vector3)(ref nextVelocity)).normalized);
			if (num4 > num)
			{
				num = num4;
				val = val2;
			}
		}
		moveVector = val;
	}

	private static void EstimateNextMovement(in BodySnapshot currentSnapshot, in SnapshotDelta currentSnapshotDelta, in Vector3 moveVector, float nextDeltaTime, out Vector3 nextPosition, out Vector3 nextVelocity)
	{
		//IL_0001: Unknown result type (might be due to invalid IL or missing references)
		//IL_0006: Unknown result type (might be due to invalid IL or missing references)
		//IL_0010: Unknown result type (might be due to invalid IL or missing references)
		//IL_0012: Unknown result type (might be due to invalid IL or missing references)
		//IL_001d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0025: Unknown result type (might be due to invalid IL or missing references)
		//IL_002a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0031: Unknown result type (might be due to invalid IL or missing references)
		//IL_0036: Unknown result type (might be due to invalid IL or missing references)
		//IL_0037: Unknown result type (might be due to invalid IL or missing references)
		//IL_003c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0054: Unknown result type (might be due to invalid IL or missing references)
		//IL_0059: Unknown result type (might be due to invalid IL or missing references)
		//IL_005a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0060: Unknown result type (might be due to invalid IL or missing references)
		//IL_0065: Unknown result type (might be due to invalid IL or missing references)
		//IL_0069: Unknown result type (might be due to invalid IL or missing references)
		//IL_006f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0074: Unknown result type (might be due to invalid IL or missing references)
		//IL_007f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0084: Unknown result type (might be due to invalid IL or missing references)
		//IL_0089: Unknown result type (might be due to invalid IL or missing references)
		//IL_008d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0093: Unknown result type (might be due to invalid IL or missing references)
		//IL_0098: Unknown result type (might be due to invalid IL or missing references)
		//IL_009d: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a2: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a4: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a9: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ab: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b0: Unknown result type (might be due to invalid IL or missing references)
		Vector3 estimatedVelocity = currentSnapshotDelta.estimatedVelocity;
		float acceleration = currentSnapshot.acceleration;
		nextVelocity = Vector3.MoveTowards(estimatedVelocity, moveVector * currentSnapshot.maxMoveSpeed, acceleration * nextDeltaTime);
		Vector3 val = nextVelocity - estimatedVelocity;
		float num = Mathf.Min(((Vector3)(ref val)).magnitude / acceleration, nextDeltaTime);
		float num2 = nextDeltaTime - num;
		Vector3 val2 = (nextVelocity - estimatedVelocity) / num;
		Vector3 val3 = nextVelocity * num + val2 * (0.5f * num * num);
		Vector3 val4 = nextVelocity * num2;
		nextPosition = currentSnapshot.position + val3 + val4;
	}

	private RaycastResults GetRaycasts(in BodySnapshot currentSnapshot, Vector3 positionToTargetNormalized, float lookaheadTime)
	{
		//IL_0009: Unknown result type (might be due to invalid IL or missing references)
		//IL_000e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0018: Unknown result type (might be due to invalid IL or missing references)
		//IL_002b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0036: Unknown result type (might be due to invalid IL or missing references)
		//IL_003b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0056: Unknown result type (might be due to invalid IL or missing references)
		//IL_007d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0082: Unknown result type (might be due to invalid IL or missing references)
		//IL_0083: Unknown result type (might be due to invalid IL or missing references)
		//IL_0088: Unknown result type (might be due to invalid IL or missing references)
		//IL_008f: Unknown result type (might be due to invalid IL or missing references)
		//IL_00af: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b4: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b5: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ba: Unknown result type (might be due to invalid IL or missing references)
		//IL_00c1: Unknown result type (might be due to invalid IL or missing references)
		RaycastResults result = default(RaycastResults);
		Vector3 origin = currentSnapshot.chestPosition;
		LayerMask layerMask = LayerMask.op_Implicit(LayerMask.op_Implicit(LayerIndex.world.mask) | LayerMask.op_Implicit(LayerIndex.defaultLayer.mask));
		float maxDistance = currentSnapshot.bodyRadius + currentSnapshot.maxMoveSpeed * lookaheadTime;
		result.forwardObstructed = Raycast(in origin, in positionToTargetNormalized, out var hitInfo, maxDistance, layerMask);
		if (result.forwardObstructed)
		{
			Vector3 direction = Quaternion.Euler(0f, avoidanceAngle, 0f) * positionToTargetNormalized;
			result.leftObstructed = Raycast(in origin, in direction, out hitInfo, maxDistance, layerMask);
			direction = Quaternion.Euler(0f, 0f - avoidanceAngle, 0f) * positionToTargetNormalized;
			result.rightObstructed = Raycast(in origin, in direction, out hitInfo, maxDistance, layerMask);
		}
		return result;
	}

	private bool Raycast(in Vector3 origin, in Vector3 direction, out RaycastHit hitInfo, float maxDistance, LayerMask layerMask)
	{
		//IL_0001: Unknown result type (might be due to invalid IL or missing references)
		//IL_0007: Unknown result type (might be due to invalid IL or missing references)
		//IL_000f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0029: Unknown result type (might be due to invalid IL or missing references)
		//IL_002f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0034: Unknown result type (might be due to invalid IL or missing references)
		//IL_0037: Unknown result type (might be due to invalid IL or missing references)
		//IL_003e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0043: Unknown result type (might be due to invalid IL or missing references)
		//IL_0056: Unknown result type (might be due to invalid IL or missing references)
		//IL_0060: Unknown result type (might be due to invalid IL or missing references)
		bool flag = Physics.Raycast(origin, direction, ref hitInfo, maxDistance, LayerMask.op_Implicit(layerMask));
		if (cvLocalNavigatorDebugDraw.value)
		{
			Vector3 val = origin;
			Vector3 val2 = direction;
			Debug.DrawRay(val, ((Vector3)(ref val2)).normalized * maxDistance, Color.yellow, raycastUpdateInterval);
			if (flag)
			{
				Util.DebugCross(((RaycastHit)(ref hitInfo)).point, 1f, Color.red, raycastUpdateInterval);
			}
		}
		return flag;
	}

	private bool Linecast(in Vector3 start, in Vector3 end, out RaycastHit hitInfo, LayerMask layerMask)
	{
		//IL_0001: Unknown result type (might be due to invalid IL or missing references)
		//IL_0007: Unknown result type (might be due to invalid IL or missing references)
		//IL_000d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0027: Unknown result type (might be due to invalid IL or missing references)
		//IL_002d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0032: Unknown result type (might be due to invalid IL or missing references)
		//IL_0045: Unknown result type (might be due to invalid IL or missing references)
		//IL_004f: Unknown result type (might be due to invalid IL or missing references)
		bool flag = Physics.Linecast(start, end, ref hitInfo, LayerMask.op_Implicit(layerMask));
		if (cvLocalNavigatorDebugDraw.value)
		{
			Debug.DrawLine(start, end, Color.yellow, raycastUpdateInterval);
			if (flag)
			{
				Util.DebugCross(((RaycastHit)(ref hitInfo)).point, 1f, Color.red, raycastUpdateInterval);
			}
		}
		return flag;
	}

	private bool CheckCliffAhead(in BodySnapshot currentSnapshot, in SnapshotDelta currentSnapshotDelta, in Vector3 currentMoveVector, float deltaTime)
	{
		//IL_0054: Unknown result type (might be due to invalid IL or missing references)
		//IL_0055: Unknown result type (might be due to invalid IL or missing references)
		//IL_0063: Unknown result type (might be due to invalid IL or missing references)
		//IL_0065: Unknown result type (might be due to invalid IL or missing references)
		//IL_0093: Unknown result type (might be due to invalid IL or missing references)
		if (!allowWalkOffCliff && currentSnapshot.isGrounded)
		{
			EstimateNextMovement(in currentSnapshot, in currentSnapshotDelta, in currentMoveVector, deltaTime, out var nextPosition, out var _);
			float num = currentSnapshot.chestPosition.y - currentSnapshot.position.y;
			float num2 = currentSnapshot.footPosition.y - currentSnapshot.position.y;
			Vector3 start = nextPosition;
			start.y += num;
			Vector3 end = start;
			end.y += num2;
			end.y -= 4f;
			RaycastHit hitInfo;
			return !Linecast(in start, in end, out hitInfo, LayerIndex.world.mask);
		}
		return false;
	}

	private void CalculateFrustration(float deltaTime, ref float frustration)
	{
		//IL_0051: Unknown result type (might be due to invalid IL or missing references)
		//IL_0056: Unknown result type (might be due to invalid IL or missing references)
		//IL_005b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0062: Unknown result type (might be due to invalid IL or missing references)
		//IL_0067: Unknown result type (might be due to invalid IL or missing references)
		//IL_006c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0073: Unknown result type (might be due to invalid IL or missing references)
		//IL_0078: Unknown result type (might be due to invalid IL or missing references)
		//IL_007d: Unknown result type (might be due to invalid IL or missing references)
		//IL_007e: Unknown result type (might be due to invalid IL or missing references)
		//IL_007f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0087: Unknown result type (might be due to invalid IL or missing references)
		//IL_0088: Unknown result type (might be due to invalid IL or missing references)
		//IL_0089: Unknown result type (might be due to invalid IL or missing references)
		//IL_008e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0095: Unknown result type (might be due to invalid IL or missing references)
		//IL_009b: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a0: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a6: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ab: Unknown result type (might be due to invalid IL or missing references)
		//IL_011e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0123: Unknown result type (might be due to invalid IL or missing references)
		//IL_0147: Unknown result type (might be due to invalid IL or missing references)
		//IL_0152: Unknown result type (might be due to invalid IL or missing references)
		//IL_0157: Unknown result type (might be due to invalid IL or missing references)
		//IL_015c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0191: Unknown result type (might be due to invalid IL or missing references)
		//IL_0196: Unknown result type (might be due to invalid IL or missing references)
		if (!currentSnapshot.canJump)
		{
			frustration = 0f;
		}
		else
		{
			if (!currentSnapshot.isValid || !currentSnapshotDelta.isValid || !currentSnapshotDelta.isValid || !previousSnapshotDelta.isValid)
			{
				return;
			}
			if (true)
			{
				Vector3 val = FlattenDirection(moveVector);
				Vector3 val2 = FlattenDirection(currentSnapshotDelta.estimatedVelocity);
				Vector3 val3 = FlattenDirection(previousSnapshotDelta.estimatedVelocity);
				float num = Vector3.Dot(val2, val);
				velocityDelta = val2 - val3;
				estimatedAcceleration = velocityDelta / deltaTime;
				float num2 = Vector3.Dot(estimatedAcceleration, val);
				float num3 = ((Vector3)(ref estimatedAcceleration)).magnitude - num2;
				speedAsFractionOfTopSpeed = num / currentSnapshot.maxMoveSpeed;
				isAlreadyMovingAtSufficientSpeed = speedAsFractionOfTopSpeed >= 0.45f;
				if (isAlreadyMovingAtSufficientSpeed)
				{
					frustration = 0f;
				}
				else if (num3 > num2 * 2f)
				{
					frustration += 1.25f * deltaTime;
				}
				return;
			}
			Vector3 val4 = currentSnapshot.motorVelocity;
			float num4 = ((Vector3)(ref val4)).magnitude + ((Vector3)(ref bodyComponents.motor.rootMotion)).magnitude / deltaTime;
			val4 = targetPosition - currentSnapshot.position;
			float magnitude = ((Vector3)(ref val4)).magnitude;
			if (currentSnapshot.maxMoveSpeed != 0f && num4 != 0f && magnitude > Mathf.Epsilon)
			{
				Vector3 estimatedVelocity = currentSnapshotDelta.estimatedVelocity;
				float magnitude2 = ((Vector3)(ref estimatedVelocity)).magnitude;
				if (magnitude2 <= num4 || magnitude2 <= 0.1f)
				{
					frustration += 1.25f * deltaTime;
				}
			}
		}
		static Vector3 FlattenDirection(Vector3 vector)
		{
			//IL_0000: Unknown result type (might be due to invalid IL or missing references)
			//IL_001e: Unknown result type (might be due to invalid IL or missing references)
			if (Mathf.Abs(vector.y) > 0f)
			{
				vector.y = 0f;
			}
			return vector;
		}
	}
}
