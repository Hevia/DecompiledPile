using UnityEngine;

namespace RoR2.Achievements.Treebot;

[RegisterAchievement("TreebotLowHealthTeleporter", "Skills.Treebot.Barrage", "RescueTreebot", null)]
public class TreebotLowHealthTeleporterAchievement : BaseAchievement
{
	private static readonly float requirement = 0.5f;

	private ToggleAction healthMonitor;

	private HealthComponent targetHealthComponent;

	private bool failed = true;

	private HealthComponent healthComponent;

	protected override BodyIndex LookUpRequiredBodyIndex()
	{
		return BodyCatalog.FindBodyIndex("TreebotBody");
	}

	private void OnTeleporterBeginChargingGlobal(TeleporterInteraction teleporterInteraction)
	{
		failed = false;
		healthMonitor.SetActive(newActive: true);
	}

	private void OnTeleporterChargedGlobal(TeleporterInteraction teleporterInteraction)
	{
		if (!failed)
		{
			Grant();
		}
	}

	private void SubscribeHealthMonitor()
	{
		RoR2Application.onFixedUpdate += OnFixedUpdateMonitorHealth;
	}

	private void UnsubscribeHealthMonitor()
	{
		RoR2Application.onFixedUpdate -= OnFixedUpdateMonitorHealth;
	}

	private void OnFixedUpdateMonitorHealth()
	{
		if (!Object.op_Implicit((Object)(object)healthComponent) || requirement < healthComponent.combinedHealthFraction)
		{
			failed = true;
			healthMonitor.SetActive(newActive: false);
		}
	}

	public override void OnInstall()
	{
		base.OnInstall();
		healthMonitor = new ToggleAction(SubscribeHealthMonitor, UnsubscribeHealthMonitor);
	}

	protected override void OnBodyRequirementMet()
	{
		base.OnBodyRequirementMet();
		TeleporterInteraction.onTeleporterBeginChargingGlobal += OnTeleporterBeginChargingGlobal;
		TeleporterInteraction.onTeleporterChargedGlobal += OnTeleporterChargedGlobal;
		base.localUser.onBodyChanged += OnBodyChanged;
		OnBodyChanged();
	}

	private void OnBodyChanged()
	{
		healthComponent = base.localUser.cachedBody?.healthComponent;
	}

	protected override void OnBodyRequirementBroken()
	{
		base.localUser.onBodyChanged -= OnBodyChanged;
		TeleporterInteraction.onTeleporterChargedGlobal -= OnTeleporterChargedGlobal;
		TeleporterInteraction.onTeleporterBeginChargingGlobal -= OnTeleporterBeginChargingGlobal;
		healthMonitor.SetActive(newActive: false);
		healthComponent = null;
		base.OnBodyRequirementBroken();
	}
}
