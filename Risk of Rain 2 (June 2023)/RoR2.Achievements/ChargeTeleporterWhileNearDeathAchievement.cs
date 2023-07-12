using UnityEngine;

namespace RoR2.Achievements;

[RegisterAchievement("ChargeTeleporterWhileNearDeath", "Items.WarCryOnMultiKill", null, null)]
public class ChargeTeleporterWhileNearDeathAchievement : BaseAchievement
{
	private const float requirement = 0.1f;

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
		if (Object.op_Implicit((Object)(object)base.localUser.cachedBody) && Object.op_Implicit((Object)(object)base.localUser.cachedBody.healthComponent) && base.localUser.cachedBody.healthComponent.alive && base.localUser.cachedBody.healthComponent.combinedHealthFraction <= 0.1f)
		{
			Grant();
		}
	}
}
