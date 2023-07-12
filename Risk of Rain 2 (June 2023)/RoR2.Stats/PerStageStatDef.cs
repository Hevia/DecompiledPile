using System;
using System.Collections.Generic;
using HG;
using JetBrains.Annotations;

namespace RoR2.Stats;

public class PerStageStatDef
{
	private readonly string prefix;

	private readonly StatRecordType recordType;

	private readonly StatDataType dataType;

	private readonly Dictionary<string, StatDef> keyToStatDef = new Dictionary<string, StatDef>();

	private StatDef.DisplayValueFormatterDelegate displayValueFormatter;

	private static readonly List<PerStageStatDef> instancesList;

	public static readonly PerStageStatDef totalTimesVisited;

	public static readonly PerStageStatDef totalTimesCleared;

	static PerStageStatDef()
	{
		instancesList = new List<PerStageStatDef>();
		totalTimesVisited = Register("totalTimesVisited", StatRecordType.Sum, StatDataType.ULong);
		totalTimesCleared = Register("totalTimesCleared", StatRecordType.Sum, StatDataType.ULong);
	}

	public static void RegisterStatDefs()
	{
		//IL_0015: Unknown result type (might be due to invalid IL or missing references)
		//IL_001a: Unknown result type (might be due to invalid IL or missing references)
		//IL_001d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0022: Unknown result type (might be due to invalid IL or missing references)
		foreach (PerStageStatDef instances in instancesList)
		{
			Enumerator<string> enumerator2 = SceneCatalog.allBaseSceneNames.GetEnumerator();
			try
			{
				while (enumerator2.MoveNext())
				{
					string current2 = enumerator2.Current;
					StatDef value = StatDef.Register(instances.prefix + "." + current2, instances.recordType, instances.dataType, 0.0, instances.displayValueFormatter);
					instances.keyToStatDef[current2] = value;
				}
			}
			finally
			{
				((IDisposable)enumerator2).Dispose();
			}
		}
	}

	private PerStageStatDef(string prefix, StatRecordType recordType, StatDataType dataType, StatDef.DisplayValueFormatterDelegate displayValueFormatter)
	{
		this.prefix = prefix;
		this.recordType = recordType;
		this.dataType = dataType;
		this.displayValueFormatter = displayValueFormatter ?? new StatDef.DisplayValueFormatterDelegate(StatDef.DefaultDisplayValueFormatter);
	}

	[NotNull]
	private static PerStageStatDef Register(string prefix, StatRecordType recordType, StatDataType dataType, StatDef.DisplayValueFormatterDelegate displayValueFormatter = null)
	{
		PerStageStatDef perStageStatDef = new PerStageStatDef(prefix, recordType, dataType, displayValueFormatter);
		instancesList.Add(perStageStatDef);
		return perStageStatDef;
	}

	[CanBeNull]
	public StatDef FindStatDef(string key)
	{
		if (keyToStatDef.TryGetValue(key, out var value))
		{
			return value;
		}
		return null;
	}
}
