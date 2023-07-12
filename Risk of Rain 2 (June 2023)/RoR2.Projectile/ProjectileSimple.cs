using System;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.Serialization;

namespace RoR2.Projectile;

public class ProjectileSimple : MonoBehaviour
{
	[Header("Lifetime")]
	public float lifetime = 5f;

	public GameObject lifetimeExpiredEffect;

	[FormerlySerializedAs("velocity")]
	[Header("Velocity")]
	public float desiredForwardSpeed;

	public bool updateAfterFiring;

	public bool enableVelocityOverLifetime;

	public AnimationCurve velocityOverLifetime;

	[Header("Oscillation")]
	public bool oscillate;

	public float oscillateMagnitude = 20f;

	public float oscillateSpeed;

	private float deltaHeight;

	private float oscillationStopwatch;

	private float stopwatch;

	private Rigidbody rigidbody;

	private Transform transform;

	[Obsolete("Use 'desiredForwardSpeed' instead.", false)]
	public float velocity
	{
		get
		{
			return desiredForwardSpeed;
		}
		set
		{
			desiredForwardSpeed = value;
		}
	}

	protected void Awake()
	{
		transform = ((Component)this).transform;
		rigidbody = ((Component)this).GetComponent<Rigidbody>();
	}

	protected void OnEnable()
	{
		SetForwardSpeed(desiredForwardSpeed);
	}

	protected void Start()
	{
		SetForwardSpeed(desiredForwardSpeed);
	}

	protected void OnDisable()
	{
		SetForwardSpeed(0f);
	}

	protected void FixedUpdate()
	{
		//IL_00a0: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ab: Unknown result type (might be due to invalid IL or missing references)
		if (oscillate)
		{
			deltaHeight = Mathf.Sin(oscillationStopwatch * oscillateSpeed);
		}
		if (updateAfterFiring || enableVelocityOverLifetime)
		{
			SetForwardSpeed(desiredForwardSpeed);
		}
		oscillationStopwatch += Time.deltaTime;
		stopwatch += Time.deltaTime;
		if (NetworkServer.active && stopwatch > lifetime)
		{
			Object.Destroy((Object)(object)((Component)this).gameObject);
			if (Object.op_Implicit((Object)(object)lifetimeExpiredEffect) && NetworkServer.active)
			{
				EffectManager.SimpleEffect(lifetimeExpiredEffect, transform.position, transform.rotation, transmit: true);
			}
		}
	}

	public void SetLifetime(float newLifetime)
	{
		lifetime = newLifetime;
		stopwatch = 0f;
	}

	protected void SetForwardSpeed(float speed)
	{
		//IL_003b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0040: Unknown result type (might be due to invalid IL or missing references)
		//IL_0057: Unknown result type (might be due to invalid IL or missing references)
		//IL_005d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0068: Unknown result type (might be due to invalid IL or missing references)
		//IL_007a: Unknown result type (might be due to invalid IL or missing references)
		//IL_007f: Unknown result type (might be due to invalid IL or missing references)
		if (Object.op_Implicit((Object)(object)rigidbody))
		{
			if (enableVelocityOverLifetime)
			{
				rigidbody.velocity = speed * velocityOverLifetime.Evaluate(stopwatch / lifetime) * transform.forward;
			}
			else
			{
				rigidbody.velocity = transform.forward * speed + transform.right * (deltaHeight * oscillateMagnitude);
			}
		}
	}
}
