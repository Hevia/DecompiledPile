using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using RoR2.Stats;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.Networking;

namespace RoR2;

[RequireComponent(typeof(Highlight))]
public sealed class PurchaseInteraction : NetworkBehaviour, IInteractable, IHologramContentProvider, IDisplayNameProvider
{
	[SyncVar]
	public string displayNameToken;

	[SyncVar]
	public string contextToken;

	public CostTypeIndex costType;

	[SyncVar]
	public bool available = true;

	[SyncVar]
	public int cost;

	public bool automaticallyScaleCostWithDifficulty;

	[Tooltip("The unlockable that a player must have to be able to interact with this terminal.")]
	public string requiredUnlockable = "";

	public bool ignoreSpherecastForInteractability;

	public string[] purchaseStatNames;

	public bool setUnavailableOnTeleporterActivated;

	public bool isShrine;

	public bool isGoldShrine;

	[HideInInspector]
	public Interactor lastActivator;

	[SyncVar]
	public GameObject lockGameObject;

	private Xoroshiro128Plus rng;

	private static readonly StringBuilder sharedStringBuilder = new StringBuilder();

	public PurchaseEvent onPurchase;

	private NetworkInstanceId ___lockGameObjectNetId;

	public string NetworkdisplayNameToken
	{
		get
		{
			return displayNameToken;
		}
		[param: In]
		set
		{
			((NetworkBehaviour)this).SetSyncVar<string>(value, ref displayNameToken, 1u);
		}
	}

	public string NetworkcontextToken
	{
		get
		{
			return contextToken;
		}
		[param: In]
		set
		{
			((NetworkBehaviour)this).SetSyncVar<string>(value, ref contextToken, 2u);
		}
	}

	public bool Networkavailable
	{
		get
		{
			return available;
		}
		[param: In]
		set
		{
			((NetworkBehaviour)this).SetSyncVar<bool>(value, ref available, 4u);
		}
	}

	public int Networkcost
	{
		get
		{
			return cost;
		}
		[param: In]
		set
		{
			((NetworkBehaviour)this).SetSyncVar<int>(value, ref cost, 8u);
		}
	}

	public GameObject NetworklockGameObject
	{
		get
		{
			return lockGameObject;
		}
		[param: In]
		set
		{
			((NetworkBehaviour)this).SetSyncVarGameObject(value, ref lockGameObject, 16u, ref ___lockGameObjectNetId);
		}
	}

	public static event Action<PurchaseInteraction, Interactor> onItemSpentOnPurchase;

	public static event Action<PurchaseInteraction, Interactor, EquipmentIndex> onEquipmentSpentOnPurchase;

	private void Awake()
	{
		//IL_0035: Unknown result type (might be due to invalid IL or missing references)
		//IL_003f: Expected O, but got Unknown
		if (NetworkServer.active)
		{
			if (automaticallyScaleCostWithDifficulty)
			{
				Networkcost = Run.instance.GetDifficultyScaledCost(cost);
			}
			rng = new Xoroshiro128Plus(Run.instance.treasureRng.nextUlong);
		}
	}

	[Server]
	public void SetAvailable(bool newAvailable)
	{
		if (!NetworkServer.active)
		{
			Debug.LogWarning((object)"[Server] function 'System.Void RoR2.PurchaseInteraction::SetAvailable(System.Boolean)' called on client");
		}
		else
		{
			Networkavailable = newAvailable;
		}
	}

	[Server]
	public void SetUnavailableTemporarily(float time)
	{
		if (!NetworkServer.active)
		{
			Debug.LogWarning((object)"[Server] function 'System.Void RoR2.PurchaseInteraction::SetUnavailableTemporarily(System.Single)' called on client");
			return;
		}
		Networkavailable = false;
		((MonoBehaviour)this).Invoke("SetAvailableTrue", time);
	}

	private void SetAvailableTrue()
	{
		Networkavailable = true;
	}

	public string GetDisplayName()
	{
		return Language.GetString(displayNameToken);
	}

	private static bool ActivatorHasUnlockable(Interactor activator, string unlockableName)
	{
		NetworkUser networkUser = Util.LookUpBodyNetworkUser(((Component)activator).gameObject);
		if (Object.op_Implicit((Object)(object)networkUser))
		{
			LocalUser localUser = networkUser.localUser;
			if (localUser != null)
			{
				return localUser.userProfile.HasUnlockable(unlockableName);
			}
		}
		return true;
	}

	public string GetContextString(Interactor activator)
	{
		sharedStringBuilder.Clear();
		sharedStringBuilder.Append(Language.GetString(contextToken));
		if (costType != 0)
		{
			sharedStringBuilder.Append(" <nobr>(");
			CostTypeCatalog.GetCostTypeDef(costType).BuildCostStringStyled(cost, sharedStringBuilder, forWorldDisplay: false);
			sharedStringBuilder.Append(")</nobr>");
		}
		return sharedStringBuilder.ToString();
	}

	public Interactability GetInteractability(Interactor activator)
	{
		if (!string.IsNullOrEmpty(requiredUnlockable) && !ActivatorHasUnlockable(activator, requiredUnlockable))
		{
			return Interactability.Disabled;
		}
		if (!available || Object.op_Implicit((Object)(object)lockGameObject))
		{
			return Interactability.Disabled;
		}
		if (!CanBeAffordedByInteractor(activator))
		{
			return Interactability.ConditionsNotMet;
		}
		return Interactability.Available;
	}

	public bool CanBeAffordedByInteractor(Interactor activator)
	{
		return CostTypeCatalog.GetCostTypeDef(costType).IsAffordable(cost, activator);
	}

	public void OnInteractionBegin(Interactor activator)
	{
		//IL_0078: Unknown result type (might be due to invalid IL or missing references)
		if (!CanBeAffordedByInteractor(activator))
		{
			return;
		}
		CharacterBody component = ((Component)activator).GetComponent<CharacterBody>();
		CostTypeDef costTypeDef = CostTypeCatalog.GetCostTypeDef(costType);
		ItemIndex itemIndex = ItemIndex.None;
		ShopTerminalBehavior component2 = ((Component)this).GetComponent<ShopTerminalBehavior>();
		if (Object.op_Implicit((Object)(object)component2))
		{
			itemIndex = PickupCatalog.GetPickupDef(component2.CurrentPickupIndex())?.itemIndex ?? ItemIndex.None;
		}
		CostTypeDef.PayCostResults payCostResults = costTypeDef.PayCost(cost, activator, ((Component)this).gameObject, rng, itemIndex);
		foreach (ItemIndex item in payCostResults.itemsTaken)
		{
			CreateItemTakenOrb(component.corePosition, ((Component)this).gameObject, item);
			if (item != itemIndex)
			{
				PurchaseInteraction.onItemSpentOnPurchase?.Invoke(this, activator);
			}
		}
		foreach (EquipmentIndex item2 in payCostResults.equipmentTaken)
		{
			PurchaseInteraction.onEquipmentSpentOnPurchase?.Invoke(this, activator, item2);
		}
		IEnumerable<StatDef> statDefsToIncrement = purchaseStatNames.Select(StatDef.Find);
		StatManager.OnPurchase(component, costType, statDefsToIncrement);
		((UnityEvent<Interactor>)onPurchase).Invoke(activator);
		lastActivator = activator;
	}

	[Server]
	public static void CreateItemTakenOrb(Vector3 effectOrigin, GameObject targetObject, ItemIndex itemIndex)
	{
		//IL_0025: Unknown result type (might be due to invalid IL or missing references)
		if (!NetworkServer.active)
		{
			Debug.LogWarning((object)"[Server] function 'System.Void RoR2.PurchaseInteraction::CreateItemTakenOrb(UnityEngine.Vector3,UnityEngine.GameObject,RoR2.ItemIndex)' called on client");
			return;
		}
		GameObject effectPrefab = LegacyResourcesAPI.Load<GameObject>("Prefabs/Effects/OrbEffects/ItemTakenOrbEffect");
		EffectData effectData = new EffectData
		{
			origin = effectOrigin,
			genericFloat = 1.5f,
			genericUInt = (uint)(itemIndex + 1)
		};
		effectData.SetNetworkedObjectReference(targetObject);
		EffectManager.SpawnEffect(effectPrefab, effectData, transmit: true);
	}

	public bool ShouldDisplayHologram(GameObject viewer)
	{
		return available;
	}

	public GameObject GetHologramContentPrefab()
	{
		return LegacyResourcesAPI.Load<GameObject>("Prefabs/CostHologramContent");
	}

	public void UpdateHologramContent(GameObject hologramContentObject)
	{
		CostHologramContent component = hologramContentObject.GetComponent<CostHologramContent>();
		if (Object.op_Implicit((Object)(object)component))
		{
			component.displayValue = cost;
			component.costType = costType;
		}
	}

	public bool ShouldIgnoreSpherecastForInteractibility(Interactor activator)
	{
		return false;
	}

	public bool ShouldShowOnScanner()
	{
		return available;
	}

	public void ScaleCost(float scalar)
	{
		Networkcost = (int)Mathf.Floor((float)cost * scalar);
	}

	private void OnEnable()
	{
		InstanceTracker.Add<PurchaseInteraction>(this);
	}

	private void OnDisable()
	{
		InstanceTracker.Remove<PurchaseInteraction>(this);
	}

	[RuntimeInitializeOnLoadMethod]
	private static void Init()
	{
		TeleporterInteraction.onTeleporterBeginChargingGlobal += OnTeleporterBeginCharging;
	}

	private static void OnTeleporterBeginCharging(TeleporterInteraction teleporterInteraction)
	{
		if (!NetworkServer.active)
		{
			return;
		}
		foreach (PurchaseInteraction instances in InstanceTracker.GetInstancesList<PurchaseInteraction>())
		{
			if (instances.setUnavailableOnTeleporterActivated)
			{
				instances.SetAvailable(newAvailable: false);
				((MonoBehaviour)instances).CancelInvoke("SetUnavailableTemporarily");
			}
		}
	}

	private void UNetVersion()
	{
	}

	public override bool OnSerialize(NetworkWriter writer, bool forceAll)
	{
		if (forceAll)
		{
			writer.Write(displayNameToken);
			writer.Write(contextToken);
			writer.Write(available);
			writer.WritePackedUInt32((uint)cost);
			writer.Write(lockGameObject);
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
			writer.Write(displayNameToken);
		}
		if ((((NetworkBehaviour)this).syncVarDirtyBits & 2u) != 0)
		{
			if (!flag)
			{
				writer.WritePackedUInt32(((NetworkBehaviour)this).syncVarDirtyBits);
				flag = true;
			}
			writer.Write(contextToken);
		}
		if ((((NetworkBehaviour)this).syncVarDirtyBits & 4u) != 0)
		{
			if (!flag)
			{
				writer.WritePackedUInt32(((NetworkBehaviour)this).syncVarDirtyBits);
				flag = true;
			}
			writer.Write(available);
		}
		if ((((NetworkBehaviour)this).syncVarDirtyBits & 8u) != 0)
		{
			if (!flag)
			{
				writer.WritePackedUInt32(((NetworkBehaviour)this).syncVarDirtyBits);
				flag = true;
			}
			writer.WritePackedUInt32((uint)cost);
		}
		if ((((NetworkBehaviour)this).syncVarDirtyBits & 0x10u) != 0)
		{
			if (!flag)
			{
				writer.WritePackedUInt32(((NetworkBehaviour)this).syncVarDirtyBits);
				flag = true;
			}
			writer.Write(lockGameObject);
		}
		if (!flag)
		{
			writer.WritePackedUInt32(((NetworkBehaviour)this).syncVarDirtyBits);
		}
		return flag;
	}

	public override void OnDeserialize(NetworkReader reader, bool initialState)
	{
		//IL_0038: Unknown result type (might be due to invalid IL or missing references)
		//IL_003d: Unknown result type (might be due to invalid IL or missing references)
		if (initialState)
		{
			displayNameToken = reader.ReadString();
			contextToken = reader.ReadString();
			available = reader.ReadBoolean();
			cost = (int)reader.ReadPackedUInt32();
			___lockGameObjectNetId = reader.ReadNetworkId();
			return;
		}
		int num = (int)reader.ReadPackedUInt32();
		if (((uint)num & (true ? 1u : 0u)) != 0)
		{
			displayNameToken = reader.ReadString();
		}
		if (((uint)num & 2u) != 0)
		{
			contextToken = reader.ReadString();
		}
		if (((uint)num & 4u) != 0)
		{
			available = reader.ReadBoolean();
		}
		if (((uint)num & 8u) != 0)
		{
			cost = (int)reader.ReadPackedUInt32();
		}
		if (((uint)num & 0x10u) != 0)
		{
			lockGameObject = reader.ReadGameObject();
		}
	}

	public override void PreStartClient()
	{
		//IL_0012: Unknown result type (might be due to invalid IL or missing references)
		if (!((NetworkInstanceId)(ref ___lockGameObjectNetId)).IsEmpty())
		{
			NetworklockGameObject = ClientScene.FindLocalObject(___lockGameObjectNetId);
		}
	}
}
