using System;
using System.Runtime.InteropServices;
using UnityEngine;
using UnityEngine.Networking;

namespace RoR2.Projectile;

[RequireComponent(typeof(ProjectileTargetComponent))]
[RequireComponent(typeof(ProjectileController))]
public class GatewayProjectileController : NetworkBehaviour, IInteractable
{
	private ProjectileController projectileController;

	private ProjectileTargetComponent projectileTargetComponent;

	[SyncVar(hook = "SetLinkedObject")]
	private GameObject linkedObject;

	private GatewayProjectileController linkedGatewayProjectileController;

	private NetworkInstanceId ___linkedObjectNetId;

	public GameObject NetworklinkedObject
	{
		get
		{
			return linkedObject;
		}
		[param: In]
		set
		{
			if (NetworkServer.localClientActive && !((NetworkBehaviour)this).syncVarHookGuard)
			{
				((NetworkBehaviour)this).syncVarHookGuard = true;
				SetLinkedObject(value);
				((NetworkBehaviour)this).syncVarHookGuard = false;
			}
			((NetworkBehaviour)this).SetSyncVarGameObject(value, ref linkedObject, 1u, ref ___linkedObjectNetId);
		}
	}

	public string GetContextString(Interactor activator)
	{
		throw new NotImplementedException();
	}

	public Interactability GetInteractability(Interactor activator)
	{
		if (!Object.op_Implicit((Object)(object)linkedObject))
		{
			return Interactability.ConditionsNotMet;
		}
		return Interactability.Available;
	}

	public void OnInteractionBegin(Interactor activator)
	{
		throw new NotImplementedException();
	}

	public bool ShouldIgnoreSpherecastForInteractibility(Interactor activator)
	{
		return false;
	}

	public bool ShouldShowOnScanner()
	{
		return false;
	}

	private void SetLinkedObject(GameObject newLinkedObject)
	{
		if ((Object)(object)linkedObject != (Object)(object)newLinkedObject)
		{
			NetworklinkedObject = newLinkedObject;
			linkedGatewayProjectileController = (Object.op_Implicit((Object)(object)linkedObject) ? linkedObject.GetComponent<GatewayProjectileController>() : null);
			if (Object.op_Implicit((Object)(object)linkedGatewayProjectileController))
			{
				linkedGatewayProjectileController.SetLinkedObject(((Component)this).gameObject);
			}
		}
	}

	private void Awake()
	{
		projectileController = ((Component)this).GetComponent<ProjectileController>();
		projectileTargetComponent = ((Component)this).GetComponent<ProjectileTargetComponent>();
	}

	public override void OnStartServer()
	{
		//IL_003f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0044: Unknown result type (might be due to invalid IL or missing references)
		//IL_0051: Unknown result type (might be due to invalid IL or missing references)
		//IL_0056: Unknown result type (might be due to invalid IL or missing references)
		((NetworkBehaviour)this).OnStartServer();
		if (Object.op_Implicit((Object)(object)projectileTargetComponent.target))
		{
			SetLinkedObject(((Component)projectileTargetComponent.target).gameObject);
			return;
		}
		FireProjectileInfo fireProjectileInfo = default(FireProjectileInfo);
		fireProjectileInfo.position = ((Component)this).transform.position;
		fireProjectileInfo.rotation = ((Component)this).transform.rotation;
		fireProjectileInfo.target = ((Component)this).gameObject;
		fireProjectileInfo.owner = projectileController.owner;
		fireProjectileInfo.speedOverride = 0f;
		fireProjectileInfo.projectilePrefab = LegacyResourcesAPI.Load<GameObject>("Prefabs/Projectiles/GatewayProjectile");
		ProjectileManager.instance.FireProjectile(fireProjectileInfo);
	}

	private void UNetVersion()
	{
	}

	public override bool OnSerialize(NetworkWriter writer, bool forceAll)
	{
		if (forceAll)
		{
			writer.Write(linkedObject);
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
			writer.Write(linkedObject);
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
		if (initialState)
		{
			___linkedObjectNetId = reader.ReadNetworkId();
			return;
		}
		int num = (int)reader.ReadPackedUInt32();
		if (((uint)num & (true ? 1u : 0u)) != 0)
		{
			SetLinkedObject(reader.ReadGameObject());
		}
	}

	public override void PreStartClient()
	{
		//IL_0012: Unknown result type (might be due to invalid IL or missing references)
		if (!((NetworkInstanceId)(ref ___linkedObjectNetId)).IsEmpty())
		{
			NetworklinkedObject = ClientScene.FindLocalObject(___linkedObjectNetId);
		}
	}
}
