using UnityEngine;

namespace RoR2.Achievements;

[RegisterAchievement("CompleteTeleporter", "Items.BossDamageBonus", null, typeof(CompleteTeleporterServerAchievement))]
public class CompleteTeleporterAchievement : BaseAchievement
{
	private class CompleteTeleporterServerAchievement : BaseServerAchievement
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
			CharacterBody currentBody = GetCurrentBody();
			if (Object.op_Implicit((Object)(object)currentBody) && Object.op_Implicit((Object)(object)currentBody.healthComponent) && currentBody.healthComponent.alive)
			{
				Grant();
			}
		}
	}

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
