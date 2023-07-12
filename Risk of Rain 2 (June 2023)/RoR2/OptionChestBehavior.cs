using EntityStates;
using EntityStates.Barrel;
using RoR2.Networking;
using UnityEngine;
using UnityEngine.Networking;

namespace RoR2;

public class OptionChestBehavior : NetworkBehaviour, IChestBehavior
{
	[SerializeField]
	private PickupDropTable dropTable;

	[SerializeField]
	private Transform dropTransform;

	[SerializeField]
	private float dropUpVelocityStrength = 20f;

	[SerializeField]
	private float dropForwardVelocityStrength = 2f;

	[SerializeField]
	private SerializableEntityStateType openState = new SerializableEntityStateType(typeof(Opening));

	[SerializeField]
	private GameObject pickupPrefab;

	[SerializeField]
	private int numOptions;

	[SerializeField]
	private ItemTier displayTier;

	private PickupIndex[] generatedDrops;

	private Xoroshiro128Plus rng;

	public override int GetNetworkChannel()
	{
		return QosChannelIndex.defaultReliable.intVal;
	}

	[Server]
	public void Roll()
	{
		if (!NetworkServer.active)
		{
			Debug.LogWarning((object)"[Server] function 'System.Void RoR2.OptionChestBehavior::Roll()' called on client");
		}
		else
		{
			generatedDrops = dropTable.GenerateUniqueDrops(numOptions, rng);
		}
	}

	private void Awake()
	{
		if ((Object)(object)dropTransform == (Object)null)
		{
			dropTransform = ((Component)this).transform;
		}
	}

	private void Start()
	{
		//IL_0017: Unknown result type (might be due to invalid IL or missing references)
		//IL_0021: Expected O, but got Unknown
		if (NetworkServer.active)
		{
			rng = new Xoroshiro128Plus(Run.instance.treasureRng.nextUlong);
			Roll();
		}
	}

	[Server]
	public void Open()
	{
		if (!NetworkServer.active)
		{
			Debug.LogWarning((object)"[Server] function 'System.Void RoR2.OptionChestBehavior::Open()' called on client");
			return;
		}
		EntityStateMachine component = ((Component)this).GetComponent<EntityStateMachine>();
		if (Object.op_Implicit((Object)(object)component))
		{
			component.SetNextState(EntityStateCatalog.InstantiateState(openState));
		}
	}

	[Server]
	public void ItemDrop()
	{
		//IL_0056: Unknown result type (might be due to invalid IL or missing references)
		//IL_005b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0062: Unknown result type (might be due to invalid IL or missing references)
		//IL_0067: Unknown result type (might be due to invalid IL or missing references)
		//IL_0085: Unknown result type (might be due to invalid IL or missing references)
		//IL_008a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0095: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a0: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ab: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b0: Unknown result type (might be due to invalid IL or missing references)
		if (!NetworkServer.active)
		{
			Debug.LogWarning((object)"[Server] function 'System.Void RoR2.OptionChestBehavior::ItemDrop()' called on client");
		}
		else if (generatedDrops != null && generatedDrops.Length != 0)
		{
			GenericPickupController.CreatePickupInfo pickupInfo = default(GenericPickupController.CreatePickupInfo);
			pickupInfo.pickerOptions = PickupPickerController.GenerateOptionsFromArray(generatedDrops);
			pickupInfo.prefabOverride = pickupPrefab;
			pickupInfo.position = dropTransform.position;
			pickupInfo.rotation = Quaternion.identity;
			pickupInfo.pickupIndex = PickupCatalog.FindPickupIndex(displayTier);
			PickupDropletController.CreatePickupDroplet(pickupInfo, dropTransform.position, Vector3.up * dropUpVelocityStrength + dropTransform.forward * dropForwardVelocityStrength);
			generatedDrops = null;
		}
	}

	public bool HasRolledPickup(PickupIndex pickupIndex)
	{
		if (generatedDrops != null)
		{
			PickupIndex[] array = generatedDrops;
			for (int i = 0; i < array.Length; i++)
			{
				if (array[i] == pickupIndex)
				{
					return true;
				}
			}
		}
		return false;
	}

	private void UNetVersion()
	{
	}

	public override bool OnSerialize(NetworkWriter writer, bool forceAll)
	{
		bool result = default(bool);
		return result;
	}

	public override void OnDeserialize(NetworkReader reader, bool initialState)
	{
	}

	public override void PreStartClient()
	{
	}
}
