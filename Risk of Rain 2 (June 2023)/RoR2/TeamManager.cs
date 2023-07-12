using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using RoR2.Networking;
using UnityEngine;
using UnityEngine.Networking;

namespace RoR2;

public class TeamManager : NetworkBehaviour
{
	public static readonly uint naturalLevelCap;

	private static readonly ulong[] levelToExperienceTable;

	public static readonly ulong hardExpCap;

	private ulong[] teamExperience = new ulong[5];

	private uint[] teamLevels = new uint[5];

	private ulong[] teamCurrentLevelExperience = new ulong[5];

	private ulong[] teamNextLevelExperience = new ulong[5];

	private const uint teamExperienceDirtyBitsMask = 31u;

	public static TeamManager instance { get; private set; }

	static TeamManager()
	{
		List<ulong> list = new List<ulong> { 0uL, 0uL };
		naturalLevelCap = 2u;
		while (true)
		{
			ulong num = (ulong)InitialCalcExperience(naturalLevelCap);
			if (num <= list[list.Count - 1])
			{
				break;
			}
			list.Add(num);
			naturalLevelCap++;
		}
		naturalLevelCap--;
		levelToExperienceTable = list.ToArray();
		hardExpCap = levelToExperienceTable[levelToExperienceTable.Length - 1];
	}

	private static double InitialCalcLevel(double experience, double experienceForFirstLevelUp = 20.0, double growthRate = 1.55)
	{
		return Math.Max(Math.Log(1.0 - experience / experienceForFirstLevelUp * (1.0 - growthRate), growthRate) + 1.0, 1.0);
	}

	private static double InitialCalcExperience(double level, double experienceForFirstLevelUp = 20.0, double growthRate = 1.55)
	{
		return Math.Max(experienceForFirstLevelUp * ((1.0 - Math.Pow(growthRate, level - 1.0)) / (1.0 - growthRate)), 0.0);
	}

	private static uint FindLevelForExperience(ulong experience)
	{
		for (uint num = 1u; num < levelToExperienceTable.Length; num++)
		{
			if (levelToExperienceTable[num] > experience)
			{
				return num - 1;
			}
		}
		return naturalLevelCap;
	}

	private static ulong GetExperienceForLevel(uint level)
	{
		if (level > naturalLevelCap)
		{
			level = naturalLevelCap;
		}
		return levelToExperienceTable[level];
	}

	private void OnEnable()
	{
		instance = SingletonHelper.Assign<TeamManager>(instance, this);
	}

	private void OnDisable()
	{
		instance = SingletonHelper.Unassign<TeamManager>(instance, this);
	}

	private void Start()
	{
		if (NetworkServer.active)
		{
			for (TeamIndex teamIndex = TeamIndex.Neutral; teamIndex < TeamIndex.Count; teamIndex++)
			{
				SetTeamExperience(teamIndex, 0uL);
			}
		}
	}

	public override int GetNetworkChannel()
	{
		return QosChannelIndex.defaultReliable.intVal;
	}

	[Server]
	public void GiveTeamMoney(TeamIndex teamIndex, uint money)
	{
		if (!NetworkServer.active)
		{
			Debug.LogWarning((object)"[Server] function 'System.Void RoR2.TeamManager::GiveTeamMoney(RoR2.TeamIndex,System.UInt32)' called on client");
			return;
		}
		int num = (Object.op_Implicit((Object)(object)Run.instance) ? Run.instance.livingPlayerCount : 0);
		if (num != 0)
		{
			money = (uint)Mathf.CeilToInt((float)money / (float)num);
		}
		ReadOnlyCollection<TeamComponent> teamMembers = TeamComponent.GetTeamMembers(teamIndex);
		for (int i = 0; i < teamMembers.Count; i++)
		{
			CharacterBody component = ((Component)teamMembers[i]).GetComponent<CharacterBody>();
			if (Object.op_Implicit((Object)(object)component) && component.isPlayerControlled)
			{
				CharacterMaster master = component.master;
				if (Object.op_Implicit((Object)(object)master))
				{
					master.GiveMoney(money);
				}
			}
		}
	}

	[Server]
	public void GiveTeamExperience(TeamIndex teamIndex, ulong experience)
	{
		if (!NetworkServer.active)
		{
			Debug.LogWarning((object)"[Server] function 'System.Void RoR2.TeamManager::GiveTeamExperience(RoR2.TeamIndex,System.UInt64)' called on client");
			return;
		}
		ulong num = teamExperience[(int)teamIndex];
		ulong num2 = num + experience;
		if (num2 < num)
		{
			num2 = ulong.MaxValue;
		}
		SetTeamExperience(teamIndex, num2);
	}

	private void SetTeamExperience(TeamIndex teamIndex, ulong newExperience)
	{
		if (newExperience > hardExpCap)
		{
			newExperience = hardExpCap;
		}
		teamExperience[(int)teamIndex] = newExperience;
		uint num = teamLevels[(int)teamIndex];
		uint num2 = FindLevelForExperience(newExperience);
		if (num != num2)
		{
			ReadOnlyCollection<TeamComponent> teamMembers = TeamComponent.GetTeamMembers(teamIndex);
			for (int i = 0; i < teamMembers.Count; i++)
			{
				CharacterBody component = ((Component)teamMembers[i]).GetComponent<CharacterBody>();
				if (Object.op_Implicit((Object)(object)component))
				{
					component.OnTeamLevelChanged();
				}
			}
			teamLevels[(int)teamIndex] = num2;
			teamCurrentLevelExperience[(int)teamIndex] = GetExperienceForLevel(num2);
			teamNextLevelExperience[(int)teamIndex] = GetExperienceForLevel(num2 + 1);
			if (num < num2)
			{
				GlobalEventManager.OnTeamLevelUp(teamIndex);
			}
		}
		if (NetworkServer.active)
		{
			((NetworkBehaviour)this).SetDirtyBit((uint)(1 << (int)teamIndex));
		}
	}

	public override bool OnSerialize(NetworkWriter writer, bool initialState)
	{
		uint num = (initialState ? 31u : ((NetworkBehaviour)this).syncVarDirtyBits);
		writer.WritePackedUInt32(num);
		if (num == 0)
		{
			return false;
		}
		for (TeamIndex teamIndex = TeamIndex.Neutral; teamIndex < TeamIndex.Count; teamIndex++)
		{
			if ((num & (uint)(1 << (int)teamIndex)) != 0)
			{
				writer.WritePackedUInt64(teamExperience[(int)teamIndex]);
			}
		}
		return true;
	}

	public override void OnDeserialize(NetworkReader reader, bool initialState)
	{
		uint num = reader.ReadPackedUInt32();
		for (TeamIndex teamIndex = TeamIndex.Neutral; teamIndex < TeamIndex.Count; teamIndex++)
		{
			if ((num & (uint)(1 << (int)teamIndex)) != 0)
			{
				ulong newExperience = reader.ReadPackedUInt64();
				SetTeamExperience(teamIndex, newExperience);
			}
		}
	}

	public ulong GetTeamExperience(TeamIndex teamIndex)
	{
		if (teamIndex < TeamIndex.Neutral || teamIndex >= TeamIndex.Count)
		{
			return 0uL;
		}
		return teamExperience[(int)teamIndex];
	}

	public ulong GetTeamCurrentLevelExperience(TeamIndex teamIndex)
	{
		if (teamIndex < TeamIndex.Neutral || teamIndex >= TeamIndex.Count)
		{
			return 0uL;
		}
		return teamCurrentLevelExperience[(int)teamIndex];
	}

	public ulong GetTeamNextLevelExperience(TeamIndex teamIndex)
	{
		if (teamIndex < TeamIndex.Neutral || teamIndex >= TeamIndex.Count)
		{
			return 0uL;
		}
		return teamNextLevelExperience[(int)teamIndex];
	}

	public uint GetTeamLevel(TeamIndex teamIndex)
	{
		if (teamIndex < TeamIndex.Neutral || teamIndex >= TeamIndex.Count)
		{
			return 0u;
		}
		return teamLevels[(int)teamIndex];
	}

	public void SetTeamLevel(TeamIndex teamIndex, uint newLevel)
	{
		if (teamIndex >= TeamIndex.Neutral && teamIndex < TeamIndex.Count && GetTeamLevel(teamIndex) != newLevel)
		{
			SetTeamExperience(teamIndex, GetExperienceForLevel(newLevel));
		}
	}

	public static bool IsTeamEnemy(TeamIndex teamA, TeamIndex teamB)
	{
		if (teamA != teamB)
		{
			return true;
		}
		return false;
	}

	[ConCommand(commandName = "team_set_level", flags = (ConVarFlags.ExecuteOnServer | ConVarFlags.Cheat), helpText = "Sets the team specified by the first argument to the level specified by the second argument.")]
	private static void CCTeamSetLevel(ConCommandArgs args)
	{
		TeamIndex argEnum = args.GetArgEnum<TeamIndex>(0);
		ulong argULong = args.GetArgULong(1);
		if (!Object.op_Implicit((Object)(object)instance))
		{
			throw new ConCommandException("No TeamManager exists.");
		}
		instance.SetTeamLevel(argEnum, (uint)argULong);
	}

	private void UNetVersion()
	{
	}

	public override void PreStartClient()
	{
	}
}
