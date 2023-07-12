using System.Runtime.InteropServices;
using UnityEngine;
using UnityEngine.Networking;

namespace RoR2;

[RequireComponent(typeof(DelayBlast))]
public class SpiteBombController : NetworkBehaviour
{
	public float radius;

	public float bounce = 0.8f;

	public float minimumBounceVelocity;

	public GameObject[] meshVisuals;

	public string[] bounceSoundStrings;

	private Transform transform;

	private Rigidbody rb;

	[SyncVar]
	private Vector3 _bouncePosition;

	[SyncVar]
	private float _initialVelocityY;

	private static readonly int maxBounces = 2;

	private Vector3 startPosition;

	private Vector3 velocity;

	private int bounces;

	public DelayBlast delayBlast { get; private set; }

	public Vector3 bouncePosition
	{
		get
		{
			//IL_0001: Unknown result type (might be due to invalid IL or missing references)
			return _bouncePosition;
		}
		set
		{
			//IL_0001: Unknown result type (might be due to invalid IL or missing references)
			Network_bouncePosition = value;
		}
	}

	public float initialVelocityY
	{
		get
		{
			return _initialVelocityY;
		}
		set
		{
			Network_initialVelocityY = value;
		}
	}

	public Vector3 Network_bouncePosition
	{
		get
		{
			//IL_0001: Unknown result type (might be due to invalid IL or missing references)
			return _bouncePosition;
		}
		[param: In]
		set
		{
			//IL_0001: Unknown result type (might be due to invalid IL or missing references)
			((NetworkBehaviour)this).SetSyncVar<Vector3>(value, ref _bouncePosition, 1u);
		}
	}

	public float Network_initialVelocityY
	{
		get
		{
			return _initialVelocityY;
		}
		[param: In]
		set
		{
			((NetworkBehaviour)this).SetSyncVar<float>(value, ref _initialVelocityY, 2u);
		}
	}

	private void Awake()
	{
		transform = ((Component)this).transform;
		rb = ((Component)this).GetComponent<Rigidbody>();
		delayBlast = ((Component)this).GetComponent<DelayBlast>();
	}

	private void Start()
	{
		//IL_0007: Unknown result type (might be due to invalid IL or missing references)
		//IL_000c: Unknown result type (might be due to invalid IL or missing references)
		//IL_001d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0033: Unknown result type (might be due to invalid IL or missing references)
		//IL_0039: Unknown result type (might be due to invalid IL or missing references)
		//IL_003e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0043: Unknown result type (might be due to invalid IL or missing references)
		//IL_0060: Unknown result type (might be due to invalid IL or missing references)
		//IL_0062: Unknown result type (might be due to invalid IL or missing references)
		//IL_0068: Unknown result type (might be due to invalid IL or missing references)
		//IL_006d: Unknown result type (might be due to invalid IL or missing references)
		startPosition = transform.position;
		float time = Trajectory.CalculateFlightDuration(startPosition.y, bouncePosition.y, initialVelocityY);
		Vector3 val = bouncePosition - startPosition;
		val.y = 0f;
		float magnitude = ((Vector3)(ref val)).magnitude;
		float num = Trajectory.CalculateGroundSpeed(time, magnitude);
		velocity = val / magnitude * num;
		velocity.y = initialVelocityY;
	}

	private void FixedUpdate()
	{
		//IL_0014: Unknown result type (might be due to invalid IL or missing references)
		//IL_0027: Unknown result type (might be due to invalid IL or missing references)
		//IL_002c: Unknown result type (might be due to invalid IL or missing references)
		//IL_002d: Unknown result type (might be due to invalid IL or missing references)
		//IL_002f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0035: Unknown result type (might be due to invalid IL or missing references)
		//IL_003a: Unknown result type (might be due to invalid IL or missing references)
		//IL_003f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0040: Unknown result type (might be due to invalid IL or missing references)
		//IL_0047: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a6: Unknown result type (might be due to invalid IL or missing references)
		//IL_00c8: Unknown result type (might be due to invalid IL or missing references)
		float fixedDeltaTime = Time.fixedDeltaTime;
		velocity.y += fixedDeltaTime * Physics.gravity.y;
		Vector3 position = transform.position;
		position += velocity * fixedDeltaTime;
		if (position.y < bouncePosition.y + radius)
		{
			velocity.y = Mathf.Max(velocity.y * (0f - bounce), minimumBounceVelocity);
			velocity.x = 0f;
			velocity.z = 0f;
			position.y = bouncePosition.y + radius;
			OnBounce();
		}
		rb.MovePosition(position);
	}

	private void OnBounce()
	{
		meshVisuals[bounces].SetActive(false);
		Util.PlaySound(bounceSoundStrings[bounces], ((Component)this).gameObject);
		bounces++;
		if (bounces > maxBounces)
		{
			OnFinalBounce();
		}
		else
		{
			meshVisuals[bounces].SetActive(true);
		}
	}

	private void OnFinalBounce()
	{
		//IL_0013: Unknown result type (might be due to invalid IL or missing references)
		//IL_0018: Unknown result type (might be due to invalid IL or missing references)
		if (NetworkServer.active)
		{
			delayBlast.position = transform.position;
			delayBlast.Detonate();
		}
	}

	private void UNetVersion()
	{
	}

	public override bool OnSerialize(NetworkWriter writer, bool forceAll)
	{
		//IL_0008: Unknown result type (might be due to invalid IL or missing references)
		//IL_004b: Unknown result type (might be due to invalid IL or missing references)
		if (forceAll)
		{
			writer.Write(_bouncePosition);
			writer.Write(_initialVelocityY);
			return true;
		}
		bool flag = false;
		if ((((NetworkBehaviour)this).syncVarDirtyBits & (true ? 1u : 0u)) != 0)
		{
			if (!flag)
			{
				writer.WritePackedUInt32(((NetworkBehaviour)this).syncVarDirtyBits);
				flag = true;
			}
			writer.Write(_bouncePosition);
		}
		if ((((NetworkBehaviour)this).syncVarDirtyBits & 2u) != 0)
		{
			if (!flag)
			{
				writer.WritePackedUInt32(((NetworkBehaviour)this).syncVarDirtyBits);
				flag = true;
			}
			writer.Write(_initialVelocityY);
		}
		if (!flag)
		{
			writer.WritePackedUInt32(((NetworkBehaviour)this).syncVarDirtyBits);
		}
		return flag;
	}

	public override void OnDeserialize(NetworkReader reader, bool initialState)
	{
		//IL_0008: Unknown result type (might be due to invalid IL or missing references)
		//IL_000d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0035: Unknown result type (might be due to invalid IL or missing references)
		//IL_003a: Unknown result type (might be due to invalid IL or missing references)
		if (initialState)
		{
			_bouncePosition = reader.ReadVector3();
			_initialVelocityY = reader.ReadSingle();
			return;
		}
		int num = (int)reader.ReadPackedUInt32();
		if (((uint)num & (true ? 1u : 0u)) != 0)
		{
			_bouncePosition = reader.ReadVector3();
		}
		if (((uint)num & 2u) != 0)
		{
			_initialVelocityY = reader.ReadSingle();
		}
	}

	public override void PreStartClient()
	{
	}
}
