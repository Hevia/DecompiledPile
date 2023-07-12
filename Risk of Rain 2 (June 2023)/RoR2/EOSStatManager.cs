using Epic.OnlineServices;
using Epic.OnlineServices.Stats;

namespace RoR2;

public class EOSStatManager
{
	public static class StatNames
	{
		public const string kFastestWeeklyRun = "FASTESTWEEKLYRUN";
	}

	private static StatsInterface Interface;

	public EOSStatManager()
	{
		Interface = EOSPlatformManager.GetPlatformInterface().GetStatsInterface();
	}

	public static void IngestStat(IngestData[] statsToIngest, object callbackData = null, OnIngestStatCompleteCallback callback = null)
	{
		//IL_0005: Unknown result type (might be due to invalid IL or missing references)
		//IL_000a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0015: Unknown result type (might be due to invalid IL or missing references)
		//IL_001c: Unknown result type (might be due to invalid IL or missing references)
		//IL_002e: Expected O, but got Unknown
		Interface.IngestStat(new IngestStatOptions
		{
			LocalUserId = EOSLoginManager.loggedInProductId,
			Stats = statsToIngest,
			TargetUserId = EOSLoginManager.loggedInProductId
		}, callbackData, callback);
	}

	public static void QueryStats(QueryStatsOptions queryStatsOptions, object callbackData = null, OnQueryStatsCompleteCallback callback = null)
	{
		Interface.QueryStats(queryStatsOptions, callbackData, callback);
	}

	public static Stat GetStat(string statName, ProductUserId targetUserId)
	{
		//IL_0005: Unknown result type (might be due to invalid IL or missing references)
		//IL_000a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0011: Unknown result type (might be due to invalid IL or missing references)
		//IL_001f: Expected O, but got Unknown
		//IL_001a: Unknown result type (might be due to invalid IL or missing references)
		Stat result = default(Stat);
		Interface.CopyStatByName(new CopyStatByNameOptions
		{
			Name = statName,
			TargetUserId = targetUserId
		}, ref result);
		return result;
	}

	public static uint GetStatsCount(ProductUserId targetUserId)
	{
		//IL_0005: Unknown result type (might be due to invalid IL or missing references)
		//IL_000a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0016: Expected O, but got Unknown
		return Interface.GetStatsCount(new GetStatCountOptions
		{
			TargetUserId = targetUserId
		});
	}
}
