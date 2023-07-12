using System;
using HG;
using UnityEngine;
using UnityEngine.Networking;

namespace RoR2;

[RequireComponent(typeof(WormBodyPositions2))]
public class WormBodyPositionsDriver : MonoBehaviour
{
	public Transform referenceTransform;

	public Transform chasePositionVisualizer;

	public float maxTurnSpeed = 180f;

	public float verticalTurnSquashFactor = 2f;

	public float ySpringConstant = 100f;

	public float yDamperConstant = 1f;

	public bool allowShoving;

	public float yShoveVelocityThreshold;

	public float yShovePositionThreshold;

	public float yShoveForce;

	public float turnRateCoefficientAboveGround;

	public float wormForceCoefficientAboveGround;

	public float keyFrameGenerationInterval = 0.25f;

	public float maxBreachSpeed = 40f;

	private WormBodyPositions2 wormBodyPositions;

	private CharacterDirection characterDirection;

	private Vector3 chaserPreviousVelocity;

	private bool chaserIsUnderground;

	private float keyFrameGenerationTimer;

	public Vector3 chaserVelocity { get; set; }

	public Vector3 chaserPosition { get; private set; }

	private void Awake()
	{
		wormBodyPositions = ((Component)this).GetComponent<WormBodyPositions2>();
		characterDirection = ((Component)this).GetComponent<CharacterDirection>();
	}

	private void OnEnable()
	{
		wormBodyPositions.onPredictedBreachDiscovered += OnPredictedBreachDiscovered;
	}

	private void OnDisable()
	{
		wormBodyPositions.onPredictedBreachDiscovered -= OnPredictedBreachDiscovered;
	}

	private void Start()
	{
		//IL_000e: Unknown result type (might be due to invalid IL or missing references)
		//IL_001f: Unknown result type (might be due to invalid IL or missing references)
		if (NetworkServer.active)
		{
			chaserPosition = ((Component)this).transform.position;
			chaserVelocity = characterDirection.forward;
		}
	}

	private void FixedUpdate()
	{
		if (NetworkServer.active)
		{
			FixedUpdateServer();
		}
	}

	public void OnTeleport(Vector3 oldPosition, Vector3 newPosition)
	{
		//IL_0000: Unknown result type (might be due to invalid IL or missing references)
		//IL_0001: Unknown result type (might be due to invalid IL or missing references)
		//IL_0002: Unknown result type (might be due to invalid IL or missing references)
		//IL_0007: Unknown result type (might be due to invalid IL or missing references)
		//IL_000a: Unknown result type (might be due to invalid IL or missing references)
		//IL_000f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0010: Unknown result type (might be due to invalid IL or missing references)
		Vector3 val = newPosition - oldPosition;
		chaserPosition += val;
	}

	private void OnPredictedBreachDiscovered(float expectedTime, Vector3 hitPosition, Vector3 hitNormal)
	{
		//IL_0001: Unknown result type (might be due to invalid IL or missing references)
		//IL_0006: Unknown result type (might be due to invalid IL or missing references)
		//IL_001a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0027: Unknown result type (might be due to invalid IL or missing references)
		Vector3 val = chaserVelocity;
		float magnitude = ((Vector3)(ref val)).magnitude;
		if (magnitude > maxBreachSpeed)
		{
			chaserVelocity /= magnitude / maxBreachSpeed;
		}
	}

	private void FixedUpdateServer()
	{
		//IL_0006: Unknown result type (might be due to invalid IL or missing references)
		//IL_0017: Unknown result type (might be due to invalid IL or missing references)
		//IL_0019: Unknown result type (might be due to invalid IL or missing references)
		//IL_001e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0023: Unknown result type (might be due to invalid IL or missing references)
		//IL_0027: Unknown result type (might be due to invalid IL or missing references)
		//IL_002c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0054: Unknown result type (might be due to invalid IL or missing references)
		//IL_0064: Unknown result type (might be due to invalid IL or missing references)
		//IL_0075: Unknown result type (might be due to invalid IL or missing references)
		//IL_0080: Unknown result type (might be due to invalid IL or missing references)
		//IL_008b: Unknown result type (might be due to invalid IL or missing references)
		//IL_008c: Unknown result type (might be due to invalid IL or missing references)
		//IL_008f: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a0: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a5: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a8: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ae: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b3: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ba: Unknown result type (might be due to invalid IL or missing references)
		//IL_00c8: Unknown result type (might be due to invalid IL or missing references)
		//IL_00f0: Unknown result type (might be due to invalid IL or missing references)
		//IL_0113: Unknown result type (might be due to invalid IL or missing references)
		//IL_0118: Unknown result type (might be due to invalid IL or missing references)
		//IL_011d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0134: Unknown result type (might be due to invalid IL or missing references)
		//IL_015e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0163: Unknown result type (might be due to invalid IL or missing references)
		//IL_0168: Unknown result type (might be due to invalid IL or missing references)
		//IL_017e: Unknown result type (might be due to invalid IL or missing references)
		//IL_018a: Unknown result type (might be due to invalid IL or missing references)
		//IL_018f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0199: Unknown result type (might be due to invalid IL or missing references)
		//IL_019e: Unknown result type (might be due to invalid IL or missing references)
		//IL_01a9: Unknown result type (might be due to invalid IL or missing references)
		//IL_01b0: Unknown result type (might be due to invalid IL or missing references)
		//IL_01ba: Unknown result type (might be due to invalid IL or missing references)
		//IL_01c0: Unknown result type (might be due to invalid IL or missing references)
		//IL_01cc: Unknown result type (might be due to invalid IL or missing references)
		//IL_01d2: Unknown result type (might be due to invalid IL or missing references)
		//IL_01dc: Unknown result type (might be due to invalid IL or missing references)
		//IL_01e1: Unknown result type (might be due to invalid IL or missing references)
		//IL_01f2: Unknown result type (might be due to invalid IL or missing references)
		//IL_025b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0261: Unknown result type (might be due to invalid IL or missing references)
		Vector3 position = referenceTransform.position;
		float speedMultiplier = wormBodyPositions.speedMultiplier;
		Vector3 val = position - chaserPosition;
		Vector3 normalized = ((Vector3)(ref val)).normalized;
		float num = (chaserIsUnderground ? maxTurnSpeed : (maxTurnSpeed * turnRateCoefficientAboveGround)) * (MathF.PI / 180f);
		Vector3 val2 = default(Vector3);
		((Vector3)(ref val2))._002Ector(chaserVelocity.x, 0f, chaserVelocity.z);
		Vector3 val3 = default(Vector3);
		((Vector3)(ref val3))._002Ector(normalized.x, 0f, normalized.z);
		val2 = Vector3.RotateTowards(val2, val3 * speedMultiplier, num * Time.fixedDeltaTime, float.PositiveInfinity);
		val2 = ((Vector3)(ref val2)).normalized * speedMultiplier;
		float num2 = position.y - chaserPosition.y;
		float num3 = (0f - chaserVelocity.y) * yDamperConstant;
		float num4 = num2 * ySpringConstant;
		if (allowShoving && Mathf.Abs(chaserVelocity.y) < yShoveVelocityThreshold && num2 > yShovePositionThreshold)
		{
			val = chaserVelocity;
			chaserVelocity = Vector3Utils.XAZ(ref val, chaserVelocity.y + yShoveForce * Time.fixedDeltaTime);
		}
		if (!chaserIsUnderground)
		{
			num4 *= wormForceCoefficientAboveGround;
			num3 *= wormForceCoefficientAboveGround;
		}
		val = chaserVelocity;
		chaserVelocity = Vector3Utils.XAZ(ref val, chaserVelocity.y + (num4 + num3) * Time.fixedDeltaTime);
		chaserVelocity += Physics.gravity * Time.fixedDeltaTime;
		chaserVelocity = new Vector3(val2.x, chaserVelocity.y, val2.z);
		chaserPosition += chaserVelocity * Time.fixedDeltaTime;
		chasePositionVisualizer.position = chaserPosition;
		chaserIsUnderground = 0f - num2 < wormBodyPositions.undergroundTestYOffset;
		keyFrameGenerationTimer -= Time.deltaTime;
		if (keyFrameGenerationTimer <= 0f)
		{
			keyFrameGenerationTimer = keyFrameGenerationInterval;
			wormBodyPositions.AttemptToGenerateKeyFrame(wormBodyPositions.GetSynchronizedTimeStamp() + wormBodyPositions.followDelay, chaserPosition, chaserVelocity);
		}
	}
}
