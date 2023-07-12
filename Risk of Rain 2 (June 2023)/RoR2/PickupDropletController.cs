using System;
using System.Runtime.InteropServices;
using Unity;
using UnityEngine;
using UnityEngine.Networking;

namespace RoR2;

public class PickupDropletController : NetworkBehaviour
{
	public delegate void PickupDropletHitGroundDelegate(ref GenericPickupController.CreatePickupInfo createPickupInfo, ref bool shouldSpawn);

	[NonSerialized]
	[SyncVar]
	public PickupIndex pickupIndex = PickupIndex.none;

	private bool alive = true;

	private GenericPickupController.CreatePickupInfo createPickupInfo;

	private static GameObject pickupDropletPrefab;

	public PickupIndex NetworkpickupIndex
	{
		get
		{
			return pickupIndex;
		}
		[param: In]
		set
		{
			((NetworkBehaviour)this).SetSyncVar<PickupIndex>(value, ref pickupIndex, 1u);
		}
	}

	public static event PickupDropletHitGroundDelegate onDropletHitGroundServer;

	public static void CreatePickupDroplet(PickupIndex pickupIndex, Vector3 position, Vector3 velocity)
	{
		//IL_000a: Unknown result type (might be due to invalid IL or missing references)
		//IL_000f: Unknown result type (might be due to invalid IL or missing references)
		//IL_001d: Unknown result type (might be due to invalid IL or missing references)
		//IL_001e: Unknown result type (might be due to invalid IL or missing references)
		GenericPickupController.CreatePickupInfo pickupInfo = default(GenericPickupController.CreatePickupInfo);
		pickupInfo.rotation = Quaternion.identity;
		pickupInfo.pickupIndex = pickupIndex;
		CreatePickupDroplet(pickupInfo, position, velocity);
	}

	public static void CreatePickupDroplet(GenericPickupController.CreatePickupInfo pickupInfo, Vector3 position, Vector3 velocity)
	{
		//IL_0005: Unknown result type (might be due to invalid IL or missing references)
		//IL_0006: Unknown result type (might be due to invalid IL or missing references)
		//IL_003a: Unknown result type (might be due to invalid IL or missing references)
		//IL_004f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0054: Unknown result type (might be due to invalid IL or missing references)
		GameObject obj = Object.Instantiate<GameObject>(pickupDropletPrefab, position, Quaternion.identity);
		PickupDropletController component = obj.GetComponent<PickupDropletController>();
		if (Object.op_Implicit((Object)(object)component))
		{
			component.createPickupInfo = pickupInfo;
			component.NetworkpickupIndex = pickupInfo.pickupIndex;
		}
		Rigidbody component2 = obj.GetComponent<Rigidbody>();
		component2.velocity = velocity;
		component2.AddTorque(Random.Range(150f, 120f) * Random.onUnitSphere);
		NetworkServer.Spawn(obj);
	}

	[SystemInitializer(new Type[] { })]
	private static void Init()
	{
		pickupDropletPrefab = LegacyResourcesAPI.Load<GameObject>("Prefabs/NetworkedObjects/PickupDroplet");
	}

	public void OnCollisionEnter(Collision collision)
	{
		//IL_0022: Unknown result type (might be due to invalid IL or missing references)
		//IL_0027: Unknown result type (might be due to invalid IL or missing references)
		if (NetworkServer.active && alive)
		{
			alive = false;
			createPickupInfo.position = ((Component)this).transform.position;
			bool shouldSpawn = true;
			PickupDropletController.onDropletHitGroundServer?.Invoke(ref createPickupInfo, ref shouldSpawn);
			if (shouldSpawn)
			{
				GenericPickupController.CreatePickup(in createPickupInfo);
			}
			Object.Destroy((Object)(object)((Component)this).gameObject);
		}
	}

	private void Start()
	{
		GameObject val = PickupCatalog.GetPickupDef(pickupIndex)?.dropletDisplayPrefab;
		if (Object.op_Implicit((Object)(object)val))
		{
			Object.Instantiate<GameObject>(val, ((Component)this).transform);
		}
	}

	private void UNetVersion()
	{
	}

	public override bool OnSerialize(NetworkWriter writer, bool forceAll)
	{
		if (forceAll)
		{
			GeneratedNetworkCode._WritePickupIndex_None(writer, pickupIndex);
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
			GeneratedNetworkCode._WritePickupIndex_None(writer, pickupIndex);
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
			pickupIndex = GeneratedNetworkCode._ReadPickupIndex_None(reader);
			return;
		}
		int num = (int)reader.ReadPackedUInt32();
		if (((uint)num & (true ? 1u : 0u)) != 0)
		{
			pickupIndex = GeneratedNetworkCode._ReadPickupIndex_None(reader);
		}
	}

	public override void PreStartClient()
	{
	}
}
