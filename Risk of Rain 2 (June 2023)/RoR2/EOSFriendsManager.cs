using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using Epic.OnlineServices;
using Epic.OnlineServices.Connect;
using Epic.OnlineServices.Friends;

namespace RoR2;

public class EOSFriendsManager
{
	[Serializable]
	[CompilerGenerated]
	private sealed class _003C_003Ec
	{
		public static readonly _003C_003Ec _003C_003E9 = new _003C_003Ec();

		public static OnQueryFriendsCallback _003C_003E9__10_0;

		internal void _003CInitialize_003Eb__10_0(QueryFriendsCallbackInfo data)
		{
			PopulateFriendsEpicAccountIds();
		}
	}

	public static FriendsInterface Interface;

	private static ConnectInterface _connectInterface;

	public static List<ProductUserId> FriendsProductUserIds { get; } = new List<ProductUserId>();


	private static EpicAccountId LocalUserEpicAccountId => EOSLoginManager.loggedInAuthId;

	private static ProductUserId LocalUserProductUserId => EOSLoginManager.loggedInProductId;

	public EOSFriendsManager()
	{
		Interface = EOSPlatformManager.GetPlatformInterface().GetFriendsInterface();
		_connectInterface = EOSPlatformManager.GetPlatformInterface().GetConnectInterface();
		EOSLoginManager.OnAuthLoggedIn += Initialize;
	}

	private static void Initialize(EpicAccountId obj)
	{
		//IL_0015: Unknown result type (might be due to invalid IL or missing references)
		//IL_001a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0020: Expected O, but got Unknown
		//IL_002a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0037: Unknown result type (might be due to invalid IL or missing references)
		//IL_0041: Expected O, but got Unknown
		//IL_0041: Expected O, but got Unknown
		object obj2 = _003C_003Ec._003C_003E9__10_0;
		if (obj2 == null)
		{
			OnQueryFriendsCallback val = delegate
			{
				PopulateFriendsEpicAccountIds();
			};
			obj2 = (object)val;
			_003C_003Ec._003C_003E9__10_0 = val;
		}
		QueryFriendsRefresh(null, (OnQueryFriendsCallback)obj2);
		Interface.AddNotifyFriendsUpdate(new AddNotifyFriendsUpdateOptions(), (object)null, new OnFriendsUpdateCallback(FriendsUpdateHandler));
	}

	private static void FriendsUpdateHandler(OnFriendsUpdateInfo data)
	{
		//IL_0001: Unknown result type (might be due to invalid IL or missing references)
		//IL_0006: Unknown result type (might be due to invalid IL or missing references)
		//IL_0007: Unknown result type (might be due to invalid IL or missing references)
		//IL_000a: Unknown result type (might be due to invalid IL or missing references)
		//IL_000c: Invalid comparison between Unknown and I4
		//IL_002e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0038: Expected O, but got Unknown
		FriendsStatus currentStatus = data.CurrentStatus;
		if ((int)currentStatus != 0)
		{
			if ((int)currentStatus == 3)
			{
				QueryFriendsRefresh(null, new OnQueryFriendsCallback(TryAddFriend));
			}
		}
		else
		{
			FriendsProductUserIds.Remove(data.TargetUserId.ToProductUserId());
		}
	}

	private static void TryAddFriend(QueryFriendsCallbackInfo data)
	{
		ProductUserId val = data.LocalUserId.ToProductUserId();
		if ((Handle)(object)val != (Handle)null && !FriendsProductUserIds.Contains(val))
		{
			FriendsProductUserIds.Add(val);
		}
	}

	private static void QueryFriendsRefresh(object clientData = null, OnQueryFriendsCallback callback = null)
	{
		//IL_0005: Unknown result type (might be due to invalid IL or missing references)
		//IL_000a: Unknown result type (might be due to invalid IL or missing references)
		//IL_001c: Expected O, but got Unknown
		Interface.QueryFriends(new QueryFriendsOptions
		{
			LocalUserId = LocalUserEpicAccountId
		}, clientData, callback);
	}

	private static void PopulateFriendsEpicAccountIds()
	{
		//IL_0005: Unknown result type (might be due to invalid IL or missing references)
		//IL_000a: Unknown result type (might be due to invalid IL or missing references)
		//IL_001a: Expected O, but got Unknown
		//IL_0032: Unknown result type (might be due to invalid IL or missing references)
		//IL_0037: Unknown result type (might be due to invalid IL or missing references)
		//IL_003e: Unknown result type (might be due to invalid IL or missing references)
		//IL_004e: Expected O, but got Unknown
		//IL_006c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0071: Unknown result type (might be due to invalid IL or missing references)
		//IL_0078: Unknown result type (might be due to invalid IL or missing references)
		//IL_007f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0092: Unknown result type (might be due to invalid IL or missing references)
		//IL_009c: Expected O, but got Unknown
		//IL_009c: Expected O, but got Unknown
		int friendsCount = Interface.GetFriendsCount(new GetFriendsCountOptions
		{
			LocalUserId = LocalUserEpicAccountId
		});
		string[] array = new string[friendsCount];
		EpicAccountId[] array2 = (EpicAccountId[])(object)new EpicAccountId[friendsCount];
		for (int i = 0; i < friendsCount; i++)
		{
			EpicAccountId friendAtIndex = Interface.GetFriendAtIndex(new GetFriendAtIndexOptions
			{
				Index = i,
				LocalUserId = LocalUserEpicAccountId
			});
			array[i] = ((object)friendAtIndex).ToString();
			array2[i] = friendAtIndex;
		}
		_connectInterface.QueryExternalAccountMappings(new QueryExternalAccountMappingsOptions
		{
			AccountIdType = (ExternalAccountType)0,
			ExternalAccountIds = array,
			LocalUserId = LocalUserProductUserId
		}, (object)array2, new OnQueryExternalAccountMappingsCallback(PopulateFriendsProductUserId));
	}

	private static void PopulateFriendsProductUserId(QueryExternalAccountMappingsCallbackInfo data)
	{
		//IL_0001: Unknown result type (might be due to invalid IL or missing references)
		if ((int)data.ResultCode == 0 && data.ClientData is EpicAccountId[] array)
		{
			EpicAccountId[] array2 = array;
			for (int i = 0; i < array2.Length; i++)
			{
				ProductUserId val = array2[i].ToProductUserId();
				FriendsProductUserIds.Add(val);
				(PlatformSystems.userManager as UserManagerEOS)?.QueryForDisplayNames(val);
			}
			FriendsProductUserIds.Add(EOSLoginManager.loggedInProductId);
			(PlatformSystems.userManager as UserManagerEOS)?.QueryForDisplayNames(EOSLoginManager.loggedInProductId);
		}
	}
}
