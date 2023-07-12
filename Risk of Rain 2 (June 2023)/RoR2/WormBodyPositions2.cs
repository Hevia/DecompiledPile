using System;
using System.Collections.Generic;
using EntityStates.MagmaWorm;
using RoR2.Projectile;
using Unity;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.Networking;

namespace RoR2;

[RequireComponent(typeof(CharacterBody))]
public class WormBodyPositions2 : NetworkBehaviour, ITeleportHandler, IEventSystemHandler, ILifeBehavior, IPainAnimationHandler
{
	private struct PositionRotationPair
	{
		public Vector3 position;

		public Quaternion rotation;
	}

	[Serializable]
	public struct KeyFrame
	{
		public CubicBezier3 curve;

		public float length;

		public float time;
	}

	public delegate void BurrowExpectedCallback(float expectedTime, Vector3 hitPosition, Vector3 hitNormal);

	public delegate void BreachExpectedCallback(float expectedTime, Vector3 hitPosition, Vector3 hitNormal);

	public struct TravelCallback
	{
		public float time;

		public Action callback;
	}

	public Transform[] bones;

	public float[] segmentLengths;

	[Tooltip("How far behind the chaser the head is, in seconds.")]
	public float followDelay = 2f;

	[Tooltip("Whether or not the jaw will close/open.")]
	public bool animateJaws;

	public Animator animator;

	public string jawMecanimCycleParameter;

	public float jawMecanimDampTime;

	public float jawClosedDistance;

	public float jawOpenDistance;

	public GameObject warningEffectPrefab;

	public GameObject burrowEffectPrefab;

	public float maxPainDisplacementMagnitude = 2f;

	public float painDisplacementRecoverySpeed = 8f;

	public bool shouldFireMeatballsOnImpact = true;

	public bool shouldFireBlastAttackOnImpact = true;

	public bool enableSurfaceTests = true;

	public float undergroundTestYOffset;

	private CharacterBody characterBody;

	private CharacterDirection characterDirection;

	private PositionRotationPair[] boneTransformationBuffer;

	private Vector3[] boneDisplacements;

	private float headDistance;

	private float totalLength;

	private const float endBonusLength = 4f;

	private const float fakeEndSegmentLength = 2f;

	private readonly List<KeyFrame> keyFrames = new List<KeyFrame>();

	private float keyFramesTotalLength;

	public float spawnDepth = 30f;

	private Collider entranceCollider;

	private Collider exitCollider;

	private Vector3 previousSurfaceTestEnd;

	private List<TravelCallback> travelCallbacks;

	private const float impactSoundPrestartDuration = 0.5f;

	public float impactCooldownDuration = 0.1f;

	private float enterTriggerCooldownTimer;

	private float exitTriggerCooldownTimer;

	private bool shouldTriggerDeathEffectOnNextImpact;

	private float deathTime = float.NegativeInfinity;

	public GameObject meatballProjectile;

	public GameObject blastAttackEffect;

	public int meatballCount;

	public float meatballAngle;

	public float meatballDamageCoefficient;

	public float meatballProcCoefficient;

	public float meatballForce;

	public float blastAttackDamageCoefficient;

	public float blastAttackProcCoefficient;

	public float blastAttackRadius;

	public float blastAttackForce;

	public float blastAttackBonusVerticalForce;

	public float speedMultiplier = 2f;

	private bool _playingBurrowSound;

	private static int kRpcRpcSendKeyFrame;

	private static int kRpcRpcPlaySurfaceImpactSound;

	public KeyFrame newestKeyFrame { get; private set; }

	public bool underground { get; private set; }

	private bool playingBurrowSound
	{
		get
		{
			return _playingBurrowSound;
		}
		set
		{
			if (value != _playingBurrowSound)
			{
				_playingBurrowSound = value;
				Util.PlaySound(value ? "Play_magmaWorm_burrowed_loop" : "Stop_magmaWorm_burrowed_loop", ((Component)bones[0]).gameObject);
			}
		}
	}

	public event BurrowExpectedCallback onPredictedBurrowDiscovered;

	public event BreachExpectedCallback onPredictedBreachDiscovered;

	private void Awake()
	{
		characterBody = ((Component)this).GetComponent<CharacterBody>();
		characterDirection = ((Component)this).GetComponent<CharacterDirection>();
		boneTransformationBuffer = new PositionRotationPair[bones.Length + 1];
		totalLength = 0f;
		for (int i = 0; i < segmentLengths.Length; i++)
		{
			totalLength += segmentLengths[i];
		}
		if (NetworkServer.active)
		{
			travelCallbacks = new List<TravelCallback>();
		}
		boneDisplacements = (Vector3[])(object)new Vector3[bones.Length];
	}

	private void Start()
	{
		//IL_0021: Unknown result type (might be due to invalid IL or missing references)
		//IL_0026: Unknown result type (might be due to invalid IL or missing references)
		if (NetworkServer.active)
		{
			PopulateInitialKeyFrames();
			previousSurfaceTestEnd = newestKeyFrame.curve.Evaluate(1f);
		}
	}

	private void OnDestroy()
	{
		travelCallbacks = null;
		_playingBurrowSound = false;
	}

	private void BakeSegmentLengths()
	{
		//IL_001b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0020: Unknown result type (might be due to invalid IL or missing references)
		//IL_002d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0032: Unknown result type (might be due to invalid IL or missing references)
		//IL_0033: Unknown result type (might be due to invalid IL or missing references)
		//IL_0034: Unknown result type (might be due to invalid IL or missing references)
		//IL_0035: Unknown result type (might be due to invalid IL or missing references)
		//IL_003a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0050: Unknown result type (might be due to invalid IL or missing references)
		//IL_0051: Unknown result type (might be due to invalid IL or missing references)
		segmentLengths = new float[bones.Length];
		Vector3 val = bones[0].position;
		for (int i = 1; i < bones.Length; i++)
		{
			Vector3 position = bones[i].position;
			Vector3 val2 = val - position;
			float magnitude = ((Vector3)(ref val2)).magnitude;
			segmentLengths[i - 1] = magnitude;
			val = position;
		}
		segmentLengths[bones.Length - 1] = 2f;
	}

	[Server]
	private void PopulateInitialKeyFrames()
	{
		//IL_0029: Unknown result type (might be due to invalid IL or missing references)
		//IL_002e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0039: Unknown result type (might be due to invalid IL or missing references)
		//IL_003e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0043: Unknown result type (might be due to invalid IL or missing references)
		//IL_0056: Unknown result type (might be due to invalid IL or missing references)
		//IL_0057: Unknown result type (might be due to invalid IL or missing references)
		//IL_0061: Unknown result type (might be due to invalid IL or missing references)
		//IL_0066: Unknown result type (might be due to invalid IL or missing references)
		//IL_006b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0070: Unknown result type (might be due to invalid IL or missing references)
		//IL_0071: Unknown result type (might be due to invalid IL or missing references)
		//IL_007b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0080: Unknown result type (might be due to invalid IL or missing references)
		//IL_0085: Unknown result type (might be due to invalid IL or missing references)
		//IL_00bc: Unknown result type (might be due to invalid IL or missing references)
		//IL_00bd: Unknown result type (might be due to invalid IL or missing references)
		//IL_00c2: Unknown result type (might be due to invalid IL or missing references)
		//IL_00c7: Unknown result type (might be due to invalid IL or missing references)
		//IL_00d3: Unknown result type (might be due to invalid IL or missing references)
		//IL_00d4: Unknown result type (might be due to invalid IL or missing references)
		if (!NetworkServer.active)
		{
			Debug.LogWarning((object)"[Server] function 'System.Void RoR2.WormBodyPositions2::PopulateInitialKeyFrames()' called on client");
			return;
		}
		bool flag = enableSurfaceTests;
		enableSurfaceTests = false;
		Vector3 val = ((Component)this).transform.position + Vector3.down * spawnDepth;
		float synchronizedTimeStamp = GetSynchronizedTimeStamp();
		newestKeyFrame = new KeyFrame
		{
			curve = CubicBezier3.FromVelocities(val + Vector3.down * 2f, Vector3.up, val + Vector3.down * 1f, Vector3.down),
			time = synchronizedTimeStamp - 2f,
			length = 1f
		};
		AttemptToGenerateKeyFrame(synchronizedTimeStamp - 1f, val + Vector3.down, Vector3.up);
		AttemptToGenerateKeyFrame(synchronizedTimeStamp, val, Vector3.up);
		headDistance = 0f;
		enableSurfaceTests = flag;
	}

	private Vector3 EvaluatePositionAlongCurve(float positionDownBody)
	{
		//IL_0041: Unknown result type (might be due to invalid IL or missing references)
		//IL_0046: Unknown result type (might be due to invalid IL or missing references)
		//IL_0097: Unknown result type (might be due to invalid IL or missing references)
		//IL_009d: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a3: Unknown result type (might be due to invalid IL or missing references)
		float num = 0f;
		foreach (KeyFrame keyFrame in keyFrames)
		{
			float num2 = num;
			num += keyFrame.length;
			if (num >= positionDownBody)
			{
				float t = Mathf.InverseLerp(num, num2, positionDownBody);
				CubicBezier3 curve = keyFrame.curve;
				return curve.Evaluate(t);
			}
		}
		if (keyFrames.Count > 0)
		{
			return keyFrames[keyFrames.Count - 1].curve.Evaluate(1f);
		}
		return Vector3.zero;
	}

	private void UpdateBones()
	{
		//IL_002a: Unknown result type (might be due to invalid IL or missing references)
		//IL_002f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0036: Unknown result type (might be due to invalid IL or missing references)
		//IL_003b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0067: Unknown result type (might be due to invalid IL or missing references)
		//IL_006c: Unknown result type (might be due to invalid IL or missing references)
		//IL_006d: Unknown result type (might be due to invalid IL or missing references)
		//IL_007c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0081: Unknown result type (might be due to invalid IL or missing references)
		//IL_0086: Unknown result type (might be due to invalid IL or missing references)
		//IL_008b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0090: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a3: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a4: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ab: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ad: Unknown result type (might be due to invalid IL or missing references)
		//IL_00e6: Unknown result type (might be due to invalid IL or missing references)
		//IL_00eb: Unknown result type (might be due to invalid IL or missing references)
		//IL_0115: Unknown result type (might be due to invalid IL or missing references)
		//IL_0122: Unknown result type (might be due to invalid IL or missing references)
		//IL_0127: Unknown result type (might be due to invalid IL or missing references)
		//IL_0133: Unknown result type (might be due to invalid IL or missing references)
		//IL_0149: Unknown result type (might be due to invalid IL or missing references)
		//IL_014e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0153: Unknown result type (might be due to invalid IL or missing references)
		//IL_0158: Unknown result type (might be due to invalid IL or missing references)
		//IL_0164: Unknown result type (might be due to invalid IL or missing references)
		//IL_0169: Unknown result type (might be due to invalid IL or missing references)
		float num = totalLength;
		boneTransformationBuffer[boneTransformationBuffer.Length - 1] = new PositionRotationPair
		{
			position = EvaluatePositionAlongCurve(headDistance + num),
			rotation = Quaternion.identity
		};
		for (int num2 = boneTransformationBuffer.Length - 2; num2 >= 0; num2--)
		{
			num -= segmentLengths[num2];
			Vector3 val = EvaluatePositionAlongCurve(headDistance + num);
			Quaternion rotation = Util.QuaternionSafeLookRotation(val - boneTransformationBuffer[num2 + 1].position, Vector3.up);
			boneTransformationBuffer[num2] = new PositionRotationPair
			{
				position = val,
				rotation = rotation
			};
		}
		if (bones.Length == 0 || !Object.op_Implicit((Object)(object)bones[0]))
		{
			return;
		}
		Vector3 forward = bones[0].forward;
		for (int i = 0; i < bones.Length; i++)
		{
			Transform val2 = bones[i];
			if (Object.op_Implicit((Object)(object)val2))
			{
				val2.position = boneTransformationBuffer[i].position + boneDisplacements[i];
				val2.forward = forward;
				val2.up = boneTransformationBuffer[i].rotation * -Vector3.forward;
				forward = val2.forward;
			}
		}
	}

	public void AddKeyFrame(in KeyFrame newKeyFrame)
	{
		//IL_0126: Unknown result type (might be due to invalid IL or missing references)
		newestKeyFrame = newKeyFrame;
		keyFrames.Insert(0, newKeyFrame);
		keyFramesTotalLength += newKeyFrame.length;
		headDistance += newKeyFrame.length;
		bool flag = false;
		float num = keyFramesTotalLength;
		float num2 = totalLength + headDistance + 4f;
		while (keyFrames.Count > 0 && (num -= keyFrames[keyFrames.Count - 1].length) > num2)
		{
			keyFrames.RemoveAt(keyFrames.Count - 1);
			flag = true;
		}
		if (flag)
		{
			keyFramesTotalLength = 0f;
			foreach (KeyFrame keyFrame in keyFrames)
			{
				keyFramesTotalLength += keyFrame.length;
			}
		}
		if (NetworkServer.active)
		{
			CallRpcSendKeyFrame(newKeyFrame);
			if (enableSurfaceTests)
			{
				SurfaceTest(newKeyFrame.curve.p1, ref previousSurfaceTestEnd, newKeyFrame.time, OnPredictedBurrowDiscovered, OnPredictedBreachDiscovered);
			}
		}
	}

	[Server]
	public void AttemptToGenerateKeyFrame(float arrivalTime, Vector3 position, Vector3 velocity)
	{
		//IL_002c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0038: Unknown result type (might be due to invalid IL or missing references)
		//IL_003d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0042: Unknown result type (might be due to invalid IL or missing references)
		//IL_0043: Unknown result type (might be due to invalid IL or missing references)
		//IL_0044: Unknown result type (might be due to invalid IL or missing references)
		//IL_0050: Unknown result type (might be due to invalid IL or missing references)
		if (!NetworkServer.active)
		{
			Debug.LogWarning((object)"[Server] function 'System.Void RoR2.WormBodyPositions2::AttemptToGenerateKeyFrame(System.Single,UnityEngine.Vector3,UnityEngine.Vector3)' called on client");
			return;
		}
		KeyFrame keyFrame = newestKeyFrame;
		float num = arrivalTime - keyFrame.time;
		CubicBezier3 curve = CubicBezier3.FromVelocities(keyFrame.curve.p1, -keyFrame.curve.v1, position, -velocity * (num * 0.25f));
		float length = curve.ApproximateLength(50);
		KeyFrame keyFrame2 = default(KeyFrame);
		keyFrame2.curve = curve;
		keyFrame2.length = length;
		keyFrame2.time = arrivalTime;
		KeyFrame newKeyFrame = keyFrame2;
		if (newKeyFrame.length >= 0f)
		{
			AddKeyFrame(in newKeyFrame);
		}
	}

	[ClientRpc]
	private void RpcSendKeyFrame(KeyFrame newKeyFrame)
	{
		if (!NetworkServer.active)
		{
			AddKeyFrame(in newKeyFrame);
		}
	}

	private void Update()
	{
		UpdateBoneDisplacements(Time.deltaTime);
		UpdateHeadOffset();
		if (animateJaws)
		{
			UpdateJaws();
		}
	}

	private void UpdateJaws()
	{
		//IL_0015: Unknown result type (might be due to invalid IL or missing references)
		//IL_0020: Unknown result type (might be due to invalid IL or missing references)
		//IL_0025: Unknown result type (might be due to invalid IL or missing references)
		//IL_002a: Unknown result type (might be due to invalid IL or missing references)
		if (Object.op_Implicit((Object)(object)animator))
		{
			Vector3 val = bones[0].position - ((Component)this).transform.position;
			float num = Mathf.Clamp01(Util.Remap(((Vector3)(ref val)).magnitude, jawClosedDistance, jawOpenDistance, 0f, 1f));
			animator.SetFloat(jawMecanimCycleParameter, num, jawMecanimDampTime, Time.deltaTime);
		}
	}

	private void UpdateHeadOffset()
	{
		float num = headDistance;
		int num2 = keyFrames.Count - 1;
		float num3 = 0f;
		float synchronizedTimeStamp = GetSynchronizedTimeStamp();
		for (int i = 0; i < num2; i++)
		{
			float time = keyFrames[i + 1].time;
			float length = keyFrames[i].length;
			if (time < synchronizedTimeStamp)
			{
				num = num3 + length * Mathf.InverseLerp(keyFrames[i].time, time, synchronizedTimeStamp);
				break;
			}
			num3 += length;
		}
		OnTravel(headDistance - num);
	}

	private void OnTravel(float distance)
	{
		headDistance -= distance;
		UpdateBones();
	}

	private void OnPredictedBurrowDiscovered(float expectedTime, Vector3 point, Vector3 surfaceNormal)
	{
		//IL_000e: Unknown result type (might be due to invalid IL or missing references)
		//IL_000f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0015: Unknown result type (might be due to invalid IL or missing references)
		//IL_0016: Unknown result type (might be due to invalid IL or missing references)
		//IL_0082: Unknown result type (might be due to invalid IL or missing references)
		//IL_0088: Unknown result type (might be due to invalid IL or missing references)
		AddTravelCallback(new TravelCallback
		{
			time = expectedTime,
			callback = delegate
			{
				//IL_0007: Unknown result type (might be due to invalid IL or missing references)
				//IL_000d: Unknown result type (might be due to invalid IL or missing references)
				OnEnterSurface(point, surfaceNormal);
			}
		});
		AddTravelCallback(new TravelCallback
		{
			time = expectedTime - 0.5f,
			callback = RpcPlaySurfaceImpactSound
		});
		this.onPredictedBurrowDiscovered?.Invoke(expectedTime, point, surfaceNormal);
	}

	private void OnPredictedBreachDiscovered(float expectedTime, Vector3 point, Vector3 surfaceNormal)
	{
		//IL_000e: Unknown result type (might be due to invalid IL or missing references)
		//IL_000f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0015: Unknown result type (might be due to invalid IL or missing references)
		//IL_0016: Unknown result type (might be due to invalid IL or missing references)
		//IL_0035: Unknown result type (might be due to invalid IL or missing references)
		//IL_0041: Unknown result type (might be due to invalid IL or missing references)
		//IL_0046: Unknown result type (might be due to invalid IL or missing references)
		//IL_004b: Unknown result type (might be due to invalid IL or missing references)
		//IL_00bd: Unknown result type (might be due to invalid IL or missing references)
		//IL_00c3: Unknown result type (might be due to invalid IL or missing references)
		if (Object.op_Implicit((Object)(object)warningEffectPrefab))
		{
			EffectManager.SpawnEffect(warningEffectPrefab, new EffectData
			{
				origin = point,
				rotation = Util.QuaternionSafeLookRotation(surfaceNormal)
			}, transmit: true);
		}
		AddTravelCallback(new TravelCallback
		{
			time = expectedTime,
			callback = delegate
			{
				//IL_0007: Unknown result type (might be due to invalid IL or missing references)
				//IL_000d: Unknown result type (might be due to invalid IL or missing references)
				OnExitSurface(point, surfaceNormal);
			}
		});
		AddTravelCallback(new TravelCallback
		{
			time = expectedTime - 0.5f,
			callback = RpcPlaySurfaceImpactSound
		});
		this.onPredictedBreachDiscovered?.Invoke(expectedTime, point, surfaceNormal);
	}

	[Server]
	private static void SurfaceTest(Vector3 currentPosition, ref Vector3 previousPosition, float arrivalTime, BurrowExpectedCallback onPredictedBurrowDiscovered, BreachExpectedCallback onPredictedBreachDiscovered)
	{
		//IL_0015: Unknown result type (might be due to invalid IL or missing references)
		//IL_0017: Unknown result type (might be due to invalid IL or missing references)
		//IL_001c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0021: Unknown result type (might be due to invalid IL or missing references)
		//IL_002b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0030: Unknown result type (might be due to invalid IL or missing references)
		//IL_003c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0055: Unknown result type (might be due to invalid IL or missing references)
		//IL_005c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0066: Unknown result type (might be due to invalid IL or missing references)
		//IL_0067: Unknown result type (might be due to invalid IL or missing references)
		//IL_0068: Unknown result type (might be due to invalid IL or missing references)
		//IL_0078: Unknown result type (might be due to invalid IL or missing references)
		//IL_0093: Unknown result type (might be due to invalid IL or missing references)
		//IL_009a: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a5: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a6: Unknown result type (might be due to invalid IL or missing references)
		if (!NetworkServer.active)
		{
			Debug.LogWarning((object)"[Server] function 'System.Void RoR2.WormBodyPositions2::SurfaceTest(UnityEngine.Vector3,UnityEngine.Vector3&,System.Single,RoR2.WormBodyPositions2/BurrowExpectedCallback,RoR2.WormBodyPositions2/BreachExpectedCallback)' called on client");
			return;
		}
		Vector3 val = currentPosition - previousPosition;
		float magnitude = ((Vector3)(ref val)).magnitude;
		RaycastHit val2 = default(RaycastHit);
		if (Physics.Raycast(previousPosition, val, ref val2, magnitude, LayerMask.op_Implicit(LayerIndex.world.mask), (QueryTriggerInteraction)1))
		{
			onPredictedBurrowDiscovered?.Invoke(arrivalTime, ((RaycastHit)(ref val2)).point, ((RaycastHit)(ref val2)).normal);
		}
		if (Physics.Raycast(currentPosition, -val, ref val2, magnitude, LayerMask.op_Implicit(LayerIndex.world.mask), (QueryTriggerInteraction)1))
		{
			onPredictedBreachDiscovered?.Invoke(arrivalTime, ((RaycastHit)(ref val2)).point, ((RaycastHit)(ref val2)).normal);
		}
		previousPosition = currentPosition;
	}

	public void AddTravelCallback(TravelCallback newTravelCallback)
	{
		int index = travelCallbacks.Count;
		float time = newTravelCallback.time;
		for (int i = 0; i < travelCallbacks.Count; i++)
		{
			if (time < travelCallbacks[i].time)
			{
				index = i;
				break;
			}
		}
		travelCallbacks.Insert(index, newTravelCallback);
	}

	[ClientRpc]
	private void RpcPlaySurfaceImpactSound()
	{
	}

	[Server]
	private void OnEnterSurface(Vector3 point, Vector3 surfaceNormal)
	{
		//IL_0069: Unknown result type (might be due to invalid IL or missing references)
		//IL_0070: Unknown result type (might be due to invalid IL or missing references)
		//IL_0071: Unknown result type (might be due to invalid IL or missing references)
		//IL_0076: Unknown result type (might be due to invalid IL or missing references)
		//IL_0095: Unknown result type (might be due to invalid IL or missing references)
		//IL_0096: Unknown result type (might be due to invalid IL or missing references)
		//IL_0097: Unknown result type (might be due to invalid IL or missing references)
		//IL_009d: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a2: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ad: Unknown result type (might be due to invalid IL or missing references)
		//IL_00d2: Unknown result type (might be due to invalid IL or missing references)
		//IL_00d3: Unknown result type (might be due to invalid IL or missing references)
		//IL_00d4: Unknown result type (might be due to invalid IL or missing references)
		if (!NetworkServer.active)
		{
			Debug.LogWarning((object)"[Server] function 'System.Void RoR2.WormBodyPositions2::OnEnterSurface(UnityEngine.Vector3,UnityEngine.Vector3)' called on client");
		}
		else
		{
			if (enterTriggerCooldownTimer > 0f)
			{
				return;
			}
			if (shouldTriggerDeathEffectOnNextImpact && Run.instance.fixedTime - deathTime >= DeathState.duration - 3f)
			{
				shouldTriggerDeathEffectOnNextImpact = false;
				return;
			}
			enterTriggerCooldownTimer = impactCooldownDuration;
			EffectManager.SpawnEffect(burrowEffectPrefab, new EffectData
			{
				origin = point,
				rotation = Util.QuaternionSafeLookRotation(surfaceNormal),
				scale = 1f
			}, transmit: true);
			if (shouldFireMeatballsOnImpact)
			{
				FireMeatballs(surfaceNormal, point + surfaceNormal * 3f, characterDirection.forward, meatballCount, meatballAngle, meatballForce);
			}
			if (shouldFireBlastAttackOnImpact)
			{
				FireImpactBlastAttack(point + surfaceNormal);
			}
		}
	}

	public void OnDeathStart()
	{
		deathTime = Run.instance.fixedTime;
		shouldTriggerDeathEffectOnNextImpact = true;
	}

	[Server]
	private void PerformDeath()
	{
		//IL_0040: Unknown result type (might be due to invalid IL or missing references)
		//IL_004b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0050: Unknown result type (might be due to invalid IL or missing references)
		if (!NetworkServer.active)
		{
			Debug.LogWarning((object)"[Server] function 'System.Void RoR2.WormBodyPositions2::PerformDeath()' called on client");
			return;
		}
		for (int i = 0; i < bones.Length; i++)
		{
			if (Object.op_Implicit((Object)(object)bones[i]))
			{
				EffectManager.SpawnEffect(LegacyResourcesAPI.Load<GameObject>("Prefabs/Effects/MagmaWormDeathDust"), new EffectData
				{
					origin = bones[i].position,
					rotation = Random.rotation,
					scale = 1f
				}, transmit: true);
			}
		}
		Object.Destroy((Object)(object)((Component)this).gameObject);
	}

	[Server]
	private void OnExitSurface(Vector3 point, Vector3 surfaceNormal)
	{
		//IL_003b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0042: Unknown result type (might be due to invalid IL or missing references)
		//IL_0043: Unknown result type (might be due to invalid IL or missing references)
		//IL_0048: Unknown result type (might be due to invalid IL or missing references)
		//IL_0067: Unknown result type (might be due to invalid IL or missing references)
		//IL_0068: Unknown result type (might be due to invalid IL or missing references)
		//IL_0069: Unknown result type (might be due to invalid IL or missing references)
		//IL_006f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0074: Unknown result type (might be due to invalid IL or missing references)
		//IL_007f: Unknown result type (might be due to invalid IL or missing references)
		if (!NetworkServer.active)
		{
			Debug.LogWarning((object)"[Server] function 'System.Void RoR2.WormBodyPositions2::OnExitSurface(UnityEngine.Vector3,UnityEngine.Vector3)' called on client");
		}
		else if (!(exitTriggerCooldownTimer > 0f))
		{
			exitTriggerCooldownTimer = impactCooldownDuration;
			EffectManager.SpawnEffect(burrowEffectPrefab, new EffectData
			{
				origin = point,
				rotation = Util.QuaternionSafeLookRotation(surfaceNormal),
				scale = 1f
			}, transmit: true);
			if (shouldFireMeatballsOnImpact)
			{
				FireMeatballs(surfaceNormal, point + surfaceNormal * 3f, characterDirection.forward, meatballCount, meatballAngle, meatballForce);
			}
		}
	}

	private void FireMeatballs(Vector3 impactNormal, Vector3 impactPosition, Vector3 forward, int meatballCount, float meatballAngle, float meatballForce)
	{
		//IL_000a: Unknown result type (might be due to invalid IL or missing references)
		//IL_000b: Unknown result type (might be due to invalid IL or missing references)
		//IL_000c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0011: Unknown result type (might be due to invalid IL or missing references)
		//IL_0014: Unknown result type (might be due to invalid IL or missing references)
		//IL_0019: Unknown result type (might be due to invalid IL or missing references)
		//IL_001a: Unknown result type (might be due to invalid IL or missing references)
		//IL_001b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0029: Unknown result type (might be due to invalid IL or missing references)
		//IL_002e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0039: Unknown result type (might be due to invalid IL or missing references)
		//IL_003a: Unknown result type (might be due to invalid IL or missing references)
		//IL_003f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0040: Unknown result type (might be due to invalid IL or missing references)
		//IL_0045: Unknown result type (might be due to invalid IL or missing references)
		//IL_0052: Unknown result type (might be due to invalid IL or missing references)
		//IL_0053: Unknown result type (might be due to invalid IL or missing references)
		//IL_0055: Unknown result type (might be due to invalid IL or missing references)
		float num = 360f / (float)meatballCount;
		Vector3 val = Vector3.ProjectOnPlane(forward, impactNormal);
		Vector3 normalized = ((Vector3)(ref val)).normalized;
		Vector3 val2 = Vector3.RotateTowards(impactNormal, normalized, meatballAngle * (MathF.PI / 180f), float.PositiveInfinity);
		for (int i = 0; i < meatballCount; i++)
		{
			Vector3 forward2 = Quaternion.AngleAxis(num * (float)i, impactNormal) * val2;
			ProjectileManager.instance.FireProjectile(meatballProjectile, impactPosition, Util.QuaternionSafeLookRotation(forward2), ((Component)this).gameObject, characterBody.damage * meatballDamageCoefficient, meatballForce, Util.CheckRoll(characterBody.crit, characterBody.master));
		}
	}

	private void FireImpactBlastAttack(Vector3 impactPosition)
	{
		//IL_0036: Unknown result type (might be due to invalid IL or missing references)
		//IL_0041: Unknown result type (might be due to invalid IL or missing references)
		//IL_0046: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a8: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a9: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ce: Unknown result type (might be due to invalid IL or missing references)
		BlastAttack obj = new BlastAttack
		{
			baseDamage = characterBody.damage * blastAttackDamageCoefficient,
			procCoefficient = blastAttackProcCoefficient,
			baseForce = blastAttackForce,
			bonusForce = Vector3.up * blastAttackBonusVerticalForce,
			crit = Util.CheckRoll(characterBody.crit, characterBody.master),
			radius = blastAttackRadius,
			damageType = DamageType.IgniteOnHit,
			falloffModel = BlastAttack.FalloffModel.SweetSpot,
			attacker = ((Component)this).gameObject
		};
		obj.teamIndex = TeamComponent.GetObjectTeam(obj.attacker);
		obj.position = impactPosition;
		obj.attackerFiltering = AttackerFiltering.NeverHitSelf;
		obj.Fire();
		if (NetworkServer.active)
		{
			EffectManager.SpawnEffect(blastAttackEffect, new EffectData
			{
				origin = impactPosition,
				scale = blastAttackRadius
			}, transmit: true);
		}
	}

	private void FixedUpdate()
	{
		//IL_00be: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ce: Unknown result type (might be due to invalid IL or missing references)
		if (NetworkServer.active)
		{
			enterTriggerCooldownTimer -= Time.fixedDeltaTime;
			exitTriggerCooldownTimer -= Time.fixedDeltaTime;
			float synchronizedTimeStamp = GetSynchronizedTimeStamp();
			while (travelCallbacks.Count > 0 && travelCallbacks[0].time <= synchronizedTimeStamp)
			{
				TravelCallback travelCallback = travelCallbacks[0];
				travelCallbacks.RemoveAt(0);
				travelCallback.callback();
			}
		}
		if (bones.Length != 0 && Object.op_Implicit((Object)(object)bones[0]) && Object.op_Implicit((Object)(object)((Component)bones[0]).transform) && Object.op_Implicit((Object)(object)((Component)this).transform))
		{
			bool flag = ((Component)bones[0]).transform.position.y - ((Component)this).transform.position.y < undergroundTestYOffset;
			playingBurrowSound = flag;
		}
	}

	private void DrawKeyFrame(KeyFrame keyFrame)
	{
		//IL_0000: Unknown result type (might be due to invalid IL or missing references)
		//IL_0005: Unknown result type (might be due to invalid IL or missing references)
		//IL_000f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0020: Unknown result type (might be due to invalid IL or missing references)
		//IL_002c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0036: Unknown result type (might be due to invalid IL or missing references)
		//IL_003b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0045: Unknown result type (might be due to invalid IL or missing references)
		//IL_0056: Unknown result type (might be due to invalid IL or missing references)
		//IL_0062: Unknown result type (might be due to invalid IL or missing references)
		//IL_0079: Unknown result type (might be due to invalid IL or missing references)
		//IL_007e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0084: Unknown result type (might be due to invalid IL or missing references)
		//IL_009c: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a1: Unknown result type (might be due to invalid IL or missing references)
		//IL_00aa: Unknown result type (might be due to invalid IL or missing references)
		//IL_00af: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b0: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b1: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b2: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b3: Unknown result type (might be due to invalid IL or missing references)
		Gizmos.color = Color.Lerp(Color.green, Color.black, 0.5f);
		Gizmos.DrawRay(keyFrame.curve.p0, keyFrame.curve.v0);
		Gizmos.color = Color.Lerp(Color.red, Color.black, 0.5f);
		Gizmos.DrawRay(keyFrame.curve.p1, keyFrame.curve.v1);
		for (int i = 1; i <= 20; i++)
		{
			float num = (float)i * 0.05f;
			Gizmos.color = Color.Lerp(Color.red, Color.green, num);
			Vector3 val = keyFrame.curve.Evaluate(num - 0.05f);
			Vector3 val2 = keyFrame.curve.Evaluate(num);
			Gizmos.DrawRay(val, val2 - val);
		}
	}

	private void OnDrawGizmos()
	{
		//IL_0049: Unknown result type (might be due to invalid IL or missing references)
		//IL_005a: Unknown result type (might be due to invalid IL or missing references)
		//IL_005f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0069: Unknown result type (might be due to invalid IL or missing references)
		//IL_006e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0078: Unknown result type (might be due to invalid IL or missing references)
		//IL_007d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0082: Unknown result type (might be due to invalid IL or missing references)
		//IL_008c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0096: Unknown result type (might be due to invalid IL or missing references)
		//IL_009b: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a0: Unknown result type (might be due to invalid IL or missing references)
		//IL_00aa: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b4: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b9: Unknown result type (might be due to invalid IL or missing references)
		//IL_00be: Unknown result type (might be due to invalid IL or missing references)
		//IL_00c8: Unknown result type (might be due to invalid IL or missing references)
		foreach (KeyFrame keyFrame in keyFrames)
		{
			DrawKeyFrame(keyFrame);
		}
		for (int i = 0; i < boneTransformationBuffer.Length; i++)
		{
			Gizmos.matrix = Matrix4x4.TRS(boneTransformationBuffer[i].position, boneTransformationBuffer[i].rotation, Vector3.one * 3f);
			Gizmos.DrawRay(-Vector3.forward, Vector3.forward * 2f);
			Gizmos.DrawRay(-Vector3.right, Vector3.right * 2f);
			Gizmos.DrawRay(-Vector3.up, Vector3.up * 2f);
		}
	}

	public void OnTeleport(Vector3 oldPosition, Vector3 newPosition)
	{
		//IL_0000: Unknown result type (might be due to invalid IL or missing references)
		//IL_0001: Unknown result type (might be due to invalid IL or missing references)
		//IL_0002: Unknown result type (might be due to invalid IL or missing references)
		//IL_0007: Unknown result type (might be due to invalid IL or missing references)
		//IL_002b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0030: Unknown result type (might be due to invalid IL or missing references)
		//IL_0031: Unknown result type (might be due to invalid IL or missing references)
		//IL_0036: Unknown result type (might be due to invalid IL or missing references)
		//IL_0043: Unknown result type (might be due to invalid IL or missing references)
		//IL_0048: Unknown result type (might be due to invalid IL or missing references)
		//IL_0049: Unknown result type (might be due to invalid IL or missing references)
		//IL_004e: Unknown result type (might be due to invalid IL or missing references)
		//IL_005b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0060: Unknown result type (might be due to invalid IL or missing references)
		//IL_0061: Unknown result type (might be due to invalid IL or missing references)
		//IL_0066: Unknown result type (might be due to invalid IL or missing references)
		//IL_0073: Unknown result type (might be due to invalid IL or missing references)
		//IL_0078: Unknown result type (might be due to invalid IL or missing references)
		//IL_0079: Unknown result type (might be due to invalid IL or missing references)
		//IL_007e: Unknown result type (might be due to invalid IL or missing references)
		//IL_00af: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b4: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b5: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ba: Unknown result type (might be due to invalid IL or missing references)
		Vector3 val = newPosition - oldPosition;
		for (int i = 0; i < keyFrames.Count; i++)
		{
			KeyFrame value = keyFrames[i];
			CubicBezier3 curve = value.curve;
			ref Vector3 a = ref curve.a;
			a += val;
			ref Vector3 b = ref curve.b;
			b += val;
			ref Vector3 c = ref curve.c;
			c += val;
			ref Vector3 d = ref curve.d;
			d += val;
			value.curve = curve;
			keyFrames[i] = value;
		}
		previousSurfaceTestEnd += val;
	}

	private int FindNearestBone(Vector3 worldPosition)
	{
		//IL_0019: Unknown result type (might be due to invalid IL or missing references)
		//IL_001e: Unknown result type (might be due to invalid IL or missing references)
		//IL_001f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0024: Unknown result type (might be due to invalid IL or missing references)
		int result = -1;
		float num = float.PositiveInfinity;
		for (int i = 0; i < bones.Length; i++)
		{
			Vector3 val = ((Component)bones[i]).transform.position - worldPosition;
			float sqrMagnitude = ((Vector3)(ref val)).sqrMagnitude;
			if (sqrMagnitude < num)
			{
				num = sqrMagnitude;
				result = i;
			}
		}
		return result;
	}

	private void UpdateBoneDisplacements(float deltaTime)
	{
		//IL_001b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0020: Unknown result type (might be due to invalid IL or missing references)
		//IL_002d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0032: Unknown result type (might be due to invalid IL or missing references)
		int i = 0;
		for (int num = boneDisplacements.Length; i < num; i++)
		{
			boneDisplacements[i] = Vector3.MoveTowards(boneDisplacements[i], Vector3.zero, painDisplacementRecoverySpeed * deltaTime);
		}
	}

	void IPainAnimationHandler.HandlePain(float damage, Vector3 damagePosition)
	{
		//IL_0001: Unknown result type (might be due to invalid IL or missing references)
		//IL_0013: Unknown result type (might be due to invalid IL or missing references)
		//IL_001e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0023: Unknown result type (might be due to invalid IL or missing references)
		int num = FindNearestBone(damagePosition);
		if (num != -1)
		{
			boneDisplacements[num] = Random.onUnitSphere * maxPainDisplacementMagnitude;
		}
	}

	public float GetSynchronizedTimeStamp()
	{
		return Run.instance.time;
	}

	private static void WriteKeyFrame(NetworkWriter writer, KeyFrame keyFrame)
	{
		//IL_0007: Unknown result type (might be due to invalid IL or missing references)
		//IL_0018: Unknown result type (might be due to invalid IL or missing references)
		//IL_0029: Unknown result type (might be due to invalid IL or missing references)
		//IL_003a: Unknown result type (might be due to invalid IL or missing references)
		writer.Write(keyFrame.curve.a);
		writer.Write(keyFrame.curve.b);
		writer.Write(keyFrame.curve.c);
		writer.Write(keyFrame.curve.d);
		writer.Write(keyFrame.length);
		writer.Write(keyFrame.time);
	}

	private static KeyFrame ReadKeyFrame(NetworkReader reader)
	{
		//IL_0010: Unknown result type (might be due to invalid IL or missing references)
		//IL_0015: Unknown result type (might be due to invalid IL or missing references)
		//IL_0022: Unknown result type (might be due to invalid IL or missing references)
		//IL_0027: Unknown result type (might be due to invalid IL or missing references)
		//IL_0034: Unknown result type (might be due to invalid IL or missing references)
		//IL_0039: Unknown result type (might be due to invalid IL or missing references)
		//IL_0046: Unknown result type (might be due to invalid IL or missing references)
		//IL_004b: Unknown result type (might be due to invalid IL or missing references)
		KeyFrame result = default(KeyFrame);
		result.curve.a = reader.ReadVector3();
		result.curve.b = reader.ReadVector3();
		result.curve.c = reader.ReadVector3();
		result.curve.d = reader.ReadVector3();
		result.length = reader.ReadSingle();
		result.time = reader.ReadSingle();
		return result;
	}

	public override bool OnSerialize(NetworkWriter writer, bool initialState)
	{
		uint syncVarDirtyBits = ((NetworkBehaviour)this).syncVarDirtyBits;
		if (initialState)
		{
			writer.Write((ushort)keyFrames.Count);
			for (int i = 0; i < keyFrames.Count; i++)
			{
				WriteKeyFrame(writer, keyFrames[i]);
			}
		}
		if (!initialState)
		{
			return syncVarDirtyBits != 0;
		}
		return false;
	}

	public override void OnDeserialize(NetworkReader reader, bool initialState)
	{
		if (initialState)
		{
			keyFrames.Clear();
			int num = reader.ReadUInt16();
			for (int i = 0; i < num; i++)
			{
				keyFrames.Add(ReadKeyFrame(reader));
			}
		}
	}

	private void UNetVersion()
	{
	}

	protected static void InvokeRpcRpcSendKeyFrame(NetworkBehaviour obj, NetworkReader reader)
	{
		if (!NetworkClient.active)
		{
			Debug.LogError((object)"RPC RpcSendKeyFrame called on server.");
		}
		else
		{
			((WormBodyPositions2)(object)obj).RpcSendKeyFrame(GeneratedNetworkCode._ReadKeyFrame_WormBodyPositions2(reader));
		}
	}

	protected static void InvokeRpcRpcPlaySurfaceImpactSound(NetworkBehaviour obj, NetworkReader reader)
	{
		if (!NetworkClient.active)
		{
			Debug.LogError((object)"RPC RpcPlaySurfaceImpactSound called on server.");
		}
		else
		{
			((WormBodyPositions2)(object)obj).RpcPlaySurfaceImpactSound();
		}
	}

	public void CallRpcSendKeyFrame(KeyFrame newKeyFrame)
	{
		//IL_0016: Unknown result type (might be due to invalid IL or missing references)
		//IL_001c: Expected O, but got Unknown
		//IL_0041: Unknown result type (might be due to invalid IL or missing references)
		if (!NetworkServer.active)
		{
			Debug.LogError((object)"RPC Function RpcSendKeyFrame called on client.");
			return;
		}
		NetworkWriter val = new NetworkWriter();
		val.Write((short)0);
		val.Write((short)2);
		val.WritePackedUInt32((uint)kRpcRpcSendKeyFrame);
		val.Write(((Component)this).GetComponent<NetworkIdentity>().netId);
		GeneratedNetworkCode._WriteKeyFrame_WormBodyPositions2(val, newKeyFrame);
		((NetworkBehaviour)this).SendRPCInternal(val, 0, "RpcSendKeyFrame");
	}

	public void CallRpcPlaySurfaceImpactSound()
	{
		//IL_0016: Unknown result type (might be due to invalid IL or missing references)
		//IL_001c: Expected O, but got Unknown
		//IL_0041: Unknown result type (might be due to invalid IL or missing references)
		if (!NetworkServer.active)
		{
			Debug.LogError((object)"RPC Function RpcPlaySurfaceImpactSound called on client.");
			return;
		}
		NetworkWriter val = new NetworkWriter();
		val.Write((short)0);
		val.Write((short)2);
		val.WritePackedUInt32((uint)kRpcRpcPlaySurfaceImpactSound);
		val.Write(((Component)this).GetComponent<NetworkIdentity>().netId);
		((NetworkBehaviour)this).SendRPCInternal(val, 0, "RpcPlaySurfaceImpactSound");
	}

	static WormBodyPositions2()
	{
		//IL_0020: Unknown result type (might be due to invalid IL or missing references)
		//IL_002a: Expected O, but got Unknown
		//IL_004a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0054: Expected O, but got Unknown
		kRpcRpcSendKeyFrame = 874152969;
		NetworkBehaviour.RegisterRpcDelegate(typeof(WormBodyPositions2), kRpcRpcSendKeyFrame, new CmdDelegate(InvokeRpcRpcSendKeyFrame));
		kRpcRpcPlaySurfaceImpactSound = 2010133795;
		NetworkBehaviour.RegisterRpcDelegate(typeof(WormBodyPositions2), kRpcRpcPlaySurfaceImpactSound, new CmdDelegate(InvokeRpcRpcPlaySurfaceImpactSound));
		NetworkCRC.RegisterBehaviour("WormBodyPositions2", 0);
	}

	public override void PreStartClient()
	{
	}
}
