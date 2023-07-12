using System;
using HG;
using JetBrains.Annotations;
using RoR2.EntitlementManagement;
using RoR2.ExpansionManagement;
using UnityEngine;
using UnityEngine.Networking;

namespace RoR2;

[RequireComponent(typeof(PlayerCharacterMasterController))]
public class PlayerCharacterMasterControllerEntitlementTracker : NetworkBehaviour
{
	private PlayerCharacterMasterController playerCharacterMasterController;

	private static readonly uint entitlementsSetDirtyBit = 1u;

	private static readonly uint allDirtyBits = entitlementsSetDirtyBit;

	private bool[] entitlementsSet;

	private void Awake()
	{
		//IL_000d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0012: Unknown result type (might be due to invalid IL or missing references)
		playerCharacterMasterController = ((Component)this).GetComponent<PlayerCharacterMasterController>();
		entitlementsSet = new bool[EntitlementCatalog.entitlementDefs.Length];
		playerCharacterMasterController.onLinkedToNetworkUserServer += OnLinkedToNetworkUserServer;
	}

	private void OnLinkedToNetworkUserServer()
	{
		UpdateEntitlementsServer();
		GameObject bodyPrefab = playerCharacterMasterController.master.bodyPrefab;
		if (Object.op_Implicit((Object)(object)bodyPrefab))
		{
			ExpansionRequirementComponent component = bodyPrefab.GetComponent<ExpansionRequirementComponent>();
			if (Object.op_Implicit((Object)(object)component) && !component.PlayerCanUseBody(playerCharacterMasterController))
			{
				Debug.LogWarning((object)("Player can't use body " + ((Object)bodyPrefab).name + "; defaulting to default survivor."));
				playerCharacterMasterController.master.bodyPrefab = SurvivorCatalog.defaultSurvivor.bodyPrefab;
			}
		}
	}

	[Server]
	private unsafe void UpdateEntitlementsServer()
	{
		//IL_0032: Unknown result type (might be due to invalid IL or missing references)
		//IL_0037: Unknown result type (might be due to invalid IL or missing references)
		if (!NetworkServer.active)
		{
			Debug.LogWarning((object)"[Server] function 'System.Void RoR2.PlayerCharacterMasterControllerEntitlementTracker::UpdateEntitlementsServer()' called on client");
			return;
		}
		NetworkUser networkUser = playerCharacterMasterController.networkUser;
		if (!Object.op_Implicit((Object)(object)networkUser))
		{
			return;
		}
		bool flag = false;
		NetworkUserServerEntitlementTracker networkUserEntitlementTracker = EntitlementManager.networkUserEntitlementTracker;
		ReadOnlyArray<EntitlementDef> entitlementDefs = EntitlementCatalog.entitlementDefs;
		for (int i = 0; i < entitlementDefs.Length; i++)
		{
			EntitlementDef entitlementDef = *(EntitlementDef*)entitlementDefs[i];
			if (!entitlementsSet[i] && networkUserEntitlementTracker.UserHasEntitlement(networkUser, entitlementDef))
			{
				entitlementsSet[i] = true;
				flag = true;
			}
		}
		if (flag)
		{
			((NetworkBehaviour)this).SetDirtyBit(entitlementsSetDirtyBit);
		}
	}

	public override bool OnSerialize(NetworkWriter writer, bool initialState)
	{
		uint num = (initialState ? allDirtyBits : ((NetworkBehaviour)this).syncVarDirtyBits);
		writer.WritePackedUInt32(num);
		if ((num & entitlementsSetDirtyBit) != 0)
		{
			writer.WriteBitArray(entitlementsSet);
		}
		if (!initialState)
		{
			return num != 0;
		}
		return false;
	}

	public override void OnDeserialize(NetworkReader reader, bool initialState)
	{
		if ((reader.ReadPackedUInt32() & entitlementsSetDirtyBit) != 0)
		{
			reader.ReadBitArray(entitlementsSet);
		}
	}

	public bool HasEntitlement([NotNull] EntitlementDef entitlementDef)
	{
		if (entitlementDef == null)
		{
			throw new ArgumentNullException("entitlementDef");
		}
		bool[] array = entitlementsSet;
		EntitlementIndex entitlementIndex = entitlementDef.entitlementIndex;
		bool flag = false;
		return ArrayUtils.GetSafe<bool>(array, (int)entitlementIndex, ref flag);
	}

	private void UNetVersion()
	{
	}

	public override void PreStartClient()
	{
	}
}
