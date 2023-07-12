using System;
using System.Collections.Generic;
using System.Linq;
using RoR2.UI;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.Networking;

namespace RoR2;

[RequireComponent(typeof(NetworkUIPromptController))]
public class PickupPickerController : NetworkBehaviour, IInteractable
{
	public struct Option
	{
		public PickupIndex pickupIndex;

		public bool available;
	}

	[Serializable]
	public class PickupIndexUnityEvent : UnityEvent<int>
	{
	}

	public GameObject panelPrefab;

	public PickupIndexUnityEvent onPickupSelected;

	public GenericInteraction.InteractorUnityEvent onServerInteractionBegin;

	public float cutoffDistance;

	public string contextString = "";

	private NetworkUIPromptController networkUIPromptController;

	private const byte msgSubmit = 0;

	private const byte msgCancel = 1;

	private GameObject panelInstance;

	private PickupPickerPanel panelInstanceController;

	private Option[] options = Array.Empty<Option>();

	private static readonly uint optionsDirtyBit = 1u;

	private static readonly uint allDirtyBits = optionsDirtyBit;

	public bool available { get; private set; } = true;


	private void Awake()
	{
		networkUIPromptController = ((Component)this).GetComponent<NetworkUIPromptController>();
		if (NetworkClient.active)
		{
			networkUIPromptController.onDisplayBegin += OnDisplayBegin;
			networkUIPromptController.onDisplayEnd += OnDisplayEnd;
		}
		if (NetworkServer.active)
		{
			networkUIPromptController.messageFromClientHandler = HandleClientMessage;
		}
	}

	private void OnEnable()
	{
		InstanceTracker.Add<PickupPickerController>(this);
	}

	private void OnDisable()
	{
		InstanceTracker.Remove<PickupPickerController>(this);
	}

	private void HandleClientMessage(NetworkReader reader)
	{
		switch (reader.ReadByte())
		{
		case 0:
		{
			int choiceIndex = reader.ReadInt32();
			HandlePickupSelected(choiceIndex);
			break;
		}
		case 1:
			networkUIPromptController.SetParticipantMaster(null);
			break;
		}
	}

	private void FixedUpdate()
	{
		if (NetworkServer.active)
		{
			FixedUpdateServer();
		}
	}

	private void FixedUpdateServer()
	{
		//IL_0029: Unknown result type (might be due to invalid IL or missing references)
		//IL_0034: Unknown result type (might be due to invalid IL or missing references)
		//IL_0039: Unknown result type (might be due to invalid IL or missing references)
		//IL_003e: Unknown result type (might be due to invalid IL or missing references)
		CharacterMaster currentParticipantMaster = networkUIPromptController.currentParticipantMaster;
		if (!Object.op_Implicit((Object)(object)currentParticipantMaster))
		{
			return;
		}
		CharacterBody body = currentParticipantMaster.GetBody();
		if (Object.op_Implicit((Object)(object)body))
		{
			Vector3 val = body.inputBank.aimOrigin - ((Component)this).transform.position;
			if (!(((Vector3)(ref val)).sqrMagnitude > cutoffDistance * cutoffDistance))
			{
				return;
			}
		}
		networkUIPromptController.SetParticipantMaster(null);
	}

	private void OnPanelDestroyed(OnDestroyCallback onDestroyCallback)
	{
		NetworkWriter val = networkUIPromptController.BeginMessageToServer();
		val.Write((byte)1);
		networkUIPromptController.FinishMessageToServer(val);
	}

	private void OnDisplayBegin(NetworkUIPromptController networkUIPromptController, LocalUser localUser, CameraRigController cameraRigController)
	{
		panelInstance = Object.Instantiate<GameObject>(panelPrefab, cameraRigController.hud.mainContainer.transform);
		panelInstanceController = panelInstance.GetComponent<PickupPickerPanel>();
		panelInstanceController.pickerController = this;
		panelInstanceController.SetPickupOptions(options);
		OnDestroyCallback.AddCallback(panelInstance, OnPanelDestroyed);
	}

	private void OnDisplayEnd(NetworkUIPromptController networkUIPromptController, LocalUser localUser, CameraRigController cameraRigController)
	{
		Object.Destroy((Object)(object)panelInstance);
		panelInstance = null;
		panelInstanceController = null;
	}

	[Server]
	public void SetAvailable(bool newAvailable)
	{
		if (!NetworkServer.active)
		{
			Debug.LogWarning((object)"[Server] function 'System.Void RoR2.PickupPickerController::SetAvailable(System.Boolean)' called on client");
		}
		else
		{
			available = newAvailable;
		}
	}

	public void SubmitChoice(int choiceIndex)
	{
		if (!NetworkServer.active)
		{
			NetworkWriter val = networkUIPromptController.BeginMessageToServer();
			val.Write((byte)0);
			val.Write(choiceIndex);
			networkUIPromptController.FinishMessageToServer(val);
		}
		else
		{
			HandlePickupSelected(choiceIndex);
		}
	}

	public bool IsChoiceAvailable(PickupIndex choice)
	{
		for (int i = 0; i < options.Length; i++)
		{
			ref Option reference = ref options[i];
			if (reference.pickupIndex == choice)
			{
				return reference.available;
			}
		}
		return false;
	}

	[Server]
	private void HandlePickupSelected(int choiceIndex)
	{
		if (!NetworkServer.active)
		{
			Debug.LogWarning((object)"[Server] function 'System.Void RoR2.PickupPickerController::HandlePickupSelected(System.Int32)' called on client");
		}
		else if ((uint)choiceIndex < options.Length)
		{
			ref Option reference = ref options[choiceIndex];
			if (reference.available)
			{
				((UnityEvent<int>)onPickupSelected)?.Invoke(reference.pickupIndex.value);
			}
		}
	}

	[Server]
	public void SetOptionsServer(Option[] newOptions)
	{
		if (!NetworkServer.active)
		{
			Debug.LogWarning((object)"[Server] function 'System.Void RoR2.PickupPickerController::SetOptionsServer(RoR2.PickupPickerController/Option[])' called on client");
		}
		else
		{
			SetOptionsInternal(newOptions);
		}
	}

	private void SetOptionsInternal(Option[] newOptions)
	{
		Array.Resize(ref options, newOptions.Length);
		Array.Copy(newOptions, options, newOptions.Length);
		if (Object.op_Implicit((Object)(object)panelInstanceController))
		{
			panelInstanceController.SetPickupOptions(options);
		}
		if (NetworkServer.active)
		{
			((NetworkBehaviour)this).SetDirtyBit(optionsDirtyBit);
		}
	}

	[Server]
	public void SetTestOptions()
	{
		if (!NetworkServer.active)
		{
			Debug.LogWarning((object)"[Server] function 'System.Void RoR2.PickupPickerController::SetTestOptions()' called on client");
			return;
		}
		SetOptionsServer((from itemIndex in ItemCatalog.allItems
			select ItemCatalog.GetItemDef(itemIndex) into itemDef
			where itemDef.tier == ItemTier.Tier2
			select itemDef).Select(delegate(ItemDef itemDef)
		{
			Option result = default(Option);
			result.pickupIndex = PickupCatalog.FindPickupIndex(itemDef.itemIndex);
			result.available = true;
			return result;
		}).ToArray());
	}

	public static Option[] GenerateOptionsFromDropTable(int numOptions, PickupDropTable dropTable, Xoroshiro128Plus rng)
	{
		PickupIndex[] array = dropTable.GenerateUniqueDrops(numOptions, rng);
		Option[] array2 = new Option[array.Length];
		for (int i = 0; i < array2.Length; i++)
		{
			array2[i] = new Option
			{
				available = true,
				pickupIndex = array[i]
			};
		}
		return array2;
	}

	private static Option[] GetOptionsFromPickupIndex(PickupIndex pickupIndex)
	{
		PickupIndex[] groupFromPickupIndex = PickupTransmutationManager.GetGroupFromPickupIndex(pickupIndex);
		if (groupFromPickupIndex == null)
		{
			return new Option[1]
			{
				new Option
				{
					available = true,
					pickupIndex = pickupIndex
				}
			};
		}
		Option[] array = new Option[groupFromPickupIndex.Length];
		for (int i = 0; i < groupFromPickupIndex.Length; i++)
		{
			PickupIndex pickupIndex2 = groupFromPickupIndex[i];
			array[i] = new Option
			{
				available = Run.instance.IsPickupAvailable(pickupIndex2),
				pickupIndex = pickupIndex2
			};
		}
		return array;
	}

	public static Option[] GenerateOptionsFromArray(PickupIndex[] drops)
	{
		Option[] array = new Option[drops.Length];
		for (int i = 0; i < array.Length; i++)
		{
			array[i] = new Option
			{
				available = true,
				pickupIndex = drops[i]
			};
		}
		return array;
	}

	[Server]
	public void SetOptionsFromPickupForCommandArtifact(PickupIndex pickupIndex)
	{
		if (!NetworkServer.active)
		{
			Debug.LogWarning((object)"[Server] function 'System.Void RoR2.PickupPickerController::SetOptionsFromPickupForCommandArtifact(RoR2.PickupIndex)' called on client");
		}
		else
		{
			SetOptionsServer(GetOptionsFromPickupIndex(pickupIndex));
		}
	}

	[Server]
	public void CreatePickup(int intPickupIndex)
	{
		if (!NetworkServer.active)
		{
			Debug.LogWarning((object)"[Server] function 'System.Void RoR2.PickupPickerController::CreatePickup(System.Int32)' called on client");
		}
		else
		{
			CreatePickup(new PickupIndex(intPickupIndex));
		}
	}

	[Server]
	public void CreatePickup(PickupIndex pickupIndex)
	{
		//IL_001f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0024: Unknown result type (might be due to invalid IL or missing references)
		//IL_0031: Unknown result type (might be due to invalid IL or missing references)
		//IL_0036: Unknown result type (might be due to invalid IL or missing references)
		if (!NetworkServer.active)
		{
			Debug.LogWarning((object)"[Server] function 'System.Void RoR2.PickupPickerController::CreatePickup(RoR2.PickupIndex)' called on client");
			return;
		}
		GenericPickupController.CreatePickupInfo createPickupInfo = default(GenericPickupController.CreatePickupInfo);
		createPickupInfo.rotation = Quaternion.identity;
		createPickupInfo.position = ((Component)this).transform.position;
		createPickupInfo.pickupIndex = pickupIndex;
		GenericPickupController.CreatePickupInfo createPickupInfo2 = createPickupInfo;
		GenericPickupController.CreatePickup(in createPickupInfo2);
	}

	public override bool OnSerialize(NetworkWriter writer, bool initialState)
	{
		uint syncVarDirtyBits = ((NetworkBehaviour)this).syncVarDirtyBits;
		if (initialState)
		{
			syncVarDirtyBits = allDirtyBits;
		}
		bool num = (syncVarDirtyBits & optionsDirtyBit) != 0;
		writer.WritePackedUInt32(syncVarDirtyBits);
		if (num)
		{
			writer.WritePackedUInt32((uint)options.Length);
			for (int i = 0; i < options.Length; i++)
			{
				ref Option reference = ref options[i];
				writer.Write(reference.pickupIndex);
				writer.Write(reference.available);
			}
		}
		return syncVarDirtyBits != 0;
	}

	public override void OnDeserialize(NetworkReader reader, bool initialState)
	{
		if ((reader.ReadPackedUInt32() & optionsDirtyBit) != 0)
		{
			Option[] array = new Option[reader.ReadPackedUInt32()];
			for (int i = 0; i < array.Length; i++)
			{
				ref Option reference = ref array[i];
				reference.pickupIndex = reader.ReadPickupIndex();
				reference.available = reader.ReadBoolean();
			}
			SetOptionsInternal(array);
		}
	}

	public string GetContextString(Interactor activator)
	{
		return Language.GetString(contextString);
	}

	public Interactability GetInteractability(Interactor activator)
	{
		if (networkUIPromptController.inUse)
		{
			return Interactability.ConditionsNotMet;
		}
		if (!available)
		{
			return Interactability.Disabled;
		}
		return Interactability.Available;
	}

	public void OnInteractionBegin(Interactor activator)
	{
		((UnityEvent<Interactor>)onServerInteractionBegin).Invoke(activator);
		networkUIPromptController.SetParticipantMasterFromInteractor(activator);
	}

	public void SetOptionsFromInteractor(Interactor activator)
	{
		if (!Object.op_Implicit((Object)(object)activator))
		{
			Debug.Log((object)"No activator.");
			return;
		}
		CharacterBody component = ((Component)activator).GetComponent<CharacterBody>();
		if (!Object.op_Implicit((Object)(object)component))
		{
			Debug.Log((object)"No body.");
			return;
		}
		Inventory inventory = component.inventory;
		if (!Object.op_Implicit((Object)(object)inventory))
		{
			Debug.Log((object)"No inventory.");
			return;
		}
		List<Option> list = new List<Option>();
		for (int i = 0; i < inventory.itemAcquisitionOrder.Count; i++)
		{
			ItemIndex itemIndex = inventory.itemAcquisitionOrder[i];
			ItemDef itemDef = ItemCatalog.GetItemDef(itemIndex);
			ItemTierDef itemTierDef = ItemTierCatalog.GetItemTierDef(itemDef.tier);
			PickupIndex pickupIndex = PickupCatalog.FindPickupIndex(itemIndex);
			if ((!Object.op_Implicit((Object)(object)itemTierDef) || itemTierDef.canScrap) && itemDef.canRemove && !itemDef.hidden && itemDef.DoesNotContainTag(ItemTag.Scrap))
			{
				list.Add(new Option
				{
					available = true,
					pickupIndex = pickupIndex
				});
			}
		}
		Debug.Log((object)list.Count);
		SetOptionsServer(list.ToArray());
	}

	public bool ShouldIgnoreSpherecastForInteractibility(Interactor activator)
	{
		return false;
	}

	public bool ShouldShowOnScanner()
	{
		return true;
	}

	private void UNetVersion()
	{
	}

	public override void PreStartClient()
	{
	}
}
