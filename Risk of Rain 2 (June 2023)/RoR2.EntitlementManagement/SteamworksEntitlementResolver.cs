using System;
using System.Collections.Generic;
using Facepunch.Steamworks;
using JetBrains.Annotations;
using RoR2.ContentManagement;
using RoR2.Networking;
using UnityEngine;
using UnityEngine.Networking;

namespace RoR2.EntitlementManagement;

public class SteamworksEntitlementResolver : IUserEntitementsResolverNetworkAndLocal, IUserEntitlementResolver<NetworkUser>, IUserEntitlementResolver<LocalUser>
{
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
		}
		remove
		{
		}
	}

	private static void OnDlcInstalled(uint appId)
	{
		Debug.Log((object)$"OnDlcInstalled appId={appId}");
		SteamworksEntitlementResolver.onDlcInstalled?.Invoke();
	}

	bool IUserEntitlementResolver<LocalUser>.CheckUserHasEntitlement([NotNull] LocalUser localUser, [NotNull] EntitlementDef entitlementDef)
	{
		return EntitlementAbstractions.VerifyLocalSteamUser(entitlementDef);
	}

	bool IUserEntitlementResolver<NetworkUser>.CheckUserHasEntitlement([NotNull] NetworkUser networkUser, [NotNull] EntitlementDef entitlementDef)
	{
		if (!Object.op_Implicit((Object)(object)networkUser))
		{
			return false;
		}
		if (((NetworkBehaviour)networkUser).isLocalPlayer)
		{
			return EntitlementAbstractions.VerifyLocalSteamUser(entitlementDef);
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

	public SteamworksEntitlementResolver()
	{
		EntitlementManager.collectLocalUserEntitlementResolvers += delegate(Action<IUserEntitlementResolver<LocalUser>> add)
		{
			add(this);
		};
		EntitlementManager.collectNetworkUserEntitlementResolvers += delegate(Action<IUserEntitlementResolver<NetworkUser>> add)
		{
			add(this);
		};
		SteamworksClientManager.onLoaded += delegate
		{
			//IL_0016: Unknown result type (might be due to invalid IL or missing references)
			//IL_0020: Expected O, but got Unknown
			SteamworksClientManager.instance.steamworksClient.App.OnDlcInstalled += new DlcInstalledDelegate(OnDlcInstalled);
		};
	}

	public string[] BuildEntitlements()
	{
		List<string> list = new List<string>();
		EntitlementDef[] entitlementDefs = ContentManager.entitlementDefs;
		foreach (EntitlementDef entitlementDef in entitlementDefs)
		{
			if (Client.Instance.App.IsDlcInstalled(entitlementDef.steamAppId))
			{
				list.Add(((Object)entitlementDef).name);
			}
		}
		return list.ToArray();
	}
}
