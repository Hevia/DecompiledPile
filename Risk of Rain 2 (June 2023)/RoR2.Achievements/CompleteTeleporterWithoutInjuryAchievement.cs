using UnityEngine;

namespace RoR2.Achievements;

[RegisterAchievement("CompleteTeleporterWithoutInjury", "Items.SecondarySkillMagazine", null, null)]
public class CompleteTeleporterWithoutInjuryAchievement : BaseAchievement
{
	private bool hasBeenHit;

	public override void OnInstall()
	{
		base.OnInstall();
		TeleporterInteraction.onTeleporterBeginChargingGlobal += OnTeleporterBeginCharging;
		TeleporterInteraction.onTeleporterChargedGlobal += OnTeleporterCharged;
		GlobalEventManager.onClientDamageNotified += OnClientDamageNotified;
	}

	public override void OnUninstall()
	{
		TeleporterInteraction.onTeleporterBeginChargingGlobal -= OnTeleporterBeginCharging;
		TeleporterInteraction.onTeleporterChargedGlobal -= OnTeleporterCharged;
		GlobalEventManager.onClientDamageNotified -= OnClientDamageNotified;
		base.OnUninstall();
	}

	private void OnTeleporterBeginCharging(TeleporterInteraction teleporterInteraction)
	{
		hasBeenHit = false;
	}

	private void OnTeleporterCharged(TeleporterInteraction teleporterInteraction)
	{
		Check();
	}

	private void OnClientDamageNotified(DamageDealtMessage damageDealtMessage)
	{
		if (!hasBeenHit && Object.op_Implicit((Object)(object)damageDealtMessage.victim) && (Object)(object)damageDealtMessage.victim == (Object)(object)base.localUser.cachedBodyObject)
		{
			hasBeenHit = true;
		}
	}

	private void Check()
	{
		if (Object.op_Implicit((Object)(object)base.localUser.cachedBody) && Object.op_Implicit((Object)(object)base.localUser.cachedBody.healthComponent) && base.localUser.cachedBody.healthComponent.alive && !hasBeenHit)
		{
			Grant();
		}
	}
}
