using RoR2.Stats;
using UnityEngine;

namespace RoR2.Achievements;

[RegisterAchievement("CompleteThreeStages", "Characters.Bandit2", null, null)]
public class CompleteThreeStagesAchievement : BaseAchievement
{
	private const int requirement = 3;

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
		Check();
	}

	private void Check()
	{
		if (Object.op_Implicit((Object)(object)Run.instance) && ((object)Run.instance).GetType() == typeof(Run))
		{
			SceneDef sceneDefForCurrentScene = SceneCatalog.GetSceneDefForCurrentScene();
			if (!((Object)(object)sceneDefForCurrentScene == (Object)null) && base.localUser.currentNetworkUser.masterPlayerStatsComponent.currentStats.GetStatValueULong(StatDef.totalDeaths) == 0L && sceneDefForCurrentScene.stageOrder == 3)
			{
				Grant();
			}
		}
	}
}
