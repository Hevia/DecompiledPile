using UnityEngine;

namespace RoR2.Achievements;

[RegisterAchievement("CompleteUnknownEnding", "Characters.Mercenary", null, typeof(CompleteUnknownEndingServerAchievement))]
public class CompleteUnknownEndingAchievement : BaseAchievement
{
	private class CompleteUnknownEndingServerAchievement : BaseServerAchievement
	{
		public override void OnInstall()
		{
			base.OnInstall();
			Run.onServerGameOver += OnServerGameOver;
		}

		public override void OnUninstall()
		{
			base.OnInstall();
			Run.onServerGameOver -= OnServerGameOver;
		}

		private void OnServerGameOver(Run run, GameEndingDef gameEndingDef)
		{
			if ((Object)(object)gameEndingDef == (Object)(object)RoR2Content.GameEndings.ObliterationEnding || (Object)(object)gameEndingDef == (Object)(object)RoR2Content.GameEndings.LimboEnding)
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
		base.OnUninstall();
	}
}
