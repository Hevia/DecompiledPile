using UnityEngine;

namespace RoR2.Achievements;

[RegisterAchievement("MajorMultikill", "Items.BurnNearby", null, typeof(MajorMultikillServerAchievement))]
public class MajorMultikillAchievement : BaseAchievement
{
	private class MajorMultikillServerAchievement : BaseServerAchievement
	{
		private const int multiKillThreshold = 15;

		public override void OnInstall()
		{
			base.OnInstall();
			GlobalEventManager.onCharacterDeathGlobal += OnCharacterDeath;
		}

		public override void OnUninstall()
		{
			GlobalEventManager.onCharacterDeathGlobal -= OnCharacterDeath;
			base.OnUninstall();
		}

		private void OnCharacterDeath(DamageReport damageReport)
		{
			GameObject attacker = damageReport.damageInfo.attacker;
			if (Object.op_Implicit((Object)(object)attacker))
			{
				CharacterBody component = attacker.GetComponent<CharacterBody>();
				if (Object.op_Implicit((Object)(object)component) && component.multiKillCount >= 15 && (Object)(object)component.masterObject == (Object)(object)serverAchievementTracker.networkUser.masterObject)
				{
					Grant();
				}
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
