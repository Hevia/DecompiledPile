using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using Unity;
using UnityEngine;
using UnityEngine.Networking;

namespace RoR2;

public class ShopTerminalBehavior : NetworkBehaviour
{
	[SyncVar(hook = "OnSyncPickupIndex")]
	private PickupIndex pickupIndex = PickupIndex.none;

	[SyncVar(hook = "OnSyncHidden")]
	private bool hidden;

	[SyncVar(hook = "SetHasBeenPurchased")]
	private bool hasBeenPurchased;

	[Tooltip("The PickupDisplay component that should show which item this shop terminal is offering.")]
	public PickupDisplay pickupDisplay;

	[Tooltip("The position from which the drop will be emitted")]
	public Transform dropTransform;

	[Tooltip("The drop table to select a pickup index from - only works if the pickup generates itself")]
	public PickupDropTable dropTable;

	[Tooltip("The velocity with which the drop will be emitted. Rotates with this object.")]
	public Vector3 dropVelocity;

	public Animator animator;

	[Tooltip("The tier of items to drop - only works if the pickup generates itself and the dropTable field is empty.")]
	[Header("Deprecated")]
	public ItemTier itemTier;

	public ItemTag bannedItemTag;

	[Tooltip("Whether or not the shop terminal should drive itself")]
	public bool selfGeneratePickup = true;

	private Xoroshiro128Plus rng;

	private bool hasStarted;

	[NonSerialized]
	public MultiShopController serverMultiShopController;

	public bool pickupIndexIsHidden => hidden;

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
				OnSyncPickupIndex(value);
				((NetworkBehaviour)this).syncVarHookGuard = false;
			}
			((NetworkBehaviour)this).SetSyncVar<PickupIndex>(value, ref pickupIndex, 1u);
		}
	}

	public bool Networkhidden
	{
		get
		{
			return hidden;
		}
		[param: In]
		set
		{
			if (NetworkServer.localClientActive && !((NetworkBehaviour)this).syncVarHookGuard)
			{
				((NetworkBehaviour)this).syncVarHookGuard = true;
				OnSyncHidden(value);
				((NetworkBehaviour)this).syncVarHookGuard = false;
			}
			((NetworkBehaviour)this).SetSyncVar<bool>(value, ref hidden, 2u);
		}
	}

	public bool NetworkhasBeenPurchased
	{
		get
		{
			return hasBeenPurchased;
		}
		[param: In]
		set
		{
			if (NetworkServer.localClientActive && !((NetworkBehaviour)this).syncVarHookGuard)
			{
				((NetworkBehaviour)this).syncVarHookGuard = true;
				SetHasBeenPurchased(value);
				((NetworkBehaviour)this).syncVarHookGuard = false;
			}
			((NetworkBehaviour)this).SetSyncVar<bool>(value, ref hasBeenPurchased, 4u);
		}
	}

	public void SetHasBeenPurchased(bool newHasBeenPurchased)
	{
		if (hasBeenPurchased != newHasBeenPurchased)
		{
			NetworkhasBeenPurchased = newHasBeenPurchased;
		}
	}

	private void OnSyncHidden(bool newHidden)
	{
		SetPickupIndex(pickupIndex, newHidden);
	}

	private void OnSyncPickupIndex(PickupIndex newPickupIndex)
	{
		SetPickupIndex(newPickupIndex, hidden);
		if (NetworkClient.active)
		{
			UpdatePickupDisplayAndAnimations();
		}
	}

	public void Start()
	{
		//IL_001e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0028: Expected O, but got Unknown
		hasStarted = true;
		if (NetworkServer.active)
		{
			rng = new Xoroshiro128Plus(Run.instance.treasureRng.nextUlong);
			if (selfGeneratePickup)
			{
				GenerateNewPickupServer();
			}
		}
		if (NetworkClient.active)
		{
			UpdatePickupDisplayAndAnimations();
		}
	}

	[Server]
	public void GenerateNewPickupServer()
	{
		if (!NetworkServer.active)
		{
			Debug.LogWarning((object)"[Server] function 'System.Void RoR2.ShopTerminalBehavior::GenerateNewPickupServer()' called on client");
		}
		else
		{
			GenerateNewPickupServer(hidden);
		}
	}

	[Server]
	public void GenerateNewPickupServer(bool newHidden)
	{
		if (!NetworkServer.active)
		{
			Debug.LogWarning((object)"[Server] function 'System.Void RoR2.ShopTerminalBehavior::GenerateNewPickupServer(System.Boolean)' called on client");
		}
		else if (!hasBeenPurchased)
		{
			PickupIndex none = PickupIndex.none;
			if (Object.op_Implicit((Object)(object)dropTable))
			{
				none = dropTable.GenerateDrop(rng);
			}
			else
			{
				List<PickupIndex> list2 = null;
				none = Pick(itemTier switch
				{
					ItemTier.Tier1 => Run.instance.availableTier1DropList, 
					ItemTier.Tier2 => Run.instance.availableTier2DropList, 
					ItemTier.Tier3 => Run.instance.availableTier3DropList, 
					ItemTier.Lunar => Run.instance.availableLunarCombinedDropList, 
					ItemTier.Boss => Run.instance.availableBossDropList, 
					ItemTier.VoidTier1 => Run.instance.availableVoidTier1DropList, 
					ItemTier.VoidTier2 => Run.instance.availableVoidTier2DropList, 
					ItemTier.VoidTier3 => Run.instance.availableVoidTier3DropList, 
					ItemTier.VoidBoss => Run.instance.availableVoidBossDropList, 
					_ => throw new ArgumentOutOfRangeException(), 
				});
			}
			SetPickupIndex(none, newHidden);
		}
		bool PassesFilter(PickupIndex pickupIndex)
		{
			if (bannedItemTag == ItemTag.Any)
			{
				return true;
			}
			PickupDef pickupDef = PickupCatalog.GetPickupDef(pickupIndex);
			if (pickupDef.itemIndex != ItemIndex.None)
			{
				return !ItemCatalog.GetItemDef(pickupDef.itemIndex).ContainsTag(bannedItemTag);
			}
			return true;
		}
		PickupIndex Pick(List<PickupIndex> list)
		{
			return rng.NextElementUniform<PickupIndex>(list.Where(PassesFilter).ToList());
		}
	}

	public void SetPickupIndex(PickupIndex newPickupIndex, bool newHidden = false)
	{
		if (pickupIndex != newPickupIndex || hidden != newHidden)
		{
			NetworkpickupIndex = newPickupIndex;
			Networkhidden = newHidden;
		}
	}

	public void SetHidden(bool newHidden)
	{
		SetPickupIndex(pickupIndex, newHidden);
	}

	private void UpdatePickupDisplayAndAnimations()
	{
		if (Object.op_Implicit((Object)(object)pickupDisplay))
		{
			pickupDisplay.SetPickupIndex(pickupIndex, hidden);
		}
		if (!hasStarted)
		{
			return;
		}
		if (pickupIndex == PickupIndex.none)
		{
			Util.PlaySound("Play_UI_tripleChestShutter", ((Component)this).gameObject);
			if (Object.op_Implicit((Object)(object)animator))
			{
				int layerIndex = animator.GetLayerIndex("Body");
				animator.PlayInFixedTime(hasBeenPurchased ? "Open" : "Closing", layerIndex);
			}
		}
		else if (Object.op_Implicit((Object)(object)animator) && !hasBeenPurchased)
		{
			int layerIndex2 = animator.GetLayerIndex("Body");
			animator.PlayInFixedTime("Idle", layerIndex2);
		}
	}

	public PickupIndex CurrentPickupIndex()
	{
		return pickupIndex;
	}

	[Server]
	public void SetNoPickup()
	{
		if (!NetworkServer.active)
		{
			Debug.LogWarning((object)"[Server] function 'System.Void RoR2.ShopTerminalBehavior::SetNoPickup()' called on client");
		}
		else
		{
			SetPickupIndex(PickupIndex.none);
		}
	}

	[Server]
	public void DropPickup()
	{
		//IL_003d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0049: Unknown result type (might be due to invalid IL or missing references)
		//IL_004e: Unknown result type (might be due to invalid IL or missing references)
		if (!NetworkServer.active)
		{
			Debug.LogWarning((object)"[Server] function 'System.Void RoR2.ShopTerminalBehavior::DropPickup()' called on client");
			return;
		}
		SetHasBeenPurchased(newHasBeenPurchased: true);
		PickupDropletController.CreatePickupDroplet(pickupIndex, (Object.op_Implicit((Object)(object)dropTransform) ? dropTransform : ((Component)this).transform).position, ((Component)this).transform.TransformVector(dropVelocity));
	}

	private void UNetVersion()
	{
	}

	public override bool OnSerialize(NetworkWriter writer, bool forceAll)
	{
		if (forceAll)
		{
			GeneratedNetworkCode._WritePickupIndex_None(writer, pickupIndex);
			writer.Write(hidden);
			writer.Write(hasBeenPurchased);
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
			writer.Write(hidden);
		}
		if ((((NetworkBehaviour)this).syncVarDirtyBits & 4u) != 0)
		{
			if (!flag)
			{
				writer.WritePackedUInt32(((NetworkBehaviour)this).syncVarDirtyBits);
				flag = true;
			}
			writer.Write(hasBeenPurchased);
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
			hidden = reader.ReadBoolean();
			hasBeenPurchased = reader.ReadBoolean();
			return;
		}
		int num = (int)reader.ReadPackedUInt32();
		if (((uint)num & (true ? 1u : 0u)) != 0)
		{
			OnSyncPickupIndex(GeneratedNetworkCode._ReadPickupIndex_None(reader));
		}
		if (((uint)num & 2u) != 0)
		{
			OnSyncHidden(reader.ReadBoolean());
		}
		if (((uint)num & 4u) != 0)
		{
			SetHasBeenPurchased(reader.ReadBoolean());
		}
	}

	public override void PreStartClient()
	{
	}
}
