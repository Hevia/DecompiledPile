using System.Runtime.InteropServices;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.Networking;

namespace RoR2.Projectile;

[RequireComponent(typeof(ProjectileController))]
public class BoomerangProjectile : NetworkBehaviour, IProjectileImpactBehavior
{
	private enum BoomerangState
	{
		FlyOut,
		Transition,
		FlyBack
	}

	public float travelSpeed = 40f;

	public float charge;

	public float transitionDuration;

	private float maxFlyStopwatch;

	public GameObject impactSpark;

	public GameObject crosshairPrefab;

	public bool canHitCharacters;

	public bool canHitWorld;

	private ProjectileController projectileController;

	[SyncVar]
	private BoomerangState boomerangState;

	private Transform ownerTransform;

	private ProjectileDamage projectileDamage;

	private Rigidbody rigidbody;

	private float stopwatch;

	private float fireAge;

	private float fireFrequency;

	public float distanceMultiplier = 2f;

	public UnityEvent onFlyBack;

	private bool setScale;

	public BoomerangState NetworkboomerangState
	{
		get
		{
			return boomerangState;
		}
		[param: In]
		set
		{
			ulong num = (ulong)value;
			ulong num2 = (ulong)boomerangState;
			((NetworkBehaviour)this).SetSyncVarEnum<BoomerangState>(value, num, ref boomerangState, num2, 1u);
		}
	}

	private void Awake()
	{
		rigidbody = ((Component)this).GetComponent<Rigidbody>();
		projectileController = ((Component)this).GetComponent<ProjectileController>();
		projectileDamage = ((Component)this).GetComponent<ProjectileDamage>();
		if (Object.op_Implicit((Object)(object)projectileController) && Object.op_Implicit((Object)(object)projectileController.owner))
		{
			ownerTransform = projectileController.owner.transform;
		}
		maxFlyStopwatch = charge * distanceMultiplier;
	}

	private void Start()
	{
		//IL_0024: Unknown result type (might be due to invalid IL or missing references)
		//IL_0036: Unknown result type (might be due to invalid IL or missing references)
		//IL_0048: Unknown result type (might be due to invalid IL or missing references)
		//IL_005e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0079: Unknown result type (might be due to invalid IL or missing references)
		float num = charge * 7f;
		if (num < 1f)
		{
			num = 1f;
		}
		Vector3 localScale = default(Vector3);
		((Vector3)(ref localScale))._002Ector(num * ((Component)this).transform.localScale.x, num * ((Component)this).transform.localScale.y, num * ((Component)this).transform.localScale.z);
		((Component)this).transform.localScale = localScale;
		((Component)((Component)this).gameObject.GetComponent<ProjectileController>().ghost).transform.localScale = localScale;
		((Component)this).GetComponent<ProjectileDotZone>().damageCoefficient *= num;
	}

	public void OnProjectileImpact(ProjectileImpactInfo impactInfo)
	{
		//IL_0028: Unknown result type (might be due to invalid IL or missing references)
		//IL_0033: Unknown result type (might be due to invalid IL or missing references)
		//IL_0038: Unknown result type (might be due to invalid IL or missing references)
		if (canHitWorld)
		{
			NetworkboomerangState = BoomerangState.FlyBack;
			UnityEvent obj = onFlyBack;
			if (obj != null)
			{
				obj.Invoke();
			}
			EffectManager.SimpleImpactEffect(impactSpark, impactInfo.estimatedPointOfImpact, -((Component)this).transform.forward, transmit: true);
		}
	}

	private bool Reel()
	{
		//IL_0010: Unknown result type (might be due to invalid IL or missing references)
		//IL_001b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0020: Unknown result type (might be due to invalid IL or missing references)
		//IL_0025: Unknown result type (might be due to invalid IL or missing references)
		//IL_0028: Unknown result type (might be due to invalid IL or missing references)
		Vector3 val = projectileController.owner.transform.position - ((Component)this).transform.position;
		_ = ((Vector3)(ref val)).normalized;
		return ((Vector3)(ref val)).magnitude <= 2f;
	}

	public void FixedUpdate()
	{
		//IL_006d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0072: Unknown result type (might be due to invalid IL or missing references)
		//IL_00d3: Unknown result type (might be due to invalid IL or missing references)
		//IL_00d8: Unknown result type (might be due to invalid IL or missing references)
		//IL_00eb: Unknown result type (might be due to invalid IL or missing references)
		//IL_00f0: Unknown result type (might be due to invalid IL or missing references)
		//IL_00fb: Unknown result type (might be due to invalid IL or missing references)
		//IL_00fc: Unknown result type (might be due to invalid IL or missing references)
		//IL_0102: Unknown result type (might be due to invalid IL or missing references)
		//IL_0142: Unknown result type (might be due to invalid IL or missing references)
		//IL_0147: Unknown result type (might be due to invalid IL or missing references)
		//IL_0155: Unknown result type (might be due to invalid IL or missing references)
		//IL_0157: Unknown result type (might be due to invalid IL or missing references)
		if (!NetworkServer.active)
		{
			return;
		}
		if (!setScale)
		{
			setScale = true;
		}
		if (!Object.op_Implicit((Object)(object)projectileController.owner))
		{
			Object.Destroy((Object)(object)((Component)this).gameObject);
			return;
		}
		switch (boomerangState)
		{
		case BoomerangState.FlyOut:
			if (NetworkServer.active)
			{
				rigidbody.velocity = travelSpeed * ((Component)this).transform.forward;
				stopwatch += Time.fixedDeltaTime;
				if (stopwatch >= maxFlyStopwatch)
				{
					stopwatch = 0f;
					NetworkboomerangState = BoomerangState.Transition;
				}
			}
			break;
		case BoomerangState.Transition:
		{
			stopwatch += Time.fixedDeltaTime;
			float num = stopwatch / transitionDuration;
			Vector3 val2 = CalculatePullDirection();
			rigidbody.velocity = Vector3.Lerp(travelSpeed * ((Component)this).transform.forward, travelSpeed * val2, num);
			if (num >= 1f)
			{
				NetworkboomerangState = BoomerangState.FlyBack;
				UnityEvent obj = onFlyBack;
				if (obj != null)
				{
					obj.Invoke();
				}
			}
			break;
		}
		case BoomerangState.FlyBack:
		{
			bool flag = Reel();
			if (NetworkServer.active)
			{
				canHitWorld = false;
				Vector3 val = CalculatePullDirection();
				rigidbody.velocity = travelSpeed * val;
				if (flag)
				{
					Object.Destroy((Object)(object)((Component)this).gameObject);
				}
			}
			break;
		}
		}
		Vector3 CalculatePullDirection()
		{
			//IL_0022: Unknown result type (might be due to invalid IL or missing references)
			//IL_002d: Unknown result type (might be due to invalid IL or missing references)
			//IL_0032: Unknown result type (might be due to invalid IL or missing references)
			//IL_0037: Unknown result type (might be due to invalid IL or missing references)
			//IL_003a: Unknown result type (might be due to invalid IL or missing references)
			//IL_0046: Unknown result type (might be due to invalid IL or missing references)
			if (Object.op_Implicit((Object)(object)projectileController.owner))
			{
				Vector3 val3 = projectileController.owner.transform.position - ((Component)this).transform.position;
				return ((Vector3)(ref val3)).normalized;
			}
			return ((Component)this).transform.forward;
		}
	}

	private void UNetVersion()
	{
	}

	public override bool OnSerialize(NetworkWriter writer, bool forceAll)
	{
		if (forceAll)
		{
			writer.Write((int)boomerangState);
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
			writer.Write((int)boomerangState);
		}
		if (!flag)
		{
			writer.WritePackedUInt32(((NetworkBehaviour)this).syncVarDirtyBits);
		}
		return flag;
	}

	public override void OnDeserialize(NetworkReader reader, bool initialState)
	{
		if (initialState)
		{
			boomerangState = (BoomerangState)reader.ReadInt32();
			return;
		}
		int num = (int)reader.ReadPackedUInt32();
		if (((uint)num & (true ? 1u : 0u)) != 0)
		{
			boomerangState = (BoomerangState)reader.ReadInt32();
		}
	}

	public override void PreStartClient()
	{
	}
}
