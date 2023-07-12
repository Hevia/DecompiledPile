using System.Runtime.InteropServices;
using UnityEngine;
using UnityEngine.Networking;

namespace RoR2;

public class HealingFollowerController : NetworkBehaviour
{
	public float fractionHealthHealing = 0.01f;

	public float fractionHealthBurst = 0.05f;

	public float healingInterval = 1f;

	public float rotationAngularVelocity;

	public float acceleration = 20f;

	public float damping = 0.1f;

	public bool enableSpringMotion;

	[SyncVar]
	public GameObject ownerBodyObject;

	[SyncVar]
	public GameObject targetBodyObject;

	public GameObject indicator;

	private GameObject cachedTargetBodyObject;

	private HealthComponent cachedTargetHealthComponent;

	private float healingTimer;

	private Vector3 velocity;

	private NetworkInstanceId ___ownerBodyObjectNetId;

	private NetworkInstanceId ___targetBodyObjectNetId;

	public GameObject NetworkownerBodyObject
	{
		get
		{
			return ownerBodyObject;
		}
		[param: In]
		set
		{
			((NetworkBehaviour)this).SetSyncVarGameObject(value, ref ownerBodyObject, 1u, ref ___ownerBodyObjectNetId);
		}
	}

	public GameObject NetworktargetBodyObject
	{
		get
		{
			return targetBodyObject;
		}
		[param: In]
		set
		{
			((NetworkBehaviour)this).SetSyncVarGameObject(value, ref targetBodyObject, 2u, ref ___targetBodyObjectNetId);
		}
	}

	private void FixedUpdate()
	{
		if ((Object)(object)cachedTargetBodyObject != (Object)(object)targetBodyObject)
		{
			cachedTargetBodyObject = targetBodyObject;
			OnTargetChanged();
		}
		if (NetworkServer.active)
		{
			FixedUpdateServer();
		}
	}

	[Server]
	public void AssignNewTarget(GameObject target)
	{
		if (!NetworkServer.active)
		{
			Debug.LogWarning((object)"[Server] function 'System.Void RoR2.HealingFollowerController::AssignNewTarget(UnityEngine.GameObject)' called on client");
			return;
		}
		NetworktargetBodyObject = (Object.op_Implicit((Object)(object)target) ? target : ownerBodyObject);
		cachedTargetBodyObject = targetBodyObject;
		cachedTargetHealthComponent = (Object.op_Implicit((Object)(object)cachedTargetBodyObject) ? cachedTargetBodyObject.GetComponent<HealthComponent>() : null);
		OnTargetChanged();
	}

	private void OnTargetChanged()
	{
		cachedTargetHealthComponent = (Object.op_Implicit((Object)(object)cachedTargetBodyObject) ? cachedTargetBodyObject.GetComponent<HealthComponent>() : null);
	}

	private void FixedUpdateServer()
	{
		healingTimer -= Time.fixedDeltaTime;
		if (healingTimer <= 0f)
		{
			healingTimer = healingInterval;
			DoHeal(fractionHealthHealing * healingInterval);
		}
		if (!Object.op_Implicit((Object)(object)targetBodyObject))
		{
			NetworktargetBodyObject = ownerBodyObject;
		}
		if (!Object.op_Implicit((Object)(object)ownerBodyObject))
		{
			Object.Destroy((Object)(object)((Component)this).gameObject);
		}
	}

	private void Update()
	{
		//IL_000d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0013: Unknown result type (might be due to invalid IL or missing references)
		//IL_001d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0022: Unknown result type (might be due to invalid IL or missing references)
		//IL_003e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0043: Unknown result type (might be due to invalid IL or missing references)
		//IL_004e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0053: Unknown result type (might be due to invalid IL or missing references)
		//IL_0076: Unknown result type (might be due to invalid IL or missing references)
		UpdateMotion();
		Transform transform = ((Component)this).transform;
		transform.position += velocity * Time.deltaTime;
		((Component)this).transform.rotation = Quaternion.AngleAxis(rotationAngularVelocity * Time.deltaTime, Vector3.up) * ((Component)this).transform.rotation;
		if (Object.op_Implicit((Object)(object)targetBodyObject))
		{
			indicator.transform.position = GetTargetPosition();
		}
	}

	[Server]
	private void DoHeal(float healFraction)
	{
		if (!NetworkServer.active)
		{
			Debug.LogWarning((object)"[Server] function 'System.Void RoR2.HealingFollowerController::DoHeal(System.Single)' called on client");
		}
		else if (Object.op_Implicit((Object)(object)cachedTargetHealthComponent))
		{
			cachedTargetHealthComponent.HealFraction(healFraction, default(ProcChainMask));
		}
	}

	public override void OnStartClient()
	{
		//IL_000d: Unknown result type (might be due to invalid IL or missing references)
		((NetworkBehaviour)this).OnStartClient();
		((Component)this).transform.position = GetDesiredPosition();
	}

	private Vector3 GetTargetPosition()
	{
		//IL_002e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0035: Unknown result type (might be due to invalid IL or missing references)
		//IL_0041: Unknown result type (might be due to invalid IL or missing references)
		GameObject val = targetBodyObject ?? ownerBodyObject;
		if (Object.op_Implicit((Object)(object)val))
		{
			CharacterBody component = val.GetComponent<CharacterBody>();
			if (!Object.op_Implicit((Object)(object)component))
			{
				return val.transform.position;
			}
			return component.corePosition;
		}
		return ((Component)this).transform.position;
	}

	private Vector3 GetDesiredPosition()
	{
		//IL_0001: Unknown result type (might be due to invalid IL or missing references)
		return GetTargetPosition();
	}

	private void UpdateMotion()
	{
		//IL_0001: Unknown result type (might be due to invalid IL or missing references)
		//IL_0006: Unknown result type (might be due to invalid IL or missing references)
		//IL_000f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0016: Unknown result type (might be due to invalid IL or missing references)
		//IL_001b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0020: Unknown result type (might be due to invalid IL or missing references)
		//IL_0021: Unknown result type (might be due to invalid IL or missing references)
		//IL_0022: Unknown result type (might be due to invalid IL or missing references)
		//IL_0030: Unknown result type (might be due to invalid IL or missing references)
		//IL_003b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0040: Unknown result type (might be due to invalid IL or missing references)
		//IL_0042: Unknown result type (might be due to invalid IL or missing references)
		//IL_004e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0053: Unknown result type (might be due to invalid IL or missing references)
		//IL_0056: Unknown result type (might be due to invalid IL or missing references)
		//IL_005b: Unknown result type (might be due to invalid IL or missing references)
		//IL_005c: Unknown result type (might be due to invalid IL or missing references)
		//IL_005d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0067: Unknown result type (might be due to invalid IL or missing references)
		//IL_006c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0071: Unknown result type (might be due to invalid IL or missing references)
		//IL_0083: Unknown result type (might be due to invalid IL or missing references)
		//IL_0088: Unknown result type (might be due to invalid IL or missing references)
		//IL_0095: Unknown result type (might be due to invalid IL or missing references)
		Vector3 desiredPosition = GetDesiredPosition();
		if (enableSpringMotion)
		{
			Vector3 val = desiredPosition - ((Component)this).transform.position;
			if (val != Vector3.zero)
			{
				Vector3 val2 = ((Vector3)(ref val)).normalized * acceleration;
				Vector3 val3 = velocity * (0f - damping);
				velocity += (val2 + val3) * Time.deltaTime;
			}
		}
		else
		{
			((Component)this).transform.position = Vector3.SmoothDamp(((Component)this).transform.position, desiredPosition, ref velocity, damping);
		}
	}

	private void UNetVersion()
	{
	}

	public override bool OnSerialize(NetworkWriter writer, bool forceAll)
	{
		if (forceAll)
		{
			writer.Write(ownerBodyObject);
			writer.Write(targetBodyObject);
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
			writer.Write(ownerBodyObject);
		}
		if ((((NetworkBehaviour)this).syncVarDirtyBits & 2u) != 0)
		{
			if (!flag)
			{
				writer.WritePackedUInt32(((NetworkBehaviour)this).syncVarDirtyBits);
				flag = true;
			}
			writer.Write(targetBodyObject);
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
		//IL_0014: Unknown result type (might be due to invalid IL or missing references)
		//IL_0019: Unknown result type (might be due to invalid IL or missing references)
		if (initialState)
		{
			___ownerBodyObjectNetId = reader.ReadNetworkId();
			___targetBodyObjectNetId = reader.ReadNetworkId();
			return;
		}
		int num = (int)reader.ReadPackedUInt32();
		if (((uint)num & (true ? 1u : 0u)) != 0)
		{
			ownerBodyObject = reader.ReadGameObject();
		}
		if (((uint)num & 2u) != 0)
		{
			targetBodyObject = reader.ReadGameObject();
		}
	}

	public override void PreStartClient()
	{
		//IL_0012: Unknown result type (might be due to invalid IL or missing references)
		//IL_0034: Unknown result type (might be due to invalid IL or missing references)
		if (!((NetworkInstanceId)(ref ___ownerBodyObjectNetId)).IsEmpty())
		{
			NetworkownerBodyObject = ClientScene.FindLocalObject(___ownerBodyObjectNetId);
		}
		if (!((NetworkInstanceId)(ref ___targetBodyObjectNetId)).IsEmpty())
		{
			NetworktargetBodyObject = ClientScene.FindLocalObject(___targetBodyObjectNetId);
		}
	}
}
