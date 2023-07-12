using System;
using System.Collections.Generic;
using RoR2.Networking;
using RoR2.Orbs;
using UnityEngine;
using UnityEngine.Networking;

namespace RoR2;

[RequireComponent(typeof(PurchaseInteraction))]
public class VendingMachineBehavior : NetworkBehaviour
{
	[SerializeField]
	private int maxPurchases;

	[SerializeField]
	private float healFraction;

	[SerializeField]
	private GameObject detonateEffect;

	[SerializeField]
	private float vendingRadius;

	[SerializeField]
	private int numBonusOrbs;

	[SerializeField]
	private Transform orbOrigin;

	[SerializeField]
	private Animator animator;

	[SerializeField]
	private string animatorTriggerNameVend;

	[SerializeField]
	private string animatorIntNameVendsRemaining;

	private PurchaseInteraction purchaseInteraction;

	private static int kRpcRpcTriggerVendAnimation;

	public int purchaseCount { get; private set; }

	public static event Action<ShrineHealingBehavior, Interactor> onActivated;

	public override int GetNetworkChannel()
	{
		return QosChannelIndex.defaultReliable.intVal;
	}

	private void Awake()
	{
		purchaseInteraction = ((Component)this).GetComponent<PurchaseInteraction>();
	}

	[Server]
	public void Vend(Interactor activator)
	{
		//IL_00aa: Unknown result type (might be due to invalid IL or missing references)
		//IL_00af: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b5: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ba: Unknown result type (might be due to invalid IL or missing references)
		if (!NetworkServer.active)
		{
			Debug.LogWarning((object)"[Server] function 'System.Void RoR2.VendingMachineBehavior::Vend(RoR2.Interactor)' called on client");
			return;
		}
		int num = purchaseCount + 1;
		purchaseCount = num;
		CallRpcTriggerVendAnimation(maxPurchases - purchaseCount);
		CharacterBody component = ((Component)activator).GetComponent<CharacterBody>();
		if (Object.op_Implicit((Object)(object)component))
		{
			SendOrbToBody(component);
			BullseyeSearch bullseyeSearch = new BullseyeSearch();
			bullseyeSearch.teamMaskFilter = TeamMask.none;
			if (Object.op_Implicit((Object)(object)component.teamComponent))
			{
				bullseyeSearch.teamMaskFilter.AddTeam(component.teamComponent.teamIndex);
			}
			bullseyeSearch.filterByLoS = false;
			bullseyeSearch.maxDistanceFilter = vendingRadius;
			bullseyeSearch.maxAngleFilter = 360f;
			bullseyeSearch.searchOrigin = ((Component)this).transform.position;
			bullseyeSearch.searchDirection = Vector3.up;
			bullseyeSearch.sortMode = BullseyeSearch.SortMode.None;
			bullseyeSearch.RefreshCandidates();
			bullseyeSearch.FilterOutGameObject(((Component)activator).gameObject);
			List<HurtBox> list = new List<HurtBox>(bullseyeSearch.GetResults());
			list.Sort(BonusOrbHurtBoxSort);
			for (int i = 0; i < numBonusOrbs && i < list.Count; i++)
			{
				SendOrbToBody(list[i].healthComponent.body);
			}
		}
	}

	private static int BonusOrbHurtBoxSort(HurtBox lhs, HurtBox rhs)
	{
		return (int)(100f * (lhs.healthComponent.combinedHealthFraction - rhs.healthComponent.combinedHealthFraction));
	}

	private void SendOrbToBody(CharacterBody body)
	{
		//IL_001c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0021: Unknown result type (might be due to invalid IL or missing references)
		HealthComponent component = ((Component)body).GetComponent<HealthComponent>();
		if (Object.op_Implicit((Object)(object)component))
		{
			VendingMachineOrb vendingMachineOrb = new VendingMachineOrb();
			vendingMachineOrb.origin = orbOrigin.position;
			vendingMachineOrb.target = component.body.mainHurtBox;
			vendingMachineOrb.healFraction = healFraction;
			OrbManager.instance.AddOrb(vendingMachineOrb);
		}
	}

	[Server]
	public void Detonate()
	{
		//IL_0022: Unknown result type (might be due to invalid IL or missing references)
		if (!NetworkServer.active)
		{
			Debug.LogWarning((object)"[Server] function 'System.Void RoR2.VendingMachineBehavior::Detonate()' called on client");
			return;
		}
		EffectData effectData = new EffectData();
		effectData.origin = ((Component)this).transform.position;
		EffectManager.SpawnEffect(detonateEffect, effectData, transmit: true);
		Object.Destroy((Object)(object)((Component)this).gameObject);
	}

	[Server]
	public void RefreshPurchaseInteractionAvailability()
	{
		if (!NetworkServer.active)
		{
			Debug.LogWarning((object)"[Server] function 'System.Void RoR2.VendingMachineBehavior::RefreshPurchaseInteractionAvailability()' called on client");
		}
		else
		{
			purchaseInteraction.Networkavailable = purchaseCount < maxPurchases;
		}
	}

	[ClientRpc]
	public void RpcTriggerVendAnimation(int vendsRemaining)
	{
		if (Object.op_Implicit((Object)(object)animator))
		{
			animator.SetTrigger(animatorTriggerNameVend);
			animator.SetInteger(animatorIntNameVendsRemaining, vendsRemaining);
		}
	}

	private void UNetVersion()
	{
	}

	protected static void InvokeRpcRpcTriggerVendAnimation(NetworkBehaviour obj, NetworkReader reader)
	{
		if (!NetworkClient.active)
		{
			Debug.LogError((object)"RPC RpcTriggerVendAnimation called on server.");
		}
		else
		{
			((VendingMachineBehavior)(object)obj).RpcTriggerVendAnimation((int)reader.ReadPackedUInt32());
		}
	}

	public void CallRpcTriggerVendAnimation(int vendsRemaining)
	{
		//IL_0016: Unknown result type (might be due to invalid IL or missing references)
		//IL_001c: Expected O, but got Unknown
		//IL_0041: Unknown result type (might be due to invalid IL or missing references)
		if (!NetworkServer.active)
		{
			Debug.LogError((object)"RPC Function RpcTriggerVendAnimation called on client.");
			return;
		}
		NetworkWriter val = new NetworkWriter();
		val.Write((short)0);
		val.Write((short)2);
		val.WritePackedUInt32((uint)kRpcRpcTriggerVendAnimation);
		val.Write(((Component)this).GetComponent<NetworkIdentity>().netId);
		val.WritePackedUInt32((uint)vendsRemaining);
		((NetworkBehaviour)this).SendRPCInternal(val, 0, "RpcTriggerVendAnimation");
	}

	static VendingMachineBehavior()
	{
		//IL_0020: Unknown result type (might be due to invalid IL or missing references)
		//IL_002a: Expected O, but got Unknown
		kRpcRpcTriggerVendAnimation = 2105333433;
		NetworkBehaviour.RegisterRpcDelegate(typeof(VendingMachineBehavior), kRpcRpcTriggerVendAnimation, new CmdDelegate(InvokeRpcRpcTriggerVendAnimation));
		NetworkCRC.RegisterBehaviour("VendingMachineBehavior", 0);
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
