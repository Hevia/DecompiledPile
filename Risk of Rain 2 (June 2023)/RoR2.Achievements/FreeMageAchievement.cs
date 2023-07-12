using EntityStates.LockedMage;
using UnityEngine;

namespace RoR2.Achievements;

[RegisterAchievement("FreeMage", "Characters.Mage", null, typeof(FreeMageServerAchievement))]
public class FreeMageAchievement : BaseAchievement
{
	private class FreeMageServerAchievement : BaseServerAchievement
	{
		public override void OnInstall()
		{
			base.OnInstall();
			UnlockingMage.onOpened += OnOpened;
		}

		public override void OnUninstall()
		{
			base.OnInstall();
			UnlockingMage.onOpened -= OnOpened;
		}

		private void OnOpened(Interactor interactor)
		{
			CharacterBody currentBody = serverAchievementTracker.networkUser.GetCurrentBody();
			if (Object.op_Implicit((Object)(object)currentBody) && (Object)(object)((Component)currentBody).GetComponent<Interactor>() == (Object)(object)interactor)
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
