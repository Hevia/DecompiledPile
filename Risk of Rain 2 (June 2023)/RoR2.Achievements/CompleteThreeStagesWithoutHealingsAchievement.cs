using RoR2.Stats;
using UnityEngine;

namespace RoR2.Achievements;

[RegisterAchievement("CompleteThreeStagesWithoutHealing", "Items.IncreaseHealing", null, typeof(CompleteThreeStagesWithoutHealingServerAchievement))]
public class CompleteThreeStagesWithoutHealingsAchievement : BaseAchievement
{
	private class CompleteThreeStagesWithoutHealingServerAchievement : BaseServerAchievement
	{
		public override void OnInstall()
		{
			base.OnInstall();
			SceneExitController.onBeginExit += OnSceneBeginExit;
		}

		public override void OnUninstall()
		{
			SceneExitController.onBeginExit -= OnSceneBeginExit;
			base.OnInstall();
		}

		private void OnSceneBeginExit(SceneExitController exitController)
		{
			Check();
		}

		private void Check()
		{
			if (Object.op_Implicit((Object)(object)Run.instance) && ((object)Run.instance).GetType() == typeof(Run) && (Object)(object)base.networkUser != (Object)null)
			{
				StatSheet currentStats = base.networkUser.masterPlayerStatsComponent.currentStats;
				CharacterBody currentBody = GetCurrentBody();
				if (currentStats.GetStatValueULong(StatDef.highestStagesCompleted) >= 2 && (float)currentStats.GetStatValueULong(StatDef.totalHealthHealed) <= 0f && Object.op_Implicit((Object)(object)currentBody) && Object.op_Implicit((Object)(object)currentBody.healthComponent) && currentBody.healthComponent.alive)
				{
					Grant();
				}
			}
		}
	}

	private const int requirement = 2;

	public override void OnInstall()
	{
		base.OnInstall();
		SetServerTracked(shouldTrack: true);
	}

	public override void OnUninstall()
	{
		SetServerTracked(shouldTrack: false);
		base.OnUninstall();
	}
}
