using Epic.OnlineServices;
using Epic.OnlineServices.Connect;

namespace RoR2;

public static class EOSExtensions
{
	public static ProductUserId ToProductUserId(this EpicAccountId epicAccountId)
	{
		//IL_000a: Unknown result type (might be due to invalid IL or missing references)
		//IL_000f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0016: Unknown result type (might be due to invalid IL or missing references)
		//IL_0021: Unknown result type (might be due to invalid IL or missing references)
		//IL_0032: Expected O, but got Unknown
		return EOSPlatformManager.GetPlatformInterface().GetConnectInterface().GetExternalAccountMapping(new GetExternalAccountMappingsOptions
		{
			AccountIdType = (ExternalAccountType)0,
			LocalUserId = EOSLoginManager.loggedInProductId,
			TargetExternalUserId = ((object)epicAccountId).ToString()
		});
	}
}
