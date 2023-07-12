using System;
using System.Collections.Generic;
using System.Linq;
using Epic.OnlineServices;
using Epic.OnlineServices.Connect;
using Epic.OnlineServices.UserInfo;
using UnityEngine;

namespace RoR2;

public class UserManagerEOS : UserManager
{
	private string LocalUserName = string.Empty;

	private static Dictionary<ProductUserId, ExternalAccountInfo> UserIDsToAccountInfo = new Dictionary<ProductUserId, ExternalAccountInfo>();

	private static List<ProductUserId> IDQueryList = new List<ProductUserId>();

	public void InitializeUserManager()
	{
		EOSLoginManager.OnAuthLoggedIn += TryGetLogin;
	}

	public override void GetAvatar(UserID userID, GameObject requestSender, Texture2D tex, AvatarSize size, Action<Texture2D> onRecieved)
	{
		ulong result = 0uL;
		ProductUserId egsValue = userID.CID.egsValue;
		if (UserIDsToAccountInfo.ContainsKey(egsValue) && ulong.TryParse(UserIDsToAccountInfo[egsValue].AccountId, out result))
		{
			SteamUserManager.GetSteamAvatar(new UserID(result), requestSender, tex, size, onRecieved);
		}
	}

	public override string GetUserName()
	{
		return LocalUserName;
	}

	public override int GetUserID()
	{
		return -1;
	}

	public void QueryForDisplayNames(UserID[] ids, Action callback = null)
	{
		if (ids == null)
		{
			Debug.Log((object)"Cannot query. ids is null.");
			return;
		}
		QueryForDisplayNames(ids.Select((UserID x) => x.CID.egsValue).ToArray(), callback);
	}

	public void QueryForDisplayNames(ProductUserId productId, Action callback = null)
	{
		QueryForDisplayNames((ProductUserId[])(object)new ProductUserId[1] { productId }, callback);
	}

	public void QueryForDisplayNames(ProductUserId[] productIds, Action callback = null)
	{
		//IL_0014: Unknown result type (might be due to invalid IL or missing references)
		//IL_0019: Unknown result type (might be due to invalid IL or missing references)
		//IL_0024: Unknown result type (might be due to invalid IL or missing references)
		//IL_002c: Expected O, but got Unknown
		//IL_005e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0068: Expected O, but got Unknown
		QueryProductUserIdMappingsOptions val = new QueryProductUserIdMappingsOptions
		{
			LocalUserId = EOSLoginManager.loggedInProductId,
			ProductUserIds = productIds
		};
		IDQueryList.AddRange(productIds);
		IDQueryList = IDQueryList.Distinct().ToList();
		EOSPlatformManager.GetPlatformInterface().GetConnectInterface().QueryProductUserIdMappings(val, (object)productIds, (OnQueryProductUserIdMappingsCallback)delegate(QueryProductUserIdMappingsCallbackInfo data)
		{
			OnQueryProductUserIdMappingsComplete(data);
			callback?.Invoke();
		});
	}

	private bool GetUserNameFromExternalAccountType(ProductUserId id, uint index, out ExternalAccountInfo accountInfo)
	{
		//IL_000d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0012: Unknown result type (might be due to invalid IL or missing references)
		//IL_0019: Unknown result type (might be due to invalid IL or missing references)
		//IL_0026: Expected O, but got Unknown
		//IL_0021: Unknown result type (might be due to invalid IL or missing references)
		accountInfo = null;
		if ((int)EOSPlatformManager.GetPlatformInterface().GetConnectInterface().CopyProductUserExternalAccountByIndex(new CopyProductUserExternalAccountByIndexOptions
		{
			TargetUserId = id,
			ExternalAccountInfoIndex = index
		}, ref accountInfo) == 0)
		{
			return true;
		}
		return false;
	}

	private void OnQueryProductUserIdMappingsComplete(QueryProductUserIdMappingsCallbackInfo data)
	{
		//IL_0008: Unknown result type (might be due to invalid IL or missing references)
		//IL_000b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0010: Unknown result type (might be due to invalid IL or missing references)
		if (data.GetResultCode() == (Result?)0)
		{
			List<ProductUserId> list = new List<ProductUserId>(IDQueryList);
			foreach (ProductUserId iDQuery in IDQueryList)
			{
				if (GetUserNameFromExternalAccountType(iDQuery, 1u, out var accountInfo))
				{
					UserIDsToAccountInfo[iDQuery] = accountInfo;
					list.Remove(iDQuery);
				}
				else if (GetUserNameFromExternalAccountType(iDQuery, 0u, out accountInfo))
				{
					UserIDsToAccountInfo[iDQuery] = accountInfo;
					list.Remove(iDQuery);
				}
			}
			IDQueryList = list;
			if (IDQueryList.Count == 0)
			{
				InvokeDisplayMappingCompleteAction();
			}
		}
		else
		{
			Debug.Log((object)("Failed to get ProductUserIdMappings, result = " + data.GetResultCode().ToString()));
		}
	}

	public override string GetUserDisplayName(UserID other)
	{
		if (UserIDsToAccountInfo.ContainsKey(other.CID.egsValue))
		{
			return UserIDsToAccountInfo[other.CID.egsValue].DisplayName;
		}
		if (!string.IsNullOrEmpty(((object)other.CID.egsValue).ToString()))
		{
			QueryForDisplayNames(new UserID[1] { other });
		}
		return string.Empty;
	}

	private void TryGetLogin(EpicAccountId obj)
	{
		//IL_0021: Unknown result type (might be due to invalid IL or missing references)
		//IL_0026: Unknown result type (might be due to invalid IL or missing references)
		//IL_002d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0035: Expected O, but got Unknown
		//IL_003f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0049: Expected O, but got Unknown
		UserInfoInterface userInfoInterface = EOSPlatformManager.GetPlatformInterface().GetUserInfoInterface();
		if ((Handle)(object)userInfoInterface != (Handle)null && (Handle)(object)EOSLoginManager.loggedInAuthId != (Handle)null)
		{
			QueryUserInfoOptions val = new QueryUserInfoOptions
			{
				LocalUserId = obj,
				TargetUserId = obj
			};
			userInfoInterface.QueryUserInfo(val, (object)null, new OnQueryUserInfoCallback(OnQueryUserInfo));
		}
	}

	private void OnQueryUserInfo(QueryUserInfoCallbackInfo data)
	{
		//IL_0001: Unknown result type (might be due to invalid IL or missing references)
		//IL_001c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0021: Unknown result type (might be due to invalid IL or missing references)
		//IL_002c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0038: Expected O, but got Unknown
		//IL_0038: Unknown result type (might be due to invalid IL or missing references)
		//IL_003e: Expected O, but got Unknown
		//IL_0042: Unknown result type (might be due to invalid IL or missing references)
		if ((int)data.ResultCode == 0)
		{
			UserInfoInterface userInfoInterface = EOSPlatformManager.GetPlatformInterface().GetUserInfoInterface();
			if ((Handle)(object)userInfoInterface != (Handle)null)
			{
				CopyUserInfoOptions val = new CopyUserInfoOptions
				{
					LocalUserId = EOSLoginManager.loggedInAuthId,
					TargetUserId = EOSLoginManager.loggedInAuthId
				};
				UserInfoData val2 = new UserInfoData();
				userInfoInterface.CopyUserInfo(val, ref val2);
				LocalUserName = val2.DisplayName;
				Debug.Log((object)("Got userinfo, user name = " + LocalUserName));
			}
		}
	}
}
