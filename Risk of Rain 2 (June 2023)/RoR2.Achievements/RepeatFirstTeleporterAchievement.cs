using System;
using RoR2.Stats;
using UnityEngine;

namespace RoR2.Achievements;

[RegisterAchievement("RepeatFirstTeleporter", "Characters.Toolbot", null, typeof(RepeatFirstTeleporterServerAchievement))]
public class RepeatFirstTeleporterAchievement : BaseAchievement
{
	private class RepeatFirstTeleporterServerAchievement : BaseServerAchievement
	{
		public override void OnInstall()
		{
			base.OnInstall();
			TeleporterInteraction.onTeleporterChargedGlobal += OnTeleporterCharged;
		}

		public override void OnUninstall()
		{
			TeleporterInteraction.onTeleporterChargedGlobal -= OnTeleporterCharged;
			base.OnUninstall();
		}

		private void OnTeleporterCharged(TeleporterInteraction teleporterInteraction)
		{
			SceneCatalog.GetSceneDefForCurrentScene();
			_ = base.networkUser.masterPlayerStatsComponent.currentStats;
			if (Object.op_Implicit((Object)(object)Run.instance) && Run.instance.stageClearCount == 0)
			{
				PlayerStatsComponent masterPlayerStatsComponent = base.networkUser.masterPlayerStatsComponent;
				if (Object.op_Implicit((Object)(object)masterPlayerStatsComponent))
				{
					masterPlayerStatsComponent.currentStats.PushStatValue(StatDef.firstTeleporterCompleted, 1uL);
				}
			}
		}
	}

	private const int requirement = 5;

	public override void OnInstall()
	{
		base.OnInstall();
		UserProfile obj = base.userProfile;
		obj.onStatsReceived = (Action)Delegate.Combine(obj.onStatsReceived, new Action(Check));
		SetServerTracked(shouldTrack: true);
	}

	public override void OnUninstall()
	{
		UserProfile obj = base.userProfile;
		obj.onStatsReceived = (Action)Delegate.Remove(obj.onStatsReceived, new Action(Check));
		base.OnUninstall();
	}

	private void Check()
	{
		if (base.userProfile.statSheet.GetStatValueULong(StatDef.firstTeleporterCompleted) >= 5)
		{
			Grant();
		}
	}

	public override float ProgressForAchievement()
	{
		return (float)base.userProfile.statSheet.GetStatValueULong(StatDef.firstTeleporterCompleted) / 5f;
	}
}
