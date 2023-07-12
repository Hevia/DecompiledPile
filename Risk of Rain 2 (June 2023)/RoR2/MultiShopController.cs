using System.Runtime.InteropServices;
using HG;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.Networking;

namespace RoR2;

public class MultiShopController : NetworkBehaviour, IHologramContentProvider
{
	[Tooltip("The shop terminal prefab to instantiate.")]
	public GameObject terminalPrefab;

	[Tooltip("The positions at which to instantiate shop terminals.")]
	public Transform[] terminalPositions;

	[Tooltip("The number of terminals guaranteed to have their item revealed")]
	public int revealCount = 1;

	[Tooltip("The percentage chance that terminals after the reveal count will be hidden")]
	public float hiddenChance = 0.2f;

	[Tooltip("The tier of items to drop")]
	[Header("Deprecated")]
	public ItemTier itemTier;

	public bool doEquipmentInstead;

	[Tooltip("Whether or not there's a chance the item contents are replaced with a '?'")]
	private bool hideDisplayContent = true;

	private bool[] doCloseOnTerminalPurchase;

	private GameObject[] _terminalGameObjects;

	public ReadOnlyArray<GameObject> terminalGameObjects;

	[SyncVar]
	private bool available = true;

	public int baseCost;

	public CostTypeIndex costType;

	[SyncVar]
	private int cost;

	private Xoroshiro128Plus rng;

	public bool Networkavailable
	{
		get
		{
			return available;
		}
		[param: In]
		set
		{
			((NetworkBehaviour)this).SetSyncVar<bool>(value, ref available, 1u);
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
			((NetworkBehaviour)this).SetSyncVar<int>(value, ref cost, 2u);
		}
	}

	private void Start()
	{
		//IL_0023: Unknown result type (might be due to invalid IL or missing references)
		//IL_002d: Expected O, but got Unknown
		if (!Object.op_Implicit((Object)(object)Run.instance) || !NetworkServer.active)
		{
			return;
		}
		rng = new Xoroshiro128Plus(Run.instance.treasureRng.nextUlong);
		CreateTerminals();
		Networkcost = Run.instance.GetDifficultyScaledCost(baseCost);
		if (_terminalGameObjects != null)
		{
			GameObject[] array = _terminalGameObjects;
			for (int i = 0; i < array.Length; i++)
			{
				PurchaseInteraction component = array[i].GetComponent<PurchaseInteraction>();
				component.Networkcost = cost;
				component.costType = costType;
			}
		}
	}

	private void OnDestroy()
	{
		if (_terminalGameObjects != null)
		{
			for (int num = _terminalGameObjects.Length - 1; num >= 0; num--)
			{
				Object.Destroy((Object)(object)_terminalGameObjects[num]);
			}
			_terminalGameObjects = null;
		}
	}

	private void CreateTerminals()
	{
		//IL_002d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0032: Unknown result type (might be due to invalid IL or missing references)
		//IL_0055: Unknown result type (might be due to invalid IL or missing references)
		//IL_0062: Unknown result type (might be due to invalid IL or missing references)
		doCloseOnTerminalPurchase = new bool[terminalPositions.Length];
		_terminalGameObjects = (GameObject[])(object)new GameObject[terminalPositions.Length];
		terminalGameObjects = new ReadOnlyArray<GameObject>(_terminalGameObjects);
		for (int i = 0; i < terminalPositions.Length; i++)
		{
			doCloseOnTerminalPurchase[i] = true;
			GameObject val = Object.Instantiate<GameObject>(terminalPrefab, terminalPositions[i].position, terminalPositions[i].rotation);
			_terminalGameObjects[i] = val;
			ShopTerminalBehavior component = val.GetComponent<ShopTerminalBehavior>();
			component.serverMultiShopController = this;
			if (!component.selfGeneratePickup)
			{
				PickupIndex newPickupIndex = PickupIndex.none;
				if (doEquipmentInstead)
				{
					newPickupIndex = rng.NextElementUniform<PickupIndex>(Run.instance.availableEquipmentDropList);
				}
				else
				{
					switch (itemTier)
					{
					case ItemTier.Tier1:
						newPickupIndex = rng.NextElementUniform<PickupIndex>(Run.instance.availableTier1DropList);
						break;
					case ItemTier.Tier2:
						newPickupIndex = rng.NextElementUniform<PickupIndex>(Run.instance.availableTier2DropList);
						break;
					case ItemTier.Tier3:
						newPickupIndex = rng.NextElementUniform<PickupIndex>(Run.instance.availableTier3DropList);
						break;
					case ItemTier.Lunar:
						newPickupIndex = rng.NextElementUniform<PickupIndex>(Run.instance.availableLunarCombinedDropList);
						break;
					case ItemTier.VoidTier1:
						newPickupIndex = rng.NextElementUniform<PickupIndex>(Run.instance.availableVoidTier1DropList);
						break;
					case ItemTier.VoidTier2:
						newPickupIndex = rng.NextElementUniform<PickupIndex>(Run.instance.availableVoidTier2DropList);
						break;
					case ItemTier.VoidTier3:
						newPickupIndex = rng.NextElementUniform<PickupIndex>(Run.instance.availableVoidTier3DropList);
						break;
					case ItemTier.VoidBoss:
						newPickupIndex = rng.NextElementUniform<PickupIndex>(Run.instance.availableVoidBossDropList);
						break;
					}
				}
				bool newHidden = hideDisplayContent && i >= revealCount && rng.nextNormalizedFloat < hiddenChance;
				component.SetPickupIndex(newPickupIndex, newHidden);
			}
			else
			{
				component.SetHidden(i >= revealCount && rng.nextNormalizedFloat < hiddenChance);
			}
			NetworkServer.Spawn(val);
		}
		GameObject[] array = _terminalGameObjects;
		foreach (GameObject val2 in array)
		{
			PurchaseInteraction purchaseInteraction = val2.GetComponent<PurchaseInteraction>();
			((UnityEvent<Interactor>)purchaseInteraction.onPurchase).AddListener((UnityAction<Interactor>)delegate(Interactor interactor)
			{
				OnPurchase(interactor, purchaseInteraction);
			});
		}
	}

	private void OnPurchase(Interactor interactor, PurchaseInteraction purchaseInteraction)
	{
		bool flag = false;
		Networkavailable = false;
		for (int i = 0; i < _terminalGameObjects.Length; i++)
		{
			GameObject val = _terminalGameObjects[i];
			PurchaseInteraction component = val.GetComponent<PurchaseInteraction>();
			if (purchaseInteraction == component)
			{
				component.Networkavailable = false;
				val.GetComponent<ShopTerminalBehavior>().SetNoPickup();
				flag = doCloseOnTerminalPurchase[i];
			}
			Networkavailable = available || component.available;
		}
		if (flag)
		{
			Networkavailable = false;
			GameObject[] array = _terminalGameObjects;
			foreach (GameObject obj in array)
			{
				obj.GetComponent<PurchaseInteraction>().Networkavailable = false;
				obj.GetComponent<ShopTerminalBehavior>().SetNoPickup();
			}
		}
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

	public void SetCloseOnTerminalPurchase(PurchaseInteraction terminalPurchaseInteraction, bool doCloseMultiShop)
	{
		for (int i = 0; i < _terminalGameObjects.Length; i++)
		{
			if (_terminalGameObjects[i].GetComponent<PurchaseInteraction>() == terminalPurchaseInteraction)
			{
				doCloseOnTerminalPurchase[i] = doCloseMultiShop;
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
			writer.Write(available);
			writer.WritePackedUInt32((uint)cost);
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
			writer.Write(available);
		}
		if ((((NetworkBehaviour)this).syncVarDirtyBits & 2u) != 0)
		{
			if (!flag)
			{
				writer.WritePackedUInt32(((NetworkBehaviour)this).syncVarDirtyBits);
				flag = true;
			}
			writer.WritePackedUInt32((uint)cost);
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
			available = reader.ReadBoolean();
			cost = (int)reader.ReadPackedUInt32();
			return;
		}
		int num = (int)reader.ReadPackedUInt32();
		if (((uint)num & (true ? 1u : 0u)) != 0)
		{
			available = reader.ReadBoolean();
		}
		if (((uint)num & 2u) != 0)
		{
			cost = (int)reader.ReadPackedUInt32();
		}
	}

	public override void PreStartClient()
	{
	}
}
