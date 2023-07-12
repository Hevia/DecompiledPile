using System;
using System.Collections.Generic;
using System.Linq;
using Epic.OnlineServices;
using Epic.OnlineServices.Ecom;
using JetBrains.Annotations;
using RoR2.ContentManagement;
using RoR2.Networking;
using UnityEngine;
using UnityEngine.Networking;

namespace RoR2.EntitlementManagement;

public class EgsEntitlementResolver : IUserEntitementsResolverNetworkAndLocal, IUserEntitlementResolver<NetworkUser>, IUserEntitlementResolver<LocalUser>
{
	private const string DevAudienceEntitlementName = "8fc64849a03741faaf51824d6e727cc1";

	private static EcomInterface EOS_Ecom;

	private List<string> ownedEntitlementIDs = new List<string>();

	private static event Action onDlcInstalled;

	event Action IUserEntitlementResolver<LocalUser>.onEntitlementsChanged
	{
		add
		{
			onDlcInstalled += value;
		}
		remove
		{
			onDlcInstalled -= value;
		}
	}

	event Action IUserEntitlementResolver<NetworkUser>.onEntitlementsChanged
	{
		add
		{
			onDlcInstalled += value;
		}
		remove
		{
			onDlcInstalled -= value;
		}
	}

	public EgsEntitlementResolver()
	{
		if ((Handle)(object)EOS_Ecom == (Handle)null)
		{
			EOS_Ecom = EOSPlatformManager.GetPlatformInterface().GetEcomInterface();
		}
		RoR2Application.onLoad = (Action)Delegate.Combine(RoR2Application.onLoad, new Action(GetEGSEntitlements));
	}

	private void GetEGSEntitlements(EpicAccountId accountId)
	{
		GetEGSEntitlements();
	}

	private void GetEGSEntitlements()
	{
		//IL_000d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0041: Unknown result type (might be due to invalid IL or missing references)
		//IL_0047: Expected O, but got Unknown
		//IL_0067: Unknown result type (might be due to invalid IL or missing references)
		//IL_0071: Expected O, but got Unknown
		if ((Handle)(object)EOSLoginManager.loggedInAuthId != (Handle)null)
		{
			string[] catalogItemIds = ((IEnumerable<EntitlementDef>)(object)EntitlementCatalog.entitlementDefs).Select((EntitlementDef x) => x.eosItemId).ToArray();
			QueryOwnershipOptions val = new QueryOwnershipOptions();
			val.LocalUserId = EOSLoginManager.loggedInAuthId;
			val.CatalogItemIds = catalogItemIds;
			EOS_Ecom.QueryOwnership(val, (object)null, new OnQueryOwnershipCallback(HandleQueryOwnershipCallback));
		}
		else
		{
			EOSLoginManager.OnAuthLoggedIn += GetEGSEntitlements;
		}
	}

	private void HandleQueryOwnershipCallback(QueryOwnershipCallbackInfo data)
	{
		//IL_0008: Unknown result type (might be due to invalid IL or missing references)
		//IL_000b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0010: Unknown result type (might be due to invalid IL or missing references)
		//IL_0034: Unknown result type (might be due to invalid IL or missing references)
		//IL_003a: Invalid comparison between Unknown and I4
		if (data.GetResultCode() == (Result?)0)
		{
			ownedEntitlementIDs.Clear();
			for (int i = 0; i < data.ItemOwnership.Length; i++)
			{
				if ((int)data.ItemOwnership[i].OwnershipStatus == 1)
				{
					ownedEntitlementIDs.Add(data.ItemOwnership[i].Id);
				}
			}
		}
		EgsEntitlementResolver.onDlcInstalled();
	}

	bool IUserEntitlementResolver<LocalUser>.CheckUserHasEntitlement([NotNull] LocalUser localUser, [NotNull] EntitlementDef entitlementDef)
	{
		return CheckLocalUserHasEntitlement(entitlementDef);
	}

	private bool CheckLocalUserHasEntitlement(EntitlementDef entitlementDef)
	{
		for (int i = 0; i < ownedEntitlementIDs.Count; i++)
		{
			if (ownedEntitlementIDs[i] == entitlementDef.eosItemId)
			{
				return true;
			}
		}
		return false;
	}

	bool IUserEntitlementResolver<NetworkUser>.CheckUserHasEntitlement([NotNull] NetworkUser networkUser, [NotNull] EntitlementDef entitlementDef)
	{
		if (!Object.op_Implicit((Object)(object)networkUser))
		{
			return false;
		}
		if (((NetworkBehaviour)networkUser).isLocalPlayer)
		{
			return CheckLocalUserHasEntitlement(entitlementDef);
		}
		ClientAuthData clientAuthData = ServerAuthManager.FindAuthData(((NetworkBehaviour)networkUser).connectionToClient);
		if (clientAuthData == null)
		{
			return false;
		}
		CSteamID steamId = clientAuthData.steamId;
		if (!steamId.isValid)
		{
			return false;
		}
		return EntitlementAbstractions.VerifyRemoteUser(clientAuthData, entitlementDef);
	}

	public string[] BuildEntitlements()
	{
		string[] array = new string[ownedEntitlementIDs.Count];
		int num = array.Length;
		for (int i = 0; i < num; i++)
		{
			EntitlementDef[] entitlementDefs = ContentManager.entitlementDefs;
			foreach (EntitlementDef entitlementDef in entitlementDefs)
			{
				if (entitlementDef.eosItemId == ownedEntitlementIDs[i] || "8fc64849a03741faaf51824d6e727cc1" == ownedEntitlementIDs[i])
				{
					array[i] = ((Object)entitlementDef).name;
					break;
				}
			}
		}
		return array;
	}
}
