using System;
using System.Collections.Generic;
using System.Text;
using HG;
using UnityEngine;

namespace RoR2.EntitlementManagement;

public static class EntitlementManager
{
	private static IUserEntitlementResolver<LocalUser>[] localUserEntitlementResolvers = Array.Empty<IUserEntitlementResolver<LocalUser>>();

	private static IUserEntitlementResolver<NetworkUser>[] networkUserEntitlementResolvers = Array.Empty<IUserEntitlementResolver<NetworkUser>>();

	private static bool isLoaded = true;

	private static bool collectLocalUserEntitlementResolversLocked = false;

	private static bool collectNetworkUserEntitlementResolversLocked;

	public static LocalUserEntitlementTracker localUserEntitlementTracker { get; private set; }

	public static NetworkUserServerEntitlementTracker networkUserEntitlementTracker { get; private set; }

	private static event Action<Action<IUserEntitlementResolver<LocalUser>>> _collectLocalUserEntitlementResolvers;

	public static event Action<Action<IUserEntitlementResolver<LocalUser>>> collectLocalUserEntitlementResolvers
	{
		add
		{
			if (collectLocalUserEntitlementResolversLocked)
			{
				throw new InvalidOperationException("collectLocalUserEntitlementResolvers has already been invoked. It is too late to add additional subscribers.");
			}
			_collectLocalUserEntitlementResolvers += value;
		}
		remove
		{
			_collectLocalUserEntitlementResolvers -= value;
		}
	}

	private static event Action<Action<IUserEntitlementResolver<NetworkUser>>> _collectNetworkUserEntitlementResolvers;

	public static event Action<Action<IUserEntitlementResolver<NetworkUser>>> collectNetworkUserEntitlementResolvers
	{
		add
		{
			if (collectNetworkUserEntitlementResolversLocked)
			{
				throw new InvalidOperationException("collectNetworkUserEntitlementResolvers has already been invoked. It is too late to add additional subscribers.");
			}
			_collectNetworkUserEntitlementResolvers += value;
		}
		remove
		{
			_collectNetworkUserEntitlementResolvers -= value;
		}
	}

	[SystemInitializer(new Type[] { })]
	private static void Init()
	{
		List<IUserEntitlementResolver<LocalUser>> list = new List<IUserEntitlementResolver<LocalUser>>();
		collectLocalUserEntitlementResolversLocked = true;
		EntitlementManager._collectLocalUserEntitlementResolvers?.Invoke(list.Add);
		EntitlementManager._collectLocalUserEntitlementResolvers = null;
		List<IUserEntitlementResolver<NetworkUser>> list2 = new List<IUserEntitlementResolver<NetworkUser>>();
		collectNetworkUserEntitlementResolversLocked = true;
		EntitlementManager._collectNetworkUserEntitlementResolvers?.Invoke(list2.Add);
		EntitlementManager._collectNetworkUserEntitlementResolvers = null;
		localUserEntitlementResolvers = list.ToArray();
		networkUserEntitlementResolvers = list2.ToArray();
		localUserEntitlementTracker = new LocalUserEntitlementTracker(localUserEntitlementResolvers);
		networkUserEntitlementTracker = new NetworkUserServerEntitlementTracker(networkUserEntitlementResolvers);
	}

	[ConCommand(commandName = "entitlement_check_local", flags = ConVarFlags.None, helpText = "Displays the availability of all entitlements for the sender.")]
	private static void CCEntitlementCheckLocal(ConCommandArgs args)
	{
		//IL_000e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0013: Unknown result type (might be due to invalid IL or missing references)
		//IL_0016: Unknown result type (might be due to invalid IL or missing references)
		//IL_001b: Unknown result type (might be due to invalid IL or missing references)
		LocalUser senderLocalUser = args.GetSenderLocalUser();
		StringBuilder stringBuilder = StringBuilderPool.RentStringBuilder();
		Enumerator<EntitlementDef> enumerator = EntitlementCatalog.entitlementDefs.GetEnumerator();
		try
		{
			while (enumerator.MoveNext())
			{
				EntitlementDef current = enumerator.Current;
				stringBuilder.Append(((Object)current).name).Append("=").Append(localUserEntitlementTracker.UserHasEntitlement(senderLocalUser, current))
					.AppendLine();
			}
		}
		finally
		{
			((IDisposable)enumerator).Dispose();
		}
		args.Log(stringBuilder.ToString());
		StringBuilderPool.ReturnStringBuilder(stringBuilder);
	}

	[ConCommand(commandName = "entitlement_force_refresh", flags = ConVarFlags.None, helpText = "Forces the entitlement trackers to refresh.")]
	private static void CCEntitlementForceRefresh(ConCommandArgs args)
	{
		localUserEntitlementTracker.UpdateAllUserEntitlements();
		networkUserEntitlementTracker.UpdateAllUserEntitlements();
	}
}
