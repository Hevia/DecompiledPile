using System.Collections.Generic;
using UnityEngine;

namespace RoR2.Achievements;

[RegisterAchievement("HardEliteBossKill", "Items.KillEliteFrenzy", null, typeof(EliteBossKillServerAchievement))]
internal class HardEliteBossKillAchievement : BaseAchievement
{
	private class EliteBossKillServerAchievement : BaseServerAchievement
	{
		private static readonly List<EliteBossKillServerAchievement> instancesList = new List<EliteBossKillServerAchievement>();

		public override void OnInstall()
		{
			base.OnInstall();
			instancesList.Add(this);
			if (instancesList.Count == 1)
			{
				GlobalEventManager.onCharacterDeathGlobal += OnCharacterDeath;
			}
		}

		public override void OnUninstall()
		{
			if (instancesList.Count == 1)
			{
				GlobalEventManager.onCharacterDeathGlobal -= OnCharacterDeath;
			}
			instancesList.Remove(this);
			base.OnUninstall();
		}

		private static void OnCharacterDeath(DamageReport damageReport)
		{
			if (!Object.op_Implicit((Object)(object)damageReport.victim))
			{
				return;
			}
			CharacterBody component = ((Component)damageReport.victim).GetComponent<CharacterBody>();
			if (!Object.op_Implicit((Object)(object)component) || !component.isChampion || !component.isElite)
			{
				return;
			}
			foreach (EliteBossKillServerAchievement instances in instancesList)
			{
				GameObject masterObject = instances.serverAchievementTracker.networkUser.masterObject;
				if (!Object.op_Implicit((Object)(object)masterObject))
				{
					continue;
				}
				CharacterMaster component2 = masterObject.GetComponent<CharacterMaster>();
				if (Object.op_Implicit((Object)(object)component2))
				{
					CharacterBody body = component2.GetBody();
					if (Object.op_Implicit((Object)(object)body) && Object.op_Implicit((Object)(object)body.healthComponent) && body.healthComponent.alive)
					{
						instances.Grant();
					}
				}
			}
		}
	}

	public override void OnInstall()
	{
		base.OnInstall();
		NetworkUser.OnPostNetworkUserStart += OnPostNetworkUserStart;
		Run.onRunStartGlobal += OnRunStart;
	}

	public override void OnUninstall()
	{
		NetworkUser.OnPostNetworkUserStart -= OnPostNetworkUserStart;
		Run.onRunStartGlobal -= OnRunStart;
		base.OnUninstall();
	}

	private void UpdateTracking()
	{
		SetServerTracked(Object.op_Implicit((Object)(object)Run.instance) && (DifficultyCatalog.GetDifficultyDef(Run.instance.selectedDifficulty)?.countsAsHardMode ?? false));
	}

	private void OnPostNetworkUserStart(NetworkUser networkUser)
	{
		UpdateTracking();
	}

	private void OnRunStart(Run run)
	{
		UpdateTracking();
	}
}
