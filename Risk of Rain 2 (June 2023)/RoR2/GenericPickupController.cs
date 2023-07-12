using System;
using System.Runtime.InteropServices;
using RoR2.Items;
using RoR2.Networking;
using Unity;
using UnityEngine;
using UnityEngine.Networking;

namespace RoR2;

public sealed class GenericPickupController : NetworkBehaviour, IInteractable, IDisplayNameProvider
{
	private class PickupMessage : MessageBase
	{
		public GameObject masterGameObject;

		public PickupIndex pickupIndex;

		public uint pickupQuantity;

		public void Reset()
		{
			masterGameObject = null;
			pickupIndex = PickupIndex.none;
			pickupQuantity = 0u;
		}

		public override void Serialize(NetworkWriter writer)
		{
			writer.Write(masterGameObject);
			GeneratedNetworkCode._WritePickupIndex_None(writer, pickupIndex);
			writer.WritePackedUInt32(pickupQuantity);
		}

		public override void Deserialize(NetworkReader reader)
		{
			masterGameObject = reader.ReadGameObject();
			pickupIndex = GeneratedNetworkCode._ReadPickupIndex_None(reader);
			pickupQuantity = reader.ReadPackedUInt32();
		}
	}

	public struct CreatePickupInfo
	{
		public Vector3 position;

		public Quaternion rotation;

		private PickupIndex? _pickupIndex;

		public PickupPickerController.Option[] pickerOptions;

		public GameObject prefabOverride;

		public PickupIndex pickupIndex
		{
			get
			{
				if (!_pickupIndex.HasValue)
				{
					return PickupIndex.none;
				}
				return _pickupIndex.Value;
			}
			set
			{
				_pickupIndex = value;
			}
		}
	}

	public PickupDisplay pickupDisplay;

	[SyncVar(hook = "SyncPickupIndex")]
	public PickupIndex pickupIndex = PickupIndex.none;

	[SyncVar(hook = "SyncRecycled")]
	public bool Recycled;

	public bool selfDestructIfPickupIndexIsNotIdeal;

	public SerializablePickupIndex idealPickupIndex;

	private static readonly PickupMessage pickupMessageInstance = new PickupMessage();

	public float waitDuration = 0.5f;

	private Run.FixedTimeStamp waitStartTime;

	private bool consumed;

	public const string pickupSoundString = "Play_UI_item_pickup";

	private static GameObject pickupPrefab;

	public PickupIndex NetworkpickupIndex
	{
		get
		{
			return pickupIndex;
		}
		[param: In]
		set
		{
			if (NetworkServer.localClientActive && !((NetworkBehaviour)this).syncVarHookGuard)
			{
				((NetworkBehaviour)this).syncVarHookGuard = true;
				SyncPickupIndex(value);
				((NetworkBehaviour)this).syncVarHookGuard = false;
			}
			((NetworkBehaviour)this).SetSyncVar<PickupIndex>(value, ref pickupIndex, 1u);
		}
	}

	public bool NetworkRecycled
	{
		get
		{
			return Recycled;
		}
		[param: In]
		set
		{
			if (NetworkServer.localClientActive && !((NetworkBehaviour)this).syncVarHookGuard)
			{
				((NetworkBehaviour)this).syncVarHookGuard = true;
				SyncRecycled(value);
				((NetworkBehaviour)this).syncVarHookGuard = false;
			}
			((NetworkBehaviour)this).SetSyncVar<bool>(value, ref Recycled, 2u);
		}
	}

	private void SyncPickupIndex(PickupIndex newPickupIndex)
	{
		NetworkpickupIndex = newPickupIndex;
		UpdatePickupDisplay();
	}

	private void SyncRecycled(bool isRecycled)
	{
		NetworkRecycled = isRecycled;
	}

	[Server]
	public static void SendPickupMessage(CharacterMaster master, PickupIndex pickupIndex)
	{
		if (!NetworkServer.active)
		{
			Debug.LogWarning((object)"[Server] function 'System.Void RoR2.GenericPickupController::SendPickupMessage(RoR2.CharacterMaster,RoR2.PickupIndex)' called on client");
			return;
		}
		uint pickupQuantity = 1u;
		if (Object.op_Implicit((Object)(object)master.inventory))
		{
			ItemIndex itemIndex = PickupCatalog.GetPickupDef(pickupIndex)?.itemIndex ?? ItemIndex.None;
			if (itemIndex != ItemIndex.None)
			{
				pickupQuantity = (uint)master.inventory.GetItemCount(itemIndex);
			}
		}
		PickupMessage pickupMessage = new PickupMessage
		{
			masterGameObject = ((Component)master).gameObject,
			pickupIndex = pickupIndex,
			pickupQuantity = pickupQuantity
		};
		NetworkServer.SendByChannelToAll((short)57, (MessageBase)(object)pickupMessage, QosChannelIndex.chat.intVal);
	}

	[NetworkMessageHandler(msgType = 57, client = true)]
	private static void HandlePickupMessage(NetworkMessage netMsg)
	{
		//IL_0123: Unknown result type (might be due to invalid IL or missing references)
		//IL_012b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0130: Unknown result type (might be due to invalid IL or missing references)
		PickupMessage pickupMessage = pickupMessageInstance;
		netMsg.ReadMessage<PickupMessage>(pickupMessage);
		GameObject masterGameObject = pickupMessage.masterGameObject;
		PickupIndex pickupIndex = pickupMessage.pickupIndex;
		PickupDef pickupDef = PickupCatalog.GetPickupDef(pickupIndex);
		uint pickupQuantity = pickupMessage.pickupQuantity;
		pickupMessage.Reset();
		if (!Object.op_Implicit((Object)(object)masterGameObject))
		{
			return;
		}
		CharacterMaster component = masterGameObject.GetComponent<CharacterMaster>();
		if (!Object.op_Implicit((Object)(object)component))
		{
			return;
		}
		PlayerCharacterMasterController component2 = ((Component)component).GetComponent<PlayerCharacterMasterController>();
		if (Object.op_Implicit((Object)(object)component2))
		{
			NetworkUser networkUser = component2.networkUser;
			if (Object.op_Implicit((Object)(object)networkUser))
			{
				networkUser.localUser?.userProfile.DiscoverPickup(pickupIndex);
			}
		}
		CharacterBody body = component.GetBody();
		Object.op_Implicit((Object)(object)body);
		ItemDef itemDef = ItemCatalog.GetItemDef(pickupDef?.itemIndex ?? ItemIndex.None);
		if ((Object)(object)itemDef != (Object)null && itemDef.hidden)
		{
			return;
		}
		if (pickupIndex != PickupIndex.none)
		{
			ItemIndex transformedItemIndex = ContagiousItemManager.GetTransformedItemIndex(itemDef?.itemIndex ?? ItemIndex.None);
			if ((Object)(object)itemDef == (Object)null || transformedItemIndex == ItemIndex.None || component.inventory.GetItemCount(transformedItemIndex) <= 0)
			{
				CharacterMasterNotificationQueue.PushPickupNotification(component, pickupIndex);
			}
		}
		Chat.AddPickupMessage(body, pickupDef?.nameToken ?? PickupCatalog.invalidPickupToken, Color32.op_Implicit(pickupDef?.baseColor ?? Color.black), pickupQuantity);
		if (Object.op_Implicit((Object)(object)body))
		{
			Util.PlaySound("Play_UI_item_pickup", ((Component)body).gameObject);
		}
	}

	public void StartWaitTime()
	{
		waitStartTime = Run.FixedTimeStamp.now;
	}

	private void OnTriggerStay(Collider other)
	{
		if (!NetworkServer.active || !(waitStartTime.timeSince >= waitDuration) || consumed)
		{
			return;
		}
		CharacterBody component = ((Component)other).GetComponent<CharacterBody>();
		if (!Object.op_Implicit((Object)(object)component))
		{
			return;
		}
		PickupDef pickupDef = PickupCatalog.GetPickupDef(pickupIndex);
		ItemIndex itemIndex = pickupDef?.itemIndex ?? ItemIndex.None;
		if (itemIndex != ItemIndex.None)
		{
			ItemTierDef itemTierDef = ItemTierCatalog.GetItemTierDef(ItemCatalog.GetItemDef(itemIndex).tier);
			if (Object.op_Implicit((Object)(object)itemTierDef) && (itemTierDef.pickupRules == ItemTierDef.PickupRules.ConfirmAll || (itemTierDef.pickupRules == ItemTierDef.PickupRules.ConfirmFirst && Object.op_Implicit((Object)(object)component.inventory) && component.inventory.GetItemCount(itemIndex) <= 0)))
			{
				return;
			}
		}
		EquipmentIndex equipmentIndex = pickupDef?.equipmentIndex ?? EquipmentIndex.None;
		if ((equipmentIndex == EquipmentIndex.None || (!EquipmentCatalog.GetEquipmentDef(equipmentIndex).isLunar && (!Object.op_Implicit((Object)(object)component.inventory) || component.inventory.currentEquipmentIndex == EquipmentIndex.None))) && (pickupDef == null || pickupDef.coinValue == 0) && BodyHasPickupPermission(component))
		{
			AttemptGrant(component);
		}
	}

	private static bool BodyHasPickupPermission(CharacterBody body)
	{
		if (Object.op_Implicit((Object)(object)(Object.op_Implicit((Object)(object)body.masterObject) ? body.masterObject.GetComponent<PlayerCharacterMasterController>() : null)))
		{
			return Object.op_Implicit((Object)(object)body.inventory);
		}
		return false;
	}

	public bool ShouldIgnoreSpherecastForInteractibility(Interactor activator)
	{
		return false;
	}

	public string GetContextString(Interactor activator)
	{
		return string.Format(Language.GetString(PickupCatalog.GetPickupDef(pickupIndex)?.interactContextToken ?? string.Empty), GetDisplayName());
	}

	private void UpdatePickupDisplay()
	{
		if (!Object.op_Implicit((Object)(object)pickupDisplay))
		{
			return;
		}
		pickupDisplay.SetPickupIndex(pickupIndex);
		if (Object.op_Implicit((Object)(object)pickupDisplay.modelRenderer))
		{
			Highlight component = ((Component)this).GetComponent<Highlight>();
			if (Object.op_Implicit((Object)(object)component))
			{
				component.targetRenderer = pickupDisplay.modelRenderer;
			}
		}
	}

	[Server]
	private void AttemptGrant(CharacterBody body)
	{
		if (!NetworkServer.active)
		{
			Debug.LogWarning((object)"[Server] function 'System.Void RoR2.GenericPickupController::AttemptGrant(RoR2.CharacterBody)' called on client");
			return;
		}
		TeamComponent component = ((Component)body).GetComponent<TeamComponent>();
		if (!Object.op_Implicit((Object)(object)component) || component.teamIndex != TeamIndex.Player)
		{
			return;
		}
		PickupDef pickupDef = PickupCatalog.GetPickupDef(pickupIndex);
		if (Object.op_Implicit((Object)(object)body.inventory) && pickupDef != null)
		{
			PickupDef.GrantContext grantContext = default(PickupDef.GrantContext);
			grantContext.body = body;
			grantContext.controller = this;
			PickupDef.GrantContext context = grantContext;
			pickupDef.attemptGrant?.Invoke(ref context);
			if (context.shouldNotify)
			{
				SendPickupMessage(body.master, pickupDef.pickupIndex);
			}
			if (context.shouldDestroy)
			{
				consumed = true;
				Object.Destroy((Object)(object)((Component)this).gameObject);
			}
		}
	}

	private void Start()
	{
		waitStartTime = Run.FixedTimeStamp.now;
		consumed = false;
		UpdatePickupDisplay();
	}

	private void OnEnable()
	{
		InstanceTracker.Add<GenericPickupController>(this);
	}

	private void OnDisable()
	{
		InstanceTracker.Remove<GenericPickupController>(this);
	}

	public Interactability GetInteractability(Interactor activator)
	{
		if (!((Behaviour)this).enabled)
		{
			return Interactability.Disabled;
		}
		if (waitStartTime.timeSince < waitDuration || consumed)
		{
			return Interactability.Disabled;
		}
		CharacterBody component = ((Component)activator).GetComponent<CharacterBody>();
		if (Object.op_Implicit((Object)(object)component))
		{
			if (!BodyHasPickupPermission(component))
			{
				return Interactability.Disabled;
			}
			return Interactability.Available;
		}
		return Interactability.Disabled;
	}

	public void OnInteractionBegin(Interactor activator)
	{
		AttemptGrant(((Component)activator).GetComponent<CharacterBody>());
	}

	public bool ShouldShowOnScanner()
	{
		return true;
	}

	public string GetDisplayName()
	{
		return Language.GetString(PickupCatalog.GetPickupDef(pickupIndex)?.nameToken ?? PickupCatalog.invalidPickupToken);
	}

	public void SetPickupIndexFromString(string pickupString)
	{
		if (NetworkServer.active)
		{
			PickupIndex networkpickupIndex = PickupCatalog.FindPickupIndex(pickupString);
			NetworkpickupIndex = networkpickupIndex;
		}
	}

	[SystemInitializer(new Type[] { })]
	private static void Init()
	{
		pickupPrefab = LegacyResourcesAPI.Load<GameObject>("Prefabs/NetworkedObjects/GenericPickup");
	}

	public static GenericPickupController CreatePickup(in CreatePickupInfo createPickupInfo)
	{
		//IL_0010: Unknown result type (might be due to invalid IL or missing references)
		//IL_0016: Unknown result type (might be due to invalid IL or missing references)
		GameObject obj = Object.Instantiate<GameObject>(createPickupInfo.prefabOverride ?? pickupPrefab, createPickupInfo.position, createPickupInfo.rotation);
		GenericPickupController component = obj.GetComponent<GenericPickupController>();
		if (Object.op_Implicit((Object)(object)component))
		{
			component.NetworkpickupIndex = createPickupInfo.pickupIndex;
		}
		PickupIndexNetworker component2 = obj.GetComponent<PickupIndexNetworker>();
		if (Object.op_Implicit((Object)(object)component2))
		{
			component2.NetworkpickupIndex = createPickupInfo.pickupIndex;
		}
		PickupPickerController component3 = obj.GetComponent<PickupPickerController>();
		if (Object.op_Implicit((Object)(object)component3) && createPickupInfo.pickerOptions != null)
		{
			component3.SetOptionsServer(createPickupInfo.pickerOptions);
		}
		NetworkServer.Spawn(obj);
		return component;
	}

	[ContextMenu("Print Pickup Index")]
	private void PrintPickupIndex()
	{
		Debug.LogFormat("pickupIndex={0}", new object[1] { PickupCatalog.GetPickupDef(pickupIndex)?.internalName ?? "Invalid" });
	}

	private void UNetVersion()
	{
	}

	public override bool OnSerialize(NetworkWriter writer, bool forceAll)
	{
		if (forceAll)
		{
			GeneratedNetworkCode._WritePickupIndex_None(writer, pickupIndex);
			writer.Write(Recycled);
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
		if ((((NetworkBehaviour)this).syncVarDirtyBits & 2u) != 0)
		{
			if (!flag)
			{
				writer.WritePackedUInt32(((NetworkBehaviour)this).syncVarDirtyBits);
				flag = true;
			}
			writer.Write(Recycled);
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
			Recycled = reader.ReadBoolean();
			return;
		}
		int num = (int)reader.ReadPackedUInt32();
		if (((uint)num & (true ? 1u : 0u)) != 0)
		{
			SyncPickupIndex(GeneratedNetworkCode._ReadPickupIndex_None(reader));
		}
		if (((uint)num & 2u) != 0)
		{
			SyncRecycled(reader.ReadBoolean());
		}
	}

	public override void PreStartClient()
	{
	}
}
