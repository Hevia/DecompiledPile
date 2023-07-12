using EntityStates.TimedChest;
using UnityEngine;

namespace RoR2.Achievements;

[RegisterAchievement("FindTimedChest", "Items.BFG", null, typeof(FindTimedChestServerAchievement))]
public class FindTimedChestAchievement : BaseAchievement
{
	private class FindTimedChestServerAchievement : BaseServerAchievement
	{
		public override void OnInstall()
		{
			base.OnInstall();
			Debug.Log((object)"subscribed");
			Opening.onOpened += OnOpened;
		}

		public override void OnUninstall()
		{
			base.OnInstall();
			Opening.onOpened -= OnOpened;
		}

		private void OnOpened()
		{
			Debug.Log((object)"grant");
			Grant();
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
