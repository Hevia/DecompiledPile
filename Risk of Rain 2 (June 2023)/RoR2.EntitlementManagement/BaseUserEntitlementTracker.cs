using System;
using System.Collections.Generic;
using HG;
using JetBrains.Annotations;
using UnityEngine;

namespace RoR2.EntitlementManagement;

public abstract class BaseUserEntitlementTracker<TUser> : IDisposable where TUser : class
{
	private Dictionary<TUser, List<EntitlementDef>> userToEntitlements = new Dictionary<TUser, List<EntitlementDef>>();

	private IUserEntitlementResolver<TUser>[] entitlementResolvers;

	private bool disposed;

	protected abstract void SubscribeToUserDiscovered();

	protected abstract void SubscribeToUserLost();

	protected abstract void UnsubscribeFromUserDiscovered();

	protected abstract void UnsubscribeFromUserLost();

	protected abstract IList<TUser> GetCurrentUsers();

	public BaseUserEntitlementTracker(IUserEntitlementResolver<TUser>[] entitlementResolvers)
	{
		this.entitlementResolvers = ArrayUtils.Clone<IUserEntitlementResolver<TUser>>(entitlementResolvers);
		SubscribeToUserDiscovered();
		SubscribeToUserLost();
		for (int i = 0; i < entitlementResolvers.Length; i++)
		{
			entitlementResolvers[i].onEntitlementsChanged += UpdateAllUserEntitlements;
		}
		IList<TUser> currentUsers = GetCurrentUsers();
		for (int j = 0; j < currentUsers.Count; j++)
		{
			OnUserDiscovered(currentUsers[j]);
		}
	}

	public virtual void Dispose()
	{
		disposed = true;
		for (int num = entitlementResolvers.Length - 1; num >= 0; num--)
		{
			entitlementResolvers[num].onEntitlementsChanged -= UpdateAllUserEntitlements;
		}
		UnsubscribeFromUserLost();
		UnsubscribeFromUserDiscovered();
		IList<TUser> currentUsers = GetCurrentUsers();
		for (int i = 0; i < currentUsers.Count; i++)
		{
			OnUserLost(currentUsers[i]);
		}
	}

	protected virtual void OnUserDiscovered(TUser user)
	{
		try
		{
			List<EntitlementDef> value = new List<EntitlementDef>();
			userToEntitlements.Add(user, value);
			UpdateUserEntitlements(user);
		}
		catch (Exception message)
		{
			LogError(message);
		}
	}

	protected virtual void OnUserLost(TUser user)
	{
		try
		{
			userToEntitlements.Remove(user);
		}
		catch (Exception message)
		{
			LogError(message);
		}
	}

	public void UpdateAllUserEntitlements()
	{
		IList<TUser> currentUsers = GetCurrentUsers();
		for (int i = 0; i < currentUsers.Count; i++)
		{
			UpdateUserEntitlements(currentUsers[i]);
		}
	}

	protected void UpdateUserEntitlements(TUser user)
	{
		//IL_0026: Unknown result type (might be due to invalid IL or missing references)
		//IL_002b: Unknown result type (might be due to invalid IL or missing references)
		//IL_002f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0034: Unknown result type (might be due to invalid IL or missing references)
		if (!userToEntitlements.TryGetValue(user, out var value))
		{
			return;
		}
		value.Clear();
		IUserEntitlementResolver<TUser>[] array = entitlementResolvers;
		foreach (IUserEntitlementResolver<TUser> userEntitlementResolver in array)
		{
			Enumerator<EntitlementDef> enumerator = EntitlementCatalog.entitlementDefs.GetEnumerator();
			try
			{
				while (enumerator.MoveNext())
				{
					EntitlementDef current = enumerator.Current;
					if (!value.Contains(current) && userEntitlementResolver.CheckUserHasEntitlement(user, current))
					{
						value.Add(current);
					}
				}
			}
			finally
			{
				((IDisposable)enumerator).Dispose();
			}
		}
	}

	public bool UserHasEntitlement([NotNull] TUser user, [NotNull] EntitlementDef entitlementDef)
	{
		if (!Object.op_Implicit((Object)(object)entitlementDef))
		{
			throw new ArgumentNullException("entitlementDef");
		}
		if (user == null || !userToEntitlements.TryGetValue(user, out var value))
		{
			return false;
		}
		return value.Contains(entitlementDef);
	}

	public bool AnyUserHasEntitlement([NotNull] EntitlementDef entitlementDef)
	{
		IList<TUser> currentUsers = GetCurrentUsers();
		for (int i = 0; i < currentUsers.Count; i++)
		{
			if (UserHasEntitlement(currentUsers[i], entitlementDef))
			{
				return true;
			}
		}
		return false;
	}

	protected static void LogError(object message)
	{
		RoR2Application.onNextUpdate += delegate
		{
			Debug.LogError(message);
		};
	}
}
